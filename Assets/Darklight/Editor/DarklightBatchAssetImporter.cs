using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightBatchAssetImporter
    {
        public static void ImportAllFormsAndArconte()
        {
            Debug.Log("Iniciando fatiamento de Fabius Forms...");
            DarklightFabiusFormsImporter.ImportAll();

            Debug.Log("Iniciando fatiamento de Lucius Arconte complementar...");
            string arconteFolder = "Assets/Darklight/Resources/Darklight/LuciusArconte20261111/";
            string[] arconteSheets = new[]
            {
                "LuciusArconteFloatFlyV1",
                "LuciusArconteHurtDissipateV1",
                "LuciusArconteFloatLateralV1",
                "LuciusArconteDashSOSEV1",
                "LuciusArconteDashNONEV1",
                "LuciusArconteWalkNSV1"
            };

            foreach (var s in arconteSheets)
            {
                string pngPath = arconteFolder + s + ".png";
                string jsonPath = arconteFolder + s + ".json";
                if (!File.Exists(pngPath) || !File.Exists(jsonPath)) continue;

                var sheetMeta = JsonUtility.FromJson<DarklightFabiusFormsImporter.SheetMeta>(File.ReadAllText(jsonPath));
                if (sheetMeta?.frames == null) continue;

                var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = 214.9709f;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 8192;
                importer.alphaIsTransparency = true;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();

                var factories = new SpriteDataProviderFactories();
                factories.Init();
                var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
                if (provider != null)
                {
                    provider.InitSpriteEditorDataProvider();
                    var identity = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();

                    var rects = sheetMeta.frames.Select(f => new SpriteRect
                    {
                        name = f.id,
                        rect = new Rect(f.rect[0], f.rect[1], f.rect[2], f.rect[3]),
                        pivot = new Vector2(f.pivot[0], f.pivot[1]),
                        alignment = SpriteAlignment.Custom,
                        spriteID = GUID.Generate()
                    }).ToArray();

                    provider.SetSpriteRects(rects);
                    identity?.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
                    provider.Apply();
                    importer.SaveAndReimport();
                    Debug.Log($"Sliced Lucius Arconte sheet: {s} with {rects.Length} frames.");
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("BATCH_IMPORT_COMPLETED_SUCCESSFULLY");
        }
    }
}

