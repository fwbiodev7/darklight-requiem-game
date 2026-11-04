"""Package manually reviewed complete figures without resampling source pixels."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
SOURCE = Path(__file__).resolve().parent
OUT = ROOT / 'outputs/retomada-20261103/essential-states'
DIRECTIONS = ['S', 'SW', 'W', 'NW', 'N', 'NE', 'E', 'SE']

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def write(p, value):
    p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('recipe', type=Path)
    args = parser.parse_args()
    recipe = json.loads(args.recipe.read_text(encoding='utf-8-sig'))
    all_pages, reports = [], []
    for spec in recipe['sheets']:
        source = SOURCE / spec['source']
        original = Image.open(source).convert('RGBA')
        name, action, status = spec['name'], spec['action'], spec['status']
        target = OUT / 'staging' / (name + '.png')
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source, target)
        frames, clips, pages = [], [], []
        qa = OUT / 'qa' / name
        qa.mkdir(parents=True, exist_ok=True)
        for color, suffix in [((28, 31, 38, 255), 'dark'), ((239, 235, 228, 255), 'light')]:
            background = Image.new('RGBA', original.size, color)
            background.alpha_composite(original)
            background.convert('RGB').save(qa / ('native-' + suffix + '.png'))
        for index, f in enumerate(spec['figures']):
            x, y, w, h = f['rect']
            assert 0 <= x < x+w <= original.width and 0 <= y < y+h <= original.height
            ax, ay = f['anchor']
            assert x <= ax <= x+w and y <= ay <= y+h
            bbox = original.crop((x,y,x+w,y+h)).getchannel('A').point(lambda a: 255 if a > 16 else 0).getbbox()
            assert bbox and bbox[0] > 0 and bbox[1] > 0 and bbox[2] < w and bbox[3] < h, (name, index, bbox)
            frames.append({'id': 'Redesign_' + name + '_' + str(index).zfill(2),
                           'rect': f['rect'], 'pivot': [(ax-x)/w, 1-(ay-y)/h],
                           'durationMs': f['durationMs'], 'action': action.lower(),
                           'direction': f['direction'], 'skin': 'Requiem', 'form': 'Normal',
                           'phase': f['phase'], 'sourceRect': f['rect'],
                           'sourceAnchor': f['anchor'], 'sourcePackingAnchor': f['anchor'],
                           'sourceFrameIndex': index, 'sourceScale': 1,
                           'supportGuide': {'anchor': f['anchor'], 'method': f['anchorMethod']},
                           'pixelsPerUnit': f.get('pixelsPerUnit', spec['pixelsPerUnit']),
                           'visibleAlphaBoundsInFrame': list(bbox)})
            if f.get('guardContactSource'):
                cx,cy=f['guardContactSource']
                frames[-1]['guardContact']=[(cx-x)/w,1-(cy-y)/h]
                frames[-1]['guardContactSource']=f['guardContactSource']
                frames[-1]['guardContactVisibility']=f.get('guardContactVisibility','visible-tooth-junction')
            for key in ('originalSourcePath','originalSourceRect','originalSourceAnchor','sourceRGBAEqualityVerified'):
                if key in f:
                    frames[-1][key]=f[key]
        for direction in dict.fromkeys(f['direction'] for f in frames):
            selected = [f for f in frames if f['direction'] == direction]
            clip_name = action + '_' + direction
            clips.append({'name': clip_name, 'frames': [f['id'] for f in selected],
                          'loop': False, 'allowStatic': len(selected) == 1,
                          'holdLastFrame': action == 'Dead',
                          'intentionalStaticReason': 'Single braced contact guard held for the 260ms parry window.' if action == 'Parry' else ''})
            common = {'imageSha256': sha(target), 'sourcePath': source.relative_to(ROOT).as_posix(),
                      'sourceSha256': sha(source), 'nativeSize': list(original.size),
                      'sourceSize': list(original.size), 'pixelsPerUnit': spec['pixelsPerUnit'],
                      'status': status, 'wholeFigures': True}
            page = dict(common, id=name+'_'+clip_name, skin=0, weapon=spec['weapon'], form=0,
                        direction=DIRECTIONS.index(direction), action=action, attackKey='',
                        resourcePath='Darklight/WeaponRedesign20261023/'+name,
                        spriteNames=[f['id'] for f in selected],
                        framePixelsPerUnit=[f['pixelsPerUnit'] for f in selected],
                        durationMs=[f['durationMs'] for f in selected], loop=False)
            if spec['weapon'] == 3:
                page['presentation'] = 'OnFoot'
            if len(selected)==1 and selected[0].get('guardContact'):
                page['guardContact']=selected[0]['guardContact']
                page['guardContactVisibility']=selected[0]['guardContactVisibility']
            pages.append(page)
            left=max(round(f['pivot'][0]*f['rect'][2]) for f in selected)
            right=max(f['rect'][2]-round(f['pivot'][0]*f['rect'][2]) for f in selected)
            above=max(round((1-f['pivot'][1])*f['rect'][3]) for f in selected)
            below=max(f['rect'][3]-round((1-f['pivot'][1])*f['rect'][3]) for f in selected)
            animation=[]
            for f in selected:
                x,y,w,h=f['rect']
                preview=Image.new('RGBA',(left+right+16,above+below+16),(28,31,38,255))
                preview.alpha_composite(original.crop((x,y,x+w,y+h)),
                    (8+left-round(f['pivot'][0]*w),8+above-round((1-f['pivot'][1])*h)))
                animation.append(preview.convert('RGB'))
            save_args={'save_all':True,'append_images':animation[1:],
                       'duration':[f['durationMs'] for f in selected], 'disposal':2}
            # Deliberately omit GIF loop metadata: finite actions and corpse hold.
            animation[0].save(qa/(clip_name+'.gif'),**save_args)
        sheet = dict(common, schemaVersion=1, image=target.name, frames=frames, clips=clips,
                     requiredClips=[c['name'] for c in clips],
                     worldVisualScale=1.325, sourcePixelResampling=False,
                     references=recipe['references'], review=spec['review'],
                     quality={'alphaThreshold':16, 'maxColors':65536, 'maxHeightRatio':2.5})
        if spec.get('generationReferences'):
            sheet['generationReferences']=[{'path':p,'sha256':sha(ROOT/p)} for p in spec['generationReferences']]
        if spec.get('sourceArtifacts'):
            sheet['sourceArtifacts']=[{'path':p,'sha256':sha(ROOT/p),'nativeSize':list(Image.open(ROOT/p).size)} for p in spec['sourceArtifacts']]
            sheet['derivedWholeFigurePacking']=True
        write(target.with_suffix('.json'), sheet)
        write(target.with_suffix('.pages.json'), {'pages':pages})
        all_pages.extend(pages)
        reports.append({'name':name,'status':status,'figures':len(frames),'clips':len(clips),
                        'sourceSha256':sha(source),'imageSha256':sha(target),
                        'nativeSize':list(original.size),'imported':False,'runtimeVerified':False})
    write(OUT/'catalog-fragment.json', {'schemaVersion':1,'complete':False,'pages':all_pages})
    for weapon,label in [(0,'katana'),(2,'fists'),(3,'moto')]:
        selected=[p for p in all_pages if p['weapon']==weapon]
        if selected:
            write(OUT/('catalog-fragment-'+label+'.json'),{'schemaVersion':1,'complete':False,'pages':selected})
    write(OUT/'packet-summary.json', {'packets':reports,'assetsModified':False,'unityExecuted':False})
    print(json.dumps({'sheets':len(reports),'figures':sum(r['figures'] for r in reports),
                      'pages':len(all_pages),'statuses':list(set(r['status'] for r in reports))}))

if __name__ == '__main__':
    main()
