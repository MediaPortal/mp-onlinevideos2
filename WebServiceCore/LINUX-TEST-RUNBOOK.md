> **Updated security revision:** This document records the initial migration. The later [SECURITY-HARDENING.md](SECURITY-HARDENING.md) supersedes statements about unchanged authentication/write behavior. Legacy password email, anonymous moderation and automatic DLL publication are no longer enabled.

# Isolated Linux validation runbook

The 2026-10-15 trial used `/srv/onlinevideos-net10-test`, not `/var/www/publish`, and a dedicated `ov-net10-test` user. The existing `kestrel-ov2.service` was never stopped or edited. The new executable includes its own .NET 10 runtime; no system runtime was installed or replaced on the server.

Preparation:

1. Extract the self-contained publish output to a new test directory. Copy only public assets from the existing deployment. Make the apphost executable.
2. Use Python `sqlite3.Connection.backup()` with a read-only source connection to produce a consistent copy of the existing database. Do not copy a live WAL-mode database file with a plain file copy.
3. Give the dedicated user ownership of the test directory. Keep it inaccessible to other users. Do not copy production SMTP configuration; the test does not call mail-producing actions.
4. Run the compatibility executable as the test user, then start the application transiently:

```sh
systemd-run --unit=onlinevideos-net10-test --uid=ov-net10-test \
  --working-directory=/srv/onlinevideos-net10-test \
  --property=ProtectSystem=strict \
  --property=ReadWritePaths=/srv/onlinevideos-net10-test \
  --property=ProtectHome=true --property=PrivateTmp=true \
  --property=NoNewPrivileges=true \
  --property=IPAddressDeny=any --property=IPAddressAllow=localhost \
  --setenv=ASPNETCORE_ENVIRONMENT=Production \
  --setenv=ASPNETCORE_URLS=http://127.0.0.1:5086 \
  --setenv='ConnectionStrings__OnlineVideos=Data Source=/srv/onlinevideos-net10-test/test.db3' \
  /srv/onlinevideos-net10-test/WebServiceCore
```

Before running `http_compatibility.py`, review its paths and ports and create a private record of the production DLL SHA256 at `/var/backups/onlinevideos-net10-test-20261015/production-dll.sha256`, containing only the hexadecimal digest. The script expects this record to confirm the live binary is unchanged.

After testing:

```sh
systemctl stop onlinevideos-net10-test.service
systemctl is-active kestrel-ov2.service
```

The transient unit is not enabled at boot. The stopped test files and copied SQLite database remain private on the server for later Debian 13 validation; they must never be committed to GitHub. The native test uses its own temporary in-memory database and cleans its temporary images. HTTP fixtures are deleted from the copied database after execution.

The provided GitHub Actions workflow runs only local image/SQLite tests and a publish build. It does not connect to the deployed server, contain server credentials or execute these host-specific HTTP tests.
