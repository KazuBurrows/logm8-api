# LogMate API

.NET 8 isolated-worker Azure Functions app backing the LogMate service/asset-logging product (NFC-tag scan → service history via one-time "OneLife" URLs, no user login system).

## Architecture

- **Compute**: Azure Functions v4, Consumption plan (`LogMate` / resource group `logmate2`, `logmate.azurewebsites.net`).
- **Data**: SQL Server (`dbwebchat.database.windows.net`, database `webchat` — shared with an unrelated project, see Known Issues), Cosmos DB (`cosmoslogmate`), Azure Blob Storage, Upstash Redis.
- **Layout**: `Functions/` (HTTP triggers, grouped by domain: `ServiceRecord`, `ServiceOption`, `NfcTag`, `OneLife`, `Database`, `General`), `Application/Services` (business logic), `Infrastructure/Data` (connection factories/repositories), `Common/Validation` (DataAnnotations-based request validation via `ModelValidator`), `Common/Crypto` (AES token cipher, pepper hashing), `Middleware/` (global exception handling, SQL-injection screening, APIM auto-block). The old `Functions/Legacy/LogMateFunction.*.cs` partial-class endpoints have been fully retired as of 2026-08-16 (see Known Issues history below).
- **Auth model**: there is no login/user system. Access is granted per-resource via opaque tokens (OneLife URLs, per-record tokens) validated against SQL at request time — not Azure Function keys. Several endpoints are intentionally `AuthorizationLevel.Anonymous` because the token *is* the auth check. A handful of endpoints (catalog/config management with no token concept) are currently also Anonymous with no protection — this is known, tracked as a deferred redesign (see Known Issues).

## Local development

Requires .NET 8 SDK and Azure Functions Core Tools v4.

1. Copy the required settings into a local, gitignored `local.settings.json` (never commit this file). Keys currently required: Azure Storage connection/account key, SQL connection string, Cosmos DB endpoint/key, Redis connection, Blob storage key, `AES_KEY`, `HASH_PEPPER`, SMTP host/port/username/password. Ask a teammate for current values — there is no secrets manager wired up yet (see Known Issues).
2. `dotnet restore`
3. `func start`

## Deployment

No CI/CD pipeline deploys this app yet. `.github/workflows/ci.yml` runs build + a NuGet vulnerability audit on push/PR to `main`, but does not deploy. Deployment today is manual (e.g. `func azure functionapp publish LogMate` or zip deploy via the portal) against the `LogMate` Function App in resource group `logmate2`.

## Key Azure resources

| Resource | Name | Resource Group |
|---|---|---|
| Function App | `LogMate` | `logmate2` |
| Application Insights | `logmate2` | `logmate2` |
| SQL Server (shared) | `dbwebchat` | `RG-WEBCHAT` |
| Cosmos DB | `cosmoslogmate` | `RG-WEBCHAT` |
| API Management | `logm8-apim` | `RG-WEBCHAT` |

## Known issues / deferred work

- **`Functions/Legacy/` retired (2026-08-16).** The 6 endpoints still called by real clients (`AcquireNewTag`, `SubmitTag`, `OneLifeUrlOneStep`, `OneLifeUrlGuestMode`, `OneLifeUrlOneStepNoDecrypt`, `GetLogDataForGarage`) were migrated into `Functions/NfcTag` and `Functions/OneLife` with identical `[Function]` names/routes/response shapes — no client changes needed in `logm8`, `logm8-mobile`, or `Logm8NFC`. Everything else (`UpdateTag`, `OneLifeUrl`, `OneLifeUrlNoEncrypt`, `ConsumeOneLifeUrl`, `GetMotorbikeOptions`, `UpdateServiceOptionsDb`, `GetServiceRecordHierarchy`, `AddServiceType`, `AddParentOption`, `CreateServiceOption`, `ReplaceNfcTag`, `Negotiate`) had zero references across all three client repos (grepped directly, not inferred) and was deleted outright, along with `Functions/Database/RsaFunctions.cs` (`GetPrivateKey`/`InsertRSAsAsync`, only called by the now-deleted RSA-based `Negotiate`/`OneLifeUrl` flow), `Data/ServiceOptions.sql` (schema for the deleted SQLite path), and the `Microsoft.Data.Sqlite` package reference. This was done without confirming via Application Insights invocation counts first, since ingestion is still broken (see below) — the safety net here was three-repo grep coverage, not live traffic data, so watch for 404s from these specific endpoint names for a while after deploy.
- **Application Insights has zero telemetry.** `requests`, `traces`, and every other table were empty across all time as of 2026-08-16, despite the instrumentation key matching what's wired into the Function App. Either this API has had no real traffic, or ingestion is silently broken. Needs investigating (check `APPLICATIONINSIGHTS_CONNECTION_STRING` app setting in production) before invocation counts can be trusted for anything, including the Legacy cleanup above.
- **APIM is not fronting this API.** `logm8-apim` only has an unrelated `echo-api` registered. The Function App is directly publicly reachable with no gateway in front of it. `Middleware/ApimAutoBlockMiddleware.cs` (bans attacker IPs via APIM policy in response to Azure Monitor alerts) currently has no effect on LogMate traffic as a result.
- **Auth model redesign pending.** Several catalog/admin endpoints (`AddServiceOption`, `AddParentServiceOption`, `AddServiceOptionServiceType`, `ReplaceAssetNfcTag`) have no token concept and are currently unprotected `Anonymous` endpoints. Intended fix: consolidate token validation into shared middleware and decide what gates the token-less admin endpoints — deferred by design, not yet scheduled.
- **Shared SQL database.** The `webchat` SQL database is shared with an unrelated project. Its credentials, backups, and redundancy (currently `Local`, non-geo-redundant, 7-day retention) are not solely LogMate's to change — coordinate before rotating credentials or altering backup policy.
- **Secrets are plaintext app settings**, not Key Vault-backed. No managed identity is used for secret retrieval yet.
- **No automated tests.**
