using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using OnlineVideos.Helpers;
using Newtonsoft.Json.Linq;

namespace OnlineVideos.Hoster
{
    public class Youtube : HosterBase, ISubtitle
    {
        private class YoutubeQuality
        {
            public string Url;
            
            public int VideoID = -1;
            public string VideoUrl;
            public string VideoType;
            public int VideoBitrate = -1;
            public int VideoWidth = -1;
            public int VideoHeight = -1;

            public int AudioID = -1;
            public string AudioUrl;
            public string AudioType;
            public int AudioBitrate = -1;
            public int AudioChannels = -1;
            public int AudioSampleRate = -1;
        }

        [Category("OnlineVideosUserConfiguration"), Description("Select subtitle language preferences (; separated and ISO 3166-2?), for example: en;de")]
        protected string subtitleLanguages = "";

        private string subtitleText = null;
        private const string YoutubePlayerKey = "<playerkey>";
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

        static readonly ushort[] fmtOptions3D = new ushort[] { 82, 83, 84, 85, 100, 101, 102 };
        static readonly ushort[] fmtOptionsMobile = new ushort[] { 13, 17 };
        static readonly ushort[] fmtOptionsQualitySorted = new ushort[] {
            // > 2160p
            272, 571, 702, 38, 402,

            //2160p
            337, 315, 266, 138, 401,

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

        static readonly ushort[] fmtOptionsQualityAudioSorted = new ushort[] { 141, 251, 140, 171, 250, 249 };

        public override Dictionary<string, string> GetPlaybackOptions(string url)
        {
            IWebProxy proxy = null;
            Dictionary<string, string> PlaybackOptions = null;

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

            List<YoutubeQuality> qualities = new List<YoutubeQuality>();

            try
            {
                CookieContainer cc = new CookieContainer();
                cc.Add(new Cookie("CONSENT", "YES+cb.20210328-17-p0.en+FX+684", "", ".youtube.com"));

                NameValueCollection headers = new NameValueCollection
                {
                    { "X-Youtube-Client-Name", "14" },
                    { "X-Youtube-Client-Version", "22.30.100" },
                    { "Origin", "https://www.youtube.com" },
                    { "Content-Type", "application/json" },
                    { "User-Agent", "com.google.android.youtube/19.09.37 (Linux; U; Android 11) gzip" },
                    { "Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7" },
                    { "Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8" },
                    { "Accept-Encoding", "gzip, deflate" },
                    { "Accept-Language", "en-us,en;q=0.5" }
                };

                string postdata = String.Format(@"{{""context"": {{""client"": {{""clientName"": ""ANDROID_CREATOR"", ""clientVersion"": ""{1}"", ""androidSdkVersion"": 30, ""userAgent"": ""{2}"", ""hl"": ""en"", ""timeZone"": ""UTC"", ""utcOffsetMinutes"": 0}}}}, ""videoId"": ""{0}"", ""params"": ""CgIQBg=="", ""playbackContext"": {{""contentPlaybackContext"": {{""html5Preference"": ""HTML5_PREF_WANTS""}}}}, ""contentCheckOk"": true, ""racyCheckOk"": true}}", videoId,
                    headers["X-Youtube-Client-Version"], headers["User-Agent"]);
                JObject jData = WebCache.Instance.GetWebData<JObject>(YoutubePlayerUrl, postData: postdata, headers: headers);
                parsePlayerStatus(jData["streamingData"], qualities, videoId);

                //Load webpage; there is more available options (e.g. UHD)
                JObject jDataWeb = null;
                string strContentsWeb = null;
                try
                {
                    strContentsWeb = WebCache.Instance.GetWebData(string.Format("https://www.youtube.com/watch?v={0}&bpctr=9999999999&has_verified=1", videoId),
                        proxy: proxy,
                        cookies: cc,
                        userAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36"
                        );
                    Match m = Regex.Match(strContentsWeb, @"ytInitialPlayerResponse\s+=\s+(?<js>[^<]*?(?<=}));", RegexOptions.IgnoreCase);
                    if (m.Success)
                        jDataWeb = JObject.Parse(m.Groups["js"].Value);
                }
                catch { jDataWeb = null; };

                if (jDataWeb != null)
                    parsePlayerStatus(jDataWeb["streamingData"], qualities, videoId);

                if (qualities.Count == 0 && strContentsWeb != null)
                {
                    //this one can be slow
                    Match m = Regex.Match(strContentsWeb, @"""(?i)(?:sts|signatureTimestamp)"":(?<sts>[0-9]{5})", RegexOptions.IgnoreCase);
                    if (m.Success)
                    {
                        postdata = String.Format(@"{{""context"": {{""client"": {{""clientName"": ""WEB"", ""clientVersion"": ""2.20210622.10.00"", ""hl"": ""en"", ""clientScreen"": ""EMBED""}}, ""thirdParty"": {{""embedUrl"": ""https://google.com""}}}}, ""videoId"": ""{0}"", ""playbackContext"": {{""contentPlaybackContext"": {{""html5Preference"": ""HTML5_PREF_WANTS"", ""signatureTimestamp"": {1}}}}}, ""contentCheckOk"": true, ""racyCheckOk"": true}}", videoId, m.Groups["sts"].Value);
                        jData = WebCache.Instance.GetWebData<JObject>(YoutubePlayerUrl, postData: postdata, headers: headers);
                        parsePlayerStatus(jData["streamingData"], qualities, videoId);
                    }
                }

                //Sort by quality
                qualities.Sort(new Comparison<YoutubeQuality>((a, b) =>
                {
                    if (a.VideoID != b.VideoID)
                        return Array.IndexOf(fmtOptionsQualitySorted, (ushort)b.VideoID).CompareTo(Array.IndexOf(fmtOptionsQualitySorted, (ushort)a.VideoID));
                    else
                    {
                        if (b.AudioID == a.AudioID)
                            return a.AudioBitrate.CompareTo(b.AudioBitrate);
                        else
                            return Array.IndexOf(fmtOptionsQualityAudioSorted, (ushort)b.AudioID).CompareTo(Array.IndexOf(fmtOptionsQualityAudioSorted, (ushort)a.AudioID));
                    }
                }));

                //Remove duplicates
                for (int i = qualities.Count - 1; i > 0; i--)
                {
                    YoutubeQuality q = qualities[i];
                    YoutubeQuality qPrev = qualities[i - 1];
                    if (q.VideoID == qPrev.VideoID && q.AudioID == qPrev.AudioID)
                        qualities.RemoveAt(--i);
                }

                //Create playback list
                Dictionary<string, YoutubeQuality> options = new();
                System.Globalization.CultureInfo ciEn = System.Globalization.CultureInfo.GetCultureInfo("en-US");
                Regex codecRegex = new(@"; codecs=""[^""]*""");
                char[] codecSep = new char[] { '/', '-' };
                foreach (YoutubeQuality quality in qualities)
                {
                    ushort uId = (ushort)quality.VideoID;

                    if (!fmtOptionsQualitySorted.Contains(uId))
                        continue;

                    if (hideMobileFormats && fmtOptionsMobile.Any(b => b == uId))
                        continue;

                    if (hide3DFormats && fmtOptions3D.Any(b => b == uId))
                        continue;

                    if (Uri.IsWellFormedUriString(quality.Url, UriKind.Absolute))
                    {
                        NameValueCollection urlOptions = HttpUtility.ParseQueryString(new Uri(quality.Url).Query);
                        string strType = urlOptions.Get("type");
                        string strStereo = urlOptions["stereo3d"] == "1" ? " 3D " : " ";

                        if (string.IsNullOrEmpty(strType))
                            strType = quality.VideoType;

                        if (!string.IsNullOrEmpty(strType))
                        {
                            strType = codecRegex.Replace(strType, "");
                            strType = strType.Substring(strType.LastIndexOfAny(codecSep) + 1);
                        }
                        if (quality.AudioID >= 0)
                        {
                            //Add audio codec
                            string strTypeAudio = codecRegex.Replace(quality.AudioType, "");
                            strType += "/" + strTypeAudio.Substring(strTypeAudio.LastIndexOfAny(codecSep) + 1);
                        }

                        //If the option already exists, then override old value
                        options[string.Format("{0}x{1}/{{0}} | {2}{3}({4})",
                            quality.VideoWidth, quality.VideoHeight, strType, strStereo, quality.VideoID)] = quality;
                    }
                };

                //Create final playback list and insert bitrate
                PlaybackOptions = new Dictionary<string, string>();
                foreach (string strKey in options.Keys)
                {
                    YoutubeQuality quality = options[strKey];

                    string strBitrate = string.Empty;
                    int iBr = quality.VideoBitrate + (quality.AudioBitrate > 0 ? quality.AudioBitrate : 0);
                    if (iBr >= 1000000)
                        strBitrate = (iBr / 1000000f).ToString("0.0", ciEn) + "mb";
                    else if (iBr >= 1000)
                        strBitrate = (iBr / 1000f).ToString("0.0", ciEn) + "kb";
                    else if (iBr > 0)
                        strBitrate = iBr.ToString() + "b";

                    PlaybackOptions[string.Format(strKey, strBitrate)] = quality.Url;
                }

                //Subtitles
                subtitleText = null;
                if (jDataWeb != null && !string.IsNullOrEmpty(subtitleLanguages))
                {
                    try
                    {
                        var captions = jDataWeb["captions"]?["playerCaptionsTracklistRenderer"]?["captionTracks"] as JArray;

                        string subUrl = getSubUrl(captions, subtitleLanguages);
                        if (!String.IsNullOrEmpty(subUrl))
                        {
                            subtitleText = WebCache.Instance.GetWebData(subUrl + "&fmt=vtt");
                        }
                    }
                    catch { };
                }
            }
            catch (Exception e)
            {
                Log.Error("Error getting url {0}", e.Message);
            }
            return PlaybackOptions;
        }
        public override string GetVideoUrl(string url)
        {
            // return highest quality by default
            var result = GetPlaybackOptions(url);
            if (result != null && result.Count > 0) return result.Last().Value;
            else return String.Empty;
        }

        public string SubtitleText
        {
            get
            {
                return subtitleText;
            }
        }


        private string getSubUrl(JArray captions, string languages)
        {
            if (captions != null)
            {
                string[] langs = languages.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string lang in langs)
                    foreach (JToken caption in captions)
                        if (lang == caption.Value<string>("languageCode"))
                            return caption.Value<string>("baseUrl");
            }
            return null;
        }

        private void parsePlayerStatus(JToken streamingData, List<YoutubeQuality> qualities, string strVideoId)
        {
            if (streamingData == null)
                return;

            if (streamingData["formats"] is JArray formats)
                parseFormats(formats, qualities, null, strVideoId);

            if (streamingData["adaptiveFormats"] is JArray formatsAdapt)
                parseFormats(formatsAdapt, qualities, formatsAdapt.Where(j => j["width"] == null && j["audioChannels"] != null).ToArray(), strVideoId);

            if (qualities.Count == 0)
            {
                string hlsUrl = streamingData.Value<String>("hlsManifestUrl");
                if (!String.IsNullOrEmpty(hlsUrl))
                {
                    var data = GetWebData(hlsUrl);
                    var res = HlsPlaylistParser.GetPlaybackOptions(data, hlsUrl, (x, y) => x.Bandwidth.CompareTo(y.Bandwidth), (x) => x.Width + "x" + x.Height);
                    foreach (var kv in res)
                    {
                        string[] qualityKey = { "0", kv.Key };
                        qualities.Add(new YoutubeQuality()
                        {
                            Url = kv.Value,
                            VideoType = kv.Key,
                            VideoID = 0
                        });
                    }
                }
            }
        }

        private static void parseFormats(JArray formats, List<YoutubeQuality> qualities, JToken[] audioStreams, string strVideoId)
        {
            foreach (JToken format in formats)
            {
                if (format["width"] == null)
                    continue; //skip audio streams

                if (format["audioChannels"] == null)
                {
                    //Video stream only

                    if (audioStreams != null)
                    {
                        for (int i = 0; i < audioStreams.Length; i++)
                        {
                            JToken jAudio = audioStreams[i];

                            YoutubeQuality q = new()
                            {
                                VideoType = format.Value<string>("mimeType"),
                                VideoBitrate = format.Value<int>("bitrate"),
                                VideoID = format.Value<int>("itag"),
                                VideoUrl = format.Value<string>("url"),
                                VideoWidth = format.Value<int>("width"),
                                VideoHeight = format.Value<int>("height"),
                                AudioID = jAudio.Value<int>("itag"),
                                AudioUrl = jAudio.Value<string>("url"),
                                AudioSampleRate = jAudio.Value<int>("audioSampleRate"),
                                AudioChannels = jAudio.Value<int>("audioChannels"),
                                AudioType = jAudio.Value<string>("mimeType"),
                                AudioBitrate = jAudio.Value<int>("bitrate"),
                            };

                            //Specific scheme to hold multiple urls
                            q.Url = string.Format("onlinevideos://127.0.0.1/VideoLink?idYoutube={0}&url={1}&urlAudio={2}&type={3}&typeAudio={4}",
                                strVideoId,
                                HttpUtility.UrlEncode(q.VideoUrl),
                                HttpUtility.UrlEncode(q.AudioUrl),
                                HttpUtility.UrlEncode(q.VideoType),
                                HttpUtility.UrlEncode(q.AudioType)
                                );
                            qualities.Add(q);
                        }
                    }
                }
                else
                {
                    qualities.Add(new YoutubeQuality()
                    {
                        VideoType = format.Value<string>("mimeType"),
                        VideoBitrate = format.Value<int>("bitrate"),
                        Url = format.Value<string>("url"),
                        VideoID = format.Value<int>("itag"),
                        VideoUrl = format.Value<string>("url"),
                        VideoWidth = format.Value<int>("width"),
                        VideoHeight = format.Value<int>("height"),
                        AudioSampleRate = format.Value<int>("audioSampleRate"),
                        AudioChannels = format.Value<int>("audioChannels"),
                    });
                }
            }
        }
    }
}
