# OnlineVideos server for .NET 10

This server-only change includes the English web frontend, secure account management, reviewed site/package publishing and opt-in email notifications. See [the final PR description](WebServiceCore/GITHUB-PR-DRAFT.md), [security policies](WebServiceCore/SECURITY-HARDENING.md), [user management](WebServiceCore/USER-MANAGEMENT.md) and [validation](WebServiceCore/VALIDATION.md).

## Build and test

Install the .NET 10 SDK, then run from the repository root:

```sh
dotnet restore WebServiceCore/WebServiceCore.csproj --locked-mode
dotnet build WebServiceCore/WebServiceCore.csproj -c Release --no-restore
dotnet run --project WebServiceCore.CompatibilityTests -c Release
dotnet run --project WebServiceCore.SecurityTests -c Release
dotnet publish WebServiceCore/WebServiceCore.csproj -c Release -r linux-x64 --self-contained true -o artifacts/webservice
```

The suites create temporary synthetic accounts and databases. Their role labels and generated fixture passwords are test data, not deployed accounts or credentials. No live database or mail recipient is required.

## Deployment

Follow [the Linux runbook](WebServiceCore/LINUX-TEST-RUNBOOK.md). Supply database, backend URL, private storage and optional mail settings through private configuration. No live configuration, database, credentials, user list or provider package is included in this change.

Sensitive operations require HTTPS. Any active administrator can review site changes and inspect, approve or reject staged packages in Reviews. Operator commands remain available for initial account provisioning and package approval. Password conversion requires a paired pre-migration database backup for rollback.
