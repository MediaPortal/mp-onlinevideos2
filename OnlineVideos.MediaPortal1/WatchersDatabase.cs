using System;
using System.Text;
using System.Collections.Generic;
using SQLite.NET;
using MediaPortal.Configuration;
using MediaPortal.Database;
using MediaPortal.Services;

namespace OnlineVideos.MediaPortal1
{
    public class WatchersDatabase : MarshalByRefObject, IWatchersDatabase
    {
        private readonly SQLiteClient _SqlClient;
        private readonly System.Timers.Timer _WatchersTimer;
        private readonly List<WatcherDbCategory> _CachedWatchers = new List<WatcherDbCategory>();
        private volatile bool _Enabled = false;

        private static WatchersDatabase _Instance;

        public static WatchersDatabase Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WatchersDatabase();

                return _Instance;
            }
        }

        private WatchersDatabase()
        {
            try
            {
                this._SqlClient = Database.Instance.Client;
                DatabaseUtility.AddTable(this._SqlClient, "WATCHER_Categories", "CREATE TABLE WATCHER_Categories(CAT_ID integer primary key autoincrement,CAT_Name text,CAT_Desc text,CAT_ThumbUrl text,CAT_Hierarchy text,CAT_SITE_ID text, CAT_IS_SEARCH boolean, SEARCH_CAT_HASSUBS boolean, CAT_PERIOD integer, CAT_LAST_REFRESH text, CAT_LAST_VDO text, CAT_LINK text)\n");
                this._CachedWatchers.AddRange(this.GetCategories(null));

                this._WatchersTimer = new System.Timers.Timer
                {
                    Interval = 60000,
                    AutoReset = false
                };
                this._WatchersTimer.Elapsed += this.cbWatchersTimer;
            }
            catch (SQLiteException ex)
            {
                Log.Instance.Error("[WatchersDatabase][ctor] Exception err:{0} stack:{1}", ex.Message, ex.StackTrace);
            }
        }

        public List<KeyValuePair<string, uint>> GetSiteIds()
        {
            lock (this._CachedWatchers)
            {
                string strSQL = @"select CAT_SITE_ID, count(*) as ItemCount from WATCHER_Categories group by CAT_SITE_ID";
                SQLiteResultSet sqlResult = this._SqlClient.Execute(strSQL);
                List<KeyValuePair<string, uint>> siteIdList = new List<KeyValuePair<string, uint>>();
                for (int iRow = 0; iRow < sqlResult.Rows.Count; iRow++)
                    siteIdList.Add(new KeyValuePair<string, uint>(DatabaseUtility.Get(sqlResult, iRow, "CAT_SITE_ID"), (uint)DatabaseUtility.GetAsInt(sqlResult, iRow, "ItemCount")));

                return siteIdList;
            }
        }

        public int AddCategory(Category cat, string strSiteName, int iPeriod, DateTime dtLastRefresh, string strLastVideo)
        {
            lock (this._CachedWatchers)
            {
                DatabaseUtility.RemoveInvalidChars(ref strSiteName);
                string strCategoryHierarchyName = DatabaseUtility.RemoveInvalidChars(cat.RecursiveName("|"));

                //check if the category is already in the favorite list
                if (this._SqlClient.Execute(string.Format("select CAT_ID from WATCHER_Categories where CAT_Hierarchy='{0}' AND CAT_SITE_ID='{1}'",
                    strCategoryHierarchyName, strSiteName)).Rows.Count > 0)
                {
                    Log.Instance.Info("Watcher Category {0} already in database", cat.Name);
                    return 0;
                }

                Log.Instance.Info("inserting watcher category on site {0} with name: {1}, desc: {2}, image: {3}",
                    strSiteName, cat.Name, cat.Description, cat.Thumb, strSiteName);

                string strSQL =
                    string.Format(
                        "insert into WATCHER_Categories(CAT_Name,CAT_Desc,CAT_ThumbUrl,CAT_Hierarchy,CAT_SITE_ID,CAT_IS_SEARCH,SEARCH_CAT_HASSUBS,CAT_PERIOD,CAT_LAST_REFRESH,CAT_LAST_VDO,CAT_LINK) " +
                        "VALUES('{0}','{1}','{2}','{3}','{4}',{5},{6},{7},'{8}','{9}','{10}')",
                        DatabaseUtility.RemoveInvalidChars(cat.Name),
                        cat.Description == null ? "" : DatabaseUtility.RemoveInvalidChars(cat.Description),
                        cat.Thumb,
                        strCategoryHierarchyName,
                        strSiteName,
                        cat.ParentCategory is SearchCategory,
                        cat.HasSubCategories,
                        iPeriod,
                        dtLastRefresh.ToUniversalTime(),
                        DatabaseUtility.RemoveInvalidChars(strLastVideo),
                        !string.IsNullOrWhiteSpace(cat.TagLink) ? DatabaseUtility.RemoveInvalidChars(cat.TagLink) : string.Empty
                        );
                this._SqlClient.Execute(strSQL);
                if (this._SqlClient.ChangedRows() > 0)
                {
                    Log.Instance.Info("Watcher Category '{0}' inserted successfully into database", cat.Name);

                    this._CachedWatchers.Add(
                         new WatcherDbCategory()
                         {
                             Name = cat.Name,
                             Description = cat.Description,
                             Thumb = cat.Thumb,
                             Id = this._SqlClient.LastInsertID(),
                             RecursiveName = strCategoryHierarchyName,
                             IsSearchCat = cat.ParentCategory is SearchCategory,
                             SearchCatHasSubcategories = cat.HasSubCategories,
                             RefreshPeriod = iPeriod,
                             LastRefresh = dtLastRefresh,
                             LastVideo = strLastVideo,
                             SiteId = strSiteName,
                             TagLink = cat.TagLink
                         });

                    this._WatchersTimer.Interval = 60000;


                    return this._SqlClient.LastInsertID();
                }
                else
                {
                    Log.Instance.Warn("Watcher Category '{0}' failed to insert into database", cat.Name);
                    return -1;
                }
            }
        }

        public List<WatcherDbCategory> GetCategories(string strSiteName)
        {
            lock (this._CachedWatchers)
            {
                SQLiteResultSet sqlResult;
                List<WatcherDbCategory> results = new List<WatcherDbCategory>();
                if (strSiteName != null)
                {
                    DatabaseUtility.RemoveInvalidChars(ref strSiteName);
                    sqlResult = this._SqlClient.Execute(string.Format("select * from WATCHER_Categories where CAT_SITE_ID = '{0}'", strSiteName));
                }
                else
                    sqlResult = this._SqlClient.Execute("select * from WATCHER_Categories");

                for (int iRow = 0; iRow < sqlResult.Rows.Count; iRow++)
                    results.Add(createCategory(sqlResult, iRow));

                return results;
            }
        }

        public WatcherDbCategory GetCategory(int iId)
        {
            lock (this._CachedWatchers)
            {
                SQLiteResultSet sqlResult = this._SqlClient.Execute(string.Format("select * from WATCHER_Categories where CAT_ID = '{0}'", iId));

                if (sqlResult.Rows.Count > 0)
                    return createCategory(sqlResult, 0);
            }
            return null;
        }

        public List<string> GetCategoriesNames(string strSiteName)
        {
            DatabaseUtility.RemoveInvalidChars(ref strSiteName);
            List<string> results = new List<string>();
            SQLiteResultSet sqlResult = this._SqlClient.Execute(string.Format("select CAT_Hierarchy from WATCHER_Categories where CAT_SITE_ID = '{0}'", strSiteName));
            for (int iRow = 0; iRow < sqlResult.Rows.Count; iRow++)
            {
                results.Add(DatabaseUtility.Get(sqlResult, iRow, "CAT_Hierarchy"));
            }
            return results;
        }

        public bool UpdateCategory(int iId, int iPeriod, DateTime dtLastRefresh, string strLastVideo)
        {
            lock (this._CachedWatchers)
            {
                this._SqlClient.Execute(string.Format("UPDATE WATCHER_Categories SET CAT_LAST_REFRESH='{1}', CAT_LAST_VDO='{2}', CAT_PERIOD={3} WHERE CAT_ID = '{0}'",
                    iId, dtLastRefresh.ToUniversalTime(), DatabaseUtility.RemoveInvalidChars(strLastVideo), iPeriod));

                if (this._SqlClient.ChangedRows() > 0)
                {
                    WatcherDbCategory cat = this._CachedWatchers.Find(c => c.Id == iId);
                    cat.RefreshPeriod = iPeriod;
                    cat.LastRefresh = dtLastRefresh;
                    cat.LastVideo = strLastVideo;

                    return true;
                }
            }

            return false;
        }

        public bool RemoveCategory(WatcherDbCategory cat)
        {
            lock (this._CachedWatchers)
            {
                string strSQL = string.Format("delete from WATCHER_Categories where CAT_ID = '{0}'", cat.Id);
                this._SqlClient.Execute(strSQL);
                if (this._SqlClient.ChangedRows() > 0)
                {
                    this._CachedWatchers.Remove(cat);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveCategory(string strSiteName, string strRecursiveCategoryName)
        {
            lock (this._CachedWatchers)
            {
                DatabaseUtility.RemoveInvalidChars(ref strSiteName);
                string strSQL = string.Format("delete from WATCHER_Categories where CAT_Hierarchy='{0}' AND CAT_SITE_ID='{1}'", strRecursiveCategoryName, strSiteName);
                this._SqlClient.Execute(strSQL);
                if (this._SqlClient.ChangedRows() > 0)
                {
                    this._CachedWatchers.Remove(this._CachedWatchers.Find(c => c.SiteId == strSiteName && c.RecursiveName == strRecursiveCategoryName));
                    return true;
                }
            }
            return false;
        }

        public bool CategoryExists(string strSiteName, string strCategoryName)
        {
            lock (this._CachedWatchers)
            {
                return this._CachedWatchers.Exists(c => c.SiteId == strSiteName && c.RecursiveName == strCategoryName);
            }
        }

        public void Start()
        {
            if (!this._Enabled)
            {
                this._Enabled = true;
                Log.Instance.Debug("[WatchersDatabase] Start");
                this._WatchersTimer.Interval = 60000;
                this._WatchersTimer.Start();
                
            }
        }

        public void Stop()
        {
            if (this._Enabled)
            {
                this._Enabled = false;
                Log.Instance.Debug("[WatchersDatabase] Stop");
                this._WatchersTimer.Stop();
            }
        }

        public void Refresh()
        {
            lock (this._CachedWatchers)
            {
                this.watchersRefresh();
                if (this._Enabled)
                    this._WatchersTimer.Start();
            }
        }

        #region MarshalByRefObject overrides
        public override object InitializeLifetimeService()
        {
            // In order to have the lease across appdomains live forever, we return null.
            return null;
        }
        #endregion

        public bool SetPreferredLayout(string strSiteName, Category cat, int iLayout)
        {
            try
            {
                if (string.IsNullOrEmpty(strSiteName))
                    return false;

                DatabaseUtility.RemoveInvalidChars(ref strSiteName);
                string strCategoryHierarchyName = cat != null ? DatabaseUtility.RemoveInvalidChars(cat.RecursiveName("|")) : string.Empty;
                this._SqlClient.Execute(string.Format("insert into PREFERRED_LAYOUT(Site_Name, Category_Hierarchy, Layout) VALUES ('{0}','{1}',{2})",
                    strSiteName, strCategoryHierarchyName, iLayout));

                return this._SqlClient.ChangedRows() > 0;
            }
            catch (Exception ex)
            {
                Log.Instance.Warn("Exception storing preferred Layout in DB: {0}", ex.ToString());
                return false;
            }
        }

        public MediaPortal.GUI.Library.GUIFacadeControl.Layout? GetPreferredLayout(string strSiteName, Category cat)
        {
            try
            {
                if (string.IsNullOrEmpty(strSiteName)) 
                    return null;
                DatabaseUtility.RemoveInvalidChars(ref strSiteName);
                string strCategoryHierarchyName = cat != null ? DatabaseUtility.RemoveInvalidChars(cat.RecursiveName("|")) : string.Empty;
                if (!string.IsNullOrEmpty(strCategoryHierarchyName))
                {
                    SQLiteResultSet sqlresult = this._SqlClient.Execute(string.Format("SELECT Layout FROM PREFERRED_LAYOUT WHERE Site_Name = '{0}' AND Category_Hierarchy = '{1}'",
                        strSiteName, strCategoryHierarchyName));

                    if (sqlresult.Rows.Count > 0)
                        return (MediaPortal.GUI.Library.GUIFacadeControl.Layout)int.Parse(DatabaseUtility.Get(sqlresult, 0, "Layout"));
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Instance.Warn("Exception getting preferred Layout from DB: {0}", ex.ToString());
                return null;
            }
        }


        private void cbWatchersTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (this._CachedWatchers)
            {
                this.watchersRefresh();
                if (this._Enabled)
                    this._WatchersTimer.Start();
            }
        }

        private int watchersRefresh()
        {
            Log.Instance.Debug("[WatchersDatabase][watchersRefresh] Running...");

            StringBuilder sb = new StringBuilder(256);
            DateTime dtNow = DateTime.Now.AddMinutes(1);
            DateTime dtTargetRefresh = DateTime.Now.AddDays(1);
            for (int i = 0; i < this._CachedWatchers.Count; i++)
            {
                WatcherDbCategory catWatcher = this._CachedWatchers[i];
                DateTime dtTarget = catWatcher.NextRefresh;
                if (dtTarget <= dtNow)
                {
                    //Category refresh is in due
                    try
                    {
                        Category cat;

                        string strRecursiveName = catWatcher.RecursiveName.Replace('|', '/');

                        Log.Instance.Debug("[WatchersDatabase][watchersRefresh] Refreshing: '{0} / {1}'  LastVideo: {2}",
                            catWatcher.SiteId, strRecursiveName, catWatcher.LastVideo);

                        List<VideoInfo> videos = null;
                        try
                        {
                            if (catWatcher.Site is Sites.ILastCategoryVideos)
                            {
                                if (!string.IsNullOrWhiteSpace(catWatcher.TagLink)) //we have direct link; do not use recursive path to get category
                                    videos = ((Sites.ILastCategoryVideos)catWatcher.Site).GetLatestVideos(catWatcher.LastRefresh, catWatcher.LastVideo, catWatcher);
                                else if ((cat = catWatcher.SiteCategory) != null)
                                    videos = ((Sites.ILastCategoryVideos)catWatcher.Site).GetLatestVideos(catWatcher.LastRefresh, catWatcher.LastVideo, cat);
                            }
                            else if ((cat = catWatcher.SiteCategory) != null)
                                videos = catWatcher.Site.GetVideos(cat);
                        }
                        catch (Exception ex)
                        {
                            Log.Instance.Error("[WatchersDatabase][watchersRefresh] Error getting videos: {0}", ex.Message);
                            catWatcher.ErrorCounter++;
                            continue;
                        }

                        if (videos != null)
                        {
                            int iCnt = 10; //max videos to report
                            string strLastVideo = null;
                            INotifyMessageService srvc = GlobalServiceProvider.Get<INotifyMessageService>();
                            for (int iIdxVideo = 0; iIdxVideo < videos.Count && iCnt > 0; iIdxVideo++)
                            {
                                VideoInfo vi = videos[iIdxVideo];
                                if (!string.IsNullOrWhiteSpace(vi.VideoUrl))
                                {
                                    if (string.IsNullOrWhiteSpace(catWatcher.LastVideo) || !vi.VideoUrl.Equals(catWatcher.LastVideo))
                                    {
                                        if (strLastVideo == null)
                                            strLastVideo = vi.VideoUrl;

                                        if (!DateTime.TryParse(vi.Airdate, out DateTime dtPublish))
                                            dtPublish = DateTime.Now;

                                        Log.Instance.Debug("[WatchersDatabase][watchersRefresh] New video: '{0}'  Url: {1}", vi.Title, vi.VideoUrl);

                                        //Send message to the MediaPortal's notification service
                                        if (srvc != null)
                                        {
                                            //Description
                                            sb.Clear();

                                            if (!string.IsNullOrWhiteSpace(vi.Airdate))
                                            {
                                                sb.Append(Translation.Instance.Airdate);
                                                sb.Append(": ");
                                                sb.Append(vi.Airdate);
                                                sb.Append(Environment.NewLine);
                                            }

                                            if (!string.IsNullOrWhiteSpace(vi.Length))
                                            {
                                                sb.Append(Translation.Instance.Runtime);
                                                sb.Append(": ");
                                                sb.Append(vi.Length);
                                                sb.Append(Environment.NewLine);
                                            }

                                            if (!string.IsNullOrWhiteSpace(vi.Description))
                                                sb.Append(vi.Description);

                                            srvc.MessageRegister(
                                                vi.Title,
                                                "Online Videos",
                                                GUIOnlineVideos.WindowId,
                                                dtPublish,
                                                out string strToken,
                                                strDescription: sb.ToString(),
                                                strOriginLogo: OnlineVideoSettings.Instance.ThumbsDir + @"Icons\OnlineVideos.png",
                                                strThumb: vi.Thumb,
                                                strAuthor: catWatcher.SiteId + ": " + strRecursiveName,
                                                strPluginArgs: "site:" + catWatcher.SiteId + "|category:" + strRecursiveName + "|return:Locked|video:" + System.Web.HttpUtility.UrlEncode(vi.Title),
                                                level: NotifyMessageLevelEnum.Information,
                                                cls: NotifyMessageClassEnum.News | NotifyMessageClassEnum.Video | NotifyMessageClassEnum.Online,
                                                strMediaLink: vi.VideoUrl
                                                );
                                        }

                                        iCnt--;
                                    }
                                    else
                                        break;
                                }
                            }

                            if (strLastVideo != null && catWatcher.LastVideo != strLastVideo)
                                catWatcher.LastVideo = strLastVideo;

                            catWatcher.LastRefresh = DateTime.Now;
                            this.UpdateCategory(catWatcher.Id, catWatcher.RefreshPeriod, catWatcher.LastRefresh, catWatcher.LastVideo);

                            catWatcher.ErrorCounter = 0;
                        }
                        else
                            catWatcher.ErrorCounter++;
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.Error("[WatchersDatabase][watchersRefresh] Error: {0}", ex.Message);
                        catWatcher.ErrorCounter++;
                    }
                }    

                dtTarget = catWatcher.NextRefresh;

                if (dtTarget < dtTargetRefresh)
                    dtTargetRefresh = dtTarget;
            }

            this._WatchersTimer.Interval = Math.Max(60000, (dtTargetRefresh - DateTime.Now).TotalMilliseconds);

            Log.Instance.Debug("[WatchersDatabase][watchersRefresh] Next refresh: {0}", DateTime.Now.AddMilliseconds(this._WatchersTimer.Interval));

            return 0;
        }

        private static WatcherDbCategory createCategory(SQLiteResultSet sqlResult, int iIdx)
        {
            return new WatcherDbCategory()
            {
                Name = DatabaseUtility.Get(sqlResult, iIdx, "CAT_Name"),
                Description = DatabaseUtility.Get(sqlResult, iIdx, "CAT_Desc"),
                Thumb = DatabaseUtility.Get(sqlResult, iIdx, "CAT_ThumbUrl"),
                Id = DatabaseUtility.GetAsInt(sqlResult, iIdx, "CAT_ID"),
                RecursiveName = DatabaseUtility.Get(sqlResult, iIdx, "CAT_Hierarchy"),
                IsSearchCat = DatabaseUtility.GetAsInt(sqlResult, iIdx, "CAT_IS_SEARCH") == 1,
                SearchCatHasSubcategories = DatabaseUtility.GetAsInt(sqlResult, iIdx, "SEARCH_CAT_HASSUBS") == 1,
                RefreshPeriod = DatabaseUtility.GetAsInt(sqlResult, iIdx, "CAT_PERIOD"),
                LastRefresh = DateTime.Parse(DatabaseUtility.Get(sqlResult, iIdx, "CAT_LAST_REFRESH")).ToLocalTime(),
                LastVideo = DatabaseUtility.Get(sqlResult, iIdx, "CAT_LAST_VDO"),
                SiteId = DatabaseUtility.Get(sqlResult, iIdx, "CAT_SITE_ID"),
                TagLink = DatabaseUtility.Get(sqlResult, iIdx, "CAT_LINK"),
            };
        }
    }
}
