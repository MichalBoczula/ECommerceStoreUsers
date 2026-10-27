# USERS-BDD-09 — Technical behavior and truthful evidence

Task 9 adds **12 BDD cases** and strengthens **15 existing cases**, retaining all
previous identities, titles and expected HTTP outcomes. Current inventory:
**272 expanded BDD cases, 119 declarations in 46 features, 244 named-operation
links and 297 acceptance-project cases**. The latter is 272 BDD plus **25
standalone xUnit cases**, reported separately. Infrastructure has **101 cases**.
Domain/Application membership and production application behavior are unchanged.

## Health and readiness

The original compatibility `/health` and readiness `/health/ready` cases now
check exact status, `Healthy` body, `text/plain` media type and all default
non-caching headers. A new explicit `/health/live` case has the same complete
checks. Every case begins with checked version-one Customer/Admin data, both
history versions and a Favorite; all five canonical collection snapshots must
remain identical. Readiness additionally records a real `hello` and `ping`,
checking writable replica-set membership and sessions. This independent topology
check accompanies, rather than impersonates, the actual health middleware call.

The existing three standalone readiness facts remain outside BDD. They now
record exact text/header/status evidence in TRX output for preparation without
Mongo, a paused replica set and recovery, and standalone Mongo. They prove that
liveness stays healthy while readiness returns **503 `Unhealthy` plain text**;
these responses are not API problem JSON. The real topology is recorded for the
replica-set and standalone cases. The registered five-second readiness timeout
and existing bounded outage/recovery checks remain enforced.

