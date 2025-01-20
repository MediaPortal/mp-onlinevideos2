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

        private List<DownloadTask> _Tasks;

        private DateTime _TsReport = DateTime.MinValue;

        public bool Cancelled => this._Cancelled;

        #region IDownloader
        public void Abort()
        {
            this._Cancelled = true;

            if (this._Tasks != null)
            {
                this._Tasks.ForEach(t => t.Cancelled = true);
                this._Tasks.ForEach(t => t.Complete.WaitOne());
            }
        }

        public void CancelAsync()
        {
            this._Cancelled = true;

            if (this._Tasks != null)
                this._Tasks.ForEach(t => t.Cancelled = true);
        }

        public Exception Download(DownloadInfo downloadInfo)
        {
            try
            {
                this._Cancelled = false;

                MixedUrl url = new MixedUrl(downloadInfo.Url);
                if (url.Valid && Uri.IsWellFormedUriString(url.VideoUrl, UriKind.Absolute)
                        && url.AudioTracks?.Length > 0)
                {
                    this._Tasks = new List<DownloadTask>();

                    this._Tasks.Add(new DownloadTask()
                    {
                        Url = url.VideoUrl,
                        FilePath = downloadInfo.LocalFile + ".video",
                        Callback = this.cbTask,
                        DownloadInfo = downloadInfo
                    });

                    for (int i = 0; i < url.AudioTracks.Length; i++)
                    {
                        this._Tasks.Add(new DownloadTask()
                        {
                            Url = url.AudioTracks[i].Url,
                            FilePath = downloadInfo.LocalFile + ".audio_" + i,
                            Callback = this.cbTask,
                            DownloadInfo = downloadInfo
                        });
                    }

                    //Start download tasks
                    this._Tasks.ForEach(t => t.Download());

                    //Wait for finish
                    this._Tasks.ForEach(t => t.Complete.WaitOne());

                    if (this._Cancelled)
                        return null;

                    //Merge all files
                    if (this._Tasks.All(t => t.Result == null && t.FileSize > 0))
                    {
                        //Build ffmpeg arguments
                        StringBuilder sbArgs = new StringBuilder(1024);

                        //Input files
                        this._Tasks.ForEach(t =>
                        {
                            sbArgs.Append(" -i \"");
                            sbArgs.Append(t.FilePath);
                            sbArgs.Append('\"');
                        });

                        //Mapping
                        for (int i = 0; i < this._Tasks.Count; i++)
                        {
                            sbArgs.Append(" -map ");
                            sbArgs.Append(i);
                        }

                        //Language tags
                        CultureInfo ciTrack = null;
                        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                        for (int i = 0; i < url.AudioTracks.Length; i++)
                        {
                            string strLanguage = url.AudioTracks[i].Language;

                            if (!string.IsNullOrWhiteSpace(strLanguage))
                            {
                                if (strLanguage.Length >= 5 && strLanguage[2] == '-')
                                    strLanguage = strLanguage.Substring(0, 2);

                                ciTrack = cultures.FirstOrDefault(ci => ci.Name.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
                                    || ci.ThreeLetterISOLanguageName.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
                                    || ci.EnglishName.Equals(strLanguage, StringComparison.OrdinalIgnoreCase)
                                    );

                                //-metadata:s:a:0 language=...
                                if (ciTrack != null)
                                {
                                    sbArgs.Append(" -metadata:s:a:");
                                    sbArgs.Append(i);
                                    sbArgs.Append(" language=");
                                    sbArgs.Append(ciTrack.ThreeLetterISOLanguageName);
                                }
                            }
                        }

                        //Destination file
                        sbArgs.Append(" -strict -2 -c copy \"");
                        sbArgs.Append(downloadInfo.LocalFile);
                        sbArgs.Append('\"');

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = "MovieThumbnailer\\ffmpeg.exe",
                            Arguments = sbArgs.ToString()
                        };

                        Log.Debug("[Download] ffmpeg: {0}", psi.Arguments);

                        Process proc = new Process()
                        {
                            StartInfo = psi
                        };

                        //Start ffmpeg process
                        proc.Start();

                        //Wait for ffmpeg exit
                        proc.WaitForExit();

                        if (!File.Exists(downloadInfo.LocalFile) || new FileInfo(downloadInfo.LocalFile).Length <= 0)
                            return new Exception("[OnlineVideosDownloader] FFmpeg merge failed.");

                        //Final callback
                        downloadInfo.DownloadProgressCallback(100);

                        return null;
                    }
                    else
                    {
                        //Download failed
                        this._Tasks.ForEach(t =>
                        {
                            if (t.Result != null)
                                Log.Error("[OnlineVideosDownloader][Download] Error Audio: {0} {1} {2}",
                                    t.Result.Message,
                                    t.Result.Source,
                                    t.Result.StackTrace);
                        });

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
                if (this._Tasks != null)
                {
                    this._Tasks.ForEach(t =>
                    {
                        if (File.Exists(t.FilePath))
                        {
                            try { File.Delete(t.FilePath); }
                            catch { }
                        }
                    });
                }
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
                        && url.AudioTracks?.Length > 0;
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
                            this._Tasks.Sum(t => t.FileSize),
                            this._Tasks.Sum(t => t.CurrentRead)
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
