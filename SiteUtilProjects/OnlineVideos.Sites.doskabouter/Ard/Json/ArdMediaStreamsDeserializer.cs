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
            var collectionEmbedded = firstWidget?[ELEMENT_MEDIACOLLECTION][ELEMENT_EMBEDDED];

            return MediaArrayConverter.ParseVideoUrls(collectionEmbedded as JObject);
        }


        public static int? GetDuration(JToken jsonElement)
        {
            return jsonElement?[ELEMENT_MEDIACOLLECTION][ELEMENT_EMBEDDED]?.Value<int>(ATTRIBUTE_DURATION);
        }
    }
}
