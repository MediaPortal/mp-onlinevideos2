using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace OnlineVideos.Sites.Ard.Json
{
    internal class ArdVideoInfoDeserializer : ArdWidgetTeaserDeserializerBase
    {
        protected static readonly string ATTRIBUTE_UNTIL_DATETIME = "availableTo";
        protected static readonly string ATTRIBUTE_AIR_DATETIME = "broadcastedOn";
        protected static readonly string ATTRIBUTE_DURATION = "duration";

        protected static readonly string ATTRIBUTE_NUMBER_OF_CLIPS = "numberOfClips";

        public IEnumerable<ArdVideoInfoDto> ParseChannels(JToken jsonElement)
        {
            var channels = TryGetChannelsArray(jsonElement);
            var entries = TryGetTimeSlotEntries(channels.First());

            return EnumerateItems(entries, ParseTimeSlotEntryVideoInfo);
        }

        public IEnumerable<ArdVideoInfoDto> ParseWidgets(JToken jsonElement, int takeWidgets = int.MaxValue)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);
            var selectedWidgetElements = TrySelectWidgetElements(widgetsElement, takeWidgets);

            Func<JToken, ArdVideoInfoDto> videoInfoParser = widgetElement =>
            {
                if (IsVideoElement(widgetElement as JObject))
                {
                    return ParseWidgetVideoInfo(widgetElement);
                }
                else
                {
                    return ParseTeaserVideoInfo(widgetElement);
                }
            };
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

        private static string GetTeaserImageUrl(JToken teaserElement)
        {
            var url = teaserElement[ELEMENT_IMAGES]?[ELEMENT_ASPECT_16X9]?.Value<string>(ATTRIBUTE_SRC)
                ?? teaserElement[ELEMENT_IMAGES]?[ELEMENT_ASPECT_4X3]?.Value<string>(ATTRIBUTE_SRC);

            return url?.Replace(ArdMediathekUtil.PLACEHOLDER_IMAGE_WIDTH, ArdMediathekUtil.IMAGE_WIDTH);
        }

        public ArdVideoInfoDto ParseTeaserVideoInfo(JToken teaserElement)
        {
            var id = teaserElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_ID) ??
                        teaserElement.Value<string>(ATTRIBUTE_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var numberOfClips = teaserElement.Value<int?>(ATTRIBUTE_NUMBER_OF_CLIPS) ?? 0;
            return new ArdVideoInfoDto(id, numberOfClips)
            {
                Title = teaserElement.Value<string>(ATTRIBUTE_TITLE),
                AirDate = teaserElement.Value<DateTime?>(ATTRIBUTE_AIR_DATETIME),
                AvailableUntilDate = teaserElement.Value<DateTime?>(ATTRIBUTE_UNTIL_DATETIME),
                Description = teaserElement.Value<string>(ATTRIBUTE_DESCRIPTION),
                Duration = teaserElement.Value<int>(ATTRIBUTE_DURATION),
                ImageUrl = GetTeaserImageUrl(teaserElement)
            };
        }

        public ArdVideoInfoDto ParseWidgetVideoInfo(JToken itemElement)
        {
            var id = itemElement.Value<string>(ATTRIBUTE_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var numberOfClips = itemElement.Value<int?>(ATTRIBUTE_NUMBER_OF_CLIPS) ?? 0;
            return new ArdVideoInfoDto(id, numberOfClips)
            {
                Title = itemElement.Value<string>(ATTRIBUTE_TITLE),
                AirDate = itemElement.Value<DateTime?>(ATTRIBUTE_AIR_DATETIME),
                AvailableUntilDate = itemElement.Value<DateTime?>(ATTRIBUTE_UNTIL_DATETIME),
                Description = itemElement.Value<string>(ATTRIBUTE_DESCRIPTION),
                Duration = ArdMediaStreamsV6Deserializer.GetDuration(itemElement),
                ImageUrl = GetTeaserImageUrl(itemElement)
            };
        }

        public ArdVideoInfoDto ParseTimeSlotEntryVideoInfo(JToken timeSlotEntryElement)
        {
            var id = timeSlotEntryElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_URL_ID)
                ?? timeSlotEntryElement.Value<string>(ATTRIBUTE_URL_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            var videoElement = timeSlotEntryElement.Value<JObject>(ATTRIBUTE_VIDEO);
            return new ArdVideoInfoDto(id, 1)
            {
                Title = timeSlotEntryElement.Value<string>(ATTRIBUTE_TITLE),
                AirDate = timeSlotEntryElement.Value<DateTime?>(ATTRIBUTE_AIR_DATETIME),
                AvailableUntilDate = videoElement.Value<DateTime?>(ATTRIBUTE_UNTIL_DATETIME),
                Description = timeSlotEntryElement.Value<string>(ATTRIBUTE_DESCRIPTION),
                Duration = timeSlotEntryElement.Value<int>(ATTRIBUTE_DURATION),
                ImageUrl = GetTeaserImageUrl(timeSlotEntryElement)
            };
        }
    }
}
