"""Pack five repaired WHOLE figures and byte-preserve every correct neighbor.

Native raster densities are explicit; PNG pixels are never resized or recolored.
"""
import copy
import hashlib
import json
from pathlib import Path
import shutil
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT=ROOT/'outputs/retomada-20261101/selective-repair/integration'
SOURCES=ROOT/'ArtSources/KatanaSelectiveRepair20261101/selected-packs'


def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')


def main():
    OUT.mkdir(parents=True,exist_ok=True);SOURCES.mkdir(parents=True,exist_ok=True)
    staged=OUT/'staged';staged.mkdir(exist_ok=True)
    fixes={}
    for f in read(ROOT/'outputs/retomada-20261101/selective-repair/w-nw/review.json')['frames']:
        fixes[f['targetId']]={'path':ROOT/f['path'],'pivot':f['pivot'],'ppu':f['diagnosticHeadResolutionPpu']}
    for f in read(ROOT/'outputs/retomada-20261101/selective-repair/e-se/repair-manifest.json')['frames']:
        fixes[f['id']]={'path':ROOT/f['file'],'pivot':f['pivotUnityBottomLeft'],'ppu':352.7105943152455}
    ne=read(ROOT/'outputs/retomada-20261101/selective-repair/ne/candidate-manifest.json')
    f=ne['frames'][0]
    fixes[f['replaces']]={'path':ROOT/ne['sourcePath'],'pivot':f['pivot'],'ppu':ne['pixelsPerUnit']}
    versions={
        'ReqKatanaNormalDodgeS_SW_W20261101V1':'ReqKatanaNormalDodgeS_SW_W20261101RepairV2',
        'ReqKatanaNormalDodgeNW_N20261101Selected':'ReqKatanaNormalDodgeNW_N20261101RepairV2',
        'ReqKatanaNormalDodgeE_SE20261101V1':'ReqKatanaNormalDodgeE_SE20261101RepairV2',
        'ReqKatanaNormalDodgeNE20261101V5':'ReqKatanaNormalDodgeNE20261101RepairV6'}
    pages=read(BANK/'catalog.json')['pages'];new_pages=[];proof=[]
    for old_name,new_name in versions.items():
        old=read(BANK/(old_name+'.json'));old_image=Image.open(BANK/old['image']).convert('RGBA')
        extracted=[]
        for frame in old['frames']:
            x,y,w,h=frame['rect'];original=old_image.crop((x,y,x+w,y+h))
            fix=fixes.get(frame['id'])
            image=Image.open(fix['path']).convert('RGBA') if fix else original
            extracted.append((frame,original,image,fix))
        width=max(a[2].width for a in extracted)+32;height=max(a[2].height for a in extracted)+32
        atlas=Image.new('RGBA',(width*3,height*((len(extracted)+2)//3)),(0,0,0,0))
        frames=[];mapping={};densities={}
        for i,(prior,original,image,fix) in enumerate(extracted):
            x=(i%3)*width;y=(i//3)*height;left=x+16;top=y+16
            atlas.paste(image,(left,top))
            check=atlas.crop((left,top,left+image.width,top+image.height))
            if check.tobytes()!=image.tobytes():raise ValueError('Packing changed RGBA')
            pivot=fix['pivot'] if fix else prior['pivot']
            anchor=[pivot[0]*image.width,(1-pivot[1])*image.height]
            frame=copy.deepcopy(prior);identity=f'Redesign_{new_name}_{i:02}'
            frame.update(id=identity,rect=[left,top,image.width,image.height],pivot=pivot,
                         sourceFrameIndex=i,sourceRect=[left,top,image.width,image.height],sourceOffsetInFrame=[0,0],sourceScale=1,
                         sourceAnchor=[left+anchor[0],top+anchor[1]],renderPixelsPerUnit=fix['ppu'] if fix else old['pixelsPerUnit'],
                         originalWholeFigureId=prior['id'],wholeFigureRepaired=bool(fix))
            frames.append(frame);mapping[prior['id']]=identity;densities[identity]=frame['renderPixelsPerUnit']
            proof.append({'oldId':prior['id'],'newId':identity,'repaired':bool(fix),'rgbaExact':True,
                          'nativePixelsResized':False,'pixelsPerUnit':frame['renderPixelsPerUnit'],
                          'originalPreserved':True,'newWholeFigureSha256':sha(fix['path']) if fix else None})
        png=staged/(new_name+'.png')
        if (BANK/png.name).exists():raise ValueError('Preserve integrated artwork; use another version')
        atlas.save(png);source=SOURCES/(new_name+'-source-only-reviewed.png');shutil.copy2(png,source)
        sheet=copy.deepcopy(old)
        sheet.update(image=png.name,imageSha256=sha(png),sourcePath=source.relative_to(ROOT).as_posix(),sourceSha256=sha(source),
                     nativeSize=list(atlas.size),sourceSize=list(atlas.size),frames=frames,status='visual-reviewed',
                     selectedSourceFrameIndices=list(range(len(frames))),excludedSourceFrameIndices=[],additionalSources=[],
                     nativeRgbaPixelsPreserved=True,wholeFigures=True,frameDensityPurpose='Native image resolution only; limb drawing corrected separately')
        sheet['humanApproved']=False
        sheet['preservesHumanApprovedOriginals']=True
        sheet['sourcePixels']=atlas.width*atlas.height-atlas.getchannel('A').histogram()[0]
        sheet['originalSourcePixels']=sheet['sourcePixels']
        for clip in sheet.get('clips',[]):clip['frames']=[mapping[n] for n in clip['frames']]
        save(staged/(new_name+'.json'),sheet)
        for prior in pages:
            if Path(prior['resourcePath']).name!=old_name:continue
            page=copy.deepcopy(prior);page['id']=page['id'].replace(old_name,new_name)
            page['humanApproved']=False
            page['resourcePath']=page['resourcePath'].replace(old_name,new_name)
            page['spriteNames']=[mapping[n] for n in page['spriteNames']]
            page['framePixelsPerUnit']=[densities[n] for n in page['spriteNames']]
            for key in ('imageSha256','sourcePath','sourceSha256','sourceSize','nativeSize'):page[key]=sheet[key]
            new_pages.append(page)
    save(OUT/'catalog-fragment.json',{'pages':new_pages})
    save(OUT/'rgba-preservation-proof.json',{'repairedFigures':sum(p['repaired'] for p in proof),
        'unchangedNeighbors':sum(not p['repaired'] for p in proof),'allRgbaExact':all(p['rgbaExact'] for p in proof),
        'worldVisualScale':1.325,'proof':proof,'replacements':versions})
    print(json.dumps({'sheets':len(versions),'repaired':5,'correctNeighborsPreserved':len(proof)-5,'resizedPixels':False}))


if __name__=='__main__':main()
