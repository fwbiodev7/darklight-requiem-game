"""Choose representative whole-sheet metadata for eye-palette visual QA."""
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT/'outputs/olhos-todas-formas-20261020/metadata'
OUT = ROOT/'outputs/olhos-todas-formas-20261020/pilot-metadata'
TOKENS = [
    'RequiemDodgeSupportDaggersYoung_Violet',
    'RequiemDodgeSupportDaggersYoung_White',
    'RockWalkKatanaYoung_Crimson',
    'RockCombatSupportKatanaYoung_Violet',
    'RequiemIdleLivingCardinal20261017',
    'RequiemIdleLivingCardinal20261017_Absolute',
    'RockIdleLivingDiagonal20261017_Violet',
    'AbyssPrototype__AttackSix0',
    'UserWholeSprites__requiemkatana1',
    'TodayCharacterAnimations__RockWalkKatanaYoung',
]
OUT.mkdir(exist_ok=True)
selected=[]
for path in SOURCE.glob('*.json'):
    if any(path.stem.endswith(t) for t in TOKENS):
        shutil.copy2(path,OUT/path.name)
        selected.append(path.name)
print({'pilotSheets':len(selected)})
