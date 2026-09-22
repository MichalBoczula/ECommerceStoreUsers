# Definition of done for a change

This checklist is the review standard for a change, not a claim that all existing endpoints already satisfy it. Apply items relevant to the PR and explain deferred or non-applicable checks.

- The PR links a `REF-xx` or other backlog item, explains resulting behavior and scope, and justifies added packages or public contract/document changes.
- Code follows the API → Application → Domain/Infrastructure boundaries; MongoDB document mapping, index setup and history/transaction implications are reviewed if affected.
- An endpoint change covers request validation, safe error responses, status and media type in OpenAPI, source descriptions of flows/validation policies, and tests for the affected success and distinct failure causes. For writes, verify current document, history, and rollback or no partial state after failure.
- Relevant Domain, Application, MongoDB integration, and HTTP acceptance tests pass. Domain and Application retain separate minimum 70% line coverage; coverage percentage does not prove full HTTP path coverage.
- `dotnet restore`, Release build, `dotnet format --verify-no-changes`, and relevant suites from `AGENTS.md` pass. For contract changes, compare generated OpenAPI with real HTTP behavior. CI security and Docker checks pass where applicable.
- Ordinary compiler warnings are allowed; material warnings are reported. Vulnerabilities have a separate agreed severity gate. Tests, coverage and security checks are not weakened to make a PR pass.
- Update README/ADR or source documentation when behavior or decisions change. The PR records actual commands/results, unavailable checks with reasons, and any follow-up. Update backlog status only after verifying the criterion.

OpenAPI CI validation remains in REF-11; toolchain and artifact improvements remain in REF-03. This checklist alone does not implement either gate.
