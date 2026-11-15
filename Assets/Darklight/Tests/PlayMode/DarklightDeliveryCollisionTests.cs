using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    // Geometry-only checks: never instantiate the campaign, preferences or saves.
    public sealed class DarklightDeliveryCollisionTests
    {
        readonly List<GameObject> owned = new();
        bool silent, paused;
        float volume;
        SimulationMode2D simulation;
        [SetUp] public void Prepare()
        {
            Assert.That(DarklightCampaign.Instance, Is.Null);
            silent=DarklightAutomation.Silent;paused=AudioListener.pause;volume=AudioListener.volume;
            simulation=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
        }
        [TearDown] public void Finish()
        {
            foreach(var item in owned)if(item!=null)Object.DestroyImmediate(item);
            owned.Clear();Physics2D.SyncTransforms();Physics2D.simulationMode=simulation;
            DarklightAutomation.Silent=silent;AudioListener.pause=paused;AudioListener.volume=volume;
        }
        GameObject Create(string name){var o=new GameObject(name);owned.Add(o);return o;}
        [UnityTest] public IEnumerator InitialWallOverlapSeparatesTowardTheFreeSide()
        {
            var wall=Create("Wall");wall.transform.position=new Vector2(1.5f,0);
            wall.AddComponent<BoxCollider2D>().size=new Vector2(1,4);
            var actor=Create("Actor");var body=actor.AddComponent<Rigidbody2D>();
            body.bodyType=RigidbodyType2D.Kinematic;actor.AddComponent<CircleCollider2D>().radius=.3f;
            body.position=new Vector2(.9f,0);Physics2D.SyncTransforms();
            DarklightNavigation.ResolvePenetration(body);Physics2D.SyncTransforms();
            Assert.That(body.position.x,Is.InRange(.68f,.701f),"Resolve the overlap without pushing through the wall.");
            var before=body.position;DarklightNavigation.SafeStep(body,new Vector2(-.2f,0));
            Assert.That(body.position.x,Is.LessThan(before.x-.19f),"The escape direction must stay available.");
            yield return null;
        }
        [UnityTest] public IEnumerator AllTenMapsHaveTraversableObjectiveRoutesWithTheActualFeetCapsule()
        {
            var build=typeof(DarklightPaintedMap).GetMethod("BuildGeometry",BindingFlags.Static|BindingFlags.NonPublic);
            Assert.That(build,Is.Not.Null);var path=new List<Vector2>();
            for(int chapter=0;chapter<10;chapter++)
            {
                var root=Create("Geometry"+chapter);var map=DarklightMapDefinition.ForChapter(chapter);
                build.Invoke(null,new object[]{root.transform,map});Physics2D.SyncTransforms();
                Assert.That(root.GetComponentInChildren<CompositeCollider2D>(),Is.Not.Null);
                var actor=Create("Feet"+chapter);var body=actor.AddComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;
                var feet=actor.AddComponent<CapsuleCollider2D>();feet.size=new Vector2(.42f,.28f);feet.offset=new Vector2(0,.14f);
                var targets=new List<Vector2>(map.Arenas){map.Ally,map.Relic,map.Portal};targets.AddRange(map.Checkpoints);
                if(map.Art.areas!=null)foreach(var area in map.Art.areas)targets.Add(map.Art.World(area.point));
                foreach(var target in targets)for(int direction=0;direction<2;direction++)
                {
                    feet.enabled=false;Physics2D.SyncTransforms();
                    var spawn=map.SafeSpawn(map.Spawn);var end=map.SafeSpawn(target);
                    var from=direction==0?spawn:end;var to=direction==0?end:spawn;
                    Assert.That(DarklightNavigation.Instance.FindPath(from,to,path),Is.True,$"No route: chapter {chapter}, target {target}");
                    body.position=from;feet.enabled=true;Physics2D.SyncTransforms();
                    foreach(var waypoint in path)
                    {
                        for(int step=0;step<250&&Vector2.Distance(body.position,waypoint)>.01f;step++)
                        {DarklightNavigation.SafeStep(body,Vector2.ClampMagnitude(waypoint-body.position,.12f));Physics2D.SyncTransforms();}
                        Assert.That(Vector2.Distance(body.position,waypoint),Is.LessThan(.015f),$"Blocked: chapter {chapter}, target {target}, position {body.position}, waypoint {waypoint}");
                    }
                    Assert.That(Vector2.Distance(body.position,to),Is.LessThan(.015f));
                }
                Object.DestroyImmediate(actor);Object.DestroyImmediate(root);Physics2D.SyncTransforms();yield return null;
            }
        }
    }
}
