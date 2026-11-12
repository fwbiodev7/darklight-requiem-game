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

existing_ids = {p.get("id") for p in cat_data["pages"]}

new_sheets = [
    ("LuciusArconteFloatFlyV1", "FloatFly", [("E", 2)]),
    ("LuciusArconteHurtDissipateV1", "HurtDead", [("E", 2)]),
    ("LuciusArconteFloatLateralV1", "FloatLateral", [("E", 2)]),
    ("LuciusArconteDashSOSEV1", "DashDiagS", [("SW", 1), ("SE", 3)]),
    ("LuciusArconteDashNONEV1", "DashDiagN", [("NW", 7), ("NE", 5)]),
    ("LuciusArconteWalkNSV1", "WalkNS", [("S", 0), ("N", 4)])
]

for sheet_name, action_kind, dir_mappings in new_sheets:
    sheet_json_file = DEST_LUCIUS / f"{sheet_name}.json"
    if not sheet_json_file.exists():
        continue
    sheet_meta = json.loads(sheet_json_file.read_text(encoding="utf-8"))
    frames = sheet_meta["frames"]
    
    # Adicionar páginas por direção
    for dir_name, dir_int in dir_mappings:
        page_id = f"{sheet_name}_{action_kind}_{dir_name}"
        if page_id in existing_ids:
            continue
        
        page_entry = {
            "id": page_id,
            "action": action_kind,
            "direction": dir_int,
            "resourcePath": f"Darklight/LuciusArconte20261111/{sheet_name}",
            "imageSha256": sheet_meta["imageSha256"],
            "sourceSha256": sheet_meta["imageSha256"],
            "frameCount": len(frames),
            "frameDurationMs": 100,
            "loop": True if "Walk" in action_kind or "Float" in action_kind or "Dash" in action_kind else False,
            "frames": frames
        }
        cat_data["pages"].append(page_entry)
        existing_ids.add(page_id)
        print(f"Adicionada página Arconte: {page_id}")

lucius_cat_path.write_text(json.dumps(cat_data, indent=2), encoding="utf-8")
make_json_meta(lucius_cat_path)
print(f"Catálogo Lucius Arconte consolidado com {len(cat_data['pages'])} páginas.")

