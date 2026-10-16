# Accounts, reviews and email notifications

The server now separates login names from verified notification email addresses. Existing identifiers and password hashes are preserved. Existing administrator accounts retain their rights; publisher permissions are managed in the Users page instead of Security:Publishers configuration.

## Administrator workflow

Open **Users** to search accounts, invite new users, assign User/Publisher/Administrator roles, disable accounts or delete them after typing the exact login name. Invitations expire after 24 hours and can be used once. New users choose their own password (14–256 characters). There is no public registration.

Administrators cannot disable, demote or delete their own account. The last active administrator is protected inside a database transaction. Disabled/deleted accounts cannot authenticate; password, role and status changes invalidate existing browser sessions. Deleted users' existing sites and packages are preserved under a disabled anonymous owner. Pending site changes and invitation tokens for deleted accounts are removed.

**Account settings** supports password changes after verification of the current password, a separate email address, email confirmation and notification preferences. Saving a different address requires a new confirmation. Administrator notifications are opt-in: site changes, package submissions and hourly report summaries. Users can opt into decisions concerning their own contributions. Unverified or disabled recipients receive no event notifications. Repeated report events in the same hour are grouped and recipients are deduplicated.

## Reviews and compatibility

Site submissions now return HTTP 202 with a review identifier instead of publishing immediately. Owners may propose updates to their existing sites; Publishers and Administrators may propose new sites. **Reviews** lets Administrators inspect escaped XML and approve/reject site changes. Sender ownership and current permissions are checked again at approval. Packages remain private until the existing **local operator** approval command is used; web administrators cannot publish executable code. Publication generates an opt-in notification to the submitter.

This intentionally changes the server publication workflow. Existing read DTOs and download checksums remain unchanged. Existing clients may need to handle HTTP 202 as a pending submission rather than immediate publication.

## Mail configuration

Configure these environment variables on the server, outside source control:

```
Mail__Enabled=true
Mail__Host=127.0.0.1
Mail__Port=25
Mail__From=onlinevideos@team-mediaportal.com
Mail__PublicUrl=https://ovtest.team-mediaportal.com
```

The test deployment uses the existing loopback Exim relay. Optional Mail__StartTls, Mail__Username and Mail__Password support another relay. Never store relay credentials in the repository. PublicUrl must be an operator-configured HTTPS URL; request Host headers are never used to build invitation links.

The background mail queue runs once per minute, retries five times with increasing delay, checks current role/subscription/address before delivery, and clears message bodies after delivery/cancellation/final failure. Expired account links are removed. Mail uses a fixed sender and fixed subjects; arbitrary client-provided report text is not forwarded. Mail delivery is at least once: an interruption after SMTP acceptance can cause a duplicate retry. Mail is disabled by default and automated tests do not contact real recipients.

## Database update and rollback

Back up the SQLite database consistently before deploying. Startup applies an idempotent SQLite schema extension for account preferences, hashed one-use tokens, queued messages and site reviews. Existing data is retained. The default --initialize-empty command creates the complete current schema. For rollback restore both the previous application release and its paired database backup. Keep the database, queued invitation links, backups and Data Protection keys outside the public webroot and restrict filesystem access.

Activation URLs contain one-time secrets: suppress query-string logging for `/Activate` at every reverse proxy. Account pages send no-store and no-referrer headers, and application request-start logging is suppressed. Rate limits and Razor antiforgery protections remain enabled.

## Visual design and assets

The test frontend uses the LogoManager palette and layout with its own OnlineVideos header. The header was generated using the built-in image tool with only the right-hand LogoManager text treatment as reference: dark blue textured background, white OnlineVideos title, light subtitle, no MP2 logo. Prompt: create a compact sibling-site masthead with the exact text “OnlineVideos” and “Sites, Streams & Community”, omitting all MediaPortal/MP2 branding.

Generic IPTV and Xtream replacement icons are unmodified OpenMoji 15.1.0 television/play symbols, licensed CC BY-SA 4.0. Attribution and the license accompany the PNG files; attribution is also visible in the footer. These are not official provider logos.

## Validation and deployment — 2026-10-16

62 security/account/mail checks and 15 image/SQLite checks pass. The SMTP test uses an isolated loopback listener, not a real recipient. HTTPS login, Users, Account and Reviews return 200 on the live test deployment; the old HTTP test listener rejects account access. Both replacement icons and the new header return valid PNGs. Real recipient mailbox delivery is not asserted.

The test server has a persistent systemd unit definition with loopback binding, filesystem restrictions, resource limits and loopback-only network access. It is started for testing and is not enabled at boot. Existing production services are unchanged. The paired pre-update backup is `/var/backups/onlinevideos-accounts-20261016-164946`. Historical review records are retained.
