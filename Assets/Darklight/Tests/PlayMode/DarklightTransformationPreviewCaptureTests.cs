using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.Rendering.Universal;

namespace Darklight.Tests
{
    // Opt-in evidence capture: real controllers, authored sprites and runtime FX.
    // Simulation is stepped explicitly at 12 fps while gameplay/audio remain paused.
    public sealed class DarklightTransformationPreviewCaptureTests
    {
        const int Fps=12,Size=768,Layer=30;
        static void Call(object actor,string method,params object[] args)=>actor.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(actor,args);
        readonly Dictionary<GameObject,int> layers=new();
        void Isolate(GameObject actor)
        {
            foreach(var child in actor.GetComponentsInChildren<Transform>(true))
            {
                if(!layers.ContainsKey(child.gameObject))layers.Add(child.gameObject,child.gameObject.layer);
                child.gameObject.layer=Layer;
            }
        }
        void Capture(Camera camera,RenderTexture target,Texture2D readback,string directory,int frame)
        {
            DarklightNativeCapture.Render(camera,target);
            var previous=RenderTexture.active;
            try
            {
                RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,Size,Size),0,0);readback.Apply();
                File.WriteAllBytes(Path.Combine(directory,$"frame-{frame:000}.png"),readback.EncodeToPNG());
            }
            finally{RenderTexture.active=previous;}
        }
        [UnityTest,Explicit("Produces silent runtime preview frames for user review; run this filter explicitly.")]
        public IEnumerator CaptureThreeCandidateTransitions()
        {
#if UNITY_EDITOR
            string priorOutput=UnityEditor.SessionState.GetString("Darklight.Tests.Output","");
            UnityEditor.SessionState.SetString("Darklight.Tests.Output",Path.GetFullPath("outputs/session20261114/official-transitions"));
#endif
            bool silent=DarklightAutomation.Silent,audioPause=AudioListener.pause;
            float volume=AudioListener.volume;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            Camera camera=null;RenderTexture target=null;Texture2D readback=null;GameObject lucius=null;GameObject previewLight=null;
            var output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","outputs","session20261114","official-transitions"));
            using var storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/session20261114/capture-isolated-progress.json"));
            try
            {
                yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
                yield return null;yield return null;
                var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
                game.PreviewChapter(4);yield return null;yield return null;
                lab.SetPaused(true);game.SelectSkin(0,false);game.Progress.formsUnlocked=3;game.Progress.equippedWeapon=0;game.Weapons.Restore();
                camera=new GameObject("SilentTransformationPreviewCamera").AddComponent<Camera>();
                camera.enabled=false;camera.orthographic=true;camera.orthographicSize=2.65f;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.065f,.075f,.085f,1);
                camera.cullingMask=1<<Layer;camera.allowHDR=false;camera.allowMSAA=false;
                camera.GetUniversalAdditionalCameraData().SetRenderer(0);
                previewLight=new GameObject("NeutralPreviewLight");previewLight.layer=Layer;
                var lighting=previewLight.AddComponent<Light2D>();lighting.lightType=Light2D.LightType.Global;
                lighting.color=Color.white;lighting.intensity=1;
                target=new RenderTexture(Size,Size,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();camera.targetTexture=target;
                readback=new Texture2D(Size,Size,TextureFormat.RGB24,false);
                var view=lab.Player.GetComponent<DarklightCharacterView>();
                for(int form=1;form<=2;form++)
                {
                    string directory=Path.Combine(output,form==1?"ruptura":"serafim");Directory.CreateDirectory(directory);
                    lab.Player.ResetModel();game.Progress.preferredForm=form;game.Blood.Set(100);
                    view.Present(lab.Player.Model,Vector2.zero);Isolate(lab.Player.gameObject);
                    camera.transform.position=lab.Player.transform.position+new Vector3(0,1.8f,-10);
                    int frame=0;
                    for(int i=0;i<6;i++)Capture(camera,target,readback,directory,frame++);
                    Assert.That(game.TryTransformPlayer(),Is.True);
                    int frames=Mathf.CeilToInt(DarklightCampaign.TransformationEntryDuration(form)*Fps);
                    for(int i=0;i<=frames;i++)
                    {
                        Call(game,"TickFormState",1f/Fps);view.Present(lab.Player.Model,Vector2.zero);
                        lab.Player.GetComponent<DarklightTransformationPresentation>()?.SendMessage("LateUpdate");
                        Isolate(lab.Player.gameObject);Capture(camera,target,readback,directory,frame++);
                    }
                    for(int i=0;i<12;i++)Capture(camera,target,readback,directory,frame++);
                    Call(game,"EndForm",false);
                    lab.Player.GetComponent<DarklightOriginalVampireView>()?.ClearEffects();
                }
                lucius=new GameObject("LuciusPreviewActor");lucius.AddComponent<Rigidbody2D>().gravityScale=0;
                lucius.AddComponent<CircleCollider2D>();var health=lucius.AddComponent<HealthSystem>();health.SetMaxHealth(260,false);
                var luciusView=lucius.AddComponent<DarklightLuciusView>();var enemy=lucius.AddComponent<DarklightEnemy>();
                enemy.InitializeLucius(null,luciusView);luciusView.Present(enemy.CombatModel,Vector2.zero);
                string arconte=Path.Combine(output,"lucius-arconte");Directory.CreateDirectory(arconte);Isolate(lucius);
                camera.transform.position=new Vector3(0,1.8f,-10);int luciusFrame=0;
                for(int i=0;i<6;i++)Capture(camera,target,readback,arconte,luciusFrame++);
                health.TakeDamage(health.MaxHealth*.5f);
                for(int i=0;i<=Mathf.CeilToInt(DarklightEnemy.LuciusTransformationDuration*Fps);i++)
                {
                    enemy.TickLucius(null,null,1f/Fps);
                    lucius.GetComponent<DarklightTransformationPresentation>()?.SendMessage("LateUpdate");
                    Isolate(lucius);Capture(camera,target,readback,arconte,luciusFrame++);
                }
                luciusView.Present(enemy.CombatModel,Vector2.zero);
                for(int i=0;i<12;i++)Capture(camera,target,readback,arconte,luciusFrame++);
                File.WriteAllText(Path.Combine(output,"capture.json"),"{\"fps\":12,\"width\":768,\"height\":768,\"status\":\"official-source-approved-runtime-evidence\",\"method\":\"real-runtime-controllers-manually-stepped-with-paused-gameplay\",\"audio\":\"muted\",\"physicsRootMovedForLevitation\":false}");
            }
            finally
            {
                foreach(var entry in layers)if(entry.Key!=null)entry.Key.layer=entry.Value;
                layers.Clear();if(lucius!=null)UnityEngine.Object.Destroy(lucius);
                if(previewLight!=null)UnityEngine.Object.Destroy(previewLight);
                if(camera!=null)UnityEngine.Object.Destroy(camera.gameObject);
                if(target!=null){target.Release();UnityEngine.Object.Destroy(target);}
                if(readback!=null)UnityEngine.Object.Destroy(readback);
                DarklightAutomation.Silent=silent;AudioListener.pause=audioPause;AudioListener.volume=volume;
#if UNITY_EDITOR
                UnityEditor.SessionState.SetString("Darklight.Tests.Output",priorOutput);
#endif
            }
        }
    }
}
