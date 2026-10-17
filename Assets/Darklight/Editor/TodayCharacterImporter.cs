using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class TodayCharacterImporter
    {
        [Serializable] public class Manifest { public Page[] pages; }
        [Serializable] public class Page
        {
            public string name, kind; public int skin, age, weapon, phases;
            public int[] directions; public float ppu; public Frame[] sprites;
            public bool ownerMask;
            public int startupFrameEnd, activeFrameEnd;
            public int idleBlinkFrame=-1;
        }
        [Serializable] public class Frame { public string name, previousName; public int x, y, w, h; public float pivotX, pivotY; }
        public static string Import()
        {
            const string folder = "Assets/Darklight/Resources/Darklight/TodayCharacterAnimations/";
            var data = JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/Darklight/ArtSources/WholeCharacters20261013/manifest.json"));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            if (data?.pages == null) throw new InvalidOperationException("Missing whole-character manifest pages");
            var coverage = new System.Collections.Generic.HashSet<string>();
            var pages = data.pages.Select(page =>
            {
                var descriptor = new TodayCharacterPage { name = page.name, kind = page.kind, skin = page.skin, age = page.age,
                    weapon = page.weapon, phases = page.phases, directions = page.directions,
                    resourcePath = "Darklight/TodayCharacterAnimations/" + page.name,
                    paletteResourcePath = "Darklight/TodayCharacterAnimations/" + page.name + "_Palette",
                    spriteNames = page.sprites?.Select(f => f.name).ToArray(), ownerMask = page.ownerMask,
                    startupFrameEnd=page.startupFrameEnd, activeFrameEnd=page.activeFrameEnd,idleBlinkFrame=page.idleBlinkFrame };
                if (!TodayCharacterBank.ValidDescriptor(descriptor)) throw new InvalidOperationException("Invalid ordered whole-character descriptor: " + page.name);
                foreach (int direction in page.directions)
                    if (!coverage.Add($"{page.skin}:{page.age}:{page.weapon}:{direction}:{page.kind}"))
                        throw new InvalidOperationException("Overlapping whole-character page coverage: " + page.name);
                var path = folder + page.name + ".png";
                var maskPath = folder + page.name + "_Palette.png";
                ImportTextureFrames(path,page.ppu,page.sprites,maskPath,page.ownerMask);
                return descriptor;
            }).ToArray();
            const string catalogPath = "Assets/Darklight/Resources/Darklight/TodayCharacterCatalog.asset";
            var catalog = AssetDatabase.LoadAssetAtPath<TodayCharacterCatalog>(catalogPath);
            if (catalog == null) { catalog = ScriptableObject.CreateInstance<TodayCharacterCatalog>(); AssetDatabase.CreateAsset(catalog, catalogPath); }
            catalog.pages = pages; EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); TodayCharacterBank.ClearCache();
            DarklightLivingIdleImporter.ImportIfPresent();
            return "Imported " + pages.Length + " base sheets / " + pages.Sum(p => p.spriteNames.Length) + " base frames";
        }
        public static Sprite[] ImportTextureFrames(string path,float ppu,Frame[] sprites,string maskPath,bool ownerMask)
        {
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                if(ti==null || sprites==null || sprites.Length==0)throw new InvalidOperationException("Missing full sprite source: "+path);
                ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.spritePixelsPerUnit = ppu; ti.filterMode = FilterMode.Point; ti.mipmapEnabled = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed; ti.maxTextureSize = 4096;
                ti.npotScale = TextureImporterNPOTScale.None; ti.wrapMode = TextureWrapMode.Clamp;
                ti.alphaIsTransparency = true;
                var settings = new TextureImporterSettings(); ti.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; ti.SetTextureSettings(settings); ti.SaveAndReimport();
                var factory = new SpriteDataProviderFactories(); factory.Init();
                var provider = factory.GetSpriteEditorDataProviderFromObject(ti);
                if (provider == null) throw new InvalidOperationException("Sprite provider missing: " + path);
                provider.InitSpriteEditorDataProvider();
                var capability = provider.GetDataProvider<ISpriteFrameEditCapability>();
                if (capability == null) throw new InvalidOperationException("Sprite capability missing: " + path);
                var cap = capability.GetEditCapability();
                foreach (var required in new[] { EEditCapability.CreateAndDeleteSprite, EEditCapability.EditSpriteName, EEditCapability.EditPivot, EEditCapability.EditSpriteRect })
                    if (!cap.HasCapability(required)) throw new InvalidOperationException("Sprite operation unavailable: " + required);
                var previous = provider.GetSpriteRects().ToDictionary(s => s.name, s => s.spriteID);
                var rects = sprites.Select(f => new SpriteRect
                {
                    name = f.name, rect = new Rect(f.x, f.y, f.w, f.h), pivot = new Vector2(f.pivotX, f.pivotY),
                    alignment = SpriteAlignment.Custom, spriteID = previous.TryGetValue(f.name, out var id) ? id : f.previousName != null && previous.TryGetValue(f.previousName, out var oldId) ? oldId : GUID.Generate()
                }).ToArray();
                provider.SetSpriteRects(rects);
                provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(f => new SpriteNameFileIdPair(f.name, f.spriteID)));
                provider.Apply(); ti.SaveAndReimport();
                var mi = (TextureImporter)AssetImporter.GetAtPath(maskPath);
                mi.textureType = TextureImporterType.Default; mi.filterMode = FilterMode.Point; mi.mipmapEnabled = false;
                mi.textureCompression = TextureImporterCompression.Uncompressed; mi.sRGBTexture = false;
                mi.alphaSource = ownerMask ? TextureImporterAlphaSource.FromInput : TextureImporterAlphaSource.None; mi.alphaIsTransparency = false; mi.npotScale = TextureImporterNPOTScale.None;
                mi.maxTextureSize = 4096; mi.wrapMode = TextureWrapMode.Clamp; mi.SaveAndReimport();
                var frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => int.Parse(s.name[(s.name.LastIndexOf('_') + 1)..])).ToArray();
                if (frames.Length != sprites.Length || !frames.Select(f => f.name).SequenceEqual(sprites.Select(f=>f.name)))
                    throw new InvalidOperationException("Incomplete or misordered whole-character sheet: " + path);
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(maskPath) == null) throw new InvalidOperationException("Missing palette: " + path);
                return frames;
        }
    }
}
