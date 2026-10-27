# USERS-BDD-10 — Generated request and response contracts

Task 10 keeps the existing wire behavior and strengthens its documentation and
verification. Inventory: **278 BDD cases, 122 declarations in 47 features,
249 operation links and 339 acceptance-project cases**. The latter is **278 BDD
+ 61 standalone**: the previous 25, 34 schema-validator cases and two exchange
pipeline cases. Domain/Application/Infrastructure membership stays 123/54/101.
No new package, handwritten specification, generated client or committed script.

## One generated contract

CLR DTOs, System.Text.Json required-member metadata and nullable annotations
remain the source of field shapes. XML comments beside all 23 exported schema
types and their fields supply descriptions and typed examples. API, Application
and Domain generate XML documentation; Swagger reads those assembly XML files.
Only the newly enabled missing-XML-comment warning CS1591 is suppressed; test,
coverage, vulnerability and other compiler settings are unchanged. Reference
properties use allOf so their descriptions/examples can accompany the reference.

The API schema filter supplies two guarantees which cannot be inferred from
the existing CLR declarations alone:

- Request objects permit unknown members, matching System.Text.Json's existing
  ignore behavior. Member matching remains case-insensitive. No serializer
  setting or binding rule changes.
- The existing problem writer always supplies all nine fields. The generated
  problem therefore requires them and marks the inherited fields non-null.
  Its extension dictionary remains open. Status is an int32; errors and
  missingProperties remain arrays even when empty.

CreateCustomer's Companies annotation now explicitly permits null, matching
the existing mapping's `request.Companies ?? []`. This is a source annotation,
not a new runtime normalization. ApartmentNumber remains required and nullable:
the member must occur, while JSON null represents no apartment. Ordinary required
string/object inputs still reach the existing binding/mapping/business errors;
schema annotations do not claim the serializer alone enforces non-null strings.
GUIDs and UTC timestamps retain their generated uuid/date-time formats. Email
and phone stay strings with descriptions of the actual business rules; no wider
standard or new regex/length validation is silently promised. Route parameters
also receive descriptions and typed examples without new binding rules.

The read-only export and a new BDD documentation case compare every generated
field with serializer-required metadata, CLR scalar/container/reference types
and nullable annotations. They check all field descriptions and validate each
typed example, including nested objects, arrays and descriptor dictionaries.
The explicit problem-writer guarantee is the one documented required/nullability
override. This derives checks from source instead of maintaining a second JSON
schema inventory. XML documentation must accompany assemblies in build/publish
output; an absent file or incomplete export fails Swagger/export verification.

## Actual HTTP evidence

The shared checker now validates scalars, required member presence, nullability,
UUID/date-time/int formats, nested objects, every array item and dictionary
value, references/allOf, declared enum choices and additional properties.
Unknown composition/formats and excessive nesting fail explicitly rather than
silently skipping children. This is a checker for this service's emitted
OpenAPI 3.0 vocabulary, not a general JSON Schema implementation. New schema
keywords require extending its supported checks before claiming coverage.

For named operations, the checker validates actual route values and request
bodies **after the HTTP response**, allowing invalid inputs to reach the API.
A successful exchange must have a conforming request and response. For rejected
inputs, request violations are evidence, not a reason to prevent the request or
fail the scenario: business-invalid text may also conform to the structural
schema. Every returned response must conform. A 204 stays bodyless. Framework
404/405/415 use the full generated problem schema. Health text and Swagger's
document stay explicit technical paths rather than business DTOs. Cancelled
requests without a response retain task 9's request/failure evidence.

`OpenAPI exchange contract` attachments carry the actual exchangeId, operation,
method/path/status, parameter/body check scope and request/response findings.
They link directly to the shared HTTP request/response attachments and the
same bundle's `sources/openapi.json`. Framework routing/media problems receive
their own contract observation. Capture still runs inside validation, so a
failed contract assertion cannot erase the actual HTTP bytes. The checker does
not invent a response or replace current/history assertions.

## New BDD evidence

- One populated, read-only documentation case checks explicit Gherkin field
  expectations and all source metadata/examples; all five collections remain
  identical.
- Four Customer creations cover omitted/null/empty companies and uppercase
  member names with ignored root/nested members. Gherkin supplies the base JSON
  and complete response expectations. Each verifies the full response, real
  version-zero current/Insert-history profiles, generated fields/timestamps,
  and all five collection snapshots. Companies return as [] and both nullable
  apartment fields remain present and null.
- One populated rejection case omits only apartmentNumber and checks the
  complete 400 invalid_json problem, exact missing-member name, raw request
  and full preservation. It distinguishes required presence from nullable value.

The original 272 BDD identities/titles/outcomes and 244 matrix links must remain.
Only five primary CreateCustomer links are added; the metadata read is a
technical Swagger call. Existing business/framework failures retain their
status/cause assertions and full persistence evidence.

## Verification and remaining scope

Run the existing source gate and full CI. Independently inspect the produced
bundle and TRX: manifest provenance/hashes, all prior instances, six additions,
typed metadata/examples, real exchange-to-contract links, raw bodies, new
current/history profiles and populated failure/read preservation. Report the
standalone schema and pipeline checks separately from BDD. No local .NET/Docker
are available here, so runtime/build/format/security results come from actual CI.

No endpoint, business-rule, persistence or flow/policy JSON change. Richer
operation/flow/rule/branch/RAG links and stale guidance belong to task 11.
Final rendered report audit/selection and LiveDocs refresh belong to task 12;
Azure deployment stays separate. Task 9's temporary Mongo runner pin remains
subject to its documented retirement review.
