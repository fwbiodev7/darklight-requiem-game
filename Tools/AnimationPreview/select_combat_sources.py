"""Compose the preview selection from intact sources and manually registered guards."""
from pathlib import Path
import json
from PIL import Image
from whole_figure_components import identify

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/fidelidade-original-20261018'
PACK=OUT/'pacote-v6'
FORMS=['normal','vermelha','roxa','branca']
TARGET={'Requiem':266,'Rock':278}
REFERENCE_HEAD={'Requiem':77/266,'Rock':82/278}
# Crown and full boot-outline marks; no weapon bounding box sets body scale.
GUARDS={
 ('Requiem','katana'):[(12,80,230),(12,80,231),(23,87,233),(11,80,231)],
 ('Requiem','adagas'):[(13,84,245),(13,84,247),(13,84,246),(14,84,246)],
 ('Requiem','corrente'):[(34,103,257),(30,101,258),(37,104,259),(31,102,258)],
 ('Rock','katana'):[(13,81,240),(11,81,241),(12,81,241),(13,81,241)],
 ('Rock','adagas'):[(18,89,255),(15,87,256),(17,88,257),(18,89,259)],
 ('Rock','corrente'):[(41,112,277),(40,111,278),(39,111,278),(41,112,278)],
 ('Rock','cavaleiro'):[(43,116,270),(35,112,270),(41,115,271),(42,115,273)]
}

def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def remap_events(events, selected):
    """Map events to the surviving complete poses, preserving each real impact."""
    mapping={original:i for i,original in enumerate(selected)}
    result=[]
    for e in events:
        candidates=[i for i in selected if e['start']<=i<=e['end']]
        if not candidates:raise ValueError(('No selected phase for event',e))
        impact=e['impact'] if e['impact'] in mapping else min(candidates,key=lambda i:abs(i-e['impact']))
        result.append(dict(e,start=mapping[candidates[0]],end=mapping[candidates[-1]],impact=mapping[impact]))
    return result

