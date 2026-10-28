using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        [Serializable] sealed class Playback1850Row
        {
            public string id,clip;
            public float seconds,sourcePeak,listenerPeak;
            public bool requestAccepted,requestAcceptanceMeasured,sourcePlayingObserved,categoryGainVerified;
            public float healthPercentAtStart,minimumHealthPercent,maximumHealthPercent;
            public int advancingFrames,walkingFrames;
            public bool trainingDamageEnabled,stoppedWhenConditionEnded;
        }
        [Serializable] sealed class Playback1850Report
        {
            public int outputSampleRate;
            public double dspElapsed;
            public float controlledListenerVolume=.12f;
            public bool musicMuted=true,isolatedSave=true,subjectiveListeningCertified=false;
            public bool contextualConditionsUsed;
            public Playback1850Row[] effects;
        }

        [UnityTest,Timeout(90000)] public IEnumerator BreathRequiresItsRealLowHealthConditionAndStopsAfterHealing()
            => ProbeContextualDelivery("PC04");

        [UnityTest,Timeout(90000)] public IEnumerator ClothRequiresItsRealWalkingConditionAndStopsAtRest()
            => ProbeContextualDelivery("PC06");

        IEnumerator ProbeContextualDelivery(string id)
        {
            string folder="outputs/retomada-20261026/audio";Directory.CreateDirectory(folder);
            var rows=new List<Playback1850Row>();var buffer=new float[4096];var mixed=new float[4096];
            float effects=game.EffectsVolume,player=game.PlayerVolume,enemy=game.EnemyVolume,music=game.MusicVolume,ambient=game.AmbientVolume,ui=game.InterfaceVolume;
            bool trainingDamage=game.TrainingDamage;
            var health=fighter.GetComponent<HealthSystem>();var body=fighter.GetComponent<Rigidbody2D>();
            Vector2 origin=body.position;double dspStart=AudioSettings.dspTime;
            try
            {
                Context(0,0,0,0);health.FullHeal();
                // Use the real arena damage option. With it disabled, Update
                // heals the fixture before the audio observer's LateUpdate.
                Call(game,"set_TrainingDamage",true);
                foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))source.Stop();
                game.EffectsVolume=game.PlayerVolume=1;game.EnemyVolume=game.MusicVolume=game.AmbientVolume=game.InterfaceVolume=0;
                DarklightAutomation.Silent=false;AudioListener.pause=false;AudioListener.volume=.12f;lab.SetPaused(false);
                {
                    health.FullHeal();fighter.ResetModel();
                    if(id=="PC04")health.TakeDamage(health.MaxHealth*.8f);
                    Assert.That(game.Active,Is.True,"The real campaign audio gate must be open");
                    Assert.That(game.TrainingDamage,Is.True,"Training must preserve the low-health fixture");
                    Assert.That(health.HealthPercent,Is.EqualTo(id=="PC04"?.2f:1f).Within(.001f),id+": verify the actual health condition before measuring audio");
                    var clip=Resources.Load<AudioClip>("Darklight/Audio/Delivery20261023/dl-user-"+id.ToLowerInvariant());Assert.That(clip,Is.Not.Null);
                    var row=new Playback1850Row{id=id,clip=clip.name,seconds=clip.length,trainingDamageEnabled=game.TrainingDamage,
                        healthPercentAtStart=health.HealthPercent,minimumHealthPercent=health.HealthPercent,maximumHealthPercent=health.HealthPercent};
                    rows.Add(row);
                    double began=Time.realtimeSinceStartupAsDouble,firstPlaying=-1;
                    while(Time.realtimeSinceStartupAsDouble-began<clip.length+3)
                    {
                        row.minimumHealthPercent=Mathf.Min(row.minimumHealthPercent,health.HealthPercent);
                        row.maximumHealthPercent=Mathf.Max(row.maximumHealthPercent,health.HealthPercent);
                        if(id=="PC04")Assert.That(health.HealthPercent,Is.LessThan(.28f),"PC04 requires sustained real low health, not a discarded damage request");
                        bool walking=id=="PC06";
                        Tick(Facing(6),new CombatInput{move=walking?Vector2.right:Vector2.zero});
                        if(game.Active&&lab.CombatDelta>0)row.advancingFrames++;
                        if(walking&&fighter.Model.Motion.sqrMagnitude>.01f)row.walkingFrames++;
                        Vector2 point=origin+(walking?Vector2.right*(Mathf.PingPong((float)(Time.realtimeSinceStartupAsDouble-began)*3,3)-1.5f):Vector2.zero);
                        body.position=point;body.transform.position=point;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
                        foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                        {
                            if(source.clip!=clip||!source.isPlaying)continue;
                            // Contextual hooks do not expose a return value;
                            // record source playback without inventing acceptance.
                            row.sourcePlayingObserved=true;
                            if(firstPlaying<0)firstPlaying=Time.realtimeSinceStartupAsDouble;
                            source.GetOutputData(buffer,0);foreach(float value in buffer)row.sourcePeak=Mathf.Max(row.sourcePeak,Mathf.Abs(value));
                        }
                        AudioListener.GetOutputData(mixed,0);foreach(float value in mixed)row.listenerPeak=Mathf.Max(row.listenerPeak,Mathf.Abs(value));
                        if(firstPlaying>=0&&Time.realtimeSinceStartupAsDouble-firstPlaying>=clip.length+.1)break;
                        yield return null;
                    }
                    Assert.That(row.advancingFrames,Is.GreaterThan(0),"The actual campaign must advance during the contextual probe");
                    if(id=="PC06")Assert.That(row.walkingFrames,Is.GreaterThan(0),"Cloth must be driven by actual walking model state and displacement");
                    Assert.That(row.sourcePlayingObserved,Is.True,id+": real context must start its voice");
                    Assert.That(row.sourcePeak,Is.GreaterThan(0),id+": actual source must produce DSP samples under its valid context");
                    health.FullHeal();for(int i=0;i<20;i++){Tick(Facing(6));yield return null;}
                    row.stoppedWhenConditionEnded=!UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Any(s=>s.clip==clip&&s.isPlaying);
                    Assert.That(row.stoppedWhenConditionEnded,Is.True,
                        id+": healing or stopping movement must stop the contextual voice");
                    game.AudioDirector.StopAll();
                }
            }
            finally
            {
                lab.SetPaused(true);DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;health.FullHeal();
                Call(game,"set_TrainingDamage",trainingDamage);
                game.AudioDirector.StopAll();foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))source.Stop();
                game.EffectsVolume=effects;game.PlayerVolume=player;game.EnemyVolume=enemy;game.MusicVolume=music;game.AmbientVolume=ambient;game.InterfaceVolume=ui;
                File.WriteAllText(Path.Combine(folder,"contextual-playback-"+id.ToLowerInvariant()+".json"),JsonUtility.ToJson(new Playback1850Report{
                    outputSampleRate=AudioSettings.outputSampleRate,dspElapsed=AudioSettings.dspTime-dspStart,contextualConditionsUsed=true,effects=rows.ToArray()},true));
            }
        }

        [UnityTest,Timeout(180000)] public IEnumerator DeliveredEffectsUseTheActualPoolAndExposeDspOutput()
        {
            string folder="outputs/redesign-armas-20261025-retomada1850/audio";
            Directory.CreateDirectory(folder);
            var rows=new List<Playback1850Row>();var sourceBuffer=new float[512];var listenerBuffer=new float[512];
            float effects=game.EffectsVolume,player=game.PlayerVolume,music=game.MusicVolume,ambient=game.AmbientVolume,ui=game.InterfaceVolume;
            double dspStart=AudioSettings.dspTime;
            try
            {
                foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))source.Stop();
                game.EffectsVolume=1;game.PlayerVolume=1;game.MusicVolume=game.AmbientVolume=game.InterfaceVolume=0;
                DarklightAutomation.Silent=false;AudioListener.pause=false;AudioListener.volume=.12f;
                foreach(string id in DarklightSoundDirector.DeliveredIds)
                {
                    var clip=Resources.Load<AudioClip>("Darklight/Audio/Delivery20261023/dl-user-"+id.ToLowerInvariant());
                    Assert.That(clip,Is.Not.Null,id);clip.LoadAudioData();yield return null;
                    var row=new Playback1850Row{id=id,clip=clip.name,seconds=clip.length};
                    row.requestAccepted=game.AudioDirector.Delivered(id,DarklightAudioCategory.Character,.2f,.01f,true);
                    row.requestAcceptanceMeasured=true;
                    Assert.That(row.requestAccepted,Is.True,id+": delivered resource must reach the real voice pool");
                    double start=Time.realtimeSinceStartupAsDouble;
                    while(Time.realtimeSinceStartupAsDouble-start<clip.length+.08)
                    {
                        foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                        {
                            if(source.clip!=clip||!source.isPlaying)continue;
                            row.sourcePlayingObserved=true;source.GetOutputData(sourceBuffer,0);
                            foreach(float value in sourceBuffer)row.sourcePeak=Mathf.Max(row.sourcePeak,Mathf.Abs(value));
                            if(id=="WM01"&&!row.categoryGainVerified&&Time.realtimeSinceStartupAsDouble-start>.08)
                            {
                                float before=source.volume;game.EffectsVolume=.5f;yield return null;
                                Assert.That(source.volume,Is.EqualTo(before*.5f).Within(.002f),"Effects gain must reach the actual voice");
                                game.EffectsVolume=1;row.categoryGainVerified=true;
                            }
                        }
                        AudioListener.GetOutputData(listenerBuffer,0);
                        foreach(float value in listenerBuffer)row.listenerPeak=Mathf.Max(row.listenerPeak,Mathf.Abs(value));
                        yield return null;
                    }
                    rows.Add(row);game.AudioDirector.StopAll();yield return null;
                }
                Assert.That(rows.Count,Is.EqualTo(19));
            }
            finally
            {
                DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
                game.AudioDirector.StopAll();foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))source.Stop();
                game.EffectsVolume=effects;game.PlayerVolume=player;game.MusicVolume=music;game.AmbientVolume=ambient;game.InterfaceVolume=ui;
                File.WriteAllText(Path.Combine(folder,"dsp-playback-probe.json"),JsonUtility.ToJson(new Playback1850Report{
                    outputSampleRate=AudioSettings.outputSampleRate,dspElapsed=AudioSettings.dspTime-dspStart,effects=rows.ToArray()},true));
            }
        }
    }
}
