using OnlineVideos.Helpers;

namespace OnlineVideos.Sites
{

    public interface IWebViewSiteUtilBase
    {
        void StartPlayback();
    }

    public interface IWebViewSiteUtil : IWebViewSiteUtilBase
    {
        void OnPageLoaded(ref bool doStopPlayback);
        void DoPause();
        void DoPlay();
    }
    public interface IWebViewHTMLMediaElement : IWebViewSiteUtilBase
    {
        string VideoElementSelector { get; }
    }
}
