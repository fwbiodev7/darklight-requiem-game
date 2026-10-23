"""Extend the existing Requiem-rest skin palette to all inventoried player sources.

Writes color metadata only. Never rewrites a source PNG or sprite rectangle.
"""
import json
from pathlib import Path
import sys

import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
from prepare_complexion import median


def main():
    inventory=json.loads((ROOT/'outputs/olhos-todas-formas-20261020/INVENTARIO.json').read_text())
    path=ROOT/'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json'
    before=json.loads(path.read_text())
    target=np.array(before['referenceMedian'],dtype=float)
    rows={r['texture']:r for r in before['entries']}
    updated,failures=[],[]
    for source in inventory['sources']:
        png=ROOT/'Assets/Darklight/Resources'/(source['resource']+'.png')
        try:
            actual=median(png)
        except ValueError as exc:
            failures.append(dict(texture=source['texture'],reason=str(exc)))
            continue
        shift=(target-actual)/255
        if max(abs(shift))>.30:
            failures.append(dict(texture=source['texture'],reason='Needs visual palette reconstruction; shift exceeds .30',
                                 sourceMedian=actual.tolist()))
            continue
        rows[source['texture']]=dict(texture=source['texture'],shift=shift.tolist(),sourceMedian=actual.tolist())
        updated.append(source['texture'])
    output=dict(before,referenceRole='Approved neutral/rest Requiem complexion; one shared skin base for both player skins and every form',
                scope='inventoried complete player sources, all weapons/forms/states; original PNGs and protected geometry unchanged',
                entries=sorted(rows.values(),key=lambda r:r['texture']))
    out=ROOT/'outputs/olhos-todas-formas-20261020'
    (out/'CanonicalComplexion-complete.json').write_text(json.dumps(output,indent=2)+'\n')
    report=dict(referenceMedian=target.tolist(),previousEntries=len(before['entries']),
                inventoriedTextures=inventory['textures'],coveredTextures=len(updated),
                failures=failures,sourcePixelsModified=False,productionApplied=False)
    (out/'QA-PELE.json').write_text(json.dumps(report,indent=2)+'\n')
    print(json.dumps(report))
    return 0 if not failures else 2


if __name__=='__main__':
    raise SystemExit(main())
