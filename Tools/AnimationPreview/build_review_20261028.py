"""Reuse the established viewer, preserving full frames and their actual timings."""
from pathlib import Path
import json
import re

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/retomada-20261028'
SHEETS=[
    ('Punhos — Giro, Sul a Noroeste','punhos/staged/ReqFistsNormalGiroSSWWNW20261028V1.json'),
    ('Punhos — Giro, Norte a Sudeste','punhos/staged/ReqFistsNormalGiroNNEESE20261028Selected.json'),
    ('Moto — reunião e separação','moto/staging/ReqMotoNormalJoinSeparateS20261028V1.json'),
    ('Katana — terceiro corte, Sul a Oeste','katana/staged/ReqKatanaNormalLeve3S_SW_W20261028Selected.json'),
    ('Katana — terceiro corte, Leste/Sudeste','katana/staged/ReqKatanaNormalLeve3E_SE20261028Selected.json'),
]
def main():
    sheets=[]
    for title,relative in SHEETS:
        path=OUT/relative
        if not path.exists():continue
        data=json.loads(path.read_text(encoding='utf-8-sig'));indices={f['id']:i for i,f in enumerate(data['frames'])}
        frames=[]
        for frame in data['frames']:
            x,y,w,h=frame['rect'];px,py=frame['pivot']
            frames.append({'rect':[x,y,x+w,y+h],'anchor':[x+round(px*w),y+round((1-py)*h)],'durationMs':frame['durationMs']})
        clips=[{'name':clip['name'].replace('assemble_S','Reunir — Sul').replace('separate_S','Separar — Sul'),
                'frames':[indices[name] for name in clip['frames']],'loop':True,'gif':'#'} for clip in data['clips']]
        sheets.append({'weapon':title,'skin':'Réquiem','imageData':path.with_suffix('.png').relative_to(OUT).as_posix(),
                       'file':path.with_suffix('.png').relative_to(OUT).as_posix(),'frameCount':len(frames),'frames':frames,'clips':clips,
                       'previewScale':72*1.325*1.5/data['pixelsPerUnit']})
    template=(ROOT/'Tools/AnimationPreview/review-template.html').read_text(encoding='utf-8')
    header='<header><div><div class="eyebrow">DARKLIGHT: REQUIEM · 28/10/2026</div><h1>Réquiem em movimento.</h1><p>Novos golpes e a separação da Moto em duas serras. Pause e avance os quadros para conferir cada movimento.</p><p>Coleção parcial em integração. A repetição desta prévia facilita a revisão; no jogo os golpes seguem suas janelas próprias.</p></div></header>'
    template=re.sub(r'<header>.*?</header>',lambda _:header,template,flags=re.S)
    template=re.sub(r'(<select[^>]+id="skin"[^>]*>).*?(</select>)',r'\1<option value="">Réquiem</option>\2',template,flags=re.S)
    template=template.replace('</style>','.gif{display:none}</style>')
    tick='''function tick(now){for(const p of players){if(!p.playing||!p.img.complete||p.card.hidden)continue;const c=p.sheet.clips[p.clip];if(now>=p.next){if(p.next!==0){if(p.frame===c.frames.length-1){if(!c.loop){p.playing=false;p.draw();continue}p.frame=0}else p.frame++}p.draw();const f=p.sheet.frames[c.frames[p.frame]];p.next=now+f.durationMs/speed}}requestAnimationFrame(tick)}requestAnimationFrame(tick);'''
    template=re.sub(r'function tick\(now\).*?requestAnimationFrame\(tick\);(?=\s*window.previewReview)',lambda _:tick,template,flags=re.S)
    template=template.replace('__SHEETS__',json.dumps(sheets,ensure_ascii=False)).replace('__ORIGINAL__','#')
    (OUT/'index.html').write_text(template,encoding='utf-8')
    print(json.dumps({'sheets':len(sheets),'frames':sum(s['frameCount'] for s in sheets),'clips':sum(len(s['clips'])for s in sheets)}))
if __name__=='__main__':main()
