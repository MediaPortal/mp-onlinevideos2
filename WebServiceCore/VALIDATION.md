# Validation and limitations

## Automated validation

The final implementation passed 106 security/account/SMTP regression checks and 15 native image/SQLite checks on Windows. Regression coverage includes fake-client requests, authorization, password migration, session revocation, CSRF, XML and image limits, private package inspection, checksum-bound approval/rejection, publication preservation and notification eligibility/retries.

Earlier isolated Debian 12 smoke validation passed 14 assertions. Catalogue sorting validation passed 11 assertions. These historical results are retained for traceability and are distinct from the final cleaned branch rerun. A Release build and self-contained linux-x64 publish succeeded. A Windows/Linux GitHub Actions workflow is provided; its execution on GitHub is not claimed.

## Desktop client validation

MP1 1.39 with OnlineVideos 2.9.2 successfully read the HTTPS catalogue, provider XML, images, reports and package bytes. Its updater version check was compatible. A separate GUI test browsed a provider and played a live stream with moving video; that playback used bundled provider definitions/packages and does not establish a complete fresh provider installation from the new server.

MP2 2.5 with installed OnlineVideos 2.4.0.1 successfully deserialized the HTTPS API responses and displayed the remote catalogue in its GUI. Full MP2 playback is not verified: the installed integration rejects the advertised provider version during its normal update path. A newer core build alone does not resolve the older MP2 integration dependencies. No version guard or certificate validation was bypassed. A compatible MP2 integration build and a complete provider-installation/playback test remain required.

## Operational limits

Historical deployment checks do not establish Debian 13 compatibility. NuGet auditing reported no known package vulnerabilities at the recorded check; this does not cover every frontend or native dependency or guarantee the absence of vulnerabilities. Mail uses at-least-once delivery, so interruption after SMTP acceptance can cause a duplicate. Use one delivery worker per database. Administrator review is not an end-to-end client package-signature system.

## Clean branch rerun

The sanitized review branch passed all 106 security/account/SMTP checks and all 15 image/SQLite checks again. A self-contained linux-x64 Release publish also succeeded. The security suite required normal access to its temporary package fixtures. A new online vulnerability query was unavailable during restore; the earlier NuGet audit remains historical evidence. The build still reports pre-existing compiler/analyzer warnings; a successful build is not claimed to be warning-free.


## Combined review follow-up
Six additional security checks cover combined approval hash binding, dependency mismatch, successful joint publication, package consumption, joint rejection and preservation of published data (112 checks total). The standalone-package UI test now checks the approval form action rather than obsolete button wording. The updated cleaned branch passed all 112 security/account/SMTP checks on Windows. GitHub Actions validation of this follow-up is pending.
