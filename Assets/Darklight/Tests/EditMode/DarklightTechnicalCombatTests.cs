using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightTechnicalCombatTests
    {
        AttackDefinition light,heavy,pause,branch;CombatLoadout loadout;CombatModel model;
        AttackDefinition New(StrikeKind kind){var a=ScriptableObject.CreateInstance<AttackDefinition>();a.kind=kind;a.startup=.05f;a.active=.08f;a.recovery=.15f;a.damage=10;return a;}
        [SetUp]public void Setup(){light=New(StrikeKind.Light);heavy=New(StrikeKind.Heavy);pause=New(StrikeKind.Light);branch=New(StrikeKind.Heavy);loadout=new CombatLoadout{lights=new[]{light,light,light},heavy=heavy,charged=heavy,counter=heavy,skill=heavy,pauseBranch=pause,heavyBranch=branch,attackBuffer=.18f,defenseBuffer=.18f};model=new CombatModel(loadout);}
        [TearDown]public void End(){Object.DestroyImmediate(light);Object.DestroyImmediate(heavy);Object.DestroyImmediate(pause);Object.DestroyImmediate(branch);}
        void Light()=>model.Tick(.01f,new CombatInput{lightPressed=true});
        [Test]public void IntentionalPauseSelectsItsOwnBranch(){Light();model.Tick(.3f,default);model.Tick(.21f,default);Light();Assert.That(model.Attack,Is.SameAs(pause));Assert.That(model.Branch,Is.EqualTo(1));}
        [Test]public void TwoLightsAndHeavySelectTheFinisherBranch(){Light();model.Tick(.3f,default);Light();model.Tick(.15f,new CombatInput{heavyPressed=true});model.Tick(.06f,default);model.Tick(.01f,default);Assert.That(model.Attack,Is.SameAs(branch));}
        [Test]public void LargeFramePreservesEveryAuthoredContactPulse(){light.hitPulses=6;Light();model.Tick(.5f,default);Assert.That(model.ContactPulses,Is.EqualTo(63));for(int i=0;i<6;i++){Assert.That(model.RegisterPulseHit("target",i),Is.True);Assert.That(model.RegisterPulseHit("target",i),Is.False);}}
        [Test]public void ExpiredBufferedAttackDoesNotFireAfterLongRecovery(){Light();model.Tick(.01f,new CombatInput{lightPressed=true});model.Tick(.3f,default);model.Tick(.01f,default);Assert.That(model.AttackSerial,Is.EqualTo(1));}
        [Test]public void ConfirmedHitAllowsWeaponChangeWithoutClearingCombo(){Light();model.Tick(.14f,default);Assert.That(model.RegisterHit("target"),Is.True);model.Tick(.01f,default);Assert.That(model.Equip(loadout),Is.True);Assert.That(model.ComboCount,Is.EqualTo(1));}
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void ThrustGeometryHonorsFacingAndWidthInEveryDirection(int direction){light.hitShape=HitShape.Thrust;light.width=.5f;light.range=2;float a=direction*Mathf.PI/4;var f=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Assert.That(DarklightHitGeometry.Contains(light,Vector2.zero,f,f,0,.5f),Is.True);Assert.That(DarklightHitGeometry.Contains(light,Vector2.zero,f,-f,0,.5f),Is.False);var side=new Vector2(-f.y,f.x);Assert.That(DarklightHitGeometry.Contains(light,Vector2.zero,f,f+side,0,.5f),Is.False);}
        [Test]public void RepeatingOneOrTwoMovesCannotFarmTopRanks(){var score=new DarklightStyleMeter();for(int i=0;i<200;i++)score.Hit(i%2,0);Assert.That(score.Points,Is.LessThan(195));Assert.That(score.Rank,Is.Not.EqualTo("SSS"));}
        [Test]public void VariedEffectiveMovesCanReachSSSAndDamageReducesRank(){var score=new DarklightStyleMeter();for(int i=0;i<25;i++)score.Hit(i,i%3);Assert.That(score.Rank,Is.EqualTo("SSS"));float before=score.Points;score.Damage();Assert.That(score.Points,Is.LessThan(before));}
        [Test]public void DifficultiesChangeCoordinationReactionAndPatterns(){var normal=DarklightDifficultyProfile.For(0);var hard=DarklightDifficultyProfile.For(1);var extreme=DarklightDifficultyProfile.For(2);Assert.That(hard.meleeSlots,Is.GreaterThan(normal.meleeSlots));Assert.That(extreme.comboLength,Is.GreaterThan(hard.comboLength));Assert.That(extreme.reaction,Is.LessThan(normal.reaction));Assert.That(extreme.reaction,Is.GreaterThanOrEqualTo(.15f));}
        [Test]public void ExtremeRequiresCompletionAndOldSaveDefaultsRemainValid(){var p=DarklightProgress.Decode("{\"version\":2,\"chapter\":3,\"checkpoint\":1,\"level\":7}");Assert.That(p.difficulty,Is.Zero);Assert.That(DarklightDifficultyProfile.ExtremeUnlocked(p),Is.False);p.completed=true;Assert.That(DarklightDifficultyProfile.ExtremeUnlocked(p),Is.True);}
        [Test]public void TrainingMapHasConnectedWalkableAnchors(){var map=DarklightMapDefinition.TrainingArena();foreach(var point in map.Arenas)Assert.That(map.Walkable(new Vector2Int(Mathf.FloorToInt(point.x/map.CellSize),Mathf.FloorToInt(point.y/map.CellSize))),Is.True);}
        [Test]public void HotModelAndStyleLoopDoNotAllocateAfterWarmup()
        {
            var score=new DarklightStyleMeter();
            for(int i=0;i<1000;i++){model.Tick(.02f,new CombatInput{lightPressed=i%8==0});score.Hit(i%8,i%3);}
            var watch=System.Diagnostics.Stopwatch.StartNew();long before=System.GC.GetAllocatedBytesForCurrentThread();
            for(int i=0;i<10000;i++){model.Tick(.02f,new CombatInput{lightPressed=i%8==0});score.Hit(i%8,i%3);}
            long bytes=System.GC.GetAllocatedBytesForCurrentThread()-before;watch.Stop();
            System.IO.Directory.CreateDirectory("outputs/finalizacao-20261018");
            System.IO.File.WriteAllText("outputs/finalizacao-20261018/model-metrics.json","{\"ticks\":10000,\"allocatedBytes\":"+bytes+",\"milliseconds\":"+watch.ElapsedMilliseconds+"}");
            Assert.That(bytes,Is.LessThan(1024),"Combat and style hot path allocated after warmup");
        }
        [Test]public void VioletDodgePreservesTheComboForAnExtension(){model.FormStyle=2;Light();model.Tick(.14f,default);model.Tick(.01f,new CombatInput{dodgePressed=true,move=Vector2.right});Assert.That(model.State,Is.EqualTo(CombatState.Dodge));Assert.That(model.ComboCount,Is.EqualTo(1));model.Tick(.24f,default);Light();Assert.That(model.ComboCount,Is.GreaterThan(1));}
        [Test]public void WhiteFormCanLinkAConfirmedHitIntoItsSkill(){model.FormStyle=3;Light();model.Tick(.14f,default);model.RegisterHit("target");model.Tick(.02f,new CombatInput{skillPressed=true});Assert.That(model.Attack,Is.SameAs(loadout.skill));Assert.That(model.SkillRemaining,Is.GreaterThan(0));}
    }
}
