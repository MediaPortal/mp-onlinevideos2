using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;


namespace OnlineVideos.Sites.Ard.Json
{
    internal class ArdMediaStreamsDeserializer : ArdWidgetTeaserDeserializerBase
    {
        protected static readonly string ELEMENT_MEDIACOLLECTION = "mediaCollection";
        protected static readonly string ELEMENT_EMBEDDED = "embedded";

        protected static readonly string ATTRIBUTE_DURATION = "_duration";
        protected static readonly string ATTRIBUTE_TYPE = "_type"; //"video"
        protected static readonly string ATTRIBUTE_IS_LIVE = "_isLive";
        protected static readonly string ATTRIBUTE_PREVIEW_IMAGE = "_previewImage";

        protected ArdMediaArrayConverter MediaArrayConverter { get; } = new ArdMediaArrayConverter();

        public IEnumerable<DownloadDetailsDto> ParseWidgets(JToken jsonElement)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);
            var firstWidget = TrySelectWidgetElements(widgetsElement, 1).FirstOrDefault();
            JToken? collectionEmbedded = null;

            JToken mediaCollection = firstWidget?[ELEMENT_MEDIACOLLECTION];
            if (mediaCollection?.HasValues is true)
            {
                collectionEmbedded = mediaCollection[ELEMENT_EMBEDDED];
            }

            return collectionEmbedded is null ? [] : MediaArrayConverter.ParseVideoUrls(collectionEmbedded as JObject);
        }


        public static int? GetDuration(JToken jsonElement)
        {
            JToken mediaCollection = jsonElement?[ELEMENT_MEDIACOLLECTION];
            if (mediaCollection?.HasValues is true)
            {
                return mediaCollection[ELEMENT_EMBEDDED]?.Value<int>(ATTRIBUTE_DURATION);
            }
            return null;
        }
    }
}
