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
            public bool requestAccepted,sourcePlayingObserved,categoryGainVerified;
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

        [UnityTest,Timeout(90000)] public IEnumerator BreathAndClothRequireTheirRealHealthAndWalkingConditions()
        {
            string folder="outputs/redesign-armas-20261025-retomada1850/audio";Directory.CreateDirectory(folder);
            var rows=new List<Playback1850Row>();var buffer=new float[4096];var mixed=new float[4096];
            float effects=game.EffectsVolume,player=game.PlayerVolume,enemy=game.EnemyVolume,music=game.MusicVolume,ambient=game.AmbientVolume,ui=game.InterfaceVolume;
            var health=fighter.GetComponent<HealthSystem>();var body=fighter.GetComponent<Rigidbody2D>();
            Vector2 origin=body.position;double dspStart=AudioSettings.dspTime;
            try
            {
                Context(0,0,0,0);health.FullHeal();
                foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))source.Stop();
                game.EffectsVolume=game.PlayerVolume=1;game.EnemyVolume=game.MusicVolume=game.AmbientVolume=game.InterfaceVolume=0;
                DarklightAutomation.Silent=false;AudioListener.pause=false;AudioListener.volume=.12f;lab.SetPaused(false);
                foreach(string id in new[]{"PC04","PC06"})
                {
                    health.FullHeal();fighter.ResetModel();
                    if(id=="PC04")health.TakeDamage(health.MaxHealth*.8f);
                    var clip=Resources.Load<AudioClip>("Darklight/Audio/Delivery20261023/dl-user-"+id.ToLowerInvariant());Assert.That(clip,Is.Not.Null);
                    var row=new Playback1850Row{id=id,clip=clip.name,seconds=clip.length};
                    double began=Time.realtimeSinceStartupAsDouble,firstPlaying=-1;
                    while(Time.realtimeSinceStartupAsDouble-began<clip.length+3)
                    {
                        bool walking=id=="PC06";
                        Tick(Facing(6),new CombatInput{move=walking?Vector2.right:Vector2.zero});
                        Vector2 point=origin+(walking?Vector2.right*(Mathf.PingPong((float)(Time.realtimeSinceStartupAsDouble-began)*3,3)-1.5f):Vector2.zero);
                        body.position=point;body.transform.position=point;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();
                        foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                        {
                            if(source.clip!=clip||!source.isPlaying)continue;
                            row.requestAccepted=row.sourcePlayingObserved=true;
                            if(firstPlaying<0)firstPlaying=Time.realtimeSinceStartupAsDouble;
                            source.GetOutputData(buffer,0);foreach(float value in buffer)row.sourcePeak=Mathf.Max(row.sourcePeak,Mathf.Abs(value));
                        }
                        AudioListener.GetOutputData(mixed,0);foreach(float value in mixed)row.listenerPeak=Mathf.Max(row.listenerPeak,Mathf.Abs(value));
                        if(firstPlaying>=0&&Time.realtimeSinceStartupAsDouble-firstPlaying>=clip.length+.1)break;
                        yield return null;
                    }
                    rows.Add(row);Assert.That(row.sourcePlayingObserved,Is.True,id+": real context must start its voice");
                    Assert.That(row.sourcePeak,Is.GreaterThan(0),id+": actual source must produce DSP samples under its valid context");
                    health.FullHeal();for(int i=0;i<20;i++){Tick(Facing(6));yield return null;}
                    Assert.That(UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Any(s=>s.clip==clip&&s.isPlaying),Is.False,
                        id+": healing or stopping movement must stop the contextual voice");
                    game.AudioDirector.StopAll();
                }
            }
            finally
            {
                lab.SetPaused(true);DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;health.FullHeal();
                game.AudioDirector.StopAll();foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))source.Stop();
                game.EffectsVolume=effects;game.PlayerVolume=player;game.EnemyVolume=enemy;game.MusicVolume=music;game.AmbientVolume=ambient;game.InterfaceVolume=ui;
                File.WriteAllText(Path.Combine(folder,"contextual-playback-retest.json"),JsonUtility.ToJson(new Playback1850Report{
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
