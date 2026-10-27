# USERS-BDD-06: Business validation and complete rejection evidence

This follow-up completes the task skipped when task 7 was requested directly.
It retains all 129 previous BDD cases and adds 105: 83 isolated JSON-value
failures, 11 identifier cases, two multi-error cases, three company-conflict
causes, three null-object characterizations and three accepted boundaries.
The current inventory is **234 expanded BDD cases, 101 declarations in 44
feature files, 217 named-operation links and 255 acceptance-project cases**
(234 BDD plus the existing 21 standalone cases). The fixed 81-case baseline
and all previous identities remain unchanged.

## Existing policy paths

| Registered policy | Covered behavior |
| --- | --- |
| CustomerValidationPolicy | External ID, individual/contact fields, both individual addresses, company name/tax ID, both company addresses and duplicate tax-ID collection policy |
| IndividualDataValidationPolicy | Individual update: each name/contact field and each field in both addresses |
| CompanyValidationPolicy | Add and update: company name/tax ID and each field in both addresses |
| AdminValidationPolicy | Create/update name and email, external ID create/lookup; retained external ID on update |
| FavoriteValidationPolicy | Client/product identifiers independently and together |
| EmptyGuidValidationPolicy | Customer/Admin/company route IDs and Favorite list/remove/clear, including two identical errors for two empty identifiers |

There are **19 used rule classes with 20 published error signatures**; Customer
ExternalId publishes two distinct errors. The following is a snapshot of their
literal source descriptors and representative HTTP evidence, not a replacement
policy implementation. `Common/BusinessValidation.feature` contains the complete
operation/pointer/value/expected-error examples; `acceptance-matrix.tsv` links
every new case to the actual named operation and declared status.

| Rule | Entity | Published error signature | HTTP behavior and representative scenario ID |
| --- | --- | --- | --- |
| `AddressApartmentNumberValidationRule` | Address | Apartment Number cannot be empty or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_Individual_billingAddress_apartmentNumber` |
| `AddressBuildingNumberValidationRule` | Address | Building Number cannot be empty or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_Individual_billingAddress_buildingNumber` |
| `AddressCityValidationRule` | Address | City cannot be empty or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_Individual_billingAddress_city` |
| `AddressPostalCodeValidationRule` | Address | Postal Code cannot be empty or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_Individual_billingAddress_postalCode` |
| `AddressStreetValidationRule` | Address | Street cannot be empty or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_Individual_billingAddress_street` |
| `AdminEmailValidationRule` | Admin | Email must be a valid format (address@domain.something). | `validation_failed` 400; `REF06_Validation_CreateAdmin_Email` |
| `AdminExternalIdValidationRule` | Admin | ExternalId cannot be null or white space. | `validation_failed` 400; `REF06_Validation_CreateAdmin_ExternalBlank` |
| `AdminFullNameValidationRule` | Admin | Full Name cannot be null, white space, or contain special characters (@#$%^&*). | `validation_failed` 400; `REF06_Validation_CreateAdmin_FullName` |
| `CompanyDataCompanyNameValidationRule` | CompanyData | Company Name cannot be empty or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_CompanyName` |
| `CompanyDataTaxIdUniquenessValidationRule` | CompanyData | Customer already contains a company with the same Tax Id. | policy error mapped to `resource_conflict` 409; `REF06_Validation_CreateCustomer_DuplicateTax` |
| `CompanyDataTaxIdValidationRule` | CompanyData | Tax Id must be a valid Polish NIP containing exactly 10 digits. | `validation_failed` 400; `REF06_Validation_CreateCustomer_TaxId` |
| `CustomerExternalIdValidationRule` | Customer | ExternalId cannot be null or white space. | `validation_failed` 400; `REF06_Validation_CreateCustomer_ExternalBlank` |
| `CustomerExternalIdValidationRule` | Customer | ExternalId cannot be an empty guid. | `validation_failed` 400; `REF06_Validation_CreateCustomer_ExternalEmptyGuid` |
| `EmptyGuidRule` | Guid | Guid cannot be empty. | `validation_failed` 400; `REF06_Validation_UpdateIndividualData_EmptyCustomerId` |
| `FavoriteClientIdValidationRule` | Favorite | ClientId cannot be an empty GUID. | `validation_failed` 400; `REF06_Validation_AddProductToFavorites_EmptyClientId` |
| `FavoriteProductIdValidationRule` | Favorite | ProductId cannot be an empty GUID. | `validation_failed` 400; `REF06_Validation_AddProductToFavorites_EmptyProduct` |
| `IndividualDataEmailValidationRule` | IndividualData | Email must be a valid format (address@domain.something). | `validation_failed` 400; `REF06_Validation_CreateCustomer_email` |
| `IndividualDataFirstNameValidationRule` | IndividualData | First Name cannot be empty or white space and must contain at least one letter. | `validation_failed` 400; `REF06_Validation_CreateCustomer_firstName` |
| `IndividualDataLastNameValidationRule` | IndividualData | Last Name cannot be empty or white space and must contain at least one letter. | `validation_failed` 400; `REF06_Validation_CreateCustomer_lastName` |
| `IndividualDataPhoneValidationRule` | IndividualData | Phone must contain only digits and be between 7 and 10 characters long. | `validation_failed` 400; `REF06_Validation_CreateCustomer_phone` |

