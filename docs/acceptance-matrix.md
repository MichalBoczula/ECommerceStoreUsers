# REF-07: acceptance matrix

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
linked acceptance tests. Concurrency and transactional failures remain REF-08.
