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
        static string Promo => DarklightTestOutput.GetDirectory("promocional-20261016/final");
        Keyboard promoKeys;Gamepad promoPad;int promoFrame;List<string> promoShots=new();
        void PromoInput(params Key[] keys)
        {
            var state=new GamepadState();Vector2 move=Vector2.zero;bool escape=false;
            foreach(var key in keys)
            {
                if(key==Key.I)state=state.WithButton(GamepadButton.West);
                if(key==Key.P)state=state.WithButton(GamepadButton.North);
                if(key==Key.U)state=state.WithButton(GamepadButton.DpadUp);
                if(key==Key.O)state=state.WithButton(GamepadButton.RightShoulder);
                if(key==Key.Space)state=state.WithButton(GamepadButton.East);
                if(key==Key.A)move.x-=1;if(key==Key.D)move.x+=1;if(key==Key.S)move.y-=1;if(key==Key.W)move.y+=1;
                escape|=key==Key.Escape;
            }
            state.leftStick=Vector2.ClampMagnitude(move,1);InputSystem.QueueStateEvent(promoPad,state);
            InputSystem.QueueStateEvent(promoKeys,escape?new KeyboardState(Key.Escape):new KeyboardState());
        }
        void PromoFrame()
        {
            typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Promo+"frames/frame-"+(promoFrame++).ToString("D4")+".png"});
        }
        void PromoCamera()
        {
            lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled=false;
            lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.55f,-10);
        }
        IEnumerator PromoStage(int weapon,int form)
        {
            PromoInput();lab.SetPaused(true);Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);yield return null;yield return null;
            lab.SetPaused(true);fighter.ResetModel();Equip(weapon);Move(game.Map.Spawn);game.Blood.Set(100);float range=game.Weapons.Equipped.loadout.lights[0].range;float spacing=weapon==1?Mathf.Min(1.45f,range+.22f):1.7f;var e=PracticeVictim(Vector2.left*spacing);e.enabled=true;
            fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.left});PromoCamera();Call(game,"Close");yield return null;yield return null;
        }
        IEnumerator PromoSawsOrWeapon(int weapon,int frames)
        {
            yield return PromoStage(weapon,0);float before=game.MapEnemies[0].Health.CurrentHealth;PromoInput(Key.I);
            // Begin the first shot on a real contact frame, with no menu or logo lead-in.
            if(weapon==3){for(int i=0;i<40&&!fighter.Model.ActiveThisTick;i++)yield return null;}
            int queued=-1;float minDistance=float.MaxValue;
            for(int i=0;i<frames;i++)
            {
                yield return null;var m=fighter.Model;var keys=new List<Key>();
                float distance=Vector2.Distance(body.position,game.MapEnemies[0].GetComponent<Rigidbody2D>().position);minDistance=Mathf.Min(minDistance,distance);
                float reach=game.Weapons.Equipped.loadout.lights[0].range;
                if(distance>reach+.30f)keys.Add(Key.A);
                if(m.State==CombatState.Neutral&&i%2==0||m.State==CombatState.Attack&&m.AttackConnected&&m.AttackSerial!=queued&&m.StateTime>=m.Attack.startup+m.Attack.active-.025f)
                {keys.Add(Key.I);queued=m.AttackSerial;}
                PromoInput(keys.ToArray());PromoCamera();PromoFrame();
            }
            Assert.That(game.MapEnemies[0].Health.CurrentHealth,Is.LessThan(before),"Trailer must show working weapon "+weapon);Assert.That(minDistance,Is.GreaterThan(.95f),"Stationary overlap is not acceptable for the promo");promoShots.Add(weapon+": "+frames+" actual frames with damage; min distance "+minDistance.ToString("F3"));PromoInput();
        }
        IEnumerator PromoTransformation(int form)
        {
            yield return PromoStage(0,0);for(int settle=0;settle<8;settle++)yield return null;game.Progress.preferredForm=form;game.Blood.Set(100);Assert.That(game.TryTransformPlayer(),Is.True);
            for(int i=0;i<8;i++)yield return null;
            for(int i=0;i<50;i++){yield return null;PromoCamera();PromoFrame();}
            Assert.That(game.Form,Is.EqualTo(form));Assert.That(game.Transforming,Is.False);promoShots.Add("transformation "+form+": 50 actual frames");
        }
        IEnumerator PromoSlam(int form)
        {
            yield return PromoStage(3,form);PromoInput(Key.O);bool slam=false,hit=false;float before=game.MapEnemies[0].Health.CurrentHealth;
            for(int i=0;i<60;i++)
            {yield return null;slam|=rider.Slamming;hit|=game.MapEnemies[0].Health.CurrentHealth<before;PromoInput(i>40&&i%2==0?new[]{Key.I}:new Key[0]);PromoCamera();PromoFrame();}
            Assert.That(slam&&hit,Is.True,"Motorcycle lift must be a real damaging slam");promoShots.Add("slam form "+form+": 60 actual frames with damage");
        }
        IEnumerator PromoRideChain()
        {
            yield return PromoStage(3,3);var direction=new Vector2(-1,-.65f).normalized;
            var target=game.MapEnemies[0];var enemyBody=target.GetComponent<Rigidbody2D>();enemyBody.position=game.Map.Spawn+direction*3.4f;target.transform.position=enemyBody.position;Physics2D.SyncTransforms();fighter.Model.Tick(.01f,new CombatInput{aim=direction});yield return null;PromoInput(Key.P);bool driftSent=false,slamSent=false,sawSent=false,drift=false,slam=false;
            for(int i=0;i<120;i++)
            {
                yield return null;var m=fighter.Model;var keys=new List<Key>();
                if(rider.Dashing&&!rider.Drifting&&!driftSent){keys.Add(Key.U);driftSent=true;}
                if(rider.Drifting&&m.ActiveThisTick&&!slamSent){keys.Add(Key.O);slamSent=true;}
                if(rider.Slamming&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active+.24f&&!sawSent){keys.Add(Key.I);sawSent=true;}
                PromoInput(keys.ToArray());drift|=rider.Drifting;slam|=rider.Slamming;PromoCamera();PromoFrame();
            }
            Assert.That(drift&&slam,Is.True,"Trailer ride must contain actual drift and mounted slam");promoShots.Add("white ride/drift/slam: 120 actual frames");
        }
        IEnumerator PromoSingleBossShot()
        {
            PromoInput();game.ExitTraining();game.PreviewChapter(6);game.Progress.checkpoint=3;game.Progress.weaponUnlocks=7;Call(game,"LoadMap",6);Call(game,"Close");yield return null;yield return null;
            Move(game.Map.SafeSpawn(game.Map.Arenas[3]+Vector2.left*2));
            // Enter the arena and let the real introduction callback spawn its single boss.
            for(int settle=0;settle<25;settle++){PromoInput(!game.Active?new[]{Key.Escape}:new Key[0]);yield return null;PromoInput();yield return null;}
            var boss=game.CurrentBoss;Assert.That(boss,Is.Not.Null);int bosses=0;foreach(var enemy in game.MapEnemies)if(enemy!=null&&enemy.Boss&&enemy.gameObject.activeInHierarchy)bosses++;
            Assert.That(bosses,Is.EqualTo(1),"Footage must use the actual single-boss encounter");
            Move(game.Map.SafeSpawn((Vector2)boss.transform.position+Vector2.left*1.8f));Equip(0);fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.right});
            Assert.That(game.Active,Is.True,"Boss introduction must be closed before recording");
            for(int i=0;i<120;i++)
            {
                yield return null;var press=new List<Key>();
                if(Vector2.Distance(body.position,boss.transform.position)>2)press.Add(Key.D);
                if(i==42||i==90){press.Clear();press.Add(Key.A);press.Add(Key.Space);}
                if(i%20==0)press.Add(Key.I);PromoInput(press.ToArray());PromoCamera();PromoFrame();
            }
            Assert.That(game.Active,Is.True);Assert.That(fighter.Model.State,Is.Not.EqualTo(CombatState.Dead));
            File.WriteAllText(Promo+"ending-evidence.json","{\"frames\":120,\"bosses\":1,\"realCampaignEncounter\":true,\"playerAlive\":true,\"dialogueClosed\":true}");
        }
        [UnityTest]public IEnumerator RecordSingleBossPromotionEnding()
        {
            for(int i=0;i<630;i++)Assert.That(File.Exists(Promo+"frames/frame-"+i.ToString("D4")+".png"),Is.True);
            promoFrame=630;promoKeys=InputSystem.AddDevice<Keyboard>();promoPad=InputSystem.AddDevice<Gamepad>();var bindings=lab.Input.Asset.SaveBindingOverridesAsJson();int rate=Time.captureFramerate;
            try{Time.captureFramerate=30;lab.Input.Asset.RemoveAllBindingOverrides();yield return PromoSingleBossShot();Assert.That(promoFrame,Is.EqualTo(750));}
            finally{lab.SetPaused(true);Time.captureFramerate=rate;InputSystem.RemoveDevice(promoKeys);InputSystem.RemoveDevice(promoPad);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
        [UnityTest]public IEnumerator RecordReadablePromoGameplayTwentyFiveSeconds()
        {
            Directory.CreateDirectory(Promo+"frames");promoFrame=0;promoShots.Clear();promoKeys=InputSystem.AddDevice<Keyboard>();promoPad=InputSystem.AddDevice<Gamepad>();var bindings=lab.Input.Asset.SaveBindingOverridesAsJson();int rate=Time.captureFramerate;
            try
            {
                Time.captureFramerate=30;lab.Input.Asset.RemoveAllBindingOverrides();
                yield return PromoSawsOrWeapon(3,90);yield return PromoSawsOrWeapon(0,60);yield return PromoSawsOrWeapon(1,45);yield return PromoSawsOrWeapon(2,45);
                for(int form=1;form<4;form++)yield return PromoTransformation(form);
                yield return PromoSlam(0);yield return PromoRideChain();yield return PromoSlam(2);
                yield return PromoSingleBossShot();
                promoShots.Add("Valtherion / Cripta dos Primeiros: 120 actual frames");Assert.That(promoFrame,Is.EqualTo(750));File.WriteAllLines(Promo+"capture-evidence.txt",promoShots);File.WriteAllText(Promo+"capture-result.json","{\"frames\":750,\"fps\":30,\"seconds\":25,\"realUnityGameplay\":true,\"silent\":true}");
            }
            finally{lab.SetPaused(true);Time.captureFramerate=rate;if(promoKeys!=null)InputSystem.RemoveDevice(promoKeys);if(promoPad!=null)InputSystem.RemoveDevice(promoPad);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
    }
}
