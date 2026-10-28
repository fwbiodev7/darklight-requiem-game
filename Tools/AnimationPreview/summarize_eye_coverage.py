"""Compact diagnostics for eye-region coverage; no source mutation."""
from collections import Counter
import json
from pathlib import Path
import sys


def main():
    root = Path(sys.argv[1])
    no_iris_directions, states = Counter(), Counter()
    sheets = []
    for path in root.glob('*.json'):
        if path.name == 'summary.json':
            continue
        d = json.loads(path.read_text(encoding='utf-8-sig'))
        n = 0
        for frame in d['frames']:
            states[frame['status']] += 1
            if frame['status'] == 'no-iris-proposal':
                no_iris_directions[str(frame.get('direction'))] += 1
                n += 1
        sheets.append((n,path.stem))
    print(json.dumps(dict(states=dict(states),noIrisDirections=dict(no_iris_directions),
                          largestNoIris=sorted(sheets,reverse=True)[:12])))


if __name__ == '__main__':
    main()
