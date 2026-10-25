using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightDeliveredSfxTests
    {
        [SetUp] public void Silent() { DarklightAutomation.Silent=true; }
        [Test] public void SuppliedCatalogueContainsNineteenCuesAndRejectsUnreviewedLongSources()
        {
            var unique=new HashSet<string>();
            foreach(var id in DarklightSoundDirector.DeliveredIds)
            {
                Assert.That(unique.Add(id),Is.True,id);
                Assert.That(DarklightSoundDirector.DeliveredPrefix(id),Is.EqualTo("dl-user-"+id.ToLowerInvariant()));
            }
            Assert.That(unique.Count,Is.EqualTo(19));
            foreach(var id in new[]{"WM02","WM06","PW02","PW09","Going Under","Afterlife","See U in Hell",null,""})
                Assert.That(DarklightSoundDirector.DeliveredPrefix(id),Is.Null,id);
        }
        [Test] public void ImportedDeliveryIsBoundedMonoPcmWithPeakHeadroom()
        {
            var clips=Resources.LoadAll<AudioClip>("Darklight/Audio/Delivery20261023");
            Assert.That(clips.Length,Is.EqualTo(19));long memory=0;
            foreach(var id in DarklightSoundDirector.DeliveredIds)
            {
                var clip=Array.Find(clips,c=>c.name==DarklightSoundDirector.DeliveredPrefix(id));
                Assert.That(clip,Is.Not.Null,id);
                Assert.That(clip.channels,Is.EqualTo(1),id);
                Assert.That(clip.frequency,Is.EqualTo(24000).Or.EqualTo(32000),id);
                Assert.That(clip.length,Is.InRange(.2f,6.6f),id);
                Assert.That(clip.loadType,Is.EqualTo(AudioClipLoadType.DecompressOnLoad),id);
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                Assert.That(importer.defaultSampleSettings.compressionFormat,Is.EqualTo(AudioCompressionFormat.PCM),id);
                Assert.That(importer.forceToMono,Is.False,"Already mono; avoid importer normalization");
                var data=new float[clip.samples];Assert.That(clip.GetData(data,0),Is.True,id);
                float peak=0;foreach(float sample in data)peak=Mathf.Max(peak,Mathf.Abs(sample));
                Assert.That(peak,Is.InRange(.01f,.901f),id);
                memory+=(long)clip.samples*4;
            }
            Assert.That(memory,Is.LessThan(5000000L));
        }
        [Test] public void WeaponAndPowerMappingsPreserveSemanticFallbacks()
        {
            var attack=ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                Assert.That(DarklightSoundDirector.SwingDelivery(attack,0),Is.Null,"Long katana source remains excluded");
                Assert.That(DarklightSoundDirector.SwingDelivery(attack,1),Is.EqualTo("WM03"));
                attack.ruinStrike=DarklightRuinStrike.SideKick;
                Assert.That(DarklightSoundDirector.SwingDelivery(attack,2),Is.EqualTo("WM05"));
                attack.ruinStrike=DarklightRuinStrike.SpinKick;
                Assert.That(DarklightSoundDirector.SwingDelivery(attack,2),Is.EqualTo("WM05"));
                attack.ruinStrike=DarklightRuinStrike.Uppercut;
                Assert.That(DarklightSoundDirector.SwingDelivery(attack,2),Is.Null,"Do not play the long multi-impact source");
            }
            finally{UnityEngine.Object.DestroyImmediate(attack);}
            Assert.That(DarklightSoundDirector.AbilityDelivery(0),Is.Null,"No claws replacement supplied");
            Assert.That(DarklightSoundDirector.AbilityDelivery(1),Is.Null,"Long teleport source remains excluded");
            Assert.That(DarklightSoundDirector.AbilityDelivery(2),Is.EqualTo("PW03"));
            Assert.That(DarklightSoundDirector.AbilityDelivery(3),Is.EqualTo("PW04"));
            Assert.That(DarklightSoundDirector.AbilityDelivery(4),Is.EqualTo("PW05"));
            Assert.That(DarklightSoundDirector.AbilityDelivery(5),Is.EqualTo("PW06"));
        }
        [Test] public void DeliveredRequestsCannotAllocateOrPlayUnderSilentAutomation()
        {
            var go=new GameObject("DeliveredSilentGuard");
            try
            {
                var director=go.AddComponent<DarklightSoundDirector>();
                foreach(var id in DarklightSoundDirector.DeliveredIds)
                    Assert.That(director.Delivered(id,DarklightAudioCategory.Character,.3f,.1f,true),Is.False,id);
                Assert.That(go.GetComponentsInChildren<AudioSource>().Length,Is.Zero);
                Assert.That(director.ActiveVoices,Is.Zero);
            }
            finally{UnityEngine.Object.DestroyImmediate(go);}
        }
    }
}
