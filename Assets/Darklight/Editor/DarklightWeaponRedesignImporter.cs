using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightWeaponRedesignImporter
    {
        const string Folder="Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/";
        [Serializable] public sealed class Frame{public string id;public int[] rect;public float[] pivot;}
        [Serializable] public sealed class Sheet
        {
            public int schemaVersion;
            public string image,imageSha256,sourcePath,sourceSha256,status;
            public int[] nativeSize,sourceSize;
            public float pixelsPerUnit;
            public bool wholeFigures;
            public Frame[] frames;
        }
        static string Hash(string path)
        {
            using var sha=SHA256.Create();using var stream=File.OpenRead(path);
            return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
        static int BigEndian(byte[] bytes,int start)=>(bytes[start]<<24)|(bytes[start+1]<<16)|(bytes[start+2]<<8)|bytes[start+3];
        static void VerifyPngSize(string path,int[] expected)
        {
            var header=new byte[24];using(var file=File.OpenRead(path))if(file.Read(header,0,header.Length)!=header.Length)throw new InvalidDataException(path);
            if(header[0]!=137||header[1]!=80||header[2]!=78||header[3]!=71||expected?.Length!=2||
                BigEndian(header,16)!=expected[0]||BigEndian(header,20)!=expected[1])throw new InvalidDataException("PNG native dimensions disagree with manifest: "+path);
        }
        static void VerifySheet(string path,Sheet sheet)
        {
            if(sheet==null||sheet.schemaVersion!=1||!sheet.wholeFigures||sheet.image!=Path.GetFileName(path)||
                sheet.status!="visual-reviewed"&&sheet.status!="imported"&&sheet.status!="runtime-verified"||
                !DarklightWeaponRedesignBank.Sha256(sheet.imageSha256)||!DarklightWeaponRedesignBank.Sha256(sheet.sourceSha256)||
                sheet.nativeSize?.Length!=2||sheet.nativeSize.Any(x=>x<=0||x>8192)||sheet.sourceSize?.Length!=2||
                sheet.pixelsPerUnit<=0||float.IsNaN(sheet.pixelsPerUnit)||float.IsInfinity(sheet.pixelsPerUnit)||sheet.frames==null||sheet.frames.Length<1||
                string.IsNullOrEmpty(sheet.sourcePath)||!sheet.sourcePath.StartsWith("ArtSources/",StringComparison.Ordinal)||sheet.sourcePath.Contains(".."))
                throw new InvalidDataException("Invalid whole-figure redesign sheet: "+path);
            if(!File.Exists(sheet.sourcePath)||!string.Equals(Hash(path),sheet.imageSha256,StringComparison.OrdinalIgnoreCase)||
                !string.Equals(Hash(sheet.sourcePath),sheet.sourceSha256,StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Changed or missing preserved redesign source: "+path);
            VerifyPngSize(path,sheet.nativeSize);VerifyPngSize(sheet.sourcePath,sheet.sourceSize);
            var names=new HashSet<string>(StringComparer.Ordinal);var rectangles=new List<RectInt>();
            foreach(var frame in sheet.frames)
            {
                if(frame==null||string.IsNullOrEmpty(frame.id)||!frame.id.StartsWith("Redesign_",StringComparison.Ordinal)||!names.Add(frame.id)||
                    frame.rect?.Length!=4||frame.pivot?.Length!=2||frame.pivot.Any(x=>float.IsNaN(x)||float.IsInfinity(x)||x<0||x>1))
                    throw new InvalidDataException("Invalid figure identity or pivot: "+path);
                var rect=new RectInt(frame.rect[0],frame.rect[1],frame.rect[2],frame.rect[3]);
                if(rect.x<0||rect.y<0||rect.width<1||rect.height<1||rect.xMax>sheet.nativeSize[0]||rect.yMax>sheet.nativeSize[1]||rectangles.Any(other=>other.Overlaps(rect)))
                    throw new InvalidDataException("Overlapping or out-of-bounds whole figure: "+frame.id);
                rectangles.Add(rect);
            }
        }
        static void ImportSheet(string path,Sheet sheet)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer==null)throw new InvalidOperationException("Missing redesign texture importer: "+path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=sheet.pixelsPerUnit;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=8192;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;importer.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            var defaults=importer.GetDefaultPlatformTextureSettings();defaults.textureCompression=TextureImporterCompression.Uncompressed;
            defaults.format=TextureImporterFormat.RGBA32;defaults.maxTextureSize=8192;importer.SetPlatformTextureSettings(defaults);
            foreach(string platform in new[]{"Standalone","Android","iPhone"})
            {
                var platformSettings=importer.GetPlatformTextureSettings(platform);
                if(platformSettings.overridden){platformSettings.textureCompression=TextureImporterCompression.Uncompressed;platformSettings.format=TextureImporterFormat.RGBA32;platformSettings.maxTextureSize=8192;importer.SetPlatformTextureSettings(platformSettings);}
            }
            importer.SaveAndReimport();
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);
            if(provider==null)throw new InvalidOperationException("Missing sprite data provider: "+path);
            provider.InitSpriteEditorDataProvider();
            var previous=provider.GetSpriteRects().ToDictionary(rect=>rect.name,rect=>rect.spriteID);
            var identity=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(identity==null)throw new InvalidOperationException("Missing stable sprite identity provider: "+path);
            var rects=sheet.frames.Select(frame=>new SpriteRect{
                name=frame.id,rect=new Rect(frame.rect[0],sheet.nativeSize[1]-frame.rect[1]-frame.rect[3],frame.rect[2],frame.rect[3]),
                pivot=new Vector2(frame.pivot[0],frame.pivot[1]),alignment=SpriteAlignment.Custom,
                spriteID=previous.TryGetValue(frame.id,out var old)?old:GUID.Generate()}).ToArray();
            provider.SetSpriteRects(rects);identity.SetNameFileIdPairs(rects.Select(rect=>new SpriteNameFileIdPair(rect.name,rect.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(sprite=>sprite.name,StringComparer.Ordinal);
            if(sprites.Count!=sheet.frames.Length||sheet.frames.Any(frame=>!sprites.ContainsKey(frame.id)))throw new InvalidOperationException("Missing imported redesign frames: "+path);
            foreach(var sprite in sprites.Values)
                if(sprite.texture.width!=sheet.nativeSize[0]||sprite.texture.height!=sheet.nativeSize[1]||Mathf.Abs(sprite.pixelsPerUnit-sheet.pixelsPerUnit)>.001f)
                    throw new InvalidOperationException("Redesign was resized or changed scale on import: "+path);
        }
        public static void ImportStandalone(string path)
        {
            var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(Path.ChangeExtension(path,"json")));
            VerifySheet(path,sheet);
            ImportSheet(path,sheet);
        }
        [MenuItem("DARKLIGHT/Arte/Importar redesign completo das armas")]
        public static void Import()
        {
            string catalogPath=Folder+"catalog.json";
            var catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(File.ReadAllText(catalogPath));
            if(!DarklightWeaponRedesignBank.ValidCatalog(catalog))throw new InvalidDataException("Invalid weapon redesign catalog");
            // Validate every source before mutating any import settings. Legacy banks are untouched.
            var sheets=new Dictionary<string,Sheet>(StringComparer.Ordinal);
            foreach(var page in catalog.pages)
            {
                string path="Assets/Darklight/Resources/"+page.resourcePath+".png";
                if(!sheets.TryGetValue(path,out var sheet))
                {
                    sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(Path.ChangeExtension(path,"json")));
                    VerifySheet(path,sheet);sheets[path]=sheet;
                }
                if(page.imageSha256!=sheet.imageSha256||page.sourceSha256!=sheet.sourceSha256||page.sourcePath!=sheet.sourcePath||
                    !page.nativeSize.SequenceEqual(sheet.nativeSize)||!page.sourceSize.SequenceEqual(sheet.sourceSize)||
                    Mathf.Abs(page.pixelsPerUnit-sheet.pixelsPerUnit)>.001f||page.spriteNames.Any(name=>!sheet.frames.Any(frame=>frame.id==name)))
                    throw new InvalidDataException("Page and complete-figure sheet disagree: "+page.id);
            }
            const string iconPath=Folder+"EquipmentIcons.png";
            if(File.Exists(Path.ChangeExtension(iconPath,"json")))
            {
                var icons=JsonUtility.FromJson<Sheet>(File.ReadAllText(Path.ChangeExtension(iconPath,"json")));
                VerifySheet(iconPath,icons);
                foreach(var frame in icons.frames)
                {
                    bool expected=false;
                    for(int weapon=0;weapon<3;weapon++)for(int form=0;form<4;form++)
                        if(frame.id=="Redesign_Icon_Weapon"+weapon+"_Form"+form)expected=true;
                    if(!expected)throw new InvalidDataException("Unknown equipment icon identity: "+frame.id);
                }
                sheets[iconPath]=icons;
            }
            foreach(var pair in sheets)ImportSheet(pair.Key,pair.Value);
            AssetDatabase.ImportAsset(catalogPath,ImportAssetOptions.ForceSynchronousImport);AssetDatabase.SaveAssets();
            DarklightWeaponRedesignBank.ClearCache();
            Debug.Log($"WEAPON_REDESIGN_IMPORTED: {sheets.Count} sheets, {catalog.pages.Length} exact context clips; complete flag={catalog.complete}. Coverage audit remains separate.");
        }
    }
}
