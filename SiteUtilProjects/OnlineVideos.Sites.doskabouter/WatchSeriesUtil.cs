﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OnlineVideos.Sites
{
    public class WatchSeriesUtil : DeferredResolveUtil
    {
        [Category("OnlineVideosUserConfiguration"), Description("Proxy to use for WebRequests. Define like this: 83.84.85.86:8116")]
        string proxy = null;

        [Category("OnlineVideosConfiguration")]
        protected string azRegEx;

        private enum Depth { MainMenu = 0, Alfabet = 1, Series = 2, Seasons = 3, BareList = 4 };
        public CookieContainer cc = null;
        private string nextVideoListPageUrl = null;
        private string nextVideoListPostData = null;
        private Category currCategory = null;

        private WebProxy webProxy = null;
        private Regex regex_Az;
        #region singleton
        private WebProxy GetProxy()
        {
            if (webProxy == null && !String.IsNullOrEmpty(proxy))
                webProxy = new WebProxy(proxy);
            return webProxy;
        }
        #endregion



        /*public void GetBaseCookie()
        {
            HttpWebRequest request = WebRequest.Create(baseUrl) as HttpWebRequest;
            if (request == null) return;
            request.UserAgent = OnlineVideoSettings.Instance.UserAgent;
            request.Accept = "* /*";
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.CookieContainer = new CookieContainer();
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            finally
            {
                if (response != null) ((IDisposable)response).Dispose();
            }

            cc = new CookieContainer();
            CookieCollection ccol = request.CookieContainer.GetCookies(new Uri(baseUrl));
            foreach (Cookie c in ccol)
                cc.Add(c);
        }
        */

        public override int DiscoverDynamicCategories()
        {
            //GetBaseCookie();
            regex_Az = new Regex(azRegEx, defaultRegexOptions);
            base.DiscoverDynamicCategories();
            int i = 0;
            do
            {
                RssLink cat = (RssLink)Settings.Categories[i];
                if (cat.Name.ToUpperInvariant() == "HOME" || cat.Name.ToUpperInvariant() == "HOW TO WATCH" ||
                    cat.Name.ToUpperInvariant() == "CONTACT" || cat.Name.ToUpperInvariant() == "ABOUT US" ||
                    cat.Name.ToUpperInvariant() == "SPORT"
                   )
                    Settings.Categories.Remove(cat);
                else
                {
                    if (cat.Name == "Series")
                    {
                        cat.Other = Depth.MainMenu;
                    }
                    else
                    {
                        cat.Other = Depth.BareList;
                        cat.HasSubCategories = false;
                    }
                    i++;
                }
            }
            while (i < Settings.Categories.Count);
            return Settings.Categories.Count;
        }

        public override int DiscoverSubCategories(Category parentCategory)
        {
            return GetSubCategories(parentCategory, ((RssLink)parentCategory).Url);
        }

        private int GetSubCategories(Category parentCategory, string url)
        {
            string webData;
            int p = url.IndexOf('#');
            if (p >= 0)
            {
                string nm = url.Substring(p + 1);
                webData = GetWebData(url.Substring(0, p), cookies: cc, forceUTF8: true);
                webData = @"class=""listbig"">" + Helpers.StringUtils.GetSubString(webData, @"class=""listbig""><a name=""" + nm + @"""", @"class=""listbig""");
            }
            else
                webData = GetWebData(url, cookies: cc, forceUTF8: true);

            parentCategory.SubCategories = new List<Category>();
            Match m = null;
            switch ((Depth)parentCategory.Other)
            {
                case Depth.MainMenu:
                    webData = Helpers.StringUtils.GetSubString(webData, @"class=""pagination""", @"class=""listbig""");
                    m = regex_Az.Match(webData);
                    break;
                case Depth.Alfabet:
                    //webData = Helpers.StringUtils.GetSubString(webData, @"class=""listbig""", @"class=""clear""");
                    m = regEx_dynamicSubCategories.Match(webData);
                    break;
                case Depth.Series:
                    Match m2 = Regex.Match(webData, @"imdb\.com/title/(?<imdbId>[^/]*)/");
                    Match m3 = Regex.Match(webData, @"function\sloadMoreLinks\(season\)\s{\s*\$\.post\(\s*'(?<url>[^']*)',\s*{(?<postdata>[^}]*)}", defaultRegexOptions);
                    webData = Helpers.StringUtils.GetSubString(webData, @"class=""lists"" >", @"class=""clear"" ");
                    string[] tmp = { @"class=""lists"" >" };
                    string[] seasons = webData.Split(tmp, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in seasons)
                    {
                        SeriesRssLink cat = new SeriesRssLink();
                        if (m2.Success)
                            cat.imDbId = m2.Groups["imdbId"].Value;

                        cat.Name = HttpUtility.HtmlDecode(Helpers.StringUtils.GetSubString(s, @"name"">", "<")).Trim();
                        cat.Url = s;
                        cat.SubCategoriesDiscovered = true;
                        cat.HasSubCategories = false;
                        Match m4 = Regex.Match(s, @"javascript:loadMoreLinks\((?<season>\d+)\)");
                        if (m4.Success && m3.Success)
                        {
                            string posturl = m3.Groups["url"].Value;
                            Match m5 = Regex.Match(m3.Groups["postdata"].Value, @"'(?<key>[^']*)'\s*:\s*(?:')?(?<value>[^',]*)");
                            string postData = "";
                            while (m5.Success)
                            {
                                postData += "&" + m5.Groups["key"].Value + "=";
                                if (m5.Groups["key"].Value == "season")
                                    postData += m4.Groups["season"].Value;
                                else
                                    postData += m5.Groups["value"].Value;
                                m5 = m5.NextMatch();
                            }
                            cat.Other = new Tuple<string, string>(posturl, postData.Substring(1));
                        }
                        else
                            cat.Other = ((Depth)parentCategory.Other) + 1;

                        parentCategory.SubCategories.Add(cat);
                        cat.ParentCategory = parentCategory;
                    }
                    break;
                default:
                    m = null;
                    break;
            }

            while (m != null && m.Success)
            {
                RssLink cat = new RssLink();
                cat.Name = HttpUtility.HtmlDecode(m.Groups["title"].Value).Trim();
                cat.Url = m.Groups["url"].Value;
                if (!String.IsNullOrEmpty(cat.Url) && !Uri.IsWellFormedUriString(cat.Url, System.UriKind.Absolute))
                    cat.Url = new Uri(new Uri(baseUrl), cat.Url).AbsoluteUri;

                cat.Description = HttpUtility.HtmlDecode(m.Groups["description"].Value);
                cat.HasSubCategories = !parentCategory.Other.Equals(Depth.Series);
                cat.Other = ((Depth)parentCategory.Other) + 1;

                if (cat.Name == "NEW")
                {
                    cat.HasSubCategories = false;
                    cat.Other = Depth.BareList;
                }

                parentCategory.SubCategories.Add(cat);
                cat.ParentCategory = parentCategory;
                m = m.NextMatch();
            }

            parentCategory.SubCategoriesDiscovered = true;
            return parentCategory.SubCategories.Count;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            return getOnePageVideoList(category, ((RssLink)category).Url);
        }

        private List<VideoInfo> getOnePageVideoList(Category category, string url)
        {
            currCategory = category;
            string webData;
            nextVideoListPageUrl = null;
            nextVideoListPostData = null;
            if (category.Other.Equals(Depth.BareList))
            {
                webData = GetWebData(url, cookies: cc, forceUTF8: true);
                if (regEx_NextPage != null)
                {
                    Match mNext = regEx_NextPage.Match(webData);
                    if (mNext.Success)
                        nextVideoListPageUrl = FormatDecodeAbsolutifyUrl(url, mNext.Groups["url"].Value, nextPageRegExUrlFormatString, nextPageRegExUrlDecoding);
                    else
                        nextVideoListPageUrl = null;
                }
            }
            else
            {
                var nextPage = category.Other as Tuple<string, string>;
                if (nextPage != null)
                {
                    nextVideoListPageUrl = nextPage.Item1;
                    nextVideoListPostData = nextPage.Item2;
                }
                webData = url;
            }

            List<VideoInfo> videos = new List<VideoInfo>();
            if (!string.IsNullOrEmpty(webData))
            {
                Match m = regEx_VideoList.Match(webData);
                while (m.Success)
                {
                    VideoInfo video = CreateVideoInfo();

                    video.Title = HttpUtility.HtmlDecode(m.Groups["Title"].Value);
                    video.VideoUrl = m.Groups["VideoUrl"].Value;
                    if (!String.IsNullOrEmpty(video.VideoUrl) && !Uri.IsWellFormedUriString(video.VideoUrl, System.UriKind.Absolute))
                        video.VideoUrl = new Uri(new Uri(baseUrl), video.VideoUrl).AbsoluteUri;
                    video.Airdate = m.Groups["Airdate"].Value;
                    if (video.Airdate == "-")
                        video.Airdate = String.Empty;

                    try
                    {
                        TrackingInfo tInfo = new TrackingInfo()
                        {
                            Regex = Regex.Match(video.Title, @"(?<Title>.+?)\s+(?:-)?\s*Seas(?:\.|on)\s*?(?<Season>\d+)\s+(?:-)?\s*Ep(?:\.|isode)\s*?(?<Episode>\d+)", RegexOptions.IgnoreCase),
                            VideoKind = VideoKind.TvSeries
                        };

                        if (tInfo.Season == 0 &&
                            category != null && category.ParentCategory != null &&
                            !string.IsNullOrEmpty(category.Name) && !string.IsNullOrEmpty(category.ParentCategory.Name))
                        {
                            // 2nd way - using parent category name, category name and video title 
                            //Aaron Stone Season 1 (19 episodes) 1. Episode 21 1 Hero Rising (1)
                            string parseString = string.Format("{0} {1} {2}", Regex.Replace(category.ParentCategory.Name, @"\(\d{4}\)", ""), category.Name, video.Title);
                            tInfo.Regex = Regex.Match(parseString, @"(?<Title>.+?)\s+Season\s*?(?<Season>\d+).*?Episode\s*?(?<Episode>\d+)", RegexOptions.IgnoreCase);
                        }

                        if (tInfo.Season == 0 && !String.IsNullOrEmpty(m.Groups["epid"].Value))
                        {
                            tInfo.Regex = Regex.Match(m.Groups["epid"].Value + ' ' + video.Title, @"(?<Season>\d+)x(?<Episode>\d+)\s(?<Title>.*)", RegexOptions.IgnoreCase);
                        }
                        if (tInfo.Season != 0)
                        {
                            if (category is SeriesRssLink)
                                tInfo.ID_IMDB = ((SeriesRssLink)category).imDbId;
                            video.Other = tInfo;
                            Log.Debug(String.Format("Trackinginfo: {0} Season: {1} Episode: {2}", tInfo.Title, tInfo.Season, tInfo.Episode));
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Error parsing TrackingInfo data: {0}", e.ToString());
                    }

                    videos.Add(video);
                    m = m.NextMatch();
                }

            }
            return videos;
        }

        public override bool HasNextPage
        {
            get
            {
                return nextVideoListPageUrl != null;
            }
        }

        public override List<VideoInfo> GetNextPageVideos()
        {
            if (currCategory.Other.Equals(Depth.BareList))
                return getOnePageVideoList(currCategory, nextVideoListPageUrl);
            else
            {
                //next page for series with large number of episodes
                var webData = GetWebData(nextVideoListPageUrl, postData: nextVideoListPostData, cookies: cc, forceUTF8: true);
                nextVideoListPageUrl = null;
                var json = JToken.Parse(webData);
                List<VideoInfo> videos = new List<VideoInfo>();
                var seriesName = "";
                var seriesTitle = "";
                if (currCategory.ParentCategory != null)
                {
                    seriesName = ((RssLink)currCategory.ParentCategory).Url;
                    seriesTitle = currCategory.ParentCategory.Name;
                    int p = seriesName.LastIndexOf('/');
                    if (p >= 0)
                        seriesName = seriesName.Substring(p + 1);
                }

                foreach (var vid in json)
                {
                    VideoInfo video = CreateVideoInfo();

                    video.Title = String.Format("Episode {0} ", vid.Value<uint>("episode")) + vid.Value<string>("name");
                    video.VideoUrl = new Uri(new Uri(baseUrl), "episode/" + seriesName + "_s" + vid.Value<string>("season") + "_e" + vid.Value<uint>("episode") + ".html").AbsoluteUri;
                    video.Airdate = vid.Value<string>("aired");
                    video.Description = vid.Value<string>("description");

                    TrackingInfo tInfo = new TrackingInfo()
                    {
                        VideoKind = VideoKind.TvSeries,
                        Title = seriesTitle,
                        Season = vid.Value<uint>("season"),
                        Episode = vid.Value<uint>("episode")
                    };
                    video.Other = tInfo;
                    videos.Add(video);
                }

                return videos;
            }
        }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            List<SearchResultItem> cats = new List<SearchResultItem>();

            Regex r = new Regex(@"<img\ssrc=""(?<thumb>[^""]*)""[^>]*>\s*</div>\s*<div[^>]*>\s*<h3><i[^>]*></i></h3>\s*</div>\s*</a>\s*</div>\s*</div>\s*<div[^>]*>\s*<a\shref=""(?<url>[^""]*)""[^>]*><strong>(?<title>[^<]*)</strong></a>\s*<br\s/>\s*<strong>Description:</strong>(?<description>[^<]*)<br/>", defaultRegexOptions);

            string webData = GetWebData(String.Format(searchUrl, query), forceUTF8: true);
            Match m = r.Match(webData);
            while (m.Success)
            {
                RssLink cat = new RssLink();
                cat.Url = m.Groups["url"].Value;
                if (!string.IsNullOrEmpty(dynamicSubCategoryUrlFormatString)) cat.Url = string.Format(dynamicSubCategoryUrlFormatString, cat.Url);
                cat.Url = ApplyUrlDecoding(cat.Url, dynamicSubCategoryUrlDecoding);
                if (!Uri.IsWellFormedUriString(cat.Url, System.UriKind.Absolute)) cat.Url = new Uri(new Uri(baseUrl), cat.Url).AbsoluteUri;
                cat.Name = HttpUtility.HtmlDecode(m.Groups["title"].Value.Trim());
                cat.Thumb = m.Groups["thumb"].Value;
                if (!String.IsNullOrEmpty(cat.Thumb) && !Uri.IsWellFormedUriString(cat.Thumb, System.UriKind.Absolute)) cat.Thumb = new Uri(new Uri(baseUrl), cat.Thumb).AbsoluteUri;
                cat.Description = m.Groups["description"].Value;
                cat.Other = Depth.Series;
                cat.HasSubCategories = true;
                cats.Add(cat);
                m = m.NextMatch();
            }

            return cats;
        }

        public override ITrackingInfo GetTrackingInfo(VideoInfo video)
        {
            if (video.Other is ITrackingInfo)
                return video.Other as ITrackingInfo;

            return base.GetTrackingInfo(video);
        }

        public override string GetFileNameForDownload(VideoInfo video, Category category, string url)
        {
            string pre = "";
            if (category != null && category.ParentCategory != null && !category.Other.Equals(Depth.BareList))
            {
                string season = category.Name.Split('(')[0];
                pre = category.ParentCategory.Name + ' ' + season + ' ' + pre;
                int l;
                do
                {
                    l = pre.Length;
                    pre = pre.Replace("  ", " ");
                } while (l != pre.Length);
            }

            if (string.IsNullOrEmpty(url)) // called for adding to favorites
                return pre + video.Title;
            else // called for downloading
            {
                string name = base.GetFileNameForDownload(video, category, url);
                string extension = Path.GetExtension(name);
                if (String.IsNullOrEmpty(extension) || !OnlineVideoSettings.Instance.VideoExtensions.ContainsKey(extension))
                    name += ".flv";
                name = pre + name;
                return Helpers.FileUtils.GetSaveFilename(name);
            }
        }

        protected override CookieContainer GetCookie()
        {
            return cc;
        }

        public override string ResolveVideoUrl(string url)
        {
            //http://onwatchseries.to/cale.html?r=aHR0cDovL2dvcmlsbGF2aWQuaW4vN2MzcGlzZmIybWgx
            int p = url.IndexOf("?r=");
            if (p >= 0)
            {
                string hoster = Encoding.ASCII.GetString(Convert.FromBase64String(url.Substring(p + 3)));
                return GetVideoUrl(hoster);
            }
            else
                return String.Empty;
        }

    }

    public class SeriesRssLink : RssLink
    {
        public string imDbId = null;
    }

}
