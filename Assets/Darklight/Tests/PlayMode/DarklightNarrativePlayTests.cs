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
        [UnityTest]public IEnumerator NeroKeepsThreePhasesWhileOtherCombatGatesUseCommonEnemies()
        {
            foreach(int chapter in new[]{1,3,4,5,6,7,8,9})
            {
                ResumeAt(chapter,3);Invoke(game,"StartEncounter");
                for(int line=0;line<12&&!game.Active;line++){Invoke(game,"NextDialogue");yield return null;}
                Assert.That(game.Active,Is.True,"Chapter gate dialogue did not finish");
                yield return null;
                if(chapter!=DarklightCampaignRoute.NeroChapter)
                {
                    Assert.That(game.CurrentBoss,Is.Null,$"Chapter {chapter+1} should use common enemies");
                    var guards=game.MapEnemies.Where(e=>e!=null&&!e.Health.IsDead).ToArray();
                    Assert.That(guards.Length,Is.GreaterThan(0));
                    Assert.That(guards.All(e=>(int)e.kind<5),Is.True);
                    continue;
                }
                var boss=game.CurrentBoss;Assert.That(boss.kind,Is.EqualTo(RequiemEnemyKind.KaelFractured));Assert.That(boss.poses.Length,Is.EqualTo(4));Assert.That(boss.attackFrames.Length,Is.EqualTo(4));
                Assert.That(boss.attackFrames[2].name,Is.EqualTo("NeroFraturadoCombatS20261119_06"));
                float deadline=Time.realtimeSinceStartup+5;while(!boss.AcceptDamage&&Time.realtimeSinceStartup<deadline)yield return null;
                Assert.That(boss.AcceptDamage,Is.True,"Nero's entry protection did not finish");
                Assert.That(boss.Phase,Is.EqualTo(1));boss.Health.TakeDamage(boss.Health.MaxHealth*.4f);yield return new WaitForSeconds(.05f);Assert.That(boss.Phase,Is.EqualTo(2));Assert.That(boss.AcceptDamage,Is.False,"No phase-transition invulnerability");
                game.PreviewPanel("Pause");float clock=lab.Player.Model.Clock;yield return new WaitForSeconds(.05f);Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));Assert.That(boss.Phase,Is.EqualTo(2));Invoke(game,"Close");
                yield return new WaitForSeconds(.95f);boss.Health.TakeDamage(boss.Health.MaxHealth*.35f);yield return new WaitForSeconds(.05f);Assert.That(boss.Phase,Is.EqualTo(3));Assert.That(game.Progress.bossPhase,Is.EqualTo(3));
            }
        }
        [UnityTest]public IEnumerator NeroIsPurifiedThroughInteractionAndRemainsAlive()
        {
            ResumeAt(8,3);Invoke(game,"Spawn",RequiemEnemyKind.KaelFractured,game.Map.SafeSpawn(game.Map.Arenas[3]),false);
            var boss=game.CurrentBoss;float deadline=Time.realtimeSinceStartup+5;
            while(!boss.AcceptDamage&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(boss.AcceptDamage,Is.True,"Boss transition did not finish; paused="+lab.Paused+" phase="+boss.Phase);
            Assert.That(boss.GetComponent<DarklightCombatant>().Receive(99999,Vector2.up),Is.True);
            Assert.That(boss.Health.IsDead,Is.False);Assert.That(boss.Health.HealthPercent,Is.EqualTo(.08f).Within(.001f));Assert.That(boss.PurificationReady,Is.True);
            Move(game.Map.SafeSpawn((Vector2)boss.transform.position+Vector2.down));Assert.That(game.TryGetInteraction(out var target),Is.True);Assert.That(target.Id,Is.EqualTo("purify_kael"));Assert.That(game.TryInteract(),Is.True);
            Assert.That(boss.Resolved,Is.True);Assert.That(boss.Health.IsDead,Is.False);Assert.That(game.CurrentBoss,Is.Null);Assert.That(game.Progress.checkpoint,Is.EqualTo(4));Assert.That(game.Progress.expansionFlags&4,Is.EqualTo(4));
            Assert.That(GameObject.Find("KaelPurificado"),Is.Null);
            Assert.That(game.Progress.NeroDefeated&&game.Progress.NeroSurvived&&game.Progress.NeroRevenge,Is.True);
            Assert.That(game.Progress.RivalryPending,Is.True);
            Assert.That(game.Progress.TruceStarted,Is.False);
            Assert.That(game.NeroTruceActive,Is.False);
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
        [UnityTest]public IEnumerator FinalPortalDuelRespectsBothRecordedStances()
        {
            var path=System.IO.Path.GetFullPath("work/lucius-finale/"+System.Guid.NewGuid().ToString("N")+".json");
            using var storage=DarklightProgress.IsolateStorageForTests(path);
            foreach(var stance in new[]{1,-1})
            {
                ResumeAt(9,4);
                game.Progress.RecordNeroSurvival();
                game.Progress.luciusStance=stance;
                Move(game.Map.SafeSpawn(game.Map.Portal));
                Assert.That(game.TryInteract(),Is.True);
                for(int line=0;line<10&&!game.Active;line++)Invoke(game,"NextDialogue");
                var opponent=game.CurrentBoss;
                Assert.That(opponent,Is.Not.Null);
                Assert.That(opponent.IsLucius,Is.True);
                Assert.That(game.NeroTruceActive,Is.True);
                opponent.GetComponent<DarklightCombatant>().Receive(opponent.Health.MaxHealth+1,Vector2.up);
                Assert.That(opponent.Resolved,Is.EqualTo(stance>0));
                Assert.That(opponent.Health.IsDead,Is.EqualTo(stance<0));
                // A lethal hit is reported to the campaign by the enemy on its next update.
                float deadline=Time.realtimeSinceStartup+2f;
                while(game.Progress.luciusFate==0 && Time.realtimeSinceStartup<deadline)yield return null;
                Assert.That(game.Progress.luciusFate,Is.EqualTo(stance));
                var saved=DarklightProgress.Load();
                Assert.That(saved.luciusFate,Is.EqualTo(stance));
                Assert.That(saved.TruceEnded&&saved.RivalryPending,Is.True);
                Assert.That(game.NeroTruceActive,Is.False);
                yield return null;
            }
        }

        [UnityTest]public IEnumerator ObservatoryPortalContinuesWhileLastPortalOpensFinalChoice()
        {
            ResumeAt(5,4);Move(game.Map.SafeSpawn(game.Map.Portal));Assert.That(game.TryInteract(),Is.True);Assert.That(game.PhaseLoading,Is.True);
            var veil=typeof(DarklightCampaign).GetField("screenTransitionBusy",BindingFlags.Instance|BindingFlags.NonPublic);
            float deadline=Time.realtimeSinceStartup+10;
            while((game.PhaseLoading||(bool)veil.GetValue(game))&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.That(game.PhaseLoading,Is.False);Assert.That(game.Progress.chapter,Is.EqualTo(6));Assert.That(game.Progress.completed,Is.False);
            Assert.That(veil.GetValue(game),Is.False,"The observatory portal veil did not finish");
            ResumeAt(9,4);yield return null;Move(game.Map.SafeSpawn(game.Map.Portal));
            Assert.That(game.TryInteract(),Is.True,"The chapter 10 portal did not open the final choice");
            Assert.That(game.Progress.chapter,Is.EqualTo(9));Assert.That(lab.Paused,Is.True);yield return null;
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
