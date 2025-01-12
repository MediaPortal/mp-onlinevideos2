using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace OnlineVideos
{
    public class PlaybackOptionsBuilder
    {
        #region Public types
        public enum VideoSelection
        {
            SD = 0,
            HD,
            FHD,
            UHD2K,
            UHD4k,
            UHD8k,

            Lowest,
            Highest,
        }

        public enum VideoResolution
        {
            SD = 0,
            HD,
            FHD,
            UHD2K,
            UHD4K,
            UHD8K,
        }

        public enum Container
        {
            Default = 0,
            AVI,
            DASH,
            HLS,
            WEBM,
            MKA,
            MKV,
            TS,
            MP4,
        }

        public enum VideoCodec
        {
            Default = 0,
            H264,
            HEVC,
            MPEG2,
            VP9,
            AV1 //AOMedia
        }

        public enum AudioCodec
        {
            Default = 0,
            AC3,
            DTS,
            AAC,
            FLAC,
            OPUS,
            MP3,
            MP4A,
            PCM
        }

        public class SelectionOptions : MarshalByRefObject
        {
            /// <summary>
            /// True to select best video resolution based on VideoSelection option.
            /// </summary>
            public bool AutomaticVideoSelection;

            /// <summary>
            /// Preferred video resolution.
            /// </summary>
            public VideoSelection VideoResolution;

            /// <summary>
            /// True to allow 3D video format.
            /// </summary>
            public bool Allow3D;

            /// <summary>
            /// True to allow HDR video format.
            /// </summary>
            public bool AllowHDR;

            /// <summary>
            /// List of preferred video codecs. Top codec in the list has highest priority.
            /// </summary>
            public VideoCodec[] PreferredVideoCodecs;

            /// <summary>
            /// List of preferred audio codecs. Top codec in the list has highest priority.
            /// </summary>
            public AudioCodec[] PreferredAudioCodecs;

            /// <summary>
            /// List of preferred containers. Top container in the list has highest priority.
            /// </summary>
            public Container[] PreferredContainers;

            public override object InitializeLifetimeService()
            {
                // In order to have the lease across appdomains live forever, we return null.
                return null;
            }
        };

        #endregion

        #region Private types
        private class VideoQuality
        {
            public string Url;
            public int Width;
            public int Height;
            public int Bitrate;
            public bool Is3D;
            public bool IsHDR;
            public bool IsVideoOnly;
            public VideoCodec Codec;
            public Container Container;

            public VideoQuality(string strUrl, int iWidth, int iHeight)
            {
                this.Url = strUrl;
                this.Width = iWidth;
                this.Height = iHeight;
            }

            public override string ToString()
            {
                return string.Format("{0}x{1}", this.Width, this.Height);
            }

        }

        private class AudioQuality
        {
            public string Url;
            public int Channels;
            public int Bitrate;
            public int SampleRate;
            public AudioCodec Codec;
            public Container Container;

            public AudioQuality(string strUrl)
            {
                this.Url = strUrl;
            }
        }

        private class AudioTrack
        {
            public bool IsDefault;
            public string Language;
            public string Description;


            private List<AudioQuality> _Qualities = new List<AudioQuality>();

            public void AddQuality(string strUrl, Container cont, AudioCodec codec, int iBitrate, int iChannels, int iSampleRate)
            {
                this._Qualities.Add(new AudioQuality(strUrl)
                {
                    Container = cont,
                    Codec = codec,
                    Bitrate = iBitrate,
                    Channels = iChannels,
                    SampleRate = iSampleRate
                });
            }

            public AudioQuality GetBest(SelectionOptions selection)
            {
                if (this._Qualities.Count > 0)
                {
                    this._Qualities.Sort((q1, q2) =>
                    {
                        int i;
                        if (selection.PreferredContainers != null)
                        {
                            i = compare(selection.PreferredContainers, (int)q1.Container, (int)q2.Container);
                            if (i != 0)
                                return i;
                        }

                        if (selection.PreferredAudioCodecs != null)
                        {
                            i = compare(selection.PreferredAudioCodecs, (int)q1.Codec, (int)q2.Codec);
                            if (i != 0)
                                return i;
                        }

                        i = q1.SampleRate.CompareTo(q2.SampleRate);
                        if (i != 0)
                            return i;

                        return q1.Bitrate.CompareTo(q2.Bitrate);
                    });

                    return this._Qualities[this._Qualities.Count - 1];
                }

                return null;
            }

            public int Count
            {
                get => this._Qualities.Count;
            }
        }

        private class VideoResolutionGroup
        {
            private List<VideoQuality> _Qualities = new List<VideoQuality>();

            public void AddQuality(VideoQuality quality)
            {
                this._Qualities.Add(quality);
            }

            public VideoQuality GetBest(SelectionOptions selection)
            {
                if (this._Qualities.Count > 0)
                {
                    this._Qualities.Sort((q1, q2) =>
                    {
                        int i;
                        if (selection.PreferredContainers != null)
                        {
                            i = compare(selection.PreferredContainers, (int)q1.Container, (int)q2.Container);
                            if (i != 0)
                                return i;
                        }

                        if (selection.PreferredVideoCodecs != null)
                        {
                            i = compare(selection.PreferredVideoCodecs, (int)q1.Codec, (int)q2.Codec);
                            if (i != 0)
                                return i;
                        }

                        return q1.Bitrate.CompareTo(q2.Bitrate);
                    });

                    for (int i = this._Qualities.Count - 1; i >= 0; i--)
                    {
                        VideoQuality q = this._Qualities[i];
                        if ((selection.Allow3D || !q.Is3D) && (selection.AllowHDR || !q.IsHDR))
                            return q;
                    }
                }

                return null;
            }

            public int Count
            {
                get => this._Qualities.Count;
            }

        }
        #endregion

        # region Private fields
        private Dictionary<VideoResolution, VideoResolutionGroup> _VideoResolutions;
        private Dictionary<string, AudioTrack> _AudioTracks;
        #endregion

        #region Public properties

        public void Clear()
        {
            this._VideoResolutions = null;
            this._AudioTracks = null;
        }

        public int Count
        {
            get
            {
                int iResult = 0;

                if (this._VideoResolutions != null)
                {
                    foreach (VideoResolutionGroup group in this._VideoResolutions.Values)
                        iResult += group.Count;
                }

                if (this._AudioTracks != null)
                {
                    foreach (AudioTrack track in this._AudioTracks.Values)
                        iResult += track.Count;
                }

                return iResult;
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Add video quality to the list.
        /// </summary>
        /// <param name="strUrl">URL link</param>
        /// <param name="iWidth">Video width resolution</param>
        /// <param name="iHeight">Video height resolution</param>
        public void AddVideoQuality(string strUrl, int iWidth, int iHeight)
        {
            this.AddVideoQuality(strUrl, false, null, null, iWidth, iHeight, 0, false, false);
        }

        /// <summary>
        /// Add video quality to the list.
        /// </summary>
        /// <param name="strUrl">URL link</param>
        /// <param name="bIsVideoOnly">True if video has no audio</param>
        /// <param name="strType">Container type like MP4, MKV, WEBM, etc.</param>
        /// <param name="strCodec">Codec type like H264, HEVC, etc.</param>
        /// <param name="iWidth">Video width resolution</param>
        /// <param name="iHeight">Video height resolution</param>
        /// <param name="iBitrate">Video bitrete</param>
        /// <param name="bIs3D">True if the video is 3D</param>
        /// <param name="bIsHDR">Truhe if the video is HDR</param>
        public void AddVideoQuality(string strUrl, bool bIsVideoOnly, string strType, string strCodec, int iWidth, int iHeight, int iBitrate, bool bIs3D, bool bIsHDR)
        {
            if (this._VideoResolutions == null)
                this._VideoResolutions = new Dictionary<VideoResolution, VideoResolutionGroup>();

            VideoResolution resolution;

            if (iWidth > 3840 || iHeight > 2160)
                resolution = VideoResolution.UHD8K;
            else if (iWidth > 2560 || iHeight > 1440)
                resolution = VideoResolution.UHD4K;
            else if (iWidth > 1920 || iHeight > 1080)
                resolution = VideoResolution.UHD2K;
            else if (iWidth > 1280 || iHeight > 720)
                resolution = VideoResolution.FHD;
            else if (iWidth > 1024 || iHeight > 576)
                resolution = VideoResolution.HD;
            else
                resolution = VideoResolution.SD;


            if (!this._VideoResolutions.TryGetValue(resolution, out VideoResolutionGroup group))
            {
                group = new VideoResolutionGroup();
                this._VideoResolutions.Add(resolution, group);
            }


            VideoQuality vq = new VideoQuality(strUrl, iWidth, iHeight)
            {
                IsVideoOnly = bIsVideoOnly,
                Container = parseContainer(strType),
                Codec = parseVideoCodec(strCodec),
                Bitrate = iBitrate,
                Is3D = bIs3D,
                IsHDR = bIsHDR
            };

            group.AddQuality(vq);

            Log.Debug("[AddVideoQuality] Group:{0} Width:{1} Height:{2} Container:{3} Codec:{4} 3D:{5} HDR:{6} Bitrate:{7}",
                resolution, vq.Width, vq.Height, vq.Container, vq.Codec, vq.Is3D, vq.IsHDR, vq.Bitrate);
        }

        /// <summary>
        /// Add explicit audio quality to the list
        /// </summary>
        /// <param name="strUrl">URL link</param>
        /// <param name="bIsDefault">True id the audio is default track.</param>
        /// <param name="strTrackID">Unique language track id. Identifies audio group. Leave null for default.</param>
        /// <param name="strLanguage">Audio track language like en, de, fr, etc.</param>
        /// <param name="strLanguageDescr">Optional audio track description</param>
        /// <param name="strType">Container type like MP4, MKV, WEBM, etc.</param>
        /// <param name="strCodec">Codec type like AAC, AC3, etc.</param>
        /// <param name="iBitrate">Audio bitrate,</param>
        /// <param name="iChannels">Nr. of channels.</param>
        /// <param name="iSampleRate">Audio sample rate.</param>
        public void AddAudioQuality(string strUrl, bool bIsDefault, string strTrackID, string strLanguage, string strLanguageDescr,
            string strType, string strCodec, int iBitrate, int iChannels, int iSampleRate)
        {
            if (string.IsNullOrWhiteSpace(strTrackID))
                strTrackID = "default";

            if (this._AudioTracks == null)
                this._AudioTracks = new Dictionary<string, AudioTrack>();

            if (!this._AudioTracks.TryGetValue(strTrackID, out AudioTrack track))
            {
                track = new AudioTrack()
                {
                    Language = strLanguage,
                    Description = strLanguageDescr,
                    IsDefault = bIsDefault
                };
                this._AudioTracks[strTrackID] = track;
            }

            Container cont = parseContainer(strType);
            AudioCodec cod = parseAudioCodec(strCodec);
            track.AddQuality(strUrl, cont, cod, iBitrate, iChannels, iSampleRate);

            Log.Debug("[AddAudioQuality] TrackID:{0} Channels:{1} SampleRate:{2} Container:{3} Codec:{4} Bitrate:{5}",
                strTrackID, iChannels, iSampleRate, cont, cod, iBitrate);
        }

        /// <summary>
        /// Get plabayck option list. Global OV settings is used to select video/audio quality.
        /// </summary>
        /// <param name="iPreselection">Returns index of preselected option(if more than one is returned).</param>
        /// <returns></returns>
        public Dictionary<string, string> GetPlaybackOptions(out int iPreselection)
        {
            return this.GetPlaybackOptions(OnlineVideoSettings.Instance.VideoQualitySelectionOptions, out iPreselection);
        }

        /// <summary>
        /// Get plabayck option list.
        /// </summary>
        /// <param name="selection">Quality selection options used to select video/audio quality</param>
        /// <param name="iPreselection">Returns index of preselected option(if more than one is returned).</param>
        /// <returns></returns>
        public Dictionary<string, string> GetPlaybackOptions(SelectionOptions selection, out int iPreselection)
        {
            iPreselection = -1;

            try
            {
                Dictionary<string, string> result = new Dictionary<string, string>();


                if (this._VideoResolutions != null && this._VideoResolutions.Count > 0)
                {
                    #region Build audiotracks
                    MixedUrl.AudioTrack[] audioTracks = null;
                    int iBitrateAudio = 0;
                    if (this._AudioTracks != null)
                    {
                        int iCnt = 0;
                        audioTracks = new MixedUrl.AudioTrack[this._AudioTracks.Count];
                        foreach (KeyValuePair<string, AudioTrack> pair in this._AudioTracks)
                        {
                            AudioTrack track = pair.Value;
                            AudioQuality audioQ = track.GetBest(selection);
                            audioTracks[iCnt++] = new MixedUrl.AudioTrack()
                            {
                                Url = audioQ.Url,
                                Language = track.Language,
                                Description = track.Description,
                                IsDefault = track.IsDefault
                            };

                            //Pick first bitrate
                            if (iBitrateAudio == 0 && audioQ.Bitrate > 0)
                                iBitrateAudio = audioQ.Bitrate;

                            Log.Debug("[GetPlaybackOptions] AudioTrackID:{0} Language:{1} Channels:{2} SampleRate:{3} Container:{4} Codec:{5} Bitrate:{6}",
                               pair.Key, track.Language, audioQ.Channels, audioQ.SampleRate, audioQ.Container, audioQ.Codec, audioQ.Bitrate);
                        }
                    }
                    #endregion

                    #region Build sorted quality list
                    List<KeyValuePair<VideoResolution, VideoQuality>> sorted = new List<KeyValuePair<VideoResolution, VideoQuality>>();
                    foreach (VideoResolution r in Enum.GetValues(typeof(VideoResolution)))
                    {
                        if (this._VideoResolutions.TryGetValue(r, out VideoResolutionGroup g))
                        {
                            VideoQuality q = g.GetBest(selection);
                            if (q != null)
                                sorted.Add(new KeyValuePair<VideoResolution, VideoQuality>(r, q));
                        }
                    }
                    #endregion

                    #region Build result based on selection arguments
                    if (!selection.AutomaticVideoSelection)
                    {
                        #region All playback options
                        int iBr;
                        CultureInfo ciEn = CultureInfo.GetCultureInfo("en-US");
                        StringBuilder sb = new StringBuilder(128);
                        for (int i = 0; i < sorted.Count; i++)
                        {
                            KeyValuePair<VideoResolution, VideoQuality> pair = sorted[i];
                            VideoQuality videoQ = pair.Value;

                            //Title
                            sb.Clear();
                            sb.Append(videoQ.Width);
                            sb.Append('x');
                            sb.Append(videoQ.Height);

                            iBr = videoQ.Bitrate + iBitrateAudio;
                            if (iBr > 0)
                            {
                                sb.Append('/');
                                printBitrate(iBr, ciEn, sb);
                            }

                            if (videoQ.Container > Container.Default)
                            {
                                sb.Append(" [");
                                sb.Append(videoQ.Container);
                                sb.Append(']');
                            }

                            if (videoQ.Codec > VideoCodec.Default)
                            {
                                sb.Append(" [");
                                sb.Append(videoQ.Codec);
                                sb.Append(']');
                            }

                            if (videoQ.Is3D)
                                sb.Append("[3D]");

                            if (videoQ.IsHDR)
                                sb.Append("[HDR]");

                            //Add option to the playback list
                            result.Add(sb.ToString(),
                                !videoQ.IsVideoOnly ? videoQ.Url : (audioTracks != null ? new MixedUrl(videoQ.Url, audioTracks).ToString() : videoQ.Url));

                            //Preselection
                            if (iPreselection < 0 || (int)sorted[i].Key <= (selection.VideoResolution - VideoSelection.SD))
                                iPreselection = i;

                            Log.Debug("[GetPlaybackOptions] Option:{0} Width:{1} Height:{2} Container:{3} Codec:{4} 3D:{5} HDR:{6} Bitrate:{7}",
                             pair.Key, videoQ.Width, videoQ.Height, videoQ.Container, videoQ.Codec, videoQ.Is3D, videoQ.IsHDR, videoQ.Bitrate);
                        }
                        #endregion
                    }
                    else
                    {
                        #region Single playback option
                        VideoQuality videoQ = null;

                        switch (selection.VideoResolution)
                        {
                            case VideoSelection.Lowest:
                                videoQ = sorted[0].Value;
                                break;

                            case VideoSelection.Highest:
                                videoQ = sorted[sorted.Count - 1].Value;
                                break;

                            default:
                                for (int i = 0; i < sorted.Count; i++)
                                {
                                    if (videoQ == null || (int)sorted[i].Key <= (selection.VideoResolution - VideoSelection.SD))
                                        videoQ = sorted[i].Value;
                                    else
                                        break;
                                }
                                break;
                        }

                        if (videoQ.IsVideoOnly)
                            result.Add(string.Empty, audioTracks != null ? new MixedUrl(videoQ.Url, audioTracks).ToString() : videoQ.Url);
                        else
                            result.Add(string.Empty, videoQ.Url);

                        Log.Debug("[GetPlaybackOptions] Option:{0} Width:{1} Height:{2} Container:{3} Codec:{4} 3D:{5} HDR:{6} Bitrate:{7}",
                             selection.VideoResolution, videoQ.Width, videoQ.Height, videoQ.Container, videoQ.Codec, videoQ.Is3D, videoQ.IsHDR, videoQ.Bitrate);

                        iPreselection = 0;

                        #endregion
                    }
                    #endregion
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error("[GetPlaybackOptions] Error: {0}", ex);
                return null;
            }
        }

        #endregion

        #region Private methods
        private static Container parseContainer(string strType)
        {
            Container result = Container.Default;

            if (!string.IsNullOrEmpty(strType))
            {
                string strResult = Enum.GetNames(typeof(Container)).FirstOrDefault(strName => strType.IndexOf(strName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (strResult != null)
                    return (Container)Enum.Parse(typeof(Container), strResult);
            }

            return result;
        }

        private static VideoCodec parseVideoCodec(string strType)
        {
            VideoCodec result = VideoCodec.Default;

            if (!string.IsNullOrEmpty(strType))
            {
                string strResult = Enum.GetNames(typeof(VideoCodec)).FirstOrDefault(strName => strType.IndexOf(strName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (strResult != null)
                    return (VideoCodec)Enum.Parse(typeof(VideoCodec), strResult);
                else if (strType.IndexOf("vp09", StringComparison.OrdinalIgnoreCase) >= 0)
                    return VideoCodec.VP9;
                else if (strType.IndexOf("avc1", StringComparison.OrdinalIgnoreCase) >= 0)
                    return VideoCodec.H264;
                else if (strType.IndexOf("av01", StringComparison.OrdinalIgnoreCase) >= 0)
                    return VideoCodec.AV1;
            }

            return result;

        }

        private static AudioCodec parseAudioCodec(string strType)
        {
            AudioCodec result = AudioCodec.Default;

            if (!string.IsNullOrEmpty(strType))
            {
                string strResult = Enum.GetNames(typeof(AudioCodec)).FirstOrDefault(strName => strType.IndexOf(strName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (strResult != null)
                    return (AudioCodec)Enum.Parse(typeof(AudioCodec), strResult);
            }

            return result;

        }

        private static void printBitrate(int iBr, CultureInfo ci, StringBuilder sb)
        {
            if (iBr >= 1000000)
            {
                sb.Append((iBr / 1000000f).ToString("0.0", ci));
                sb.Append("mb");
            }
            else if (iBr >= 1000)
            {
                sb.Append((iBr / 1000f).ToString("0.0", ci));
                sb.Append("kb");
            }
            else
            {
                sb.Append(iBr.ToString());
                sb.Append('b');
            }
        }

        private static int compare(Array preferredList, int iItemA, int iItemB)
        {
            int iResultA = Array.FindIndex((int[])preferredList, item => item == iItemA);
            int iResultB = Array.FindIndex((int[])preferredList, item => item == iItemB);

            if (iResultA < 0)
                iResultA = int.MaxValue;

            if (iResultB < 0)
                iResultB = int.MaxValue;

            return iResultB.CompareTo(iResultA);
        }
        #endregion
    }
}
