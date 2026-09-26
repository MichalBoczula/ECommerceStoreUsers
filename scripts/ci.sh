#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."
command="${1:?Usage: scripts/ci.sh source|build|format|contract|test SUITE}"
shift
results_dir="${VERIFY_RESULTS_DIR:-$PWD/artifacts/verification}"

case "$command" in
  source)
    python3 scripts/check-acceptance-matrix.py
    python3 scripts/check-description-links.py
    python3 scripts/test-architecture.py
    python3 scripts/check-architecture.py
    mkdir -p "$results_dir"
    python3 scripts/generate-operation-links.py --output "$results_dir/operation-links.json"
    python3 scripts/test-operation-links.py
    python3 scripts/test-openapi-contract.py
    ;;
  build)
    dotnet restore ECommerceStoreUsers.slnx
    dotnet build ECommerceStoreUsers.slnx --configuration Release --no-restore
    ;;
  format)
    dotnet format ECommerceStoreUsers.slnx --verify-no-changes --no-restore
    ;;
  contract)
    bash scripts/validate-openapi.sh "$results_dir/openapi.json"
    ;;
  test)
    suite="${1:?Provide a test suite}"
    test_args=()
    assembly=""
    threshold=""
    case "$suite" in
      domain)
        project=tests/ECommerceStoreUsers.Domain.UnitTests/ECommerceStoreUsers.Domain.UnitTests.csproj
        assembly=ECommerceStoreUsers.Domain
        threshold=70
        ;;
      application)
        project=tests/ECommerceStoreUsers.Application.UnitTests/ECommerceStoreUsers.Application.UnitTests.csproj
        assembly=ECommerceStoreUsers.Application
        threshold=70
        ;;
      infrastructure)
        project=tests/ECommerceStoreUsers.Infrastructure.UnitTests/ECommerceStoreUsers.Infrastructure.UnitTests.csproj
        assembly=ECommerceStoreUsers.Infrastructure
        ;;
      acceptance)
        project=tests/ECommerceStoreUsers.AcceptanceTests/ECommerceStoreUsers.AcceptanceTests.csproj
        ;;
      *)
        echo "Unknown test suite: $suite" >&2
        exit 2
        ;;
    esac
    if [[ -n "$assembly" ]]; then
      test_args=(--collect 'XPlat Code Coverage' --
        'DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura'
        "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include=[$assembly]*")
    fi

    mkdir -p "$results_dir"
    rm -rf "$results_dir/$suite" "$results_dir/$suite-coverage"
    summary_file="${VERIFY_SUMMARY_FILE:-$results_dir/summary.md}"
    dotnet restore ECommerceStoreUsers.slnx
    test_status=0
    dotnet test "$project" --configuration Release --no-restore \
      --logger "trx;LogFileName=$suite.trx" --results-directory "$results_dir/$suite" \
      "${test_args[@]}" || test_status=$?
    summary_status=0
    python3 scripts/summarize-trx.py "$suite" "$results_dir/$suite" "$summary_file" || summary_status=$?
    if (( test_status != 0 )); then exit "$test_status"; fi
    if (( summary_status != 0 )); then exit "$summary_status"; fi

    if [[ -n "$assembly" ]]; then
      bash scripts/report-coverage.sh "$results_dir/$suite" "$results_dir/$suite-coverage" \
        "$assembly" "$threshold" "$summary_file"
    fi
    ;;
  *)
    echo "Unknown verification command: $command" >&2
    exit 2
    ;;
esac
