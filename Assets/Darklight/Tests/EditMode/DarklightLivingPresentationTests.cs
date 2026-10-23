using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightLivingPresentationTests
    {
        static void Idle(DarklightLivingPresentation p,float time,float hp=1,bool threat=false) =>
            p.Observe(time,CombatState.Neutral,Vector2.zero,hp,0,threat,false);
        [Test] public void HealthHysteresisAvoidsFlickeringAroundCriticalThreshold()
        {
            var p=new DarklightLivingPresentation();Idle(p,0,.24f);Assert.That(p.Clip,Is.EqualTo("IdleWounded"));
            Idle(p,.1f,.29f);Assert.That(p.Critical,Is.True);Idle(p,.2f,.33f);Assert.That(p.Critical,Is.False);
        }
        [Test] public void DecorationIsImmediatelyInterruptedByAnyAttack()
        {
            var p=new DarklightLivingPresentation();for(int i=0;i<90;i++)Idle(p,i*.1f);
            Assert.That(p.Clip,Is.EqualTo("IdleImpatient"));
            p.Observe(9,CombatState.Attack,Vector2.zero,1,0,false,false);Assert.That(p.Clip,Is.EqualTo("Attack"));
        }
        [Test] public void PausedCombatClockFreezesBreathingAndVariations()
        {
            var p=new DarklightLivingPresentation();Idle(p,2);Idle(p,2.1f);float clock=p.BreathClock;
            for(int i=0;i<200;i++)Idle(p,2.1f);Assert.That(p.BreathClock,Is.EqualTo(clock));Assert.That(p.Clip,Is.EqualTo("Idle"));
        }
        [Test] public void TurnAndBrakingNeverChangeTheProvidedMovement()
        {
            var p=new DarklightLivingPresentation();p.Observe(0,CombatState.Neutral,Vector2.right,1,0,false,false);
            p.Observe(.2f,CombatState.Neutral,Vector2.left,1,0,false,false);Assert.That(p.Clip,Is.EqualTo("Turn"));
            Idle(p,.3f);Assert.That(p.Clip,Is.EqualTo("Brake"));Idle(p,.5f);Assert.That(p.Clip,Is.EqualTo("Idle"));
        }
        [Test] public void DeathWinsOverThreatWoundsAndTransformation()
        {
            var p=new DarklightLivingPresentation();p.Observe(1,CombatState.Dead,Vector2.right,.1f,2,true,true);
            Assert.That(p.Clip,Is.EqualTo("Death"));
        }
        [Test] public void RespawnClearsPreviousIdleTimers()
        {
            var p=new DarklightLivingPresentation();for(int i=0;i<90;i++)Idle(p,i*.1f);Idle(p,0);
            Assert.That(p.Clip,Is.EqualTo("Idle"));Assert.That(p.BreathClock,Is.Zero);
        }
        [Test] public void ThreatOverridesCosmeticVariationWithoutLockingControls()
        {
            var p=new DarklightLivingPresentation();Idle(p,0,1,true);Assert.That(p.Clip,Is.EqualTo("IdleAlert"));
            p.Observe(.1f,CombatState.Dodge,Vector2.right,1,0,true,false);Assert.That(p.Clip,Is.EqualTo("Dodge"));
        }
    }
    public sealed class DarklightAttackFrameTimingTests
    {
        [TestCase(0f,0)] [TestCase(.119f,2)] [TestCase(.12f,3)] [TestCase(.219f,5)]
        [TestCase(.22f,6)] [TestCase(.9f,11)]
        public void DetailedFramesRespectActualHitboxPhaseBoundaries(float time,int expected) =>
            Assert.That(DarklightAttackFrameTiming.Frame(12,3,6,time,.12f,.1f,.3f),Is.EqualTo(expected));
        [Test] public void InvalidOrWholeComboDescriptorsAreNotAcceptedImplicitly()
        {
            Assert.That(DarklightAttackFrameTiming.Valid(16,0,0),Is.False);
            Assert.That(DarklightAttackFrameTiming.Valid(16,4,4),Is.False);
            Assert.That(DarklightAttackFrameTiming.Valid(16,4,8),Is.True);
        }
    }
}
