# Local verification

From the repository root, with .NET SDK `10.0.100` or a newer .NET 10 feature band (selected by `global.json`), Bash and Docker running:

```bash
bash scripts/verify.sh
```

The script runs restore, Release build, formatting, Domain and Application tests
with separate 70% line coverage thresholds, MongoDB Infrastructure and HTTP
acceptance tests with Testcontainers, and a Docker build. Infrastructure coverage
is measured for diagnosis without a percentage gate. Separate HTML and text
coverage reports for all three layers are written to the ignored
`artifacts/verification/{domain,application,infrastructure}-coverage` directories.
It also exports OpenAPI from the running API in an isolated test host and validates
the generated document with Redocly CLI. The export starts without MongoDB; the
acceptance suite still uses its MongoDB Testcontainer. The generated document is
written to ignored `artifacts/verification/openapi.json` and requires Node.js 22.
Acceptance uses one MongoDB replica set container per test run and a separate
database, API host, and HTTP client per scenario. Scenario hooks dispose the host
and drop its database on both success and failure; isolation scenarios verify
current and history collections between independent runs.
CI publishes the same detailed reports in job summaries and artifacts. CI selects the SDK
using `global.json`; the Docker build stage uses SDK 10.0.100.

CI additionally checks dependencies, secrets and image vulnerabilities; it does
not publish a Users image.