def make_plan():
    assets=[]
    audit=read(OUT/'auditoria-frontais-grandes-20261018.json')
    for record in read(PACK/'FONTES-CAMINHADA-GRANDE.json'):
        checked=next(a for a in audit['assets'] if a['skin']==record['skin'] and a['form']==record['form'])
        cal=dict(record.get('calibration',checked['calibration']))
        cal.update(targetBodyHeightPx=TARGET[record['skin']],referenceHeadRatio=REFERENCE_HEAD[record['skin']],
                   notes=cal.get('notes','')+' Original full boot outline: Requiem y290, Rock y612. Manual marks approximate, not approval.')
        assets.append(dict(file=record['file'],skin=record['skin'],form=record['form'],weapon='katana',
            kind='walk',rows=2,columns=4,direction='S',frameOrder=[0,6,3,5,1,7,4,2],
            idleFrame=3,durationsMs=[105,90,85,105,105,90,85,105],calibration=cal))
    timing=read(PACK/'tempos-combos.json')
    index_maps={}
    combo_records=read(PACK/'FONTES-COMBOS.json')
    files_by_key={r['id']:r['file'] for r in combo_records}
    for record in combo_records:
        skin,weapon,_,form=record['id'].split('-')
        file=record['file'];path=PACK/file
        image=Image.open(path).convert('RGBA')
        try:
            figures,_=identify(image,6,4,650)
            rows,columns,merged=6,4,False
        except ValueError:
            figures,_=identify(image,1,23,650)
            rows,columns,merged=1,23,True
        if merged:
            by_grid={}
            for f in figures:
                x0,y0,x1,y1=f['grid']
                grid=int(((y0+y1)/2)//(image.height/6))*4+int(((x0+x1)/2)//(image.width/4))
                if grid in by_grid:raise ValueError(('Duplicate whole figure mapping',file,grid))
                by_grid[grid]=f['index']
        else:by_grid={i:i for i in range(24)}
        index_maps[file]=by_grid
        omitted=[]
        if skin=='Requiem' and weapon=='corrente':omitted=[1,15,17,18]
        if weapon=='cavaleiro':omitted=[9,20]+([8] if form=='branca' else [])
        omitted=sorted(set(omitted+record.get('excludedOriginalPoses',[])))
        original=[i for i in range(24) if i not in omitted]
        if not all(i in by_grid for i in original):raise ValueError(('Missing selected source',file,by_grid,original))
        crown,chin,sole=GUARDS[(skin,weapon)][FORMS.index(form)]
        cal=dict(bodyHeightPx=sole-crown,targetBodyHeightPx=TARGET[skin],headHeightPx=chin-crown,
                 referenceHeadRatio=REFERENCE_HEAD[skin],guardFrame=by_grid[0],referenceFile='../referencia-original.png',
                 notes='Manual guard crown/chin/boot marks, excluding weapon and glow. Whole-sheet scale, no fit to weapon. These remain visual candidates.')
        cal.update(record.get('calibration',{}))
        asset=dict(file=file,skin=skin,weapon=weapon,form=form,kind='attack',rows=rows,columns=columns,
                   frameOrder=[by_grid[i] for i in original],originalPoseOrder=original,
                   calibration=cal,restMs=420,excludedOriginalPoses=omitted)
        if weapon=='adagas' and form=='normal':
            asset['anchorExclusions']={'7':[1]}
            asset['anchorExclusionReason']='Visual audit: second dagger is occluded in pose7; the second red component is an eye, not a weapon.'
        if weapon=='cavaleiro':
            melee=[i for i in original if i<12];moto=[i for i in original if i>=12]
            melee_times=[70,32,26,38,24,38,24,65,30,45,65,90]
            moto_times=[90,75,85,65,38,27,26,38,40,62,65,90]
            events=[dict(start=2,end=3,impact=3,power=.8,kind='serra',sign=1),
                    dict(start=4,end=5,impact=5,power=.9,kind='serra',sign=-1),
                    dict(start=6,end=8,impact=7,power=1.25,kind='serra',sign=1)]
            me=remap_events(events,melee)
            local={orig:i for i,orig in enumerate(original)}
            # Variant event indices are local to each variant's timeline.
            mot=remap_events([
                dict(start=16,end=17,impact=16,power=.7,kind='launch',sign=1),
                dict(start=17,end=19,impact=18,power=1.1,kind='charge',sign=1),
                dict(start=19,end=21,impact=21,power=.8,kind='brake',sign=-1)],moto)
            asset.update(durationsMs=[melee_times[i] if i<12 else moto_times[i-12] for i in original],
                         events=me,activePhases=[e['impact'] for e in me],
                         variants=dict(melee=dict(indices=[local[i] for i in melee],durationsMs=[melee_times[i] for i in melee],events=me,
                                                  activePhases=[e['impact'] for e in me]),
                                       moto=dict(indices=[local[i] for i in moto],durationsMs=[moto_times[i-12] for i in moto],events=mot,
                                                 activePhases=[e['impact'] for e in mot])))
        else:
            t=timing[weapon]
            events=remap_events(t['events'],original)
            asset.update(durationsMs=[t['durationsMs'][i] for i in original],events=events,activePhases=[e['impact'] for e in events])
        if form!='normal':
            normal=files_by_key[skin+'-'+weapon+'-combo-normal']
            asset['anchorSource']=normal
            normal_map=index_maps[normal]
            asset['anchorFrameMap']={str(by_grid[i]):normal_map[i] for i in original}
        assets.append(asset)
    plan=dict(revision=7,includeLegacyWalk=False,reference=dict(file='../referencia-original.png',
              Requiem=dict(crownY=24,soleY=290,bodyHeightPx=266),
              Rock=dict(crownY=334,soleY=612,bodyHeightPx=278),
              method='Manual full outline marks, approximate. Never measure weapon extents.'),
              scope='Frontal candidate walks, real neutral poses, frontal weapon-specific combos. Other directions remain under revision.',
              productionImported=False,visualApproval=False,assets=assets)
    (PACK/'fontes.json').write_text(json.dumps(plan,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(dict(assets=len(assets),walkSources=sum(a['kind']=='walk' for a in assets),attackSources=sum(a['kind']=='attack' for a in assets))))

if __name__=='__main__':make_plan()
