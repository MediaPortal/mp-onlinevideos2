﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using OnlineVideos.Sites.JSurf.Factories;
using OnlineVideos.Sites.JSurf.Interfaces;
using OnlineVideos.Sites.JSurf.Entities;
using OnlineVideos.Sites.Interfaces;
using OnlineVideos.Sites.JSurf.Properties;
using OnlineVideos.Sites.JSurf.ConnectorImplementations.AmazonPrime.Connectors;

namespace OnlineVideos.Sites.JSurf
{
    /// <summary>
    /// General Util class for web automation - that is where we load the information by scraping the website and play via a browser
    /// </summary>
    public class AmazonPrimeSiteUtil : SiteUtilBase, IBrowserVersionEmulation, IInputStreamSite
    {
        IInformationConnector _connector;

        /// <summary>
        /// The object type of the connector to use - required for IBrowserSiteUtil
        /// </summary>
        public string ConnectorEntityTypeName
        {
            get { return _connector.ConnectorEntityTypeName; }
        }

        /// <summary>
        /// The username - required for IBrowserSiteUtil
        /// </summary>
        public string UserName
        {
            get { return username; }
        }

        /// <summary>
        /// The password - required for IBrowserSiteUtil
        /// </summary>
        public string Password
        {
            get { return password; }
        }

        /// <summary>
        /// The AmznAdultPin
        /// </summary>
        public string AmznAdultPin
        {
            get { return amznAdultPin; }
        }

        /// <summary>
        /// The Amazon player type
        /// </summary>
        public AmazonPlayerType AmznPlayerType
        {
            get { return amznPlayerType; }
        }

        public enum AmazonPlayerType { InputStream, /*Browser, */ BrowserHTML5 };

        [Category("OnlineVideosConfiguration"), Description("Type of web automation to run")]
        ConnectorType webAutomationType = ConnectorType.AmazonPrime;

        /// <summary>
        /// Username required for some web automation
        /// </summary>
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Login"), Description("Website user name")]
        string username;

        /// <summary>
        /// Password required for some web automation
        /// </summary>
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Password"), Description("Website password"), PasswordPropertyText(true)]
        string password;

        /// <summary>
        /// Password required for some web automation
        /// </summary>
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Amazon Adult Pin"), Description("Amazon Adult Pin"), PasswordPropertyText(true)]
        string amznAdultPin;

        /// <summary>
        /// Player type for amazon
        /// </summary>
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Amazon Player Type"), Description("Amazon Player Type, InputStream or BrowserHTML5")]
        AmazonPlayerType amznPlayerType = AmazonPlayerType.BrowserHTML5;

        /// <summary>
        /// Set the Web Automation Description from the enum
        /// </summary>
        /// <param name="siteSettings"></param>
        public override void Initialize(SiteSettings siteSettings)
        {
            base.Initialize(siteSettings);
            Resources.ResourceManager = new SingleAssemblyComponentResourceManager(typeof(Resources));
            _connector = ConnectorFactory.GetInformationConnector(webAutomationType, this);
        }

        /// <summary>
        /// Load the list of videos - see if they've been pre-loaded when populating categories or not
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public override List<VideoInfo> GetVideos(Category category)
        {
            var result = new List<VideoInfo>();
            BuildVideos(result, category);
            return result;
        }

        /// <summary>
        /// Override the loading of main categories
        /// </summary>
        /// <returns></returns>
        public override int DiscoverDynamicCategories()
        {
            Settings.DynamicCategoriesDiscovered = true;
            BuildCategories(null, Settings.Categories);
            return Settings.Categories.Count;
        }

        /// <summary>
        /// Override the loading of sub categories
        /// </summary>
        /// <param name="parentCategory"></param>
        /// <returns></returns>
        public override int DiscoverSubCategories(Category parentCategory)
        {
            if (parentCategory != null && parentCategory.SubCategories != null && parentCategory.SubCategories.Count > 0)
                return 1;
            parentCategory.SubCategories = new List<Category>();
            BuildCategories(parentCategory, null);

            parentCategory.SubCategoriesDiscovered = true;
            if (_connector.ShouldSortResults)
                parentCategory.SubCategories = parentCategory.SubCategories.OrderBy(GetCategorySortField).ToList();

            return parentCategory.SubCategories.Count;
        }

        /// <summary>
        /// Get the sort field for the category 
        /// </summary>
        /// <param name="categ"></param>
        /// <returns></returns>
        private string GetCategorySortField(Category categ)
        {
            var categTyped = categ as ExtendedCategory;
            if (categTyped == null) return categ.Name;
            return categTyped.SortValue;
        }

        public override List<String> GetMultipleVideoUrls(VideoInfo video, bool inPlaylist = false)
        {
            return _connector is AmazonPrimeInformationConnector ? ((AmazonPrimeInformationConnector)_connector).getMultipleVideoUrls(video, inPlaylist) : new List<string> { video.Other.ToString() };
        }

        /// <summary>
        /// Get the next page of categories
        /// </summary>
        /// <param name="nextPagecategory"></param>
        /// <returns></returns>
        public override int DiscoverNextPageCategories(NextPageCategory nextPagecategory)
        {
            nextPagecategory.ParentCategory.SubCategories.Remove(nextPagecategory);
            BuildCategories(nextPagecategory, null);
            return nextPagecategory.ParentCategory.SubCategories.Count;
        }

        /// <summary>
        /// Build the specified category list
        /// </summary>
        /// <param name="parentCategory"></param>
        /// <param name="categoriesToPopulate"></param>
        private void BuildCategories(Category parentCategory, IList<Category> categoriesToPopulate)
        {
            //if (categoriesToPopulate != null) categoriesToPopulate.Clear();

            var results = _connector.LoadCategories(parentCategory);
            if (parentCategory == null)
                results.ForEach(categoriesToPopulate.Add);
        }

        /// <summary>
        /// Build the video list for the specified category
        /// </summary>
        /// <param name="videosToPopulate"></param>
        /// <param name="parentCategory"></param>
        private void BuildVideos(IList<VideoInfo> videosToPopulate, Category parentCategory)
        {
            var results = _connector.LoadVideos(parentCategory);
            results.ForEach(videosToPopulate.Add);
        }

        public override bool CanSearch { get { return _connector.CanSearch; } }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            return _connector.DoSearch(query);
        }

        public int EmulatedVersion
        {
            get
            {
                return 12000 /* IE11/Edge with HTML5*/;
            }
        }

        public bool GetStreamProperties(VideoInfo videoInfo, out Dictionary<string, string> properties)
        {
            properties = null;
            AmazonPrimeInformationConnector ap = (AmazonPrimeInformationConnector)_connector;
            if (ap == null)
                return false;

            if (AmznPlayerType != AmazonPlayerType.InputStream)
                return false;
            string streamUrl;
            string licenseUrl;
            Dictionary<string, string> additionalTags;
            if (!ap.GetInputStreamProperties(videoInfo, out streamUrl, out licenseUrl, out additionalTags))
                return false;

            properties = new Dictionary<string, string>
              {
                { "inputstreamaddon", "inputstream.adaptive" },
                { "inputstream.adaptive.manifest_type", "mpd" },
                { "inputstream.adaptive.license_type", "com.widevine.alpha" },
                { "inputstream.adaptive.license_key", licenseUrl },
                { "inputstream.streamurl", streamUrl }
              };

            // Copy over all additional tags
            if (additionalTags != null)
                foreach (var tagKeys in additionalTags.Keys)
                    properties[tagKeys] = additionalTags[tagKeys];

            return true;
        }
    }
}
