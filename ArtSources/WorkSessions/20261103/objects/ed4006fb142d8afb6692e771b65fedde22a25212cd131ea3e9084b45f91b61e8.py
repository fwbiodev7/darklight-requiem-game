from pathlib import Path
import hashlib,json
from PIL import Image,ImageDraw,ImageFilter
ROOT=Path(r'C:/Users/Usuario/Documents/Codex/2026-11-03/ba');OUT=ROOT/'outputs/fechamento-fabius-20261103/moto-qa';SRC=ROOT/'outputs/continuacao-escola-20261103/moto-walk';mp=SRC/'ReqMotoNormalWalkW_E20261103SchoolV2.json';d=json.loads(mp.read_text());im=Image.open(SRC/d['image']).convert('RGBA')
def write(n,o):(OUT/n).write_text(json.dumps(o,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
cells=[];alpha=[]
for f in d['frames']:
    x,y,w,h=f['rect'];cell=im.crop((x,y,x+w,y+h));cells.append(cell);a=cell.getchannel('A');core=a.point(lambda v:255 if v>16 else 0);dil=core.filter(ImageFilter.MaxFilter(33));hist=a.histogram();pairs=list(zip(a.getdata(),dil.getdata()));far=[v for v,m in pairs if v and not m]
    alpha.append(dict(id=f['id'],visible=sum(hist[1:]),residueAlpha1to16=sum(hist[1:17]),corePixels=sum(hist[17:]),coreBounds=list(core.getbbox()),fartherThan16pxFromCore=dict(count=len(far),maxAlpha=max(far,default=0),opaquePixelEquivalent=sum(far)/255),groundCoreEdge=core.getbbox()[3]))
for theme,color in [('light','#eef0f4'),('dark','#121722')]:
    contact=Image.new('RGB',(1920,1032),color);dr=ImageDraw.Draw(contact);fg='#121722' if theme=='light' else '#eef0f4'
    for i,c in enumerate(cells):
        row,col=divmod(i,4);x,y=col*480,row*516;contact.paste(c,(x,y),c);dr.line((x+6,y+420,x+473,y+420),fill='#788da7');dr.line((x+240,y+412,x+240,y+428),fill='#d39b00');dr.text((x+8,y+490),f'{["W","E"][row]} | {i%4+1} {d["frames"][i]["phase"]} | native 1x',fill=fg)
    contact.save(OUT/f'sequence-native-{theme}.png')
    for r,di in enumerate(['W','E']):
        strip=Image.new('RGB',(1920,516),color);strip.paste(contact.crop((0,r*516,1920,(r+1)*516)))
        strip.save(OUT/f'{di}-sequence-native-{theme}.png')
write('alpha-and-registration.json',dict(atlasSha256=sha(SRC/d['image']),allSourcePixelsPreserved=True,frames=alpha,sourceAltered=False,interpretation='Near-opaque generated alpha is retained. Far residuals quantified without removing pixels. Core threshold16 used only for measurement, never export.'))
# Manual observations on complete native-frame rulers, pixels from canvas top-left.
landmarks=[
 dict(name='Moto W A',file='Moto-W-A.png',crownY=51,chinY=152,neckRootY=165,beltCenterY=221,ppu=d['pixelsPerUnit'],view='W'),
 dict(name='Moto W B',file='Moto-W-B.png',crownY=50,chinY=152,neckRootY=165,beltCenterY=220,ppu=d['pixelsPerUnit'],view='W'),
 dict(name='Moto E A',file='Moto-E-A.png',crownY=53,chinY=155,neckRootY=167,beltCenterY=220,ppu=d['pixelsPerUnit'],view='E'),
 dict(name='Moto E B',file='Moto-E-B.png',crownY=52,chinY=155,neckRootY=166,beltCenterY=220,ppu=d['pixelsPerUnit'],view='E'),
 dict(name='Moto S A',file='Moto-S-A.png',crownY=26,chinY=146,neckRootY=157,beltCenterY=203,ppu=195.95959595959596,view='S'),
 dict(name='Moto SW A',file='Moto-SW-A.png',crownY=44,chinY=162,neckRootY=177,beltCenterY=223,ppu=198,view='SW'),
 dict(name='Katana W upright',file='Katana-W-upright-manual-whole.png',crownY=23,chinY=142,neckRootY=151,beltCenterY=211,ppu=175,view='W'),
 dict(name='Katana E walking A',file='Katana-E-walking-A.png',crownY=20,chinY=125,neckRootY=137,beltCenterY=184,ppu=173.23232323232324,view='E'),
 dict(name='Katana E walking B',file='Katana-E-walking-B.png',crownY=25,chinY=132,neckRootY=145,beltCenterY=193,ppu=173.23232323232324,view='E')]
for v in landmarks:
    v.update(headHeightPx=v['chinY']-v['crownY'],chinToBeltPx=v['beltCenterY']-v['chinY'],neckToBeltPx=v['beltCenterY']-v['neckRootY'],crownToBeltPx=v['beltCenterY']-v['crownY'])
    v.update(headOverChinToBelt=round(v['headHeightPx']/v['chinToBeltPx'],3),crownToBeltWorldUnits=round(v['crownToBeltPx']/v['ppu'],4),manualUncertaintyPx=4,measurementMethod='Visual full-figure native ruler; includes hair silhouette; chin and collar partially occluded; not automated anatomy detection')
    fr=Image.open(OUT/v['file']).convert('RGBA');bg=Image.new('RGBA',fr.size,'#e7ebef');bg.alpha_composite(fr);dr=ImageDraw.Draw(bg)
    for key,co in [('crownY','#168844'),('chinY','#9e4bce'),('neckRootY','#0088cc'),('beltCenterY','#b34a12')]:dr.line((5,v[key],fr.width-5,v[key]),fill=co,width=1);dr.text((5,v[key]+2),key+'='+str(v[key]),fill=co)
    bg.convert('RGB').save(OUT/(v['file'][:-4]+'-landmarks.png'))
write('manual-upper-body-measurements.json',dict(method='Manual landmarks, no resize/normalization, no corpse or weapon bbox used',uncertainty='±4 pixels per landmark, ±8 on derived lengths in worst case; camera/view differences limit direct comparisons',landmarks=landmarks))
comparison=Image.new('RGB',(1800,1080),'#dfe3e7');dr=ImageDraw.Draw(comparison)
selection=[landmarks[i] for i in [0,2,6,4,5,7]]
for i,v in enumerate(selection):
    fr=Image.open(OUT/v['file']).convert('RGBA');a=fr.getchannel('A').point(lambda a:255 if a>16 else 0);ground=a.getbbox()[3];x=i%3*600+10;y=i//3*540+450-ground;comparison.paste(fr,(x,y),fr);dr.line((i%3*600+5,i//3*540+450,i%3*600+594,i//3*540+450),fill='#8190a5');dr.text((i%3*600+10,i//3*540+500),f'{v["name"]} | native 1x | head {v["headHeightPx"]} / chin-belt {v["chinToBeltPx"]} px',fill='#111111')
comparison.save(OUT/'native-reference-comparison.png')
write('metadata-proposal.json',dict(status='READY_FOR_COORDINATOR',applied=False,sourceUnchanged=True,targetManifest=str(mp),targetManifestSha256=sha(mp),keepPixelsPerUnit=186.36363636363637,ppuEvidence='Retain uniform PPU: crown-to-belt gives Moto W 0.9122, E 0.8961 versus Moto S 0.9032 and SW 0.9040 world units (manual uncertainty). Previous total-height derivation is superseded as justification; no visual/anatomical scaling correction applied.',requiredBeforeImport=['Temporal review by coordinator: W/E four-phase cycle 4->1, low swing/contact and foot sliding/idle transitions','Stage exact composite source under declared ArtSources path; verify SHA256 f7183e8e8c255aab2b22dd60b58dfb09ee81b2c38583ad8d3ddd71d25baed9e0','After real review, coordinator may set status visual-reviewed and update review fields; no automatic status promotion','Import actual RGBA32/Point/NPOTNone/uncompressed/MipmapOff slices with stable GUID/name tracking and actual max size >=1920','Add only Walk pages skin0 weapon3 form0 W=2/E=6 using existing bank, four IDs+120ms/loop; verify OnFoot path selects them and Mounted path retains motorcycle'],replaceIntegrationBlockReason='Pending actual W/E temporal review, exact source installation, bounded catalog/import configuration and Unity verification; independent of any absent home package or unrelated older sheets.',optionalRegistrationProposal=dict(applyOnlyAfterTemporalReview=True,currentPivotPixel=[240,420],candidatePivotPixel=[240,419],reason='Core sole edge419 for all W and E contact; E passing420. A uniform 1px root shift would reduce W static 1px gap without altering source. Difference is within manual uncertainty and need not be applied before seeing runtime.',notAnArtCorrection=True),noImageGenerationNeeded='No concrete mature-proportion defect established; do not regenerate correct complete figures speculatively.'))
write('handoff.json',dict(status='READY_FOR_COORDINATOR',scope='Only new Moto Normal OnFoot W/E clips, eight complete figures',staticReviewCompleted=True,temporalReviewCompleted=False,unityImported=False,runtimeVerified=False,automaticArtApproval=False,homePackageRequired=False,legacy52SheetsRequired=False,sourceModified=False,artisticStaticBlockers=[],requiredGates=['coordinator temporal review','install exact ArtSources source and provenance','bounded catalog + stable slices/PPU import','Unity OnFoot playback and idle/motion transitions'],metadataProposed=True,ppuValidatedByUpperBodyApproximation=True,alphaResidualsRetained=True))
print(json.dumps(dict(status='READY_FOR_COORDINATOR',frames=8,temporal=False,artSourceModified=False)))
