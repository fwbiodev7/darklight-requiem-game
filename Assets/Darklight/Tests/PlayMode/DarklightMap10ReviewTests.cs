using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed class DarklightMap10ReviewTests
    {
        static string Folder => DarklightTestOutput.GetDirectory("mapas-reforma-20261016/map10");
        DarklightCampaign game;DarklightCombatLab lab;
        static void Call(object o,string method,params object[] args)=>o.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        [UnitySetUp]public IEnumerator Setup(){DarklightAutomation.Silent=true;AudioListener.volume=0;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;}
        void Move(Vector2 point){lab.Player.ResetModel();var b=lab.Player.GetComponent<Rigidbody2D>();b.position=point;lab.Player.transform.position=point;Physics2D.SyncTransforms();}
        [UnityTest]public IEnumerator CaptureFourOriginalMaps()
        {
            for(int n=6;n<10;n++){game.PreviewChapter(n);yield return null;yield return null;yield return CaptureAreas(n,"before");}
        }
        IEnumerator CaptureAreas(int n,string phase)
        {
            lab.SetPaused(true);var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;
            var points=new List<Vector2>{game.Map.Spawn};points.AddRange(game.Map.Art.areas.Select(a=>game.Map.Art.World(a.point)));
            for(int i=0;i<points.Count;i++)
            {
                Vector2 p=game.Map.SafeSpawn(points[i]);Move(p);lab.WorldCamera.transform.position=(Vector3)p+new Vector3(0,.75f,-10);yield return null;Canvas.ForceUpdateCanvases();Capture(Folder+phase+"/map"+(n+1)+"-area"+i+".png");
            }
            follow.enabled=true;lab.SetPaused(false);
        }
        [UnityTest]public IEnumerator Map10PhysicalCapsuleRoutesBothDirectionsAndOldCheckpoints()
        {
            game.PreviewChapter(9);yield return null;yield return null;lab.SetPaused(true);Physics2D.SyncTransforms();
            var body=lab.Player.GetComponent<Rigidbody2D>();var path=new List<Vector2>();var spawn=game.Map.SafeSpawn(game.Map.Spawn);
            var targets=new List<Vector2>(game.Map.Arenas){game.Map.Ally,game.Map.Relic,game.Map.Portal};targets.AddRange(game.Map.Checkpoints);targets.AddRange(game.Map.Art.areas.Select(a=>game.Map.Art.World(a.point)));
            foreach(var p in new[]{new Vector2(565,480),new Vector2(574,625),new Vector2(216,500),new Vector2(390,775),new Vector2(1000,834),new Vector2(1308,480),new Vector2(1050,170),new Vector2(430,150),new Vector2(1110,425),new Vector2(660,720)})targets.Add(game.Map.Art.World(p));
            int steps=0,routes=0;
            foreach(var target in targets)
            {
                var end=game.Map.SafeSpawn(target);Assert.That(Vector2.Distance(target,end),Is.LessThan(.8f),"New anchor intersects scenery: "+target);
                for(int direction=0;direction<2;direction++)
                {
                    var from=direction==0?spawn:end;var to=direction==0?end:spawn;
                    Assert.That(DarklightNavigation.Instance.FindPath(from,to,path),Is.True,"No path to "+target);
                    Move(from);
                    foreach(var waypoint in path)
                    {
                        for(int i=0;i<250&&Vector2.Distance(body.position,waypoint)>.01f;i++){DarklightNavigation.SafeStep(body,Vector2.ClampMagnitude(waypoint-body.position,.12f),.3f);Physics2D.SyncTransforms();steps++;}
                        Assert.That(Vector2.Distance(body.position,waypoint),Is.LessThan(.015f),"Capsule blocked at "+game.Map.Art.Pixel(body.position)+" toward "+game.Map.Art.Pixel(waypoint));
                    }
                    Assert.That(Vector2.Distance(body.position,to),Is.LessThan(.015f));routes++;
                }
                yield return null;
            }
            for(int checkpoint=0;checkpoint<=4;checkpoint++)
            {
                game.Progress.checkpoint=checkpoint;Call(game,"LoadMap",9);yield return null;Physics2D.SyncTransforms();
                Assert.That(game.Map.Art.Contains(body.position),Is.True);Assert.That(game.Progress.checkpoint,Is.EqualTo(checkpoint));
                foreach(var collider in Physics2D.OverlapCircleAll(body.position,.28f))Assert.That(collider.name,Is.Not.EqualTo("LimiteDoCenario"));
            }
            File.WriteAllText(Folder+"map10-physical-metrics.json","{\"routes\":"+routes+",\"capsuleSteps\":"+steps+",\"checkpoints\":5,\"radius\":0.3}");
        }
        [UnityTest]public IEnumerator Map10NativeDetailsAndRealCameraCaptures()
        {
            game.PreviewChapter(9);yield return null;yield return null;
            var details=Object.FindFirstObjectByType<DarklightMapDetails>();Assert.That(details,Is.Not.Null);Assert.That(details.Data.chunks.Length,Is.EqualTo(4));
            foreach(var c in details.Data.chunks){var s=Resources.Load<Sprite>(c.asset);Assert.That(s.rect.size,Is.EqualTo(new Vector2(1536,1024)));Assert.That(s.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(s.texture.mipmapCount,Is.EqualTo(1));}
            yield return CaptureAreas(9,"after");
            var views=new[]{new Vector2(565,480),new Vector2(574,625),new Vector2(216,500)};
            for(int i=0;i<views.Length;i++){var p=game.Map.SafeSpawn(game.Map.Art.World(views[i]));Move(p);lab.WorldCamera.transform.position=(Vector3)p+new Vector3(0,.75f,-10);yield return null;Capture(Folder+"after/map10-extra"+i+".png");}
        }
        [UnityTest]public IEnumerator Map10NoctharThreePhasesPauseAndFinalPortal()
        {
            game.PreviewChapter(9);game.Progress.checkpoint=3;Call(game,"LoadMap",9);Call(game,"Close");Move(game.Map.SafeSpawn(game.Map.Arenas[3]+Vector2.down));yield return null;yield return null;
            for(int i=0;i<8&&!game.Active;i++){Call(game,"NextDialogue");yield return null;}
            Assert.That(game.MapEnemies.Count(e=>e.Boss),Is.EqualTo(1));var boss=game.CurrentBoss;Assert.That(boss.kind,Is.EqualTo(RequiemEnemyKind.Nocthar));
            boss.Health.TakeDamage(boss.Health.MaxHealth*.4f);yield return new WaitForSeconds(.06f);Assert.That(boss.Phase,Is.EqualTo(2));Assert.That(boss.AcceptDamage,Is.False);
            game.PreviewPanel("Pause");float clock=lab.Player.Model.Clock;yield return new WaitForSeconds(.05f);Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));Call(game,"Close");yield return new WaitForSeconds(.95f);
            boss.Health.TakeDamage(boss.Health.MaxHealth*.35f);yield return new WaitForSeconds(.06f);Assert.That(boss.Phase,Is.EqualTo(3));Assert.That(game.Progress.FormUnlocked,Is.EqualTo(3));Call(game,"Close");yield return new WaitForSeconds(.95f);
            boss.Health.TakeDamage(99999);for(int i=0;i<20&&game.Progress.checkpoint!=4;i++)yield return null;Assert.That(game.Progress.checkpoint,Is.EqualTo(4));Call(game,"Close");
            Move(game.Map.SafeSpawn(game.Map.Portal));Assert.That(game.TryInteract(),Is.True);Assert.That(lab.Paused,Is.True);Assert.That(game.Progress.chapter,Is.EqualTo(9));Assert.That(game.Progress.completed,Is.False);
            File.WriteAllText(Folder+"finale-evidence.json","{\"phases\":3,\"checkpoint\":4,\"finalChoiceOpened\":true,\"endingNotForced\":true}");
        }
        [UnityTest]public IEnumerator Map10AtmosphereIsOptionalBoundedSilentAndPauses()
        {
            game.PreviewChapter(9);Call(game,"Close");yield return null;
            var effect=Object.FindFirstObjectByType<DarklightRegionAtmosphere>();Assert.That(effect,Is.Not.Null);
            string progress=JsonUtility.ToJson(game.Progress);Move(effect.Point);yield return null;yield return null;
            Assert.That(effect.Playing,Is.True);Assert.That(effect.Activations,Is.EqualTo(1));Assert.That(effect.Activate(),Is.False);
            // Isolate the optional event from enemy knockback; combat is tested separately.
            foreach(var e in game.MapEnemies){e.enabled=false;e.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;}
            game.PreviewPanel("Pause");float remaining=effect.Remaining;yield return new WaitForSecondsRealtime(.1f);Assert.That(effect.Remaining,Is.EqualTo(remaining));
            foreach(var light in effect.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>())Assert.That(light.enabled,Is.False);
            Call(game,"Close");yield return new WaitForSeconds(3.4f);Assert.That(effect.Playing,Is.False);
            Move(effect.Point);Assert.That(game.TryGetInteraction(out var target),Is.True);Assert.That(target.Id,Is.EqualTo("environment_echo"));Assert.That(game.TryInteract(),Is.True);
            Assert.That(effect.Activations,Is.EqualTo(2));Assert.That(JsonUtility.ToJson(game.Progress),Is.EqualTo(progress));
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
            Assert.That(new[]{6,7,8,9}.Select(DarklightRegionAtmosphere.Bed).Distinct().Count(),Is.EqualTo(4));
        }
        [UnityTest]public IEnumerator Map10OriginalWeaponsContactEightDirectionsInNewArena()
        {
            game.PreviewChapter(9);Call(game,"Close");game.Progress.weaponUnlocks=7;yield return null;lab.SetPaused(true);
            Vector2 center=game.Map.Art.World(new Vector2(773,140));Call(game,"Spawn",RequiemEnemyKind.Crawler,center,false);yield return null;
            var enemy=game.MapEnemies.Last();enemy.enabled=false;enemy.Health.SetMaxHealth(100000);enemy.Health.FullHeal();
            for(int weapon=0;weapon<3;weapon++)
            {
                lab.Player.ResetModel();game.Weapons.Request(weapon);game.Weapons.BeforeTick(.01f,true);
                for(int direction=0;direction<8;direction++)
                {
                    float angle=direction*Mathf.PI/4;Vector2 aim=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));Move(center);
                    lab.Player.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=aim});var attack=lab.Player.Model.Attack;
                    Vector2 e=center+aim*attack.range*.6f;enemy.transform.position=e;enemy.GetComponent<Rigidbody2D>().position=e;Physics2D.SyncTransforms();
                    Assert.That(game.Map.Art.Contains(e),Is.True,"Target outside authored arena");
                    lab.Player.Model.Tick(attack.startup+attack.active*.6f,new CombatInput{aim=aim});float hp=enemy.Health.CurrentHealth;
                    Call(lab.Player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.LessThan(hp),"Map10 contact failed weapon "+weapon+" dir "+direction);
                }
            }
            lab.SetPaused(false);
        }
        [UnityTest]public IEnumerator Map10MemoryAndThreeOriginalWeaponsRemainMandatory()
        {
            game.PreviewChapter(9);Call(game,"Close");game.Progress.weaponUnlocks=7;yield return null;
            Move(game.Map.SafeSpawn(game.Map.Arenas[0]));yield return null;yield return null;
            Assert.That(game.MapEnemies.Count(e=>!e.Health.IsDead),Is.GreaterThan(0));foreach(var e in game.MapEnemies.ToArray())e.Health.TakeDamage(99999);
            for(int i=0;i<20&&game.Progress.checkpoint==0;i++)yield return null;Assert.That(game.Progress.checkpoint,Is.EqualTo(1));
            Move(game.Map.SafeSpawn(game.Map.Arenas[1]));yield return null;Assert.That(game.TryGetInteraction(out var t),Is.True);Assert.That(t.Id,Is.EqualTo("narrative"));Assert.That(game.TryInteract(),Is.True);Assert.That(game.Progress.checkpoint,Is.EqualTo(2));
            for(int i=0;i<8&&!game.Active;i++){Call(game,"NextDialogue");yield return null;}
            Move(game.Map.SafeSpawn(game.Map.Arenas[2]));yield return null;yield return null;
            foreach(var e in game.MapEnemies.ToArray())e.Health.TakeDamage(99999);yield return null;yield return null;
            Assert.That(game.Progress.checkpoint,Is.EqualTo(2));Assert.That(game.WeaponTrialCount,Is.EqualTo(0));Assert.That(game.MapEnemies.Count(e=>!e.Health.IsDead),Is.GreaterThan(0));
            lab.SetPaused(true);var enemy=game.MapEnemies.First(e=>!e.Health.IsDead);enemy.enabled=false;enemy.Health.SetMaxHealth(100000);enemy.Health.FullHeal();
            Vector2 center=game.Map.Arenas[2];Move(center);
            for(int weapon=0;weapon<3;weapon++)
            {
                lab.Player.Model.Tick(3,new CombatInput{aim=Vector2.up});game.Weapons.Request(weapon);game.Weapons.BeforeTick(.01f,true);lab.Player.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=Vector2.up});var a=lab.Player.Model.Attack;
                Vector2 pos=center+Vector2.up*a.range*.6f;enemy.transform.position=pos;enemy.GetComponent<Rigidbody2D>().position=pos;Physics2D.SyncTransforms();
                lab.Player.Model.Tick(a.startup+a.active*.6f,new CombatInput{aim=Vector2.up});float hp=enemy.Health.CurrentHealth;Call(lab.Player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.LessThan(hp));Assert.That(game.WeaponTrialCount,Is.EqualTo(weapon+1));
            }
            lab.SetPaused(false);foreach(var e in game.MapEnemies.ToArray())if(!e.Health.IsDead)e.Health.TakeDamage(999999);
            for(int i=0;i<20&&game.Progress.checkpoint==2;i++)yield return null;Assert.That(game.Progress.checkpoint,Is.EqualTo(3));Assert.That(game.Progress.expansionFlags&8,Is.EqualTo(8));Assert.That(game.Progress.missionWeaponHits&7,Is.EqualTo(7));
            File.WriteAllText(Folder+"mission-evidence.json","{\"lyraMemory\":true,\"originalWeaponContacts\":3,\"missingWeaponsBlockProgress\":true,\"abyssOptional\":true,\"checkpoint\":3}");
        }
        // Capture the actual world RenderTexture and its HUD, without relying on
        // WaitForEndOfFrame (which does not advance in headless Editor batches).
        static void Capture(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var lab=DarklightCombatLab.Instance;var canvas=lab.PixelHUD.canvas;
            var nodes=canvas.GetComponentsInChildren<Transform>(true);var layers=new int[nodes.Length];for(int i=0;i<nodes.Length;i++){layers[i]=nodes[i].gameObject.layer;nodes[i].gameObject.layer=30;}
            var mode=canvas.renderMode;var previousCamera=canvas.worldCamera;float plane=canvas.planeDistance,scale=canvas.scaleFactor,scaler=lab.PixelHUD.scaler.scaleFactor;
            var go=new GameObject("QA_RegionCapture");var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            var rt=new RenderTexture(1280,720,24){antiAliasing=1};rt.Create();camera.targetTexture=rt;var previous=RenderTexture.active;var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);
            try{camera.transform.position=new Vector3(0,0,-10);canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;lab.PixelHUD.scaler.scaleFactor=2;canvas.scaleFactor=2;Canvas.ForceUpdateCanvases();lab.WorldCamera.Render();camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());Assert.That(image.GetPixels32().Count(p=>p.r+p.g+p.b>100),Is.GreaterThan(10000));}
            finally{canvas.renderMode=mode;canvas.worldCamera=previousCamera;canvas.planeDistance=plane;lab.PixelHUD.scaler.scaleFactor=scaler;canvas.scaleFactor=scale;for(int i=0;i<nodes.Length;i++)if(nodes[i]!=null)nodes[i].gameObject.layer=layers[i];RenderTexture.active=previous;rt.Release();Object.Destroy(rt);Object.Destroy(image);Object.Destroy(go);}
        }
    }
}
