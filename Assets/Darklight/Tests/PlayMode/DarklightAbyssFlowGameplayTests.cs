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
        [UnityTest]public IEnumerator RealInputsChainSawBikeDriftSlamAndReturnAcrossFourForms()
        {
            string root = DarklightTestOutput.GetDirectory("cavaleiro-sprites-20261015/capturas/flow");
            var keys=InputSystem.AddDevice<Keyboard>();string bindings=lab.Input.Asset.SaveBindingOverridesAsJson();int oldRate=Time.captureFramerate;
            try
            {
                lab.Input.Action("AbyssSaw").ApplyBindingOverride(0,"<Keyboard>/i");lab.Input.Action("AbyssBike").ApplyBindingOverride(0,"<Keyboard>/p");lab.Input.Action("AbyssDrift").ApplyBindingOverride(0,"<Keyboard>/u");lab.Input.Action("Skill").ApplyBindingOverride(0,"<Keyboard>/o");Time.captureFramerate=30;
                for(int form=0;form<4;form++)
                {
                    InputSystem.QueueStateEvent(keys,new KeyboardState());lab.SetPaused(true);fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                    Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);var enemy=PracticeVictim(Vector2.down*.8f);enemy.kind=RequiemEnemyKind.Crawler;enemy.enabled=true;
                    fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.down});string folder=root+"forma"+form+"/";Directory.CreateDirectory(folder);
                    var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.8f,-10);
                    bool bikeSent=false,driftSent=false,slamSent=false,sawSent=false,sawBike=false,sawDrift=false,sawSlam=false,sawReturn=false;int frame=0;float start=Time.time;var times=new List<float>();var poses=new HashSet<string>();var trace=new List<string>();
                    Call(game,"Close");yield return null;yield return null;InputSystem.QueueStateEvent(keys,new KeyboardState(Key.I));
                    for(int tick=0;tick<180&&!sawReturn;tick++)
                    {
                        yield return null;var m=fighter.Model;var press=new List<Key>();
                        if(!bikeSent)
                        {
                            press.Add(Key.I);press.Add(Key.S); // Close the distance using the actual movement control.
                            if(m.State==CombatState.Attack&&m.ActiveThisTick&&m.AttackConnected){press.Add(Key.P);bikeSent=true;}
                        }
                        if(rider.Dashing&&!rider.Drifting&&!driftSent){press.Add(Key.U);driftSent=true;}
                        if(rider.Drifting&&m.ActiveThisTick&&!slamSent){press.Add(Key.O);slamSent=true;}
                        if(rider.Slamming&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active+.24f&&!sawSent){press.Add(Key.I);sawSent=true;}
                        InputSystem.QueueStateEvent(keys,new KeyboardState(press.ToArray()));
                        trace.Add(tick+" "+m.State+" "+m.Attack?.displayName+" t="+m.StateTime.ToString("F3")+" combo="+m.ComboCount+" held="+lab.Input.Action("AbyssSaw").IsPressed()+" edge="+lab.Input.Action("AbyssSaw").WasPressedThisFrame()+" next="+string.Join(",",press));
                        sawBike|=rider.Dashing&&!rider.Drifting;sawDrift|=rider.Drifting;sawSlam|=rider.Slamming;
                        sawReturn|=sawSent&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&m.ActiveThisTick;
                        if(tick%2==0)
                        {
                            poses.Add(fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>().sprite.name);
                            typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"frame-"+(frame++).ToString("D4")+".png"});times.Add(Time.time-start);
                        }
                    }
                    lab.SetPaused(true);File.WriteAllLines(folder+"trace.txt",trace);File.WriteAllText(folder+"timestamps.json",JsonUtility.ToJson(new FrameTimes{seconds=times.ToArray(),count=frame}));
                    File.WriteAllText(folder+"evidence.json","{\"form\":"+form+",\"bike\":"+sawBike.ToString().ToLowerInvariant()+",\"drift\":"+sawDrift.ToString().ToLowerInvariant()+",\"slam\":"+sawSlam.ToString().ToLowerInvariant()+",\"returnToSaws\":"+sawReturn.ToString().ToLowerInvariant()+",\"blocked\":"+rider.DashBlocked.ToString().ToLowerInvariant()+",\"runOver\":"+rider.RunOverConfirmed.ToString().ToLowerInvariant()+",\"distance\":"+rider.DashDistance.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"poses\":"+poses.Count+"}");
                    Assert.That(sawBike&&sawDrift&&sawSlam&&sawReturn,Is.True,"Real chain failed: "+File.ReadAllText(folder+"evidence.json"));Assert.That(poses.Count,Is.GreaterThanOrEqualTo(7));Assert.That(rider.Sustaining,Is.False);follow.enabled=true;
                }
            }
            finally{lab.SetPaused(true);Time.captureFramerate=oldRate;InputSystem.RemoveDevice(keys);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
    }
}
