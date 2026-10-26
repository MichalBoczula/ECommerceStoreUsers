#!/usr/bin/env python3
"""Verify that named HTTP operations and documented acceptance cases stay linked."""

import csv
import json
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
BASELINE = ROOT / "docs/bdd-baseline.json"


def scenarios(feature):
    """Read the English scenario/Examples subset used by this suite, not step tables."""
    result = {}
    current = None
    header = None
    examples = False
    docstring = None
    pending_tags = []
    description = False
    for line in feature.read_text(encoding="utf-8-sig").splitlines():
        stripped = line.strip()
        if stripped.startswith(('"""', '```')):
            delimiter = stripped[:3]
            docstring = None if docstring == delimiter else delimiter
            continue
        if docstring:
            continue
        if stripped.startswith("@"):
            pending_tags.extend(stripped.split())
            continue
        match = re.fullmatch(r"Scenario( Outline)?:\s*(.+)", stripped)
        if match:
            outline, name = match.groups()
            assert name not in result, f"Duplicate scenario title: {feature} {name}"
            current = {"outline": bool(outline), "examples": [], "tags": pending_tags, "description": []}
            result[name] = current
            pending_tags = []
            description = True
            examples, header = False, None
        elif stripped.startswith("Examples:"):
            assert current and current["outline"], f"Examples without outline: {feature}"
            description = False
            examples, header = True, None
        elif examples and stripped.startswith("|"):
            # Gherkin table escapes: \n, \| and \\. Step tables are never read here.
            cells = [re.sub(r"\\([n|\\])", lambda m: "\n" if m[1] == "n" else m[1], cell.strip())
                     for cell in re.split(r"(?<!\\)\|", stripped)[1:-1]]
            if header is None:
                assert cells and all(cells) and len(cells) == len(set(cells)), f"Invalid Examples header: {feature}"
                header = cells
            else:
                assert len(cells) == len(header), f"Invalid Examples row: {feature} {stripped}"
                current["examples"].append(dict(zip(header, cells)))
        elif stripped and not stripped.startswith(("#", "@")):
            examples = False
            if re.match(r"(?:Given|When|Then|And|But|\*)\s", stripped):
                description = False
            elif current and description:
                current["description"].append(stripped)
            elif stripped.startswith("Feature:"):
                pending_tags = []
    assert result, f"No scenarios found: {feature}"
    for name, item in result.items():
        assert not item["outline"] or item["examples"], f"Outline without examples: {feature} {name}"
    return result


def inventory():
    return [{"feature": feature.relative_to(FEATURES).as_posix(), "scenario": name, "example": example}
            for feature in sorted(FEATURES.rglob("*.feature"))
            for name, item in scenarios(feature).items()
            for example in (item["examples"] if item["outline"] else [None])]


def verify_baseline(cases):
    baseline = json.loads(BASELINE.read_text(encoding="utf-8"))
    assert baseline["schemaVersion"] == 1 and baseline["cases"], "Invalid BDD baseline"
    # Task 2 adds identity metadata; retain every original behavioral parameter.
    def key(case):
        example = case["example"]
        if example is not None:
            example = {name: value for name, value in example.items() if name != "scenarioId"}
        return json.dumps({**case, "example": example}, sort_keys=True, ensure_ascii=False)
    original = [key(case) for case in baseline["cases"]]
    current = [key(case) for case in cases]
    assert len(original) == len(set(original)), "Duplicate baseline case"
    assert len(current) == len(set(current)), "Duplicate expanded BDD case"
    missing = set(original) - set(current)
    assert not missing, f"Missing baseline BDD cases (review an explicit migration for renames): {sorted(missing)}"


def scenario_id(item, example=None):
    prefix = "@allure.label.scenarioId:"
    tags = [tag[len(prefix):] for tag in item["tags"] if tag.startswith(prefix)]
    argument = (example or {}).get("scenarioId")
    assert len(tags) + (argument is not None) == 1, "Specify scenarioId once: scenario tag or Examples column"
    value = argument if argument is not None else tags[0]
    assert re.fullmatch(r"REF\d{2}_[A-Za-z0-9_]+", value), f"Invalid scenarioId: {value}"
    return value


