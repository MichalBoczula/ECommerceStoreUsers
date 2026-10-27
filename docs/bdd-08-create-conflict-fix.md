# USERS-BDD-08-FIX — Creation conflicts return 409

The task-8 race audit found a Customer/Admin create path that returned safe
500 after the initial ExternalId lookup had already passed. This follow-up
closes that gap without changing the application services, DTOs, storage schema,
production indexes, source flow/rule identities or global error fallback.

## Resulting behavior

| Failure | Result |
| --- | --- |
| Existing ExternalId seen by the application lookup | Existing 409 resource_conflict |
| Current insert loses the known ExternalId unique-index race and the committed ExternalId is confirmed after abort | 409 resource_conflict, with the same entity/action/ExternalId detail as the sequential duplicate |
| Create transaction returns MongoDB WriteConflict 112, as MongoCommandException or MongoWriteException | Existing 409 concurrency_conflict with reload/retry detail |
| Duplicate current `_id`, including when the ExternalId also already exists | Original MongoWriteException; safe HTTP 500 fallback |
| Duplicate key from a different unique index, or from history insertion | Original failure; safe HTTP 500 fallback |
| Other write/commit failures | Existing safe fallback |

Both create repositories remember whether the current insert completed. The
11000 filter applies only before that point and requires the exact named index
marker `index: UX_Customer_ExternalId dup key:` or
`index: UX_Admin_ExternalId dup key:` in the driver error. After aborting the
failed transaction, they confirm the target ExternalId through the current
collection. An unconfirmed collision rethrows the original exception. They do
not classify all 11000 errors as duplicates just because the ExternalId exists.

The explicit named marker uses the server message format exercised by real
MongoDB 8; an unrecognized error format fails safely rather than becoming an
unconfirmed resource conflict. No new provider-specific compatibility promise is
made. The 112 mapping follows the existing update-repository behavior. No retry
loop, delay, new exception type or broad exception-handler change is introduced.
All error paths retain the existing bounded best-effort transaction abort.

CreateCustomer and CreateAdmin OpenAPI operation descriptions explain the two
409 codes. Both operations already declare the common 409 problem schema;
statuses, schemas, request/response DTOs, 12 flows/92 steps and 6 registered
Domain policies are unchanged. Application flow identity/order stays stable:
validation → initial lookup → save → response; save can now surface the already
supported conflict exceptions. The new failure branch is documented here and
in the operation descriptions; task 11 will enrich source flow descriptions.

## Evidence and cases

The original two `REF08_CreateCustomer_UniqueKeyRace` and
`REF08_CreateAdmin_UniqueKeyRace` IDs, example parameters and titles are retained.
Their intentional contract migration is **500 internal_error → 409
resource_conflict**. The decorator still returns the stale empty lookup after a
real competing repository create commits. The observed real repository call
now throws ResourceAlreadyExistsException; the test checks its complete entity,
action and ExternalId rather than claiming the raw MongoDB error escaped the
mapper. Complete winner/current/history and all-five-collection checks remain.

Two additional `REF08_CreateCustomer_ConcurrentHttp409` and
`REF08_CreateAdmin_ConcurrentHttp409` examples issue actual concurrent HTTP
requests with distinct names, emails, companies and addresses from a Gherkin
table. A test-only two-reader gate ensures both initial lookups are empty before
allowing either service to save. Its timeout only prevents a deadlock; assertions
do not depend on time, a particular winner, or which valid conflict is reported.
Both repositories execute normally. Exactly one response must be 200 and one
409 resource_conflict or concurrency_conflict, with full paired HTTP evidence.

The successful response is checked against the complete winning request and
used only for generated IDs/timestamps. Current and Insert history must match
that intended profile, at version zero, preserving unrelated version-one
Customer/Admin histories and Favorites. The winner snapshot in these two
concurrent examples is taken after both responses finish; it is checked against
that independently constructed expected profile. The deterministic interleaving
cases still capture their winner immediately after its commit.

The two existing overlapping Infrastructure create facts now require one
success and one mapped ResourceAlreadyExistsException or
ConcurrencyConflictException, and still verify complete winning current/history
state. Eight additional real-MongoDB theory cases (four per entity) exercise the
known ExternalId constraint, an `_id` collision with a different ExternalId, an
`_id` collision where ExternalId also exists, and an unrelated named unique
index. All eight compare complete snapshots after rejection. The original four
Infrastructure history-insert rollback facts and all six HTTP history-insert
rollback cases retain their original raw failure/safe-500 expectations.

## Verification and scope

Current inventory: **260 expanded BDD cases, 115 declarations in 45 feature
files, 243 named-operation links and 281 acceptance-project cases** (21 standalone
unchanged). Infrastructure has 98 cases; Domain and Application membership is
unchanged. The existing source gate, full PR CI, separate coverage/security/image
gates and independent produced-bundle audit apply. Runtime checks require .NET
and Docker; those are unavailable locally and must be verified in actual CI.

Audit all prior identities, the two deliberate status/code changes, both new
concurrent HTTP outcomes, full problems and complete stored data. Preserve all
six real history-rollbacks, the pre-repository failure distinction, and unchanged
OpenAPI schemas/flow/policy exports. The only operation-link changes should be
two corrected statuses and two additions; the only OpenAPI changes should be the
two appended create-operation descriptions.

Tasks 9–12 remain. The hosted-runner kernel/MongoDB startup incompatibility
recorded by task 8 remains a separate task-9 concern; this fix does not change
CI runners, test images or startup behavior. No Azure rollout or final LiveDocs
selection/refresh.
