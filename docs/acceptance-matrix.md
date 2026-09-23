# Acceptance matrix

[`acceptance-matrix.tsv`](acceptance-matrix.tsv) links each exposed, named HTTP operation
to observed status, cause and a Reqnroll scenario. `scenarioId` is a stable key for
the case; retain it when editing the scenario. Paths and operation IDs come from
the actual endpoint registration. Health endpoints and framework-level
404/405/415 remain technical routes; the framework contract is exercised in
`Common/ApiProblemContract.feature`.

`python3 scripts/check-acceptance-matrix.py` runs locally and in CI. It compares
the matrix with all named endpoint registrations and declared response statuses,
checks unique IDs, and confirms the referenced feature/scenario and outline
example exist. For a new distinct response cause, add a scenario and matrix row.
The matrix is a navigation aid; the HTTP and database assertions are in the
linked acceptance tests. REF-08 adds race cases as their behavior is implemented;
the remaining concurrency and transactional cases are still tracked there.

Customer updates now compare the server-side version loaded from MongoDB with
the stored version before replacing the document. A changed version returns
`concurrency_conflict` (409); a removed customer returns `resource_not_found`
(404). Losing updates do not append history. The version is internal to the
aggregate and is not a client-supplied HTTP precondition; clients can reload
and retry a conflicting request. Existing documents without a version are
treated as version zero and receive version one on their first successful update.

Admin profile updates follow the same internal version check. A stale update
returns `concurrency_conflict` (409), and a removed administrator returns
`resource_not_found` (404). Repeating an unchanged profile PUT returns 200
without incrementing the version or adding history. This does not turn the
HTTP DTO into a client-supplied precondition: a later sequential request
based on stale client state remains a separate contract decision. Existing
Admin documents without `Version` are treated as version zero on first write.
