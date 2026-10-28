"""Import approved intact whole figures, without assembling or altering source PNGs."""
from pathlib import Path
import hashlib,json,shutil,sys
import numpy as np
from PIL import Image
from whole_figure_components import identify
from prepare_living_idle import descriptor_frames
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/revisao-consistencia-20261017'
RES=ROOT/'Assets/Darklight/Resources/Darklight'
ART=ROOT/'Assets/Darklight/ArtSources'

def inspect(path,rows=4,columns=6,strict=True):
    im=Image.open(path).convert('RGBA');frames,labels=identify(im,rows,columns)
    if im.getchannel('A').getextrema()[0]!=0:raise ValueError('Missing transparency: '+str(path))
    if strict and any(f['touchesExtractionEdge'] for f in frames):raise ValueError('Clipped whole figure: '+str(path))
    return im,frames,labels

def install(source,destination,mask,frames,labels):
    destination.parent.mkdir(parents=True,exist_ok=True)
    if source.resolve()!=destination.resolve():shutil.copy2(source,destination)
    owner=Image.new('RGBA',(labels.shape[1],labels.shape[0]),(0,0,0,0));owner.putalpha(Image.fromarray(labels));owner.save(mask)
    assert hashlib.sha256(source.read_bytes()).digest()==hashlib.sha256(destination.read_bytes()).digest()

