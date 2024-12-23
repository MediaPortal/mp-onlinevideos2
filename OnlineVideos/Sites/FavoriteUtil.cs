using System;
using System.Linq;
using System.Collections.Generic;

namespace OnlineVideos.Sites
{
    /// <summary>
    /// A site implementation for favorites that uses the <see cref="IFavoritesDatabase"/>.
    /// </summary>
    public class FavoriteUtil : SiteUtilBase, IFilter
    {
        public class FavoriteCategory : Category
        {
            public SiteUtilBase FavSite { get; protected set; }
            public SiteUtilBase Site { get; protected set; }
            public Category SiteCategory { get; protected set; }
            public FavoriteDbCategory FavoriteDbCategory { get; protected set; }

            public FavoriteCategory(FavoriteDbCategory favCat, SiteUtilBase util, SiteUtilBase favUtil)
            {
                Site = util;
                FavSite = favUtil;
                FavoriteDbCategory = favCat;
                Name = favCat.Name;
                Description = favCat.Description;
                Thumb = favCat.Thumb;
            }

            public void DiscoverSiteCategory()
            {
                if (FavoriteDbCategory.IsSearchCat)
                {
                    SiteCategory = new RssLink()
                    {
                        Name = FavoriteDbCategory.Name,
                        Url = FavoriteDbCategory.RecursiveName,
                        HasSubCategories = FavoriteDbCategory.SearchCatHasSubcategories
                    };
                }
                else
                {
                    string[] hierarchy = FavoriteDbCategory.RecursiveName.Split('|');
                    for (int i = 0; i < hierarchy.Length; i++)
                    {
                        if (SiteCategory != null)
                        {
                            if (!SiteCategory.SubCategoriesDiscovered) Site.DiscoverSubCategories(SiteCategory);
                            Category foundCat = SiteCategory.SubCategories.FirstOrDefault(c => c.Name == hierarchy[i]);
                            // nextpage until found or no more
                            while (foundCat == null && SiteCategory.SubCategories.Last() is NextPageCategory)
                            {
                                Site.DiscoverNextPageCategories(SiteCategory.SubCategories.Last() as NextPageCategory);
                                foundCat = SiteCategory.SubCategories.FirstOrDefault(c => c.Name == hierarchy[i]);
                            }
                            SiteCategory = foundCat;
                        }
                        else
                        {
                            if (!Site.Settings.DynamicCategoriesDiscovered) Site.DiscoverDynamicCategories();
                            Category foundCat = Site.Settings.Categories.FirstOrDefault(c => c.Name == hierarchy[i]);
                            // nextpage until found or no more
                            while (foundCat == null && Site.Settings.Categories.Last() is NextPageCategory)
                            {
                                Site.DiscoverNextPageCategories(Site.Settings.Categories.Last() as NextPageCategory);
                                foundCat = Site.Settings.Categories.FirstOrDefault(c => c.Name == hierarchy[i]);
                            }
                            SiteCategory = foundCat;
                        }
                        if (SiteCategory == null) break;
                    }
                }
            }
        }

        public override List<String> GetMultipleVideoUrls(VideoInfo video, bool inPlaylist = false)
        {
            SiteUtilBase util = OnlineVideoSettings.Instance.SiteUtilsList[(video as FavoriteDbVideoInfo).SiteName];
            return util.GetMultipleVideoUrls(video, inPlaylist);
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            if (category is RssLink)
            {
                return OrderVideos(OnlineVideoSettings.Instance.FavDB.GetFavoriteVideos(((RssLink)category).Url, null));
            }
            return null;
        }

        // keep a reference of all Categories ever created and reuse them, to get them selected when returning to the category view
        readonly Dictionary<string, RssLink> _cachedCategories = new Dictionary<string, RssLink>();

