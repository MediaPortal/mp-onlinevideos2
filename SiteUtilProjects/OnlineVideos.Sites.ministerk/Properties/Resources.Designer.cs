﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineVideos.Sites.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OnlineVideos.Sites.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function myZoom() {
        ///    document.getElementById(&apos;katsomo-navi&apos;).style.zIndex = 0;
        ///    document.getElementById(&apos;video0_desktop-player&apos;).style.position = &apos;fixed&apos;;
        ///    document.getElementById(&apos;video0_desktop-player&apos;).style.zIndex = 101;
        ///    document.getElementById(&apos;video0_desktop-player&apos;).style.left = &quot;0px&quot;;
        ///    document.getElementById(&apos;video0_desktop-player&apos;).style.top = &quot;0px&quot;;
        ///    document.getElementById(&apos;video0_desktop-player&apos;).style.width = (window.innerWidth) + &apos;px&apos;;
        ///    document.getElementById(&apos;video0_de [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Katsomo {
            get {
                return ResourceManager.GetString("Katsomo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function switchProfile(profiletoken) {
        ///    var proflinks = document.querySelectorAll(&apos;a.profile-link&apos;);
        ///    setTimeout(function () {
        ///        for (var i = 0; i &lt; proflinks.length; i++) {
        ///            if (proflinks[i].href.indexOf(profiletoken) !== -1) {
        ///                proflinks[i].click();
        ///            };
        ///        };
        ///        setTimeout(function () {
        ///            switchProfileCallback(&quot;{}&quot;)
        ///        }, 1500);
        ///    }, 500);
        ///}
        ///
        ///function switchProfileCallback(data) {
        ///    window.location.href = &quot;https:/ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NetflixJs {
            get {
                return ResourceManager.GetString("NetflixJs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to function myLogin(u, p) {
        ///    $(&apos;#LoginForm_email&apos;).val(u);
        ///    $(&apos;#LoginForm_password&apos;).val(p);
        ///    $(&apos;#submit-login-password&apos;).click();
        ///}
        ///
        ///var __url = &quot;dummy&quot;;
        ///
        ///function myPlay() {
        ///    var __m = $(&apos;div[class=&quot;react-play-button large&quot;]&apos;).first();
        ///    if (__m.length &gt; 0) {
        ///        __m.click();
        ///    } else {
        ///        setTimeout(&quot;myPlay()&quot;, 250);
        ///    }
        ///};.
        /// </summary>
        internal static string ViaplayPlayMovieJs {
            get {
                return ResourceManager.GetString("ViaplayPlayMovieJs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to var steps = [-10000000, -7200, -3600, -1800, -900, -600, -300, -180, -60, -30, -15, 0, 15, 30, 60, 180, 300, 600, 900, 1800, 3600, 7200, 10000000];
        ///var stepNames = [&apos;Start&apos;, &apos;-2h&apos;, &apos;-1h&apos;, &apos;-30m&apos;, &apos;-15m&apos;, &apos;-10m&apos;, &apos;-5m&apos;, &apos;-3m&apos;, &apos;-1m&apos;, &apos;-30s&apos;, &apos;-15s&apos;, &apos; &apos;, &apos;+15s&apos;, &apos;+30s&apos;, &apos;+1m&apos;, &apos;+3m&apos;, &apos;+5m&apos;, &apos;+10m&apos;, &apos;+15m&apos;, &apos;+30m&apos;, &apos;+1h&apos;, &apos;+2h&apos;, &apos;End&apos;];
        ///var skipIndex = 11;
        ///var minIndex = 0;
        ///var maxIndex = steps.length - 1;
        ///var skipTimeout = 3000;
        ///var skipTimer = null;
        ///
        ///var contentDiv = document.getElementById(&apos;content [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ViaplayVideoControlJs {
            get {
                return ResourceManager.GetString("ViaplayVideoControlJs", resourceCulture);
            }
        }
    }
}
