import os, sys, json, hashlib
from pathlib import Path
from PIL import Image

REPO_ROOT = Path("c:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main")
FABIUS_DIR = REPO_ROOT / "ArtSources" / "FabiusForms20261109"
COMP_DIR = REPO_ROOT / "ArtSources" / "NovasArtesComplementares20261111"
DEST_FABIUS = REPO_ROOT / "Assets" / "Darklight" / "Resources" / "Darklight" / "FabiusForms20261111"
DEST_LUCIUS = REPO_ROOT / "Assets" / "Darklight" / "Resources" / "Darklight" / "LuciusArconte20261111"

DEST_FABIUS.mkdir(parents=True, exist_ok=True)
DEST_LUCIUS.mkdir(parents=True, exist_ok=True)

def remove_background(img, bg_color=(255, 255, 255), tolerance=28):
    img = img.convert("RGBA")
    datas = img.getdata()
    new_data = []
    for item in datas:
        r, g, b, a = item
        dr = abs(r - bg_color[0])
        dg = abs(g - bg_color[1])
        db = abs(b - bg_color[2])
        if dr <= tolerance and dg <= tolerance and db <= tolerance:
            new_data.append((r, g, b, 0))
        elif dr <= tolerance + 25 and dg <= tolerance + 25 and db <= tolerance + 25:
            factor = max(dr, dg, db) - tolerance
            alpha = int((factor / 25.0) * 255)
            new_data.append((r, g, b, min(a, alpha)))
        else:
            new_data.append(item)
    img.putdata(new_data)
    return img

