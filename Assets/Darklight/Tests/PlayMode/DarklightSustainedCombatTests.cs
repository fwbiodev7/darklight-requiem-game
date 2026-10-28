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
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed class DarklightSustainedCombatTests
    {
        [Serializable] sealed class Sample {public int skin,weapon,frames,attacks,enemies;public float medianMs,p95Ms,maxMs;}
        [Serializable] sealed class Report {public string environment="Unity Editor PlayMode, actual campaign, silent simulated input; not standalone FPS";public Sample[] samples;}
        [UnityTest] public IEnumerator SevenEquipmentModesSurviveSustainedActualCombat()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            Directory.CreateDirectory(Path.GetFullPath("work/gameplay-20261019"));
            using var storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/gameplay-20261019/isolated-combat.json"));
            Directory.CreateDirectory(Path.GetFullPath("outputs/validacao-pendencias-20261019"));
            var background=InputSystem.settings.backgroundBehavior;var editor=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var pad=InputSystem.AddDevice<Gamepad>();var samples=new List<Sample>();
            try
            {
                yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
                var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewCampaign();game.Progress.weaponUnlocks=15;
                var health=lab.Player.GetComponent<HealthSystem>();health.SetMaxHealth(20000,false);
                var spawn=typeof(DarklightCampaign).GetMethod("Spawn",BindingFlags.Instance|BindingFlags.NonPublic);
                var center=game.Map.SafeSpawn(game.Map.Spawn+Vector2.down);
                for(int i=0;i<8;i++)spawn.Invoke(game,new object[]{(RequiemEnemyKind)(i%3),game.Map.SafeSpawn(center+new Vector2(Mathf.Cos(i*.785f),Mathf.Sin(i*.785f))*1.3f),false});
                foreach(var enemy in Object.FindObjectsByType<DarklightEnemy>(FindObjectsSortMode.None))enemy.GetComponent<HealthSystem>().SetMaxHealth(20000,false);
                for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<(skin==1?4:3);weapon++)
                {
                    InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                    game.SelectSkin(skin,false);game.Progress.equippedWeapon=weapon;game.Weapons.Restore();lab.SetPaused(false);
                    Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(weapon));
                    var body=lab.Player.GetComponent<Rigidbody2D>();body.position=center;lab.Player.transform.position=center;Physics2D.SyncTransforms();
                    var times=new List<float>();float started=Time.realtimeSinceStartup;int serial=0;
                    while(Time.realtimeSinceStartup-started<6)
                    {
                        float t=Time.realtimeSinceStartup-started;var state=new GamepadState{leftStick=new Vector2(Mathf.Sin(t*1.5f),Mathf.Cos(t*1.5f))*.45f,rightStick=Vector2.down};
                        if((int)(t*6)%2==0)state=state.WithButton(GamepadButton.X);
                        if(t%2.5f<.08f)state=state.WithButton(GamepadButton.B);
                        InputSystem.QueueStateEvent(pad,state);yield return null;
                        if(t>.7f)times.Add(Time.unscaledDeltaTime*1000);serial=Math.Max(serial,lab.Player.Model.AttackSerial);
                        Assert.That(health.IsDead,Is.False);Assert.That(lab.Paused,Is.False);
                    }
                    Assert.That(serial,Is.GreaterThan(0),"Actual input must trigger attacks, weapon "+weapon);Assert.That(times.Count,Is.GreaterThan(10));
                    times.Sort();samples.Add(new Sample{skin=skin,weapon=weapon,frames=times.Count,attacks=serial,enemies=Object.FindObjectsByType<DarklightEnemy>(FindObjectsSortMode.None).Length,
                        medianMs=times[times.Count/2],p95Ms=times[Math.Min(times.Count-1,(int)(times.Count*.95f))],maxMs=times[times.Count-1]});
                    InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;lab.SetPaused(true);
                    var camera=lab.WorldCamera;camera.transform.position=new Vector3(body.position.x,body.position.y,-10);camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=camera.targetTexture;var image=new Texture2D(camera.targetTexture.width,camera.targetTexture.height,TextureFormat.RGB24,false);
                    try{image.ReadPixels(new Rect(0,0,image.width,image.height),0,0);image.Apply();File.WriteAllBytes(Path.GetFullPath("outputs/validacao-pendencias-20261019/combate-real-"+skin+"-"+weapon+".png"),image.EncodeToPNG());}
                    finally{RenderTexture.active=previous;Object.Destroy(image);}
                }
                foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
                File.WriteAllText(Path.GetFullPath("outputs/validacao-pendencias-20261019/combate-real-editor.json"),JsonUtility.ToJson(new Report{samples=samples.ToArray()},true));
            }
            finally
            {
                InputSystem.RemoveDevice(pad);InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editor;
            }
        }
    }
}
