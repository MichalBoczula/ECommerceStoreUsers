#!/usr/bin/env python3
"""Mutation cases prove that the architecture gate rejects real forbidden edges."""
import importlib.util
import tempfile
from pathlib import Path

spec = importlib.util.spec_from_file_location("architecture", Path(__file__).with_name("check-architecture.py"))
architecture = importlib.util.module_from_spec(spec)
spec.loader.exec_module(architecture)


def fixture(root):
    for layer in architecture.LAYERS:
        folder = root / "src" / f"{architecture.PREFIX}.{layer}"
        folder.mkdir(parents=True)
        (folder / f"{architecture.PREFIX}.{layer}.csproj").write_text('<Project Sdk="Microsoft.NET.Sdk"><ItemGroup /></Project>')
        (folder / "Sample.cs").write_text(f"namespace {architecture.PREFIX}.{layer};\n")


with tempfile.TemporaryDirectory() as temp:
    root = Path(temp)
    fixture(root)
    assert not architecture.check(root), architecture.check(root)
    domain = root / 'src' / f'{architecture.PREFIX}.Domain'
    app = root / 'src' / f'{architecture.PREFIX}.Application'
    api = root / 'src' / f'{architecture.PREFIX}.{architecture.API}'
    (domain / 'Sample.cs').write_text(f'using {architecture.PREFIX}.Infrastructure.Repositories;\n')
    assert any('Domain -> Infrastructure' in e for e in architecture.check(root))
    (domain / 'Sample.cs').write_text('// using forbidden.namespace;\n')
    (app / 'Sample.cs').write_text(f'using {architecture.PREFIX}.{architecture.API}.Endpoints;\n')
    assert any('Application -> ' in e for e in architecture.check(root))
    (app / 'Sample.cs').write_text('namespace Test;\n')
    (api / 'Sample.cs').write_text(f'using {architecture.PREFIX}.Infrastructure.Persistence;\n')
    assert any('API -> Infrastructure' in e or 'Api -> Infrastructure' in e for e in architecture.check(root))
    (api / 'Sample.cs').write_text('namespace Test;\n')
    (api / 'Program.cs').write_text(f'using {architecture.PREFIX}.Infrastructure.Configuration;\n')
    assert not architecture.check(root), architecture.check(root)
    (api / 'Sample.cs').write_text(f'using {architecture.STORAGE[0]}.Driver;\n')
    assert any('persistence type' in e for e in architecture.check(root))
    (api / 'Sample.cs').write_text(f'using OtherService.Domain;\nusing {architecture.PREFIX}.OtherService.Contracts;\n')
    assert any('foreign service namespace' in e for e in architecture.check(root))
    (api / 'Sample.cs').write_text('namespace Test;\n')
    project = app / f'{architecture.PREFIX}.Application.csproj'
    project.write_text(f'<Project Sdk="Microsoft.NET.Sdk"><ItemGroup><ProjectReference Include="../{architecture.PREFIX}.Infrastructure/{architecture.PREFIX}.Infrastructure.csproj" /></ItemGroup></Project>')
    assert any('forbidden project reference' in e for e in architecture.check(root))
print('Architecture mutation cases passed.')
