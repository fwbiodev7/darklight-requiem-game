"""Pack four regenerated WHOLE poses and replace four catalog references only."""
import copy
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys
from types import SimpleNamespace
import uuid

import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
sys.path[:0]=[str(ROOT/'work/skill-validation-deps'),str(ROOT/'Tools/Skills/pixel-art-sprite-sheet-master/scripts')]
import yaml
import sprite_pipeline as sp
from prepare_complexion import median
from inventory_player_eyes import load_asset

OUT=ROOT/'outputs/padronizacao-artes-20261020/katana-roxa-frente'
RESOURCE='Darklight/PlayerVisualRevision20261020/RequiemKatanaVioletFront'
GENERATED=Path('C:/Users/Usuario/.codex/generated_images/01a15b7e-0807-7182-8679-42a44a41d593')
SOURCES=['exec-611792cb-b077-4edc-acb2-96ff19661d15.png','exec-76630f37-5367-4878-b1d7-ee645411fb12.png',
         'exec-6c9d8d04-41a1-4128-8e1c-b58538872d11.png','exec-d9a970ce-ab50-4e43-b77e-a17a40c834bf.png']
# Anatomical landmarks were inspected on each COMPLETE generated figure. They
# set one uniform scale/translation for the whole pose, including its weapon.
LANDMARKS=[(640,247,1228),(479,157,1125),(485,163,1141),(477,137,1163)]


def guid(name):return uuid.uuid5(uuid.NAMESPACE_URL,'darklight-requiem/20261020/'+name).hex


