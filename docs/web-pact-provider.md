# PACT/10 Web → Users provider verification

The storefront's Load profile flow calls `GET /customers/external/{externalId}`
through the BFF. The Users acceptance suite replays the original
[Web master artifact](https://github.com/MichalBoczula/ecommerce-store-web/actions/runs/42683076959).
The [source pin](../contracts/pact/web-source.json) records the exact Web commit,
artifact, archive hash and contract hash. PactNet verifies both cases through
a loopback Kestrel listener and the real MongoDB repository.

| Interaction | Expected outcome |
| --- | --- |
| Existing external ID | HTTP 200 with the pinned individual and company profile, billing and shipping addresses, null apartment number, and update timestamp. |
| Unknown external ID | HTTP 404 with `application/problem+json` and `status: 404`. |

The provider state handler is attached only to the test host. It seeds synthetic
data in an isolated database in the acceptance replica set using the Users repository.

Run the provider test locally with .NET 10 and Docker available.

```bash
dotnet test tests/ECommerceStoreUsers.AcceptanceTests/ECommerceStoreUsers.AcceptanceTests.csproj \
  --filter FullyQualifiedName~UsersPactProviderTests
```

The required acceptance job uploads `web-users-verification`. After the quality
gate on `master`, the publisher finds the exact Web publication, checks the
registry's bytes and posts the report for the verified Users commit.

| Location | Setting |
| --- | --- |
| Terraform `pact_publishers` | `users-ci = ["users"]` alongside the existing scopes |
| Terraform `pact_publisher_token_sha256` | SHA-256 of the distinct raw Users publisher token, keyed by `users-ci` |
| Users repository Actions variable | `PACT_REGISTRY_URL` set to the deployed registry HTTPS origin |
| Users repository Actions secret | `PACT_PUBLISHER_TOKEN` set to the raw Users publisher token |

The `web-users-verification-publication` receipt links the result to the
original Web publication and Users CI run. Use **Actions → CI → Run workflow**
on `master` after configuring the publisher if needed. Profile edits, favorites
and BFF registration are separate contract slices.
