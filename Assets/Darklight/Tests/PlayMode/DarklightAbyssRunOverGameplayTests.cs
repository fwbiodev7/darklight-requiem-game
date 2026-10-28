using System.Collections;
using System.Collections.Generic;
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
        [UnityTest]public IEnumerator CaptureRunOverPrototypeFromRealInputs()
        {
            string root = DarklightTestOutput.GetDirectory("cavaleiro-sprites-20261015/capturas/runover");
            var pad=InputSystem.AddDevice<Gamepad>();int oldRate=Time.captureFramerate;string bindings=lab.Input.Asset.SaveBindingOverridesAsJson();
            try
            {
                lab.Input.Action("AbyssBike").RemoveAllBindingOverrides();lab.Input.Action("AbyssSaw").RemoveAllBindingOverrides();Time.captureFramerate=30;
                var views=new[]{Vector2.down,new Vector2(-1,-1).normalized,Vector2.left};
                var labels=new[]{"frente","diagonal","lateral"};
                var report=new List<string>();
                for(int clip=0;clip<views.Length;clip++)
                {
                    lab.SetPaused(true);InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                    var enemy=PracticeVictim(views[clip]*.8f);enemy.kind=RequiemEnemyKind.Crawler;enemy.enabled=true;
                    // Training passive AI is intentional; damage, stagger and physics remain real.
                    fighter.Model.Tick(.01f,new CombatInput{aim=views[clip]});
                    var camera=lab.WorldCamera.GetComponent<DarklightLabCamera>();camera.enabled=false;lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.7f,-10);
                    string folder=root+labels[clip]+"/";Directory.CreateDirectory(folder);var times=new List<float>();var poses=new HashSet<string>();
                    bool confirmed=false,cleared=false,mounted=false,jumped=false,continued=false,queued=false,overlap=false;float firstDamage=-1;int count=0;float start=Time.time;
                    Call(game,"Close");yield return null;yield return null;
                    InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.North));
                    for(int frame=0;frame<90;frame++)
                    {
                        yield return null;
                        var m=fighter.Model;mounted|=rider.Dashing&&m.ActiveThisTick;confirmed|=rider.RunOverConfirmed;cleared|=rider.RunOverCleared;jumped|=rider.JumpHeight>.06f;
                        if(rider.RunOverConfirmed)
                        {
                            overlap|=fighter.GetComponent<Collider2D>().Distance(enemy.GetComponent<Collider2D>()).isOverlapped;
                            if(firstDamage<0)firstDamage=enemy.Health.CurrentHealth;
                        }
                        bool press=!queued&&rider.Dashing&&m.StateTime>=m.Attack.startup+m.Attack.active+.15f;
                        if(press)queued=true;
                        // Aim back toward the crossed enemy during the landing, then tap X.
                        Vector2 aim=queued?((Vector2)enemy.transform.position-body.position).normalized:views[clip];
                        // The game's aim follows movement; right stick is reserved for vampire powers.
                        var state=new GamepadState{leftStick=queued?aim*.5f:Vector2.zero};if(press)state=state.WithButton(GamepadButton.West);InputSystem.QueueStateEvent(pad,state);
                        continued|=queued&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&m.ActiveThisTick;
                        if(frame%2==0)
                        {
                            poses.Add(fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>().sprite.name);
                            typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"frame-"+(count++).ToString("D4")+".png"});times.Add(Time.time-start);
                        }
                    }
                    lab.SetPaused(true);InputSystem.QueueStateEvent(pad,new GamepadState());
                    File.WriteAllText(folder+"timestamps.json",JsonUtility.ToJson(new FrameTimes{seconds=times.ToArray(),count=count}));
                    File.WriteAllText(folder+"evidence.json","{\"confirmed\":"+confirmed.ToString().ToLowerInvariant()+",\"crossed\":"+cleared.ToString().ToLowerInvariant()+",\"overlapWhileMounted\":"+overlap.ToString().ToLowerInvariant()+",\"jumped\":"+jumped.ToString().ToLowerInvariant()+",\"sawFollowup\":"+continued.ToString().ToLowerInvariant()+",\"poses\":"+poses.Count+",\"captureFramerate\":30}");
                    Assert.That(mounted&&confirmed&&cleared&&jumped&&continued&&overlap,Is.True,"Missing real input/physics stage: "+File.ReadAllText(folder+"evidence.json"));
                    Assert.That(Physics2D.GetIgnoreCollision(fighter.GetComponent<Collider2D>(),enemy.GetComponent<Collider2D>()),Is.False,"Victim collision must be restored after the rear wheel passes");
                    Assert.That(poses.Count,Is.GreaterThanOrEqualTo(7));Assert.That(enemy.Health.CurrentHealth,Is.LessThan(firstDamage),"Returning saw combo must actually damage the struck enemy");
                    Assert.That(rider.DashDistance,Is.LessThanOrEqualTo(DarklightAbyssRider.MaximumDashDistance+.001f));camera.enabled=true;
                    report.Add(labels[clip]+": mounted impact, real crossing, vault/split and saw damage");
                }
                File.WriteAllLines(root+"report.txt",report);
            }
            finally{lab.SetPaused(true);Time.captureFramerate=oldRate;InputSystem.RemoveDevice(pad);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
    }
}
