using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightSimplifiedScopeTests
    {
        [Test] public void OldRemovedSelectionsMigrateWithoutLosingProgressOrUnlockHistory()
        {
            var old=new DarklightProgress{chapter=8,checkpoint=3,level=17,attack=4,mastery=3,weaponUnlocks=15,equippedWeapon=1,preferredForm=3,missionWeaponHits=7,formsUnlocked=3};
            var migrated=DarklightProgress.Decode(JsonUtility.ToJson(old));
            Assert.That(migrated.equippedWeapon,Is.Zero);Assert.That(migrated.legacyEquippedWeapon,Is.EqualTo(1));
            Assert.That(migrated.preferredForm,Is.EqualTo(2));Assert.That(migrated.legacyPreferredForm,Is.EqualTo(3));
            Assert.That(migrated.weaponUnlocks,Is.EqualTo(15));Assert.That(migrated.formsUnlocked,Is.EqualTo(3));
            Assert.That(migrated.chapter,Is.EqualTo(8));Assert.That(migrated.checkpoint,Is.EqualTo(3));Assert.That(migrated.level,Is.EqualTo(17));
            Assert.That(migrated.attack,Is.EqualTo(4));Assert.That(migrated.mastery,Is.EqualTo(3));
            Assert.That(migrated.missionWeaponHits&DarklightPlayableScope.WeaponMask,Is.EqualTo(DarklightPlayableScope.WeaponMask));
            var again=DarklightProgress.Decode(JsonUtility.ToJson(migrated));
            Assert.That(again.legacyEquippedWeapon,Is.EqualTo(1));Assert.That(again.legacyPreferredForm,Is.EqualTo(3));
            Assert.That(old.equippedWeapon,Is.EqualTo(1),"Reading must not mutate the original save object");
        }
        [Test] public void NewCampaignDoesNotGainMotorOrLateProgressFromMigration()
        {
            var state=DarklightProgress.Decode(JsonUtility.ToJson(new DarklightProgress()));
            Assert.That(state.weaponUnlocks,Is.EqualTo(1));Assert.That(state.equippedWeapon,Is.Zero);
            Assert.That(state.FormUnlocked,Is.EqualTo(2));Assert.That(state.preferredForm,Is.EqualTo(1));
            Assert.That(state.missionWeaponHits,Is.Zero);Assert.That(state.completed,Is.False);
        }
        [Test] public void EclipseAbsolutoPreservesVioletDodgeAndWhiteConfirmedSkillLink()
        {
            var light=ScriptableObject.CreateInstance<AttackDefinition>();var skill=ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                light.kind=StrikeKind.Light;light.startup=.05f;light.active=.08f;light.recovery=.15f;
                skill.kind=StrikeKind.Skill;skill.startup=.05f;skill.active=.08f;skill.recovery=.15f;
                var loadout=new CombatLoadout{lights=new[]{light,light,light},heavy=light,charged=light,counter=light,skill=skill};
                var model=new CombatModel(loadout){FormStyle=2};
                model.Tick(.01f,new CombatInput{lightPressed=true});model.Tick(.14f,default);model.RegisterHit("target");
                model.Tick(.02f,new CombatInput{skillPressed=true});Assert.That(model.Attack,Is.SameAs(skill));
                model=new CombatModel(loadout){FormStyle=2};model.Tick(.01f,new CombatInput{lightPressed=true});model.Tick(.14f,default);
                model.Tick(.01f,new CombatInput{dodgePressed=true,move=Vector2.right});
                Assert.That(model.State,Is.EqualTo(CombatState.Dodge));Assert.That(model.ComboCount,Is.EqualTo(1));
            }
            finally{Object.DestroyImmediate(light);Object.DestroyImmediate(skill);}
        }
    }
}
