using System;
using System.Collections.Generic;

namespace OnlineVideos.Sites.Ard
{
    internal class ArdDayPageDeserializer : PageDeserializerBase
    {
        private static readonly string _categoryLevel = "Level";

        /// <inheritdoc />
        public override ArdCategoryInfoDto RootCategory { get; } = new ArdCategoryInfoDto(nameof(ArdDayPageDeserializer), string.Empty)
        {
            Title = "Was lief",
            Description = "Sendungen der letzten 7 Tage.",
            HasSubCategories = true,
            //ImageUrl = ,
            //TargetUrl =
        };

        public ArdDayPageDeserializer(WebCache webClient) : base(webClient) { }


        /// <inheritdoc />
        public override Result<IEnumerable<ArdCategoryInfoDto>> GetCategories(string targetUrl, ContinuationToken continuationToken = null)
        {
            continuationToken ??= new ContinuationToken() { { _categoryLevel, 0 } };
            var currentLevel = continuationToken.GetValueOrDefault(_categoryLevel) as int? ?? 0;

            var categoryInfos = currentLevel switch
            {
                0 => LastSevenDays(),
                1 => PartnerNames(targetUrl),
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


        private static readonly string PLACEHOLDER_PARTNERNAME = "{{partnerName}}";

        private IEnumerable<ArdCategoryInfoDto> LastSevenDays()
        {
            const string DAY_PAGE_DATE_FORMAT = "yyyy-MM-dd";
            static string CreateDayUrl(DateTime day)
                => $"https://api.ardmediathek.de//page-gateway/compilations/{PLACEHOLDER_PARTNERNAME}/pastbroadcasts" +
                   $"?startDateTime={day.ToString(DAY_PAGE_DATE_FORMAT)}T00:00:00.000Z" +
                   $"&endDateTime={day.ToString(DAY_PAGE_DATE_FORMAT)}T23:59:59.000Z" +
                   $"&pageNumber=0" +
                   $"&pageSize={ArdConstants.DAY_PAGE_SIZE}";

            for (var i = 0; i <= 7; i++)
            {
                var day = DateTime.Today.AddDays(-i);
                var url = CreateDayUrl(day);
                yield return new ArdCategoryInfoDto(nameof(ArdDayPageDeserializer) + i, url)
                {
                    Title = i switch
                    {
                        0 => "Heute",
                        1 => "Gestern",
                        _ => day.ToString("ddd, d.M.")
                    },
                    //Url = url,
                    HasSubCategories = true,
                    //ImageUrl =
                };
            }
        }

        private IEnumerable<ArdCategoryInfoDto> PartnerNames(string placeholderUrl)
        {
            static string CreatePartnerUrl(string placeholderUrl, string partnerName)
                => placeholderUrl.Replace(PLACEHOLDER_PARTNERNAME, partnerName);

            foreach (var partner in ArdPartner.Values)
            {
                var url = CreatePartnerUrl(placeholderUrl, partner);
                yield return new ArdCategoryInfoDto(nameof(ArdDayPageDeserializer) + partner, url)
                {
                    Title = partner.DisplayName,
                    //Url = url,
                    //HasSubCategories = true,
                    //ImageUrl =
                };
            }
        }
    }
}
