using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightMotorcycleRedesignTests
    {
        [System.Serializable] sealed class TransitionSheet
        {
            public bool wholeFigures;
            public int[] nativeSize;
            public float pixelsPerUnit;
            public WholeFrame[] frames;
            public TransitionEntry motorcycleTransition;
        }
        [System.Serializable] sealed class WholeFrame {public string id;public int[] rect;public float[] pivot;public int durationMs;}
        [System.Serializable] sealed class TransitionEntry {public string[] assemble,separate;}

        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void MissingDirectionalTransitionReusesExactWholeEndpointsWithReversibleHolds(int direction)
        {
            DarklightMotorcycleRedesignBank.ClearCache();
            Assert.That(Resources.Load<TextAsset>(DarklightMotorcycleRedesignBank.TransitionResource(direction)),Is.Null,
                "A new authored direction must be tested as choreography instead of two reused poses");
            Assert.That(DarklightMotorcycleRedesignBank.TransitionUsesReusedPoses(direction),Is.True);
            var onFoot=DarklightMotorcycleRedesignBank.DualSawsPose(direction);
            var mounted=DarklightMotorcycleRedesignBank.Riding(0,direction,0);
            AssertWholeFrame(onFoot,DarklightMotorcycleRedesignBank.RequiemDualSawsResource);
            AssertWholeFrame(mounted,DarklightMotorcycleRedesignBank.RequiemResource(0));
            Assert.That(onFoot.name,Does.EndWith("_"+direction.ToString("00")));
            Assert.That(mounted.name,Does.EndWith("_"+direction.ToString("00")));
            Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,true,0),Is.SameAs(onFoot));
            Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,true,1),Is.SameAs(mounted));
            Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,false,0),Is.SameAs(mounted));
            Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,false,1),Is.SameAs(onFoot));
            foreach(float progress in new[]{0f,.125f,.25f,.499f,.5f,.501f,.75f,.875f,1f})
            {
                var pose=DarklightMotorcycleRedesignBank.TransitionPose(direction,true,progress);
                Assert.That(pose,Is.SameAs(progress<.5f?onFoot:mounted),"Intentional hold at "+progress);
                Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,false,1-progress),Is.SameAs(pose),
                    "Reverse must sample the same whole pose at "+progress);
                Assert.That(DarklightMotorcycleRedesignBank.IsTransition(pose),Is.True);
            }
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void TransitionsClampFiniteProgressAndRejectInvalidTime(int direction)
        {
            foreach(bool assemble in new[]{true,false})
            {
                Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,assemble,-4),
                    Is.SameAs(DarklightMotorcycleRedesignBank.TransitionPose(direction,assemble,0)));
                Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,assemble,4),
                    Is.SameAs(DarklightMotorcycleRedesignBank.TransitionPose(direction,assemble,1)));
                foreach(float progress in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity})
                    Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,assemble,progress),Is.Null);
            }
        }

        [Test]
        public void DirectionalTransitionsNeverBorrowOtherDirectionsAndCacheCanReload()
        {
            DarklightMotorcycleRedesignBank.ClearCache();
            var onFoot=new System.Collections.Generic.HashSet<Sprite>();var mounted=new System.Collections.Generic.HashSet<Sprite>();
            for(int direction=0;direction<8;direction++)
            {
                var first=DarklightMotorcycleRedesignBank.TransitionPose(direction,true,0);
                var last=DarklightMotorcycleRedesignBank.TransitionPose(direction,true,1);
                Assert.That(first,Is.Not.Null);Assert.That(last,Is.Not.Null);
                Assert.That(onFoot.Add(first),Is.True);Assert.That(mounted.Add(last),Is.True);
                Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(first),Is.True);
                Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(last),Is.True);
            }
            foreach(int direction in new[]{-1,8})
            {
                Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(direction,true,0),Is.Null);
                Assert.That(DarklightMotorcycleRedesignBank.TransitionUsesReusedPoses(direction),Is.False);
            }
            var previous=DarklightMotorcycleRedesignBank.TransitionPose(4,true,0);
            DarklightMotorcycleRedesignBank.ClearCache();
            Assert.That(DarklightMotorcycleRedesignBank.IsTransition(previous),Is.False);
            var reloaded=DarklightMotorcycleRedesignBank.TransitionPose(4,true,0);
            Assert.That(reloaded,Is.SameAs(previous));
            Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(reloaded),Is.True);
            Assert.That(DarklightMotorcycleRedesignBank.IsTransition(reloaded),Is.True);
        }

        [Test]
        public void SouthRetainsAuthoredAssembleAndSeparateOrderAndTimings()
        {
            DarklightMotorcycleRedesignBank.ClearCache();
            string resource=DarklightMotorcycleRedesignBank.TransitionResource(0);
            var sheet=JsonUtility.FromJson<TransitionSheet>(Resources.Load<TextAsset>(resource).text);
            Assert.That(DarklightMotorcycleRedesignBank.TransitionUsesReusedPoses(0),Is.False);
            Assert.That(sheet.motorcycleTransition.assemble.Length,Is.EqualTo(6));
            Assert.That(sheet.motorcycleTransition.separate.Length,Is.EqualTo(7));
            foreach(bool assemble in new[]{true,false})
            {
                var ids=assemble?sheet.motorcycleTransition.assemble:sheet.motorcycleTransition.separate;
                int total=0;
                foreach(string id in ids)total+=System.Array.Find(sheet.frames,frame=>frame.id==id).durationMs;
                int elapsed=0;
                foreach(string id in ids)
                {
                    int duration=System.Array.Find(sheet.frames,frame=>frame.id==id).durationMs;
                    var pose=DarklightMotorcycleRedesignBank.TransitionPose(0,assemble,(elapsed+duration*.5f)/total);
                    Assert.That(pose,Is.Not.Null);Assert.That(pose.name,Is.EqualTo(id));
                    AssertWholeFrame(pose,resource);elapsed+=duration;
                }
                Assert.That(DarklightMotorcycleRedesignBank.TransitionPose(0,assemble,1).name,Is.EqualTo(ids[ids.Length-1]));
            }
        }

        static void AssertWholeFrame(Sprite sprite,string resource)
        {
            Assert.That(sprite,Is.Not.Null);
            var sheet=JsonUtility.FromJson<TransitionSheet>(Resources.Load<TextAsset>(resource).text);
            Assert.That(sheet.wholeFigures,Is.True);
            var frame=System.Array.Find(sheet.frames,value=>value.id==sprite.name);
            Assert.That(frame,Is.Not.Null);
            Assert.That(sprite.texture,Is.SameAs(Resources.Load<Texture2D>(resource)),"Reuse the source texture, not a generated fragment");
            Assert.That(sprite.rect,Is.EqualTo(new Rect(frame.rect[0],sheet.nativeSize[1]-frame.rect[1]-frame.rect[3],frame.rect[2],frame.rect[3])));
            Assert.That(sprite.pivot.x,Is.EqualTo(frame.pivot[0]*frame.rect[2]).Within(.01f));
            Assert.That(sprite.pivot.y,Is.EqualTo(frame.pivot[1]*frame.rect[3]).Within(.01f));
            Assert.That(sprite.pixelsPerUnit,Is.EqualTo(sheet.pixelsPerUnit).Within(.001f));
            Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(sprite),Is.True);
        }

        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void EachMotorcycleFormUsesEightDistinctDrawnViewsAndFourExactSlamStages(int form)
        {
            DarklightMotorcycleRedesignBank.ClearCache();
            var views=new System.Collections.Generic.HashSet<Sprite>();
            for(int direction=0;direction<8;direction++)
            {
                var sprite=DarklightMotorcycleRedesignBank.Riding(form,direction);
                Assert.That(sprite,Is.Not.Null,"Missing authored motorcycle direction "+direction);
                Assert.That(views.Add(sprite),Is.True,"Asymmetric directions need separate drawings");
                Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(sprite),Is.True);
            }
            var phases=new System.Collections.Generic.HashSet<Sprite>();
            foreach(float time in new[]{0f,.3f,.6f,.9f})
            {
                var sprite=DarklightMotorcycleRedesignBank.GroundSlam(form,0,time,.58f,.16f,.58f);
                Assert.That(sprite,Is.Not.Null);Assert.That(phases.Add(sprite),Is.True);
            }
            for(int direction=1;direction<8;direction++)
                Assert.That(DarklightMotorcycleRedesignBank.GroundSlam(form,direction,.6f,.58f,.16f,.58f),Is.Null,
                    "A frontal slam cannot stand in for another direction");
            Assert.That(DarklightMotorcycleRedesignBank.Riding(-1,0),Is.Null);
            Assert.That(DarklightMotorcycleRedesignBank.Riding(4,0),Is.Null);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void AuthoredMotorcycleClearsLegacyMasksAndPaletteWithoutTintingRider(int form)
        {
            var holder=new GameObject("Motorcycle palette verification");
            try
            {
                var renderer=holder.AddComponent<SpriteRenderer>();
                renderer.sprite=DarklightMotorcycleRedesignBank.Riding(form,6);Assert.That(renderer.sprite,Is.Not.Null);
                var properties=new MaterialPropertyBlock();
                foreach(string key in new[]{"_VampireForm","_DarkForm","_RuinFists","_WholeFrameId"})properties.SetFloat(key,3);
                properties.SetTexture("_VampireMask",Texture2D.whiteTexture);
                properties.SetVector("_CanonicalSkinShift",Vector4.one);renderer.SetPropertyBlock(properties);
                DarklightMotorcycleRedesignBank.ApplyOwnership(renderer);renderer.GetPropertyBlock(properties);
                foreach(string key in new[]{"_VampireForm","_DarkForm","_RuinFists","_WholeFrameId"})Assert.That(properties.GetFloat(key),Is.Zero,key);
                Assert.That(properties.GetTexture("_VampireMask"),Is.SameAs(Texture2D.blackTexture));
                Assert.That(properties.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
            }
            finally{Object.DestroyImmediate(holder);DarklightMotorcycleRedesignBank.ClearCache();}
        }
    }
}
