using System;


namespace OnlineVideos.Sites.Ard
{
    public class ArdConstants
    {

        public static Uri API_URL { get; } = new Uri("https://api.ardmediathek.de");
        public static string ITEM_URL { get; } = API_URL + "/page-gateway/pages/ard/item/";
        public static int DAY_PAGE_SIZE { get; } = 100;
    }
}
