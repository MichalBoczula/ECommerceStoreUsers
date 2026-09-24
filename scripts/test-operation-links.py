#!/usr/bin/env python3
"""Mutation checks for operation-to-flow link validation."""

import importlib.util
import unittest
from pathlib import Path
from unittest.mock import patch


SCRIPT = Path(__file__).with_name("generate-operation-links.py")
spec = importlib.util.spec_from_file_location("operation_links", SCRIPT)
links = importlib.util.module_from_spec(spec)
spec.loader.exec_module(links)


class OperationLinksTests(unittest.TestCase):
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


if __name__ == "__main__":
    unittest.main()
