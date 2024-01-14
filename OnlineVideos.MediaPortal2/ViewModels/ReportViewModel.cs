using MediaPortal.UI.Presentation.DataObjects;

namespace OnlineVideos.MediaPortal2
{
    public class ReportViewModel : ListItem
    {
        public WebService.Report Report { get; protected set; }

        public ReportViewModel(WebService.Report report)
        {
            Report = report;
        }
    }
}
