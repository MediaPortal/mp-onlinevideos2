# MP2 client validation – 2026-10-15

## Setup

MediaPortal 2.5 with its installed OnlineVideos client assembly 2.3.1.0 (assembly reports source commit `091328ef739d476def5d1aad9c992e984d3ec263`) was tested against the isolated .NET 10 service.

The server endpoint is hardcoded in this client. A separate copy of the installed MP2 client was made. Exactly one URL literal in the copied `OnlineVideos.dll` constructor was changed from `https://ov2.team-mediaportal.com/api/OnlineVideos` to `http://127.0.0.1:5087/api/OnlineVideos`. A loopback-only SSH tunnel forwarded that address to the isolated server's loopback port 5086. No certificate validation was disabled and no hosts-file or public proxy change was made. All client protocol, DTO and JSON code remained the installed version's code.

This is consequently a test-address-only copy of the installed client, not a byte-identical unmodified DLL. The original installed DLL was not edited; its SHA256 before and after testing was `671a8dcd5327f5943f6f42269fc23af6a2c6bc6409132c99d3402d34f0e122e1`.

## Real MP2 GUI result

MP2 Settings → Plugins → OnlineVideos → Manage Sites successfully called the isolated service's `GetSitesOverview` endpoint. The GUI displayed the 16 returned sites, including YouTube, ARD Mediathek and ZDF Mediathek. The tunnel request log and the client log confirmed the catalogue request went to the isolated .NET 10 service. An initial launch accidentally selected the registered original MP2 installation; that session was closed before catalogue validation and was not counted as a successful server test.

Adding ARD Mediathek did not result in a module download. The local installation had no usable site modules, and its client version is 2.3.1. The official `MPEI/update.xml` listed versions through 2.9.2 at the time of testing. The existing updater requires matching major and a local minor version greater than or equal to the advertised minor; this prevents 2.3.1 from updating modules. The version check was not bypassed. A video playback result is therefore not claimed.

The MP2 HID input plugin also logged pre-existing NullReferenceException messages; mouse navigation allowed the catalogue test to proceed. These input errors were not treated as server compatibility failures.

## Installed client API result

A small .NET Framework x86 harness loaded the same test-address-only copy of the installed client DLL and called its real public client methods. It did not reimplement serialization or HTTP calls:

| Check | Result |
|---|---|
| GetSitesOverview | 16 entries deserialized |
| GetDllsOverview | 3 entries deserialized |
| ARD Mediathek GetSiteXml | XML parsed with `Site` root |
| GetSiteIcon | PNG 512×512, 7,940 bytes |
| GetSiteBanner | PNG 1200×400, 169,404 bytes |
| GetReports | 2 entries deserialized |
| GetDll | 238,080 bytes, PE header validated; not executed |

See `../WebServiceCore.CompatibilityTests/MP2-NATIVE-RESULTS.txt` for the captured output. These results extend the earlier generic HTTP tests with the actual installed MP2 client protocol implementation.

## Cleanup and remaining validation

The MP2 test copy was closed, the backed-up local OnlineVideos settings restored, the SSH tunnel closed and the isolated server stopped. The original server service remained active and returned HTTP 200; its binary hash remained unchanged.

Before declaring full end-to-end OnlineVideos compatibility, test a current compatible MP2/OnlineVideos plugin with current site modules, then browse categories and play a real stream. MP1, account/admin workflows, real email delivery and Debian 13 execution also remain outside the completed tests. No production switch was performed.
