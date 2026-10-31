using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightCombatBufferRegressionTests
    {
        readonly List<AttackDefinition> owned = new();
        AttackDefinition Move(StrikeKind kind, float recovery = .2f)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            owned.Add(attack);
            attack.kind = kind;
            attack.startup = .02f;
            attack.active = .02f;
            attack.recovery = recovery;
            return attack;
        }
        [TearDown] public void Cleanup()
        {
            foreach (var attack in owned) Object.DestroyImmediate(attack);
            owned.Clear();
        }
        [TestCase(StrikeKind.Skill)]
        [TestCase(StrikeKind.Aerial)]
        [TestCase(StrikeKind.WeaponSpecial)]
        public void ConfirmedSpecialCancelConsumesCompetingBufferedLight(StrikeKind kind)
        {
            var light = Move(StrikeKind.Light);
            var special = Move(kind, .04f);
            var model = new CombatModel(new CombatLoadout {
                lights = new[] { light }, skill = special, aerial = special,
                weaponSpecial = special, attackBuffer = .5f
            }) { FormStyle = 2 };
            model.Tick(.001f, new CombatInput { lightPressed = true });
            model.Tick(.03f, default);
            Assert.That(model.RegisterHit("target"), Is.True);
            model.Tick(.04f, default);
            model.Tick(.001f, new CombatInput {
                lightPressed = true, skillPressed = kind == StrikeKind.Skill,
                aerialPressed = kind == StrikeKind.Aerial,
                weaponSpecialPressed = kind == StrikeKind.WeaponSpecial
            });
            Assert.That(model.Attack, Is.SameAs(special));
            for (int i = 0; i < 25; i++) model.Tick(.01f, default);
            Assert.That(model.AttackSerial, Is.EqualTo(2), "The selected special must consume the competing light input");
            Assert.That(model.State, Is.EqualTo(CombatState.Neutral));
        }
        [Test] public void ReleasingChargedAttackConsumesLightBufferedDuringCharge()
        {
            var light = Move(StrikeKind.Light);
            var heavy = Move(StrikeKind.Heavy, .04f);
            var model = new CombatModel(new CombatLoadout {
                lights = new[] { light }, heavy = heavy, charged = heavy, attackBuffer = .5f
            });
            model.Tick(.01f, new CombatInput { heavyPressed = true, heavyHeld = true });
            model.Tick(.01f, new CombatInput { lightPressed = true, heavyHeld = true });
            model.Tick(.01f, new CombatInput { heavyReleased = true });
            Assert.That(model.Attack, Is.SameAs(heavy));
            for (int i = 0; i < 25; i++) model.Tick(.01f, default);
            Assert.That(model.AttackSerial, Is.EqualTo(1));
            Assert.That(model.State, Is.EqualTo(CombatState.Neutral));
        }
        [TestCase(0)] [TestCase(2)]
        public void MissingOptionalSkillDoesNotCrashOrDiscardValidLight(int form)
        {
            var light=Move(StrikeKind.Light);
            var model=new CombatModel(new CombatLoadout{lights=new[]{light},weapon=3}){FormStyle=form};
            Assert.DoesNotThrow(()=>model.Tick(.01f,new CombatInput{skillPressed=true,lightPressed=true}));
            Assert.That(model.Attack,Is.SameAs(light));
            model.Tick(.03f,default);Assert.That(model.RegisterHit("target"),Is.True);
            model.Tick(.04f,default);
            Assert.DoesNotThrow(()=>model.Tick(.01f,new CombatInput{skillPressed=true}));
            Assert.That(model.AttackSerial,Is.EqualTo(1));
        }
    }
}
