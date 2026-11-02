"""Consolidate preserved independent audit evidence; never modify art or Assets."""
import json
from collections import Counter
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261101/proportion-audit'
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'


def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))


def main():
    historical=OUT/'historical-before-repair.json'
    final=OUT/'final-matrix.json'
    if not historical.exists() and final.exists():
        historical.write_bytes(final.read_bytes())
    targeted=read(OUT/'targeted-nine.json')
    proof_path=ROOT/'outputs/retomada-20261101/selective-repair/integration/rgba-preservation-proof.json'
    proof=read(proof_path)
    mapping={item['oldId']:item for item in proof['proof']}
    reverse={item['newId']:item for item in proof['proof']}
    reviewed={item['id'] for item in targeted['frames'] if item['decision']=='APROVADA-compatible'}
    repaired={item['oldId'] for item in proof['proof'] if item['repaired']}
    if (len(reviewed)!=9 or len(repaired)!=5 or proof['unchangedNeighbors']!=19 or
            len(mapping)!=24 or not proof['allRgbaExact'] or
            any(not item['rgbaExact'] or item['nativePixelsResized'] for item in proof['proof'])):
        raise ValueError('Unexpected targeted review or whole-frame preservation evidence')
    katana=read(OUT/'katana.json');punhos=read(OUT/'punhos.json')
    corrections={f['frameId']:f for f in katana['findings']}
    pending={f['id'] for page in katana['inventory'] for f in page['frames'] if f['auditStatus']=='borderline-review'}
    for finding in punhos['findings']:
        if finding['severity']=='warning':pending.update(finding.get('frameIds',[]))
    pending-=reviewed
    unresolved={key:value for key,value in corrections.items() if key not in repaired}
    index=read(OUT/'comparison-index.json')
    selected={n for entry in index['comparisons'] for n in entry['spriteNames']}
    selected={mapping[n]['newId'] if n in mapping else n for n in selected}
    pages=[p for p in read(BANK/'catalog.json')['pages'] if p['skin']==0 and p['form']==0 and
           p['action']!='Idle' and any(n in selected for n in p['spriteNames'])]
    rows=[];frames={}
    for page in pages:
        statuses=[]
        for name in page['spriteNames']:
            preserved=reverse.get(name)
            old=preserved['oldId'] if preserved else name
            status='CORREÇÃO NECESSÁRIA' if old in unresolved else 'PENDENTE' if old in pending else 'APROVADA'
            detail={'status':status,'weapon':page['weapon'],'pageId':page['id'],'staticReviewOnly':True}
            if preserved:
                detail.update({'previousId':old,'wholeFigureReplaced':preserved['repaired'],
                               'rgbaPreservationVerified':preserved['rgbaExact'],
                               'nativePixelsResized':preserved['nativePixelsResized'],
                               'evidence':'selective-repair/integration/rgba-preservation-proof.json'})
            if old in reviewed:detail['targetedReviewEvidence']='proportion-audit/targeted-nine.json'
            frames[name]=detail;statuses.append(status)
        status='CORREÇÃO NECESSÁRIA' if 'CORREÇÃO NECESSÁRIA' in statuses else 'PENDENTE' if 'PENDENTE' in statuses else 'APROVADA'
        rows.append({'weapon':page['weapon'],'action':page.get('attackKey') or page['action'],
                     'direction':page['direction'],'pageId':page['id'],'status':status,
                     'frames':[{'id':n,**frames[n]} for n in page['spriteNames']],
                     'runtimeVerification':'separate receipt required'})
    transition=read(BANK/'ReqMotoNormalJoinSeparateS20261028V1.json')
    for frame in transition['frames']:
        frames[frame['id']]={'status':'APROVADA','weapon':3,'action':'transition','limit':'partially occluded by vehicle'}
    counts=Counter(f['status'] for f in frames.values())
    if len(frames)!=176 or unresolved or pending:
        raise ValueError('Consolidation must retain 176 figures with all five corrections and nine reviews resolved')
    result={'status':'static-audit-consolidated-after-selective-repair; motion approval separate',
            'uniquePreparedFramesInspected':len(frames),'frameClassification':dict(counts),
            'sourceAssetsModified':False,'animationMatrix':rows,'corrections':list(unresolved.values()),
            'historicalBeforeRepair':'historical-before-repair.json',
            'resolvedCorrections':[dict(finding,newFrameId=mapping[old]['newId'],
                                       resolution='whole-figure-replaced; statically reviewed',
                                       evidence='selective-repair/integration/rgba-preservation-proof.json')
                                   for old,finding in corrections.items() if old in repaired],
            'targetedReview':{'reviewed':9,'staticallyCompatible':9,'evidence':'targeted-nine.json'},
            'selectiveRepair':{'wholeFiguresReplaced':5,'unchangedNeighborsRgbaExact':19,
                               'mapping':proof['proof'],'evidence':str(proof_path.relative_to(ROOT)).replace('\\','/')},
            'classificationMeaning':'APROVADA means statically reviewed compatibility only; no motion or human approval implied',
            'targetedPendingFrameIds':sorted(pending),'blockedCandidateFigures':18,
            'blockedCandidateReason':'Two Parry sources have no final prepared scale/import contract; not approved runtime clips',
            'motionAndRuntimeAreSeparate':True,'approvedNEOriginalPreserved':True,
            'warningsDoNotAuthorizeSpeculativeRedraw':True}
    (OUT/'final-matrix.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    text='# Auditoria consolidada de fidelidade — 01/11/2026\n\n'
    text+=f"{len(frames)} figuras preparadas inspecionadas: {counts['APROVADA']} compatíveis estaticamente, {counts['CORREÇÃO NECESSÁRIA']} com correção necessária e {counts['PENDENTE']} para confirmação dirigida. Arte original intacta. Aprovação estática não certifica importação ou movimento em gameplay.\n\n"
    text+='Katana: as cinco figuras inteiras de Dodge afetadas (W, NW, NE, E, SE) foram substituídas seletivamente; 19 vizinhas permaneceram RGBA exatas, sem redimensionamento dos pixels nativos. Os 24 IDs e as páginas atuais foram conciliados com rgba-preservation-proof.json e o catálogo ativo. A NE V5 original aprovada permanece preservada como fonte histórica.\n\n'
    text+='As nove pendências dirigidas foram revistas como estaticamente compatíveis em targeted-nine.json: oito figuras de Punhos e a pose baixa SW da Katana, preservadas sem redesenho especulativo. Moto: 68 figuras sem alongamento inequívoco; a transição permanece naturalmente ocluída pelo veículo. A revisão estática não emite nova aprovação humana ou aprovação do movimento.\n\n'
    text+='Dezoito figuras Parry candidatas permanecem bloqueadas fora do runtime por falta de contrato final de preparo/escala. O retrato anterior está preservado em historical-before-repair.json; não confundir os antigos cinco reparos e nove avisos com pendências atuais. Testes, importação e movimento exigem seus próprios recibos.\n'
    (OUT/'REPORT.md').write_text(text,encoding='utf-8')
    print(json.dumps({'inspected':len(frames),'classification':dict(counts),'artModified':False},ensure_ascii=False))


if __name__=='__main__':main()
