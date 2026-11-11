"""
DARKLIGHT: REQUIEM — Integração das Artes de Lucius Normal (11/11/2026)
Gera os arquivos PNG, JSON, metadados e atualiza o catalog.json.
"""
import os, sys, json, hashlib, shutil
from pathlib import Path
from PIL import Image

ROOT = Path(r"c:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main")
ACERVO = ROOT / "ArtSources/AcervoRecuperado20261111/02_Lucius_Normal/Animações_e_referencias"
DEST = ROOT / "Assets/Darklight/Resources/Darklight/LuciusNormal20261109"
CATALOG_PATH = DEST / "catalog.json"
SRC_NORMAL = ROOT / "ArtSources/LuciusNormal20261109/selected"
SRC_ATTACKS = ROOT / "ArtSources/LuciusAttacks20261110/selected"
SRC_NORMAL.mkdir(parents=True, exist_ok=True)
SRC_ATTACKS.mkdir(parents=True, exist_ok=True)
DEST.mkdir(parents=True, exist_ok=True)

PPU = 214.97090106361628
DIRS = ["S", "SW", "W", "NW", "N", "NE", "E", "SE"]

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def clean_bg(img):
    im = img.convert("RGBA")
    data = list(im.getdata())
    new_data = []
    for r, g, b, a in data:
        diff = max(abs(r - g), abs(r - b), abs(g - b))
        if r > 225 and g > 225 and b > 225 and diff < 12:
            new_data.append((r, g, b, 0))
        elif r > 210 and g > 210 and b > 210 and diff < 8:
            new_data.append((r, g, b, 0))
        else:
            new_data.append((r, g, b, a))
    im.putdata(new_data)
    return im

def export_sheet(sheet_name, src_file, is_native_alpha, rects, pivots, src_dest_folder):
    src_path = ACERVO / src_file
    im = Image.open(src_path)
    if not is_native_alpha:
        im = clean_bg(im)
    else:
        im = im.convert("RGBA")
        
    # Save source in ArtSources
    art_src = src_dest_folder / f"{sheet_name}-source.png"
    im.save(art_src)
    
    # Save in Resources
    res_png = DEST / f"{sheet_name}.png"
    im.save(res_png)
    
    frames = []
    for i, (r, piv) in enumerate(zip(rects, pivots)):
        fid = f"Redesign_{sheet_name}_{i:02d}"
        frames.append({
            "id": fid,
            "rect": list(r),
            "pivot": list(piv)
        })
        
    sheet_json = {
        "schemaVersion": 1,
        "image": f"{sheet_name}.png",
        "imageSha256": sha(res_png),
        "sourcePath": art_src.relative_to(ROOT).as_posix(),
        "sourceSha256": sha(art_src),
        "sourceSize": list(im.size),
        "nativeSize": list(im.size),
        "pixelsPerUnit": PPU,
        "wholeFigures": True,
        "status": "visual-reviewed",
        "frames": frames
    }
    
    res_json = DEST / f"{sheet_name}.json"
    res_json.write_text(json.dumps(sheet_json, indent=2) + "\n", encoding="utf-8")
    print(f"Exported {sheet_name}: {len(frames)} frames")
    return sheet_json, res_png, art_src

# 1. LuciusDeadV1 (026)
rects_dead = []
gw26, gh26 = 1774 // 4, 887 // 2
for r in range(2):
    for c in range(4):
        rw = 1774 - c * gw26 if c == 3 else gw26
        rh = 887 - r * gh26 if r == 1 else gh26
        rects_dead.append((c * gw26, r * gh26, rw, rh))
pivots_dead = [
    [0.4673, 0.0745], [0.4763, 0.0767], [0.4673, 0.0767], [0.4831, 0.0745],
    [0.4470, 0.1059], [0.4740, 0.1104], [0.4695, 0.1104], [0.4921, 0.1104]
]
dead_sheet, dead_png, dead_src = export_sheet(
    "LuciusDeadV1", "026_a_clean_spritesheet_image_on_a_transparent_checker.png",
    False, rects_dead, pivots_dead, SRC_NORMAL
)

# 2. LuciusSwordThrustV1 (073)
rects_thrust = []
gw73, gh73 = 1536 // 3, 1024 // 2
for r in range(2):
    for c in range(3):
        rects_thrust.append((c * gw73, r * gh73, gw73, gh73))
pivots_thrust = [
    [0.4941, 0.1211], [0.5273, 0.1211], [0.4570, 0.1230],
    [0.5469, 0.2148], [0.5000, 0.2109], [0.4258, 0.2148]
]
thrust_sheet, thrust_png, thrust_src = export_sheet(
    "LuciusSwordThrustV1", "073_sprite_de_lucius_combo_de_espada_em_seis_quadros.png",
    True, rects_thrust, pivots_thrust, SRC_ATTACKS
)

# 3. LuciusParryV1 (070)
rects_parry = []
gw70, gh70 = 1536 // 3, 1024 // 2
for r in range(2):
    for c in range(3):
        rects_parry.append((c * gw70, r * gh70, gw70, gh70))
pivots_parry = [
    [0.5137, 0.1309], [0.4688, 0.1270], [0.4844, 0.1309],
    [0.5254, 0.1445], [0.4785, 0.1484], [0.4785, 0.1445]
]
parry_sheet, parry_png, parry_src = export_sheet(
    "LuciusParryV1", "070_lucius_em_guarda_de_batalha_geschniegelt.png",
    True, rects_parry, pivots_parry, SRC_NORMAL
)

