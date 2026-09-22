# Local verification

From the repository root, with .NET SDK `10.0.100`, Bash and Docker running:

```bash
bash scripts/verify.sh
```

The script runs restore, Release build, formatting, Domain and Application tests
with separate 70% coverage thresholds, MongoDB Infrastructure and HTTP acceptance
tests with Testcontainers, and a Docker build. Results and coverage reports are
written to the ignored `artifacts/verification` directory. The SDK version is
selected by `global.json` and used by CI and the build stage of the Dockerfile.

CI additionally checks dependencies, secrets and image vulnerabilities; it does
not publish a Users image. OpenAPI validation remains a separate backlog item.
