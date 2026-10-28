"""Full-figure direction references: never isolate or assemble anatomical parts."""
from prepare_eclipse_fangs_preview import ROOT,extract,sp

OUT=ROOT/'ArtSources/RuinFists20261021/references'
def main():
    OUT.mkdir(parents=True,exist_ok=True);records=[]
    source=ROOT/'ArtSources/EclipseFangs20261021/candidate-v1/oito-direcoes-source.png'
    _,directions=extract(source,2)
    for i,direction in enumerate(['S','SW','W','NW','N','NE','E','SE']):
        origin=source;f=directions[i]
        if i==1:
            origin=ROOT/'ArtSources/EclipseFangs20261021/candidate-v1/correcao-sw-source.png'
            _,replacement=extract(origin,1);f=replacement[0]
        target=OUT/f'Requiem_Normal_{direction}_Whole.png';f['image'].save(target)
        records.append(dict(image=target.name,source=origin.relative_to(ROOT).as_posix(),
                            sourceSha256=sp.digest(origin),sourceRect=f['sourceRect'],sha256=sp.digest(target),wholeFigure=True,rgbaPreserved=True))
    source=ROOT/'ArtSources/EclipseFangs20261021/candidate-v1/formas-source.png'
    _,forms=extract(source,2)
    for i,f in enumerate(forms):
        target=OUT/f'{["Requiem","Rock"][i//4]}_{["Normal","Ruptura","Eclipse","Absoluto"][i%4]}_Whole.png'
        f['image'].save(target)
        records.append(dict(image=target.name,source=source.relative_to(ROOT).as_posix(),sourceSha256=sp.digest(source),
                            sourceRect=f['sourceRect'],sha256=sp.digest(target),wholeFigure=True,rgbaPreserved=True,
                            hairAuthority='ArtSources/AbsoluteHairReference20261021/referencia-cabelo-absoluto.png' if i%4==3 else None))
    sp.write(OUT/'provenance.json',records);print('16 full-figure references; source RGBA conserved')
if __name__=='__main__':main()
