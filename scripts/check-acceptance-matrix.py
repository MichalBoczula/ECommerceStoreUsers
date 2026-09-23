#!/usr/bin/env python3
"""Verify that named HTTP operations and documented acceptance cases stay linked."""

import csv
import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
API = next((ROOT / "src").glob("*.Api/Endpoints"), None)
if API is None:
    API = next((ROOT / "src").glob("*.API/Endpoints"))
FEATURES = next((ROOT / "tests").glob("*.Acceptance.Tests/Features"), None)
if FEATURES is None:
    FEATURES = next((ROOT / "tests").glob("*.AcceptanceTests/Features"))
MATRIX = ROOT / "docs/acceptance-matrix.tsv"


def operations():
    result = {}
    for file in API.glob("*.cs"):
        source = file.read_text(encoding="utf-8-sig")
        prefix = re.search(r'MapGroup\("([^"]+)"\)', source).group(1)
        pattern = r'group\.Map(Get|Post|Put|Delete)\("([^"]*)"[\s\S]*?\.WithName\("([^"]+)"\)'
        for match in re.finditer(pattern, source):
            method, path, name = match.groups()
            section = match.group(0)
            statuses = set(re.findall(r'\.Produces(?:<[^>]+>)?\(StatusCodes\.Status(\d{3})', section))
            result[name] = (method.upper(), (prefix + path).rstrip("/") or "/", statuses)
    return result


def verify():
    actual = operations()
    with MATRIX.open(newline="", encoding="utf-8") as stream:
        rows = list(csv.DictReader(stream, delimiter="\t"))
    columns = {"scenarioId", "operationId", "method", "path", "status", "cause", "feature", "scenario"}
    assert rows and columns == set(rows[0]), "Unexpected matrix columns"
    assert {row["operationId"] for row in rows} == set(actual), "Missing or stale operationId in matrix"
    ids = [row["scenarioId"] for row in rows]
    assert len(ids) == len(set(ids)), "Duplicate scenarioId"
    statuses_by_operation = {name: set() for name in actual}
    for row in rows:
        name = row["operationId"]
        method, path, _ = actual[name]
        assert (row["method"], row["path"]) == (method, path), row
        assert re.fullmatch(r"REF07_[A-Za-z0-9_]+", row["scenarioId"]), row
        assert row["cause"].strip(), row
        assert row["status"] in {"200", "201", "204", "400", "404", "409", "500"}, row
        feature = FEATURES / row["feature"]
        assert feature.is_file(), feature
        text = feature.read_text(encoding="utf-8-sig")
        scenarios = re.findall(r"^\s*Scenario(?: Outline)?:\s*(.+?)\s*$", text, re.MULTILINE)
        assert row["scenario"] in scenarios, (feature, row["scenario"])
        if "<operationId>" in row["scenario"]:
            assert re.search(r"\|\s*" + re.escape(name) + r"\s*\|", text), row
        if "<area>" in row["scenario"]:
            assert re.search(r"\|\s*" + re.escape(row["cause"].split()[0]) + r"\s*\|", text), row
        if "<Case>" in text and row["scenario"] == "Framework failures have the same JSON problem contract":
            assert re.search(r"\|\s*" + re.escape(row["cause"]) + r"\s*\|", text), row
        statuses_by_operation[name].add(row["status"])
    missing = {name: expected - statuses_by_operation[name]
               for name, (_, _, expected) in actual.items() if expected - statuses_by_operation[name]}
    assert not missing, f"Missing documented response status/operation links: {missing}"
    print(f"Verified {len(actual)} named HTTP operations and {len(rows)} acceptance cases")


if __name__ == "__main__":
    try:
        verify()
    except (AssertionError, OSError, ValueError) as error:
        print(f"Acceptance matrix error: {error}", file=sys.stderr)
        sys.exit(1)
