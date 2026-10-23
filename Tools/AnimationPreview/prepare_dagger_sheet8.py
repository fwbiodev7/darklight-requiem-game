"""Validate/pack eight complete generated poses; update matching catalog refs only."""
import copy
import json
from pathlib import Path
import re
import shutil
import sys
from types import SimpleNamespace

import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
sys.path[:0]=[str(ROOT/'work/skill-validation-deps'),str(ROOT/'Tools/Skills/pixel-art-sprite-sheet-master/scripts')]
import yaml
import sprite_pipeline as sp
from whole_figure_components import identify
from inventory_player_eyes import load_asset
from prepare_complexion import median
from prepare_sharp_katana import guid

OUT=ROOT/'outputs/padronizacao-artes-20261020/adagas-8-v2'
RESOURCE='Darklight/PlayerVisualRevision20261020/RequiemDaggersViolet8'
ORIGINAL='Darklight/TransformedYoung/RequiemCombatSupportDaggersYoung_Violet'
SOURCE=Path('C:/Users/Usuario/.codex/generated_images/01a15b7e-0807-7182-8679-42a44a41d593/exec-60cec064-9eff-413a-867b-de5831eda05c.png')


def main():
    if (OUT/'PROVENIENCIA.json').exists():raise ValueError('Use a new versioned destination; existing deliveries are preserved')
    OUT.mkdir(parents=True,exist_ok=True)
    art=ROOT/'Assets/Darklight/ArtSources/PlayerVisualRevision20261020'
    preserved=art/'RequiemDaggersViolet8-v2-source.png'
    if not preserved.exists():shutil.copy2(SOURCE,preserved)
    im=Image.open(preserved).convert('RGBA');figures,owners=identify(im,rows=2,columns=4)
    old=json.loads((ROOT/'outputs/olhos-todas-formas-20261020/metadata/Darklight__TransformedYoung__RequiemCombatSupportDaggersYoung_Violet.json').read_text())
    cell,ground=512,464
    ppu=figures[0]['bodyHeight']/1.99
    atlas=Image.new('RGBA',(cell*4,cell*2));mask_atlas=Image.new('RGBA',atlas.size)
    descriptors,provenance=[],[]
    for i,f in enumerate(figures):
        x0,y0,x1,y1=f['rect'];tile=np.array(im)[y0:y1,x0:x1].copy()
        tile[owners[y0:y1,x0:x1]!=i+1,3]=0
        h,w=tile.shape[:2]
        old_pivot=old['frames'][i]['pivot']
        anchor_x=round(w*old_pivot[0]);anchor_y=round(h*(1-old_pivot[1]))
        dx,dy=(cell-w)//2,ground-h
        if dx<4 or dy<4 or dx+w>cell-4 or dy+h>cell-4:raise ValueError('Complete pose exceeds canvas; do not clip')
        canvas=Image.new('RGBA',(cell,cell));canvas.paste(Image.fromarray(tile),(dx,dy))
        canvas.save(OUT/f'f{i:02}.png')
        gx,gy=(i%4)*cell,(i//4)*cell
        atlas.paste(canvas,(gx,gy))
        m=np.zeros((cell,cell,4),np.uint8);m[np.array(canvas)[:,:,3]>0,3]=i+1
        mask_atlas.paste(Image.fromarray(m),(gx,gy))
        descriptors.append(dict(id=f'RequiemDaggersViolet8_{i}',rect=[gx,gy,cell,cell],
                                pivot=[(dx+anchor_x)/cell,(cell-dy-anchor_y)/cell],durationMs=120,
                                action='ready' if i%4<2 else 'dodge',direction='S' if i<4 else 'SW',skin='Requiem',form='Violet'))
        provenance.append(dict(frame=i,sourceRectXYXY=f['rect'],owner=i+1,offset=[dx,dy],
                               originalPivot=old_pivot,wholeFigure=True,sourceScale=1,anatomyEdits=False))
    atlas.save(OUT/'sheet.png');mask_atlas.save(OUT/'owner.png')
    d=dict(schemaVersion=1,image='sheet.png',imageSha256=sp.digest(OUT/'sheet.png'),nativeSize=list(atlas.size),
           pixelsPerUnit=ppu,status='visual-reviewed',frames=descriptors,
           clips=[dict(name='ready_S',frames=[descriptors[i]['id'] for i in [0,1]],loop=True),
                  dict(name='dodge_S',frames=[descriptors[i]['id'] for i in [0,2,3,2,0]],loop=False),
                  dict(name='ready_SW',frames=[descriptors[i]['id'] for i in [4,5]],loop=True),
                  dict(name='dodge_SW',frames=[descriptors[i]['id'] for i in [4,6,7,6,4]],loop=False)],
           notes=['Eight complete figures generated together in one sheet, as requested.',
                  'Exact source scale; whole-figure ownership only. Shared body height 1.99 world units.',
                  'Preview clips are inspection sequences; existing gameplay uses unchanged catalog phase timing.'])
    sp.write(OUT/'sheet.json',d)
    dest=ROOT/'Assets/Darklight/Resources'/(RESOURCE+'.png')
    shutil.copy2(OUT/'sheet.png',dest);shutil.copy2(OUT/'owner.png',dest.with_name(dest.stem+'_Owner.png'))
    template=yaml.safe_load(dest.with_name('RequiemKatanaVioletFront.png.meta').read_text())
    template['guid']=guid(RESOURCE);imp=template['TextureImporter'];ss=imp['spriteSheet']
    example=copy.deepcopy(ss['sprites'][0]);imp['spritePixelsToUnits']=ppu
    imp['internalIDToNameTable']=[];ss['sprites']=[];ss['nameFileIdTable']={};ss['spriteID']=guid(RESOURCE+'-atlas')
    for i,f in enumerate(descriptors):
        item=copy.deepcopy(example);gx,gy,w,h=f['rect'];internal=200000+i
        item.update(name=f['id'],rect=dict(serializedVersion=2,x=gx,y=atlas.height-gy-h,width=w,height=h),
                    pivot=dict(x=f['pivot'][0],y=f['pivot'][1]),spriteID=guid(f['id']),internalID=internal)
        ss['sprites'].append(item);ss['nameFileIdTable'][f['id']]=internal
        imp['internalIDToNameTable'].append(dict(first={213:internal},second=f['id']))
    Path(str(dest)+'.meta').write_text(yaml.safe_dump(template,sort_keys=False))
    om=copy.deepcopy(template);om['guid']=guid(RESOURCE+'_Owner');oi=om['TextureImporter']
    oi.update(textureType=0,spriteMode=0,alphaIsTransparency=0);oi['mipmaps']['sRGBTexture']=0
    oi['internalIDToNameTable']=[];oi['spriteSheet']['sprites']=[];oi['spriteSheet']['nameFileIdTable']={}
    Path(str(dest.with_name(dest.stem+'_Owner.png'))+'.meta').write_text(yaml.safe_dump(om,sort_keys=False))
    catalog=ROOT/'Assets/Darklight/Resources/Darklight/TransformedCatalog.asset'
    before=load_asset(catalog);text=catalog.read_text(encoding='utf-8-sig')
    changes=[]
    pattern=re.compile(r'    - resourcePath: (?P<path>'+re.escape(ORIGINAL)+'|'+re.escape(RESOURCE)+r')\n      palettePath: (?P=path)_Owner\n      spriteName: (?P<name>[^\n]+)_(?P<index>\d+)\n      ownerId: (?P<owner>\d+)\n')
    def replace(match):
        i=int(match['index'])
        if i>=8:return match[0]
        if int(match['owner'])!=i+1:raise ValueError('Unexpected original owner')
        changes.append(dict(spriteName=match['name']+'_'+str(i),index=i))
        return ('    - resourcePath: '+RESOURCE+'\n      palettePath: '+RESOURCE+'_Owner\n      spriteName: '+descriptors[i]['id']+'\n      ownerId: '+str(i+1)+'\n')
    revised=pattern.sub(replace,text)
    if not changes or {r['index'] for r in changes}!=set(range(8)):raise ValueError('Not all eight poses have real consumers')
    catalog.write_text(revised,encoding='utf-8')
    after=load_asset(catalog)
    untouched=0;affected=[]
    for oldpage,newpage in zip(before['pages'],after['pages']):
        oldrefs,newrefs=oldpage.get('references') or [],newpage.get('references') or []
        consumed=any(r['resourcePath']==RESOURCE for r in newrefs)
        if oldpage==newpage and not consumed:untouched+=1;continue
        assert oldpage['form']==2
        assert {k:v for k,v in oldpage.items() if k!='references'}=={k:v for k,v in newpage.items() if k!='references'}
        for a,b in zip(oldrefs,newrefs):
            if a!=b:assert a['resourcePath'] in (ORIGINAL,RESOURCE) and b['resourcePath']==RESOURCE and 1<=b['ownerId']<=8
        affected.append(oldpage['baseName'])
    palette=ROOT/'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json';colors=json.loads(palette.read_text())
    target=np.array(colors['referenceMedian']);actual=median(dest)
    colors['entries']=[r for r in colors['entries'] if r['texture']!=dest.stem]
    colors['entries'].append(dict(texture=dest.stem,shift=((target-actual)/255).tolist(),sourceMedian=actual.tolist()))
    colors['scope']='All 260 inventoried player sources plus two new complete-figure sheets (4 katana and 8 daggers); death disabled in renderer'
    palette.write_text(json.dumps(colors,indent=2)+'\n')
    report=dict(source=str(preserved.relative_to(ROOT)),sourceSha256=sp.digest(preserved),sourceSize=list(im.size),
                frames=provenance,changedCatalogReferences=len(changes),affectedPages=affected,
                otherPagesUnchanged=untouched,originalPngsUnchanged=True,visualReviewed=True,
                userApproved=False,unityImported=False,runtimeVerified=False)
    sp.write(OUT/'PROVENIENCIA.json',report);sp.write(art/'daggers8-v2-manifest.json',report)
    sp.audit(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'audit',unity_meta=Path(str(dest)+'.meta')))
    sp.preview(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'preview',zoom=1))
    print(json.dumps(dict(wholePoses=8,sourceSize=list(im.size),nativeAtlas=list(atlas.size),
                          changedReferences=len(changes),affectedPages=affected,otherPagesUnchanged=untouched)))


if __name__=='__main__':main()
