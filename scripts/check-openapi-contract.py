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
    actual = {}
    for path, path_item in document["paths"].items():
        for method, operation in path_item.items():
            if method not in VERBS:
                continue
            name = operation.get("operationId")
            if name not in {item["operationId"] for item in generated["operations"]}:
                continue  # Health and framework routes are outside the acceptance matrix.
            assert name not in actual, f"Duplicate operationId in OpenAPI: {name}"
            actual[name] = (method.upper(), normalize(path), operation)
    assert set(actual) == {item["operationId"] for item in generated["operations"]}, \
        f"OpenAPI operation mismatch: missing {set(item['operationId'] for item in generated['operations']) - set(actual)}"
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
    return len(actual), checked


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("openapi", type=Path, help="OpenAPI JSON exported from a running application")
    args = parser.parse_args()
    try:
        result = check(json.loads(args.openapi.read_text(encoding="utf-8")), links.generate())
        print(f"Verified {result[0]} generated OpenAPI operations and {result[1]} acceptance status/cause cases")
    except (AssertionError, KeyError, OSError, ValueError) as error:
        print(f"OpenAPI contract drift: {error}", file=sys.stderr)
        sys.exit(1)
