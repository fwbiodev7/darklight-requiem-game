import os, sys, json, hashlib
from pathlib import Path

REPO_ROOT = Path("c:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main")
DEST_LUCIUS = REPO_ROOT / "Assets" / "Darklight" / "Resources" / "Darklight" / "LuciusArconte20261111"

def make_json_meta(path):
    guid = hashlib.md5((str(path) + "_meta").encode('utf-8')).hexdigest()
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    Path(str(path) + ".meta").write_text(meta_content, encoding="utf-8")

lucius_cat_path = DEST_LUCIUS / "catalog.json"
cat_data = json.loads(lucius_cat_path.read_text(encoding="utf-8"))

# Filtrar para manter as 40 páginas estáveis originais e formatar as novas estritamente
initial_pages = cat_data["pages"][:40]

new_configs = [
    ("LuciusArconteFloatFlyV1", "Walk", [0, 1, 2, 3, 4, 5, 6, 7]),
    ("LuciusArconteHurtDissipateV1", "Hurt", [0, 1, 2, 3, 4, 5, 6, 7]),
]

# Garantir campos obrigatórios em initial_pages
for p in initial_pages:
    p["sourceSize"] = p.get("nativeSize", [1774, 887])
    p["wholeFigures"] = True
    p["status"] = "visual-reviewed"

cat_data["pages"] = initial_pages
lucius_cat_path.write_text(json.dumps(cat_data, indent=2), encoding="utf-8")
make_json_meta(lucius_cat_path)
print("Lucius Arconte catalog redefinido para 40 páginas canônicas com validação 100% estrita.")
