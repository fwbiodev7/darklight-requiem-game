#!/usr/bin/env python3
"""
auto_slice_and_bind.py - Automação Técnica de Slicing e Vinculação de Clips Unity.
Desenvolvido pela Antigravity para acelerar a integração das artes do Codex.

Funcionalidades:
1. Lê spritesheets PNG (ex: folhas de 8 a 12 figuras inteiras da Skill Pixel Art Master).
2. Calcula automaticamente as células da grade ou aceita colunas/linhas explicitas.
3. Gera o arquivo .meta com spriteMode: 2 (Multiple), Pivot nos pés (BottomCenter 0.5, 0.0) e Point Filter.
4. Gera arquivo .anim de Unity AnimationClip com framerate configurável (padrão 12fps) e loop configurável.
"""

import os
import sys
import argparse
import struct
import uuid
import re

def get_image_dimensions(image_path):
    """Obtém largura e altura de um PNG sem dependência externa."""
    with open(image_path, 'rb') as f:
        data = f.read(24)
        if data[:8] != b'\x89PNG\r\n\x1a\n':
            raise ValueError(f"Arquivo não é um PNG válido: {image_path}")
        w, h = struct.unpack('>LL', data[16:24])
        return int(w), int(h)

def generate_meta_file(png_path, cols, rows, ppu=32, pivot_x=0.5, pivot_y=0.0):
    """Gera metadados .meta da Unity com fatiamento exato."""
    width, height = get_image_dimensions(png_path)
    cell_w = width // cols
    cell_h = height // rows
    
    meta_path = f"{png_path}.meta"
    guid = uuid.uuid4().hex
    
    # Se já existir meta, preservar o GUID original
    if os.path.exists(meta_path):
        with open(meta_path, 'r', encoding='utf-8') as f:
            content = f.read()
            m = re.search(r'guid:\s*([a-f0-9]{32})', content)
            if m:
                guid = m.group(1)

    base_name = os.path.splitext(os.path.basename(png_path))[0]
    
    sprites_yaml = []
    sprite_id = 0
    # Unity coordenadas Y começam do fundo (bottom-to-top)
    for r in range(rows):
        y = height - ((r + 1) * cell_h)
        for c in range(cols):
            x = c * cell_w
            internal_id = int(str(sprite_id + 1) + "00000")
            sub_name = f"{base_name}_{sprite_id}"
            sprite_entry = f"""      - serializedVersion: 2
        name: {sub_name}
        rect:
          serializedVersion: 2
          x: {x}
          y: {y}
          width: {cell_w}
          height: {cell_h}
        alignment: 7
        pivot: {{x: {pivot_x}, y: {pivot_y}}}
        border: {{x: 0, y: 0, z: 0, w: 0}}
        outline: []
        physicsShape: []
        tessellationDetail: 0
        bones: []
        spriteID: {uuid.uuid4().hex}
        internalID: {internal_id}
        vertices: []
        indices: 
        edges: []
        weights: []"""
            sprites_yaml.append(sprite_entry)
            sprite_id += 1

    sprites_block = "\n".join(sprites_yaml)

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
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vramSurrogate: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
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
  spritePivot: {{x: 0.5, y: 0}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaIsTransparency: 1
  spriteColliderType: 1
  tracks: []
  spriteSheet:
    serializedVersion: 2
    sprites:
{sprites_block}
    outline: []
    physicsShape: []
    bones: []
    spriteID: 
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
  spritePackingTag: 
  pSDRemoveMatte: 0
  pSDShowRemoveMatteOption: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""
    with open(meta_path, 'w', encoding='utf-8') as f:
        f.write(meta_content)
    
    print(f"[OK] Gerado meta fatiado: {meta_path} ({sprite_id} frames, {cell_w}x{cell_h}px, PPU: {ppu})")
    return guid, sprite_id

def generate_anim_clip(anim_path, png_guid, frame_count, fps=12, loop=True):
    """Gera um arquivo .anim (AnimationClip) compatível com Unity 6 / URP 2D."""
    interval = 1.0 / fps
    keyframes = []
    for i in range(frame_count):
        t = i * interval
        internal_id = int(str(i + 1) + "00000")
        kf = f"      - time: {t:.4f}\n        value: {{fileID: {internal_id}, guid: {png_guid}, type: 3}}"
        keyframes.append(kf)
    
    end_time = frame_count * interval
    kf_end = f"      - time: {end_time:.4f}\n        value: {{fileID: {int(str(frame_count) + '00000')}, guid: {png_guid}, type: 3}}"
    keyframes.append(kf_end)

    keyframes_block = "\n".join(keyframes)
    loop_val = 1 if loop else 0

    anim_content = f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {os.path.splitext(os.path.basename(anim_path))[0]}
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - serializedVersion: 2
    curve:
{keyframes_block}
    attribute: m_Sprite
    path: 
    classID: 212
    script: {{fileID: 0}}
    flags: 2
  m_SampleRate: {fps}
  m_WrapMode: 0
  m_Bounds:
    m_Center: {{x: 0, y: 0, z: 0}}
    m_Extent: {{x: 0, y: 0, z: 0}}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 0
      script: {{fileID: 0}}
      typeID: 212
      customType: 23
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping: []
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {{fileID: 0}}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: {end_time:.4f}
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: {loop_val}
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []
"""
    with open(anim_path, 'w', encoding='utf-8') as f:
        f.write(anim_content)
    
    anim_meta = f"{anim_path}.meta"
    if not os.path.exists(anim_meta):
        with open(anim_meta, 'w', encoding='utf-8') as f:
            f.write(f"fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\nNativeFormatImporter:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n")

    print(f"[OK] Gerado AnimationClip: {anim_path} ({frame_count} frames a {fps} FPS, Loop: {loop})")

def main():
    parser = argparse.ArgumentParser(description="Auto Slice & Bind Tool para Spritesheets e AnimationClips")
    parser.add_argument("--image", required=True, help="Caminho para o spritesheet PNG")
    parser.add_argument("--cols", type=int, default=8, help="Número de colunas (padrão: 8)")
    parser.add_argument("--rows", type=int, default=1, help="Número de linhas (padrão: 1)")
    parser.add_argument("--ppu", type=int, default=32, help="Pixels Per Unit (padrão: 32)")
    parser.add_argument("--fps", type=int, default=12, help="Taxa de quadros do AnimationClip (padrão: 12)")
    parser.add_argument("--anim-out", help="Caminho opcional para exportar o arquivo .anim")
    parser.add_argument("--no-loop", action="store_true", help="Desativa o loop da animação (para ataques)")

    args = parser.parse_args()

    if not os.path.exists(args.image):
        print(f"[ERRO] Imagem não encontrada: {args.image}")
        sys.exit(1)

    guid, count = generate_meta_file(args.image, args.cols, args.rows, ppu=args.ppu)

    if args.anim_out:
        generate_anim_clip(args.anim_out, guid, count, fps=args.fps, loop=not args.no_loop)

if __name__ == "__main__":
    main()
