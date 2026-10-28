"""Technical whole-frame packaging of the corrected low walking cycle."""
from pathlib import Path
from types import SimpleNamespace
import argparse
import json
import numpy as np
from PIL import Image
from prepare_eclipse_fangs_preview import ROOT, extract, palette, translate_preview, sp

OUT=ROOT/'outputs/punhos-ruina-20261021/caminhada-natural-v3'
SOURCE=ROOT/'ArtSources/RuinFists20261021/generated/walk-west-natural-v3.png'

def main(corrected=False):
    global OUT
    if corrected:OUT=ROOT/'outputs/punhos-ruina-20261021/caminhada-natural-v4'
    if OUT.exists():raise ValueError('Preserve earlier versions; choose a new destination')
    OUT.mkdir(parents=True)
    source,figures=extract(SOURCE,2)
    originals=[f['image'].tobytes() for f in figures]
    if corrected:
        replacement_source=ROOT/'ArtSources/RuinFists20261021/generated/walk-alternation-candidate.png'
        _,replacements=extract(replacement_source,2,columns=2)
        for skin in range(2):
            scale=figures[skin*4]['bodyHeight']/replacements[skin*2]['bodyHeight']
            for phase in range(2):
                index=skin*4+phase+2;replacement=replacements[skin*2+phase]
                tile=replacement['image'];size=tuple(round(n*scale) for n in tile.size)
                tile=tile.resize(size,Image.Resampling.NEAREST)
                figures[index]=dict(image=tile,sourceRect=[0,0,*size],bodyHeight=round(replacement['bodyHeight']*scale),
                    solidRect=[0,0,*size],replacementSource=str(replacement_source.relative_to(ROOT)),
                    replacementSourceRect=replacement['sourceRect'],wholeFigureUniformScale=scale)
        assert all(figures[i]['image'].tobytes()==originals[i] for i in [0,1,4,5])
    anchors=[]
    for f in figures:
        tile=np.array(f['image']);yy,xx=np.where(tile[:,:,3]>=128)
        top=yy.min();head=xx[yy<top+f['bodyHeight']*.24]
        anchors.append([int(np.median(head)),f['solidRect'][3]-2-f['sourceRect'][1]])
    left=max(a[0] for a in anchors)+16;top=max(a[1] for a in anchors)+16
    right=max(f['image'].width-a[0] for f,a in zip(figures,anchors))+16
    bottom=max(f['image'].height-a[1] for f,a in zip(figures,anchors))+16
    cell=(left+right,top+bottom)
    atlas=Image.new('RGBA',(cell[0]*4,cell[1]*2));frames=[];provenance=[]
    for i,(f,a) in enumerate(zip(figures,anchors)):
        canvas=Image.new('RGBA',cell);offset=(left-a[0],top-a[1]);canvas.paste(f['image'],offset)
        assert canvas.crop((*offset,offset[0]+f['image'].width,offset[1]+f['image'].height)).tobytes()==f['image'].tobytes()
        x,y=(i%4)*cell[0],(i//4)*cell[1];atlas.paste(canvas,(x,y))
        frames.append(dict(id=f'{"Requiem" if i<4 else "Rock"}_walk_W_{i%4}',rect=[x,y,*cell],
                           pivot=[left/cell[0],1-top/cell[1]],durationMs=150,action='walk',direction='W',
                           skin='Requiem' if i<4 else 'Rock',form='normal'))
        provenance.append(dict(sourceRect=f['sourceRect'],offset=list(offset),wholeFigure=True,
            rgbaPreserved='replacementSource' not in f,originalFrameUnchanged=i in [0,1,4,5],
            replacement={k:v for k,v in f.items() if k.startswith('replacement') or k=='wholeFigureUniformScale'}))
    atlas.save(OUT/'sheet.png')
    clips=[dict(name=skin+'_caminhada_natural_W',frames=[f['id'] for f in frames if f['skin']==skin],loop=True) for skin in ['Requiem','Rock']]
    data=dict(schemaVersion=1,image='sheet.png',imageSha256=sp.digest(OUT/'sheet.png'),nativeSize=list(atlas.size),
              pixelsPerUnit=figures[0]['bodyHeight']/1.99,status='candidate',visualApproval=False,imported=False,runtimeVerified=False,
              frames=frames,clips=clips,requiredClips=[c['name'] for c in clips],quality=dict(maxColors=256,maxHeightRatio=1.3),
              sourceSize=list(source.size),logicalPixelResolution=None,nativePixelArtCertified=False,
              provenance=dict(source=str(SOURCE.relative_to(ROOT)),sourceSha256=sp.digest(SOURCE),frames=provenance),
              notes=['User rejected high-knee marching; these are newly generated complete figures with lower steps.',
                     'No planar rotation, mirroring, anatomical transplant, leg warp, interpolation or simulated bobbing.',
                     'Four authored phase drawings per skin. 150 ms/phase is for inspection; gameplay calibration pending.',
                     'V4 replaces only full frames 2,3,6,7; frames 0,1,4,5 retain exact RGBA. Complete replacement figures use uniform nearest-neighbor scaling.',
                     'Ground baseline annotated; horizontal anchor estimated from full-figure head region, visual calibration required.',
                     'RGBA source conserved; GIF quantizes colors and is only an inspection preview.'])
    sp.write(OUT/'sheet.json',data)
    audit=sp.audit(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'audit',unity_meta=None))
    assert audit['ok'],audit
    sp.preview(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'preview',zoom=1))
    translate_preview(OUT/'preview/preview.html','Punhos da Ruína — caminhada natural candidata')
    html=(OUT/'preview/preview.html').read_text(encoding='utf-8').replace('Visual aprovado em 21/10/2026.','Caminhada candidata após correção de marcha em 21/10/2026.')
    (OUT/'preview/preview.html').write_text(html,encoding='utf-8')
    colors=palette(OUT/'sheet.png',OUT)
    sp.write(OUT/'QA.json',dict(frames=8,sourceSize=list(source.size),atlasSize=list(atlas.size),
                              audit=audit,colors=colors,completeFiguresOnly=True,rgbaConservation=True,
                              animationVisuallyReviewed=False,unityImported=False,runtimeVerified=False))
    print(json.dumps(dict(frames=8,errors=audit['errors'],warnings=audit['warnings'],preview=str(OUT/'preview/preview.html'))))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--corrected',action='store_true')
    main(parser.parse_args().corrected)
