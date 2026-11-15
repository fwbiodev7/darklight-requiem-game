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
            var pages = catalog.pages.Where(p => p.resourcePath.EndsWith("Session20261114")).ToArray();
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
    }
}
