﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json.Linq;

namespace OnlineVideos.Sites
{
    public class DailyMotionUtil : SiteUtilBase
    {
        string api_base_url = "https://api.dailymotion.com";
        string api_channel_list_url = "/channels?sort=popular";
        string api_channel_videos_url = "/channel/{0}/videos?fields=created_time,description%2Cduration%2Cembed_url%2Cthumbnail_240_url%2Ctitle&limit=50&page={1}";
        string api_channel_videos_search_url = "/channel/{0}/videos?fields=created_time,description%2Cduration%2Cembed_url%2Cthumbnail_240_url%2Ctitle&limit=50&search={1}&page={2}";
        string api_video_search_url = "/videos?fields=created_time,description%2Cduration%2Cembed_url%2Cthumbnail_240_url%2Ctitle&limit=50&search={0}&page={1}";

        int current_videos_page = 1;
        string current_videos_url = "";

        public override int DiscoverDynamicCategories()
        {
            Settings.Categories.Clear();
            foreach (JObject jChannel in GetWebData<JObject>(api_base_url + api_channel_list_url)["list"])
            {
                Settings.Categories.Add(new RssLink()
                {
                    Name = jChannel.Value<string>("name"),
                    Description = jChannel.Value<string>("description"),
                    Url = jChannel.Value<string>("id")
                });
            }
            Settings.DynamicCategoriesDiscovered = Settings.Categories.Count > 0;
            return Settings.Categories.Count;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            return VideosFromJson(api_base_url + string.Format(api_channel_videos_url, (category as RssLink).Url, 1));
        }

        public override bool CanSearch { get { return true; } }

        public override Dictionary<string, string> GetSearchableCategories()
        {
            return Settings.Categories.ToDictionary(c => c.Name, c => ((RssLink)c).Url);
        }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            if (string.IsNullOrEmpty(category))
                return VideosFromJson(api_base_url + string.Format(api_video_search_url, HttpUtility.UrlEncode(query), 1))
                    .ConvertAll<SearchResultItem>(v => v as SearchResultItem);
            else
                return VideosFromJson(api_base_url + string.Format(api_channel_videos_search_url, category, HttpUtility.UrlEncode(query), 1))
                    .ConvertAll<SearchResultItem>(v => v as SearchResultItem);
        }

        public override List<VideoInfo> GetNextPageVideos()
        {
            return VideosFromJson(current_videos_url.Replace("&page=" + current_videos_page, "&page=" + (current_videos_page + 1).ToString()));
        }

        public override string GetVideoUrl(VideoInfo video)
        {
            video.PlaybackOptions = Hoster.HosterFactory.GetHoster("DailyMotion").GetPlaybackOptions(video.VideoUrl);
            return video.PlaybackOptions.Last().Value;
        }

        List<VideoInfo> VideosFromJson(string url)
        {
            var result = new List<VideoInfo>();

            JObject json = GetWebData<JObject>(url);

            HasNextPage = json.Value<bool>("has_more");
            current_videos_page = json.Value<int>("page");
            current_videos_url = url;

            foreach (JObject jVideo in json["list"])
            {
                result.Add(new VideoInfo()
                {
                    Title = jVideo.Value<string>("title"),
                    Description = jVideo.Value<string>("description"),
                    Airdate = Helpers.TimeUtils.UNIXTimeToDateTime(jVideo.Value<double>("created_time")).ToString("g", OnlineVideoSettings.Instance.Locale),
                    Length = jVideo.Value<string>("duration"),
                    Thumb = jVideo.Value<string>("thumbnail_240_url"),
                    VideoUrl = jVideo.Value<string>("embed_url")
                });
            }
            return result;
        }
    }
}
