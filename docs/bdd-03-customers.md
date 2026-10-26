# USERS-BDD-03: Customer success scenarios

The five original Customer success cases retain their scenario IDs, titles,
request values and HTTP outcomes. Three additional cases exercise preservation
of existing companies and nullable apartment values. All original 81 BDD cases
remain in the fixed baseline; the suite now has 84 BDD cases, 64 declarations
in 42 features, and 74 matrix links covering the same 54 operation/status pairs.
The acceptance-project total is 103, including 19 standalone cases.

## Coverage

| Operation | Response and database evidence |
| --- | --- |
| CreateCustomer | All individual/company/address values, generated IDs, UTC UpdatedAt, one current document at version 0 and one Insert history snapshot. |
| GetCustomerByExternalId | Complete profile and identity, retained timestamp/version, unchanged current and history documents. |
| UpdateIndividualData | Complete revised individual/address values, retained Customer identity, version 0→1 and one Update snapshot; an additional case retains a company and round-trips null apartments. |
| AddCompany | Full new company and unchanged individual data, a new company ID, version 0→1 and one Update snapshot; an additional case retains a previously existing company's complete data and ID. |
| UpdateCompany | Full selected-company update with its ID retained, unchanged individual data, version 0→1 and one Update snapshot; an additional case retains a second company's complete data and ID. |

History contains **resulting snapshots**: Insert version 0 describes the created
profile; Update version 1 describes the resulting profile. The original history
document remains unchanged, including its ID, ChangedAt, action and all profile
data. Every current/history comparison includes external identity, individual,
addresses, companies and timestamps; history checks also include CustomerId,
history ID, action and version.

## Gherkin and evidence

The five success step-definition files now share the small, scenario-local
`CustomerSuccessState` fixture/assertion helper. It requires each expected
profile field instead of falling back to the received value when a table omits
a field. All tables use the same dotted paths (`Individual.BillingAddress.City`,
`Company.TaxId`, and `Company2.*` for a second company). `<null>` explicitly
represents a nullable apartment value. Companies are matched by tax identifier
for profile assertions, without adding an HTTP ordering promise.

Each success case has an explicit persistence table: current/history counts
before and after, versions, history actions, and timestamp behavior. Setup
creates are checked against the entire fixture and their stored Insert
snapshot before the operation under test. The request is then surrounded by
fresh reads of the real isolated MongoDB database. GET compares the complete
before/after projections and leaves history unchanged.

Task 2's canonical request/response capture remains in use. Additional JSON
evidence carries the same service, scenario and instance identities:

- Customer setup request/database, where applicable;
- Customer database before/after, containing complete known document fields;
- Customer response and persistence expectations;
- Customer persistence verified, emitted only after the assertions pass.

Snapshots are complete deserialized document projections rather than raw BSON
bytes. They show UTC dates, GUIDs and readable Insert/Update action names.
They are queued before assertions so a failed check retains the observed state.

## Timestamp behavior

BSON dates have millisecond precision. Create and individual/company-add HTTP
responses can retain finer .NET ticks, so comparisons with MongoDB normalize
only the response timestamp to milliseconds. Newly generated UpdatedAt and
ChangedAt values must fall within the measured HTTP request window.

The existing company-update implementation changes the embedded company,
increments the Customer version and appends history while retaining the
Customer's UpdatedAt. These tests state and verify that behavior. GET also
retains UpdatedAt. No domain timestamp behavior is changed by this task.

## Verification and scope

Run the existing source stage and acceptance suite:

```bash
bash scripts/ci.sh source
bash scripts/ci.sh test acceptance
```

The PR records full CI and inspection of the actual report bundle, including
all eight success cases, complete JSON snapshots and preserved original case
identities. The local workspace has no .NET SDK or Docker daemon; local checks
cover source links, inventory, architecture and whitespace. Runtime checks run
in the existing CI.

No API, DTO, policy, repository, database schema, dependency, CI job or Azure
resource changes. Validation/framework errors, further races/rollback coverage,
and stronger schema/flow/rule checks remain in their planned tasks. The
LiveDocs report selection and Azure rollout remain separate until task 12.
