# USERS-BDD-11 — Flows, rules and evidence joins

The [source description contract](source-description-contract.md) defines stable
IDs, authoritative sources, RAG boundaries and actual-versus-declared evidence.
This task adds meaning to the existing 12 flows / 92 steps, 64 outcome groups,
six active policies, 19 installed rule classes, one uninstalled class and the
shared inline null-object guard. None of those attributes executes a rule or
changes persistence. No new package, committed script, agent or Azure resource.

The existing matrix adds explicit branchId/ruleIds columns. All previous 249
links and 278 BDD identities/titles/outcomes remain. One new populated-state case
covers a valid UpdateCompany request whose Customer is missing at initial load;
it distinguishes that 404 from a missing company or removal before saving.
Current inventory: **279 BDD cases, 123 declarations in 47 features, 250 links
and 346 acceptance cases (279 BDD + 67 standalone)**. The six additional
standalone cases check the evidence join, separate from BDD. Domain/Application/
Infrastructure membership remains 123/54/101; the existing Application flow-order
assertion now checks stable BPMN IDs instead of method-name display text.

Every scenario client receives source documentation observations in addition to
raw HTTP and OpenAPI contract evidence. One matrix/technical primary When
exchange is required for each bound case, while setup and follow-ups retain
supporting roles. The two-host peer POST is explicitly supporting; no original
HTTP phase/outcome changes. Cancellation waits for both raw capture and the
outer documentation recorder, avoiding a flush race without fabricating HTTP.

Generated source links are extended in the existing generator. The existing
source and export gates validate links and compare actual flow/policy exports;
six additional source mutation checks exercise missing prose, duplicate IDs,
wrong status, foreign branches and unrelated/uninstalled rules. Existing
architecture, separate 70% coverage, vulnerability, secret and image gates remain.
Documentation now reflects actual image/archive publication, three coverage
gates, current Mongo runner compatibility and the distinction between RAG-ready
source links and an unimplemented retrieval service. Historical ADR decisions
point to their later publication extensions instead of denying current behavior.

Local .NET/Docker are unavailable. Run the existing source stage, full actual CI
and independently audit its produced bundle/TRX before completing the PR. Verify
all prior cases, the added populated 404, stable source IDs, exact runtime step
prose and policy membership, every exchange link, one primary per binding,
actual error rule IDs, conflict-only rule context, nine truthful cancellation
links and zero orphan attachments. Check that production method bodies and
wire schemas are unchanged apart from clearer descriptions; policy JSON stays
unchanged. StepName display text intentionally changes to authored prose.

Task 12 remains pending: final rendered report audit and LiveDocs selection /
refresh. No Azure deployment, ACA rollout, selected portal snapshot update,
RAG ingestion or historical design-approval claim is included here.
