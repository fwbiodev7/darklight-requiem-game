using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightCombatParticlesV2Tests
    {
        [Test]
        public void ManifestLoadsWithSevenFamiliesAndFiftySixFrames()
        {
            var textAsset = Resources.Load<TextAsset>(DarklightCombatParticlesV2.ManifestResource);
            Assert.That(textAsset, Is.Not.Null, "Manifest text asset CombatParticlesV2 must exist in Resources");

            var manifest = JsonUtility.FromJson<DarklightCombatParticlesV2.RootManifest>(textAsset.text);
            Assert.That(manifest, Is.Not.Null);
            Assert.That(manifest.schemaVersion, Is.EqualTo(2));
            Assert.That(manifest.totalFamilies, Is.EqualTo(7));
            Assert.That(manifest.totalFrames, Is.EqualTo(56));
            Assert.That(manifest.families, Has.Length.EqualTo(7));

            for (int i = 0; i < manifest.families.Length; i++)
            {
                var fam = manifest.families[i];
                Assert.That(fam.frames, Has.Length.EqualTo(8), $"Family {fam.name} must have exactly 8 frames");
                Assert.That(fam.frameDurationMs, Is.InRange(25, 60), $"Family {fam.name} frame duration must be well-bounded");
                Assert.That(fam.recommendedScale, Is.GreaterThan(0.5f));
            }
        }

        [Test]
        public void AllFiftySixFramesLoadWithCorrectNamingAndOrder()
        {
            string[] familyPrefixes = new[]
            {
                "Contato_Impacto",
                "Ataque_Katana",
                "Ataque_Punhos",
                "Ataque_Moto",
                "Esquiva_Katana",
                "Esquiva_Punhos",
                "Esquiva_Moto"
            };

            int totalLoaded = 0;
            foreach (var prefix in familyPrefixes)
            {
                for (int frame = 1; frame <= 8; frame++)
                {
                    string path = $"{DarklightCombatParticlesV2.ResourcePrefix}Frames/{prefix}_{frame:D2}";
                    var sprite = Resources.Load<Sprite>(path);
                    Assert.That(sprite, Is.Not.Null, $"Frame sprite must load: {path}");
                    Assert.That(sprite.rect.width, Is.GreaterThan(20), $"Sprite width must be valid: {path}");
                    Assert.That(sprite.rect.height, Is.GreaterThan(20), $"Sprite height must be valid: {path}");
                    totalLoaded++;
                }
            }

            Assert.That(totalLoaded, Is.EqualTo(56), "Must load exactly 56 approved pixel-art frames");
        }

        [Test]
        public void PivotsMatchGuiaOficialSpecifications()
        {
            var textAsset = Resources.Load<TextAsset>(DarklightCombatParticlesV2.ManifestResource);
            var manifest = JsonUtility.FromJson<DarklightCombatParticlesV2.RootManifest>(textAsset.text);

            var expectedPivots = new Dictionary<string, Vector2>
            {
                { "Contato_Impacto", new Vector2(0.5f, 0.5f) },
                { "Ataque_Katana", new Vector2(0.5f, 0.5f) },
                { "Ataque_Punhos", new Vector2(0.5f, 0.5f) },
                { "Ataque_Moto", new Vector2(0.5f, 0.2f) },
                { "Esquiva_Katana", new Vector2(0.5f, 0.5f) },
                { "Esquiva_Punhos", new Vector2(0.5f, 0.25f) },
                { "Esquiva_Moto", new Vector2(0.5f, 0.2f) }
            };

            foreach (var fam in manifest.families)
            {
                Assert.That(expectedPivots.ContainsKey(fam.name), $"Family {fam.name} must be in expected pivots table");
                var exp = expectedPivots[fam.name];
                Assert.That(fam.pivot[0], Is.EqualTo(exp.x).Within(0.01f), $"{fam.name} pivot X");
                Assert.That(fam.pivot[1], Is.EqualTo(exp.y).Within(0.01f), $"{fam.name} pivot Y");
            }
        }

        [Test]
        public void RuntimeSystemInitializesAndEmitsAllFamiliesCleanly()
        {
            var host = new GameObject("Test_VFX2_Host");
            try
            {
                var vfx = host.AddComponent<DarklightCombatParticlesV2>();
                bool ready = vfx.Initialize();
                Assert.That(ready, Is.True, "CombatParticlesV2 must initialize successfully");
                Assert.That(vfx.TotalFramesLoaded, Is.GreaterThanOrEqualTo(40), "Total loaded frames must meet threshold");

                // Emit one burst of each family
                for (int f = 0; f < 7; f++)
                {
                    var family = (DarklightParticleV2Family)f;
                    int before = vfx.Count(family);
                    vfx.Emit(family, Vector3.zero, Vector2.right, 1.0f, 0f);
                    Assert.That(vfx.Count(family), Is.EqualTo(before + 1), $"{family} count must increment on emit");
                    Assert.That(vfx.LastPresentedSprite, Is.Not.Null, $"{family} last presented sprite must be set");
                }

                // Advance simulation past full lifecycle
                vfx.Advance(0.6f);
                Assert.That(vfx.StartedCount, Is.EqualTo(7));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
            }
        }
    }
}

