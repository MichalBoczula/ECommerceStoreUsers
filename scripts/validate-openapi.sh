#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
output="${1:-artifacts/verification/openapi.json}"
mkdir -p "$(dirname "$output")"
output="$(cd "$(dirname "$output")" && pwd)/$(basename "$output")"
rm -f "$output"

OPENAPI_EXPORT_PATH="$output" dotnet test \
  tests/ECommerceStoreUsers.AcceptanceTests/ECommerceStoreUsers.AcceptanceTests.csproj \
  --configuration Release --no-build --no-restore \
  --filter 'FullyQualifiedName~OpenApiExportTests'

test -s "$output"
npx --yes @redocly/cli@2.53.3 lint "$output" --extends=spec
