using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using OnlineVideos.Downloading;

namespace OnlineVideos.Sites
{
    public class DownloadedVideoUtil : SiteUtilBase, IFilter
    {
        string lastSort = "date";

        // keep a reference of all Categories ever created and reuse them, to get them selected when returning to the category view
        Dictionary<string, RssLink> cachedCategories = new Dictionary<string, RssLink>();

        public override int DiscoverDynamicCategories()
        {
            Settings.Categories.Clear();
            RssLink cat = null;
            // add a category for all files
            if (!cachedCategories.TryGetValue(Translation.Instance.All, out cat))
            {
                cat = new RssLink() { Name = Translation.Instance.All, Url = OnlineVideoSettings.Instance.DownloadDir };
                cachedCategories.Add(cat.Name, cat);
            }
            Settings.Categories.Add(cat);

            if (DownloadManager.Instance.Count > 0)
            {
                // add a category for all downloads in progress
                if (!cachedCategories.TryGetValue(Translation.Instance.Downloading, out cat))
                {
                    cat = new RssLink() { Name = Translation.Instance.Downloading, Description = Translation.Instance.DownloadingDescription, EstimatedVideoCount = (uint)DownloadManager.Instance.Count };
                    cachedCategories.Add(cat.Name, cat);
                }
                else
                {
                    cat.EstimatedVideoCount = (uint)DownloadManager.Instance.Count; // refresh the count
                }
                Settings.Categories.Add(cat);
            }

            foreach (string aDir in Directory.GetDirectories(OnlineVideoSettings.Instance.DownloadDir))
            {
                // try to find a SiteUtil according to the directory name
                string siteName = Path.GetFileName(aDir);
                SiteUtilBase util = null;
                OnlineVideoSettings.Instance.SiteUtilsList.TryGetValue(siteName, out util);

                DirectoryInfo dirInfo = new DirectoryInfo(aDir);
                FileInfo[] files = dirInfo.GetFiles();
                if (files.Length == 0)
                {
                    try { Directory.Delete(aDir); }
                    catch { } // try to delete empty directories
                }
                else
                {
                    // treat folders without a corresponding site as adult site
                    if ((util == null && (!OnlineVideoSettings.Instance.UseAgeConfirmation || OnlineVideoSettings.Instance.AgeConfirmed)) ||
                        ((util != null && !util.Settings.ConfirmAge) || !OnlineVideoSettings.Instance.UseAgeConfirmation || OnlineVideoSettings.Instance.AgeConfirmed))
                    {
                        if (!cachedCategories.TryGetValue(siteName + " - " + Translation.Instance.DownloadedVideos, out cat))
                        {
                            cat = new RssLink();
                            cat.Name = siteName + " - " + Translation.Instance.DownloadedVideos;
                            cat.Description = util != null ? util.Settings.Description : "";
                            ((RssLink)cat).Url = aDir;
                            cat.Thumb = Path.Combine(OnlineVideoSettings.Instance.ThumbsDir, @"Icons\" + siteName + ".png");
                            cachedCategories.Add(cat.Name, cat);
                        }
                        cat.EstimatedVideoCount = (uint)files.Count(f => IsPossibleVideo(f.Name));
                        Settings.Categories.Add(cat);
                    }
                }
            }

            // need to always get the categories, because when adding new fav video from a new site, a removing the last one for a site, the categories must be refreshed 
            Settings.DynamicCategoriesDiscovered = false;
            return Settings.Categories.Count;
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            return getVideoList((category as RssLink).Url, "*", category.Name == Translation.Instance.All);
        }

        List<VideoInfo> getVideoList(string path, string search, bool recursive)
        {
            List<VideoInfo> loVideoInfoList = new List<VideoInfo>();
            if (!(string.IsNullOrEmpty(path)))
            {
                FileInfo[] files = new DirectoryInfo(path).GetFiles(search, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foreach (FileInfo file in files)
                {
                    if (IsPossibleVideo(file.Name) && PassesAgeCheck(file.FullName))
                    {
                        string description_xml = "";
                        string airdate_xml = "";
                        string title_xml = "";
                        TrackingInfo ti = new TrackingInfo();
                        // read info from Matroska Xml File if available
                        if (File.Exists(Path.ChangeExtension(file.FullName, ".xml")))
                        {
                            var matroskaTagsXmlDoc = XDocument.Load(Path.ChangeExtension(file.FullName, ".xml"));
                            foreach (var tagNode in matroskaTagsXmlDoc.Document.Descendants("Tag"))
                            {
                                var targetType = tagNode.Descendants("TargetTypeValue").First<XElement>().Value;
                                switch (targetType)
                                {
                                    case "70":
                                        {
                                            ti.VideoKind = VideoKind.TvSeries;
                                            ti.Title = tagNode.Descendants("String").First<XElement>().Value;
                                            break;
                                        }
                                    case "60":
                                        {
                                            ti.Season = Convert.ToUInt32(tagNode.Descendants("String").First<XElement>().Value);
                                            break;
                                        }
                                    case "50":
                                        {
                                            foreach (var simpleNode in tagNode.Descendants("Simple"))
                                            {
                                                if (simpleNode.Element("Name").Value == "TITLE")
                                                {
                                                    title_xml = simpleNode.Element("String").Value;
                                                }
                                                else if (simpleNode.Element("Name").Value == "DESCRIPTION")
                                                    description_xml = simpleNode.Element("String").Value;
                                                else if (simpleNode.Element("Name").Value == "DATE_RELEASED")
                                                {
                                                    airdate_xml = simpleNode.Element("String").Value;
                                                    UInt32 year;
                                                    if (UInt32.TryParse(airdate_xml, out year))
                                                        ti.Year = year;
                                                }
                                                else if (simpleNode.Element("Name").Value == "PART_NUMBER")
                                                    ti.Episode = Convert.ToUInt32(simpleNode.Element("String").Value);
                                                else if (simpleNode.Element("Name").Value == "CONTENT_TYPE")
                                                {
                                                    VideoKind kind;
                                                    if (VideoKind.TryParse(simpleNode.Element("String").Value, out kind))
                                                    {
                                                        ti.VideoKind = kind;
                                                    }
                                                }
                                                else if (simpleNode.Element("Name").Value == "IMDB")
                                                {
                                                    ti.ID_IMDB = simpleNode.Element("String").Value;
                                                }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        if (ti.VideoKind == VideoKind.Movie)
                            ti.Title = title_xml;
                        VideoInfo loVideoInfo = new VideoInfo();
                        loVideoInfo.VideoUrl = file.FullName;
                        loVideoInfo.Thumb = file.FullName.Substring(0, file.FullName.LastIndexOf(".")) + ".jpg";
                        loVideoInfo.Title = string.IsNullOrEmpty(title_xml) ? file.Name : title_xml;
                        loVideoInfo.Length = string.Format("{0} MB", (file.Length / 1024 / 1024).ToString("N0"));
                        loVideoInfo.Airdate = string.IsNullOrEmpty(airdate_xml) ? file.LastWriteTime.ToString("g", OnlineVideoSettings.Instance.Locale) : airdate_xml;
                        loVideoInfo.Description = description_xml;
                        loVideoInfo.TrackingInfo = ti;
                        loVideoInfo.Other = file;
                        loVideoInfoList.Add(loVideoInfo);
                    }
                }

                switch (lastSort)
                {
                    case "name":
                        loVideoInfoList.Sort((Comparison<VideoInfo>)delegate (VideoInfo v1, VideoInfo v2)
                        {
                            return v1.Title.CompareTo(v2.Title);
                        });
                        break;
                    case "date":
                        loVideoInfoList.Sort((Comparison<VideoInfo>)delegate (VideoInfo v1, VideoInfo v2)
                        {
                            return (v2.Other as FileInfo).LastWriteTime.CompareTo((v1.Other as FileInfo).LastWriteTime);
                        });
                        break;
                    case "size":
                        loVideoInfoList.Sort((Comparison<VideoInfo>)delegate (VideoInfo v1, VideoInfo v2)
                        {
                            return (v2.Other as FileInfo).Length.CompareTo((v1.Other as FileInfo).Length);
                        });
                        break;
                }
            }
            else
            {
                foreach (DownloadInfo di in DownloadManager.Instance.GetAll())
                {
                    if (PassesAgeCheck(di.LocalFile))
                    {
                        VideoInfo loVideoInfo = new VideoInfo();
                        loVideoInfo.Title = string.IsNullOrEmpty(di.Title) ? di.VideoInfo.Title : di.Title;
                        loVideoInfo.Thumb = string.IsNullOrEmpty(di.ThumbFile) ? (string.IsNullOrEmpty(di.VideoInfo.ThumbnailImage) ? di.VideoInfo.Thumb : di.VideoInfo.ThumbnailImage) : di.ThumbFile;
                        loVideoInfo.Airdate = di.Start.ToString("HH:mm:ss");
                        loVideoInfo.Length = di.ProgressInfo;
                        loVideoInfo.Description = string.Format("{0}\n{1}", di.Url, di.LocalFile);
                        loVideoInfo.Other = di;
                        loVideoInfo.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == "Other")
                            {
                                (s as VideoInfo).Length = ((s as VideoInfo).Other as DownloadInfo).ProgressInfo;
                                (s as VideoInfo).NotifyPropertyChanged("Length");
                            }
                        };
                        loVideoInfoList.Add(loVideoInfo);
                    }
                }
            }
            return loVideoInfoList;
        }

        public override List<ContextMenuEntry> GetContextMenuEntries(Category selectedCategory, VideoInfo selectedItem)
        {
            List<ContextMenuEntry> options = new List<ContextMenuEntry>();
            if (selectedItem != null)
            {
                if (selectedItem.Other as DownloadInfo == null)
                {
                    options.Add(new ContextMenuEntry()
                    {
                        DisplayText = Translation.Instance.Delete,
                        PromptText = string.Format("{0}: \"{1}?\"", Translation.Instance.Delete, selectedItem.Title),
                        Action = ContextMenuEntry.UIAction.PromptYesNo
                    });

                    options.Add(new ContextMenuEntry()
                    {
                        DisplayText = Translation.Instance.DeleteAll,
                        PromptText = string.Format("{0}?", Translation.Instance.DeleteAll),
                        Action = ContextMenuEntry.UIAction.PromptYesNo
                    });
                }
                else
                {
                    options.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.Cancel });
                }
            }
            return options;
        }

        public override ContextMenuExecutionResult ExecuteContextMenuEntry(Category selectedCategory, VideoInfo selectedItem, ContextMenuEntry choice)
        {
            if (choice.DisplayText == Translation.Instance.Delete)
            {
                try
                {
                    DeleteVideo(selectedItem.VideoUrl);
                }
                catch { } // file might be locked (e.g. still downloading)
            }
            else if (choice.DisplayText == Translation.Instance.DeleteAll)
            {
                FileInfo[] files = new DirectoryInfo((selectedCategory as RssLink).Url).GetFiles("*", selectedCategory.Name == Translation.Instance.All ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (FileInfo file in files)
                {
                    if (IsPossibleVideo(file.Name))
                    {
                        try
                        {
                            DeleteVideo(file.FullName);
                        }
                        catch { } // file might be locked (e.g. still downloading)
                    }
                }
            }
            else if (choice.DisplayText == Translation.Instance.Cancel)
            {
                ((IDownloader)(selectedItem.Other as DownloadInfo).Downloader).CancelAsync();
            }
            return new ContextMenuExecutionResult() { RefreshCurrentItems = true };
        }

        void DeleteVideo(string path)
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            //Get all files starting with given path
            string[] files = Directory.GetFiles(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".*", SearchOption.TopDirectoryOnly);

            //Take video files from the list only
            string[] filesVideo = files.Where(f => IsPossibleVideo(f)).Select(f => f.Substring(0, f.LastIndexOf('.'))).ToArray();

            //Remove additional files
            foreach (string strFile in files)
            {
                if (filesVideo.Any(f => strFile.StartsWith(f, StringComparison.CurrentCultureIgnoreCase)))
                    continue; //the file belongs to another video; skip this file

                File.Delete(strFile);
            }
        }

        public override bool IsPossibleVideo(string fsUrl)
        {
            if (string.IsNullOrEmpty(fsUrl)) return false; // empty string is not a video
            string extension = Path.GetExtension(fsUrl);
            if (string.IsNullOrEmpty(extension)) return false; // can't be a video file if empty extension
            extension = extension.ToLower();
            return OnlineVideoSettings.Instance.VideoExtensions.ContainsKey(extension);
        }

        #region Search

        public override bool CanSearch { get { return true; } }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            query = FixQuery(query);
            return getVideoList(OnlineVideoSettings.Instance.DownloadDir, query, true)
                .ConvertAll<SearchResultItem>(v => v as SearchResultItem);
        }

        #endregion

        #region IFilter Member

        public List<VideoInfo> FilterVideos(Category category, int maxResult, string orderBy, string timeFrame)
        {
            lastSort = orderBy;
            return GetVideos(category);
        }

        public List<VideoInfo> FilterSearchResults(string query, int maxResult, string orderBy, string timeFrame)
        {
            lastSort = orderBy;
            query = FixQuery(query);
            return getVideoList(OnlineVideoSettings.Instance.DownloadDir, query, true);
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
            options.Add(Translation.Instance.Date, "date");
            options.Add(Translation.Instance.Name, "name");
            options.Add(Translation.Instance.Size, "size");
            return options;
        }

        public Dictionary<string, string> GetTimeFrameOptions()
        {
            return new Dictionary<string, string>();
        }

        #endregion

        bool PassesAgeCheck(string fullFileName)
        {
            if (!OnlineVideoSettings.Instance.UseAgeConfirmation) return true;
            if (OnlineVideoSettings.Instance.UseAgeConfirmation && OnlineVideoSettings.Instance.AgeConfirmed) return true;

            try
            {
                // try to find out what site this video belongs to
                string siteName = Path.GetDirectoryName(fullFileName);
                siteName = siteName.Substring(siteName.LastIndexOf('\\') + 1);
                SiteUtilBase util = null;
                if (OnlineVideoSettings.Instance.SiteUtilsList.TryGetValue(siteName, out util))
                {
                    return !util.Settings.ConfirmAge;
                }
            }
            catch { }
            return false;
        }

        string FixQuery(string query)
        {
            query = query.Replace(' ', '*');
            if (!query.StartsWith("*")) query = "*" + query;
            if (!query.EndsWith("*")) query += "*";
            return query;
        }
    }
}
