using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightLoadingArtImporter
    {
        public static string Import(string sources)
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            for(int chapter=1;chapter<=10;chapter++)if(!File.Exists(Path.Combine(sources,"preview"+chapter+".png")))throw new InvalidOperationException("Missing dedicated loading illustration "+chapter);
            const string folder="Assets/Darklight/Resources/Darklight/Loading";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Darklight/Resources/Darklight","Loading");
            for(int chapter=1;chapter<=10;chapter++)File.Copy(Path.Combine(sources,"preview"+chapter+".png"),folder+"/preview"+chapter+".png",true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            for(int chapter=1;chapter<=10;chapter++)
            {
                string path=folder+"/preview"+chapter+".png";var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new InvalidOperationException("Missing texture importer");
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.spritePixelsPerUnit=100;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;importer.SetTextureSettings(settings);importer.SaveAndReimport();
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null||sprite.rect.width<512||sprite.rect.width/sprite.rect.height<1.2f||sprite.rect.width/sprite.rect.height>2)throw new InvalidOperationException("Invalid loading illustration dimensions: "+chapter);
            }
            string result="{\"success\":true,\"dedicatedIllustrations\":10,\"wholeMapOverview\":false}";File.WriteAllText(Path.Combine(sources,"import-result.json"),result);return result;
        }
    }
}
