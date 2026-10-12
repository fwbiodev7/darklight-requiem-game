using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight.Tests
{
    public sealed class DarklightCombatTests
    {
        readonly List<AttackDefinition> assets = new();
        CombatModel model;
        AttackDefinition Make(StrikeKind kind)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.kind=kind;attack.startup=.1f;attack.active=.1f;attack.recovery=.2f;assets.Add(attack);return attack;
        }
        [SetUp] public void SetUp() { model=new CombatModel(new CombatLoadout { lights=new[] { Make(StrikeKind.Light),Make(StrikeKind.Light),Make(StrikeKind.Light) },heavy=Make(StrikeKind.Heavy),charged=Make(StrikeKind.Charged),counter=Make(StrikeKind.Counter),skill=Make(StrikeKind.Skill) }); }
        [TearDown] public void TearDown() { foreach(var asset in assets) Object.DestroyImmediate(asset); assets.Clear(); }
        void Light() => model.Tick(.01f,new CombatInput { lightPressed=true });
        [Test] public void ContactIsUniqueAndWindupCannotDamage()
        {
            Light();Assert.That(model.RegisterHit(1),Is.False);
            model.Tick(.12f,default);Assert.That(model.RegisterHit(1),Is.True);Assert.That(model.RegisterHit(1),Is.False);Assert.That(model.RegisterHit(2),Is.True);
            model.Tick(.04f,default);Assert.That(model.RegisterHit(1),Is.False);
            model.Tick(.25f,default);Light();model.Tick(.12f,default);Assert.That(model.RegisterHit(1),Is.True);
        }
        [Test] public void LargeFrameCrossesContactWithoutLosingTheHit()
        { Light();model.Tick(.5f,default);Assert.That(model.ActiveThisTick,Is.True);Assert.That(model.RegisterHit(1),Is.True); }
        [Test] public void HitstopPreservesInputWithoutAdvancingContact()
        { Light();float clock=model.Clock;model.Tick(0,new CombatInput { lightPressed=true });Assert.That(model.Clock,Is.EqualTo(clock));Assert.That(model.StateTime,Is.Zero);Assert.That(model.RegisterHit(1),Is.False);model.Tick(.28f,default);model.Tick(.01f,default);Assert.That(model.ComboCount,Is.EqualTo(2)); }
        [Test] public void BufferChainsAndSuspensionClearsIt()
        {
            Light();model.Tick(.3f,new CombatInput { lightPressed=true });model.Tick(.11f,default);model.Tick(.01f,default);
            Assert.That(model.ComboCount,Is.EqualTo(2));
            model.Tick(.3f,new CombatInput { lightPressed=true });model.SuspendInput();model.Tick(.11f,default);model.Tick(.01f,default);
            Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
        }
        [Test] public void ThreeLightsThenResetAfterInactivity()
        {
            for(int i=1;i<=3;i++) { Light();Assert.That(model.ComboCount,Is.EqualTo(i));model.Tick(.4f,default); }
            model.Tick(.7f,default);Light();Assert.That(model.ComboCount,Is.EqualTo(1));
        }
        [TestCase(.1f,StrikeKind.Heavy)] [TestCase(.7f,StrikeKind.Charged)]
        public void HoldingHeavyChoosesTheChargedProfile(float duration,StrikeKind expected)
        { model.Tick(.01f,new CombatInput { heavyPressed=true,heavyHeld=true });model.Tick(duration,new CombatInput { heavyHeld=true });model.Tick(.01f,new CombatInput { heavyReleased=true });Assert.That(model.Attack.kind,Is.EqualTo(expected)); }
        [Test] public void HeavyFinishesALightCombo()
        { Light();model.Tick(.3f,new CombatInput { heavyPressed=true });model.Tick(.11f,default);model.Tick(.01f,default);Assert.That(model.Attack.kind,Is.EqualTo(StrikeKind.Heavy));Assert.That(model.AttackDamage,Is.GreaterThan(model.Attack.damage)); }
        [Test] public void DodgeHasLimitedInvulnerabilityAndCooldown()
        { model.Tick(.01f,new CombatInput { dodgePressed=true,move=Vector2.right });Assert.That(model.Invulnerable,Is.False);model.Tick(.08f,default);Assert.That(model.Invulnerable,Is.True);model.Tick(.26f,default);Assert.That(model.Invulnerable,Is.False);model.Tick(.01f,new CombatInput { dodgePressed=true });Assert.That(model.State,Is.EqualTo(CombatState.Neutral)); }
        [Test] public void DodgeOnlyCancelsDuringRecovery()
        { Light();model.Tick(.05f,new CombatInput { dodgePressed=true });Assert.That(model.State,Is.EqualTo(CombatState.Attack));model.Tick(.17f,new CombatInput { dodgePressed=true });Assert.That(model.ActiveThisTick,Is.True);model.Tick(.01f,default);Assert.That(model.State,Is.EqualTo(CombatState.Dodge)); }
        [Test] public void ParryOpensCounterThenExpires()
        { model.Tick(.01f,new CombatInput { parryPressed=true });Assert.That(model.Defend(),Is.True);model.Tick(.36f,default);Light();Assert.That(model.Attack.kind,Is.EqualTo(StrikeKind.Counter)); }
        [Test] public void LateParryDoesNotDefend()
        { model.Tick(.01f,new CombatInput { parryPressed=true });model.Tick(.2f,default);Assert.That(model.Defend(),Is.False); }
        [Test] public void ParryCannotProtectTheCharactersBack()
        { model.Tick(.01f,new CombatInput { parryPressed=true,aim=Vector2.right });Assert.That(model.Defend(Vector2.left),Is.False);Assert.That(model.Defend(Vector2.right),Is.True); }
        [Test] public void SkillCooldownCannotBeBypassedWithRepeatedInputs()
        { model.Tick(.01f,new CombatInput { skillPressed=true });model.Tick(.5f,default);model.Tick(.01f,new CombatInput { skillPressed=true });Assert.That(model.State,Is.EqualTo(CombatState.Neutral));model.Tick(3f,default);model.Tick(.01f,new CombatInput { skillPressed=true });Assert.That(model.State,Is.EqualTo(CombatState.Attack)); }
        [Test] public void DifferentCommandsCannotExtendAnOldDodge()
        {
            assets[0].active=.18f;Light();model.Tick(.01f,new CombatInput{dodgePressed=true});model.Tick(.19f,default);
            model.Tick(.01f,new CombatInput{skillPressed=true});model.Tick(.08f,default);model.Tick(.01f,default);
            Assert.That(model.State,Is.EqualTo(CombatState.Attack),"An old dodge was refreshed by another action");
        }
        [Test] public void ParryCancelsRecoveryButNeverDeletesTheContactFrame()
        {
            Light();model.Tick(.05f,new CombatInput{parryPressed=true});Assert.That(model.State,Is.EqualTo(CombatState.Attack));
            model.Tick(.07f,default);Assert.That(model.RegisterHit("target"),Is.True);
            model.Tick(.09f,new CombatInput{parryPressed=true});Assert.That(model.State,Is.EqualTo(CombatState.Attack));Assert.That(model.ActiveThisTick,Is.True);
            model.Tick(.01f,default);Assert.That(model.State,Is.EqualTo(CombatState.Parry));Assert.That(model.RegisterHit("another"),Is.False);
        }
        [Test] public void SkillsDoNotCarryAnUnwantedQueuedLightIntoRecovery()
        {
            model.Tick(.01f,new CombatInput{lightPressed=true,skillPressed=true});Assert.That(model.Attack.kind,Is.EqualTo(StrikeKind.Skill));
            model.Tick(.4f,default);model.Tick(.01f,default);Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
        }
        [Test] public void DeathCancelsAttackAndFutureInputs()
        { Light();model.Hurt(true);model.Tick(1,new CombatInput { lightPressed=true,dodgePressed=true });Assert.That(model.State,Is.EqualTo(CombatState.Dead));Assert.That(model.RegisterHit(1),Is.False); }
        [TestCase(0,-1,0)] [TestCase(-1,-1,1)] [TestCase(-1,0,2)] [TestCase(-1,1,3)]
        [TestCase(0,1,4)] [TestCase(1,1,5)] [TestCase(1,0,6)] [TestCase(1,-1,7)]
        public void DirectionMatchesTheEightViewContract(float x,float y,int index)
        { Assert.That(DarklightCharacterView.DirectionIndex(new Vector2(x,y)),Is.EqualTo(index)); }
    }
    public sealed class DarklightInputTests : InputTestFixture
    {
        [Test] public void XInputAttackAndPauseMapDoNotLeakAcrossMenus()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            using(var input=new DarklightInput())
            {
                Press(pad.buttonWest);Assert.That(input.Action("Light").WasPressedThisFrame(),Is.True);Assert.That(input.UsingGamepad,Is.True);
                Release(pad.buttonWest);input.SetGameplay(false);Press(pad.buttonWest);
                Assert.That(input.Action("Light").WasPressedThisFrame(),Is.False);
                Press(pad.buttonSouth);Assert.That(input.Menu("Submit").WasPressedThisFrame(),Is.True);
                Assert.That(input.Action("Interact").WasPressedThisFrame(),Is.False);
            }
        }
        [Test] public void RemovingControllerRestoresKeyboardDeviceState()
        { var pad=InputSystem.AddDevice<Gamepad>();using(var input=new DarklightInput()) { Press(pad.buttonWest);Assert.That(input.UsingGamepad,Is.True);InputSystem.RemoveDevice(pad);Assert.That(input.UsingGamepad,Is.False); } }
    }
}
