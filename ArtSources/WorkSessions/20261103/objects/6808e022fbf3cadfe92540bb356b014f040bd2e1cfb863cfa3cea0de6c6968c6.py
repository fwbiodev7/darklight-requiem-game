import concurrent.futures
import hashlib
import json
import os
from pathlib import Path
import subprocess
import time
import urllib.parse
import urllib.request
import zlib

BASE = Path(__file__).resolve().parent
REPO = BASE / 'darklight-requiem-game'
TREE = json.loads((BASE / 'tree-api.json').read_text(encoding='utf-8'))
FILES = {x['sha']: x for x in TREE['tree'] if x['type'] == 'blob'}
env = dict(os.environ, GIT_NO_LAZY_FETCH='1')
check = subprocess.run(['git', '-C', str(REPO), 'cat-file', '--batch-check=%(objectname) %(objecttype)'],
                       input=('\n'.join(FILES) + '\n').encode(), capture_output=True, check=True, env=env)
missing = [FILES[line.split()[0]] for line in check.stdout.decode().splitlines() if line.endswith(' missing')]
print(json.dumps({'missing': len(missing), 'MiB': round(sum(x.get('size', 0) for x in missing)/1048576, 1)}), flush=True)
SHA = '852e612cd38d12845c2bc0134e9c1af4fb610d26'

def fetch(item):
    url = f'https://raw.githubusercontent.com/fwbiodev7/darklight-requiem-game/{SHA}/' + urllib.parse.quote(item['path'])
    for attempt in range(4):
        try:
            with urllib.request.urlopen(url, timeout=90) as response:
                data = response.read()
            raw = f'blob {len(data)}\0'.encode() + data
            if hashlib.sha1(raw).hexdigest() != item['sha']:
                raise ValueError('Git blob hash mismatch: ' + item['path'])
            dest = REPO / '.git' / 'objects' / item['sha'][:2] / item['sha'][2:]
            dest.parent.mkdir(parents=True, exist_ok=True)
            tmp = dest.with_suffix('.download')
            tmp.write_bytes(zlib.compress(raw))
            tmp.replace(dest)
            return len(data)
        except Exception:
            if attempt == 3:
                raise
            time.sleep(attempt + 1)

done = 0
total = 0
errors = []
with concurrent.futures.ThreadPoolExecutor(max_workers=16) as pool:
    jobs = {pool.submit(fetch, item): item for item in missing}
    for future in concurrent.futures.as_completed(jobs):
        done += 1
        try:
            total += future.result()
        except Exception as error:
            errors.append({'path': jobs[future]['path'], 'error': str(error)})
        if done % 100 == 0 or done == len(jobs):
            print(json.dumps({'done': done, 'of': len(jobs), 'MiB': round(total/1048576, 1), 'errors': len(errors)}), flush=True)
(BASE / 'download-result.json').write_text(json.dumps({'commit': SHA, 'downloaded': done-len(errors), 'bytes': total, 'errors': errors}, indent=2), encoding='utf-8')
if errors:
    raise SystemExit(1)
subprocess.run(['git', '-C', str(REPO), 'restore', '--source=HEAD', '--staged', '--worktree', '.'], check=True, env=env)
print('Checkout complete; every downloaded file verified against its Git blob SHA.', flush=True)
