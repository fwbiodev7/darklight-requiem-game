using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightTransformedImporter
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/transformed-animation-20261014/prepared/";
        const string Folder="Assets/Darklight/Resources/Darklight/TransformedYoung/";
        [Serializable]sealed class Manifest {public bool wholeCharactersOnly;public Page[] pages;}
        [Serializable]sealed class Page {public string name,baseName;public int form;public float ppu;public Frame[] sprites;}
        [Serializable]sealed class Frame {public string name;public int x,y,w,h,normalIndex,ownerId;public float pivotX,pivotY;}
        public static string Import()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            var data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Work+"manifest.json"));if(!data.wholeCharactersOnly)throw new InvalidOperationException("Complete figures required");
            if(!AssetDatabase.IsValidFolder(Folder.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Darklight/Resources/Darklight","TransformedYoung");
            foreach(var page in data.pages)foreach(string suffix in new[]{".png","_Owner.png"}){if(Path.GetFileName(page.name)!=page.name)throw new InvalidOperationException("Unsafe source name");File.Copy(Work+page.name+suffix,Folder+page.name+suffix,true);}
            AssetDatabase.Refresh();var factories=new SpriteDataProviderFactories();factories.Init();
            var descriptors=data.pages.Select(page=>
            {
                string path=Folder+page.name+".png";var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Multiple;ti.spritePixelsPerUnit=page.ppu;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=4096;ti.alphaIsTransparency=true;ti.npotScale=TextureImporterNPOTScale.None;
                var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);
                var provider=factories.GetSpriteEditorDataProviderFromObject(ti);if(provider==null)throw new InvalidOperationException("No sprite data provider");provider.InitSpriteEditorDataProvider();if(!provider.HasDataProvider(typeof(ISpriteNameFileIdDataProvider)))throw new InvalidOperationException("No sprite identity interface");var previous=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();if(capability==null)throw new InvalidOperationException("No sprite edit capability");
                foreach(var operation in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditSpriteName,EEditCapability.EditSpriteRect,EEditCapability.EditPivot})if(!capability.GetEditCapability().HasCapability(operation))throw new InvalidOperationException("Unavailable sprite operation: "+operation);
                var rects=page.sprites.Select(f=>new SpriteRect{name=f.name,rect=new Rect(f.x,f.y,f.w,f.h),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=previous.TryGetValue(f.name,out var id)?id:GUID.Generate()}).ToArray();provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();ti.SaveAndReimport();
                var mi=(TextureImporter)AssetImporter.GetAtPath(Folder+page.name+"_Owner.png");mi.textureType=TextureImporterType.Default;mi.filterMode=FilterMode.Point;mi.mipmapEnabled=false;mi.sRGBTexture=false;mi.alphaSource=TextureImporterAlphaSource.FromInput;mi.alphaIsTransparency=false;mi.textureCompression=TextureImporterCompression.Uncompressed;mi.npotScale=TextureImporterNPOTScale.None;mi.maxTextureSize=4096;mi.SaveAndReimport();
                if(AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count()!=rects.Length)throw new InvalidOperationException("Incomplete whole sprites");
                return new DarklightTransformedPage{baseName=page.baseName,form=page.form,resourcePath="Darklight/TransformedYoung/"+page.name,palettePath="Darklight/TransformedYoung/"+page.name+"_Owner",spriteNames=page.sprites.Select(f=>f.name).ToArray(),frameIds=page.sprites.Select(f=>f.ownerId).ToArray()};
            }).ToArray();
            const string rawPath="Assets/Darklight/Editor/TransformedRawCatalog.asset";
            var raw=AssetDatabase.LoadAssetAtPath<DarklightTransformedCatalog>(rawPath);if(raw==null){raw=ScriptableObject.CreateInstance<DarklightTransformedCatalog>();AssetDatabase.CreateAsset(raw,rawPath);}raw.pages=descriptors;EditorUtility.SetDirty(raw);
            const string catalogPath="Assets/Darklight/Resources/Darklight/TransformedCatalog.asset";var catalog=AssetDatabase.LoadAssetAtPath<DarklightTransformedCatalog>(catalogPath);if(catalog==null){catalog=ScriptableObject.CreateInstance<DarklightTransformedCatalog>();AssetDatabase.CreateAsset(catalog,catalogPath);}catalog.pages=descriptors;EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();DarklightTransformedBank.ClearCache();string result="{\"success\":true,\"pages\":"+descriptors.Length+",\"frames\":"+descriptors.Sum(p=>p.spriteNames.Length)+"}";File.WriteAllText(Work+"import-result.json",result);return result;
        }
    }
}
