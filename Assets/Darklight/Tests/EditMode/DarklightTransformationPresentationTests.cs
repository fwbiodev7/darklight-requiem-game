using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightTransformationPresentationTests
    {
        [Test]
        public void CinematicHasPreparationAscentSustainAndLanding()
        {
            Assert.That(DarklightCampaign.TransformationEntryDuration(1),Is.LessThan(2));
            Assert.That(DarklightCampaign.TransformationEntryDuration(2),Is.GreaterThan(4));
            Assert.That(DarklightEnemy.LuciusTransformationDuration,Is.GreaterThan(4));
            Assert.That(DarklightTransformationPresentation.LiftEnvelope(.08f),Is.Zero);
            Assert.That(DarklightTransformationPresentation.LiftEnvelope(.23f),Is.InRange(.1f,.9f));
            Assert.That(DarklightTransformationPresentation.LiftEnvelope(.4f),Is.EqualTo(1));
            Assert.That(DarklightTransformationPresentation.LiftEnvelope(.7f),Is.EqualTo(1));
            Assert.That(DarklightTransformationPresentation.LiftEnvelope(.9f),Is.InRange(.01f,.9f));
            Assert.That(DarklightTransformationPresentation.LiftEnvelope(1),Is.Zero);
        }
        [Test]
        public void LevitationNeverMovesRootOrScaleAndResetRestoresRenderPosition()
        {
            var actor=new GameObject("isolated cinematic actor");
            try
            {
                actor.transform.position=new Vector3(3,4,0);
                var visual=new GameObject("complete figure");visual.transform.SetParent(actor.transform,false);
                var renderer=visual.AddComponent<SpriteRenderer>();
                visual.transform.localScale=Vector3.one*1.325f;
                var transition=actor.AddComponent<DarklightTransformationPresentation>();
                transition.Present(renderer,.5f,true);
                typeof(DarklightTransformationPresentation).GetMethod("LateUpdate",
                    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(transition,null);
                Assert.That(actor.transform.position,Is.EqualTo(new Vector3(3,4,0)));
                Assert.That(visual.transform.position.y,Is.GreaterThan(actor.transform.position.y+.5f));
                Assert.That(visual.transform.localScale,Is.EqualTo(Vector3.one*1.325f));
                transition.ResetPresentation();
                Assert.That(transition.VisualHeight,Is.Zero);
                Assert.That(visual.transform.position,Is.EqualTo(DarklightCharacterView.SnapRenderPosition(actor.transform.position)));
            }
            finally{Object.DestroyImmediate(actor);}
        }
        [Test]
        public void SerafimEntryUsesWholeAwakeningFiguresAndNeverAttackFrames()
        {
            DarklightFabiusFormsBank.ClearCache();
            foreach(float progress in new[]{0f,.2f,.4f,.65f,.85f,1f})
            {
                Assert.That(DarklightFabiusFormsBank.TryTransformationPose(2,0,0,progress,out var sprite),Is.True);
                Assert.That(sprite.name,Does.Not.EndWith("_05").And.Not.EndWith("_06"));
            }
            Assert.That(DarklightFabiusFormsBank.TryTransformationPose(2,2,0,.5f,out _),Is.False,"Katana awakening must not replace fists with a sword");
            Assert.That(DarklightFabiusFormsBank.TryTransformationPose(1,0,0,.5f,out _),Is.False,"Dodge art is not an awakening");
        }
        [Test]
        public void LegacyControllerRejectsInvalidIndicesAndNonfiniteDuration()
        {
            var actor=new GameObject("isolated legacy controller");
            try
            {
                actor.AddComponent<SpriteRenderer>();
                var forms=actor.AddComponent<DarklightFormController>();
                forms.TransformTo((DarklightFormController.FormKind)3);
                forms.TransformTo(DarklightFormController.FormKind.Ruptura,float.NaN);
                forms.TransformTo(DarklightFormController.FormKind.Ruptura,-1);
                Assert.That(forms.CurrentForm,Is.EqualTo(DarklightFormController.FormKind.Normal));
                Assert.That(forms.DamageMultiplier,Is.EqualTo(1));
            }
            finally{Object.DestroyImmediate(actor);}
        }
    }
}
