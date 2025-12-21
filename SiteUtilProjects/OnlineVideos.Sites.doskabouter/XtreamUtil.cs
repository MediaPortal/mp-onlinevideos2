using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using System.ComponentModel;

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

        public override int DiscoverDynamicCategories()
        {
            var data = GetWebData<JToken>(getUrl() + "get_live_categories");
            foreach (var obj in data)
            {
                RssLink cat = new RssLink()
                {
                    Name = obj.Value<string>("category_name"),
                    Url = getUrl() + "get_live_streams&category_id=" + obj.Value<string>("category_id")
                };
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
                    VideoInfo vid = new VideoInfo()
                    {
                        Title = obj.Value<string>("name"),
                        Thumb = obj.Value<string>("stream_icon"),
                        VideoUrl = basePath + '/' + userName + '/' + password + '/' + obj.Value<string>("stream_id")
                    };
                    res.Add(vid);
                }
            }
            return res;
        }

        private string getUrl()
        {
            return basePath + "/player_api.php?username=" + userName + "&password=" + password + "&action=";
        }
    }

}
