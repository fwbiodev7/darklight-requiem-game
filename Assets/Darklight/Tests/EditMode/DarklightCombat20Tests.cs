using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightCombat20Tests
    {
        DarklightCombatProfiles profiles;WeaponDefinition[] weapons;
        [SetUp]public void Profiles()
        {
            var originals=new[]{"Requiem","PresasDoVeu","LacoDaRuptura"};var source=new WeaponDefinition[3];
            for(int i=0;i<3;i++){source[i]=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/"+originals[i]+".asset");Assert.That(source[i],Is.Not.Null);}
            profiles=new DarklightCombatProfiles();weapons=profiles.Create(source);
        }
        [TearDown]public void Dispose()=>profiles?.Dispose();
        [TestCase(0,0)][TestCase(0,1)][TestCase(0,2)][TestCase(2,0)][TestCase(2,1)][TestCase(2,2)]
        public void RealProfilesConnectTwoLightsIntoTheirHeavyBranch(int weapon,int form)
        {
            var loadout=weapons[weapon].loadout;var model=new CombatModel(loadout){FormStyle=form};
            model.Tick(.001f,new CombatInput{lightPressed=true});Assert.That(model.Attack,Is.SameAs(loadout.lights[0]));
            model.Tick(model.Attack.Duration+.001f,default);Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
            model.Tick(.001f,new CombatInput{lightPressed=true});Assert.That(model.Attack,Is.SameAs(loadout.lights[1]));
            var second=model.Attack;
            model.Tick(second.startup+second.active+second.recovery*.2f,default);
            model.Tick(.001f,new CombatInput{heavyPressed=true});model.Tick(second.recovery*.3f,default);model.Tick(.001f,default);
            Assert.That(model.Attack,Is.SameAs(loadout.heavyBranch));Assert.That(model.Branch,Is.EqualTo(2));
        }
        [TestCase(0)][TestCase(2)]public void DeliberatePauseSelectsTheActualAlternativeCombo(int weapon)
        {
            var loadout=weapons[weapon].loadout;var model=new CombatModel(loadout);
            model.Tick(.001f,new CombatInput{lightPressed=true});model.Tick(model.Attack.Duration+.001f,default);
            model.Tick(.22f,default);model.Tick(.001f,new CombatInput{lightPressed=true});
            Assert.That(model.Attack,Is.SameAs(loadout.pauseBranch));Assert.That(model.Branch,Is.EqualTo(1));
        }
        [Test]public void KatanaLaunchChaseAndSlamRespectTheExistingJuggleLimit()
        {
            var l=weapons[0].loadout;Assert.That(l.heavyBranch.launch,Is.True);
            Assert.That(l.airFinisher,Is.Not.Null);Assert.That(l.airFinisher.launch,Is.False);
            Assert.That(l.airFinisher.airReaction,Is.EqualTo(DarklightAirReaction.Slam));
            var body=new DarklightAirBody();Assert.That(body.React(l.heavyBranch.airReaction,l.heavyBranch.launchSpeed),Is.True);
            body.Tick(.15f);body.HoldForChase();Assert.That(body.React(DarklightAirReaction.Suspend),Is.True);
            Assert.That(body.React(l.airFinisher.airReaction),Is.True);Assert.That(body.React(DarklightAirReaction.Suspend),Is.False);
            Assert.That(body.Tick(.5f),Is.True);Assert.That(body.React(DarklightAirReaction.Launch),Is.False);
            body.Tick(.81f);Assert.That(body.React(DarklightAirReaction.Launch),Is.True);
            var resistant=new DarklightAirBody();Assert.That(resistant.React(l.heavyBranch.airReaction,l.heavyBranch.launchSpeed,true),Is.False);
        }
        [Test]public void ProfileCreationDoesNotRewriteTheApprovedKatanaAssets()
        {
            var original=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/Requiem.asset");
            string before=JsonUtility.ToJson(original.loadout.heavy)+JsonUtility.ToJson(original.loadout.aerial);
            var another=new DarklightCombatProfiles();try
            {
                var copy=another.Create(new[]{original})[0];Assert.That(copy.loadout.heavyBranch,Is.Not.SameAs(original.loadout.heavy));
                Assert.That(copy.loadout.airFinisher,Is.Not.SameAs(original.loadout.aerial));
                Assert.That(DarklightCombatProfiles.AttackAnimationKey(copy.loadout.heavyBranch),Is.EqualTo("Pesado"));
                Assert.That(DarklightCombatProfiles.AttackAnimationKey(copy.loadout.airFinisher),Is.EqualTo(DarklightCombatProfiles.AttackAnimationKey(original.loadout.aerial)));
                Assert.That(JsonUtility.ToJson(original.loadout.heavy)+JsonUtility.ToJson(original.loadout.aerial),Is.EqualTo(before));
            }finally{another.Dispose();}
        }
    }
}
