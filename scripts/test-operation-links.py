#!/usr/bin/env python3
"""Mutation checks for operation-to-flow link validation."""

import importlib.util
import copy
import csv
import unittest
from pathlib import Path
from unittest.mock import patch


SCRIPT = Path(__file__).with_name("generate-operation-links.py")
spec = importlib.util.spec_from_file_location("operation_links", SCRIPT)
links = importlib.util.module_from_spec(spec)
spec.loader.exec_module(links)


class OperationLinksTests(unittest.TestCase):
    def change_source(self, name, transform):
        original = links.source
        return patch.object(links, "source", side_effect=lambda path: transform(original(path)) if path.name == name else original(path))

    def corrupt_endpoint(self, before, after):
        original = links.source

        def changed(path):
            code = original(path)
            if path.name == ("MobilePhonesEndpoints.cs" if links.PRODUCTS else "CustomersEndpoints.cs"):
                self.assertIn(before, code)
                return code.replace(before, after, 1)
            return code

        return patch.object(links, "source", side_effect=changed)

    def test_duplicate_operation_id_fails(self):
        before = '.WithName("GetMobilePhoneById")' if links.PRODUCTS else '.WithName("CreateCustomer")'
        after = '.WithName("GetMobilePhonesByIds")' if links.PRODUCTS else '.WithName("AddCompany")'
        with self.corrupt_endpoint(before, after), self.assertRaisesRegex(AssertionError, "Duplicate operationId"):
            links.generate()

    def test_missing_executed_flow_fails(self):
        before = 'new GetMobilePhoneByIdQuery(' if links.PRODUCTS else 'customerService.CreateCustomer('
        after = 'new MissingQuery(' if links.PRODUCTS else 'customerService.MissingMethod('
        with self.corrupt_endpoint(before, after), self.assertRaisesRegex(AssertionError, "No described flow"):
            links.generate()

    def test_missing_step_meaning_fails(self):
        import re
        with self.change_source("CreateCustomerDescriptor.cs", lambda code: re.sub(r'description: "(?:[^"\\]|\\.)*"', 'description: ""', code, count=1)), \
                self.assertRaisesRegex(AssertionError, "Missing step identity/meaning"):
            links.generate()

    def test_duplicate_rule_identity_fails(self):
        with self.change_source("AddressCityValidationRule.cs", lambda code: code.replace("users.rule.address-city", "users.rule.address-street")), \
                self.assertRaisesRegex(AssertionError, "Duplicate stable documentation ID"):
            links.generate()

    def test_outcome_status_must_match_scenario(self):
        with self.change_source("CreateCustomerDescriptor.cs", lambda code: code.replace("Status = 200", "Status = 201", 1)), \
                self.assertRaisesRegex(AssertionError, "Branch status differs"):
            links.generate()

    def test_uninstalled_rule_cannot_be_declared_by_invoked_branch(self):
        with self.change_source("CreateAdminDescriptor.cs", lambda code: code.replace('"users.rule.admin-email",', '"users.rule.customer-company-tax-id", "users.rule.admin-email",')), \
                self.assertRaisesRegex(AssertionError, "Branch uses uninstalled/unrelated rules"):
            links.generate()

    def test_branch_from_another_flow_fails(self):
        with (links.ROOT / "docs/acceptance-matrix.tsv").open(newline="") as stream:
            rows = list(csv.DictReader(stream, delimiter="\t"))
        changed = copy.deepcopy(rows)
        next(row for row in changed if row["scenarioId"] == "REF07_CreateCustomer_04")["branchId"] = "users.branch.create-admin.saved"
        with patch.object(links.csv, "DictReader", return_value=changed), self.assertRaisesRegex(AssertionError, "Branch belongs to another flow"):
            links.generate()

    def test_rule_expected_by_scenario_must_belong_to_its_branch(self):
        with (links.ROOT / "docs/acceptance-matrix.tsv").open(newline="") as stream:
            rows = list(csv.DictReader(stream, delimiter="\t"))
        changed = copy.deepcopy(rows)
        next(row for row in changed if row["scenarioId"] == "REF07_CreateCustomer_04")["ruleIds"] = "users.rule.customer-company-tax-id"
        with patch.object(links.csv, "DictReader", return_value=changed), self.assertRaisesRegex(AssertionError, "Orphan/incorrect scenario rule"):
            links.generate()


if __name__ == "__main__":
    unittest.main()

