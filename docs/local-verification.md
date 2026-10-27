# Local verification

Run `bash scripts/verify.sh` from the repository root with the prerequisites
in the README. It runs the stages in this order: **Source checks**, **Restore
and build**, **Format**, **Domain**, **Application**, **Infrastructure**,
**Acceptance**, **OpenAPI**, **Docker**.

`scripts/verify.sh` and the CI jobs call the same portable entry points in
`scripts/ci.sh`: `source`, `build`, `format`, `contract`, and `test <suite>`.
Each test invocation restores the solution in its own runner. Set
`VERIFY_RESULTS_DIR` to change the artifact root and `VERIFY_SUMMARY_FILE`
to append suite and coverage results to a chosen Markdown file. GitHub
Actions supplies its step summary path and uploads the generated artifacts;
the scripts contain the quality checks and failure rules.

The ignored `artifacts/verification/` directory has the following layout:

| Output | Path |
| --- | --- |
| Test results | `{domain,application,infrastructure,acceptance}/<suite>.trx` |
| Coverage input | `{domain,application,infrastructure}/<collector-id>/coverage.cobertura.xml` |
| Coverage reports | `{domain,application,infrastructure}-coverage/Summary.txt` and `index.html` |
| Combined local summary | `summary.md` |
| Generated API contract | `openapi.json` |
| Operation links | `operation-links.json` |
| Generated flow and policy descriptions | `flows.json`, `validation-policies.json` |

Acceptance tests also write raw Allure results and attachments to the ignored
`artifacts/allure-results/acceptance/` directory; the acceptance command clears
previous results first. The filtered API export writes the OpenAPI, flow and
validation-policy sources without MongoDB. CI packages those exports with real
acceptance results and feature files using the pinned LiveDocs reusable workflow.
Local verification does not upload documentation or require the Blob SAS secret.

Each run clears the previous test and coverage outputs and generated OpenAPI
before executing. The script exits nonzero at the first failing stage, reporting
its name and exit code. Each test suite must produce exactly one TRX with
nonzero total and passed counts and no failures. A missing coverage input or
report fails verification. Domain, Application and Infrastructure each require
at least 70% line coverage.
OpenAPI validation and Docker build must succeed. CI also runs dependency,
secret and image vulnerability gates; `verify.sh` does not replace them.

The two MongoDB jobs temporarily use `ubuntu-22.04`. A rolling Ubuntu 24 image
triggered MongoDB's kernel startup guard. The script logs the actual Docker-server
kernel and checks the same Mongo 8 image before both full database suites. Task 9
recorded kernel 6.8.0-1064-azure; labels can change, so use each run's actual log.
Review this pin before the published Ubuntu 22 runner retirement on 2027-04-17;
see [compatibility scope](bdd-09-technical.md#mongodb-test-runner-compatibility).
No database tests, coverage/security gates or startup diagnostics are skipped.

The filtered, read-only OpenAPI export does not start the Reqnroll Mongo fixture.
The export test already removes database initialization from its HTTP host;
normal acceptance runs still start the real replica set and exercise writes.

