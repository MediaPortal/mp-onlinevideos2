using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using OnlineVideos.MPUrlSourceFilter;

namespace OnlineVideos.Downloading
{
    public class OnlineVideosDownloader : MarshalByRefObject, IDownloader
    {
        private class DownloadTask: IDownloadCallback
        {
            /// <summary>
            /// Defines MediaPortal Url Source Splitter.
            /// </summary>
            [ComImport, Guid(Downloader.FilterCLSID)]
            private class MPUrlSourceSplitter { };

            public string FilePath;
            public string Url;
            public long FileSize = 0;
            public long CurrentRead = 0;
            public DownloadInfo DownloadInfo;
            public ManualResetEvent Complete = new ManualResetEvent(false);
            public volatile bool Cancelled = false;
            public Exception Result = null;
            public EventHandler Callback;

                                        
            private int _DownloadResult = 0;
            private volatile bool _DownloadFinished = false;

            public void Download()
            {
                new Thread(new ThreadStart(() =>
                    {
                        //HttpWebResponse response = null;
                        IDownload sourceFilter = null;

                        try
                        {
                            this._DownloadResult = 0;
                            this._DownloadFinished = false;

                            sourceFilter = (IDownload)new MPUrlSourceSplitter();
                            IDownload downloadFilter = (IDownload)sourceFilter;
                            int iResult = downloadFilter.DownloadAsync(this.Url, this.FilePath, this);
                            // throw exception if error occured while initializing download
                            Marshal.ThrowExceptionForHR(iResult);

                            while (!this._DownloadFinished)
                            {
                                if (downloadFilter.QueryProgress(out long lTotal, out long lCurrent) >= 0)
                                {
                                    // succeeded or estimated value
                                    this.FileSize = lTotal;
                                    this.CurrentRead = lCurrent;
                                    this.Callback(this, EventArgs.Empty);
                                }

                                // sleep some time
                                Thread.Sleep(100);

                                if (this.Cancelled)
                                {
                                    downloadFilter.AbortOperation();
                                    this._DownloadFinished = true;
                                    this._DownloadResult = 0;
                                }
                            }

                            // throw exception if error occured while downloading
                            Marshal.ThrowExceptionForHR(this._DownloadResult);
                        }
                        catch (Exception ex)
                        {
                            this.Result = ex;
                        }
                        finally
                        {
                            if (sourceFilter != null)
                                Marshal.ReleaseComObject(sourceFilter);

                            this.Complete.Set();
                        }

                        return;

                        //try
                        //{

                        //    using (FileStream fs = new FileStream(this.FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        //    {
                        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Url);
                        //        request.Timeout = 15000;
                        //        request.UserAgent = OnlineVideoSettings.Instance.UserAgent;
                        //        request.Accept = "*/*";
                        //        request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                        //        response = (HttpWebResponse)request.GetResponse();

                        //        Stream responseStream;
                        //        if (response.ContentEncoding.ToLower().Contains("gzip"))
                        //            responseStream = new System.IO.Compression.GZipStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                        //        else if (response.ContentEncoding.ToLower().Contains("deflate"))
                        //            responseStream = new System.IO.Compression.DeflateStream(response.GetResponseStream(), System.IO.Compression.CompressionMode.Decompress);
                        //        else
                        //            responseStream = response.GetResponseStream();

                        //        this.FileSize = response.ContentLength;
                        //        this.ContentType = response.ContentType;

                        //        const int BUFFER_SIZE = 1024 * 16;
                        //        byte[] buffer = new byte[BUFFER_SIZE];
                        //        this.CurrentRead = 0;
                        //        long lRead;
                        //        do
                        //        {
                        //            //Read data from response stream
                        //            lRead = responseStream.Read(buffer, 0, BUFFER_SIZE);

                        //            //Write received data to the filestream
                        //            fs.Write(buffer, 0, (int)lRead);

                        //            //Report
                        //            this.CurrentRead += lRead;
                        //            this.Callback(this, EventArgs.Empty);
                        //        }
                        //        while (lRead > 0 && !this.Cancelled);

                        //        fs.Flush();
                        //        fs.Close();

                        //        return;
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    this.Result = ex;
                        //}
                        //finally
                        //{
                        //    if (response != null)
                        //    {
                        //        try { response.Close(); }
                        //        catch { }
                        //    }

                        //    this.Complete.Set();
                        //}
                    })).Start();
            }

            void IDownloadCallback.OnDownloadCallback(int downloadResult)
            {
                this._DownloadResult = downloadResult;
                this._DownloadFinished = true;
            }
        }

        private volatile bool _Cancelled;

        private DownloadTask _TaskVideo;
        private DownloadTask _TaskAudio;

        private DateTime _TsReport = DateTime.MinValue;

        public bool Cancelled => this._Cancelled;

        #region IDownloader
        public void Abort()
        {
            this._Cancelled = true;

            if (this._TaskVideo != null)
            {
                this._TaskVideo.Cancelled = true;
                this._TaskVideo.Complete.WaitOne();
            }

            if (this._TaskAudio != null)
            {
                this._TaskAudio.Cancelled = true;
                this._TaskAudio.Complete.WaitOne();
            }
        }

