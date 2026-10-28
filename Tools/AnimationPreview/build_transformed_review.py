"""Read-only whole-frame reviews. No recoloring, limb assembly or source changes."""
import hashlib,json,zipfile
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
from build_review import ROOT,OUT as NORMAL,complete_frames,render_whole,clips_for,projection,cuts
OUT=ROOT/'outputs/transformacoes-animacoes-20261017'
FORMS=['vermelha','roxa','branca']
FONT=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
def labeled(im,label):
    canvas=Image.new('RGB',(420,450),(23,23,31));canvas.paste(im,(0,30))
    ImageDraw.Draw(canvas).text((16,7),label,font=FONT,fill=(235,228,230));return canvas
def save_gif(path,frames,ms,loop):
    durations=[ms]*len(frames);durations[0]+=120;durations[-1]+=280 if loop else 1200
    frames[0].save(path,save_all=True,append_images=frames[1:],duration=durations,
                   disposal=2,optimize=False,**({'loop':0} if loop else {}))
    with Image.open(path) as im:
        assert im.n_frames==len(frames),(path,im.n_frames,len(frames))
def death_frames(im):
    # Falling figures have unequal row heights. Find the real inter-row gutter;
    # nominal half-height slicing would include a neighboring pose in the preview.
    horizontal,_=projection(im)
    cut=min(range(round(im.height*.35),round(im.height*.7)),
            key=lambda y:(sum(horizontal[max(0,y-3):y+4]),abs(y-im.height*.5)))
    result=[]
    for top,bottom in [(0,cut),(cut,im.height)]:
        for frame in complete_frames(im.crop((0,top,im.width,bottom)),1,4):
            frame['index']=len(result);frame['rect'][1]+=top;frame['rect'][3]+=top;frame['anchor'][1]+=top
            result.append(frame)
    return result
def local_frames(im,rows,count):
    frames=complete_frames(im,rows,count)
    # Local gutters isolate whole figures when neighboring poses have unequal heights.
    # Never erase pixels or compose parts: only change complete-frame rectangle bounds.
    for frame in frames:
        x0,_,x1,_=frame['rect'];horizontal,_=projection(im.crop((x0,0,x1,im.height)))
        yy=cuts(horizontal,rows);row=frame['index']//4
        rect=[x0,yy[row],x1,yy[row+1]]
        box=im.crop(rect).getchannel('A').point(lambda a:255 if a>=180 else 0).getbbox()
        if box is None:raise ValueError('Empty whole figure')
        frame['rect']=rect;frame['opaqueBoundsInCell']=list(box);frame['anchor'][1]=rect[1]+box[3]
        frame['touchesCellEdge']=box[0]==0 or box[1]==0 or box[2]==rect[2]-rect[0] or box[3]==rect[3]-rect[1]
    return frames
