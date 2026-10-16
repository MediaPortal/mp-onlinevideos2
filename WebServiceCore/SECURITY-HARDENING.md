> **Follow-up review:** [16 October audit](SECURITY-AUDIT-20261016.md) documents the current fixes and 82 regression checks. [MP2 client validation](MP2-CLIENT-TEST-20261016.md) documents successful HTTPS reads and the outstanding plugin version/playback blocker.

> **Account/review extension:** See [USER-MANAGEMENT.md](USER-MANAGEMENT.md) for the newer invitation-based account management, notification preferences, and HTTP 202 site review workflow. Its validation results supersede earlier counts below.

# Server security hardening — 15 October 2026

This revision addresses the application-side findings in the preceding review. It deliberately tightens legacy write workflows. It does not claim that an unchanged desktop client can verify package signatures, or that any software is immune to all attacks.

## Changes

### Authentication and transport

- Legacy plaintext passwords are converted transactionally at startup to ASP.NET Core Identity password hashes using PBKDF2, a random salt and 210,000 iterations. The existing SQLite password column is reused; no schema change or plaintext authentication fallback is required. Existing clients can continue sending their existing credentials over HTTPS.
- The GET `RegisterEmail` route now returns HTTP 410. It neither creates accounts nor sends passwords. Public self-registration and automatic password recovery are intentionally disabled until an independently reviewed token-based workflow is available. Opt-in report summaries are deduplicated hourly; anonymous reports do not directly send arbitrary mail.
- Login and all mutations require HTTPS. Executable DLL and provider XML downloads also require HTTPS. Public catalogue metadata and frontend views can still be read over HTTP. There is no insecure-transport configuration switch.
- Cookies are Secure, HttpOnly and SameSite=Lax, expire after two hours and do not slide indefinitely. Password replacement or a changed administrator flag invalidates existing sessions. Cookie-protection keys persist in the private data directory; filesystem permissions must protect them.
- Unknown users and wrong passwords receive the same authentication response. A shared per-account failure budget applies to browser and API authentication: five failed attempts result in a temporary lockout. A successful attempt clears the failure count. This in-memory budget resets at restart; multi-instance deployments need shared abuse-control state.
- Forwarded headers are accepted only from the framework's default loopback proxy addresses, with one forwarding hop. Add only specific trusted proxies when deploying behind a different reverse proxy. Never trust client-supplied forwarding headers indiscriminately.

### Reports and ownership

- Anonymous clients can submit bounded Suggestion/Broken reports for compatibility. These are unverified observations and do not change authoritative provider state.
- ConfirmedBroken, RejectedBroken and Fixed require the provider owner or an administrator. The API accepts optional Email/Password fields for those operations; a browser cookie alone does not authorize API moderation.
- The Razor frontend retains antiforgery protection and checks owner/admin authorization. Report timestamps are assigned by the server, and enum/message validation is applied.
- Limits: 2,000 characters per report, 50 API reports per provider per day, 1,000 reports per provider and 100,000 globally. The browser moderation path enforces the total/provider storage caps. Views fetch bounded reports instead of loading the entire history.

### Provider and executable publishing

- New provider creation and DLL staging require an administrator or an account explicitly listed in `Security:Publishers`. Existing owners may still update their own sites.
- `SubmitDll` never publishes an upload immediately. It validates bounded size and managed assembly metadata without loading or executing the assembly. The assembly name must match the package name. The route stages the bytes and metadata privately and returns HTTP 202 with a review identifier.
- Publication requires a separate local operator command after reviewing the package. The operator must inspect the exact SHA-256 identified bytes; format validation is not malware detection. Approval rechecks the hash, publisher authorization and existing ownership before publishing.
- A filesystem lock serializes staging/approval across processes. File replacement uses a temporary file, a backup and rollback if the database update fails. A hard power loss between the filesystem and database operations still needs operator reconciliation; they are not one distributed transaction. Stop the test service for planned approvals and retain a backup.
- Direct static `/Dlls/...` access is denied. Packages are served through the bounded API. Legacy MD5 fields remain protocol change checksums only; they are not security signatures. SHA-256 protects the private review workflow.
- Existing deployed DLLs remain trusted legacy inventory. Review that inventory before production promotion. This change does not retroactively establish its provenance.

### Input, storage and resource controls

- Request bodies are capped at 16 MiB, DLLs at 8 MiB, images at 2 MiB and 4,096 pixels per dimension, XML at 262,144 characters and names at 120 characters. Unsafe filenames, invalid enum values, empty/null payloads and excessive fields return controlled errors.
- XML is parsed with DTDs prohibited, external resolution disabled and a document length limit. Authentication precedes XML parsing.
- Image writes use temporary files and a bounded directory quota. Images retain their original PNG bytes after format/ratio checks.
- Private staging is limited to 128 MiB; published DLL storage to 512 MiB; each icon/banner directory to 128 MiB. Mutations are refused when the content-root filesystem has less than 512 MiB available. For data on separate mounts, enforce filesystem quotas there as well.
- Per client IP and minute: 120 reads, 10 writes, five browser-login POSTs. Global concurrent requests are capped at 16; Kestrel connections at 100. Rate rejection returns HTTP 429 with Retry-After. These conservative defaults suit the small existing catalogue; tune only after measuring legitimate clients.
- Provider API listings are bounded at 10,000 entries; the frontend pages 200 providers at a time and counts reports in SQL. No report message bodies are loaded merely to display a count.
- Security headers include nosniff, frame protection, same-origin referrers and a restrictive Content Security Policy. Unexpected errors return generic responses; validation errors return 400.

