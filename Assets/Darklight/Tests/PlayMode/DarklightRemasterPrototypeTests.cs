using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightRemasterPrototypeTests
    {
        static string Output => DarklightTestOutput.GetDirectory("remaster-prototype-20261014");
        [Serializable] class Metrics{public string mode;public int frames,width,height;public double meanMs,p50Ms,p95Ms,fps,gpuMs;public bool gpuAvailable;}
        [Serializable] class Proof{public string status,setup,graphicsDevice,graphicsApi,cpu,measurementScope;public bool pipelineRestored;public List<Metrics> measurements=new();}
        static Type remasterType;
        static Type RemasterType => remasterType ??= Type.GetType("Darklight.Editor.RemasterPrototype, Darklight.Editor");
        static bool HasRemaster => RemasterType != null;
        static object EditorCall(string method, params object[] args)
        {
            var type = RemasterType;
            if (type == null)
                throw new InvalidOperationException("Darklight.Editor.RemasterPrototype is not available in standalone player builds.");
            var m = type.GetMethod(method, BindingFlags.Static | BindingFlags.Public);
            if (m == null)
                throw new MissingMethodException("Darklight.Editor.RemasterPrototype", method);
            return m.Invoke(null, args);
        }
        static void CaptureWorld(Camera camera,string path)
        {
            var target=camera.targetTexture;var old=RenderTexture.active;
            var image=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
            try {RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{RenderTexture.active=old;UnityEngine.Object.Destroy(image);}
        }
        static IEnumerator Sample(Camera camera,string mode,Proof proof)
        {
            var times=new List<double>();var timing=new FrameTiming[1];double gpu=0;int gpuCount=0;
            for(int i=0;i<150;i++)
            {
                FrameTimingManager.CaptureFrameTimings();yield return null;
                if(i<30)continue;times.Add(Time.unscaledDeltaTime*1000.0);
                if(FrameTimingManager.GetLatestTimings(1,timing)>0&&timing[0].gpuFrameTime>0){gpu+=timing[0].gpuFrameTime;gpuCount++;}
            }
            times.Sort();double mean=times.Average();
            proof.measurements.Add(new Metrics{mode=mode,frames=times.Count,width=camera.targetTexture.width,height=camera.targetTexture.height,meanMs=mean,p50Ms=times[times.Count/2],p95Ms=times[(int)(times.Count*.95)],fps=1000/mean,gpuAvailable=gpuCount>0,gpuMs=gpuCount>0?gpu/gpuCount:0});
        }
        [UnityTest] public IEnumerator RenderReversibleNativeLightingPrototypeAndRestoreProductionPipeline()
        {
            if (!HasRemaster)
            {
                Assert.Ignore("DarklightRemasterPrototypeTests requires Darklight.Editor.RemasterPrototype, which is only compiled inside the Unity Editor.");
                yield break;
            }
            Directory.CreateDirectory(Output);DarklightAutomation.Silent=true;AudioListener.volume=0;
            var proof=new Proof{status="RUNNING",graphicsDevice=SystemInfo.graphicsDeviceName,graphicsApi=SystemInfo.graphicsDeviceType.ToString(),cpu=SystemInfo.processorType,measurementScope="Unity Editor: paused representative world, two characters and three enemies. This is not a player-build FPS guarantee."};
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;
            game.Progress.weaponUnlocks=7;lab.SetPaused(true);game.enabled=false;lab.Player.enabled=false;
            var position=game.Map.SafeSpawn(game.Map.Arenas[0]+Vector2.down*1.5f);
            lab.Player.transform.position=position;lab.Player.GetComponent<Rigidbody2D>().position=position;Physics2D.SyncTransforms();
            var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;lab.WorldCamera.transform.position=new Vector3(position.x,position.y+1,-10);
            var view=lab.Player.GetComponent<DarklightCharacterView>();var body=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var model=new CombatModel(game.weaponDefinitions[0].loadout);model.Tick(.01f,new CombatInput{aim=Vector2.down});
            view.PresentPreview(model,Vector2.zero,0,0,0,game.weaponDefinitions[0]);
            var rock=new GameObject("RockPrototypeCompanion").AddComponent<SpriteRenderer>();rock.sprite=TodayCharacterBank.StablePose(1,0,0,0);rock.sharedMaterial=body.sharedMaterial;rock.transform.position=(Vector3)position+new Vector3(1.55f,-.8f);rock.transform.localScale=Vector3.one*DarklightCharacterView.WorldVisualScale;rock.sortingOrder=10001;
            var spawn=typeof(DarklightCampaign).GetMethod("Spawn",BindingFlags.NonPublic|BindingFlags.Instance);
            foreach(var spec in new[]{new Vector3(-2.8f,1.4f,0),new Vector3(3.5f,2.2f,1),new Vector3(.6f,3.6f,2)})spawn.Invoke(game,new object[]{(RequiemEnemyKind)(int)spec.z,position+(Vector2)spec,false});
            foreach(var enemy in UnityEngine.Object.FindObjectsByType<DarklightEnemy>(FindObjectsSortMode.None)){enemy.enabled=false;enemy.GetComponentInChildren<SpriteRenderer>().sortingOrder=10002;}
            var originalGraphics=GraphicsSettings.defaultRenderPipeline;var originalQuality=QualitySettings.renderPipeline;
            try
            {
                yield return Sample(lab.WorldCamera,"before-builtin",proof);
                for(int form=0;form<4;form++)
                {
                    view.PresentPreview(model,Vector2.zero,0,0,form,game.weaponDefinitions[0]);body.sortingOrder=10000;yield return null;yield return null;
                    CaptureWorld(lab.WorldCamera,Output+$"before-form-{form}.png");
                }
                view.PresentPreview(model,Vector2.zero,0,0,0,game.weaponDefinitions[0]);
                for(int weapon=0;weapon<3;weapon++)
                {
                    var strike=new CombatModel(game.weaponDefinitions[weapon].loadout);strike.Tick(.001f,new CombatInput{aim=Vector2.right,lightPressed=true});strike.Tick(strike.Attack.startup+.002f,new CombatInput{aim=Vector2.right});
                    view.PresentPreview(strike,Vector2.zero,0,0,0,game.weaponDefinitions[weapon]);body.sortingOrder=10000;yield return null;yield return null;
                    CaptureWorld(lab.WorldCamera,Output+$"before-attack-{weapon}.png");
                }
                view.PresentPreview(model,Vector2.zero,0,0,0,game.weaponDefinitions[0]);body.sortingOrder=10000;
                proof.setup=(string)EditorCall("Enable");yield return null;yield return null;
                yield return Sample(lab.WorldCamera,"prototype-urp2d",proof);
                for(int form=0;form<4;form++)
                {
                    view.PresentPreview(model,Vector2.zero,0,0,form,game.weaponDefinitions[0]);body.sortingOrder=10000;EditorCall("ApplyTransformation",form);EditorCall("SetWeaponLight",form);yield return null;yield return null;
                    CaptureWorld(lab.WorldCamera,Output+$"prototype-form-{form}.png");
                    Assert.That(body.sprite.name,Does.Contain("Young"));var block=new MaterialPropertyBlock();body.GetPropertyBlock(block);Assert.That(block.GetFloat("_EraseHead"),Is.Zero);
                    if(form>0)
                    {
                        Assert.That(body.sprite.name,Does.Contain("Prototype"));Assert.That(block.GetFloat("_VampireForm"),Is.Zero,"Authored transformation must not be a tint overlay");
                        for(int frame=0;frame<12;frame++){EditorCall("AuraStep",form,frame*.18f);yield return null;yield return null;CaptureWorld(lab.WorldCamera,Output+$"aura-form-{form}-{frame:00}.png");}
                    }
                }
                EditorCall("ApplyTransformation",0);EditorCall("AuraStep",0,0f);
                for(int form=1;form<4;form++)
                {
                    for(int frame=0;frame<18;frame++){EditorCall("TransitionStep",form,frame/17f);yield return null;yield return null;CaptureWorld(lab.WorldCamera,Output+$"transition-{form}-{frame:00}.png");}
                    Assert.That(body.sprite.name,Does.Contain("Prototype"));Assert.That(body.transform.Find("AuraPequenaContinua").GetComponent<MeshRenderer>().enabled,Is.True);
                }
                EditorCall("ApplyTransformation",0);EditorCall("AuraStep",0,0f);
                for(int weapon=0;weapon<3;weapon++)
                {
                    var strike=new CombatModel(game.weaponDefinitions[weapon].loadout);strike.Tick(.001f,new CombatInput{aim=Vector2.right,lightPressed=true});strike.Tick(strike.Attack.startup+.002f,new CombatInput{aim=Vector2.right});
                    view.PresentPreview(strike,Vector2.zero,0,0,0,game.weaponDefinitions[weapon]);body.sortingOrder=10000;EditorCall("AttackPreview",weapon,Vector2.right);yield return null;yield return null;
                    CaptureWorld(lab.WorldCamera,Output+$"prototype-attack-{weapon}.png");
                    for(int frame=0;frame<12;frame++){EditorCall("CombatStep",weapon,Vector2.right,(frame+1)/13f);yield return null;yield return null;CaptureWorld(lab.WorldCamera,Output+$"combat-{weapon}-{frame:00}.png");}
                }
                EditorCall("CombatStep",0,Vector2.right,1f);view.PresentPreview(model,Vector2.zero,0,0,0,game.weaponDefinitions[0]);body.sortingOrder=10000;
                EditorCall("MoveReviewLight",0f);var left=(Vector2[])EditorCall("ShadowDirections");EditorCall("MoveReviewLight",.5f);var right=(Vector2[])EditorCall("ShadowDirections");
                Assert.That(left.Length,Is.GreaterThanOrEqualTo(5));Assert.That(Vector2.Distance(left[0],right[0]),Is.GreaterThan(.5f),"Character shadow must respond to the moving light");
                for(int frame=0;frame<16;frame++){EditorCall("MoveReviewLight",frame/16f);yield return null;yield return null;CaptureWorld(lab.WorldCamera,Output+$"shadow-{frame:00}.png");}
                EditorCall("MoveReviewLight",0f);
                for(int i=0;i<18;i++){EditorCall("CinematicCameraStep",.04f,Vector2.right);yield return null;}
                CaptureWorld(lab.WorldCamera,Output+"prototype-camera.png");
                proof.status="SUCCESS";
            }
            finally
            {
                EditorCall("Restore");proof.pipelineRestored=GraphicsSettings.defaultRenderPipeline==originalGraphics&&QualitySettings.renderPipeline==originalQuality;
                File.WriteAllText(Output+"measurements.json",JsonUtility.ToJson(proof,true));UnityEngine.Object.Destroy(rock.gameObject);AudioListener.volume=0;DarklightAutomation.Silent=true;
            }
            Assert.That(proof.pipelineRestored,Is.True);
        }
    }
}
