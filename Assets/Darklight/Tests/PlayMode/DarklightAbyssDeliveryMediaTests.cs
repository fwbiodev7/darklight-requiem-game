using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        [Serializable]sealed class DeliveryClip {public string name;public int frames,uniquePoses;public float damage;public bool mounted,drift,jump,combo,slam;}
        [UnityTest]public IEnumerator RecordCurrentSawsMotorcycleAndSlamFromRealUpdate()
        {
            const string root="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/minimapa-reforma-20261015/authorial/media-current/";
            var keyboard=InputSystem.AddDevice<Keyboard>();string bindings=lab.Input.Asset.SaveBindingOverridesAsJson();int oldRate=Time.captureFramerate;Time.captureFramerate=30;
            try
            {
                lab.Input.Action("AbyssBike").ApplyBindingOverride(0,"<Keyboard>/p");lab.Input.Action("AbyssDrift").ApplyBindingOverride(0,"<Keyboard>/u");lab.Input.Action("AbyssSaw").ApplyBindingOverride(0,"<Keyboard>/i");lab.Input.Action("Skill").ApplyBindingOverride(0,"<Keyboard>/o");
                foreach(var enemy in game.MapEnemies)enemy.enabled=false;
                for(int mode=0;mode<3;mode++)
                {
                    lab.SetPaused(true);InputSystem.QueueStateEvent(keyboard,new KeyboardState());fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                    var aim=mode==2?Vector2.down:new Vector2(-1,-1).normalized;fighter.Model.Tick(.01f,new CombatInput{aim=aim});
                    var target=game.MapEnemies[0];target.Health.SetMaxHealth(100000);target.Health.FullHeal();Vector2 point=game.Map.Spawn+aim*(mode==0?1.3f:mode==1?2.8f:1.1f);target.transform.position=point;target.GetComponent<Rigidbody2D>().position=point;Physics2D.SyncTransforms();
                    string name=mode==0?"serras-combo":mode==1?"moto-derrapagem-retorno":"moto-esmagamento";string folder=root+name+"/";Directory.CreateDirectory(folder);
                    var evidence=new DeliveryClip{name=name};var poses=new HashSet<string>();bool sentDrift=false,sentCombo=false;int queued=-1;
                    Call(game,"Close");yield return null;yield return null;
                    InputSystem.QueueStateEvent(keyboard,new KeyboardState(mode==0?Key.I:mode==1?Key.P:Key.O));yield return null;
                    int total=mode==2?72:120;
                    for(int frame=0;frame<total;frame++)
                    {
                        var m=fighter.Model;Key key=Key.None;
                        if(mode==0&&m.State==CombatState.Attack&&m.AttackSerial!=queued&&m.StateTime>=m.Attack.startup+m.Attack.active-.025f){key=Key.I;queued=m.AttackSerial;}
                        if(mode==1&&rider.Dashing&&m.ActiveThisTick&&!sentDrift){key=Key.U;sentDrift=true;}
                        else if(mode==1&&rider.Dashing&&!sentCombo&&m.StateTime>=m.Attack.startup+m.Attack.active+.09f){key=Key.I;sentCombo=true;}
                        InputSystem.QueueStateEvent(keyboard,key==Key.None?new KeyboardState():new KeyboardState(key));
                        evidence.mounted|=rider.Dashing&&m.ActiveThisTick;evidence.drift|=rider.Drifting;evidence.jump|=rider.JumpHeight>.05f;evidence.slam|=rider.Slamming;
                        evidence.combo|=sentCombo&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&m.ActiveThisTick;
                        yield return new WaitForEndOfFrame();poses.Add(fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>().sprite.name);DarklightNativeCapture.Screen(folder+"frame-"+frame.ToString("D4")+".png");evidence.frames++;
                    }
                    evidence.uniquePoses=poses.Count;evidence.damage=target.Health.MaxHealth-target.Health.CurrentHealth;File.WriteAllText(folder+"evidence.json",JsonUtility.ToJson(evidence,true));
                    Assert.That(poses.Count,Is.GreaterThanOrEqualTo(3));
                    if(mode==0)Assert.That(evidence.damage,Is.GreaterThan(0));
                    if(mode==1)Assert.That(evidence.mounted&&evidence.drift&&evidence.jump&&evidence.combo,Is.True,"Missing real motorcycle/drift/vault/combo stages");
                    if(mode==2)Assert.That(evidence.slam,Is.True);
                }
            }
            finally {lab.SetPaused(true);Time.captureFramerate=oldRate;InputSystem.RemoveDevice(keyboard);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
    }
}
