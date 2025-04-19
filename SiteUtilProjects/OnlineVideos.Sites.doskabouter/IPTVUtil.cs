using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using OnlineVideos.MPUrlSourceFilter;

namespace OnlineVideos.Sites
{
    public class IPTVUtil : GenericSiteUtil
    {
        [Category("OnlineVideosUserConfiguration"), Description("Url of your m3u8")]
        protected string m3u8urlPrivate = "";

        [Category("OnlineVideosConfiguration"), Description("Url of your m3u8")]
        protected string m3u8url = "";

        private static readonly Regex extinfReg = new Regex(@"\#EXTINF[^\s]*\s(?:(?=.*(tvg-name=""(?<tvgname>[^""]*)"")))?(?:(?=.*(tvg-id=""(?<tvgid>[^""]*)"")))?(?=.*(tvg-logo=""(?<tvglogo>[^""]*)""))(?=.*(group-title=""(?<grouptitle>[^""]*)""))", RegexOptions.IgnoreCase);

        SortedList<string, SortedList<string, SortedList<string, IPTVStream>>> groups = new SortedList<string, SortedList<string, SortedList<string, IPTVStream>>>();

        public override int DiscoverDynamicCategories()
        {
            if (!String.IsNullOrEmpty(m3u8urlPrivate))
                m3u8url = m3u8urlPrivate;
            var data = GetWebData(m3u8url);
            using (StringReader sr = new StringReader(data))
            {
                string line = sr.ReadLine();
                if (line != null && line.StartsWith("#EXTM3U"))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Match m = extinfReg.Match(line);
                        if (m.Success)
                        {
                            IPTVStream stream = new IPTVStream()
                            {
                                tvgname = (m.Groups["tvgname"].Success ? m.Groups["tvgname"].Value : m.Groups["tvgid"].Value).Replace(" H.265", ""),
                                grouptitle = m.Groups["grouptitle"].Value.Replace(" Terugkijken + Overig", "").Replace(" KANALEN", "").Replace(" HEVC H.265", "").Replace('|', '∣'),
                                reso = m.Groups["reso"].Value,
                                logo = m.Groups["tvglogo"].Value
                            };
                            var nextLine = sr.ReadLine();
                            while (nextLine.StartsWith("#"))
                            {
                                stream.addOption(nextLine);
                                nextLine = sr.ReadLine();
                            };
                            stream.url = nextLine;
                            foreach (var groupName in stream.grouptitle.Split(';'))
                            {
                                if (!groups.ContainsKey(groupName))
                                    groups.Add(groupName, new SortedList<string, SortedList<string, IPTVStream>>(StringComparer.CurrentCultureIgnoreCase));
                                var group = groups[groupName];
                                if (!group.ContainsKey(stream.tvgname))
                                    group.Add(stream.tvgname, new SortedList<string, IPTVStream>());
                                var channel = group[stream.tvgname];
                                if (!channel.ContainsKey(stream.reso))
                                    channel.Add(stream.reso, stream);
                            }
                        }

                    }
                }
            }
            if (Settings.Categories == null) Settings.Categories = new BindingList<Category>();
            foreach (var group in groups)
            {
                Category cat = new Category()
                {
                    Name = group.Key,
                    Other = group.Value
                };
                Settings.Categories.Add(cat);
            }
            Settings.DynamicCategoriesDiscovered = true;
            return Settings.Categories.Count;
        }

        private VideoInfo ConvertToVideo(string key, SortedList<string, IPTVStream> value)
        {
            VideoInfo video = new VideoInfo()
            {
                Title = key,
                Other = value,
                PlaybackOptions = new Dictionary<string, string>()
            };
            foreach (var res in value)
            {
                HttpUrl httpUrl = new HttpUrl(res.Value.url);
                if (!String.IsNullOrEmpty(res.Value.useragent))
                    httpUrl.UserAgent = res.Value.useragent;
                else
                    httpUrl.UserAgent = OnlineVideoSettings.Instance.UserAgent;
                if (!String.IsNullOrEmpty(res.Value.referer))
                    httpUrl.Referer = res.Value.referer;
                httpUrl.LiveStream = String.IsNullOrEmpty(Path.GetExtension(res.Value.url));
                video.PlaybackOptions.Add(res.Key, httpUrl.ToString());
                video.Thumb = res.Value.logo;
            }
            video.VideoUrl = video.GetPreferredUrl(true);
            return video;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            var vids = (SortedList<string, SortedList<string, IPTVStream>>)category.Other;
            List<VideoInfo> videos = new List<VideoInfo>();
            foreach (var vid in vids)
            {
                try
                {
                    var video = ConvertToVideo(vid.Key, vid.Value);
                    videos.Add(video);
                }
                catch (Exception e)
                {
                    OnlineVideos.Log.Debug("Error creating video " + e.Message);
                }
            }
            return videos;
        }

        public override string GetVideoUrl(VideoInfo video)
        {
            return video.GetPreferredUrl(true);
        }

        public override bool CanSearch { get { return true; } }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            var res = new List<SearchResultItem>();

            foreach (var cat in Settings.Categories)
            {
                var vids = (SortedList<string, SortedList<string, IPTVStream>>)cat.Other;
                foreach (var vid in vids)
                {
                    if (vid.Key.ToLowerInvariant().Contains(query.ToLowerInvariant()))
                    {
                        VideoInfo video = ConvertToVideo(vid.Key, vid.Value);
                        res.Add(video as SearchResultItem);
                    }
                }
            }

            return res;
        }

    }

    class IPTVStream
    {
        public string tvgname;
        public string grouptitle;
        public string reso;
        public string url;
        public string logo;
        public string referer;
        public string useragent;
        public override string ToString()
        {
            return tvgname + " " + grouptitle;
        }

        public void addOption(string s)
        {
            if (s.StartsWith("#EXTVLCOPT:"))
            {
                var rest = s.Substring(11).Split('=');
                switch (rest[0])
                {
                    case "http-referrer": { referer = rest[1]; break; }
                    case "http-user-agent": { useragent = rest[1]; break; }
                }
            }
        }
    }
}
