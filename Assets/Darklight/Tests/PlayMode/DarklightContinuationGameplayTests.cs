using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Darklight.Tests
{
    public sealed class DarklightContinuationGameplayTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        const string Output="work/session-20261024/gameplay";
        IDisposable storage;
        DarklightCampaign game;
        DarklightCombatLab lab;
        Rigidbody2D playerBody;
        static object Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,Private).Invoke(target,args);
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,Private).GetValue(target);
        static void Move(Rigidbody2D body,Vector2 point){body.position=point;body.transform.position=point;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();}

        [Serializable] sealed class Sample
        {
            public string scenario;public int cycle,chapter,frames,attackCycles,objects;
            public double seconds;public float medianFrameMs,p95FrameMs,maxFrameMs;public long allocatedBytes;
        }
        [Serializable] sealed class Report
        {
            public string environment="Automated Unity Editor PlayMode; silent, isolated save, no physical controller or human play certification. Frame times are Editor observations, not sustained standalone FPS.";
            public Sample[] samples;
        }
        static void WriteReport(string name,List<Sample> samples)=>File.WriteAllText(Path.Combine(Output,name+".json"),JsonUtility.ToJson(new Report{samples=samples.ToArray()},true));
        static void AssertSilent()
        {
            Assert.That(DarklightAutomation.Silent,Is.True);Assert.That(AudioListener.pause,Is.True);Assert.That(AudioListener.volume,Is.Zero);
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False,"Automatic audio playback");
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            Directory.CreateDirectory(Output);
            storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath(Path.Combine(Output,Guid.NewGuid().ToString("N")+".json")));
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;playerBody=lab.Player.GetComponent<Rigidbody2D>();
            game.PreviewCampaign();yield return null;Assert.That(DarklightProgress.HasIsolatedTestStorage,Is.True);AssertSilent();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(lab!=null)lab.SetPaused(true);storage?.Dispose();yield return null;
        }

        [UnityTest,Timeout(180000)] public IEnumerator ThreeVisitorsCompleteRepeatedRealUpdateCyclesPauseAndRewardOnce()
        {
            var samples=new List<Sample>();
            foreach(int index in new[]{1,4,5})
            {
                var entry=DarklightMultiverseCatalog.Entries[index];game.PreviewChapter(entry.Chapter);yield return null;yield return null;
                Call(game,"InteractMultiverse","multiverse_"+index);Call(game,"Close");Call(game,"InteractMultiverse","multiverse_"+index);
                Assert.That(game.MultiverseEnemies.Count,Is.EqualTo(1));var visitor=game.MultiverseEnemies[0];
                foreach(var enemy in game.MapEnemies)enemy.enabled=false;
                Move(playerBody,game.Map.Spawn);Move(visitor.GetComponent<Rigidbody2D>(),game.Map.SafeSpawn(playerBody.position+Vector2.down*1.1f));
                var health=lab.Player.GetComponent<HealthSystem>();health.SetMaxHealth(100000,false);
                visitor.Health.SetMaxHealth(100000,false);lab.SetPaused(false);
                var seen=new HashSet<int>();var frames=new List<float>();double started=Time.realtimeSinceStartupAsDouble;
                while(Time.realtimeSinceStartupAsDouble-started<15)
                {
                    yield return null;seen.Add(Field<int>(visitor,"state"));frames.Add(Time.unscaledDeltaTime*1000);
                    Assert.That(health.IsDead,Is.False);Assert.That(visitor.Health.IsDead,Is.False);
                    Assert.That(game.Map.Bounds.Contains(visitor.transform.position),Is.True,"Visitor escaped map");
                    Assert.That(float.IsNaN(visitor.transform.position.x)||float.IsNaN(visitor.transform.position.y),Is.False);
                }
                int attacks=Field<int>(visitor,"serial");Assert.That(attacks,Is.GreaterThanOrEqualTo(3),entry.Challenge+" stalled");
                foreach(int phase in new[]{0,1,2,3})Assert.That(seen.Contains(phase),Is.True,entry.Challenge+" never entered phase "+phase);
                Assert.That(health.CurrentHealth,Is.LessThan(health.MaxHealth),entry.Challenge+" never connected a real contact");
                lab.SetPaused(true);yield return new WaitForFixedUpdate();
                float clock=Field<float>(visitor,"clock");Vector2 paused=visitor.transform.position;float pausedHealth=health.CurrentHealth;
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(Field<float>(visitor,"clock"),Is.EqualTo(clock));Assert.That(Field<int>(visitor,"serial"),Is.EqualTo(attacks));
                Assert.That(Vector2.Distance(paused,visitor.transform.position),Is.LessThan(.02f));Assert.That(health.CurrentHealth,Is.EqualTo(pausedHealth));
                frames.Sort();samples.Add(new Sample{scenario=entry.Challenge.ToString(),chapter=entry.Chapter,frames=frames.Count,attackCycles=attacks,
                    seconds=Time.realtimeSinceStartupAsDouble-started,medianFrameMs=frames[frames.Count/2],p95FrameMs=frames[Mathf.Min(frames.Count-1,(int)(frames.Count*.95f))],
                    maxFrameMs=frames[frames.Count-1],allocatedBytes=Profiler.GetTotalAllocatedMemoryLong(),objects=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length});
                int shards=game.Progress.shards;visitor.Health.InstantKill();Assert.That(game.Progress.shards,Is.EqualTo(shards+1));
                for(int repeat=0;repeat<20;repeat++)game.EnemyDefeated(visitor);
                Assert.That(game.Progress.shards,Is.EqualTo(shards+1));game.Progress.Save();var saved=DarklightProgress.Load();
                Assert.That(saved.multiverseDefeated&(1<<(int)entry.Challenge),Is.Not.Zero);Assert.That(saved.shards,Is.EqualTo(shards+1));
                Call(game,"LoadMap",entry.Chapter);yield return null;yield return null;lab.SetPaused(true);
                Call(game,"InteractMultiverse","multiverse_"+index);Assert.That(game.MultiverseEnemies.Count,Is.Zero);
                AssertSilent();WriteReport("visitors",samples);
            }
        }

        [UnityTest,Timeout(180000)] public IEnumerator ThreeTenMapCyclesKeepRoutesReadFlagsAndDiskRewardsStable()
        {
            var samples=new List<Sample>();var route=new List<Vector2>();int expectedLevel=0,expectedXp=0,expectedPoints=0;
            for(int cycle=0;cycle<3;cycle++)for(int chapter=0;chapter<DarklightProgress.ChapterCapacity;chapter++)
            {
                double started=Time.realtimeSinceStartupAsDouble;game.Progress.chapter=chapter;game.Progress.checkpoint=4;
                Call(game,"LoadMap",chapter);Call(game,"Close");yield return null;yield return null;lab.SetPaused(true);
                Assert.That(game.CurrentBoss,Is.Null);Assert.That(game.MultiverseEnemies.Count,Is.Zero);
                Assert.That(game.Map.Bounds.Contains(playerBody.position),Is.True);Assert.That(lab.WorldCamera.GetComponent<DarklightLabCamera>().bounds,Is.EqualTo(game.Map.Bounds));
                foreach(var point in new[]{game.Map.Ally,game.Map.Relic,game.Map.Portal})
                    Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,game.Map.SafeSpawn(point),route),Is.True,"Cycle "+cycle+" map "+chapter+" route "+point);
                for(int i=0;i<DarklightMultiverseCatalog.Entries.Length;i++)
                {
                    var entry=DarklightMultiverseCatalog.Entries[i];if(entry.Chapter!=chapter)continue;
                    int previousRead=game.Progress.multiverseRead;Call(game,"InteractMultiverse","multiverse_"+i);Call(game,"Close");
                    Assert.That(game.Progress.multiverseRead&(1<<i),Is.Not.Zero);
                    if(cycle==0&&entry.Challenge!=DarklightVisitor.None)
                    {
                        Call(game,"InteractMultiverse","multiverse_"+i);Assert.That(game.MultiverseEnemies.Count,Is.GreaterThan(0));
                        game.MultiverseEnemies[game.MultiverseEnemies.Count-1].Health.InstantKill();
                    }
                    else if(cycle>0)
                    {
                        Assert.That(game.Progress.multiverseRead,Is.EqualTo(previousRead));Assert.That(game.MultiverseEnemies.Count,Is.Zero);
                    }
                }
                game.Progress.Save();var saved=DarklightProgress.Load();Assert.That(saved,Is.Not.Null);
                Assert.That(saved.multiverseRead,Is.EqualTo(game.Progress.multiverseRead));Assert.That(saved.multiverseDefeated,Is.EqualTo(game.Progress.multiverseDefeated));
                Assert.That(saved.shards,Is.EqualTo(game.Progress.shards));Assert.That(saved.checkpoint,Is.EqualTo(4));
                typeof(DarklightCampaign).GetProperty("Progress").SetValue(game,saved);
                if(cycle==0&&chapter==9){expectedLevel=saved.level;expectedXp=saved.xp;expectedPoints=saved.points;}
                if(cycle>0)
                {
                    Assert.That(saved.multiverseRead,Is.EqualTo(4095));Assert.That(saved.multiverseDefeated,Is.EqualTo(14));Assert.That(saved.shards,Is.EqualTo(3));
                    Assert.That(saved.level,Is.EqualTo(expectedLevel));Assert.That(saved.xp,Is.EqualTo(expectedXp));Assert.That(saved.points,Is.EqualTo(expectedPoints));
                }
                AssertSilent();samples.Add(new Sample{scenario="map-reload-and-interaction-api",cycle=cycle,chapter=chapter,seconds=Time.realtimeSinceStartupAsDouble-started,
                    allocatedBytes=Profiler.GetTotalAllocatedMemoryLong(),objects=Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length});WriteReport("campaign-cycles",samples);
            }
        }

        [UnityTest,Timeout(180000)] public IEnumerator ThreeBridgeRoundsRemainTraversableBothWaysWithFixedPhysics()
        {
            game.PreviewChapter(4);yield return null;yield return null;lab.SetPaused(true);lab.Player.enabled=false;
            var paths=new[]{
                new[]{new Vector2(515,254),new Vector2(563,255),new Vector2(605,267),new Vector2(648,285)},
                new[]{new Vector2(603,500),new Vector2(651,500),new Vector2(711,500),new Vector2(748,518),new Vector2(790,501)},
                new[]{new Vector2(1060,674),new Vector2(1115,675),new Vector2(1165,681),new Vector2(1203,654),new Vector2(1245,632)}};
            var wait=new WaitForFixedUpdate();var samples=new List<Sample>();
            for(int round=0;round<3;round++)for(int bridge=0;bridge<paths.Length;bridge++)foreach(bool reverse in new[]{false,true})
            {
                double started=Time.realtimeSinceStartupAsDouble;int steps=0;var path=(Vector2[])paths[bridge].Clone();if(reverse)Array.Reverse(path);
                Move(playerBody,game.Map.Art.World(path[0]));
                foreach(var pixel in path)
                {
                    Vector2 target=game.Map.Art.World(pixel);
                    for(int i=0;i<250&&Vector2.Distance(playerBody.position,target)>.075f;i++)
                    {
                        var desired=Vector2.ClampMagnitude((target-playerBody.position)/Time.fixedDeltaTime,4.5f);
                        playerBody.linearVelocity=DarklightNavigation.SlidingVelocity(playerBody,desired,Time.fixedDeltaTime);steps++;yield return wait;
                    }
                    playerBody.linearVelocity=Vector2.zero;Assert.That(Vector2.Distance(playerBody.position,target),Is.LessThan(.08f),"Round "+round+" bridge "+bridge+" reverse "+reverse+" at "+pixel);
                }
                Assert.That(lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled,Is.True);AssertSilent();
                samples.Add(new Sample{scenario="bridge-"+bridge+(reverse?"-return":"-out"),cycle=round,chapter=4,frames=steps,seconds=Time.realtimeSinceStartupAsDouble-started});
            }
            Assert.That(samples.Count,Is.EqualTo(18));WriteReport("bridges",samples);
        }
    }
}
