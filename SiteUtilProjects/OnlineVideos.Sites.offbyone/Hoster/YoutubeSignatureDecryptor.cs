﻿using System;
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
        private static readonly Regex _RegexPlayerJsUrl = new("href=\"(?<url>/s/player/(?<id>.+?)/player_.+?base\\.js)\"", RegexOptions.Compiled);
        private static readonly Regex _RegexPlayerJsFunction = new("(?<fname>[a-z,A-Z,0-9]+)=function\\(a\\){var\\s+b=a\\.split\\(\"\"\\)(?s:.)+?return\\s+b\\.join\\(\"\"\\)}", RegexOptions.Compiled);
        private static readonly Regex _RegexPlayerJsSigFunction = new("(?<fname>[a-z,A-Z,0-9]+)=function\\(a\\){a=a.split\\(\"\"\\);(?<fnamesub>[a-z,A-Z,0-9]+)\\.(?s:.)+?return\\s+a\\.join\\(\"\"\\)};", RegexOptions.Compiled);
        private const string _JS_SUB_FUNCTION_REGEX = "var {0}={{(?s:.)+?}};";

        private readonly Jurassic.ScriptEngine _JsEngine;
        private readonly Jurassic.CompiledScript _JsCompiledNsigScript;
        private readonly Jurassic.CompiledScript _JsCompiledSignatureScript;

        private static readonly Dictionary<string, DateTime> _JsExpiresCache = new();

        //Js file cache directory
        private static readonly string _CacheDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\.cache\\OnlineVideos\\Youtube";

        public YoutubeSignatureDecryptor(string strVideoWebContent)
        {
            lock (_JsExpiresCache)
            {
                if (string.IsNullOrWhiteSpace(strVideoWebContent))
                    throw new ArgumentNullException();

                Match m = _RegexPlayerJsUrl.Match(strVideoWebContent);
                if (!m.Success)
                    throw new Exception("Failed to extract player js url.");

                //ID of the js file
                string strId = m.Groups["id"].Value;

                Log.Debug("[YoutubeSignatureDecryptor] JS player id: {0}", strId);

                //Cache files full path
                string strCacheFileNsig = _CacheDir + '\\' + strId + "_nsig.js";
                string strCacheFileSignature = _CacheDir + '\\' + strId + "_sig.js";

                //Js codes
                string strJsNsigCode = null;
                string strJsSignatureCode = null;

                //Check for cache directory
                if (!Directory.Exists(_CacheDir))
                    Directory.CreateDirectory(_CacheDir);

                //Check for existing Expire time
                if (!_JsExpiresCache.TryGetValue(strId, out DateTime dtExpires) || DateTime.Now >= dtExpires)
                {
                    #region Http request

                    Log.Debug("[YoutubeSignatureDecryptor] Checking for new JS player vsrsion...");

                    HttpWebResponse resp;
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://www.youtube.com" + m.Groups["url"].Value);
                    wr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.131 Safari/537.36";
                    wr.Timeout = 15000;
                    wr.Accept = "*/*";
                    wr.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

                    if (File.Exists(strCacheFileNsig))
                    {
                        //Append ModifiedSince to check whether the file is modifid or not
                        FileInfo fi = new(strCacheFileNsig);
                        wr.IfModifiedSince = fi.LastWriteTimeUtc;
                    }

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

                    if (!DateTime.TryParse(resp.Headers["Expires"], out dtExpires))
                        dtExpires = DateTime.MinValue;

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

                            m = _RegexPlayerJsFunction.Match(strJsBase);
                            if (!m.Success)
                                throw new Exception("Failed to locate js nsig function.");

                            Log.Debug("[YoutubeSignatureDecryptor] JS nsig function found.");

                            //Build our js code
                            sb.Clear();
                            sb.Append(m.Groups[0].Value);
                            sb.Append("; sig=");
                            sb.Append(m.Groups["fname"].Value);
                            sb.Append("(sig);");
                            strJsNsigCode = sb.ToString();

                            #endregion

                            # region Try to find js signature function

                            m = _RegexPlayerJsSigFunction.Match(strJsBase);
                            if (!m.Success)
                                throw new Exception("Failed to locate js signature function.");

                            string strFcCode = m.Groups[0].Value;
                            string strFcName = m.Groups["fname"].Value;
                            string strFcNameSub = m.Groups["fnamesub"].Value;
                            m = Regex.Match(strJsBase, string.Format(_JS_SUB_FUNCTION_REGEX, strFcNameSub));
                            if (!m.Success)
                                throw new Exception("Failed to locate js signature sub function.");

                            Log.Debug("[YoutubeSignatureDecryptor] JS signature function found.");

                            //Build our js code
                            sb.Clear();
                            sb.Append(m.Groups[0].Value);
                            sb.Append(strFcCode);
                            sb.Append("sig=");
                            sb.Append(strFcName);
                            sb.Append("(sig);");
                            strJsSignatureCode = sb.ToString();

                            #endregion

                            //Write the prepared functions to the cache files
                            File.WriteAllText(strCacheFileNsig, strJsNsigCode);
                            File.WriteAllText(strCacheFileSignature, strJsSignatureCode);

                            break;

                        case HttpStatusCode.NotModified:
                            //Nothing to download
                            Log.Debug("[YoutubeSignatureDecryptor] JS player file is not modified.");
                            break;

                        default:
                            throw new Exception("Failed to connect to youtube server.");
                    }

                    //Remember expire time
                    if (dtExpires > DateTime.MinValue)
                        _JsExpiresCache[strId] = dtExpires;

                    #endregion
                }

                //Load cached functions from cache if needed
                if (strJsNsigCode == null)
                {
                    strJsNsigCode = File.ReadAllText(strCacheFileNsig);
                    strJsSignatureCode = File.ReadAllText(strCacheFileSignature);
                    Log.Debug("[YoutubeSignatureDecryptor] JS function files loaded from cache.");
                }

                //Initialize compiled scripts
                this._JsEngine = new Jurassic.ScriptEngine();
                this._JsCompiledNsigScript = Jurassic.CompiledScript.Compile(new Jurassic.StringScriptSource(strJsNsigCode));
                this._JsCompiledSignatureScript = Jurassic.CompiledScript.Compile(new Jurassic.StringScriptSource(strJsSignatureCode));
            }
        }

        public string DecryptNSignature(string strSig)
        {
            this._JsEngine.SetGlobalValue("sig", strSig);
            this._JsCompiledNsigScript.Execute(this._JsEngine);
            return (string)this._JsEngine.GetGlobalValue("sig");
        }

        public string DecryptSignature(string strSig)
        {
            this._JsEngine.SetGlobalValue("sig", strSig);
            this._JsCompiledSignatureScript.Execute(this._JsEngine);
            return (string)this._JsEngine.GetGlobalValue("sig");
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
