# PACT/10 BFF → Users provider verification

The BFF looks up an external ID before creating a customer during registration.
The [source pin](../contracts/pact/bff-source.json) records the BFF master
commit, Actions artifact, archive checksum and Pact checksum. Users CI
replays the original Pact through Kestrel and MongoDB.

| Interaction | Expected outcome |
| --- | --- |
| Look up an existing external ID | HTTP 200 with the individual profile and company, including billing and shipping addresses. |
| Look up an unknown external ID | HTTP 404 with a problem response. |
| Create a new customer | HTTP 200 with the saved profile and an empty company list. |
| Create a duplicate customer | HTTP 409 with a problem response. |

The test host prepares each registration state from the Pact's synthetic
request data. The missing state removes a customer and its history if an earlier
interaction created it. Each Pact runs in a disposable acceptance database.

Run the focused provider test with .NET 10 and Docker.

```bash
dotnet test tests/ECommerceStoreUsers.AcceptanceTests/ECommerceStoreUsers.AcceptanceTests.csproj \
  --filter FullyQualifiedName~VerifyPinnedBffPactAgainstUsersApi
```

The acceptance job uploads `bff-users-verification`. After the quality gate on
`master`, the Users publisher checks the BFF publication and the registry's
Pact checksum before publishing the result. It uses the existing `users-ci`
credentials documented in [Web → Users verification](web-pact-provider.md).
The `bff-users-verification-publication` receipt links the four-interaction
result to the BFF publication and the Users CI run.
