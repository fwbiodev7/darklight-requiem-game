import sys, json
from pathlib import Path
from PIL import Image

REPO_ROOT = Path("c:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main")
FABIUS_DIR = REPO_ROOT / "ArtSources" / "FabiusForms20261109"
COMP_DIR = REPO_ROOT / "ArtSources" / "NovasArtesComplementares20261111"
DEST_DIR = REPO_ROOT / "Assets" / "Darklight" / "Resources" / "Darklight" / "FabiusForms20261111"
DEST_DIR.mkdir(parents=True, exist_ok=True)

print("Processador iniciado com sucesso...")

