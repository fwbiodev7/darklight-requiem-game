"""Prepare only assigned WalkArmed S/N artwork; no catalog/runtime/Unity/Git."""
import hashlib, importlib.util, json, shutil, statistics
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[2]
SESSION = Path(__file__).resolve().parent
WORK = ROOT.parent / 'work/lucius-continuidade-20261119/armedwalk'
DEST = ROOT / 'Assets/Darklight/Resources/Darklight/LuciusNormal20261109'
NAME = 'LuciusNormalWalkArmedSN20261119V1'
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def save(p, data): p.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

def main():
    src=SESSION/'source-v1.png'
    raw=Image.open(src).convert('RGBA')
    assert raw.size == (1448,1086)
    # Whole 362x362 cells only. Original correct S/N pixels are untouched.
    im=Image.new('RGBA',(1448,724))
    im.paste(raw.crop((0,0,1448,362)),(0,0))
    im.paste(raw.crop((0,724,1448,1086)),(0,362))
    selected=SESSION/(NAME+'-selected.png'); im.save(selected)
    frames=[]; clips=[]; pages=[]; lineage=[]
    phases=['contact-A','passing-B','contact-B','passing-A']
    xs=[198,560,922,1284,187,549,911,1273]
    # Ground anchor is a consistent torso projection. Fore/aft boot tips lie
    # at different image depths; do not normalize every pose to its max bbox.
    anchor_y=[335]*4+[697]*4
    heights=[327,316,321,306,328,316,323,315]
    ppu=float(format(statistics.median(heights)/(410/214.97090106361628),'.12g'))
    for row,direction in enumerate(['S','N']):
        names=[]
        for col in range(4):
            i=row*4+col; x=col*362;y=row*362;source_y=0 if row==0 else 724
            fid=f'Redesign_{NAME}_{i:02d}';names.append(fid)
            frame=im.crop((x,y,x+362,y+362))
            original=raw.crop((x,source_y,x+362,source_y+362))
            assert frame.tobytes()==original.tobytes()
            frames.append({'id':fid,'rect':[x,y,362,362],'pivot':[(xs[i]-x)/362,(y+362-anchor_y[i])/362],'bodyAnchorPx':[xs[i],anchor_y[i]],'durationMs':142,'action':'WalkArmed','direction':direction,'phase':phases[col],'skin':'Lucius','form':'Normal'})
            lineage.append({'frameId':fid,'source':src.relative_to(ROOT).as_posix(),'sourceSha256':sha(src),'sourceRect':[x,source_y,362,362],'rgbaSha256':hashlib.sha256(original.tobytes()).hexdigest(),'preservedPixelForPixel':True})
        clips.append({'name':'WalkArmed_'+direction,'frames':names,'loop':True,'allowStatic':False})
        pages.append({'id':NAME+'_WalkArmed_'+direction,'action':'WalkArmed','direction':0 if row==0 else 4,'resourcePath':'Darklight/LuciusNormal20261109/'+NAME,'imageSha256':sha(selected),'sourceSha256':sha(src),'sourcePath':src.relative_to(ROOT).as_posix(),'nativeSize':list(im.size),'sourceSize':list(raw.size),'pixelsPerUnit':ppu,'spriteNames':names,'durationMs':[142]*4,'loop':True,'wholeFigures':True,'status':'visual-reviewed'})
    references=['Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalGuardFront20261119.png','Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalIdle8V1.png','Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusWalkArmed8V1.png','Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalWalkNSSession20261114.json']
    data={'schemaVersion':1,'image':NAME+'.png','imageSha256':sha(selected),'sourcePath':src.relative_to(ROOT).as_posix(),'sourceSha256':sha(src),'sourceSize':list(raw.size),'nativeSize':list(im.size),'pixelsPerUnit':ppu,'wholeFigures':True,'status':'visual-reviewed','visualApproval':False,'frames':frames,'clips':clips,'requiredClips':[c['name'] for c in clips],'quality':{'alphaThreshold':25,'maxColors':65536,'maxHeightRatio':1.3},'provenance':[{'path':p,'sha256':sha(ROOT/p)} for p in references],'calibration':{'bodyWorldPreScale':410/214.97090106361628,'bodyHeightsPx':heights,'ppuMethod':'median hair-to-boot height divided by approved idle physical body height, no image resizing','pivotMethod':'constant torso ground projection per direction; fore/aft boot image depth is not maximum-bbox normalization','timingMethod':'4 x 142ms = 568ms versus existing Walk 6 x 95ms = 570ms; runtime WalkArmed selection is not currently wired'},'warnings':['W source row is not accepted: contact repetition, rear ankle inversion then softened corrected whole figure. No W page delivered.','Partial alpha and low-alpha generated specks preserved. Visible-alpha bounds inspected; no destructive background cleanup.','Generated raster is detailed with antialiasing; native logical pixel grid is unverified.','No Unity import, runtime or animated-preview observation has been performed.']}
    out=DEST/(NAME+'.png'); assert not out.exists(),'Refusing overwrite'
    shutil.copyfile(selected,out);save(out.with_suffix('.json'),data)
    spec=importlib.util.spec_from_file_location('metadata_helper',ROOT/'Tools/AnimationPreview/prepare_fists_combat_stance_20261118.py')
    helper=importlib.util.module_from_spec(spec);spec.loader.exec_module(helper)
    Path(str(out)+'.meta').write_bytes(helper.texture_meta(Path(str(out)+'.meta'),im.size,ppu,frames))
    meta=Path(str(out.with_suffix('.json'))+'.meta');meta.write_bytes(helper.text_meta(meta))
    save(SESSION/'whole-frame-provenance.json',lineage)
    save(SESSION/'catalog-snippet.json',{'note':'Coordinator only: replace WalkArmed S/N contexts. Does not wire runtime selection or certify Unity import. W and remaining directions unchanged.','pages':pages})
    save(WORK/'prepared-state.json',{'produced':True,'staticVisualReviewed':True,'copiedToAssets':True,'catalogIntegrated':False,'unityImported':False,'runtimeVerified':False,'animatedPreviewObserved':False,'imageSha256':sha(out),'sourceSha256':sha(src),'frames':8,'directions':['S','N'],'generationCalls':4,'pending':['W corrected figure softened; remain candidate','SW/NW/NE/E/SE WalkArmed cycles absent','runtime currently selects Walk rather than WalkArmed','animated preview observation and Unity import/runtime pending']})
    contact=Image.new('RGBA',(1448,800),(38,41,46,255));draw=ImageDraw.Draw(contact)
    for i,f in enumerate(frames):
        x,y,w,h=f['rect'];ax,ay=f['bodyAnchorPx'];dx=i%4*362+181-(ax-x);dy=i//4*400+365-(ay-y)
        contact.alpha_composite(im.crop((x,y,x+w,y+h)),(round(dx),round(dy)))
        draw.text((i%4*362+10,i//4*400+8),f['direction']+' '+f['phase'],fill='white')
        draw.line((i%4*362+171,i//4*400+365,i%4*362+191,i//4*400+365),fill='red')
    contact.save(WORK/'contact-anchors-native.png')
    contact.resize((724,400),Image.Resampling.NEAREST).save(WORK/'contact-anchors-game.png')
    save(SESSION/'delivery-hashes.json',[{'path':p.relative_to(ROOT).as_posix(),'sha256':sha(p)} for p in [out,out.with_suffix('.json'),Path(str(out)+'.meta'),Path(str(out.with_suffix('.json'))+'.meta'),src,selected]])
    print(json.dumps({'asset':out.relative_to(ROOT).as_posix(),'sha256':sha(out),'sourceSha256':sha(src),'frames':8,'directions':['S','N'],'ppu':ppu,'state':'static-visual-reviewed / copied Assets / not catalog-integrated / not Unity-imported'}))
if __name__=='__main__':main()
