using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
namespace Darklight.Editor
{
    public static class PrototypeFormsImporter
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-prototype-20261014/prepared/";
        const string Target="Assets/Darklight/Editor/RemasterPrototype/TransformationsYoung.png";
        [Serializable] class Forms{public TodayCharacterImporter.Frame[] frames;}
        public static string Import()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            var input=JsonUtility.FromJson<Forms>(File.ReadAllText(Work+"forms.json"));if(input.frames.Length!=6)throw new Exception("Six complete figures required");
            File.Copy(Work+"TransformationsYoung.png",Target,true);AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            string palette=Target.Replace(".png","_Palette.png");File.Copy(Work+"TransformationsYoung_Palette.png",palette,true);AssetDatabase.ImportAsset(palette,ImportAssetOptions.ForceSynchronousImport);
            var mask=(TextureImporter)AssetImporter.GetAtPath(palette);mask.textureType=TextureImporterType.Default;mask.filterMode=FilterMode.Point;mask.mipmapEnabled=false;mask.textureCompression=TextureImporterCompression.Uncompressed;mask.sRGBTexture=false;mask.alphaIsTransparency=false;mask.alphaSource=TextureImporterAlphaSource.FromInput;mask.npotScale=TextureImporterNPOTScale.None;mask.maxTextureSize=4096;mask.SaveAndReimport();
            var ti=(TextureImporter)AssetImporter.GetAtPath(Target);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Multiple;ti.spritePixelsPerUnit=210;
            ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.npotScale=TextureImporterNPOTScale.None;ti.maxTextureSize=4096;ti.alphaIsTransparency=true;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
            var factories=new SpriteDataProviderFactories();factories.Init();var provider=factories.GetSpriteEditorDataProviderFromObject(ti);if(provider==null)throw new Exception("Missing sprite data provider");provider.InitSpriteEditorDataProvider();
            var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();if(capability==null)throw new Exception("Missing sprite editing capability");var cap=capability.GetEditCapability();
            foreach(var required in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditSpriteName,EEditCapability.EditSpriteRect,EEditCapability.EditPivot})if(!cap.HasCapability(required))throw new Exception("Unsupported operation "+required);
            var old=provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
            var rects=input.frames.Select(f=>new SpriteRect{name=f.name,rect=new Rect(f.x,f.y,f.w,f.h),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=old.TryGetValue(f.name,out var id)?id:GUID.Generate()}).ToArray();
            provider.SetSpriteRects(rects);var pairs=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(pairs==null)throw new Exception("Name/fileID provider missing");pairs.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();ti.SaveAndReimport();
            if(AssetDatabase.LoadAllAssetsAtPath(Target).OfType<Sprite>().Count()!=6)throw new Exception("Whole figure import mismatch");return "Six complete young transformed figures imported, Rock has no coat tails";
        }
    }
}
