using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace OnlineVideos.Sites
{
    /// <summary>
    /// A site implementation for watchers that uses the <see cref="IWatchersDatabase"/>.
    /// </summary>
    public class WatcherUtil : SiteUtilBase
    {
        public class WatcherCategory : Category
        {
            public SiteUtilBase WatchSite { get; protected set; }
            public SiteUtilBase Site { get; protected set; }
            public WatcherDbCategory WatcherDbCategory { get; protected set; }

            public override string Label2
            {
                get { return this.WatcherDbCategory.LastRefresh.ToString(); }
            }

            public WatcherCategory(WatcherDbCategory cat, SiteUtilBase util, SiteUtilBase utilWatch)
            {
                this.Site = util;
                this.WatchSite = utilWatch;
                this.WatcherDbCategory = cat;
                this.Name = cat.RecursiveName.Replace("|", " / ");

                StringBuilder sb = new StringBuilder(256);
                sb.Append(Translation.Instance.WatcherPeriod);
                sb.Append(": ");
                sb.Append((cat.RefreshPeriod / 60).ToString());
                sb.Append(' ');
                sb.Append(Translation.Instance.Hours);
                sb.Append("\r\n");
                sb.Append(Translation.Instance.LastRefresh);
                sb.Append(": ");
                sb.Append(cat.LastRefresh);
                sb.Append("\r\n");
                sb.Append(cat.Description);
                this.Description = sb.ToString();

                this.Thumb = cat.Thumb;
            }
        }

        // keep a reference of all Categories ever created and reuse them, to get them selected when returning to the category view
        private readonly Dictionary<string, RssLink> _cachedCategories = new Dictionary<string, RssLink>();

        public override void Initialize(SiteSettings siteSettings)
        {
            siteSettings.Description = "Watch your favorite categories for new videos.";
            base.Initialize(siteSettings);
        }

        public override int DiscoverDynamicCategories()
        {
            this.Settings.Categories.Clear();

            List<KeyValuePair<string, uint>> lsSiteIds = OnlineVideoSettings.Instance.WatchDB.GetSiteIds();

            if (lsSiteIds == null || lsSiteIds.Count == 0)
                return 0;

            lsSiteIds.ForEach(lsSiteId =>
            {
                if (OnlineVideoSettings.Instance.SiteUtilsList.TryGetValue(lsSiteId.Key, out SiteUtilBase util))
                {
                    SiteSettings aSite = util.Settings;
                    if (aSite.IsEnabled &&
                       (!aSite.ConfirmAge || !OnlineVideoSettings.Instance.UseAgeConfirmation || OnlineVideoSettings.Instance.AgeConfirmed))
                    {
                        if (!this._cachedCategories.TryGetValue(aSite.Name + " - " + Translation.Instance.Watchers, out RssLink cat))
                        {
                            cat = new RssLink
                            {
                                Name = aSite.Name + " - " + Translation.Instance.Watchers,
                                Description = aSite.Description,
                                Url = aSite.Name,
                                Thumb = System.IO.Path.Combine(OnlineVideoSettings.Instance.ThumbsDir, @"Icons\" + aSite.Name + ".png")
                            };
                            this._cachedCategories.Add(cat.Name, cat);
                        }
                        cat.EstimatedVideoCount = lsSiteId.Value;
                        this.Settings.Categories.Add(cat);

                        // create subcategories if any
                        List<WatcherDbCategory> cats = OnlineVideoSettings.Instance.WatchDB.GetCategories(aSite.Name);
                        if (cats.Count > 0)
                        {
                            cat.HasSubCategories = true;
                            cat.SubCategoriesDiscovered = true;
                            cat.SubCategories = new List<Category>();
                            cats.ForEach(c => cat.SubCategories.Add(new WatcherCategory(c, util, this) { ParentCategory = cat }));
                        }
                    }
                }
            });

            // need to always get the categories, because when adding new watcher from a new site, a removing the last one for a site, the categories must be refreshed 
            this.Settings.DynamicCategoriesDiscovered = false;
            return this.Settings.Categories.Count;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            throw new NotImplementedException();
        }

        #region Search

        public override bool CanSearch { get { return false; } }

        #endregion

        #region Context Menu

        public override List<ContextMenuEntry> GetContextMenuEntries(Category selectedCategory, VideoInfo selectedItem)
        {
            List<ContextMenuEntry> result = new List<ContextMenuEntry>();
            if (selectedCategory is WatcherCategory)
                result.Add(new ContextMenuEntry { DisplayText = Translation.Instance.RemoveFromWatchers });

            return result;
        }

        public override ContextMenuExecutionResult ExecuteContextMenuEntry(Category selectedCategory, VideoInfo selectedItem, ContextMenuEntry choice)
        {
            ContextMenuExecutionResult result = new ContextMenuExecutionResult();
            if (choice.DisplayText == Translation.Instance.RemoveFromWatchers)
            {
                if (selectedCategory is WatcherCategory)
                {
                    result.RefreshCurrentItems = OnlineVideoSettings.Instance.WatchDB.RemoveCategory(((WatcherCategory)selectedCategory).WatcherDbCategory);
                    if (result.RefreshCurrentItems)
                        selectedCategory.ParentCategory.SubCategories.Remove(selectedCategory);

                    return result;
                }

                // we have to manually refresh the categories
                if (result.RefreshCurrentItems && selectedCategory.ParentCategory != null)
                    DiscoverDynamicCategories();
            }
            return result;
        }

        #endregion

    }

}
