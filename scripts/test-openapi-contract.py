#!/usr/bin/env python3
"""Regressions for BDD inventory, source/matrix links and exported contracts."""

import csv
import importlib.util
import json
import shutil
import unittest
from contextlib import contextmanager
from pathlib import Path
from tempfile import TemporaryDirectory
from unittest.mock import patch


spec = importlib.util.spec_from_file_location("contract", Path(__file__).with_name("check-openapi-contract.py"))
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)

matrix_spec = importlib.util.spec_from_file_location("matrix", Path(__file__).with_name("check-acceptance-matrix.py"))
matrix = importlib.util.module_from_spec(matrix_spec)
matrix_spec.loader.exec_module(matrix)


class SourceCoverageTests(unittest.TestCase):
    @contextmanager
    def endpoints(self, code):
        with TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Endpoints.cs").write_text(code, encoding="utf-8")
            with patch.object(matrix, "API", root):
                yield

    def test_complete_registration_before_and_after_name_and_nested_generic(self):
        code = '''var group = app.MapGroup("/favorites");
group.MapGet("", () => Results.Ok())
    .WithName("ListFavorites")
    .Produces<IReadOnlyList<FavoriteDto>>(StatusCodes.Status200OK)
    .Produces<ApiProblemDetails>(StatusCodes.Status500InternalServerError);
group.MapDelete("/{id:guid}", () => Results.NoContent())
    .Produces(StatusCodes.Status204NoContent)
    .WithName("DeleteFavorite");
'''
        with self.endpoints(code):
            self.assertEqual(matrix.operations(), {
                "ListFavorites": ("GET", "/favorites", {"200", "500"}),
                "DeleteFavorite": ("DELETE", "/favorites/{id:guid}", {"204"})})

    def test_empty_status_set_fails(self):
        with self.endpoints('var group = app.MapGroup("/favorites");\n'
                            'group.MapGet("", () => Results.Ok()).WithName("ListFavorites");'):
            with self.assertRaisesRegex(AssertionError, "No declared response statuses parsed"):
                matrix.operations()

    def test_duplicate_operation_name_fails(self):
        code = 'var group = app.MapGroup("/favorites");\n' + '\n'.join(
            f'group.MapGet("/{route}", () => Results.Ok()).WithName("SameName")'
            '.Produces(StatusCodes.Status200OK);' for route in ("first", "second"))
        with self.endpoints(code), self.assertRaisesRegex(AssertionError, "Duplicate operationId"):
            matrix.operations()

    @contextmanager
    def changed_matrix(self, mutate):
        with matrix.MATRIX.open(newline="", encoding="utf-8") as stream:
            reader = csv.DictReader(stream, delimiter="\t")
            columns, rows = reader.fieldnames, list(reader)
        with TemporaryDirectory() as directory:
            path = Path(directory) / "matrix.tsv"
            with path.open("w", newline="", encoding="utf-8") as stream:
                writer = csv.DictWriter(stream, fieldnames=columns, delimiter="\t")
                writer.writeheader()
                writer.writerows(mutate(rows))
            with patch.object(matrix, "MATRIX", path):
                yield

    def test_removed_response_case_fails(self):
        def remove(rows):
            return [row for row in rows if not (row["operationId"] == "GetFlowDocumentation" and row["status"] == "500")]
        with self.changed_matrix(remove), self.assertRaisesRegex(
                AssertionError, "Missing documented response status/operation links.*GetFlowDocumentation.*500"):
            matrix.verify()

    def test_undeclared_matrix_status_fails(self):
        def change(rows):
            rows[0]["status"] = "418"
            return rows
        with self.changed_matrix(change), self.assertRaisesRegex(AssertionError, "Undeclared response status"):
            matrix.verify()

    def test_stale_matrix_route_fails(self):
        def change(rows):
            rows[0]["path"] = "/stale"
            return rows
        with self.changed_matrix(change), self.assertRaisesRegex(AssertionError, "Stale method/path"):
            matrix.verify()

    def test_stale_matrix_operation_fails(self):
        def change(rows):
            rows[0]["operationId"] = "RemovedOperation"
            return rows
        with self.changed_matrix(change), self.assertRaisesRegex(AssertionError, "Missing or stale operationId"):
            matrix.verify()

    def test_stale_scenario_title_fails(self):
        def change(rows):
            rows[0]["scenario"] = "Removed scenario"
            return rows
        with self.changed_matrix(change), self.assertRaisesRegex(AssertionError, "Removed scenario"):
            matrix.verify()

    def test_duplicate_matrix_identity_fails(self):
        def change(rows):
            rows[0]["scenarioId"] = rows[1]["scenarioId"]
            return rows
        with self.changed_matrix(change), self.assertRaisesRegex(AssertionError, "Duplicate scenarioId"):
            matrix.verify()

    def test_example_in_another_outline_does_not_satisfy_matrix_link(self):
        with TemporaryDirectory() as directory:
            features = Path(directory) / "Features"
            shutil.copytree(matrix.FEATURES, features)
            feature = features / "Common/WriteFailures.feature"
            text = feature.read_text(encoding="utf-8").replace("| CreateCustomer       |", "| MissingOperation     |")
            self.assertIn("MissingOperation", text)
            feature.write_text(text + '\n  Scenario Outline: Another outline\n'
                               '    Given the "<operationId>" write will fail at the repository\n'
                               '    Examples:\n      | operationId |\n      | CreateCustomer |\n', encoding="utf-8")
            with patch.object(matrix, "FEATURES", features), self.assertRaisesRegex(
                    AssertionError, "Missing outline example for matrix case.*REF07_CreateCustomer"):
                matrix.verify()


