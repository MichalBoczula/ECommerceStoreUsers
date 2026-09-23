#!/usr/bin/env python3
"""Check that generated flow and policy descriptions follow executable code."""

import re
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
APP = next((ROOT / "src").glob("*.Application"))
API = next((ROOT / "src").glob("*.Api"), None) or next((ROOT / "src").glob("*.API"))
DOMAIN = next((ROOT / "src").glob("*.Domain"))
STEP = re.compile(
    r"\[FlowStep\(([^\]]*)\)\]\s*public\s+(?:virtual\s+)?(?:async\s+)?"
    r"[\w<>,?\[\]\s]+?\s+(\w+)\s*\("
)


def source(path):
    return path.read_text(encoding="utf-8-sig")


def check(descriptor, calls):
    steps = [(int(re.search(r"\d+", args).group()), name)
             for args, name in STEP.findall(source(descriptor))]
    assert steps, f"No annotated steps in {descriptor}"
    steps.sort()
    assert [order for order, _ in steps] == list(range(1, len(steps) + 1)), descriptor
    expected = [name for _, name in steps]
    assert len(expected) == len(set(expected)), f"Duplicate step method in {descriptor}"
    assert set(calls) == set(expected), (descriptor, "missing or unannotated calls", calls, expected)

    # A branch may return early and invoke the final mapping step before write steps.
    # Verify the full path is an ordered subsequence without rejecting that branch.
    remaining = iter(calls)
    for name in expected:
        assert any(call == name for call in remaining), (descriptor, "step order", name, calls)
    return len(steps)


def products():
    descriptors = sorted(APP.glob("Features/**/*FlowDescribtor.cs"))
    endpoint = source(API / "Endpoints/DocumentationsEndpoints.cs")
    registration = source(APP / "DependencyInjection.cs")
    count = 0
    for descriptor in descriptors:
        name = descriptor.stem
        action = re.search(r"FlowDescriberBase<(\w+)>", source(descriptor)).group(1)
        handler = descriptor.with_name(name.replace("FlowDescribtor", "Handler") + ".cs")
        assert handler.is_file(), handler
        text = source(handler)
        receiver = re.search(r"\b" + name + r"\s+(\w+)", text).group(1)
        calls = re.findall(r"\b" + receiver + r"\s*\.\s*(\w+)\s*\(", text)
        count += check(descriptor, calls)
        assert f"IFlowDescriber<{action}>" in endpoint, (descriptor, "not exposed")
        assert f"IFlowDescriber<{action}>, {name}>" in registration, (descriptor, "not registered")
    assert descriptors, "No flow descriptors found"
    assert endpoint.count(".DescribeFlow(default!)") == len(descriptors), "Flow endpoint omits a descriptor"
    return len(descriptors), count


def users():
    descriptors = sorted(path for path in (APP / "Descriptors").rglob("*.cs")
                         if "FlowDescriberBase<" in source(path))
    services = [source(path) for path in (APP / "Services/Concrete").rglob("*Service.cs")]
    endpoint = source(API / "Endpoints/DocumentationEndpoints.cs")
    count = 0
    for descriptor in descriptors:
        name = re.search(r"\bclass\s+(\w+)\s*:\s*FlowDescriberBase<",
                         source(descriptor)).group(1)
        matches = []
        for service in services:
            for match in re.finditer(r"var descriptor = new " + name + r"\(\);", service):
                # Each service method ends at this indentation; inner blocks are deeper.
                end = service.find("\n        }", match.end())
                assert end != -1, (descriptor, "service method not closed")
                matches.append(re.findall(r"\bdescriptor\s*\.\s*(\w+)\s*\(",
                                          service[match.end():end]))
        use_cases = [calls for calls in matches if calls != ["Describe"]]
        assert len(use_cases) == 1, (descriptor, "expected one use case", use_cases)
        count += check(descriptor, use_cases[0])
        providers = [service for service in services
                     if re.search(r"public FlowDescriptor (\w+)\(\)[\s\S]*?"
                                  r"var descriptor = new " + name + r"\(\);", service)]
        assert len(providers) == 1, (descriptor, "no documentation provider")
        method = re.search(r"public FlowDescriptor (\w+)\(\)\s*\{\s*"
                           r"var descriptor = new " + name + r"\(\);", providers[0]).group(1)
        assert f".{method})" in endpoint, (descriptor, "not exposed")
    assert descriptors, "No flow descriptors found"
    return len(descriptors), count


def policies():
    registration = source(DOMAIN / "DependencyInjection.cs")
    endpoint = source(API / "Endpoints" /
                      ("DocumentationsEndpoints.cs" if (APP / "Features").exists()
                       else "DocumentationEndpoints.cs"))
    assert "IEnumerable<IValidationPolicyDescriptorProvider>" in endpoint
    assert "provider.Describe()" in endpoint
    registered = set(re.findall(r"IValidationPolicyDescriptorProvider, (\w+)>", registration))
    used = set(re.findall(r"IValidationPolicy<[^>]+>, (\w+)>", registration))
    assert registered == used, ("policy registration and documentation differ", used, registered)
    policy_files = [path for path in (DOMAIN / "Validation/Concrete/Policies").rglob("*Policy.cs")
                    if path.stem in registered]
    assert {path.stem for path in policy_files} == registered, "Registered policy source missing"
    for path in policy_files:
        text = source(path)
        assert "IValidationPolicyDescriptorProvider" in text, (path, "no descriptor provider")
        parts = text.split("public ValidationPolicyDescriptor Describe()")
        assert len(parts) == 2, (path, "missing Describe")
        validate = re.split(r"public (?:async )?Task<ValidationResult> Validate\(", parts[0])
        assert len(validate) == 2, (path, "missing Validate")
        described = parts[1]
        collections = set(re.findall(r"\b(_\w*[Rr]ules)\s*=", text))
        assert collections, (path, "no rule collections")
        for collection in collections:
            assert collection in validate[1] and collection in described, (path, "undocumented rules", collection)
        assert ".Describe()" in described, (path, "rule descriptions missing")
    assert policy_files, "No validation policies found"
    return len(policy_files)


if __name__ == "__main__":
    try:
        flows, steps = products() if (APP / "Features").exists() else users()
        count = policies()
        print(f"Verified {steps} executed steps across {flows} exposed flows and {count} policies")
    except (AssertionError, AttributeError, OSError, ValueError) as error:
        print(f"Description link error: {error}", file=sys.stderr)
        sys.exit(1)