def sha256_file(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        while chunk := f.read(65536):
            h.update(chunk)
    return h.hexdigest()

def make_meta(path, guid=None):
    if guid is None:
        guid = hashlib.md5(str(path).encode('utf-8')).hexdigest()
    meta_content = f"""fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 1
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 4096
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 2
  spriteExtrude: 1
  spriteMeshType: 1
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.15}}
  spritePixelsToUnits: 214.9709
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbook: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 4096
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
"""
    meta_path = Path(str(path) + ".meta")
    meta_path.write_text(meta_content, encoding="utf-8")

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

def process_grid_sheet(src_path, dst_base_name, rows, cols, dst_dir, form_name, action_name, pivot=(0.5, 0.15)):
    img = Image.open(src_path)
    clean_img = remove_background(img)
    w, h = clean_img.size
    fw, fh = w // cols, h // rows
    out_png = dst_dir / f"{dst_base_name}.png"
    clean_img.save(out_png, "PNG")
    make_meta(out_png)
    
    frames = []
    idx = 0
    for r in range(rows):
        for c in range(cols):
            x = c * fw
            # Unity coordinates: Y=0 is bottom
            y_unity = (rows - 1 - r) * fh
            frame_id = f"{dst_base_name}_{idx:02d}"
            frames.append({
                "id": frame_id,
                "rect": [x, y_unity, fw, fh],
                "pivot": [pivot[0], pivot[1]],
                "durationMs": 100,
                "action": action_name,
                "direction": "E",
                "form": form_name,
                "frameIndex": idx
            })
            idx += 1
            
    catalog_entry = {
        "name": dst_base_name,
        "image": f"{dst_base_name}.png",
        "imageSha256": sha256_file(out_png),
        "nativeSize": [w, h],
        "frameSize": [fw, fh],
        "pixelsPerUnit": 214.9709,
        "rows": rows,
        "cols": cols,
        "form": form_name,
        "action": action_name,
        "frames": frames
    }
    
    out_json = dst_dir / f"{dst_base_name}.json"
    out_json.write_text(json.dumps(catalog_entry, indent=2), encoding="utf-8")
    make_json_meta(out_json)
    return catalog_entry

print("Iniciando fatiamento de Fabius Forms (10 folhas)...")
fabius_pages = []

fabius_configs = [
    # Ruptura
    ("01_Fabius_Ruptura_01_Katana_3x3_9quadros.png", "FabiusRupturaKatanaV1", 3, 3, "Ruptura", "KatanaAttack", (0.5, 0.15)),
    ("01_Fabius_Ruptura_02_Ativacao_Esquiva_2x4_8quadros.png", "FabiusRupturaActivationDodgeV1", 2, 4, "Ruptura", "ActivationDodge", (0.5, 0.15)),
    ("01_Fabius_Ruptura_03_Punhos_3x3_9quadros.png", "FabiusRupturaPunhosV1", 3, 3, "Ruptura", "FistAttack", (0.5, 0.15)),
    ("01_Fabius_Ruptura_04_Moto_2x4_8quadros.png", "FabiusRupturaMotoV1", 2, 4, "Ruptura", "MotorcycleRide", (0.5, 0.25)),
    ("01_Fabius_Ruptura_05_Moto_para_Serras_3x4_12quadros.png", "FabiusRupturaMotoSerrasV1", 3, 4, "Ruptura", "MotoToSaws", (0.5, 0.15)),
    # Serafim Carmesim
    ("02_Fabius_Serafim_Carmesim_01_Katana_3x3_9quadros.png", "FabiusSerafimKatanaV1", 3, 3, "Serafim", "KatanaAttack", (0.5, 0.15)),
    ("02_Fabius_Serafim_Carmesim_02_Transformacao_Ultimate_2x4_8quadros.png", "FabiusSerafimTransformationUltimateV1", 2, 4, "Serafim", "TransformationUltimate", (0.5, 0.15)),
    ("02_Fabius_Serafim_Carmesim_03_Punhos_3x3_9quadros.png", "FabiusSerafimPunhosV1", 3, 3, "Serafim", "FistAttack", (0.5, 0.15)),
    ("02_Fabius_Serafim_Carmesim_04_Moto_2x4_8quadros.png", "FabiusSerafimMotoV1", 2, 4, "Serafim", "MotorcycleRide", (0.5, 0.25)),
    ("02_Fabius_Serafim_Carmesim_05_Moto_para_Serras_3x4_12quadros.png", "FabiusSerafimMotoSerrasV1", 3, 4, "Serafim", "MotoToSaws", (0.5, 0.15)),
]

for src_name, dst_name, r, c, form, act, piv in fabius_configs:
    src_file = FABIUS_DIR / src_name
    if src_file.exists():
        entry = process_grid_sheet(src_file, dst_name, r, c, DEST_FABIUS, form, act, piv)
        fabius_pages.append(entry)
        print(f" -> Processado: {dst_name} ({r*c} quadros)")

print(f"Total de páginas Fabius 10 folhas: {len(fabius_pages)}")

# Processar as novas folhas complementares de Fabius e Lucius
print("Processando artes complementares direcionais de Fabius e Lucius...")

comp_configs = [
    # Fabius Ruptura complementares
    ("01_Fabius_Ruptura_Walk.jpg", "FabiusRupturaWalkV1", 1, 6, DEST_FABIUS, "Ruptura", "Walk", (0.5, 0.15)),
    ("02_Fabius_Ruptura_Hurt_Knockdown.jpg", "FabiusRupturaHurtKnockdownV1", 2, 4, DEST_FABIUS, "Ruptura", "HurtDead", (0.5, 0.15)),
    ("07_Fabius_Ruptura_Walk_Norte.jpg", "FabiusRupturaWalkNV1", 1, 6, DEST_FABIUS, "Ruptura", "WalkN", (0.5, 0.15)),
    ("08_Fabius_Ruptura_Walk_Oeste.jpg", "FabiusRupturaWalkWV1", 1, 6, DEST_FABIUS, "Ruptura", "WalkW", (0.5, 0.15)),
    ("09_Fabius_Ruptura_Walk_Sul.jpg", "FabiusRupturaWalkSV1", 1, 6, DEST_FABIUS, "Ruptura", "WalkS", (0.5, 0.15)),
    ("10_Fabius_Ruptura_Walk_NO_NE.jpg", "FabiusRupturaWalkNONEV1", 2, 6, DEST_FABIUS, "Ruptura", "WalkDiagN", (0.5, 0.15)),
    ("11_Fabius_Ruptura_Walk_SO_SE.jpg", "FabiusRupturaWalkSOSEV1", 2, 6, DEST_FABIUS, "Ruptura", "WalkDiagS", (0.5, 0.15)),

    # Fabius Serafim complementares
    ("03_Fabius_Serafim_Hurt_Knockdown.jpg", "FabiusSerafimHurtKnockdownV1", 2, 4, DEST_FABIUS, "Serafim", "HurtDead", (0.5, 0.15)),
    ("04_Fabius_Serafim_Fly_Glide.jpg", "FabiusSerafimFlyGlideV1", 1, 6, DEST_FABIUS, "Serafim", "FlyGlide", (0.5, 0.25)),
    ("12_Fabius_Serafim_Walk_Norte_Sul.jpg", "FabiusSerafimWalkNSV1", 2, 6, DEST_FABIUS, "Serafim", "WalkNS", (0.5, 0.15)),
    ("13_Fabius_Serafim_Fly_8D.jpg", "FabiusSerafimFly8DV1", 4, 6, DEST_FABIUS, "Serafim", "Fly8D", (0.5, 0.25)),

    # Lucius Arconte complementares
    ("05_Lucius_Arconte_Float_Fly.jpg", "LuciusArconteFloatFlyV1", 1, 6, DEST_LUCIUS, "Arconte", "FloatFly", (0.5, 0.25)),
    ("06_Lucius_Arconte_Hurt_Dissipate.jpg", "LuciusArconteHurtDissipateV1", 2, 4, DEST_LUCIUS, "Arconte", "HurtDead", (0.5, 0.15)),
    ("14_Lucius_Arconte_Float_Lateral.jpg", "LuciusArconteFloatLateralV1", 1, 6, DEST_LUCIUS, "Arconte", "FloatLateral", (0.5, 0.25)),
    ("15_Lucius_Arconte_Dash_SO_SE.jpg", "LuciusArconteDashSOSEV1", 2, 6, DEST_LUCIUS, "Arconte", "DashDiagS", (0.5, 0.25)),
    ("16_Lucius_Arconte_Dash_NO_NE.jpg", "LuciusArconteDashNONEV1", 2, 6, DEST_LUCIUS, "Arconte", "DashDiagN", (0.5, 0.25)),
    ("17_Lucius_Arconte_Walk_Norte_Sul.png", "LuciusArconteWalkNSV1", 4, 6, DEST_LUCIUS, "Arconte", "WalkNS", (0.5, 0.15)),
]

for src_name, dst_name, r, c, target_dir, form, act, piv in comp_configs:
    src_file = COMP_DIR / src_name
    if src_file.exists():
        entry = process_grid_sheet(src_file, dst_name, r, c, target_dir, form, act, piv)
        if target_dir == DEST_FABIUS:
            fabius_pages.append(entry)
        print(f" -> Complementar: {dst_name} ({r*c} quadros)")

# Salvar catálogo consolidado de Fabius Forms
catalog_fabius_path = DEST_FABIUS / "catalog.json"
catalog_fabius_path.write_text(json.dumps({"pages": fabius_pages}, indent=2), encoding="utf-8")
make_json_meta(catalog_fabius_path)
print(f"Catálogo de Fabius concluído com {len(fabius_pages)} páginas.")

# Atualizar catálogo de Lucius Arconte
lucius_cat_path = DEST_LUCIUS / "catalog.json"
if lucius_cat_path.exists():
    try:
        lucius_cat = json.loads(lucius_cat_path.read_text(encoding="utf-8"))
    except:
        lucius_cat = {"pages": []}
else:
    lucius_cat = {"pages": []}

for f in DEST_LUCIUS.glob("*.json"):
    if f.name == "catalog.json":
        continue
    entry = json.loads(f.read_text(encoding="utf-8"))
    if not any(p["name"] == entry["name"] for p in lucius_cat["pages"]):
        lucius_cat["pages"].append(entry)

lucius_cat_path.write_text(json.dumps(lucius_cat, indent=2), encoding="utf-8")
make_json_meta(lucius_cat_path)
print(f"Catálogo de Lucius Arconte atualizado com {len(lucius_cat['pages'])} páginas.")

print("PROCESSO LOCAL DE FATIAMENTO E METADADOS CONCLUÍDO COM SUCESSO!")
