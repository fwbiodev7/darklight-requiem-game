"""Pack complete newly authored low-walk figures; do not certify their anatomy."""
from pathlib import Path
from types import SimpleNamespace
import json
import argparse
import numpy as np
from PIL import Image
from prepare_eclipse_fangs_preview import ROOT,extract,palette,translate_preview,sp

OUT=ROOT/'outputs/punhos-ruina-20261021/caminhada-baixa-v5'

def main(guided=False):
    global OUT
    if guided:OUT=ROOT/'outputs/punhos-ruina-20261021/caminhada-guiada-v11'
    if OUT.exists():raise ValueError('Preserve versions; output already exists')
    OUT.mkdir(parents=True)
    figures=[];sources=[]
    for skin in ['Requiem','Rock']:
        if guided:
            filename='walk-west-guided-v9.png' if skin=='Requiem' else 'Rock_Normal_Walk_W_GuidedV10.png'
            path=ROOT/'ArtSources/RuinFists20261021/generated'/filename
            source,tiles=extract(path,2 if skin=='Requiem' else 1,columns=4)
            tiles=tiles[:4]
        else:
            path=ROOT/f'ArtSources/RuinFists20261021/generated/{skin}_Normal_Walk_W_EightPhasesV5.png'
            source,tiles=extract(path,2)
        sources.append(dict(skin=skin,path=path.relative_to(ROOT).as_posix(),sha256=sp.digest(path),nativeSize=list(source.size)))
        factor=figures[0]['bodyHeight']/tiles[0]['bodyHeight'] if guided and skin=='Rock' else 1
        for i,f in enumerate(tiles):
            if factor!=1:
                f['image']=f['image'].resize(tuple(round(n*factor) for n in f['image'].size),Image.Resampling.NEAREST)
                f['bodyHeight']=round(f['bodyHeight']*factor)
            a=np.array(f['image']);ys,xs=np.where(a[:,:,3]>=128)
            head_x=int(np.median(xs[ys<ys.min()+f['bodyHeight']*.23]))
            f.update(skin=skin,phase=i,sourceIndex=len(sources)-1,
                     wholeFigureUniformScale=factor,
                     localAnchor=[head_x,round((f['sourceGroundAnchor'][1]-f['sourceRect'][1])*factor)])
        figures.extend(tiles)
    left=max(f['localAnchor'][0] for f in figures)+12
    ground=max(f['localAnchor'][1] for f in figures)+12
    right=max(f['image'].width-f['localAnchor'][0] for f in figures)+12
    bottom=max(f['image'].height-f['localAnchor'][1] for f in figures)+12
    cell=(left+right,ground+bottom)
    atlas=Image.new('RGBA',(cell[0]*4,cell[1]*(len(figures)//4)));frames=[];provenance=[]
    for i,f in enumerate(figures):
        offset=[left-f['localAnchor'][0],ground-f['localAnchor'][1]]
        x,y=i%4*cell[0],i//4*cell[1]
        atlas.paste(f['image'],(x+offset[0],y+offset[1]))
        assert atlas.crop((x+offset[0],y+offset[1],x+offset[0]+f['image'].width,y+offset[1]+f['image'].height)).tobytes()==f['image'].tobytes()
        frames.append(dict(id=f"{f['skin']}_walk_W_{f['phase']}",rect=[x,y,*cell],pivot=[left/cell[0],1-ground/cell[1]],
                           durationMs=150 if guided else 100,action='walk',skin=f['skin'],direction='W',form='normal'))
        provenance.append(dict(sourceIndex=f['sourceIndex'],sourceRect=f['sourceRect'],offset=offset,
                               sourceGroundAnchor=f['sourceGroundAnchor'],wholeFigure=True,
                               wholeFigureUniformScale=f['wholeFigureUniformScale'],rgbaPreserved=f['wholeFigureUniformScale']==1))
    atlas.save(OUT/'sheet.png')
    clips=[dict(name=s+'_caminhada_baixa_W',frames=[f['id'] for f in frames if f['skin']==s],loop=True) for s in ['Requiem','Rock']]
    data=dict(schemaVersion=1,image='sheet.png',imageSha256=sp.digest(OUT/'sheet.png'),nativeSize=list(atlas.size),
              pixelsPerUnit=figures[0]['bodyHeight']/1.99,status='candidate',visualApproval=False,imported=False,runtimeVerified=False,
              frames=frames,clips=clips,requiredClips=[c['name'] for c in clips],quality=dict(maxColors=256,maxHeightRatio=1.3),
              logicalPixelResolution=None,nativePixelArtCertified=False,provenance=dict(sources=sources,frames=provenance),
              notes=['Whole new drawings with low knees; technical conservation does not certify alternating legs.',
                     'Ground anchor taken from connected complete body, not faint alpha fringe or full image bounds.',
                     'No mirroring, anatomical transplants, interpolation, deformation or vertical canvas bobbing.',
                     '150 ms/frame for guided four-phase cycle; 100 ms/frame for older eight-phase study. Gameplay calibration pending.',
                     'Guided V10 preserves four Requiem drawings; Rock is four new complete costume-corrected figures, uniformly scaled with nearest-neighbor for preview.'])
    sp.write(OUT/'sheet.json',data)
    result=sp.audit(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'audit',unity_meta=None))
    if not result['ok']:raise ValueError(result)
    sp.preview(SimpleNamespace(manifest=OUT/'sheet.json',out=OUT/'preview',zoom=1))
    path=OUT/'preview/preview.html';translate_preview(path,'Punhos da Ruína — revisão da caminhada lateral')
    path.write_text(path.read_text(encoding='utf-8').replace('Visual aprovado em 21/10/2026. Poses principais de inspeção; intermediários e gameplay pendentes.',
        'Passos baixos candidatos. Alternância das pernas em revisão; ainda fora do jogo.'),encoding='utf-8')
    colors=palette(OUT/'sheet.png',OUT)
    qa=dict(frames=len(figures),completeFiguresOnly=True,sourceRGBAConserved=not guided,
            rgbaPreservedForUnscaledFrames=True,wholeFigureScalingOnly=True,integerGroundAnchors=True,
            atlasSize=list(atlas.size),audit=result,colors=colors,animationVisuallyReviewed=False,
            lowKneesReviewed=True,oppositeLegContactConfirmed=False,unityImported=False,runtimeVerified=False)
    sp.write(OUT/'QA.json',qa)
    print(json.dumps(dict(frames=len(figures),errors=result['errors'],warnings=result['warnings'],preview=str(path))))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--guided',action='store_true')
    main(parser.parse_args().guided)