## Configuration

Use environment variables or a private configuration file; never commit real secrets:

```sh
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://127.0.0.1:5086
export ConnectionStrings__OnlineVideos='Data Source=/srv/onlinevideos-test/test.db3'
export Security__PrivateDataPath=/var/lib/onlinevideos-test/private
# Optional: this account is trusted to stage software, not to bypass review.
export Security__Publishers__0='publisher@example.com'
```

Keep private data outside wwwroot, owned by the service user, with a restrictive umask (0077). Protect SQLite files, session keys, staged packages and backups. Older database backups may still contain plaintext credentials. Hash migration does not sanitize historical backups or guarantee removal from old filesystem/SQLite journal pages. Plan protected retention and disposal separately.

## Local operator commands

Run from the published application directory, under the service account with the same database/private-path configuration. Use `./WebServiceCore` for the self-contained Linux app, or `dotnet WebServiceCore.dll` for a framework-dependent deployment.

```sh
# Only for a new empty database, not a replacement for the live database:
./WebServiceCore --initialize-empty

# Provision an account; password is read from the environment, never from argv.
read -r -s -p 'New password (at least 14 characters): ' OV_NEW_PASSWORD; echo
export OV_NEW_PASSWORD
./WebServiceCore --create-user publisher@example.com
unset OV_NEW_PASSWORD

# For explicit local administrator creation, set OV_CREATE_ADMIN=true as well.
# Unset it immediately afterwards. Public registration cannot set this flag.

# Replace an existing password, invalidating previous sessions:
read -r -s -p 'New password: ' OV_NEW_PASSWORD; echo
export OV_NEW_PASSWORD
./WebServiceCore --set-password publisher@example.com
unset OV_NEW_PASSWORD

# After inspecting private REVIEW_ID.json and verifying/reviewing REVIEW_ID.dll:
# Stop the test service first; run approval under its service account/configuration.
./WebServiceCore --approve-package REVIEW_ID
```

The application does not contain a default administrator or create demo accounts automatically. Remove or rotate previously shared test credentials before enabling HTTPS access or promoting test data.

## Reverse proxy and Linux service

Terminate TLS at a trusted nginx proxy. Set `Host $http_host`, `X-Forwarded-Proto $scheme` and a correctly managed client IP header. Bind Kestrel to loopback and keep it inaccessible from external networks. Do not set forwarded HTTPS unconditionally on a public HTTP listener.

Recommended nginx settings for the dedicated vhost:

```nginx
client_max_body_size 16m;
client_body_timeout 15s;
proxy_connect_timeout 10s;
proxy_read_timeout 60s;
```

Configure nginx rate/connection limits as an additional layer and enforce HTTPS at the public edge. A distributed attack can bypass per-IP application limits; upstream network protection remains necessary.

Use a dedicated service account and systemd limits such as `MemoryMax=512M`, `CPUQuota=100%`, `TasksMax=128`, `UMask=0077`, `ProtectSystem=strict`, `ProtectHome=true`, `PrivateTmp=true` and `NoNewPrivileges=true`. Grant write access only to the application data paths. The separate test deployment retains outbound network restrictions and uses the loopback SMTP relay for configured opt-in notifications.

## Compatibility and remaining client work

Catalogue response shapes, existing asset names and legacy change checksums are retained. Intentional changes: disabled password email, authenticated moderation, publisher restrictions, HTTP 202 for staged DLLs, bounded requests and HTTPS-only sensitive operations.

Older clients can continue anonymous unverified error reports and HTTPS reads. Moderation requires credentials in the extended report DTO or use of the secured web interface. Publishing tools must handle pending review. A client's normal API must not treat a 202 response as a published release.

Cryptographic verification of downloaded package signatures still requires client changes and a release-signing design. No fake User-Agent, version number or embedded shared client secret grants trust. This revision restricts server publishing but does not make a compromised approved publisher or a mistaken operator approval harmless.

## Validation

The accompanying security regression executable exercises password migration, owner/admin authorization, fake DLL rejection, private staging, explicit approval, modified-byte rejection, DTD rejection, HTML escaping, HTTPS restrictions, cookie flags, session invalidation, rate limiting and per-account failure budgets. Results are in `../WebServiceCore.SecurityTests/SECURITY-RESULTS.json`.

The older 33-check HTTP comparison predates these intentional write-policy changes and is retained only as historical evidence. It is not expected to pass unchanged against this revision. Read compatibility and the new security expectations are checked separately. Actual MP2 video playback, Debian 13 execution and client signature verification remain outside the completed validation.
