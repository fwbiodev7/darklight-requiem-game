using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        DarklightCampaign game;DarklightCombatLab lab;DarklightFighter fighter;DarklightAbyssRider rider;Rigidbody2D body;
        GameObject wall;
        static void Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        [UnitySetUp]public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;fighter=lab.Player;rider=game.AbyssRider;body=fighter.GetComponent<Rigidbody2D>();
            game.EnterTraining();yield return null;yield return null;game.SelectSkin(1,false);Call(game,"SetTrainingForm",0);
            lab.SetPaused(true);Equip(3);Move(game.Map.Spawn);
        }
        [UnityTearDown]public IEnumerator Cleanup(){if(wall!=null)Object.Destroy(wall);yield return null;}
        void Equip(int weapon){fighter.ResetModel();game.Weapons.Request(weapon);game.Weapons.BeforeTick(.01f,true);Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(weapon));}
        void Move(Vector2 point){body.position=point;fighter.transform.position=point;Physics2D.SyncTransforms();}
        void BeginDash(Vector2 facing)
        {
            var input=new CombatInput{weaponSpecialPressed=true,aim=facing};rider.BeforeCombat(.01f,ref input);
            Assert.That(input.weaponSpecialPressed,Is.True);fighter.Model.Tick(.01f,input,rider.AttackClockSpeed(game.AttackSpeed));rider.AfterCombat();
            Assert.That(rider.Dashing,Is.True);var attack=fighter.Model.Attack;
            fighter.Model.Tick(attack.startup+.02f,new CombatInput{aim=facing},rider.AttackClockSpeed(game.AttackSpeed));
            Assert.That(fighter.Model.ActiveThisTick,Is.True);
        }
        void MaxGear()
        {
            var a=fighter.loadout.lights[0];for(int i=0;i<4;i++)rider.Gear.Tick(.2f,true,i,a,a.startup+a.active,true,true,true,false,0);
            Assert.That(rider.Gear.Level,Is.EqualTo(3));
        }
        [UnityTest]public IEnumerator CaptureRealNormalPrototypeSeparatedAssembledAndTransitions()
        {
            static string folder => DarklightTestOutput.GetDirectory("cavaleiro-sprites-20261015/capturas");
            var view=fighter.GetComponent<DarklightCharacterView>();var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var target=game.MapEnemies[0];target.transform.position=(Vector3)game.Map.Spawn+new Vector3(2.8f,1.8f);target.GetComponent<Rigidbody2D>().position=target.transform.position;Physics2D.SyncTransforms();
            var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;
            var southwest=new Vector2(-1,-1).normalized;
            void PresentAndCapture(string name,string expected)
            {
                view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");Call(game,"UpdateAbyssHUD");
                Assert.That(art.sprite.name,Is.EqualTo(expected));Assert.That(art.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.75f,-10);
                typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+name+".png"});
            }
            fighter.Model.Tick(.01f,new CombatInput{aim=southwest});
            PresentAndCapture("serras-separadas","Abyss_CalmWalkF0G0_1_1");
            var input=new CombatInput{weaponSpecialPressed=true,aim=Vector2.down};rider.BeforeCombat(.01f,ref input);fighter.Model.Tick(.01f,input);rider.AfterCombat();
            fighter.Model.Tick(.2f,new CombatInput{aim=Vector2.down},rider.Speed);
            PresentAndCapture("montagem-intermediaria","Abyss_Join_0_1");
            fighter.Model.Tick(.26f,new CombatInput{aim=Vector2.down},rider.Speed);
            PresentAndCapture("moto-montada","Abyss_Join_0_3");
            fighter.Model.Tick(fighter.Model.Attack.startup+fighter.Model.Attack.active+.13f-fighter.Model.StateTime,new CombatInput{aim=Vector2.down},rider.Speed);
            PresentAndCapture("desmontagem-salto","Abyss_Dismount_0_1");
            // Queue a real attack during landing; it must not interrupt the jump.
            var queued=new CombatInput{lightPressed=true,aim=southwest};rider.BeforeCombat(.01f,ref queued);fighter.Model.Tick(.01f,queued,rider.Speed);
            Assert.That(rider.Dashing,Is.True);
            for(int i=0;i<40&&rider.Dashing;i++)fighter.Model.Tick(.01f,new CombatInput{aim=southwest},rider.Speed);
            Assert.That(fighter.Model.Attack.kind,Is.EqualTo(StrikeKind.Light));
            fighter.Model.Tick(fighter.Model.Attack.startup+.01f,new CombatInput{aim=southwest},rider.Speed);
            PresentAndCapture("serras-ataque","Abyss_AttackSix1_0_2");
            follow.enabled=true;yield return null;
        }
        [UnityTest]public IEnumerator FormsKeepRiderAndRequiemSkinRestoresThePreviousWeapon()
        {
            Equip(2);Equip(3);int unlocks=game.Progress.weaponUnlocks;
            Call(game,"SetTrainingForm",3);game.Weapons.BeforeTick(0,false);
            Assert.That(game.Form,Is.EqualTo(3));Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(3));Assert.That(game.Weapons.Unlocked(3),Is.True);
            Assert.That(game.Progress.weaponUnlocks,Is.EqualTo(unlocks));
            Call(game,"SetTrainingForm",0);Equip(1);Equip(3);game.SelectSkin(0,false);
            Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(1));Assert.That(rider.Active,Is.False);
            game.SelectSkin(1,false);Equip(3);game.ExitTraining();
            Assert.That(game.Weapons.Unlocked(3),Is.False);Assert.That((int)game.Weapons.Equipped.id,Is.LessThan(3));
            yield return null;
        }
        [UnityTest]public IEnumerator SixWholeAttackFramesFollowStartupContactAndRecoveryInBothViews()
        {
            var view=fighter.GetComponent<DarklightCharacterView>();var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int direction=0;direction<2;direction++)
            {
                fighter.ResetModel();var aim=direction==0?Vector2.down:new Vector2(-1,-1).normalized;
                fighter.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=aim});var a=fighter.Model.Attack;
                float[] times={0,a.startup*.7f,a.startup+a.active*.25f,a.startup+a.active*.75f,a.startup+a.active+a.recovery*.25f,a.startup+a.active+a.recovery*.8f};
                for(int phase=0;phase<6;phase++)
                {
                    while(fighter.Model.StateTime+.0001f<times[phase])fighter.Model.Tick(Mathf.Min(.01f,times[phase]-fighter.Model.StateTime),new CombatInput{aim=aim});
                    view.Present(fighter.Model,Vector2.zero);
                    Assert.That(art.sprite.name,Is.EqualTo("Abyss_AttackSix"+direction+"_"+(phase/3)+"_"+(phase%3)));
                    Assert.That(art.sprite.pixelsPerUnit,Is.EqualTo(197));
                    if(phase<2)Assert.That(fighter.Model.ActiveThisTick,Is.False);
                    if(phase==2||phase==3)Assert.That(fighter.Model.ActiveThisTick,Is.True);
                }
            }
            yield return null;
        }
        [UnityTest]public IEnumerator AuthoredFormsRenderTheirOwnCompleteSheetsInRealUnityFrames()
        {
            static string folder => DarklightTestOutput.GetDirectory("cavaleiro-sprites-20261015/capturas");
            var view=fighter.GetComponent<DarklightCharacterView>();var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;
            var textures=new System.Collections.Generic.HashSet<Texture>();
            for(int form=0;form<4;form++)
            {
                Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);lab.SetPaused(true);
                yield return null;yield return null;
                Assert.That(DarklightAbyssSpriteBank.HasCompleteForm(form),Is.True,"Missing authored complete page: "+form);
                string prefix=form==0?"":"Form"+form;
                BeginDash(Vector2.down);view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");Call(game,"UpdateAbyssHUD");
                Assert.That(art.sprite.name,Is.EqualTo("Abyss_"+prefix+"Join_0_3"));
                Assert.That(textures.Add(art.sprite.texture),Is.True,"Transform must use independent PNG, not a global tint");
                Assert.That(art.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(art.color,Is.EqualTo(Color.white));
                lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.75f,-10);
                typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"final-form"+form+"-montada.png"});
                fighter.Model.Tick(fighter.Model.Attack.startup+fighter.Model.Attack.active+.13f-fighter.Model.StateTime,new CombatInput{aim=Vector2.down},rider.AttackClockSpeed(game.AttackSpeed));
                view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");Assert.That(art.sprite.name,Does.Contain(prefix+"Dismount_0_1"));
                typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"final-form"+form+"-salto.png"});
                fighter.ResetModel();fighter.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=new Vector2(-1,-1)});
                fighter.Model.Tick(fighter.Model.Attack.startup+.01f,new CombatInput{aim=new Vector2(-1,-1)});
                view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");Assert.That(art.sprite.name,Does.Contain(prefix+"AttackSix1_0_2"));
                typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"final-form"+form+"-serras.png"});
            }
            game.SelectSkin(0,false);Assert.That(rider.Active,Is.False);Assert.That((int)game.Weapons.Equipped.id,Is.LessThan(3));follow.enabled=true;
        }
        [UnityTest]public IEnumerator ActualMountedClockRemainsStableAcrossFormsAndGears()
        {
            for(int form=0;form<4;form++)for(int gear=1;gear<=3;gear++)
            {
                Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);lab.SetPaused(true);fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                if(gear>1){var a=fighter.loadout.lights[0];for(int i=0;i<gear*2-2;i++)rider.Gear.Tick(.2f,true,i,a,a.startup+a.active,true,true,true,false,form);}
                BeginDash(new Vector2(1,1).normalized);Assert.That(rider.AttackClockSpeed(game.AttackSpeed),Is.EqualTo(1));
                float startTime=fighter.Model.StateTime;fighter.Model.Tick(.1f,new CombatInput(),rider.AttackClockSpeed(game.AttackSpeed));
                Assert.That(fighter.Model.StateTime-startTime,Is.EqualTo(.1f).Within(.0001f));Assert.That(rider.BlocksSwap,Is.True);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator DriftCostsOnceCurvesStopsAtSceneryAndLandsIntoCombo()
        {
            game.Blood.Set(100);BeginDash(Vector2.down);float cooldown=fighter.Model.WeaponSpecialRemaining;
            var input=new CombatInput{heavyPressed=true,move=Vector2.right,aim=Vector2.down};rider.BeforeCombat(.01f,ref input);
            Assert.That(input.heavyPressed,Is.False);fighter.Model.Tick(.01f,input,rider.AttackClockSpeed(game.AttackSpeed));rider.AfterCombat();
            for(int i=0;i<40&&!rider.Drifting;i++)
            {input=new CombatInput();rider.BeforeCombat(.01f,ref input);fighter.Model.Tick(.01f,input,rider.AttackClockSpeed(game.AttackSpeed));rider.AfterCombat();}
            Assert.That(rider.Drifting,Is.True);Assert.That(game.Blood.Amount,Is.EqualTo(72));Assert.That(fighter.Model.WeaponSpecialRemaining,Is.LessThan(cooldown));
            fighter.Model.Tick(.07f,new CombatInput(),rider.AttackClockSpeed(game.AttackSpeed));
            Vector2 velocity=rider.DashVelocity(body,Vector2.down*10,.02f);Assert.That(velocity.x,Is.GreaterThan(2));Assert.That(velocity.y,Is.GreaterThan(-2));
            for(int i=0;i<50;i++)rider.AfterCombat();Assert.That(game.Blood.Amount,Is.EqualTo(72));
            wall=new GameObject("QA_DriftWall");var box=wall.AddComponent<BoxCollider2D>();box.size=new Vector2(.1f,3);wall.transform.position=body.position+Vector2.right*.9f;Physics2D.SyncTransforms();
            for(int i=0;i<8&&!rider.DashBlocked;i++){velocity=rider.DashVelocity(body,Vector2.down*100,.25f);Move(body.position+velocity*.25f);}
            Assert.That(rider.DashBlocked,Is.True);Assert.That(rider.DriftDistance,Is.LessThanOrEqualTo(DarklightAbyssRider.MaximumDriftDistance));Assert.That(body.position.x,Is.LessThan(wall.transform.position.x));
            fighter.Model.Tick(.1f,new CombatInput(),rider.AttackClockSpeed(game.AttackSpeed));
            input=new CombatInput{lightPressed=true};rider.BeforeCombat(.01f,ref input);fighter.Model.Tick(.01f,input,rider.AttackClockSpeed(game.AttackSpeed));
            for(int i=0;i<40&&rider.Dashing;i++)fighter.Model.Tick(.01f,new CombatInput(),rider.AttackClockSpeed(game.AttackSpeed));
            Assert.That(fighter.Model.Attack.kind,Is.EqualTo(StrikeKind.Light));Assert.That(game.Blood.Amount,Is.EqualTo(72));yield return null;
        }
        [UnityTest]public IEnumerator SawContactsWorkInAllEightDirectionsWithoutRepeatedDamage()
        {
            var enemy=game.MapEnemies[0];enemy.enabled=false;enemy.Health.SetMaxHealth(100000);
            for(int direction=0;direction<8;direction++)
            {
                fighter.ResetModel();Move(game.Map.Spawn);enemy.Health.FullHeal();var aim=new Vector2(Mathf.Cos(direction*Mathf.PI/4),Mathf.Sin(direction*Mathf.PI/4));
                Vector2 point=game.Map.Spawn+aim*1.1f;enemy.transform.position=point;enemy.GetComponent<Rigidbody2D>().position=point;Physics2D.SyncTransforms();
                fighter.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=aim});var a=fighter.Model.Attack;
                fighter.Model.Tick(a.startup+.01f,new CombatInput{aim=aim});float before=enemy.Health.CurrentHealth;Call(fighter,"ResolveStrike");float after=enemy.Health.CurrentHealth;
                Assert.That(after,Is.LessThan(before),"Missing direction "+direction);
                for(int i=0;i<100;i++)Call(fighter,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(after));
                Assert.That(before-after,Is.LessThan(100));
            }
            yield return null;
        }
        [UnityTest]public IEnumerator RealUpdateAndPhysicsAcceptRemappedInputForDashDriftAndCombo()
        {
            var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            string bindings=UnityEngine.InputSystem.InputActionRebindingExtensions.SaveBindingOverridesAsJson(lab.Input.Asset);
            string folder=DarklightTestOutput.GetDirectory("cavaleiro-sprites-20261015/capturas/gameplay");int captureRateBefore=Time.captureFramerate;Time.captureFramerate=30;
            try
            {
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(lab.Input.Action("AbyssBike"),0,"<Keyboard>/p");
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(lab.Input.Action("AbyssDrift"),0,"<Keyboard>/u");
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(lab.Input.Action("AbyssSaw"),0,"<Keyboard>/i");
                foreach(var enemy in game.MapEnemies)enemy.enabled=false;
                Call(game,"Close");yield return null;yield return null;game.Blood.Set(100);Move(game.Map.Spawn);fighter.ResetModel();
                var stamps=new System.Collections.Generic.List<float>();
                bool sentDrift=false,sentCombo=false,sawMounted=false,sawDrift=false,sawJump=false,sawLight=false;int frame=0;float next=0,until=Time.time+5;
                Vector2 start=body.position;UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.P));
                yield return null;yield return null;UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
                while(Time.time<until&&!sawLight)
                {
                    var m=fighter.Model;
                    if(rider.Dashing&&m.ActiveThisTick&&!rider.Drifting&&!sentDrift){sentDrift=true;UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.U));}
                    else if(rider.Dashing&&!sentCombo&&m.StateTime>=m.Attack.startup+m.Attack.active+.09f){sentCombo=true;UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.I));}
                    else UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
                    sawMounted|=rider.Dashing&&m.ActiveThisTick;sawDrift|=rider.Drifting;sawJump|=rider.JumpHeight>.05f;
                    sawLight=sentCombo&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&m.ActiveThisTick;
                    if(Time.realtimeSinceStartup>=next)
                    {
                        typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"frame-"+(frame++).ToString("D4")+".png"});stamps.Add(Time.realtimeSinceStartup);next=Time.realtimeSinceStartup+.04f;
                    }
                    yield return null;
                }
                Assert.That(sawMounted&&sawDrift&&sawJump&&sawLight,Is.True,"Real Update path did not complete mounted→drift→jump→saw contact");
                Assert.That(Vector2.Distance(start,body.position),Is.GreaterThan(.2f),"FixedUpdate must physically move the rider");
                // Training intentionally refills blood every Update. Exact one-time
                // payments are tested separately with the campaign clock paused.
                Assert.That(game.Training,Is.True);Assert.That(game.Blood.Amount,Is.InRange(72f,100f));Assert.That(frame,Is.GreaterThan(4));
                System.IO.File.WriteAllText(folder+"timestamps.json",JsonUtility.ToJson(new FrameTimes{seconds=stamps.ToArray(),count=frame}));
            }
            finally
            {
                lab.SetPaused(true);Time.captureFramerate=captureRateBefore;UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
                UnityEngine.InputSystem.InputActionRebindingExtensions.LoadBindingOverridesFromJson(lab.Input.Asset,bindings,true);
            }
        }


        [UnityTest]public IEnumerator CaptureEightRealGameplayClipsForSawCombosAndBikeForms()
        {
            var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            string bindings=UnityEngine.InputSystem.InputActionRebindingExtensions.SaveBindingOverridesAsJson(lab.Input.Asset);
            static string root => DarklightTestOutput.GetDirectory("cavaleiro-sprites-20261015/capturas/clipes");
            try
            {
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(lab.Input.Action("AbyssBike"),0,"<Keyboard>/p");
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(lab.Input.Action("AbyssDrift"),0,"<Keyboard>/u");
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(lab.Input.Action("AbyssSaw"),0,"<Keyboard>/i");
                int clips=0;
                for(int form=0;form<4;form++)for(int mode=0;mode<2;mode++)
                {
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
                    Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);yield return null;yield return null;lab.SetPaused(true);
                    fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                    Vector2 aim=form%2==0?new Vector2(-1,-1).normalized:Vector2.down;fighter.Model.Tick(.01f,new CombatInput{aim=aim});
                    var enemy=game.MapEnemies[0];enemy.enabled=false;enemy.Health.SetMaxHealth(100000);enemy.Health.FullHeal();
                    Vector2 point=mode==0?game.Map.Spawn+aim*1.1f:game.Map.Spawn+new Vector2(2.8f,1.8f);enemy.transform.position=point;var dummy=enemy.GetComponent<Rigidbody2D>();dummy.position=point;dummy.constraints=RigidbodyConstraints2D.FreezeAll;Physics2D.SyncTransforms();
                    string folder=root+"forma"+form+(mode==0?"-serras/":"-moto/");System.IO.Directory.CreateDirectory(folder);
                    var stamps=new System.Collections.Generic.List<float>();var poses=new System.Collections.Generic.HashSet<string>();
                    bool drift=false,sawBike=false,sawJump=false;int frame=0,queued=-1;float next=0,start=Time.realtimeSinceStartup;
                    Call(game,"Close");yield return null;yield return null;
                    UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(mode==0?UnityEngine.InputSystem.Key.I:UnityEngine.InputSystem.Key.P));
                    yield return null;
                    while(Time.realtimeSinceStartup-start<(mode==0?4.2f:3.1f))
                    {
                        var m=fighter.Model;var key=UnityEngine.InputSystem.Key.None;
                        if(mode==0&&m.State==CombatState.Attack&&m.AttackSerial!=queued&&m.StateTime>=m.Attack.startup+m.Attack.active-.025f)
                        {key=UnityEngine.InputSystem.Key.I;queued=m.AttackSerial;}
                        if(mode==1&&rider.Dashing&&m.ActiveThisTick&&!drift){key=UnityEngine.InputSystem.Key.U;drift=true;}
                        if(key==UnityEngine.InputSystem.Key.None)UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState());
                        else UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(key));
                        sawBike|=rider.Dashing&&m.ActiveThisTick;sawJump|=rider.JumpHeight>.05f;
                        if(Time.realtimeSinceStartup>=next)
                        {
                            poses.Add(fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>().sprite.name);
                            typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{folder+"frame-"+(frame++).ToString("D4")+".png"});stamps.Add(Time.realtimeSinceStartup);next=Time.realtimeSinceStartup+.055f;
                        }
                        yield return null;
                    }
                    lab.SetPaused(true);System.IO.File.WriteAllText(folder+"timestamps.json",JsonUtility.ToJson(new FrameTimes{seconds=stamps.ToArray(),count=frame}));
                    Assert.That(frame,Is.GreaterThan(6));Assert.That(poses.Count,Is.GreaterThanOrEqualTo(mode==0?3:4));
                    if(mode==0)Assert.That(enemy.Health.CurrentHealth,Is.LessThan(enemy.Health.MaxHealth));
                    else Assert.That(sawBike&&sawJump&&drift,Is.True,"Missing actual mounted/vault stages");clips++;
                }
                Assert.That(clips,Is.EqualTo(8));
            }
            finally
            {
                lab.SetPaused(true);UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
                UnityEngine.InputSystem.InputActionRebindingExtensions.LoadBindingOverridesFromJson(lab.Input.Asset,bindings,true);
            }
        }

        [Serializable]sealed class FrameTimes{public float[] seconds;public int count;}
        [UnityTest]public IEnumerator ResetClearsReservationAndSerialSoNewDashChargesExactlyOnce()
        {
            game.Blood.Set(100);MaxGear();BeginDash(Vector2.right);Assert.That(game.Blood.Amount,Is.EqualTo(82).Within(.001f));
            for(int i=0;i<10;i++)rider.AfterCombat();Assert.That(game.Blood.Amount,Is.EqualTo(82).Within(.001f));
            var pending=new CombatInput{weaponSpecialPressed=true};fighter.ResetModel();rider.BeforeCombat(.01f,ref pending);
            fighter.ResetModel();Assert.That(rider.Gear.Level,Is.EqualTo(1));Assert.That(rider.DashDistance,Is.Zero);Assert.That(rider.DashBlocked,Is.False);
            // A stale input accepted by the old model must not authorize the new model.
            fighter.Model.Tick(.01f,new CombatInput{weaponSpecialPressed=true,aim=Vector2.right});rider.AfterCombat();
            Assert.That(fighter.Model.ActiveThisTick,Is.False);Assert.That(game.Blood.Amount,Is.EqualTo(82).Within(.001f));
            fighter.ResetModel();BeginDash(Vector2.right);Assert.That(game.Blood.Amount,Is.EqualTo(64).Within(.001f));
            yield return null;
        }
        [UnityTest]public IEnumerator DashTimingAndDistanceStayBoundedAcrossGearsAndLargePhysicsSteps()
        {
            for(int gear=1;gear<=3;gear++)
            {
                fighter.ResetModel();Move(game.Map.Spawn);game.Blood.Set(100);
                if(gear>1){var a=fighter.loadout.lights[0];for(int i=0;i<gear*2-2;i++)rider.Gear.Tick(.2f,true,i,a,a.startup+a.active,true,true,true,false,0);}
                Assert.That(rider.Gear.Level,Is.EqualTo(gear));BeginDash(Vector2.right);Assert.That(rider.Speed,Is.EqualTo(1));
                Vector2 start=body.position;
                for(int i=0;i<3;i++){var v=rider.DashVelocity(body,Vector2.right*100,.25f);Move(body.position+v*.25f);}
                Assert.That(rider.DashDistance,Is.LessThanOrEqualTo(DarklightAbyssRider.MaximumDashDistance+.001f));
                Assert.That(Vector2.Distance(start,body.position),Is.EqualTo(DarklightAbyssRider.MaximumDashDistance).Within(.01f));
                Assert.That(fighter.Model.ActiveThisTick,Is.False);
            }
            yield return null;
        }
        [UnityTest]public IEnumerator SweptDashStopsAtWallAndLiveEnemyButIgnoresObjectiveTriggers()
        {
            var start=game.Map.Spawn;wall=new GameObject("QA_AbyssObstacle");var box=wall.AddComponent<BoxCollider2D>();box.size=new Vector2(.25f,3);
            wall.transform.position=start+Vector2.right*1.3f;Physics2D.SyncTransforms();BeginDash(Vector2.right);
            var v=rider.DashVelocity(body,Vector2.right*100,.25f);Move(body.position+v*.25f);
            Assert.That(rider.DashBlocked,Is.True);Assert.That(body.position.x,Is.LessThan(start.x+1.15f));Assert.That(fighter.Model.ActiveThisTick,Is.False);
            // Objective trigger volumes must not become solid barriers to the bike.
            box.isTrigger=true;fighter.ResetModel();Move(start);game.Blood.Set(100);Physics2D.SyncTransforms();BeginDash(Vector2.right);
            v=rider.DashVelocity(body,Vector2.right*100,.25f);Assert.That(rider.DashBlocked,Is.False);Assert.That(v.x,Is.GreaterThan(8));
            box.enabled=false;var enemy=game.MapEnemies[0];enemy.enabled=false;enemy.transform.position=start+Vector2.right*1.3f;
            enemy.GetComponent<Rigidbody2D>().position=enemy.transform.position;Physics2D.SyncTransforms();fighter.ResetModel();Move(start);BeginDash(Vector2.right);
            v=rider.DashVelocity(body,Vector2.right*100,.25f);Move(body.position+v*.25f);
            Assert.That(rider.DashBlocked,Is.True);Assert.That(body.position.x,Is.LessThan(enemy.transform.position.x));Assert.That(fighter.Model.ActiveThisTick,Is.False);
            yield return null;
        }
        [UnityTest]public IEnumerator RepeatedStrikeResolutionCannotMultiplySawContactsOrBypassDashCooldown()
        {
            var enemy=game.MapEnemies[0];enemy.enabled=false;enemy.Health.SetMaxHealth(100000);enemy.Health.FullHeal();
            var point=game.Map.Spawn+Vector2.down*1.1f;enemy.transform.position=point;enemy.GetComponent<Rigidbody2D>().position=point;Physics2D.SyncTransforms();
            fighter.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=Vector2.down});var a=fighter.Model.Attack;
            fighter.Model.Tick(a.startup+.01f,new CombatInput{aim=Vector2.down});float before=enemy.Health.CurrentHealth;Call(fighter,"ResolveStrike");
            Assert.That(enemy.Health.CurrentHealth,Is.LessThan(before));float once=enemy.Health.CurrentHealth;
            for(int i=0;i<100;i++)Call(fighter,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(once));
            fighter.Model.Tick(a.active*.6f,new CombatInput{aim=Vector2.down});Call(fighter,"ResolveStrike");once=enemy.Health.CurrentHealth;
            for(int i=0;i<100;i++)Call(fighter,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(once));
            fighter.ResetModel();game.Blood.Set(100);BeginDash(Vector2.down);Call(fighter,"ResolveStrike");once=enemy.Health.CurrentHealth;
            for(int i=0;i<100;i++)Call(fighter,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(once));
            while(fighter.Model.State==CombatState.Attack)fighter.Model.Tick(.02f,new CombatInput{aim=Vector2.down});
            Assert.That(fighter.Model.WeaponSpecialRemaining,Is.GreaterThan(0));float blood=game.Blood.Amount;
            var retry=new CombatInput{weaponSpecialPressed=true,aim=Vector2.down};rider.BeforeCombat(.01f,ref retry);
            Assert.That(retry.weaponSpecialPressed,Is.False);fighter.Model.Tick(.01f,retry);rider.AfterCombat();
            Assert.That(rider.Dashing,Is.False);Assert.That(game.Blood.Amount,Is.EqualTo(blood));
            yield return null;
        }
    }
}