# 4. LuciusWalkArmed8V1 (007)
rects_armed = []
gw07, gh07 = 1774 // 4, 887 // 2
for r in range(2):
    for c in range(4):
        rw = 1774 - c * gw07 if c == 3 else gw07
        rh = 887 - r * gh07 if r == 1 else gh07
        rects_armed.append((c * gw07, r * gh07, rw, rh))
pivots_armed = [
    [0.5011, 0.0632], [0.4989, 0.0632], [0.4989, 0.0700], [0.4584, 0.0858],
    [0.5327, 0.0631], [0.4673, 0.0653], [0.4560, 0.0631], [0.4674, 0.0631]
]
armed_sheet, armed_png, armed_src = export_sheet(
    "LuciusWalkArmed8V1", "007_Spritesheet de Guarda com Espada de Lucius(1).png",
    False, rects_armed, pivots_armed, SRC_NORMAL
)

# 5. LuciusHurtV1 (028)
rects_hurt = [(c * 384, 0, 384, 341) for c in range(4)]
pivots_hurt = [[0.4948, 0.0], [0.4896, 0.0], [0.4714, 0.0], [0.4245, 0.0]]
hurt_sheet, hurt_png, hurt_src = export_sheet(
    "LuciusHurtV1", "028_a_clean_transparent_background_pixel_art_spritesh.png",
    False, rects_hurt, pivots_hurt, SRC_NORMAL
)

# Update catalog.json
cat = json.loads(CATALOG_PATH.read_text(encoding="utf-8"))
existing_pages = {pg["id"]: pg for pg in cat["pages"]}

# Add Dead pages (0..7)
for d in range(8):
    use_row1 = d in (3, 4, 5, 6)
    sprite_indices = [4, 5, 6, 7] if use_row1 else [0, 1, 2, 3]
    pid = f"LuciusDeadV1_Dead_{DIRS[d]}"
    page = {
        "id": pid,
        "action": "Dead",
        "direction": d,
        "resourcePath": "Darklight/LuciusNormal20261109/LuciusDeadV1",
        "imageSha256": sha(dead_png),
        "sourceSha256": sha(dead_src),
        "sourcePath": dead_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1774, 887],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusDeadV1_{i:02d}" for i in sprite_indices],
        "durationMs": [120, 120, 140, 200],
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    }
    existing_pages[pid] = page

# Add SwordThrust pages (0..7)
for d in range(8):
    pid = f"LuciusSwordThrustV1_Attack_{DIRS[d]}"
    page = {
        "id": pid,
        "action": "Attack",
        "attackKey": "SwordThrust",
        "direction": d,
        "resourcePath": "Darklight/LuciusNormal20261109/LuciusSwordThrustV1",
        "imageSha256": sha(thrust_png),
        "sourceSha256": sha(thrust_src),
        "sourcePath": thrust_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1536, 1024],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusSwordThrustV1_{i:02d}" for i in range(6)],
        "durationMs": [100, 100, 90, 90, 120, 140],
        "startupFrames": 2,
        "activeFrames": 2,
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    }
    existing_pages[pid] = page

# Add Parry pages (0..7)
for d in range(8):
    use_back = d in (3, 4, 5, 6)
    sprite_indices = [3, 4, 5] if use_back else [0, 1, 2]
    pid = f"LuciusParryV1_Parry_{DIRS[d]}"
    page = {
        "id": pid,
        "action": "Parry",
        "direction": d,
        "resourcePath": "Darklight/LuciusNormal20261109/LuciusParryV1",
        "imageSha256": sha(parry_png),
        "sourceSha256": sha(parry_src),
        "sourcePath": parry_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1536, 1024],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusParryV1_{i:02d}" for i in sprite_indices],
        "durationMs": [80, 120, 150],
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    }
    existing_pages[pid] = page

# Add WalkArmed pages (0..7)
for d in range(8):
    pid = f"LuciusWalkArmed8V1_WalkArmed_{DIRS[d]}"
    page = {
        "id": pid,
        "action": "WalkArmed",
        "direction": d,
        "resourcePath": "Darklight/LuciusNormal20261109/LuciusWalkArmed8V1",
        "imageSha256": sha(armed_png),
        "sourceSha256": sha(armed_src),
        "sourcePath": armed_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1774, 887],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusWalkArmed8V1_{d:02d}"],
        "durationMs": [350],
        "loop": True,
        "wholeFigures": True,
        "status": "visual-reviewed"
    }
    existing_pages[pid] = page

# Add Hurt pages (0..7)
for d in range(8):
    pid = f"LuciusHurtV1_Hurt_{DIRS[d]}"
    page = {
        "id": pid,
        "action": "Hurt",
        "direction": d,
        "resourcePath": "Darklight/LuciusNormal20261109/LuciusHurtV1",
        "imageSha256": sha(hurt_png),
        "sourceSha256": sha(hurt_src),
        "sourcePath": hurt_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1536, 1024],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusHurtV1_{i:02d}" for i in range(4)],
        "durationMs": [90, 100, 120, 150],
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    }
    existing_pages[pid] = page

cat["pages"] = list(existing_pages.values())
CATALOG_PATH.write_text(json.dumps(cat, indent=2) + "\n", encoding="utf-8")
print(f"Catalog updated! Total pages now: {len(cat['pages'])}")