def main():
    if (OUT/'PROVENIENCIA.json').exists():
        raise ValueError('Already prepared. Use a new versioned destination rather than overwrite the delivered sources.')
    OUT.mkdir(parents=True,exist_ok=True)
    cell,ground,body=448,400,220
    ppu=body/1.99
    atlas=Image.new('RGBA',(cell*4,cell));owner=Image.new('RGBA',atlas.size)
    descriptors,records=[],[]
    art=ROOT/'Assets/Darklight/ArtSources/PlayerVisualRevision20261020'
    art.mkdir(exist_ok=True)
    for i,(source_name,(anchor,hair,feet)) in enumerate(zip(SOURCES,LANDMARKS)):
        dest=art/f'RequiemKatanaVioletFront-{i}-source.png'
        source=dest if dest.exists() else GENERATED/source_name
        if dest.exists() and sp.digest(dest)!=sp.digest(source):raise ValueError('Source destination mismatch')
        if source!=dest:shutil.copy2(source,dest)
        im=Image.open(source).convert('RGBA')
        scale=body/(feet-hair)
        size=(round(im.width*scale),round(im.height*scale))
        native=im.resize(size,Image.Resampling.NEAREST)
        offset=(round(cell/2-anchor*size[0]/im.width),round(ground-feet*size[1]/im.height))
        if min(offset)<0 or offset[0]+size[0]>cell or offset[1]+size[1]>cell:
            raise ValueError('Whole pose would be clipped')
        canvas=Image.new('RGBA',(cell,cell));canvas.paste(native,offset)
        bbox=canvas.getchannel('A').point(lambda v:255 if v>=166 else 0).getbbox()
        if bbox is None or min(bbox[:2])<4 or max(bbox[2:])>cell-4:raise ValueError('Missing sprite margin')
        canvas.save(OUT/f'f{i:02}.png')
        atlas.paste(canvas,(i*cell,0))
        mask=np.zeros((cell,cell,4),np.uint8);mask[np.array(canvas)[:,:,3]>0,3]=i+1
        owner.paste(Image.fromarray(mask),(i*cell,0))
        fid=f'RequiemKatanaVioletFront_{i}'
        descriptors.append(dict(id=fid,rect=[cell*i,0,cell,cell],pivot=[.5,(cell-ground)/cell],durationMs=100,
                                action='attack',direction='S',skin='Requiem',form='Violet'))
        records.append(dict(frame=fid,source=str(dest.relative_to(ROOT)),sha256=sp.digest(dest),
                            sourceCanvas=list(im.size),logicalWholeCanvas=list(size),uniformScale=scale,
                            wholePoseLandmarks=[anchor,hair,feet],offset=list(offset),wholeFigure=True))
    atlas.save(OUT/'sheet.png');owner.save(OUT/'owner.png')
    data=dict(schemaVersion=1,image='sheet.png',imageSha256=sp.digest(OUT/'sheet.png'),nativeSize=list(atlas.size),
              pixelsPerUnit=ppu,status='visual-reviewed',frames=descriptors,
              clips=[dict(name='attack_S',frames=[f['id'] for f in descriptors],loop=False)],
              notes=['Four complete poses; whole-figure nearest-neighbor scale only. No anatomy assembly.',
                     '100ms preview timing; runtime uses existing combat phase selection/events.'])
    sp.write(OUT/'sheet.json',data)
    dest=ROOT/'Assets/Darklight/Resources'/(RESOURCE+'.png');dest.parent.mkdir(exist_ok=True)
    shutil.copy2(OUT/'sheet.png',dest);shutil.copy2(OUT/'owner.png',dest.with_name(dest.stem+'_Owner.png'))
    template=ROOT/'Assets/Darklight/Resources/Darklight/TodayCharacterAnimations/RequiemAttackKatanaYoungCardinal.png.meta'
    meta=yaml.safe_load(template.read_text(encoding='utf-8-sig'));meta['guid']=guid(RESOURCE)
    imp=meta['TextureImporter'];sheet=imp['spriteSheet'];example=copy.deepcopy(sheet['sprites'][0])
    imp.update(spritePixelsToUnits=ppu,maxTextureSize=2048,textureCompression=0,alphaIsTransparency=0,nPOTScale=0)
    imp['mipmaps']['enableMipMap']=0;imp['textureSettings']['filterMode']=0
    imp['platformSettings']=[dict(serializedVersion=3,buildTarget='DefaultTexturePlatform',maxTextureSize=2048,
                                  resizeAlgorithm=0,textureFormat=-1,textureCompression=0,compressionQuality=100,
                                  crunchedCompression=0,allowsAlphaSplitting=0,overridden=0,ignorePlatformSupport=0,
                                  androidETC2FallbackOverride=0,forceMaximumCompressionQuality_BC6H_BC7=0)]
    imp['internalIDToNameTable']=[]
    sheet['sprites']=[];sheet['nameFileIdTable']={};sheet['spriteID']=guid(RESOURCE+'-atlas')
    for i,f in enumerate(descriptors):
        s=copy.deepcopy(example);internal=100000+i
        s.update(name=f['id'],rect=dict(serializedVersion=2,x=i*cell,y=0,width=cell,height=cell),
                 pivot=dict(x=.5,y=(cell-ground)/cell),spriteID=guid(f['id']),internalID=internal,
                 outline=[],physicsShape=[],bones=[],vertices=[],indices=None,edges=[],weights=[])
        sheet['sprites'].append(s);sheet['nameFileIdTable'][f['id']]=internal
        imp['internalIDToNameTable'].append(dict(first={213:internal},second=f['id']))
    Path(str(dest)+'.meta').write_text(yaml.safe_dump(meta,sort_keys=False))
    mask_meta=copy.deepcopy(meta);mask_meta['guid']=guid(RESOURCE+'_Owner')
    mi=mask_meta['TextureImporter'];mi.update(textureType=0,spriteMode=0,alphaIsTransparency=0,isReadable=0)
    mi['mipmaps']['sRGBTexture']=0;mi['spriteSheet']['sprites']=[];mi['spriteSheet']['nameFileIdTable']={};mi['internalIDToNameTable']=[]
    Path(str(dest.with_name(dest.stem+'_Owner.png'))+'.meta').write_text(yaml.safe_dump(mask_meta,sort_keys=False))
    catalog=ROOT/'Assets/Darklight/Resources/Darklight/TransformedCatalog.asset'
    before=load_asset(catalog)
    selected=next(p for p in before['pages'] if p['baseName']=='RequiemAttackKatanaYoungCardinal' and p['form']==2)
    sp.write(OUT/'original-catalog-page.json',selected)
    if selected.get('references'):raise ValueError('Already has references; review before rerunning')
    refs=[]
    for i,name in enumerate(selected['spriteNames']):
        refs.append(dict(resourcePath=RESOURCE if i<4 else selected['resourcePath'],
                         palettePath=RESOURCE+'_Owner' if i<4 else selected['palettePath'],
                         spriteName=descriptors[i]['id'] if i<4 else name,ownerId=i+1))
    text=catalog.read_text(encoding='utf-8-sig')
    blocks=re.split(r'(?=^  - baseName: )',text,flags=re.M)
    matched=0
    for i,block in enumerate(blocks):
        if block.startswith('  - baseName: RequiemAttackKatanaYoungCardinal\n') and '\n    form: 2\n' in block:
            value='    references:\n'+''.join('    - resourcePath: '+r['resourcePath']+'\n      palettePath: '+r['palettePath']+'\n      spriteName: '+r['spriteName']+'\n      ownerId: '+str(r['ownerId'])+'\n' for r in refs)
            blocks[i]=block.replace('    references: []\n',value);matched+=1
    if matched!=1:raise ValueError('Expected one catalog page')
    catalog.write_text(''.join(blocks),encoding='utf-8')
    after=load_asset(catalog)
    for old,new in zip(before['pages'],after['pages']):
        if old['baseName']==selected['baseName'] and old['form']==2:
            assert new['references']==refs
        else:assert old==new
    complexion=ROOT/'Assets/Darklight/Resources/Darklight/CanonicalComplexion.json'
    colors=json.loads((ROOT/'outputs/olhos-todas-formas-20261020/CanonicalComplexion-complete.json').read_text())
    target=np.array(colors['referenceMedian']);actual=median(dest)
    colors['entries'].append(dict(texture=dest.stem,shift=((target-actual)/255).tolist(),sourceMedian=actual.tolist()))
    colors['scope']='All 260 inventoried player sources plus four complete redrawn frontal katana poses; protected death excluded at runtime'
    complexion.write_text(json.dumps(colors,indent=2)+'\n')
    report=dict(frames=records,oldReferencesPreserved=12,otherCatalogPagesPreserved=len(before['pages'])-1,
                originalPngsRewritten=False,alphaGeometryReconstructed=False,framesReplaced=[0,1,2,3],
                sourceAuthoringResolution='Generated whole-frame sources; native output and scaling recorded separately',
                visualReviewed=True,userApproved=False,runtimeVerified=False)
    sp.write(OUT/'PROVENIENCIA.json',report);sp.write(art/'manifest.json',report)
    audit=sp.audit(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'audit',unity_meta=Path(str(dest)+'.meta')))
    sp.preview(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'preview',zoom=2))
    print(json.dumps(dict(frames=4,otherReferencesPreserved=12,auditOk=audit['ok'],warnings=audit['warnings'],
                          complexionEntries=len(colors['entries']),preview=str(OUT/'preview/preview.html'))))
    return 0 if audit['ok'] else 2


if __name__=='__main__':raise SystemExit(main())
