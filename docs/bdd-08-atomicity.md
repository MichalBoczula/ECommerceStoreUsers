# USERS-BDD-08 — Concurrency and real MongoDB atomicity

Task 8 strengthens six existing HTTP race cases, adds fourteen BDD cases, and
keeps the original identities. Current inventory: **258 expanded BDD cases,
114 declarations in 45 feature files, 241 named-operation links and 279
acceptance-project cases** (258 BDD plus the unchanged 21 standalone facts).
Infrastructure adds two actual concurrent-create cases, taking that suite to 90.
This records the original task-8 audit. The later
[creation-conflict fix](bdd-08-create-conflict-fix.md) closes the two create 500
cases and adds two concurrent HTTP cases (current total: 260 BDD/281 acceptance).
No production behavior, API contracts, source flow/rule exports, dependencies,
production indexes or deployment configuration change.

## Deterministic HTTP interleavings

The race repository decorators load the losing request's original value, then
commit the competing effect through a real MongoDB repository (or perform a
direct competing database delete) before returning the stale value. They do not
pretend that two HTTP requests run simultaneously. The Infrastructure suite
separately overlaps actual repository calls with `Task.WhenAll`; there are no
sleeps or timing-based winner assumptions.

| Write | Race causes | HTTP result |
| --- | --- | --- |
| Individual update | Another complete individual wins / customer deleted | 409 concurrency_conflict / 404 resource_not_found |
| Company add | Competing new company has same or different requested TaxId / customer deleted | 409 concurrency_conflict / 404 resource_not_found |
| Company update | Competing company update uses same or different requested TaxId / customer deleted | 409 concurrency_conflict / 404 resource_not_found |
| Admin update | Another complete profile wins / admin deleted | 409 concurrency_conflict / 404 resource_not_found |
| Favorite add/remove | Pair inserted after existence check / removed after load | 409 resource_conflict / 404 resource_not_found |
| Customer/Admin create | Target ExternalId inserted after lookup | 409 resource_conflict after the [follow-up fix](bdd-08-create-conflict-fix.md) |

Company races begin with two distinct companies. The contender changes different
names and addresses, including nullable apartment data, so complete snapshots
reject mixed or lost fields. `SameTaxId` is still a stale aggregate-version
failure: it must not be mislabeled as the sequential duplicate-company
`resource_conflict` already covered by task 6. Updating a company retains the
existing Customer.UpdatedAt behavior. The competing database delete has no API
route and deliberately leaves the prior history unchanged.

Favorite race fixtures also include another product and another client. Checks
retain all four winner fields (including ID/date), compare the entire collection,
and preserve those unrelated entries. Favorites have no history collection.

`AtomicityState` records canonical Extended JSON for all five collections:
before the interleaving, immediately after the competing commit, and after the
loser returns. It checks the complete intended winner, next version, Insert or
Update action, generated history ID/date, exact latest-history profile, all prior
history metadata and unrelated data. The final state must exactly equal the
committed winner. Every HTTP response also passes the shared nine-field problem
contract with empty errors/missingProperties and a generated trace ID. Shared
HTTP recording still provides actual method/path/query/body byte evidence.

## Real rollback versus no write

The six existing `WriteFailures.feature` examples still throw **before** calling
the inner repository. They now check the full safe problem and all five
collections, record that the real write was not invoked, and remain explicitly
no-write fault tests.

Six new `MongoAtomicity.feature` examples execute the real Customer create,
individual update, company add/update, and Admin create/update transactions.
A named unique history index, scoped by a partial filter to the isolated target
ExternalId, rejects history insertion with MongoDB error **11000**. Unrelated
version-one aggregate histories and Favorites are seeded through checked HTTP.
Create fixtures additionally contain a deliberately historical-only target
blocker; update fixtures contain a target at version zero. These test-only
indexes and historical fixtures never become production schema changes.

The production repository writes current first, then history, within the same
transaction. An observing decorator catches the actual MongoWriteException,
checks its code, history collection and index name, records that evidence and
rethrows the same exception. It neither manufactures the fault nor bypasses a
write. The safe 500 response must accompany exact before/after equality across
all five collections, including every retained history field and metadata. The
four original Infrastructure history-insert rollback facts remain and now also
compare complete snapshots rather than selected fields/counts alone.

## Actual overlapping repository calls

Existing simultaneous Customer/Admin update tests now identify the successful
contender from the outcomes and check its complete current and history profile,
one version advance, all old history, and unchanged collections. Their stale
version counterparts receive the same complete checks. The Favorite concurrent
add checks the exact winning ID and timestamp as well as the pair; concurrent
delete still requires exactly one true deletion and an empty target result.

Two concurrent Customer/Admin create facts use the real startup unique indexes
and verify a complete current winner plus its matching Insert history, with no
orphan losing history. The original audit characterized raw ExternalId duplicate
key 11000 or transient WriteConflict 112 outcomes. The
[follow-up fix](bdd-08-create-conflict-fix.md) maps those outcomes to the existing
ResourceAlreadyExistsException or ConcurrencyConflictException. Arbitrary
exception types still fail the tests; full winning-state checks remain.

## Closed create gap and remaining work

The original task-8 audit found that a create losing the ExternalId race after
its lookup returned safe 500 while a sequential duplicate returned 409. That gap
is now closed by the [creation-conflict follow-up](bdd-08-create-conflict-fix.md),
including create-path WriteConflict 112 mapping. Unknown `_id` and history
index failures retain their original safe fallback. No retry loop is introduced.

Run the existing source gate and full PR CI; independently inspect the produced
bundle, fourteen new identities, all strengthened race/no-write cases, real
MongoDB error evidence, complete snapshots and unchanged source exports. Local
.NET/Docker runtime checks require unavailable prerequisites in the editing
environment, so their results must come from the actual CI jobs. The existing
70% separate coverage and security gates remain unchanged. Tasks 9–12, Azure
rollout and final LiveDocs snapshot selection remain deferred.
