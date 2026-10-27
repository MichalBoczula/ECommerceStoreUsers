#!/usr/bin/env python3
"""Compare generated operation links and acceptance outcomes with exported OpenAPI."""

import argparse
import json
import re
import sys
from pathlib import Path

from importlib.machinery import SourceFileLoader


ROOT = Path(__file__).resolve().parent.parent
links = SourceFileLoader("operation_links", str(Path(__file__).with_name("generate-operation-links.py"))).load_module()
VERBS = {"get", "post", "put", "delete", "patch"}


def normalize(path):
    return re.sub(r"\{([^}:]+):[^}]+}", r"{\1}", path).rstrip("/") or "/"


def resolve(schema, components):
    visited = set()
    while "$ref" in schema:
        ref = schema["$ref"]
        assert ref.startswith("#/components/schemas/"), f"Unexpected schema reference: {ref}"
        assert ref not in visited, f"Recursive top-level schema reference: {ref}"
        visited.add(ref)
        schema = components[ref.rsplit("/", 1)[-1]]
    return schema


def check(document, generated):
    components = document.get("components", {}).get("schemas", {})
    expected_names = [item["operationId"] for item in generated["operations"]]
    assert len(expected_names) == len(set(expected_names)), "Duplicate operationId in generated links"
    actual = {}
    for path, path_item in document["paths"].items():
        for method, operation in path_item.items():
            if method not in VERBS:
                continue
            name = operation.get("operationId")
            if method == "get" and normalize(path) in {"/health", "/health/live", "/health/ready"}:
                continue  # Explicit technical probes; no blanket unknown-operation exclusion.
            assert name in expected_names, f"OpenAPI operation without acceptance links: {method.upper()} {path} ({name})"
            assert name not in actual, f"Duplicate operationId in OpenAPI: {name}"
            actual[name] = (method.upper(), normalize(path), operation)
    assert set(actual) == set(expected_names), \
        f"OpenAPI operation mismatch: missing {set(expected_names) - set(actual)}"
    checked = 0
    for item in generated["operations"]:
        name = item["operationId"]
        method, path, operation = actual[name]
        assert (method, path) == (item["method"], normalize(item["path"])), f"Method/path drift: {name}"
        responses = operation.get("responses", {})
        for case in item["scenarios"]:
            status = str(case["status"])
            assert status in responses, f"Missing OpenAPI status: {name} {status} ({case['scenarioId']})"
            response = responses[status]
            media = response.get("content", {})
            if status == "204":
                assert not media, f"204 must not advertise a body: {name}"
            else:
                expected = "application/problem+json" if case["status"] >= 400 else "application/json"
                assert expected in media, f"Missing {expected}: {name} {status} ({case['scenarioId']})"
                schema = media[expected].get("schema", {})
                assert schema, f"Missing response DTO schema: {name} {status}"
                resolved = resolve(schema, components)
                assert resolved.get("type") or resolved.get("properties") or "$ref" in schema, \
                    f"Unusable response schema: {name} {status}"
                if case["status"] >= 400:
                    props = resolved.get("properties", {})
                    assert {"title", "status", "detail", "type"} <= set(props), \
                        f"Incomplete problem DTO: {name} {status}"
            checked += 1
        uncovered = set(responses) - {str(case["status"]) for case in item["scenarios"]}
        assert not uncovered, f"OpenAPI response without acceptance case: {name} {sorted(uncovered)}"
    return len(actual), checked


def check_descriptions(generated, flow_document, policy_document):
    documentation = generated["documentation"]
    exposed = [flow for group in flow_document["flows"] for flow in group.values()]
    actual = unique_names(exposed, "flowName")
    expected = {flow["flowName"]: flow for flow in documentation["flows"]}
    assert set(actual) == set(expected), "Runtime/source flow inventory differs"
    for name, flow in expected.items():
        steps = [{"order": step["order"], "stepName": step["description"], "bpmnId": step["bpmnId"]} for step in flow["steps"]]
        assert actual[name]["steps"] == steps, f"Runtime/source step semantics differ: {name}"
    policies = unique_names([policy for group in policy_document["validations"] for policy in group.values()], "policyName")
    expected_policies = {policy["name"]: policy for policy in documentation["policies"]}
    rules = {rule["id"]: rule for rule in documentation["rules"]}
    assert set(policies) == set(expected_policies), "Runtime/source policy inventory differs"
    for name, policy in expected_policies.items():
        assert sorted(rule["ruleName"] for rule in policies[name]["rules"]) == sorted(rules[identity]["name"] for identity in policy["ruleIds"]), f"Runtime/source active rules differ: {name}"
    return len(expected), sum(len(flow["steps"]) for flow in expected.values()), len(policies)


def unique_names(items, key):
    result = {}
    for item in items:
        name = item[key]
        assert name not in result, f"Duplicate runtime descriptor: {name}"
        result[name] = item
    return result


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("openapi", type=Path, help="OpenAPI JSON exported from a running application")
    args = parser.parse_args()
    try:
        generated = links.generate()
        result = check(json.loads(args.openapi.read_text(encoding="utf-8")), generated)
        if "documentation" in generated:
            descriptions = check_descriptions(generated,
                json.loads(args.openapi.with_name("flows.json").read_text(encoding="utf-8")),
                json.loads(args.openapi.with_name("validation-policies.json").read_text(encoding="utf-8")))
            print(f"Verified runtime/source semantics: {descriptions[0]} flows, {descriptions[1]} steps, {descriptions[2]} policies")
        print(f"Verified {result[0]} generated OpenAPI operations and {result[1]} acceptance status/cause cases")
    except (AssertionError, KeyError, OSError, ValueError) as error:
        print(f"OpenAPI contract drift: {error}", file=sys.stderr)
        sys.exit(1)

