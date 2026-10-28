using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightDaggerSheetEightTests
    {
        const string Revised="Darklight/PlayerVisualRevision20261020/RequiemDaggersViolet8";
        [TearDown] public void Clear(){DarklightTransformedBank.ClearCache();}
        [Test] public void EightRevisedPosesResolveThroughTheExistingTwoDirectionalPages()
        {
            var catalog=Resources.Load<DarklightTransformedCatalog>("Darklight/TransformedCatalog");
            var normals=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            var seen=new HashSet<string>();int changed=0;
            foreach(string name in new[]{"RequiemCombatDaggersYoungCardinal","RequiemCombatDaggersYoungDiagonal"})
            {
                var normalPage=normals.pages.Single(p=>p.name==name);
                var page=catalog.pages.Single(p=>p.baseName==name&&p.form==2);
                var frames=Resources.LoadAll<Sprite>(normalPage.resourcePath).ToDictionary(s=>s.name);
                Assert.That(page.references.Length,Is.EqualTo(normalPage.spriteNames.Length));
                for(int i=0;i<page.references.Length;i++)
                {
                    var reference=page.references[i];var normal=frames[normalPage.spriteNames[i]];
                    Assert.That(DarklightTransformedBank.TryResolve(normal,2,out var sprite,out var owner,out int id),Is.True);
                    Assert.That(sprite.name,Is.EqualTo(reference.spriteName));Assert.That(id,Is.EqualTo(reference.ownerId));Assert.That(owner,Is.Not.Null);
                    Assert.That(AssetDatabase.GetAssetPath(sprite),Is.EqualTo("Assets/Darklight/Resources/"+reference.resourcePath+".png"));
                    if(reference.resourcePath!=Revised)continue;
                    changed++;seen.Add(sprite.name);
                    foreach(int form in new[]{1,3})
                    {
                        Assert.That(DarklightTransformedBank.TryResolve(normal,form,out var other,out _,out _),Is.True);
                        Assert.That(AssetDatabase.GetAssetPath(other),Does.Not.Contain("PlayerVisualRevision20261020"));
                    }
                }
            }
            Assert.That(changed,Is.EqualTo(8));Assert.That(seen.Count,Is.EqualTo(8));
        }
        [Test] public void EightPoseSheetKeepsNativeDimensionsAndLosslessPixelImport()
        {
            var sprites=Resources.LoadAll<Sprite>(Revised);Assert.That(sprites.Length,Is.EqualTo(8));
            foreach(var sprite in sprites)
            {
                Assert.That(sprite.texture.width,Is.EqualTo(2048));Assert.That(sprite.texture.height,Is.EqualTo(1024));
                Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(512,512)));
                Assert.That(sprite.pixelsPerUnit,Is.InRange(100,250));
                Assert.That(sprite.pivot.x,Is.InRange(4,508));Assert.That(sprite.pivot.y,Is.InRange(4,508));
                Assert.That(DarklightCanonicalComplexion.ForTexture(sprite.texture).w,Is.EqualTo(1));
            }
            foreach(string suffix in new[]{"","_Owner"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Darklight/Resources/"+Revised+suffix+".png");
                Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                Assert.That(importer.sRGBTexture,Is.EqualTo(suffix==""));
            }
        }
    }
}
