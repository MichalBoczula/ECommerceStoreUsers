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


LITERAL = r'"(?:[^"\\]|\\.)*"'


def attributes(kind, code):
    for match in re.finditer(r'\[' + kind + r'\((.*?)\)\]', code, re.DOTALL):
        text = match.group(1)
        positional = re.split(r',\s*(?:Status|Code|Stage|RuleId|RuleName)\s*=', text)[0]
        values = [json.loads(value) for value in re.findall(LITERAL, positional)]
        named = {key: json.loads(value) for key, value in re.findall(r'(Status|Code|Stage|RuleId|RuleName)\s*=\s*(' + LITERAL + r'|\d+|null)', text)}
        yield values, named


def users_documentation(operations, registrations):
    registry = {}
    for path in sorted((DOMAIN / 'Validation/Concrete').rglob('*.cs')):
        meta = list(attributes('Documentation', source(path)))
        if not meta:
            continue
        assert len(meta) == 1 and len(meta[0][0]) == 2 and all(meta[0][0]), f'Missing/duplicate type metadata: {path}'
        values, _ = meta[0]
        assert path.stem not in registry, f'Ambiguous documentation type: {path.stem}'
        registry[path.stem] = {'id': values[0], 'summary': values[1], 'source': str(path.relative_to(ROOT)), 'name': path.stem}
    assert set(registrations.values()) <= registry.keys(), 'Registered policy lacks stable metadata'
    rule_classes = {path.stem for path in (DOMAIN / 'Validation/Concrete/Rules').rglob('*.cs')}
    assert rule_classes <= registry.keys(), 'Rule class lacks stable metadata'
    policies = {}
    rule_docs = {registry[name]['id']: dict(registry[name], kind='policy-rule', usage='uninstalled', policyIds=[]) for name in rule_classes}
    for name in registrations.values():
        meta = dict(registry[name]);code = source(ROOT / meta['source'])
        names = sorted(set(re.findall(r'\bnew\s+(\w+Rule)\s*\(', code)))
        assert names and set(names) <= rule_classes, f'Undocumented policy rule: {name}'
        meta['ruleIds'] = [registry[rule]['id'] for rule in names]
        for identity in meta['ruleIds']:
            rule_docs[identity]['policyIds'].append(meta['id'])
            rule_docs[identity]['usage'] = 'installed-in-registered-policy'
        policies[name] = meta
    flows = {};branches = {}
    for path in sorted((APP / 'Descriptors').rglob('*.cs')):
        code = source(path);action = re.search(r'FlowDescriberBase<(\w+)>', code).group(1)
        meta = list(attributes('Documentation', code))
        assert len(meta) == 1 and len(meta[0][0]) == 2 and all(meta[0][0]), f'Missing flow metadata: {path}'
        values, _ = meta[0];steps = []
        for match in re.finditer(r'\[FlowStep\((.*?)\)\]\s*public\s+(?:async\s+)?[\w<>,?\[\]\s]+?\s+(\w+)\s*\(', code, re.DOTALL):
            args, method = match.groups();parts = list(attributes('FlowStep', '[FlowStep(' + args + ')]'))
            strings, named = parts[0]
            assert len(strings) == 2 and all(strings), f'Missing step identity/meaning: {action}.{method}'
            order = re.search(r'\border:\s*(\d+)', args)
            assert order, f'Missing explicit step order: {action}.{method}'
            step = {'id': values[0] + '.' + strings[0], 'bpmnId': strings[0], 'order': int(order[1]), 'method': method, 'description': strings[1]}
            if 'RuleId' in named:
                assert named.get('RuleName'), f'Inline guard lacks actual error name: {action}.{method}'
                step['inlineRuleId'] = named['RuleId']
                inline = {'id': named['RuleId'], 'name': named['RuleName'], 'kind': 'inline-guard', 'summary': 'Reject explicit null required request objects before persistence.', 'source': str(path.relative_to(ROOT)), 'usage': 'invoked-inline-guard', 'policyIds': []}
                if inline['id'] in rule_docs:
                    assert rule_docs[inline['id']]['name'] == inline['name'], 'Ambiguous inline rule name'
                else:
                    rule_docs[inline['id']] = inline
            steps.append(step)
        assert steps and [step['order'] for step in steps] == list(range(1, len(steps)+1)), f'Incomplete step order: {action}'
        assert len({step['id'] for step in steps}) == len(steps), f'Duplicate step ID: {action}'
        flows[action] = {'id': values[0], 'flowName': action, 'summary': values[1], 'source': str(path.relative_to(ROOT)), 'steps': steps, 'branches': []}
        for args, named in attributes('FlowOutcome', code):
            assert len(args) >= 3 and all(args), f'Incomplete outcome: {action}'
            assert named.get('Stage') in ['application', 'persistence'], f'Invalid flow outcome stage: {action}'
            assert isinstance(named.get('Status'), int), f'Missing outcome status: {action}'
            branch = {'id': args[0], 'condition': args[1], 'effects': args[2], 'ruleIds': args[3:], 'status': named['Status'], 'problemCodes': (named.get('Code') or '').split('|') if named.get('Code') else [], 'stage': named['Stage'], 'flowId': values[0], 'source': str(path.relative_to(ROOT))}
            assert branch['id'] not in branches, f'Duplicate branch ID: {branch["id"]}'
            branches[branch['id']] = branch;flows[action]['branches'].append(branch['id'])
    for path in sorted(API.parent.rglob('*.cs')):
        for args, named in attributes('FlowOutcome', source(path)):
            assert len(args) == 3 and named.get('Stage') in ['framework','exception-handler','documentation'], f'Invalid API outcome: {path}'
            branch = {'id': args[0], 'condition': args[1], 'effects': args[2], 'ruleIds': [], 'status': named['Status'], 'problemCodes': [named['Code']] if named.get('Code') else [], 'stage': named['Stage'], 'flowId': None, 'source': str(path.relative_to(ROOT))}
            assert branch['id'] not in branches, f'Duplicate branch ID: {branch["id"]}'
            branches[branch['id']] = branch
    identities = [entry['id'] for entry in registry.values()] + [flow['id'] for flow in flows.values()] + [step['id'] for flow in flows.values() for step in flow['steps']] + list(branches) + [identity for identity, rule in rule_docs.items() if rule['kind']=='inline-guard']
    assert len(identities) == len(set(identities)), 'Duplicate stable documentation ID'
    assert all(re.fullmatch(r'users\.[A-Za-z0-9.-]+', identity) for identity in identities), 'Invalid stable documentation ID'
    for branch in branches.values():
        assert set(branch['ruleIds']) <= rule_docs.keys(), f'Orphan branch rule: {branch["id"]}'
        assert (branch['status'] < 400) == (not branch['problemCodes']), f'Outcome status/code mismatch: {branch["id"]}'
    with (ROOT/'docs/acceptance-matrix.tsv').open(newline='', encoding='utf-8') as stream:
        cases = {row['scenarioId']:row for row in csv.DictReader(stream, delimiter='\t')}
    covered_branches=set();response_rules=set();conflict_rules=set();used_policies=set()
    for operation in operations:
        flow=flows.get(operation['flow']);operation['flowId']=flow['id'] if flow else None
        operation['policyIds']=[policies[name]['id'] for name in operation['policies']]
        allowed={identity for name in operation['policies'] for identity in policies[name]['ruleIds']}
        if flow:allowed.update(step['inlineRuleId'] for step in flow['steps'] if 'inlineRuleId' in step)
        operation['ruleIds']=sorted(allowed);used_policies.update(operation['policies'])
        for case in operation['scenarios']:
            row=cases[case['scenarioId']];identity=row.get('branchId');assert identity in branches, f'Orphan/missing branch: {case["scenarioId"]}: {identity}'
            branch=branches[identity];assert branch['flowId']==operation['flowId'] or branch['stage'] in ['framework','exception-handler'], f'Branch belongs to another flow: {case["scenarioId"]}'
            assert branch['status']==case['status'], f'Branch status differs: {case["scenarioId"]}'
            assert not flow or set(branch['ruleIds'])<=allowed, f'Branch uses uninstalled/unrelated rules: {identity}'
            rules=[identity for identity in row.get('ruleIds','').split(',') if identity]
            assert len(rules)==len(set(rules)) and set(rules)<=set(branch['ruleIds']), f'Orphan/incorrect scenario rule: {case["scenarioId"]}'
            case.update({'branchId':identity,'ruleIds':rules,'feature':row['feature'],'scenario':row['scenario']})
            covered_branches.add(identity);response_rules.update(rules)
            if branch['status']==409:conflict_rules.update(branch['ruleIds'])
    # Existing Gherkin identities/examples remain intact: technical outline code values already express their intent.
    from importlib.machinery import SourceFileLoader
    matrix=SourceFileLoader('documentation_matrix',str(ROOT/'scripts/check-acceptance-matrix.py')).load_module()
    technical=[]
    for path in sorted(matrix.FEATURES.rglob('*.feature')):
        for title,item in matrix.scenarios(path).items():
            tagged=[tag.split(':',1)[1] for tag in item['tags'] if tag.startswith('@allure.label.branchId:')]
            assert len(tagged)<=1, f'Duplicate technical branch: {title}'
            by_code='@documentation.framework' in item['tags']
            assert not(tagged and by_code), f'Duplicate technical branch sources: {title}'
            if not tagged and not by_code:continue
            for example in item['examples'] if item['outline'] else [None]:
                sid=matrix.scenario_id(item,example)
                identity=tagged[0] if tagged else 'users.branch.http.'+example['Code'].replace('_','-')
                assert identity in branches and branches[identity]['flowId'] is None, f'Unknown/nontechnical branch: {identity}'
                if sid in cases:
                    assert cases[sid]['branchId']==identity, f'Technical code disagrees with matrix: {sid}'
                    continue
                technical.append({'scenarioId':sid,'branchId':identity,'source':str(path.relative_to(ROOT))});covered_branches.add(identity)
    for name, policy in policies.items():
        policy['usage']='invoked-by-endpoint-flow' if name in used_policies else 'standalone'
        policy['operationIds']=sorted(op['operationId'] for op in operations if name in op['policies'])
    active={identity for name in used_policies for identity in policies[name]['ruleIds']}
    active.update(identity for identity,rule in rule_docs.items() if rule['kind']=='inline-guard')
    return {'serviceId':'users','flows':list(flows.values()),'policies':list(policies.values()),'rules':sorted(rule_docs.values(),key=lambda r:r['id']),'branches':list(branches.values()),'technicalScenarios':technical,
            'coverage':{'basis':'Explicit scenario intent linked to real exchanges, not runtime step/branch telemetry or every error combination.',
                        'branchesWithoutLinkedScenario':sorted(set(branches)-covered_branches),
                        'activeRulesWithoutRejectedScenario':sorted(active-response_rules-conflict_rules),
                        'rulesExpectedInResponse':sorted(response_rules),'rulesMappedToConflictWithoutErrorEntries':sorted(conflict_rules),
                        'uninstalledRuleIds':sorted(identity for identity,rule in rule_docs.items() if rule['usage']=='uninstalled')}}


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
    result = {"service": "ProductsCatalog" if PRODUCTS else "ECommerceStoreUsers",
            "operations": operations,
            "standalonePolicies": sorted(set(policies.values()) - used_policies)}
    if not PRODUCTS:
        result["documentation"] = users_documentation(operations, policies)
    return result


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