def main():
    living=json.loads((ART/'LivingIdle20261017/manifest.json').read_text())
    manifest=dict(pages=[],forms=[]);review=[];abyss=dict(pages=[]);ownership=dict(pages=[])
    previous_ownership=RES/'AbyssPrototype/WholeOwnership.json'
    if previous_ownership.exists():
        ownership['pages']=[p for p in json.loads(previous_ownership.read_text())['pages'] if p['name'].startswith('ActionF')]
    inputs=[]
    for weapon in ['katana','daggers','chain']:
        for view in ['cardinal','diagonal']:
            for form in ['', '-Crimson','-Violet','-Absolute']:
                inputs.append((OUT/f'requiem-walk-{weapon}-{view}{form}.png',4,6))
    for view in ['front','diagonal']:
        for form in ['normal','Crimson','Violet','Absolute']:
            inputs.append((OUT/f'abyss-attack-{view}-{form}.png',2,3))
    # Validate the full bank before modifying any production asset.
    validated={str(p):inspect(p,rows,columns) for p,rows,columns in inputs}
    for weapon_index,weapon in enumerate(['katana','daggers','chain']):
        for view in ['cardinal','diagonal']:
            name='RequiemWalkConsistent'+weapon.title()+view.title()+'20261017'
            source=OUT/f'requiem-walk-{weapon}-{view}.png';im,frames,labels=validated[str(source)]
            reference=next(p for p in living['pages'] if p['name']=='RequiemIdleLiving'+view.title()+'20261017')
            _,rf,_=inspect(RES/'TodayCharacterAnimations'/(reference['name']+'.png'))
            # One uniform scale per complete atlas, calibrated to neutral idle.
            # Never change renderer size between phases or stretch anatomy.
            target=float(np.median([f['bodyHeight'] for f in rf[:6]]))/reference['ppu']
            ppu=float(np.median([f['bodyHeight'] for f in frames[:6]]))/target
            page=dict(name=name,kind='Walk',skin=0,age=0,weapon=weapon_index,phases=6,directions=[0,2,4,6] if view=='cardinal' else [1,3,5,7],ppu=ppu,ownerMask=True,idleBlinkFrame=-1,
                sprites=descriptor_frames(frames,im,'Today_'+name))
            install(source,RES/'TodayCharacterAnimations'/(name+'.png'),RES/'TodayCharacterAnimations'/(name+'_Palette.png'),frames,labels)
            manifest['pages'].append(page)
            review.append(dict(name=name,source=source.name,ppu=ppu,frames=frames,sha256=hashlib.sha256(source.read_bytes()).hexdigest()))
            for form_index,form in enumerate(['Crimson','Violet','Absolute'],1):
                source=OUT/f'requiem-walk-{weapon}-{view}-{form}.png';im,frames,labels=validated[str(source)];fn=name+'_'+form
                form_ppu=float(np.median([f['bodyHeight'] for f in frames[:6]]))/target
                install(source,RES/'TransformedYoung'/(fn+'.png'),RES/'TransformedYoung'/(fn+'_Owner.png'),frames,labels)
                manifest['forms'].append(dict(name=fn,baseName=name,form=form_index,ppu=form_ppu,sprites=descriptor_frames(frames,im,'Transformed_'+fn)))
                review.append(dict(name=fn,source=source.name,ppu=form_ppu,frames=frames,sha256=hashlib.sha256(source.read_bytes()).hexdigest()))
    for view_index,view in enumerate(['front','diagonal']):
        for form_index,form in enumerate(['normal','Crimson','Violet','Absolute']):
            name=('' if form_index==0 else f'Form{form_index}')+f'AttackSix{view_index}'
            source=OUT/f'abyss-attack-{view}-{form}.png';im,frames,labels=validated[str(source)]
            sprites=descriptor_frames(frames,im,'Abyss_'+name)
            for i,s in enumerate(sprites):s['name']=f'Abyss_{name}_{i//3}_{i%3}'
            ppu=197.0
            install(source,RES/'AbyssPrototype'/(name+'.png'),RES/'AbyssPrototype'/(name+'_Owner.png'),frames,labels)
            abyss['pages'].append(dict(name=name,ppu=ppu,sprites=sprites))
            ownership['pages'].append(dict(name=name,spriteNames=[s['name'] for s in sprites]))
            review.append(dict(name=name,source=source.name,ppu=ppu,frames=frames,sha256=hashlib.sha256(source.read_bytes()).hexdigest()))
    # Selected whole idle/motion revisions retain their original names and GUIDs.
    revisions={'requiem-violet-idle-cardinal':'RequiemIdleLivingCardinal20261017_Violet'}
    extra=OUT/'existing-revisions.json'
    if extra.exists():revisions.update(json.loads(extra.read_text()))
    for source_name,name in revisions.items():
        page=next(p for p in living['forms'] if p['name']==name)
        base=next(p for p in living['pages'] if p['name']==page['baseName'])
        source=OUT/(source_name+'.png');im,frames,labels=inspect(source,strict=base['kind']!='Motion')
        _,normal_frames,_=inspect(RES/'TodayCharacterAnimations'/(base['name']+'.png'),strict=False)
        normal_frames=[f for f in normal_frames if base['kind']!='Motion' or f['index']%6<5]
        target=float(np.median([f['bodyHeight'] for f in normal_frames[:base['phases']]]))/base['ppu']
        if base['kind']=='Motion':
            selected=[f for f in frames if f['index']%6<5];remap=np.zeros(25,dtype=np.uint8)
            for i,f in enumerate(selected):remap[f['index']+1]=i+1;f['index']=i
            frames=selected;labels=remap[labels]
        if any(f['touchesExtractionEdge'] for f in frames):raise ValueError('Selected complete pose clipped: '+source_name)
        page['ppu']=float(np.median([f['bodyHeight'] for f in frames[:base['phases']]]))/target
        install(source,RES/'TransformedYoung'/(name+'.png'),RES/'TransformedYoung'/(name+'_Owner.png'),frames,labels)
        page['sprites']=descriptor_frames(frames,im,'Transformed_'+name)
        review.append(dict(name=name,source=source.name,ppu=page['ppu'],frames=frames,sha256=hashlib.sha256(source.read_bytes()).hexdigest()))
    (ART/'LivingIdle20261017/manifest.json').write_text(json.dumps(living,indent=2),encoding='utf-8')
    for folder,data in [('ConsistentWalk20261017',manifest),('ConsistentAbyss20261017',abyss)]:
        (ART/folder).mkdir(parents=True,exist_ok=True);(ART/folder/'manifest.json').write_text(json.dumps(data,indent=2),encoding='utf-8')
    (RES/'AbyssPrototype/WholeOwnership.json').write_text(json.dumps(ownership,indent=2),encoding='utf-8')
    (OUT/'manifesto-revisao.json').write_text(json.dumps(dict(sheets=review,sourcePNGsUnchanged=True,wholeFrames=sum(len(s['frames']) for s in review),integrationVerified=False),indent=2),encoding='utf-8')
    print(json.dumps(dict(walkPages=len(manifest['pages']),walkForms=len(manifest['forms']),abyssPages=len(abyss['pages']),revisions=len(revisions))))
if __name__=='__main__':main()
