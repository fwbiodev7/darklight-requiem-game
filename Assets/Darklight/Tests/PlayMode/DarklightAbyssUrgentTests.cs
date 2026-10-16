using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed class DarklightAbyssUrgentTests
    {
        const string Root="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/cavaleiro-sprites-20261015/";
        DarklightCampaign game;DarklightCombatLab lab;DarklightFighter fighter;Rigidbody2D body;Gamepad pad;Mouse mouse;GameObject wall;
        string bindings;
        static void Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        void Move(Vector2 p){body.position=p;fighter.transform.position=p;Physics2D.SyncTransforms();}
        void Capture(string name)=>typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Root+"capturas/katana-caminhada/"+name+".png"});
        [UnitySetUp]public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;fighter=lab.Player;body=fighter.GetComponent<Rigidbody2D>();
            bindings=lab.Input.Asset.SaveBindingOverridesAsJson();pad=InputSystem.AddDevice<Gamepad>();mouse=InputSystem.AddDevice<Mouse>();
            game.EnterTraining();yield return null;yield return null;game.SelectSkin(1,false);Call(game,"SetTrainingForm",0);
            lab.SetPaused(true);fighter.ResetModel();game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);
            foreach(var e in game.MapEnemies){e.enabled=false;e.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;}
            Directory.CreateDirectory(Root+"capturas/katana-caminhada");Call(game,"Close");yield return null;yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            lab.SetPaused(true);if(wall!=null)Object.Destroy(wall);if(pad!=null)InputSystem.RemoveDevice(pad);if(mouse!=null)InputSystem.RemoveDevice(mouse);
            lab.Input.Asset.LoadBindingOverridesFromJson(bindings,true);yield return null;
        }
        IEnumerator Drive(Vector2 direction,float strength,float seconds,HashSet<int> poses=null,string clip=null,string snapshot=null)
        {
            InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=direction*strength});float until=Time.realtimeSinceStartup+seconds,next=0;int frame=0;
            var times=new List<float>();bool captured=false;
            while(Time.realtimeSinceStartup<until)
            {
                yield return null;var gait=fighter.GetComponent<DarklightAbyssLocomotion>();if(gait?.Moving==true)poses?.Add(gait.Pose);
                if(snapshot!=null&&!captured&&gait?.Moving==true&&poses?.Count>=2){Capture(snapshot);captured=true;}
                if(clip!=null&&Time.realtimeSinceStartup>=next){Capture(clip+"-"+(frame++).ToString("D3"));times.Add(Time.realtimeSinceStartup);next=Time.realtimeSinceStartup+.08f;}
            }
            if(clip!=null)File.WriteAllText(Root+"capturas/katana-caminhada/"+clip+"-times.json",JsonUtility.ToJson(new Stamps{seconds=times.ToArray()}));
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;yield return null;
        }
        [Serializable]sealed class Stamps{public float[] seconds;}
        IEnumerator ValidateForm(int form)
        {
            Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);yield return null;yield return null;
            var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            for(int direction=0;direction<8;direction++)
            {
                fighter.ResetModel();Move(game.Map.Spawn);yield return null;yield return null;
                var aim=new Vector2(-Mathf.Sin(direction*Mathf.PI/4),-Mathf.Cos(direction*Mathf.PI/4));var poses=new HashSet<int>();Vector2 start=body.position;
                yield return Drive(aim,1,.65f,poses,null,"forma"+form+"-direcao"+direction);
                var gait=fighter.GetComponent<DarklightAbyssLocomotion>();
                Assert.That(gait,Is.Not.Null);Assert.That(gait.HasCompleteForm(form),Is.True);Assert.That(poses.Count,Is.GreaterThanOrEqualTo(2),"No actual changing gait, form/direction "+form+"/"+direction);
                Assert.That(Vector2.Distance(start,body.position),Is.GreaterThan(.35f));Assert.That(gait.View,Is.EqualTo(direction>4?8-direction:direction));Assert.That(art.flipX,Is.EqualTo(direction>4));
                Assert.That(art.sprite.name,Does.Contain("CalmWalkF"+form));Assert.That(art.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(art.color,Is.EqualTo(Color.white));
            }
            fighter.ResetModel();Move(game.Map.Spawn);yield return null;yield return null;
            var walking=new HashSet<int>();yield return Drive(Vector2.down,.55f,1.4f,walking,"forma"+form+"-caminhada");Assert.That(walking.Count,Is.GreaterThanOrEqualTo(2));
            Assert.That(walking.Contains(0)||walking.Contains(2),Is.True,"Walking must use the four calm katana phases");
            Move(game.Map.Spawn);yield return null;yield return null;
            yield return Drive(Vector2.up,1,1.25f,new HashSet<int>(),"forma"+form+"-caminhada-costas");
        }
        [UnityTest]public IEnumerator CalmKatanaGaitMovesInEightDirectionsAndStopsAtWallAndPause()
        {
            yield return ValidateForm(0);Move(game.Map.Spawn);fighter.ResetModel();yield return null;
            wall=new GameObject("QA_AbyssWalkWall");wall.transform.position=body.position+Vector2.right*.7f;wall.AddComponent<BoxCollider2D>().size=new Vector2(.1f,4);Physics2D.SyncTransforms();
            yield return Drive(Vector2.right,1,.9f);var gait=fighter.GetComponent<DarklightAbyssLocomotion>();Assert.That(body.position.x,Is.LessThan(wall.transform.position.x));
            InputSystem.QueueStateEvent(pad,new GamepadState{leftStick=Vector2.right});yield return new WaitForSeconds(.2f);Assert.That(gait.Moving,Is.False,"Feet cannot cycle against a wall");
            Vector2 stopped=body.position;float travelled=gait.Travelled;lab.SetPaused(true);yield return new WaitForSecondsRealtime(.2f);
            Assert.That(Vector2.Distance(body.position,stopped),Is.LessThan(.01f));Assert.That(gait.Travelled,Is.EqualTo(travelled).Within(.001f));Assert.That(game.AbyssRider.Learned&1,Is.EqualTo(1));
        }
        [UnityTest]public IEnumerator TransformedGaitsMoveInAllEightDirectionsWithOwnSheets()
        {
            for(int form=1;form<=3;form++)yield return ValidateForm(form);
        }
        [UnityTest]public IEnumerator SimpleMouseAndGamepadCommandsKeepOriginalWeaponBindings()
        {
            foreach(string action in new[]{"AbyssSaw","AbyssBike","AbyssDrift"})lab.Input.Action(action).RemoveAllBindingOverrides();
            Assert.That(lab.Input.Action("Light").bindings[0].path,Is.EqualTo("<Mouse>/leftButton"));Assert.That(lab.Input.Action("Heavy").bindings[0].path,Is.EqualTo("<Mouse>/rightButton"));
            InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Right));yield return null;yield return null;
            Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Attack));Assert.That(fighter.Model.Attack.kind,Is.EqualTo(StrikeKind.Light));Assert.That(lab.Input.UsingGamepad,Is.False);
            InputSystem.QueueStateEvent(mouse,new MouseState());yield return null;fighter.ResetModel();
            InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));yield return null;yield return null;
            Assert.That(game.AbyssRider.Dashing,Is.True);InputSystem.QueueStateEvent(mouse,new MouseState());yield return null;fighter.ResetModel();
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.West));yield return null;yield return null;
            Assert.That(lab.Input.UsingGamepad,Is.True,"Dedicated weapon actions must track pad activity");Assert.That(lab.Input.BindingLabel("AbyssSaw"),Is.EqualTo("X"));Assert.That(fighter.Model.Attack.kind,Is.EqualTo(StrikeKind.Light));
            InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
            var driftSlot=lab.Input.Slots("AbyssDrift",false)[0];Assert.That(lab.Input.Conflicts(driftSlot,"<Keyboard>/r").Count,Is.EqualTo(0));Assert.That(lab.Input.Conflicts(driftSlot,"<Keyboard>/space").Count,Is.GreaterThan(0));
        }
        [UnityTest]public IEnumerator WeaponGuideShowsConfiguredCommandsAndPausesRealCombat()
        {
            lab.Input.Action("AbyssSaw").ApplyBindingOverride(0,"<Keyboard>/i");lab.Input.Action("AbyssBike").ApplyBindingOverride(0,"<Keyboard>/p");
            game.ShowAbyssHelp();yield return null;yield return null;
            var text=(Text)typeof(DarklightCampaign).GetField("bodyText",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            Assert.That(lab.Paused,Is.True);Assert.That(text.text,Does.Contain("I  SERRAS"));Assert.That(text.text,Does.Contain("P  MOTO"));Assert.That(text.text,Does.Contain("III"));Capture("guia-teclado-remapeado");
            var ui=text.GetComponent<RectTransform>();Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(ui.rect.height+2),"Guide text overflows its actual panel");
            Call(game,"Close");yield return null;yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.West));yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());
            game.ShowAbyssHelp();yield return null;yield return null;Assert.That(text.text,Does.Contain("X  SERRAS"));Assert.That(text.text,Does.Contain("Y  MOTO"));Capture("guia-controle");
            Call(game,"Open","Weapons");yield return null;var image=typeof(DarklightCampaign).GetField("abyssHelpImage",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game) as Image;Assert.That(image.gameObject.activeSelf,Is.False);
        }
    }
}
