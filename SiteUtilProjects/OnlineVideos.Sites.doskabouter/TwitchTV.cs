using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using OnlineVideos.Helpers;

namespace OnlineVideos.Sites
{
    /// <summary>
    /// Twitch API docs can be found here: https://dev.twitch.tv/docs/v5/
    /// </summary>
    public class TwitchTVUtil : SiteUtilBase, IWebViewSiteUtilBase
    {
        string clientID = "3jqzelqamssns2ybboe0ps2o6jo4tw";
        string clientSecret = "mysecret";
        string baseApiUrl = "https://api.twitch.tv/helix";
        string gamesUrl = "/games/top?first=40";
        string streamsUrl = "/streams?game_id={0}";
        string searchUrl = "/search/channels?query={0}&first=25&live_only=true";

        string nextPageUrl;

        private NameValueCollection customHeader;

        public override int DiscoverDynamicCategories()
        {
            var token = GetToken();
            customHeader = new NameValueCollection();
            customHeader.Add("Client-Id", clientID);
            customHeader.Add("Authorization", "Bearer " + token);
            Settings.Categories.Clear();
            return ParseCategories(baseApiUrl + gamesUrl);
        }

        private string GetToken()
        {
            string postData = String.Format("client_id={0}&client_secret={1}&grant_type=client_credentials", clientID, clientSecret);
            var tokenDataJson = GetWebData<JToken>(@"https://id.twitch.tv/oauth2/token", postData: postData);
            return tokenDataJson["access_token"].ToString();
        }

        public override int DiscoverNextPageCategories(NextPageCategory category)
        {
            Settings.Categories.Remove(category);
            return ParseCategories(category.Url);
        }

        private int ParseCategories(string url)
        {
            var games = GetWebData<JObject>(url, headers: customHeader);
            foreach (var game in games["data"])
            {
                Settings.Categories.Add(CategoryFromJsonGameObject(game));
            }
            Settings.DynamicCategoriesDiscovered = Settings.Categories.Count > 0;

            var cursor = games["pagination"]?.Value<String>("cursor");
            if (!String.IsNullOrEmpty(cursor))
                Settings.Categories.Add(new NextPageCategory() { Url = getNextPageUrl(url, cursor) });

            return Settings.Categories.Count;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            return VideosFromApiUrl(((RssLink)category).Url);
        }

        public override List<VideoInfo> GetNextPageVideos()
        {
            return VideosFromApiUrl(nextPageUrl);
        }

        public override bool CanSearch
        {
            get { return true; }
        }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            return VideosFromApiUrl(baseApiUrl + string.Format(searchUrl, HttpUtility.UrlEncode(query))).ConvertAll<SearchResultItem>(i => i as SearchResultItem);
        }

        List<VideoInfo> VideosFromApiUrl(string url)
        {

            List<VideoInfo> result = new List<VideoInfo>();

            var streams = GetWebData<JObject>(url, headers: customHeader);
            foreach (var stream in streams["data"])
            {
                result.Add(VideoFromJsonStreamObject(stream));
            }

            var cursor = streams["pagination"]?.Value<String>("cursor");
            nextPageUrl = getNextPageUrl(url, cursor);

            HasNextPage = (!string.IsNullOrEmpty(nextPageUrl));
            return result;
        }

        Category CategoryFromJsonGameObject(JToken game)
        {
            return new RssLink()
            {
                Name = game.Value<string>("name"),
                Url = baseApiUrl + string.Format(streamsUrl, game.Value<string>("id")),
                Other = game
            };
        }

        VideoInfo VideoFromJsonStreamObject(JToken stream)
        {
            return new VideoInfo()
            {
                Title = stream.Value<string>("title"),
                Thumb = stream.Value<string>("thumbnail_url").Replace("{width}", "200").Replace("{height}", "200"),
                Description = string.Format("{0} Viewers for {1}", stream.Value<string>("viewer_count"), stream.Value<string>("user_name")),
                Airdate = stream.Value<DateTime>("started_at").ToString("g", OnlineVideoSettings.Instance.Locale),
                VideoUrl = String.Format(@"https://player.twitch.tv/?channel={0}&parent=streamernews.example.com&muted=false", stream.Value<string>("user_login") ?? stream.Value<string>("broadcaster_login"))
            };
        }
        private string getNextPageUrl(string url, string cursor)
        {
            if (String.IsNullOrEmpty(cursor)) return null;
            int p = url.IndexOf("&after");
            if (p >= 0)
                url = url.Substring(0, p);
            return url + "&after=" + cursor;
        }

        WebViewHelper wvh = null;
        void INeedsWebView.SetWebviewHelper(WebViewHelper webViewHelper)
        {
            wvh = webViewHelper;
        }
        void IWebViewSiteUtilBase.StartPlayback()
        {
            System.Threading.Thread.Sleep(1000);
            wvh.Execute(@"document.querySelectorAll('[data-a-target=""player-mute-unmute-button""]')[0].click()");
            wvh.Execute(@"document.querySelectorAll('[data-a-target=""content-classification-gate-overlay-start-watching-button""]')[0].click()");
        }
    }

}
