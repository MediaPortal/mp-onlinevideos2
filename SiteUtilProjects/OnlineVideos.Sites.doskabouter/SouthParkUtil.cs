﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml;
using RssToolkit.Rss;

namespace OnlineVideos.Sites
{
    public class SouthParkUtil : GenericSiteUtil
    {
        [Category("OnlineVideosUserConfiguration"), Description("Enables subtitles")]
        bool enableSubtitles = false;

        Regex episodePlayerRegEx = new Regex(@"swfobject.embedSWF\(""(?<url>[^""]*)""", RegexOptions.Compiled);

        private DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public override void Initialize(SiteSettings siteSettings)
        {
            base.Initialize(siteSettings);
        }

        public override int DiscoverDynamicCategories()
        {
            int res = base.DiscoverDynamicCategories();
            foreach (Category cat in Settings.Categories)
                cat.Name = "Season " + cat.Name;
            return res;
        }

        protected override void ExtraVideoMatch(VideoInfo video, GroupCollection matchGroups)
        {

            TrackingInfo ti = new TrackingInfo();

            // for southpark world
            System.Text.RegularExpressions.Group epGroup = matchGroups["Episode"];
            if (epGroup.Success)
                ti.Regex = Regex.Match(epGroup.Value, @"(?<Season>\d\d)(?<Episode>\d\d)");

            // for nl and de
            if (ti.Season == 0)
                ti.Regex = Regex.Match(video.VideoUrl, @"\/S(?<Season>\d{1,3})E(?<Episode>\d{1,3})-", RegexOptions.IgnoreCase);

            if (ti.Season != 0)
            {
                ti.Title = "South Park";
                ti.VideoKind = VideoKind.TvSeries;
                video.TrackingInfo = ti;
                video.Other = new VideoInfoOtherHelper();
            }
            else
                video.Other = new VideoInfoOtherHelper();
            int time;
            if (Int32.TryParse(video.Airdate, out time))
            {
                video.Airdate = epoch.AddSeconds(time).ToString();
            }
        }

        private enum SouthParkCountry { Unknown, World, De };

        public override List<String> GetMultipleVideoUrls(VideoInfo video, bool inPlaylist = false)
        {
            List<string> result = new List<string>();

            string data = GetWebData(video.VideoUrl);
            if (!string.IsNullOrEmpty(data))
            {
                Match m = episodePlayerRegEx.Match(data);
                string playerUrl;
                if (m.Success)
                    playerUrl = m.Groups["url"].Value;
                else
                    playerUrl = video.VideoUrl;
                playerUrl = WebCache.Instance.GetRedirectedUrl(playerUrl);
                playerUrl = System.Web.HttpUtility.ParseQueryString(new Uri(playerUrl).Query)["uri"];
                SouthParkCountry spc = SouthParkCountry.Unknown;
                if (video.VideoUrl.Contains("southparkstudios.com"))
                    spc = SouthParkCountry.World;
                else if (video.VideoUrl.ToLower().Contains(".de") || video.VideoUrl.ToLower().Contains("de."))
                    spc = SouthParkCountry.De;
                if (spc == SouthParkCountry.World || spc == SouthParkCountry.De)
                {
                    playerUrl = System.Web.HttpUtility.UrlEncode(playerUrl);
                    playerUrl = new Uri(new Uri(baseUrl), @"/feeds/video-player/mrss/" + playerUrl).AbsoluteUri;
                }
                else
                {
                    playerUrl = System.Web.HttpUtility.UrlDecode(playerUrl);
                    playerUrl = new Uri(new Uri(baseUrl), @"/feeds/as3player/mrss.php?uri=" + playerUrl).AbsoluteUri;
                }
                //http://www.southparkstudios.com/feeds/as3player/mrss.php?uri=mgid:cms:content:southparkstudios.com:164823
                //http://www.southparkstudios.com/feeds/video-player/mrss/mgid%3Acms%3Acontent%3Asouthparkstudios.com%3A164823

                data = GetWebData(playerUrl);
                if (!string.IsNullOrEmpty(data))
                {
                    data = data.Replace("&amp;", "&");
                    data = data.Replace("&", "&amp;");
                    (video.Other as VideoInfoOtherHelper).SPCountry = spc;
                    foreach (RssItem item in RssToolkit.Rss.RssDocument.Load(data).Channel.Items)
                    {
                        if (item.Title.ToLowerInvariant().Contains("intro") || item.Title.ToLowerInvariant().Contains("vorspann")) continue;
                        if (video.PlaybackOptions == null)
                        {
                            var tmp = getPlaybackOptions(item.MediaGroups[0].MediaContents[0].Url, spc);
                            video.PlaybackOptions = tmp.Item1;
                            video.SubtitleText = tmp.Item2;
                        }
                        result.Add(item.MediaGroups[0].MediaContents[0].Url);
                    }
                }
            }
            return result;
        }

