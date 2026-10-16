# MP2 HTTPS client compatibility test — 16 October 2026

MP2 2.5 (2.5.2401.14759, x86) and its installed OnlineVideos 2.4.0.1 integration were tested against `https://onlinevideos.example.invalid` after the follow-up server hardening.

The native .NET Framework harness loaded the installed assemblies and changed only the service instance endpoint in memory. Normal certificate validation remained enabled. The original installed `OnlineVideos.dll` was not patched; its SHA-256 was `467f2bf41ef62be76e09557cceab0a286a1a3287a5e4480cd40f09bfc749c394`.

Successful checks: 96 MP2 integration types loaded, embedded bitmap resource decoded, 16 server sites and three DLL entries deserialized, ARD Mediathek XML parsed, 512×512 icon and 1200×400 banner decoded, two reports deserialized, and a 134,656-byte PE package downloaded as bytes without executing it. Raw diagnostic output is in `../WebServiceCore.CompatibilityTests/MP2-HTTPS-RESULTS-20261016.txt`.

A separate MP2 application copy was pointed at the HTTPS test endpoint. Its GUI started successfully and Settings → Plugins → OnlineVideos → Manage Sites displayed the remote catalogue. A local NPOStart entry also appeared in that management list; it is not an additional server catalogue entry.

Full video playback is **not verified**. The installed plugin's unmodified updater reports online version **2.9.2.0** versus local **2.4.0.1**, and `VersionCompatible` is **false**. `Updater.UpdateSites` returns before adding/updating providers in that state. The empty configured-site view therefore cannot progress to an ARD playback test. The version guard, certificate checks and package validation were not bypassed. Provider runtime/codec/stream behavior cannot be inferred from successful server API reads.

Before claiming full MP2 compatibility, provide a genuinely compatible MP2 plugin build, repeat provider installation through its normal update path, then test category navigation, video listing and actual playback. The server's restricted moderation and pending-publication workflows also require publishing tools to handle authentication and HTTP 202 correctly; legacy anonymous reports do not gain moderation rights.
