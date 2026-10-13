using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightAudioExpansionTests
    {
        [SetUp] public void Silent() { DarklightAutomation.Silent=true; }
        [Test] public void EveryTerrainHasDistinctPrefixAndSeveralImportedSamples()
        {
            var prefixes=new HashSet<string>();
            foreach(TodayFootstepSurface surface in Enum.GetValues(typeof(TodayFootstepSurface)))
            {
                string prefix=TodaySurfaces.Prefix(surface);Assert.That(prefixes.Add(prefix),Is.True,surface.ToString());
                var samples=Array.FindAll(Resources.LoadAll<AudioClip>("Darklight/Audio"),c=>c.name.StartsWith(prefix));
                Assert.That(samples.Length,Is.GreaterThanOrEqualTo(3),surface.ToString());
            }
            Assert.That(prefixes.Count,Is.EqualTo(9));
        }
        [Test] public void EveryEnemyAndBossHasItsOwnIdentityAndBossSignature()
        {
            var all=Resources.LoadAll<AudioClip>("Darklight/Audio/Expansion");var prefixes=new HashSet<string>();
            foreach(RequiemEnemyKind kind in Enum.GetValues(typeof(RequiemEnemyKind)))
            {
                string prefix=DarklightSoundDirector.EnemyPrefix(kind);Assert.That(prefixes.Add(prefix),Is.True);
                Assert.That(Array.FindAll(all,c=>c.name.StartsWith(prefix+"_",StringComparison.Ordinal)).Length,Is.EqualTo(2),kind.ToString());
                if((int)kind>=5) Assert.That(Array.Find(all,c=>c.name=="dl-boss-"+kind.ToString().ToLowerInvariant()),Is.Not.Null,kind.ToString());
            }
        }
        [Test] public void OriginalExpansionIsMonoAndBoundedWithStreamedAmbientLayers()
        {
            var clips=Resources.LoadAll<AudioClip>("Darklight/Audio/Expansion");Assert.That(clips.Length,Is.EqualTo(82));
            long pcm=0;int streamed=0;
            foreach(var clip in clips)
            {
                Assert.That(clip.channels,Is.EqualTo(1));Assert.That(clip.frequency,Is.EqualTo(22050));
                if(clip.loadType==AudioClipLoadType.Streaming)streamed++;
                else pcm+=(long)clip.samples*clip.channels*sizeof(float);
            }
            Assert.That(streamed,Is.EqualTo(6));Assert.That(pcm,Is.LessThan(5000000L));
        }
        [Test] public void SkinThemesUseStreamingStereoAndBackgroundLoading()
        {
            foreach(string name in new[]{"RockTheme","RequiemTheme"})
            {
                var clip=Resources.Load<AudioClip>("Darklight/Music/"+name);Assert.That(clip,Is.Not.Null);
                Assert.That(clip.length,Is.GreaterThan(30));Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.Streaming));Assert.That(clip.channels,Is.EqualTo(2));
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                Assert.That(importer.loadInBackground,Is.True);Assert.That(importer.defaultSampleSettings.preloadAudioData,Is.False);
            }
        }
        [Test] public void ContactCadenceRequiresWalkingAndRealDisplacement()
        {
            var contacts=new TodayStepContacts();
            Assert.That(contacts.Tick(0,true,Vector2.zero),Is.False);
            Assert.That(contacts.Tick(.1f,true,Vector2.right*.1f),Is.False);
            Assert.That(contacts.Tick(.2f,true,Vector2.right*.2f),Is.True);
            Assert.That(contacts.Tick(.4f,false,Vector2.right*.3f),Is.False,"Attack or dodge");
            Assert.That(contacts.Tick(.6f,false,Vector2.right*.4f),Is.False,"Airborne or paused");
            Assert.That(contacts.Tick(.8f,true,Vector2.right*.5f),Is.False,"First walking sample after an incompatible action");
            Assert.That(contacts.Tick(1f,true,Vector2.right*.6f),Is.True);
            Assert.That(contacts.Tick(1.2f,true,Vector2.right*.6f),Is.False,"Blocked by wall");
            Assert.That(contacts.Tick(1.4f,true,Vector2.right*20),Is.False,"Warp is not a step");
        }
        [Test] public void HigherPriorityTerrainAreaOverridesRegionalFloor()
        {
            var low=new GameObject("LowSurface");var high=new GameObject("HighSurface");
            try
            {
                low.AddComponent<BoxCollider2D>().size=Vector2.one*4;var a=low.AddComponent<TodaySurfaceArea>();a.surface=TodayFootstepSurface.Sand;a.priority=1;
                high.AddComponent<BoxCollider2D>().size=Vector2.one*2;var b=high.AddComponent<TodaySurfaceArea>();b.surface=TodayFootstepSurface.Water;b.priority=2;
                typeof(TodaySurfaceArea).GetMethod("OnEnable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(a,null);
                typeof(TodaySurfaceArea).GetMethod("OnEnable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(b,null);
                Physics2D.SyncTransforms();Assert.That(TodaySurfaces.Resolve(0,Vector2.zero,Vector2.zero),Is.EqualTo(TodayFootstepSurface.Water));
                Assert.That(TodaySurfaces.Resolve(0,Vector2.right*1.5f,Vector2.zero),Is.EqualTo(TodayFootstepSurface.Sand));
            }
            finally{UnityEngine.Object.DestroyImmediate(high);UnityEngine.Object.DestroyImmediate(low);}
        }
        [Test] public void VoiceCapsAndPriorityPredicatesKeepCrowdsAndTransitionsBounded()
        {
            Assert.That(DarklightSoundDirector.CategoryVoiceLimit(DarklightAudioCategory.Enemy),Is.EqualTo(4));
            Assert.That(DarklightSoundDirector.CategoryVoiceLimit(DarklightAudioCategory.Ambient),Is.EqualTo(2));
            Assert.That(DarklightSoundDirector.CategoryVoiceLimit(DarklightAudioCategory.Combat),Is.EqualTo(16));
            Assert.That(TodaySkinMusic.ShouldSuspend(true,true,false,false),Is.True);
            Assert.That(TodaySkinMusic.ShouldSuspend(true,false,true,false),Is.True);
            Assert.That(TodaySkinMusic.ShouldSuspend(true,false,false,true),Is.True);
            Assert.That(TodaySkinMusic.ShouldSuspend(false,false,false,false),Is.True);
            Assert.That(TodaySkinMusic.ShouldSuspend(true,false,false,false),Is.False);
            Assert.That(TodaySkinMusic.Fade(0,1,TodaySkinMusic.TransitionSeconds),Is.EqualTo(1));
            Assert.That(TodaySkinMusic.Fade(1,0,-1),Is.EqualTo(1));
        }
    }
}