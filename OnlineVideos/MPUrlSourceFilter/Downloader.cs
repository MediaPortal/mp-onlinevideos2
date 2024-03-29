﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OnlineVideos.Downloading;

namespace OnlineVideos.MPUrlSourceFilter
{
    /// <summary>
    /// Represents class for downloading single stream with MediaPortal Url Source Splitter.
    /// </summary>
    public class Downloader : MarshalByRefObject, IDownloader, IDownloadCallback
    {
        #region Private fields

        System.Threading.Thread downloadThread;
        private volatile Boolean downloadFinished;
        private int downloadResult;
        private volatile Boolean cancelled;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Downloader"/> class.
        /// </summary>
        public Downloader()
        {
            this.downloadFinished = false;
            this.downloadResult = 0;
            this.cancelled = false;
        }

        #endregion

        #region Properties

        public Boolean Cancelled { get { return this.cancelled; } }

        #endregion

        #region Methods

        public void Abort()
        {
            if (downloadThread != null)
            {
                this.cancelled = true;

                while (!this.downloadFinished)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void CancelAsync()
        {
            this.cancelled = true;
        }

        public Exception Download(DownloadInfo downloadInfo)
        {
            IDownload sourceFilter = null;

            try
            {
                downloadThread = System.Threading.Thread.CurrentThread;
                this.downloadResult = 0;
                this.downloadFinished = false;
                this.cancelled = false;

                sourceFilter = (IDownload)new MPUrlSourceSplitter();
                String url = UrlBuilder.GetFilterUrl(downloadInfo.Util, downloadInfo.Url, true);

                IDownload downloadFilter = (IDownload)sourceFilter;
                int result = downloadFilter.DownloadAsync(url, downloadInfo.LocalFile, this);
                // throw exception if error occured while initializing download
                Marshal.ThrowExceptionForHR(result);

                while (!this.downloadFinished)
                {
                    long total = 0;
                    long current = 0;
                    if (downloadFilter.QueryProgress(out total, out current) >= 0)
                    {
                        // succeeded or estimated value
                        downloadInfo.DownloadProgressCallback(total, current);
                    }

                    // sleep some time
                    System.Threading.Thread.Sleep(100);

                    if (this.cancelled)
                    {
                        downloadFilter.AbortOperation();
                        this.downloadFinished = true;
                        this.downloadResult = 0;
                    }
                }

                // throw exception if error occured while downloading
                Marshal.ThrowExceptionForHR(this.downloadResult);

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
            finally
            {
                if (sourceFilter != null)
                {
                    Marshal.ReleaseComObject(sourceFilter);
                }
            }
        }

        internal void OnDownloadCallback(int downloadResult)
        {
            this.downloadResult = downloadResult;
            this.downloadFinished = true;
        }

        public static void ClearDownloadCache()
        {
            String path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Team MediaPortal\\MPUrlSourceSplitter");
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    try { File.Delete(file); }
                    catch { }
                }
            }
        }

        #endregion

        #region Constants

        public const string FilterName = "MediaPortal Url Source Splitter";
        public const string FilterCLSID = "59ED045A-A938-4A09-A8A6-8231F5834259";

        #endregion

        #region Internals

        /// <summary>
        /// Defines MediaPortal Url Source Splitter.
        /// </summary>
        [ComImport, Guid(FilterCLSID)]
        private class MPUrlSourceSplitter { };

        #endregion

        #region IDownloadCallback interface

        void IDownloadCallback.OnDownloadCallback(int downloadResult)
        {
            this.OnDownloadCallback(downloadResult);
        }

        #endregion

        #region MarshalByRefObject overrides

        public override object InitializeLifetimeService()
        {
            // In order to have the lease across appdomains live forever, we return null.
            return null;
        }

        #endregion
    }
}
