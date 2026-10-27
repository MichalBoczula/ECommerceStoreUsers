# USERS-BDD-07: Framework failures and unchanged stored data

Task 7 preserves all previous 91 BDD cases and adds 38: 37 framework rejections
and one explicit-null business-boundary control. The suite has 129 expanded BDD
cases, 89 declarations in 43 features and 112 named-operation matrix links.
The acceptance project contains 150 cases: 129 BDD and 21 standalone cases,
including two focused route-checker regressions. The fixed original 81-case
baseline is unchanged. **Task 6 remains pending**; this task does not establish
complete business-rule coverage.

## Request and rejection coverage

| Input or routing cause | Expected result | Coverage |
| --- | --- | --- |
| Array root instead of a DTO object | 400 `invalid_json`, no missing-member list | All seven JSON write operations |
| Literal `null` required request body | 400 `invalid_request` | All seven JSON write operations |
| Empty object missing required root members | 400 `invalid_json`, exact complete missing-member list and DTO name | All seven JSON write operations |
| Missing required nested members | 400 `invalid_json` with exact names and detail | Original Individual/company/Admin/Favorite cases plus billing and shipping Address cases |
| Wrong JSON scalar/collection token | 400 `invalid_json` | Invalid/null Guid, numeric name, boolean company name, object email, object instead of companies array |
| Malformed JSON | 400 `invalid_json` with safe fixed detail | Original Customer case plus multibyte Polish UTF-8 and a sensitive marker in a Favorite request |
| Absent or empty content | 400 `invalid_request` | Original empty Customer body, absent Favorite content, empty Admin update content |
| Unsupported/absent media for a nonempty body | 415 `unsupported_media_type` | Original Customer text/plain plus Favorite text/plain and no content type |
| Missing route or invalid GUID constraint | 404 `route_not_found` | Original unknown route; invalid Favorite client/product, Customer and company segments |
| Wrong method for an existing route | 405 `method_not_allowed`, exact Allow method set | Original Customer lookup plus Favorite collection route |
| Present `externalId: null` reference member | 400 `validation_failed`, exact Admin rule error | One control proving that presence enforcement is different from business validation |

The seven writes are CreateCustomer, UpdateIndividualData, AddCompany,
UpdateCompany, CreateAdmin, UpdateAdminProfile and AddProductToFavorites.
There are no typed query parameters on these endpoints to invent a separate
query-binding failure for. Failed GUID constraints select no endpoint; they
produce routing 404s rather than parameter-binding 400s. All outcomes use the
existing API behavior; this task changes no production endpoint, DTO, policy,
repository, serializer options or public contract.

`JsonRequired`/C# `required` enforce presence. They do not by themselves reject
every explicit null reference. The control case checks the actual HTTP result
for a null Admin ExternalId and its existing policy error. A null Guid member
cannot deserialize; a literal null required body cannot bind. Do not generalize
either into a claim that every nullable/nonnullable reference is rejected by
the framework. Broader business/null-reference behavior remains task 6.

## Exact HTTP and problem evidence

Requests use the existing shared checked client. Raw bodies use UTF-8 byte
content, preserving malformed JSON, whitespace, multibyte text, `null`, `{}`,
`[]`, zero-byte content and absent content without JSON reserialization.
Larger bodies are readable Gherkin docstrings; tables specify method, path,
media and problem expectations. The existing paired recorder retains exact
body bytes/base64, format, method/path/query, phase and stable scenario/instance/
exchange IDs. A request-expectation attachment independently records prepared
bytes and the response still runs through the generated OpenAPI checker.

Every strengthened rejection checks the complete nine-field problem shape:
status, fixed RFC type, title, detail, request instance, code, exact errors,
exact missingProperties and a nonempty generated traceId. Framework errors
have `errors: []`; the business-boundary control has the explicit Admin rule
error. Extra fields, diagnostic details, the request marker and exception type
leaks fail assertions. HTTP status/media are checked, as is the entire Allow
method set for 405. The before/after snapshots and expected/verified problem
objects use the existing service/scenarioId/instanceId evidence wrapper.

The test OpenAPI checker now respects generated `format: uuid` path metadata
when matching a runtime request to an operation. An invalid GUID can therefore
take the existing framework-404 validation path. Two standalone regressions
also prove that a valid matched GUID returning an undeclared 404 still fails;
the operation/status gate is not relaxed.

## Persistence evidence and its limits

All new cases and the original eight framework examples arrange real HTTP
Customer/company, Admin and Favorite data. Both aggregates receive an actual
changed update: current version 1 and history versions 0 and 1. The setup
checks nonempty IDs, successful HTTP statuses, all five collection counts and
the versions. It records the arranged documents, not just an empty baseline.
The seven original dedicated missing-member cases additionally record their
own existing setup before/after and retain their previous assertions.

Every strengthened case compares every field in `customers`,
`customers-history`, `admins`, `admins-history` and `favorites`, sorted by `_id`
only to stabilize comparison. Canonical Extended JSON preserves BSON UUID,
numeric and date types and includes fields beyond a DTO projection. This is
a comparison of complete returned MongoDB documents, not physical storage
bytes. New shared-send cases capture the resulting state before response
assertions. The older dedicated cases capture it in the stronger appended
assertion. Counts alone cannot satisfy the unchanged-data check.

This proves absence of observable business writes for these rejected requests.
It is not evidence of transaction rollback or invocation-count instrumentation;
actual race/atomicity behavior belongs to task 8. Favorites still have no history.

## Verification and publication boundary

Run `bash scripts/ci.sh source`, `git diff --check` and the existing full PR CI.
The implementation environment has no .NET SDK/Docker daemon; runtime evidence
must come from actual .NET 10 CI. Preserve separate 70% layer coverage, all
security/image gates and all original cases. Before merge, the PR records
runtime counts and an independent produced-bundle audit of manifest hashes,
case identities, attachments, exact request bytes, complete problems and
unchanged collection snapshots. Framework route/method/media cases are
explicit Gherkin cases; they are not fictitious named business operations or
new endpoint `.Produces` statuses.

No Azure apply, ACA rollout or selected LiveDocs snapshot change occurs here.
The final LiveDocs refresh remains task 12. Existing master image/archive CI
continues unchanged.
