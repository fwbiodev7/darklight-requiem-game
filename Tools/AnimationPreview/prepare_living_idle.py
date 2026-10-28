"""Describe complete figures, register ground pivots and write technical owner masks.

PNG sources are copied unchanged. No limbs are cropped, redrawn or assembled.
"""
from pathlib import Path
import hashlib,json,shutil
import numpy as np
from PIL import Image,ImageDraw
from whole_figure_components import identify
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/implementacao-fase2-20261017'
SOURCE=ROOT/'Assets/Darklight/ArtSources/LivingIdle20261017'
NORMAL=ROOT/'Assets/Darklight/Resources/Darklight/TodayCharacterAnimations'
FORMS=ROOT/'Assets/Darklight/Resources/Darklight/TransformedYoung'

def page_name(job):
    return ('Requiem' if job['skin']=='requiem' else 'Rock')+('LivingMotion' if job.get('kind')=='Motion' else 'IdleLiving')+('Cardinal' if job['set']=='cardinais' else 'Diagonal')+'20261017'

def source_path(job):
    source=Path(job['output'])
    if source.is_file():return source
    # A clean checkout already preserves the exact selected full PNG in Resources.
    # Use it when the generation machine's original absolute path is unavailable.
    name=page_name(job)
    if job.get('form'):
        return FORMS/(name+'_'+{'vermelha':'Crimson','roxa':'Violet','branca':'Absolute'}[job['form']]+'.png')
    return NORMAL/(name+'.png')

def whole_frames(im,strict=True,motion=False):
    frames,_=identify(im)
    if motion:
        # Settling returns to the already approved idle, rather than duplicating
        # an idle pose in every motion atlas. Unused sixth-column candidates are
        # not imported as sprites, including any clipped source candidates.
        frames=[dict(f,sourceIndex=f['index']) for f in frames if f['index']%6<5]
        for index,frame in enumerate(frames):frame['index']=index
    if strict and any(f['touchesExtractionEdge'] for f in frames):
        raise ValueError('Whole figure touches source canvas edge; regenerate instead of clipping')
    return frames

def descriptor_frames(frames,im,prefix):
    result=[]
    for frame in frames:
        x0,y0,x1,y1=frame['rect'];ax,ay=frame['anchor']
        result.append(dict(name=prefix+'_'+str(frame['index']),previousName='',x=x0,y=im.height-y1,
                           w=x1-x0,h=y1-y0,pivotX=(ax-x0)/(x1-x0),pivotY=(y1-ay)/(y1-y0)))
    return result

def copy_and_mask(source,destination,frames,mask_path):
    if Path(source).resolve()!=Path(destination).resolve():shutil.copy2(source,destination)
    with Image.open(source) as im:
        # Per-pixel identification of complete source figures avoids including a
        # neighbouring sword where whole-figure bounding rectangles overlap.
        _,labels=identify(im.convert('RGBA'))
        remap=np.zeros(25,dtype=np.uint8)
        for frame in frames:remap[frame.get('sourceIndex',frame['index'])+1]=frame['index']+1
        labels=remap[labels]
        owner=Image.new('RGBA',im.size,(0,0,0,0));owner.putalpha(Image.fromarray(labels))
        owner.save(mask_path)
    assert hashlib.sha256(Path(source).read_bytes()).digest()==hashlib.sha256(destination.read_bytes()).digest()

def main():
    config=json.loads((OUT/'PLANO-IDLE-FINAL.json').read_text(encoding='utf-8'))
    original=json.loads((ROOT/'Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json').read_text(encoding='utf-8'))['pages']
    SOURCE.mkdir(parents=True,exist_ok=True);manifest=dict(pages=[],forms=[]);review=[]
    # Validate every complete figure before copying anything into Resources.
    for job in config['normal']+config['forms']:
        whole_frames(Image.open(source_path(job)).convert('RGBA'),motion=job.get('kind')=='Motion')
    for job in config['normal']:
        path=source_path(job);im=Image.open(path).convert('RGBA');frames=whole_frames(im,motion=job.get('kind')=='Motion')
        assert im.getchannel('A').getextrema()[0]==0
        skin=0 if job['skin']=='requiem' else 1
        reference=next(p for p in original if p['kind']=='Idle' and p['skin']==skin)
        old=Image.open(NORMAL/(reference['name']+'.png')).convert('RGBA');old_rect=reference['sprites'][0]
        old_box=old.crop((old_rect['x'],old.height-old_rect['y']-old_rect['h'],old_rect['x']+old_rect['w'],old.height-old_rect['y'])).getchannel('A').point(lambda a:255 if a>=128 else 0).getbbox()
        ppu=frames[0]['bodyHeight']/((old_box[3]-old_box[1])/reference['ppu'])
        kind=job.get('kind','Idle')
        name=page_name(job)
        page=dict(name=name,kind=kind,skin=skin,age=0,weapon=0,directions=job['directions'],phases=5 if kind=='Motion' else 6,
                  idleBlinkFrame=5 if kind=='Idle' else -1,ppu=ppu,ownerMask=True,sprites=descriptor_frames(frames,im,'Today_'+name))
        copy_and_mask(path,NORMAL/(name+'.png'),frames,NORMAL/(name+'_Palette.png'))
        manifest['pages'].append(page)
        review.append(dict(id=job['id'],file=path.name,width=im.width,height=im.height,frames=frames,ppu=ppu,
                           sourceSha256=hashlib.sha256(path.read_bytes()).hexdigest()))
        for form_index,form in enumerate(['vermelha','roxa','branca'],1):
            formed=next(f for f in config['forms'] if f['base']==job['id'] and f['form']==form)
            form_path=source_path(formed);fi=Image.open(form_path).convert('RGBA');ff=whole_frames(fi,motion=kind=='Motion')
            assert fi.size==im.size and fi.getchannel('A').getextrema()[0]==0
            form_name=name+'_'+['','Crimson','Violet','Absolute'][form_index]
            copy_and_mask(form_path,FORMS/(form_name+'.png'),ff,FORMS/(form_name+'_Owner.png'))
            manifest['forms'].append(dict(name=form_name,baseName=name,form=form_index,ppu=ppu,
                                          sprites=descriptor_frames(ff,fi,'Transformed_'+form_name)))
            review.append(dict(id=formed['id'],file=form_path.name,width=fi.width,height=fi.height,frames=ff,ppu=ppu,
                               sourceSha256=hashlib.sha256(form_path.read_bytes()).hexdigest()))
    (SOURCE/'manifest.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')
    (SOURCE/'generation-prompts.json').write_text(json.dumps(config,indent=2,ensure_ascii=False),encoding='utf-8')
    (OUT/'manifesto-idle-revisao.json').write_text(json.dumps(dict(sheets=review,integrationVerified=False,
        sourcePNGsUnchanged=True,wholeFrames=sum(len(s['frames']) for s in review),notes=['Whole-figure rectangles only; no limb assembly.',
        'One fixed PPU per base sheet, retained for all three forms; ground pivots registered per whole frame.',
        'Rock has no hanging red strips, corrected by user instruction.']),indent=2,ensure_ascii=False),encoding='utf-8')
    print(json.dumps(dict(normalPages=len(manifest['pages']),formPages=len(manifest['forms']),wholeFrames=sum(len(s['frames']) for s in review))))
if __name__=='__main__':main()
