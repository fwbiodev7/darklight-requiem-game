using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class WholeCharacterImporter
    {
        [Serializable] class Definition {public Page[] pages;public Palette[] palettes;}
        [Serializable] class Palette {public string sourcePath,mask;}
        [Serializable] class Page {public string name;public int width,height;public float ppu;public Cut[] sprites;}
        [Serializable] class Cut {public string name;public int x,y,w,h;public float pivotX,pivotY;}
        [Serializable] class Entry {public string sourcePath,sourceName,page;public int age,index;}
        public static void Import(string manifestPath,string marker)
        {
            var data=JsonUtility.FromJson<Definition>(File.ReadAllText(manifestPath));
            const string folder="Assets/Darklight/Resources/Darklight/WholeCharacterAnimations/";
            foreach(var page in data.pages)
            {
                string path=folder+page.name+".png";
                var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Multiple;ti.spritePixelsPerUnit=page.ppu;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=4096;ti.npotScale=TextureImporterNPOTScale.None;ti.wrapMode=TextureWrapMode.Clamp;ti.alphaIsTransparency=true;ti.sRGBTexture=true;var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(ti);provider.InitSpriteEditorDataProvider();
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
                if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite)||!capability.GetEditCapability().HasCapability(EEditCapability.EditPivot)||!capability.GetEditCapability().HasCapability(EEditCapability.EditSpriteRect)||!capability.GetEditCapability().HasCapability(EEditCapability.EditSpriteName))throw new InvalidOperationException("Sprite editing capability required");
                var old=provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
                var rects=page.sprites.Select(c=>new SpriteRect{name=c.name,rect=new Rect(c.x,c.y,c.w,c.h),pivot=new Vector2(c.pivotX,c.pivotY),alignment=SpriteAlignment.Custom,spriteID=old.TryGetValue(c.name,out var id)?id:GUID.Generate()}).ToArray();
                provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();ti.SaveAndReimport();
                var mi=(TextureImporter)AssetImporter.GetAtPath(folder+page.name+"_Palette.png");mi.textureType=TextureImporterType.Default;mi.sRGBTexture=false;mi.alphaSource=TextureImporterAlphaSource.None;mi.alphaIsTransparency=false;mi.filterMode=FilterMode.Point;mi.mipmapEnabled=false;mi.textureCompression=TextureImporterCompression.Uncompressed;mi.maxTextureSize=4096;mi.npotScale=TextureImporterNPOTScale.None;mi.wrapMode=TextureWrapMode.Clamp;mi.SaveAndReimport();
                if(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count()!=rects.Length)throw new InvalidOperationException("Wrong complete sprite count: "+page.name);
            }
            foreach(var p in data.palettes)
            {
                var path="Assets/Darklight/Resources/Darklight/WholeCharacterPalettes/"+p.mask+".png";
                var mi=(TextureImporter)AssetImporter.GetAtPath(path);mi.textureType=TextureImporterType.Default;mi.sRGBTexture=false;mi.alphaSource=TextureImporterAlphaSource.None;mi.alphaIsTransparency=false;mi.filterMode=FilterMode.Point;mi.mipmapEnabled=false;mi.textureCompression=TextureImporterCompression.Uncompressed;mi.maxTextureSize=4096;mi.npotScale=TextureImporterNPOTScale.None;mi.wrapMode=TextureWrapMode.Clamp;mi.SaveAndReimport();
            }
            const string catPath="Assets/Darklight/Resources/Darklight/WholeCharacterCatalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<DarklightWholeCharacterCatalog>(catPath);if(catalog==null){catalog=ScriptableObject.CreateInstance<DarklightWholeCharacterCatalog>();AssetDatabase.CreateAsset(catalog,catPath);}
            catalog.palettes=data.palettes.Select(p=>new WholeCharacterPalette{source=AssetDatabase.LoadAssetAtPath<Texture2D>(p.sourcePath),mask=p.mask}).ToArray();EditorUtility.SetDirty(catalog);
            ArchiveRejected();AssetDatabase.SaveAssets();
            File.WriteAllText(marker,"SUCCESS: "+data.pages.Sum(p=>p.sprites.Length)+" newly drawn whole-character frames; "+data.pages.Length+" sheets; no separated heads or offline head compositions");
        }
        static void ArchiveRejected()
        {
            const string archive="Assets/Darklight/ArtSources/RejectedHeadComposition20261012";
            Directory.CreateDirectory(archive);AssetDatabase.Refresh();
            void Move(string source,string name)
            {
                if(string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(source)))return;
                string error=AssetDatabase.MoveAsset(source,archive+"/"+name);if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
            }
            Move("Assets/Darklight/Resources/Darklight/CompleteAgeBodies","CompleteAgeBodies");
            Move("Assets/Darklight/Resources/Darklight/CompleteAgeCatalog.asset","CompleteAgeCatalog.asset");
            foreach(var file in Directory.GetFiles("Assets/Darklight/Resources/Darklight/OriginalVampireArt","*Head*.png"))Move(file.Replace('\\','/'),Path.GetFileName(file));
            var original=AssetDatabase.LoadAssetAtPath<DarklightOriginalVampireCatalog>("Assets/Darklight/Resources/Darklight/OriginalVampireCatalog.asset");EditorUtility.SetDirty(original);
        }
    }
}
