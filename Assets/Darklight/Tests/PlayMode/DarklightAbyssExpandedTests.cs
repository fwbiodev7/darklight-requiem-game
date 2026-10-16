using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        // Uses the real rider, model, collider and strike resolver in the loaded scene.
        float StepExpanded(float dt,CombatInput input,bool strike=false)
        {
            float before=game.Blood.Amount;rider.BeforeCombat(dt,ref input);
            fighter.Model.Tick(dt,input,rider.AttackClockSpeed(game.AttackSpeed));rider.AfterCombat();
            float spent=Mathf.Max(0,before-game.Blood.Amount);
            if(strike&&fighter.Model.ActiveThisTick)Call(fighter,"ResolveStrike");return spent;
        }
        DarklightEnemy PracticeVictim(Vector2 offset)
        {
            var e=game.MapEnemies[0];e.enabled=false;e.Health.SetMaxHealth(100000);e.Health.FullHeal();
            e.transform.position=(Vector3)(game.Map.Spawn+offset);e.GetComponent<Rigidbody2D>().position=e.transform.position;Physics2D.SyncTransforms();return e;
        }
        void BeginHeldSaw()
        {
            StepExpanded(.01f,new CombatInput{lightPressed=true,lightHeld=true,aim=Vector2.down});
            for(int i=0;i<90&&!rider.Sustaining;i++)StepExpanded(.01f,new CombatInput{lightHeld=true,aim=Vector2.down},true);
            Assert.That(rider.Sustaining,Is.True,"A connected held saw must enter the bounded sustained attack");
        }
        [UnityTest]public IEnumerator SustainedSawsPayEachPulseOnceStopOnReleaseAndExhaustion()
        {
            PracticeVictim(Vector2.down*.85f);game.Blood.Set(100);BeginHeldSaw();Assert.That(game.Blood.Amount,Is.EqualTo(94).Within(.001f));
            float spent=0;uint contacts=0;int serial=fighter.Model.AttackSerial;
            for(int i=0;i<180&&fighter.Model.State==CombatState.Attack;i++)
            {
                spent+=StepExpanded(.01f,new CombatInput{lightHeld=true,aim=Vector2.down},true);contacts|=fighter.Model.ContactPulses;
                float blood=game.Blood.Amount,health=game.MapEnemies[0].Health.CurrentHealth;
                for(int duplicate=0;duplicate<20;duplicate++){rider.AfterCombat();if(fighter.Model.ActiveThisTick)Call(fighter,"ResolveStrike");}
                Assert.That(game.Blood.Amount,Is.EqualTo(blood));Assert.That(game.MapEnemies[0].Health.CurrentHealth,Is.EqualTo(health));
                Assert.That(fighter.Model.AttackSerial,Is.EqualTo(serial),"Holding must not restart forever or grow gears automatically");
            }
            Assert.That(contacts,Is.EqualTo(63u));Assert.That(spent,Is.EqualTo(12).Within(.001f));Assert.That(rider.Gear.Level,Is.EqualTo(1));
            fighter.ResetModel();game.Blood.Set(100);PracticeVictim(Vector2.down*.85f);BeginHeldSaw();
            StepExpanded(.14f,new CombatInput{lightHeld=true,aim=Vector2.down},true);StepExpanded(.01f,new CombatInput{lightReleased=true});
            Assert.That(fighter.Model.ActiveThisTick,Is.False,"Release: "+fighter.Model.Attack.displayName+" time="+fighter.Model.StateTime+" sustained="+rider.Sustaining);Assert.That(fighter.Model.StateTime,Is.GreaterThanOrEqualTo(fighter.Model.Attack.startup+fighter.Model.Attack.active));
            fighter.ResetModel();game.Blood.Set(100);PracticeVictim(Vector2.down*.85f);BeginHeldSaw();game.Blood.Set(1);
            StepExpanded(.15f,new CombatInput{lightHeld=true,aim=Vector2.down},true);Assert.That(fighter.Model.ActiveThisTick,Is.False,"Exhaustion: "+fighter.Model.Attack.displayName+" time="+fighter.Model.StateTime+" blood="+game.Blood.Amount);Assert.That(game.Blood.Amount,Is.EqualTo(1));
            yield return null;
        }
        [UnityTest]public IEnumerator BikeSlamCostsOnceHitsOnlyAfterWindupAndCannotHitThroughWalls()
        {
            var enemy=PracticeVictim(Vector2.down*1.15f);game.Blood.Set(100);float before=enemy.Health.CurrentHealth;
            Assert.That(StepExpanded(.01f,new CombatInput{skillPressed=true,aim=Vector2.down}),Is.EqualTo(22));Assert.That(rider.Slamming,Is.True);Assert.That(fighter.Model.SkillRemaining,Is.GreaterThan(4));
            StepExpanded(.5f,new CombatInput{aim=Vector2.down},true);Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(before));
            StepExpanded(.2f,new CombatInput{aim=Vector2.down},true);Assert.That(enemy.Health.CurrentHealth,Is.LessThan(before));float once=enemy.Health.CurrentHealth;
            for(int i=0;i<100;i++)Call(fighter,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(once));
            fighter.ResetModel();Move(game.Map.Spawn);PracticeVictim(Vector2.down*1.15f);game.Blood.Set(21);
            StepExpanded(.01f,new CombatInput{skillPressed=true,aim=Vector2.down});Assert.That(rider.Slamming,Is.False);Assert.That(game.Blood.Amount,Is.EqualTo(21));
            game.Blood.Set(100);wall=new GameObject("QA_SlamWall");wall.transform.position=body.position+Vector2.down*.6f;wall.AddComponent<BoxCollider2D>().size=new Vector2(3,.1f);Physics2D.SyncTransforms();
            StepExpanded(.01f,new CombatInput{skillPressed=true,aim=Vector2.down});StepExpanded(.7f,new CombatInput{aim=Vector2.down},true);Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(enemy.Health.MaxHealth));
            yield return null;
        }
        [UnityTest]public IEnumerator ContextFinisherConsumesGearThreeIsTargetedBoundedAndReturnsToSaws()
        {
            var enemy=PracticeVictim(Vector2.down*1.15f);typeof(DarklightEnemy).GetField("stagger",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(enemy,1f);
            game.Blood.Set(100);MaxGear();Assert.That(rider.CanFinish,Is.True);
            Assert.That(StepExpanded(.01f,new CombatInput{weaponSpecialPressed=true,aim=Vector2.right}),Is.EqualTo(35));Assert.That(rider.Finishing,Is.True);Assert.That(rider.Gear.Level,Is.EqualTo(2));Assert.That(fighter.Model.WeaponSpecialRemaining,Is.GreaterThan(8));
            Assert.That(fighter.Model.Facing.y,Is.LessThan(-.9f));StepExpanded(.5f,new CombatInput(),true);float once=enemy.Health.CurrentHealth;
            Assert.That(once,Is.LessThan(enemy.Health.MaxHealth));for(int i=0;i<100;i++)Call(fighter,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(once));
            var velocity=rider.DashVelocity(body,Vector2.down*100,.5f);Move(body.position+velocity*.5f);Assert.That(rider.DashDistance,Is.LessThanOrEqualTo(3));
            var separation=fighter.GetComponent<Collider2D>().Distance(enemy.GetComponent<Collider2D>());Assert.That(separation.isOverlapped,Is.False,"Swept capsule overlap; body="+body.position+" targetBody="+enemy.GetComponent<Rigidbody2D>().position+" targetTransform="+enemy.transform.position+" gap="+separation.distance);
            bool queued=false;for(int i=0;i<160&&rider.Dashing;i++)
            {bool press=!queued&&fighter.Model.StateTime>=fighter.Model.Attack.startup+fighter.Model.Attack.active+.12f;queued|=press;StepExpanded(.01f,new CombatInput{lightPressed=press,aim=Vector2.down});}
            Assert.That(fighter.Model.Attack.kind,Is.EqualTo(StrikeKind.Light));
            fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);MaxGear();typeof(DarklightEnemy).GetField("stagger",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(enemy,0f);
            Assert.That(rider.CanFinish,Is.False);StepExpanded(.01f,new CombatInput{weaponSpecialPressed=true,aim=Vector2.down});Assert.That(rider.Finishing,Is.False);Assert.That(game.Blood.Amount,Is.EqualTo(82));
            yield return null;
        }
        [UnityTest]public IEnumerator SecretDiscoveryKeepsCampaignStateAndRoundTripsOldAndNewSaves()
        {
            game.ExitTraining();var real=game.Progress;var state=JsonUtility.FromJson<DarklightProgress>(JsonUtility.ToJson(real));
            try
            {
                typeof(DarklightCampaign).GetProperty("Progress").SetValue(game,state);state.chapter=6;state.weaponUnlocks=7;state.relics[6]=false;state.checkpoint=2;state.xp=341;
                Call(game,"LoadMap",6);Call(game,"Close");lab.SetPaused(true);Assert.That(game.AbyssSecretAvailable,Is.False);
                state.relics[6]=true;Assert.That(game.AbyssSecretAvailable,Is.True);Move(game.AbyssSecretPoint);
                Assert.That(game.TryGetInteraction(out var target),Is.True);Assert.That(target.Id,Is.EqualTo("abyss_motor"));Call(game,"PerformInteraction",target);
                Assert.That(state.weaponUnlocks,Is.EqualTo(15));Assert.That(state.chapter,Is.EqualTo(6));Assert.That(state.checkpoint,Is.EqualTo(2));Assert.That(state.xp,Is.EqualTo(341));Assert.That(game.AbyssSecretAvailable,Is.False);
                string json=JsonUtility.ToJson(state);var restored=DarklightProgress.Decode(json);Assert.That(restored.weaponUnlocks,Is.EqualTo(15));Assert.That(restored.equippedWeapon,Is.InRange(0,3));
                var old=DarklightProgress.Decode("{\"version\":2,\"chapter\":8,\"level\":1,\"weaponUnlocks\":7,\"equippedWeapon\":2}");Assert.That(old.weaponUnlocks,Is.EqualTo(7));Assert.That(old.equippedWeapon,Is.EqualTo(2));
                typeof(DarklightCampaign).GetProperty("Progress").SetValue(game,old);Assert.That(game.AbyssSecretAvailable,Is.False); // Different region until the player actually returns to Cripta.
                Call(game,"LoadMap",6);old.chapter=8;Assert.That(game.AbyssSecretAvailable,Is.False); // Main-campaign map follows chapter, exploration is separate.
            }
            finally{typeof(DarklightCampaign).GetProperty("Progress").SetValue(game,real);}
            yield return null;
        }
    }
}
