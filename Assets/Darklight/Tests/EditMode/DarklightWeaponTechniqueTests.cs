using NUnit.Framework;
using UnityEditor;

namespace Darklight.Tests
{
    public sealed class DarklightWeaponTechniqueTests
    {
        [Test]public void KatanaAndDaggersGetDistinctSpecialsAndFinishersWithoutMutatingTheirAuthoredAssets()
        {
            var originals=new[]{AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/Requiem.asset"),AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/PresasDoVeu.asset")};
            foreach(var w in originals)Assert.That(w,Is.Not.Null);
            string before=UnityEngine.JsonUtility.ToJson(originals[0])+UnityEngine.JsonUtility.ToJson(originals[1]);
            var profiles=new DarklightCombatProfiles();try
            {
                var weapons=profiles.Create(originals);var katana=weapons[0].loadout;var daggers=weapons[1].loadout;
                Assert.That(katana.lights.Length,Is.EqualTo(originals[0].loadout.lights.Length+1));Assert.That(katana.lights[katana.lights.Length-1].finisher,Is.True);
                Assert.That(daggers.lights.Length,Is.EqualTo(4));Assert.That(katana.weaponSpecial.displayName,Is.EqualTo("Eclipse Partido"));
                Assert.That(daggers.weaponSpecial.displayName,Is.EqualTo("Dança das Presas"));Assert.That(katana.weaponSpecial.hitPulses,Is.EqualTo(2));Assert.That(daggers.weaponSpecial.hitPulses,Is.EqualTo(5));
                Assert.That(katana.weaponSpecial.cooldown,Is.GreaterThan(0));Assert.That(daggers.weaponSpecial.cooldown,Is.GreaterThan(0));
                Assert.That(UnityEngine.JsonUtility.ToJson(originals[0])+UnityEngine.JsonUtility.ToJson(originals[1]),Is.EqualTo(before));
            }finally{profiles.Dispose();}
        }
        [Test]public void MultiContactSpecialCannotDamageTheSameTargetTwiceInOnePulse()
        {
            var originals=new[]{AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/Requiem.asset"),AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/PresasDoVeu.asset")};
            var profiles=new DarklightCombatProfiles();try
            {
                var loadout=profiles.Create(originals)[1].loadout;var model=new CombatModel(loadout);
                model.Tick(.001f,new CombatInput{weaponSpecialPressed=true});Assert.That(model.Attack,Is.SameAs(loadout.weaponSpecial));
                model.Tick(model.Attack.startup+model.Attack.active-.001f,default);var target=new object();int contacts=0;
                for(int pulse=0;pulse<5;pulse++){if(model.RegisterPulseHit(target,pulse))contacts++;Assert.That(model.RegisterPulseHit(target,pulse),Is.False);}
                Assert.That(contacts,Is.EqualTo(5));model.Tick(model.Attack.recovery+.01f,default);Assert.That(model.RegisterPulseHit(target,0),Is.False);
            }finally{profiles.Dispose();}
        }
    }
}
