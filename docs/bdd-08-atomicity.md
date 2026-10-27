# USERS-BDD-08 — Concurrency and real MongoDB atomicity

Task 8 strengthens six existing HTTP race cases, adds fourteen BDD cases, and
keeps the original identities. Current inventory: **258 expanded BDD cases,
114 declarations in 45 feature files, 241 named-operation links and 279
acceptance-project cases** (258 BDD plus the unchanged 21 standalone facts).
Infrastructure adds two actual concurrent-create cases, taking that suite to 90.
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
| Customer/Admin create | Target ExternalId inserted after lookup | **500 internal_error (known functional gap)** |

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

Two new concurrent Customer/Admin create facts use the real startup unique
indexes. Each requires exactly one success and one actual ExternalId duplicate-key
11000 or transient transaction WriteConflict 112, one complete current winner
and its matching Insert history, and no
orphan losing history. They deliberately characterize the current repository
exception; HTTP mapping is a separate responsibility. Overlapping transactions
can abort at storage contention before reporting a duplicate key. The 112 case
must carry TransientTransactionError; arbitrary exception types/codes fail. The
actual losing type/code is retained in xUnit output. The deterministic HTTP
cases separately require exactly ExternalId error 11000. See MongoDB’s
[transaction write-conflict guidance](https://www.mongodb.com/docs/manual/core/transactions-production-consideration/#in-progress-transactions-and-write-conflicts)
and [error-code reference](https://www.mongodb.com/docs/manual/reference/error-codes/).

## Known gap and remaining work

A normal sequential duplicate Customer/Admin create returns 409 from the
application's existence check. Losing the race *after* that check instead lets
the raw MongoWriteException reach the safe fallback, yielding 500 internal_error.
The two new HTTP cases document this precisely and prove that data stays intact.
A small production follow-up should map a **confirmed target ExternalId**
uniqueness collision to the existing 409 resource_conflict contract, without
reclassifying arbitrary `_id` or history index failures. Create-path transient
WriteConflict 112 mapping also needs an explicit follow-up decision; the current
create repositories let it reach the same fallback. No broad duplicate-key
catch or new retry behavior is introduced in this evidence task.

Run the existing source gate and full PR CI; independently inspect the produced
bundle, fourteen new identities, all strengthened race/no-write cases, real
MongoDB error evidence, complete snapshots and unchanged source exports. Local
.NET/Docker runtime checks require unavailable prerequisites in the editing
environment, so their results must come from the actual CI jobs. The existing
70% separate coverage and security gates remain unchanged. Tasks 9–12, Azure
rollout and final LiveDocs snapshot selection remain deferred.
