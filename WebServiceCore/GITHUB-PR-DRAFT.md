# Migrate WebServiceCore to .NET 10 and validate Linux API compatibility

WebServiceCore currently targets the unsupported .NET 6 runtime, which prevents a supported deployment on Debian 13. This change targets .NET 10, updates EF Core SQLite and Swagger dependencies, and replaces the vulnerable legacy ImageSharp dependency with SkiaSharp while retaining PNG validation rules and original file bytes.

The SQLite connection string can now be overridden for isolated deployments; the original default path remains valid. Routes, DTOs and entity mappings remain unchanged. No automatic database migration is introduced.

Validation: Release/self-contained Linux publish, 15 native image/SQLite checks on Windows and Debian 12, and 33 HTTP compatibility assertions against a separate copy of the deployed database. A NuGet dependency audit reported no known vulnerabilities. Test traffic made no production writes and the deployed binary remained unchanged.

Additional MP2 validation: the real GUI displayed the service catalogue and the installed 2.3.1 client API successfully deserialized sites, DLLs and reports and downloaded XML, PNGs and DLL bytes. Only the endpoint in a separate client copy was changed. Playback is not yet validated: the existing client's updater blocks module installation because it advertises support for an older version than the official update feed. Details are in `MP2-CLIENT-TEST.md`.

Debian 13 execution, MP1, actual playback with a current compatible plugin, admin workflows and controlled mail delivery remain to be validated before production replacement. The proposed workflow is included but has not run on GitHub. Full provenance, commands, scope and rollback guidance are in `NET10-MIGRATION.md`.
