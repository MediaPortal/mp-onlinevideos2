# Modernize the OnlineVideos server for .NET 10 and add secure administration

The server moves from .NET 6 to .NET 10 and adds reviewed publishing, account administration and abuse controls while preserving the existing catalogue response formats and change checksums. This PR changes the server and web frontend; it does not replace desktop plugins.

## Final changes

- Update the runtime, EF Core SQLite and Swagger dependencies; replace ImageSharp with SkiaSharp while retaining PNG validation, aspect-ratio behavior and original image bytes.
- Migrate legacy passwords to salted hashes, require HTTPS for sensitive operations, protect browser forms with antiforgery checks and invalidate sessions after password, role or account-status changes.
- Add invitation-based accounts, verified contact email, password changes, searchable administrator user management and User/Publisher/Administrator roles. Protect the last active administrator and retain contributions when an account is deleted.
- Add administrator review of site definitions and privately staged managed packages. Any active administrator can inspect the exact package download, approve it or reject it. Checksum-bound actions, fresh permission checks and rollback handling protect publication; rejection leaves an existing published version intact.
- Group combined XML/DLL, XML-only and DLL-only reviews. A combined submission has one Approve and one Reject button for both items, plus a consistent inspection download. Verify the selected package ID/hash and submitting account, and commit both decisions together with publication rollback. XML can enter review before its required DLL is published.
- Add opt-in email preferences for pending changes and review outcomes, with verified recipients, queued delivery, retries, deduplication and expired-token checks.
- Restrict client mutations and moderation using account permissions rather than claimed client identity. Add request, authentication, upload and concurrency limits; validate XML, filenames, images and package contents. Anonymous reports remain unverified and cannot change provider status.
- Refresh the English frontend with independent OnlineVideos branding, fix every catalogue sorting direction and legacy sorting link, and prevent long site names from overlapping report counts.
- Add automated compatibility/security suites, a Windows/Linux CI definition and English deployment, rollback and security documentation. Deployment-specific identities, credentials and addresses are excluded.

## Validation

The cleaned branch passed 113 security/account/SMTP checks and 15 image/SQLite checks on Windows; see VALIDATION.md. Release builds and a self-contained Linux publish succeeded; earlier isolated Debian 12 smoke checks and sorting checks are recorded alongside the regression results. A fresh GitHub Actions run is pending for this follow-up. The older development-branch run exposed an obsolete button-label assertion, now replaced by an approval-form check.

MP1 API checks passed and an installed MP1 client played a live stream using its bundled providers. That playback is separate from the HTTPS API checks and does not prove a complete provider installation from this server. MP2 API reads and GUI catalogue browsing passed; full MP2 playback remains unverified because the installed integration cannot complete its normal provider update. Version or certificate safeguards were not bypassed.

## Deployment considerations

Deploy with the .NET 10 runtime or a self-contained publish. Configure HTTPS, restrict access to private application data and run the service with only the permissions it needs. Back up the database before migration; rollback requires a compatible application version and its matching database backup because password conversion is one-way.

Configure email delivery and a fixed public HTTPS URL before enabling notifications. Publishing clients must support authenticated submissions and pending-review responses. Validate the intended operating system, reverse-proxy configuration and desktop client versions before production use.
