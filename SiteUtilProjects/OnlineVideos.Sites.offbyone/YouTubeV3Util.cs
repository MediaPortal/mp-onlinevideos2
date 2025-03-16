using Google.Apis.Auth.OAuth2;
using Google.Apis.Json;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineVideos.Sites
{
    public class YouTubeV3Util : SiteUtilBase, IFilter, ILastCategoryVideos
    {
        #region Helper classes

        private class YouTubeUserdataStore : IDataStore
        {
            const string PREFIX = "YouTubeV3apiStore.";

            public Task StoreAsync<T>(string key, T value)
            {
                var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
                OnlineVideoSettings.Instance.UserStore.SetValue(PREFIX + key, serialized, true);
                return TaskEx.Delay(0);
            }

            public Task DeleteAsync<T>(string key)
            {
                OnlineVideoSettings.Instance.UserStore.SetValue(PREFIX + key, null);
                return TaskEx.Delay(0);
            }

            public Task<T> GetAsync<T>(string key)
            {
                TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
                var serialized = OnlineVideoSettings.Instance.UserStore.GetValue(PREFIX + key, true);
                if (!string.IsNullOrWhiteSpace(serialized))
                {
                    try
                    {
                        tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(serialized));
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }
                else
                {
                    tcs.SetResult(default(T));
                }
                return tcs.Task;
            }

            public Task ClearAsync()
            {
                return TaskEx.Delay(0);
            }
        }

        private class YouTubeVideo : VideoInfo
        {
            internal string ChannelId { get; set; }
            internal string ChannelTitle { get; set; }
            internal string PlaylistItemId { get; set; }
        }

        private class YouTubeCategory : RssLink
        {
            internal enum CategoryKind { Other, Channel, Playlist, GuideCategory, VideoCategory };
            internal CategoryKind Kind { get; set; }
            internal string Id { get; set; }
            internal bool IsMine { get; set; }
        }

        private class YoutubeChannel
        {
            public string Title;
            public string ChannelID;
            public string Description;
            public string ThumbUrl;
            public string Category;
        }

        private class YoutubeDetail
        {
            public Category Parent;
            public Func<YoutubeDetail, List<Category>> GetCategories;
            public Func<YoutubeDetail, List<VideoInfo>> GetVideos;
            public YoutubeChannel Channel;
            public string QueryType;
            public  SearchResource.ListRequest.EventTypeEnum? QueryEventType = null;
            public SearchResource.ListRequest.OrderEnum QueryOrder = SearchResource.ListRequest.OrderEnum.Relevance;
            public string QueryPageToken;
            public object Tag;
        }

        private class SiteData// : SiteDataBase
        {
            public List<YoutubeChannel> ChannelList = new List<YoutubeChannel>();
        };

        #endregion

        public enum VideoQuality { Low, Medium, High, HD, FullHD, Highest };

        public enum VideoFormat { flv, mp4, webm };

        const string CLIENT = @"eyJpbnN0YWxsZWQiOnsiYXV0aF91cmkiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20vby9vYXV0aDIvYXV0aCIsImNsaWVudF9zZWNyZXQiOiJ4cG52b05vNFB6N3lJUXdiVmdIQUdBcl8iLCJ0b2tlbl91cmkiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20vby9vYXV0aDIvdG9rZW4iLCJjbGllbnRfZW1haWwiOiIiLCJyZWRpcmVjdF91cmlzIjpbInVybjppZXRmOndnOm9hdXRoOjIuMDpvb2IiLCJvb2IiXSwiY2xpZW50X3g1MDlfY2VydF91cmwiOiIiLCJjbGllbnRfaWQiOiI5MjUzNzY1MjgyODAtMm9xdWkydnEwbHE2YjVtZjRzNTNodWNqNnRrb2JxazcuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdXRoX3Byb3ZpZGVyX3g1MDlfY2VydF91cmwiOiJodHRwczovL3d3dy5nb29nbGVhcGlzLmNvbS9vYXV0aDIvdjEvY2VydHMifX0=";
        const string APIKEY = "AIzaSyDzL_VrmG4Q2K4unBafZEoOv3UCAUTB7e4";

        [Category("OnlineVideosConfiguration"), Description("Add some dynamic categories found at startup to the list of configured ones.")]
        bool useDynamicCategories = true;

        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Videos per Page"), Description("Defines the default number of videos to display per page.")]
        int pageSize = 26;
        [Category("OnlineVideosUserConfiguration"), LocalizableDisplayName("Enable Login"), Description("Will popup a browser on first use to select your YouTube account.")]
        bool enableLogin = false;

        [Category("OnlineVideosUserConfiguration"), Browsable(false)]
        private string _Config = string.Empty;

        string hl = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        string regionCode = RegionInfo.CurrentRegion.TwoLetterISORegionName;
        YouTubeService service;
        Func<List<VideoInfo>> nextPageVideosQuery;
        SearchResource.ListRequest.OrderEnum currentSearchOrder = SearchResource.ListRequest.OrderEnum.Relevance;
        string currentVideosTitle;
        string userFavoritesPlaylistId;

        private SiteData _SiteData = null;
        private Category _ChannelsRootCategory = null;
        private SearchListResponse _SearchResult;

        private const string USER_UPLOADS_FEED = "http(?:s)?://gdata.youtube.com/feeds/api/users/(?<user>[^/]+)/uploads";

        public override int DiscoverDynamicCategories()
        {
            if (useDynamicCategories)
            {
                Settings.Categories = new BindingList<Category>();

                var guideCatgeory = new Category() { Name = "YouTube Guide", HasSubCategories = true };
                guideCatgeory.Other = (Func<List<Category>>)(() => QueryGuideCategories(guideCatgeory));
                Settings.Categories.Add(guideCatgeory);

                var videoCategory = new Category() { Name = "Video Categories", HasSubCategories = true };
                videoCategory.Other = (Func<List<Category>>)(() => QueryVideoCategories(videoCategory));
                Settings.Categories.Add(videoCategory);

                if (enableLogin)
                {
                    try
                    {
                        QueryUserChannel().ForEach(c => Settings.Categories.Add(c));
                    }
                    catch (Exception ex)
                    {
                        throw new OnlineVideosException(ex.Message);
                    }
                }
                else
                {
                    //Channels for users without YT account
                    this._ChannelsRootCategory = new Category() { Name = "Channels", HasSubCategories = true, AllowDiveDownOrUpIfSingle = false };
                    this._ChannelsRootCategory.Other = (Func<List<Category>>)(() => ChannelsGetRootItems(this._ChannelsRootCategory));
                    Settings.Categories.Add(this._ChannelsRootCategory);
                }

                Settings.DynamicCategoriesDiscovered = true;
            }

            foreach (Category link in Settings.Categories)
            {
                if (link is RssLink && !string.IsNullOrEmpty(((RssLink)link).Url))
                {
                    Match m = Regex.Match(((RssLink)link).Url, USER_UPLOADS_FEED);
                    if (m.Success)
                        link.Other = (Func<List<VideoInfo>>)(() => QueryUserUploads(m.Groups["user"].Value));
                }
            }
            return Settings.Categories.Count;
        }

        public override int DiscoverSubCategories(Category parentCategory)
        {
            var method = parentCategory.Other as Func<List<Category>>;
            if (method != null)
            {
                parentCategory.SubCategories = method.Invoke();
                parentCategory.SubCategoriesDiscovered = true;
                return parentCategory.SubCategories.Count;
            }
            else if (parentCategory.Other is YoutubeDetail detail)
            {
                parentCategory.SubCategories = detail.GetCategories.Invoke(detail);
                return parentCategory.SubCategories.Count;
            }

            return 0;
        }

        public override int DiscoverNextPageCategories(NextPageCategory category)
        {
            var method = category.Other as Func<List<Category>>;
            if (method != null)
            {
                var newCategories = method.Invoke();
                category.ParentCategory.SubCategories.Remove(category);
                category.ParentCategory.SubCategories.AddRange(newCategories);
                return newCategories.Count;
            }
            else if (category.Other is YoutubeDetail detail)
            {
                var newCategories = detail.GetCategories(detail);
                category.ParentCategory.SubCategories.Remove(category);
                category.ParentCategory.SubCategories.AddRange(newCategories);
                return newCategories.Count;
            }

            return 0;
        }

        public override string GetCurrentVideosTitle()
        {
            return currentVideosTitle;
        }

        public override string GetFileNameForDownload(VideoInfo video, Category category, string url)
        {
            if (string.IsNullOrEmpty(url)) // called for adding to favorites
                return video.Title;
            else // called for downloading
            {
                string saveName = Helpers.FileUtils.GetSaveFilename(video.Title);
                string extension = null;

                foreach (var kv in video.PlaybackOptions)
                {
                    if (kv.Value == url)
                    {
                        extension = Helpers.StringUtils.GetSubString(kv.Key, " | ", " ");
                        break;
                    }
                }

                if (!String.IsNullOrEmpty(extension))
                    extension = "." + extension;
                if (String.IsNullOrEmpty(extension) || !OnlineVideoSettings.Instance.VideoExtensions.ContainsKey(extension))
                    extension = ".mp4";// Randomly chosen fallback
                return saveName + extension;
            }
        }

        public override List<VideoInfo> GetVideos(Category category)
        {
            currentVideosTitle = null; // use default title for videos retrieved via this method (which is the Category Name)
            base.HasNextPage = false;
            nextPageVideosQuery = null;
            var method = category.Other as Func<List<VideoInfo>>;
            if (method != null)
            {
                return method.Invoke();
            }
            else if (category.Other is YoutubeDetail detail)
            {
                return detail.GetVideos(detail);
            }
            return new List<VideoInfo>();
        }

        public override List<String> GetMultipleVideoUrls(VideoInfo video, bool inPlaylist = false)
        {
            var hoster = Hoster.HosterFactory.GetHoster("Youtube");
            video.PlaybackOptions = hoster.GetPlaybackOptions(video.VideoUrl, out int iPreselection);
            if (video.PlaybackOptions?.Count > 0)
            {
                video.SubtitleTexts = ((Hoster.ISubtitle)hoster).SubtitleTexts;

                return new List<string>() { iPreselection >= 0 && iPreselection < video.PlaybackOptions.Count ? 
                    video.PlaybackOptions.ElementAt(iPreselection).Value : video.PlaybackOptions.First().Value };
                
            }
            return null; // no playback options
        }

        public override void Initialize(SiteSettings siteSettings)
        {
            base.Initialize(siteSettings);

            //Try to load channel list from custom settings
            try
            {
                this._SiteData = Newtonsoft.Json.JsonConvert.DeserializeObject<SiteData>(this._Config);
            }
            catch { }

            if (this._SiteData == null)
                this._SiteData = new SiteData();

            if (this._SiteData.ChannelList.Count == 0)
            {
                //Default Youtube channel
                this._SiteData.ChannelList.Add(new YoutubeChannel()
                {
                    Title = "YouTube",
                    ChannelID = "UCBR8-60-B28hp2BmDPdntcQ",
                    Description = "YouTube's Official Channel helps you discover what's new & trending globally. Watch must-see videos, from music to culture to Internet phenomena",
                    ThumbUrl = "https://yt3.ggpht.com/Bg5wS82KGryRmcsn1YbPThtbXoTmj2XJ9_7LmuE2RF6wbKJBkovfRypbSz6UD3gEu_nHiwGZtQ=s800-c-k-c0x00ffffff-no-rj"
                });
            }

            //if (this._SiteData.ChannelList.Count < 2)
            //    this.allowDiveDownOrUpIfSingle = false;
        }

        public override void DeInitialize()
        {
            this.SetConfigValueFromString(this.GetUserConfigurationProperties().First(p => p.DisplayName == "_Config"), Newtonsoft.Json.JsonConvert.SerializeObject(this._SiteData));
            base.DeInitialize();
        }

        public override bool CanHandleUrl(string strUrl)
        {
            return strUrl?.StartsWith("https://www.youtube.com/watch?v=", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        #region Search

        public override bool CanSearch { get { return true; } }

        Dictionary<string, string> cachedSearchCategories = null;
        public override Dictionary<string, string> GetSearchableCategories()
        {
            if (cachedSearchCategories == null)
                QueryVideoCategories(null);
            return cachedSearchCategories;
        }

        public override List<SearchResultItem> Search(string query, string category = null)
        {
            base.HasNextPage = false;
            nextPageVideosQuery = null;
            return QuerySearchVideos(query, "videos", null, category).ConvertAll(v => (SearchResultItem)v);
        }

        #endregion

        #region Paging

        public override List<VideoInfo> GetNextPageVideos()
        {
            var method = nextPageVideosQuery;
            base.HasNextPage = false;
            nextPageVideosQuery = null;
            if (method != null)
            {
                return method.Invoke();
            }
            return new List<VideoInfo>();
        }

        #endregion

        #region IFilter Members

        public List<VideoInfo> FilterVideos(Category category, int maxResults, string orderBy, string timeFrame)
        {
            return (category.Other as Func<List<VideoInfo>>).Invoke();
        }

        public List<VideoInfo> FilterSearchResults(string query, int maxResults, string orderBy, string timeFrame)
        {
            return FilterSearchResults(query, null, maxResults, orderBy, timeFrame);
        }

        public List<VideoInfo> FilterSearchResults(string query, string category, int maxResults, string orderBy, string timeFrame)
        {
            Enum.TryParse<SearchResource.ListRequest.OrderEnum>(orderBy, out currentSearchOrder);
            return QuerySearchVideos(query, "videos", null, category);
        }

        public List<int> GetResultSteps() { return new List<int>() { 10, 20, 30, 40, 50 }; }

        public Dictionary<string, string> GetOrderByOptions() { return Enum.GetNames(typeof(SearchResource.ListRequest.OrderEnum)).ToDictionary(o => o); }

        public Dictionary<string, string> GetTimeFrameOptions() { return new Dictionary<string, string>(); }

        #endregion

        #region Context Menu

        public override List<ContextMenuEntry> GetContextMenuEntries(Category selectedCategory, VideoInfo selectedItem)
        {
            List<ContextMenuEntry> result = new List<ContextMenuEntry>();
            var ytVideo = selectedItem as YouTubeVideo;
            var ytCategory = selectedCategory as YouTubeCategory;
            if (selectedItem == null && ytCategory != null)
            {
                if (ytCategory.Kind == YouTubeCategory.CategoryKind.Playlist && ytCategory.IsMine && !string.IsNullOrEmpty(ytCategory.Id))
                {
                    result.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.DeletePlaylist, Action = ContextMenuEntry.UIAction.Execute });
                }
            }
            if (ytVideo != null)
            {
                if (!string.IsNullOrEmpty(ytVideo.ChannelTitle) && !string.IsNullOrEmpty(ytVideo.ChannelId))
                {
                    result.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.UploadsBy + " [" + ytVideo.ChannelTitle + "]", Action = ContextMenuEntry.UIAction.Execute });
                    result.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.Playlists + " [" + ytVideo.ChannelTitle + "]", Action = ContextMenuEntry.UIAction.Execute });
                }
                if (!string.IsNullOrEmpty(userFavoritesPlaylistId))
                {
                    if (ytCategory != null && ytCategory.Kind == YouTubeCategory.CategoryKind.Playlist && ytCategory.Id == userFavoritesPlaylistId && !string.IsNullOrEmpty(ytVideo.PlaylistItemId))
                    {
                        result.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.RemoveFromFavorites + " (" + Settings.Name + ")", Action = ContextMenuEntry.UIAction.Execute });
                    }
                    else
                    {
                        result.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.AddToFavourites + " (" + Settings.Name + ")", Action = ContextMenuEntry.UIAction.Execute });
                    }
                }
                if (ytCategory != null && ytCategory.Kind == YouTubeCategory.CategoryKind.Playlist && ytCategory.IsMine && !string.IsNullOrEmpty(ytCategory.Id) && !string.IsNullOrEmpty(ytVideo.PlaylistItemId))
                {
                    result.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.RemoveFromPlaylist, Action = ContextMenuEntry.UIAction.Execute });
                }
                if (enableLogin)
                {
                    var plCtx = new ContextMenuEntry() { DisplayText = Translation.Instance.AddToPlaylist, Action = ContextMenuEntry.UIAction.ShowList };
                    plCtx.SubEntries.Add(new ContextMenuEntry() { DisplayText = Translation.Instance.CreateNewPlaylist, Action = ContextMenuEntry.UIAction.GetText });
                    foreach (var pl in QueryChannelPlaylists(new YouTubeCategory() { IsMine = true }, null))
                    {
                        if (pl is YouTubeCategory)
                            plCtx.SubEntries.Add(new ContextMenuEntry() { DisplayText = pl.Name, Other = (pl as YouTubeCategory).Id });
                    }
                    result.Add(plCtx);
                }
            }

            if (!enableLogin)
            {
                YoutubeChannel ch = selectedCategory.Other is YoutubeDetail detail ? detail.Channel : null;
                if (selectedCategory.ParentCategory == this._ChannelsRootCategory)
                {
                    //Add new channel
                    result.Add(new ContextMenuEntry()
                    {
                        DisplayText = Translation.Instance.AddNewChannel,
                        Action = ContextMenuEntry.UIAction.GetText,
                    });

                    //Remove selected channel
                    if (ch != null)
                    {
                        if (ch != this._SiteData.ChannelList[0])
                        {
                            result.Add(new ContextMenuEntry()
                            {
                                DisplayText = Translation.Instance.RemoveChannel,
                                Action = ContextMenuEntry.UIAction.Execute,
                            });
                        }

                        //Add to category
                        if (string.IsNullOrWhiteSpace(ch.Category))
                        {
                            result.Add(new ContextMenuEntry()
                            {
                                DisplayText = Translation.Instance.MoveToCategory,
                                Action = ContextMenuEntry.UIAction.GetText,
                            });

                            //Existing category
                            IEnumerable<IGrouping<string, YoutubeChannel>> cats = this._SiteData.ChannelList.Where(c => c.Category != null).GroupBy(c => c.Category);
                            if (cats.Count() > 0)
                            {
                                ContextMenuEntry menu = new ContextMenuEntry()
                                {
                                    DisplayText = Translation.Instance.MoveToExistingCategory,
                                    Action = ContextMenuEntry.UIAction.ShowList,
                                };

                                foreach (IGrouping<string, YoutubeChannel> cat in cats)
                                    menu.SubEntries.Add(new ContextMenuEntry() { DisplayText = cat.Key, Action = ContextMenuEntry.UIAction.Execute });

                                result.Add(menu);
                            }
                        }
                    }
                }
                else if (ch != null)
                {
                    if (!string.IsNullOrWhiteSpace(ch.Category))
                    {
                        //Remove from category
                        result.Add(new ContextMenuEntry()
                        {
                            DisplayText = Translation.Instance.RemoveFromCategory,
                            Action = ContextMenuEntry.UIAction.Execute,
                        });
                    }
                }

                if (ch != null)
                {
                    //Refresh metadata
                    result.Add(new ContextMenuEntry()
                    {
                        DisplayText = "Refresh metadata",
                        Action = ContextMenuEntry.UIAction.Execute,
                    });
                }
            }

            return result;
        }

        public override ContextMenuExecutionResult ExecuteContextMenuEntry(Category selectedCategory, VideoInfo selectedItem, ContextMenuEntry choice)
        {
            ContextMenuExecutionResult result = new ContextMenuExecutionResult();
            try
            {
                if (choice.DisplayText == Translation.Instance.AddToFavourites + " (" + Settings.Name + ")")
                {
                    var query = Service.PlaylistItems.Insert(
                        new Google.Apis.YouTube.v3.Data.PlaylistItem()
                        {
                            Snippet = new Google.Apis.YouTube.v3.Data.PlaylistItemSnippet()
                            {
                                Title = selectedItem.Title,
                                PlaylistId = userFavoritesPlaylistId,
                                ResourceId = new Google.Apis.YouTube.v3.Data.ResourceId()
                                {
                                    VideoId = selectedItem.VideoUrl,
                                    Kind = "youtube#video"
                                }
                            }
                        },
                        "snippet");
                    var response = query.Execute();
                    result.ExecutionResultMessage = string.Format("{0} {1}", Translation.Instance.Success, Translation.Instance.AddingToFavorites);
                }
                else if (choice.DisplayText == Translation.Instance.RemoveFromFavorites + " (" + Settings.Name + ")")
                {
                    var query = Service.PlaylistItems.Delete((selectedItem as YouTubeVideo).PlaylistItemId);
                    var response = query.Execute();
                    result.RefreshCurrentItems = true;
                }
                else if (choice.DisplayText.StartsWith(Translation.Instance.UploadsBy))
                {
                    base.HasNextPage = false;
                    nextPageVideosQuery = null;
                    currentVideosTitle = Translation.Instance.UploadsBy + " [" + (selectedItem as YouTubeVideo).ChannelTitle + "]";
                    result.ResultItems = QuerySearchVideos(null, "videos", (selectedItem as YouTubeVideo).ChannelId, null).ConvertAll<SearchResultItem>(v => v as SearchResultItem);
                }
                else if (choice.DisplayText.StartsWith(Translation.Instance.Playlists))
                {
                    var parentCategory = new YouTubeCategory() { Name = Translation.Instance.Playlists + " [" + (selectedItem as YouTubeVideo).ChannelTitle + "]" };
                    parentCategory.SubCategories = QueryChannelPlaylists(parentCategory, (selectedItem as YouTubeVideo).ChannelId);
                    result.ResultItems = parentCategory.SubCategories.ConvertAll<SearchResultItem>(v => v as SearchResultItem);
                }
                else if (choice.DisplayText == Translation.Instance.RemoveFromPlaylist)
                {
                    var query = Service.PlaylistItems.Delete((selectedItem as YouTubeVideo).PlaylistItemId);
                    var response = query.Execute();
                    result.RefreshCurrentItems = true;
                    if ((selectedCategory as YouTubeCategory).EstimatedVideoCount != null) (selectedCategory as YouTubeCategory).EstimatedVideoCount--;
                }
                else if (choice.DisplayText == Translation.Instance.DeletePlaylist)
                {
                    var query = Service.Playlists.Delete((selectedCategory as YouTubeCategory).Id);
                    var response = query.Execute();
                    selectedCategory.ParentCategory.SubCategoriesDiscovered = false;
                    result.RefreshCurrentItems = true;
                }
                else if (choice.ParentEntry != null && choice.ParentEntry.DisplayText == Translation.Instance.AddToPlaylist)
                {
                    if (choice.Other == null)
                    {
                        // create new playlist first
                        var query = Service.Playlists.Insert(
                            new Google.Apis.YouTube.v3.Data.Playlist()
                            {
                                Snippet = new Google.Apis.YouTube.v3.Data.PlaylistSnippet() { Title = choice.UserInputText }
                            },
                            "snippet");
                        var response = query.Execute();
                        choice.Other = response.Id;
                    }
                    var queryItem = Service.PlaylistItems.Insert(
                        new Google.Apis.YouTube.v3.Data.PlaylistItem()
                        {
                            Snippet = new Google.Apis.YouTube.v3.Data.PlaylistItemSnippet()
                            {
                                Title = selectedItem.Title,
                                PlaylistId = choice.Other as string,
                                ResourceId = new Google.Apis.YouTube.v3.Data.ResourceId()
                                {
                                    VideoId = selectedItem.VideoUrl,
                                    Kind = "youtube#video"
                                }
                            }
                        },
                        "snippet");
                    var responseItem = queryItem.Execute();
                    // force re-discovery of dynamic subcategories for my playlists category (as either a new catgeory was added or the count changed)
                    var playlistsCategory = Settings.Categories.FirstOrDefault(c => (c is YouTubeCategory) && (c as YouTubeCategory).IsMine && c.Name.EndsWith(Translation.Instance.Playlists));
                    if (playlistsCategory != null) playlistsCategory.SubCategoriesDiscovered = false;
                }
                else if (!enableLogin)
                {
                    if (choice.Other is string strChannelId)
                    {
                       if (this._SiteData.ChannelList.Exists(c => c.ChannelID == strChannelId))
                            return new ContextMenuExecutionResult() { ExecutionResultMessage = "Channel already exists." };

                        SearchResultSnippet snippet = this._SearchResult.Items.First(p => p.Snippet.ChannelId == strChannelId).Snippet;
                        YoutubeChannel ch = new YoutubeChannel()
                        {
                            ChannelID = snippet.ChannelId,
                            Title = snippet.Title,
                            Description = snippet.Description,
                            ThumbUrl = this.getThumbnailUrl(snippet.Thumbnails),
                        };

                        this._SiteData.ChannelList.Add(ch);
                        this._ChannelsRootCategory.SubCategories.Add(new Category()
                        {
                            Other = new YoutubeDetail() { Parent = this._ChannelsRootCategory, Channel = ch, GetCategories = this.ChannelsGetChannelRootItems },
                            Name = ch.Title,
                            Description = ch.Description,
                            Thumb = ch.ThumbUrl,
                            HasSubCategories = true,
                            ParentCategory = selectedCategory.ParentCategory
                        });

                        return new ContextMenuExecutionResult()
                        { ExecutionResultMessage = "Channel \'" + ch.Title + "\' created.", RefreshCurrentItems = true };
                    }

                    if (choice.ParentEntry != null && choice.ParentEntry.DisplayText == Translation.Instance.MoveToExistingCategory)
                    {
                        ((YoutubeDetail)selectedCategory.Other).Channel.Category = choice.DisplayText;
                        this.Settings.DynamicCategoriesDiscovered = false;
                        return new ContextMenuExecutionResult() { RefreshCurrentItems = true };
                    }

                    if (choice.DisplayText == Translation.Instance.AddNewChannel)
                    {
                        if (string.IsNullOrWhiteSpace(choice.UserInputText))
                            return new ContextMenuExecutionResult() { ExecutionResultMessage = "Invalid name." };

                        SearchResource.ListRequest rq = Service.Search.List("snippet");
                        rq.Q = choice.UserInputText.Trim();
                        rq.MaxResults = 20;
                        rq.Type = "channel";

                        this._SearchResult = rq.Execute();

                        if (this._SearchResult.Items.Count == 0)
                            return new ContextMenuExecutionResult() { ExecutionResultMessage = "Channel not found." };

                        ContextMenuEntry entry = new ContextMenuEntry
                        {
                            DisplayText = "Select Channel to add",
                            Action = ContextMenuEntry.UIAction.ShowList,
                        };
                        foreach (SearchResult item in this._SearchResult.Items)
                        {
                            entry.SubEntries.Add(new ContextMenuEntry()
                            {
                                DisplayText = item.Snippet.Title,
                                Action = ContextMenuEntry.UIAction.Execute,
                                Other = item.Snippet.ChannelId
                            });
                        }
                        return new ContextMenuExecutionResult() { SubMenu = entry };
                    }
                    else if (choice.DisplayText == Translation.Instance.RemoveChannel)
                    {
                        this.Settings.Categories.Remove(selectedCategory);
                        this._SiteData.ChannelList.Remove(((YoutubeDetail)selectedCategory.Other).Channel);
                        this.Settings.DynamicCategoriesDiscovered = false;
                        if (selectedCategory.ParentCategory != null)
                            selectedCategory.ParentCategory.SubCategoriesDiscovered = false;
                        return new ContextMenuExecutionResult()
                        { ExecutionResultMessage = "Channel \'" + selectedCategory.Name + "\' removed.", RefreshCurrentItems = true };
                    }
                    else if (choice.DisplayText == Translation.Instance.MoveToCategory)
                    {
                        string strName = choice.UserInputText.Trim();

                        if (string.IsNullOrWhiteSpace(strName))
                            return new ContextMenuExecutionResult() { ExecutionResultMessage = "Invalid name." };

                        ((YoutubeDetail)selectedCategory.Other).Channel.Category = strName;
                        selectedCategory.ParentCategory.SubCategoriesDiscovered = false;
                        return new ContextMenuExecutionResult() { RefreshCurrentItems = true };
                    }
                    else if (choice.DisplayText == Translation.Instance.RemoveFromCategory)
                    {
                        ((YoutubeDetail)selectedCategory.Other).Channel.Category = null;
                        selectedCategory.ParentCategory.SubCategoriesDiscovered = false;
                        return new ContextMenuExecutionResult() { RefreshCurrentItems = true };
                    }
                    else if (choice.DisplayText == "Refresh metadata")
                    {
                        YoutubeChannel ch = ((YoutubeDetail)selectedCategory.Other).Channel;

                        ChannelsResource.ListRequest rq = Service.Channels.List("snippet");
                        rq.Id = ch.ChannelID;
                        ChannelListResponse resp = rq.Execute();
                        if (resp.Items.Count > 0)
                        {
                            ch.ThumbUrl = this.getThumbnailUrl(resp.Items[0].Snippet.Thumbnails);
                            ch.Description = resp.Items[0].Snippet.Description;
                            selectedCategory.Thumb = ch.ThumbUrl;
                            return new ContextMenuExecutionResult() { RefreshCurrentItems = true };
                        }
                    }
                }
            }
            catch (Google.GoogleApiException apiEx)
            {
                throw new OnlineVideosException(string.Format("{0} {1}", apiEx.HttpStatusCode, apiEx.Message));
            }
            catch (Exception ex)
            {
                throw new OnlineVideosException(ex.Message);
            }
            return result;
        }

        /// <summary>Returns videos uploaded by user</summary>
        /// <param name="username">Name of the user</param>
        List<VideoInfo> QueryUserUploads(string username)
        {
            var query = Service.Channels.List("snippet, contentDetails");
            query.ForUsername = username;
            query.Hl = hl;
            var response = query.Execute();
            if (response.Items.Count == 0)
                return new List<VideoInfo>();
            var playlistId = response.Items[0].ContentDetails.RelatedPlaylists.Uploads;
            return QueryPlaylistVideos(playlistId);
        }
        #endregion

        #region YouTube service wrapper methods

        /// <summary>
        /// Gets a (cached) instance of the <see cref="YouTubeService"/> used to query the API.
        /// When authorization is enabled, upon first creation a user token will be retrieved using a browser popup.
        /// </summary>
        YouTubeService Service
        {
            get
            {
                if (service == null)
                {
                    UserCredential credential = null;
                    if (enableLogin)
                    {
                        using (var stream = new System.IO.MemoryStream(Convert.FromBase64String(CLIENT)))
                        {
                            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                GoogleClientSecrets.Load(stream).Secrets,
                                new[] { YouTubeService.Scope.Youtube },
                                "user",
                                CancellationToken.None,
                                new YouTubeUserdataStore()
                            ).Result;
                        }
                    }
                    service = new YouTubeService(new BaseClientService.Initializer()
                    {
                        ApiKey = APIKEY,
                        ApplicationName = "OnlineVideos",
                        HttpClientInitializer = credential,
                    });
                }
                return service;
            }
        }

        /// <summary>Returns a list of categories for the authenticated user (Watch Later, Watch History, Subscriptions, Playlists)</summary>
        List<Category> QueryUserChannel()
        {
            var query = Service.Channels.List("snippet, contentDetails");
            query.Mine = true;
            var response = query.Execute();
            var userChannel = response.Items.FirstOrDefault();
            var results = new List<Category>();
            if (userChannel != null)
            {
                var userName = userChannel.Snippet.Title;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    userFavoritesPlaylistId = userChannel.ContentDetails.RelatedPlaylists.Favorites;
                    results.Add(new Category() { Name = string.Format("{0}'s {1}", userName, "Watch Later"), Thumb = userChannel.Snippet.Thumbnails.High.Url, Other = (Func<List<VideoInfo>>)(() => QueryPlaylistVideos(userChannel.ContentDetails.RelatedPlaylists.WatchLater)) });
                    results.Add(new Category() { Name = string.Format("{0}'s {1}", userName, "Watch History"), Thumb = userChannel.Snippet.Thumbnails.High.Url, Other = (Func<List<VideoInfo>>)(() => QueryPlaylistVideos(userChannel.ContentDetails.RelatedPlaylists.WatchHistory)) });

                    var subscriptionsCategory = new Category() { Name = string.Format("{0}'s {1}", userName, Translation.Instance.Subscriptions), Thumb = userChannel.Snippet.Thumbnails.High.Url, HasSubCategories = true };
                    subscriptionsCategory.Other = (Func<List<Category>>)(() => QueryMySubscriptions(subscriptionsCategory));
                    results.Add(subscriptionsCategory);

                    var playlistsCategory = new YouTubeCategory() { Name = string.Format("{0}'s {1}", userName, Translation.Instance.Playlists), Thumb = userChannel.Snippet.Thumbnails.High.Url, HasSubCategories = true, IsMine = true };
                    playlistsCategory.Other = (Func<List<Category>>)(() => QueryChannelPlaylists(playlistsCategory, null));
                    results.Add(playlistsCategory);
                }
            }
            return results;
        }

        /// <summary>Returns a list of categories that can be associated with YouTube channels.</summary>
        /// <remarks>
        /// A guide category identifies a category that YouTube algorithmically assigns based on a channel's content or other indicators, such as the channel's popularity. 
        /// The list is similar to video categories, with the difference being that a video's uploader can assign a video category but only YouTube can assign a channel category.
        /// </remarks>
        List<Category> QueryGuideCategories(Category parentCategory)
        {
            var query = Service.GuideCategories.List("snippet");
            query.RegionCode = regionCode;
            query.Hl = hl;
            var response = query.Execute();
            var results = new List<Category>();
            foreach (var item in response.Items)
            {
                var category = new YouTubeCategory() { Name = item.Snippet.Title, HasSubCategories = true, ParentCategory = parentCategory, Kind = YouTubeCategory.CategoryKind.GuideCategory, Id = item.Id };
                category.Other = (Func<List<Category>>)(() => QueryChannelsForGuideCategory(category, item.Id));
                results.Add(category);
            }
            return results;
        }

        /// <summary>Returns a list of categories that can be associated with YouTube videos.</summary>
        List<Category> QueryVideoCategories(Category parentCategory)
        {
            var query = Service.VideoCategories.List("snippet");
            query.RegionCode = regionCode;
            query.Hl = hl;
            var response = query.Execute();
            var results = new List<Category>();
            cachedSearchCategories = new Dictionary<string, string>();
            foreach (var item in response.Items)
            {
                if (item.Snippet.Assignable == true)
                {
                    var category = new YouTubeCategory() { Name = item.Snippet.Title, ParentCategory = parentCategory, Kind = YouTubeCategory.CategoryKind.VideoCategory, Id = item.Id };
                    category.Other = (Func<List<VideoInfo>>)(() => QueryCategoryVideos(item.Id));
                    results.Add(category);
                    cachedSearchCategories.Add(item.Snippet.Title, item.Id);
                }
            }
            return results;
        }

        /// <summary>Returns a list of channels for the given guide category.</summary>
        /// <param name="guideCategoryId">The guide category to use as filter in the query.</param>
        List<Category> QueryChannelsForGuideCategory(Category parentCategory, string guideCategoryId, string pageToken = null)
        {
            var query = Service.Channels.List("snippet, statistics");
            query.CategoryId = guideCategoryId;
            query.Hl = hl;
            query.MaxResults = pageSize;
            query.PageToken = pageToken;
            var response = query.Execute();
            var results = new List<Category>();
            foreach (var item in response.Items)
            {
                var category = new YouTubeCategory()
                {
                    Name = item.Snippet.Localized.Title,
                    Description = item.Snippet.Localized.Description,
                    Thumb = item.Snippet.Thumbnails != null ? item.Snippet.Thumbnails.High.Url : null,
                    EstimatedVideoCount = (uint)(item.Statistics.VideoCount ?? 0),
                    HasSubCategories = true,
                    ParentCategory = parentCategory,
                    Kind = YouTubeCategory.CategoryKind.Channel,
                    Id = item.Id
                };
                category.Other = (Func<List<Category>>)(() => QueryChannelPlaylists(category, item.Id));
                results.Add(category);
            }
            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                results.Add(new NextPageCategory() { ParentCategory = parentCategory, Other = (Func<List<Category>>)(() => QueryChannelsForGuideCategory(parentCategory, guideCategoryId, response.NextPageToken)) });
            }
            return results;
        }

        /// <summary>Returns a list of playlists for the given channel.</summary>
        /// <param name="channelId">The channel to use as filter in the query.</param>
        List<Category> QueryChannelPlaylists(YouTubeCategory parentCategory, string channelId, string pageToken = null)
        {
            var query = Service.Playlists.List("snippet, contentDetails");
            if (string.IsNullOrEmpty(channelId))
                query.Mine = true;
            else
                query.ChannelId = channelId;
            query.Hl = hl;
            query.MaxResults = pageSize;
            query.PageToken = pageToken;
            var response = query.Execute();
            var results = new List<Category>();
            if (!string.IsNullOrEmpty(channelId) && pageToken == null && parentCategory.EstimatedVideoCount > 0)
            {
                // before all playlists add a category that will list all uploads of the channel
                results.Add(new YouTubeCategory()
                {
                    Name = string.Format("{0} {1}", Translation.Instance.UploadsBy, parentCategory.Name),
                    Thumb = parentCategory.Thumb,
                    EstimatedVideoCount = parentCategory.EstimatedVideoCount,
                    ParentCategory = parentCategory,
                    Kind = YouTubeCategory.CategoryKind.Channel,
                    Id = channelId,
                    Other = (Func<List<VideoInfo>>)(() => QuerySearchVideos(null, "videos", parentCategory.Id, null, true, null)),
                    IsWatchable = true,
                    TagLink = "type=video&id=" + parentCategory.Id
                });
            }
            foreach (var item in response.Items)
            {
                if ((long)(item.ContentDetails.ItemCount ?? 0) > 0 || parentCategory.IsMine) // hide empty playlists when not listing the authenticated user's
                    results.Add(new YouTubeCategory()
                    {
                        Name = item.Snippet.Localized.Title,
                        Description = item.Snippet.Localized.Description,
                        Thumb = getThumbnailUrl(item.Snippet.Thumbnails),
                        EstimatedVideoCount = (uint)(item.ContentDetails.ItemCount ?? 0),
                        ParentCategory = parentCategory,
                        Kind = YouTubeCategory.CategoryKind.Playlist,
                        Id = item.Id,
                        IsMine = parentCategory.IsMine,
                        Other = (Func<List<VideoInfo>>)(() => QueryPlaylistVideos(item.Id)),
                        IsWatchable = true,
                        TagLink = "type=playlist&id=" + item.Id
                    });
            }
            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                results.Add(new NextPageCategory() { ParentCategory = parentCategory, Other = (Func<List<Category>>)(() => QueryChannelPlaylists(parentCategory, channelId, response.NextPageToken)) });
            }
            return results;
        }

        private string getThumbnailUrl(Google.Apis.YouTube.v3.Data.ThumbnailDetails thumbnails)
        {
            if (thumbnails == null) return null;
            if (thumbnails.High != null) return thumbnails.High.Url;
            if (thumbnails.Medium != null) return thumbnails.Medium.Url;
            if (thumbnails.Standard != null) return thumbnails.Standard.Url;
            return null;
        }

        /// <summary>Returns a list of the authenticated user's subscriptions (channels).</summary>
        List<Category> QueryMySubscriptions(Category parentCategory, string pageToken = null)
        {
            var query = Service.Subscriptions.List("snippet, contentDetails");
            query.Mine = true;
            query.MaxResults = pageSize;
            query.PageToken = pageToken;
            var response = query.Execute();
            var results = new List<Category>();

            // before all channels add a category that will list all uploads
            results.Add(new YouTubeCategory()
            {
                Name = "Latest Videos",
                Thumb = parentCategory.Thumb,
                ParentCategory = parentCategory,
                Kind = YouTubeCategory.CategoryKind.Other,
                Other = (Func<List<VideoInfo>>)(() => QueryNewestSubscriptionVideos()),
                IsWatchable = true,
                TagLink = "type=subscr&id=none"
            });

            foreach (var item in response.Items)
            {
                var category = new YouTubeCategory()
                {
                    Name = item.Snippet.Title,
                    Description = item.Snippet.Description,
                    Thumb = item.Snippet.Thumbnails != null ? item.Snippet.Thumbnails.High.Url : null,
                    EstimatedVideoCount = (uint)(item.ContentDetails.TotalItemCount ?? 0),
                    ParentCategory = parentCategory,
                    HasSubCategories = true,
                    Kind = YouTubeCategory.CategoryKind.Channel,
                    Id = item.Snippet.ResourceId.ChannelId,
                    IsMine = true
                };
                category.Other = (Func<List<Category>>)(() => QueryChannelPlaylists(category, item.Snippet.ResourceId.ChannelId));
                results.Add(category);
            }
            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                results.Add(new NextPageCategory() { ParentCategory = parentCategory, Other = (Func<List<Category>>)(() => QueryMySubscriptions(parentCategory, response.NextPageToken)) });
            }
            return results;
        }

        List<VideoInfo> QueryNewestSubscriptionVideos(string pageToken = null)
        {
            var query = Service.Subscriptions.List("snippet, contentDetails");
            query.Mine = true;
            query.MaxResults = pageSize;
            query.Order = SubscriptionsResource.ListRequest.OrderEnum.Unread;
            query.PageToken = pageToken;
            var response = query.Execute();
            var results = new List<VideoInfo>();

            foreach (var channel in response.Items)
            {
                results.AddRange(GetNewestSubscriptionVideos(channel.Snippet.ResourceId.ChannelId, channel.ContentDetails.NewItemCount));
            }

            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                base.HasNextPage = true;
                nextPageVideosQuery = () => QueryNewestSubscriptionVideos(response.NextPageToken);
            }
            return results.OrderByDescending(x => x.Airdate).ToList();
        }

        public List<VideoInfo> GetNewestSubscriptionVideos(string channelId, long? maxResults, string nextPageToken = null)
        {
            var query = Service.Search.List("snippet");
            query.ChannelId = channelId;
            query.MaxResults = maxResults;
            query.Order = SearchResource.ListRequest.OrderEnum.Date;
            query.Type = "video";
            query.PageToken = nextPageToken;
            var response = query.Execute();

            // Collect video IDs from response for duration lookup
            StringBuilder sbVideoIDs = new StringBuilder(512);
            for (int i = 0; i < response.Items.Count; i++)
            {
                if (sbVideoIDs.Length > 0)
                    sbVideoIDs.Append(',');

                sbVideoIDs.Append(response.Items[i].Id.VideoId);
            }

            // Retrieve Video durations
            Dictionary<string, string> videoDurations = QueryVideoInfoDuration(sbVideoIDs.ToString());

            var results = response.Items.Select(i => new YouTubeVideo()
            {
                Title = i.Snippet.Title,
                Description = i.Snippet.Description,
                Thumb = i.Snippet.Thumbnails != null ? i.Snippet.Thumbnails.High.Url : null,
                Airdate = i.Snippet.PublishedAt != null ? i.Snippet.PublishedAt.Value.ToString("g", OnlineVideoSettings.Instance.Locale) : i.Snippet.PublishedAtRaw,
                VideoUrl = i.Id.VideoId,
                ChannelId = i.Snippet.ChannelId,
                ChannelTitle = i.Snippet.ChannelTitle,
                Length = videoDurations.FirstOrDefault(x => x.Key == i.Id.VideoId).Value
            }).ToList<VideoInfo>();

            if (maxResults >= 50)
            {
                GetNewestSubscriptionVideos(channelId, maxResults, response.NextPageToken);
            }
            return results.OrderByDescending(x => x.Airdate).ToList();
        }

        /// <summary>Returns a list of most popular videos for the given category.</summary>
        /// <param name="videoCategoryId">The category to use use as filter in the query.</param>
        List<VideoInfo> QueryCategoryVideos(string videoCategoryId, string pageToken = null)
        {
            var query = Service.Videos.List("snippet, contentDetails");
            query.Chart = VideosResource.ListRequest.ChartEnum.MostPopular;
            query.VideoCategoryId = videoCategoryId;
            query.RegionCode = regionCode;
            query.Hl = hl;
            query.MaxResults = pageSize;
            query.PageToken = pageToken;
            var response = query.Execute();
            var results = response.Items.Select(i => new YouTubeVideo()
            {
                Title = i.Snippet.Localized.Title,
                Description = i.Snippet.Localized.Description,
                Thumb = i.Snippet.Thumbnails != null ? i.Snippet.Thumbnails.High.Url : null,
                Airdate = i.Snippet.PublishedAt != null ? i.Snippet.PublishedAt.Value.ToString("g", OnlineVideoSettings.Instance.Locale) : i.Snippet.PublishedAtRaw,
                Length = System.Xml.XmlConvert.ToTimeSpan(i.ContentDetails.Duration).ToString(),
                VideoUrl = i.Id,
                ChannelId = i.Snippet.ChannelId,
                ChannelTitle = i.Snippet.ChannelTitle
            }).ToList<VideoInfo>();
            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                base.HasNextPage = true;
                nextPageVideosQuery = () => QueryCategoryVideos(videoCategoryId, response.NextPageToken);
            }
            return results;
        }

        /// <summary>Returns a list of videos for the given playlist.</summary>
        /// <param name="playlistId">The playlist to use as a filter in the query.</param>
        List<VideoInfo> QueryPlaylistVideos(string playlistId, string pageToken = null)
        {
            var query = Service.PlaylistItems.List("snippet");
            query.PlaylistId = playlistId;
            query.MaxResults = pageSize;
            query.PageToken = pageToken;
            var response = query.Execute();
            var results = response.Items.Where(i => i.Snippet.ResourceId.Kind == "youtube#video").Select(i => new YouTubeVideo()
            {
                Title = i.Snippet.Title,
                Description = i.Snippet.Description,
                Thumb = i.Snippet.Thumbnails != null ? this.getThumbnailUrl(i.Snippet.Thumbnails) : null,
                Airdate = i.Snippet.PublishedAt != null ? i.Snippet.PublishedAt.Value.ToString("g", OnlineVideoSettings.Instance.Locale) : i.Snippet.PublishedAtRaw,
                VideoUrl = i.Snippet.ResourceId.VideoId,
                ChannelId = i.Snippet.ChannelId,
                ChannelTitle = i.Snippet.ChannelTitle,
                PlaylistItemId = i.Id,
            }).ToList<VideoInfo>();
            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                base.HasNextPage = true;
                nextPageVideosQuery = () => QueryPlaylistVideos(playlistId, response.NextPageToken);
            }
            return results;
        }

        /// <summary>Returns a list of videos for the given search string.</summary>
        /// <param name="queryString">The search string to use as as filter in the query.</param>
        /// <param name="channelId">The channel id to use as filter in the query.</param>
        List<VideoInfo> QuerySearchVideos(string queryString, string searchType, string channelId, string categoryId, bool sortbyDate = false, string pageToken = null,
            SearchResource.ListRequest.EventTypeEnum? eventType = null, bool bQueryDurations = true)
        {
            var query = Service.Search.List("snippet");
            if (!string.IsNullOrEmpty(channelId))
                query.ChannelId = channelId;
            if (!string.IsNullOrEmpty(queryString))
                query.Q = queryString;
            if (!string.IsNullOrEmpty(categoryId))
                query.VideoCategoryId = categoryId;

            if (sortbyDate)
            {
                query.Order = SearchResource.ListRequest.OrderEnum.Date;
            }
            else
            {
                query.Order = currentSearchOrder;
            }

            query.Type = searchType;
            query.MaxResults = pageSize;
            query.PageToken = pageToken;
            query.EventType = eventType;

            var response = query.Execute();

            Dictionary<string, string> videoDurations = null;
            if (bQueryDurations)
            {
                // Collect video IDs from response for duration lookup
                StringBuilder sbVideoIDs = new StringBuilder(512);
                for (int i = 0; i < response.Items.Count; i++)
                {
                    if (sbVideoIDs.Length > 0)
                        sbVideoIDs.Append(',');

                    sbVideoIDs.Append(response.Items[i].Id.VideoId);
                }

                // Retrieve Video durations
                videoDurations = QueryVideoInfoDuration(sbVideoIDs.ToString());
            }

            var results = response.Items.Where(i => !String.IsNullOrEmpty(i.Id.VideoId)).Select(i => new YouTubeVideo()
            {
                Title = i.Snippet.Title,
                Description = i.Snippet.Description,
                Thumb = i.Snippet.Thumbnails != null ? i.Snippet.Thumbnails.High.Url : null,
                Airdate = i.Snippet.PublishedAt != null ? i.Snippet.PublishedAt.Value.ToString("g", OnlineVideoSettings.Instance.Locale) : i.Snippet.PublishedAtRaw,
                VideoUrl = i.Id.VideoId,
                ChannelId = i.Snippet.ChannelId,
                ChannelTitle = i.Snippet.ChannelTitle,
                Length = videoDurations?.FirstOrDefault(x => x.Key == i.Id.VideoId).Value,
            }).ToList<VideoInfo>();
            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                base.HasNextPage = true;
                nextPageVideosQuery = () => QuerySearchVideos(queryString, searchType, channelId, categoryId, sortbyDate, response.NextPageToken);
            }
            return results;
        }

        /// <summary>Returns video durations for given video ids.</summary>
        /// <param name="videoIDs">VideoIDSs can be a single videoID or multiple separated by ','.</param>
        Dictionary<string, string> QueryVideoInfoDuration(string videoIDs)
        {
            Dictionary<string, string> videoDurations = new Dictionary<string, string>();

            var query = Service.Videos.List("snippet, contentDetails");
            query.Id = videoIDs;
            var response = query.Execute();

            foreach (var item in response.Items)
            {
                string duration;
                if (!string.IsNullOrWhiteSpace(item.ContentDetails.Duration))
                {
                    if (string.Compare(item.ContentDetails.Duration, "P0D", true) == 0)
                    {
                        if (string.Compare(item.Snippet.LiveBroadcastContent, "upcoming", true) == 0)
                            duration = "Upcoming";
                        else
                            duration = "Live";
                    }
                    else
                        duration = System.Xml.XmlConvert.ToTimeSpan(item.ContentDetails.Duration).ToString();

                    //Trim if no hours found (00:)
                    if (duration.StartsWith("00:"))
                        duration = duration.Substring(3);
                }
                else
                    duration = string.Empty;

                videoDurations.Add(item.Id, duration);
                //Log.Debug(string.Format("ID: {0}   Duration: {1}", item.Id, duration));
            }

            return videoDurations;
        }
        #endregion

        #region Non Account Channels
        private List<Category> ChannelsGetRootItems(Category parentCategory)
        {
            List<Category> result = new List<Category>();
            Category category;

            //Build list of categories
            foreach (YoutubeChannel channel in this._SiteData.ChannelList.Where(ch => !string.IsNullOrWhiteSpace(ch.Category)))
            {
                if (result.FirstOrDefault(cat => cat.Name == channel.Category) == null)
                {
                    result.Add(category = new Category()
                    {
                        Name = channel.Category,
                        HasSubCategories = true,
                        SubCategories = new List<Category>(),
                        SubCategoriesDiscovered = false,
                        ParentCategory = parentCategory
                    });
                    category.Other = new YoutubeDetail() { Parent = category, GetCategories = this.ChannelsGetCategoryChannels };
                }
            }

            //Add each non category YT channel to the list
            this._SiteData.ChannelList.ForEach(ch =>
            {
                if (string.IsNullOrWhiteSpace(ch.Category))
                {
                    result.Add(category = new Category()
                    {
                        Name = ch.Title,
                        Description = ch.Description,
                        Thumb = ch.ThumbUrl,
                        HasSubCategories = true,
                        ParentCategory = parentCategory
                    });
                    category.Other = new YoutubeDetail() { Parent = category, Channel = ch, GetCategories = this.ChannelsGetChannelRootItems };
                }
            });

            return result;
        }

        private List<Category> ChannelsGetCategoryChannels(YoutubeDetail detail)
        {
            List<Category> result = new List<Category>();
            this._SiteData.ChannelList.ForEach(ch =>
            {
                if (ch.Category == detail.Parent.Name)
                {
                    Category cat;
                    result.Add(cat = new Category()
                    {
                        Name = ch.Title,
                        Description = ch.Description,
                        Thumb = ch.ThumbUrl,
                        HasSubCategories = true,
                        ParentCategory = detail.Parent
                    });
                    cat.Other = new YoutubeDetail() { Parent = cat, Channel = ch, GetCategories = this.ChannelsGetChannelRootItems };
                }
            });

            detail.Parent.SubCategoriesDiscovered = true;

            return result;
        }

        private List<Category> ChannelsGetChannelRootItems(YoutubeDetail detail)
        {
            Category category;
            List<Category> result = new List<Category>();

            //Videos
            result.Add(category = new Category()
            {
                Name = "Videos",
                HasSubCategories = false,
                ParentCategory = detail.Parent,
                IsWatchable = true,
                TagLink = "type=video&id=" + detail.Channel.ChannelID
            });
            category.Other = new YoutubeDetail() { Parent = category, Channel = detail.Channel, GetVideos = this.ChannelsGetVideos, QueryType = "video" };

            //Live
            result.Add(category = new Category()
            {
                Name = "Live",
                HasSubCategories = false,
                ParentCategory = detail.Parent
            });
            category.Other = new YoutubeDetail() { Parent = category, Channel = detail.Channel, GetVideos = this.ChannelsGetVideos, QueryType = "video", QueryEventType = SearchResource.ListRequest.EventTypeEnum.Live };

            //Playlists
            result.Add(category = new Category()
            {
                Name = "Playlists",
                HasSubCategories = true,
                ParentCategory = detail.Parent
            });
            category.Other = new YoutubeDetail() { Parent = category, Channel = detail.Channel, GetCategories = this.ChannelsGetPlaylists, QueryType = "playlist" };

            detail.Parent.SubCategoriesDiscovered = true;

            return result;
        }

        private List<VideoInfo> ChannelsGetVideos(YoutubeDetail detail)
        {
            return this.QuerySearchVideos(null, detail.QueryType, detail.Channel.ChannelID, null, true, null, detail.QueryEventType);
        }

        private List<Category> ChannelsGetPlaylists(YoutubeDetail detail)
        {
            PlaylistsResource.ListRequest rq = Service.Playlists.List("snippet"); //snippet,id
            rq.ChannelId = detail.Channel.ChannelID;
            rq.PageToken = detail.QueryPageToken;
            rq.MaxResults = this.pageSize;

            PlaylistListResponse response = rq.Execute();

            List<Category> result = new List<Category>();

            for (int i = 0; i < response.Items.Count; i++)
            {
                Playlist item = response.Items[i];
                result.Add(new Category()
                {
                    Name = item.Snippet.Localized.Title,
                    Description = item.Snippet.Localized.Description,
                    Thumb = getThumbnailUrl(item.Snippet.Thumbnails),
                    ParentCategory = detail.Parent,
                    Other = (Func<List<VideoInfo>>)(() => QueryPlaylistVideos(item.Id)),
                    IsWatchable = true,
                    TagLink = "type=playlist&id=" + item.Id
                });
            }

            if (!string.IsNullOrEmpty(response.NextPageToken))
            {
                detail.QueryPageToken = response.NextPageToken;
                result.Add(new NextPageCategory()
                {
                    ParentCategory = detail.Parent,
                    Other = new YoutubeDetail()
                    {
                        Channel = detail.Channel,
                        GetCategories = this.ChannelsGetPlaylists,
                        Parent = detail.Parent
                    }
                });
            }

            detail.Parent.SubCategoriesDiscovered = true;

            return result;
        }

        #endregion

        #region ILastCategoryVideos
        public List<VideoInfo> GetLatestVideos(DateTime dtLastCheck, string strLastVideoUrl, Category category, ref string strTag)
        {
            if (string.IsNullOrWhiteSpace(category.TagLink))
                throw new NotImplementedException();

            if (!DateTime.TryParse(strTag, out DateTime dtLastPublish))
                dtLastPublish = DateTime.MinValue;

            List<VideoInfo> result;
            System.Collections.Specialized.NameValueCollection args = System.Web.HttpUtility.ParseQueryString(category.TagLink);
            string strId = args["id"];
            switch (args["type"])
            {
                case "video":
                    result = this.QuerySearchVideos(null, "video", strId, null, true, null, null, false); //costs too much (100)
                    break;

                case "playlist":
                    result = this.QueryPlaylistVideos(strId);
                    break;

                case "subscr":
                    result = this.QueryNewestSubscriptionVideos();
                    break;

                default:
                    result = null;
                    break;
            }

            if (result?.Count > 0)
            {
                result.ForEach(vi => vi.VideoUrl = "https://www.youtube.com/watch?v=" + vi.VideoUrl);
                if (DateTime.TryParse(result[0].Airdate, out DateTime dt))
                    strTag = dt.ToString();
            }

            return result;
        }
        #endregion
    }
}
