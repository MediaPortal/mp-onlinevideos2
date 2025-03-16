using System.Collections.Generic;
using System.Linq;

namespace OnlineVideos
{
    /// <summary>
    /// This interface defines the contract a class should implement that can be used to manage watching categories for sites.
    /// </summary>
    public interface IWatchersDatabase
    {
        /// <summary>Gets a list of sites that have watchers.</summary>
        /// <returns>A list of key value pairs that represent the site's name and the number of watchers.</returns>
        List<KeyValuePair<string, uint>> GetSiteIds();

        /// <summary>Gets a list of watching categories of the given site.</summary>
        /// <param name="strSiteId">The name of a site to return categories for</param>
        /// <returns>A list of <see cref="FavoriteDbCategory"/> objects for the given site.</returns>
        List<WatcherDbCategory> GetCategories(string strSiteId);

        /// <summary>Gets the names of the watching categories of the given site.</summary>
        /// <param name="strSiteId">The name of the site to return categories for</param>
        /// <returns>A list of category names</returns>
        List<string> GetCategoriesNames(string strSiteId);

        /// <summary>Adds a category to the watchers of the given site.</summary>
        /// <param name="category">The category object to use for storing data.</param>
        /// <param name="strSiteId">The name of the site this category belongs to.</param>
        /// <param name="iPeriod">Refresh period in minutes.</param>
        /// <param name="strLastVideo">URL of last video.</param>
        /// <param name="strTag">User's tag saved to the database after each refresh.</param>
        /// <returns>true when the category was successfully added</returns>
        int AddCategory(Category category, string strSiteId, int iPeriod, System.DateTime dtLastRefresh, string strLastVideo, string strTag);

        /// <summary>Remove the category from watchers.</summary>
        /// <param name="category">The category to remove.</param>
        /// <returns>true when the category was removed.</returns>
        bool RemoveCategory(WatcherDbCategory category);

        /// <summary>Remove a cetegory from watchers using the recursive catgeory name.</summary>
        /// <param name="strSiteName">The name of the site this category belongs to.</param>
        /// <param name="strRecursiveCategoryName">The recursive category name.</param>
        /// <returns>true when the category was removed from the watchers of this site.</returns>
        bool RemoveCategory(string strSiteName, string strRecursiveCategoryName);

        WatcherDbCategory GetCategory(int iId);

        bool UpdateCategory(int iId, int iPeriod, System.DateTime dtLastRefresh, string strLastVideo, string strTag);

        bool CategoryExists(string strSiteName, string strCategoryName);

        void Start();

        void Stop();

        void Refresh();
    }

    /// <summary>A special class for categories retrieved from the watcher database.</summary>
    public class WatcherDbCategory : Category
    {
        private Sites.SiteUtilBase _Site = null;

        /// <summary>Holds an unique Id for the Category, so it can be deleted from the DB.</summary>
        public int Id { get; set; }

        /// <summary>Holds the recursive name of the original category.</summary>
        public new string RecursiveName { get; set; }

        public bool IsSearchCat { get; set; }

        //only valid if IsSearchCat=true
        public bool SearchCatHasSubcategories { get; set; }

        public int RefreshPeriod { get; set; }

        public string LastVideo { get; set; }

        public System.DateTime LastRefresh { get; set; }

        public System.DateTime NextRefresh
        {
            get
            {
                return this.LastRefresh.AddMinutes(this.ErrorCounter > 0 ? System.Math.Min(120, System.Math.Pow(2, this.ErrorCounter)) : this.RefreshPeriod);
            }
        }
    

        public int ErrorCounter { get; set; }

        public string SiteId { get; set; }


        public Sites.SiteUtilBase Site 
        {
            get
            {
                if (this._Site == null)
                    OnlineVideoSettings.Instance.SiteUtilsList.TryGetValue(this.SiteId, out this._Site);

                return this._Site;
            }
        }

        public Category SiteCategory
        {
            get
            {
                if (this._SiteCategory != null)
                    return this._SiteCategory;

                if (this.IsSearchCat)
                {
                    this._SiteCategory = new RssLink()
                    {
                        Name = this.Name,
                        Url = this.RecursiveName,
                        HasSubCategories = this.SearchCatHasSubcategories
                    };
                }
                else
                {
                    if (OnlineVideoSettings.Instance.SiteUtilsList.TryGetValue(this.SiteId, out Sites.SiteUtilBase site))
                    {
                        int iAttempts;
                        int iCnt;

                        string[] hierarchy = this.RecursiveName.Split('|');
                        for (int i = 0; i < hierarchy.Length; i++)
                        {
                            if (this._SiteCategory != null)
                            {
                                if (!this._SiteCategory.SubCategoriesDiscovered)
                                    site.DiscoverSubCategories(this._SiteCategory);

                                Category foundCat = this._SiteCategory.SubCategories.FirstOrDefault(c => c.Name == hierarchy[i]);
                                if (this._SiteCategory.SubCategories.Count > 0)
                                {
                                    iAttempts = 20;

                                    // nextpage until found or no more
                                    while (foundCat == null && this._SiteCategory.SubCategories.Last() is NextPageCategory)
                                    {
                                        if (iAttempts-- < 0)
                                        {
                                            Log.Error("[SiteCategory] Page count limit reached.");
                                            break;
                                        }

                                        iCnt = this._SiteCategory.SubCategories.Count;

                                        if (site.DiscoverNextPageCategories(this._SiteCategory.SubCategories.Last() as NextPageCategory) == 0)
                                            break;

                                        if (iCnt == this._SiteCategory.SubCategories.Count)
                                            break;

                                        foundCat = this._SiteCategory.SubCategories.FirstOrDefault(c => c.Name == hierarchy[i]);
                                    }
                                    this._SiteCategory = foundCat;
                                }
                            }
                            else
                            {
                                if (!site.Settings.DynamicCategoriesDiscovered) 
                                    site.DiscoverDynamicCategories();

                                Category foundCat = site.Settings.Categories.FirstOrDefault(c => c.Name == hierarchy[i]);
                                if (site.Settings.Categories.Count() > 0)
                                {
                                    iAttempts = 20;

                                    // nextpage until found or no more
                                    while (foundCat == null && site.Settings.Categories.Last() is NextPageCategory)
                                    {
                                        if (iAttempts-- < 0)
                                        {
                                            Log.Error("[SiteCategory] Page count limit reached.");
                                            break;
                                        }

                                        iCnt = site.Settings.Categories.Count();

                                        if (site.DiscoverNextPageCategories(site.Settings.Categories.Last() as NextPageCategory) == 0)
                                            break;

                                        if (iCnt == site.Settings.Categories.Count())
                                            break;

                                        foundCat = site.Settings.Categories.FirstOrDefault(c => c.Name == hierarchy[i]);
                                    }
                                    this._SiteCategory = foundCat;
                                }
                            }

                            if (this._SiteCategory == null)
                                break;
                        }

                        this._Site = site;
                    }
                }

                return this._SiteCategory;
            }
        } private Category _SiteCategory = null;
    }
}
