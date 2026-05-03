using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net;

namespace OnlineVideos.Sites.Ard
{
    internal class ArdHomeDeserializer : PageDeserializerBase
    {
        private static readonly string _categoryLevel = "Level";

        public static Uri EntryUrl { get; } = new Uri("https://api.ardmediathek.de/page-gateway/pages/ard/home?embedded=true");

        public override ArdCategoryInfoDto RootCategory { get; } = new ArdCategoryInfoDto(nameof(Ard), EntryUrl.AbsoluteUri)
        {
            Title = "Home", //"Highlights",
            //Description = "",
            HasSubCategories = true,
            //ImageUrl = ,
        };

        public ArdHomeDeserializer(WebCache webClient) : base(webClient) { }

        public override Result<IEnumerable<ArdCategoryInfoDto>> GetCategories(string url, ContinuationToken continuationToken = null)
        {
            continuationToken ??= new ContinuationToken() { { _categoryLevel, 0 } };

            var currentLevel = continuationToken.GetValueOrDefault(_categoryLevel) as int? ?? 0;
            Log.Debug($"GetCategories current Level: {currentLevel}");

            var json = WebClient.GetWebData<JObject>(url, proxy: WebRequest.GetSystemWebProxy());
            var categoryInfos = currentLevel switch
            {
                0 => CategoryDeserializer.ParseWidgets(json, hasSubCategories: true), // load A - Z
                //1 => LoadCategoriesWithDetails(json), // load e.g. Abendschau - skip level, (load infos from nextlevel) for each category load url and read synopsis
                ////2 => categoryDeserializer.ParseTeasers(json), // videos...
                _ => throw new ArgumentOutOfRangeException(),
            };

            var newToken = new ContinuationToken(continuationToken);
            newToken[_categoryLevel] = currentLevel + 1;
            return new Result<IEnumerable<ArdCategoryInfoDto>>
            {
                ContinuationToken = newToken,
                Value = categoryInfos
            };
        }
    }
}
