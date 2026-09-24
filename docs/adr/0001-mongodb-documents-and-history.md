# ADR-0001: Use MongoDB documents and explicit aggregate history

- Status: Accepted
- Date: 2026-09-24

## Context

The Users service owns Customer and Admin profiles and client Favorites. A
Customer has individual data and an embedded set of companies; a profile is
loaded and saved as an aggregate. Admin is a separate aggregate. Changes to
these profiles need a durable history alongside the current version. Favorites
are individual client/product associations with different write semantics.

## Decision

Use MongoDB as the service-owned database. Keep domain aggregates and MongoDB
document classes separate: the Domain project owns behavior and repository
interfaces, while Infrastructure owns the BSON documents, mappings and
repositories. `MongoDbContext` resolves collection names from `MongoDbSettings`.
No MongoDB document type crosses the HTTP or application-service boundary.

- Store current Customer, Admin and Favorite documents in separate collections.
  Store Customer and Admin history in their own append-only collections. Each
  history entry contains a snapshot of the resulting aggregate, its action
  (`Insert` or `Update`), changed time, aggregate ID and version. Favorites
  currently have no history collection.
- Customer and Admin create each insert the current and history documents in
  one MongoDB transaction. Updates replace the current document using a
  `Version` match and insert the new history snapshot in the same transaction.
  A version mismatch is a concurrency conflict; a missing Customer on update
  is distinguished from a conflict. MongoDB write conflicts during update are
  mapped to domain concurrency conflicts. A failed write/commit attempts a
  bounded transaction abort, preserving the original exception.
- Admin profile updates with unchanged persisted values succeed without a new
  version or history entry. Customer update paths call the repository to
  replace and append history; do not claim a general Customer no-op guarantee.
  Favorites use individual insert/delete operations without these transactions;
  the unique `(ClientId, ProductId)` index protects duplicate associations.
- Require a writable MongoDB replica-set primary with sessions for the profile
  transactions. Compose initializes `rs0`; readiness checks the actual MongoDB
  capability. Startup performs a bounded read-only connection probe, then
  creates the named indexes once. It does not retry a business write or an
  entire index operation.
- Keep named indexes for unique Customer/Admin external IDs, Customer company
  tax ID lookup, history aggregate ID lookup, and unique Favorite client/product
  pairs. Do not automatically drop/rebuild indexes when an existing definition
  conflicts. A reviewed, separate data migration is needed for a changed index
  or document shape. Existing documents without `Version` are read as version
  zero and the update filter explicitly permits a missing version field.

## Consequences

Current and history for Customer/Admin commit together on supported replica
sets. Optimistic version matching prevents a stale replacement from silently
overwriting the current document. MongoDB availability and replica-set support
are required for those writes; a standalone MongoDB server is insufficient.
History grows independently of current collections, so retention, backup and
restore need an operational policy before a production deployment. The current
code does not implement automatic schema versioning or a general document
migration framework. Index changes require planned rollout and data checks;
see [index evolution](../mongo-index-evolution.md).

## Alternatives considered

- **One document with embedded history:** simpler single-record writes, but
  repeated snapshots would grow the current profile document and couple
  history reads and retention to the aggregate's size.
- **Current and history in separate collections without a transaction:** would
  allow a current change without the corresponding history after a partial
  failure. Customer/Admin writes therefore use a transaction.
- **Use the same history/transaction model for Favorites:** not adopted for
  the existing association operations; their uniqueness and delete outcomes
  are handled by the repository and compound index.
