"""
DARKLIGHT: REQUIEM — Integração de Lucius: Arconte do Abismo (11/11/2026)
Gera os sprites, metadados e catalog.json para Lucius Arconte do Abismo.
"""
import os, sys, json, hashlib, shutil
from pathlib import Path
from PIL import Image

ROOT = Path(r"c:\Users\Usuario\Documents\Codex\2026-10-10\f\darklight-requiem-main")
ACERVO = ROOT / "ArtSources/AcervoRecuperado20261111/03_Lucius_Arconte_do_Abismo"
DEST = ROOT / "Assets/Darklight/Resources/Darklight/LuciusArconte20261111"
SRC_ARCONTE = ROOT / "ArtSources/LuciusArconte20261111/selected"
SRC_ARCONTE.mkdir(parents=True, exist_ok=True)
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

def export_sheet(sheet_name, src_file, is_native_alpha, rects, pivots):
    src_path = ACERVO / src_file
    im = Image.open(src_path)
    if not is_native_alpha:
        im = clean_bg(im)
    else:
        im = im.convert("RGBA")
        
    art_src = SRC_ARCONTE / f"{sheet_name}-source.png"
    im.save(art_src)
    
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

# 1. LuciusArconteAwakeningV1 (060 - 8 fases de despertar do Nexo Fenrir)
# 1774 x 887, 2 rows x 4 cols
rects_awake = []
gw_a, gh_a = 1774 // 4, 887 // 2
for r in range(2):
    for c in range(4):
        rw = 1774 - c * gw_a if c == 3 else gw_a
        rh = 887 - r * gh_a if r == 1 else gh_a
        rects_awake.append((c * gw_a, r * gh_a, rw, rh))
pivots_awake = [[0.5, 0.08] for _ in range(8)]
awake_sheet, awake_png, awake_src = export_sheet(
    "LuciusArconteAwakeningV1", "060_despertar_do_nexus_fenrir_em_oito_fases.png",
    True, rects_awake, pivots_awake
)

# 2. LuciusArconteIdle8V1 (038 - 8 views de Arconte)
# 1774 x 887, 2 rows x 4 cols
rects_idle = []
for r in range(2):
    for c in range(4):
        rw = 1774 - c * gw_a if c == 3 else gw_a
        rh = 887 - r * gh_a if r == 1 else gh_a
        rects_idle.append((c * gw_a, r * gh_a, rw, rh))
pivots_idle = [[0.5, 0.08] for _ in range(8)]
idle_sheet, idle_png, idle_src = export_sheet(
    "LuciusArconteIdle8V1", "038_a_high_detail_pixel_art_sprite_sheet_on_a_transpar.png",
    False, rects_idle, pivots_idle
)

# 3. LuciusArconteAttacksV1 (076 - lâminas espectrais)
# 1448 x 1086, 2 rows x 3 cols = 6 frames
gw_atk, gh_atk = 1448 // 3, 1086 // 2
rects_atk = []
for r in range(2):
    for c in range(3):
        rw = 1448 - c * gw_atk if c == 2 else gw_atk
        rh = 1086 - r * gh_atk if r == 1 else gh_atk
        rects_atk.append((c * gw_atk, r * gh_atk, rw, rh))
pivots_atk = [[0.5, 0.12] for _ in range(6)]
atk_sheet, atk_png, atk_src = export_sheet(
    "LuciusArconteAttacksV1", "076_spritesheet_de_lucius_lâminas_espectrais.png",
    True, rects_atk, pivots_atk
)

# 4. LuciusArconteUltimateV1 (000 - despertar sombrio / ultimate)
# 1254 x 1254, 2 rows x 2 cols = 4 frames
gw_u, gh_u = 1254 // 2, 1254 // 2
rects_u = []
for r in range(2):
    for c in range(2):
        rw = 1254 - c * gw_u if c == 1 else gw_u
        rh = 1254 - r * gh_u if r == 1 else gh_u
        rects_u.append((c * gw_u, r * gh_u, rw, rh))
pivots_u = [[0.5, 0.10] for _ in range(4)]
u_sheet, u_png, u_src = export_sheet(
    "LuciusArconteUltimateV1", "000_Despertar Sombrio_ Sequência de Ataques.png",
    False, rects_u, pivots_u
)

