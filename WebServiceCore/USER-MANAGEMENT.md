# Accounts, reviews and email notifications

The server now separates login names from verified notification email addresses. Existing identifiers and password hashes are preserved. Existing administrator accounts retain their rights; publisher permissions are managed in the Users page instead of Security:Publishers configuration.

## Administrator workflow

Open **Users** to search accounts, invite new users, assign User/Publisher/Administrator roles, disable accounts or delete them after typing the exact login name. Invitations expire after 24 hours and can be used once. New users choose their own password (14–256 characters). There is no public registration.

Administrators cannot disable, demote or delete their own account. The last active administrator is protected inside a database transaction. Disabled/deleted accounts cannot authenticate; password, role and status changes invalidate existing browser sessions. Deleted users' existing sites and packages are preserved under a disabled anonymous owner. Pending site changes and invitation tokens for deleted accounts are removed.

**Account settings** supports password changes after verification of the current password, a separate email address, email confirmation and notification preferences. Saving a different address requires a new confirmation. Administrator notifications are opt-in: site changes, package submissions and hourly report summaries. Users can opt into decisions concerning their own contributions. Unverified or disabled recipients receive no event notifications. Repeated report events in the same hour are grouped and recipients are deduplicated.

## Reviews and compatibility

Site submissions now return HTTP 202 with a review identifier instead of publishing immediately. Owners may propose updates to their existing sites; Publishers and Administrators may propose new sites. **Reviews** lets Administrators inspect escaped XML and approve/reject site changes. Sender ownership and current permissions are checked again at approval. Packages remain private until an **Administrator** explicitly approves them in Reviews, or the local operator uses the existing approval command. All active Administrators may approve packages. The page displays the submitter and SHA-256 hash; approval rechecks current administrator and publisher permissions, ownership, package structure and the exact displayed hash. Browser approval requires an antiforgery token. Publication generates an opt-in notification to the submitter.

This intentionally changes the server publication workflow. Existing read DTOs and download checksums remain unchanged. Existing clients may need to handle HTTP 202 as a pending submission rather than immediate publication.

## Mail configuration

Configure these environment variables on the server, outside source control:

```
Mail__Enabled=true
Mail__Host=mail.example.invalid
Mail__Port=25
Mail__From=onlinevideos@example.invalid
Mail__PublicUrl=https://onlinevideos.example.invalid
```

The test deployment uses the existing loopback Exim relay. Optional Mail__StartTls, Mail__Username and Mail__Password support another relay. Never store relay credentials in the repository. PublicUrl must be an operator-configured HTTPS URL; request Host headers are never used to build invitation links.

The background mail queue runs once per minute, retries five times with increasing delay, checks current role/subscription/address before delivery, and clears message bodies after delivery/cancellation/final failure. Expired account links are removed. Mail uses a fixed sender and fixed subjects; arbitrary client-provided report text is not forwarded. Mail delivery is at least once: an interruption after SMTP acceptance can cause a duplicate retry. Mail is disabled by default and automated tests do not contact real recipients.

## Database update and rollback

Back up the SQLite database consistently before deploying. Startup applies an idempotent SQLite schema extension for account preferences, hashed one-use tokens, queued messages and site reviews. Existing data is retained. The default --initialize-empty command creates the complete current schema. For rollback restore both the previous application release and its paired database backup. Keep the database, queued invitation links, backups and Data Protection keys outside the public webroot and restrict filesystem access.

Activation URLs contain one-time secrets: suppress query-string logging for `/Activate` at every reverse proxy. Account pages send no-store and no-referrer headers, and application request-start logging is suppressed. Rate limits and Razor antiforgery protections remain enabled.

## Visual design and assets

The test frontend uses the LogoManager palette and layout with its own OnlineVideos header. The header was generated using the built-in image tool with only the right-hand LogoManager text treatment as reference: dark blue textured background, white OnlineVideos title, light subtitle, no MP2 logo. Prompt: create a compact sibling-site masthead with the exact text “OnlineVideos” and “Sites, Streams & Community”, omitting all MediaPortal/MP2 branding.

The temporary generic IPTV and Xtream replacement icons were removed at the operator’s request. Missing provider icons remain absent.

## Validation and deployment — 2026-10-16

67 security/account/mail checks and 15 image/SQLite checks pass. The SMTP test uses an isolated loopback listener, not a real recipient. HTTPS login, Users, Account and Reviews return 200 on the live test deployment; the old HTTP test listener rejects account access. The new header returns a valid SVG. The temporary replacement icons were subsequently removed. Real recipient mailbox delivery is not asserted.

The test server has a persistent systemd unit definition with loopback binding, filesystem restrictions, resource limits and loopback-only network access. It is started for testing and is not enabled at boot. Existing production services are unchanged. The paired pre-update backup is `/var/backups/onlinevideos-accounts-20261016-164946`. Historical review records are retained.

The original generated header was replaced with native SVG typography on a transparent background for clean edges and seamless integration with the shared CSS header color. The SVG contains only a white title and light subtitle, without embedded scripts, external resources or MP2 branding.

Administrators can change a user’s notification email in Users → Change email. This never renames the legacy login identifier or grants access. A changed address is unverified and old pending account links/messages are invalidated. Active users receive a new confirmation link; disabled accounts remain disabled and do not receive an activation link through email editing.

Catalogue sorting uses regular GET links compatible with the content security policy. Site, Creator, Language and Updated toggle ascending/descending order; pagination preserves the selection. Legacy SiteGrid URLs render a complete page for direct navigation and retain partial responses for explicit AJAX requests. Eleven live comparison checks passed against the original site, including all columns/directions and legacy URLs.

Pending packages also offer **Download for inspection** and **Reject** to active Administrators. Inspection downloads are private, bounded, SHA-256 checked attachments with no-store/nosniff headers. Rejection requires antiforgery protection and the displayed hash; it removes the private pending binary, retains private reviewer/time/hash metadata, and sends an opt-in submitter notification. Already published versions are unchanged. Validation now includes 106 regression checks.


### Combined review decisions
Reviews are grouped into Combined XML and DLL, XML-only and DLL-only. Related packages are shown only with the matching site change, using the exact required DLL name and submitting account. Each combined review offers an inspection download and one Approve / Reject action pair for both submissions. Package ID, displayed SHA-256, fresh administrator permissions and the relationship are rechecked. Both decisions share one database transaction; failed publication restores DLL/image files. XML may enter review before its DLL is published; separate XML approval still requires a published dependency. Existing clients need no protocol changes for this pairing.
