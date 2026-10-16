using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        const string ControllerReview="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/avaliacao-video-20261016/";
        [UnityTest]public IEnumerator DefaultControllerSeparatesDriftFromMountedSlamInEveryForm()
        {
            var pad=InputSystem.AddDevice<Gamepad>();var previous=lab.Input.Asset.SaveBindingOverridesAsJson();int rate=Time.captureFramerate;
            try
            {
                lab.Input.Asset.RemoveAllBindingOverrides();Time.captureFramerate=30;
                var slot=lab.Input.Slots("AbyssDrift",true)[0];
                Assert.That(slot.binding.effectivePath,Is.EqualTo("<Gamepad>/dpad/up"));
                Assert.That(lab.Input.Conflicts(slot,"<Gamepad>/dpad/up").Count,Is.Zero);
                Assert.That(lab.Input.Conflicts(slot,"<Gamepad>/rightShoulder").Count,Is.GreaterThan(0),"RB already owns the mounted slam");
                Directory.CreateDirectory(ControllerReview+"capturas");int complete=0;
                for(int form=0;form<4;form++)
                {
                    InputSystem.QueueStateEvent(pad,new GamepadState());Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                    foreach(var enemy in game.MapEnemies){enemy.enabled=false;enemy.GetComponent<Rigidbody2D>().position=game.Map.Spawn+Vector2.right*6;enemy.transform.position=enemy.GetComponent<Rigidbody2D>().position;}
                    Physics2D.SyncTransforms();Call(game,"Close");yield return null;yield return null;
                    var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.6f,-10);
                    InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.North));
                    bool sentDrift=false,sentSlam=false,sentSaw=false,drift=false,slam=false,saw=false;int potions=game.Progress.potions;
                    for(int tick=0;tick<180&&!saw;tick++)
                    {
                        yield return null;var m=fighter.Model;var state=new GamepadState();
                        if(rider.Dashing&&!rider.Drifting&&!sentDrift){state=state.WithButton(GamepadButton.DpadUp);sentDrift=true;}
                        if(rider.Drifting&&m.ActiveThisTick&&!sentSlam){state=state.WithButton(GamepadButton.RightShoulder);sentSlam=true;}
                        if(rider.Slamming&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active+.24f&&!sentSaw){state=state.WithButton(GamepadButton.West);sentSaw=true;}
                        InputSystem.QueueStateEvent(pad,state);drift|=rider.Drifting;slam|=rider.Slamming&&rider.SlamFromMounted;
                        saw|=sentSaw&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&m.ActiveThisTick;
                        if(rider.Slamming&&m.StateTime>.2f&&m.StateTime<.25f)
                        {Call(game,"UpdateAbyssHUD");typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{ControllerReview+"capturas/forma"+form+"-controle.png"});}
                    }
                    Assert.That(drift&&slam&&saw,Is.True,"Default gamepad chain failed in form "+form);Assert.That(game.Progress.potions,Is.EqualTo(potions));complete++;follow.enabled=true;
                }
                File.WriteAllText(ControllerReview+"controller-evidence.json","{\"forms\":"+complete+",\"defaultGamepad\":true,\"drift\":\"dpad/up\",\"slam\":\"rightShoulder\",\"mountedSlamAndSawReturn\":true,\"syntheticController\":true}");
            }
            finally{lab.SetPaused(true);Time.captureFramerate=rate;InputSystem.RemoveDevice(pad);lab.Input.Asset.LoadBindingOverridesFromJson(previous,true);}
        }
        [UnityTest]public IEnumerator LegacyCustomDriftAndOriginalWeaponControlsRemainIntact()
        {
            string saved=PlayerPrefs.GetString("Darklight.Bindings","");bool existed=PlayerPrefs.HasKey("Darklight.Bindings");
            try
            {
                PlayerPrefs.SetString("Darklight.Bindings","{\"version\":2,\"bindings\":[{\"map\":\"Gameplay\",\"action\":\"AbyssDrift\",\"index\":1,\"defaultPath\":\"<Gamepad>/rightShoulder\",\"path\":\"<Gamepad>/leftStickPress\"}]}");
                using(var input=new DarklightInput())
                {
                    Assert.That(input.Action("AbyssDrift").bindings[1].effectivePath,Is.EqualTo("<Gamepad>/leftStickPress"));
                    Assert.That(input.Action("Skill").bindings[1].effectivePath,Is.EqualTo("<Gamepad>/rightShoulder"));
                    Assert.That(input.Action("Aerial").bindings[1].effectivePath,Is.EqualTo("<Gamepad>/dpad/up"));
                    Assert.That(input.Action("Potion").bindings[1].effectivePath,Is.EqualTo("<Gamepad>/rightTrigger"));
                    Assert.That(input.Action("AbyssDrift").bindings[0].effectivePath,Is.EqualTo("<Keyboard>/r"));
                }
            }
            finally{if(existed)PlayerPrefs.SetString("Darklight.Bindings",saved);else PlayerPrefs.DeleteKey("Darklight.Bindings");PlayerPrefs.Save();}
            yield return null;
        }
    }
}
