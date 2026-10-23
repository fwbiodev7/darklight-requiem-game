using System.Reflection;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    // Isolated EditMode component checks; no campaign Awake, audio, Play or saves.
    public sealed class DarklightAirControllerTests
    {
        GameObject player,foe,wall;AttackDefinition launcher;DarklightFighter fighter;DarklightAirCombo air;DarklightEnemy enemy;
        static void AwakeIfNeeded(Component component,string field)
        {if(component.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(component)==null)component.GetType().GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(component,null);}
        [SetUp]public void Create()
        {
            launcher=ScriptableObject.CreateInstance<AttackDefinition>();launcher.launch=true;launcher.range=2;
            player=new GameObject("AirPlayerEditOnly");player.transform.position=new Vector3(10000,10000);
            var body=player.AddComponent<Rigidbody2D>();body.gravityScale=0;player.AddComponent<CircleCollider2D>().radius=.25f;
            player.AddComponent<SpriteRenderer>();player.AddComponent<DarklightCombatant>();
            fighter=player.AddComponent<DarklightFighter>();fighter.loadout=new CombatLoadout{weapon=0,lights=new[]{launcher},heavy=launcher,aerial=launcher};
            typeof(DarklightFighter).GetField("body",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(fighter,body);
            typeof(DarklightFighter).GetProperty("Model").SetValue(fighter,new CombatModel(fighter.loadout));
            air=player.AddComponent<DarklightAirCombo>();AwakeIfNeeded(air,"chase");
            typeof(DarklightFighter).GetProperty("AirCombo").SetValue(fighter,air);
            foe=new GameObject("AirTargetEditOnly");foe.transform.position=player.transform.position+Vector3.down*2;
            foe.AddComponent<Rigidbody2D>().gravityScale=0;foe.AddComponent<CircleCollider2D>().radius=.25f;foe.AddComponent<SpriteRenderer>();
            var hp=foe.AddComponent<HealthSystem>();hp.SetMaxHealth(100,false);foe.AddComponent<DarklightCombatant>();enemy=foe.AddComponent<DarklightEnemy>();
            if(enemy.Health==null)typeof(DarklightEnemy).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(enemy,null);
            Assert.That(enemy.ReactAir(launcher,DarklightAirReaction.Launch),Is.True);enemy.AirBody.Tick(.2f);air.ConfirmContact(enemy,launcher);Physics2D.SyncTransforms();
        }
        [TearDown]public void Destroy()
        {if(wall!=null)Object.DestroyImmediate(wall);Object.DestroyImmediate(foe);Object.DestroyImmediate(player);Object.DestroyImmediate(launcher);Physics2D.SyncTransforms();}
        [Test]public void LaunchDoesNotAutomaticallyChaseOrDealUncommandedDamage()
        {
            var input=default(CombatInput);var before=player.GetComponent<Rigidbody2D>().position;float hp=enemy.Health.CurrentHealth;
            air.BeforeCombat(.02f,ref input);Assert.That(air.Active,Is.False);Assert.That(player.GetComponent<Rigidbody2D>().position,Is.EqualTo(before));Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(hp));
        }
        [Test]public void HitstopBuffersChaseWithoutQueuingTheGroundLauncher()
        {
            var input=new CombatInput{aerialPressed=true};air.BeforeCombat(0,ref input);fighter.Model.Tick(0,input);
            Assert.That(input.aerialPressed,Is.False);Assert.That(air.Active,Is.False);Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral));
            input=default;air.BeforeCombat(.02f,ref input);fighter.Model.Tick(.02f,input);
            Assert.That(air.Chasing,Is.True);Assert.That(fighter.Model.Attack,Is.Not.SameAs(launcher));Assert.That(fighter.Model.Attack.damage,Is.Zero);
        }
        [Test]public void ChaseCannotCrossAStaticWallAndRecoversInsteadOfHanging()
        {
            wall=new GameObject("AirChaseWall");wall.transform.position=player.transform.position+Vector3.down*.7f;wall.AddComponent<BoxCollider2D>().size=new Vector2(3,.2f);Physics2D.SyncTransforms();
            var input=new CombatInput{aerialPressed=true};
            for(int i=0;i<35;i++){air.BeforeCombat(.02f,ref input);fighter.Model.Tick(.02f,input);input=default;Physics2D.SyncTransforms();}
            Assert.That(player.GetComponent<Rigidbody2D>().position.y,Is.GreaterThan(9999.5f));Assert.That(air.Chasing,Is.False);Assert.That(air.Target,Is.Null);Assert.That(air.Height,Is.Zero);
        }
    }
}
