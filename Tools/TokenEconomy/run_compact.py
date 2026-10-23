"""Run argv without a shell; preserve complete output locally, emit bounded evidence."""
import argparse
from collections import deque
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import time
import uuid
import xml.etree.ElementTree as ET


SIGNAL = re.compile(r'\b(error|exception|failed|failure|fatal|traceback|assertionerror|warning)\b', re.I)


def summarize(path, limit=8, width=500):
    """Bound memory even for a tool emitting a single huge line."""
    evidence, tail, hits, chunks = [], deque(maxlen=4), 0, 0
    sha = hashlib.sha256()
    with path.open('rb') as stream:
        while True:
            raw = stream.readline(65536)
            if not raw:
                break
            sha.update(raw)
            chunks += 1
            line = raw.decode('utf-8', errors='replace').strip()
            if line:
                tail.append(line[:width])
            if SIGNAL.search(line):
                hits += 1
                if len(evidence) < limit:
                    evidence.append(dict(chunk=chunks, text=line[:width], truncated=len(line) > width))
    return dict(logBytes=path.stat().st_size, logSha256=sha.hexdigest(),
                signalChunks=hits, evidence=evidence, tail=list(tail))


def xml_result(path):
    root = ET.parse(path).getroot()
    attrs = root.attrib
    cases = list(root.iter('test-case')) + list(root.iter('testcase'))
    failed = []
    executed = 0
    for case in cases:
        skipped = (case.get('result', '').lower() in ('skipped', 'ignored', 'notrun') or
                   any(child.tag == 'skipped' for child in case))
        if not skipped:
            executed += 1
        if case.get('result', '').lower() in ('failed', 'failure', 'error') or any(
                child.tag in ('failure', 'error') for child in case):
            failed.append(case.get('fullname', case.get('name', 'unnamed')))
    declared = sum(int(attrs.get(k, 0)) for k in ('failed', 'failures', 'errors'))
    root_failed = attrs.get('result', '').lower() in ('failed', 'failure', 'error')
    return dict(path=str(path), cases=len(cases), executedCases=executed, declaredFailures=declared,
                failedCases=failed[:8], ok=executed > 0 and not failed and not declared and not root_failed,
                note='Counts are evidence in this XML; exit 0 alone does not prove tests ran.')


def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--label', required=True, help='Short purpose of the command')
    p.add_argument('--log-dir', type=Path, default=Path(__file__).resolve().parents[3] / 'work' / 'compact')
    p.add_argument('--timeout', type=float, default=180)
    p.add_argument('--xml', type=Path, help='Optional NUnit/JUnit result; must be newly generated')
    p.add_argument('--evidence', type=int, default=8)
    p.add_argument('command', nargs=argparse.REMAINDER)
    a = p.parse_args()
    argv = a.command[1:] if a.command[:1] == ['--'] else a.command
    if not argv or a.timeout <= 0 or not 0 <= a.evidence <= 30:
        p.error('Provide a command, positive timeout, and evidence count 0..30')
    a.log_dir.mkdir(parents=True, exist_ok=True)
    stamp = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ') + '-' + uuid.uuid4().hex[:8]
    label = re.sub(r'[^a-zA-Z0-9_-]+', '-', a.label).strip('-')[:60] or 'command'
    log = a.log_dir / f'{stamp}-{label}.log'
    started = time.time()
    timed_out, launch_error, code = False, None, None
    # Only accept XML created/changed by this invocation, not a stale green result.
    xml_before = (a.xml.stat().st_mtime_ns, a.xml.stat().st_size) if a.xml and a.xml.exists() else None
    with log.open('xb') as output:
        try:
            process = subprocess.Popen(argv, stdout=output, stderr=subprocess.STDOUT, shell=False)
            try:
                code = process.wait(timeout=a.timeout)
            except subprocess.TimeoutExpired:
                timed_out = True
                if sys.platform == 'win32':
                    subprocess.run(['taskkill', '/PID', str(process.pid), '/T', '/F'],
                                   stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
                if process.poll() is None:
                    process.kill()
                process.wait()
                code = 124
        except OSError as exc:
            launch_error = str(exc)
            output.write(launch_error.encode('utf-8', errors='replace'))
            code = 127
    summary = dict(label=a.label, ok=code == 0, exitCode=code, timedOut=timed_out,
                   elapsedSeconds=round(time.time() - started, 3), log=str(log.resolve()),
                   summaryMode='deterministic-local-no-model', **summarize(log, a.evidence))
    if launch_error:
        summary['launchError'] = launch_error
    if a.xml:
        try:
            after = (a.xml.stat().st_mtime_ns, a.xml.stat().st_size)
            if after == xml_before:
                raise ValueError('XML unchanged: refusing stale test evidence')
            summary['testXml'] = xml_result(a.xml)
            summary['ok'] = summary['ok'] and summary['testXml']['ok']
        except (OSError, ValueError, ET.ParseError) as exc:
            summary['testXmlError'] = str(exc)
            summary['ok'] = False
    # Success output remains useful even if no regex signal was detected.
    if code == 0 and not summary['evidence']:
        summary['resultTail'] = summary.pop('tail')
    elif code != 0:
        summary['failureTail'] = summary.pop('tail')
    else:
        summary.pop('tail')
    summary_path = log.with_suffix('.json')
    summary['summaryFile'] = str(summary_path.resolve())
    summary_path.write_text(json.dumps(summary, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(summary, ensure_ascii=False))
    return 0 if summary['ok'] else (code if code and 0 < code < 256 else 2)


if __name__ == '__main__':
    raise SystemExit(main())
