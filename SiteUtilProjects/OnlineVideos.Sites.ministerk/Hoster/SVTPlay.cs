﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace OnlineVideos.Hoster
{
    public class SVTPlay : HosterBase, ISubtitle
    {
        private string subtitleText = null;

        public override string GetHosterUrl()
        {
            return "svtplay.se";
        }

        public string SubtitleText
        {
            get { return subtitleText; }
        }

        public override Dictionary<string, string> GetPlaybackOptions(string url)
        {
            Dictionary<string, string> playbackOptions = new Dictionary<string, string>();
            if (url.ToLower().Contains("oppetarkiv"))
            {
                JToken videoToken = GetWebData<JObject>(url + "?output=json")["video"];
                url = "";
                subtitleText = "";
                try
                {
                    var subtitleReferences = videoToken["subtitleReferences"].Where(sr => ((string)sr["url"] ?? "").EndsWith("srt") || ((string)sr["url"] ?? "").EndsWith("index.m3u8"));
                    if (subtitleReferences != null && subtitleReferences.Count() > 0)
                    {
                        url = (string)subtitleReferences.First()["url"];
                        if (!string.IsNullOrEmpty(url))
                        {
                            if (url.EndsWith("index.m3u8"))
                            {
                                string baseUrl = url.Replace("index.m3u8", string.Empty);
                                //try with all.vtt
                                subtitleText = GetWebData(baseUrl + "all.vtt");
                                CleanSubtitle(true);
                            }
                            else
                            {
                                subtitleText = GetWebData(url);
                                CleanSubtitle();
                            }
                        }
                    }
                }
                catch { }

                JToken videoReference = videoToken["videoReferences"].FirstOrDefault(vr => (string)vr["playerType"] == "flash" && !string.IsNullOrEmpty((string)vr["url"]));
                if (videoReference == null)
                {
                    url = "";
                }
                else
                {
                    Boolean live = false;
                    JValue liveVal = (JValue)videoToken["live"];
                    if (liveVal != null)
                        live = liveVal.Value<bool>();
                    url = (string)videoReference["url"] + "?hdcore=3.7.0&g=" + OnlineVideos.Sites.Utils.HelperUtils.GetRandomChars(12);
                    url = new MPUrlSourceFilter.AfhsManifestUrl(url)
                    {
                        LiveStream = live,
                        Referer = "http://media.svt.se/swf/video/svtplayer-2015.01.swf"
                    }.ToString();
                }
                if (!string.IsNullOrWhiteSpace(url))
                    playbackOptions.Add("url", url);
            }
            else
            {
                subtitleText = "";
                string data = GetWebData<string>(url);
                string rgxString = @"""(?<url>[^""]*?master\.m3u8[^""]*)""";
                if (url.Contains("http://www.svtplay.se/kanaler/"))
                {
                    url = url.Replace("http://www.svtplay.se/kanaler/", string.Empty);
                    rgxString = @"title"":""" + url + @""".*?""(?<url>[^""]*?master\.m3u8[^""]*)""";
                }
                Regex rgx = new Regex(rgxString);
                Match m = rgx.Match(data);
                if (!m.Success)
                    return playbackOptions;
                string m3u8Url = m.Groups["url"].Value;
                string m3u8 = GetWebData(m3u8Url);
                rgx = new Regex(@"BANDWIDTH=(?<bandwidth>\d+)000.[^\n]*?\n(?<url>[^\n]*)", RegexOptions.Singleline);
                foreach (Match match in rgx.Matches(m3u8))
                {
                    string key = match.Groups["bandwidth"].Value + " kbps";
                    if (!playbackOptions.ContainsKey(key))
                    {
                        string value = match.Groups["url"].Value;
                        if (!value.StartsWith("http"))
                            value = m3u8Url.Remove(m3u8Url.IndexOf("master.m3u8")) + value;
                        playbackOptions.Add(key, value);
                    }
                }

                playbackOptions = playbackOptions.OrderByDescending((p) =>
                {
                    string resKey = p.Key.Replace(" kbps", "");
                    int parsedRes = 0;
                    int.TryParse(resKey, out parsedRes);
                    return parsedRes;
                }).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                rgx = new Regex(@"""(?<url>[^""]*?index\.m3u8)""");
                m = rgx.Match(data);
                if (m.Success)
                {
                    subtitleText = GetWebData(m.Groups["url"].Value.Replace("index.m3u8", "all.vtt"));
                    CleanSubtitle(true);
                }
            }

            return playbackOptions;
        }

        public override string GetVideoUrl(string url)
        {
            Dictionary<string, string> urls = GetPlaybackOptions(url);
            if (urls.Count > 0)
                return urls.First().Value;
            else
                return "";
        }

        void CleanSubtitle(bool isVtt = false)
        {
            Regex rgx;
            if (isVtt)
            {
                //Remove WEBVTT stuff

                //This is if we don't use all.vtt...
                //rgx = new Regex(@"WEBVTT.*?00:00:00.000", RegexOptions.Singleline);
                //subtitleText = rgx.Replace(subtitleText, new MatchEvaluator((Match m) =>
                //{
                 //   return string.Empty;
                //}));
                rgx = new Regex(@"WEBVTT");
                subtitleText = rgx.Replace(subtitleText, new MatchEvaluator((Match m) =>
                {
                    return string.Empty;
                }));
            }
            // For some reason the time codes in the subtitles from Öppet arkiv starts @ 10 hours. replacing first number in the
            // hour position with 0. Hope and pray there will not be any shows with 10+ h playtime...
            // Remove all trailing stuff, ie in 00:45:21.960 --> 00:45:25.400 A:end L:82%
            rgx = new Regex(@"\d(\d:\d\d:\d\d\.\d\d\d)\s*-->\s*\d(\d:\d\d:\d\d\.\d\d\d).*$", RegexOptions.Multiline);
            subtitleText = rgx.Replace(subtitleText, new MatchEvaluator((Match m) =>
            {
                return "0" + m.Groups[1].Value + " --> 0" + m.Groups[2].Value + "\r";
            }));

            rgx = new Regex(@"</{0,1}[^>]+>");
            subtitleText = rgx.Replace(subtitleText, string.Empty);
            if (isVtt)
            {
                rgx = new Regex(@"(?<time>\d\d:\d\d:\d\d\.\d\d\d\s*?-->\s*?\d\d:\d\d:\d\d\.\d\d\d)");
                int i = 0;
                foreach (Match m in rgx.Matches(subtitleText))
                {
                    i++;
                    string time = m.Groups["time"].Value;
                    subtitleText = subtitleText.Replace(time, i + "\r\n" + time);
                }
            }
            subtitleText = HttpUtility.HtmlDecode(subtitleText);
        }

    }
}
