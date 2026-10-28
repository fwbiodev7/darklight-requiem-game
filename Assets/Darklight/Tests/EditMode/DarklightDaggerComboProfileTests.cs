using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightDaggerComboProfileTests
    {
        readonly List<Object> authored=new();
        DarklightCombatProfiles profiles;

        AttackDefinition Attack(StrikeKind kind,int index=0)
        {
            var attack=ScriptableObject.CreateInstance<AttackDefinition>();authored.Add(attack);
            attack.name="Authored_"+kind+"_"+index;attack.kind=kind;
            attack.startup=.12f;attack.active=.1f;attack.recovery=.22f;
            attack.damage=18+index*3;attack.postureDamage=1.4f+index*.2f;
            return attack;
        }
        WeaponDefinition Weapon(int index,int lightCount=3)
        {
            var weapon=ScriptableObject.CreateInstance<WeaponDefinition>();authored.Add(weapon);
            weapon.name="AuthoredWeapon_"+index;weapon.id=(DarklightWeaponId)index;
            weapon.loadout=new CombatLoadout
            {
                lights=Enumerable.Range(0,lightCount).Select(i=>Attack(StrikeKind.Light,i)).ToArray(),
                heavy=Attack(StrikeKind.Heavy),charged=Attack(StrikeKind.Charged),
                counter=Attack(StrikeKind.Counter),skill=Attack(StrikeKind.Skill),
                aerial=Attack(StrikeKind.Aerial),weaponSpecial=Attack(StrikeKind.WeaponSpecial)
            };
            return weapon;
        }
        WeaponDefinition[] Originals(int daggerLights=3)=>new[]{Weapon(0),Weapon(1,daggerLights),Weapon(2),Weapon(3)};
        WeaponDefinition[] Create(WeaponDefinition[] originals)
        {
            profiles=new DarklightCombatProfiles();return profiles.Create(originals);
        }
        static void PressLight(CombatModel model)=>model.Tick(.001f,new CombatInput{lightPressed=true});
        static void Finish(CombatModel model)=>model.Tick(model.Attack.Duration+.001f,default);

        [TearDown] public void CleanUp()
        {
            profiles?.Dispose();profiles=null;
            foreach(var item in authored)if(item!=null)Object.DestroyImmediate(item);
            authored.Clear();
        }

        [Test] public void FourBufferedDaggerInputsReachTheFinisherAndTheFifthRestarts()
        {
            var loadout=Create(Originals())[1].loadout;var model=new CombatModel(loadout);
            PressLight(model);
            for(int step=0;step<5;step++)
            {
                int expected=step%4;
                Assert.That(model.State,Is.EqualTo(CombatState.Attack));
                Assert.That(model.Attack,Is.SameAs(loadout.lights[expected]));
                Assert.That(model.ComboCount,Is.EqualTo(expected+1));
                Assert.That(model.AttackSerial,Is.EqualTo(step+1));
                Assert.That(model.Branch,Is.Zero,"A continuous light chain must not choose the pause branch");
                if(step==4)break;
                model.Tick(model.Attack.startup+model.Attack.active+
                           model.Attack.recovery*model.Attack.recoveryChainFraction+.001f,default);
                PressLight(model);
            }
        }

        [Test] public void FourthDaggerLightHasTwoUniqueContactsWithOnePrecedingLightTotalDamage()
        {
            var loadout=Create(Originals())[1].loadout;var model=new CombatModel(loadout);
            for(int step=0;step<3;step++){PressLight(model);Finish(model);}
            PressLight(model);var finisher=model.Attack;
            Assert.That(finisher,Is.SameAs(loadout.lights[3]));
            Assert.That(finisher.hitPulses,Is.EqualTo(2));
            Assert.That(finisher.finisher,Is.True);
            Assert.That(finisher.startup,Is.EqualTo(.05f));
            Assert.That(finisher.active,Is.EqualTo(.08f));
            Assert.That(finisher.recovery,Is.EqualTo(.12f));
            Assert.That(finisher.hitstop,Is.EqualTo(.028f));
            Assert.That(finisher.hitShape,Is.EqualTo(HitShape.Sector));
            Assert.That(finisher.angle,Is.EqualTo(110));
            var target=new object();float damage=0,posture=0;uint pulses=0;
            model.Tick(finisher.startup+.001f,default);pulses|=model.ContactPulses;
            Assert.That(model.RegisterPulseHit(target,0),Is.True);damage+=model.AttackDamage;posture+=finisher.postureDamage;
            Assert.That(model.RegisterPulseHit(target,0),Is.False,"A pulse cannot pay twice to the same target");
            Assert.That(model.RegisterPulseHit(target,1),Is.False,"The second pulse has not begun");
            model.Tick(finisher.active*.5f,default);pulses|=model.ContactPulses;
            Assert.That(model.RegisterPulseHit(target,1),Is.True);damage+=model.AttackDamage;posture+=finisher.postureDamage;
            Assert.That(model.RegisterPulseHit(target,0),Is.False);
            Assert.That(model.RegisterPulseHit(target,1),Is.False);
            Assert.That(pulses,Is.EqualTo(3u));
            Assert.That(damage,Is.EqualTo(loadout.lights[2].damage).Within(.0001f));
            Assert.That(posture,Is.EqualTo(loadout.lights[2].postureDamage).Within(.0001f));
            model.Tick(finisher.Duration,default);model.Tick(.001f,default);
            Assert.That(model.RegisterPulseHit(target,1),Is.False,"Recovery cannot emit another contact");
        }

        [Test] public void CreatingAndUsingProfilesDoesNotChangeAnyAuthoredAsset()
        {
            var originals=Originals();
            var originalObjects=authored.ToArray();
            var snapshots=originalObjects.Select(JsonUtility.ToJson).ToArray();
            var names=originalObjects.Select(item=>item.name).ToArray();
            var copies=Create(originals);var model=new CombatModel(copies[1].loadout);
            for(int step=0;step<5;step++){PressLight(model);Finish(model);}
            Assert.That(originals[1].loadout.lights.Length,Is.EqualTo(3));
            Assert.That(copies[1].loadout.lights.Length,Is.EqualTo(4));
            Assert.That(copies[0].loadout.lights.Length,Is.EqualTo(3));
            Assert.That(copies[2].loadout.lights.Length,Is.EqualTo(3));
            Assert.That(copies[3].loadout.lights.Length,Is.EqualTo(3));
            Assert.That(copies[1].loadout.lights[3],Is.Not.SameAs(copies[1].loadout.lights[2]));
            copies[1].loadout.lights[3].damage+=100;
            for(int i=0;i<originalObjects.Length;i++)
            {
                Assert.That(JsonUtility.ToJson(originalObjects[i]),Is.EqualTo(snapshots[i]));
                Assert.That(originalObjects[i].name,Is.EqualTo(names[i]));
            }
        }

        [Test] public void AnAuthoredFourthDaggerLightKeepsItsAuthoredContactAndTiming()
        {
            var originals=Originals(4);var fourth=originals[1].loadout.lights[3];
            fourth.startup=.17f;fourth.active=.19f;fourth.recovery=.31f;
            fourth.damage=83;fourth.hitPulses=3;fourth.finisher=false;fourth.displayName="Authored fourth";
            var copy=Create(originals)[1].loadout.lights;
            Assert.That(copy.Length,Is.EqualTo(4));
            Assert.That(copy[3].displayName,Is.EqualTo("Authored fourth"));
            Assert.That(copy[3].startup,Is.EqualTo(.17f*.85f));
            Assert.That(copy[3].active,Is.EqualTo(.19f));
            Assert.That(copy[3].recovery,Is.EqualTo(.31f*.8f));
            Assert.That(copy[3].damage,Is.EqualTo(83));
            Assert.That(copy[3].hitPulses,Is.EqualTo(3));
            Assert.That(copy[3].finisher,Is.False);
            Assert.That(fourth.startup,Is.EqualTo(.17f));
            Assert.That(fourth.recovery,Is.EqualTo(.31f));
        }
    }
}
