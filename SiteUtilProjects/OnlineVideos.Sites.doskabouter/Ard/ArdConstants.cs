using System;


namespace OnlineVideos.Sites.Ard
{
    public class ArdConstants
    {
        private const string DAY_PAGE_DATE_FORMAT = "yyyy-MM-dd";

        private static readonly string API_URL = "https://api.ardmediathek.de";
        private static readonly string ITEM_URL = API_URL + "/page-gateway/pages/ard/item/{0}";

        private static readonly string DAY_PAGE_URL = "https://programm-api.ard.de/program/api/program?day={0}&channelIds={1}&mode=channel";

        public static Uri CreateItemUrl(string itemId) => new Uri(string.Format(ITEM_URL, itemId));

        public static Uri CreateDayPageUrl(DateTimeOffset day, string channelId) => new Uri(string.Format(DAY_PAGE_URL, day.ToString(DAY_PAGE_DATE_FORMAT), channelId));

    }
}
