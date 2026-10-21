"""Exercise bounded evidence, failures and stale XML handling."""
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


RUNNER = Path(__file__).with_name('run_compact.py')


class CompactTests(unittest.TestCase):
    def run_case(self, root, code, *options):
        r = subprocess.run([sys.executable, str(RUNNER), '--label', 'test', '--log-dir', str(root),
                            *options, '--', sys.executable, '-c', code], capture_output=True, text=True)
        return r.returncode, json.loads(r.stdout)

    def test_large_log_keeps_failure_and_full_source(self):
        with tempfile.TemporaryDirectory() as temp:
            code, d = self.run_case(temp, "print('ordinary output\\n'*10000);print('ERROR exact diagnostic');raise SystemExit(7)")
            self.assertEqual(code, 7)
            self.assertFalse(d['ok'])
            self.assertGreater(d['logBytes'], 100000)
            self.assertIn('ERROR exact diagnostic', d['evidence'][0]['text'])
            self.assertIn('ordinary output', Path(d['log']).read_text())
            self.assertLess(len(json.dumps(d)), 6000)

    def test_success_with_stderr_warning_preserves_exit(self):
        with tempfile.TemporaryDirectory() as temp:
            code, d = self.run_case(temp, "import sys;print('warning: inspect',file=sys.stderr)")
            self.assertEqual(code, 0)
            self.assertTrue(d['ok'])
            self.assertEqual(d['signalChunks'], 1)

    def test_timeout(self):
        with tempfile.TemporaryDirectory() as temp:
            code, d = self.run_case(temp, 'import time;time.sleep(8)', '--timeout', '0.2')
            self.assertEqual(code, 124)
            self.assertTrue(d['timedOut'])

    def test_stale_test_xml_refused(self):
        with tempfile.TemporaryDirectory() as temp:
            xml = Path(temp) / 'tests.xml'
            xml.write_text('<testsuite tests="1"><testcase name="ok"/></testsuite>')
            code, d = self.run_case(temp, "print('done')", '--xml', str(xml))
            self.assertEqual(code, 2)
            self.assertIn('unchanged', d['testXmlError'])

    def test_xml_failure_overrides_process_zero(self):
        with tempfile.TemporaryDirectory() as temp:
            xml = Path(temp) / 'tests.xml'
            script = f"from pathlib import Path;Path({str(xml)!r}).write_text('<testsuite><testcase name=\"bad\"><failure/></testcase></testsuite>')"
            code, d = self.run_case(temp, script, '--xml', str(xml))
            self.assertEqual(code, 2)
            self.assertFalse(d['ok'])
            self.assertEqual(d['testXml']['failedCases'], ['bad'])

    def test_long_unterminated_line(self):
        with tempfile.TemporaryDirectory() as temp:
            code, d = self.run_case(temp, "import sys;sys.stdout.write('x'*1000000+' ERROR tail')")
            self.assertEqual(code, 0)
            self.assertEqual(d['logBytes'], 1000011)
            self.assertLess(len(json.dumps(d)), 6000)

    def test_empty_test_xml_is_not_a_pass(self):
        with tempfile.TemporaryDirectory() as temp:
            xml = Path(temp) / 'tests.xml'
            script = f"from pathlib import Path;Path({str(xml)!r}).write_text('<testsuite tests=\"0\"/>')"
            code, d = self.run_case(temp, script, '--xml', str(xml))
            self.assertEqual(code, 2)
            self.assertEqual(d['testXml']['executedCases'], 0)

    def test_all_skipped_is_not_executed(self):
        with tempfile.TemporaryDirectory() as temp:
            xml = Path(temp) / 'tests.xml'
            script = f"from pathlib import Path;Path({str(xml)!r}).write_text('<testsuite><testcase name=\"skip\"><skipped/></testcase></testsuite>')"
            code, d = self.run_case(temp, script, '--xml', str(xml))
            self.assertEqual(code, 2)
            self.assertEqual(d['testXml']['executedCases'], 0)


if __name__ == '__main__':
    unittest.main()
