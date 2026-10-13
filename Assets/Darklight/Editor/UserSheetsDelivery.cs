using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class UserSheetsDelivery
    {
        [Serializable] class Manifest {public int cellWidth,cellHeight;public float[] pivot;public Sheet[] sheets;}
        [Serializable] class Sheet {public string name;public float ppu;public Frame[] frames;}
        [Serializable] class Frame {public string spriteName;public int[] atlasRect;}
        public static string Import()
        {
            var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText("Assets/Darklight/ArtSources/UserWholeSprites20261012/manifest.json"));int count=0;
            foreach(var sheet in manifest.sheets)
            {
                string path="Assets/Darklight/Resources/Darklight/UserWholeSprites/"+sheet.name+".png";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
                importer.spritePixelsPerUnit=sheet.ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=4096;importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.SaveAndReimport();
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);
                if(provider==null)throw new InvalidOperationException("Missing Sprite Editor provider: "+path);provider.InitSpriteEditorDataProvider();
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();if(capability==null)throw new InvalidOperationException("Missing edit capability: "+path);
                foreach(var operation in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditPivot,EEditCapability.EditSpriteRect,EEditCapability.EditSpriteName})
                    if(!capability.GetEditCapability().HasCapability(operation))throw new InvalidOperationException("Unsupported sprite operation: "+operation);
                var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(names==null)throw new InvalidOperationException("Missing sprite file ID provider");
                var old=provider.GetSpriteRects();var rects=sheet.frames.Select(f=>new SpriteRect{name=f.spriteName,rect=new Rect(f.atlasRect[0],f.atlasRect[1],f.atlasRect[2],f.atlasRect[3]),alignment=SpriteAlignment.Custom,pivot=new Vector2(manifest.pivot[0],manifest.pivot[1]),spriteID=old.FirstOrDefault(r=>r.name==f.spriteName)?.spriteID??GUID.Generate()}).ToArray();
                provider.SetSpriteRects(rects);names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();importer.SaveAndReimport();
                int imported=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Count();if(imported!=8)throw new InvalidOperationException("Expected 8 complete sprites: "+path);count+=imported;
            }
            return "SUCCESS: "+count+" whole character sprites; Point, no mipmaps, uncompressed, FullRect";
        }
        public static void Build(string output,string marker)
        {
            try
            {
                Directory.CreateDirectory(output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=Path.Combine(output,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                string result=$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nwarnings={report.summary.totalWarnings}\nbytes={report.summary.totalSize}\ntime={report.summary.totalTime}\npath={output}";
                File.WriteAllText(marker,result);File.WriteAllText(Path.Combine(output,"resultado-build.txt"),result);
                if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
            }
            catch(Exception e){File.WriteAllText(marker,e.ToString());Debug.LogException(e);}
        }
    }
}
