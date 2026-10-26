using System;
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
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        static DarklightCombatImpacts ImpactPool=>UnityEngine.Object.FindAnyObjectByType<DarklightCombatImpacts>();
        static int Emitted=>ImpactPool==null?0:(int)typeof(DarklightCombatImpacts).GetField("serial",Hidden).GetValue(ImpactPool);
        [UnityTest]public IEnumerator MotorcycleGroundImpactStartsAtContactAndDoesNotRepeatOrAddDamage()
        {
            var enemy=PracticeVictim(Vector2.down*1.1f);game.Blood.Set(100);int initial=Emitted;
            StepExpanded(.01f,new CombatInput{skillPressed=true,aim=Vector2.down});
            StepExpanded(.5f,new CombatInput{aim=Vector2.down});Assert.That(Emitted,Is.EqualTo(initial));
            StepExpanded(.2f,new CombatInput{aim=Vector2.down});Assert.That(Emitted,Is.EqualTo(initial+1));
            float health=enemy.Health.CurrentHealth,blood=game.Blood.Amount;
            for(int i=0;i<100;i++)rider.AfterCombat();
            Assert.That(Emitted,Is.EqualTo(initial+1));Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(health),"Presentation cannot apply damage");
            Assert.That(game.Blood.Amount,Is.EqualTo(blood));Assert.That(blood,Is.EqualTo(78).Within(.001f));yield return null;
        }
        [UnityTest]public IEnumerator SweptMotorcycleCollisionEmitsOnlyOneImpactAtTheObstacle()
        {
            foreach(var enemy in game.MapEnemies){enemy.transform.position=(Vector3)game.Map.Spawn+Vector3.right*5;enemy.GetComponent<Rigidbody2D>().position=enemy.transform.position;}
            wall=new GameObject("QA_ImpactWall");wall.transform.position=body.position+Vector2.down*1.4f;var obstacle=wall.AddComponent<BoxCollider2D>();obstacle.size=new Vector2(3,.1f);Physics2D.SyncTransforms();
            BeginDash(Vector2.down);int initial=Emitted;Vector2 velocity=rider.DashVelocity(body,Vector2.down*100,.3f);
            Assert.That(rider.DashBlocked,Is.True);Assert.That(Emitted,Is.EqualTo(initial+1));
            Assert.That(fighter.Model.ActiveThisTick,Is.False,"The crash ends the active sweep");
            for(int i=0;i<100;i++)rider.DashVelocity(body,Vector2.down*100,.3f);
            Assert.That(Emitted,Is.EqualTo(initial+1));Move(body.position+velocity*.3f);
            Assert.That(fighter.GetComponent<Collider2D>().Distance(obstacle).isOverlapped,Is.False);yield return null;
        }
        [UnityTest]public IEnumerator MotorcycleEffectsKeepFixedCapacityPauseAndHonorReducedFlashesAndShake()
        {
            bool oldReduced=game.ReducedFlashes;bool hadShake=PlayerPrefs.HasKey("Darklight.Shake");int oldShake=PlayerPrefs.GetInt("Darklight.Shake",1);
            try
            {
                game.ReducedFlashes=true;PlayerPrefs.SetInt("Darklight.Shake",0);
                var camera=lab.WorldCamera.GetComponent<DarklightLabCamera>();typeof(DarklightLabCamera).GetField("impulseTime",Hidden).SetValue(camera,0f);
                for(int i=0;i<100;i++)DarklightCombatFX.MotorcycleImpact(body.position,Vector2.down,DarklightMotorcycleImpact.Ground);
                Assert.That(typeof(DarklightLabCamera).GetField("impulseTime",Hidden).GetValue(camera),Is.EqualTo(0f));
                var pool=ImpactPool;var field=typeof(DarklightCombatImpacts).GetField("hits",Hidden);var contacts=(Array)field.GetValue(pool);
                Assert.That(contacts.Length,Is.EqualTo(16));
                object hit=contacts.GetValue(0);float age=(float)hit.GetType().GetField("age").GetValue(hit);
                yield return null;Call(pool,"LateUpdate");
                object after=((Array)field.GetValue(pool)).GetValue(0);
                Assert.That((float)after.GetType().GetField("age").GetValue(after),Is.EqualTo(age));
                // Exercise the same draw path for all quality levels without changing preferences.
                var draw=typeof(DarklightCombatImpacts).GetMethod("Motorcycle",Hidden);
                for(int q=0;q<4;q++)
                {
                    typeof(DarklightCombatImpacts).GetField("quads",Hidden).SetValue(pool,0);
                    draw.Invoke(pool,new object[]{hit,true,q});int count=(int)typeof(DarklightCombatImpacts).GetField("quads",Hidden).GetValue(pool);
                    Assert.That(count,Is.GreaterThan(0).And.LessThan(100));
                    var colors=(Color[])typeof(DarklightCombatImpacts).GetField("colors",Hidden).GetValue(pool);
                    for(int i=0;i<count*4;i++)Assert.That(colors[i].a,Is.LessThanOrEqualTo(.38f));
                }
            }
            finally{game.ReducedFlashes=oldReduced;if(hadShake)PlayerPrefs.SetInt("Darklight.Shake",oldShake);else PlayerPrefs.DeleteKey("Darklight.Shake");}
        }
        [UnityTest]public IEnumerator CaptureMotorcycleImpactInRealGameplayAcrossFourForms()
        {
            static string root => DarklightTestOutput.GetDirectory("cavaleiro-final-20261016/impact-captures");
            var keys=InputSystem.AddDevice<Keyboard>();string bindings=lab.Input.Asset.SaveBindingOverridesAsJson();int oldRate=Time.captureFramerate;
            try
            {
                lab.Input.Action("Skill").ApplyBindingOverride(0,"<Keyboard>/o");Time.captureFramerate=30;
                for(int form=0;form<4;form++)
                {
                    InputSystem.QueueStateEvent(keys,new KeyboardState());lab.SetPaused(true);fighter.ResetModel();rider.ResetMotion();Move(game.Map.Spawn);game.Blood.Set(100);
                    Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);var enemy=PracticeVictim(Vector2.down*1.1f);enemy.enabled=false;
                    fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.down});var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;
                    lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.75f,-10);
                    Call(game,"Close");yield return null;yield return null;
                    int initial=Emitted;bool contact=false;string folder=root+"form"+form+"/";Directory.CreateDirectory(folder);
                    InputSystem.QueueStateEvent(keys,new KeyboardState(Key.O));
                    for(int frame=0;frame<66;frame++)
                    {
                        yield return null;InputSystem.QueueStateEvent(keys,new KeyboardState());
                        bool first=!contact&&Emitted>initial;contact|=Emitted>initial;
                        if(form==0||first||frame==25)DarklightNativeCapture.Screen(folder+"frame-"+frame.ToString("D4")+".png");
                        if(frame==25&&ImpactPool!=null)
                        {
                            var renderer=ImpactPool.GetComponent<MeshRenderer>();var mesh=ImpactPool.GetComponent<MeshFilter>().sharedMesh;
                            if(form==0)
                            {
                                renderer.enabled=false;DarklightNativeCapture.Screen(folder+"effect-off.png");
                                renderer.enabled=true;DarklightNativeCapture.Screen(folder+"effect-on.png");
                            }
                            File.WriteAllText(folder+"render-state.json",JsonUtility.ToJson(new MotorcycleRenderState{quads=(int)typeof(DarklightCombatImpacts).GetField("quads",Hidden).GetValue(ImpactPool),vertices=mesh.vertexCount,enabled=renderer.enabled,position=renderer.bounds.center,size=renderer.bounds.size,scale=ImpactPool.transform.lossyScale,screen=lab.WorldCamera.WorldToScreenPoint(renderer.bounds.center),reduced=game.ReducedFlashes,delta=lab.CombatDelta},true));
                        }
                    }
                    lab.SetPaused(true);follow.enabled=true;
                    File.WriteAllText(folder+"evidence.json","{\"form\":"+form+",\"contactEffect\":"+contact.ToString().ToLowerInvariant()+",\"damage\":"+(enemy.Health.CurrentHealth<enemy.Health.MaxHealth).ToString().ToLowerInvariant()+",\"frames\":66,\"fps\":30}");
                    Assert.That(contact,Is.True);Assert.That(enemy.Health.CurrentHealth,Is.LessThan(enemy.Health.MaxHealth));Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(3));
                }
            }
            finally{lab.SetPaused(true);Time.captureFramerate=oldRate;InputSystem.RemoveDevice(keys);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
        [Serializable]sealed class MotorcycleRenderState{public int quads,vertices;public bool enabled,reduced;public Vector3 position,size,scale,screen;public float delta;}
    }
}
