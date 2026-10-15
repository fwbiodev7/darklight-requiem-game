using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightWholeAttackPhaseTests
    {
        [UnityTest] public IEnumerator YoungDaggersShowFourWholeAttackPhasesInBothSkinsEveryDirectionAndForm()
        {
            DarklightAutomation.Silent=true;
            AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            yield return null;yield return null;
            var game=DarklightCampaign.Instance;
            game.PreviewChapter(4);game.Progress.weaponUnlocks=7;game.Progress.equippedWeapon=1;game.Weapons.Restore();
            var lab=DarklightCombatLab.Instance;
            lab.SetPaused(true);game.enabled=false;lab.Player.enabled=false;
            typeof(DarklightCampaign).GetMethod("TickMinimalHUD",BindingFlags.NonPublic|BindingFlags.Instance)?.Invoke(game,null);
            var view=lab.Player.GetComponent<DarklightCharacterView>();
            var display=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var block=new MaterialPropertyBlock();
            string folder=PlayerPrefs.GetString("Darklight.Tests.AttackCaptureOutput","");
            if(folder.Length>0)Directory.CreateDirectory(folder);
            var capture=Object.FindFirstObjectByType<DarklightDeliveryCapture>();
            var method=typeof(DarklightDeliveryCapture).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Instance);
            int checks=0;
            for(int skin=0;skin<2;skin++)for(int form=0;form<4;form++)for(int direction=0;direction<8;direction++)
            {
                var aim=(Vector2)(Quaternion.Euler(0,0,-45*direction)*Vector2.down);
                var definition=game.weaponDefinitions[1];
                var names=new HashSet<string>();
                for(int phase=0;phase<4;phase++)
                {
                    var model=new CombatModel(definition.loadout);
                    model.Tick(.001f,new CombatInput{aim=aim,lightPressed=true});
                    float time=phase==0?0:phase==1?model.Attack.startup+.002f:
                        phase==2?model.Attack.startup+model.Attack.active+model.Attack.recovery*.25f:
                        model.Attack.startup+model.Attack.active+model.Attack.recovery*.8f;
                    while(model.StateTime<time)model.Tick(Mathf.Min(.005f,time-model.StateTime),new CombatInput{aim=aim});
                    view.PresentPreview(model,Vector2.zero,skin,3,form,definition);
                    Assert.That(display.sprite.name,Does.StartWith((form>0?"Transformed_":"Today_")+(skin==1?"Rock":"Requiem")+"AttackDaggersYoung"));
                    var page=TodayCharacterBank.Page(skin,3,1,direction,"Attack");
                    Assert.That(page,Is.Not.Null);
                    Assert.That(display.sprite,Is.SameAs(DarklightAgeWeaponRegressionTests.ExpectedWhole(page.frames[System.Array.IndexOf(page.directions,direction)*4+phase],form)));
                    Assert.That(view.UsesAgePoseFallback,Is.False);
                    Assert.That(game.Appearance.Stage,Is.Zero);
                    Assert.That(display.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                    display.GetPropertyBlock(block);
                    Assert.That(block.GetFloat("_VampireForm"),Is.Zero);
                    Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                    Assert.That(view.GetComponent<DarklightOriginalVampireView>().HasAgeHead,Is.False);
                    names.Add(display.sprite.name);checks++;
                    if(folder.Length>0&&capture!=null&&method!=null)
                    {
                        typeof(DarklightPixelHUD).GetMethod("LateUpdate",BindingFlags.NonPublic|BindingFlags.Instance)?.Invoke(lab.PixelHUD,null);
                        Canvas.ForceUpdateCanvases();
                        method.Invoke(capture,new object[]{Path.Combine(folder,$"{(skin==1?"Rock":"Requiem")}-young-daggers-dir{direction}-form{form}-phase{phase}.png")});
                    }
                }
                Assert.That(names.Count,Is.EqualTo(4),"A static pose cannot stand in for a complete attack");
            }
            Assert.That(checks,Is.EqualTo(256));
            yield return null;
        }
    }
}
