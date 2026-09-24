#!/usr/bin/env python3
"""Generate and validate HTTP -> executed flow -> policy -> acceptance links.

This is a source projection, not a maintained operation mapping. It deliberately
fails closed when the current endpoint/use-case patterns change.
"""

import argparse
import csv
import json
import re
import sys
from collections import defaultdict
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
API = next((ROOT / "src").glob("*.Api/Endpoints"), None)
if API is None:
    API = next((ROOT / "src").glob("*.API/Endpoints"))
APP = next((ROOT / "src").glob("*.Application"))
DOMAIN = next((ROOT / "src").glob("*.Domain"))
PRODUCTS = (APP / "Features").exists()
ROUTE = re.compile(r'group\.Map(Get|Post|Put|Delete)\("([^"]*)"([\s\S]*?)\.WithName\("([^"]+)"\)')


def source(path):
    return path.read_text(encoding="utf-8-sig")


def unique(pairs, label):
    result = {}
    for key, value in pairs:
        assert key not in result, f"Duplicate {label}: {key}"
        result[key] = value
    return result


def registrations():
    code = source(DOMAIN / "DependencyInjection.cs")
    pairs = re.findall(r'IValidationPolicy<([^>]+)>,\s*(\w+)>', code)
    policies = unique(pairs, "policy registration type")
    published = re.findall(r'IValidationPolicyDescriptorProvider,\s*(\w+)>', code)
    assert len(published) == len(set(published)), "Duplicate published policy"
    assert set(policies.values()) == set(published), "Policy registration and publication differ"
    return policies


def product_flows(policies):
    flows = []
    for file in (APP / "Features").rglob("*FlowDescribtor.cs"):
        code = source(file)
        action = re.search(r'FlowDescriberBase<(\w+)>', code).group(1)
        handler = file.with_name(file.stem.replace("FlowDescribtor", "Handler") + ".cs")
        assert handler.is_file(), f"Missing handler: {action}"
        handler_code = source(handler)
        assert f"IFlowDescriber<{action}>" in source(API / "DocumentationsEndpoints.cs"), f"Unpublished flow: {action}"
        used_types = set(re.findall(r'IValidationPolicy<([^>]+)>\s+(\w+)', handler_code))
        descriptor_types = set(re.findall(r'IValidationPolicy<([^>]+)>', code))
        types = {kind for kind, variable in used_types if re.search(r'\b' + re.escape(variable) + r'\b', handler_code.split('Handle(', 1)[-1])}
        assert types == descriptor_types, f"Flow/handler policy mismatch: {action}: {types} != {descriptor_types}"
        assert types <= policies.keys(), f"Unregistered policy in {action}: {types - policies.keys()}"
        flows.append((action, {"flow": action, "policies": sorted(policies[kind] for kind in types)}))
    assert flows, "No business flows found"
    return unique(flows, "flow action")


def user_flows(policies):
    descriptors = unique(((match.group(1), (match.group(2),
                                            set(re.findall(r'IValidationPolicy<([^>]+)>', source(file)))))
                          for file in (APP / "Descriptors").rglob("*.cs")
                          for match in re.finditer(r'\bclass\s+(\w+)\s*:\s*FlowDescriberBase<(\w+)>', source(file))), "descriptor")
    assert descriptors, "No business flows found"
    services = []
    for file in (APP / "Services/Concrete").rglob("*Service.cs"):
        code = source(file)
        identity = re.search(r'\bclass\s+\w+\s*(?:\([^)]*\))?\s*:\s*(I\w+Service)\b', code, re.DOTALL)
        if not identity or "FlowDescriptorService" in file.name or "DescriptorService" in file.name:
            continue
        fields = unique(re.findall(r'IValidationPolicy<([^>]+)>\s+(_\w+)', code), "service policy type")
        # Reverse the constructor mapping: variable -> registered policy.
        fields = {field: kind for kind, field in fields.items()}
        for method in re.finditer(r'public\s+async\s+Task[^\n]*?\s+(\w+)\s*\(', code):
            end = code.find("\n        }", method.end())
            assert end != -1, f"Unclosed service method: {method.group(1)}"
            body = code[method.end():end]
            constructed = re.findall(r'var descriptor = new (\w+Descriptor)\(\);', body)
            assert len(constructed) == 1, f"Expected one executed flow in {method.group(1)}: {constructed}"
            policy_fields = set(re.findall(r'\b(_\w+ValidationPolicy)\b', body))
            assert policy_fields <= fields.keys(), f"Unknown policy fields in {method.group(1)}"
            kinds = {fields[field] for field in policy_fields}
            assert kinds <= policies.keys(), f"Unregistered policies in {method.group(1)}"
            assert constructed[0] in descriptors, f"Undocumented flow: {constructed[0]}"
            flow, declared_kinds = descriptors[constructed[0]]
            assert kinds == declared_kinds, f"Flow/service policy mismatch: {method.group(1)}: {kinds} != {declared_kinds}"
            services.append(((identity.group(1), method.group(1)),
                             {"flow": flow,
                              "policies": sorted(policies[kind] for kind in kinds),
                              "descriptor": constructed[0]}))
    assert services, "No executed service methods found"
    return unique(services, "service method"), set(descriptors)


