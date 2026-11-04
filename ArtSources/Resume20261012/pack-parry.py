from pathlib import Path
import shutil
from crop_provenance import record_crops

here=Path(__file__).resolve().parent
text=(here.parent/'master-expansion-stage/pack-directional.py').read_text(encoding='utf-8')
text=text.replace('s=Path(__file__).parent','s=Path(__file__).resolve().parent')
text=text.replace('01a12b16-c088-7f73-a2eb-6ff2fde39e75','01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')
files=['exec-45e53aa8-7be5-4418-9fee-40d84e13daf7.png','exec-6633765c-8567-4e80-bc2f-567477ff115a.png']
text=text.replace('exec-6270dc37-21e3-4f00-b98a-63e6f50adca7.png',files[0]).replace('exec-3b800528-8b5f-4e85-bf8b-dc0128035c8d.png',files[1])
text=text.replace('RockKatanaLightStage4','RequiemKatanaParryStage4').replace('rock-light-direction-report.json','requiem-parry-report.json')
text=text.replace('accepted; swing torso rotation remains within facing viewpoint','accepted; four defense phases in eight directions; north blade tips repaired before packing')
exec(compile(record_crops(text),str(__file__),'exec'))
for name in files:
    shutil.copy2(Path('C:/Users/Usuario/.codex/generated_images/01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')/name,here/'art-sources'/name)
