using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;


namespace OnlineVideos.Sites.Ard.Json
{
    internal abstract class ArdWidgetTeaserDeserializerBase
    {
        protected static readonly string ELEMENT_WIDGETS = "widgets";
        protected static readonly string ELEMENT_TEASERS = "teasers";

        protected static readonly string ELEMENT_LINKS = "links";
        protected static readonly string ELEMENT_SELF = "self";
        protected static readonly string ELEMENT_TARGET = "target";

        protected static readonly string ELEMENT_IMAGE = "image";
        protected static readonly string ELEMENT_IMAGES = "images";
        protected static readonly string ELEMENT_ASPECT_16X9 = "aspect16x9";
        protected static readonly string ELEMENT_ASPECT_4X3 = "aspect16x9";


        protected static readonly string ATTRIBUTE_ID = "id";
        protected static readonly string ATTRIBUTE_HREF = "href";
        protected static readonly string ATTRIBUTE_SRC = "src";

        protected static readonly string ATTRIBUTE_TITLE = "title";
        protected static readonly string ATTRIBUTE_TITLE_SHORT = "shortTitle";
        protected static readonly string ATTRIBUTE_DESCRIPTION = "synopsis";


        protected IEnumerable<T> EnumerateItems<T>(IEnumerable<JToken> elements, Func<JToken, T> converter)
        {
            return elements.AsEmptyIfNull()
                           .ExceptDefault()
                           .Select(converter)
                           .ExceptDefault();
        }


        protected static JToken TryGetWidgetsTokenOrInput(JToken jsonElement)
        {
            return jsonElement?.Type == JTokenType.Object && jsonElement?[ELEMENT_WIDGETS] != null
                       ? jsonElement[ELEMENT_WIDGETS]
                       : jsonElement;
        }


        /// <summary>
        /// extract the teasers of the first #<paramref name="takeWidgets"/> widgets
        /// </summary>
        /// <param name="widgetsElement"></param>
        /// <param name="takeWidgets"></param>
        /// <returns></returns>
        protected static IEnumerable<JToken> TryGetTeasersTokenOrInput(JToken widgetsElement, int takeWidgets = 1)
        {
            IEnumerable<JObject> selectedWidgetElements = TrySelectWidgetElements(widgetsElement, takeWidgets);

            // ToDo SelectMany do we need the widget category, which is now omitted?
            return selectedWidgetElements.ExceptDefault()
                                         .Select(jObj => jObj[ELEMENT_TEASERS] as JArray)
                                         .First();
        }


        /// <summary>
        /// get first #<paramref name="takeWidgets"/> widgets
        /// </summary>
        /// <param name="widgetsElement"></param>
        /// <param name="takeWidgets"></param>
        /// <returns></returns>
        protected static IEnumerable<JObject> TrySelectWidgetElements(JToken widgetsElement, int takeWidgets)
        {
            var selectedWidgetElements = widgetsElement?.Type == JTokenType.Array
                                             ? widgetsElement.Children()
                                                             .Take(takeWidgets)
                                                             .OfType<JObject>()
                                             : new List<JObject>()
                                               {
                                                   widgetsElement as JObject
                                               };
            return selectedWidgetElements;
        }
    }
}