class BddInventoryTests(unittest.TestCase):
    def test_all_baseline_cases_are_preserved(self):
        matrix.verify_baseline(matrix.inventory())

    def test_replacing_an_example_with_equal_total_count_fails(self):
        cases = json.loads(matrix.BASELINE.read_text(encoding="utf-8"))["cases"]
        case = next(case for case in cases if case["example"])
        case["example"][next(iter(case["example"]))] = "replacement"
        with self.assertRaisesRegex(AssertionError, "Missing baseline BDD cases"):
            matrix.verify_baseline(cases)

    def test_new_cases_are_allowed(self):
        cases = matrix.inventory() + [{"feature": "New.feature", "scenario": "New behavior", "example": None}]
        matrix.verify_baseline(cases)

    def test_added_identity_column_preserves_original_behavioral_examples(self):
        cases = json.loads(matrix.BASELINE.read_text(encoding="utf-8"))["cases"]
        for case in cases:
            if case["example"] is not None:
                case["example"]["scenarioId"] = "REF09_Metadata"
        matrix.verify_baseline(cases)

    def test_identity_must_come_from_exactly_one_source(self):
        item = {"tags": ["@allure.label.scenarioId:REF09_Metadata"]}
        self.assertEqual("REF09_Metadata", matrix.scenario_id(item))
        self.assertEqual("REF09_Example", matrix.scenario_id({"tags": []}, {"scenarioId": "REF09_Example"}))
        for broken, example in [({"tags": []}, None), (item, {"scenarioId": "REF09_Example"})]:
            with self.assertRaisesRegex(AssertionError, "Specify scenarioId once"):
                matrix.scenario_id(broken, example)

    def test_missing_description_or_identity_and_bypassing_clients_fail(self):
        with TemporaryDirectory() as directory:
            root = Path(directory)
            feature = root / "Example.feature"
            valid = 'Feature: Evidence\n  @allure.label.scenarioId:REF09_Metadata\n  Scenario: Example\n' \
                    '    A readable explanation.\n    Given a step\n'
            feature.write_text(valid, encoding="utf-8")
            with patch.object(matrix, "FEATURES", root):
                matrix.verify_evidence_sources()
                feature.write_text(valid.replace('    A readable explanation.\n', ''), encoding="utf-8")
                with self.assertRaisesRegex(AssertionError, "Missing readable scenario description"):
                    matrix.verify_evidence_sources()
                feature.write_text(valid.replace('  @allure.label.scenarioId:REF09_Metadata\n', ''), encoding="utf-8")
                with self.assertRaisesRegex(AssertionError, "Specify scenarioId once"):
                    matrix.verify_evidence_sources()
                feature.write_text(valid, encoding="utf-8")
                step = root / "Steps.cs"
                for bypass in ['factory.CreateClient()', 'factory.CreateDefaultClient(capture)', 'new HttpClient()']:
                    step.write_text(bypass, encoding="utf-8")
                    with self.assertRaisesRegex(AssertionError, "BDD HTTP client bypasses shared evidence"):
                        matrix.verify_evidence_sources()
                step.write_text('context.CreateClient(factory)', encoding="utf-8")
                matrix.verify_evidence_sources()

    def test_reusing_identity_for_different_scenarios_fails(self):
        with TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "Example.feature").write_text(
                'Feature: Evidence\n' + ''.join(
                    f'  @allure.label.scenarioId:REF09_Same\n  Scenario: {name}\n'
                    '    A readable explanation.\n    Given a step\n' for name in ('First', 'Second')), encoding="utf-8")
            with patch.object(matrix, "FEATURES", root), self.assertRaisesRegex(
                    AssertionError, "scenarioId reused by different scenarios"):
                matrix.verify_evidence_sources()

    def test_multiple_example_blocks_and_escaped_values_ignore_step_tables_and_docstrings(self):
        with TemporaryDirectory() as directory:
            feature = Path(directory) / "Example.feature"
            feature.write_text('Feature: Inventory\n'
                               '  Scenario Outline: Expanded <value>\n'
                               '    Given a table\n      | value |\n      | not an example |\n'
                               '    And a docstring\n      """\nExamples:\n| fake |\n      """\n'
                               '    Examples: First\n      | value |\n      | left\\|right |\n'
                               '    @second\n    Examples: Second\n      | value |\n      | next |\n'
                               '  Scenario: Plain\n    Given a step\n', encoding="utf-8")
            with patch.object(matrix, "FEATURES", Path(directory)):
                self.assertEqual(matrix.inventory(), [
                    {"feature": "Example.feature", "scenario": "Expanded <value>", "example": {"value": "left|right"}},
                    {"feature": "Example.feature", "scenario": "Expanded <value>", "example": {"value": "next"}},
                    {"feature": "Example.feature", "scenario": "Plain", "example": None}])

    def test_outline_without_examples_fails(self):
        with TemporaryDirectory() as directory:
            feature = Path(directory) / "Example.feature"
            feature.write_text('Feature: Broken\n  Scenario Outline: Empty\n    Given a step\n', encoding="utf-8")
            with self.assertRaisesRegex(AssertionError, "Outline without examples"):
                matrix.scenarios(feature)