The older `CustomerCompanyTaxIdValidationRule` is not constructed by any
registered policy. It remains inactive; its existing class/unit tests and
legacy error-name compatibility checks do not establish current enforcement.
No rule, registration, descriptor or production flow is activated or changed.

## Requests and complete responses

The new Gherkin background supplies full valid Customer, Admin and Favorite
JSON payloads. A small scenario-local helper selects the existing endpoint body
shape from these templates: Customer root, individual wrapper, company object,
Admin root/profile fields, Favorite object or no body for queries/deletes.
Overrides use existing JSON member/array paths and explicit JSON values. A
missing path fails setup instead of creating a request field. Quoted whitespace,
empty strings and explicit nulls remain distinct. The full resulting JSON and
exact UTF-8 bytes are captured by the same shared checked HTTP client as before.
Expected errors are literal Gherkin data; they are not read from the response or
from the production policy at test runtime.

Each isolated case has one intentional invalid value. Both addresses are tested
separately on Customer create, individual update, company add and company
update. Selected null string cases, too-short/too-long/non-digit phones and
short/long/non-digit tax IDs cover representative causes without adding new
business restrictions. The multi-error Customer case checks the entire ordered
error array, including both identical City errors; Admin checks all three
invalid fields together. No deduplication or name-only membership check can pass.

All strengthened rejections assert the complete nine-field problem: status,
RFC type, fixed title/detail, decoded request-path instance, code, exact
name/message/entity errors with multiplicity, missingProperties and generated
traceId. Extra fields and sensitive diagnostics fail. For encoded external-ID
whitespace, the expected instance uses ASP.NET PathString decoding, while the
HTTP envelope retains the encoded request target. Method/media checks and the
task-7 UUID route/status guard stay intact.

The 18 original ordinary validation/conflict cases gain full errors/problems
and all-five-collection preservation assertions. Their original titles, IDs,
inputs and assertions remain. The original Customer and Favorite duplicate
flows now arrange the successful first add in Given, then snapshot before the
repeated request in When; the recorded baseline therefore includes the existing
resource instead of treating setup as a failed-request write.

## Duplicate tax IDs: three real causes

- Customer create: the collection uniqueness policy finds two submitted
  companies with one tax ID; CreateCustomer maps its error to 409.
- Company add: the new valid company's tax ID matches a stored company; the
  Customer collection policy finds the duplicate and AddCompany maps it to 409.
- Company update: a second company exists, and the update descriptor's explicit
  uniqueness guard rejects reusing its tax ID before company-policy validation.

All three responses are complete `resource_conflict` problems with `errors: []`;
the published collection rule is not exposed as a `validation_failed` 400.
The update case adds its second company by actual HTTP first, so its no-write
baseline has Customer version 2 and three history entries. This deliberate
setup differs from the normal shared version-1 fixture and is recorded.

## Accepted boundaries and existing limits

Two new Customer successes use the existing complete profile/current/history
bindings. They verify phone lengths 7 and 10, Unicode-containing names, a
nonempty postal code without a postal-format check, and a ten-digit all-zero
NIP. Current tax validation checks the regex length/digits, not NIP checksum.
Individual names require at least one Unicode letter; they do not require an
all-letter string. Existing null-apartment success/HTTP fixture evidence remains.

The new Admin success proves that an empty-GUID *string* ExternalId and numeric
FullName are currently accepted. Admin ExternalId rejects only null/whitespace;
FullName rejects blanks and the listed special characters. Customer's empty-GUID
ExternalId restriction does not apply to Admin. All success fields, generated
IDs/dates and Insert/current/version-0 history are checked with the established
success helpers.

Three explicit-null object cases characterize existing gaps: null Individual,
null individual BillingAddress and null company BillingAddress currently fail
mapping and return safe `internal_error` 500. They are not framework rejections
or successful policy-validation 400s. They must leave every collection unchanged.
This task documents that behavior without changing contracts or adding null
object rules. It does not claim every null-reference combination is covered.

## Persistence, verification and publication

The task-7 rejection helper is reused with Business validation evidence names,
complete problem support for 409/500 and ASP.NET path-instance decoding. The
original Framework evidence names and checks remain the default. There is no
second database snapshot implementation or new CI/Python tooling.

New rejection cases use actual HTTP setup: Customer/company and Admin at current
version 1 with history versions 0/1, plus a Favorite. Every stored field in
customers, customers-history, admins, admins-history and favorites is compared
before/after as canonical Extended JSON, including BSON IDs, versions, dates
and arrays. Counts alone are insufficient. Original cases keep their own setup.
This proves unchanged observable data, not transaction rollback or repository
invocation counts; concurrency/atomicity remains task 8. Favorites have no history.

Local checks are `bash scripts/ci.sh source` and `git diff --check`. The workspace
has no .NET SDK/Docker daemon; existing full PR CI supplies runtime/build/format,
OpenAPI/lint, all suites, separate 70% layer coverage, security/image and bundle
gates. Before merge, the PR records actual CI results and an independent audit
of case identities, all attachments/manifest hashes, literal error signatures,
exact request changes, complete responses and unchanged database snapshots.

No production API/DTO/policy/repository/index/package or CI-gate change is made.
No Azure apply, ACA rollout or selected LiveDocs snapshot refresh belongs here;
LiveDocs selection remains task 12. Existing master image/archive CI continues.
