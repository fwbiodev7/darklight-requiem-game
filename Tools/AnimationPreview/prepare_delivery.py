"""Technical delivery metadata and a schematic UI layout (not an engine capture)."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import hashlib,json,shutil,zipfile
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/plano-implementacao-20261017';OUT.mkdir(exist_ok=True)
TRANS=ROOT/'outputs/transformacoes-animacoes-20261017'
def main():
    protected=list(TRANS.glob('morte-*.png'))+[ROOT/'Assets/Darklight/Runtime/Character/DarklightDeathReversion.cs',ROOT/'Assets/Darklight/Runtime/Character/DarklightOriginalVampireView.cs']
    hashes={str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in protected}
    baseline=OUT/'morte-preservada-sha256.json'
    if baseline.exists():assert json.loads(baseline.read_text(encoding='utf-8'))==hashes,'Protected death sources changed'
    else:baseline.write_text(json.dumps(hashes,indent=2),encoding='utf-8')
    # Render only interface geometry/text, matching the implemented 640x360 positions.
    # This is a labeled schematic, not generated character art or a gameplay screenshot.
    k=3;im=Image.new('RGB',(640*k,360*k),(9,6,14));d=ImageDraw.Draw(im)
    def rect(box,fill,outline=None,width=1):d.rectangle(tuple(round(v*k) for v in box),fill=fill,outline=outline,width=width*k)
    def text(x,y,value,size,color,center=False):
        f=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf',size*k)
        if center:x-=d.textbbox((0,0),value,font=f)[2]/(2*k)
        d.text((round(x*k),round(y*k)),value,font=f,fill=color)
    rect((24,16,616,344),(15,10,19),(78,31,43))
    text(320,51,'GAME OVER',24,(204,36,54),True)
    text(320,108,'O VÉU TE ALCANÇOU',7,(220,207,216),True)
    text(320,134,'ÚLTIMO PONTO SALVO',7,(200,180,190),True)
    text(320,148,'CAPÍTULO E SELO DO SEU SALVAMENTO',6,(180,166,177),True)
    for i,label in enumerate(['RETOMAR ÚLTIMO PONTO','CARREGAR SALVAMENTO','MENU PRINCIPAL']):
        y=191+i*36;rect((170,y,470,y+30),(67,21,36) if i==0 else (25,18,30),(170,47,67) if i==0 else (75,54,67))
        text(320,y+10,label,7,(240,226,229),True)
    text(320,323,'SETAS / LS: ESCOLHER    ENTER / A: CONFIRMAR',5,(174,158,174),True)
    text(320,350,'ESQUEMA DE INTERFACE · NÃO É CAPTURA DA UNITY',4,(168,148,165),True)
    im.save(OUT/'game-over-esquema.png')
    shutil.copy2(ROOT/'work/game-over-validation/managed-results.json',OUT/'testes-logica-gerenciada.json')
    for name in ['Darklight.Runtime.log','Darklight.Editor.log','Darklight.EditModeTests.log']:
        shutil.copy2(ROOT/'work/game-over-validation'/name,OUT/name)
    zip_path=ROOT/'outputs/darklight-transformacoes-20261017.zip'
    with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED) as z:
        for p in sorted(TRANS.iterdir()):
            if p.is_file():z.write(p,p.relative_to(ROOT/'outputs'))
    with zipfile.ZipFile(zip_path) as z:assert z.testzip() is None
    print(json.dumps({'zip':zip_path.name,'MB':round(zip_path.stat().st_size/1048576,1),'verified':True,'UI':'schematic, not Unity screenshot'}))
if __name__=='__main__':main()
