using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightSession14LuciusSourcesTests
    {
        [TestCase(DarklightLuciusAppearance.Normal)]
        [TestCase(DarklightLuciusAppearance.Arconte)]
        public void ReceivedClipsResolveImportedWholeFramesWithoutIdleFallback(DarklightLuciusAppearance appearance)
        {
            string root = appearance == DarklightLuciusAppearance.Normal ? DarklightLuciusSpriteBank.Root : DarklightLuciusSpriteBank.ArconteRoot;
            var text = Resources.Load<TextAsset>(root + "catalog");
            Assert.That(text, Is.Not.Null);
            var catalog = JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            var pages = catalog.pages.Where(p => p.resourcePath.EndsWith("Session20261114") && !p.directionalFallback).ToArray();
            Assert.That(pages.Length, Is.EqualTo(appearance == DarklightLuciusAppearance.Normal ? 6 : 39));
            DarklightLuciusSpriteBank.ClearCache();
            foreach (var page in pages)
            {
                Assert.That(DarklightLuciusSpriteBank.TryPose(appearance, page.action, page.direction, 0,
                    out var first, out bool fallback, page.attackKey), Is.True, page.id);
                Assert.That(fallback, Is.False, page.id);
                Assert.That(first.name, Is.EqualTo(page.spriteNames[0]), page.id);
                var imported = Resources.LoadAll<Sprite>(page.resourcePath);
                foreach (string name in page.spriteNames)
                {
                    var frame = imported.SingleOrDefault(s => s.name == name);
                    Assert.That(frame, Is.Not.Null, page.id + ": " + name);
                    Assert.That(frame.pixelsPerUnit, Is.EqualTo(page.pixelsPerUnit).Within(.001f));
                    Assert.That(frame.texture.filterMode, Is.EqualTo(FilterMode.Point));
                }
            }
        }

        [TestCase(DarklightLuciusAppearance.Normal)]
        [TestCase(DarklightLuciusAppearance.Arconte)]
        public void CompleteCatalogAuditResolvesEveryDeclaredFrame(DarklightLuciusAppearance appearance)
        {
            string root=appearance==DarklightLuciusAppearance.Normal?DarklightLuciusSpriteBank.Root:DarklightLuciusSpriteBank.ArconteRoot;
            var text=Resources.Load<TextAsset>(root+"catalog");
            Assert.That(text,Is.Not.Null);
            var catalog=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(catalog,appearance),Is.True);
            Assert.That(catalog.pages.Count(page=>page.action=="Walk"&&!page.directionalFallback),Is.EqualTo(8));
            var dodges=catalog.pages.Where(page=>page.action=="Dodge").ToArray();
            Assert.That(dodges.All(page=>!page.loop&&!page.directionalFallback&&page.spriteNames.Length==3&&
                page.durationMs.Sum()==230),Is.True);
            if(appearance==DarklightLuciusAppearance.Normal)
                Assert.That(catalog.pages.Count(page=>page.action=="Parry"&&page.spriteNames.Length>=3&&!page.directionalFallback),Is.EqualTo(8));
            var techniques=catalog.pages.Where(page=>page.action=="Attack").GroupBy(page=>page.attackKey).ToArray();
            Assert.That(techniques.Length,Is.EqualTo(3));
            foreach(var attack in techniques)
            {
                Assert.That(attack.Count(),Is.EqualTo(8),attack.Key);
                Assert.That(attack.All(page=>!page.directionalFallback),Is.True,attack.Key);
                Assert.That(attack.Select(page=>page.spriteNames[0]).Distinct().Count(),Is.EqualTo(8),attack.Key);
            }
            foreach(var page in catalog.pages)
            {
                int elapsed=0;
                for(int index=0;index<page.spriteNames.Length;index++)
                {
                    float time=(elapsed+page.durationMs[index]*.5f)/1000f;
                    Assert.That(DarklightLuciusSpriteBank.TryPose(appearance,page.action,page.direction,time,out var sprite,out bool fallback,page.attackKey),Is.True,page.id);
                    Assert.That(fallback,Is.False,page.id);
                    Assert.That(sprite.name,Is.EqualTo(page.spriteNames[index]),page.id);
                    Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point),page.id);
                    elapsed+=page.durationMs[index];
                }
                if(page.directionalFallback)
                {
                    Assert.That(DarklightLuciusSpriteBank.TryActionPage(appearance,page.action,page.sourceDirection,page.attackKey,out var source),Is.True,page.id);
                    Assert.That(page.spriteNames,Is.EqualTo(source.spriteNames),page.id);
                }
            }
        }
    }
}
