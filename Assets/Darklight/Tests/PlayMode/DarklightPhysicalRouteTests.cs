using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightPhysicalRouteTests
    {
        [UnityTest] public IEnumerator EveryCampaignObjectiveCanBeTraversedWithPlayerRadiusInBothDirections()
        {
            DarklightAutomation.Silent=true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            var path=new List<Vector2>();
            for(int map=0;map<DarklightStory.Chapters.Length;map++)
            {
                game.PreviewChapter(map);yield return null;yield return null;
                lab.SetPaused(true);Physics2D.SyncTransforms();
                Assert.That(game.Map.Art,Is.Not.Null,"Missing authored map "+map);
                var body=lab.Player.GetComponent<Rigidbody2D>();
                Vector2 spawn=game.Map.SafeSpawn(game.Map.Spawn);
                var targets=new List<Vector2>(game.Map.Arenas){game.Map.Ally,game.Map.Relic,game.Map.Portal};
                targets.AddRange(game.Map.Checkpoints);
                if(game.Map.Art.areas!=null)foreach(var area in game.Map.Art.areas)targets.Add(game.Map.Art.World(area.point));
                foreach(var target in targets)
                {
                    Vector2 end=game.Map.SafeSpawn(target);
                    Assert.That(Vector2.Distance(end,target),Is.LessThan(1.7f),"Objective out of interaction reach: map "+map+" target "+target);
                    for(int direction=0;direction<2;direction++)
                    {
                        Vector2 from=direction==0?spawn:end,to=direction==0?end:spawn;
                        Assert.That(DarklightNavigation.Instance.FindPath(from,to,path),Is.True,"No route: map "+map+" direction "+direction+" target "+target);
                        body.position=from;Physics2D.SyncTransforms();
                        foreach(var waypoint in path)
                        {
                            for(int step=0;step<250&&Vector2.Distance(body.position,waypoint)>.01f;step++)
                            {
                                DarklightNavigation.SafeStep(body,Vector2.ClampMagnitude(waypoint-body.position,.12f),.3f);
                                Physics2D.SyncTransforms();
                            }
                            Assert.That(Vector2.Distance(body.position,waypoint),Is.LessThan(.015f),"Blocked physical segment: map "+map+" direction "+direction+" target "+target+" position "+game.Map.Art.Pixel(body.position)+" waypoint "+game.Map.Art.Pixel(waypoint));
                        }
                        Assert.That(Vector2.Distance(body.position,to),Is.LessThan(.015f));
                    }
                    yield return null;
                }
                lab.SetPaused(false);
            }
        }
    }
}
