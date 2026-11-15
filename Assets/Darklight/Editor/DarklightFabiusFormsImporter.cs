using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightFabiusFormsImporter
    {
        [Serializable]
        public sealed class FrameMeta
        {
            public string id;
            public int[] rect;
            public float[] pivot;
        }

        [Serializable]
        public sealed class SheetMeta
        {
            public string name;
            public string image;
            public string imageSha256;
            public int[] nativeSize;
            public int[] frameSize;
            public float pixelsPerUnit;
            public int rows;
            public int cols;
            public string form;
            public string action;
            public FrameMeta[] frames;
        }

        [Serializable]
        public sealed class CatalogMeta
        {
            public SheetMeta[] pages;
        }

        [Serializable] sealed class WalkRepairPage { public string name; }
        [Serializable] sealed class WalkRepairMap { public WalkRepairPage[] pages; }

        public static void ImportAll()
        {
            ImportMatching(null);
        }

        public static void ImportSession14()
        {
            ImportMatching("Session14");
            ImportMatching("FabiusSerafimKatanaWalk");
            ImportMatching("FabiusSerafimSawsWalk");
        }

        public static void ImportWalkRepair()
        {
            DarklightAutomation.Silent = true;
            SessionState.SetBool("Darklight.SilentChecks", true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var map = JsonUtility.FromJson<WalkRepairMap>(File.ReadAllText("ArtSources/WalkRepair20261114/integration-map.json"));
            if (map?.pages == null || map.pages.Length == 0)
                throw new InvalidOperationException("Reviewed walk repair map is empty.");
            foreach (var page in map.pages)
                ImportMatching(page.name, true);
        }

        static void ImportMatching(string namePrefix, bool exactMatch = false)
        {
            string folder = "Assets/Darklight/Resources/Darklight/FabiusForms20261111/";
            string catFile = folder + "catalog.json";
            if (!File.Exists(catFile))
            {
                Debug.LogError("Catalog not found: " + catFile);
                return;
            }

            var cat = JsonUtility.FromJson<CatalogMeta>(File.ReadAllText(catFile));
            if (cat?.pages == null)
            {
                Debug.LogError("Failed to parse catalog: " + catFile);
                return;
            }

            foreach (var page in cat.pages)
            {
                if (namePrefix != null && (exactMatch ? page.name != namePrefix : !page.name.StartsWith(namePrefix, StringComparison.Ordinal))) continue;
                string pngPath = folder + page.image;
                if (!File.Exists(pngPath)) continue;

                var importer = AssetImporter.GetAtPath(pngPath) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit = page.pixelsPerUnit > 0 ? page.pixelsPerUnit : 214.9709f;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 8192;
                importer.alphaIsTransparency = true;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                var defaults = importer.GetDefaultPlatformTextureSettings();
                defaults.textureCompression = TextureImporterCompression.Uncompressed;
                defaults.format = TextureImporterFormat.RGBA32;
                defaults.maxTextureSize = 8192;
                importer.SetPlatformTextureSettings(defaults);

                importer.SaveAndReimport();

                var factories = new SpriteDataProviderFactories();
                factories.Init();
                var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
                if (provider != null)
                {
                    provider.InitSpriteEditorDataProvider();
                    var identity = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
                    if (identity == null)
                        throw new InvalidOperationException("Missing sprite name/fileID provider: " + pngPath);

                    var existingIds = provider.GetSpriteRects()
                        .ToDictionary(rect => rect.name, rect => rect.spriteID, StringComparer.Ordinal);

                    var rects = page.frames.Select(f => new SpriteRect
                    {
                        name = f.id,
                        rect = new Rect(f.rect[0], f.rect[1], f.rect[2], f.rect[3]),
                        pivot = new Vector2(f.pivot[0], f.pivot[1]),
                        alignment = SpriteAlignment.Custom,
                        spriteID = existingIds.TryGetValue(f.id, out var existingId) ? existingId : GUID.Generate()
                    }).ToArray();

                    provider.SetSpriteRects(rects);
                    identity.SetNameFileIdPairs(rects.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)));
                    provider.Apply();
                    importer.SaveAndReimport();
                    Debug.Log($"Sliced: {page.name} with {rects.Length} frames.");
                }
            }

            AssetDatabase.SaveAssets();
            DarklightFabiusFormsBank.ClearCache();
            Debug.Log("DarklightFabiusFormsImporter: selected sheets sliced successfully.");
        }
    }
}

