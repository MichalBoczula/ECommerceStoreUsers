# USERS-BDD-05: Favorites success and complete collection evidence

Favorites remain a single MongoDB collection with a unique `(ClientId,
ProductId)` index and no history. The five original success/no-op scenarios,
their titles, IDs, inputs and assertions are retained. Six new cases cover
preservation and client isolation. The suite now has 91 expanded BDD cases,
71 declarations in 42 features and 81 operation/scenario matrix links. The
acceptance-project total is 110, including the existing 19 standalone cases.
The fixed 81-case baseline is unchanged.

## Success coverage

| Operation | Explicit response checks | Persisted collection checks |
| --- | --- | --- |
| Add | 200 JSON; expected client/product; generated nonempty ID; UTC AddedAt within the HTTP request window | Exactly one new pair, matching all four returned fields; every previous document unchanged |
| List | 200 JSON; exact expected pairs; every ID/client/product/date matches the pre-request document; no other client's entries | Entire collection unchanged |
| Remove | 204, zero response bytes and no content type | Only the requested client/product pair removed; all other fields and entries retained |
| Clear | 204, zero response bytes and no content type | Only the requested client's entries removed; all other-client entries retained |
| Clear empty | The same bodyless 204 | Entire collection unchanged, including when another client has entries |
| List empty | 200 with an empty JSON array, even when another client has the same product | Entire collection unchanged |

`FavoriteSuccessState` is scenario-local. Opted-in success operations record the
whole known-field collection before and after the actual HTTP call, before
response assertions can fail. Explicit Gherkin expectation tables specify
client/product pairs, collection counts and write effects. A table never derives
its expected pairs from the response. Preserved entries are compared in full,
including IDs and timestamps; counts alone cannot satisfy these checks.
Read response content from the buffered body so original and stronger assertions
can both run.

The original HTTP setup requests now also verify all response fields, persisted
data and the absence of changes to previously stored entries. Their request and
response envelopes remain paired by the shared recorder. All collection/setup/
expectation/verification JSON uses the existing service, scenarioId and
instanceId wrapper. Database evidence is a deserialized projection of every
known Favorite field, not a claim to capture raw BSON bytes or a history log.

## Ordering and fixtures

The real repository sorts only by `AddedAt` descending. The new list scenario
uses three target-client entries across two distinct dates, including a pair of
equal timestamps, and another client's entry. It checks exact membership and
complete fields separately from non-increasing dates. No secondary order is
required for the tie; sorting snapshots by ID only makes database comparisons
stable and does not define the HTTP list order.

Existing happy-path fixtures still add entries through HTTP. New preservation/
ordering fixtures insert real MongoDB documents with explicit dates from the
Gherkin table and generated IDs, then verify the complete arranged collection.
These are deliberately controlled fixtures, not evidence that the public add
API accepts an AddedAt input. No sleeps, mocked repository or new history
collection are introduced. API-generated dates are checked in the request
window, with millisecond normalization only when comparing HTTP precision to
MongoDB's BSON date precision.

## New scenario links

- `REF05_FavoriteAdd_PreservesOtherEntries`: another product for the target
  client and the same requested product for a different client survive the add.
- `REF05_FavoriteList_OrderAndIsolation`: exact target entries and stored fields,
  newest-first order, timestamp ties and client filtering.
- `REF05_FavoriteList_EmptyClient`: no target entries returns `[]`, preserving
  another client's entry for the same product.
- `REF05_FavoriteRemove_ExactPair`: another target product and another client's
  entry for the same selected product survive removal.
- `REF05_FavoriteClear_ClientIsolation`: all target entries removed, with both
  other-client entries unchanged.
- `REF05_FavoriteClear_NoOpPreservesOtherClient`: an empty client's clear retains
  the complete nonempty collection belonging to another client.

## Verification boundary

Run `bash scripts/ci.sh source` and the existing full PR CI. CI provides the .NET
10 build/format, unit/MongoDB integration/acceptance tests, separate 70% layer
coverage, generated OpenAPI/lint, documentation bundle and security/image gates.
The implementation environment has no .NET SDK or Docker daemon, so runtime
claims require those actual CI results. The PR records the run and independently
checks the produced case/attachment inventory, manifest hashes and Favorites
before/after evidence before merging.

No API, DTO, repository, validation policy, index, package, CI gate or Azure
resource changes belong to this task. Existing failure/race scenarios remain;
their stronger failure/atomicity evidence belongs to tasks 6–8. The final
LiveDocs selection/refresh remains task 12, with manual Azure rollout separate.
