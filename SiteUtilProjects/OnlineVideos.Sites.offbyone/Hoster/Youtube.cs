using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;
using System.Xml;
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

        //YouTube Web               AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8
        //YouTube Web Kids          AIzaSyBbZV_fZ3an51sF-mvs5w37OqqbsTOzwtU
        //YouTube Web Music         AIzaSyC9XL3ZjWddXya6X74dJoCTL-WEYFDNX30
        //YouTube Web Creator       AIzaSyBUPetSUmoZL-OhlxA7wSac5XinrygCqMo
        //YouTube Android           AIzaSyA8eiZmM1FaDVjRy-df2KTyQ_vz_yYM39w
        //YouTube Android Music     AIzaSyAOghZGza2MQSZkY_zfZ370N-PUdXEo8AI
        //YouTube Android Embedded  AIzaSyCjc_pVEDi4qsv5MtC2dMXzpIaDoRFLsxw
        //YouTube Android Creator   AIzaSyD_qjV8zaaUMehtLkrKFgVeSX_Iqbtyws8
        //YouTube IOS               AIzaSyB-63vPrdThhKuerbB2N_l7Kwwcxj6yUAc
        //YouTube IOS Music         AIzaSyBAETezhkwP0ZWA02RsqT1zu78Fpt0bC_s
        private SubtitleList subtitleTexts = null;
        private const string YoutubePlayerKey = "AIzaSyD_qjV8zaaUMehtLkrKFgVeSX_Iqbtyws8";
        private const string YoutubePlayerUrl = "https://www.youtube.com/youtubei/v1/player?key=" + YoutubePlayerKey;

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
        static readonly int[] fmtOptionsMobile = new int[] { 13, 17 };
        static readonly int[] fmtOptionsQualitySorted = new int[] {
            // > 2160p
            272, 571, 702, 38, 402,

            //2160p
            337, 315, 313, 266, 138, 401,

            //1440p
            336, 308, 264, 271, 400,

            //1080p
            85, 137, 46, 37, 335, 303, 299, 248, 169, 399,
            
            //720p
            102, 84, 136, 45, 22, 247, 334, 298, 302, 398,
            
            //480p
            101, 135, 83, 44, 35, 244, 245, 246, 218, 168, 333, 397,
            
            //360p
            100, 82, 43, 18, 34, 243, 134, 332, 396,
            
            //240p
            133, 6, 5, 0, 242, 331, 395,
        
            //144p
            17, 13, 160, 219, 278, 330, 394
        };

        static readonly int[] fmtOptionsQualityAudioSorted = new int[] { 141, 251, 140, 171, 250, 249 };

        private static YoutubeSignatureDecryptor _YoutubeDecryptor = null;
        private static DateTime _YtDlpLastCheck = DateTime.MinValue;
        private static readonly string _YtDlpExeName = Environment.Is64BitOperatingSystem ? "yt-dlp.exe" : "yt-dlp_x86.exe";
        private static readonly string _YtDlpExePath = System.IO.Path.Combine(OnlineVideoSettings.Instance.DllsDir, _YtDlpExeName);

        public override Dictionary<string, string> GetPlaybackOptions(string url, PlaybackOptionsBuilder.SelectionOptions selection, out int iPreselection)
        {
            iPreselection = 0;
            IWebProxy proxy = null;
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
                string postdata;
                NameValueCollection headers;

                //yt-dlp processing
                JToken jDataYtDlp = null;

                JObject jDataWeb = null;

                if (_YoutubeDecryptor != null && (DateTime.Now - _YoutubeDecryptor.LoadTimestamp).TotalDays >= 1)
                    _YoutubeDecryptor = null; //don't use the same decryptor for more than 1 day

                string strContentsWeb = null;
                string strSecure = null;

                try
                {
                    CookieContainer cookies = new();
                    strContentsWeb = WebCache.Instance.GetWebData(string.Format("https://www.youtube.com/watch?v={0}&bpctr=9999999999&has_verified=1", videoId),
                        proxy: proxy,
                        userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36",
                        cache: false,
                        cookies: cookies
                        );

                    if (strContentsWeb == null)
                    {
                        Log.Error("[YoutubeHoster] Failed to get web content.");
                        return null;
                    }

                    strSecure = cookies.GetCookies(new Uri("https://www.youtube.com"))["__Secure-YEC"]?.Value;

                    Log.Debug("[YoutubeHoster] Secure: {0}", strSecure);
                }
                catch { };

                //IOS
                headers = new NameValueCollection
                        {
                            { "X-Youtube-Client-Name", "5" },
                            { "X-Youtube-Client-Version", "19.29.1" },
                            { "Origin", "https://www.youtube.com" },
                            { "Content-Type", "application/json" },
                            { "User-Agent", "com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)" },
                            { "Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7" },
                            { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                            { "Accept-Encoding", "gzip, deflate" },
                            { "Accept-Language", "en-us,en;q=0.5" }
                        };

                if (strSecure != null)
                    headers.Add("X-Goog-Visitor-Id", strSecure);

                postdata = string.Format(@"{{""context"": {{""client"": {{""clientName"": ""IOS"", ""clientVersion"": ""{1}"", ""deviceMake"": ""Apple"", ""deviceModel"": ""iPhone16,2"", ""hl"": ""en"", ""osName"": ""iPhone"", ""osVersion"": ""17.5.1.21F90"", ""timeZone"": ""UTC"", ""userAgent"": ""com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)"", ""utcOffsetMinutes"": 0}}}}, ""videoId"": ""{0}"", ""playbackContext"": {{""contentPlaybackContext"": {{""html5Preference"": ""HTML5_PREF_WANTS"", ""signatureTimestamp"": {2}}}}}, ""contentCheckOk"": true, ""racyCheckOk"": true}}",
                    videoId, headers["X-Youtube-Client-Version"], "0");
                jDataWeb = WebCache.Instance.GetWebData<JObject>("https://www.youtube.com/youtubei/v1/player?prettyPrint=false", postData: postdata, headers: headers);

                PlayerStatusEnum status;

                if (jDataWeb != null)
                {
                    try
                    {
                        status = parsePlayerStatus(jDataWeb["videoDetails"], jDataWeb["streamingData"], qualities, null);
                    }
                    catch (Exception e)
                    {
                        Log.Error("[YoutubeHoster] Error parsing video links: {0} {1} {2}", e.Message, e.Source, e.StackTrace);
                        qualities.Clear();
                        status = PlayerStatusEnum.Error;
                    }
                }

            get_decr:
                bool bExistingDecryptor = false;

                if (qualities.Count == 0)
                {

                    if (this.webViewHelper != null)
                    {
                        if (_YoutubeDecryptor == null)
                        {
                            try
                            {
                                Match m = Regex.Match(strContentsWeb, @"ytInitialPlayerResponse\s+=\s+(?<js>[^<]*?(?<=}));", RegexOptions.IgnoreCase);
                                if (m.Success)
                                {
                                    jDataWeb = JObject.Parse(m.Groups["js"].Value);

                                    try
                                    {
                                        //Prepare signature decryptor
                                        _YoutubeDecryptor = new YoutubeSignatureDecryptor(strContentsWeb, webViewHelper);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.Error("[YoutubeHoster] Error creating signature decryptor: {0}", ex.Message);
                                        _YoutubeDecryptor = null;
                                    }
                                }
                            }
                            catch { jDataWeb = null; };
                        }
                        else
                        {
                            Log.Debug("[YoutubeHoster] Using existing YouTube decryptor: {0}", _YoutubeDecryptor.SignatureTimestamp);
                            bExistingDecryptor = true;
                        }
                    }
                    else
                        Log.Error("[YoutubeHoster] WebView not initialized.");

                    if (_YoutubeDecryptor != null)
                    {
                        if (jDataWeb == null)
                        {
                            Log.Error("[YoutubeHoster] Failed to get json web data from youtube server. Trying MWEB client...");

                            //MWEB
                            headers = new NameValueCollection
                        {
                            { "X-Youtube-Client-Name", "2" },
                            { "X-Youtube-Client-Version", "2.20240726.01.00" },
                            { "Origin", "https://www.youtube.com" },
                            { "Content-Type", "application/json" },
                            { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.41 Safari/537.36" },
                            { "Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7" },
                            { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                            { "Accept-Encoding", "gzip, deflate" },
                            { "Accept-Language", "en-us,en;q=0.5" }
                        };

                            postdata = string.Format(@"{{""context"": {{""client"": {{""clientName"": ""MWEB"", ""clientVersion"": ""{1}"", ""hl"": ""en"", ""timeZone"": ""UTC"", ""utcOffsetMinutes"": 0}}}}, ""videoId"": ""{0}"", ""playbackContext"": {{""contentPlaybackContext"": {{""html5Preference"": ""HTML5_PREF_WANTS"", ""signatureTimestamp"": {2}}}}}, ""contentCheckOk"": true, ""racyCheckOk"": true}}",
                                videoId, headers["X-Youtube-Client-Version"], _YoutubeDecryptor.SignatureTimestamp);
                            jDataWeb = WebCache.Instance.GetWebData<JObject>("https://www.youtube.com/youtubei/v1/player?prettyPrint=false", postData: postdata, headers: headers);
                        }

                        if (jDataWeb != null)
                        {
                            try
                            {
                                status = parsePlayerStatus(jDataWeb["videoDetails"], jDataWeb["streamingData"], qualities, _YoutubeDecryptor);
                            }
                            catch (Exception e)
                            {
                                Log.Error("[YoutubeHoster] Error parsing video links: {0} {1} {2}", e.Message, e.Source, e.StackTrace);
                                qualities.Clear();
                                status = PlayerStatusEnum.Error;
                            }

                            if (status != PlayerStatusEnum.OK)
                            {
                                qualities.Clear();
                                Log.Error("[YoutubeHoster] Failed to get AdaptiveFormat working links.");
                                _YoutubeDecryptor = null;

                                if (bExistingDecryptor)
                                    goto get_decr; //try get new fresh decryptor
                            }
                            else
                                _YoutubeDecryptor.LastUseTimestamp = DateTime.Now;
                        }
                        else
                            Log.Error("[YoutubeHoster] Failed to get json web data from youtube server.");
                    }


                    Log.Error("[YoutubeHoster] Failed to get AdaptiveFormat working links. Trying yt-dlp ...");

                    jDataYtDlp = this.parsePlayerStatusFromYtDlp(qualities, videoId);
                    if (jDataYtDlp == null || qualities.Count == 0)
                    {
                        Log.Error("[YoutubeHoster] Failed to get working links from yt-dlp. Fallback to non JS player ...");

                        try
                        {
                            //Non JS player API

                            CookieContainer cc = new CookieContainer();
                            cc.Add(new Cookie("CONSENT", "YES+cb.20210328-17-p0.en+FX+684", "", ".youtube.com"));

                            headers = new NameValueCollection
                            {
                                { "X-Youtube-Client-Name", "95" },
                                { "X-Youtube-Client-Version", "0.1" },
                                { "Origin", "https://www.youtube.com" },
                                { "Content-Type", "application/json" },
                                { "User-Agent", "com.google.android.apps.youtube.producer/0.111.1 (Linux; U; Android 11) gzip" },
                                { "Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7" },
                                { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                                { "Accept-Encoding", "gzip, deflate" },
                                { "Accept-Language", "en-us,en;q=0.5" }
                            };

                            postdata = string.Format(@"{{""context"": {{""client"": {{""clientName"": ""MEDIA_CONNECT_FRONTEND"", ""clientVersion"": ""{1}"", ""androidSdkVersion"": 30, ""userAgent"": ""{2}"", ""hl"": ""en"", ""timeZone"": ""UTC"", ""utcOffsetMinutes"": 0}}}}, ""videoId"": ""{0}"", ""params"": ""CgIQBg=="", ""playbackContext"": {{""contentPlaybackContext"": {{""html5Preference"": ""HTML5_PREF_WANTS""}}}}, ""contentCheckOk"": true, ""racyCheckOk"": true}}",
                                videoId, headers["X-Youtube-Client-Version"], headers["User-Agent"]);
                            jDataWeb = WebCache.Instance.GetWebData<JObject>(YoutubePlayerUrl, postData: postdata, headers: headers);

                            if (jDataWeb != null)
                            {
                                parsePlayerStatus(jDataWeb["videoDetails"], jDataWeb["streamingData"], qualities, null);

                                if (qualities.Count == 0)
                                {
                                    Log.Error("[YoutubeHoster] Failed to get working links from non JS player.");
                                    return null;
                                }
                            }
                            else
                                Log.Error("[YoutubeHoster] Failed to get json web data from youtube server.");
                        }
                        catch (Exception e)
                        {
                            Log.Error("Error getting links from non JS player api: {0}", e.Message);
                            return null;
                        }
                    }
                }

                Log.Debug("[YoutubeHoster] VideoQualities count: {0}", qualities.Count);

                //Subtitles
                subtitleTexts = null;
                if (!string.IsNullOrEmpty(subtitleLanguages))
                {
                    try
                    {
                        Dictionary<string, string> subUrls = null;
                        if (jDataYtDlp != null)
                        {
                            subUrls = getSubUrlFromYtDlp(jDataYtDlp["subtitles"], subtitleLanguages);
                        }
                        else
                        {
                            JArray captions = null;

                            if (jDataWeb != null)
                                captions = jDataWeb["captions"]?["playerCaptionsTracklistRenderer"]?["captionTracks"] as JArray;
                            else
                            {
                                string contents = WebCache.Instance.GetWebData(string.Format("https://www.youtube.com/watch?v={0}&bpctr=9999999999&has_verified=1", videoId), proxy: proxy);
                                Match m = Regex.Match(contents, @"ytInitialPlayerResponse\s+=\s+(?<js>[^<]*?(?<=}));", RegexOptions.IgnoreCase);
                                if (m.Success)
                                {
                                    jDataWeb = JObject.Parse(m.Groups["js"].Value);
                                    captions = jDataWeb["captions"]?["playerCaptionsTracklistRenderer"]?["captionTracks"] as JArray;
                                }
                            }

                            if (captions != null)
                            {
                                subUrls = getSubUrl(captions, subtitleLanguages);
                            }
                        }

                        if (subUrls != null && subUrls.Count > 0)
                        {
                            subtitleTexts = new SubtitleList();
                            foreach (var kv in subUrls)
                            {
                                Uri uri;
                                if (Uri.TryCreate(new Uri("https://www.youtube.com"), kv.Value, out uri))
                                {
                                    var subs = WebCache.Instance.GetWebData(uri.ToString());
                                    subs = Regex.Replace(subs, @"([\d\:\.]+\s*-->\s*[\d\:\.]+).*", "$1"); //remove possible alignment
                                    subtitleTexts.Add(kv.Key, subs);
                                }
                            }
                        }
                    }
                    catch { };
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

        private Dictionary<string, string> getSubUrl(JArray captions, string languages)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (captions != null)
            {
                string[] langs = languages.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string lang in langs)
                    foreach (JToken caption in captions)
                        if (lang == caption.Value<string>("languageCode"))
                            result.Add(lang, caption.Value<string>("baseUrl") + "&fmt=vtt");
            }
            return result;
        }

        private Dictionary<string, string> getSubUrlFromYtDlp(JToken captions, string languages)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (captions != null)
            {
                string[] langs = languages.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string lang in langs)
                {
                    string strName = " - " + lang;
                    foreach (JProperty jSubtitle in captions)
                    {
                        foreach (JToken jSub in jSubtitle.Value)
                        {
                            if (jSub.Value<string>("ext") == "vtt" && jSub.Value<string>("name").EndsWith(strName))
                                result.Add(lang, jSub.Value<string>("url"));
                        }
                    }
                }
            }
            return result;
        }

        private PlayerStatusEnum parsePlayerStatus(JToken videoDetails, JToken streamingData, PlaybackOptionsBuilder qualities, YoutubeSignatureDecryptor dec)
        {
            if (streamingData == null)
                return PlayerStatusEnum.Error;

            string hlsUrl = streamingData.Value<string>("hlsManifestUrl");

            //In case of LIVE stream take the HLS or DASH link only; adaptive formats have cca 5s streams only
            bool bIsLive = (videoDetails?.Value<bool>("isLive") ?? false) || (videoDetails?.Value<int>("lengthSeconds") ?? 0) == 0;

            if (!bIsLive)
            {
                if (streamingData["formats"] is JArray formats)
                    parseFormats(formats, qualities, dec, false);

                if (streamingData["adaptiveFormats"] is JArray formatsAdapt)
                {
                    if (parseFormats(formatsAdapt, qualities, dec, dec != null) == PlayerStatusEnum.InvalidLink)
                        return PlayerStatusEnum.InvalidLink;
                }
            }

            // Web page links can have higher resolution so take always the LIVE streams
            if (qualities.Count == 0 || bIsLive)
            {
                if (!String.IsNullOrEmpty(hlsUrl))
                {
                    var data = GetWebData(hlsUrl);
                    var res = HlsPlaylistParser.GetPlaybackOptionsEx(data, hlsUrl, HlsStreamInfoComparer.BandwidtLowHigh, HlsStreamInfoFormatter.VideoDimension);
                    foreach (var kv in res)
                    {
                        qualities.AddVideoQuality(kv.Value.Url, false, "HLS", string.Empty, kv.Value.Width, kv.Value.Height, kv.Value.Bandwidth, false, false);
                    }
                }
            }

            return PlayerStatusEnum.OK; //LAV is failing to play YouTube MPEG-DASH(so far)

            //MPEG-DASH; LAV does support this format
            /*
            string strDashUrl = streamingData.Value<string>("dashManifestUrl");
            if (!string.IsNullOrEmpty(strDashUrl))
            {
                try
                {
                    const string MPD_NS = "urn:mpeg:dash:schema:mpd:2011";
                    const string MPD_NS2 = "urn:mpeg:DASH:schema:MPD:2011";
                    string strContent = WebCache.Instance.GetWebData(strDashUrl);
                    XmlDocument xml = new();
                    xml.LoadXml(strContent);
                    XmlNamespaceManager mgr = new(xml.NameTable);
                    mgr.AddNamespace("ns", MPD_NS);
                    XmlNode nodeMPD = xml.SelectSingleNode("ns:MPD", mgr);
                    if (nodeMPD == null)
                    {
                        mgr.AddNamespace("ns", MPD_NS2);
                        nodeMPD = xml.SelectSingleNode("ns:MPD", mgr);
                    }

                    //Find highest video resolution
                    XmlNodeList nodesRepresentation = nodeMPD.SelectNodes("//ns:AdaptationSet/ns:Representation[@width][@height]", mgr);
                    int iWidthMax = 0;
                    int iHeightMax = 0;
                    int iBandwidth = 0;
                    for (int i = 0; i < nodesRepresentation.Count; i++)
                    {
                        XmlNode nodeR = nodesRepresentation[i];
                        int iW = int.Parse(nodeR.Attributes["width"].Value);
                        int iH = int.Parse(nodeR.Attributes["height"].Value);
                        if (iW > iWidthMax || iH > iHeightMax)
                        {
                            iWidthMax = iW;
                            iHeightMax = iH;
                        }

                        if (!int.TryParse(nodeR.Attributes["bandwidth"]?.Value, out iBandwidth))
                            iBandwidth = 0;
                    }

                    qualities.AddVideoQuality(strDashUrl, false, "DASH", string.Empty, iWidthMax, iHeightMax, iBandwidth, false, false);

                }
                catch (Exception ex)
                {
                    Log.Error("[YoutubeHoster] parsePlayerStatus() Error downloading MPEG-DASH manifest: {0}", ex.Message);
                }
            }
            */
        }

        private static bool checkYtDlpVersion()
        {
            if ((DateTime.Now - _YtDlpLastCheck).TotalHours < 24)
                return true;

            Log.Debug("[YoutubeHoster] checkYtDlpVersion() Checking latest version...");

            try
            {
                string strContent;
                if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.MajorRevision < 2)
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
                StringBuilder sbErr = new(1024 * 1);

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
                            sbErr.Append(e.Data);
                    });
                proc.OutputDataReceived += new DataReceivedEventHandler(
                    (sender, e) =>
                    {
                        if (e.Data != null)
                            sbStd.Append(e.Data);
                    }
                    );

                //Start yt-dlp process
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                //Wait for yt-dlp exit
                proc.WaitForExit();
                proc.CancelOutputRead();
                proc.CancelErrorRead();

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
                                Log.Error("[YoutubeHoster] parsePlayerStatusFromYtDlp() Error: {0}", ex.Message);
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
                                Log.Error("[YoutubeHoster] parsePlayerStatusFromYtDlp() Error: {0}", ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("[YoutubeHoster] parsePlayerStatusFromYtDlp() Error: {0}", ex.Message);
                return null;
            }

            return jStreamingData;
        }

        private static PlayerStatusEnum parseFormats(JArray formats, PlaybackOptionsBuilder qualities, YoutubeSignatureDecryptor dec, bool bCheckLink)
        {
            bool bStreamValid = !bCheckLink;

            foreach (JToken format in formats)
            {
                string strUrl = getStreamUrl(format, dec);
                if (strUrl == null)
                    continue;

                int iItag = (int)format["itag"];

                if (!bStreamValid)
                {
                    //Check for stream availability
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(strUrl);
                    wr.Method = "HEAD";
                    HttpWebResponse resp = null;
                    try { resp = (HttpWebResponse)wr.GetResponse(); }
                    catch { }
                    if (resp == null || resp.StatusCode != HttpStatusCode.OK)
                    {
                        Log.Warn("[YoutubeHoster] parseFormats() Invalid link.");
                        return PlayerStatusEnum.InvalidLink;
                    }

                    Log.Debug("[YoutubeHoster] parseFormats() Link is valid.");
                    bStreamValid = true; //links seems to be valid; no more checks
                }

                if (format["width"] == null)
                {
                    //Audio only

                    //Audio language track check (MLA)
                    JToken jTrack = format["audioTrack"];
                    string strLanguageID = null;
                    string strLanguage = null;
                    string strLanguageDisplayName = null;
                    bool bDefault = false;
                    if (jTrack != null)
                    {
                        strLanguageID = jTrack["id"]?.Value<string>();
                        strLanguageDisplayName = jTrack["displayName"]?.Value<string>();

                        if (!string.IsNullOrEmpty(strLanguageID))
                        {
                            int iIdx = strLanguageID.IndexOf('.');
                            if (iIdx > 0)
                                strLanguage = strLanguageID.Substring(0, iIdx);
                            else
                                strLanguage = strLanguageID;
                        }

                        bDefault = jTrack["audioIsDefault"]?.Value<bool>() ?? false;
                    }

                    qualities.AddAudioQuality(
                        strUrl,
                        bDefault,
                        strLanguageID,
                        strLanguage,
                        strLanguageDisplayName,
                        format.Value<string>("mimeType"),
                        format.Value<string>("mimeType"),
                        format.Value<int>("bitrate"),
                        format.Value<int>("audioChannels"),
                        format.Value<int>("audioSampleRate"));


                }
                else
                {
                    if (format["audioChannels"] == null)
                    {
                        //Video stream only; no audio

                        qualities.AddVideoQuality(
                            (string)format["url"],
                            true,
                            format.Value<string>("mimeType"),
                            format.Value<string>("mimeType"),
                            format.Value<int>("width"),
                            format.Value<int>("height"),
                            format.Value<int>("bitrate"),
                            fmtOptions3D.Contains(iItag),
                            fmtOptionsHDR.Contains(iItag));
                    }
                    else
                    {
                        //Video with audio

                        qualities.AddVideoQuality(
                          (string)format["url"],
                          false,
                          format.Value<string>("mimeType"),
                          format.Value<string>("mimeType"),
                          format.Value<int>("width"),
                          format.Value<int>("height"),
                          format.Value<int>("bitrate"),
                          fmtOptions3D.Contains(iItag),
                          fmtOptionsHDR.Contains(iItag));
                    }
                }
            }

            return PlayerStatusEnum.OK;
        }

        private static string getStreamUrl(JToken jFormat, YoutubeSignatureDecryptor decryptor)
        {
            JToken jUrlDecr = jFormat["urlDecrypted"];
            if (jUrlDecr == null)
            {
                string strUrl = null;
                string strSig = null;
                JToken jUrl = jFormat["url"];
                if (jUrl == null)
                {
                    string strSignatureCipher = jFormat["signatureCipher"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(strSignatureCipher))
                    {
                        NameValueCollection col = HttpUtility.ParseQueryString(strSignatureCipher);
                        strUrl = col["url"];
                        strSig = col["s"];
                    }
                }
                else
                    strUrl = (string)jUrl;

                if (strUrl != null)
                {
                    if (decryptor != null)
                    {
                        Match m = Regex.Match(strUrl, "&n=(?<n>[^&]+)");
                        if (m.Success)
                        {
                            string str = decryptor.DecryptNSignature(m.Groups["n"].Value);
                            strUrl = strUrl.Substring(0, m.Index + 3) + str + strUrl.Substring(m.Index + m.Length);
                            if (strSig != null)
                                strUrl += "&sig=" + decryptor.DecryptSignature(strSig);
                        }
                    }
                    jFormat["urlDecrypted"] = strUrl; //to avoid further decrypting
                    return strUrl;
                }

                return null;
            }
            else
                return (string)jUrlDecr;
        }
    }
}