class ContractDriftTests(unittest.TestCase):
    def setUp(self):
        self.links = contract.links.generate()
        self.document = {"paths": {}, "components": {"schemas": {
            "ApiProblemDetails": {"type": "object", "properties": {
                "title": {"type": "string"}, "status": {"type": "integer"},
                "detail": {"type": "string"}, "type": {"type": "string"}}}}}}
        for item in self.links["operations"]:
            responses = {}
            for case in item["scenarios"]:
                status = case["status"]
                if status == 204:
                    responses[str(status)] = {"description": "No content"}
                else:
                    media = "application/problem+json" if status >= 400 else "application/json"
                    schema = ({"$ref": "#/components/schemas/ApiProblemDetails"} if status >= 400
                              else {"type": "object", "properties": {"id": {"type": "string"}}})
                    responses[str(status)] = {"content": {media: {"schema": schema}}}
            path = contract.normalize(item["path"])
            self.document["paths"].setdefault(path, {})[item["method"].lower()] = {
                "operationId": item["operationId"], "responses": responses}
        contract.check(self.document, self.links)

    def test_removed_status_fails(self):
        item = next(item for item in self.links["operations"] if any(case["status"] >= 400 for case in item["scenarios"]))
        response = self.document["paths"][contract.normalize(item["path"])][item["method"].lower()]["responses"]
        status = str(next(case["status"] for case in item["scenarios"] if case["status"] >= 400))
        del response[status]
        with self.assertRaisesRegex(AssertionError, "Missing OpenAPI status"):
            contract.check(self.document, self.links)

    def test_wrong_problem_media_type_fails(self):
        item = next(item for item in self.links["operations"] if any(case["status"] >= 400 for case in item["scenarios"]))
        response = self.document["paths"][contract.normalize(item["path"])][item["method"].lower()]["responses"]
        status = str(next(case["status"] for case in item["scenarios"] if case["status"] >= 400))
        response[status]["content"]["application/json"] = response[status]["content"].pop("application/problem+json")
        with self.assertRaisesRegex(AssertionError, r"application/problem\+json"):
            contract.check(self.document, self.links)

    def test_exported_status_without_acceptance_case_fails(self):
        item = self.links["operations"][0]
        operation = self.document["paths"][contract.normalize(item["path"])][item["method"].lower()]
        operation["responses"]["418"] = {"description": "Undocumented outcome"}
        with self.assertRaisesRegex(AssertionError, "OpenAPI response without acceptance case.*418"):
            contract.check(self.document, self.links)

    def test_unlinked_named_operation_fails(self):
        self.document["paths"]["/unlinked"] = {"get": {"operationId": "Unlinked", "responses": {}}}
        with self.assertRaisesRegex(AssertionError, "OpenAPI operation without acceptance links"):
            contract.check(self.document, self.links)

    def test_unnamed_non_health_operation_fails(self):
        self.document["paths"]["/unlinked"] = {"get": {"responses": {}}}
        with self.assertRaisesRegex(AssertionError, "OpenAPI operation without acceptance links"):
            contract.check(self.document, self.links)

    def test_missing_operation_fails(self):
        item = self.links["operations"][0]
        del self.document["paths"][contract.normalize(item["path"])][item["method"].lower()]
        with self.assertRaisesRegex(AssertionError, "OpenAPI operation mismatch"):
            contract.check(self.document, self.links)

    def test_duplicate_operation_fails(self):
        item = self.links["operations"][0]
        operation = self.document["paths"][contract.normalize(item["path"])][item["method"].lower()]
        self.document["paths"]["/duplicate"] = {"get": operation}
        with self.assertRaisesRegex(AssertionError, "Duplicate operationId in OpenAPI"):
            contract.check(self.document, self.links)

    def test_only_explicit_get_health_routes_are_excluded(self):
        for path in ("/health", "/health/live", "/health/ready"):
            self.document["paths"][path] = {"get": {"responses": {"200": {"description": "Healthy"}}}}
        contract.check(self.document, self.links)
        self.document["paths"]["/health/unknown"] = {"get": {"responses": {}}}
        with self.assertRaisesRegex(AssertionError, "OpenAPI operation without acceptance links"):
            contract.check(self.document, self.links)


if __name__ == "__main__":
    unittest.main()

