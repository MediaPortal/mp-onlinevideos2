using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.IO.Compression;

namespace OnlineVideos.Hoster
{
    public class YoutubeSignatureDecryptor
    {
        //Current version of the Decryptor structure
        private const int _DECRYPTOR_VERSION_CURRENT = 2;

        private class Decryptor
        {
            public int Version = _DECRYPTOR_VERSION_CURRENT;
            public DateTime TimeStamp;
            public DateTime Expires;
            public string NSignatureJsCode;
            public string SignatureJsCode;
            public string SignatureTimestamp;

            [Newtonsoft.Json.JsonIgnore]
            public DateTime LastUse;

            [Newtonsoft.Json.JsonIgnore]
            public DateTime LoadTime;

            [Newtonsoft.Json.JsonIgnore]
            public string CacheFilepath;
        }

        private static readonly Regex _RegexPlayerJsUrl = new("href=\"(?<url>/s/player/(?<id>.+?)/player_.+?base\\.js)\"", RegexOptions.Compiled);
        private static readonly Regex _RegexPlayerTs = new("\"STS\":(?<sts>[0-9]+)", RegexOptions.Compiled);
        private static readonly Regex[] _RegexPlayerJsFunctions = new Regex[]
        {
            new Regex(@"(?<fname>[a-zA-Z0-9$]+)=function\([a-zA-Z0-9$]+\){var\s+[a-zA-Z0-9$]+=[a-zA-Z0-9$]+\.split\((?s:.)+?return\s+[a-zA-Z0-9$]+\.join\(""""\)};", RegexOptions.Compiled),
            new Regex("(?<fname>[a-zA-Z0-9$]+)=function\\(a\\){var\\s+b=String\\.prototype\\.split\\.(?s:.)+?return\\s+Array\\.prototype\\.join\\.call\\(b,\"\"\\)};", RegexOptions.Compiled),
            new Regex("(?<=(?<fname>[a-zA-Z0-9$]+)\\=function\\((?s:.)+?)\"enhanced_except_(?s:.)+?\\)};", RegexOptions.Compiled)
        };
        private static readonly Regex _RegexPlayerJsSigFunction = new(@"(?<fname>[a-zA-Z0-9$]+)=function\([a-zA-Z0-9$]+\){[a-zA-Z0-9$]+=[a-zA-Z0-9$]+\.split\(""""\);(?<fnamesub>[a-zA-Z0-9$]+)\.(?s:.)+?return\s+[a-zA-Z0-9$]+\.join\(""""\)};", RegexOptions.Compiled);
        private const string _JS_SUB_FUNCTION_REGEX = "var {0}={{(?s:.)+?}};";
        private const string _JS_FUNCTION_NAME = "decrypt";

        //Local decryptor cache
        private static readonly Dictionary<string, Decryptor> _Cache = new();
        private static DateTime _CacheLastMaintenance = DateTime.MinValue;

        //Js file cache directory
        private static readonly string _CacheDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.cache\\OnlineVideos\\Youtube";

        private readonly Helpers.WebViewHelper _Webview;
        private readonly Decryptor _Decryptor;


        public string SignatureTimestamp { get => this._Decryptor?.SignatureTimestamp; }
        public DateTime LoadTimestamp { get { return this._Decryptor?.LoadTime ?? DateTime.MinValue; } }

        public DateTime LastUseTimestamp
        {
            get
            {
                return this._Decryptor?.LastUse ?? DateTime.MinValue;
            }
            set
            {
                if (this._Decryptor != null)
                {
                    new FileInfo(this._Decryptor.CacheFilepath).LastAccessTime = DateTime.Now;
                    this._Decryptor.LastUse = DateTime.Now;
                }
            }
        }


        public YoutubeSignatureDecryptor(string strVideoWebContent, Helpers.WebViewHelper webview)
        {
            lock (_Cache)
            {
                if (string.IsNullOrWhiteSpace(strVideoWebContent))
                    throw new ArgumentNullException();

                Match m = _RegexPlayerTs.Match(strVideoWebContent);
                if (!m.Success)
                    throw new Exception("Failed to get player STS.");

                //STS of the js file
                string strSts = m.Groups["sts"].Value;

                Log.Debug("[YoutubeSignatureDecryptor] JS player STS: {0}", strSts);

                m = _RegexPlayerJsUrl.Match(strVideoWebContent);
                if (!m.Success)
                    throw new Exception("Failed to extract player js url.");

                //ID of the js file
                string strId = m.Groups["id"].Value;

                Log.Debug("[YoutubeSignatureDecryptor] JS player id: {0}", strId);

                //Cache files full path
                string strCacheFile = _CacheDir + '\\' + strId + ".json";

                //Check for cache directory
                if (!Directory.Exists(_CacheDir))
                    Directory.CreateDirectory(_CacheDir);

                //Try to load existing decryptor
                if (!_Cache.TryGetValue(strId, out Decryptor dec))
                {
                    if (File.Exists(strCacheFile))
                    {
                        try
                        {
                            dec = Newtonsoft.Json.JsonConvert.DeserializeObject<Decryptor>(File.ReadAllText(strCacheFile));
                            if (dec.Version != _DECRYPTOR_VERSION_CURRENT)
                                dec = null;
                            else
                            {
                                //Put decryptor to the local cache
                                _Cache[strId] = dec;

                                dec.CacheFilepath = strCacheFile;
                                dec.LoadTime = DateTime.Now;

                                Log.Debug("[YoutubeSignatureDecryptor] Decryptor loaded from file cache.");
                            }
                        }
                        catch
                        {
                            dec = null;
                            Log.Error("[YoutubeSignatureDecryptor] Failed to load decryptor from cache.");
                        }
                    }
                }
                else
                    Log.Debug("[YoutubeSignatureDecryptor] Decryptor loaded from cache.");

                //Check for existing Expire time
                if (dec == null || DateTime.Now >= dec.Expires)
                {
                    #region Http request

                    Log.Debug("[YoutubeSignatureDecryptor] Checking for new JS player version...");

                    HttpWebResponse resp;
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://www.youtube.com" + m.Groups["url"].Value);
                    wr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36";
                    wr.Timeout = 15000;
                    wr.Accept = "*/*";
                    wr.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

                    //Append ModifiedSince to check whether the file is modified or not
                    if (dec == null)
                        dec = new Decryptor() { SignatureTimestamp = strSts, LoadTime = DateTime.Now, CacheFilepath = strCacheFile };
                    else if (dec.TimeStamp > DateTime.MinValue)
                        wr.IfModifiedSince = dec.TimeStamp.ToUniversalTime();

                    //Get web response
                    try
                    {
                        resp = (HttpWebResponse)wr.GetResponse();
                    }
                    catch (WebException ex)
                    {
                        //NotModified rises exception
                        resp = (HttpWebResponse)ex.Response;
                    }


                    bool bStoreToFileCache = false;

                    //Update Expires timestamp
                    if (!DateTime.TryParse(resp.Headers["Expires"], out DateTime dtExpires))
                        dtExpires = DateTime.MinValue;
                    else if (dtExpires > dec.Expires)
                    {
                        dec.Expires = dtExpires;
                        bStoreToFileCache = true;
                    }

                    switch (resp.StatusCode)
                    {
                        case HttpStatusCode.OK:

                            # region Download the js file

                            Log.Debug("[YoutubeSignatureDecryptor] Downloading new js player file...");
                            Stream stream;
                            if (resp.ContentEncoding.IndexOf("gzip", StringComparison.OrdinalIgnoreCase) >= 0)
                                stream = new GZipStream(resp.GetResponseStream(), CompressionMode.Decompress);
                            else if (resp.ContentEncoding.IndexOf("deflate", StringComparison.OrdinalIgnoreCase) >= 0)
                                stream = new DeflateStream(resp.GetResponseStream(), CompressionMode.Decompress);
                            else
                                stream = resp.GetResponseStream();

                            const int _BUFFER_SIZE = 1024 * 16;
                            byte[] buffer = new byte[_BUFFER_SIZE];
                            int iRead;
                            int iUTF8ToRead = 0;
                            int iUTF8Char = 0;
                            StringBuilder sb = new(1024 * 1024 * 3);

                            do
                            {
                                iRead = stream.Read(buffer, 0, _BUFFER_SIZE);
                                AppendUTF8Buffer(sb, buffer, 0, iRead, ref iUTF8ToRead, ref iUTF8Char);
                            }
                            while (iRead > 0);

                            string strJsBase = sb.ToString();

                            #endregion

                            #region Try to find js nsig function

                            for (int i = 0; i < _RegexPlayerJsFunctions.Length; i++)
                            {
                                m = _RegexPlayerJsFunctions[i].Match(strJsBase);
                                if (m.Success)
                                    break;
                            }
                            if (!m.Success)
                                throw new Exception("Failed to locate js nsig function.");

                            Log.Debug("[YoutubeSignatureDecryptor] JS nsig function found.");

                            //Build our js code
                            System.Text.RegularExpressions.Group grFcCode = m.Groups[0];
                            System.Text.RegularExpressions.Group grFcName = m.Groups["fname"];
                            int iIdxStart = grFcName.Index + grFcName.Length;
                            sb.Clear();
                            sb.Append(_JS_FUNCTION_NAME);
                            sb.Append(strJsBase, iIdxStart, grFcCode.Index + grFcCode.Length - iIdxStart);
                            dec.NSignatureJsCode = sb.ToString();

                            #endregion

                            # region Try to find js signature function

                            m = _RegexPlayerJsSigFunction.Match(strJsBase);
                            if (!m.Success)
                                throw new Exception("Failed to locate js signature function.");

                            string strFcCode = m.Groups[0].Value;
                            string strFcName = m.Groups["fname"].Value;
                            string strFcNameSub = m.Groups["fnamesub"].Value.Replace("$", "\\$");
                            m = Regex.Match(strJsBase, string.Format(_JS_SUB_FUNCTION_REGEX, strFcNameSub));
                            if (!m.Success)
                                throw new Exception("Failed to locate js signature sub function.");

                            Log.Debug("[YoutubeSignatureDecryptor] JS signature function found.");

                            //Build our js code
                            sb.Clear();
                            sb.Append(m.Groups[0].Value);
                            sb.Append(_JS_FUNCTION_NAME);
                            sb.Append(strFcCode, strFcName.Length, strFcCode.Length - strFcName.Length);
                            dec.SignatureJsCode = sb.ToString();

                            #endregion

                            bStoreToFileCache = true;
                            break;

                        case HttpStatusCode.NotModified:
                            //Nothing to download
                            Log.Debug("[YoutubeSignatureDecryptor] JS player file is not modified.");
                            break;

                        default:
                            throw new Exception("Failed to connect to youtube server.");
                    }

                    if (bStoreToFileCache)
                    {
                        //Write the decryptor to the file cache
                        dec.TimeStamp = DateTime.Now;
                        File.WriteAllText(strCacheFile, Newtonsoft.Json.JsonConvert.SerializeObject(dec));
                        Log.Debug("[YoutubeSignatureDecryptor] Decryptor stored to file cache.");
                    }

                    //Put decryptor to the local cache
                    _Cache[strId] = dec;

                    #endregion
                }

                //Complete the initialization
                new FileInfo(strCacheFile).LastAccessTime = DateTime.Now;
                dec.LastUse = DateTime.Now;
                this._Decryptor = dec;
                this._Webview = webview;

                #region Cache maintenance
                if ((DateTime.Now - _CacheLastMaintenance).TotalDays >= 1)
                {
                    Log.Debug("[YoutubeSignatureDecryptor] Cache maintenance...");

                    //Local cache
                    _Cache.Where(d => (DateTime.Now - d.Value.LastUse).TotalDays >= 7).ToList().ForEach(d =>
                    {
                        _Cache.Remove(d.Key);
                        Log.Debug("[YoutubeSignatureDecryptor] Cache maintenance: ID '{0}' removed", d.Key);
                    });

                    //File cache
                    string[] files = Directory.GetFiles(_CacheDir, "*.json");
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo fi = new(files[i]);
                        if ((DateTime.Now - fi.LastAccessTime).TotalDays >= 90)
                        {
                            try
                            {
                                File.Delete(fi.FullName);
                                Log.Debug("[YoutubeSignatureDecryptor] Cache maintenance: File '{0}' removed.", fi.FullName);
                            }
                            catch
                            {
                                Log.Error("[YoutubeSignatureDecryptor] Cache maintenance: Failed to remove '{0}'.", fi.FullName);
                            }
                        }
                    }

                    _CacheLastMaintenance = DateTime.Now;
                }
                #endregion
            }
        }

        public string DecryptNSignature(string strSig)
        {
            string strResult = this._Webview.ExecuteFunc(this._Decryptor.NSignatureJsCode + _JS_FUNCTION_NAME + "(\"" + strSig + "\");");

            if (string.IsNullOrWhiteSpace(strResult) || strResult == "\"\"" || strResult == "null")
                Log.Error("[DecryptNSignature] Failed to execute the js function. Signature: " + strSig);

            return strResult.Trim('\"');
        }

        public string DecryptSignature(string strSig)
        {
            string strResult = this._Webview.ExecuteFunc(this._Decryptor.SignatureJsCode + _JS_FUNCTION_NAME + "(\"" + strSig + "\");");

            if (string.IsNullOrWhiteSpace(strResult) || strResult == "\"\"" || strResult == "null")
                Log.Error("[DecryptSignature] Failed to execute the js function. Signature: " + strSig);

            return strResult.Trim('\"');
        }

        private static StringBuilder AppendUTF8Buffer(StringBuilder self, byte[] buffer, int iIdxFrom, int iLength, ref int iUTF8ToRead, ref int iUTF8Char)
        {
            if (iUTF8ToRead < 0 || iUTF8ToRead > 3)
                throw new ArgumentException("[AppendUTF8Buffer] Invalid argument 'iUTF8ToRead'.");

            if (buffer == null)
                throw new ArgumentNullException("[AppendUTF8Buffer] Invalid argument 'buffer'.");

            if (iIdxFrom < 0 || iIdxFrom > buffer.Length)
                throw new ArgumentException("[AppendUTF8Buffer] Invalid argument 'iIdxFrom'.");

            int iIdxTo = iIdxFrom + iLength;

            if (iLength < 0 || iIdxTo > buffer.Length)
                throw new ArgumentException("[AppendUTF8Buffer] Invalid argument 'iLength'.");

            while (iIdxFrom < iIdxTo)
            {
                //UTF8 stream decoding
                int iVal = buffer[iIdxFrom++];
                if (iUTF8ToRead == 0)
                {
                    // Range    Byte 1 	    Byte 2 	    Byte 3 	    Byte 4
                    //------------------------------------------------------
                    if (iVal < 0x80)                // 00007F   0xxxxxxx
                        self.Append((char)iVal);
                    else if ((iVal & 0xE0) == 0xC0) // 0007FF   110xxxxx    10xxxxxx
                    {
                        iUTF8Char = (iVal & 0x1F);
                        iUTF8ToRead = 1;
                    }
                    else if ((iVal & 0xF0) == 0xE0) // 00FFFF   1110xxxx	10xxxxxx	10xxxxxx
                    {
                        iUTF8Char = (iVal & 0x0F);
                        iUTF8ToRead = 2;
                    }
                    else if ((iVal & 0xF8) == 0xF0) // 10FFFF   11110xxx	10xxxxxx	10xxxxxx	10xxxxxx
                    {
                        iUTF8Char = (iVal & 0x7);
                        iUTF8ToRead = 3;
                    }
                    else
                        throw new Exception("[AppendUTF8Buffer] Invalid UTF8 encoding.");
                }
                else if ((iVal & 0xC0) == 0x80) //10xxxxxx
                {
                    iUTF8Char <<= 6;
                    iUTF8Char |= (iVal & 0x3F);
                    if (--iUTF8ToRead == 0)
                    {
                        if (iUTF8Char >= 0x10000)
                        {
                            //surrogate code
                            iUTF8Char -= 0x10000;
                            self.Append((char)((iUTF8Char >> 10) | 0xd800));
                            self.Append((char)((iUTF8Char & 0x03FF) | 0xdc00));
                        }
                        else
                            self.Append((char)iUTF8Char);
                    }
                }
                else
                    throw new Exception("[AppendUTF8Buffer] Invalid UTF8 encoding.");
            }

            return self;
        }
    }
}
