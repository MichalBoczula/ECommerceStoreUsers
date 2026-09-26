# Local verification

Run `bash scripts/verify.sh` from the repository root with the prerequisites
in the README. It runs the stages in this order: **Source checks**, **Restore
and build**, **Format**, **Domain**, **Application**, **Infrastructure**,
**Acceptance**, **OpenAPI**, **Docker**.

The ignored `artifacts/verification/` directory has the following layout:

| Output | Path |
| --- | --- |
| Test results | `{domain,application,infrastructure,acceptance}/<suite>.trx` |
| Coverage input | `{domain,application,infrastructure}/<collector-id>/coverage.cobertura.xml` |
| Coverage reports | `{domain,application,infrastructure}-coverage/Summary.txt` and `index.html` |
| Combined local summary | `summary.md` |
| Generated API contract | `openapi.json` |
| Operation links | `operation-links.json` |

Each run clears the previous test and coverage outputs and generated OpenAPI
before executing. The script exits nonzero at the first failing stage, reporting
its name and exit code. Each test suite must produce exactly one TRX with
nonzero total and passed counts and no failures. A missing coverage input or
report fails verification. Domain and Application each require at least 70%
line coverage; Infrastructure coverage is reported without a percentage gate.
OpenAPI validation and Docker build must succeed. CI also runs dependency,
secret and image vulnerability gates; `verify.sh` does not replace them.
