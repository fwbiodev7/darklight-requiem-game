using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightExpansionTests
    {
        [Test] public void LegacyMigrationRemovesVisualAgeAndPreservesRewardsFinaleAndRelics()
        {
            var old=new DarklightProgress{version=1,chapter=5,checkpoint=4,completed=true,ending=2,formsUnlocked=3,preferredForm=3,appearanceStage=4,narrativeDays=410,level=8,attack=3,points=4,weaponUnlocks=7,equippedWeapon=2,relics=new[]{true,false,true,false,true,true}};
            var s=DarklightProgress.Decode(JsonUtility.ToJson(old));
            Assert.That(s.version,Is.EqualTo(2));Assert.That(s.completed&&s.legacyFinaleCompleted,Is.True);Assert.That(s.ending,Is.EqualTo(2));Assert.That(s.formsUnlocked,Is.EqualTo(3));Assert.That(s.preferredForm,Is.EqualTo(2));
            Assert.That(s.appearanceStage,Is.Zero);Assert.That(s.narrativeDays,Is.EqualTo(410));Assert.That(s.level,Is.EqualTo(8));Assert.That(s.weaponUnlocks,Is.EqualTo(7));Assert.That(s.equippedWeapon,Is.EqualTo(2));Assert.That(s.points,Is.EqualTo(4));
            Assert.That(s.relics.Length,Is.EqualTo(10));Assert.That(s.relics[0]&&s.relics[2]&&s.relics[4]&&s.relics[5],Is.True);Assert.That(s.relics[6],Is.False);
        }
        [TestCase("bad-json")][TestCase("{\"version\":3,\"chapter\":0,\"level\":1}")][TestCase("{\"version\":1,\"chapter\":-1,\"level\":1}")]
        public void UnsupportedOrCorruptSavesAreRejected(string json)=>Assert.That(DarklightProgress.Decode(json),Is.Null);
        [Test] public void CombatRewardsCannotBeMultipliedByTargetsOrPulses()
        {
            var b=new DarklightBlood();Assert.That(b.RewardStrike(StrikeKind.Light,1),Is.True);
            for(int i=0;i<100;i++)Assert.That(b.RewardStrike(StrikeKind.Light,1),Is.False);
            Assert.That(b.Amount,Is.EqualTo(5));b.RewardStrike(StrikeKind.Light,2);b.RewardStrike(StrikeKind.Light,3);b.RewardStrike(StrikeKind.Light,4);Assert.That(b.Amount,Is.EqualTo(7.25f));
            b.RewardStrike(StrikeKind.Counter,5);Assert.That(b.Amount,Is.EqualTo(15.25f));Assert.That(b.RewardParry(),Is.True);Assert.That(b.RewardParry(),Is.False);
        }
        [Test] public void AbilitiesHaveIndependentCostsAndRecoveryWithoutPermanentBuffOnReload()
        {
            var p=new DarklightProgress{vampireMastery=3,vampireSkills=63};var b=new DarklightBlood();b.Set(100);Assert.That(b.Activate(4,p),Is.True);Assert.That(b.Amount,Is.EqualTo(60));Assert.That(b.Activate(4,p),Is.False);
            b.Tick(0);Assert.That(b.FrenzyRemaining,Is.EqualTo(6));b.Tick(2);Assert.That(b.Amount,Is.EqualTo(54));b.Store(p);
            var reload=new DarklightBlood();reload.Restore(p);Assert.That(reload.FrenzyRemaining,Is.Zero);Assert.That(reload.Recovery[4],Is.EqualTo(18));Assert.That(reload.Amount,Is.EqualTo(54));
            Assert.That(reload.Activate(0,p),Is.True);Assert.That(reload.Activate(5,p),Is.False);
        }
        [Test] public void HealingSharesCeilingAndStopsDuringPause()
        {
            var b=new DarklightBlood();Assert.That(b.AllowHealing(1.4f,100),Is.EqualTo(1.4f));Assert.That(b.AllowHealing(5,100),Is.EqualTo(1.6f).Within(.001f));Assert.That(b.AllowHealing(9,100),Is.Zero);
            b.Tick(0);Assert.That(b.AllowHealing(9,100),Is.Zero);b.Tick(1);Assert.That(b.AllowHealing(9,100),Is.EqualTo(3));
        }
        [Test] public void VampireMasteryDoesNotFollowAgeOrGrantFreeAdvancedPowers()
        {
            var s=new DarklightProgress{appearanceStage=4,narrativeDays=500,points=4};Assert.That(s.LearnVampireSkill(5),Is.False);Assert.That(s.points,Is.EqualTo(4));
            s.CompleteMilestone(0,1);Assert.That(s.LearnVampireSkill(0),Is.True);Assert.That(s.LearnVampireSkill(0),Is.False);Assert.That(s.points,Is.EqualTo(3));Assert.That(s.LearnVampireSkill(5),Is.False);
            s.CompleteMilestone(8,3);Assert.That(s.LearnVampireSkill(5),Is.True);Assert.That(s.appearanceStage,Is.EqualTo(4));Assert.That(s.narrativeMilestones&(1L<<8),Is.Not.Zero);
        }
    }
}
