using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Darklight.Tests
{
    public sealed class DarklightMap1ReviewTests
    {
        DarklightCampaign game;
        DarklightCombatLab lab;

        static void Call(object o, string method, params object[] args) =>
            o.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(o, args);

        [UnitySetUp]
        public IEnumerator Setup()
        {
            DarklightAutomation.Silent = true;
            AudioListener.volume = 0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            yield return null;
            yield return null;
            game = DarklightCampaign.Instance;
            lab = DarklightCombatLab.Instance;
        }

        void Move(Vector2 point)
        {
            lab.Player.ResetModel();
            var b = lab.Player.GetComponent<Rigidbody2D>();
            b.position = point;
            lab.Player.transform.position = point;
            Physics2D.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator Map1Layer1_PlantaBaixaWalkmeshIsUnobstructed()
        {
            game.PreviewChapter(0);
            yield return null;
            yield return null;

            Assert.That(game.Map, Is.Not.Null);
            Assert.That(game.Map.Art, Is.Not.Null);
            Assert.That(game.Map.Art.width, Is.EqualTo(1536));
            Assert.That(game.Map.Art.height, Is.EqualTo(1024));
            Assert.That(game.Map.Art.pixelsPerUnit, Is.EqualTo(24f).Within(0.01f));
            Assert.That(game.Map.Art.walk.Length, Is.EqualTo(15), "Map 1 must have 15 canonical walk shapes");

            var ground = GameObject.Find("MapaIlustrado");
            Assert.That(ground, Is.Not.Null);
            var sr = ground.GetComponent<SpriteRenderer>();
            Assert.That(sr.sortingOrder, Is.EqualTo(-200));

            Vector2 spawn = game.Map.SafeSpawn(game.Map.Spawn);
            Assert.That(game.Map.Art.Contains(spawn), Is.True, "Spawn must be in clean walkmesh");
        }

        [UnityTest]
        public IEnumerator Map1Layer2_CasasFachadasOclusaoEZSorting()
        {
            game.PreviewChapter(0);
            yield return null;
            yield return null;

            Assert.That(game.Map.Art.foreground.Length, Is.EqualTo(9), "Map 1 must have 9 foreground structures");
            Assert.That(game.Map.Art.blocked.Length, Is.EqualTo(11), "Map 1 must have 11 blocked obstacle shapes");

            foreach (var fg in game.Map.Art.foreground)
            {
                var obj = GameObject.Find(fg.name);
                Assert.That(obj, Is.Not.Null, "Missing foreground object: " + fg.name);
                var fade = obj.GetComponent<DarklightMapOcclusion>();
                Assert.That(fade, Is.Not.Null, "Foreground object must have DarklightMapOcclusion: " + fg.name);

                var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
                Assert.That(renderers.Length, Is.GreaterThan(0));
                int expectedOrder = DarklightSorting.OrderForY(game.Map.Art.World(fg.foot).y);
                foreach (var r in renderers)
                    Assert.That(r.sortingOrder, Is.EqualTo(expectedOrder), "Incorrect Z-sorting on " + fg.name);
            }
        }

        [UnityTest]
        public IEnumerator Map1Layer3_InteractivePointsSafetyMargins()
        {
            game.PreviewChapter(0);
            yield return null;
            yield return null;

            var points = new Dictionary<string, Vector2>
            {
                { "Spawn", game.Map.Spawn },
                { "Ally", game.Map.Ally },
                { "Relic", game.Map.Relic },
                { "Portal", game.Map.Portal }
            };
            for (int i = 0; i < game.Map.Checkpoints.Length; i++)
                points.Add("Checkpoint" + i, game.Map.Checkpoints[i]);

            foreach (var pair in points)
            {
                Vector2 target = pair.Value;
                Vector2 safe = game.Map.SafeSpawn(target);
                Assert.That(Vector2.Distance(target, safe), Is.LessThan(0.8f), "Interactive point too far from clear floor: " + pair.Key);

                // Ensure at least 0.6 world units distance from solid scenery colliders
                foreach (var hit in Physics2D.OverlapCircleAll(safe, 0.6f))
                {
                    if (hit.name == "LimiteDoCenario" && !hit.isTrigger)
                        Assert.Fail("Interactive point " + pair.Key + " collides or overlaps with scenery wall: " + hit.name);
                }
            }

            Assert.That(game.AllyName, Is.EqualTo("LYRA MOREAU"), "Map 1 canonical ally must be Lyra Moreau");
        }

        [UnityTest]
        public IEnumerator Map1Layer4_ArenasAndBossKaelVossEncounter()
        {
            game.PreviewChapter(0);
            yield return null;
            yield return null;

            Assert.That(game.Map.Arenas.Length, Is.EqualTo(4), "Map 1 must have 4 arenas");

            for (int i = 0; i < 4; i++)
            {
                Vector2 arena = game.Map.Arenas[i];
                Assert.That(game.Map.InArena(i, arena), Is.True, "Arena center must be inside arena bounds: " + i);
            }

            Assert.That(DarklightStory.Bosses[0], Does.Contain("Kael Voss").IgnoreCase.Or.EqualTo("Kael Voss"), "Boss 1 must be Kael Voss");
        }

        [UnityTest]
        public IEnumerator Map1PhysicalCapsuleRoutesBothDirections()
        {
            game.PreviewChapter(0);
            yield return null;
            yield return null;
            lab.SetPaused(true);
            Physics2D.SyncTransforms();

            var body = lab.Player.GetComponent<Rigidbody2D>();
            var path = new List<Vector2>();
            Vector2 spawn = game.Map.SafeSpawn(game.Map.Spawn);

            var targets = new List<Vector2>(game.Map.Arenas)
            {
                game.Map.Ally,
                game.Map.Relic,
                game.Map.Portal
            };
            targets.AddRange(game.Map.Checkpoints);

            int routes = 0, steps = 0;
            foreach (var target in targets)
            {
                var end = game.Map.SafeSpawn(target);
                Assert.That(Vector2.Distance(target, end), Is.LessThan(0.8f), "Target intersects scenery: " + target);

                for (int direction = 0; direction < 2; direction++)
                {
                    var from = direction == 0 ? spawn : end;
                    var to = direction == 0 ? end : spawn;
                    Assert.That(DarklightNavigation.Instance.FindPath(from, to, path), Is.True, "No path between " + from + " and " + to);

                    Move(from);
                    foreach (var waypoint in path)
                    {
                        for (int i = 0; i < 250 && Vector2.Distance(body.position, waypoint) > 0.01f; i++)
                        {
                            DarklightNavigation.SafeStep(body, Vector2.ClampMagnitude(waypoint - body.position, 0.12f), 0.3f);
                            Physics2D.SyncTransforms();
                            steps++;
                        }
                        Assert.That(Vector2.Distance(body.position, waypoint), Is.LessThan(0.015f),
                            "Capsule blocked at " + game.Map.Art.Pixel(body.position) + " toward " + game.Map.Art.Pixel(waypoint));
                    }
                    Assert.That(Vector2.Distance(body.position, to), Is.LessThan(0.015f));
                    routes++;
                }
                yield return null;
            }

            lab.SetPaused(false);
            Assert.That(routes, Is.EqualTo(targets.Count * 2));
            Assert.That(steps, Is.GreaterThan(0));
        }
    }
}
