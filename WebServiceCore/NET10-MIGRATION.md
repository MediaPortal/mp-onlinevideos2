# .NET 10 server migration

The server targets .NET 10 with updated EF Core SQLite and Swagger dependencies. SkiaSharp replaces ImageSharp; image validation retains the existing accepted PNG geometry and returns original bytes. The server solution and Windows/Linux CI workflow include image/SQLite and security regression executables.

Existing read response shapes and legacy MD5 change checksums remain compatible. Authentication, moderation and publishing are intentionally stricter: passwords become salted hashes, sensitive endpoints require HTTPS, site changes await administrator review and managed packages remain private until approval. Publishing tools must support authenticated requests and pending-review responses. Desktop plugin binaries are not changed by this PR.

See [the complete final change summary](GITHUB-PR-DRAFT.md), [security policy](SECURITY-HARDENING.md), [deployment runbook](LINUX-TEST-RUNBOOK.md) and [validation limitations](VALIDATION.md). The obsolete deployment-specific mutation harness has been removed; current tests use isolated synthetic fixtures.
