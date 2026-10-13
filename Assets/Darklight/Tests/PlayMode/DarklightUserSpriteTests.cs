using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightUserSpriteTests
    {
        [UnitySetUp] public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;}
        [UnityTest] public IEnumerator WholeSpritesReplacePriorHeadMasksAcrossAllEquipment()
        {
            var game=DarklightCampaign.Instance;var view=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();var body=view.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();var block=new MaterialPropertyBlock();
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)
            {
                var definition=game.weaponDefinitions[weapon];var m=new CombatModel(definition.loadout);m.Tick(.02f,new CombatInput{aim=Vector2.right});
                view.PresentPreview(m,Vector2.zero,skin,4,1,definition);view.PresentPreview(m,Vector2.zero,skin,0,0,definition);
                var page=DarklightUserSpriteBank.Page(skin,weapon);Assert.That(page.Length,Is.EqualTo(8));Assert.That(body.sprite,Is.SameAs(page[7]));Assert.That(view.UsesUserWholeSprite,Is.True);
                Assert.That(view.GetComponent<DarklightOriginalVampireView>().HasAgeHead,Is.False);body.GetPropertyBlock(block);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                foreach(var sprite in page){Assert.That(sprite.pivot.x,Is.EqualTo(384).Within(.01));Assert.That(sprite.pivot.y,Is.EqualTo(32).Within(.01));Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprite.texture.mipmapCount,Is.EqualTo(1));}
                view.PresentPreview(m,Vector2.zero,skin,3,0,definition);Assert.That(view.UsesUserWholeSprite,Is.False);Assert.That(game.Appearance.Stage,Is.EqualTo(3));
            }
            yield return null;
        }
        [UnityTest] public IEnumerator ThreePalettesKeepWholeCharactersAndAuraAttached()
        {
            var game=DarklightCampaign.Instance;var view=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();var body=view.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();var block=new MaterialPropertyBlock();
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int form=1;form<=3;form++)
            {
                var definition=game.weaponDefinitions[weapon];var m=new CombatModel(definition.loadout);m.Tick(.001f,new CombatInput{aim=Vector2.right,dodgePressed=true});m.Tick(.05f,new CombatInput());
                view.PresentPreview(m,Vector2.zero,skin,0,form,definition);var layers=view.GetComponent<DarklightOriginalVampireView>();body.GetPropertyBlock(block);
                Assert.That(view.UsesUserWholeSprite,Is.True);Assert.That(layers.HasAgeHead,Is.False);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);Assert.That(block.GetFloat("_WholeSprite"),Is.EqualTo(1));Assert.That(block.GetFloat("_VampireForm"),Is.EqualTo(form));
                var mask=DarklightUserSpriteBank.Mask(body.sprite);Assert.That(mask,Is.Not.SameAs(Texture2D.blackTexture));Assert.That(mask.width,Is.EqualTo(body.sprite.texture.width));Assert.That(mask.height,Is.EqualTo(body.sprite.texture.height));Assert.That(mask.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(block.GetTexture("_VampireMask"),Is.SameAs(mask));
                var aura=body.transform.Find("AuraVampiraOriginal");Assert.That(aura.GetComponent<SpriteRenderer>().sprite,Is.SameAs(layers.PresentedAura));Assert.That(aura.localPosition.y,Is.EqualTo(.76f).Within(.001));
                var before=aura.position;var shift=new Vector3(.3f,.2f,0);body.transform.position+=shift;Assert.That(Vector3.Distance(aura.position-before,shift),Is.LessThan(.0001f));body.transform.position-=shift;
                view.PresentPreview(m,Vector2.zero,skin,0,0,definition);Assert.That(layers.PresentedAura,Is.Null);
            }
            yield return null;
        }
        [UnityTest] public IEnumerator SuppliedActionFramesFollowTheCombatClock()
        {
            var game=DarklightCampaign.Instance;var view=DarklightCombatLab.Instance.Player.GetComponent<DarklightCharacterView>();var body=view.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int weapon=0;weapon<3;weapon++)
            {
                var definition=game.weaponDefinitions[weapon];var m=new CombatModel(definition.loadout);m.Tick(.001f,new CombatInput{aim=Vector2.right,lightPressed=true});
                Assert.That(DarklightUserSpriteBank.Pose(m,Vector2.zero),Is.EqualTo(2));m.Tick(m.Attack.startup+.01f,new CombatInput{aim=Vector2.right});
                view.PresentPreview(m,Vector2.zero,1,0,0,definition);Assert.That(body.sprite,Is.SameAs(DarklightUserSpriteBank.Page(1,weapon)[3]));
                m=new CombatModel(definition.loadout);m.Tick(.001f,new CombatInput{aim=Vector2.left,parryPressed=true});view.PresentPreview(m,Vector2.zero,1,0,0,definition);Assert.That(body.sprite,Is.SameAs(DarklightUserSpriteBank.Page(1,weapon)[4]));Assert.That(body.flipX,Is.True);
                m=new CombatModel(definition.loadout);m.Tick(.001f,new CombatInput{aim=Vector2.right,dodgePressed=true});m.Tick(.05f,new CombatInput());Assert.That(DarklightUserSpriteBank.Pose(m,Vector2.zero),Is.EqualTo(5));m.Tick(.12f,new CombatInput());Assert.That(DarklightUserSpriteBank.Pose(m,Vector2.zero),Is.EqualTo(6));
                m=new CombatModel(definition.loadout);m.Tick(.02f,new CombatInput{move=Vector2.down});Assert.That(DarklightUserSpriteBank.Pose(m,Vector2.down),Is.Zero);m.Tick(.14f,new CombatInput{move=Vector2.down});Assert.That(DarklightUserSpriteBank.Pose(m,Vector2.down),Is.EqualTo(1));
            }
            yield return null;
        }
    }
}
