using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Darklight.Tests
{
    public sealed class DarklightOfficialTransitionBankTests
    {
        [SetUp]public void Reset()=>DarklightOfficialTransitionBank.ClearCache();
        [TestCase(1,"FabiusRupturaOfficial20261114")]
        [TestCase(2,"FabiusSerafimOfficial20261114")]
        [TestCase(3,"LuciusArconteOfficial20261114")]
        public void AllEightApprovedFiguresPlayInOrderWithAuthoredHolds(int kind,string name)
        {
            int[] durations=kind==1?new[]{100,150,150,200,200,150,150,150}:kind==2?new[]{350,450,550,1050,850,600,450,300}:new[]{350,400,550,900,800,600,350,250};
            int total=0;foreach(int duration in durations)total+=duration;
            int elapsed=0;
            for(int i=0;i<8;i++)
            {
                float middle=(elapsed+durations[i]*.5f)/total;
                Assert.That(DarklightOfficialTransitionBank.FrameIndex(kind,middle),Is.EqualTo(i));
                Assert.That(DarklightOfficialTransitionBank.TryPose(kind,middle,out var sprite),Is.True);
                Assert.That(sprite.name,Is.EqualTo("Redesign_"+name+"_"+i.ToString("00")));
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(sprite),Is.True);
                elapsed+=durations[i];
            }
            Assert.That(DarklightOfficialTransitionBank.FrameIndex(kind,0),Is.Zero);
            Assert.That(DarklightOfficialTransitionBank.FrameIndex(kind,1),Is.EqualTo(7));
            Assert.That(total/1000f,Is.EqualTo(kind==1?DarklightCampaign.RupturaEnterDuration:kind==2?DarklightCampaign.TransformationEnterDuration:DarklightEnemy.LuciusTransformationDuration).Within(.001f));
        }
        [TestCase(1)]
        [TestCase(2)]
        public void ApprovedFabiusIdleIsLimitedToFrontalStationaryKatana(int form)
        {
            var model=new CombatModel(new CombatLoadout{weapon=0});
            Assert.That(DarklightFabiusFormsBank.TryResolveFormPose(form,0,0,model,Vector2.zero,out var idle),Is.True);
            Assert.That(DarklightOfficialTransitionBank.IsAuthored(idle),Is.True);
            DarklightFabiusFormsBank.TryResolveFormPose(form,0,0,model,Vector2.down,out var walking);
            Assert.That(DarklightOfficialTransitionBank.IsAuthored(walking),Is.False);
            DarklightFabiusFormsBank.TryResolveFormPose(form,0,4,model,Vector2.zero,out var rearIdle);
            Assert.That(DarklightOfficialTransitionBank.IsAuthored(rearIdle),Is.False);
            DarklightFabiusFormsBank.TryResolveFormPose(form,2,0,model,Vector2.zero,out var fists);
            Assert.That(DarklightOfficialTransitionBank.IsAuthored(fists),Is.False);
        }
        [Test]
        public void ApprovedArconteIdlePreservesOtherActionsDirectionsAndExplicitCancellation()
        {
            var actor=new GameObject("Arconte idle contract");
            try
            {
                var view=actor.AddComponent<DarklightLuciusView>();view.appearance=DarklightLuciusAppearance.Arconte;
                Assert.That(view.PresentPose("Idle",0,0),Is.True);
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.True);
                Assert.That(view.PresentPose("Idle",0,0,null,false),Is.True);
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.False);
                view.PresentPose("Walk",0,0);
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.False);
                view.PresentPose("Idle",4,0);
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.False);
                view.appearance=DarklightLuciusAppearance.Normal;view.PresentPose("Idle",0,0);
                Assert.That(DarklightOfficialTransitionBank.IsAuthored(view.Renderer.sprite),Is.False);
            }
            finally{Object.DestroyImmediate(actor);}
        }
        [Test]
        public void InvalidKindAndNonfiniteProgressCannotSelectArt()
        {
            Assert.That(DarklightOfficialTransitionBank.TryPose(0,.5f,out _),Is.False);
            Assert.That(DarklightOfficialTransitionBank.TryPose(4,.5f,out _),Is.False);
            Assert.That(DarklightOfficialTransitionBank.TryPose(1,float.NaN,out _),Is.False);
            Assert.That(DarklightOfficialTransitionBank.TryPose(2,float.PositiveInfinity,out _),Is.False);
        }
        [TestCase("FabiusRupturaOfficial20261114","FabioVisual",1.25f)]
        [TestCase("FabiusSerafimOfficial20261114","FabioVisual",4.6f)]
        [TestCase("LuciusArconteOfficial20261114","LuciusVisual",4.2f)]
        public void BoundAnimationClipMatchesControllerDurationAndFrameOrder(string name,string path,float duration)
        {
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Darklight/Animations/OfficialTransitions20261114/"+name+".anim");
            Assert.That(clip,Is.Not.Null);
            var bindings=AnimationUtility.GetObjectReferenceCurveBindings(clip);
            Assert.That(bindings.Length,Is.EqualTo(1));Assert.That(bindings[0].path,Is.EqualTo(path));
            var frames=AnimationUtility.GetObjectReferenceCurve(clip,bindings[0]);
            Assert.That(frames.Length,Is.EqualTo(9));
            for(int i=0;i<8;i++)Assert.That(frames[i].value.name,Is.EqualTo("Redesign_"+name+"_"+i.ToString("00")));
            Assert.That(frames[8].time,Is.EqualTo(duration).Within(.001f));
            Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime,Is.False);
        }
        [Test]
        public void ApprovedWholeFigureOwnsItsColorsAndIsNeverMirrored()
        {
            Assert.That(DarklightOfficialTransitionBank.TryPose(2,.5f,out var sprite),Is.True);
            var actor=new GameObject("Official ownership contract");
            try
            {
                var body=actor.AddComponent<SpriteRenderer>();body.sprite=sprite;body.color=Color.red;body.flipX=true;
                var block=new MaterialPropertyBlock();block.SetFloat("_VampireForm",2);block.SetFloat("_FormIntensity",1);block.SetVector("_CanonicalSkinShift",Vector4.one);body.SetPropertyBlock(block);
                DarklightOfficialTransitionBank.ApplyOwnership(body);body.GetPropertyBlock(block);
                Assert.That(block.GetFloat("_VampireForm"),Is.Zero);
                Assert.That(block.GetFloat("_FormIntensity"),Is.Zero);
                Assert.That(block.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
                Assert.That(body.color,Is.EqualTo(Color.white));Assert.That(body.flipX,Is.False);
            }
            finally{Object.DestroyImmediate(actor);}
        }
    }
}
