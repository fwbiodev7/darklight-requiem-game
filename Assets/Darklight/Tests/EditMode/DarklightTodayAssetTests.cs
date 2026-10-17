using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightTodayAssetTests
    {
        [UnityTest] public IEnumerator ImportedTodayPagesHaveExactIdentityPhasesGroundPivotsAndPixelSettings()
        {
            var catalog=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            if(catalog==null)Assert.Ignore("Today catalog is awaiting protected final import; no new-art validation claimed");
            Assert.That(catalog.pages.Length,Is.GreaterThan(0));
            TodayCharacterBank.ClearCache();
            var identities=new System.Collections.Generic.HashSet<string>();
            foreach(var metadata in catalog.pages)
            {
                Assert.That(metadata.skin,Is.InRange(0,1));Assert.That(metadata.age,Is.Zero);Assert.That(metadata.weapon,Is.InRange(0,2));
                Assert.That(metadata.directions.Distinct().Count(),Is.EqualTo(metadata.directions.Length));
                CollectionAssert.Contains(new[]{"Combat","GuardDodge","Walk","Attack","Idle","Motion"},metadata.kind);
                Assert.That(metadata.phases,Is.EqualTo(metadata.kind=="Motion"?5:metadata.kind=="Idle"?(metadata.idleBlinkFrame==5?6:2):metadata.kind=="Walk"||metadata.kind=="Attack"?4:8));
                foreach(int direction in metadata.directions)
                {
                    Assert.That(direction,Is.InRange(0,7));
                    Assert.That(identities.Add($"{metadata.skin}/{metadata.age}/{metadata.weapon}/{direction}/{metadata.kind}"),Is.True,"Duplicate identity: "+metadata.name);
                }
                var page=TodayCharacterBank.Page(metadata.skin,metadata.age,metadata.weapon,metadata.directions[0],metadata.kind);
                Assert.That(page.name,Is.EqualTo(metadata.name));Assert.That(page.skin,Is.EqualTo(metadata.skin));Assert.That(page.age,Is.EqualTo(metadata.age));Assert.That(page.weapon,Is.EqualTo(metadata.weapon));Assert.That(page.spriteNames.Length,Is.EqualTo(page.phases*page.directions.Length));
                Assert.That(TodayCharacterBank.LoadedPageCount,Is.Zero,"Descriptor lookup eagerly loaded art");
                Assert.That(page.frames,Is.Null);Assert.That(page.palette,Is.Null);
                var leaf=Resources.LoadAll<Sprite>(page.resourcePath).Single(s=>s.name==page.spriteNames[0]);
                Assert.That(TodayCharacterBank.LoadedPageCount,Is.Zero,"Direct leaf lookup should not populate the page cache");
                var palette=TodayCharacterBank.Mask(leaf);
                Assert.That(TodayCharacterBank.LoadedPageCount,Is.EqualTo(1));
                Assert.That(page.frames.Length,Is.EqualTo(page.phases*page.directions.Length));Assert.That(page.palette,Is.SameAs(palette));
                CollectionAssert.AreEqual(page.spriteNames,page.frames.Select(s=>s.name).ToArray());
                Assert.That(page.palette,Is.Not.Null);
                foreach(var frame in page.frames)
                {
                    Assert.That(frame,Is.Not.Null);Assert.That(frame.name,Does.StartWith("Today_"));
                    Assert.That(frame.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(frame.texture.mipmapCount,Is.EqualTo(1));
                    Assert.That(frame.pixelsPerUnit,Is.GreaterThan(0));Assert.That(frame.pivot.x,Is.InRange(0,frame.rect.width));
                    Assert.That(frame.pivot.y,Is.InRange(0,frame.rect.height*.35f),"Ground pivot moved into the torso: "+frame.name);
                    Assert.That(page.palette.width,Is.EqualTo(frame.texture.width));Assert.That(page.palette.height,Is.EqualTo(frame.texture.height));
                    Assert.That(TodayCharacterBank.Mask(frame),Is.SameAs(page.palette));
                }
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(page.frames[0]));
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                Assert.That(importer.mipmapEnabled,Is.False);Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
                TodayCharacterBank.ClearCache();Assert.That(TodayCharacterBank.LoadedPageCount,Is.Zero);
                yield return Resources.UnloadUnusedAssets();
            }
        }
    }
}