        public override int DiscoverDynamicCategories()
        {
            Settings.Categories.Clear();

            List<KeyValuePair<string, uint>> lsSiteIds = OnlineVideoSettings.Instance.FavDB.GetSiteIds();

            if (lsSiteIds == null || lsSiteIds.Count == 0) return 0;

            uint sumOfAllVideos = (uint)lsSiteIds.Sum(s => s.Value);

            if (sumOfAllVideos > 0)  // only add the "ALL" category if we have single favorite videos in addition to favorites categories
            {
                RssLink all = null;
                if (!_cachedCategories.TryGetValue(Translation.Instance.All, out all))
                {
                    all = new RssLink
                    {
                        Name = Translation.Instance.All,
                        Url = string.Empty
                    };
                    _cachedCategories.Add(all.Name, all);
                }
                all.EstimatedVideoCount = sumOfAllVideos;
                Settings.Categories.Add(all);
            }

            foreach (var lsSiteId in lsSiteIds)
            {
                SiteUtilBase util = null;
                if (OnlineVideoSettings.Instance.SiteUtilsList.TryGetValue(lsSiteId.Key, out util))
                {
                    SiteSettings aSite = util.Settings;
                    if (aSite.IsEnabled &&
                       (!aSite.ConfirmAge || !OnlineVideoSettings.Instance.UseAgeConfirmation || OnlineVideoSettings.Instance.AgeConfirmed))
                    {
                        RssLink cat = null;
                        if (!_cachedCategories.TryGetValue(aSite.Name + " - " + Translation.Instance.Favourites, out cat))
                        {
                            cat = new RssLink();
                            cat.Name = aSite.Name + " - " + Translation.Instance.Favourites;
                            cat.Description = aSite.Description;
                            cat.Url = aSite.Name;
                            cat.Thumb = System.IO.Path.Combine(OnlineVideoSettings.Instance.ThumbsDir, @"Icons\" + aSite.Name + ".png");
                            _cachedCategories.Add(cat.Name, cat);
                        }
                        cat.EstimatedVideoCount = lsSiteId.Value;
                        Settings.Categories.Add(cat);

                        // create subcategories if any
                        var favCats = OnlineVideoSettings.Instance.FavDB.GetFavoriteCategories(aSite.Name);
                        if (favCats.Count > 0)
                        {
                            cat.HasSubCategories = true;
                            cat.SubCategoriesDiscovered = true;
                            cat.SubCategories = new List<Category>();
                            if (lsSiteId.Value > 0) // only add the "ALL" category if we have single favorite videos in addition to favorites categories
                            {
                                cat.SubCategories.Add(new RssLink { Name = Translation.Instance.All, Url = aSite.Name, ParentCategory = cat, EstimatedVideoCount = lsSiteId.Value });
                            }
                            foreach (var favCat in favCats)
                            {
                                FavoriteCategory fc = new FavoriteCategory(favCat, util, this) { ParentCategory = cat };
                                if (String.IsNullOrEmpty(fc.Description))
                                {
                                    string[] parts = favCat.RecursiveName.Split('|');
                                    if (parts.Length > 1)
                                        fc.Description = String.Join(" / ", parts, 0, parts.Length - 1);
                                }

                                cat.SubCategories.Add(fc);
                            }
                        }
                    }
                }
            }

            // need to always get the categories, because when adding new fav video from a new site, a removing the last one for a site, the categories must be refreshed 
            Settings.DynamicCategoriesDiscovered = false;
            return Settings.Categories.Count;
        }

        #region Search

        public override bool CanSearch { get { return true; } }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            return OrderVideos(OnlineVideoSettings.Instance.FavDB.GetFavoriteVideos(null, query))
                .ConvertAll<SearchResultItem>(v => v as SearchResultItem);
        }

        #endregion

        #region Context Menu

        public override List<ContextMenuEntry> GetContextMenuEntries(Category selectedCategory, VideoInfo selectedItem)
        {
            List<ContextMenuEntry> result = new List<ContextMenuEntry>();
            if (selectedCategory is FavoriteCategory)
            {
                if (selectedItem == null) result.Add(new ContextMenuEntry { DisplayText = Translation.Instance.RemoveFromFavorites });
            }
            else if (selectedItem != null)
            {
                result.Add(new ContextMenuEntry { DisplayText = Translation.Instance.RemoveFromFavorites });
                result.Add(new ContextMenuEntry { DisplayText = Translation.Instance.DeleteAll });
            }
            return result;
        }

