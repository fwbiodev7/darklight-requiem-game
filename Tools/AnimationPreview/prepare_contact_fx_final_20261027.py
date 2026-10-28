"""Package whole contact effects byte-for-byte; never clean generated raster pixels."""
import json
import shutil

import numpy as np
from PIL import Image
import prepare_weapon_redesign as pipeline

ROOT = pipeline.ROOT
OUT = ROOT / 'outputs/retomada-20261027/contact-fx/final'
ART = ROOT / 'ArtSources/CombatContacts20261027'
RES = ROOT / 'Assets/Darklight/Resources/Darklight/CombatContacts20261027'
TIMINGS = [25, 40, 45, 55]
ANCHORS = [[174, 218], [532, 218], [920, 218], [1275, 218],
           [174, 551], [535, 551], [924, 551], [1275, 551],
           [177, 875], [535, 875], [926, 875], [1275, 875]]


def main():
    source = ART / 'selected/ContactBursts-v3-source.png'
    pipeline.preserved_copy(ART / 'revisions/ContactBursts-v3-source.png', source)
    rgba = np.array(Image.open(source).convert('RGBA'))
    height, width = rgba.shape[:2]
    RES.mkdir(parents=True, exist_ok=True)
    target = RES / 'ContactBursts.png'
    shutil.copy2(source, target)
    assert pipeline.sha(source) == pipeline.sha(target)
    data = json.loads((ROOT / 'outputs/retomada-20261027/contact-fx/staged/ContactBursts.json').read_text())
    frames, metrics = [], []
    for row in range(3):
        for col in range(4):
            index = row * 4 + col
            x0, x1 = round(col * width / 4), round((col + 1) * width / 4)
            # Actual row gaps are not equal thirds: keep the complete top rock of Heavy peak.
            y0, y1 = [0, 384, 696][row], [384, 696, height][row]
            frame = dict(data['frames'][index])
            anchor = ANCHORS[index]
            frame.update(rect=[x0, y0, x1-x0, y1-y0], sourceRect=[x0, y0, x1-x0, y1-y0],
                         sourceAnchor=anchor, pivot=[(anchor[0]-x0)/(x1-x0), 1-(anchor[1]-y0)/(y1-y0)])
            frames.append(frame)
            part = rgba[y0:y1, x0:x1]
            alpha = part[:, :, 3]
            edge = np.concatenate([alpha[0], alpha[-1], alpha[:, 0], alpha[:, -1]])
            yy, xx = np.where(alpha >= 32)
            assert len(xx) and int(edge.max()) < 8, f'Incomplete or empty motif {index}'
            metrics.append(dict(frame=index, edgeAlphaMax=int(edge.max()), nonzeroAlpha=int((alpha>0).sum()),
                                softAlpha=int(((alpha>0)&(alpha<255)).sum()),
                                visibleBoundsAlpha32=[int(xx.min()+x0),int(yy.min()+y0),int(xx.max()+1+x0),int(yy.max()+1+y0)],
                                colorsAtAlpha128=len(np.unique(part[alpha>=128], axis=0))))
    data.update(imageSha256=pipeline.sha(target), nativeSize=[width,height], sourceSize=[width,height],
                sourcePath=source.relative_to(ROOT).as_posix(), sourceSha256=pipeline.sha(source),
                status='visual-reviewed', frames=frames, coordinatorReviewed=False, agentReviewed=True,
                humanApproved=False, unityImported=False, runtimeVerified=False,
                logicalResolution='Generated native raster 1448x1086; exact indexed logical grid unverified',
                notes=[
                    '12 complete contact effects; source preserved and delivered PNG copied byte-for-byte.',
                    'Agent visually reviewed all whole motifs against the complete canonical Katana sheet and complete v1 source.',
                    'v1/v2 rejected for broad soft halos; v3 selected for compact graphic contours and distinct temporal stages.',
                    'Residual generated fractional-alpha edge texture and palette expansion remain; binary alpha and 12-16 indexed colors are NOT achieved.',
                    'No threshold, palette quantization, cleanup, recolor or background removal was applied.',
                    'All 12 cell boundaries have alpha <=2/255 (no visible crossing). Every detached particle remains in its complete motif.',
                    'PPU512 retained; full cell width is 0.70703125world units. Verify contact size and occlusion in Unity.',
                    'Pivots manually estimated emission origins, approximate uncertainty8 native pixels.',
                    'Point filter/no mipmaps/uncompressed; 25/40/45/55ms=165ms per clip; non-looping.',
                    'visual-reviewed records agent inspection, not perfect indexed-pixel compliance, human approval, Unity import or runtime verification.'
                ])
    pipeline.write(RES / 'ContactBursts.json', data)
    pipeline.RES, pipeline.OUT = RES, OUT
    checks = pipeline.qa('ContactBursts')
    preview = OUT / 'ContactBursts/preview'
    # APNG preserves the exact millisecond contract; GIF cannot encode25/45/55ms.
    sheet = Image.open(target).convert('RGBA')
    for row, name in enumerate(['Punch','Katana','Heavy']):
        views = []
        for frame in frames[row*4:row*4+4]:
            x,y,w,h = frame['rect']
            crop = sheet.crop((x,y,x+w,y+h))
            anchor = [round(frame['pivot'][0]*w), round((1-frame['pivot'][1])*h)]
            canvas = Image.new('RGBA',(512,512))
            canvas.alpha_composite(crop,(256-anchor[0],256-anchor[1]))
            views.append(canvas)
        views[0].save(preview / (name+'-exact165ms.png'),save_all=True,append_images=views[1:],
                      duration=TIMINGS,loop=0,disposal=1,blend=0)
    reference = ROOT / 'Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqKatanaNormalNeutralIdleS_SW_W20261025.png'
    receipt = dict(status='visual-reviewed', selectedVersion='v3', imageSha256=data['imageSha256'],
                   sourcePath=data['sourcePath'], sourceSha256=data['sourceSha256'], nativeSize=[width,height],
                   referencePath=reference.relative_to(ROOT).as_posix(), referenceSha256=pipeline.sha(reference),
                   motifs=12, clips=3, timingsMs=TIMINGS, totalDurationMs=165, pixelsPerUnit=512,
                   exactSourceCopy=True, rasterCleanup=False, alphaLevels=len(np.unique(rgba[:,:,3])),
                   softAlphaPixels=int(((rgba[:,:,3]>0)&(rgba[:,:,3]<255)).sum()),
                   indexedPaletteAchieved=False, fieldsMeasured=metrics, qa=checks,
                   agentVisualReview=True, coordinatorReviewed=False, humanApproved=False,
                   unityImported=False, runtimeVerified=False, generationCallsThisRevision=4)
    pipeline.write(OUT / 'prepared.json',receipt)
    (OUT / 'REVIEW.md').write_text('# Contatos — revisão de27/10/2026\n\n'
        'Estado: visual-reviewed pelo agente.12motivos completos em3sequências não cíclicas. '
        'Nativo1448×1086,4colunas362px e3linhas de alturas384/312/390px,PPU512,tempos25/40/45/55ms.\n\n'
        'A folha v3 remove a névoa ampla de v1/v2 e mantém formas compactas, '
        'pico forte, fragmentação e remanescentes distintos. V4/v5 foram preservadas; '
        'não substituem v3 porque acentuam franjas e simplificam o desenho sem resolver alpha.\n\n'
        'Limite real: alpha fracionário e textura fina de borda gerados permanecem; '
        'paleta indexada12–16cores e alpha binário não foram alcançados. '
        'Não houve limpeza raster nem alteração de cores por scripts. '
        'Os limites das12células têm alpha máximo2/255, sem cruzamento visível; os motivos são inteiros.\n\n'
        'Prévia APNG conserva165ms exatos. GIF da ferramenta é aproximação temporal. '
        'Inspeção estática da fonte inteira concluída; animação e escala no jogo '
        'devem ser verificadas pelo coordenador. Unity/importação/runtime não executados neste pacote.\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in receipt.items() if k!='fieldsMeasured'},ensure_ascii=False))


if __name__ == '__main__':
    main()
