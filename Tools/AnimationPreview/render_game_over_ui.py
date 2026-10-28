"""Render a UI composition from unchanged project textures and its pixel font.

This is an offline layout preview, never an engine/gameplay screenshot. Atlas
viewports mirror existing UI sprite descriptors; no character art is edited.
"""
import base64,hashlib,io,json
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'outputs/game-over-20261017'
RES=ROOT/'Assets/Darklight/Resources/Darklight'
FONT=ROOT/'Assets/Darklight/Art/Fonts/PressStart2P-Regular.ttf'
UI=ROOT/'Assets/Darklight/Art/User/ui.png'
def data(path):return 'data:image/png;base64,'+base64.b64encode(path.read_bytes()).decode()
def main():
    OUT.mkdir(exist_ok=True)
    sources={'city':RES/'Menu/Vespera.png','mist':RES/'Menu/Mist.png',
             'shrine':RES/'Props/CheckpointShrine.png','logo':RES/'Branding/GameLogo.png','ui':UI}
    defs=[]
    for name,path in sources.items():
        with Image.open(path) as im:w,h=im.size
        defs.append(f'<image id="{name}" width="{w}" height="{h}" href="{data(path)}"/>')
    defs+=['<filter id="cityTint" color-interpolation-filters="sRGB"><feColorMatrix type="matrix" values=".78 0 0 0 0  0 .7 0 0 0  0 0 .8 0 0  0 0 0 1 0"/></filter>',
           '<filter id="frameTint" color-interpolation-filters="sRGB"><feColorMatrix type="matrix" values=".6 0 0 0 0  0 .39 0 0 0  0 0 .46 0 0  0 0 0 .42 0"/></filter>']
    parts=[]
    def image(name,x,y,w,h,preserve=False,opacity=1,extra=''):
        sw,sh=Image.open(sources[name]).size
        if preserve:
            scale=min(w/sw,h/sh);nw,nh=sw*scale,sh*scale;x+=(w-nw)/2;y+=(h-nh)/2;w,h=nw,nh
        parts.append(f'<svg x="{x}" y="{y}" width="{w}" height="{h}" viewBox="0 0 {sw} {sh}" preserveAspectRatio="none" opacity="{opacity}" {extra}><use href="#{name}"/></svg>')
    def rect(x,y,w,h,fill,opacity=1):parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{fill}" opacity="{opacity}"/>')
    def label(value,x,y,w,h,size,color=(240,230,224),align='left'):
        # Render native UI text with the actual approved font, at 4x layout pixels.
        scale=4;canvas=Image.new('RGBA',(round(w*scale),round(h*scale)))
        d=ImageDraw.Draw(canvas);font=ImageFont.truetype(str(FONT),round(size*scale))
        lines=value.split('\n');line_height=size*1.25
        for row,line in enumerate(lines):
            width=d.textlength(line,font=font);px=(w*scale-width)/2 if align=='center' else 0
            py=row*line_height*scale
            if len(lines)==1:py=(h-size)*scale/2
            for dx,dy in [(-scale,-scale),(scale,-scale),(-scale,scale),(scale,scale)]:
                d.text((px+dx,py+dy),line,font=font,fill=(0,0,0,255),anchor='lt')
            d.text((px,py),line,font=font,fill=tuple(color)+(255,),anchor='lt')
        buf=io.BytesIO();canvas.save(buf,format='PNG');uri='data:image/png;base64,'+base64.b64encode(buf.getvalue()).decode()
        parts.append(f'<image x="{x}" y="{y}" width="{w}" height="{h}" href="{uri}"/>')
    frames=json.loads((UI.parent/'uiRects.json').read_text(encoding='utf-8'))['frames']
    def nine_slice(index,x,y,w,h,filter_name=None,opacity=1):
        f=frames[index];sx,sy,sw,sh=f['rect'];left,bottom,right,top=f['border']
        # Canvas reference PPU 100 / sprite PPU 48 / multiplier 8.3333 = 0.25.
        scale=.25;xs=[sx,sx+left,sx+sw-right,sx+sw];ys=[sy,sy+top,sy+sh-bottom,sy+sh]
        dx=[x,x+left*scale,x+w-right*scale,x+w];dy=[y,y+top*scale,y+h-bottom*scale,y+h]
        extra=f' filter="url(#{filter_name})"' if filter_name else ''
        parts.append(f'<g opacity="{opacity}"{extra}>')
        for r in range(3):
            for c in range(3):
                parts.append(f'<svg x="{dx[c]}" y="{dy[r]}" width="{dx[c+1]-dx[c]}" height="{dy[r+1]-dy[r]}" viewBox="{xs[c]} {ys[r]} {xs[c+1]-xs[c]} {ys[r+1]-ys[r]}" preserveAspectRatio="none" overflow="hidden"><use href="#ui"/></svg>')
        parts.append('</g>')
    image('city',0,0,640,360,extra='filter="url(#cityTint)"')
    rect(0,0,640,360,'#040206',.38)
    image('logo',54,30,182,62,True,.86)
    rect(308,20,316,324,'#060409',.94);nine_slice(0,308,20,316,324,'frameTint')
    rect(308,42,1,280,'#850e1f',.9)
    image('shrine',64,113,162,174,True)
    image('mist',-20,234,680,112,False,.16)
    label('O SELO AINDA TE AGUARDA',42,293,206,14,6,align='center')
    label('Retome seu caminho na forma normal.',44,313,202,22,5,(168,153,171),'center')
    label('D A R K L I G H T   /   R E Q U I E M',336,46,264,12,5,(168,135,150))
    label('GAME OVER',336,72,274,42,26,(230,54,69))
    rect(340,117,252,1,'#6e2136',.7)
    label('O VÉU TE ALCANÇOU\n\nÚLTIMO PONTO SALVO\nRuas de Vespera · SELO 1',340,128,258,51,7)
    for i,value in enumerate(['RETOMAR ÚLTIMO PONTO','CARREGAR SALVAMENTO','MENU PRINCIPAL']):
        y=192+i*37;nine_slice(2 if i==0 else 1,336,y,264,30,opacity=1 if i==0 else .65)
        label(value,344,y,248,30,7,(245,230,219),'center')
    label('SETAS / LS: ESCOLHER   ENTER / A: CONFIRMAR',336,328,284,12,4)
    svg='<svg xmlns="http://www.w3.org/2000/svg" width="1280" height="720" viewBox="0 0 640 360"><defs>'+''.join(defs)+'</defs>'+''.join(parts)+'</svg>'
    (OUT/'game-over-texturas.svg').write_text(svg,encoding='utf-8')
    (OUT/'fontes-e-validacao.json').write_text(json.dumps(dict(kind='offline UI layout preview, not Unity capture',
        assets=[dict(path=str(p.relative_to(ROOT)),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in [*sources.values(),FONT]],
        sourceTexturesUnchanged=True,engineVerified=False,exampleLocation='Ruas de Vespera · SELO 1; runtime uses the actual saved chapter'),indent=2,ensure_ascii=False),encoding='utf-8')
    print('Saved textured UI SVG and original-asset hashes.')
if __name__=='__main__':main()
