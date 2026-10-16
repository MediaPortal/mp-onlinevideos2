# OnlineVideos server — .NET 10 with server-side security fixes

Prepared on 15 October 2026. Based on [MediaPortal/mp-onlinevideos2](https://github.com/MediaPortal/mp-onlinevideos2), upstream commit `dfcf36e5661ee45019bc8a48604cee48a87c9c09`.

This is the complete server source package, including the web frontend, required language assets, automated tests and English migration/security documentation. It is not the MP2 desktop plugin or the separate PHP LogoManager project.

**Read [SECURITY-HARDENING.md](WebServiceCore/SECURITY-HARDENING.md) before deployment.** The write API is intentionally stricter than the legacy implementation. Password email is disabled; anonymous reports cannot moderate providers; DLL uploads require publisher approval and a separate local review step. Sensitive operations require HTTPS.

## Build and test

Prerequisite: .NET 10 SDK (validated with 10.0.402). From the extracted package directory:

```sh
dotnet restore WebServiceCore/WebServiceCore.csproj --locked-mode
dotnet build WebServiceCore/WebServiceCore.csproj -c Release --no-restore
dotnet run --project WebServiceCore.CompatibilityTests -c Release
dotnet run --project WebServiceCore.SecurityTests -c Release
dotnet publish WebServiceCore/WebServiceCore.csproj -c Release -r linux-x64 --self-contained true -o publish
```

`OnlineVideos.Server.sln` groups the three server/test projects. The tests use temporary synthetic databases and files; no live credentials or database are needed. Do not substitute a production connection string into a test fixture.

## Test deployment

Use a consistent SQLite backup and a separate copy of the public provider assets. The package contains neither. Configure the application through private settings or environment variables:

```sh
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://127.0.0.1:5086
export ConnectionStrings__OnlineVideos='Data Source=/srv/onlinevideos-test/test.db3'
export Security__PrivateDataPath=/var/lib/onlinevideos-test/private
```

Run the self-contained `publish/WebServiceCore` executable from its published directory under a dedicated service account. Use a trusted TLS reverse proxy and keep Kestrel bound to loopback. Only loopback proxies are trusted by default. Public HTTP allows catalogue/frontend views, but login, writes, provider XML and DLL downloads return 403 until accessed through HTTPS.

The supplied `WebServiceCore/Deploy/nginx-test-http.conf` is a **read-only HTTP test configuration**, not a complete TLS configuration. The isolated deployed test was hardened in place; production was not replaced. On the existing public IP/HTTP test URL, the login is intentionally unavailable until a proper HTTPS endpoint is configured.

For a brand-new empty database, use the explicit `--initialize-empty` command described in the hardening notes. It creates no demo accounts. Existing passwords are converted to salted hashes at startup. Keep a protected pre-upgrade backup; an old plaintext-password service must not be pointed at the converted database. Historical backups may still contain plaintext passwords and need protected retention.

Administrators can provision invited users and manage roles in the browser. Local operator alternatives use `--create-user` and `--set-password`, with a strong password supplied through `OV_NEW_PASSWORD`. DLL publication uses `--approve-package REVIEW_ID` after inspecting the exact staged package. Detailed commands and safeguards are in the hardening notes. There are no production secrets or default administrator credentials in this archive.

## What was tested

- 106 automated HTTP/security/account/SMTP regression checks passed on Windows, including the fake-client attack scenarios, password migration, staging/approval and session protections.
- 15 native PNG/image and SQLite compatibility checks passed on Windows.
- 14 smoke assertions passed against the hardened Debian 12 test deployment, including nginx header spoofing, permission checks and hashed-password verification. Synthetic Linux fixtures were removed afterwards.
- Release build and self-contained Linux publish succeeded. Production executable checksum remained unchanged and the production service stayed active.

See `WebServiceCore.SecurityTests/SECURITY-RESULTS.json` and `LINUX-SMOKE-RESULTS.txt`. The older 33-check HTTP compatibility results describe the pre-hardening migration; some old expectations deliberately no longer apply. Debian 13, full MP2 video playback and client-side package signature verification remain unverified.

## Important limitations

Server authorization and local review reduce the risk from fake clients. They cannot make executable updates cryptographically trustworthy to an unchanged client. End-to-end package signature verification needs client changes and a signing/revocation process. Review existing legacy DLL inventory before production promotion. Approved publishers and local operators remain trusted roles.

Tune limits against real client behavior, maintain upstream network protection and use filesystem/service quotas. The in-memory authentication failure budget is not shared across multiple instances. A hard crash during a file/database publication may require reconciliation with the retained backup; ordinary failure rollback is implemented.

## Documentation and provenance

- [Current security design, operations and compatibility changes](WebServiceCore/SECURITY-HARDENING.md)
- [Migration notes and historical validation](WebServiceCore/NET10-MIGRATION.md)
- [Linux test runbook](WebServiceCore/LINUX-TEST-RUNBOOK.md)
- [GitHub pull request draft](WebServiceCore/GITHUB-PR-DRAFT.md)
- `docs/onlinevideos-net10-security.patch`: local migration and hardening commits, suitable for review against the stated upstream base. The current branch is proposed in https://github.com/MediaPortal/mp-onlinevideos2/pull/4.
- `MANIFEST-SHA256.json`: checksums for all other archive files.
- [Follow-up review and corrected findings](WebServiceCore/SECURITY-AUDIT-20261016.md)
- [MP2 HTTPS client test and playback blocker](WebServiceCore/MP2-CLIENT-TEST-20261016.md)

Databases, provider binaries, user data, private keys/configuration, Git history and compiled outputs are excluded. Only upstream SMTP placeholders remain in appsettings files; account verification and opt-in notification email are configured through private deployment settings. Passwords are never emailed.

For this source-only ZIP, the project item copying `Onlinevideos.db3` was removed because the database is not included. A root NuGet.Config makes both test projects restore only from nuget.org. These packaging-only changes are not part of the migration patch. Application behavior otherwise matches the tracked revision. Original source and third-party asset licenses continue to apply.

## Account and review extension

See [USER-MANAGEMENT.md](WebServiceCore/USER-MANAGEMENT.md) for browser user management, invitation links, verified notification addresses, role/subscription checks and queued email. Site submissions now require administrator review (HTTP 202); package publication requires explicit Administrator approval in Reviews or the local operator command.
