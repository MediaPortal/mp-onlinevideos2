using System.Collections.Generic;

using Newtonsoft.Json.Linq;


namespace OnlineVideos.Sites.Ard.Json
{
    internal class ArdCategoryDeserializer : ArdWidgetTeaserDeserializerBase
    {
        protected static readonly string WIDGET_ATTRIBUTE_COMPILATIONTYPE = "compilationType";

        public IEnumerable<ArdCategoryInfoDto> ParseWidgets(JToken jsonElement, bool hasSubCategories = false)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);

            return EnumerateItems(widgetsElement as JArray, widget => ParseWidget(widget, hasSubCategories));
        }

        public IEnumerable<ArdCategoryInfoDto> ParseTeasers(JToken jsonElement, bool hasSubCategories = false, int widgetsToUse = 1)
        {
            var widgetsElement = TryGetWidgetsTokenOrInput(jsonElement);

            var teasers = TryGetTeasersTokenOrInput(widgetsElement);
            return EnumerateItems(teasers, teaser => ParseTeaser(teaser, hasSubCategories));
        }

        protected ArdCategoryInfoDto ParseWidget(JToken widgetElement, bool hasSubCategories = false)
        {
            var compilationType = widgetElement.Value<string>(WIDGET_ATTRIBUTE_COMPILATIONTYPE);

            var id = //widgetElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_ID) ??
                widgetElement.Value<string>(ATTRIBUTE_ID);

            var selfUrl = widgetElement[ELEMENT_LINKS]?[ELEMENT_SELF]?.Value<string>(ATTRIBUTE_HREF);

            return new ArdCategoryInfoDto(id, selfUrl)
            {
                Title = widgetElement.Value<string>(ATTRIBUTE_TITLE),
                //Description =,
                //ImageUrl = ,
                //NavigationUrl = selfUrl, //string.Format(ArdConstants.EDITORIAL_URL, id),
                //Pagination = ,
                HasSubCategories = hasSubCategories,
            };
        }

        public ArdCategoryInfoDto ParseTeaser(JToken teaserElement, bool hasSubCategories = false)
        {
            //var teaserObject = teaserElement;
            var id = teaserElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_ID) ??
                        teaserElement.Value<string>(ATTRIBUTE_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            var url = teaserElement[ELEMENT_LINKS]?[ELEMENT_TARGET]?.Value<string>(ATTRIBUTE_HREF);
            var imageUrl = teaserElement[ELEMENT_IMAGES]?[ELEMENT_ASPECT_16X9]?.Value<string>(ATTRIBUTE_SRC) ??
                            teaserElement[ELEMENT_IMAGES]?[ELEMENT_ASPECT_4X3]?.Value<string>(ATTRIBUTE_SRC);

            return new ArdCategoryInfoDto(id, url)
            {
                Title = teaserElement.Value<string>(ATTRIBUTE_TITLE),
                //AirDate = teaserElement.Value<DateTime>(ATTRIBUTE_DATETIME),
                Description = teaserElement.Value<string>(ATTRIBUTE_DESCRIPTION),
                //Duration = teaserElement[ELEMENT_MEDIACOLLECTION]?[ELEMENT_EMBEDDED]?.Value<int>(ATTRIBUTE_DURATION),
                ImageUrl = imageUrl?.Replace(ArdMediathekUtil.PLACEHOLDER_IMAGE_WIDTH, ArdMediathekUtil.IMAGE_WIDTH),
                HasSubCategories = hasSubCategories,
            };
        }
    }
}
