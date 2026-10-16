# Isolated Linux deployment and validation

Use a separate test directory, private data directory, unprivileged service account and consistent SQLite backup. Supply their values in private deployment configuration; no deployed host, account name or filesystem location is specified here. Do not overwrite the active service or point tests at its database.

1. Build the self-contained Linux executable using the commands in README-SERVER.md. Copy public provider assets separately from a trusted existing deployment.
2. Back up SQLite using its online backup API. Preserve a paired copy of the previous application and database before password conversion. Restrict access to all backups, which can contain legacy plaintext credentials.
3. Configure ASPNETCORE_ENVIRONMENT=Production, ASPNETCORE_URLS, ConnectionStrings__OnlineVideos and Security__PrivateDataPath privately. Keep the backend reachable only through the trusted local reverse proxy. Private storage must be outside wwwroot and accessible only to the service account.
4. Use a dedicated service with NoNewPrivileges, PrivateTmp and read-only system protection; permit writes only to the application data paths. Configure request/body limits at the TLS proxy as well as in the application. Never trust forwarded headers from arbitrary remote hosts.
5. Run both test executables. Provision the first administrator with the documented operator command, reading the password securely into OV_NEW_PASSWORD and clearing that environment variable afterwards. No default administrator is created.
6. Configure verified invitation and review mail only after setting a fixed HTTPS public URL and an approved relay. Mail is disabled by default. See USER-MANAGEMENT.md and Deploy/mail-settings.example.json.
7. Verify HTTPS login, invitation activation, account changes, catalogue sorting and administrator review/download/rejection. Confirm anonymous users cannot retrieve pending packages or moderate status. Keep test submissions private until intentionally approved.

Deploy/nginx-test-http.conf is a read-only HTTP example, not a TLS configuration. Its onlinevideos_backend upstream must be supplied privately and target a trusted local backend. Public login and sensitive downloads require a separate HTTPS configuration.

Rollback restores both the previous application and its matching pre-migration database; the previous plaintext-password application cannot use the converted database. The CI workflow runs synthetic tests and a publish build; it does not connect to a deployment. See VALIDATION.md for completed checks and remaining client validation.
