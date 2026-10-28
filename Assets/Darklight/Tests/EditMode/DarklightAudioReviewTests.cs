using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightAudioReviewTests
    {
        [Test] public void AllTenChaptersHaveDistinctBoundedAmbientProfiles()
        {
            var profiles=new HashSet<Vector3>();
            for(int chapter=0;chapter<10;chapter++)
            {
                var p=DarklightRegionSoundProfile.ForChapter(chapter);
                Assert.That(p.Wind,Is.InRange(0,.4f));Assert.That(p.Veil,Is.InRange(0,.4f));Assert.That(p.Regional,Is.InRange(0,.4f));
                Assert.That(p.Wind+p.Veil+p.Regional,Is.LessThan(.8f));Assert.That(profiles.Add(new Vector3(p.Wind,p.Veil,p.Regional)),Is.True);
            }
        }
        [Test] public void ExternalCreatureEffectHasSafePeakAndMonoImportWithoutPlayback()
        {
            var clip=Resources.Load<AudioClip>("Darklight/Audio/Review20261017/dl-web-creature-alert");
            Assert.That(clip,Is.Not.Null);Assert.That(clip.channels,Is.EqualTo(1));Assert.That(clip.frequency,Is.EqualTo(22050));
            Assert.That(clip.length,Is.InRange(3,4));var samples=new float[clip.samples];Assert.That(clip.GetData(samples,0),Is.True);
            float peak=0;foreach(float v in samples){Assert.That(float.IsNaN(v)||float.IsInfinity(v),Is.False);peak=Mathf.Max(peak,Mathf.Abs(v));}
            Assert.That(peak,Is.InRange(.48f,.51f));var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
            Assert.That(importer.defaultSampleSettings.loadType,Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
        }
    }
}
