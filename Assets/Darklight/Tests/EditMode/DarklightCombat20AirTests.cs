using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed partial class DarklightAirControllerTests
    {
        [Test]public void HybridWeaponSwapKeepsTheSameAirTargetAndUsesEachWeaponsAuthoredMotion()
        {
            var source=new WeaponDefinition[3];var names=new[]{"Requiem","PresasDoVeu","LacoDaRuptura"};
            for(int i=0;i<3;i++)source[i]=UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/"+names[i]+".asset");
            var profiles=new DarklightCombatProfiles();try
            {
                var weapons=profiles.Create(source);fighter.loadout=weapons[2].loadout;fighter.ResetModel();air.ConfirmContact(enemy,launcher);
                var input=new CombatInput{aerialPressed=true};
                for(int i=0;i<20;i++){air.BeforeCombat(.02f,ref input);fighter.Model.Tick(.02f,input);input=default;Physics2D.SyncTransforms();}
                Assert.That(air.Active,Is.True);Assert.That(air.Target,Is.SameAs(enemy));
                input=new CombatInput{heavyPressed=true};air.BeforeCombat(.01f,ref input);
                Assert.That(fighter.Model.Attack.ruinStrike,Is.EqualTo(DarklightRuinStrike.SideKick));
                Assert.That(DarklightCombatProfiles.AttackAnimationKey(fighter.Model.Attack),Is.EqualTo(DarklightCombatProfiles.AttackAnimationKey(weapons[2].loadout.lights[1])));
                var model=fighter.Model;var kick=model.Attack;
                model.Tick(kick.startup+.001f,default);Assert.That(model.RegisterHit(enemy),Is.True);air.ConfirmContact(enemy,kick);
                model.Tick(kick.active+kick.recovery*.3f,default);model.Tick(.001f,default);
                float height=air.Height;int contacts=air.Contacts;
                Assert.That(model.Equip(weapons[0].loadout),Is.True);fighter.loadout=weapons[0].loadout;
                Assert.That(air.Height,Is.EqualTo(height));Assert.That(air.Contacts,Is.EqualTo(contacts));Assert.That(air.Target,Is.SameAs(enemy));
                input=new CombatInput{lightPressed=true};air.BeforeCombat(.01f,ref input);
                Assert.That(fighter.Model.Attack.ruinStrike,Is.EqualTo(DarklightRuinStrike.None));
                Assert.That(DarklightCombatProfiles.AttackAnimationKey(fighter.Model.Attack),Is.EqualTo(DarklightCombatProfiles.AttackAnimationKey(weapons[0].loadout.lights[0])));
                Assert.That(air.Target,Is.SameAs(enemy));
            }finally{fighter.ResetModel();profiles.Dispose();}
        }
    }
}
