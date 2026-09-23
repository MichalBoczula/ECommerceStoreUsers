# Health checks

| Path | Meaning | MongoDB unavailable |
| --- | --- | --- |
| `/health/live` | The API process is running. No MongoDB dependency. | `200` |
| `/health/ready` | MongoDB responds on the configured database and exposes a writable replica-set primary with sessions, as required for Customer/Admin transactions. | `503` |
| `/health` | Backward-compatible alias of `/health/live`. | `200` |

Health responses use ASP.NET Core's built-in plain-text format, including the
`503` response. They do not return connection strings or driver error details.
Readiness has a five-second timeout; it returns to `200` when MongoDB recovers.
The probe does not write data, open a business transaction, or rebuild indexes.

An instance still initializes indexes before it begins serving requests. Startup
retries only a read-only MongoDB connection probe; index creation runs once and
the host stops if it fails. See [local startup](local-startup.md) for limits and
recovery. Use `/health/ready` for traffic routing
and `/health/live` for process liveness. Existing `/health` consumers retain
their former process-only status; switch routing probes to `/health/ready` when
database availability must determine whether traffic is sent to the instance.
