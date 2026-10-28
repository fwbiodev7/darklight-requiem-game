"""Silent whole-figure viewer from the current Unity catalogs; never writes Assets."""
from __future__ import annotations
import copy
import hashlib
import json
import re
import struct
from collections import Counter
from pathlib import Path
from urllib.parse import quote

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'outputs/previa-armas-20261024'
RES = ROOT / 'Assets/Darklight/Resources'
DIRS = ['S', 'SO', 'O', 'NO', 'N', 'NE', 'L', 'SE']
FORMS = ['Normal', 'Ruptura', 'Eclipse', 'Absoluto']
FRAMES, SHEETS, CLIPS, ISSUES = {}, {}, [], []


def read(path):
    return json.loads(Path(path).read_text(encoding='utf-8-sig'))


def scalar(text, key, default=''):
    m = re.search(r'^\s*' + re.escape(key) + r':[ \t]*(.*)$', text, re.M)
    return m[1].strip().strip('"') if m else default


def packed(text):
    raw = re.match('[0-9a-fA-F]*', text).group()
    return list(struct.unpack('<' + 'i' * (len(raw) // 8), bytes.fromhex(raw)))


def strings(block):
    section = block.split('    spriteNames:', 1)[-1]
    return re.findall(r'^    - ([^\n]+)$', section.split('    frameIds:', 1)[0].split('    references:', 1)[0], re.M)


def url(path):
    return quote('../../' + Path(path).relative_to(ROOT).as_posix(), safe='/.-_')


def catalog_pages():
    text = (RES / 'Darklight/TodayCharacterCatalog.asset').read_text(encoding='utf-8-sig')
    pages = []
    for block in re.split(r'^  - name: ', text, flags=re.M)[1:]:
        p = {key: scalar(block, key) for key in ['kind', 'resourcePath', 'paletteResourcePath', 'attackKey']}
        p.update(name=block.splitlines()[0], directions=packed(scalar(block, 'directions')), spriteNames=strings(block))
        p.update({k: int(scalar(block, k, '0')) for k in ['skin', 'weapon', 'age', 'phases', 'startupFrameEnd', 'activeFrameEnd', 'ownerMask']})
        pages.append(p)
    transformed = {}
    text = (RES / 'Darklight/TransformedCatalog.asset').read_text(encoding='utf-8-sig')
    for block in re.split(r'^  - baseName: ', text, flags=re.M)[1:]:
        p = {key: scalar(block, key) for key in ['resourcePath', 'palettePath']}
        p.update(spriteNames=strings(block), frameIds=packed(scalar(block, 'frameIds')), references=[])
        for ref in re.split(r'^    - resourcePath: ', block.split('    references:', 1)[-1], flags=re.M)[1:]:
            p['references'].append(dict(resourcePath=ref.splitlines()[0], palettePath=scalar(ref, 'palettePath'),
                                        spriteName=scalar(ref, 'spriteName'), ownerId=int(scalar(ref, 'ownerId', '0'))))
        transformed[block.splitlines()[0], int(scalar(block, 'form'))] = p
    return pages, transformed


def sheet(resource):
    if resource in SHEETS:
        return SHEETS[resource]
    path = RES / (resource + '.png')
    with path.open('rb') as stream:
        header = stream.read(24)
    width, height = struct.unpack('>II', header[16:24])
    meta = path.with_suffix('.png.meta').read_text(encoding='utf-8-sig')
    ppu = float(scalar(meta, 'spritePixelsToUnits', '100'))
    frames = {}
    for block in re.split(r'\n      name: ', meta)[1:]:
        name = block.splitlines()[0]
        area = block.split('      rect:', 1)[-1].split('      alignment:', 1)[0]
        x, y, w, h = [float(scalar(area, key, '0')) for key in ['x', 'y', 'width', 'height']]
        pivot = re.search(r'pivot: \{x: ([\d.eE+-]+), y: ([\d.eE+-]+)\}', block)
        if pivot:
            point=[float(pivot[1]),float(pivot[2])]
        else:
            point_text=block.split('      pivot:',1)[-1].split('      border:',1)[0]
            point=[float(scalar(point_text,'x','.5')),float(scalar(point_text,'y','.5'))]
        if w > 0 and h > 0:
            frames[name] = dict(rect=[int(x), height-int(y+h), int(w), int(h)], pivot=point)
    if not frames:
        pivot = re.search(r'spritePivot: \{x: ([\d.eE+-]+), y: ([\d.eE+-]+)\}', meta)
        frames[path.stem] = dict(rect=[0, 0, width, height], pivot=[float(pivot[1]), float(pivot[2])] if pivot else [.5, .5])
    result = dict(image=url(path), size=[width, height], ppu=ppu, frames=frames)
    SHEETS[resource] = result
    return result


def frame(resource, name, mask='', owner=0, form_filter=0, presentation='legacy-authored', image_override=None, mirror=False):
    identity = '|'.join(map(str, [resource, name, mask, owner, form_filter, presentation, image_override, mirror]))
    key = hashlib.sha256(identity.encode()).hexdigest()[:20]
    if key not in FRAMES:
        s = sheet(resource)
        if name not in s['frames']:
            raise ValueError('Missing actual imported sprite: ' + resource + '/' + name)
        if mask and not (RES / (mask + '.png')).is_file():
            raise ValueError('Missing owner mask: ' + mask)
        FRAMES[key] = dict(image=url(image_override) if image_override else s['image'], **s['frames'][name], ppu=s['ppu'],
                           mask=url(RES / (mask + '.png')) if mask else None, owner=owner, formFilter=form_filter,
                           presentation=presentation, sourceName=name, resourcePath=resource, mirror=mirror)
    return key


def select_page(skin, weapon, direction, kind, attack_key=''):
    return next((p for p in TODAY if p['skin']==skin and p['weapon']==weapon and p['age']==0 and
                 direction in p['directions'] and p['kind']==kind and (p['attackKey'] or '')==attack_key), None)


def base_frame(page, direction, phase, form):
    index = page['directions'].index(direction) * page['phases'] + phase
    normal_name = page['spriteNames'][index]
    tp = TRANSFORMED.get((page['name'], form)) if form else None
    if tp:
        if tp['references']:
            ref = tp['references'][index]
            selected = frame(ref['resourcePath'], ref['spriteName'], ref['palettePath'], ref['ownerId'])
        else:
            selected = frame(tp['resourcePath'], tp['spriteNames'][index], tp['palettePath'], tp['frameIds'][index])
    else:
        selected = frame(page['resourcePath'], normal_name, page['paletteResourcePath'], index+1 if page['ownerMask'] else 0,
                         form, 'legacy-filter' if form else 'legacy-normal')
    return selected, normal_name


def ruin_frame(skin, form, direction, bank, index):
    page = next(p for p in RUIN if (p['skin'],p['form'],p['direction'],p['bank'])==(skin,form,direction,bank))
    normal = next(p for p in RUIN if (p['skin'],p['form'],p['direction'],p['bank'])==(skin,0,direction,bank))
    prior = next(p for p in NATIVE_RUIN if (p['skin'],p['form'],p['direction'],p['bank'])==(skin,form,direction,bank))
    image_path = ROOT / 'outputs/punhos-ruina-final-20261021' / prior['image']
    # These are actual shader captures; no browser-wide red/violet tint is added.
    selected = frame(page['resourcePath'], page['spriteNames'][index], presentation='legacy-filter' if page['paletteVariant'] else 'legacy-authored',
                     image_override=image_path)
    return selected, normal['spriteNames'][index]


def redesigned(skin, weapon, form, direction, action, attack_key, source_name):
    eligible = [p for p in REDESIGN if (p['skin'],p['weapon'],p['form'],p['direction'],p['action']) == (skin,weapon,form,direction,action)]
    eligible.sort(key=lambda p: not bool(p.get('attackKey')))
    for page in eligible:
        if page.get('attackKey') and page['attackKey']!=attack_key:
            continue
        if source_name in page.get('sourceSpriteNames', []):
            index = page['sourceSpriteNames'].index(source_name)
            return frame(page['resourcePath'],page['spriteNames'][index],presentation='redesign')
    return None


def timed_redesign(skin, weapon, form, direction, action):
    page = next((p for p in REDESIGN if (p['skin'],p['weapon'],p['form'],p['direction'],p['action'])==
                 (skin,weapon,form,direction,action) and not p.get('sourceSpriteNames')), None)
    return [dict(frame=frame(page['resourcePath'], name, presentation='redesign'), durationMs=ms,phase=action,sourceName=None)
            for name, ms in zip(page['spriteNames'], page['durationMs'])] if page else None


def append_step(target, selected, ms, phase, source):
    if target and target[-1]['frame']==selected and target[-1]['phase']==phase and target[-1]['sourceName']==source:
        target[-1]['durationMs']+=ms
    else:
        target.append(dict(frame=selected,durationMs=ms,phase=phase,sourceName=source))


def attack_phase(page, attack, seconds):
    start, active, recovery = [attack[key] for key in ['startup','active','recovery']]
    if page['kind']=='Combat':
        return 1 if seconds<start else 2 if seconds<start+active else 3 if seconds<start+active+recovery*.7 else 0
    if page['phases']>4:
        lo,hi,t,dur = (0,page['startupFrameEnd'],seconds,start) if seconds<start else (page['startupFrameEnd'],page['activeFrameEnd'],seconds-start,active) if seconds<start+active else (page['activeFrameEnd'],page['phases'],seconds-start-active,recovery)
        return min(hi-1,lo+int(max(0,t)/max(.0001,dur)*(hi-lo)))
    return 0 if seconds<start else 1 if seconds<start+active else 2 if seconds<start+active+recovery*.5 else 3


def ruin_phase(attack, seconds):
    start, active, recovery = [attack[key] for key in ['startup','active','recovery']]
    if seconds>=start+active:
        return 10 if attack['visual']=='AerialSlam' and seconds<start+active+recovery*.55 else 11
    startup = seconds<start
    pairs={'Punch':(2,3),'SideKick':(4,5),'SpinKick':(4,6),'Uppercut':(7,8),'GroundSlam':(7,10),'AerialSlam':(8,9),'Cyclone':(4,6 if (seconds-start)/max(active,.001)<.7 else 10)}
    return pairs[attack['visual']][0 if startup else 1]


def make_attack(skin, weapon, form, direction, attack):
    current,legacy=[],[]
    page=None if weapon==2 else select_page(skin,weapon,direction,'Attack',attack['displayName']) or select_page(skin,weapon,direction,'Attack') or select_page(skin,weapon,direction,'Combat')
    duration=sum(attack[k] for k in ['startup','active','recovery'])
    # One-ms deterministic sampling groups adjacent equal phases afterwards.
    # It preserves authored hit windows while avoiding a second gameplay clock.
    for ms in range(round(duration*1000)):
        t=(ms+.5)/1000
        old,source=ruin_frame(skin,form,direction,'Combat',ruin_phase(attack,t)) if weapon==2 else base_frame(page,direction,attack_phase(page,attack,t),form)
        phase='Antecipação' if t<attack['startup'] else 'Contato' if t<attack['startup']+attack['active'] else 'Recuperação'
        new=redesigned(skin,weapon,form,direction,'Attack',attack['displayName'],source)
        append_step(legacy,old,1,phase,source)
        append_step(current,new or old,1,phase,source)
    return dict(current=current,legacy=legacy,attackKey=attack['displayName'],contacts=attack.get('hitPulses',1))


def add_clip(skin,weapon,form,direction,action,label,current,legacy,loop=True,**extra):
    if not current or not legacy:
        ISSUES.append('Missing clip '+str((skin,weapon,form,direction,action)))
        return
    CLIPS.append(dict(id=f'{skin}-{weapon}-{form}-{direction}-{action}',skin=skin,weapon=weapon,form=form,direction=direction,
                      action=action,label=label,current=current,legacy=legacy,loop=loop,**extra))


def simple_clips(skin,weapon,form,direction):
    labels={'Idle':'Repouso','Walk':'Caminhada','Dodge':'Esquiva','Parry':'Defesa / parry','Hurt':'Dano','Dead':'Morte','Charging':'Preparar carregado',
            'IdleAlert':'Repouso: alerta','IdleImpatient':'Repouso: impaciente','IdleInspect':'Repouso: observar arma','IdleFatigued':'Repouso: cansaço',
            'IdleWounded':'Repouso: ferido','StartMove':'Iniciar movimento','Brake':'Frear','Turn':'Mudar direção'}
    for action,label in labels.items():
        old=[]
        if weapon==2:
            bank='Combat'
            phases, durations = ([0,1],[600,600])
            if action=='Walk':bank='Motion';phases,durations=[0,1,2,3],[150]*4
            elif action=='Dodge':bank='Motion';phases,durations=[4,5,6],[45,95,90]
            elif action=='Parry':bank='Motion';phases,durations=[7,8,7],[50,160,50]
            elif action=='Hurt':bank='Motion';phases,durations=[9],[300]
            elif action=='Dead':bank='Motion';phases,durations=[10,11],[250,650]
            elif action=='Charging':phases,durations=[7],[650]
            for index,ms in zip(phases,durations):
                selected,source=ruin_frame(skin,form,direction,bank,index)
                old.append(dict(frame=selected,durationMs=ms,phase=label,sourceName=source))
        else:
            kind = 'GuardDodge' if action in ('Dodge','Parry') else 'Combat' if action in ('Hurt','Dead','Charging') else action
            page=select_page(skin,weapon,direction,kind)
            if page is None and action.startswith('Idle'):page=select_page(skin,weapon,direction,'Idle')
            if page is None and action in ('StartMove','Brake','Turn'):page=select_page(skin,weapon,direction,'Motion')
            if page is None and action=='Idle':page=select_page(skin,weapon,direction,'Combat')
            if page is None:continue
            n=page['phases']
            phases,durations=list(range(n)),[125]*n
            if action=='Idle':
                phases,durations=([0,1,2,3,4,5],[520]*5+[130]) if n==6 else ([0,1],[500,500]) if page['kind']=='Idle' else ([0,5],[500,500])
            elif action=='Walk':durations=[400/n]*n
            elif action=='Dodge':phases,durations=[4,5,6,7],[45,75,70,40]
            elif action=='Parry':phases,durations=[0,1,2,3],[40,100,70,50]
            elif action=='Hurt':phases,durations=[6],[300]
            elif action=='Dead':phases,durations=[7],[900]
            elif action=='Charging':phases,durations=[1],[450]
            elif action in ('StartMove','Brake','Turn') and page['kind']=='Motion':
                phases,durations=([0,1,2],[40]*3) if action=='StartMove' else ([2,3],[70]*2) if action=='Brake' else ([3,4],[70]*2)
            for index,ms in zip(phases,durations):
                selected,source=base_frame(page,direction,index,form)
                old.append(dict(frame=selected,durationMs=ms,phase=label,sourceName=source))
        add_clip(skin,weapon,form,direction,action,label,timed_redesign(skin,weapon,form,direction,action) or copy.deepcopy(old),old,action not in ('Dead','Hurt','StartMove','Brake','Turn'))


def attacks():
    def asset(path):
        text=path.read_text(encoding='utf-8-sig')
        result={key:float(scalar(text,key,'0')) for key in ['startup','active','recovery','hitPulses']}
        result['displayName']=scalar(text,'displayName');return result
    all_assets={}
    for path in (ROOT/'Assets/Darklight/Data').rglob('*.asset'):
        text=path.read_text(encoding='utf-8-sig')
        if 'Darklight.AttackDefinition' not in text:continue
        meta=path.with_suffix('.asset.meta').read_text(encoding='utf-8-sig')
        all_assets[scalar(meta,'guid')]=asset(path)
    sets=[]
    for weapon,name in enumerate(['Requiem','PresasDoVeu']):
        text=(ROOT/f'Assets/Darklight/Data/Weapons/{name}.asset').read_text(encoding='utf-8-sig')
        lights=re.findall(r'guid: ([0-9a-f]{32})',text.split('    lights:',1)[1].split('    heavy:',1)[0])
        mapped={key:all_assets.get(scalar(text,key).split('guid: ')[-1][:32]) for key in ['heavy','charged','counter','skill','aerial','weaponSpecial']}
        base_lights=[all_assets[x] for x in lights]
        def adjust(a):
            a=copy.deepcopy(a)
            if weapon==1:a['startup']*=.85;a['recovery']*=.8
            return a
        selected={k:adjust(v) for k,v in mapped.items() if v}
        selected['lights']=[adjust(a) for a in base_lights]
        selected['pauseBranch']=adjust(base_lights[-1]);selected['pauseBranch']['displayName']='Corte do Silencio' if weapon==0 else 'Passo das Presas'
        selected['heavyBranch']=adjust(mapped['heavy']);selected['heavyBranch']['displayName']='Ruptura Precisa' if weapon==0 else 'Dilaceracao'
        selected['pauseBranch']['hitPulses']=selected['heavyBranch']['hitPulses']=3 if weapon==1 else 1
        finisher=adjust(base_lights[-1]);finisher.update(displayName='Corte do Crepúsculo' if weapon==0 else 'Execucao Dupla',startup=.12 if weapon==0 else .05,active=.16 if weapon==0 else .08,recovery=.25 if weapon==0 else .12,hitPulses=2)
        selected['lights'].append(finisher)
        special=adjust(mapped['heavy'] if weapon==0 else mapped['weaponSpecial'])
        special.update(displayName='Eclipse Partido' if weapon==0 else 'Dança das Presas',startup=.16 if weapon==0 else .055,active=.22 if weapon==0 else .32,recovery=.30 if weapon==0 else .18,hitPulses=2 if weapon==0 else 5)
        selected['weaponSpecial']=special;sets.append(selected)
    text=(ROOT/'Assets/Darklight/Runtime/Combat/DarklightRuinFistsProfile.cs').read_text(encoding='utf-8-sig')
    pattern=r'Make\("([^"]+)",StrikeKind\.\w+,DarklightRuinStrike\.(\w+),[\d.]+,([\d.]+)f?,([\d.]+)f?,([\d.]+)f?,'
    fists=[dict(displayName=n,visual=v,startup=float(s),active=float(a),recovery=float(r),hitPulses=3 if v=='Cyclone' else 1) for n,v,s,a,r in re.findall(pattern,text)]
    if len(fists)!=12:raise ValueError('RuinFists profile changed; inspect actual attack definitions')
    selected=dict(zip(['heavy','charged','counter','skill','aerial','weaponSpecial','pauseBranch','heavyBranch'],fists[4:]))
    # airFinisher aliases aerial; there are twelve distinct definitions.
    selected['lights']=fists[:4];sets.append(selected)
    return sets


def motorcycle():
    owners={p['name']:p['spriteNames'] for p in read(RES/'Darklight/AbyssPrototype/WholeOwnership.json')['pages']}
    for form in range(4):
        for direction in range(8):
            view=8-direction if direction>4 else direction
            mirror=direction>4
            def moto_frame(resource,index):
                resource='Darklight/AbyssPrototype/'+resource
                sprites=sheet(resource)['frames']
                names=sorted(sprites,key=lambda n:tuple(int(x) for x in n.split('_')[-2:]) if n.split('_')[-1].isdigit() else (0,0))
                if index>=len(names):raise ValueError(f'Moto index {resource} {index}/{len(names)}')
                name=names[index]
                mask=resource+'_Owner' if resource.split('/')[-1] in owners else ''
                owner=owners[resource.split('/')[-1]].index(name)+1 if mask else 0
                return frame(resource,name,mask,owner,presentation='motorcycle',mirror=mirror)
            prefix='' if form==0 else 'Form'+str(form)
            if view>=2:
                resource='ActionF'+str(form)+['W','NW','N'][view-2]
                groups={'Idle':[0],'Attack':[0,1,2,3,4,0],'Dodge':[8],'Parry':[11],'Slam':[0,5,9,10,4,0],'Mount':[0,5,6,7,8,4,0]}
                for action,indices in groups.items():
                    track=[dict(frame=moto_frame(resource,i),durationMs=140,phase=action,sourceName=None) for i in indices]
                    add_clip(1,3,form,direction,action,{'Idle':'Repouso','Attack':'Cortes da motosserra','Dodge':'Esquiva','Parry':'Defesa','Slam':'Martelo da moto','Mount':'Montar / avanço / desmontar'}[action],track,copy.deepcopy(track),action=='Idle')
            else:
                row=1 if direction==1 else 0
                for action,page,indices in [('Idle',prefix+'Join',[row*4]),('Mount',prefix+'Join',[row*4+i for i in range(4)]),('Slam',prefix+'Slam',[row*2,row*2+1]),('Finish',prefix+'Finisher',[row*2,row*2+1])]:
                    if form==0 and page=='Slam':page='Form0Slam'
                    if form==0 and page=='Finisher':page='Form0Finisher'
                    track=[dict(frame=moto_frame(page,i),durationMs=150,phase=action,sourceName=None) for i in indices]
                    add_clip(1,3,form,direction,action,{'Idle':'Repouso','Mount':'Montar / avanço','Slam':'Martelo da moto','Finish':'Finalização'}[action],track,copy.deepcopy(track),action=='Idle')
                page=prefix+'AttackSix'+str(row)
                track=[dict(frame=moto_frame(page,i),durationMs=120,phase='Cortes',sourceName=None) for i in range(6)]
                add_clip(1,3,form,direction,'Attack','Sequência de seis cortes',track,copy.deepcopy(track),False)
            resource='CalmWalkF'+str(form)+'G'+str(view//2)
            track=[dict(frame=moto_frame(resource,(view%2)*4+i),durationMs=100,phase='Caminhada',sourceName=None) for i in range(4)]
            add_clip(1,3,form,direction,'Walk','Caminhada com equipamento',track,copy.deepcopy(track))


def main():
    global TODAY,TRANSFORMED,RUIN,NATIVE_RUIN,REDESIGN
    TODAY,TRANSFORMED=catalog_pages()
    RUIN=read(RES/'Darklight/RuinFists/catalog.json')['pages']
    NATIVE_RUIN=read(ROOT/'outputs/punhos-ruina-final-20261021/review-index.json')
    REDESIGN=read(RES/'Darklight/WeaponRedesign20261023/catalog.json')['pages']
    profiles=attacks()
    for skin in range(2):
        for weapon in range(3):
            for form in range(4):
                for direction in range(8):
                    simple_clips(skin,weapon,form,direction)
                    profile=profiles[weapon];attack_tracks={}
                    for key,attack in [(f'light{i+1}',a) for i,a in enumerate(profile['lights'])]+[(k,a) for k,a in profile.items() if k!='lights']:
                        tracks=make_attack(skin,weapon,form,direction,attack);attack_tracks[key]=tracks
                        add_clip(skin,weapon,form,direction,'Attack:'+key,'Golpe: '+attack['displayName'],loop=False,**tracks)
                    for key,label,sequence in [('combo','Combo principal ×4',['light1','light2','light3','light4']),('pause','Combo com pausa',['light1','pauseBranch']),('heavy','Combo com final pesado',['light1','light2','heavyBranch'])]:
                        current=[];legacy=[]
                        for move in sequence:
                            current.extend(copy.deepcopy(attack_tracks[move]['current']));legacy.extend(copy.deepcopy(attack_tracks[move]['legacy']))
                        add_clip(skin,weapon,form,direction,'Combo:'+key,label,current,legacy,False,sequence=[attack_tracks[m]['attackKey'] for m in sequence])
    motorcycle()
    OUT.mkdir(parents=True,exist_ok=True)
    summary=dict(clips=len(CLIPS),frames=len(FRAMES),sourceSheets=len(SHEETS),redesignContextPages=len(REDESIGN),
                 redesignFiguresReferenced=len({s['frame'] for c in CLIPS for s in c['current'] if FRAMES[s['frame']]['presentation']=='redesign'}),
                 clipsByWeapon=dict(Counter(str(c['weapon']) for c in CLIPS)),issues=ISSUES,
                 nativeRuinCaptureDate='2026-10-21',audio=False,unityExecution=False)
    data=dict(schemaVersion=1,date='2026-10-24',directions=DIRS,forms=FORMS,frames=FRAMES,clips=CLIPS,summary=summary,
              limits=['Viewer de figuras completas e janelas de animação. Não simula colisão, dano, movimento no mapa, efeitos de contato ou regras de cancelamento do combo.',
                      'Resultado atual aplica o catálogo novo somente ao contexto e nome de fase correspondentes. Outras técnicas preservam arte anterior.',
                      'Comparação anterior dos punhos usa capturas reais do shader de 21/10/2026. O redesign atual usa os atlas preparados/importados em 24/10.',
                      'Filtros de fallback usam os seletores de materiais da máscara real. Não há filtro global sobre pele e olhos.',
                      'Moto disponível somente para Rock; espelhamento explícito segue o consumidor existente, não cria direção nova de redesign.',
                      'Durações exibem as janelas base dos ataques. Modificadores de velocidade de gameplay não são simulados; o controle Velocidade é de inspeção.',
                      'Variedades de repouso dos punhos seguem as poses existentes de idle; não representam desenhos inéditos.',
                      'Morte mostra a pose da forma selecionada; a reversão/cutscene de morte exige a execução do jogo.'])
    (OUT/'manifest.json').write_text(json.dumps(data,ensure_ascii=False,separators=(',',':'))+'\n',encoding='utf-8')
    (OUT/'index.html').write_text(HTML,encoding='utf-8')
    (OUT/'viewer.js').write_text(JS,encoding='utf-8')
    (OUT/'abrir-previa.ps1').write_text(LAUNCHER,encoding='utf-8-sig')
    command='; '.join(line.strip() for line in LAUNCHER.splitlines() if line.strip())
    cmd='@echo off\r\nsetlocal\r\nset "DARKLIGHT_PREVIEW_FOLDER=%~dp0"\r\npowershell.exe -NoLogo -NoProfile -Command "'+command+'"\r\nif errorlevel 1 (\r\n  echo Nao foi possivel abrir a previa. Confira se a pasta do projeto e o Python do Codex continuam neste computador.\r\n  pause\r\n)\r\nendlocal\r\n'
    (OUT/'ABRIR-PREVIA.cmd').write_bytes(cmd.encode('ascii'))
    (OUT/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(summary,ensure_ascii=False))


HTML='''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>DARKLIGHT · Armas e animações</title>
<style>*{box-sizing:border-box}body{margin:0;background:#11131b;color:#f2eef3;font:15px system-ui}main{max-width:1380px;margin:auto;padding:26px}h1{font-size:30px;margin:8px 0}p{color:#b5bac8;line-height:1.5}.eyebrow{font-size:12px;color:#e66887;letter-spacing:.15em}.weapons{display:flex;gap:9px;flex-wrap:wrap;margin:24px 0 16px}button,select,input{font:inherit;color:inherit;background:#242938;border:1px solid #48516a;border-radius:7px;padding:9px 12px}button{cursor:pointer}button:hover,button.active{background:#4c2031;border-color:#db6789}button:disabled{opacity:.4;cursor:default}.filters,.playback{display:flex;gap:12px;align-items:flex-end;flex-wrap:wrap;margin:14px 0}label{font-size:12px;display:grid;gap:5px;color:#b7bfce}select{min-width:130px;font-size:14px}#action{min-width:280px;max-width:460px}#stage{overflow:auto;display:grid;place-items:center;background:repeating-conic-gradient(#202631 0% 25%,#282f3d 0% 50%) 50%/28px 28px;border:1px solid #39435b;border-radius:12px;min-height:460px}canvas{width:100%;max-width:1050px;height:auto;image-rendering:pixelated}.status{display:flex;gap:12px;flex-wrap:wrap;align-items:center;margin:13px 0}.badge{font-size:12px;border-radius:20px;padding:6px 11px;border:1px solid #706077;color:#e5b4dc}.badge.new{border-color:#459c83;color:#a4e7cf}.timeline{display:flex;gap:4px;overflow-x:auto;margin:12px 0}.timeline button{min-width:44px;font-size:12px;padding:7px}.timeline .contact{border-bottom:3px solid #e76b85}.timeline .active{outline:2px solid #ddd}#scrub{width:100%;padding:0;accent-color:#e76b85}#counter{font-variant-numeric:tabular-nums}details{margin-top:20px;background:#1b202c;border:1px solid #333d54;padding:15px;border-radius:8px}summary{cursor:pointer;color:#d4bfdc}#sources{font-size:12px;word-break:break-word;color:#aab2c4}#limits{font-size:13px}#sequence{color:#d4bced}a{color:#d2a3ef}.foot{font-size:12px;color:#8c97aa}#loading{color:#eaa370}.mode-note{font-size:12px;margin:3px 0}.row{display:flex;justify-content:space-between;gap:20px;flex-wrap:wrap}@media(max-width:650px){main{padding:15px}h1{font-size:24px}#action{min-width:230px;max-width:100%}#stage{min-height:300px}}</style>
<main><div class="eyebrow">DARKLIGHT: REQUIEM · REVISÃO DE 24/10/2026</div><h1>Armas, formas e combos</h1><p>Veja uma arma por vez, com cada animação separada. Compare as figuras novas com a apresentação anterior e avance pelos quadros de cada golpe.</p>
<nav class="weapons" aria-label="Arma"><button data-weapon="0" class="active">Katana Réquiem</button><button data-weapon="1">Presas do Caos</button><button data-weapon="2">Punhos da Ruína</button><button data-weapon="3">Moto · Cavaleiro do Abismo</button></nav>
<div class="filters"><label>Skin<select id="skin"><option value="0">Réquiem</option><option value="1">Rock</option></select></label><label>Forma<select id="form"><option value="0">Normal</option><option value="1">Ruptura</option><option value="2">Eclipse</option><option value="3">Absoluto</option></select></label><label>Direção<select id="direction"></select></label><label>Animação ou combo<select id="action"></select></label><label>Apresentação<select id="mode"><option value="current">Resultado atual</option><option value="legacy">Comparar arte anterior</option></select></label></div>
<details><summary>Como usar esta prévia</summary><p>Escolha uma arma, a skin e a forma. Em “Animação ou combo”, selecione um golpe para vê-lo sozinho ou uma sequência de golpes. Use “Resultado atual” para ver as artes implementadas; “Comparar arte anterior” mostra a apresentação anterior, incluindo os filtros existentes.</p><p>Pause para avançar um quadro por vez. Em 1× a figura inteira fica visível. Ao ampliar, use a barra de rolagem para explorar a imagem. Tudo funciona sem som.</p></details>
<p id="sequence"></p><div class="row"><div class="status"><span id="badge" class="badge"></span><span id="counter"></span></div><p id="loading">Carregando catálogos…</p></div><div id="stage"><canvas id="canvas" width="1000" height="570" aria-label="Personagem completo da arma e forma selecionadas"></canvas></div>
<div class="playback"><button id="play">Pausar</button><button id="previous">← Quadro</button><button id="next">Quadro →</button><button id="restart">Reiniciar</button><label>Velocidade<select id="speed"><option value="0.25">0,25×</option><option value="0.5">0,5×</option><option value="1" selected>1×</option><option value="2">2×</option></select></label><label>Ampliação<select id="zoom"><option value="0.7">0,7×</option><option value="1" selected>1×</option><option value="1.5">1,5×</option><option value="2">2×</option></select></label><label>Fundo<select id="background"><option value="checker">Quadriculado</option><option value="#10131b">Escuro</option><option value="#eeeae4">Claro</option><option value="#376353">Verde</option></select></label><label><span>Repetir</span><input type="checkbox" id="loop" checked></label></div>
<input id="scrub" type="range" min="0" max="1" value="0" step="1" aria-label="Posição na animação"><div id="timeline" class="timeline"></div><p id="description" class="mode-note"></p>
<details><summary>Cobertura e origem da apresentação</summary><p id="coverage"></p><p id="sources"></p><ul id="limits"></ul><a href="manifest.json">Dados completos da prévia</a></details><p class="foot">Prévia silenciosa. Sem música, reprodução de áudio ou alterações nos sprites, saves e preferências do jogo. Espaço: reproduzir/pausar · ← e →: quadro anterior/seguinte.</p></main><script src="viewer.js"></script></html>'''

HTML=HTML.replace('overflow:auto;display:grid;place-items:center;background:','overflow:auto;display:block;background:').replace('canvas{width:100%;','canvas{display:block;margin:auto;width:100%;')
HTML=HTML.replace('<h1>Armas, formas e combos</h1>','<h1>Prévia parcial · armas, formas e combos</h1><p style="color:#efb273">Pendência urgente: os punhos novos cobrem somente a frente. A aparência Normal ainda precisa ser diferenciada da Ruptura; as demais direções, animações e combos mantêm arte anterior, incluindo filtros. Essa entrega visual ainda não foi aprovada pelo usuário.</p>')

JS=r'''"use strict";
const $=id=>document.getElementById(id),canvas=$('canvas'),ctx=canvas.getContext('2d');
let data,weapon=0,clip,track=[],step=0,clock=0,playing=true,last=0,generation=0,paintSerial=0;
const images=new Map(),processed=new Map();
const labels={redesign:'Arte própria · redesign 24/10','legacy-authored':'Arte própria anterior','legacy-normal':'Arte anterior · Normal','legacy-filter':'Arte anterior com filtro','motorcycle':'Arte própria · banco da moto'};
function selection(){return data.clips.filter(c=>c.weapon===weapon&&c.skin===Number($('skin').value)&&c.form===Number($('form').value)&&c.direction===Number($('direction').value));}
function loadImage(src){if(!images.has(src)){const img=new Image();img.src=src;images.set(src,img.decode().then(()=>img));while(images.size>12)images.delete(images.keys().next().value);}return images.get(src);}
async function pixels(id){
 if(processed.has(id))return processed.get(id);
 const f=data.frames[id],img=await loadImage(f.image),[x,y,w,h]=f.rect,c=document.createElement('canvas');c.width=w;c.height=h;const c2=c.getContext('2d',{willReadFrequently:true});c2.imageSmoothingEnabled=false;c2.drawImage(img,x,y,w,h,0,0,w,h);
 if(f.mask){const m=await loadImage(f.mask),mc=document.createElement('canvas');mc.width=w;mc.height=h;const m2=mc.getContext('2d',{willReadFrequently:true});m2.drawImage(m,x,y,w,h,0,0,w,h);const a=c2.getImageData(0,0,w,h),b=m2.getImageData(0,0,w,h).data;
  const pal=f.formFilter===3?[.95,.96,1]:f.formFilter===2?[.62,.16,.96]:[.86,.035,.08];
  for(let i=0;i<a.data.length;i+=4){if(f.owner&&b[i+3]!==f.owner){a.data[i+3]=0;continue;}if(f.formFilter&&b[i]<128&&b[i+1]>25){const strength=b[i+1]/255*.85,bright=Math.max(.3,Math.max(a.data[i],a.data[i+1],a.data[i+2])/255*1.3);for(let k=0;k<3;k++)a.data[i+k]=a.data[i+k]*(1-strength)+255*pal[k]*bright*strength;}}
  c2.putImageData(a,0,0);
 }
 processed.set(id,c);while(processed.size>96)processed.delete(processed.keys().next().value);return c;
}
function duration(){return track.reduce((n,s)=>n+s.durationMs,0);}
function setPlaying(value){playing=value;$('play').textContent=playing?'Pausar':'Reproduzir';last=0;}
function atTime(ms){let t=0;for(let i=0;i<track.length;i++){t+=track[i].durationMs;if(ms<t)return i;}return track.length-1;}
function updateStatus(){if(!track.length)return;const s=track[step],f=data.frames[s.frame];$('badge').textContent=labels[f.presentation];$('badge').classList.toggle('new',f.presentation==='redesign');$('counter').textContent=`Quadro ${step+1}/${track.length} · ${s.phase} · ${(clock/1000).toFixed(2)} / ${(duration()/1000).toFixed(2)} s`;$('scrub').value=Math.round(clock);$('timeline').querySelectorAll('button').forEach((b,i)=>b.classList.toggle('active',i===step));$('sources').textContent=`Figura: ${f.sourceName} · ${f.resourcePath}${f.mirror?' · Espelhamento do banco existente':''}${s.sourceName?' · Fase fonte: '+s.sourceName:''}`;
 $('description').textContent=f.presentation==='legacy-filter'&&f.image.includes('punhos-ruina-final')?'Filtro real do shader: captura do banco anterior em 21/10/2026. Olhos e pele seguem os seletores existentes.':f.presentation==='redesign'?'Figura completa desenhada para esta arma, forma, direção e fase. A forma final já está na arte.':f.formFilter?'Filtro limitado ao canal de materiais da máscara do runtime; sem tingimento global da imagem.':'Figura completa existente para esta seleção. A cobertura nova permanece restrita às páginas declaradas.';
}
async function draw(){if(!track.length)return;updateStatus();const serial=++paintSerial,frameId=track[step].frame,f=data.frames[frameId];$('loading').textContent='Carregando figura…';try{const img=await pixels(frameId);if(serial!==paintSerial)return;ctx.clearRect(0,0,canvas.width,canvas.height);ctx.imageSmoothingEnabled=false;
 let left=0,right=0,top=0,bottom=0;for(const s of track){const a=data.frames[s.frame],[,,w,h]=a.rect,p=a.pivot;left=Math.max(left,w*p[0]/a.ppu);right=Math.max(right,w*(1-p[0])/a.ppu);top=Math.max(top,h*(1-p[1])/a.ppu);bottom=Math.max(bottom,h*p[1]/a.ppu);}const scale=Math.min(145,860/(left+right),490/(top+bottom));const ox=500+(left-right)*scale*.5,oy=45+top*scale,[,,w,h]=f.rect;ctx.strokeStyle='rgba(166,182,205,.20)';ctx.beginPath();ctx.moveTo(50,oy);ctx.lineTo(950,oy);ctx.stroke();ctx.save();ctx.translate(ox,oy);if(f.mirror)ctx.scale(-1,1);ctx.drawImage(img,-w*f.pivot[0]/f.ppu*scale,-h*(1-f.pivot[1])/f.ppu*scale,w/f.ppu*scale,h/f.ppu*scale);ctx.restore();canvas.style.width=Math.round(Math.min($('stage').clientWidth-2,1050)*Number($('zoom').value))+'px';canvas.style.maxWidth='none';canvas.style.height='auto';canvas.dataset.frame=frameId;$('loading').textContent='';}catch(e){$('loading').textContent='Falha ao carregar figura: '+e.message;console.error(e);}}
function startTrack(){track=clip[$('mode').value];step=0;clock=0;generation++;$('scrub').max=Math.ceil(duration()-1);$('timeline').replaceChildren(...track.map((s,i)=>{const b=document.createElement('button');b.textContent=String(i+1);b.title=s.phase+' · '+s.durationMs+' ms';if(s.phase==='Contato')b.classList.add('contact');b.onclick=()=>jump(i);return b;}));$('sequence').textContent=clip.sequence?'Sequência: '+clip.sequence.join(' → '):clip.contacts?`${clip.label} · ${clip.contacts} contato(s) conforme a definição do golpe`:'';const redesigned=new Set(track.filter(s=>data.frames[s.frame].presentation==='redesign').map(s=>s.frame));$('coverage').textContent=`Seleção atual: ${redesigned.size} figura(s) do redesign em ${new Set(track.map(s=>s.frame)).size} figura(s) distintas. Banco geral: ${data.summary.redesignContextPages} páginas exatas; as demais ações mantêm arte anterior.`;draw();}
function refresh(){const old=$('action').value,available=selection();$('action').replaceChildren(...available.map(c=>new Option(c.label,c.action)));if(available.some(c=>c.action===old))$('action').value=old;clip=available.find(c=>c.action===$('action').value);if(!clip){$('loading').textContent='Sem animação neste contexto';track=[];ctx.clearRect(0,0,1000,570);return;}startTrack();}
function jump(index){setPlaying(false);step=(index+track.length)%track.length;clock=track.slice(0,step).reduce((n,s)=>n+s.durationMs,0);draw();}
function tick(t){const dt=last?Math.min(100,t-last):0;last=t;if(playing&&track.length){clock+=dt*Number($('speed').value);const total=duration();if(clock>=total){if($('loop').checked)clock%=total;else{clock=total-1;setPlaying(false);}}const next=atTime(clock);if(next!==step){step=next;draw();}else updateStatus();}requestAnimationFrame(tick);}
for(const b of document.querySelectorAll('[data-weapon]'))b.onclick=()=>{weapon=Number(b.dataset.weapon);document.querySelectorAll('[data-weapon]').forEach(x=>x.classList.toggle('active',x===b));if(weapon===3)$('skin').value='1';$('skin').disabled=weapon===3;refresh();};
for(const id of ['skin','form','direction'])$(id).onchange=refresh;$('action').onchange=()=>{clip=selection().find(c=>c.action===$('action').value);startTrack();};$('mode').onchange=startTrack;$('zoom').onchange=draw;$('play').onclick=()=>setPlaying(!playing);$('previous').onclick=()=>jump(step-1);$('next').onclick=()=>jump(step+1);$('restart').onclick=()=>{clock=0;step=0;draw();};$('scrub').oninput=()=>{setPlaying(false);clock=Number($('scrub').value);step=atTime(clock);draw();};$('background').onchange=()=>{const v=$('background').value;$('stage').style.backgroundImage=v==='checker'?'':'none';$('stage').style.backgroundColor=v==='checker'?'#202631':v;};document.addEventListener('keydown',e=>{if(e.target.matches('select,input'))return;if(e.code==='Space'){e.preventDefault();setPlaying(!playing);}if(e.code==='ArrowLeft'){e.preventDefault();jump(step-1);}if(e.code==='ArrowRight'){e.preventDefault();jump(step+1);}});
fetch('manifest.json').then(r=>{if(!r.ok)throw new Error('Manifesto indisponível');return r.json();}).then(d=>{data=d;$('direction').replaceChildren(...d.directions.map((n,i)=>new Option(n,String(i))));$('limits').replaceChildren(...d.limits.map(t=>{const li=document.createElement('li');li.textContent=t;return li;}));refresh();requestAnimationFrame(tick);window.weaponReview={data,select:refresh,state:()=>({weapon,clip:clip.id,step,playing,clock,trackLength:track.length,frame:data.frames[track[step].frame]})};}).catch(e=>{$('loading').textContent=e.message;console.error(e);});
'''


LAUNCHER=r'''$ErrorActionPreference='Stop'
$previewFolder=if($env:DARKLIGHT_PREVIEW_FOLDER){$env:DARKLIGHT_PREVIEW_FOLDER}else{$PSScriptRoot}
$previewRoot=(Resolve-Path -LiteralPath (Join-Path $previewFolder '../..')).Path
$previewPython=Join-Path $env:USERPROFILE '.cache/codex-runtimes/codex-primary-runtime/dependencies/python/python.exe'
if(!(Test-Path -LiteralPath $previewPython)){throw 'Python do Codex nao encontrado neste computador.'}
$previewPath='/outputs/previa-armas-20261024/index.html'
$previewUrl=$null
foreach($previewPort in @(8766,8774,8775,8776,8777,8778)){try{$previewCandidate='http://127.0.0.1:'+$previewPort+$previewPath;$previewResponse=Invoke-WebRequest -UseBasicParsing -Uri $previewCandidate -TimeoutSec 1;if($previewResponse.StatusCode -eq 200 -and $previewResponse.Content.Contains('data-weapon')){$previewUrl=$previewCandidate;break}}catch{}}
if(!$previewUrl){$previewPort=$null;foreach($candidatePort in 8774..8783){try{$probe=[System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback,$candidatePort);$probe.Start();$probe.Stop();$previewPort=$candidatePort;break}catch{}};if(!$previewPort){throw 'Nao foi encontrada uma porta local disponivel.'};$previewLogs=Join-Path $previewRoot 'work/session-20261024';New-Item -ItemType Directory -Force -Path $previewLogs|Out-Null;$previewArguments=@('-m','http.server',[string]$previewPort,'--bind','127.0.0.1','--directory',([char]34+$previewRoot+[char]34));Start-Process -FilePath $previewPython -ArgumentList $previewArguments -WindowStyle Hidden -RedirectStandardOutput (Join-Path $previewLogs 'preview-launcher-server.log') -RedirectStandardError (Join-Path $previewLogs 'preview-launcher-errors.log')|Out-Null;$previewUrl='http://127.0.0.1:'+$previewPort+$previewPath;for($attempt=0;$attempt -lt 15;$attempt++){try{$null=Invoke-WebRequest -UseBasicParsing -Uri $previewUrl -TimeoutSec 1;break}catch{Start-Sleep -Milliseconds 200}}}
if($env:DARKLIGHT_PREVIEW_NO_OPEN -ne '1'){Start-Process -FilePath $previewUrl}
Write-Output $previewUrl
'''


if __name__=='__main__':
    main()
