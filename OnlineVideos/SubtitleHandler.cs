﻿using SubtitleDownloader.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Threading;

namespace OnlineVideos.Subtitles
{
    /// <summary>
    /// Helper class to easily incorporate subtitles into a siteutil that supports ITrackingInfo
    /// Class needs the SubtitleDownloader http://www.team-mediaportal.com/extensions/movies-videos/subtitledownloader plugin
    /// </summary>
    public class SubtitleHandler
    {
        // keep sdObject an object, so that the constructor doesn't throw an ecxeption is subtitledownloader isn't installed
        private object sdObject = null;
        private bool tryLoadSubtitles = false;

        private System.Threading.Thread subtitleThread = null;


        //smallest value has the highest prio
        private Dictionary<string, int> languagePrios = null;
        public delegate ITrackingInfo GetTrackingInfo(VideoInfo video);

        public SubtitleHandler(string className, string languages)
        {
            Log.Debug(String.Format("Create subtitlehandler for '{0}', languages '{1}'", className, languages));

            className = className.Trim();
            tryLoadSubtitles = !String.IsNullOrEmpty(className);

            try
            {
                if (tryLoadSubtitles)
                {
                    languagePrios = new Dictionary<string, int>();
                    string[] langs = languages.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < langs.Length; i++)
                        languagePrios.Add(langs[i], i);

                    tryLoadSubtitles = tryLoad(className);
                }
                else
                    Log.Debug("SubtitleDownloader: classname empty");
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException)
                    Log.Debug("SubtitleDownloader not installed");
                else
                    Log.Warn("Exception while creating SubtitleDownloader " + e.ToString());
                tryLoadSubtitles = false;
            }
        }

        private void SafeSetSubtitleText(VideoInfo video)
        {
            try
            {
                setSubtitleText(video);
            }
            catch (Exception e)
            {
                Log.Warn("SubtitleDownloader: " + e.ToString());
            }
        }

        public void SetSubtitleText(VideoInfo video, bool threaded = false)
        {
            subtitleThread = null;
            if (tryLoadSubtitles)
            {
                if (sdObject != null && video.TrackingInfo != null && String.IsNullOrEmpty(video.SubtitleText))
                    if (threaded)
                    {
                        subtitleThread = new Thread(
                            delegate ()
                            {
                                SafeSetSubtitleText(video);
                            });
                        subtitleThread.Start();
                    }
                    else
                        SafeSetSubtitleText(video);
            }
        }

        public void WaitForSubtitleCompleted()
        {
            if (subtitleThread != null)
                subtitleThread.Join();
        }


        [Obsolete("Use the SetSubtitleText with threaded=true")]
        public void SetSubtitleText(VideoInfo video, out Thread thread)
        {
            thread = null;
            if (tryLoadSubtitles)
            {
                if (sdObject != null && video.TrackingInfo != null && String.IsNullOrEmpty(video.SubtitleText))
                {
                    thread = new Thread(
                        delegate ()
                        {
                            SafeSetSubtitleText(video);
                        });
                    thread.Start();
                }
            }
        }

        [Obsolete("Use the SetSubtitleText with threaded=true, and WaitForSubtitleCompleted without parameters")]
        public void WaitForSubtitleCompleted(Thread thread)
        {
            if (thread != null)
                thread.Join();
        }


        // keep all references to subtitledownloader in separate methods, so that methods that are called from siteutil don't throw an ecxeption
        private bool tryLoad(string className)
        {
            Assembly subAssembly = Assembly.GetAssembly(typeof(ISubtitleDownloader));
            Type tt = subAssembly.GetType(String.Format("SubtitleDownloader.Implementations.{0}.{0}Downloader", className), false, true);
            if (tt == null)
            {
                Log.Debug("Subtitlehandler for " + className + " cannot be created");
                return false;
            }
            else
            {
                sdObject = (ISubtitleDownloader)Activator.CreateInstance(tt);
                Log.Debug("Subtitlehandler " + sdObject.ToString() + " successfully created");
                return true;
            }
        }

        private void setSubtitleText(VideoInfo video)
        {
            ISubtitleDownloader sd = (ISubtitleDownloader)sdObject;
            List<Subtitle> results;
            var it = video.TrackingInfo;
            if (it.VideoKind == VideoKind.Movie)
            {
                SearchQuery qu = new SearchQuery(it.Title);
                qu.Year = (int)it.Year;
                qu.LanguageCodes = languagePrios.Keys.ToArray();
                results = sd.SearchSubtitles(qu);
            }
            else
            {
                EpisodeSearchQuery qu = new EpisodeSearchQuery(it.Title, (int)it.Season, (int)it.Episode, it.ID_IMDB);
                qu.LanguageCodes = languagePrios.Keys.ToArray();
                results = sd.SearchSubtitles(qu);
            }
            Log.Debug("Subtitles found:" + results.Count.ToString());
            if (results.Count > 0)
            {
                int minValue = int.MaxValue;
                Subtitle minSub = results[0];

                foreach (Subtitle sub in results)
                {
                    Log.Debug("Subtitle " + sub.ProgramName + " " + sub.LanguageCode);
                    if (languagePrios.ContainsKey(sub.LanguageCode))
                    {
                        int prio = languagePrios[sub.LanguageCode];
                        if (prio < minValue)
                        {
                            minValue = prio;
                            minSub = sub;
                        }
                    }
                }

                List<FileInfo> subtitleFiles = sd.SaveSubtitle(minSub);
                if (subtitleFiles.Count > 0)
                {
                    string s = File.ReadAllText(subtitleFiles[0].FullName, System.Text.Encoding.UTF8);
                    if (s.IndexOf('�') != -1)
                        video.SubtitleText = File.ReadAllText(subtitleFiles[0].FullName, System.Text.Encoding.Default);
                    else
                        video.SubtitleText = s;
                }
                foreach (FileInfo fi in subtitleFiles)
                    fi.Delete();
            }
        }
    }

    /// <summary>
    /// Represents class for subtitle source converter.
    /// </summary>
    public class SubtitleSourceConverter : StringConverter
    {
        #region Methods

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            List<String> sources = new List<String>();

            Assembly subAssembly = Assembly.GetAssembly(typeof(ISubtitleDownloader));
            Type ISubType = typeof(ISubtitleDownloader);
            foreach (Type t in subAssembly.GetTypes())
                if (t != ISubType && ISubType.IsAssignableFrom(t) && t.Name.EndsWith("Downloader"))
                    sources.Add(t.Name.Substring(0, t.Name.Length - 10));
            return new StandardValuesCollection(sources);
        }

        #endregion
    }

}
