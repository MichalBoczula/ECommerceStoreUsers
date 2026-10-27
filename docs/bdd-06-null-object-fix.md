# USERS-BDD-06-FIX: Required Individual and Address objects

The three task-6 null-object characterizations now return **400
validation_failed**, replacing their previous safe internal_error 500. An
explicit request-shape guard rejects the missing object before mapping can
dereference it. The existing ValidationException handler supplies the complete
ApiProblemDetails response; unexpected exceptions still use the normal 500 path.

## Public error contract

Every failure has code `validation_failed`, title `Validation failed.`, detail
`One or more validation errors occurred.`, the normal 400 RFC type, request
instance and generated traceId, `missingProperties: []`, and one exact error:

| Missing object | Error name | Entity | Message |
| --- | --- | --- | --- |
| Individual | RequiredRequestObject | IndividualData | individual is required. |
| Billing address | RequiredRequestObject | Address | billingAddress is required. |
| Shipping address | RequiredRequestObject | Address | shippingAddress is required. |

These are **application request-shape guards**, not new registered Domain
policies. The six Domain policies and their twenty published signatures stay
unchanged. The four affected operations' generated OpenAPI descriptions publish
these additional guard names, entities and messages. No separate endpoint,
registry, policy layer or handwritten OpenAPI file is introduced.

The guard returns the first missing object in mapping order. With two null
addresses, billingAddress is reported first. Normal field-level validation
still runs in the existing Domain policies once the required objects exist.
Omitted required JSON members, malformed bodies and null root bodies keep their
framework-error behavior. Nullable apartment numbers retain their behavior.

## Flow coverage

| Operation | Existing mapping step | Required objects covered |
| --- | --- | --- |
| CreateCustomer | MapRequestToDomain | Individual, both individual addresses, both company addresses |
| UpdateIndividualData | MapRequestToIndividualData | Individual and both addresses |
| AddCompany | MapAddress | Billing and shipping addresses |
| UpdateCompany | MapAddress | Billing and shipping addresses |

The existing flow IDs, step IDs and step order remain. Checks run inside their
mapping steps; route-ID and existing-Customer checks retain their precedence.
A null address in the second submitted company is tested after the first company
has already been mapped in memory, proving no partial aggregate is persisted.

The original three scenario IDs are retained while their titles/expectations
and matrix statuses change to the corrected 400 contract. They were added in
task 6, so the fixed original 81-case baseline needs no migration. Ten additional
cases cover the shared mapping paths, both address roles and the first-error
behavior when both addresses are null. There are **13 dedicated null-object
cases**: 12 operation/object combinations and one both-addresses case.

All use the existing real-HTTP fixture, exact request/response evidence, literal
Gherkin error tables and complete canonical Extended JSON before/after snapshots
for customers, customers-history, admins, admins-history and favorites. Current
version 1 and both history versions must remain unchanged, including every ID,
date and profile field. Counts alone cannot satisfy the check. Three focused
Application mapping tests also verify the complete required-object error.

## Verification and boundary

Current inventory: **244 BDD cases, 111 declarations in 44 feature files,
227 named-operation links and 265 acceptance-project cases** (244 BDD plus the
existing 21 standalone). The Application suite adds three cases; other test
suite membership is retained.

Run the existing source gate, full .NET 10 CI and independent produced-bundle
audit. Verify all previous case identities, the deliberate three 500-to-400
changes, all thirteen guard cases, exact error contracts, unchanged stored data
and the four OpenAPI description changes. Flow/policy exports and OpenAPI schemas
must stay unchanged. No test/security/coverage gate is weakened.

This fix covers required Individual and Address objects on the four listed
writes; it does not claim exhaustive validation of every possible JSON graph.
No storage schema/index, dependency, global serializer option or exception
fallback changes. Azure rollout and final LiveDocs selection remain deferred.
