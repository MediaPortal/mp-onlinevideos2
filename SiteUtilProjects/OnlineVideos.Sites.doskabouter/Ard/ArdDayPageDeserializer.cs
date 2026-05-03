using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace OnlineVideos.Sites.Ard
{
    internal class ArdDayPageDeserializer : PageDeserializerBase
    {
        private const string PLACEHOLDER_PARTNERNAME = "$$partnerName$$";
        private const string _categoryLevel = "Level";

        private const int MAX_DAYS_PAST = 7;
        private const int MAX_DAYS_FUTURE = 7;

        /// <inheritdoc />
        public override ArdCategoryInfoDto RootCategory { get; } = new ArdCategoryInfoDto(nameof(ArdDayPageDeserializer), string.Empty)
        {
            Title = "Programm",
            Description = "Programmübersicht",
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

            var dayCategories = currentLevel switch
            {
                0 => GetTvListings(),
                1 => PartnerNames(targetUrl),
                _ => throw new ArgumentOutOfRangeException(),
            };

            var newToken = new ContinuationToken(continuationToken);
            newToken[_categoryLevel] = currentLevel + 1;
            return new Result<IEnumerable<ArdCategoryInfoDto>>
            {
                ContinuationToken = newToken,
                Value = dayCategories
            };
        }



        private IEnumerable<ArdCategoryInfoDto> GetTvListings()
        {
            static Uri CreateDayUrl(DateTimeOffset day)
                => ArdConstants.CreateDayPageUrl(day, PLACEHOLDER_PARTNERNAME);

            var today = DateTimeOffset.Now;
            for (int i = -MAX_DAYS_PAST; i <= MAX_DAYS_FUTURE; i++)
            {
                var day = today.AddDays(i);
                var url = CreateDayUrl(day);
                var formattedDay = $"{day:ddd, dd.MM.}";
                yield return new ArdCategoryInfoDto(nameof(ArdDayPageDeserializer) + i, url.AbsoluteUri)
                {
                    Title = i switch
                    {
                        -1 => $"Gestern - {formattedDay}",
                        0 => $"Heute - {formattedDay}",
                        1 => $"Morgen - {formattedDay}",
                        _ => formattedDay
                    },
                    HasSubCategories = true,
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

        public override Result<IEnumerable<ArdVideoInfoDto>> GetVideos(string url, ContinuationToken continuationToken = null)
        {
            var json = WebClient.GetWebData<JToken>(url, cache: false, proxy: WebRequest.GetSystemWebProxy());
            var filmInfos = VideoDeserializer.ParseChannels(json).ToList();

            return new Result<IEnumerable<ArdVideoInfoDto>>()
            {
                ContinuationToken = continuationToken,
                Value = filmInfos
            };
        }
    }
}
