#!/usr/bin/env bash
# Run from any directory: bash scripts/verify.sh (from the repository root).
set -euo pipefail

cd "$(dirname "$0")/.."
python3 scripts/check-acceptance-matrix.py
python3 scripts/check-description-links.py
python3 scripts/test-architecture.py
python3 scripts/check-architecture.py
results_dir="$PWD/artifacts/verification"
mkdir -p "$results_dir"
python3 scripts/generate-operation-links.py --output "$results_dir/operation-links.json"
python3 scripts/test-operation-links.py

dotnet restore ECommerceStoreUsers.slnx
dotnet build ECommerceStoreUsers.slnx --configuration Release --no-restore
dotnet format ECommerceStoreUsers.slnx --verify-no-changes --no-restore

verify_coverage() {
  local suite="$1" assembly="$2" minimum="${3:-}" summary="$results_dir/$1-coverage/Summary.txt" percent
  local tool="$results_dir/tools/reportgenerator"
  if [[ ! -x "$tool" ]]; then
    dotnet tool install dotnet-reportgenerator-globaltool --tool-path "$results_dir/tools" --version 5.4.7
  fi
  "$tool" -reports:"$results_dir/$suite/**/coverage.cobertura.xml" \
    -targetdir:"$results_dir/$suite-coverage" '-reporttypes:Html;TextSummary' \
    -assemblyfilters:"+$assembly"
  percent="$(sed -n 's/^[[:space:]]*Line coverage:[[:space:]]*\([0-9.]*\)%.*/\1/p' "$summary" | head -n 1)"
  [[ "$percent" =~ ^[0-9]+([.][0-9]+)?$ ]] || { echo "Missing $suite coverage" >&2; exit 1; }
  echo "$suite line coverage: $percent%"
  if [[ -n "$minimum" ]]; then
    awk -v value="$percent" -v required="$minimum" 'BEGIN { exit !(value + 0 >= required + 0) }' || {
      echo "$suite coverage $percent% is below $minimum%" >&2
      exit 1
    }
  fi
}

dotnet test tests/ECommerceStoreUsers.Domain.UnitTests/ECommerceStoreUsers.Domain.UnitTests.csproj \
  --configuration Release --no-restore --logger 'trx;LogFileName=domain.trx' \
  --results-directory "$results_dir/domain" --collect:'XPlat Code Coverage' -- \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura' \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=[ECommerceStoreUsers.Domain]*'
verify_coverage domain ECommerceStoreUsers.Domain 70

dotnet test tests/ECommerceStoreUsers.Application.UnitTests/ECommerceStoreUsers.Application.UnitTests.csproj \
  --configuration Release --no-restore --logger 'trx;LogFileName=application.trx' \
  --results-directory "$results_dir/application" --collect:'XPlat Code Coverage' -- \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura' \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=[ECommerceStoreUsers.Application]*'
verify_coverage application ECommerceStoreUsers.Application 70

dotnet test tests/ECommerceStoreUsers.Infrastructure.UnitTests/ECommerceStoreUsers.Infrastructure.UnitTests.csproj \
  --configuration Release --no-restore --logger 'trx;LogFileName=infrastructure.trx' \
  --results-directory "$results_dir/infrastructure" --collect:'XPlat Code Coverage' -- \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura' \
  'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=[ECommerceStoreUsers.Infrastructure]*'
verify_coverage infrastructure ECommerceStoreUsers.Infrastructure
dotnet test tests/ECommerceStoreUsers.AcceptanceTests/ECommerceStoreUsers.AcceptanceTests.csproj \
  --configuration Release --no-restore --logger 'trx;LogFileName=acceptance.trx' \
  --results-directory "$results_dir/acceptance"

bash scripts/validate-openapi.sh "$results_dir/openapi.json"

docker build -f src/ECommerceStoreUsers.API/Dockerfile -t ecommerce-store-users:verify .
echo 'Local verification passed. CI also runs dependency, secret and image vulnerability checks.'
