using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightNewRegionTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        [UnitySetUp] public IEnumerator SetUp(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;}
        [UnityTest] public IEnumerator FourRegionsHavePhysicalRoutesAndPreserveCampaign()
        {
            var original=JsonUtility.ToJson(game.Progress);var path=new List<Vector2>();
            string saved=System.IO.File.Exists(DarklightProgress.Path)?System.IO.File.ReadAllText(DarklightProgress.Path):null;
            for(int n=6;n<10;n++)
            {
                game.ExploreRegion(n);yield return null;yield return null;Physics2D.SyncTransforms();
                Assert.That(game.ExploringNewRegion,Is.True);Assert.That(game.Map.Art.areas.Length,Is.EqualTo(4));
                foreach(var point in new[]{game.Map.Spawn,game.Map.Ally,game.Map.Relic,game.Map.Portal})CheckRoute(point,path,n);
                foreach(var area in game.Map.Art.areas){Assert.That(area.description.Length,Is.GreaterThan(70));CheckRoute(game.Map.Art.World(area.point),path,n);}
                Assert.That(JsonUtility.ToJson(game.Progress),Is.EqualTo(original),"Exploration changed campaign progress");
                Assert.That(lab.WorldCamera.orthographicSize,Is.EqualTo(DarklightLabCamera.GameplayHalfHeight).Within(.01f));
                var body=lab.Player.GetComponent<Rigidbody2D>();
                foreach(var direction in new[]{Vector2.left,Vector2.right,Vector2.up,Vector2.down})
                {
                    body.position=game.Map.SafeSpawn(game.Map.Spawn);Physics2D.SyncTransforms();DarklightNavigation.SafeStep(body,direction*100);Physics2D.SyncTransforms();
                    Assert.That(game.Map.Bounds.Contains(body.position),Is.True,"Knockback left region "+n);
                    foreach(var c in Physics2D.OverlapCircleAll(body.position,.28f))Assert.That(c.name,Is.Not.EqualTo("LimiteDoCenario"),"Knockback entered wall "+n);
                }
                game.PreviewPanel("Pause");yield return null;
                var label=lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel/Botao1/Rotulo").GetComponent<UnityEngine.UI.Text>();Assert.That(label.text,Is.EqualTo("ESCOLHER REGIÃO"));
            }
            Assert.That(System.IO.File.Exists(DarklightProgress.Path)?System.IO.File.ReadAllText(DarklightProgress.Path):null,Is.EqualTo(saved));
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
        void CheckRoute(Vector2 point,List<Vector2> path,int region)
        {
            Vector2 safe=game.Map.SafeSpawn(point);
            Assert.That(Vector2.Distance(safe,point),Is.LessThan(1.7f),"Interaction unreachable "+region+" "+point);
            Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,safe,path),Is.True,"No physical path "+region+" "+point);
            foreach(var waypoint in path)foreach(var c in Physics2D.OverlapCircleAll(waypoint,.27f))Assert.That(c.name,Is.Not.EqualTo("LimiteDoCenario"),"Path crossed wall "+region+" "+waypoint);
        }
    }
}


