using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    public static class DarklightMenuBuilder
    {
        const string Root="Assets/Darklight/Resources/Darklight/Menu/";
        [Serializable] class Frame {public string name;public float[] rect,pivot,border;}
        [Serializable] class Sheet {public Frame[] frames;}
        public static void Import()
        {
            foreach(string name in new[]{"Vespera","Mist","RequiemPose","RockPose"})
            {
                string path=Root+name+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)throw new InvalidOperationException("Missing menu art: "+path);
                bool poses=name.EndsWith("Pose");
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=poses?SpriteImportMode.Multiple:SpriteImportMode.Single;
                importer.spritePixelsPerUnit=48;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=name!="Vespera";
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.isReadable=false;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
                if(!poses)continue;
                var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(Root+name+"Rects.json"));
                var factories=new SpriteDataProviderFactories();factories.Init();
                var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
                if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite|EEditCapability.EditSpriteRect|EEditCapability.EditPivot|EEditCapability.EditSpriteName))throw new InvalidOperationException("Menu sprite editing unavailable");
                var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();if(names==null)throw new InvalidOperationException("Menu identity provider unavailable");
                var old=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);
                int height=AssetDatabase.LoadAssetAtPath<Texture2D>(path).height;
                var rects=sheet.frames.Select(f=>new SpriteRect{name=f.name,spriteID=old.TryGetValue(f.name,out var id)?id:GUID.Generate(),rect=new Rect(f.rect[0],height-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]),alignment=SpriteAlignment.Custom,pivot=new Vector2(f.pivot[0],f.pivot[1]),border=Vector4.zero}).ToArray();
                provider.SetSpriteRects(rects);names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/menu-import-result.txt")),"SUCCESS: original Vespera, 2 skins x4 wind frames, fog, Point filter, no mipmaps or compression");
        }
        public static void BuildWindows()
        {
            string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../outputs/menu-cinematografico/Windows"));Directory.CreateDirectory(output);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=Path.Combine(output,"DarklightRequiem.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Path.Combine(output,"resultado-build.txt"),$"result={report.summary.result}\nerrors={report.summary.totalErrors}\nwarnings={report.summary.totalWarnings}\nbytes={report.summary.totalSize}\ntime={report.summary.totalTime}");
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Menu build failed.");
        }
    }
}
