using System;
using System.Collections.Generic;

namespace OnlineVideos.Sites.Ard
{
    internal class ArdLiveStreamsDeserializer : PageDeserializerBase
    {
        public static Uri EntryUrl { get; } = new Uri("https://api.ardmediathek.de/page-gateway/widgets/ard/editorials/4hEeBDgtx6kWs6W6sa44yY");

        public override ArdCategoryInfoDto RootCategory { get; } = new ArdCategoryInfoDto("", EntryUrl.AbsoluteUri)
        {
            Title = "Live TV",
            //Description = "",
            HasSubCategories = false,
            //ImageUrl = ,
            //TargetUrl =
        };

        public ArdLiveStreamsDeserializer(WebCache webClient) : base(webClient) { }

        /// <inheritdoc />
        public override Result<IEnumerable<ArdCategoryInfoDto>> GetCategories(string targetUrl, ContinuationToken continuationToken = null) => throw new NotImplementedException();
    }
}
