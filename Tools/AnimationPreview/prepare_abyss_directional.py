"""Register intact authored directional figures; never assemble character parts."""
import hashlib,json
from pathlib import Path
from PIL import Image
from whole_figure_components import identify
from prepare_consistency import install
from prepare_living_idle import descriptor_frames
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/finalizacao-20261018'
RES=ROOT/'Assets/Darklight/Resources/Darklight/AbyssPrototype'
ART=ROOT/'Assets/Darklight/ArtSources/AbyssDirectional20261018'

def main():
    validated={}
    for form in range(1,4):
        for view in ['W','NW','N']:
            name=f'ActionF{form}{view}';path=OUT/(name+'.png')
            im=Image.open(path).convert('RGBA');frames,labels=identify(im,4,3)
            if im.getchannel('A').getextrema()[0]!=0 or len(frames)!=12 or any(f['touchesExtractionEdge'] for f in frames):
                raise ValueError('Incomplete directional whole-figure page: '+name)
            validated[name]=(path,im,frames,labels)
    manifest=dict(pages=[]);audit=[]
    ownership=json.loads((RES/'WholeOwnership.json').read_text())
    ownership['pages']=[p for p in ownership['pages'] if not p['name'].startswith('ActionF')]
    for view in ['W','NW','N']:
        normal=Image.open(RES/f'ActionF0{view}.png').convert('RGBA');normal_frames,normal_labels=identify(normal,4,3)
        target=normal_frames[0]['bodyHeight']/167.838
        owner=Image.new('RGBA',normal.size,(0,0,0,0));owner.putalpha(Image.fromarray(normal_labels));owner.save(RES/f'ActionF0{view}_Owner.png')
        ownership['pages'].append(dict(name=f'ActionF0{view}',spriteNames=[f'Abyss_ActionF0{view}_0_{i}' for i in range(12)]))
        for form in range(1,4):
            name=f'ActionF{form}{view}';path,im,frames,labels=validated[name]
            ppu=frames[0]['bodyHeight']/target
            sprites=descriptor_frames(frames,im,'Abyss_'+name)
            for i,s in enumerate(sprites):s['name']=f'Abyss_{name}_{i//3}_{i%3}'
            install(path,RES/(name+'.png'),RES/(name+'_Owner.png'),frames,labels)
            manifest['pages'].append(dict(name=name,ppu=ppu,sprites=sprites))
            ownership['pages'].append(dict(name=name,spriteNames=[s['name'] for s in sprites]))
            audit.append(dict(name=name,wholeFrames=len(frames),ppu=ppu,sourceSha256=hashlib.sha256(path.read_bytes()).hexdigest(),frames=frames))
    ART.mkdir(parents=True,exist_ok=True)
    (ART/'manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
    (RES/'WholeOwnership.json').write_text(json.dumps(ownership,indent=2),encoding='utf-8')
    (OUT/'direcionais-manifesto.json').write_text(json.dumps(dict(pages=audit,sourcePNGsUnchanged=True,integrationVerified=False),indent=2),encoding='utf-8')
    print(json.dumps(dict(newPages=len(audit),newWholeFrames=sum(p['wholeFrames'] for p in audit))))
if __name__=='__main__':main()