def main():
    normal=json.loads((NORMAL/'manifesto-preview.json').read_text(encoding='utf-8'))['sheets']
    entries=[];deaths=[];qa=[]
    for n in normal:
        source=Image.open(NORMAL/n['file']).convert('RGBA');variants=[]
        n['frames']=local_frames(source,n['rows'],n['frameCount'])
        for form in FORMS:
            key=n['id']+'-'+form;path=OUT/(key+'.png');im=Image.open(path).convert('RGBA')
            assert im.size==source.size,(key,im.size,source.size)
            frames=local_frames(im,n['rows'],n['frameCount'])
            e={k:v for k,v in n.items() if k not in ('frames','clips','sha256')}
            e.update(id=key,file=path.name,form=form,frames=frames,clips=clips_for(n['id']),
                     alphaRange=list(im.getchannel('A').getextrema()),sha256=hashlib.sha256(path.read_bytes()).hexdigest())
            assert e['alphaRange'][0]==0
            for idx,clip in enumerate(e['clips']):
                name=key+(f'-{idx+1}' if len(e['clips'])>1 else '')+'.gif'
                # A separate non-looping death reversion supersedes transformed defeat poses.
                if n['id'].startswith('suporte') and idx==3:
                    clip['supersededBy']='morte-'+n['skin'].lower()+'-'+form+'.gif'
                    continue
                save_gif(OUT/name,[render_whole(im,e,frames[f]) for f in clip['frames']],clip['frameMs'],clip['loop'])
                clip['gif']=name
            entries.append(e);variants.append((form,im,e))
        samples=[0,n['frameCount']//2,n['frameCount']-1]
        contact=Image.new('RGB',(1680,450*len(samples)),(23,23,31))
        for row,f in enumerate(samples):
            contact.paste(labeled(render_whole(source,n,n['frames'][f]),f"{n['skin']} normal | quadro {f+1}"),(0,row*450))
            for col,(form,im,e) in enumerate(variants,1):
                contact.paste(labeled(render_whole(im,e,e['frames'][f]),form),(col*420,row*450))
        contact.save(OUT/('comparacao-'+n['id']+'.jpg'),quality=90);qa.append('comparacao-'+n['id']+'.jpg')
        # Four-way animated comparisons of actual source frames, no intermediate interpolation.
        for idx,clip in enumerate(n['clips']):
            if n['id'].startswith('suporte') and idx==3:continue
            sequence=[]
            for f in clip['frames']:
                panel=Image.new('RGB',(1680,450),(23,23,31))
                panel.paste(labeled(render_whole(source,n,n['frames'][f]),'Normal'),(0,0))
                for col,(form,im,e) in enumerate(variants,1):
                    panel.paste(labeled(render_whole(im,e,e['frames'][f]),form),(420*col,0))
                sequence.append(panel)
            save_gif(OUT/('comparacao-'+n['id']+f'-{idx+1}.gif'),sequence,clip['frameMs'],clip['loop'])
    for skin in ['requiem','rock']:
        contact=Image.new('RGB',(1260,900),(23,23,31))
        for col,form in enumerate(FORMS):
            key='morte-'+skin+'-'+form;path=OUT/(key+'.png');im=Image.open(path).convert('RGBA')
            frames=death_frames(im);h=frames[0]['opaqueBoundsInCell'];scale=min(1.6,285/(h[3]-h[1]))
            e=dict(id=key,file=path.name,skin=skin,form=form,frameCount=8,rows=2,columns=4,
                   previewScale=scale,frames=frames,alphaRange=list(im.getchannel('A').getextrema()),
                   sha256=hashlib.sha256(path.read_bytes()).hexdigest(),loop=False,integrationVerified=False)
            assert e['alphaRange'][0]==0
            save_gif(OUT/(key+'.gif'),[render_whole(im,e,f) for f in frames],100,False)
            for row,f in enumerate([0,7]):contact.paste(labeled(render_whole(im,e,frames[f]),form+(' | início' if row==0 else ' | final normal')),(420*col,450*row))
            deaths.append(e)
        contact.save(OUT/('morte-comparacao-'+skin+'.jpg'),quality=92)
    manifest=dict(date='2026-10-17',kind='whole-character-visual-prototypes',
                  integrationVerified=False,directions='one-view studies; not eight-direction production coverage',
                  sheets=entries,deathReversions=deaths,sourceSheets=33,generatedWholeFrames=600,
                  reviewedContactSheets=qa,
                  notes=['Whole-frame previews only; fixed uniform scale per sheet, no limb assembly.',
                         'Death art is frozen by the new master prompt; final two frames return to normal.',
                         'Raw transformed defeat poses are superseded by separate death reversion clips.',
                         'Pivots and cell edge contacts are provisional, requiring Unity import review.'])
    (OUT/'manifesto-transformacoes.json').write_text(json.dumps(manifest,indent=2,ensure_ascii=False),encoding='utf-8')
    print(json.dumps(dict(sheets=len(entries)+len(deaths),wholeFrames=600,gifs=len(list(OUT.glob('*.gif'))),
                          edgeContacts=sum(f['touchesCellEdge'] for e in entries+deaths for f in e['frames']),
                          integrity='GIF frame counts verified'),ensure_ascii=False))
if __name__=='__main__':main()
