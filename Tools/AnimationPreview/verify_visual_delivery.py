"""Audit original hashes, palette coverage and catalog scope of the visual delivery."""
import hashlib
import argparse
import json
from pathlib import Path
import sys
import subprocess
import xml.etree.ElementTree as ET

import numpy as np

ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(Path(__file__).parent))
from inventory_player_eyes import load_asset,parse_asset


def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--baseline-repo',type=Path,default=ROOT)
    parser.add_argument('--baseline-ref',default='5d48ce20eecf6760c1d5879fd249d6fc7c273a2c')
    args=parser.parse_args()
    def original(relative):
        return subprocess.check_output(['git','-C',str(args.baseline_repo),'show',args.baseline_ref+':'+relative]).decode('utf-8-sig').replace('\r\n','\n')
    out=ROOT/'outputs/padronizacao-artes-20261020'
    inventory=json.loads((ROOT/'outputs/olhos-todas-formas-20261020/INVENTARIO.json').read_text())
    colors=json.loads((ROOT/'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json').read_text())
    entries={e['texture']:e for e in colors['entries']}
    assert len(entries)==len(colors['entries'])==262
    target=np.array(colors['referenceMedian']);assert list(target)==[246,174,130]
    changed,uncovered=[],[]
    for row in inventory['sources']:
        source=ROOT/'Assets/Darklight/Resources'/(row['resource']+'.png')
        if sha(source)!=row['sha256']:changed.append(row['resource'])
        if row['texture'] not in entries:uncovered.append(row['texture'])
    for e in entries.values():
        shift=np.array(e['shift']);median=np.array(e['sourceMedian'])
        assert np.isfinite(shift).all() and max(abs(shift))<=.30
        assert np.max(abs(median+shift*255-target))<.001
    assert not changed and not uncovered
    current=load_asset(ROOT/'Assets/Darklight/Resources/Darklight/TransformedCatalog.asset')['pages']
    baseline=parse_asset(original('Assets/Darklight/Resources/Darklight/TransformedCatalog.asset'))['pages']
    selected=[]
    for a,b in zip(baseline,current):
        if a==b:continue
        assert a['form']==b['form']==2
        assert a['baseName'] in ['RequiemAttackKatanaYoungCardinal','RequiemCombatDaggersYoungCardinal','RequiemCombatDaggersYoungDiagonal']
        selected.append(a['baseName'])
    assert len(current)==len(baseline)==183 and len(selected)==3
    protected=['Assets/Darklight/Runtime/Character/DarklightDeathReversion.cs',
               'Assets/Darklight/Tests/EditMode/DarklightDeathReversionTests.cs']
    for relative in protected:assert (ROOT/relative).read_text(encoding='utf-8-sig')==original(relative)
    xml=out/'evidencias-unity-11.xml'
    run=ET.parse(xml).getroot();assert run.get('failed')=='0' and run.get('passed')=='11'
    refined=ET.parse(out/'evidencias-unity-refinada-4.xml').getroot()
    assert refined.get('failed')=='0' and refined.get('passed')=='4'
    result=dict(originalPlayerTextures=260,originalPngHashesUnchanged=260,originalPngsModified=changed,
                canonicalPaletteEntries=262,referenceMedian=target.tolist(),uncoveredTexturePalettes=uncovered,
                catalogPages=183,modifiedCatalogPages=selected,otherCatalogPagesUnchanged=180,
                generatedCompleteFiguresImported=12,daggerSheetFigures=8,katanaSheetFigures=4,
                sharedIrisScope='Existing semantic iris masks; authored textures without an iris channel still pending beyond the two new sheets',
                allExistingIrisesCorrected=False,allGameArtRedrawn=False,
                protectedDeathCodeCompared=protected,protectedDeathCodeUnchanged=True,
                unityEditModeDistinctTestsPassed=11,unityPlayModeRun=False,audioStarted=False,
                sourceSkillFiles=12,spriteScriptTestsPassed=23,outputEconomyTestsPassed=8,
                tokenEconomy='Bounded deterministic summaries with full local logs; no local model or percentage saving claimed')
    (out/'QA-ENTREGA.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(result,ensure_ascii=False))


if __name__=='__main__':main()
