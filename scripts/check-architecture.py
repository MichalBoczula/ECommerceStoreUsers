#!/usr/bin/env python3
"""REF-11 architecture gate: source and project references; no .NET runtime required."""
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

PREFIX = 'ECommerceStoreUsers'
API = 'API'
STORAGE = ('MongoDB',)
COMPOSITION = ('Program.cs', 'Configuration/MongoInitializationHostedService.cs')
LAYERS = ("Domain", "Application", "Infrastructure", API)
# API may assemble infrastructure services only at these explicit startup locations.
ALLOWED_REFS = {
    "Domain": set(),
    "Application": {"Domain"},
    "Infrastructure": {"Domain"},
    API: {"Domain", "Application", "Infrastructure"},
}
TOKEN = re.compile(r"(?:[A-Za-z_][\w]*\.)+[A-Za-z_][\w]*")
# Ignore comments and string literals so route text, diagnostics and documentation are not imports.
NON_CODE = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:""|[^"])*"|\$?"(?:\\.|[^"\\])*"', re.M)


def source_code(contents):
    # Keep line breaks to report useful diagnostics.
    return NON_CODE.sub(lambda match: "\n" * match.group().count("\n") + " ", contents)


def check(root):
    errors = []
    sources = root / "src"
    for layer in LAYERS:
        project = sources / f"{PREFIX}.{layer}" / f"{PREFIX}.{layer}.csproj"
        if not project.exists():
            errors.append(f"missing project: {project}")
            continue
        tree = ET.parse(project)
        for ref in tree.findall(".//ProjectReference"):
            target = Path(ref.attrib["Include"].replace("\\", "/")).stem
            if target not in {f"{PREFIX}.{name}" for name in ALLOWED_REFS[layer]}:
                errors.append(f"{project.relative_to(root)}: forbidden project reference {target}")
        if layer in ("Domain", "Application"):
            for ref in tree.findall(".//PackageReference"):
                package = ref.attrib.get("Include", "")
                if package.startswith(STORAGE) or package.startswith("Microsoft.EntityFrameworkCore") or package == "Dapper":
                    errors.append(f"{project.relative_to(root)}: persistence package {package}")
        folder = project.parent
        for file in folder.rglob("*.cs"):
            relative = file.relative_to(folder).as_posix()
            if relative.startswith(("bin/", "obj/")):
                continue
            code = source_code(file.read_text(encoding="utf-8-sig"))
            for match in TOKEN.finditer(code):
                token = match.group()
                location = f"{file.relative_to(root)}:{code.count(chr(10), 0, match.start()) + 1}"
                if token.startswith(PREFIX + "."):
                    target = token[len(PREFIX) + 1:].split(".")[0]
                    if target in LAYERS and target != layer:
                        if target not in ALLOWED_REFS[layer] or (layer == API and target == "Infrastructure" and relative not in COMPOSITION):
                            errors.append(f"{location}: forbidden {layer} -> {target}: {token}")
                    elif target not in LAYERS:
                        errors.append(f"{location}: foreign service namespace: {token}")
                if layer in ("Domain", "Application", API) and any(token == item or token.startswith(item + ".") for item in STORAGE):
                    errors.append(f"{location}: persistence type outside Infrastructure: {token}")
    return errors


if __name__ == "__main__":
    repo = Path(__file__).resolve().parent.parent
    violations = check(repo)
    for violation in violations:
        print(violation, file=sys.stderr)
    if violations:
        sys.exit(1)
    print("Architecture boundaries verified.")
