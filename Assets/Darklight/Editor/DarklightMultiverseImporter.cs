using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightMultiverseImporter
    {
        [Serializable]class Frame{public string id;public int[] rect;public float[] pivot;}
        [Serializable]class Sheet{public float pixelsPerUnit;public int[] nativeSize;public Frame[] frames;}
        [MenuItem("Darklight/Expansion/Import Multiverse")]
        public static void Import()
        {
            foreach(var name in new[]{"MossDetonator","NeonRevenant","AshRonin","Relics","MotorcycleSouth","Maelis"})
            {
                string path="Assets/Darklight/Resources/Darklight/Multiverse/"+name+".png";
                if(!File.Exists(path))throw new FileNotFoundException(path);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(Path.ChangeExtension(path,"json")));
                var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Multiple;
                t.filterMode=FilterMode.Point;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.Uncompressed;
                t.spritePixelsPerUnit=sheet.pixelsPerUnit;t.maxTextureSize=8192;t.npotScale=TextureImporterNPOTScale.None;
                var sprites=new SpriteMetaData[sheet.frames.Length];
                for(int i=0;i<sprites.Length;i++)
                {var f=sheet.frames[i];sprites[i]=new SpriteMetaData{name=f.id,rect=new Rect(f.rect[0],sheet.nativeSize[1]-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]),alignment=9,pivot=new Vector2(f.pivot[0],f.pivot[1])};}
#pragma warning disable CS0618
                t.spritesheet=sprites;
#pragma warning restore CS0618
                t.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();Debug.Log("EXPANSION_IMPORT_OK: Point, mipmaps off, uncompressed");
        }
    }
}
