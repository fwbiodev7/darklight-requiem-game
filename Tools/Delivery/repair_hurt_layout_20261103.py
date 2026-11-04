"""Repack three proven overlapping atlases; preserve every whole-frame RGBA byte."""
import copy, hashlib, json, math
from pathlib import Path
from PIL import Image

ROOT=Path(__file__).resolve().parents[2]
BANK=ROOT/'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023'
OUT=ROOT/'outputs/retomada-20261103/hurt-layout-repair'
NAMES=['Req'+weapon+'NormalHurtN_NE_E_SE20261103V1' for weapon in ('Katana','Fists','Moto')]

def sha(path):return hashlib.sha256(path.read_bytes()).hexdigest()
def read(path):return json.loads(path.read_text(encoding='utf-8-sig'))
def write(path,data):path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

def main():
    staged=OUT/'staging';staged.mkdir(parents=True,exist_ok=True)
    catalog=read(BANK/'catalog.json');pages=[];proofs=[];replacements={}
    for name in NAMES:
        sheet=read(BANK/(name+'.json'));image=Image.open(BANK/(name+'.png')).convert('RGBA')
        assert sha(BANK/(name+'.png'))==sheet['imageSha256']
        derived=copy.deepcopy(sheet);new=name+'LayoutV2';replacements[name]=new
        width=max(f['rect'][2] for f in sheet['frames'])+16
        height=max(f['rect'][3] for f in sheet['frames'])+16
        atlas=Image.new('RGBA',(width*2,height*math.ceil(len(sheet['frames'])/2)),(0,0,0,0))
        for index,(old,frame) in enumerate(zip(sheet['frames'],derived['frames'])):
            x,y,w,h=old['rect'];whole=image.crop((x,y,x+w,y+h))
            px=(index%2)*width+8;py=(index//2)*height+8
            atlas.paste(whole,(px,py))  # No alpha mask: preserve even RGB beneath alpha zero.
            frame['rect']=[px,py,w,h]
            packed=atlas.crop((px,py,px+w,py+h))
            assert packed.tobytes()==whole.tobytes()
            assert frame['pivot']==old['pivot']
            proofs.append({'sheet':new,'frame':frame['id'],'rgbaSha256':hashlib.sha256(whole.tobytes()).hexdigest(),'dimensions':[w,h],'pivotPreserved':True,'rgbaExact':True})
        path=staged/(new+'.png');atlas.save(path)
        derived.update(image=path.name,imageSha256=sha(path),nativeSize=list(atlas.size),status='visual-reviewed',layoutRepairOf=name,layoutRepair='Repacked whole rectangles with 8px margins. No figure edits, resampling, pivot or density changes.')
        write(staged/(new+'.json'),derived)
        old_resource='Darklight/WeaponRedesign20261023/'+name
        for page in catalog['pages']:
            if page['resourcePath']!=old_resource:continue
            p=copy.deepcopy(page);p.update(resourcePath='Darklight/WeaponRedesign20261023/'+new,imageSha256=derived['imageSha256'],nativeSize=derived['nativeSize'],status='visual-reviewed')
            pages.append(p)
    write(OUT/'pages.json',{'pages':pages})
    write(OUT/'rgba-proofs.json',{'frames':proofs,'replacements':replacements,'sourceFilesModified':False,'artworkGenerated':False})
    print(json.dumps({'repackedSheets':len(NAMES),'rgbaExactWholeFigures':len(proofs),'clips':len(pages),'sourceFilesModified':False}))

if __name__=='__main__':main()
