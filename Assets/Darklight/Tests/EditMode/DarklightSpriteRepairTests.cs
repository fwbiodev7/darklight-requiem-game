using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightSpriteRepairTests
    {
        [Serializable] class Frame{public string name;public float[] rect,pivot;}
        [Serializable] class Sheet{public Frame[] frames;public float pixelsPerUnit;}
        [TestCase("Campaign/Actions")][TestCase("Campaign/BossAttacks")][TestCase("Campaign/Bosses")]
        [TestCase("Campaign/Enemies")][TestCase("Campaign/EnemyAttacks")][TestCase("Campaign/FabioMotion")]
        [TestCase("Campaign/Forms")][TestCase("User/skinIdle")][TestCase("User/skinMotion")]
        [TestCase("User/skinActions")][TestCase("User/skinSlash")][TestCase("User/rockForms")]
        [TestCase("User/rock_daggers")][TestCase("User/requiem_daggers")]
        [TestCase("User/rock_chain")][TestCase("User/requiem_chain")]
        public void RebuiltFramesMatchSourceDimensionsAndKeepTransparentGutters(string sheetName)
        {
            string path="Assets/Darklight/Art/"+sheetName+".png";
            var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText("Assets/Darklight/Art/"+sheetName+"Rects.json"));
            var texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                texture.LoadImage(File.ReadAllBytes(path));var pixels=texture.GetPixels32();
                var imported=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.That(imported.width,Is.EqualTo(texture.width),"Importer resized atlas");Assert.That(imported.height,Is.EqualTo(texture.height));
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
                Assert.That(sprites.Count,Is.EqualTo(sheet.frames.Length));
                for(int i=0;i<sheet.frames.Length;i++)
                {
                    var f=sheet.frames[i];string name=f.name??Path.GetFileNameWithoutExtension(path)+"_"+i;var sprite=sprites[name];
                    var rect=new Rect(f.rect[0],texture.height-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]);
                    Assert.That(sprite.rect,Is.EqualTo(rect),name);Assert.That(sprite.pixelsPerUnit,Is.EqualTo(sheet.pixelsPerUnit).Within(.001f));
                    for(int y=(int)rect.yMin-16;y<(int)rect.yMax+16;y++)for(int x=(int)rect.xMin-16;x<(int)rect.xMax+16;x++)
                        if(!rect.Contains(new Vector2(x,y)))Assert.That(pixels[y*texture.width+x].a,Is.EqualTo(0),name+" has a neighboring sprite in its gutter");
                    Assert.That(sprite.pivot.x,Is.EqualTo(f.pivot[0]*f.rect[2]).Within(.02f));Assert.That(sprite.pivot.y,Is.EqualTo(f.pivot[1]*f.rect[3]).Within(.02f));
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(texture);}
        }
        [Test]public void SuppliedMenuMusicIsStreamedAndSeparateFromResonanceTracks()
        {
            const string path="Assets/Darklight/Audio/Music/metal-in-the-night-def-lev.mp3";var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            Assert.That(clip,Is.Not.Null);Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.Streaming));Assert.That(clip.length,Is.GreaterThan(10));
        }
    }
}
