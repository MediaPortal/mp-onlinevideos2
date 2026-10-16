# Follow-up security and performance review — 16 October 2026

The invitation, account, review, notification and client-facing paths were reviewed after the .NET 10 port. Confirmed issues below were corrected and deployed to the isolated HTTPS test service. The original production service was not replaced.

## Corrected findings

| Area | Finding and correction |
| --- | --- |
| Sessions and permissions | A cookie could become valid again after an account was suspended and subsequently re-enabled. An explicit security version now changes with role/status changes and invitation activation. Mutating account operations reload tracked users inside their transaction, so an already tracked administrator cannot retain revoked privileges. Package and site submissions also recheck current permissions. |
| Resource exhaustion | Large submissions shared the ordinary concurrency budget. DLL/site submissions now share a separate two-request limit. Other write bodies are capped at 32 KiB, including chunked requests; oversize requests preserve HTTP 413. Existing rate, account, queue and storage limits remain in force. |
| Publication consistency | If an icon was written but a later banner or database step failed, the previous icon could remain overwritten. Ordinary publication failures now restore previous files and roll back catalogue/review changes. Success detection uses the explicit save result prefix. |
| Email queue | A stale queued verification message could pass the presence-of-any-token check. Delivery now requires its exact unexpired token. Shared administrator mailboxes are checked against all eligible subscriptions, so one subscriber opting out cannot suppress another. Interrupted final attempts become failed instead of remaining stuck. Async SMTP delivery has an explicit 15-second cancellation deadline. |
| Reports frontend | Removed the unused AJAX library that requires dynamic code incompatible with the CSP. Report forms work through normal POST/redirect; explicit AJAX remains supported. Missing report targets return 404. Write authorization uses current database permissions. |
| Queries | Notification selection and delivery eligibility are filtered in SQL. The admin list projects only required fields, excluding password hashes. Queue/review indexes were added. Password migration scans only legacy password rows. |

## Validation

- 82 security/account/SMTP regression checks passed against synthetic fixtures.
- 15 native image/SQLite compatibility checks passed on Windows outside the restricted execution environment. A restricted run could not save its temporary image fixture; that failed run is not counted as a pass.
- Release build and self-contained Linux publish succeeded.
- The NuGet vulnerability query, including transitive packages, returned no reported vulnerable packages at the time of the query; see `../WebServiceCore.SecurityTests/NUGET-AUDIT.json`. This does not cover all bundled frontend assets or prove absence of vulnerabilities.
- Live HTTPS login, Users, Account, Reviews, homepage and SVG header returned the expected responses. Removed replacement icons remain 404; plaintext access to Users remains 403.
- Eleven sorting/legacy URL assertions passed after deployment.
- Installed MP2 2.4.0.1 assemblies successfully read the HTTPS API, provider XML, icons, banners, reports and DLL bytes. See `MP2-CLIENT-TEST-20261016.md` for the separate playback blocker.

## Deployment and operating limits

A consistent SQLite backup and the previous binary/configuration were retained before deployment. A temporary second application instance was prepared and checked before switching the HTTPS proxy. The additive schema update adds `Users.SecurityVersion` and two indexes. Existing browser sessions require a fresh login because the session stamp format changed.

Per-process rate and concurrency budgets do not replace upstream protection against distributed attacks. Keep Kestrel on loopback, configure trusted proxy headers and preserve service/storage limits. SMTP remains at-least-once: a crash after relay acceptance but before committing success can duplicate a message. A hard crash during mixed filesystem/database publication may still need reconciliation with backups. Run one queue-delivery worker per database.

Downloaded executable packages remain dependent on publisher/operator trust. Client-side cryptographic package signature verification and revocation are still required for end-to-end executable authenticity. No User-Agent or claimed client version grants publishing authority. Historical review records were not automatically purged by this change. No complete infrastructure penetration test or guarantee of security is claimed.
