using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using System.ComponentModel;
using System.Linq;
using System;

namespace OnlineVideos.Sites
{
    public class XtreamUtil : SiteUtilBase
    {
        [Category("OnlineVideosUserConfiguration"), Description("The base url of the server (f.e. https://servername.com:8080")]
        private string basePath = null;

        [Category("OnlineVideosUserConfiguration"), Description("The username for accessing the server")]
        private string userName = null;

        [Category("OnlineVideosUserConfiguration"), Description("The password for accessing the server")]
        private string password = null;

        private Dictionary<string, string> categoryNames = new Dictionary<string, string>();

        public override int DiscoverDynamicCategories()
        {
            var data = GetWebData<JToken>(getUrl() + "get_live_categories");
            foreach (var obj in data)
            {
                var name = obj.Value<string>("category_name").Replace('|', '∣');
                var id = obj.Value<string>("category_id");
                RssLink cat = new RssLink()
                {
                    Name = name,
                    Url = getUrl() + "get_live_streams&category_id=" + id
                };
                categoryNames[id] = name;
                Settings.Categories.Add(cat);
            }
            Settings.DynamicCategoriesDiscovered = true;
            return Settings.Categories.Count;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            var res = new List<VideoInfo>();
            RssLink cat = category as RssLink;
            if (cat != null)
            {
                var data = GetWebData<JToken>(cat.Url);
                foreach (var obj in data)
                {
                    var vid = objToVideo(obj);
                    res.Add(vid);
                }
            }
            return res;
        }

        public override bool CanSearch { get { return true; } }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            var res = new List<SearchResultItem>();
            var search = query.Trim().ToLowerInvariant();

            var data = GetWebData<JToken>(getUrl() + "get_live_streams");
            foreach (var obj in data)
            {
                var vid = objToVideo(obj);

                if (match(search, vid.Title.Trim().ToLowerInvariant()))
                {
                    var id = obj.Value<string>("category_id");
                    if (categoryNames.TryGetValue(id, out string catName))
                        vid.Description = catName;
                    else
                        search = search;
                    res.Add(vid);
                }
            }

            return res;
        }

        private bool match(string search, string name)
        {
            if (name.Contains(search))
                return true;

            var searchWords = search.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var nameWords = name.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return searchWords.All(sw =>
                nameWords.Any(nw => nw.StartsWith(sw)));
        }

        private string getUrl()
        {
            return basePath + "/player_api.php?username=" + userName + "&password=" + password + "&action=";
        }

        private VideoInfo objToVideo(JToken obj)
        {
            return new VideoInfo()
            {
                Title = obj.Value<string>("name"),
                Thumb = obj.Value<string>("stream_icon"),
                VideoUrl = basePath + '/' + userName + '/' + password + '/' + obj.Value<string>("stream_id")
            };
        }
    }

}
