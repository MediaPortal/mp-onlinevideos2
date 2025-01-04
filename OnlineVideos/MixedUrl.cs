using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Text;

namespace OnlineVideos
{
    public class MixedUrl
    {
        public const string MIXED_URL_SCHEME = "onlinevideos";

        public struct AudioTrack
        {
            public string Url;
            public string Language;
            public string Description;
            public bool IsDefault;
            public object Tag;
        }
        
        public string VideoUrl { get; private set; }
        public AudioTrack[] AudioTracks { get; private set; }
        public bool Valid { get; private set; }
        public int DefaultAudio { get; private set; }


        public MixedUrl(string strUrl)
        {
            Uri uri = new Uri(strUrl);
            if (uri.Scheme == MIXED_URL_SCHEME)
            {
                NameValueCollection args = HttpUtility.ParseQueryString(uri.Query);
                this.VideoUrl = args.Get("videoUrl");

                if (!int.TryParse(args.Get("audioDefault"), out int iDefault))
                    iDefault = 0;

                this.DefaultAudio = iDefault;

                List<AudioTrack> audios = new List<AudioTrack>();

                int iCnt = 0;
                while (true)
                {
                    string strSuffix = iCnt > 0 ? iCnt.ToString() : null;
                    AudioTrack audio = new AudioTrack()
                    {
                        Url = args.Get("audioUrl" + strSuffix),
                        Language = args.Get("audioLang" + strSuffix),
                        Description = args.Get("audioDescr" + strSuffix)
                    };

                    if (!string.IsNullOrWhiteSpace(audio.Url))
                    {
                        if (iDefault == audios.Count)
                            audio.IsDefault = true;

                        audios.Add(audio);
                        iCnt++;
                    }
                    else
                        break;
                }

                this.AudioTracks = audios.ToArray();
                this.Valid = true;
            }
        }
        public MixedUrl(string strVideoUrl, AudioTrack[] audiolinks)
        {
            if (Uri.IsWellFormedUriString(strVideoUrl, UriKind.Absolute) && audiolinks != null && audiolinks.Length > 0)
            {
                this.VideoUrl = strVideoUrl;
                this.AudioTracks = audiolinks;
                this.Valid = true;
            }
        }

        public override string ToString()
        {
            if (this.Valid)
            {
                int iCnt = 0;
                int iDefaultTrack = -1;

                StringBuilder sb = new StringBuilder(1024);

                sb.Append(MIXED_URL_SCHEME);
                sb.Append("://127.0.0.1/VideoLink?videoUrl=");
                sb.Append(HttpUtility.UrlEncode(this.VideoUrl));

                for (int i = 0; i < this.AudioTracks.Length; i++)
                {
                    AudioTrack audio = this.AudioTracks[i];

                    sb.Append("&audioUrl");
                    if (iCnt > 0)
                        sb.Append(iCnt);
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(audio.Url));
                    if (this.AudioTracks.Length == 1)
                        break;

                    sb.Append("&audioLang");
                    if (iCnt > 0)
                        sb.Append(iCnt);
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(audio.Language));

                    sb.Append("&audioDescr");
                    if (iCnt > 0)
                        sb.Append(iCnt);
                    sb.Append('=');
                    sb.Append(HttpUtility.UrlEncode(audio.Description));

                    if (audio.IsDefault && iDefaultTrack < 0)
                        iDefaultTrack = iCnt;

                    iCnt++;
                }

                if (iDefaultTrack >= 0)
                {
                    sb.Append("&audioDefault=");
                    sb.Append(iDefaultTrack);
                }

                return sb.ToString();
            }

            return null;
        }
    }
}
