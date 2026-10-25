"""Refresh the silent viewer from imported pages, including authored motorcycle figures."""
import copy
import json
from pathlib import Path
import prepare_weapon_review_20261024 as viewer


def add_current_motorcycle_figures():
    legacy_motorcycle()
    for form in range(4):
        resource = f'Darklight/WeaponRedesign20261023/RockMotoForm{form}Ride8SlamS20261025V2'
        descriptor = viewer.read(viewer.RES / (resource + '.json'))
        entry = descriptor['motorcycle']
        for direction, name in enumerate(entry['ride']):
            mount = next(c for c in viewer.CLIPS if (c['weapon'], c['form'], c['direction'], c['action']) == (3, form, direction, 'Mount'))
            previous = copy.deepcopy(mount['legacy'][3])
            previous.update(durationMs=560, phase='Avanço ativo · pose estática')
            current = dict(frame=viewer.frame(resource, name, presentation='redesign'),
                durationMs=560, phase='Avanço ativo · pose estática', sourceName=None)
            viewer.add_clip(1, 3, form, direction, 'RideActive', 'Avanço montado · pose estática',
                [current], [previous], False, staticPose=True, idleCycle=False)
        slam = next(c for c in viewer.CLIPS if (c['weapon'], c['form'], c['direction'], c['action']) == (3, form, 0, 'Slam'))
        slam['current'] = [dict(frame=viewer.frame(resource, name, presentation='redesign'),
            durationMs=duration, phase=phase, sourceName=None)
            for name, duration, phase in zip(entry['slam'], [232, 348, 160, 580],
                ['Preparação', 'Elevação', 'Contato', 'Recuperação'])]


def add_standing_pose_sources():
    for clip in list(viewer.CLIPS):
        if clip['weapon'] == 3 or clip['action'] != 'Idle':
            continue
        if viewer.FRAMES[clip['current'][0]['frame']]['presentation'] != 'redesign':
            continue
        current, previous = copy.deepcopy(clip['current'][:1]), copy.deepcopy(clip['legacy'][:1])
        for step in current + previous:
            step.update(durationMs=1000, phase='Pose neutra · parada')
        viewer.add_clip(clip['skin'], clip['weapon'], clip['form'], clip['direction'],
            'StandingPose', 'Parada · pose neutra do idle', current, previous, False,
            staticPose=True, reusesIdleFrame=True, runtimeActionAdded=False)


legacy_motorcycle = viewer.motorcycle


def main():
    viewer.OUT = viewer.ROOT / 'outputs/previa-armas-20261025'
    viewer.motorcycle = lambda: (add_current_motorcycle_figures(), add_standing_pose_sources())
    viewer.HTML = viewer.HTML.replace('24/10/2026', '25/10/2026')
    viewer.JS = viewer.JS.replace('redesign 24/10', 'redesign 25/10')
    before = 'const scale=Math.min(145,860/(left+right),490/(top+bottom));const ox=500+(left-right)*scale*.5'
    after = ('const scale=145*1.325;canvas.width=Math.max(1000,Math.ceil((left+right)*scale+100));'
        'canvas.height=Math.max(570,Math.ceil((top+bottom)*scale+90));ctx.imageSmoothingEnabled=false;'
        'const ox=canvas.width/2+(left-right)*scale*.5')
    if before not in viewer.JS:
        raise ValueError('Viewer framing changed; review fixed world-scale replacement')
    viewer.JS = viewer.JS.replace(before, after).replace('ctx.lineTo(950,oy)', 'ctx.lineTo(canvas.width-50,oy)')
    viewer.JS = viewer.JS.replace("Math.min($('stage').clientWidth-2,1050)*Number($('zoom').value)",
        "canvas.width*Number($('zoom').value)")
    viewer.LAUNCHER = viewer.LAUNCHER.replace('previa-armas-20261024', 'previa-armas-20261025').replace('session-20261024', 'session-20261025')
    viewer.main()
    path = viewer.OUT / 'manifest.json'
    data = json.loads(path.read_text(encoding='utf-8'))
    data['date'] = '2026-10-25'
    data['summary'].update(motorcycleFormSheets=4, motorcycleAuthoredFigures=48,
        fixedWorldScale=True, worldVisualScale=1.325, unityExecution=False, audio=False)
    data['limits'] = [line for line in data['limits'] if '24/10' not in line and 'espelhamento explícito' not in line]
    data['limits'] += [
        'Prévia atualizada pelo catálogo importado de25/10. Não executa jogo, áudio, colisões ou controles.',
        'Escala fixa do personagem entre seleções, usando os PPUs importados e o fator visual1,325 do jogo. Armas maiores ampliam a área de rolagem sem encolher o corpo.',
        'Parada reutiliza a pose neutra do idle e não acrescenta desenho nem ação de runtime.',
        'Moto nova: oito vistas montadas estáticas durante avanço e quatro fases de Esmagamento Vertical frontal por forma. Não cobre parada/idle montado ou outras técnicas.',
        'As quatro formas novas da moto usam suas próprias oito vistas, sem espelhamento. O banco anterior ainda pode espelhar contextos não substituídos.',
        'Ruptura/Eclipse da moto preservam fontesV1 de cabelo curto. Cor mais forte sem crescimento ainda é pendência artística.'
    ]
    path.write_text(json.dumps(data, ensure_ascii=False, separators=(',', ':')) + '\n', encoding='utf-8')
    print(json.dumps(dict(manifest=path.relative_to(viewer.ROOT).as_posix(),
        authoredMotorcycleFigures=48, fixedWorldScale=True, clips=len(data['clips']))))


if __name__ == '__main__':
    main()
