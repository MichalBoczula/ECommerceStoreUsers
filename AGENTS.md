# Agent instructions

Read this file and [the definition of done](docs/definition-of-done.md) before editing. This repository and ProductsCatalog are equal reference implementations. Transfer a useful practice between them when it fits; preserve each service's own architecture and storage model.

## Scope and architecture

- Preserve .NET 10 Minimal API, application services, MongoDB, aggregate repositories, and current/history documents. Do not introduce CQRS/MediatR merely to match ProductsCatalog.
- `src/ECommerceStoreUsers.API` owns HTTP routes, error handling, OpenAPI, and composition. `src/ECommerceStoreUsers.Application` owns application services and flow descriptors. `src/ECommerceStoreUsers.Domain` owns aggregates, repository contracts, and validation policies. `src/ECommerceStoreUsers.Infrastructure` owns MongoDB documents, mapping, repositories, indexes, and startup configuration.
- Inspect an existing feature before introducing abstractions, packages, or naming patterns. Justify public contracts, persistence-schema, dependency, and architectural changes in the PR.
- Stay within the assigned backlog item. Do not modify secrets, repository rulesets, security gates, or unrelated services as incidental cleanup.

## Contracts and tests

- For an endpoint change, review request/response DTOs, validation, safe error status/content type, `.Produces` metadata, generated OpenAPI, flow descriptors, validation-policy descriptors, and the affected acceptance scenarios together. Generated clients belong to consumer repositories.
- Test pure rules in Domain, use cases in Application, actual MongoDB behavior in Infrastructure, and HTTP outcomes in Reqnroll acceptance tests. Use Testcontainers when MongoDB behavior, indexes, transactions, or concurrency matter. Verify both current and history documents after writes and absence of partial writes on failures.
- Do not hand-edit generated `.feature.cs`; edit `.feature` and step definitions. Do not skip failing tests, hide failures with `continue-on-error`, or lower the 70% Domain/Application line-coverage thresholds to get a green PR.
- Ordinary compiler warnings are permitted and remain visible. Build, format, tests, coverage, agreed vulnerability checks (including NuGet high/critical), secret scanning, contracts, and image scanning are separate gates. REF-02/03 will correct existing CI gaps; do not claim those corrections are already complete.

## Local verification

From the repository root, with .NET 10 and Docker available for MongoDB tests:

```bash
dotnet restore ECommerceStoreUsers.slnx
dotnet build ECommerceStoreUsers.slnx --configuration Release --no-restore
dotnet format ECommerceStoreUsers.slnx --verify-no-changes --no-restore
dotnet test tests/ECommerceStoreUsers.Domain.UnitTests/ECommerceStoreUsers.Domain.UnitTests.csproj --configuration Release --no-restore
dotnet test tests/ECommerceStoreUsers.Application.UnitTests/ECommerceStoreUsers.Application.UnitTests.csproj --configuration Release --no-restore
dotnet test tests/ECommerceStoreUsers.Infrastructure.UnitTests/ECommerceStoreUsers.Infrastructure.UnitTests.csproj --configuration Release --no-restore
dotnet test tests/ECommerceStoreUsers.AcceptanceTests/ECommerceStoreUsers.AcceptanceTests.csproj --configuration Release --no-restore
```

CI additionally checks coverage by layer, secrets, dependency changes on PRs, and builds/scans an image. `.github/workflows/ci.yml` is the current source for CI commands and results. OpenAPI export/lint and several CI wiring improvements remain tracked in later REF items. The local list above does not reproduce all CI gates. Report only checks actually run.

## Handoff

Use `.github/pull_request_template.md`. Report changed behavior, linked backlog ID, commands and tests run, what could not be verified and why, and remaining data/contract risks. A shared REF item is complete only after its criteria have been met in both repositories.
