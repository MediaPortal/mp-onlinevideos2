# WebServiceCore .NET 10 migration

## Status and provenance

Prepared 2026-10-15 from upstream commit `dfcf36e5661ee45019bc8a48604cee48a87c9c09` on local branch `codex/webservice-net10`. The deployed .NET 6 binary dates from January 2024; its exact source commit is not embedded in the available deployment, so this port deliberately starts from current upstream rather than claiming to reproduce the deployed source exactly.

This is a reviewable migration candidate, not a production replacement. No repository push or GitHub pull request has been made. No production database migration was executed. The existing service, nginx configuration, application files and runtime configuration were preserved.

## Changes

- Target .NET 10; update EF Core SQLite/Design to 10.0.13 and Swashbuckle to 10.3.0.
- Remove unused InMemory, EF Tools and MVC scaffolding dependencies from the web project. They are not used in application code. Keep EF Design available for explicit developer tooling.
- Replace ImageSharp with SkiaSharp 4.153.1 and matching Linux native assets. ImageSharp 3.1.12/3.2.0 produced NuGet vulnerability warnings; 3.2.0 additionally required a license key. No license checks were disabled. SkiaSharp passes the existing PNG format, square-icon and 3:1-banner checks while preserving uploaded bytes and response strings.
- Allow the SQLite connection string to be supplied through `ConnectionStrings:OnlineVideos`, preserving the existing relative database path as the default. The test uses `ConnectionStrings__OnlineVideos` to point to an isolated copy.
- Add dependency lockfile, a public-NuGet-only source configuration scoped to this web project, a native image/SQLite test executable, HTTP compatibility tests and a proposed GitHub Actions workflow.

API routes, DTOs, authentication implementation and entity mappings are unchanged. No automatic `Migrate` or `EnsureCreated` call was added to the web application.

## Validation performed

- SDK 10.0.402; Release build and self-contained linux-x64 publish succeeded.
- 15 PNG-validation and SQLite CRUD/projection checks passed on Windows x64 and Debian 12 x64.
- 33 HTTP checks passed against the isolated Linux instance, including overview responses, existing/missing XML, icons, banners and reports, unknown/wrong credentials, DLL upload/download, site upload/update, original PNG bytes and conditional HTTP 204 responses.
- Existing-service comparisons are GET-only. All writes use a consistent backup of the live SQLite database in a separate directory. Synthetic fixtures are removed after the test, including on assertion failure. No registration, password-email or report-submission call was made, and outbound networking from the test service was limited to localhost.
- JSON collection order is normalized for comparison. Framework-generated problem-response trace IDs and RFC links are ignored; response status and remaining error data are compared.
- The live WebServiceCore binary SHA256 remained unchanged. The production systemd service stayed active. Test service was stopped after validation.
- NuGet audit including transitive dependencies reported no known vulnerable packages at validation time. This is a point-in-time package audit, not a complete security audit.

See `../WebServiceCore.CompatibilityTests/HTTP-RESULTS.json` for the individual HTTP assertions.

## Reproduce

Use a .NET 10 SDK:

```sh
dotnet restore WebServiceCore/WebServiceCore.csproj --locked-mode
dotnet run --project WebServiceCore.CompatibilityTests -c Release
dotnet publish WebServiceCore/WebServiceCore.csproj -c Release -r linux-x64 --self-contained true -o artifacts/webservice
```

Run the publish output under a separate unprivileged user with a separate SQLite backup and a loopback-only port. Set `ASPNETCORE_ENVIRONMENT=Production` to avoid development seeding. Copy the existing public Icons/Banners/Dlls files into the test web root. Never publish over the live directory or copy the sample repository database over production.

The HTTP test script is intentionally host-specific: old service port 5000, test port 5086, copied database `/srv/onlinevideos-net10-test/test.db3`. Review these constants before use. It creates a clearly synthetic `.invalid` account and synthetic files only in the test copy. Do not repoint its mutation requests to production.

## Before production adoption

1. Run these tests on actual Debian 13 after the OS upgrade. Current Linux results are Debian 12 with a private .NET 10 runtime; they are not a Debian 13 execution result.
2. MP2 GUI catalogue and installed-client API checks have now passed; see `MP2-CLIENT-TEST.md`. Real playback still requires a current compatible plugin: the installed 2.3.1 updater rejects the advertised 2.9.2 version. Test MP1, actual streaming, administrator login, remaining management pages and email delivery before production adoption.
3. Review existing security debt separately: legacy password storage/email behavior, upload authorization and request-size policy. This migration preserves those behaviors rather than silently changing the client protocol.
4. Review the SkiaSharp native dependency/license inventory and operational image-processing limits. Test native decoding with representative production uploads.
5. Back up the SQLite database and files, publish to a new versioned directory, switch the service path only after approval, and retain the prior directory for rollback. Do not replace the live SQLite file. Existing-schema access passed without migrations; any future schema changes require separate review.

No production switch is authorized by this document.
