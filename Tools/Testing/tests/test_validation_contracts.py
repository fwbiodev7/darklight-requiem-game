"""Regression evidence for tooling false-positive cases found during handoff review."""
import contextlib
import importlib.util
import io
from pathlib import Path
import subprocess
import shutil
import uuid
import time
import os
import unittest
from unittest.mock import patch

ROOT=Path(__file__).resolve().parents[2]
FIXTURES=ROOT.parent/"outputs/tooling-fixtures"
@contextlib.contextmanager
def fixture_directory():
    target=FIXTURES/uuid.uuid4().hex
    target.mkdir(mode=0o777,parents=True)
    try:yield str(target)
    finally:
        assert target.resolve().is_relative_to(FIXTURES.resolve())
        shutil.rmtree(target)

def module(name,relative):
    spec=importlib.util.spec_from_file_location(name,ROOT/relative)
    value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
runner=module("unity_runner","Testing/run_tests.py")
metadata=module("metadata_audit","Validation/audit_unity_metadata.py")
handoff=module("handoff_audit","Handoff/verify_integration.py")

class ValidationContracts(unittest.TestCase):
    def probe(self,xml,exit_code=0,stale=False,timeout=False):
        with fixture_directory() as directory:
            project=Path(directory);(project/"ProjectSettings").mkdir()
            (project/"ProjectSettings/ProjectVersion.txt").write_text("6000.6.0f1")
            def execute(command,timeout):
                if timeout is None:raise AssertionError("Runner must bound Unity execution")
                if xml is not None:
                    target=Path(command[command.index("-testResults")+1]);target.write_text(xml)
                    if stale:os.utime(target,(time.time()-60,time.time()-60))
                return subprocess.CompletedProcess(command,exit_code)
            with patch.object(runner,"find_unity_binary",return_value="Unity.exe"),patch.object(runner,"check_concurrent_lock",return_value=False),patch.object(runner.subprocess,"run",side_effect=subprocess.TimeoutExpired("Unity",1) if timeout else execute),contextlib.redirect_stdout(io.StringIO()):
                return runner.run_tests(project_path=directory,timeout_sec=1)
    def test_nonzero_unity_exit_cannot_pass_with_green_xml(self):
        result=self.probe('<test-run result="Passed" passed="1"><test-case result="Passed"/></test-run>',exit_code=9)
        self.assertEqual(result["status"],"UNITY_EXIT_FAILURE")
    def test_zero_cases_cannot_pass(self):
        self.assertEqual(self.probe('<test-run result="Passed" passed="0"/>')["status"],"NO_EXECUTED_TESTS")
    def test_legacy_nunit_xml_cannot_pass(self):
        self.assertEqual(self.probe('<test-results result="Passed" total="1" passed="1"><test-case result="Passed"/></test-results>')["status"],"XML_SCHEMA_ERROR")
    def test_root_counts_must_match_test_cases(self):
        self.assertEqual(self.probe('<test-run result="Passed" passed="0"><test-case result="Passed"/></test-run>')["status"],"XML_INCONSISTENT")
    def test_stale_results_cannot_pass(self):
        self.assertEqual(self.probe('<test-run result="Passed" passed="1"><test-case result="Passed"/></test-run>',stale=True)["status"],"STALE_XML")
    def test_absent_and_malformed_results_cannot_pass(self):
        self.assertNotEqual(self.probe(None)["status"],"PASSED")
        self.assertEqual(self.probe("broken xml")["status"],"XML_PARSE_ERROR")
    def test_failed_child_cannot_hide_behind_zero_root_count(self):
        self.assertEqual(self.probe('<test-run result="Passed" failed="0"><test-case result="Failed"/></test-run>')["status"],"TESTS_FAILED")
    def test_timeout_is_not_an_empty_success(self):
        self.assertEqual(self.probe(None,timeout=True)["status"],"TIMEOUT")
    def test_actual_green_case_remains_accepted(self):
        self.assertEqual(self.probe('<test-run result="Passed" passed="1"><test-case result="Passed"/></test-run>')["status"],"PASSED")
    def test_missing_meta_increments_reported_errors(self):
        with fixture_directory() as folder:
            assets=Path(folder,"Assets");assets.mkdir();(assets/"unimported.txt").write_text("asset")
            result=metadata.audit_metadata(folder)
            self.assertGreater(result["summary"]["errors"],0)
            self.assertIn("Assets/unimported.txt",result["missing_meta"])
    def test_duplicate_guid_is_a_reported_error(self):
        with fixture_directory() as folder:
            assets=Path(folder,"Assets");assets.mkdir()
            for name in ("a","b"):
                (assets/(name+".txt")).write_text(name)
                (assets/(name+".txt.meta")).write_text("guid: "+"a"*32)
            result=metadata.audit_metadata(folder)
            self.assertEqual(len(result["duplicate_guids"]),1)
            self.assertEqual(result["summary"]["errors"],1)
    def test_git_failures_are_not_silent_empty_manifests(self):
        with patch.object(handoff.subprocess,"check_output",side_effect=subprocess.CalledProcessError(128,"git")):
            with self.assertRaises(subprocess.CalledProcessError):handoff.verify_integration()

if __name__=="__main__":unittest.main()
