using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;


namespace OnlineVideos.Sites.Ard.Json
{
    internal class ArdVideoInfoDeserializer : ArdWidgetTeaserDeserializerBase
    {
        protected static readonly string ATTRIBUTE_UNTIL_DATETIME = "availableTo";
        protected static readonly string ATTRIBUTE_AIR_DATETIME = "broadcastedOn";
        protected static readonly string ATTRIBUTE_DURATION = "duration";

        protected static readonly string ATTRIBUTE_NUMBER_OF_CLIPS = "numberOfClips";

        public IEnumerable<ArdVideoInfoDto> ParseWidgets(JToken jsonElement, int takeWidgets = int.MaxValue)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);
            var selectedWidgetElements = TrySelectWidgetElements(widgetsElement, takeWidgets);

            Func<JToken, ArdVideoInfoDto> videoInfoParser = IsVideoElement(selectedWidgetElements.FirstOrDefault()) ? ParseWidgetVideoInfo : ParseTeaserVideoInfo;
            return EnumerateItems(selectedWidgetElements, videoInfoParser);
        }


        private static bool IsVideoElement(JObject widgetElement)
        {
            return widgetElement?[ATTRIBUTE_AIR_DATETIME] != null;
        }


        public IEnumerable<ArdVideoInfoDto> ParseTeasers(JToken jsonElement, int widgetsToUse = 1)
        {
            var teasers = ParseTeasersInternal(jsonElement, widgetsToUse);


            return EnumerateItems(teasers, ParseTeaserVideoInfo);
        }

        public IEnumerable<string> ParseTeasersUrl(JToken jsonElement, int widgetsToUse = 1)
        {
            var teasers = ParseTeasersInternal(jsonElement, widgetsToUse);

            return EnumerateItems(teasers, GetTeaserTargetUrl);
        }


        private static IEnumerable<JToken> ParseTeasersInternal(JToken jsonElement, int widgetsToUse)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);

            var teasers = TryGetTeasersTokenOrInput(widgetsElement);

            return teasers;
        }

        private static string GetTeaserTargetUrl(JToken teaserElement)
        {
            return teaserElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_HREF);
        }

        public ArdVideoInfoDto ParseTeaserVideoInfo(JToken teaserElement)
        {
            var id = teaserElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_ID) ??
                        teaserElement.Value<string>(ATTRIBUTE_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            var url = GetTeaserTargetUrl(teaserElement);
            var imageUrl = teaserElement[ELEMENT_IMAGES]?[ELEMENT_ASPECT_16X9]?.Value<string>(ATTRIBUTE_SRC) ??
                            teaserElement[ELEMENT_IMAGES]?[ELEMENT_ASPECT_4X3]?.Value<string>(ATTRIBUTE_SRC);

            var numberOfClips = teaserElement.Value<int?>(ATTRIBUTE_NUMBER_OF_CLIPS) ?? 0;

            return new ArdVideoInfoDto(id, numberOfClips, url)
            {
                Title = teaserElement.Value<string>(ATTRIBUTE_TITLE),
                AirDate = teaserElement.Value<DateTime?>(ATTRIBUTE_AIR_DATETIME),
                AvailableUntilDate = teaserElement.Value<DateTime?>(ATTRIBUTE_UNTIL_DATETIME),
                Description = teaserElement.Value<string>(ATTRIBUTE_DESCRIPTION),
                Duration = teaserElement.Value<int>(ATTRIBUTE_DURATION),
                ImageUrl = imageUrl?.Replace(ArdMediathekUtil.PLACEHOLDER_IMAGE_WIDTH, ArdMediathekUtil.IMAGE_WIDTH)
            };
        }

        public ArdVideoInfoDto ParseWidgetVideoInfo(JToken itemElement)
        {
            var id = itemElement.Value<string>(ATTRIBUTE_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            // fskRating
            // tracking.atiCustomVars.clipLength
            // widgets[0].mediaCollection.embedded._duration
            var url = itemElement[ELEMENT_LINKS]?[ELEMENT_SELF]?.Value<string>(ATTRIBUTE_HREF);
            var imageUrl = itemElement[ELEMENT_IMAGE]?.Value<string>(ATTRIBUTE_SRC);

            var numberOfClips = itemElement.Value<int?>(ATTRIBUTE_NUMBER_OF_CLIPS) ?? 0;

            return new ArdVideoInfoDto(id, numberOfClips, url)
            {
                Title = itemElement.Value<string>(ATTRIBUTE_TITLE),
                AirDate = itemElement.Value<DateTime?>(ATTRIBUTE_AIR_DATETIME),
                AvailableUntilDate = itemElement.Value<DateTime?>(ATTRIBUTE_UNTIL_DATETIME),
                Description = itemElement.Value<string>(ATTRIBUTE_DESCRIPTION),
                Duration = ArdMediaStreamsDeserializer.GetDuration(itemElement),
                ImageUrl = imageUrl?.Replace(ArdMediathekUtil.PLACEHOLDER_IMAGE_WIDTH, ArdMediathekUtil.IMAGE_WIDTH)
            };
        }
    }
}
