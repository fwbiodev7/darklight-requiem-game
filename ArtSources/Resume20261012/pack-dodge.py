from pathlib import Path
import shutil
from crop_provenance import record_crops

here=Path(__file__).resolve().parent
text=(here.parent/'master-expansion-stage/pack-directional.py').read_text(encoding='utf-8')
text=text.replace("s=Path(__file__).parent", "s=Path(__file__).resolve().parent")
text=text.replace('01a12b16-c088-7f73-a2eb-6ff2fde39e75','01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')
text=text.replace('exec-6270dc37-21e3-4f00-b98a-63e6f50adca7.png','exec-385db0d2-a7db-4524-8966-812119b55930.png')
text=text.replace('exec-3b800528-8b5f-4e85-bf8b-dc0128035c8d.png','exec-7a7673c6-6886-4e5d-a80c-03aa91835371.png')
text=text.replace('RockKatanaLightStage4','RequiemKatanaDodgeStage4')
text=text.replace('rock-light-direction-report.json','requiem-dodge-report.json')
text=text.replace('accepted; swing torso rotation remains within facing viewpoint','accepted; four dodge phases; north/right hand corrected; common recovery-pose PPU preserves crouch height')
exec(compile(record_crops(text),str(__file__),'exec'))
folder=here/'art-sources'
for name in ['exec-385db0d2-a7db-4524-8966-812119b55930.png','exec-7a7673c6-6886-4e5d-a80c-03aa91835371.png']:
    shutil.copy2(Path('C:/Users/Usuario/.codex/generated_images/01a13015-b5c3-7ff3-8d1c-cefab2fed5b3')/name,folder/name)
