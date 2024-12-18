﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using OnlineVideos.Hoster;
using OnlineVideos.Sites;

namespace OnlineVideos.CrossDomain
{
    internal class PluginLoader : MarshalByRefObject
    {
        static readonly string _onlineVideosMainDllName = Assembly.GetExecutingAssembly().GetName().Name;
        readonly Dictionary<String, Type> _utils = new Dictionary<String, Type>();
        readonly Dictionary<String, HosterBase> _hostersByName = new Dictionary<String, HosterBase>();
        readonly Dictionary<String, HosterBase> _hostersByDns = new Dictionary<String, HosterBase>();

        public PluginLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
        }

        Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // this should only be called to resolve OnlineVideos.dll -> return it regardless of the version, only the name "OnlineVideos"
            AssemblyName an = new AssemblyName(args.Name);
            var asm = (sender as AppDomain).GetAssemblies().FirstOrDefault(a => a.GetName().Name == an.Name);
            return asm;
        }

        internal void LoadAllSiteUtilDlls(string path)
        {
            var assemblies = new Dictionary<Assembly, DateTime>();
            assemblies.Add(Assembly.GetExecutingAssembly(), Helpers.FileUtils.RetrieveLinkerTimestamp(new Uri(Assembly.GetExecutingAssembly().EscapedCodeBase).LocalPath));
            if (Directory.Exists(path))
            {
                foreach (string dll in Directory.GetFiles(path, "OnlineVideos.Sites.*.dll"))
                {
                    try
                    {
                        assemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(dll)), Helpers.FileUtils.RetrieveLinkerTimestamp(dll));
                    }
                    catch (Exception dllLoadException)
                    {
                        Log.Warn("Error loading {0}: {1}", dll, dllLoadException.Message);
                    }
                }
            }
            // search all assemblies for exported SiteUtilBase and HosterBase implementing non abstract classes
            foreach (var assembly in assemblies)
            {
                try
                {
                    var versionInfo = assembly.Key.GetName().Version.ToString();
                    var extendVersionInfoAttrs = assembly.Key.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
                    if (extendVersionInfoAttrs.Length > 0)
                        versionInfo = ((AssemblyInformationalVersionAttribute)extendVersionInfoAttrs[0]).InformationalVersion;

                    Log.Info("Looking for SiteUtils and Hosters in {0} [Version: {1}, Compiled: {2}]",
                        assembly.Key.ManifestModule.Name,
                        versionInfo,
                        assembly.Value.ToString("yyyy-MM-dd HH:mm:ss"));

                    Type[] typeArray = assembly.Key.GetExportedTypes();
                    foreach (Type type in typeArray)
                    {
                        if (type.BaseType != null && type.IsSubclassOf(typeof(SiteUtilBase)) && !type.IsAbstract)
                        {
                            string shortName = type.Name;
                            if (shortName.EndsWith("Util")) shortName = shortName.Substring(0, shortName.Length - 4);

                            Type alreadyAddedType = null;
                            if (!_utils.TryGetValue(shortName, out alreadyAddedType))
                            {
                                _utils.Add(shortName, type);
                            }
                            else
                            {
                                // use the type from the assembly with the latest CompileTime
                                if (assemblies[alreadyAddedType.Assembly] < assembly.Value)
                                {
                                    _utils[shortName] = type;
                                    Log.Warn(string.Format("Duplicate SiteUtil '{0}'. Using the one from '{1}', because DLL has newer compile time than '{2}'.",
                                        shortName,
                                        type.Assembly.GetName().Name.Replace("OnlineVideos.Sites.", ""),
                                        alreadyAddedType.Assembly.GetName().Name.Replace("OnlineVideos.Sites.", "")));
                                }
                                else
                                {
                                    Log.Warn(string.Format("Duplicate SiteUtil '{0}'. Using the one from '{1}', because DLL has newer compile time than '{2}'.",
                                        shortName,
                                        alreadyAddedType.Assembly.GetName().Name.Replace("OnlineVideos.Sites.", ""),
                                        type.Assembly.GetName().Name.Replace("OnlineVideos.Sites.", "")));
                                }
                            }
                        }
                        else if (type.BaseType != null && type.IsSubclassOf(typeof(HosterBase)) && !type.IsAbstract)
                        {
                            string shortName = type.Name.ToLower();
                            HosterBase alreadyAddedHoster = null;
                            if (!_hostersByName.TryGetValue(shortName, out alreadyAddedHoster))
                            {
                                HosterBase hb = (HosterBase)Activator.CreateInstance(type);
                                hb.Initialize();
                                _hostersByName.Add(shortName, hb);
                                if (!_hostersByDns.ContainsKey(hb.GetHosterUrl().ToLower())) _hostersByDns.Add(hb.GetHosterUrl().ToLower(), hb);
                            }
                            else
                            {
                                // use the type from the assembly with the latest CompileTime
                                if (assemblies[alreadyAddedHoster.GetType().Assembly] < assembly.Value)
                                {
                                    HosterBase hb = (HosterBase)Activator.CreateInstance(type);
                                    hb.Initialize();
                                    _hostersByName[shortName] = hb;
                                    Log.Warn(string.Format("Duplicate Hoster '{0}'. Using the one from '{1}', because DLL has newer compile time than '{2}'.",
                                        shortName,
                                        type.Assembly.GetName().Name.Replace("OnlineVideos.Sites.", ""),
                                        alreadyAddedHoster.GetType().Assembly.GetName().Name.Replace("OnlineVideos.Sites.", "")));
                                }
                                else
                                {
                                    Log.Warn(string.Format("Duplicate Hoster '{0}'. Using the one from '{1}', because DLL has newer compile time than '{2}'.",
                                        shortName,
                                        alreadyAddedHoster.GetType().Assembly.GetName().Name.Replace("OnlineVideos.Sites.", ""),
                                        type.Assembly.GetName().Name.Replace("OnlineVideos.Sites.", "")));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("Error loading SiteUtils and Hosters: {0} ", ex.Message);
                }
            }
            Log.Info("Found {0} SiteUtils and {1} Hosters in {2} assemblies", _utils.Count, _hostersByName.Count, assemblies.Count);
        }

        public bool UtilExists(string shortName)
        {
            if (string.IsNullOrEmpty(shortName)) return false;
            else return _utils.ContainsKey(shortName);
        }

        public SiteUtilBase CreateUtilFromShortName(string name, SiteSettings settings)
        {
            Type result = null;
            if (_utils.TryGetValue(name, out result))
            {
                SiteUtilBase util = null;
                try
                {
                    util = (SiteUtilBase)Activator.CreateInstance(result);
                    util.Initialize(settings);
                    return util;
                }
                catch (Exception ex)
                {
                    Log.Warn("SiteUtil '{0}' is faulty or not compatible with this build of OnlineVideos: {1}", name, ex.Message);
                    return util;
                }
            }
            else
            {
                Log.Warn(string.Format("SiteUtil with name: {0} not found!", name));
                return null;
            }
        }

        public SiteUtilBase CloneFreshSiteFromExisting(SiteUtilBase site)
        {
            // create new instance of this site with reset settings
            SerializableSettings s = new SerializableSettings() { Sites = new BindingList<SiteSettings>() };
            s.Sites.Add(site.Settings);
            MemoryStream ms = new MemoryStream();
            s.Serialize(ms);
            ms.Position = 0;
            SiteSettings originalSettings = SerializableSettings.Deserialize(new StreamReader(ms))[0];
            var res = CreateUtilFromShortName(site.Settings.UtilName, originalSettings);
            return res;
        }

        public IList<SiteSettings> CreateSiteSettingsFromXml(string siteXml)
        {
            return SerializableSettings.Deserialize(new StringReader(siteXml));
        }

        public string GetRequiredDllForUtil(string name)
        {
            Type result = null;
            if (_utils.TryGetValue(name, out result))
            {
                string dll = result.Assembly.GetName().Name;
                return dll != _onlineVideosMainDllName ? dll : null;
            }
            else
            {
                Log.Error(string.Format("SiteUtil with name: {0} not found!", name));
                return null;
            }
        }

        public string[] GetAllUtilNames()
        {
            string[] names = new string[_utils.Count];
            _utils.Keys.CopyTo(names, 0);
            Array.Sort(names);
            return names;
        }

        public HosterBase GetHoster(string name)
        {
            HosterBase hb = null;
            if (_hostersByName.TryGetValue(name.ToLower(), out hb)) return hb;
            return null;
        }

        public List<HosterBase> GetAllHosters()
        {
            return _hostersByName.Values.OrderByDescending(hb => hb.UserPriority).ToList();
        }

        public bool ContainsHoster(string name)
        {
            return _hostersByName.ContainsKey(name);
        }

        public bool ContainsHosters(Uri uri)
        {
            return _hostersByDns.ContainsKey(uri.Host.Replace("www.", ""));
        }

        #region MarshalByRefObject overrides
        public override object InitializeLifetimeService()
        {
            // In order to have the lease across appdomains live forever, we return null.
            return null;
        }
        #endregion
    }
}
