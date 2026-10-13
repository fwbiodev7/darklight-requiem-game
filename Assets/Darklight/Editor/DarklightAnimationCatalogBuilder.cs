using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightAnimationCatalogBuilder
    {
        [Serializable] class Frame {public string name;public float[] rect,pivot;}
        [Serializable] class Sheet {public Frame[] frames;public float pixelsPerUnit;}
        public static void Import() => ImportSheets(null);
        public static void ImportOnly(string name) => ImportSheets(name);
        static void ImportSheets(string onlyName)
        {
            foreach(string manifest in Directory.GetFiles("Assets/Darklight/Resources/Darklight/AnimationCatalog","*Rects.json"))
            {
                string name=Path.GetFileNameWithoutExtension(manifest).Replace("Rects","");
                if(onlyName!=null && name!=onlyName)continue;
                string path="Assets/Darklight/Resources/Darklight/AnimationCatalog/"+name+".png";
                var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(path.Replace(".png","Rects.json")));
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new InvalidOperationException(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;importer.spritePixelsPerUnit=sheet.pixelsPerUnit;
                importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
                if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite|EEditCapability.EditSpriteRect|EEditCapability.EditPivot|EEditCapability.EditSpriteName))throw new InvalidOperationException("No walking sprite editor capability");
                var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(names==null)throw new InvalidOperationException("Missing identity provider");
                var old=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);int height=AssetDatabase.LoadAssetAtPath<Texture2D>(path).height;
                var rects=sheet.frames.Select(f=>new SpriteRect{name=f.name,spriteID=old.TryGetValue(f.name,out var id)?id:GUID.Generate(),rect=new Rect(f.rect[0],height-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]),alignment=SpriteAlignment.Custom,pivot=new Vector2(f.pivot[0],f.pivot[1])}).ToArray();
                provider.SetSpriteRects(rects);names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/animation-catalog-import.txt")),"SUCCESS: complete walking manifests imported; padding24; Point uncompressed");
        }
    }
}



