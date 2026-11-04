from pathlib import Path
from PIL import Image,ImageDraw
import json
R=Path(r'C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main');S=R/'ArtSources/WeaponRedesign20261102/normal-katana-completion';O=R/'outputs/retomada-20261102/normal-katana-completion'
for pair,ref in [('S_SW','S_SW_W'),('W_NW','S_SW_W'),('N_NE','NW_N_NE')]:
 im=Image.open(S/('ReqKatanaNormalCarregado'+pair+'20261102V1-source.png')).convert('RGBA');ri=Image.open(R/('ArtSources/WeaponRedesign20261025/candidates/ReqKatanaNormalNeutralIdle'+ref+'20261025-source.png')).convert('RGBA')
 co=Image.new('RGB',(ri.width+im.width,max(ri.height,im.height)+30),(66,66,73));d=ImageDraw.Draw(co)
 co.paste(ri,(0,30),ri);co.paste(im,(ri.width,30),im);d.text((10,8),'APPROVED KATANA IDLE: complete source, native pixels',fill='white');d.text((ri.width+10,8),'REJECTED CARREGADO '+pair+': complete source, native pixels',fill='white')
 co.save(O/('whole-source-comparison-'+pair+'-v1.png'))
p=O/'proportion-review-v1.json';m=json.loads(p.read_text());m['comparisonMethod']='Whole source PNGs side by side at native pixels, no cropping/resampling. Earlier cell contacts are superseded diagnostics; complete source comparison is authoritative.';m['extractionFailure']='Connected-component attempt found 7 rather than8; sources rejected, not split, erased or anatomically assembled.';p.write_text(json.dumps(m,indent=2)+'\n');print('3 whole-source comparisons saved; 24 candidate figures rejected; 0 approved edits')
