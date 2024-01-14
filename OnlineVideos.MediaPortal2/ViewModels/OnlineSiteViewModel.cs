using System;
using MediaPortal.Common.General;
using MediaPortal.UI.Presentation.DataObjects;
using MediaPortal.UiComponents.Media.General;

namespace OnlineVideos.MediaPortal2
{
    public class OnlineSiteViewModel : ListItem
    {
        public WebService.Site Site { get; protected set; }
        public string Owner { get; protected set; }
        public SiteSettings LocalSite { get; protected set; }

        protected AbstractProperty _lastUpdatedProperty;
        public AbstractProperty LastUpdatedProperty { get { return _lastUpdatedProperty; } }
        public DateTime LastUpdated
        {
            get { return (DateTime)_lastUpdatedProperty.GetValue(); }
            set { _lastUpdatedProperty.SetValue(value); }
        }

        public OnlineSiteViewModel(WebService.Site site, SiteSettings localSite)
            : base(Consts.KEY_NAME, site.Name)
        {
            _lastUpdatedProperty = new WProperty(typeof(DateTime), default(DateTime));

            Site = site;
            LocalSite = localSite;
            Owner = !string.IsNullOrEmpty(site.OwnerId) ? site.OwnerId.Substring(0, site.OwnerId.IndexOf('@')) : string.Empty;
            LastUpdated = site.LastUpdated.ToLocalTime();
        }
    }
}
