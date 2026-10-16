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
        const string Film="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/trailer-20261016/";
        Keyboard filmKeys;int filmFrame;List<string> filmShots=new();
        void FilmInput(params Key[] keys)=>InputSystem.QueueStateEvent(filmKeys,new KeyboardState(keys));
        void FilmFrame()
        {
            typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Film+"frames/frame-"+(filmFrame++).ToString("D4")+".png"});
        }
        void FilmCamera()
        {
            lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled=false;
            lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.55f,-10);
        }
        IEnumerator FilmStage(int weapon,int form)
        {
            FilmInput();lab.SetPaused(true);Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);yield return null;yield return null;
            lab.SetPaused(true);fighter.ResetModel();Equip(weapon);Move(game.Map.Spawn);game.Blood.Set(100);var e=PracticeVictim(Vector2.down*.85f);e.kind=RequiemEnemyKind.Crawler;e.enabled=true;
            fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.down});FilmCamera();Call(game,"Close");yield return null;yield return null;
        }
        IEnumerator FilmSawsOrWeapon(int weapon,int frames)
        {
            yield return FilmStage(weapon,0);float before=game.MapEnemies[0].Health.CurrentHealth;FilmInput(Key.I,Key.S);
            // Begin the first shot on a real contact frame, with no menu or logo lead-in.
            if(weapon==3){for(int i=0;i<40&&!fighter.Model.ActiveThisTick;i++)yield return null;}
            int queued=-1;
            for(int i=0;i<frames;i++)
            {
                yield return null;var m=fighter.Model;var keys=new List<Key>{Key.S};
                if(weapon==3)
                {if(m.State!=CombatState.Neutral||i%2==0)keys.Add(Key.I);}
                else if(m.State==CombatState.Neutral||m.State==CombatState.Attack&&m.AttackSerial!=queued&&m.StateTime>=m.Attack.startup+m.Attack.active-.02f)
                {keys.Add(Key.I);queued=m.AttackSerial;}
                FilmInput(keys.ToArray());FilmCamera();FilmFrame();
            }
            Assert.That(game.MapEnemies[0].Health.CurrentHealth,Is.LessThan(before),"Trailer must show working weapon "+weapon);filmShots.Add(weapon+": "+frames+" actual frames with damage");FilmInput();
        }
        IEnumerator FilmTransformation(int form)
        {
            yield return FilmStage(0,0);for(int settle=0;settle<8;settle++)yield return null;game.Progress.preferredForm=form;game.Blood.Set(100);Assert.That(game.TryTransformPlayer(),Is.True);
            for(int i=0;i<8;i++)yield return null;
            for(int i=0;i<50;i++){yield return null;FilmCamera();FilmFrame();}
            Assert.That(game.Form,Is.EqualTo(form));Assert.That(game.Transforming,Is.False);filmShots.Add("transformation "+form+": 50 actual frames");
        }
        IEnumerator FilmSlam(int form)
        {
            yield return FilmStage(3,form);FilmInput(Key.O);bool slam=false,hit=false;float before=game.MapEnemies[0].Health.CurrentHealth;
            for(int i=0;i<60;i++)
            {yield return null;slam|=rider.Slamming;hit|=game.MapEnemies[0].Health.CurrentHealth<before;FilmInput(i>40&&i%2==0?new[]{Key.I}:new Key[0]);FilmCamera();FilmFrame();}
            Assert.That(slam&&hit,Is.True,"Motorcycle lift must be a real damaging slam");filmShots.Add("slam form "+form+": 60 actual frames with damage");
        }
        IEnumerator FilmRideChain()
        {
            yield return FilmStage(3,3);FilmInput(Key.P);bool driftSent=false,slamSent=false,sawSent=false,drift=false,slam=false;
            for(int i=0;i<120;i++)
            {
                yield return null;var m=fighter.Model;var keys=new List<Key>();
                if(rider.Dashing&&!rider.Drifting&&!driftSent){keys.Add(Key.U);driftSent=true;}
                if(rider.Drifting&&m.ActiveThisTick&&!slamSent){keys.Add(Key.O);slamSent=true;}
                if(rider.Slamming&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active+.24f&&!sawSent){keys.Add(Key.I);sawSent=true;}
                FilmInput(keys.ToArray());drift|=rider.Drifting;slam|=rider.Slamming;FilmCamera();FilmFrame();
            }
            Assert.That(drift&&slam,Is.True,"Trailer ride must contain actual drift and mounted slam");filmShots.Add("white ride/drift/slam: 120 actual frames");
        }
        [UnityTest]public IEnumerator RecordTwentyFiveSecondTrailerFromWorkingGameplay()
        {
            Directory.CreateDirectory(Film+"frames");filmFrame=0;filmShots.Clear();filmKeys=InputSystem.AddDevice<Keyboard>();var bindings=lab.Input.Asset.SaveBindingOverridesAsJson();int rate=Time.captureFramerate;
            try
            {
                Time.captureFramerate=30;lab.Input.Action("AbyssSaw").ApplyBindingOverride(0,"<Keyboard>/i");lab.Input.Action("AbyssBike").ApplyBindingOverride(0,"<Keyboard>/p");lab.Input.Action("AbyssDrift").ApplyBindingOverride(0,"<Keyboard>/u");lab.Input.Action("Light").ApplyBindingOverride(0,"<Keyboard>/i");lab.Input.Action("Skill").ApplyBindingOverride(0,"<Keyboard>/o");
                yield return FilmSawsOrWeapon(3,90);yield return FilmSawsOrWeapon(0,60);yield return FilmSawsOrWeapon(1,45);yield return FilmSawsOrWeapon(2,45);
                for(int form=1;form<4;form++)yield return FilmTransformation(form);
                yield return FilmSlam(0);yield return FilmRideChain();yield return FilmSlam(2);
                FilmInput();game.ExitTraining();game.PreviewChapter(6);game.Progress.checkpoint=3;game.Progress.weaponUnlocks=7;Call(game,"LoadMap",6);Call(game,"Close");yield return null;yield return null;
                if(game.CurrentBoss==null)Call(game,"Spawn",RequiemEnemyKind.Valtherion,game.Map.SafeSpawn(game.Map.Arenas[3]),false);
                var boss=game.CurrentBoss;Assert.That(boss,Is.Not.Null);Move(game.Map.SafeSpawn((Vector2)boss.transform.position+Vector2.down*1.5f));Equip(0);fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.up});Call(game,"Close");
                for(int settle=0;settle<20;settle++){FilmInput(!game.Active?new[]{Key.Escape}:new Key[0]);yield return null;FilmInput();yield return null;}
                Assert.That(game.Active,Is.True,"Boss shot must be live gameplay, not its introduction dialogue");
                for(int i=0;i<120;i++){yield return null;FilmInput(i%20==0?new[]{Key.I}:new Key[0]);FilmCamera();FilmFrame();}
                filmShots.Add("Valtherion / Cripta dos Primeiros: 120 actual frames");Assert.That(filmFrame,Is.EqualTo(750));File.WriteAllLines(Film+"capture-evidence.txt",filmShots);File.WriteAllText(Film+"capture-result.json","{\"frames\":750,\"fps\":30,\"seconds\":25,\"realUnityGameplay\":true,\"silent\":true}");
            }
            finally{lab.SetPaused(true);Time.captureFramerate=rate;if(filmKeys!=null)InputSystem.RemoveDevice(filmKeys);lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);}
        }
    }
}
