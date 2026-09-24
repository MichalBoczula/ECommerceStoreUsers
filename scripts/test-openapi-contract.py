#!/usr/bin/env python3
"""Prove that a removed status or changed error media type breaks the gate."""

import importlib.util
import unittest
from pathlib import Path


spec = importlib.util.spec_from_file_location("contract", Path(__file__).with_name("check-openapi-contract.py"))
contract = importlib.util.module_from_spec(spec)
spec.loader.exec_module(contract)


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


if __name__ == "__main__":
    unittest.main()