The expected caching headers come from the framework's
[.NET 10 health middleware source](https://github.com/dotnet/aspnetcore/blob/v10.0.0/src/Middleware/HealthChecks/src/HealthCheckMiddleware.cs),
not new production configuration.

## Startup and linked cancellation

The two original standalone lifecycle cases remain. The paused-startup test now
uses an observing hosted-service wrapper to signal that the **real initializer**
has returned an incomplete task while Mongo is paused. It verifies that host
startup is still pending, unpauses Mongo, checks readiness and every expected
current/history/Favorite index, including unique flags and empty collections.
There is no arbitrary four-second sleep as proof of startup progress. The
observer does not replace initialization or index creation.

The host-stop case now checks its pending startup task directly before stopping,
and awaits cancellation within a bound. Two additional xUnit theory instances
check the supplied start token and `ApplicationStopping` token independently.
These are host-lifecycle outcomes, not HTTP statuses; output says exactly which
cancellation source was observed. The existing Infrastructure startup deadline
and abort-cleanup tests remain intact.

Standalone acceptance membership is **15 recorder + 2 route-matching + 3
readiness + 4 startup/lifetime + 1 export = 25**. Generated OpenAPI export without
Mongo deliberately remains an xUnit fact. It is not relabeled as a BDD report.

## Request cancellation before persistence

Nine new BDD examples cover every write operation: Customer create/individual
update/company add/company update, Admin create/update, and Favorite add/remove/
clear. Valid versioned data is seeded through real checked HTTP. Normal binding,
validation and reads run; a test-only repository-entry gate then waits on the
actual token passed from HTTP through the application service **before invoking
any inner write**. The client cancels only after that entry is signaled.

Each example checks one repository entry, a cancellable and cancelled server
token, a client cancellation exception, no observed HTTP response, no inner
write, and full equality across all five collections. Shared HTTP recording
publishes a request plus its cancellation failure event; it must not fabricate a
response/status to make a request/response pair. Each cancellation request
waits for the recorder's per-request completion signal after the actual terminal
event is enqueued, because HttpClient can notify its caller before the inner
pipeline finishes. Response-body buffering failures also produce a failure event
and dispose the unreturned response. Two focused recorder regression instances verify
that cancellation during response buffering is captured before completion,
without leaking an exception message or inventing a response. The recorded
exception type stays exact: a TestServer transport abort can be
HttpRequestException even when the caller sees cancellation. The cancellation
flag also checks the actual transport token and separately records its state;
ordinary uncancelled transport failures remain distinguishable. These examples prove token
propagation and a **pre-write NoWrite** outcome, not rollback of a transaction
that had already written current. Three new real-MongoDB Infrastructure cases
independently verify already-cancelled Customer/Admin/Favorite writes leave all
five populated collections unchanged. The existing real history-insert rollback
tests retain their separate claims.

Cancellation has no declared HTTP status and therefore adds no invented status
row to the operation/status matrix. Its `operationId`, method, resolved path,
request bytes, scenario/instance/exchange identities and outcome are published
in the case evidence. Health and Swagger routes are likewise outside the named
business-operation matrix. The source guard is not weakened to accept fictional
status values or duplicate scenario IDs.

## Isolation and peer cleanup

Both original marker examples now start with all five collections empty and
write exactly one fully identified marker per collection. Each marker has an
explicit ID, scenario database and collection identity. Exact canonical
before/expected/after snapshots replace selected marker counts alone. These are
technical BSON markers, not valid business Customer/Admin aggregates; no business
HTTP request is claimed in those examples. Their existing shared scenario ID
still has two distinct example-instance IDs.

The dependency-failure isolation case and all eight existing unexpected-failure
examples now use versioned populated fixtures, full nine-field safe problems,
and exact all-five-collection preservation. Documentation-provider failures
retain the same safe 500 and internal-marker redaction checks.

A new two-host scenario uses separate databases on the same real replica set.
Both hosts create a different complete Admin with the **same ExternalId**, each
returning 200 with its own current and Insert-history profile at version zero.
All other collections remain empty. The peer host is then disposed and its
database dropped; absence is checked against the actual database list. A real
GET from the main host still returns the original full profile and exact stored
state, normalizing only the known BSON millisecond timestamp precision between
POST and GET. A separate AfterScenario cleanup handles a failed peer scenario too.
This proves explicit peer cleanup and isolation, without assuming execution
order or claiming every failed-test teardown has been demonstrated. Its primary
matrix link is CreateAdmin; the follow-up GET remains in shared HTTP evidence.

## Documentation reads

The two existing runtime descriptor cases now publish Gherkin expectations for
all twelve descriptor/flow names and 92 ordered steps, plus all six policy names,
35 rule entries and 36 error entries (20 distinct complete error signatures).
Checks require nonempty step names/BPMN IDs, unique IDs per flow, contiguous step
order, and complete rule name/message/entity triplets. No new policy is activated
and richer branch/effect/RAG descriptions remain task 11.

A new read-only Swagger case checks the entire actual generated document against
that same host's bootstrap contract, 14 unique named operations and 23 schemas.
This proves endpoint consistency and no writes; it is not a substitute for the
stronger field/type/nullability/format validation planned for task 10. All three
technical documentation reads and both provider-failure paths preserve populated
current/history/Favorite data. The existing OpenAPI/flow/policy exports should
remain semantically unchanged; the matrix gains only the one isolation link.

## MongoDB test-runner compatibility

Task-8 TRX stderr identified MongoDB's kernel startup guard, not missing shell
packages or a business assertion. Ubuntu 24.04 rolling images reproduced the
failure even though earlier 24.04 images used a compatible kernel. Only the two
MongoDB CI jobs are temporarily pinned to **ubuntu-22.04**; the
[official image manifest](https://github.com/actions/runner-images/blob/main/images/ubuntu/Ubuntu2204-Readme.md)
reviewed on 2026-10-27 lists kernel 6.8 and the .NET 10 SDK family. The existing
preflight now logs the actual Docker-server kernel before pulling and checking
the same MongoDB 8 image. Full replica-set tests remain enabled; no skip,
continue-on-error, unsafe Mongo override or automatic retry is added.

This is a temporary compatibility pin, not a guarantee that a runner label never
changes. Review and migrate before the published Ubuntu 22 runner
[retirement on 2027-04-17](https://github.com/actions/runner-images/issues/14254),
preferably once the current MongoDB release and newer runner kernels work
together. Record actual runner/kernel and test results from this PR's CI. This
change does not alter the production MongoDB cluster, application image or ACA.

## Verification boundary

Run the existing source gate, full CI and independent produced-bundle audit.
Verify all previous instances/titles/status outcomes, the twelve additions,
truthful text and cancellation failures, full metadata and collection evidence,
peer cleanup and unchanged contract/flow/policy exports. Check the standalone
TRX lifecycle/readiness output and three new Infrastructure cancellation cases
separately from the Allure BDD count. Local .NET/Docker are unavailable here;
runtime/build/format/security results must come from actual CI. No test, coverage
or security threshold changes. Tasks 10–12, Azure deployment and final LiveDocs
selection/refresh remain deferred.
