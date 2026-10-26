# USERS-BDD-02: shared HTTP evidence and scenario identities

## Scenario sources and identities

All 61 scenario declarations have a readable Gherkin description. The first
step publishes that description to the active Allure result. Existing step
bodies, scenario titles and behavioral example values are retained.

Each scenario obtains `scenarioId` from exactly one source:

- a scenario-level `@allure.label.scenarioId:REFxx_Name` tag; or
- a `scenarioId` Examples column where rows correspond to different matrix
  cases, such as write/dependency failures and framework errors.

The 71 acceptance-matrix IDs are reused directly. Technical cases outside the
matrix receive REF05/REF06/REF09 IDs. The two isolation examples intentionally
share one scenario ID while retaining distinct `run` parameters. This gives
80 scenario IDs and 81 example-instance IDs.

The existing matrix gate checks identities, descriptions, matrix-ID agreement
and client creation. It still preserves the original baseline. The only
baseline comparison adjustment ignores the newly introduced `scenarioId`
metadata column; every original behavioral parameter remains part of the
comparison. The fixed `bdd-baseline.json` is unchanged.

`instanceId` is SHA-256 over the scenario ID and ordinally sorted example
parameters, excluding the metadata column. Scenario wording, feature title,
generated database names, HTTP payload IDs and Allure UUIDs are not inputs.
Changing wording therefore preserves identity; a different behavioral example
has a different instance. Each scenario owns its recorder and exchange counter.

The result receives `service=users`, `scenarioId` and `instanceId` labels and a
**Scenario identity** JSON attachment with feature, scenario and parameters.
The two database-only isolation examples receive this identity and description;
they do not fabricate business HTTP exchanges.

## HTTP capture

`ScenarioApiContext.CreateClient(factory)` creates the shared pipeline:
existing OpenAPI response validation wraps HTTP evidence capture, which wraps
the test server. Normal, repository-failure, dependency-failure and all three
race-client groups use it. The fault clients now also receive the existing
response-shape checks.

The unwrapped Swagger bootstrap reads the generated contract before the
pipeline can be created. Standalone export and lifecycle xUnit tests remain
outside the BDD pipeline. The deliberate `OPENAPI_EXPORT_PATH` behavior and
MongoDB startup/cleanup are preserved.

Each actual HTTP exchange emits **HTTP NNN request** and **HTTP NNN response**
JSON attachments with matching `exchangeId=<instanceId>/<sequence>`. The
scenario-wide sequence is shared across replacement clients and safe for nested
calls. Envelopes contain:

| Field | Meaning |
| --- | --- |
| `service`, `scenarioId`, `instanceId`, `exchangeId` | Evidence correlation. |
| `stepPhase` | Reqnroll Given/When/Then phase when the exchange began. |
| `method`, `path`, `query` | Actual request target and method. |
| `status`, `location` | Actual HTTP response status and optional Location. |
| `content.contentType` | Observed Content-Type including charset when present. |
| `content.bodyFormat` | `none`, `empty`, `json`, `invalid-json` or `text`. |
| `content.body` | Parsed JSON value, text, or null for absent/empty content. |
| `content.rawBody`, `byteLength`, `bodyBytesBase64` | Decoded text, exact byte count and lossless original bytes. |

JSON `null` remains a JSON value. A request without content is `none`; present
zero-length content is `empty`. A 204 response has no invented JSON body.
Malformed JSON stays inside a valid evidence envelope with `invalid-json` and
the original bytes. Existing Gherkin-table and manual attachments remain.

The handler buffers content without replacing it, so the server, contract
validator and assertions can still read it. Responses are queued before the
outer validator or scenario assertion can fail. Transport exceptions queue an
**HTTP NNN failure** record with exception type and cancellation flag, then
rethrow. Attachments are flushed from the Reqnroll hook on the active test
context, including failed steps; an AfterScenario flush covers remaining data.

## Verification and boundaries

The existing source stage includes 27 Python contract/inventory regressions.
The acceptance project adds 13 standalone recorder-test cases covering content
preservation, JSON/problem/malformed/null/text/empty formats, exact bytes,
bodyless requests, multiple clients, nested execution contexts, validator
failure, transport/cancellation and stable isolated identities. These are
helper tests rather than new BDD business scenarios: the expected acceptance
total is 100, with the same 81 BDD cases and 19 non-BDD test cases.

Run the existing commands:

```bash
bash scripts/ci.sh source
bash scripts/ci.sh test acceptance
```

Local source checks pass. This workspace has no .NET SDK or Docker daemon;
the PR records full CI and actual fresh-bundle inspection. In that bundle,
check all descriptions/identity labels, all request/response pairs, raw body
formats, and preservation of every original case/example. Do not infer
execution or attachment correctness from the source guard alone.

This task supplies transport evidence. Complete business response assertions,
current/history snapshots, rule coverage, stronger schema validation and
flow/rule/branch links remain in tasks 3–11. Azure rollout and the LiveDocs
report selection remain separate until task 12.
