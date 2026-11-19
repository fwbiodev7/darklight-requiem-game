"""Prepare this assigned sheet only; no catalog, C#, Git or Unity mutation."""
import hashlib,importlib.util,json,shutil,statistics
from pathlib import Path
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[2]
SESSION=Path(__file__).resolve().parent
DEST=ROOT/'Assets/Darklight/Resources/Darklight/LuciusArconte20261111'
NAME='LuciusArconteSwordThrustBack20261119V1'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def main():
    src=SESSION/'source-v1.png'; im=Image.open(src).convert('RGBA')
    # Rectangles measured in untouched full source. Last row's contact includes
    # the complete lance flash with 29px right margin at alpha>8.
    rows=[(0,384,[0,362,783,1247]),(384,798,[0,411,824,1247]),(798,1261,[0,420,860,1247])]
    anchors=[(217,366),(612,365),(1011,364),(227,785),(618,785),(1013,787),(225,1166),(633,1162),(1022,1166)]
    # Hair top and boot support, visually annotated; aura/sword excluded.
    body_heights=[272,278,269,284,284,285,281,284,282]
    ppu=float(format(statistics.median(body_heights)/(410/214.97090106361628),'.12g'))
    refs=['Docs/Darklight/References/LuciusAttackReference20261110/Lucius_Attack_Reference_Primary_20261110.png','Assets/Darklight/Resources/Darklight/LuciusArconte20261111/LuciusArconteAttacksBackSession20261114.png','Assets/Darklight/Resources/Darklight/LuciusArconte20261111/LuciusArconteIdle13.png']
    frames=[];clips=[];pages=[];phase=['anticipation','contact','recovery'];times=[120,200,220]
    for row,(y0,y1,xs) in enumerate(rows):
        direction=['NW','N','NE'][row];names=[]
        for col,(x0,x1) in enumerate(zip(xs,xs[1:])):
            i=row*3+col;a=anchors[i];w=x1-x0;h=y1-y0
            fid=f'Redesign_{NAME}_{i:02d}';names.append(fid)
            frames.append({'id':fid,'rect':[x0,y0,w,h],'pivot':[(a[0]-x0)/w,(y1-a[1])/h],'bodyAnchorPx':list(a),'bodyHeightPx':body_heights[i],'durationMs':times[col],'action':'Attack','attackKey':'SwordThrust','direction':direction,'phase':phase[col],'skin':'Lucius','form':'Arconte'})
        clips.append({'name':f'SwordThrust_{direction}','frames':names,'loop':False,'allowStatic':False})
        pages.append({'id':f'{NAME}_SwordThrust_{direction}','action':'Attack','attackKey':'SwordThrust','direction':[3,4,5][row],'resourcePath':'Darklight/LuciusArconte20261111/'+NAME,'imageSha256':sha(src),'sourceSha256':sha(src),'sourcePath':src.relative_to(ROOT).as_posix(),'nativeSize':list(im.size),'sourceSize':list(im.size),'pixelsPerUnit':ppu,'spriteNames':names,'durationMs':times,'startupFrames':1,'activeFrames':1,'loop':False,'wholeFigures':True,'status':'visual-reviewed'})
    data={'schemaVersion':1,'image':NAME+'.png','imageSha256':sha(src),'sourcePath':src.relative_to(ROOT).as_posix(),'sourceSha256':sha(src),'sourceSize':list(im.size),'nativeSize':list(im.size),'logicalPixelSize':'unverified; generated raster has antialiasing, no native pixel-grid claim','pixelsPerUnit':ppu,'wholeFigures':True,'status':'visual-reviewed','visualApproval':False,'frames':frames,'clips':clips,'requiredClips':[c['name'] for c in clips],'quality':{'alphaThreshold':8,'maxColors':65536,'maxHeightRatio':1.3},'provenance':[{'path':p,'sha256':sha(ROOT/p),'sourceSize':list(Image.open(ROOT/p).size)} for p in refs],'calibration':{'method':'median visually measured hair-to-boot height / approved idle physical body height','bodyWorldPreScale':410/214.97090106361628,'measuredBodyHeightsPx':body_heights,'pivotMethod':'projected torso/support anchor; sword and aura excluded; no image pixel edits','timingMethod':'[120,200,220] retains old total540ms; runtime AttackTime remaps preparation/contact/recovery to authored attack timing'},'warnings':['Strong foreshortening in N places projected sword above head. No swing arc; Unity contact readability remains unverified.','Image contains partial alpha and almost transparent generation specks. Source bytes preserved; meaningful figure/effect bounds checked at alpha>8.','Frames not tested or imported in Unity.']}
    out=DEST/(NAME+'.png'); assert not out.exists(),'Refusing overwrite'
    shutil.copyfile(src,out);save(out.with_suffix('.json'),data)
    spec=importlib.util.spec_from_file_location('stance_helper',ROOT/'Tools/AnimationPreview/prepare_fists_combat_stance_20261118.py');helper=importlib.util.module_from_spec(spec);spec.loader.exec_module(helper)
    Path(str(out)+'.meta').write_bytes(helper.texture_meta(Path(str(out)+'.meta'),im.size,ppu,frames))
    json_meta=Path(str(out.with_suffix('.json'))+'.meta')
    json_meta.write_bytes(helper.text_meta(json_meta))
    save(SESSION/'catalog-snippet.json',{'note':'Replace matching Attack:direction:SwordThrust pages; coordinator only. Not yet integrated.','pages':pages})
    save(SESSION/'provenance.json',{'source':data['sourcePath'],'sourceSha256':sha(src),'generatedBy':'built-in image_gen','calls':1,'references':data['provenance'],'produced':True,'visualReviewed':True,'copiedToAssets':True,'unityImported':False,'runtimeVerified':False,'catalogIntegrated':False,'rejectedFrames':[],'warnings':data['warnings']})
    # Intact frames on common canvas, using measured body anchor; image pixel
    # changes below are previews only. Delivered PNG remains byte-identical.
    contact=Image.new('RGBA',(1500,1350),(30,32,36,255));d=ImageDraw.Draw(contact)
    for i,f in enumerate(frames):
        x,y,w,h=f['rect'];a=f['bodyAnchorPx'];row=i//3;col=i%3
        ox=col*500+250-(a[0]-x);oy=row*450+400-(a[1]-y)
        contact.alpha_composite(im.crop((x,y,x+w,y+h)),(round(ox),round(oy)))
        d.text((col*500+8,row*450+8),str(i)+' '+f['direction']+' '+f['phase'],fill='white')
        d.line((col*500+238,row*450+400,col*500+262,row*450+400),fill='red',width=2)
    contact.save(SESSION/'review-contact-native.png')
    contact.resize((750,675),Image.Resampling.NEAREST).save(SESSION/'review-contact-game.png')
    print(json.dumps({'asset':out.relative_to(ROOT).as_posix(),'sha256':sha(out),'frames':len(frames),'ppu':ppu,'status':'visual-reviewed','imported':False}))
if __name__=='__main__':main()
