using System;
using System.Linq;
using System.Text.RegularExpressions;

using OnlineVideos.Sites.Ard;

namespace OnlineVideos.Sites.Zdf
{
    internal class ZdfApiTokenProvider
    {
        private static readonly string API_TOKEN_URL = "https://www.zdf.de";
        private static readonly string JSON_API_TOKEN = "apiToken";
        private Lazy<BearerToken> _token;

        public ZdfApiTokenProvider(WebCache webClient)
        {
            _token = new Lazy<BearerToken>(() => Initialize(webClient));
        }

        public string SearchBearer => _token.Value.SearchBearer;

        public string VideoBearer => _token.Value.VideoBearer;

        private static BearerToken Initialize(WebCache webClient)
        {
            var data = webClient.GetWebData(API_TOKEN_URL);

            string videoBearer = String.Empty;
            string searchBearer = String.Empty;
            var match = Regex.Match(data, @"\\""videoToken\\"":{\\""apiToken\\"":\\""(?<apitoken>[^\\]*)\\""");
            if (match.Success) videoBearer = match.Groups["apitoken"].Value;
            match = Regex.Match(data, @"\\""appToken\\"":{\\""apiToken\\"":\\""(?<apitoken>[^\\]*)\\""");
            if (match.Success) searchBearer = match.Groups["apitoken"].Value;

            return new BearerToken(searchBearer, videoBearer);
        }

        private class BearerToken
        {
            public BearerToken(string searchBearer, string videoBearer)
            {
                SearchBearer = searchBearer;
                VideoBearer = videoBearer;
            }

            public string SearchBearer { get; }
            public string VideoBearer { get; }
        }
    }
}

