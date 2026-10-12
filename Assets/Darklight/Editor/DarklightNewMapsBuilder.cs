using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightNewMapsBuilder
    {
        public static void Import()
        {
            for(int n=7;n<=10;n++)
            {
                string path="Assets/Darklight/Resources/Darklight/Maps/map"+n+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer==null)throw new InvalidOperationException("Missing new region: "+path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=24;
                importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.alphaIsTransparency=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/new-regions-import.txt")),"SUCCESS: 4 regions, source 1536x1024, PPU24, Point, uncompressed");
        }
    }
}
