# ECommerceStoreUsers

.NET 10 Minimal API for customer profiles, admin profiles, and client favorites.
The API uses application services and a MongoDB replica set. Customer and admin
writes maintain current and history collections; favorites have a separate
collection. The repository includes Domain, Application, Infrastructure and
Reqnroll HTTP acceptance tests.

## Requirements

- Docker with Compose for the local API and MongoDB replica set.
- Bash, Python 3, .NET SDK `10.0.100` (pinned in [`global.json`](global.json)),
  Node.js 22, and Docker for the full [`scripts/verify.sh`](scripts/verify.sh) check.
- For Windows Compose, `APPDATA` must point to the usual Windows profile folder
  because [`docker-compose.yml`](docker-compose.yml) mounts UserSecrets and HTTPS
  certificate paths from it. On another host, set `APPDATA` to an appropriate
  local folder before starting Compose. HTTPS is not needed for the HTTP examples.

## Run locally with Docker Compose

From the repository root, copy [`.env.example`](.env.example) to the ignored
`.env` file and replace the example password with a local password. In Bash:

```bash
cp .env.example .env
# Edit .env: set MONGODB_PASSWORD to a local-only password.
docker compose up -d --build
curl -i http://localhost:8080/health/live
curl -i http://localhost:8080/health/ready
```

On Windows PowerShell, use `Copy-Item .env.example .env` instead of `cp`.
Compose starts MongoDB, initializes the single-node `rs0` replica set, then
starts the API. Wait for `/health/ready` to return `200` before making API calls.
The API receives its connection string through
`MongoDbSettings__ConnectionString`; the collection names and database name are
defined in [`appsettings.json`](src/ECommerceStoreUsers.API/appsettings.json).
See [local startup](docs/local-startup.md) for probe deadlines and failures.

Stop the stack with `docker compose down`. Its named `mongo-data` volume retains
data; `docker compose down --volumes` also removes that local data.

## API and generated documentation

- Swagger UI: <http://localhost:8080/swagger>
- Generated OpenAPI: <http://localhost:8080/swagger/v1/swagger.json>
- Source-generated flow and policy descriptions: `/users-documentation/flows`
  and `/users-documentation/validations`.

Business routes live under `/customers`, `/admins`, and `/favorites`. Read the
generated OpenAPI for their current operations, bodies and response contracts;
the repository does not maintain a second handwritten endpoint catalog.
Acceptance scenarios are related to operation IDs in
[`docs/acceptance-matrix.md`](docs/acceptance-matrix.md). The error response
format is described in [`docs/api-problem-contract.md`](docs/api-problem-contract.md).

| Probe | Purpose |
| --- | --- |
| `/health/live` | Process liveness, independent of MongoDB. |
| `/health/ready` | Writable MongoDB replica-set primary and sessions; returns `503` when unavailable. |
| `/health` | Compatibility alias for liveness. |

Health checks use plain-text responses; see [health details](docs/health-checks.md).

## Verify changes

Run `bash scripts/verify.sh` from the repository root. It verifies source links
and architecture boundaries, restores and builds the solution, checks formatting,
runs Domain and Application tests with separate 70% line coverage gates,
Infrastructure and HTTP acceptance tests on Testcontainers, exports and lints
generated OpenAPI, and builds the Docker image. It requires a running Docker
daemon; generated reports remain under the ignored `artifacts/verification`
directory. See [local verification](docs/local-verification.md) and
[`AGENTS.md`](AGENTS.md) for focused commands. CI also runs dependency, secret
and container image checks; it does not publish a Users image.

## Troubleshooting

- **Startup fails or `/health/ready` returns `503`:** inspect
  `docker compose ps` and the logs for `mongo-init`, `compose-mongodb`, and
  `ecommercestoreusers.api`. Check the `.env` credentials, replica-set primary
  and MongoDB reachability. The API stops on a conflicting named index instead
  of changing it automatically; see [index evolution](docs/mongo-index-evolution.md).
- **The host cannot connect using the Compose URI:** `compose-mongodb` is the
  hostname inside the Compose network. Port `27018` is exposed for local tools,
  but the replica set advertises its container hostname; use a hostname the
  client can resolve when connecting from outside Compose.
- **Acceptance or Infrastructure tests cannot start:** start Docker and verify
  that Testcontainers can create a MongoDB replica set. The OpenAPI export test
  itself starts without MongoDB.
- **`APPDATA` mount error:** set `APPDATA` to a host folder accessible to Docker,
  or use the default Windows profile location. The Compose file contains these
  mounts even when using only HTTP locally.
