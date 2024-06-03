using System;
using System.Collections.Specialized;
using System.Web;

namespace OnlineVideos
{
    public class MixedUrl
    {
        public const string MixedUrlScheme = "onlinevideos";
        public string VideoUrl;
        public string AudioUrl;
        public bool Valid;

        public MixedUrl(string videoUrl, string audioUrl) 
        {
            VideoUrl = videoUrl;
            AudioUrl = audioUrl;
            Valid = true;
        }

        public MixedUrl(string url)
        {
            var uri = new Uri(url);
            if (uri.Scheme == MixedUrl.MixedUrlScheme)
            {
                NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
                VideoUrl = args.Get("urlVideo");
                AudioUrl = args.Get("urlAudio");
                Valid = true;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}://127.0.0.1/VideoLink?urlVideo={1}&urlAudio={2}",
                                MixedUrl.MixedUrlScheme,
                                HttpUtility.UrlEncode(VideoUrl),
                                HttpUtility.UrlEncode(AudioUrl)
                                );
        }


    }
}
