"""Register complete individual-strike frames; never edit or assemble anatomy."""
from pathlib import Path
import json,hashlib
from PIL import Image
from prepare_living_idle import whole_frames,descriptor_frames,copy_and_mask,NORMAL,FORMS,ROOT

OUT=ROOT/'outputs/combate-direcional-20261017'

def main():
    source=json.loads((OUT/'fontes-e-prompts.json').read_text(encoding='utf-8'))
    jobs=source['jobs'];manifest=dict(pages=[],forms=[]);audit=[]
    original=json.loads((ROOT/'Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json').read_text(encoding='utf-8'))['pages']
    for job in jobs:whole_frames(Image.open(job['output']).convert('RGBA'))
    for job in [j for j in jobs if j['kind']=='normal']:
        skin=0 if job['job']['skin']=='Requiem' else 1
        weapon=0 if job['job']['weapon']=='katana' else 1
        key='Leve1' if weapon==0 else 'Presas1'
        name=('Requiem' if skin==0 else 'Rock')+('Katana' if weapon==0 else 'Daggers')+key+('Cardinal' if job['job']['set']=='cardinais' else 'Diagonal')+'20261017'
        path=Path(job['output']);im=Image.open(path).convert('RGBA');frames=whole_frames(im)
        reference=next(p for p in original if p['kind']=='Idle' and p['skin']==skin)
        old=Image.open(NORMAL/(reference['name']+'.png')).convert('RGBA');f=reference['sprites'][0]
        box=old.crop((f['x'],old.height-f['y']-f['h'],f['x']+f['w'],old.height-f['y'])).getchannel('A').point(lambda a:255 if a>=128 else 0).getbbox()
        ppu=frames[0]['bodyHeight']/((box[3]-box[1])/reference['ppu'])
        copy_and_mask(path,NORMAL/(name+'.png'),frames,NORMAL/(name+'_Palette.png'))
        manifest['pages'].append(dict(name=name,kind='Attack',attackKey=key,skin=skin,age=0,weapon=weapon,
            directions=[0,2,4,6] if job['job']['set']=='cardinais' else [1,3,5,7],phases=6,startupFrameEnd=2,activeFrameEnd=4,
            ownerMask=True,ppu=ppu,sprites=descriptor_frames(frames,im,'Today_'+name)))
        audit.append(dict(id=job['id'],frames=frames,ppu=ppu,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
        for form,tag,index in [('vermelha','Crimson',1),('roxa','Violet',2),('branca','Absolute',3)]:
            variant=next(j for j in jobs if j.get('base')==job['id'] and j['form']==form)
            fp=Path(variant['output']);fi=Image.open(fp).convert('RGBA');ff=whole_frames(fi)
            if fi.size!=im.size:raise ValueError('Transformed canvas must match base')
            fn=name+'_'+tag;copy_and_mask(fp,FORMS/(fn+'.png'),ff,FORMS/(fn+'_Owner.png'))
            manifest['forms'].append(dict(name=fn,baseName=name,form=index,ppu=ppu,sprites=descriptor_frames(ff,fi,'Transformed_'+fn)))
            audit.append(dict(id=variant['id'],frames=ff,ppu=ppu,sha256=hashlib.sha256(fp.read_bytes()).hexdigest()))
    folder=ROOT/'Assets/Darklight/ArtSources/DetailedCombat20261017';folder.mkdir(parents=True,exist_ok=True)
    (folder/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    (OUT/'auditoria-quadros-inteiros.json').write_text(json.dumps(dict(sheets=audit,wholeFrames=sum(len(p['frames']) for p in audit),sourcePNGsUnchanged=True),indent=2),encoding='utf-8')
    print(json.dumps(dict(normalPages=len(manifest['pages']),formPages=len(manifest['forms']),wholeFrames=sum(len(p['frames']) for p in audit))))

if __name__=='__main__':main()
