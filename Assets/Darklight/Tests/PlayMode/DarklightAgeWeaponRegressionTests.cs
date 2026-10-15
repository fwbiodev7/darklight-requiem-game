using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightAgeWeaponRegressionTests
    {
        [UnitySetUp] public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;DarklightCampaign.Instance.PreviewCampaign();DarklightCampaign.Instance.Progress.weaponUnlocks=7;}
        internal static Sprite ExpectedWhole(Sprite normal,int form)
        {
            if(form==0)return normal;
            Assert.That(DarklightTransformedBank.TryResolve(normal,form,out var complete,out var mask,out var owner),Is.True,"Missing authored transformation for "+normal.name+":"+form);
            Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.InRange(1,32));return complete;
        }
        internal static void AssertIdentity(Sprite sprite,int skin,int age,int weapon)
        {
            Assert.That(sprite,Is.Not.Null);string n=sprite.name,who=skin==1?"Rock":"Requiem",arm=new[]{"Katana","Daggers","Chain"}[weapon];
            Assert.That(n.StartsWith("Today_")||n.StartsWith("Transformed_"),Is.True);Assert.That(n,Does.Contain(who));
            Assert.That(n,Does.Contain("Young"));Assert.That(n,Does.Not.Contain("Stage"));
            Assert.That(n,Does.Contain(arm),"Wrong equipped weapon in young sprite: "+n);

        }
        [UnityTest] public IEnumerator LegacyAgeInputsCannotChangeYoungIdentityAcrossAllEquipmentAndForms()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;var view=lab.Player.GetComponent<DarklightCharacterView>();var body=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var block=new MaterialPropertyBlock();int checks=0;
            for(int skin=0;skin<2;skin++)for(int age=0;age<5;age++)for(int weapon=0;weapon<3;weapon++)
            {
                for(int form=0;form<4;form++)for(int direction=0;direction<8;direction++)for(int pose=0;pose<5;pose++)
                {
                    var definition=game.weaponDefinitions[weapon];var model=new CombatModel(definition.loadout);var facing=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);var move=pose==1?facing:Vector2.zero;
                    model.Tick(.01f,new CombatInput{aim=facing,move=move,lightPressed=pose==2,parryPressed=pose==3,dodgePressed=pose==4});
                    if(pose==2)model.Tick(model.Attack.startup+.01f,new CombatInput{aim=facing});
                    view.PresentPreview(model,move,skin,age,form,definition);AssertIdentity(body.sprite,skin,age,weapon);
                    body.GetPropertyBlock(block);Assert.That(block.GetFloat("_VampireForm"),Is.Zero);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                    Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));Assert.That(view.GetComponent<DarklightOriginalVampireView>().HasAgeHead,Is.False);
                    Assert.That(game.Appearance.Stage,Is.Zero);checks++;
                }
                TodayCharacterBank.ClearCache();yield return Resources.UnloadUnusedAssets();
            }
            Assert.That(checks,Is.EqualTo(4800));
        }
        [UnityTest] public IEnumerator ActualSequentialWeaponSwapsKeepYoungIdentityAndEquippedWeapon()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int skin=0;skin<2;skin++)for(int age=1;age<5;age++)
            {
                game.SelectSkin(skin,false);game.Appearance.PreviewStage(age);game.Progress.weaponUnlocks=7;game.Progress.equippedWeapon=0;game.Weapons.Restore();
                foreach(int weapon in new[]{1,2,0,2,1,0})
                {
                    game.Weapons.Request(weapon);game.Weapons.BeforeTick(.01f,true);Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(weapon));Assert.That(game.Weapons.Swapping,Is.True);
                    view.Present(lab.Player.Model,Vector2.zero);AssertIdentity(art.sprite,skin,age,weapon);Assert.That(game.Appearance.Stage,Is.Zero);
                    game.Weapons.BeforeTick(.2f,true);view.Present(lab.Player.Model,Vector2.zero);AssertIdentity(art.sprite,skin,age,weapon);
                }
                TodayCharacterBank.ClearCache();yield return Resources.UnloadUnusedAssets();
            }
        }
    }
}

