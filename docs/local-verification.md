# Local verification

From the repository root, with .NET SDK `10.0.100`, Bash and Docker running:

```bash
bash scripts/verify.sh
```

The script runs restore, Release build, formatting, Domain and Application tests
with separate 70% line coverage thresholds, MongoDB Infrastructure and HTTP
acceptance tests with Testcontainers, and a Docker build. Infrastructure coverage
is measured for diagnosis without a percentage gate. Separate HTML and text
coverage reports for all three layers are written to the ignored
`artifacts/verification/{domain,application,infrastructure}-coverage` directories.
CI publishes the same detailed reports in job summaries and artifacts. The SDK version is
selected by `global.json` and used by CI and the build stage of the Dockerfile.

CI additionally checks dependencies, secrets and image vulnerabilities; it does
not publish a Users image. OpenAPI validation remains a separate backlog item.
