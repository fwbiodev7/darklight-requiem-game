import shutil
from pathlib import Path

ROOT = Path(r"c:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main")
ACERVO = ROOT / "ArtSources/AcervoRecuperado20261111"

# 1. Inimigos Comuns
dest_enemies = ROOT / "ArtSources/InimigosComuns20261111"
dest_enemies.mkdir(parents=True, exist_ok=True)
for item in (ACERVO / "04_Inimigos_Comuns").glob("**/*.*"):
    if item.is_file() and item.suffix.lower() in (".png", ".jpg", ".jpeg"):
        shutil.copy2(item, dest_enemies / item.name)
print("Copied Inimigos Comuns to ArtSources/InimigosComuns20261111")

# 2. Bosses
dest_bosses = ROOT / "ArtSources/Bosses20261111"
dest_bosses.mkdir(parents=True, exist_ok=True)
for item in (ACERVO / "05_Bosses").glob("**/*.*"):
    if item.is_file() and item.suffix.lower() in (".png", ".jpg", ".jpeg"):
        shutil.copy2(item, dest_bosses / item.name)
print("Copied Bosses to ArtSources/Bosses20261111")

# 3. Fabius Formas (10 Folhas)
dest_fabius = ROOT / "ArtSources/FabiusForms20261109"
dest_fabius.mkdir(parents=True, exist_ok=True)
for item in (ACERVO / "01_Fabius_Formas_10_Folhas").glob("**/*.*"):
    if item.is_file() and item.suffix.lower() in (".png", ".jpg", ".jpeg"):
        # prefix with parent folder name to prevent overwriting
        parent_name = item.parent.name
        shutil.copy2(item, dest_fabius / f"{parent_name}_{item.name}")
print("Copied Fabius Formas to ArtSources/FabiusForms20261109")