        Tuple<Dictionary<string, string>, string> getPlaybackOptions(string videoUrl, SouthParkCountry spc)
        {
            Dictionary<string, string> res = new Dictionary<string, string>();

            string data = GetWebData(videoUrl);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            XmlNodeList list = doc.SelectNodes("//src");
            for (int i = 0; i < list.Count; i++)
            {
                string bitrate = list[i].ParentNode.Attributes["bitrate"].Value;
                string videoType = list[i].ParentNode.Attributes["type"].Value.Replace(@"video/", String.Empty);
                string url = list[i].InnerText;

                string swfUrl = null;
                if (spc == SouthParkCountry.World)
                    url = url.Replace(@"viacomspstrmfs.fplive.net/viacomspstrm", @"cp10740.edgefcs.net/ondemand/mtvnorigin");
                /*switch (spc)
                {
                    case SouthParkCountry.World:
                    case SouthParkCountry.De: 
                        swfUrl = @"http://media.mtvnservices.com/player/prime/mediaplayerprime.1.11.3.swf"; break;
                }*/
                string br = bitrate + "K " + videoType;
                if (!res.ContainsKey(br))
                {
                    MPUrlSourceFilter.RtmpUrl rtmpUrl;
                    if (spc == SouthParkCountry.World)
                    {
                        rtmpUrl = new MPUrlSourceFilter.RtmpUrl(@"rtmpe://viacommtvstrmfs.fplive.net:1935/viacommtvstrm");
                        int p = url.IndexOf("gsp.comedystor");
                        if (p >= 0)
                            rtmpUrl.PlayPath = "mp4:" + url.Substring(p);
                    }
                    else
                        rtmpUrl = new MPUrlSourceFilter.RtmpUrl(url) { SwfVerify = swfUrl != null, SwfUrl = swfUrl };

                    res.Add(br, rtmpUrl.ToString());
                }

            }
            string subtitleText = null;
            if (enableSubtitles)
            {
                XmlNode sub = doc.SelectSingleNode("//transcript/typographic[@format='vtt' and @src]");
                if (sub != null)
                {
                    string url = sub.Attributes["src"].Value;
                    if (!String.IsNullOrEmpty(url))
                        subtitleText = Helpers.SubtitleUtils.Webvtt2SRT(GetWebData(url));
                }
            }
            return new Tuple<Dictionary<string, string>, string>(res, subtitleText);
        }

        public override string GetPlaylistItemVideoUrl(VideoInfo clonedVideoInfo, string chosenPlaybackOption, bool inPlaylist = false)
        {
            if (String.IsNullOrEmpty(chosenPlaybackOption))
                return clonedVideoInfo.VideoUrl;

            var result = getPlaybackOptions(clonedVideoInfo.VideoUrl, (clonedVideoInfo.Other as VideoInfoOtherHelper).SPCountry);
            Dictionary<string, string> options = result.Item1;
            clonedVideoInfo.SubtitleText = result.Item2;
            if (options.ContainsKey(chosenPlaybackOption))
            {
                return options[chosenPlaybackOption];
            }
            int bitRate = getBitrate(chosenPlaybackOption);
            Log.Debug("playbackoption " + chosenPlaybackOption + " not found, searching for nearest from " + bitRate.ToString());

            int nearestDiff = Int32.MaxValue;
            string bestUrl = null;
            foreach (var kv in options)
            {
                int diff = Math.Abs(bitRate - getBitrate(kv.Key));
                if (diff < nearestDiff)
                {
                    Log.Debug("better: " + kv.Key);
                    nearestDiff = diff;
                    bestUrl = kv.Value;
                }
            }
            return bestUrl;
        }

        private int getBitrate(string playbackOption)
        {
            int p = 0;
            while (p < playbackOption.Length && playbackOption[p] >= '0' && playbackOption[p] <= '9')
                p++;
            int bitRate;
            if (Int32.TryParse(playbackOption.Substring(0, p), out bitRate))
                return bitRate;
            return 0;
        }

        private class VideoInfoOtherHelper
        {
            public SouthParkCountry SPCountry = SouthParkCountry.Unknown;

            public VideoInfoOtherHelper()
            {
            }

            public VideoInfoOtherHelper(SouthParkCountry spCountry)
            {
                this.SPCountry = spCountry;
            }
        }

    }
}