def verify_evidence_sources():
    seen = {}
    for feature in sorted(FEATURES.rglob("*.feature")):
        for name, item in scenarios(feature).items():
            assert item["description"], f"Missing readable scenario description: {feature} {name}"
            for example in (item["examples"] if item["outline"] else [None]):
                identity = scenario_id(item, example)
                source = (feature, name)
                assert identity not in seen or seen[identity] == source, f"scenarioId reused by different scenarios: {identity}"
                seen[identity] = source
    # All BDD clients must use ScenarioApiContext. Swagger bootstrap is the only
    # direct client: it obtains the contract before the checked pipeline exists.
    for file in FEATURES.rglob("*.cs"):
        if file.name in {"ScenarioApiContext.cs", "TestHooks.cs"}:
            continue
        code = file.read_text(encoding="utf-8-sig")
        receivers = re.findall(r"\b(\w+)\.Create(?:Default)?Client\s*\(", code)
        assert all(receiver in {"context", "_apiContext"} for receiver in receivers) and not re.search(
            r"new\s+HttpClient\s*\(", code), f"BDD HTTP client bypasses shared evidence: {file}"


def operations():
    result = {}
    for file in sorted(API.glob("*.cs")):
        source = file.read_text(encoding="utf-8-sig")
        group = re.search(r'MapGroup\("([^"]+)"\)', source)
        assert group, f"Missing route group: {file}"
        prefix = group.group(1)
        matches = list(re.finditer(r'group\.Map(Get|Post|Put|Delete|Patch)\("([^"]*)"', source))
        for index, match in enumerate(matches):
            method, path = match.groups()
            # Include metadata after WithName and nested generic response DTOs.
            end = matches[index + 1].start() if index + 1 < len(matches) else len(source)
            section = source[match.start():end]
            names = re.findall(r'\.WithName\("([^"]+)"\)', section)
            assert len(names) == 1, f"Expected one named operation: {method} {prefix + path}"
            name = names[0]
            assert name not in result, f"Duplicate operationId: {name}"
            statuses = set(re.findall(
                r'\.Produces(?:<[^;()]+>)?\s*\(\s*StatusCodes\.Status(\d{3})', section))
            assert statuses, f"No declared response statuses parsed: {name}"
            result[name] = (method.upper(), (prefix + path).rstrip("/") or "/", statuses)
    assert result, "No HTTP operations found"
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
        assert (row["method"], row["path"]) == (method, path), f"Stale method/path: {row['scenarioId']}"
        assert re.fullmatch(r"REF\d{2}_[A-Za-z0-9_]+", row["scenarioId"]), row
        assert row["cause"].strip(), row
        assert row["status"] in actual[name][2], f"Undeclared response status in matrix: {name} {row['status']}"
        feature = FEATURES / row["feature"]
        assert feature.is_file(), feature
        declared = scenarios(feature)
        assert row["scenario"] in declared, (feature, row["scenario"])
        item = declared[row["scenario"]]
        if item["outline"]:
            # Match the referenced outline, never a different scenario's table.
            selector = ({"operationId": name} if any("operationId" in x for x in item["examples"]) else
                        {"area": row["cause"].split()[0]} if any("area" in x for x in item["examples"]) else
                        {"Case": row["cause"]} if any("Case" in x for x in item["examples"]) else {})
            assert selector, f"No matrix example selector for outline: {feature} {row['scenario']}"
            matching = [example for example in item["examples"] if selector.items() <= example.items()]
            assert matching, \
                f"Missing outline example for matrix case: {row['scenarioId']} {selector}"
            assert any(scenario_id(item, example) == row["scenarioId"] for example in matching), \
                f"Matrix/evidence scenarioId mismatch: {row['scenarioId']}"
        else:
            assert scenario_id(item) == row["scenarioId"], f"Matrix/evidence scenarioId mismatch: {row['scenarioId']}"
        statuses_by_operation[name].add(row["status"])
    missing = {name: expected - statuses_by_operation[name]
               for name, (_, _, expected) in actual.items() if expected - statuses_by_operation[name]}
    assert not missing, f"Missing documented response status/operation links: {missing}"
    cases = inventory()
    verify_baseline(cases)
    verify_evidence_sources()
    print(f"Verified {len(actual)} named HTTP operations, "
          f"{sum(len(item[2]) for item in actual.values())} declared operation/status pairs, "
          f"{len(rows)} matrix links and {len(cases)} expanded BDD cases")


if __name__ == "__main__":
    try:
        verify()
    except (AssertionError, KeyError, OSError, ValueError) as error:
        print(f"Acceptance matrix error: {error}", file=sys.stderr)
        sys.exit(1)

