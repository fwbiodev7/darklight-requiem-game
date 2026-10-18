using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightLivingIdleAssetTests
    {
        [SetUp] public void RequireReviewedProductionManifest()
        {
            if(!System.IO.File.Exists("Assets/Darklight/ArtSources/LivingIdle20261017/manifest.json"))
                Assert.Ignore("New idle sheets are under visual review and have not been imported into production.");
        }
        [Test] public void NewIdlesCoverEightDirectionsBothSkinsAndAllThreeForms()
        {
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();
            for(int skin=0;skin<2;skin++)for(int direction=0;direction<8;direction++)
            {
                var page=TodayCharacterBank.Page(skin,0,0,direction,"Idle");
                Assert.That(page,Is.Not.Null);Assert.That(page.phases,Is.EqualTo(6));
                Assert.That(page.idleBlinkFrame,Is.EqualTo(5));Assert.That(page.ownerMask,Is.True);
                var normal=TodayCharacterBank.YoungIdle(skin,0,direction,0);
                var blink=TodayCharacterBank.YoungIdle(skin,0,direction,6.05f);
                Assert.That(normal,Is.Not.Null);Assert.That(blink,Is.Not.SameAs(normal));
                int row=Array.IndexOf(page.directions,direction);
                Assert.That(blink.name,Does.EndWith("_"+(row*6+5)));
                for(int form=1;form<=3;form++)
                {
                    Assert.That(DarklightTransformedBank.TryResolve(blink,form,out var transformed,out var mask,out var owner),Is.True);
                    Assert.That(transformed,Is.Not.Null);Assert.That(mask,Is.Not.Null);
                    Assert.That(owner,Is.EqualTo(row*6+6));
                    Assert.That(transformed.bounds.size.y/normal.bounds.size.y,Is.InRange(.88f,1.12f),"Whole atlas import must keep physical body scale across forms");
                    Assert.That(transformed.pivot.y,Is.InRange(0,transformed.rect.height*.35f));
                    Assert.That(transformed.texture.filterMode,Is.EqualTo(FilterMode.Point));
                }
            }
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();
        }
        [Test] public void CombatAndDeathDescriptorsRemainTheExistingProductionPages()
        {
            var catalog=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            Assert.That(catalog.pages.Count(p=>p.kind=="Combat"),Is.EqualTo(12));
            Assert.That(catalog.pages.Count(p=>p.kind=="GuardDodge"),Is.EqualTo(20));
            foreach(var p in catalog.pages.Where(p=>p.name.Contains("IdleLiving")))
                Assert.That(p.kind,Is.EqualTo("Idle"));
        }
        [Test] public void OwnerMasksRetainRawAlphaIdentityInsteadOfTransparencyOptimization()
        {
            var catalog=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            foreach(var page in catalog.pages.Where(p=>p.name.Contains("IdleLiving")))
            {
                var path="Assets/Darklight/Resources/"+page.paletteResourcePath+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                Assert.That(importer.alphaIsTransparency,Is.False);Assert.That(importer.sRGBTexture,Is.False);
                Assert.That(importer.mipmapEnabled,Is.False);Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            }
        }
    }
}
