using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using OnlineVideos.Helpers;
using Newtonsoft.Json.Linq;

namespace OnlineVideos.Hoster
{
    public class Youtube : HosterBase, ISubtitle
    {
        private enum PlayerStatusEnum
        {
            OK = 0,
            InvalidLink,
            Error,
        }

        [Category("OnlineVideosUserConfiguration"), Description("Select subtitle language preferences (; separated and ISO 3166-2?), for example: en;de")]
        protected string subtitleLanguages = "";

        private SubtitleList subtitleTexts = null;

        public override string GetHosterUrl()
        {
            return "Youtube.com";
        }

        [Category("OnlineVideosUserConfiguration"), Description("Don't show the 3D formats that youtube offers on some clips."), LocalizableDisplayName("Hide 3D Formats")]
        protected bool hide3DFormats = true;
        [Category("OnlineVideosUserConfiguration"), Description("Don't show the 3gpp formats that youtube offers on some clips."), LocalizableDisplayName("Hide Mobile Formats")]
        protected bool hideMobileFormats = true;

        #region itagCode ContainerContent Resolution Bitrate RangeVR 3D
        //5	    flv audio/video	    240p	-	    -	-
        //6  	flv audio/video	    270p	-	    -	-
        //17	3gp audio/video	    144p	-	    -	-
        //18	mp4 audio/video	    360p	-	    -	-
        //22	mp4 audio/video	    720p	-	    -	-
        //34	flv audio/video	    360p	-	    -	-
        //35	flv audio/video	    480p	-	    -	-
        //36	3gp audio/video 	180p	-	    -	-
        //37	mp4 audio/video	    1080p	-	    -	-
        //38	mp4 audio/video	    3072p	-	    -	-
        //43	webm audio/video	360p	-	    -	-
        //44	webm audio/video	480p	-	    -	-
        //45	webm audio/video	720p	-	    -	-
        //46	webm audio/video	1080p	-	    -	-
        //82	mp4 audio/video	    360p	-	    -	3D
        //83	mp4 audio/video	    480p	-	    -	3D
        //84	mp4 audio/video 	720p	-	    -	3D
        //85	mp4 audio/video	    1080p	-	    -	3D
        //92	hls audio/video	    240p	-	    -	3D
        //93	hls audio/video	    360p	-	    -	3D
        //94	hls audio/video	    480p	-	    -	3D
        //95	hls audio/video	    720p	-   	-	3D
        //96	hls audio/video	    1080p	-	    -	-
        //100	webm audio/video	360p	-	    -	3D
        //101	webm audio/video	480p	-	    -	3D
        //102	webm audio/video	720p	-	    -	3D
        //132	hls audio/video	    240p	-	    -	
        //133	mp4 video	        240p	-	    -	
        //134	mp4 video	        360p	-	    -	
        //135	mp4 video	        480p	-   	-	
        //136	mp4 video       	720p	-   	-	
        //137	mp4 video           1080p	-	    -	
        //138	mp4 video	        2160p60	-	    -	
        //139	m4a audio	        -	    48k	    -	
        //140	m4a audio	        -	    128k	-	
        //141	m4a audio	        -	    256k	-	
        //151	hls audio/video	    72p	    -	    -	
        //160	mp4 video	        144p	-	    -	
        //167	webm video	        360p	-	    -	
        //168	webm video      	480p	-	    -	
        //169	webm video	        1080p	-	    -	
        //171	webm audio	        -	    128k	-	
        //218	webm video	        480p	-	    -	
        //219	webm video	        144p	-	    -	
        //233   hls audio           -       50k
        //234   hls audio           -       133k
        //242	webm video      	240p	-	    -	
        //243	webm video	        360p	-	    -	
        //244	webm video	        480p	-	    -	
        //245	webm video	        480p	-	    -	
        //246	webm video	        480p	-	    -	
        //247	webm video	        720p	-	    -	
        //248	webm video	        1080p	-	    -	
        //249	webm audio	        -	    50k	    -	
        //250	webm audio	        -	    70k	    -	
        //251	webm audio	        -	    160k	-	
        //264	mp4 video	        1440p	-	    -	
        //266	mp4 video	        2160p60	-	    -	
        //271	webm video      	1440p	-	    -	
        //272	webm video      	4320p	-	    -	
        //278	webm video	        144p	-	    -	
        //298	mp4 video	        720p60	-	    -	
        //299	mp4 video	        1080p60	-	    -	
        //302	webm video	        720p60	-	    -	
        //303	webm video	        1080p60	-	    -	
        //308	webm video	        1440p60	-	    -	
        //313	webm video	        2160p	-	    -	
        //315	webm video	        2160p60	-	    -	
        //330	webm video	        144p60	-	    hdr	
        //331	webm video	        240p60	-	    hdr	
        //332	webm video	        360p60	-	    hdr	
        //333	webm video	        480p60	-	    hdr	
        //334	webm video	        720p60	-	    hdr	
        //335	webm video	        1080p60	-	    hdr	
        //336	webm video      	1440p60	-	    hdr	
        //337	webm video      	2160p60	-	    hdr
        //394   mp4 video           144p
        //395   mp4 video           240p
        //396   mp4 video           360p
        //397   mp4 video           480p
        //398   mp4 video           720pp60
        //399   mp4 video           1080p60
        //400   mp4 video           1440p60
        //401   mp4 video           2160p60
        //402   mp4 video           2880p
        //571   mp4 video           3840p
        //702   mp4 video           3840p
        #endregion

