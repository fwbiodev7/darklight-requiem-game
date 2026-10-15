using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightOriginalVampireTests
    {
        [UnitySetUp] public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;}
        [UnityTest] public IEnumerator EveryLegacyAgeFormWeaponAndActionKeepsYoungPosesAndScale()
        {
            var game=DarklightCampaign.Instance;var view=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();var body=DarklightCombatLab.Instance.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var catalog=Resources.Load<DarklightOriginalVampireCatalog>("Darklight/OriginalVampireCatalog");Assert.That(catalog,Is.Not.Null);Assert.That(catalog.aura.Length,Is.EqualTo(12));
            var block=new MaterialPropertyBlock();
            for(int skin=0;skin<2;skin++)for(int age=1;age<=4;age++)for(int weapon=0;weapon<3;weapon++)for(int d=0;d<8;d++)for(int action=0;action<4;action++)
            {
                var definition=game.weaponDefinitions[weapon];var m=new CombatModel(definition.loadout);Vector2 aim=Quaternion.Euler(0,0,-d*45)*Vector2.down,move=action==0?aim:Vector2.zero;
                m.Tick(.001f,new CombatInput{aim=aim,move=move,lightPressed=action==1,parryPressed=action==2,dodgePressed=action==3});m.Tick(.05f,new CombatInput{aim=aim,move=move});
                view.PresentPreview(m,move,skin,age,0,definition);var original=body.sprite;var state=m.State;var clock=m.Clock;
                var today=DarklightSpriteExpectations.Today(skin,age,weapon,d,m,move);
                Assert.That(TodayCharacterBank.Mask(original)!=null||DarklightWholeCharacterBank.Mask(original)!=null||catalog.Find(original)!=null,Is.True,"Unrecognized complete frame: "+original.name);
                if(today!=null)Assert.That(original,Is.SameAs(today));
                for(int form=1;form<=3;form++)
                {
                    view.PresentPreview(m,move,skin,age,form,definition);
                    Assert.That(body.sprite,Is.SameAs(DarklightAgeWeaponRegressionTests.ExpectedWhole(original,form)),"Transformation must resolve the matching complete authored pose");
                    Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                    Assert.That(m.State,Is.EqualTo(state));Assert.That(m.Clock,Is.EqualTo(clock));
                    body.GetPropertyBlock(block);Assert.That(block.GetFloat("_DarkForm"),Is.Zero);Assert.That(block.GetFloat("_VampireForm"),Is.Zero);
                    var layers=view.GetComponent<DarklightOriginalVampireView>();Assert.That(layers.PresentedAura,Is.Not.Null);
                    Assert.That(layers.PresentedHead,Is.Null);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                    Assert.That(body.transform.Find("IdadeOriginal"),Is.Null);
                }
            }
            Assert.That(body.transform.parent.Find("AsasEspectrais"),Is.Null);yield return null;
        }
        [UnityTest] public IEnumerator FormExitHidesAuraAndKeepsYoungAppearance()
        {
            var view=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();var weapon=DarklightCampaign.Instance.weaponDefinitions[0];var m=new CombatModel(weapon.loadout);m.Tick(.01f,new CombatInput{aim=Vector2.down});
            view.PresentPreview(m,Vector2.zero,1,4,3,weapon);var layers=view.GetComponent<DarklightOriginalVampireView>();Assert.That(layers.PresentedAura,Is.Not.Null);
            view.PresentPreview(m,Vector2.zero,1,4,0,weapon);Assert.That(layers.PresentedAura,Is.Null);Assert.That(view.GetComponent<CharacterAppearanceSystem>().Stage,Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator RaisedKatanaUsesTheOriginalFaceBelowTheWeapon()
        {
            var pose=DarklightCampaign.Instance.rockSlash[0];var frame=Resources.Load<DarklightOriginalVampireCatalog>("Darklight/OriginalVampireCatalog").Find(pose);
            // The original raised katana extends above the scalp. The mouth remains lower:
            // selecting the top of this rectangle produced a second face in the preview.
            Assert.That(frame.lip.y,Is.InRange(1.1f,1.3f));Assert.That(pose.bounds.max.y-frame.lip.y,Is.GreaterThan(.7f));
            yield return null;
        }
    }
}