        public void CancelAsync()
        {
            this._Cancelled = true;

            if (this._TaskVideo != null)
                this._TaskVideo.Cancelled = true;

            if (this._TaskAudio != null)
                this._TaskAudio.Cancelled = true;
        }

        public Exception Download(DownloadInfo downloadInfo)
        {
            try
            {
                this._Cancelled = false;

                MixedUrl url = new MixedUrl(downloadInfo.Url);
                if (url.Valid && Uri.IsWellFormedUriString(url.VideoUrl, UriKind.Absolute)
                        && Uri.IsWellFormedUriString(url.AudioUrl, UriKind.Absolute))
                {
                    this._TaskVideo = new DownloadTask()
                    {
                        Url = url.VideoUrl,
                        FilePath = downloadInfo.LocalFile + ".video",
                        Callback = this.cbTask,
                        DownloadInfo = downloadInfo
                    };

                    this._TaskAudio = new DownloadTask()
                    {
                        Url = url.AudioUrl,
                        FilePath = downloadInfo.LocalFile + ".audio",
                        Callback = this.cbTask,
                        DownloadInfo = downloadInfo
                    };

                    //Start download tasks
                    this._TaskVideo.Download();
                    this._TaskAudio.Download();

                    //Wait for finish
                    this._TaskVideo.Complete.WaitOne();
                    this._TaskAudio.Complete.WaitOne();

                    if (this._Cancelled)
                        return null;

                    if (this._TaskVideo.Result == null && this._TaskVideo.FileSize > 0
                        && this._TaskAudio.Result == null && this._TaskAudio.FileSize > 0)
                    {
                        string strArgs = string.Format(" -i \"{0}\" -i \"{1}\" -c copy \"{2}\"",
                            this._TaskVideo.FilePath,
                            this._TaskAudio.FilePath,
                            downloadInfo.LocalFile
                            );

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "MovieThumbnailer\\ffmpeg.exe",
                            Arguments = strArgs
                        };

                        Process proc = new Process()
                        {
                            StartInfo = psi
                        };

                        //Start ffmpeg process
                        proc.Start();

                        //Wait for ffmpeg exit
                        proc.WaitForExit();

                        if (!File.Exists(downloadInfo.LocalFile))
                            return new Exception("[OnlineVideosDownloader] FFmpeg merge failed.");

                        //Final callback
                        downloadInfo.DownloadProgressCallback(100);

                        return null;
                    }
                    else
                    {
                        //Download failed

                        if (this._TaskVideo.Result != null)
                            Log.Error("[OnlineVideosDownloader][Download] Error Video: {0} {1} {2}",
                                this._TaskVideo.Result.Message,
                                this._TaskVideo.Result.Source,
                                this._TaskVideo.Result.StackTrace);

                        if (this._TaskAudio.Result != null)
                            Log.Error("[OnlineVideosDownloader][Download] Error Audio: {0} {1} {2}",
                                this._TaskAudio.Result.Message,
                                this._TaskAudio.Result.Source,
                                this._TaskAudio.Result.StackTrace);

                        return new Exception("[OnlineVideosDownloader] Download failed.");
                    }
                }

                return new Exception("Unsupported url: " + downloadInfo.Url);
            }
            catch (Exception ex)
            {
                if (!this._Cancelled)
                {
                    Log.Error("[OnlineVideosDownloader][Download] Error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
                    return ex;
                }
                else
                    return null;
            }
            finally
            {
                //Delete downloaded files (if exists)

                if (this._TaskVideo != null && File.Exists(this._TaskVideo.FilePath))
                    try { File.Delete(this._TaskVideo.FilePath); }
                    catch { }

                if (this._TaskAudio != null && File.Exists(this._TaskAudio.FilePath))
                    try { File.Delete(this._TaskAudio.FilePath); }
                    catch { }
            }
        }

        #endregion

        #region MarshalByRefObject overrides
        public override object InitializeLifetimeService()
        {
            // In order to have the lease across appdomains live forever, we return null.
            return null;
        }
        #endregion

        /// <summary>
        /// Check whether the url can be handled by downloader
        /// </summary>
        /// <param name="downloadInfo"></param>
        /// <returns></returns>
        public static bool CanHandleUrl(DownloadInfo downloadInfo)
        {
            if (downloadInfo == null || string.IsNullOrWhiteSpace(downloadInfo.Url))
                return false;

            MixedUrl url = new MixedUrl(downloadInfo.Url);
            return url.Valid && Uri.IsWellFormedUriString(url.VideoUrl, UriKind.Absolute)
                        && Uri.IsWellFormedUriString(url.AudioUrl, UriKind.Absolute);
        }


        /// <summary>
        /// Callback from download tasks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTask(object sender, EventArgs e)
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    //Half second inerval is fine
                    if ((DateTime.Now - this._TsReport).TotalMilliseconds > 500)
                    {
                        DownloadTask task = (DownloadTask)sender;

                        task.DownloadInfo.DownloadProgressCallback(
                            this._TaskVideo.FileSize + this._TaskAudio.FileSize,
                            this._TaskVideo.CurrentRead + this._TaskAudio.CurrentRead
                            );

                        this._TsReport = DateTime.Now;
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
    }
}