        static readonly int[] fmtOptions3D = new int[] { 82, 83, 84, 85, 92, 93, 94, 95, 100, 101, 102 };
        static readonly int[] fmtOptionsHDR = new int[] { 330, 331, 332, 333, 334, 335, 336, 337 };

        private static DateTime _YtDlpLastCheck = DateTime.MinValue;
        private static readonly string _YtDlpExeName = Environment.Is64BitOperatingSystem ? "yt-dlp.exe" : "yt-dlp_x86.exe";
        private static readonly string _YtDlpExePath = System.IO.Path.Combine(OnlineVideoSettings.Instance.DllsDir, _YtDlpExeName);

        public override Dictionary<string, string> GetPlaybackOptions(string url, PlaybackOptionsBuilder.SelectionOptions selection, out int iPreselection)
        {
            iPreselection = 0;
            PlaybackOptionsBuilder qualities = new();

            string videoId = url;
            if (videoId.ToLower().Contains("youtube.com"))
            {
                // get an Id from the Url
                int p = videoId.LastIndexOf("watch?v="); // for http://www.youtube.com/watch?v=jAgBeAFZVgI
                if (p >= 0)
                    p += +8;
                else
                    p = videoId.LastIndexOf('/') + 1;
                int q = videoId.IndexOf('?', p);
                if (q < 0) q = videoId.IndexOf('&', p);
                if (q < 0) q = videoId.Length;
                videoId = videoId.Substring(p, q - p);
            }

            try
            {
                var jDataYtDlp = this.parsePlayerStatusFromYtDlp(qualities, videoId);

                Log.Debug("[YoutubeHoster] VideoQualities count: {0}", qualities.Count);

                //Subtitles
                subtitleTexts = null;
                if (!string.IsNullOrEmpty(subtitleLanguages))
                {
                    Log.Debug("[YoutubeHoster] Looking for subtitles... [{0}]", this.subtitleLanguages);

                    try
                    {
                        Dictionary<string, string> subUrls = null;
                        subUrls = getSubUrlFromYtDlp(jDataYtDlp["automatic_captions"], subtitleLanguages);

                        if (subUrls != null && subUrls.Count > 0)
                        {
                            subtitleTexts = new SubtitleList();
                            foreach (var kv in subUrls)
                            {
                                Uri uri;
                                if (Uri.TryCreate(new Uri("https://www.youtube.com"), kv.Value, out uri))
                                {
                                    string strSubText = WebCache.Instance.GetWebData(uri.ToString());
                                    if (string.IsNullOrWhiteSpace(strSubText))
                                        Log.Error("[YoutubeHoster] Failed to download subtitle: {0}/{1}", kv.Key, kv.Value);
                                    else
                                    {
                                        strSubText = Regex.Replace(strSubText, @"([\d\:\.]+\s*-->\s*[\d\:\.]+).*", "$1"); //remove possible alignment
                                        subtitleTexts.Add(kv.Key, strSubText);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error("[YoutubeHoster] Error parsing subtitles: {0} {1} {2}", e.Message, e.Source, e.StackTrace);
                    };
                }
            }
            catch (Exception e)
            {
                Log.Error("[YoutubeHoster] Error getting url: {0} {1} {2}", e.Message, e.Source, e.StackTrace);
                return null;
            }

            return qualities.GetPlaybackOptions(selection, out iPreselection);
        }
        public override string GetVideoUrl(string url)
        {
            // return highest quality by default
            var result = GetPlaybackOptions(url);
            if (result != null && result.Count > 0) return result.Last().Value;
            else return String.Empty;
        }

        public SubtitleList SubtitleTexts
        {
            get
            {
                return subtitleTexts;
            }
        }

        private Dictionary<string, string> getSubUrlFromYtDlp(JToken captions, string languages)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (captions != null)
            {
                string[] langs = languages.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string lang in langs)
                {
                    string strName = lang.Trim();
                    foreach (JProperty jSubtitle in captions)
                    {
                        if (jSubtitle.Name.StartsWith(lang, StringComparison.OrdinalIgnoreCase) && !jSubtitle.Name.EndsWith("-orig", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (JToken jSub in jSubtitle.Value)
                            {
                                if (jSub.Value<string>("ext") == "vtt")
                                {
                                    string strUrl = jSub.Value<string>("url");
                                    if (strUrl.IndexOf("&kind=asr") > 0)
                                        strName += ".Autogenerated";
                                    else if (strUrl.IndexOf("&kind=forced") > 0)
                                        strName += ".Forced";

                                    if (strUrl.IndexOf("&tlang=") > 0)
                                        strName += ".Translated";

                                    if (result.ContainsKey(strName))
                                        strName += '.' + (result.Count(pair => pair.Key == strName) + 1).ToString();

                                    result.Add(strName, strUrl);

                                    Log.Debug("[YoutubeHoster][getSubUrlFromYtDlp] {0}/{1}", strName, strUrl);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static bool checkYtDlpVersion()
        {
            if ((DateTime.Now - _YtDlpLastCheck).TotalHours < 24)
                return true;

            Log.Debug("[YoutubeHoster] checkYtDlpVersion() Checking latest version...");

            try
            {
                string strContent;

                if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor < 2)
                {
                    //2024.10.22 is latest working version for Win7
                    Log.Warn("[YoutubeHoster] checkYtDlpVersion() Latest working version for Windows 7 is 2024.10.22");
                    strContent = WebCache.Instance.GetWebData("https://api.github.com/repos/yt-dlp/yt-dlp/releases/181139283", cache: false);
                }
                else
                    strContent = WebCache.Instance.GetWebData("https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest", cache: false);

                JToken j = JToken.Parse(strContent);
                string strLatest = (string)j["tag_name"];

                Log.Debug("[YoutubeHoster] checkYtDlpVersion() Latest available version: {0}", strLatest);

                if (System.IO.File.Exists(_YtDlpExePath))
                {
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(_YtDlpExePath);

                    if (strLatest == fvi.FileVersion)
                    {
                        Log.Debug("[YoutubeHoster] checkYtDlpVersion() Current version is up to date.");
                        goto ok;
                    }
                }

                JToken jAsset = j["assets"].First(asset => (string)asset["name"] == _YtDlpExeName);
                string strUrlDownload = (string)jAsset["browser_download_url"];

                Log.Debug("[YoutubeHoster] checkYtDlpVersion() Downloading latest version: {0}", strLatest);
                string strDownloadPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _YtDlpExeName);
                WebClient wc = new();
                wc.DownloadFile(strUrlDownload, strDownloadPath);
                if (!System.IO.File.Exists(_YtDlpExePath))
                    System.IO.File.Move(strDownloadPath, _YtDlpExePath);
                else
                    System.IO.File.Replace(strDownloadPath, _YtDlpExePath, null);
                ok:
                _YtDlpLastCheck = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug("[YoutubeHoster] checkYtDlpVersion() Error: {0}", ex.Message);
                return System.IO.File.Exists(_YtDlpExePath);
            }
        }

        private JToken parsePlayerStatusFromYtDlp(PlaybackOptionsBuilder qualities, string strVideoId)
        {
            if (!checkYtDlpVersion())
                return null;

            Log.Debug("[YoutubeHoster] parsePlayerStatusFromYtDlp() loading json data: " + strVideoId);

            JToken jStreamingData = null;
            try
            {
                StringBuilder sbStd = new(1024 * 128);

                ProcessStartInfo psi = new()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    FileName = _YtDlpExePath,
                    Arguments = "--proxy \"\" -j https://www.youtube.com/watch?v=" + strVideoId //Ignore proxy; ytdlp can fail due to certificate error
                };

                Process proc = new() { StartInfo = psi };
                proc.ErrorDataReceived += new DataReceivedEventHandler(
                    (sender, e) =>
                    {
                        if (e.Data != null)
                            Log.Debug(e.Data);
                    });
                proc.OutputDataReceived += new DataReceivedEventHandler(
                    (sender, e) =>
                    {
                        if (e.Data != null)
                            sbStd.Append(e.Data);
                    }
                    );

                //Start yt-dlp process
                try
                {
                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    //Wait for yt-dlp exit
                    while (!proc.WaitForExit(500)) // give the os a chance to properly exit in case of thread.abort
                    {
                    }
                    proc.CancelOutputRead();
                    proc.CancelErrorRead();
                }
                catch (System.Threading.ThreadAbortException)
                {
                    Log.Debug("Thread aborted, killing yt-dlp");
                    proc.Kill();
                    throw;
                }

                jStreamingData = Newtonsoft.Json.JsonConvert.DeserializeObject<JToken>(sbStd.ToString());
                JArray jFormats = jStreamingData["formats"] as JArray;

                Log.Debug("[YoutubeHoster] parsePlayerStatusFromYtDlp() json data loaded");

                //Check Live HLS first
                JToken jHlsManifestUrl = jStreamingData["manifest_url"];
                if (jHlsManifestUrl != null)
                {
                    string strContent = this.GetWebData((string)jHlsManifestUrl);
                    Dictionary<string, HlsStreamInfo> options = HlsPlaylistParser.GetPlaybackOptionsEx(strContent, (string)jHlsManifestUrl, HlsStreamInfoComparer.BandwidtLowHigh, HlsStreamInfoFormatter.VideoDimension);
                    foreach (KeyValuePair<string, HlsStreamInfo> kv in options)
                    {
                        qualities.AddVideoQuality(kv.Value.Url, false, "HLS", string.Empty, kv.Value.Width, kv.Value.Height, kv.Value.Bandwidth, false, false);
                    }

                    return jStreamingData;
                }

                foreach (JToken format in jFormats)
                {
                    if (format.Value<string>("protocol").StartsWith("http"))
                    {
                        string strID = format.Value<string>("format_id");
                        int iIdx = strID.IndexOf('-');
                        if (!int.TryParse(iIdx > 0 ? strID.Substring(0, iIdx) : strID, out int iID))
                            iID = 0;

                        JToken jWidth = format["width"];
                        if (jWidth != null && jWidth.Type == JTokenType.Integer && (int)jWidth > 0)
                        {
                            JToken jAudioCh = format["audio_channels"];
                            int iAudioCh = jAudioCh != null && jAudioCh.Type == JTokenType.Integer ? (int)jAudioCh : -1;

                            try
                            {
                                qualities.AddVideoQuality(
                                     format.Value<string>("url"),
                                     iAudioCh <= 0,
                                     format.Value<string>("video_ext"),
                                     format.Value<string>("vcodec"),
                                     format.Value<int>("width"),
                                     format.Value<int>("height"),
                                     (int)format.Value<float>(iAudioCh <= 0 ? "vbr" : "tbr") * 1000,
                                     fmtOptions3D.Contains(iID),
                                     fmtOptionsHDR.Contains(iID));
                            }
                            catch (Exception ex)
                            {
                                Log.Error("[YoutubeHoster] parsePlayerStatusFromYtDlp()1 Error: {0}", ex.Message);
                            }
                        }
                        else
                        {
                            //Audio only

                            try
                            {
                                string strLanguageID = null;
                                string strLanguage = null;
                                string strLanguageDisplayName = null;
                                bool bDefault = false;

                                //Audio language track (MLA)
                                JToken jLang = format["language"];
                                if (jLang != null)
                                {
                                    strLanguageDisplayName = format["format_note"]?.Value<string>();
                                    if (!string.IsNullOrWhiteSpace(strLanguageDisplayName) && (iIdx = strLanguageDisplayName.IndexOf(',')) > 0)
                                        strLanguageDisplayName = strLanguageDisplayName.Substring(0, iIdx); //remove other text like 'medium, IOS'

                                    strLanguage = (string)format["language"];
                                    if (!string.IsNullOrWhiteSpace(strLanguage))
                                    {
                                        strLanguageID = strLanguage;
                                        if ((iIdx = strLanguage.IndexOf('-')) > 0)
                                            strLanguage = strLanguage.Substring(0, iIdx); //remove suffix (like 'desc')
                                    }
                                }

                                qualities.AddAudioQuality(
                                   format.Value<string>("url"),
                                   bDefault,
                                   strLanguageID,
                                   strLanguage,
                                   strLanguageDisplayName,
                                   format.Value<string>("audio_ext"),
                                   format.Value<string>("acodec"),
                                   (int)format.Value<float>("abr") * 1000,
                                   format.Value<int>("audio_channels"),
                                   format.Value<int>("asr")
                                   );
                            }
                            catch (Exception ex)
                            {
                                Log.Error("[YoutubeHoster] parsePlayerStatusFromYtDlp()2 Error: {0}", ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[YoutubeHoster] parsePlayerStatusFromYtDlp()3 Error: {0}", ex.Message);
                return null;
            }

            return jStreamingData;
        }
    }
}
