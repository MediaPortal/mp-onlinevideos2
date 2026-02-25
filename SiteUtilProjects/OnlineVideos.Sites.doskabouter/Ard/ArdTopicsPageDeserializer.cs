using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Net;

namespace OnlineVideos.Sites.Ard
{
    /// <summary>
    ///                     case CATEGORYNAME_BROADCASTS_AZ:
    /// https://api.ardmediathek.de/page-gateway/pages/ard/editorial/experiment-a-z
    /// https://api.ardmediathek.de/page-gateway/pages/ard/editorial/experiment-a-z?embedded=false
    /// https://api.ardmediathek.de/page-gateway/pages/ard/editorial/experiment-a-z");
    /// </summary>
    internal class ArdTopicsPageDeserializer : PageDeserializerBase
    {
        private static readonly string _categoryLevel = "Level";

        public static Uri EntryUrl { get; } = new Uri("https://api.ardmediathek.de/page-gateway/pages/ard/editorial/experiment-a-z");

        public override ArdCategoryInfoDto RootCategory { get; } = new ArdCategoryInfoDto("", EntryUrl.AbsoluteUri)
        {
            Title = "Sendungen A-Z",
            //Description = "",
            HasSubCategories = true,
            //ImageUrl = ,
            //TargetUrl =
        };

        public ArdTopicsPageDeserializer(WebCache webClient) : base(webClient) { }


        public override Result<IEnumerable<ArdCategoryInfoDto>> GetCategories(string targetUrl, ContinuationToken continuationToken = null)
        {
            continuationToken ??= new ContinuationToken() { { _categoryLevel, 0 } };

            var currentLevel = continuationToken.GetValueOrDefault(_categoryLevel) as int? ?? 0;
            Log.Debug($"GetCategories current Level: {currentLevel}");

            var json = WebClient.GetWebData<JObject>(targetUrl, proxy: WebRequest.GetSystemWebProxy());
            var categoryInfos = currentLevel switch
            {
                0 => CategoryDeserializer.ParseWidgets(json, hasSubCategories: true), // load A - Z
                1 => LoadCategoriesWithDetails(json), // load e.g. Abendschau - skip level, (load infos from nextlevel) for each category load url and read synopsis
                //2 => categoryDeserializer.ParseTeasers(json), // videos...
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

        private IEnumerable<ArdCategoryInfoDto> LoadCategoriesWithDetails(JObject json)
        {
            var categories = CategoryDeserializer.ParseTeasers(json);

            foreach (var category in categories)
            {
                //TODO workaround
                var details = WebClient.GetWebData<JObject>(category.TargetUrl, proxy: WebRequest.GetSystemWebProxy());
                var newCategory = CategoryDeserializer.ParseTeaser(details);
                category.Title = newCategory.Title;
                category.Description = newCategory.Description;
                yield return category;
                // sub item sind videos, aber ...
            }
        }
    }
}