        public override ContextMenuExecutionResult ExecuteContextMenuEntry(Category selectedCategory, VideoInfo selectedItem, ContextMenuEntry choice)
        {
            ContextMenuExecutionResult result = new ContextMenuExecutionResult();
            if (choice.DisplayText == Translation.Instance.DeleteAll)
            {
                result.RefreshCurrentItems = OnlineVideoSettings.Instance.FavDB.RemoveAllFavoriteVideos(((RssLink)selectedCategory).Url);
                // we have to manually refresh the categories
                if (result.RefreshCurrentItems && selectedCategory.ParentCategory != null) DiscoverDynamicCategories();
            }
            else if (choice.DisplayText == Translation.Instance.RemoveFromFavorites)
            {
                if (selectedCategory is FavoriteCategory)
                {
                    result.RefreshCurrentItems = OnlineVideoSettings.Instance.FavDB.RemoveFavoriteCategory(((FavoriteCategory)selectedCategory).FavoriteDbCategory);
                    if (result.RefreshCurrentItems) selectedCategory.ParentCategory.SubCategories.Remove(selectedCategory);
                    return result;
                }
                else
                {
                    result.RefreshCurrentItems = OnlineVideoSettings.Instance.FavDB.RemoveFavoriteVideo(selectedItem as FavoriteDbVideoInfo);
                }
                // we have to manually refresh the categories
                if (result.RefreshCurrentItems && selectedCategory.ParentCategory != null) DiscoverDynamicCategories();
            }
            return result;
        }

        #endregion

        #region IFilter Member

        string _lastSort = "default";

        public List<VideoInfo> FilterVideos(Category category, int maxResult, string orderBy, string timeFrame)
        {
            _lastSort = orderBy;
            return GetVideos(category);
        }

        public List<VideoInfo> FilterSearchResults(string query, int maxResult, string orderBy, string timeFrame)
        {
            _lastSort = orderBy;
            return OrderVideos(OnlineVideoSettings.Instance.FavDB.GetFavoriteVideos(null, query));
        }

        public List<VideoInfo> FilterSearchResults(string query, string category, int maxResult, string orderBy, string timeFrame)
        {
            return null;
        }

        public List<int> GetResultSteps()
        {
            return new List<int>();
        }

        public Dictionary<string, string> GetOrderByOptions()
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add(Translation.Instance.Default, "default");
            options.Add(Translation.Instance.Name, "name");
            options.Add(Translation.Instance.Airdate, "airdate");
            options.Add(Translation.Instance.Runtime, "runtime");
            return options;
        }

        public Dictionary<string, string> GetTimeFrameOptions()
        {
            return new Dictionary<string, string>();
        }

        List<VideoInfo> OrderVideos(List<VideoInfo> videos)
        {
            switch (_lastSort)
            {
                case "name":
                    videos.Sort((Comparison<VideoInfo>)((v1, v2) => v1.Title.CompareTo(v2.Title)));
                    break;
                case "airdate":
                    videos.Sort((Comparison<VideoInfo>)delegate (VideoInfo v1, VideoInfo v2)
                    {
                        DateTime airdate_v1;
                        DateTime airdate_v2;
                        if (DateTime.TryParse(v1.Airdate, out airdate_v1) && DateTime.TryParse(v2.Airdate, out airdate_v2))
                            return airdate_v2.CompareTo(airdate_v1);
                        else
                            return v2.Airdate.CompareTo(v1.Airdate); // string compare as fallback
                    });
                    break;
                case "runtime":
                    videos.Sort((Comparison<VideoInfo>)delegate (VideoInfo v1, VideoInfo v2)
                    {
                        double seconds_v1 = TryGetSeconds(v1.Length);
                        double seconds_v2 = TryGetSeconds(v2.Length);
                        if (seconds_v1 != 0.0f && seconds_v2 != 0.0f)
                            return seconds_v2.CompareTo(seconds_v1);
                        else
                            return v2.Length.CompareTo(v1.Length); // string compare as fallback
                    });
                    break;
            }
            return videos;
        }

        double TryGetSeconds(string length)
        {
            // might be a single number
            double seconds;
            if (!double.TryParse(length, System.Globalization.NumberStyles.None | System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.CreateSpecificCulture("en-US"), out seconds))
            {
                // might be in HH:mm:ss notation
                TimeSpan timespan;
                if (TimeSpan.TryParse(length, out timespan))
                {
                    seconds = timespan.TotalSeconds;
                }
            }
            return seconds;
        }

        #endregion

    }

}
