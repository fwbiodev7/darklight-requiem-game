using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    [Parallelizable(ParallelScope.None)]
    public sealed class DarklightLuciusAnimationIntegrationTests
    {
        const string IdleSha256 = "68947a7143047ed76221ef7038726fcc9fd3e9b8403727e697f6aee2e7cfb761";
        static readonly string[] AttackKeys =
        {
            DarklightLuciusCombatProfile.SwordHorizontal,
            DarklightLuciusCombatProfile.SwordVertical,
            DarklightLuciusCombatProfile.SwordThrust
        };

        [TearDown]
        public void ClearSpriteBank() => DarklightLuciusSpriteBank.ClearCache();

        static DarklightLuciusSpriteCatalog LoadNormalCatalog()
        {
            var text = Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root + "catalog");
            Assert.That(text, Is.Not.Null, "Normal Lucius catalog must be available to EditMode tests");
            var catalog = JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(catalog), Is.True);
            return catalog;
        }

        static DarklightLuciusSpritePage ExactPage(string action, int direction, string attackKey = null)
        {
            Assert.That(DarklightLuciusSpriteBank.TryActionPage(
                DarklightLuciusAppearance.Normal, action, direction, attackKey, out var page), Is.True,
                $"Missing exact {action} page for direction {direction}, key {attackKey ?? "(none)"}");
            Assert.That(page, Is.Not.Null);
            Assert.That(page.action, Is.EqualTo(action));
            Assert.That(page.direction, Is.EqualTo(direction));
            Assert.That(page.attackKey, Is.EqualTo(attackKey));
            return page;
        }

        static void AssertFramesResolveWithoutFallback(DarklightLuciusSpritePage page, string action, string attackKey = null)
        {
            Assert.That(page.spriteNames, Has.Length.GreaterThanOrEqualTo(action == "Walk" ? 4 : 3));
            Assert.That(page.durationMs, Has.Length.EqualTo(page.spriteNames.Length));
            Assert.That(page.spriteNames.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(page.spriteNames.Length),
                "A cycle cannot repeat one pose as multiple frames");

            int elapsedMs = 0;
            for (int i = 0; i < page.spriteNames.Length; i++)
            {
                float time = (elapsedMs + page.durationMs[i] * .5f) / 1000f;
                Assert.That(DarklightLuciusSpriteBank.TryPose(
                    DarklightLuciusAppearance.Normal, action, page.direction, time,
                    out var sprite, out bool fallback, attackKey), Is.True,
                    $"Could not present {action} direction {page.direction}, frame {i}");
                Assert.That(fallback, Is.False, "An authored action frame must not silently show Idle");
                Assert.That(sprite, Is.Not.Null);
                Assert.That(sprite.name, Is.EqualTo(page.spriteNames[i]));
                elapsedMs += page.durationMs[i];
            }
        }

        static string Sha256(string path)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "").ToLowerInvariant();
        }

        [Test]
        public void NormalIdleSourceAndRuntimeSheetRemainByteIdentical()
        {
            const string runtime = "Assets/Darklight/Resources/Darklight/LuciusNormal20261109/LuciusNormalIdle8V1.png";
            const string source = "ArtSources/LuciusNormal20261109/selected/LuciusNormalIdle8Final-source.png";
            Assert.That(Sha256(runtime), Is.EqualTo(IdleSha256));
            Assert.That(Sha256(source), Is.EqualTo(IdleSha256));
            Assert.That(Sha256(runtime), Is.EqualTo(Sha256(source)));
        }

        [Test]
        public void IntegratedWalkCyclesHaveFourDistinctFramesAndNoIdleReuse()
        {
            var catalog = LoadNormalCatalog();
            var idleNames = catalog.pages.Where(p => p.action == "Idle")
                .SelectMany(p => p.spriteNames).ToHashSet(StringComparer.Ordinal);
            var walks=catalog.pages.Where(p => p.action == "Walk").ToArray();
            Assert.That(walks.Length,Is.GreaterThan(0));

            foreach(var integrated in walks)
            {
                int direction=integrated.direction;
                var page = ExactPage("Walk", direction);
                Assert.That(page.spriteNames.Intersect(idleNames, StringComparer.Ordinal), Is.Empty,
                    "Walking must use authored frames instead of cycling static Idle poses");
                AssertFramesResolveWithoutFallback(page, "Walk");
            }
        }

        [Test]
        public void IntegratedSwordAttacksHaveDistinctThreeFrameCyclesWithoutIdleReuse()
        {
            var catalog = LoadNormalCatalog();
            var idleNames = catalog.pages.Where(p => p.action == "Idle")
                .SelectMany(p => p.spriteNames).ToHashSet(StringComparer.Ordinal);
            var attacks=catalog.pages.Where(p=>p.action=="Attack").ToArray();
            Assert.That(attacks.Length,Is.GreaterThan(0));

            foreach(var integrated in attacks)
            {
                string key=integrated.attackKey;int direction=integrated.direction;
                Assert.That(AttackKeys,Does.Contain(key));
                var page = ExactPage("Attack", direction, key);
                Assert.That(page.spriteNames.Intersect(idleNames, StringComparer.Ordinal), Is.Empty,
                    "Attack frames cannot be aliases of static Idle poses");
                AssertFramesResolveWithoutFallback(page, "Attack", key);
            }
        }

        [Test]
        public void UnknownAttackKeyDoesNotResolveToAnotherSwordTechnique()
        {
            foreach (int direction in Enumerable.Range(0, 8))
                Assert.That(DarklightLuciusSpriteBank.TryActionPage(
                    DarklightLuciusAppearance.Normal, "Attack", direction, "UnknownSwordAction", out _), Is.False);
        }

        [Test]
        public void AttackClockMapsGameplayPhasesToAuthoredStartupActiveAndRecoveryFrames()
        {
            var page = new DarklightLuciusSpritePage
            {
                action = "Attack",
                startupFrames = 2,
                activeFrames = 2,
                durationMs = new[] { 80, 120, 70, 130, 150, 250 }
            };
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                attack.startup = .2f;
                attack.active = .1f;
                attack.recovery = .3f;
                Assert.That(DarklightLuciusSpriteBank.AttackTime(page, attack, .1f), Is.EqualTo(.1f).Within(.0001f),
                    "Startup midpoint must map to the midpoint of authored startup frames");
                Assert.That(DarklightLuciusSpriteBank.AttackTime(page, attack, .25f), Is.EqualTo(.3f).Within(.0001f),
                    "Active midpoint must map after the startup frame span");
                Assert.That(DarklightLuciusSpriteBank.AttackTime(page, attack, .45f), Is.EqualTo(.6f).Within(.0001f),
                    "Recovery midpoint must map after startup and active frame spans");
            }
            finally { UnityEngine.Object.DestroyImmediate(attack); }
        }

        [Test]
        public void WalkPresentationFacesTravelAndAdvancesByDistanceRatherThanModelClock()
        {
            var available=LoadNormalCatalog().pages.First(p=>p.action=="Walk");
            var page = ExactPage("Walk", available.direction);
            var actor = new GameObject("LuciusWalkDistanceIntegration");
            try
            {
                var view = actor.AddComponent<DarklightLuciusView>();
                var model = new CombatModel(new CombatLoadout());
                float angle=page.direction*45*Mathf.Deg2Rad;
                var travel = new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));
                model.Tick(5f,new CombatInput{aim=-travel}); // Idle clock and aim must not select a wrong walk frame.
                Assert.That(view.Present(model, travel), Is.True);
                Assert.That(view.UsesIdleFallback, Is.False);
                Assert.That(view.Renderer.sprite.name, Is.EqualTo(page.spriteNames[0]));

                float elapsed = (page.durationMs[0] + page.durationMs[1] * .5f) / 1000f;
                actor.transform.position += (Vector3)(travel * (elapsed * 2.3f));
                Assert.That(view.Present(model, travel), Is.True);
                Assert.That(view.UsesIdleFallback, Is.False);
                Assert.That(view.Renderer.sprite.name, Is.EqualTo(page.spriteNames[1]),
                    "Travel of distance = 2.3 units/s × elapsed must advance the Walk frame");
            }
            finally { UnityEngine.Object.DestroyImmediate(actor); }
        }
    }
}
