# ADR-0007: Publish Users source bundles for LiveDocs

- Status: Accepted; extends ADR-0005 generated sources and ADR-0006 publication
- Date: 2026-10-25

## Context

The shared LiveDocs portal already renders Products from a verified private Blob
bundle. Users has the same Reqnroll/Allure adapter and executable documentation
sources, but CI does not yet package its results or archive them for that portal.

## Decision

Follow the Products publication pattern. Keep Users' service architecture and
all existing quality/security gates. Export flows and validation-policy responses
alongside generated OpenAPI in the existing read-only export test. Give Allure a
stable ignored output directory and clear it before each acceptance suite.

Call the commit-pinned LiveDocs producer workflow with project `users`, actual
acceptance results, feature sources and generated contract exports. The quality
gate requires successful source-bundle validation before image work.

Only after a successful master image publication, upload the ZIP to
`reports/users/<commit>/<sha256>.zip`, then a receipt to
`candidates/<version>/users/<run-id>-<sha256>.json` in the existing private
`stecproductsdevmb261013/livedocs` container. Use repository secret
`LIVEDOCS_STORAGE_SAS_TOKEN` with read/create/write permissions. Retries verify
existing bytes and never replace different content. A missing secret or failed
upload fails the archive job; PRs never upload.

LiveDocs selects the exact archived reference in a manifest PR and generates
Allure HTML before image creation. Its runtime remains stateless. The existing
ECommerceStore.Terraform root owns storage and ACA deployment. This change adds
no Azure resources, runtime API storage credentials or automatic rollout.

## Consequences

The producer run, source SHA and bundle digest identify the BDD evidence. Reports
and attachments are archived outside Git, and credentials stay outside manifests
and images. Archival requires the Users Actions SAS secret; credentials remain
outside source and manifests.
CI packaging does not itself make a report visible: a verified archive reference,
LiveDocs image publication and Terraform apply complete the handoff.

Only synthetic acceptance data belongs in attachments. Private archives do not
make the reports baked into a public image or hosted at public ACA confidential.
Preserve every bundle referenced by retained versions. RAG ingestion and the
optional automated importer remain outside this change.

## Alternatives considered

- Commit generated HTML: duplicates rebuildable content in Git.
- Fetch reports from running Users or LiveDocs replicas: introduces runtime storage credentials and mutable content.
- Add dedicated storage or identities: unnecessary for the current shared educational store.
- Hand-maintain API/flow JSON: duplicates executable service documentation sources.

