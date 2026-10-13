using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightNarrativePlayTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        static void Set(object o,string field,object value)=>o.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
        static void Invoke(object o,string method,params object[] args)=>o.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        [UnitySetUp]public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;}
        void Move(Vector2 point){lab.Player.ResetModel();lab.Player.GetComponent<Rigidbody2D>().position=point;lab.Player.transform.position=point;Physics2D.SyncTransforms();}
        void ResumeAt(int chapter,int checkpoint){game.PreviewChapter(chapter);game.Progress.checkpoint=checkpoint;Invoke(game,"LoadMap",chapter);Invoke(game,"Close");}
        [UnityTest]public IEnumerator NewBossCatalogAndThreePhasesWorkInEveryNewChapter()
        {
            for(int chapter=6;chapter<10;chapter++)
            {
                ResumeAt(chapter,3);var kind=(RequiemEnemyKind)(5+chapter);Invoke(game,"Spawn",kind,game.Map.SafeSpawn(game.Map.Arenas[3]),false);yield return null;
                var boss=game.CurrentBoss;Assert.That(boss.kind,Is.EqualTo(kind));Assert.That(boss.poses.Length,Is.EqualTo(4));Assert.That(boss.attackFrames.Length,Is.EqualTo(4));Assert.That(boss.poses[0].name,Does.StartWith("ExpansionBosses_"));
                Assert.That(boss.Phase,Is.EqualTo(1));boss.Health.TakeDamage(boss.Health.MaxHealth*.4f);yield return new WaitForSeconds(.05f);Assert.That(boss.Phase,Is.EqualTo(2));Assert.That(boss.AcceptDamage,Is.False,"No phase-transition invulnerability");
                game.PreviewPanel("Pause");float clock=lab.Player.Model.Clock;yield return new WaitForSeconds(.05f);Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));Assert.That(boss.Phase,Is.EqualTo(2));Invoke(game,"Close");
                yield return new WaitForSeconds(.95f);boss.Health.TakeDamage(boss.Health.MaxHealth*.35f);yield return new WaitForSeconds(.05f);Assert.That(boss.Phase,Is.EqualTo(3));Assert.That(game.Progress.bossPhase,Is.EqualTo(3));
                if(chapter==9)Assert.That(game.Progress.FormUnlocked,Is.EqualTo(3));
            }
        }
        [UnityTest]public IEnumerator KaelIsPurifiedThroughInteractionAndRemainsAlive()
        {
            ResumeAt(8,3);Invoke(game,"Spawn",RequiemEnemyKind.KaelFractured,game.Map.SafeSpawn(game.Map.Arenas[3]),false);
            var boss=game.CurrentBoss;float deadline=Time.realtimeSinceStartup+5;
            while(!boss.AcceptDamage&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(boss.AcceptDamage,Is.True,"Boss transition did not finish; paused="+lab.Paused+" phase="+boss.Phase);
            Assert.That(boss.GetComponent<DarklightCombatant>().Receive(99999,Vector2.up),Is.True);
            Assert.That(boss.Health.IsDead,Is.False);Assert.That(boss.Health.HealthPercent,Is.EqualTo(.08f).Within(.001f));Assert.That(boss.PurificationReady,Is.True);
            Move(game.Map.SafeSpawn((Vector2)boss.transform.position+Vector2.down));Assert.That(game.TryGetInteraction(out var target),Is.True);Assert.That(target.Id,Is.EqualTo("purify_kael"));Assert.That(game.TryInteract(),Is.True);
            Assert.That(boss.Resolved,Is.True);Assert.That(boss.Health.IsDead,Is.False);Assert.That(game.CurrentBoss,Is.Null);Assert.That(game.Progress.checkpoint,Is.EqualTo(4));Assert.That(game.Progress.expansionFlags&4,Is.EqualTo(4));
            yield return null;Assert.That(boss.Health.IsDead,Is.False);
        }
        [UnityTest]public IEnumerator InscriptionAndAncestorMemoryAdvanceDistinctCheckpoints()
        {
            ResumeAt(6,0);Move(game.Map.SafeSpawn(game.Map.Arenas[0]));yield return null;
            Assert.That(game.TryGetInteraction(out var target),Is.True);Assert.That(target.Id,Is.EqualTo("narrative"));Assert.That(game.TryInteract(),Is.True);Assert.That(game.Progress.checkpoint,Is.EqualTo(1));
            Invoke(game,"Close");Set(game,"NarrativeCombatDone",true);Set(game,"encounterStarted",false);Move(game.Map.SafeSpawn(game.Map.Arenas[1]));
            Assert.That(game.TryInteract(),Is.True);Assert.That(game.Progress.checkpoint,Is.EqualTo(2));Assert.That(game.Progress.expansionFlags&1,Is.EqualTo(1));
            yield return null;var reloaded=DarklightProgress.Decode(JsonUtility.ToJson(game.Progress));Assert.That(reloaded.checkpoint,Is.EqualTo(2));Assert.That(reloaded.expansionFlags&1,Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator ShelterWaveCheckpointsResumeWithoutRepeatingCompletedWaves()
        {
            ResumeAt(7,1);Invoke(game,"BeginNarrativeEncounter");foreach(var e in game.MapEnemies.ToArray())e.Health.TakeDamage(99999);yield return null;yield return null;
            Assert.That(game.Progress.missionWave,Is.EqualTo(1));Assert.That(game.Progress.checkpoint,Is.EqualTo(1));
            var saved=DarklightProgress.Decode(JsonUtility.ToJson(game.Progress));Assert.That(saved.missionWave,Is.EqualTo(1));Invoke(game,"LoadMap",7);Invoke(game,"BeginNarrativeEncounter");
            Assert.That(game.MapEnemies.Count(e=>e!=null&&!e.Health.IsDead),Is.EqualTo(4));yield return null;
        }
        [UnityTest]public IEnumerator LucienPortalContinuesWhileLastPortalOpensFinalChoice()
        {
            ResumeAt(5,4);Move(game.Map.SafeSpawn(game.Map.Portal));Assert.That(game.TryInteract(),Is.True);Assert.That(game.Progress.chapter,Is.EqualTo(6));Assert.That(game.Progress.completed,Is.False);
            ResumeAt(9,4);Move(game.Map.SafeSpawn(game.Map.Portal));Assert.That(game.TryInteract(),Is.True);Assert.That(game.Progress.chapter,Is.EqualTo(9));Assert.That(lab.Paused,Is.True);yield return null;
        }
        [UnityTest]public IEnumerator MatureLightCatalogUsesExactSkinAgeDirectionAndContactPhase()
        {
            game.PreviewChapter(4);var catalog=new DarklightAnimationCatalog();
            for(int skin=0;skin<2;skin++)for(int d=0;d<8;d++)
            {
                Vector2 facing=Quaternion.Euler(0,0,-d*45)*Vector2.down;
                var model=new CombatModel(lab.Player.loadout);model.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});
                string prefix=(skin==1?"Rock":"Requiem")+"KatanaLightStage4_";
                var art=catalog.Light(skin,4,0,d,model);Assert.That(art.name,Is.EqualTo(prefix+(d*4)));
                model.Tick(model.Attack.startup+.001f,default);Assert.That(catalog.Light(skin,4,0,d,model).name,Is.EqualTo(prefix+(d*4+1)));
                // A tick crossing the active interval must display contact because damage occurs on that tick.
                model.Tick(model.Attack.active+.001f,default);Assert.That(model.ActiveThisTick,Is.True);
                Assert.That(catalog.Light(skin,4,0,d,model).name,Is.EqualTo(prefix+(d*4+1)));
                model.Tick(.001f,default);Assert.That(model.ActiveThisTick,Is.False);
                Assert.That(catalog.Light(skin,4,0,d,model).name,Is.EqualTo(prefix+(d*4+2)));
                model.Tick(model.Attack.recovery*.6f,default);Assert.That(catalog.Light(skin,4,0,d,model).name,Is.EqualTo(prefix+(d*4+3)));
                Assert.That(art.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(art.pivot.y,Is.InRange(0,12));
                Assert.That(catalog.Light(skin,3,0,d,model),Is.Null);Assert.That(catalog.Light(skin,4,1,d,model),Is.Null);
                model=new CombatModel(lab.Player.loadout);model.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});
                model.Tick(model.Attack.Duration+.01f,default);
                Assert.That(model.State,Is.EqualTo(CombatState.Neutral));Assert.That(model.ActiveThisTick,Is.True);
                Assert.That(catalog.Light(skin,4,0,d,model).name,Is.EqualTo(prefix+(d*4+1)),"A long damage tick returned to wind-up instead of contact");
                model.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});
                Assert.That(model.ComboCount,Is.EqualTo(2));
                if(skin==0)Assert.That(catalog.Light(skin,4,0,d,model).name,Is.EqualTo("RequiemKatanaLight2Stage4_"+(d*4)));
                else Assert.That(catalog.Light(skin,4,0,d,model),Is.Null,"Missing Rock second gesture cannot use first-strike art");
            }
            yield return null;
        }
        [UnityTest]public IEnumerator RequiemMatureDodgeKeepsAuthoredCrouchAndFourPhasesInEveryDirection()
        {
            var catalog=new DarklightAnimationCatalog();
            for(int d=0;d<8;d++)
            {
                var model=new CombatModel(lab.Player.loadout);Vector2 facing=Quaternion.Euler(0,0,-d*45)*Vector2.down;
                model.Tick(.001f,new CombatInput{aim=facing,dodgePressed=true});
                var first=catalog.Dodge(0,4,0,d,model);Assert.That(first.name,Is.EqualTo("RequiemKatanaDodgeStage4_"+(d*4)));
                float previous=0;
                foreach(float time in new[]{.05f,.13f,.20f})
                {
                    model.Tick(time-previous,default);previous=time;
                    int phase=time<.12f?1:time<.19f?2:3;
                    var frame=catalog.Dodge(0,4,0,d,model);Assert.That(frame.name,Is.EqualTo("RequiemKatanaDodgeStage4_"+(d*4+phase)));
                    Assert.That(frame.pixelsPerUnit,Is.EqualTo(first.pixelsPerUnit));Assert.That(frame.texture.filterMode,Is.EqualTo(FilterMode.Point));
                }
                var standing=catalog.Dodge(0,4,0,d,model);Assert.That(standing.bounds.size.y,Is.GreaterThan(first.bounds.size.y),"Crouch was resized to standing height");
                Assert.That(catalog.Dodge(1,4,0,d,model),Is.Null);Assert.That(catalog.Dodge(0,3,0,d,model),Is.Null);
                model.Tick(.04f,default);Assert.That(model.State,Is.EqualTo(CombatState.Neutral));Assert.That(catalog.Dodge(0,4,0,d,model),Is.Null);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator RequiemMatureParryUsesFourAuthoredPhasesAndKeepsCombatWindow()
        {
            var catalog=new DarklightAnimationCatalog();
            for(int d=0;d<8;d++)
            {
                var model=new CombatModel(lab.Player.loadout);Vector2 facing=Quaternion.Euler(0,0,-d*45)*Vector2.down;
                model.Tick(.001f,new CombatInput{aim=facing,parryPressed=true});
                string prefix="RequiemKatanaParryStage4_";
                Assert.That(catalog.Parry(0,4,0,d,model).name,Is.EqualTo(prefix+(d*4)));
                float previous=0;
                foreach(float time in new[]{.05f,.15f,.22f})
                {
                    model.Tick(time-previous,default);previous=time;
                    int phase=time<.14f?1:time<.21f?2:3;
                    var frame=catalog.Parry(0,4,0,d,model);
                    Assert.That(frame.name,Is.EqualTo(prefix+(d*4+phase)));
                    Assert.That(frame.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    Assert.That(model.PerfectParry,Is.EqualTo(time<.14f));
                }
                Assert.That(catalog.Parry(1,4,0,d,model),Is.Null);Assert.That(catalog.Parry(0,3,0,d,model),Is.Null);Assert.That(catalog.Parry(0,4,1,d,model),Is.Null);
                model.Tick(.05f,default);Assert.That(model.State,Is.EqualTo(CombatState.Neutral));Assert.That(catalog.Parry(0,4,0,d,model),Is.Null);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator RequiemMatureLightComboSelectsDistinctSheetsAndKeepsContactAndRecovery()
        {
            var catalog=new DarklightAnimationCatalog();
            Assert.That(lab.Player.loadout.lights.Length,Is.EqualTo(3));
            for(int d=0;d<8;d++)
            {
                var model=new CombatModel(lab.Player.loadout);Vector2 facing=Quaternion.Euler(0,0,-d*45)*Vector2.down;
                var textures=new System.Collections.Generic.HashSet<Texture>();
                for(int strike=1;strike<=3;strike++)
                {
                    model.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});
                    Assert.That(model.State,Is.EqualTo(CombatState.Attack));Assert.That(model.ComboCount,Is.EqualTo(strike));
                    string sheet="RequiemKatanaLight"+(strike==1?"":strike.ToString())+"Stage4_";
                    var prepare=catalog.Light(0,4,0,d,model);Assert.That(prepare.name,Is.EqualTo(sheet+(d*4)));textures.Add(prepare.texture);
                    model.Tick(model.Attack.startup+.001f,default);
                    Assert.That(model.ActiveThisTick,Is.True);Assert.That(catalog.Light(0,4,0,d,model).name,Is.EqualTo(sheet+(d*4+1)));
                    var target=new object();Assert.That(model.RegisterHit(target),Is.True);Assert.That(model.RegisterHit(target),Is.False);
                    model.Tick(model.Attack.active+.001f,default);model.Tick(.001f,default);
                    Assert.That(model.ActiveThisTick,Is.False);var follow=catalog.Light(0,4,0,d,model);
                    Assert.That(follow.name,Is.EqualTo(sheet+(d*4+2)));Assert.That(follow.pixelsPerUnit,Is.EqualTo(prepare.pixelsPerUnit));Assert.That(follow.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    model.Tick(model.Attack.recovery*.75f,default);
                    Assert.That(catalog.Light(0,4,0,d,model).name,Is.EqualTo(sheet+(d*4+3)));
                    model.Tick(model.Attack.Duration-model.StateTime+.001f,default);
                    Assert.That(model.State,Is.EqualTo(CombatState.Neutral));Assert.That(catalog.Light(0,4,0,d,model),Is.Null);
                    Assert.That(catalog.Light(0,3,0,d,model),Is.Null);Assert.That(catalog.Light(0,4,1,d,model),Is.Null);
                }
                Assert.That(textures.Count,Is.EqualTo(3),"Three gestures were mapped to the same texture");
                model.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});Assert.That(model.ComboCount,Is.EqualTo(1));
                Assert.That(catalog.Light(0,4,0,d,model).name,Is.EqualTo("RequiemKatanaLightStage4_"+(d*4)));
            }
            yield return null;
        }
        [UnityTest]public IEnumerator AudioPlaybackIsBoundedMutedAndStopsWhenAutomationReturnsToSilence()
        {
            game.PreviewChapter(4);float volume=AudioListener.volume;AudioListener.volume=0;
            try
            {
                DarklightAutomation.Silent=false;var audio=game.AudioDirector;
                Assert.That(audio.Play("rpg-knifeSlice",DarklightAudioCategory.Combat,.3f,null,1,0),Is.True);
                yield return null;Assert.That(audio.ActiveVoices,Is.GreaterThan(0));Assert.That(audio.AllocatedVoices,Is.EqualTo(16));
                game.EffectsVolume=0;yield return null;
                foreach(var src in audio.GetComponentsInChildren<AudioSource>())if(!src.loop&&src.isPlaying)Assert.That(src.volume,Is.Zero);
                for(int i=0;i<40;i++)audio.Play("interface-click_",DarklightAudioCategory.Interface,.2f,null,1,0);
                Assert.That(audio.ActiveVoices,Is.LessThanOrEqualTo(16));DarklightAutomation.Silent=true;yield return null;
                foreach(var src in audio.GetComponentsInChildren<AudioSource>())Assert.That(src.isPlaying,Is.False);
            }
            finally{DarklightAutomation.Silent=true;game.AudioDirector.StopAll();AudioListener.volume=volume;}
        }
    }
}
