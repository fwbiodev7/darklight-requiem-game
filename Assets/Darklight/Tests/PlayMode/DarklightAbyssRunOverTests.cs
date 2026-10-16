using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        [UnityTest]public IEnumerator ConfirmedRunOverCrossesOneLightTargetWithBoundedDamageAndLandsIntoSaws()
        {
            var enemy=PracticeVictim(Vector2.down*.8f);enemy.kind=RequiemEnemyKind.Crawler;game.Blood.Set(100);
            BeginDash(Vector2.down);Call(fighter,"ResolveStrike");
            Assert.That(rider.RunOverConfirmed,Is.True,"Victim must enable wheel passage: vulnerable="+enemy.Vulnerable+" trigger="+enemy.GetComponent<Collider2D>().isTrigger+" target="+enemy.GetComponent<Collider2D>().bounds+" rider="+fighter.GetComponent<Collider2D>().bounds+" distance="+rider.DashDistance+" health="+enemy.Health.CurrentHealth);
            float health=enemy.Health.CurrentHealth;var start=body.position;bool cleared=false;
            for(int i=0;i<35&&fighter.Model.ActiveThisTick;i++)
            {
                var velocity=rider.DashVelocity(body,Vector2.down*100,.01f);Move(body.position+velocity*.01f);
                cleared|=rider.RunOverCleared;StepExpanded(.01f,new CombatInput{aim=Vector2.down},true);
                Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(health),"The wheel must not deal infinite contacts");
            }
            Assert.That(rider.DashBlocked,Is.False);Assert.That(cleared,Is.True,"Bike must actually pass the target, not stop before it");
            Assert.That(rider.DashDistance,Is.LessThanOrEqualTo(DarklightAbyssRider.MaximumDashDistance+.001f));
            Assert.That(fighter.GetComponent<Collider2D>().Distance(enemy.GetComponent<Collider2D>()).isOverlapped,Is.False,"Must land clear of the victim");
            bool queued=false,jumped=false;for(int i=0;i<90&&rider.Dashing;i++)
            {
                jumped|=rider.JumpHeight>.08f;bool press=!queued&&fighter.Model.StateTime>=fighter.Model.Attack.startup+fighter.Model.Attack.active+.15f;queued|=press;
                StepExpanded(.01f,new CombatInput{lightPressed=press,aim=Vector2.up});
            }
            Assert.That(jumped,Is.True);Assert.That(queued,Is.True);Assert.That(fighter.Model.Attack.kind,Is.EqualTo(StrikeKind.Light));Assert.That(fighter.Model.Facing.y,Is.GreaterThan(.9f),"Landing must allow aiming back at the crossed target");
            yield return null;
        }
        [UnityTest]public IEnumerator RunOverStillStopsAtWallsOtherEnemiesAndHeavyTargets()
        {
            var enemy=PracticeVictim(Vector2.down*.8f);enemy.kind=RequiemEnemyKind.Crawler;game.Blood.Set(100);BeginDash(Vector2.down);Call(fighter,"ResolveStrike");Assert.That(rider.RunOverConfirmed,Is.True);
            wall=new GameObject("QA_RunOverWall");wall.transform.position=body.position+Vector2.down*2;wall.AddComponent<BoxCollider2D>().size=new Vector2(4,.1f);Physics2D.SyncTransforms();
            var v=rider.DashVelocity(body,Vector2.down*100,.5f);Move(body.position+v*.5f);Assert.That(rider.DashBlocked,Is.True);Assert.That(body.position.y,Is.GreaterThan(wall.transform.position.y));
            Object.Destroy(wall);yield return null;wall=null;
            fighter.ResetModel();Move(game.Map.Spawn);PracticeVictim(Vector2.down*.8f);enemy.kind=RequiemEnemyKind.Armored;game.Blood.Set(100);BeginDash(Vector2.down);Call(fighter,"ResolveStrike");Assert.That(rider.RunOverConfirmed,Is.False);v=rider.DashVelocity(body,Vector2.down*100,.5f);Assert.That(rider.DashBlocked,Is.True);
            enemy.kind=RequiemEnemyKind.Crawler;fighter.ResetModel();Move(game.Map.Spawn);PracticeVictim(Vector2.down*.8f);game.Blood.Set(100);BeginDash(Vector2.down);Call(fighter,"ResolveStrike");Assert.That(rider.RunOverConfirmed,Is.True);
            wall=new GameObject("QA_SecondEnemy");wall.transform.position=game.Map.Spawn+Vector2.down*2;wall.AddComponent<HealthSystem>().SetMaxHealth(100);wall.AddComponent<DarklightCombatant>();wall.AddComponent<CircleCollider2D>().radius=.28f;Physics2D.SyncTransforms();
            v=rider.DashVelocity(body,Vector2.down*100,.5f);Assert.That(rider.DashBlocked,Is.True,"Other targets cannot be ghosted through");
            yield return null;
        }
    }
}
