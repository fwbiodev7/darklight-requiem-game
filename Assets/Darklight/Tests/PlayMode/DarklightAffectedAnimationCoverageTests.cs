using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightAffectedAnimationCoverageTests
    {
        [UnityTest] public IEnumerator AllYoungWeaponsHaveFourContactAndRecoveryPhasesInEveryDirectionAndForm()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;game.PreviewChapter(4);game.Progress.weaponUnlocks=7;
            var lab=DarklightCombatLab.Instance;lab.SetPaused(true);game.enabled=false;lab.Player.enabled=false;
            var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            int checks=0;
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int form=0;form<4;form++)for(int d=0;d<8;d++)
            {
                Vector2 aim=Quaternion.Euler(0,0,-45*d)*Vector2.down;var definition=game.weaponDefinitions[weapon];var frames=new HashSet<Sprite>();
                for(int phase=0;phase<4;phase++)
                {
                    var model=new CombatModel(definition.loadout);model.Tick(.001f,new CombatInput{aim=aim,lightPressed=true});
                    float time=phase==0?0:phase==1?model.Attack.startup+.002f:phase==2?model.Attack.startup+model.Attack.active+model.Attack.recovery*.25f:model.Attack.startup+model.Attack.active+model.Attack.recovery*.8f;
                    while(model.StateTime<time)model.Tick(Mathf.Min(.005f,time-model.StateTime),new CombatInput{aim=aim});
                    view.PresentPreview(model,Vector2.zero,skin,4,form,definition);
                    var page=TodayCharacterBank.Page(skin,0,weapon,d,"Attack");Assert.That(page,Is.Not.Null);
                    Assert.That(art.sprite,Is.SameAs(DarklightAgeWeaponRegressionTests.ExpectedWhole(page.frames[System.Array.IndexOf(page.directions,d)*4+phase],form)));
                    Assert.That(view.UsesPoseFallback,Is.False);Assert.That(game.Appearance.Stage,Is.Zero);DarklightAgeWeaponRegressionTests.AssertIdentity(art.sprite,skin,0,weapon);
                    var block=new MaterialPropertyBlock();art.GetPropertyBlock(block);Assert.That(block.GetFloat("_VampireForm"),Is.Zero);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                    frames.Add(art.sprite);checks++;
                    if((skin==0&&weapon==0&&d==1)||(skin==1&&weapon==2&&d==3))DarklightReviewedAnimationCapture.Save($"young-s{skin}-w{weapon}-d{d}-f{form}-p{phase}");
                }
                Assert.That(frames.Count,Is.EqualTo(4),"Static pose replacing attack");
            }
            Assert.That(checks,Is.EqualTo(768));yield return null;
        }
    }
}

