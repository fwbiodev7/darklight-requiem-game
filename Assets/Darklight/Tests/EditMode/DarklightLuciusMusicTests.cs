using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightLuciusMusicTests
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Set(object owner,string field,object value)=>owner.GetType().GetField(field,Private).SetValue(owner,value);
        static AudioSource Source(Transform parent)
        {
            var child=new GameObject("SilentMusicProbe");child.transform.SetParent(parent,false);
            var source=child.AddComponent<AudioSource>();source.playOnAwake=false;source.volume=0;return source;
        }
        [Test] public void ActualEventPriorityMutesBothSkinsAndAllFormsAndPauseKeepsTheBossSample()
        {
            bool silent=DarklightAutomation.Silent;DarklightAutomation.Silent=true;
            var context=new GameObject("InactiveLuciusMusicMixProbe");context.SetActive(false);
            var game=context.AddComponent<DarklightCampaign>();
            // Inactive context avoids campaign Awake/Start, scenes, UI, saves
            // and preference initialization. Wire only real mixer dependencies.
            Set(game,"tones",Array.Empty<AudioClip>());
            var clip=AudioClip.Create("SilentPrivateTrackProbe",1024,1,22050,false);
            try
            {
                var eventSource=Source(context.transform);eventSource.clip=clip;eventSource.loop=true;eventSource.timeSamples=73;
                Set(game,"music",eventSource);Set(game,"panel","");
                var enemyObject=new GameObject("LuciusMusicOpponent");enemyObject.transform.SetParent(context.transform,false);
                var health=enemyObject.AddComponent<HealthSystem>();health.SetMaxHealth(100,false);
                var enemy=enemyObject.AddComponent<DarklightEnemy>();
                typeof(DarklightEnemy).GetProperty("IsLucius").GetSetMethod(true).Invoke(enemy,new object[]{true});
                ((List<DarklightEnemy>)typeof(DarklightCampaign).GetField("enemies",Private).GetValue(game)).Add(enemy);
                Set(game,"luciusBossTrack",clip);Set(game,"luciusMusicOwned",true);
                Set(game,"luciusMusicEncounter",enemy);Set(game,"luciusMusicSample",73);
                var skin=context.AddComponent<TodaySkinMusic>();Set(skin,"game",game);
                var trackType=typeof(TodaySkinMusic).GetNestedType("Track",BindingFlags.NonPublic);
                foreach(string field in new[]{"rock","requiem"})
                {
                    var track=Activator.CreateInstance(trackType,true);
                    trackType.GetField("main").SetValue(track,Source(context.transform));
                    trackType.GetField("tail").SetValue(track,Source(context.transform));Set(skin,field,track);
                }
                typeof(DarklightCampaign).GetProperty("SkinMusic").GetSetMethod(true).Invoke(game,new object[]{skin});
                var forms=context.AddComponent<TodayTransformationMusic>();Set(forms,"game",game);
                Set(forms,"source",Source(context.transform));Set(forms,"themes",new[]{clip,clip,clip});
                typeof(DarklightCampaign).GetProperty("TransformationMusic").GetSetMethod(true).Invoke(game,new object[]{forms});
                for(int form=1;form<=3;form++)
                {
                    skin.AdvanceSkinFade(form%2,false,1);
                    typeof(DarklightCampaign).GetProperty("Form").GetSetMethod(true).Invoke(game,new object[]{form});
                    Assert.That(game.LuciusBossMusicHasPriority,Is.True);Assert.That(game.EventMusicHasPriority,Is.True);
                    skin.EvaluateMix(1);forms.EvaluateMix(1);
                    Assert.That(skin.EventWeight,Is.EqualTo(1));Assert.That(skin.RockWeight,Is.Zero);Assert.That(skin.RequiemWeight,Is.Zero);
                    Assert.That(forms.HasTheme,Is.True);Assert.That(forms.Weight,Is.Zero);Assert.That(forms.Source.volume,Is.Zero);
                }
                Set(game,"panel","Pause");skin.EvaluateMix(5);forms.EvaluateMix(5);
                Assert.That(game.EventMusicSource.clip,Is.SameAs(clip));Assert.That(game.LuciusBossMusicHasPriority,Is.True);
                Assert.That(typeof(DarklightCampaign).GetField("luciusMusicSample",Private).GetValue(game),Is.EqualTo(73));
                Assert.That(eventSource.timeSamples,Is.EqualTo(73));Assert.That(skin.EventWeight,Is.EqualTo(1),"Pause freezes the mix, not release it");
                foreach(var source in context.GetComponentsInChildren<AudioSource>(true))Assert.That(source.isPlaying,Is.False);
            }
            finally
            {
                // This synthetic clip is owned by the test, not the runtime loader.
                Set(game,"luciusBossTrack",null);UnityEngine.Object.DestroyImmediate(context);
                UnityEngine.Object.DestroyImmediate(clip);DarklightAutomation.Silent=silent;
            }
        }
        [Test] public void OnlyAnUnresolvedLiveLuciusDuelSelectsThePrivateTheme()
        {
            Assert.That(DarklightLuciusMusicTrack.Select(true,true,false,true,false,false),Is.True);
            Assert.That(DarklightLuciusMusicTrack.Select(false,true,false,true,false,false),Is.False,"Lucien or another boss cannot select Lucius's track");
            Assert.That(DarklightLuciusMusicTrack.Select(true,false,false,true,false,false),Is.False,"Victory releases the event");
            Assert.That(DarklightLuciusMusicTrack.Select(true,true,true,true,false,false),Is.False,"Sparing Lucius releases the event");
            Assert.That(DarklightLuciusMusicTrack.Select(true,true,false,false,false,false),Is.False,"Player death releases the event");
            Assert.That(DarklightLuciusMusicTrack.Select(true,true,false,true,true,false),Is.False,"Map changes release the event");
            Assert.That(DarklightLuciusMusicTrack.Select(true,true,false,true,false,true),Is.False,"Main menu cannot retain the duel music");
        }
        [Test] public void PausedAndSilentSessionsNeverPermitPlayback()
        {
            Assert.That(DarklightLuciusMusicTrack.CanPlay(true,false),Is.True);
            Assert.That(DarklightLuciusMusicTrack.CanPlay(false,false),Is.False);
            Assert.That(DarklightLuciusMusicTrack.CanPlay(true,true),Is.False);
            Assert.That(DarklightLuciusMusicTrack.CanPlay(false,true),Is.False);
            string path=DarklightLuciusMusicTrack.LocalPath(Application.persistentDataPath);
            Assert.That(Path.GetFileName(path),Is.EqualTo("LuciusBoss.mp3"));
            Assert.That(path,Does.Not.StartWith(Application.dataPath),"Private track stays outside project Assets/build inclusion");
        }
        [UnityTest] public IEnumerator SuppliedPrivateMp3DecodesWithoutCreatingAnAudioSource()
        {
            string path=DarklightLuciusMusicTrack.LocalPath(Application.persistentDataPath);
            Assert.That(DarklightLuciusMusicTrack.MatchesSuppliedFile(path),Is.True,"Local installation must match the exact supplied MP3");
            int before=UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
            bool silent=DarklightAutomation.Silent;DarklightAutomation.Silent=true;
            AudioClip clip=null;
            try
            {
                using(var request=UnityWebRequestMultimedia.GetAudioClip(new Uri(path).AbsoluteUri,AudioType.MPEG))
                {
                    ((DownloadHandlerAudioClip)request.downloadHandler).streamAudio=true;
                    var pending=request.SendWebRequest();while(!pending.isDone)yield return null;
                    Assert.That(request.result,Is.EqualTo(UnityWebRequest.Result.Success),request.error);
                    clip=DownloadHandlerAudioClip.GetContent(request);
                    Assert.That(clip,Is.Not.Null);Assert.That(clip.samples,Is.GreaterThan(0));Assert.That(clip.channels,Is.GreaterThan(0));
                }
                Assert.That(UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,Is.EqualTo(before));
            }
            finally{if(clip!=null)UnityEngine.Object.DestroyImmediate(clip);DarklightAutomation.Silent=silent;}
        }
    }
}
