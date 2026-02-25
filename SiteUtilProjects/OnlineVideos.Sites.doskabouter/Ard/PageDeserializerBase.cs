using Newtonsoft.Json.Linq;
using OnlineVideos.Sites.Ard.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OnlineVideos.Sites.Ard
{
    internal abstract class PageDeserializerBase
    {
        protected ArdCategoryDeserializer CategoryDeserializer { get; } = new ArdCategoryDeserializer();
        protected ArdVideoInfoDeserializer VideoDeserializer { get; } = new ArdVideoInfoDeserializer();
        protected ArdMediaStreamsDeserializer VideoStreamsDeserializer { get; } = new ArdMediaStreamsDeserializer();

        protected WebCache WebClient { get; }

        protected PageDeserializerBase(WebCache webClient) => WebClient = webClient;

        public abstract ArdCategoryInfoDto RootCategory { get; }

        public abstract Result<IEnumerable<ArdCategoryInfoDto>> GetCategories(string url, ContinuationToken continuationToken = null);

        public virtual Result<IEnumerable<ArdVideoInfoDto>> GetVideos(string url, ContinuationToken continuationToken = null)
        {
            var json = WebClient.GetWebData<JToken>(url, cache: false, proxy: WebRequest.GetSystemWebProxy());
            var detailUrls = VideoDeserializer.ParseTeasersUrl(json);
            var filmInfos = LoadVideosWithDetails(detailUrls);

            return new Result<IEnumerable<ArdVideoInfoDto>>()
            {
                ContinuationToken = continuationToken,
                Value = filmInfos
            };
        }

        private IEnumerable<ArdVideoInfoDto> LoadVideosWithDetails(IEnumerable<string> urls)
        {
            foreach (var url in urls)
            {
                if (url.Contains("api.ardmediathek"))
                    yield return GetVideoDetails(url);
            }
        }


        private ArdVideoInfoDto GetVideoDetails(string url)
        {
            var details = WebClient.GetWebData<JObject>(url, proxy: WebRequest.GetSystemWebProxy());
            var filmInfos = VideoDeserializer.ParseWidgets(details).ToList();
            return filmInfos.FirstOrDefault();
        }


        public virtual Result<IEnumerable<DownloadDetailsDto>> GetStreams(string url, ContinuationToken continuationToken = null)
        {
            //continuationToken ??= new ContinuationToken() { { _level, 0 } };

            var json = WebClient.GetWebData<JToken>(url, cache: false, proxy: WebRequest.GetSystemWebProxy());
            var streamInfos = VideoStreamsDeserializer.ParseWidgets(json);

            return new Result<IEnumerable<DownloadDetailsDto>>()
            {
                ContinuationToken = continuationToken,
                Value = streamInfos
            };
        }
    }
}
