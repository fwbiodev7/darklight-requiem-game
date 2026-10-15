using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
namespace Darklight.Editor
{
    public static class SecondaryStyleImporter
    {
        const string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/secondary-style-20261014/";
        const string Target="Assets/Darklight/Resources/Darklight/SecondaryCharacters/";
        [Serializable] class Manifest{public Page[] pages;}
        [Serializable] class Page{public string name,field,sourceHash;public int offset;public float ppu;public TodayCharacterImporter.Frame[] sprites;}
        [Serializable] class Result{public bool success;public int pages,frames;public string error;}
        public static string Import()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            var result=new Result();
            try
            {
                var input=JsonUtility.FromJson<Manifest>(File.ReadAllText(Work+"prepared/manifest.json"));
                if(input.pages.Length!=18||input.pages.Any(p=>p.sprites.Length!=8||Path.GetFileName(p.name)!=p.name))throw new InvalidOperationException("Expected18 whole secondary sheets/8frames each");
                Directory.CreateDirectory(Target);AssetDatabase.StartAssetEditing();
                try{foreach(var p in input.pages)foreach(string suffix in new[]{".png","_Palette.png"})File.Copy(Work+"prepared/"+p.name+suffix,Target+p.name+suffix,true);}
                finally{AssetDatabase.StopAssetEditing();}
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                var catalog=new SecondaryStyleCatalog{pages=input.pages.Select(p=>new SecondaryStylePage{name=p.name,resourcePath="Darklight/SecondaryCharacters/"+p.name,paletteResourcePath="Darklight/SecondaryCharacters/"+p.name+"_Palette",spriteNames=p.sprites.Select(f=>f.name).ToArray()}).ToArray()};
                foreach(var p in input.pages)
                {
                    string path=Target+p.name+".png";var ti=(TextureImporter)AssetImporter.GetAtPath(path);
                    ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Multiple;ti.spritePixelsPerUnit=p.ppu;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.npotScale=TextureImporterNPOTScale.None;ti.wrapMode=TextureWrapMode.Clamp;ti.maxTextureSize=4096;ti.alphaIsTransparency=true;
                    var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
                    var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(ti);if(provider==null)throw new InvalidOperationException("Sprite provider missing");provider.InitSpriteEditorDataProvider();
                    var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();if(capability==null)throw new InvalidOperationException("Sprite edit capability missing");var cap=capability.GetEditCapability();
                    foreach(var required in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditSpriteName,EEditCapability.EditPivot,EEditCapability.EditSpriteRect})if(!cap.HasCapability(required))throw new InvalidOperationException("Sprite edit unavailable: "+required);
                    var previous=provider.GetSpriteRects().ToDictionary(s=>s.name,s=>s.spriteID);
                    var rects=p.sprites.Select(f=>new SpriteRect{name=f.name,rect=new Rect(f.x,f.y,f.w,f.h),pivot=new Vector2(f.pivotX,f.pivotY),alignment=SpriteAlignment.Custom,spriteID=previous.TryGetValue(f.name,out var id)?id:GUID.Generate()}).ToArray();
                    provider.SetSpriteRects(rects);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();ti.SaveAndReimport();
                    var mask=(TextureImporter)AssetImporter.GetAtPath(Target+p.name+"_Palette.png");mask.textureType=TextureImporterType.Default;mask.filterMode=FilterMode.Point;mask.mipmapEnabled=false;mask.textureCompression=TextureImporterCompression.Uncompressed;mask.sRGBTexture=false;mask.alphaIsTransparency=false;mask.alphaSource=TextureImporterAlphaSource.FromInput;mask.npotScale=TextureImporterNPOTScale.None;mask.wrapMode=TextureWrapMode.Clamp;mask.maxTextureSize=4096;mask.SaveAndReimport();
                    var actual=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
                    if(actual.Count!=8||p.sprites.Any(f=>!actual.ContainsKey(f.name)))throw new InvalidOperationException("Secondary frame import mismatch: "+p.name);
                }
                File.WriteAllText("Assets/Darklight/Resources/Darklight/SecondaryStyleCatalog.json",JsonUtility.ToJson(catalog,true));AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);DarklightSecondaryStyleBank.Clear();
                var scene=EditorSceneManager.GetActiveScene();if(scene.path!="Assets/Darklight/Scenes/DarklightCampaign.unity")scene=EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
                var game=UnityEngine.Object.FindFirstObjectByType<DarklightCampaign>();
                foreach(var p in input.pages.Where(p=>p.field!="ExpansionBosses"&&p.field!="NoctharFinal"))
                {
                    var frames=DarklightSecondaryStyleBank.Load(p.name);
                    var field=typeof(DarklightCampaign).GetField(p.field);var array=(Sprite[])field.GetValue(game);
                    if(p.field=="icaroFrames"||p.field=="lyraFrames")field.SetValue(game,frames);
                    else
                    {
                        Array.Copy(frames,0,array,p.offset,4);var attacks=typeof(DarklightCampaign).GetField(p.field.Replace("Frames","Attacks"));var attackArray=(Sprite[])attacks.GetValue(game);Array.Copy(frames,4,attackArray,p.offset,4);attacks.SetValue(game,attackArray);field.SetValue(game,array);
                    }
                }
                EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();result.pages=input.pages.Length;result.frames=result.pages*8;result.success=true;
            }
            catch(Exception ex){result.error=ex.ToString();Debug.LogException(ex);}
            finally{File.WriteAllText(Work+"imported-result.json",JsonUtility.ToJson(result,true));}
            return result.success?$"Integrated{result.pages} secondary sheets/{result.frames} whole frames":result.error;
        }
    }
}
