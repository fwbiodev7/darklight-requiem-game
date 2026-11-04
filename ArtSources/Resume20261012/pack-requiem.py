from pathlib import Path
from crop_provenance import record_crops

# Technical atlas assembly only: reuse the existing component-based cropper.
root = Path(__file__).resolve().parent.parent
source = (root / 'master-expansion-stage/pack-directional.py').read_text(encoding='utf-8')
source = source.replace("s=Path(__file__).parent", "s=Path(__file__).resolve().parent")
source = source.replace('exec-6270dc37-21e3-4f00-b98a-63e6f50adca7.png', 'exec-800a4257-b971-4fc3-bc8e-776f52b245e9.png')
source = source.replace('exec-3b800528-8b5f-4e85-bf8b-dc0128035c8d.png', 'exec-59b6de73-1b88-467c-9069-8e1ef62da7a5.png')
source = source.replace('RockKatanaLightStage4', 'RequiemKatanaLightStage4')
source = source.replace('rock-light-direction-report.json', 'requiem-light-direction-report.json')
(Path(__file__).parent/'Assets/Darklight/Resources/Darklight/AnimationCatalog').mkdir(parents=True, exist_ok=True)
exec(compile(record_crops(source), str(__file__), 'exec'))