def generate():
    policies = registrations()
    flows = product_flows(policies) if PRODUCTS else None
    services, descriptors = (None, None) if PRODUCTS else user_flows(policies)
    operations = []
    seen = set()
    used_flows = set()
    used_policies = set()
    for file in sorted(API.glob("*.cs")):
        code = source(file)
        group = re.search(r'MapGroup\("([^"]+)"\)', code)
        assert group, f"Missing route group: {file}"
        for match in ROUTE.finditer(code):
            verb, route, section, operation_id = match.groups()
            assert operation_id not in seen, f"Duplicate operationId: {operation_id}"
            seen.add(operation_id)
            calls = (re.findall(r'mediator\s*\.\s*Send\(\s*new\s+(\w+)\s*\(', section)
                     if PRODUCTS else re.findall(r'\b(\w+Service)\s*\.\s*(\w+)\s*\(', section))
            if file.stem in {"DocumentationsEndpoints", "DocumentationEndpoints"}:
                if PRODUCTS:
                    assert not calls, f"Unexpected business call in documentation endpoint: {operation_id}"
                link = {"flow": None, "policies": []}
            else:
                assert len(calls) == 1, f"Expected one business call in {operation_id}: {calls}"
                key = calls[0] if PRODUCTS else calls[0]
                if PRODUCTS:
                    assert key in flows, f"No described flow for {operation_id}: {key}"
                    link = flows[key]
                    assert key not in used_flows, f"Flow linked by multiple operations: {key}"
                    used_flows.add(key)
                else:
                    # Match the endpoint receiver to its injected interface, not its name alone.
                    receiver, method = key
                    interface = re.search(r'\b(I\w+Service)\s+' + re.escape(receiver) + r'\b', section)
                    assert interface, f"Unresolved service receiver: {operation_id}"
                    identity = (interface.group(1), method)
                    assert identity in services, f"No described flow for {operation_id}: {identity}"
                    link = services[identity]
                    assert link["descriptor"] not in used_flows, f"Flow linked by multiple operations: {link['descriptor']}"
                    used_flows.add(link["descriptor"])
                used_policies.update(link["policies"])
            operations.append({"operationId": operation_id, "method": verb.upper(),
                               "path": (group.group(1) + route).rstrip("/") or "/",
                               "flow": link["flow"], "policies": link["policies"]})
    expected = set(flows) if PRODUCTS else descriptors
    assert used_flows == expected, f"Orphan flows: {sorted(expected - used_flows)}; unexpected: {sorted(used_flows - expected)}"
    with (ROOT / "docs/acceptance-matrix.tsv").open(newline="", encoding="utf-8") as stream:
        rows = list(csv.DictReader(stream, delimiter="\t"))
    scenarios = defaultdict(list)
    ids = set()
    for row in rows:
        assert row["scenarioId"] not in ids, f"Duplicate scenarioId: {row['scenarioId']}"
        ids.add(row["scenarioId"])
        scenarios[row["operationId"]].append({"scenarioId": row["scenarioId"],
                                               "status": int(row["status"]), "cause": row["cause"]})
        matching = [operation for operation in operations if operation["operationId"] == row["operationId"]]
        assert len(matching) == 1, f"Orphan scenario: {row['scenarioId']}"
        assert (row["method"], row["path"]) == (matching[0]["method"], matching[0]["path"]), f"Stale route: {row['scenarioId']}"
    for operation in operations:
        operation["scenarios"] = scenarios[operation["operationId"]]
        assert operation["scenarios"], f"Operation without scenario: {operation['operationId']}"
    assert operations, "No HTTP operations found"
    return {"service": "ProductsCatalog" if PRODUCTS else "ECommerceStoreUsers",
            "operations": operations,
            "standalonePolicies": sorted(set(policies.values()) - used_policies)}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, help="Write generated JSON to this path (stdout by default)")
    args = parser.parse_args()
    try:
        result = generate()
        output = json.dumps(result, indent=2, ensure_ascii=False) + "\n"
        if args.output:
            args.output.parent.mkdir(parents=True, exist_ok=True)
            args.output.write_text(output, encoding="utf-8")
        else:
            print(output, end="")
        print(f"Linked {len(result['operations'])} operations; standalone policies: {len(result['standalonePolicies'])}", file=sys.stderr)
    except (AssertionError, AttributeError, OSError, ValueError) as error:
        print(f"Operation link error: {error}", file=sys.stderr)
        sys.exit(1)
