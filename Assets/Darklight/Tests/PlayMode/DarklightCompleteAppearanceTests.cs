using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Darklight.Tests
{
    public sealed class DarklightCompleteAppearanceTests
    {
        [UnitySetUp] public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;}
        [UnityTest] public IEnumerator YoungBreathingFramesIgnoreLegacyAgeInputsForBothSkins()
        {
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.Progress.equippedWeapon=0;game.Weapons.Restore();
            var view=lab.Player.GetComponent<DarklightCharacterView>();var age=lab.Player.GetComponent<CharacterAppearanceSystem>();
            var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int skin=0;skin<2;skin++)for(int stage=1;stage<5;stage++)for(int d=0;d<8;d++)
            {
                game.SelectSkin(skin,false);age.PreviewStage(stage);
                var facing=(Vector2)(Quaternion.Euler(0,0,-d*45)*Vector2.down);var model=new CombatModel(lab.Player.loadout);model.Tick(.01f,new CombatInput{aim=facing});view.Present(model,Vector2.zero);
                var authored=age.IdleBody(skin,d,model.Clock);
                Assert.That(authored.name,Does.StartWith(skin==0?"Today_RequiemIdleKatanaYoung":"Today_RockIdleKatanaYoung"));
                Assert.That(authored.texture.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(authored.bounds.size.y,Is.InRange(1.75f,2.25f));
                Assert.That(age.Stage,Is.Zero);
                var today=DarklightSpriteExpectations.Today(skin,stage,0,d,model,Vector2.zero);
                Assert.That(art.sprite,Is.EqualTo(today??authored));
                if(today!=null){Assert.That(art.flipX,Is.False);Assert.That(view.UsesCompleteAgeSprite,Is.True);}
                
                Assert.That(age.IdleBody(skin,d,.51f),Is.Not.EqualTo(age.IdleBody(skin,d,0)),"Missing second breathing pose");
            }
            yield return null;
        }
        [UnityTest] public IEnumerator RemovesSerializedHeadLayersAndKeepsFixedYoungAppearance()
        {
            var holder=new GameObject("LegacyAppearanceFixture");var age=holder.AddComponent<CharacterAppearanceSystem>();age.profile=DarklightCampaign.Instance.Appearance.profile;age.PreviewStage(4);
            var body=new GameObject("Body").AddComponent<SpriteRenderer>();body.transform.SetParent(holder.transform,false);
            foreach(var name in new[]{"Aparencia_Cabelo","Aparencia_Cavanhaque","Aparencia_Ombros"}){var layer=new GameObject(name).AddComponent<SpriteRenderer>();layer.transform.SetParent(body.transform,false);}
            var image=new GameObject("Portrait",typeof(RectTransform),typeof(Image)).GetComponent<Image>();image.transform.SetParent(holder.transform,false);
            foreach(var name in new[]{"AparenciaRetratoCabelo","AparenciaRetratoRosto"}){var layer=new GameObject(name,typeof(RectTransform),typeof(Image));layer.transform.SetParent(image.transform,false);}
            age.Present(body,0,0,0,null);age.UpdatePortrait(image);yield return null;
            Assert.That(body.transform.childCount,Is.Zero);Assert.That(image.transform.childCount,Is.Zero);Assert.That(age.Stage,Is.Zero);Assert.That(age.Physique,Is.Zero);
            Object.Destroy(holder);
        }
    }
}
