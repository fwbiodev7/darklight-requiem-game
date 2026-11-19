from pathlib import Path
from PIL import Image,ImageDraw
import shutil,json,hashlib,uuid,re
root=Path(__file__).resolve().parent
rawpath=Path('C:/Users/Usuario/.codex/generated_images/01a1f5bf-9577-74d0-b04b-68cb07b1200b/exec-40b3aa97-36c3-4944-8059-3c5d3ee759d7.png')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
shutil.copy2(rawpath,root/'Lady-Idle-S-v2-raw.png')
raw=Image.open(rawpath).convert('RGBA'); b=raw.getchannel('A').point(lambda v:255 if v>=8 else 0).getbbox()
rect=(b[0]-16,b[1]-16,b[2]+16,b[3]+16)
im=raw.crop(rect); out=root/'Lady-Idle-S-candidate-v2.png'; im.save(out)
assert im.tobytes()==raw.crop(rect).tobytes()
body=b[3]-b[1]; ppu=body/302*157.5757576; pivot=[(610-rect[0])/im.width,(rect[3]-1182)/im.height]
guid=uuid.uuid5(uuid.NAMESPACE_URL,'darklight:LadyDesignCanon20261119:IdleS:candidate-v2').hex
m=(root/'Lady-Idle-S-candidate-v1.png.meta').read_text(encoding='utf-8')
m=re.sub(r'guid: [0-9a-f]+','guid: '+guid,m)
m=re.sub(r'  spritePivot: .*',f'  spritePivot: {{x: {pivot[0]}, y: {pivot[1]}}}',m)
m=re.sub(r'  spritePixelsToUnits: .*',f'  spritePixelsToUnits: {ppu}',m)
m=re.sub(r'    spriteID: [0-9a-f]+','    spriteID: '+uuid.uuid5(uuid.NAMESPACE_URL,guid).hex,m)
out.with_suffix('.png.meta').write_text(m,encoding='utf-8')
j=json.loads((root/'Lady-Idle-S-candidate-v1.json').read_text(encoding='utf-8'))
j.update(image=out.name,imageSha256=sha(out),nativeSize=list(im.size),pixelsPerUnit=ppu)
j['frames'][0].update(rect=[0,0,*im.size],pivot=pivot)
j['provenance'].update(rawImage='Lady-Idle-S-v2-raw.png',rawSha256=sha(rawpath),rawCropTopLeftXYXY=list(rect),operation='second integrated call: whole-figure proportion correction using exact Lady + Fabius references; technical whole-figure crop afterward')
j['calibration'].update(bodyHeightPx=body,bodyWorldHeight=body/ppu*1.325,supportRawXY=[610,1182],rawSize=list(raw.size),rawVisibleBounds=list(b),headCrownToChinApproxRawPx=308,headToBodyRatioApprox=308/body)
j['limits'][2]='not exact source pixels: entire figure redrawn for proportions; exact original and first candidate preserved'
j['limits'].append('approximate manually inspected crown-to-chin/head ratio, not an automated anatomy measurement')
(root/'Lady-Idle-S-candidate-v2.json').write_text(json.dumps(j,ensure_ascii=False,indent=2),encoding='utf-8')
preview=Image.new('RGBA',(1080,380),(43,45,52,255));d=ImageDraw.Draw(preview);d.line((12,344,1068,344),fill=(150,150,155))
figures=[('Fabius Normal',Image.open(root/'Fabius-Normal-S-reference.png'),(53,26,386,333),302),('Lady v1 (mais esguia)',Image.open(root/'Lady-Idle-S-candidate-v1.png'),(16,16,533,980),964),('Lady v2 (selecionada)',im,(16,16,im.width-16,im.height-16),body)]
for idx,(label,fig,bounds,h) in enumerate(figures):
    whole=fig.crop(bounds); scaled=whole.resize((round(whole.width*302/h),round(whole.height*302/h)),Image.Resampling.NEAREST)
    preview.alpha_composite(scaled,(idx*360+(360-scaled.width)//2,344-scaled.height));d.text((idx*360+24,14),label,fill='white');d.text((idx*360+24,360),'corpo: 302 px equivalentes',fill=(215,215,220))
preview.convert('RGB').save(root/'Lady-v1-v2-Fabius-proportions-1x.png')
preview.resize((2160,760),Image.Resampling.NEAREST).convert('RGB').save(root/'Lady-v1-v2-Fabius-proportions-2x.png')
print(json.dumps({'sha256':sha(out),'rawSha256':sha(rawpath),'size':im.size,'bodyHeightPx':body,'ppu':ppu,'pivot':pivot,'guid':guid,'headToBodyApprox':308/body}))
