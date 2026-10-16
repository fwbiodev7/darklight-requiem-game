using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        [UnityTest]public IEnumerator ImpactPauseRetainsBikeAndMountedDriftWithoutDoublePayment()
        {
            PracticeVictim(Vector2.down*.85f);game.Blood.Set(100);
            StepExpanded(.01f,new CombatInput{lightPressed=true,lightHeld=true,aim=Vector2.down});StepExpanded(.27f,new CombatInput{lightHeld=true,aim=Vector2.down},true);
            lab.SetPaused(false);StepExpanded(0,new CombatInput{weaponSpecialPressed=true,lightHeld=true,aim=Vector2.down});lab.SetPaused(true);
            float spent=0;for(int i=0;i<60&&!rider.Dashing;i++)spent+=StepExpanded(.01f,new CombatInput{lightHeld=true,aim=Vector2.down});
            Assert.That(rider.Dashing,Is.True,"Bike input during hitstop must survive");Assert.That(spent,Is.EqualTo(18).Within(.001f));
            StepExpanded(.47f,new CombatInput{aim=Vector2.down});lab.SetPaused(false);StepExpanded(0,new CombatInput{heavyPressed=true,move=Vector2.right});lab.SetPaused(true);
            spent=0;for(int i=0;i<60&&!rider.Drifting;i++)spent+=StepExpanded(.01f,new CombatInput());
            Assert.That(rider.Drifting,Is.True,"Mounted input during hitstop must survive");Assert.That(spent,Is.EqualTo(10).Within(.001f));yield return null;
        }
        [UnityTest]public IEnumerator BufferedBikeWinsOverHeldSawAndPaysOnceAfterContact()
        {
            PracticeVictim(Vector2.down*.85f);game.Blood.Set(100);
            StepExpanded(.01f,new CombatInput{lightPressed=true,lightHeld=true,aim=Vector2.down});
            StepExpanded(.27f,new CombatInput{lightHeld=true,aim=Vector2.down},true);Assert.That(fighter.Model.AttackConnected,Is.True);
            StepExpanded(.01f,new CombatInput{weaponSpecialPressed=true,lightHeld=true,aim=Vector2.down});Assert.That(rider.Dashing||rider.Sustaining,Is.False);
            float cost=0;for(int i=0;i<50&&!rider.Dashing;i++){cost+=StepExpanded(.01f,new CombatInput{lightHeld=true,aim=Vector2.down});Assert.That(rider.Sustaining,Is.False,"Holding saws must not steal the bike command");}
            Assert.That(rider.Dashing,Is.True);Assert.That(cost,Is.EqualTo(18).Within(.001f));float blood=game.Blood.Amount;
            for(int i=0;i<30;i++)rider.AfterCombat();Assert.That(game.Blood.Amount,Is.EqualTo(blood));
            yield return null;
        }
        [UnityTest]public IEnumerator BufferedBikeExpiresAndInterruptionDoesNotReplayIt()
        {
            PracticeVictim(Vector2.down*.85f);game.Blood.Set(100);
            StepExpanded(.01f,new CombatInput{lightPressed=true,aim=Vector2.down});StepExpanded(.01f,new CombatInput{weaponSpecialPressed=true,aim=Vector2.down});
            for(int i=0;i<140;i++){StepExpanded(.01f,new CombatInput{aim=Vector2.down},true);Assert.That(rider.Dashing,Is.False,"Early commands must expire rather than fire late");}
            fighter.ResetModel();StepExpanded(.01f,new CombatInput{lightPressed=true,aim=Vector2.down});StepExpanded(.27f,new CombatInput{aim=Vector2.down},true);
            StepExpanded(.01f,new CombatInput{weaponSpecialPressed=true});fighter.Model.Hurt(false,.18f);
            for(int i=0;i<100;i++){StepExpanded(.01f,new CombatInput());Assert.That(rider.Dashing,Is.False,"Hurt must clear the buffered bike command");}
            yield return null;
        }
        [UnityTest]public IEnumerator ReleasedSustainQueuesBikeWithoutCuttingPaidContactOrRecovery()
        {
            PracticeVictim(Vector2.down*.85f);game.Blood.Set(100);BeginHeldSaw();StepExpanded(.2f,new CombatInput{lightHeld=true,aim=Vector2.down},true);
            Assert.That(rider.Sustaining,Is.True);StepExpanded(.01f,new CombatInput{weaponSpecialPressed=true,lightReleased=true,aim=Vector2.down});
            Assert.That(rider.Dashing,Is.False);Assert.That(fighter.Model.ActiveThisTick,Is.False);
            float cost=0;for(int i=0;i<45&&!rider.Dashing;i++)
            {if(i<17)Assert.That(rider.Dashing,Is.False);cost+=StepExpanded(.01f,new CombatInput{aim=Vector2.down});}
            Assert.That(rider.Dashing,Is.True);Assert.That(cost,Is.EqualTo(18).Within(.001f));
            yield return null;
        }
        [UnityTest]public IEnumerator DriftCannotRemountOnceVaultHasStarted()
        {
            BeginDash(Vector2.down);var attack=fighter.Model.Attack;
            StepExpanded(attack.active+.14f,new CombatInput{aim=Vector2.down});Assert.That(rider.JumpHeight,Is.GreaterThan(.05f));float blood=game.Blood.Amount;
            StepExpanded(.01f,new CombatInput{heavyPressed=true,aim=Vector2.down});
            Assert.That(rider.Drifting,Is.False);Assert.That(game.Blood.Amount,Is.EqualTo(blood));Assert.That(fighter.Model.Attack,Is.SameAs(attack));
            yield return null;
        }
        [UnityTest]public IEnumerator MountedSlamLiftsBikeWithoutFirstSplittingAndPaysOnce()
        {
            BeginDash(Vector2.down);StepExpanded(.01f,new CombatInput{skillPressed=true,aim=Vector2.down});Assert.That(rider.Slamming,Is.False);
            float spent=0;for(int i=0;i<50&&!rider.Slamming;i++)spent+=StepExpanded(.01f,new CombatInput{aim=Vector2.down});
            Assert.That(rider.Slamming,Is.True);Assert.That(rider.SlamFromMounted,Is.True);Assert.That(spent,Is.EqualTo(22).Within(.001f));Assert.That(rider.JumpHeight,Is.Zero);
            var view=fighter.GetComponent<DarklightCharacterView>();view.Present(fighter.Model,Vector2.zero);
            Assert.That(fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>().sprite.name,Is.EqualTo("Abyss_Join_0_3"));
            float blood=game.Blood.Amount;for(int i=0;i<30;i++)rider.AfterCombat();Assert.That(game.Blood.Amount,Is.EqualTo(blood));
            yield return null;
        }
    }
}
