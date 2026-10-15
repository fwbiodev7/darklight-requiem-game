using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightAuthoredFormsTests
    {
        [UnityTest]public IEnumerator VioletIdleCyclesWholeAuthoredFiguresForBothSkinsAndAllDirections()
        {
            DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;lab.SetPaused(true);
            var view=lab.Player.GetComponent<DarklightCharacterView>();var body=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int skin=0;skin<2;skin++)for(int direction=0;direction<8;direction++)
            {
                var model=new CombatModel(game.weaponDefinitions[0].loadout);var vector=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);
                model.Tick(.01f,new CombatInput{aim=vector});view.PresentPreview(model,Vector2.zero,skin,0,2,game.weaponDefinitions[0]);var exhale=body.sprite;var scale=body.transform.localScale;
                Assert.That(exhale.name,Does.Contain("IdleKatanaYoung_Violet"));
                model.Tick(.51f,new CombatInput{aim=vector});view.PresentPreview(model,Vector2.zero,skin,0,2,game.weaponDefinitions[0]);
                Assert.That(body.sprite,Is.Not.EqualTo(exhale));Assert.That(body.sprite.name,Does.Contain("IdleKatanaYoung_Violet"));Assert.That(body.transform.localScale,Is.EqualTo(scale));
                model.Tick(.5f,new CombatInput{aim=vector});view.PresentPreview(model,Vector2.zero,skin,0,2,game.weaponDefinitions[0]);Assert.That(body.sprite,Is.EqualTo(exhale));
            }
        }
        [UnityTest] public IEnumerator AuthoredWalkingFormsKeepFullBodiesFootAnchorsAndUniformScale()
        {
            DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;lab.SetPaused(true);
            var view=lab.Player.GetComponent<DarklightCharacterView>();var body=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            int captured=0;var block=new MaterialPropertyBlock();
            for(int skin=0;skin<2;skin++)for(int form=1;form<=3;form++)for(int direction=0;direction<8;direction++)
            {
                var vector=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);var model=new CombatModel(game.Weapons.Equipped.loadout);model.Tick(.01f,new CombatInput{move=vector,aim=vector});
                var normal=TodayCharacterBank.Resolve(skin,0,0,direction,model,vector);Assert.That(DarklightTransformedBank.TryResolve(normal,form,out var frame,out var mask,out int owner),Is.True);
                view.PresentPreview(model,vector,skin,0,form,game.Weapons.Equipped);Assert.That(body.sprite,Is.EqualTo(frame));Assert.That(frame.name,Does.StartWith("Transformed_"));Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.InRange(1,32));
                body.GetPropertyBlock(block);Assert.That(block.GetFloat("_VampireForm"),Is.Zero);Assert.That(block.GetFloat("_WholeFrameId"),Is.EqualTo(owner));Assert.That(block.GetTexture("_VampireMask"),Is.EqualTo(mask));
                Assert.That(body.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));Assert.That(Mathf.Abs(frame.bounds.size.y/normal.bounds.size.y-1),Is.LessThan(.16f),frame.name);
                Assert.That(Mathf.Abs(frame.bounds.min.y-normal.bounds.min.y),Is.LessThan(.08f),frame.name);captured++;
            }
            Assert.That(captured,Is.EqualTo(48));
        }
    }
}