# Create catalog.json for Lucius Arconte
pages = []

# 8 Idle pages
for d in range(8):
    pages.append({
        "id": f"LuciusArconteIdle8V1_Idle_{DIRS[d]}",
        "action": "Idle",
        "direction": d,
        "resourcePath": "Darklight/LuciusArconte20261111/LuciusArconteIdle8V1",
        "imageSha256": sha(idle_png),
        "sourceSha256": sha(idle_src),
        "sourcePath": idle_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1774, 887],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusArconteIdle8V1_{d:02d}"],
        "durationMs": [350],
        "loop": True,
        "wholeFigures": True,
        "status": "visual-reviewed"
    })

# 8 Transformation pages (Awakening / Despertar do Nexo Fenrir)
for d in range(8):
    pages.append({
        "id": f"LuciusArconteAwakeningV1_Transformation_{DIRS[d]}",
        "action": "Transformation",
        "direction": d,
        "resourcePath": "Darklight/LuciusArconte20261111/LuciusArconteAwakeningV1",
        "imageSha256": sha(awake_png),
        "sourceSha256": sha(awake_src),
        "sourcePath": awake_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1774, 887],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusArconteAwakeningV1_{i:02d}" for i in range(8)],
        "durationMs": [100, 100, 100, 120, 120, 140, 150, 180],
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    })

# 8 Attack pages for SwordHorizontal (Spectral Blade Sweep)
for d in range(8):
    pages.append({
        "id": f"LuciusArconteAttacksV1_SwordHorizontal_{DIRS[d]}",
        "action": "Attack",
        "attackKey": "SwordHorizontal",
        "direction": d,
        "resourcePath": "Darklight/LuciusArconte20261111/LuciusArconteAttacksV1",
        "imageSha256": sha(atk_png),
        "sourceSha256": sha(atk_src),
        "sourcePath": atk_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1448, 1086],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusArconteAttacksV1_{i:02d}" for i in [0, 1, 2]],
        "durationMs": [110, 100, 180],
        "startupFrames": 1,
        "activeFrames": 1,
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    })

# 8 Attack pages for SwordVertical (Spectral Blade Downward)
for d in range(8):
    pages.append({
        "id": f"LuciusArconteAttacksV1_SwordVertical_{DIRS[d]}",
        "action": "Attack",
        "attackKey": "SwordVertical",
        "direction": d,
        "resourcePath": "Darklight/LuciusArconte20261111/LuciusArconteAttacksV1",
        "imageSha256": sha(atk_png),
        "sourceSha256": sha(atk_src),
        "sourcePath": atk_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1448, 1086],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusArconteAttacksV1_{i:02d}" for i in [3, 4, 5]],
        "durationMs": [120, 110, 190],
        "startupFrames": 1,
        "activeFrames": 1,
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    })

# 8 Attack pages for SwordThrust (Abyss Burst Ultimate)
for d in range(8):
    pages.append({
        "id": f"LuciusArconteUltimateV1_SwordThrust_{DIRS[d]}",
        "action": "Attack",
        "attackKey": "SwordThrust",
        "direction": d,
        "resourcePath": "Darklight/LuciusArconte20261111/LuciusArconteUltimateV1",
        "imageSha256": sha(u_png),
        "sourceSha256": sha(u_src),
        "sourcePath": u_src.relative_to(ROOT).as_posix(),
        "nativeSize": [1254, 1254],
        "pixelsPerUnit": PPU,
        "spriteNames": [f"Redesign_LuciusArconteUltimateV1_{i:02d}" for i in range(4)],
        "durationMs": [120, 100, 100, 220],
        "startupFrames": 1,
        "activeFrames": 2,
        "loop": False,
        "wholeFigures": True,
        "status": "visual-reviewed"
    })

cat_arconte = {
    "schemaVersion": 1,
    "characterId": "Lucius",
    "form": "Normal",
    "weapon": "Sword",
    "appearance": "Arconte",
    "complete": True,
    "pages": pages
}

(DEST / "catalog.json").write_text(json.dumps(cat_arconte, indent=2) + "\n", encoding="utf-8")
print(f"Catalog for Lucius Arconte generated: {len(pages)} pages!")

