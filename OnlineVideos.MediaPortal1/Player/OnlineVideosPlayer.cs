using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DirectShowLib;
using DShowNET.Helper;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using System.Runtime.InteropServices;
using MediaPortal.Player.Subtitles;
using MediaPortal.Player.PostProcessing;
using MediaPortal.Player.LAV;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Globalization;

namespace OnlineVideos.MediaPortal1.Player
{
    public class OnlineVideosPlayer : VideoPlayerVMR9, OVSPLayer
    {
        #region Refeshrate Adaption

        [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        static extern double EVRGetVideoFPS(int fpsSource);

        [DllImport("dshowhelper.dll", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
        static extern void EVRUpdateDisplayFPS();

        private enum RefreshRateState { None, Reported, Done };

        string cacheFile = null;
        private RefreshRateState refreshRateAdapted = RefreshRateState.None;
        private double reportedFPS_Matched;

        private static string _VideoDecoder = null;
        private static string _AudioDecoder = null;
        private static string _AudioRenderer = null;

        private MixedUrl _MixedUrl = null;
        private int _CurrentAudioStream = 0;
        private int _InternalAudioStreams = 0;

        void AdaptRefreshRateFromCacheFile()
        {
            if (!string.IsNullOrEmpty(cacheFile))
            {
                try
                {
                    MediaInfo.MediaInfo mi = new MediaInfo.MediaInfo();
                    mi.Open(cacheFile);
                    double framerate;
                    double.TryParse(mi.Get(MediaInfo.StreamKind.Video, 0, "FrameRate"), System.Globalization.NumberStyles.AllowDecimalPoint, new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." }, out framerate);
                    if (framerate > 1)
                    {
                        this._VideoSampleDuration = 1d / framerate;
                        Log.Instance.Info("OnlineVideosPlayer got {0} FPS from MediaInfo", framerate);
                        double matchedFps = RefreshRateHelper.MatchConfiguredFPS(framerate);
                        if (matchedFps != default(double))
                        {
                            refreshRateAdapted = RefreshRateState.Done;
                            RefreshRateHelper.ChangeRefreshRateToMatchedFps(matchedFps, cacheFile);
                            try
                            {
                                if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
                                    EVRUpdateDisplayFPS();
                            }
                            catch (EntryPointNotFoundException)
                            {
                                Log.Instance.Warn("OnlineVideosPlayer: Your version of dshowhelper.dll does not support FPS updating.");
                            }
                            catch (Exception ex)
                            {
                                Log.Instance.Warn("OnlineVideosPlayer: Exception trying update refresh rate fo EVR: {0}", ex.ToString());
                            }
                        }
                        else
                        {
                            Log.Instance.Info("No matching configured FPS found - skipping RefreshRate Adaption from Cache File");
                        }
                    }
                    else
                    {
                        Log.Instance.Info("OnlineVideosPlayer got no FPS from MediaInfo");
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Warn("OnlineVideosPlayer: Exception trying refresh rate change from cache file: {0}", ex.ToString());
                }
            }
            else
            {
                Log.Instance.Info("OnlineVideosPlayer: No cache file, skipping FPS detection via MediaInfo");

                try
                {
                    if (!string.IsNullOrEmpty(_VideoDecoder))
                    {
                        IBaseFilter filterFound = DirectShowUtil.GetFilterByName(graphBuilder, _VideoDecoder);
                        if (filterFound != null)
                        {
                            Log.Instance.Debug("[adaptRefreshRateFromCacheFile] Video Decoder found: '{0}'", _VideoDecoder);
                            int iResult = filterFound.FindPin("Out", out IPin pin);
                            if (iResult == 0)
                            {
                                Log.Instance.Debug("[adaptRefreshRateFromCacheFile] Output Pin of Video Decoder retrieved.");
                                AMMediaType mediatype = new AMMediaType();
                                iResult = pin.ConnectionMediaType(mediatype);

                                if (iResult == 0)
                                {
                                    string strGUID = mediatype.formatType.ToString();
                                    Log.Instance.Debug("[adaptRefreshRateFromCacheFile] AMMediaType: {0}", strGUID);
                                    double dFps = -1;
                                    bool bInterlaced = false;

                                    if (strGUID.Equals("F72A76A0-EB0A-11D0-ACE4-0000C0CC16BA", StringComparison.OrdinalIgnoreCase) //VIDEOINFOHEADER2
                                        || strGUID.Equals("E06D80E3-DB46-11CF-B4D1-00805F6CBBEA", StringComparison.OrdinalIgnoreCase)) //WMFORMAT_MPEG2Video
                                    {
                                        VideoInfoHeader2 videoHeader = new VideoInfoHeader2();
                                        Marshal.PtrToStructure(mediatype.formatPtr, videoHeader);
                                        if (videoHeader != null)
                                        {
                                            bInterlaced = (videoHeader.InterlaceFlags & AMInterlace.IsInterlaced) == AMInterlace.IsInterlaced;
                                            dFps = Math.Round(10000000F / videoHeader.AvgTimePerFrame, 2);
                                            Log.Instance.Debug("[adaptRefreshRateFromCacheFile] AvgTimePerFrame from VideoInfoHeader2: {0}, Interlaced: {1}",
                                                videoHeader.AvgTimePerFrame, bInterlaced);
                                        }
                                    }
                                    else
                                    {
                                        VideoInfoHeader videoHeader = new VideoInfoHeader();
                                        Marshal.PtrToStructure(mediatype.formatPtr, videoHeader);
                                        if (videoHeader != null)
                                        {
                                            dFps = Math.Round(10000000F / videoHeader.AvgTimePerFrame, 2);
                                            Log.Instance.Debug("[adaptRefreshRateFromCacheFile] AvgTimePerFrame from VideoInfoHeader: {0}", videoHeader.AvgTimePerFrame);
                                        }
                                    }

                                    if (dFps > 0)
                                    {
                                        Log.Instance.Info("[adaptRefreshRateFromCacheFile] Detected FPS from Video Decoder: {0}", dFps);

                                        this._VideoSampleDuration = 1d / dFps;

                                        if (bInterlaced)
                                        {
                                            switch (dFps)
                                            {
                                                case 25:
                                                    dFps = 50.0;
                                                    break;

                                                case 29.97:
                                                    dFps = 59.97;
                                                    break;

                                                case 30:
                                                    dFps = 60.0;
                                                    break;
                                            }
                                        }

                                        double dMatchedFps = RefreshRateHelper.MatchConfiguredFPS(dFps);
                                        if (dMatchedFps != default)
                                        {
                                            refreshRateAdapted = RefreshRateState.Done;
                                            RefreshRateHelper.ChangeRefreshRateToMatchedFps(dMatchedFps, cacheFile);

                                            try
                                            {
                                                if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
                                                    EVRUpdateDisplayFPS();

                                            }
                                            catch (EntryPointNotFoundException)
                                            {
                                                Log.Instance.Warn("[adaptRefreshRateFromCacheFile] Your version of dshowhelper.dll does not support FPS updating.");
                                            }
                                            catch (Exception ex)
                                            {
                                                Log.Instance.Warn("[adaptRefreshRateFromCacheFile] Exception trying update refresh rate fo EVR: " + ex.ToString());
                                            }
                                        }
                                        else
                                            Log.Instance.Info("[adaptRefreshRateFromCacheFile] No matching configured FPS found - skipping RefreshRate Adaption from Video Decoder");
                                    }

                                }
                                else
                                    Log.Instance.Debug("[adaptRefreshRateFromCacheFile] Unable to get AMMediaType.");

                                DirectShowUtil.ReleaseComObject(pin);
                            }

                            DirectShowUtil.ReleaseComObject(filterFound);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Instance.Warn("[adaptRefreshRateFromCacheFile] Exception trying refresh rate change from Video Decoder: " + ex.ToString());
                }

            }
        }

        private double SetRefreshRate(double fps, double currentFps)
        {
            double matchedFps = RefreshRateHelper.MatchConfiguredFPS(fps);
            if (matchedFps != default(double))
            {
                if (matchedFps != currentFps)
                {
                    RefreshRateHelper.ChangeRefreshRateToMatchedFps(matchedFps, m_strCurrentFile);
                    EVRUpdateDisplayFPS();
                }
            }
            else
            {
                Log.Instance.Info("No matching configured FPS found - skipping RefreshRate Adaption");
            }
            return matchedFps;
        }

        void AdaptRefreshRateFromVideoRenderer()
        {
            if (GUIGraphicsContext.VideoRenderer == GUIGraphicsContext.VideoRendererType.EVR)
            {
                if (refreshRateAdapted != RefreshRateState.Done && m_state == PlayState.Playing)
                {
                    try
                    {
                        if (refreshRateAdapted == RefreshRateState.None)
                        {
                            double fps = EVRGetVideoFPS(0);
                            if (fps > 1)
                            {
                                this._VideoSampleDuration = 1d / fps;
                                refreshRateAdapted = RefreshRateState.Reported;
                                Log.Instance.Info("OnlineVideosPlayer got {0} reported FPS from dshowhelper.dll after {1} sec", fps, CurrentPosition);
                                reportedFPS_Matched = SetRefreshRate(fps, default(double));
                            }
                        }
                        else
                        {
                            double fps = EVRGetVideoFPS(1);
                            if (fps > 1)
                            {
                                this._VideoSampleDuration = 1d / fps;
                                refreshRateAdapted = RefreshRateState.Done;
                                Log.Instance.Info("OnlineVideosPlayer got {0} detected FPS from dshowhelper.dll after {1} sec", fps, CurrentPosition);
                                SetRefreshRate(fps, reportedFPS_Matched);
                            }
                        }
                    }
                    catch (EntryPointNotFoundException)
                    {
                        Log.Instance.Warn("OnlineVideosPlayer: Your version of dshowhelper.dll does not support FPS reporting.");
                        refreshRateAdapted = RefreshRateState.Done;
                    }
                    catch (Exception ex)
                    {
                        Log.Instance.Warn("OnlineVideosPlayer: Exception trying refresh rate change while playing : {0}", ex.ToString());
                        refreshRateAdapted = RefreshRateState.Done;
                    }
                }
            }
        }

        #endregion

        public OnlineVideosPlayer()
            : base(g_Player.MediaType.Video)
        { }

        public OnlineVideosPlayer(g_Player.MediaType type)
            : base(type)
        { }

        public OnlineVideosPlayer(string url, bool useLAV)
            : base(g_Player.MediaType.Video)
        {
            m_strCurrentFile = url;
            this.UseLAV = useLAV;
        }

        public override string CurrentFile // hack to get around the MP 1.3 Alpha bug with non http URLs
        {
            get { return "http://localhost/OnlineVideo.mp4"; }
        }

        protected override bool GetInterfaces()
        {
            if (graphBuilder != null) // graph was already started and playback file buffered
                return FinishPreparedGraph();
            else
                return base.GetInterfaces();
        }

        private int CalculateBufferedEnough(long total, long current)
        {
            if (Duration == 0 || current == 0)
                return 0;
            TimeSpan buffertime = DateTime.Now - bufferingStart;
            var estimatedBufferTime = ((double)total / current) * buffertime.TotalSeconds;
            if (estimatedBufferTime == 0)
                return 0;
            double remainingPlayTime = Duration - CurrentPosition;//seconds
            var enough = 100 - 100 * (remainingPlayTime / estimatedBufferTime);

            return (int)(enough < 0 ? 0 : enough);
        }

        public override void Process()
        {
            if ((DateTime.Now - lastProgressCheck).TotalMilliseconds > 100) // check progress at maximum 10 times per second
            {
                lastProgressCheck = DateTime.Now;

                if (PluginConfiguration.Instance.AllowRefreshRateChange)
                    AdaptRefreshRateFromVideoRenderer();

                //LAV buffer monitoring
                if (this.UseLAV)
                    this.processLavBufferLevel();
                else
                {
                    //LAV is not used
                    if (percentageBuffered >= 100.0f) // already buffered 100%, simply set the Property
                    {
                        GUIPropertyManager.SetProperty("#TV.Record.percent3", percentageBuffered.ToString());
                        GUIPropertyManager.SetProperty("#OnlineVideos.bufferedenough", "0");
                    }
                    else
                    {
                        if (graphBuilder != null && GetSourceFilterName(m_strCurrentFile) == OnlineVideos.MPUrlSourceFilter.Downloader.FilterName) // only when progress reporting is possible
                        {
                            IBaseFilter sourceFilter = null;
                            try
                            {
                                int result = graphBuilder.FindFilterByName(OnlineVideos.MPUrlSourceFilter.Downloader.FilterName, out sourceFilter);
                                if (result == 0)
                                {
                                    long total = 0, current = 0;
                                    ((IAMOpenProgress)sourceFilter).QueryProgress(out total, out current);
                                    percentageBuffered = (float)current / (float)total * 100.0f;
                                    GUIPropertyManager.SetProperty("#TV.Record.percent3", percentageBuffered.ToString());
                                    GUIPropertyManager.SetProperty("#OnlineVideos.bufferedenough", CalculateBufferedEnough(total, current).ToString());

                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Instance.Warn("Error Quering Progress: {0}", ex.Message);
                            }
                            finally
                            {
                                if (sourceFilter != null) DirectShowUtil.ReleaseComObject(sourceFilter, 2000);
                            }
                        }
                    }
                }
            }

            base.Process();
        }

        float percentageBuffered;
        DateTime lastProgressCheck = DateTime.MinValue;
        DateTime bufferingStart = DateTime.MinValue;

        public bool BufferingStopped { get; protected set; }
        public void StopBuffering() { BufferingStopped = true; }

        protected bool skipBuffering = false;
        public void SkipBuffering() { skipBuffering = true; }

        public bool UseLAV { get; }
        public string GetSourceFilterName(string videoUrl)
        {
            return GetSourceFilterName(videoUrl, UseLAV);
        }

        public static string GetSourceFilterName(string videoUrl, bool forceUseLAV)
        {
            string sourceFilterName = string.Empty;
            Uri uri = new Uri(videoUrl);
            switch (uri.Scheme)
            {
                case MixedUrl.MIXED_URL_SCHEME:
                case "http":
                case "https":
                case "rtmp":
                case "rtmps":
                    if (!forceUseLAV)
                        sourceFilterName = OnlineVideos.MPUrlSourceFilter.Downloader.FilterName;
                    else
                        sourceFilterName = "LAV Splitter Source";
                    break;
                case "sop":
                    sourceFilterName = "SopCast ASF Splitter";
                    break;
                case "mms":
                    sourceFilterName = "WM ASF Reader";
                    break;
            }
            return sourceFilterName;
        }

        /// <summary>
        /// If the url to be played can be buffered before starting playback, this function
        /// starts building a graph by adding the preferred video and audio render to it.
        /// This needs to be called on the MpMain Thread.
        /// </summary>
        /// <returns>true, if the url can be buffered (a graph was started), false if it can't be and null if an error occured building the graph</returns>
        public bool? PrepareGraph()
        {
            string sourceFilterName = GetSourceFilterName(m_strCurrentFile);

            this._MixedUrl = null;
            MixedUrl mixedUrl = new MixedUrl(m_strCurrentFile);
            if (mixedUrl.Valid && mixedUrl.AudioTracks?.Length > 0)
                this._MixedUrl = mixedUrl;

            if (!string.IsNullOrEmpty(sourceFilterName))
            {
                graphBuilder = (IGraphBuilder)new FilterGraph();
                _rotEntry = new DsROTEntry((IFilterGraph)graphBuilder);

                basicVideo = graphBuilder as IBasicVideo2;

                Vmr9 = new VMR9Util();
                Vmr9.AddVMR9(graphBuilder);
                Vmr9.Enable(false);
                // set VMR9 back to NOT Active -> otherwise GUI is not refreshed while graph is building
                GUIGraphicsContext.Vmr9Active = false;

                // add the audio renderer
                using (Settings settings = new MPSettings())
                {
                    _AudioRenderer = settings.GetValueAsString("movieplayer", "audiorenderer", "Default DirectSound Device");
                    DirectShowUtil.ReleaseComObject(DirectShowUtil.AddAudioRendererToGraph(graphBuilder, _AudioRenderer, false));
                }

                // set fields for playback
                mediaCtrl = (IMediaControl)graphBuilder;
                mediaEvt = (IMediaEventEx)graphBuilder;
                mediaSeek = (IMediaSeeking)graphBuilder;
                mediaPos = (IMediaPosition)graphBuilder;
                basicAudio = (IBasicAudio)graphBuilder;
                videoWin = (IVideoWindow)graphBuilder;

                // add the source filter
                IBaseFilter sourceFilter = null;
                IBaseFilter sourceFilterAudio = null;
                try
                {
                    if (sourceFilterName == OnlineVideos.MPUrlSourceFilter.Downloader.FilterName)
                    {
                        sourceFilter = FilterFromFile.LoadFilterFromDll("MPUrlSourceSplitter\\MPUrlSourceSplitter.ax", new Guid(OnlineVideos.MPUrlSourceFilter.Downloader.FilterCLSID), true);
                        if (sourceFilter != null)
                        {
                            Marshal.ThrowExceptionForHR(graphBuilder.AddFilter(sourceFilter, OnlineVideos.MPUrlSourceFilter.Downloader.FilterName));

                            if (this._MixedUrl != null)
                            {
                                for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
                                {
                                    sourceFilterAudio = FilterFromFile.LoadFilterFromDll("MPUrlSourceSplitter\\MPUrlSourceSplitter.ax", new Guid(MPUrlSourceFilter.Downloader.FilterCLSID), true);
                                    if (sourceFilterAudio != null)
                                    {
                                        Marshal.ThrowExceptionForHR(graphBuilder.AddFilter(sourceFilterAudio, MPUrlSourceFilter.Downloader.FilterName));
                                        DirectShowUtil.ReleaseComObject(sourceFilterAudio, 2000);
                                        sourceFilterAudio = null;

                                        Log.Instance.Debug("AudioTrack: ID={0}, Default={5}, Language={1}, Description={2}, Filter={3}, Url={4}",
                                            i, this._MixedUrl.AudioTracks[i].Language, this._MixedUrl.AudioTracks[i].Description,
                                            MPUrlSourceFilter.Downloader.FilterName, this._MixedUrl.AudioTracks[i].Url, this._MixedUrl.AudioTracks[i].IsDefault);
                                    }
                                }
                            }
                        }
                    }

                    if (sourceFilter == null)
                    {
                        sourceFilter = DirectShowUtil.AddFilterToGraph(graphBuilder, sourceFilterName);

                        if (this._MixedUrl != null)
                        {
                            for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
                            {
                                sourceFilterAudio = DirectShowUtil.AddFilterToGraph(graphBuilder, sourceFilterName);
                                DirectShowUtil.ReleaseComObject(sourceFilterAudio, 2000);
                                sourceFilterAudio = null;

                                Log.Instance.Debug("AudioTrack: ID={0}, Default={5}, Language={1}, Description={2}, Filter={3}, Url={4}",
                                        i, this._MixedUrl.AudioTracks[i].Language, this._MixedUrl.AudioTracks[i].Description,
                                        sourceFilterName, this._MixedUrl.AudioTracks[i].Url, this._MixedUrl.AudioTracks[i].IsDefault);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Warn("Error adding '{0}' filter to graph: {1}", sourceFilterName, ex.Message);
                    return null;
                }
                finally
                {
                    if (sourceFilter != null)
                        DirectShowUtil.ReleaseComObject(sourceFilter, 2000);

                    if (sourceFilterAudio != null)
                        DirectShowUtil.ReleaseComObject(sourceFilterAudio, 2000);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function can be called by a background thread. It finishes building the graph and
        /// waits until the buffer is filled to the configured percentage.
        /// If a filter in the graph requires the full file to be downloaded, the function will return only afterwards.
        /// </summary>
        /// <returns>true, when playback can be started</returns>
        public bool BufferFile(Sites.SiteUtilBase siteUtil)
        {
            bufferingStart = DateTime.Now;
            Thread renderPinsThread = null;
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent; // prevents the BlackRectangle on first time playback
            bool PlaybackReady = false;
            IBaseFilter sourceFilter = null;
            IBaseFilter sourceFilterAudio = null;
            string sourceFilterName = null;

            string strUrlVideo = null;
            if (this._MixedUrl != null)
            {
                strUrlVideo = this._MixedUrl.VideoUrl;

                Log.Instance.Info("BufferFile : using onlinevideos scheme: urlVideo:'{0}' audioTacks:'{1}'", strUrlVideo, this._MixedUrl.AudioTracks.Length);
            }

            try
            {
                sourceFilterName = GetSourceFilterName(m_strCurrentFile);

                int result = graphBuilder.FindFilterByName(sourceFilterName, out sourceFilter);
                if (result != 0)
                {
                    string errorText = DirectShowLib.DsError.GetErrorText(result);
                    if (errorText != null) errorText = errorText.Trim();
                    Log.Instance.Warn("BufferFile : FindFilterByName returned '{0}'{1}", "0x" + result.ToString("X8"), !string.IsNullOrEmpty(errorText) ? " : (" + errorText + ")" : "");
                    return false;
                }

                //Explicit audio streams
                if (this._MixedUrl != null)
                {
                    for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
                    {
                        result = graphBuilder.FindFilterByName(sourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilterAudio);
                        if (result != 0)
                        {
                            string errorText = DsError.GetErrorText(result);
                            if (errorText != null)
                                errorText = errorText.Trim();
                            Log.Instance.Warn("BufferFile : FindFilterByName returned '{0}'{1}", "0x" + result.ToString("X8"), !string.IsNullOrEmpty(errorText) ? " : (" + errorText + ")" : "");
                            return false;
                        }

                        try
                        {
                            Marshal.ThrowExceptionForHR(((IFileSourceFilter)sourceFilterAudio).Load(this._MixedUrl.AudioTracks[i].Url, null));
                        }
                        finally
                        {
                            DirectShowUtil.ReleaseComObject(sourceFilterAudio);
                            sourceFilterAudio = null;
                        }
                    }
                }


                MPUrlSourceFilter.IFilterStateEx filterStateEx = sourceFilter as MPUrlSourceFilter.IFilterStateEx;

                if (filterStateEx != null)
                {
                    // MediaPortal IPTV filter and url source splitter
                    Log.Instance.Info("BufferFile : using 'MediaPortal IPTV filter and url source splitter' as source filter");

                    String url = strUrlVideo ?? OnlineVideos.MPUrlSourceFilter.UrlBuilder.GetFilterUrl(siteUtil, m_strCurrentFile, true);

                    Log.Instance.Info("BufferFile : loading url: '{0}'", url);
                    result = filterStateEx.LoadAsync(url);

                    if (result < 0)
                        throw new OnlineVideosException(FilterError.ErrorDescription(filterStateEx, result));

                    while (!this.BufferingStopped)
                    {
                        Boolean opened = false;

                        result = filterStateEx.IsStreamOpened(out opened);

                        if (result < 0)
                        {
                            throw new OnlineVideosException(FilterError.ErrorDescription(filterStateEx, result));
                        }

                        if (opened)
                        {
                            break;
                        }

                        Thread.Sleep(1);
                    }

                    // buffer before starting playback
                    bool filterConnected = false;
                    bool filterIsReadyToConnect = false;
                    percentageBuffered = 0.0f;
                    long total = 0, current = 0, last = 0;

                    while (!PlaybackReady && graphBuilder != null && !BufferingStopped)
                    {
                        result = ((IAMOpenProgress)sourceFilter).QueryProgress(out total, out current);
                        if ((result != 0) && (result != 0x00040260))
                        {
                            // 0x00040260 - VFW_S_ESTIMATED - correct state, but value is estimated
                            throw new OnlineVideosException(FilterError.ErrorDescription(filterStateEx, result));
                        }

                        result = filterStateEx.IsFilterReadyToConnectPins(out filterIsReadyToConnect);
                        if (result != 0)
                        {
                            throw new OnlineVideosException(FilterError.ErrorDescription(filterStateEx, result));
                        }

                        percentageBuffered = (float)current / (float)total * 100.0f;
                        // after configured percentage has been buffered, connect the graph

                        if (!filterConnected && (percentageBuffered >= PluginConfiguration.Instance.playbuffer || skipBuffering))
                        {
                            if (filterIsReadyToConnect)
                            {
                                result = filterStateEx.GetCacheFileName(out cacheFile);
                                if (result != 0)
                                {
                                    throw new OnlineVideosException(FilterError.ErrorDescription(filterStateEx, result));
                                }

                                if (skipBuffering) Log.Instance.Debug("Buffering skipped at {0}%", percentageBuffered);
                                filterConnected = true;
                                renderPinsThread = new Thread(delegate ()
                                {
                                    try
                                    {
                                        //Log.Instance.Debug("BufferFile : Rendering unconnected output pins of source filter ...");
                                        //// add audio and video filter from MP Movie Codec setting section
                                        //AddPreferredFilters(graphBuilder, sourceFilter);
                                        //// connect the pin automatically -> will buffer the full file in cases of bad metadata in the file or request of the audio or video filter
                                        //DirectShowUtil.RenderUnconnectedOutputPins(graphBuilder, sourceFilter);

                                        //if (sourceFilterAudio != null)
                                        //{
                                        //    AddPreferredFilters(graphBuilder, sourceFilterAudio);
                                        //    DirectShowUtil.RenderUnconnectedOutputPins(graphBuilder, sourceFilterAudio);
                                        //}

                                        //Log.Instance.Debug("BufferFile : Playback Ready.");
                                        PlaybackReady = true;
                                    }
                                    catch (ThreadAbortException)
                                    {
                                        Thread.ResetAbort();
                                        Log.Instance.Info("RenderUnconnectedOutputPins foribly aborted.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Instance.Warn(ex.Message);
                                        StopBuffering();
                                    }
                                })
                                { IsBackground = true, Name = "OVGraph" };
                                renderPinsThread.Start();
                            }
                        }

                        // log every percent
                        if (current > last && current - last >= (double)total * 0.01)
                        {
                            Log.Instance.Debug("Buffering: {0}/{1} KB ({2}%)", current / 1024, total / 1024, (int)percentageBuffered);
                            last = current;
                        }
                        // set the percentage to a gui property, formatted according to percentage, so the user knows very early if anything is buffering                   
                        string formatString = "###";
                        if (percentageBuffered == 0f) formatString = "0.0";
                        else if (percentageBuffered < 1f) formatString = ".00";
                        else if (percentageBuffered < 10f) formatString = "0.0";
                        else if (percentageBuffered < 100f) formatString = "##";
                        GUIPropertyManager.SetProperty("#OnlineVideos.buffered", percentageBuffered.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture));
                        Thread.Sleep(50); // no need to do this more often than 20 times per second
                    }
                }
                else
                {
                    if (m_strCurrentFile.IndexOf(MPUrlSourceFilter.SimpleUrl.ParameterSeparator) >= 0)
                    {
                        m_strCurrentFile = MPUrlSourceFilter.UrlBuilder.GetFilterUrl(siteUtil, m_strCurrentFile, false);
                    }
                    else if (strUrlVideo != null)
                        m_strCurrentFile = strUrlVideo;

                    Marshal.ThrowExceptionForHR(((IFileSourceFilter)sourceFilter).Load(m_strCurrentFile, null));

                    
                    Log.Instance.Info("BufferFile : using unknown filter as source filter");

                    if (!UseLAV && sourceFilter is IAMOpenProgress && !m_strCurrentFile.Contains("live=true") && !m_strCurrentFile.Contains("RtmpLive=1"))
                    {
                        // buffer before starting playback
                        bool filterConnected = false;
                        percentageBuffered = 0.0f;
                        long total = 0, current = 0, last = 0;      
                        do
                        {
                            result = ((IAMOpenProgress)sourceFilter).QueryProgress(out total, out current);
                            Marshal.ThrowExceptionForHR(result);

                            percentageBuffered = (float)current / (float)total * 100.0f;
                            // after configured percentage has been buffered, connect the graph
                            if (!filterConnected && (percentageBuffered >= PluginConfiguration.Instance.playbuffer || skipBuffering))
                            {
                                //cacheFile = filterState.GetCacheFileName();
                                if (skipBuffering) Log.Instance.Debug("Buffering skipped at {0}%", percentageBuffered);
                                filterConnected = true;
                                renderPinsThread = new Thread(delegate ()
                                {
                                    try
                                    {
                                        //Log.Instance.Debug("BufferFile : Rendering unconnected output pins of source filter ...");
                                        //// add audio and video filter from MP Movie Codec setting section
                                        //AddPreferredFilters(graphBuilder, sourceFilter);
                                        //// connect the pin automatically -> will buffer the full file in cases of bad metadata in the file or request of the audio or video filter
                                        //DirectShowUtil.RenderUnconnectedOutputPins(graphBuilder, sourceFilter);

                                        //if (sourceFilterAudio != null)
                                        //{
                                        //    AddPreferredFilters(graphBuilder, sourceFilterAudio);
                                        //    DirectShowUtil.RenderUnconnectedOutputPins(graphBuilder, sourceFilterAudio);
                                        //}

                                        //Log.Instance.Debug("BufferFile : Playback Ready.");
                                        PlaybackReady = true;
                                    }
                                    catch (ThreadAbortException)
                                    {
                                        Thread.ResetAbort();
                                        Log.Instance.Info("RenderUnconnectedOutputPins foribly aborted.");
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Instance.Warn(ex.Message);
                                        StopBuffering();
                                    }
                                })
                                { IsBackground = true, Name = "OVGraph" };
                                renderPinsThread.Start();
                            }
                            // log every percent
                            if (current > last && current - last >= (double)total * 0.01)
                            {
                                Log.Instance.Debug("Buffering: {0}/{1} KB ({2}%)", current / 1024, total / 1024, (int)percentageBuffered);
                                last = current;
                            }
                            // set the percentage to a gui property, formatted according to percentage, so the user knows very early if anything is buffering                   
                            string formatString = "###";
                            if (percentageBuffered == 0f) formatString = "0.0";
                            else if (percentageBuffered < 1f) formatString = ".00";
                            else if (percentageBuffered < 10f) formatString = "0.0";
                            else if (percentageBuffered < 100f) formatString = "##";
                            GUIPropertyManager.SetProperty("#OnlineVideos.buffered", percentageBuffered.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture));
                            Thread.Sleep(50); // no need to do this more often than 20 times per second
                        }
                        while (!PlaybackReady && graphBuilder != null && !BufferingStopped);
                    }
                    else
                    {
                        GUIPropertyManager.SetProperty("#OnlineVideos.IsBuffering", "False");

                        Thread.Sleep(1000);

                        int iVideoOutputPinId = -1;
                        int iOutPinsCounter = 0;
                        IEnumPins pinEnum;
                        if (sourceFilter.EnumPins(out pinEnum) == 0)
                        {
                            IPin[] pins = new IPin[1];
                            int iFetched;
                            while (iVideoOutputPinId < 0 && pinEnum.Next(1, pins, out iFetched) == 0 && iFetched > 0)
                            {
                                IPin pin = pins[0];
                                PinDirection pinDirection;
                                if (pin.QueryDirection(out pinDirection) == 0 && pinDirection == PinDirection.Output)
                                {
                                    IEnumMediaTypes enumMediaTypesVideo;
                                    if (pin.EnumMediaTypes(out enumMediaTypesVideo) == 0)
                                    {
                                        AMMediaType[] mediaTypes = new AMMediaType[1];
                                        int iTypesFetched;
                                        while (iVideoOutputPinId < 0 && enumMediaTypesVideo.Next(1, mediaTypes, out iTypesFetched) == 0 && iTypesFetched > 0)
                                        {
                                            if (mediaTypes[0].majorType == MediaType.Video)
                                                iVideoOutputPinId = iOutPinsCounter;
                                        }
                                        DirectShowUtil.ReleaseComObject(enumMediaTypesVideo);
                                    }

                                    iOutPinsCounter++;
                                }
                                DirectShowUtil.ReleaseComObject(pin);
                            }
                            DirectShowUtil.ReleaseComObject(pinEnum);
                        }

                        if (iVideoOutputPinId < 0)
                        {
                            Log.Instance.Error("[BufferFile] Failed to get video pin.");
                            return false;
                        }

                        this._SourceFilterVideoPinIndex = iVideoOutputPinId;

                        percentageBuffered = 100.0f; // no progress reporting possible
                        GUIPropertyManager.SetProperty("#TV.Record.percent3", percentageBuffered.ToString());
                        GUIPropertyManager.SetProperty("#OnlineVideos.bufferedenough", "80");
                        PlaybackReady = true;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (OnlineVideosException)
            {
                throw;
            }
            catch (COMException comEx)
            {
                Log.Instance.Warn(comEx.ToString());

                string errorText = DirectShowLib.DsError.GetErrorText(comEx.ErrorCode);
                if (errorText != null) errorText = errorText.Trim();
                if (!string.IsNullOrEmpty(errorText))
                {
                    throw new OnlineVideosException(errorText);
                }
            }
            
            catch (Exception ex)
            {
                Log.Instance.Warn(ex.ToString());
            }
            finally
            {
                if (sourceFilter != null)
                {
                    // the render pin thread was already started and is still runnning
                    if (renderPinsThread != null && (renderPinsThread.ThreadState & ThreadState.Stopped) == 0)
                    {
                        // buffering was stopped by the user -> abort the thread
                        if (BufferingStopped) renderPinsThread.Abort();
                    }

                    int result;

                    // playback is not ready but the source filter is already downloading -> abort the operation
                    if (!PlaybackReady)
                    {
                        Log.Instance.Info("Buffering was aborted.");
                        if (sourceFilter is IAMOpenProgress) ((IAMOpenProgress)sourceFilter).AbortOperation();
                        
                        Thread.Sleep(100); // give it some time
                        result = graphBuilder.RemoveFilter(sourceFilter); // remove the filter from the graph to prevent lockup later in Dispose
                    }

                    // release the COM pointer that we created
                    DirectShowUtil.ReleaseComObject(sourceFilter);

                    if (!PlaybackReady && this._MixedUrl != null)
                    {
                        for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
                        {
                            graphBuilder.FindFilterByName(sourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilterAudio);

                            if (sourceFilterAudio is IAMOpenProgress)
                            {
                                ((IAMOpenProgress)sourceFilterAudio).AbortOperation();
                                Thread.Sleep(100); // give it some time
                            }
                                                        
                            result = graphBuilder.RemoveFilter(sourceFilterAudio);

                            DirectShowUtil.ReleaseComObject(sourceFilterAudio);
                        }

                    }
                }
            }

            return PlaybackReady;
        }

        /// <summary>
        /// Third and last step of a graph build with the file source url filter used to monitor buffer.
        /// Needs to be called on the MpMain Thread.
        /// </summary>
        /// <returns></returns>
        bool FinishPreparedGraph()
        {
            try
            {
                //Load all decoders and connect them with MPAudioSwitcher(if needed)
                //This has to be done on main thread(due to use of MPAudioSwitcher)
                this.loadDecoderFilters();

                DirectShowUtil.EnableDeInterlace(graphBuilder);

                if (Vmr9 == null || !Vmr9.IsVMR9Connected)
                {
                    Log.Instance.Warn("OnlineVideosPlayer: Failed to render file -> No video renderer connected");
                    mediaCtrl = null;
                    Cleanup();
                    return false;
                }

                try
                {
                    // remove filter that are not used from the graph
                    DirectShowUtil.RemoveUnusedFiltersFromGraph(graphBuilder);
                }
                catch (Exception ex)
                {
                    Log.Instance.Warn("Error during RemoveUnusedFiltersFromGraph: {0}", ex.ToString());
                }

                if (Log.Instance.LogLevel < log4net.Core.Level.Debug)
                {
                    string sourceFilterName = GetSourceFilterName(m_strCurrentFile);
                    if (!string.IsNullOrEmpty(sourceFilterName))
                    {
                        IBaseFilter sourceFilter;
                        if (graphBuilder.FindFilterByName(sourceFilterName, out sourceFilter) == 0 && sourceFilter != null)
                        {
                            LogOutputPinsConnectionRecursive(sourceFilter);
                        }

                        if (sourceFilter != null)
                            DirectShowUtil.ReleaseComObject(sourceFilter);

                        if (this._MixedUrl != null)
                        {
                            for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
                            {
                                if (graphBuilder.FindFilterByName(sourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilter) == 0 && sourceFilter != null)
                                    LogOutputPinsConnectionRecursive(sourceFilter);

                                if (sourceFilter != null)
                                {
                                    DirectShowUtil.ReleaseComObject(sourceFilter);
                                    sourceFilter = null;
                                }
                            }
                        }
                    }
                }

                this.Vmr9.SetDeinterlaceMode();

                // now set VMR9 to Active
                GUIGraphicsContext.Vmr9Active = true;

                // set fields for playback                
                m_iVideoWidth = Vmr9.VideoWidth;
                m_iVideoHeight = Vmr9.VideoHeight;

                Vmr9.SetDeinterlaceMode();
                return true;
            }
            catch (Exception ex)
            {
                Error.SetError("Unable to play movie", "Unable build graph for VMR9");
                Log.Instance.Error("OnlineVideosPlayer:exception while creating DShow graph {0} {1}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        public override bool Play(string strFile)
        {
            updateTimer = DateTime.Now;
            m_speedRate = 10000;
            m_bVisible = false;
            m_iVolume = 100;
            m_state = PlayState.Init;
            if (strFile != "http://localhost/OnlineVideo.mp4") m_strCurrentFile = strFile; // hack to get around the MP 1.3 Alpha bug with non http URLs
            m_bFullScreen = true;
            m_ar = GUIGraphicsContext.ARType;
            VideoRendererStatistics.VideoState = VideoRendererStatistics.State.VideoPresent;
            _updateNeeded = true;
            Log.Instance.Info("OnlineVideosPlayer: Play '{0}'", m_strCurrentFile);

            m_bStarted = false;
            if (!GetInterfaces())
            {
                m_strCurrentFile = "";
                CloseInterfaces();
                return false;
            }

            // if we are playing a local file set the cache file so refresh rate adaption can happen
            Uri uri = new Uri(m_strCurrentFile);
            string protocol = uri.Scheme.Substring(0, Math.Min(uri.Scheme.Length, 4));
            if (protocol == "file") cacheFile = m_strCurrentFile;

            if (PluginConfiguration.Instance.AllowRefreshRateChange)
                AdaptRefreshRateFromCacheFile();

            ISubEngine engine = SubEngine.GetInstance(true);
            if (!engine.LoadSubtitles(graphBuilder, string.IsNullOrEmpty(SubtitleFile) ? m_strCurrentFile : SubtitleFile))
            {
                SubEngine.engine = new SubEngine.DummyEngine();
            }
            else
            {
                engine.Enable = true;
            }

            IPostProcessingEngine postengine = PostProcessingEngine.GetInstance(true);
            if (!postengine.LoadPostProcessing(graphBuilder))
            {
                PostProcessingEngine.engine = new PostProcessingEngine.DummyEngine();
            }

            IAudioPostEngine audioEngine = AudioPostEngine.GetInstance(true);
            if (audioEngine != null && !audioEngine.LoadPostProcessing(graphBuilder))
            {
                AudioPostEngine.engine = new AudioPostEngine.DummyEngine();
            }

            if (this._audioSwitcher != null)
            {
                this.analyseStreams();

                if (this._MixedUrl.DefaultAudio >= 0 && this._MixedUrl.DefaultAudio < this._MixedUrl.AudioTracks.Length)
                    this.CurrentAudioStream = this._MixedUrl.DefaultAudio;
                else
                    SelectAudioLanguage();
            }
            else
            {
                AnalyseStreams();
                SelectAudioLanguage();
            }

            SelectSubtitles();
            OnInitialized();

            int hr = mediaEvt.SetNotifyWindow(GUIGraphicsContext.ActiveForm, WM_GRAPHNOTIFY, IntPtr.Zero);
            if (hr < 0)
            {
                Error.SetError("Unable to play movie", "Can not set notifications");
                m_strCurrentFile = "";
                CloseInterfaces();
                return false;
            }
            if (videoWin != null)
            {
                videoWin.put_WindowStyle((WindowStyle)((int)WindowStyle.Child + (int)WindowStyle.ClipChildren + (int)WindowStyle.ClipSiblings));
                videoWin.put_MessageDrain(GUIGraphicsContext.form.Handle);
            }

            DirectShowUtil.SetARMode(graphBuilder, AspectRatioMode.Stretched);

            try
            {
                if (protocol == "file")
                {
                    if (Vmr9 != null) Vmr9.StartMediaCtrl(mediaCtrl);
                }
                else
                {
                    hr = mediaCtrl.Run();
                    DsError.ThrowExceptionForHR(hr);
                    if (hr == 1)
                    // S_FALSE from IMediaControl::Run means: The graph is preparing to run, but some filters have not completed the transition to a running state.
                    {
                        // wait max. 20 seconds for the graph to transition to the running state
                        DateTime startTime = DateTime.Now;
                        FilterState filterState;
                        do
                        {
                            Thread.Sleep(100);
                            hr = mediaCtrl.GetState(100, out filterState);
                            // check with timeout max. 10 times a second if the state changed
                        } while ((hr != 0) && ((DateTime.Now - startTime).TotalSeconds <= 20));
                        if (hr != 0) // S_OK
                        {
                            DsError.ThrowExceptionForHR(hr);
                            throw new Exception(string.Format("IMediaControl.GetState after 20 seconds: 0x{0} - '{1}'",
                                hr.ToString("X8"), DsError.GetErrorText(hr)));
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Log.Instance.Warn("OnlineVideosPlayer: Unable to play with reason: {0}", error.Message);
            }
            if (hr != 0) // S_OK
            {
                Error.SetError("Unable to play movie", "Unable to start movie");
                m_strCurrentFile = "";
                CloseInterfaces();
                return false;
            }

            if (basicVideo != null)
            {
                basicVideo.GetVideoSize(out m_iVideoWidth, out m_iVideoHeight);
            }

            if (GoFullscreen) GUIWindowManager.ActivateWindow(GUIOnlineVideoFullscreen.WINDOW_FULLSCREEN_ONLINEVIDEO);
            GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_PLAYBACK_STARTED, 0, 0, 0, 0, 0, null);
            msg.Label = CurrentFile;
            GUIWindowManager.SendThreadMessage(msg);
            m_state = PlayState.Playing;
            m_iPositionX = GUIGraphicsContext.VideoWindow.X;
            m_iPositionY = GUIGraphicsContext.VideoWindow.Y;
            m_iWidth = GUIGraphicsContext.VideoWindow.Width;
            m_iHeight = GUIGraphicsContext.VideoWindow.Height;
            m_ar = GUIGraphicsContext.ARType;
            _updateNeeded = true;
            SetVideoWindow();
            mediaPos.get_Duration(out m_dDuration);
            Log.Instance.Info("OnlineVideosPlayer: Duration {0} sec", m_dDuration.ToString("F"));

            return true;
        }

        public override void Stop()
        {
            Log.Instance.Info("OnlineVideosPlayer: Stop");
            m_strCurrentFile = "";
            this.disposeSourceFilter();
            CloseInterfaces();
            m_state = PlayState.Init;
            GUIGraphicsContext.IsPlaying = false;
        }

        public override void Dispose()
        {
            base.Dispose();
            GUIPropertyManager.SetProperty("#TV.Record.percent3", 0.0f.ToString());
            GUIPropertyManager.SetProperty("#OnlineVideos.IsBuffering", "False");
        }

        public override int CurrentAudioStream 
        {
            get
            {
                return this._audioSwitcher != null ? this._CurrentAudioStream : base.CurrentAudioStream;
            }

            set
            {
                this.disposeSourceFilter();

                if (this._audioSwitcher != null)
                {
                    if (this._CurrentAudioStream != value)
                    {
                        Log.Instance.Debug("OnlineVideosPlayer: CurrentAudioStream:{0} Request:{1}", this._CurrentAudioStream, value);

                        int iIdxSwitcher = -1;
                        bool bResult = false;

                        if (this._InternalAudioStreams == 0)
                            iIdxSwitcher = value; //external only
                        else if (value >= this._InternalAudioStreams)
                            iIdxSwitcher = value - this._InternalAudioStreams + 1; //to external
                        else
                        {
                            //to internal
                            FilterStreamInfos fsi = FStreams.GetStreamInfos(StreamType.Audio, value);
                            if (!(bResult = EnableStream(fsi.Id, AMStreamSelectEnableFlags.Enable, fsi.Filter)))
                                goto log;

                            if (this._CurrentAudioStream >= this._InternalAudioStreams)
                                iIdxSwitcher = 0; //Switch to first input of AudioSwitcher(internal audio)
                        }

                        if (iIdxSwitcher >= 0)
                        {
                            Log.Instance.Debug("OnlineVideosPlayer: Switching AudioSwitcher to input: {0}", iIdxSwitcher);
                            bResult = this.EnableStream(iIdxSwitcher, AMStreamSelectEnableFlags.Enable, MEDIAPORTAL_AUDIOSWITCHER_FILTER);
                        }

                        if (bResult)
                            this._CurrentAudioStream = value;
                    log:
                        Log.Instance.Info("OnlineVideosPlayer: CurrentAudioStream:{0} Result:{1}", value, bResult);
                    }
                }
                else
                    base.CurrentAudioStream = value;
            }
        }

        public override int AudioStreams
        {
            get
            {
                if (this._audioSwitcher != null)
                    return this._MixedUrl.AudioTracks.Length + this._InternalAudioStreams;

                return base.AudioStreams;
            }
        }

        public override string AudioType(int iStream)
        {
            return null;
        }

        public override string AudioLanguage(int iStream)
        {
            if (this._audioSwitcher != null && (this._InternalAudioStreams == 0 || iStream >= this._InternalAudioStreams))
            {
                iStream -= this._InternalAudioStreams;

                CultureInfo ciTrack = null;
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                string strLanguage = this._MixedUrl.AudioTracks[iStream].Language;

                if (!string.IsNullOrWhiteSpace(strLanguage))
                {
                    if (strLanguage.Length >= 5 && strLanguage[2] == '-')
                        strLanguage = strLanguage.Substring(0, 2);

                    ciTrack = cultures.FirstOrDefault(ci => ci.Name.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
                        || ci.ThreeLetterISOLanguageName.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
                        || ci.EnglishName.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
                        );
                }

                if (ciTrack == null)
                    strLanguage = "Undetermined";
                else
                    strLanguage = ciTrack.EnglishName;

                strLanguage = MediaPortal.Util.Utils.TranslateLanguageString(strLanguage);

                if (!string.IsNullOrWhiteSpace(this._MixedUrl.AudioTracks[iStream].Description))
                    strLanguage += " [" + this._MixedUrl.AudioTracks[iStream].Description + ']';

                return strLanguage;
            }

            return base.AudioLanguage(iStream);
        }

        public override void SeekAbsolute(double dTime)
        {
            //this.onBuffer();
            base.SeekAbsolute(dTime);
            this.onSeek();
        }

        public override void SeekAsolutePercentage(int iPercentage)
        {
            //this.onBuffer();
            base.SeekAsolutePercentage(iPercentage);
            this.onSeek();
        }

        public override void SeekRelative(double dTime)
        {
            //this.onBuffer();
            base.SeekRelative(dTime);
            this.onSeek();
        }

        public override void SeekRelativePercentage(int iPercentage)
        {
            //this.onBuffer();
            base.SeekRelativePercentage(iPercentage);
            this.onSeek();
        }

        #region OVSPLayer Member

        public bool GoFullscreen { get; set; }
        public string SubtitleFile { get; set; }
        public string PlaybackUrl { get { return m_strCurrentFile; } }

        #endregion

        public static IBaseFilter AddPreferredFilters(IGraphBuilder graphBuilder, IBaseFilter sourceFilter, bool bReleaseAudioDecoder = true)
        {
            using (Settings xmlreader = new MPSettings())
            {
                bool autodecodersettings = xmlreader.GetValueAsBool("movieplayer", "autodecodersettings", false);

                if (!autodecodersettings) // the user has not chosen automatic graph building by merits
                {
                    bool bVc1ICodec = false, bVc1Codec = false, bXvidCodec = false; //- will come later
                    bool aacCodec = false;
                    bool h264Codec = false;
                    bool videoCodec = false;
                    bool audioCodec = false;
                    bool bHevcCodec = false;

                    // check the output pins of the splitter for known media types
                    IEnumPins pinEnum = null;
                    if (sourceFilter.EnumPins(out pinEnum) == 0)
                    {
                        int fetched = 0;
                        IPin[] pins = new IPin[1];
                        while (pinEnum.Next(1, pins, out fetched) == 0 && fetched > 0)
                        {
                            IPin pin = pins[0];
                            PinDirection pinDirection;
                            if (pin.QueryDirection(out pinDirection) == 0 && pinDirection == PinDirection.Output)
                            {
                                IEnumMediaTypes enumMediaTypesVideo = null;
                                if (pin.EnumMediaTypes(out enumMediaTypesVideo) == 0)
                                {
                                    AMMediaType[] mediaTypes = new AMMediaType[1];
                                    int typesFetched;
                                    while (enumMediaTypesVideo.Next(1, mediaTypes, out typesFetched) == 0 && typesFetched > 0)
                                    {
                                        if (mediaTypes[0].majorType == MediaType.Video)
                                        {
                                            if (mediaTypes[0].subType == MediaSubType.HEVC)
                                            {
                                                Log.Instance.Info("[AddPreferredFilters] Found HEVC video on output pin");
                                                bHevcCodec = true;
                                            }
                                            else if (mediaTypes[0].subType == MediaSubType.H264 || mediaTypes[0].subType == MEDIASUBTYPE_AVC1)
                                            {
                                                Log.Instance.Info("found H264 video on output pin");
                                                h264Codec = true;
                                            }
                                            else if (mediaTypes[0].subType == MediaSubType.VC1)
                                            {
                                                Log.Instance.Info("[AddPreferredFilters] Found VC1 video on output pin");

                                                //if (g_Player.MediaInfo.IsInterlaced)
                                                //    bVc1ICodec = true;
                                                //else
                                                bVc1Codec = true;
                                            }
                                            else if (mediaTypes[0].subType == MediaSubType.XVID || mediaTypes[0].subType == MediaSubType.xvid)
                                            {
                                                Log.Instance.Info("[AddPreferredFilters] Found xvid video on output pin");
                                                bXvidCodec = true;
                                            }
                                            else
                                                videoCodec = true;

                                        }
                                        else if (mediaTypes[0].majorType == MediaType.Audio)
                                        {
                                            if (mediaTypes[0].subType == MediaSubType.LATMAAC || mediaTypes[0].subType == MediaSubType.LATMAACLAF)
                                            {
                                                Log.Instance.Info("found AAC audio on output pin");
                                                aacCodec = true;
                                            }
                                            else
                                                audioCodec = true;
                                        }
                                    }
                                    DirectShowUtil.ReleaseComObject(enumMediaTypesVideo);
                                }
                            }
                            DirectShowUtil.ReleaseComObject(pin);
                        }
                        DirectShowUtil.ReleaseComObject(pinEnum);
                    }

                    // add filters for found media types to the graph as configured in MP
                    if (h264Codec)
                    {
                        _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "h264videocodec", "");
                        DirectShowUtil.ReleaseComObject(
                            DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
                    }
                    else if (bHevcCodec)
                    {
                        _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "hevcvideocodec", "");
                        DirectShowUtil.ReleaseComObject(
                            DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
                    }
                    else if (bXvidCodec)
                    {
                        _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "xvidvideocodec", "");
                        DirectShowUtil.ReleaseComObject(
                            DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
                    }
                    else if (bVc1Codec)
                    {
                        _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "vc1videocodec", "");
                        DirectShowUtil.ReleaseComObject(
                            DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
                    }
                    else if (bVc1ICodec)
                    {
                        _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "vc1ivideocodec", "");
                        DirectShowUtil.ReleaseComObject(
                            DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
                    }
                    else if (videoCodec)
                    {
                        _VideoDecoder = xmlreader.GetValueAsString("movieplayer", "mpeg2videocodec", "");
                        DirectShowUtil.ReleaseComObject(
                            DirectShowUtil.AddFilterToGraph(graphBuilder, _VideoDecoder));
                    }

                    //Get audio decoder
                    if (aacCodec || audioCodec)
                    {
                        if (aacCodec)
                            _AudioDecoder = xmlreader.GetValueAsString("movieplayer", "aacaudiocodec", "");
                        else
                            _AudioDecoder = xmlreader.GetValueAsString("movieplayer", "mpeg2audiocodec", "");

                        if (bReleaseAudioDecoder)
                            DirectShowUtil.ReleaseComObject(DirectShowUtil.AddFilterToGraph(graphBuilder, _AudioDecoder));
                        else
                            return DirectShowUtil.AddFilterToGraph(graphBuilder, _AudioDecoder);
                    }
                }
            }

            return null;
        }

        public static readonly Guid MEDIASUBTYPE_AVC1 = new Guid("31435641-0000-0010-8000-00aa00389b71");

        public static void LogOutputPinsConnectionRecursive(IBaseFilter filter)
        {
            StringBuilder sb = new StringBuilder(128);
            LogOutputPinsConnectionRecursive(filter, sb, true);
            Log.Instance.Debug(sb.ToString());
        }
        private static void LogOutputPinsConnectionRecursive(IBaseFilter filter,  StringBuilder sb, bool bIsRoot)
        {
            if (filter.EnumPins(out IEnumPins pinEnum) == 0)
            {
                int iPinCnt = 0;
                filter.QueryFilterInfo(out FilterInfo sourceFilterInfo);
                IPin[] pins = new IPin[1];
                while (pinEnum.Next(1, pins, out int fetched) == 0 && fetched > 0)
                {
                    IPin pin = pins[0];
                    if (pin.QueryDirection(out PinDirection pinDirection) == 0 && pinDirection == PinDirection.Output)
                    {
                        if (pin.ConnectedTo(out IPin connectedPin) == 0 && connectedPin != null)
                        {
                            connectedPin.QueryPinInfo(out PinInfo connectedPinInfo);
                            connectedPinInfo.filter.QueryFilterInfo(out FilterInfo connectedFilterInfo);

                            DsUtils.FreePinInfo(connectedPinInfo);

                            if (sb.Length == 0)
                                sb.Append(sourceFilterInfo.achName);

                            DirectShowUtil.ReleaseComObject(connectedPin, 2000);
                            if (connectedFilterInfo.pGraph.FindFilterByName(connectedFilterInfo.achName, out IBaseFilter connectedFilter) == 0 && connectedFilter != null)
                            {
                                if (bIsRoot)
                                {
                                    sb.Append("\r\n#Pin[");
                                    sb.Append(iPinCnt);
                                    sb.Append("]");
                                }

                                sb.Append(" --> ");
                                sb.Append(connectedFilterInfo.achName);
                                LogOutputPinsConnectionRecursive(connectedFilter, sb, false);
                                DirectShowUtil.ReleaseComObject(connectedFilter);
                            }

                            DirectShowUtil.ReleaseComObject(connectedFilterInfo.pGraph);
                        }
                        DirectShowUtil.ReleaseComObject(pin, 2000);
                        iPinCnt++;
                    }
                }
                DirectShowUtil.ReleaseComObject(sourceFilterInfo.pGraph);
            }
            DirectShowUtil.ReleaseComObject(pinEnum, 2000);
        }


        private void loadDecoderFilters()
        {
            Log.Instance.Debug("loadDecoderFilters : Rendering unconnected output pins of source filter ...");

            string strSourceFilterName = GetSourceFilterName(this.m_strCurrentFile);

            IBaseFilter sourceFilter = null;

            int iAudioSwitcherPinIndex = 0;

            try
            {
                this.graphBuilder.FindFilterByName(strSourceFilterName, out sourceFilter);

                this._InternalAudioStreams = this.getNumberOfAudioStreams(sourceFilter);

                //Multiple audio tracks: we need MPAudioSwitcher
                if (this._MixedUrl != null && (this._MixedUrl.AudioTracks.Length > 1 || (this._MixedUrl.AudioTracks.Length > 0 && this._InternalAudioStreams > 0)))
                    this._audioSwitcher = DirectShowUtil.AddFilterToGraph(this.graphBuilder, MEDIAPORTAL_AUDIOSWITCHER_FILTER);

                if (this._audioSwitcher != null && this._InternalAudioStreams > 0)
                {
                    IBaseFilter audioDecoder = AddPreferredFilters(this.graphBuilder, sourceFilter, false);
                    try
                    {
                        //Connect sourcefilter's audio output to audio decoder
                        IPin pinIn = DsFindPin.ByDirection(audioDecoder, PinDirection.Input, 0);
                        this.tryConnect(sourceFilter, pinIn);
                        DirectShowUtil.ReleaseComObject(pinIn, 2000);

                        //Connect audiodecoder with audioswitcher
                        this.connectAudioToSwitcher(audioDecoder, ref iAudioSwitcherPinIndex);
                    }
                    finally
                    {
                        DirectShowUtil.ReleaseComObject(audioDecoder);
                    }
                }
                else
                {
                    // add audio and video filter from MP Movie Codec setting section
                    AddPreferredFilters(this.graphBuilder, sourceFilter);
                }

                // connect the pin automatically -> will buffer the full file in cases of bad metadata in the file or request of the audio or video filter
                DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, sourceFilter);
            }
            finally
            {
                if (sourceFilter != null)
                    DirectShowUtil.ReleaseComObject(sourceFilter);
            }

            if (this._MixedUrl != null)
            {
                for (int i = 0; i < this._MixedUrl.AudioTracks.Length; i++)
                {
                    try
                    {
                        this.graphBuilder.FindFilterByName(strSourceFilterName + ' ' + (i + 1).ToString("0000"), out sourceFilter);

                        if (this._MixedUrl.AudioTracks.Length == 1)
                        {
                            AddPreferredFilters(this.graphBuilder, sourceFilter);
                            DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, sourceFilter);
                        }
                        else
                        {
                            IBaseFilter audioDecoder = AddPreferredFilters(this.graphBuilder, sourceFilter, false);
                            IPin pinOut = null;
                            IPin pinIn = null;
                            try
                            {
                                //Connect source filter with audiodecoder
                                pinOut = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 0);
                                pinIn = DsFindPin.ByDirection(audioDecoder, PinDirection.Input, 0);
                                Marshal.ThrowExceptionForHR(this.graphBuilder.Connect(pinOut, pinIn));

                                //Connect audiodecoder with audioswitcher
                                this.connectAudioToSwitcher(audioDecoder, ref iAudioSwitcherPinIndex);
                            }
                            finally
                            {
                                if (audioDecoder != null)
                                    DirectShowUtil.ReleaseComObject(audioDecoder);

                                if (pinOut != null)
                                    DirectShowUtil.ReleaseComObject(pinOut, 2000);

                                if (pinIn != null)
                                    DirectShowUtil.ReleaseComObject(pinIn, 2000);
                            }
                        }
                    }
                    finally
                    {
                        if (sourceFilter != null)
                            DirectShowUtil.ReleaseComObject(sourceFilter);
                    }
                }

                //Connect audioswitcher with renderer
                if (this._audioSwitcher != null)
                    DirectShowUtil.RenderUnconnectedOutputPins(this.graphBuilder, this._audioSwitcher);
            }
        }

        private bool analyseStreams()
        {
            try
            {
                if (this.FStreams == null)
                    this.FStreams = new FilterStreams();

                this.FStreams.DeleteAllStreams();

                string strSourceFilterName = GetSourceFilterName(this.m_strCurrentFile);

                for (int i = 0; i <= this._MixedUrl.AudioTracks.Length; i++)
                {
                    string strFilterName = strSourceFilterName + (i > 0 ? " " + i.ToString("0000") : null);
                    this.graphBuilder.FindFilterByName(strFilterName, out IBaseFilter sourceFilter);

                    if (sourceFilter is IAMStreamSelect pStrm)
                    {
                        pStrm.Count(out int cStreams);

                        //GET STREAMS
                        for (int istream = 0; istream < cStreams; istream++)
                        {
                            //STREAM INFO
                            pStrm.Info(istream, out AMMediaType sType, out AMStreamSelectInfoFlags sFlag, out int sPLCid,
                                       out int sPDWGroup, out string sName, out _, out _);

                            FilterStreamInfos fsInfos = new FilterStreamInfos
                            {
                                Current = false,
                                Filter = strFilterName,
                                Name = sName,
                                LCID = sPLCid,
                                Id = istream,
                                Type = StreamType.Unknown,
                                sFlag = sFlag
                            };

                            if (sPDWGroup == 0)
                            {
                                fsInfos.Type = StreamType.Video;
                            }
                            else if (sPDWGroup == 1)
                            {
                                fsInfos.Type = StreamType.Audio;
                            }
                            else if (sPDWGroup == 2 && sName.LastIndexOf("off", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Hide ", StringComparison.Ordinal) == -1 &&
                                sName.LastIndexOf("No ", StringComparison.Ordinal) == -1 && sName.LastIndexOf("Miscellaneous ", StringComparison.Ordinal) == -1)
                            {
                                fsInfos.Type = StreamType.Subtitle;
                            }
                            else
                                continue;

                            Log.Instance.Debug("AnalyseStreams: FoundStreams: Type={0}; Name={1}, Filter={2}, Id={3}, PDWGroup={4}, LCID={5}",
                                      fsInfos.Type.ToString(), fsInfos.Name, fsInfos.Filter, fsInfos.Id.ToString(),
                                      sPDWGroup.ToString(), sPLCid.ToString());

                            this.FStreams.AddStreamInfos(fsInfos);
                        }
                    }
                    DirectShowUtil.ReleaseComObject(sourceFilter);
                }
            }
            catch { }
            return true;
        }

        private int getNumberOfAudioStreams(IBaseFilter filterSource)
        {
            int iResult = 0;
            if (filterSource is IAMStreamSelect pStrm)
            {
                pStrm.Count(out int iStreams);

                for (int i = 0; i < iStreams; i++)
                {
                    //STREAM INFO
                    pStrm.Info(i, out _, out _, out _, out int iPDWGroup, out _, out _, out _);

                    if (iPDWGroup == 1) //Audio
                        iResult++;
                }
            }
            Log.Instance.Debug("[getNumberOfAudioStreams] Number of audio streams in source filter: {0}", iResult);
            return iResult;
        }

        private bool tryConnect(IBaseFilter filterSource, IPin pinTarget)
        {
            bool bResult = false;
            int iHr = filterSource.EnumPins(out IEnumPins pinEnum);
            DsError.ThrowExceptionForHR(iHr);
            if (iHr == 0 && pinEnum != null)
            {
                pinEnum.Reset();
                IPin[] pins = new IPin[1];
                while (!bResult && pinEnum.Next(1, pins, out int iFetchedd) == 0 && iFetchedd > 0)
                {
                    pins[0].QueryDirection(out PinDirection pinDir);
                    if (pinDir == PinDirection.Output)
                        bResult = this.graphBuilder.Connect(pins[0], pinTarget) == 0;
                }
                DirectShowUtil.ReleaseComObject(pins[0]);
            }
            DirectShowUtil.ReleaseComObject(pinEnum);

            return bResult;
        }

        private void connectAudioToSwitcher(IBaseFilter audioDecoder, ref int iInpuPinIIdx)
        {
            //Connect audiodecoder with audioswitcher
            IPin pinOut = DsFindPin.ByDirection(audioDecoder, PinDirection.Output, 0);
            IPin pinIn = DsFindPin.ByDirection(this._audioSwitcher, PinDirection.Input, iInpuPinIIdx++);
            Marshal.ThrowExceptionForHR(this.graphBuilder.Connect(pinOut, pinIn));
            DirectShowUtil.ReleaseComObject(pinOut, 2000);
            DirectShowUtil.ReleaseComObject(pinIn, 2000);
        }


        const bool LAV_ALWAYS_PREBUFFER = false; //set to true if we wants to always prebuffer at start or after seek
        const int LAV_MIN_BUFFER_LEVEL = 5; //[%]; when frame duration is unknown
        const double LAV_MIN_BUFFER_TIME = 0.5; //[s]
        const int LAV_BUFFERED_LEVEL = 33; //[%]; when frame duration is unknown
        const double LAV_BUFFERED_TIME = 5.0; //[s]
        const double LAV_MIN_BUFFER_TOTAL_TIME = 20.0; //[s]; set LAV buffer size at least to this value(if frame duration is known)
        const double LAV_DEFAULT_BUFFER_TOTAL_TIME = 10.0; //[s]; when frame duration is unknown

        private enum BufferStatusEnum
        {
            BufferingNeeded = -1,
            AboveMinLevel = 0,
            BufferedEnough = 1
        };
        private int _LavMaxQueue = -1;
        private double _LavMaxQueueTime = -1;
        private int _LavMaxQueueMemSize = -1;
        private IBaseFilter _SourceFilter;
        private string _SourceFilterName;
        private int _SourceFilterVideoPinIndex = -1;
        private double _VideoSampleDuration = -1;
        private DateTime _LastProgressCheck;
        private bool _Buffering = false;
        private double _LastCurrentPosition = 0.0d;
        private bool _PlaybackDetected = false;
        private bool _PreBufferNeeded = true;
        private DateTime _SeekTimeStamp = DateTime.MinValue;
        private static System.Globalization.CultureInfo _Culture_EN = new System.Globalization.CultureInfo("en-US");

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void onSeek()
        {
            if (this.mediaPos != null)
                this.mediaPos.get_CurrentPosition(out this._LastCurrentPosition);

            this._PlaybackDetected = false;
            this._PreBufferNeeded = true;
            this._LastProgressCheck = DateTime.MinValue;
            this._SeekTimeStamp = DateTime.Now;

            Log.Instance.Debug("[onSeek] Current position: {0}", this._LastCurrentPosition);
        }

        private IBaseFilter getSourceFilter()
        {
            if (this._SourceFilter == null)
            {
                Log.Instance.Debug("[GetSourceFilter]");

                if (this._SourceFilterName == null)
                    this._SourceFilterName = this.GetSourceFilterName(this.m_strCurrentFile);

                int iResult = this.graphBuilder.FindFilterByName(this._SourceFilterName, out this._SourceFilter);
                if (iResult != 0)
                {
                    string strErrorText = DsError.GetErrorText(iResult);
                    if (strErrorText != null)
                        strErrorText = strErrorText.Trim();

                    Log.Instance.Warn("[GetSourceFilter] FindFilterByName returned '{0}'{1}", "0x" + iResult.ToString("X8"), !string.IsNullOrEmpty(strErrorText) ? " : (" + strErrorText + ")" : "");
                    return null;
                }
            }
            return this._SourceFilter;
        }

        private bool disposeSourceFilter()
        {
            if (this._SourceFilter != null)
            {
                DirectShowUtil.ReleaseComObject(this._SourceFilter);
                this._SourceFilter = null;
                return true;
            }
            return false;
        }

        private static string printFileSize(long lValue)
        {
            return printFileSize(lValue, "0");
        }
        private static string printFileSize(long lValue, string strFormat)
        {
            if (lValue < 0)
                return string.Empty;

            string strSuffix, strValue;

            if (lValue < 1024)
            {
                strValue = lValue.ToString();
                strSuffix = " B";
            }
            else if (lValue < 1048576)
            {
                strValue = ((float)lValue / 1024).ToString(strFormat, _Culture_EN);
                strSuffix = " kB";
            }
            else if (lValue < 1073741824)
            {
                strValue = ((float)lValue / 1048576).ToString(strFormat, _Culture_EN);
                strSuffix = " MB";
            }
            else
            {
                strValue = ((float)lValue / 1073741824).ToString("0.00", _Culture_EN);
                strSuffix = " GB";
            }

            return strValue + strSuffix;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void onBuffer()
        {
            if (!g_Player.Paused)
                g_Player.Pause();

            GUIPropertyManager.SetProperty("#OnlineVideos.IsBuffering", "True");
            this._Buffering = true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void processLavBufferLevel()
        {
            if (!this.UseLAV || this._LavMaxQueue < -1)
                return; //non LAV filter

            if ((DateTime.Now - this._LastProgressCheck).TotalMilliseconds >= 1000) //once per second is enough
            {
                this._LastProgressCheck = DateTime.Now;

                try
                {
                    int iLAVsize = -1;
                    int iLAVlevel = -1;
                    double dLAVtime = -1;
                    bool bPlaybackEnds;
                    BufferStatusEnum bufferStatus;

                get:
                    IBaseFilter sourceFilter = this.getSourceFilter();

                    if (this._LavMaxQueue < 0 && sourceFilter != null)
                    {
                        #region LAV init

                        try
                        {
                            ILAVSplitterSettings lav = (ILAVSplitterSettings)sourceFilter;
                            this._LavMaxQueue = lav.GetMaxQueueSize(); //Samples
                            this._LavMaxQueueMemSize = lav.GetMaxQueueMemSize(); //MB

                            if (this._VideoSampleDuration > 0)
                            {
                                //Check min buffer total time
                                int iQueue = (int)Math.Ceiling(LAV_MIN_BUFFER_TOTAL_TIME / this._VideoSampleDuration);
                                if (iQueue > this._LavMaxQueue)
                                {
                                    //Modify MaxQueueSize (for runtime only)
                                    lav.SetRuntimeConfig(true);
                                    lav.SetMaxQueueSize(iQueue);
                                    this._LavMaxQueue = iQueue;
                                }

                                this._LavMaxQueueTime = this._VideoSampleDuration * this._LavMaxQueue;
                            }

                            Log.Instance.Debug("[processLavBufferLevel] MaxQueue: {0} / {1:0.0}s / {2}MB",
                                this._LavMaxQueue, this._LavMaxQueueTime, this._LavMaxQueueMemSize);
                        }
                        catch (Exception ex)
                        {
                            Log.Instance.Error("[processLavBufferLevel] Error: {0}", ex.Message);
                            this._LavMaxQueue = -2; //no success; never use LAV IBuffer
                        }

                        if (this._LavMaxQueue < 0)
                        {
                            //Non LAV filter

                            this.disposeSourceFilter();
                            sourceFilter = null;

                            Log.Instance.Error("[processLavBufferLevel] LAV filter not available.");
                        }
                        #endregion
                    }

                    if (sourceFilter != null)
                    {
                        #region Buffer status

                        LavSplitterSourceInterfaces.IBufferInfo buffer;
                        try
                        {
                            buffer = (LavSplitterSourceInterfaces.IBufferInfo)sourceFilter;
                        }
                        catch (Exception ex)
                        {
                            Log.Instance.Error("[processLavBufferLevel] Error: {0}", ex.Message);
                            this._LavMaxQueue = -1;
                            this.disposeSourceFilter();
                            sourceFilter = null;
                            goto get; //try get source filter again
                        }

                        //Number of buffers (splitter's output pins)
                        uint wCnt = buffer.GetCount();
                        iLAVsize = 0;
                        uint wSamplesVideo = 0;
                        for (uint i = 0; i < wCnt; i++)
                        {
                            if (buffer.GetStatus(i, out uint wSamples, out uint wSize) != 0)
                            {
                                iLAVsize = -1;
                                break;
                            }

                            //Total buffer size
                            iLAVsize += (int)wSize;

                            //Sample count of video output pin
                            if (i == this._SourceFilterVideoPinIndex)
                                wSamplesVideo = wSamples;
                        }

                        //Current LAV video buffer size
                        GUIPropertyManager.SetProperty("#OnlineVideos.BufferSize", printFileSize(iLAVsize));

                        //Current LAV video buffer time
                        if (this._LavMaxQueue > 0)
                        {
                            //Default MaxQueue is 350
                            //Each sample represents 1 video frame(progressive)
                            iLAVlevel = (int)(float)wSamplesVideo * 100 / this._LavMaxQueue;

                            if (this._LavMaxQueueTime > 0)
                            {
                                dLAVtime = this._VideoSampleDuration * wSamplesVideo;
                                GUIPropertyManager.SetProperty("#OnlineVideos.BufferTime", string.Format(_Culture_EN, "{0,4:0.0}", dLAVtime));
                            }
                        }

                        //Current LAV video buffer level
                        if (iLAVlevel >= 0)
                            GUIPropertyManager.SetProperty("#OnlineVideos.BufferLevel", string.Format("{0,3}", iLAVlevel));
                        else
                        {
                            Log.Instance.Error("[processLavBufferLevel] Unknown buffer level.");
                            return;
                        }

                        #endregion
                    }
                    else
                    {
                        Log.Instance.Error("[processLavBufferLevel] Unknown filter.");
                        return;
                    }


                    if (dLAVtime >= 0)
                    {
                        //Frame duration is known

                        if (dLAVtime < LAV_MIN_BUFFER_TIME)
                            bufferStatus = BufferStatusEnum.BufferingNeeded;
                        else if (dLAVtime >= LAV_BUFFERED_TIME)
                            bufferStatus = BufferStatusEnum.BufferedEnough;
                        else
                            bufferStatus = BufferStatusEnum.AboveMinLevel;

                        //Check ending of the stream(to avoid pausing the playback at the end)
                        bPlaybackEnds = this.Duration - this.CurrentPosition < this._LavMaxQueueTime;

                        GUIPropertyManager.SetProperty("#TV.Record.percent3", ((this.CurrentPosition + (this._LavMaxQueueTime * iLAVlevel / 100)) / this.Duration * 100).ToString("0.000"));

                        //double d = (this.CurrentPosition + (this._LavMaxQueueTime * LEVEL_UNPAUSE / 100)) / this.Duration * 100;
                        //GUIPropertyManager.SetProperty("#OnlineVideos.bufferedenough", (d).ToString("0.000"));
                        //GUIPropertyManager.SetProperty("#OnlineVideos.bufferedenoughend", (d + 0.5).ToString("0.000"));
                    }
                    else
                    {
                        //Use % level

                        if (iLAVlevel < LAV_MIN_BUFFER_LEVEL)
                            bufferStatus = BufferStatusEnum.BufferingNeeded;
                        else if (iLAVlevel >= LAV_BUFFERED_LEVEL)
                            bufferStatus = BufferStatusEnum.BufferedEnough;
                        else
                            bufferStatus = BufferStatusEnum.AboveMinLevel;

                        //Check ending of the stream(to avoid pausing the playback at the end)
                        bPlaybackEnds = this.Duration - this.CurrentPosition < LAV_DEFAULT_BUFFER_TOTAL_TIME;

                        GUIPropertyManager.SetProperty("#TV.Record.percent3", ((this.CurrentPosition + (LAV_DEFAULT_BUFFER_TOTAL_TIME * iLAVlevel / 100)) / this.Duration * 100).ToString("0.000"));
                        //GUIPropertyManager.SetProperty("#OnlineVideos.bufferedenough", "0");
                    }

                    //The memory size reached 75% of MaxMemSize(256MB by default); consider the level as enough
                    if (bufferStatus < BufferStatusEnum.BufferedEnough && ((float)iLAVsize / 0x100000 / this._LavMaxQueueMemSize >= 0.75f))
                        bufferStatus = BufferStatusEnum.BufferedEnough;

                    //Playback detection: to avoid buffering at start or after seek (if prebuffering is disabled)
                    if (this._SeekTimeStamp == DateTime.MinValue)
                        this._SeekTimeStamp = DateTime.Now;

                    if (!this._PlaybackDetected && this.m_state == PlayState.Playing &&
                        (bufferStatus >= BufferStatusEnum.AboveMinLevel || (DateTime.Now - this._SeekTimeStamp).TotalMilliseconds >= 5000) &&
                        this.CurrentPosition - this._LastCurrentPosition >= 2.0)
                    {
                        this._PlaybackDetected = true;
                        Log.Instance.Debug("[processLavBufferLevel] Playback detected.");
                    }

                    //GUI Buffering indicator
                    if (this.m_state == PlayState.Playing && !this._Buffering && !bPlaybackEnds &&
                        ((LAV_ALWAYS_PREBUFFER && (bufferStatus < BufferStatusEnum.AboveMinLevel || this._PreBufferNeeded))
                        || (!LAV_ALWAYS_PREBUFFER && bufferStatus < BufferStatusEnum.AboveMinLevel && this._PlaybackDetected))
                        )
                    {
                        if (this._PreBufferNeeded)
                        {
                            this._PreBufferNeeded = false;

                            if (bufferStatus >= BufferStatusEnum.BufferedEnough) //allready enough
                                return;
                        }

                        //Buffer is too low; pause the playback

                        Log.Instance.Debug("[processLavBufferLevel] Buffering activated:  {0} / {1:0.0}s / {2}",
                            iLAVlevel, dLAVtime, printFileSize(iLAVsize));

                        GUIPropertyManager.SetProperty("#OnlineVideos.IsBuffering", "True");
                        this._Buffering = true;

                        if (!g_Player.Paused)
                            g_Player.Pause();
                    }
                    else if (this._Buffering && (bPlaybackEnds || bufferStatus >= BufferStatusEnum.BufferedEnough))
                    {
                        //Buffer has sufficient level; resume the playback

                        Log.Instance.Debug("[processLavBufferLevel] Buffering deactivated.");

                        GUIPropertyManager.SetProperty("#OnlineVideos.IsBuffering", "False");
                        this._Buffering = false;

                        if (g_Player.Paused)
                            g_Player.Pause();
                    }
                }
                catch (Exception ex)
                {
                    Log.Instance.Error("[processLavBufferLevel] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
                }
            }
        }

        private bool MpHasFixedSubs()
        {
            string[] version = System.Windows.Forms.Application.ProductVersion.Split('.');
            int major, minor;
            if (version.Length >= 2 && int.TryParse(version[0], out major) && int.TryParse(version[1], out minor))
                return (major > 1) || (major == 1 && minor > 35);
            return false;
        }

        /// <summary>
        /// Property to get/set the name for a subtitle stream
        /// </summary>
        public override string SubtitleLanguage(int iStream)
        {
            if (MpHasFixedSubs()) return base.SubtitleLanguage(iStream);

            string streamName = SubEngine.GetInstance().GetLanguage(iStream);
            string langName = SubEngine.GetInstance().GetLanguage(iStream);
            string streamNameUND = SubEngine.GetInstance().GetSubtitleName(iStream);
            string strSubtitleFileName = string.IsNullOrEmpty(SubtitleFile) ? m_strCurrentFile : SubtitleFile;

            if (streamName == null)
            {
                return Strings.Unknown;
            }

            if (!string.IsNullOrWhiteSpace(strSubtitleFileName))
            {
                //MPC-HC 2.0.0
                //"[Local] 4-3 bar test.English-Forced.srt\tEnglish"
                Regex regexMPCHC = new Regex(@"^\[([^\]]+)\]\s(?<file>[^\t]+)(\t(?<lng>.+))?");
                Match match = regexMPCHC.Match(streamName);
                if (match.Success)
                {
                    var grLng = match.Groups["lng"];
                    if (grLng.Success)
                        return grLng.Value; //language parsed by MPC-HC

                    string strVideNoExt = Path.GetFileNameWithoutExtension(strSubtitleFileName);
                    string strSubNoExt = Path.GetFileNameWithoutExtension(match.Groups["file"].Value);
                    if (strVideNoExt.Equals(strSubNoExt, StringComparison.CurrentCultureIgnoreCase))
                        return "Undetermined"; //no subtitle suffix
                    else if (strSubNoExt.StartsWith(strVideNoExt, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (strSubNoExt.Length > strVideNoExt.Length)
                            streamName = strSubNoExt.Substring(strVideNoExt.Length + 1); //Subtitle filename has a suffix
                        else
                            streamName = string.Empty;

                        if (string.IsNullOrWhiteSpace(streamName))
                            streamName = strVideNoExt;
                    }
                    else
                    {
                        //difference between m_strCurrentFile and ISubEngine.LoadSubtitles call
                        streamName = strSubNoExt;
                    }

                    langName = streamName;
                    streamNameUND = streamName;
                }
            }

            // remove prefix, which is added by Haali Media Splitter
            streamName = Regex.Replace(streamName, @"^S: ", "");
            // Check if returned string contains both language and trackname info
            // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
            // where "trackname" is stream's "trackname" property muxed in the mkv.
            Regex regex = new Regex(@"\[([^\]]+)\]");
            Regex regexFFD = new Regex(@"\[.+\]");
            Regex regexLAVF =
              new Regex(
                @"(?:S:\s)(?<lang_or_title>.+?)(?:\s*\[(?<lang>[^\]]*?)\])?(?:\s*\((?<info>[^\)]*?)\))?(?:\s*\[(?<Default>[^\]]*?)\])?$");
            // For example MPC Splitter and MPC Engine returns mkv streams as: "language (trackname)",
            // where "trackname" is stream's "trackname" property muxed in the mkv.
            Regex regexMPCEngine = new Regex(@"(\w.+)\((\D+[^\]]+)\)"); //(@"(\w.+)\(([^\]]+)\)");
            Match result = regex.Match(streamName);
            Match resultFFD = regexFFD.Match(streamName);
            Match resultMPCEngine = regexMPCEngine.Match(streamName);
            Match resultLAVF = regexLAVF.Match(streamNameUND);
            if (result.Success)
            {
                string language = MediaPortal.Util.Utils.TranslateLanguageString(result.Groups[1].Value);
                if (language.Length > 0)
                {
                    streamName = language;
                }
            }
            else if (resultFFD.Success)
            {
                string subtitleLanguage = MediaPortal.Util.Utils.TranslateLanguageString(resultFFD.Groups[0].Value);
                if (subtitleLanguage.Length > 0)
                {
                    streamName = subtitleLanguage;
                }
            }
            else if (resultMPCEngine.Success)
            // check for mpc-hc engine response format, e.g.: 
            // Language (Trackname)
            {
                streamName = resultMPCEngine.Groups[1].Value.TrimEnd();
            }
            else if (resultLAVF.Success)
            // check for LAVF response format, e.g.: 
            // S: Title [Lang] (Info) here is to detect if langID = 0 so the language is set as Undetermined
            {
                string lang_or_title = resultLAVF.Groups[1].Value;
                string lang = resultLAVF.Groups[2].Value;
                string info = resultLAVF.Groups[3].Value;
                streamNameUND = Regex.Replace(streamNameUND, @"^S: ", "");
                if (lang_or_title == streamNameUND && lang_or_title == streamName && lang_or_title != langName &&
                    string.IsNullOrEmpty(lang) && string.IsNullOrEmpty(info))
                //|| lang_or_title.Contains("Stream #") && string.IsNullOrEmpty(info)) //string.IsNullOrEmpty(lang_or_title) && string.IsNullOrEmpty(lang))
                {
                    streamName = "Undetermined";
                }
            }
            // mpeg splitter subtitle format
            Match m = Regex.Match(streamName, @"Subtitle\s+-\s+(?<1>.+?),", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string language = MediaPortal.Util.Utils.TranslateLanguageString(m.Groups[1].Value);
                if (language.Length > 0)
                {
                    streamName = language;
                }
            }
            return streamName;
        }

        public override string SubtitleName(int iStream)
        {
            if (MpHasFixedSubs()) return base.SubtitleName(iStream);

            string streamName = SubEngine.GetInstance().GetSubtitleName(iStream);
            string streamNameFalse = SubEngine.GetInstance().GetSubtitleName(iStream);
            string langName = SubEngine.GetInstance().GetLanguage(iStream);
            string strSubtitleFileName = string.IsNullOrEmpty(SubtitleFile) ? m_strCurrentFile : SubtitleFile;

            if (streamName == null)
            {
                return Strings.Unknown;
            }

            if (!string.IsNullOrWhiteSpace(strSubtitleFileName))
            {
                //MPC-HC 2.0.0
                //"[Local] 4-3 bar test.English-Forced.srt\tEnglish"
                Regex regexMPCHC = new Regex(@"^\[([^\]]+)\]\s(?<file>[^\t]+)(\t(?<lng>.+))?");
                Match match = regexMPCHC.Match(streamName);
                if (match.Success)
                {
                    string strVideNoExt = Path.GetFileNameWithoutExtension(strSubtitleFileName);
                    string strSubNoExt = Path.GetFileNameWithoutExtension(match.Groups["file"].Value);
                    if (strSubNoExt.StartsWith(strVideNoExt, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (strSubNoExt.Length > strVideNoExt.Length)
                        {
                            //Subtitle filename has a suffix
                            var grLng = match.Groups["lng"];
                            if (grLng.Success)
                            {
                                //Try to extract additional suffix following the language
                                match = Regex.Match(strSubNoExt.Substring(strVideNoExt.Length), @"[-\._](?<lng>[A-Za-z]+)[-\._\[]+(?<suffix>.+?)\]?\z");
                                if (match.Success)
                                    return match.Groups["suffix"].Value;
                                else
                                    return string.Empty; //no additional suffix - just language
                            }

                            //Unknown subtitle filename suffix
                            return strSubNoExt.Substring(strVideNoExt.Length + 1);
                        }
                        else
                            return string.Empty;
                    }
                    else
                    {
                        //difference between m_strCurrentFile and ISubEngine.LoadSubtitles call
                        streamName = strSubNoExt;
                        streamNameFalse = streamName;
                        langName = streamName;
                    }
                }
            }

            // remove prefix, which is added by Haali Media Splitter
            streamName = Regex.Replace(streamName, @"^S: ", "");

            // Check if returned string contains both language and trackname info
            // For example Haali Media Splitter returns mkv streams as: "trackname [language]",
            // where "trackname" is stream's "trackname" property muxed in the mkv.
            Regex regex = new Regex(@"\[([^\]]+)\]");
            Regex regexFFDShow = new Regex(@"\s\[.+\]");
            Regex regexMPCEngine = new Regex(@"\((\D+[^\]]+)\)");
            Regex regexLAVF =
              new Regex(@"(?:S:\s)(?<lang_or_title>.+?)(?:\s*\[(?<lang>[^\]]*?)\])?(?:\s*\((?<info>[^\)]*?)\))?$");
            Match result = regex.Match(streamName);
            Match resultFFDShow = regexFFDShow.Match(streamName);
            Match resultMPCEngine = regexMPCEngine.Match(streamName);
            Match resultLAVF = regexLAVF.Match(streamNameFalse);
            if (resultFFDShow.Success)
            {
                //Get the trackname part by removing the language part from the string.
                streamName = regex.Replace(streamName, "").Trim();

                //Put things back together
                streamName = (streamName == string.Empty ? "" : "" + streamName + "");
            }
            else if (result.Success)
            {
                //if language only is detected -> set to ""
                streamName = "";
            }
            else if (resultMPCEngine.Success)
            // check for mpc-hc engine response format, e.g.: 
            // Language (Trackname)
            {
                //Get the trackname.
                streamName = resultMPCEngine.Groups[1].Value;
            }
            else if (resultLAVF.Success)
            // check for LAVF response format, e.g.: 
            // S: Title [Lang] (Info) when only Language in stream -> answer is S: Lang -> start to detect if [lang] is present if not replace Lang by "" 
            {
                string lang_or_title = resultLAVF.Groups[1].Value;
                string lang = resultLAVF.Groups[2].Value;
                string info = resultLAVF.Groups[3].Value;
                if (lang_or_title == langName || lang_or_title.Contains("Stream #") && string.IsNullOrEmpty(info))
                {
                    streamName = "";
                }
            }
            // mpeg splitter subtitle format
            Match m = Regex.Match(streamName, @"Subtitle\s+-\s+(?<1>.+?),", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                string language = MediaPortal.Util.Utils.TranslateLanguageString(m.Groups[1].Value);
                if (language.Length > 0)
                {
                    streamName = "";
                }
            }

            #region Remove the false detection of Language Name when is detected as Stream Name

            //Look if Language Name is equal Stream Name, if it's equal, remove it.
            if (streamName == langName)
            {
                streamName = "";
            }

            #endregion

            return streamName;
        }

    }
}
