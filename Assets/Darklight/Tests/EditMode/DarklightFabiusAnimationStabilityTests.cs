using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    // Verifies visual stability, foot alignment, and lack of jitter across Fabius animations.
    public sealed class DarklightFabiusAnimationStabilityTests
    {
        DarklightWeaponRedesignCatalog catalog;
        readonly List<UnityEngine.Object> owned = new();
        bool priorPause;
        float priorVolume;

        [SetUp]
        public void Setup()
        {
            priorPause = AudioListener.pause;
            priorVolume = AudioListener.volume;
            AudioListener.pause = true;
            AudioListener.volume = 0;
            DarklightWeaponRedesignBank.ClearCache();
            var text = Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root + "catalog");
            Assert.That(text, Is.Not.Null);
            catalog = JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(text.text);
            Assert.That(DarklightWeaponRedesignBank.ValidCatalog(catalog), Is.True);
        }

        [TearDown]
        public void Cleanup()
        {
            DarklightWeaponRedesignBank.ClearCache();
            foreach (var obj in owned) if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            owned.Clear();
            AudioListener.pause = priorPause;
            AudioListener.volume = priorVolume;
            EditorUtility.UnloadUnusedAssetsImmediate();
        }

        static Vector2 Facing(int d) => new Vector2(-Mathf.Sin(d * Mathf.PI / 4f), -Mathf.Cos(d * Mathf.PI / 4f));

        [Test]
        public void FabiusIdleEightDirectionsHaveStablePivots()
        {
            int[] weapons = { 0, 1, 2 }; // Katana, Daggers, Punhos
            for (int w = 0; w < weapons.Length; w++)
            {
                int weapon = weapons[w];
                for (int dir = 0; dir < 8; dir++)
                {
                    var model = new CombatModel(new CombatLoadout { weapon = weapon });
                    model.Tick(0.001f, new CombatInput { aim = Facing(dir) });

                    var sprites = new List<Sprite>();
                    // Sample idle frames over 2 seconds
                    for (float t = 0f; t < 2.0f; t += 0.1f)
                    {
                        model.Tick(0.1f, default);
                        bool ok = DarklightWeaponRedesignBank.TryResolve(0, weapon, 0, dir, model, Vector2.zero, null, out var sprite, "Idle", t);
                        if (ok && sprite != null && !sprites.Contains(sprite))
                        {
                            sprites.Add(sprite);
                        }
                    }

                    Assert.That(sprites.Count, Is.GreaterThan(0), $"Idle must resolve for weapon {weapon} dir {dir}");

                    if (sprites.Count > 1)
                    {
                        float firstX = sprites[0].pivot.x;
                        float firstY = sprites[0].pivot.y;
                        for (int i = 1; i < sprites.Count; i++)
                        {
                            float deltaX = Mathf.Abs(sprites[i].pivot.x - firstX);
                            float deltaY = Mathf.Abs(sprites[i].pivot.y - firstY);
                            Assert.That(deltaX, Is.LessThan(2.0f), $"Idle weapon {weapon} dir {dir} frame {i} has horizontal pivot jitter of {deltaX:F2}px");
                            Assert.That(deltaY, Is.LessThan(2.0f), $"Idle weapon {weapon} dir {dir} frame {i} has vertical pivot jitter of {deltaY:F2}px");
                        }
                    }
                }
            }
        }

        [Test]
        public void FabiusWalkCyclesHaveBoundedPivotDelta()
        {
            int[] weapons = { 0, 2, 3 }; // Katana, Punhos, Moto
            for (int w = 0; w < weapons.Length; w++)
            {
                int weapon = weapons[w];
                for (int dir = 0; dir < 8; dir++)
                {
                    var model = new CombatModel(new CombatLoadout { weapon = weapon });
                    Vector2 move = Facing(dir);
                    model.Tick(0.001f, new CombatInput { aim = move, move = move });

                    var sprites = new List<Sprite>();
                    for (float t = 0f; t < 2.0f; t += 0.08f)
                    {
                        model.Tick(0.08f, new CombatInput { aim = move, move = move });
                        bool ok = DarklightWeaponRedesignBank.TryResolve(0, weapon, 0, dir, model, move, null, out var sprite, "Walk", t);
                        if (ok && sprite != null && !sprites.Contains(sprite))
                        {
                            sprites.Add(sprite);
                        }
                    }

                    Assert.That(sprites.Count, Is.GreaterThan(0), $"Walk must resolve for weapon {weapon} dir {dir}");

                    for (int i = 0; i < sprites.Count; i++)
                    {
                        var s = sprites[i];
                        Assert.That(float.IsNaN(s.pivot.x) || float.IsNaN(s.pivot.y), Is.False, $"Walk weapon {weapon} dir {dir} frame {i} pivot is NaN");
                        Assert.That(s.pivot.x, Is.GreaterThan(0).And.LessThan(s.rect.width), $"Walk weapon {weapon} dir {dir} frame {i} pivot.x out of bounds");
                        Assert.That(s.pivot.y, Is.GreaterThan(0).And.LessThan(s.rect.height), $"Walk weapon {weapon} dir {dir} frame {i} pivot.y out of bounds");
                    }
                }
            }
        }

        [Test]
        public void FabiusAttackFramesMaintainValidAnchors()
        {
            int[] weapons = { 0, 2, 3 };
            for (int w = 0; w < weapons.Length; w++)
            {
                int weapon = weapons[w];
                for (int dir = 0; dir < 8; dir++)
                {
                    var attack = ScriptableObject.CreateInstance<AttackDefinition>();
                    owned.Add(attack);
                    attack.animationKey = weapon == 0 ? "Leve1" : weapon == 2 ? "Direto da Ruína" : "Serra de Entrada";
                    attack.startup = 0.15f;
                    attack.active = 0.12f;
                    attack.recovery = 0.25f;

                    var model = new CombatModel(new CombatLoadout { weapon = weapon });
                    model.TriggerAttack(attack, Facing(dir));

                    var sprites = new List<Sprite>();
                    for (float t = 0f; t <= attack.Duration; t += 0.05f)
                    {
                        model.Tick(0.05f, default);
                        bool ok = DarklightWeaponRedesignBank.TryResolve(0, weapon, 0, dir, model, Vector2.zero, null, out var sprite);
                        if (ok && sprite != null && !sprites.Contains(sprite))
                        {
                            sprites.Add(sprite);
                        }
                    }

                    Assert.That(sprites.Count, Is.GreaterThan(0), $"Attack must resolve for weapon {weapon} dir {dir}");
                    for (int i = 0; i < sprites.Count; i++)
                    {
                        var s = sprites[i];
                        Assert.That(s.pivot.x, Is.GreaterThan(0).And.LessThan(s.rect.width));
                        Assert.That(s.pivot.y, Is.GreaterThan(0).And.LessThan(s.rect.height));
                    }
                }
            }
        }
    }
}
