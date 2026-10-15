using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    public sealed class DarklightRemasterSmoke:MonoBehaviour
    {
        static string output;
        public static bool Active=>!string.IsNullOrEmpty(output);
        [Serializable]sealed class Sample{public int chapter,quality,frames;public float seconds,averageFps,p95FrameMs;}
        [Serializable]sealed class Report {public bool success,savesPreserved,ltTransforms,pauseFreezes,formsAvailable,loadingScreen,violetIdleAnimated=true,minimumThreeMinutes=true;public string pipeline,exception;public int forms,captures;public float averageFps,p95FrameMs;public List<Sample> renderedSamples=new();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--requiem-remaster-check");if(i<0||i+1>=args.Length)return;
            output=args[i+1];Directory.CreateDirectory(output);DarklightAutomation.Silent=true;AudioListener.volume=0;Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=(scene,mode)=>{if(FindFirstObjectByType<DarklightRemasterSmoke>()==null)new GameObject("VerificacaoSilenciosaDoExecutavel").AddComponent<DarklightRemasterSmoke>();};
        }
        static string SaveBytes(string path)=>File.Exists(path)?Convert.ToBase64String(File.ReadAllBytes(path)):"";
        static void WorldCapture(Camera camera,string name)
        {
            var rt=camera.targetTexture;RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});var previous=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());}finally{RenderTexture.active=previous;Destroy(image);}
        }
        IEnumerator Start()
        {
            if(string.IsNullOrEmpty(output)){enabled=false;yield break;}
            var report=new Report();string before=SaveBytes(DarklightProgress.Path),backup=SaveBytes(DarklightProgress.Path+".bak");var times=new List<float>();
            while(!SplashScreen.isFinished)yield return null;yield return new WaitForSecondsRealtime(.5f);
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            if(game==null||lab==null){report.exception="Campaign did not initialize";Write(report);Application.Quit(2);yield break;}
            lab.Input.Asset.RemoveAllBindingOverrides(); // In memory only; user preferences are never written.
            var pad=InputSystem.AddDevice<Gamepad>();report.pipeline=GraphicsSettings.currentRenderPipeline?.name;
            yield return new WaitForEndOfFrame();DarklightNativeCapture.Screen(Path.Combine(output,"menu-windows.png"));report.captures++;yield return null;
            game.PreviewChapter(0);report.formsAvailable=game.Progress.FormUnlocked==3&&game.Progress.formsUnlocked==0;
            for(int skin=0;skin<2;skin++)for(int form=1;form<=3;form++)
            {
                game.SelectSkin(skin,false);lab.Player.ResetModel();lab.Player.GetComponent<HealthSystem>().FullHeal();game.Blood.Set(100);game.Progress.preferredForm=form;yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftTrigger=1});yield return null;yield return null;
                report.ltTransforms|=game.Form==form&&game.Transforming;
                float timeout=Time.realtimeSinceStartup+5;while(game.Transforming&&Time.realtimeSinceStartup<timeout)yield return null;
                if(game.Form==form&&!game.Transforming&&game.Energy<=40.1f)report.forms++;
                report.minimumThreeMinutes&=game.FormTime>=179.5f;
                if(form==2)
                {
                    var framesSeen=new HashSet<string>();var body=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();float until=Time.realtimeSinceStartup+.8f;
                    while(Time.realtimeSinceStartup<until){framesSeen.Add(body.sprite.name);yield return null;}
                    report.violetIdleAnimated&=framesSeen.Count>=2;
                }
                yield return null;WorldCapture(lab.WorldCamera,$"skin-{skin}-forma-{form}");report.captures++;
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftTrigger=1});yield return null;yield return null;while(game.Transforming)yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
            }
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Start));yield return null;yield return null;
            float clock=lab.Player.Model.Clock;yield return new WaitForSecondsRealtime(.15f);report.pauseFreezes=Mathf.Approximately(clock,lab.Player.Model.Clock);
            game.PreviewPanel("Controls");yield return null;yield return new WaitForEndOfFrame();DarklightNativeCapture.Screen(Path.Combine(output,"controles-windows.png"));report.captures++;yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());game.PreviewChapter(0);yield return null;
            typeof(DarklightCampaign).GetMethod("BeginPhaseTransition",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,new object[]{1,(Action)(()=>game.PreviewChapter(1)),(Action)(()=>{})});
            float loadingTimeout=Time.realtimeSinceStartup+10;yield return new WaitForSecondsRealtime(.2f);
            report.loadingScreen=game.PhaseLoading&&game.LoadingChapter==1&&!game.Active;
            DarklightNativeCapture.Screen(Path.Combine(output,"loading-windows.png"));report.captures++;
            while(game.PhaseLoading&&Time.realtimeSinceStartup<loadingTimeout)yield return null;
            report.loadingScreen&=!game.PhaseLoading&&game.Progress.chapter==1&&game.Active;
            // Every measured frame explicitly submits the real world render, including while the window is hidden.
            using(var frameCapture=new DarklightNativeCapture.FrameSession())
            {
            for(int chapter=0;chapter<10;chapter++)
            {
                game.PreviewChapter(chapter);yield return null;yield return null;var remaster=game.GetComponent<DarklightRemaster>();
                lab.Player.GetComponent<HealthSystem>().FullHeal();game.Blood.Set(100);game.Progress.preferredForm=3;
                InputSystem.QueueStateEvent(pad,new GamepadState{leftTrigger=1});yield return null;yield return null;InputSystem.QueueStateEvent(pad,new GamepadState());
                while(game.Transforming)yield return null;
                for(int quality=0;quality<4;quality++)
                {
                    remaster.SetQuality(quality,false);for(int i=0;i<15;i++){frameCapture.RenderFrame();yield return null;}
                    var sample=new Sample{chapter=chapter,quality=quality};times.Clear();float start=Time.realtimeSinceStartup,previous=start;
                    while(Time.realtimeSinceStartup-start<2)
                    {
                        var state=new GamepadState{leftStick=new Vector2(Mathf.Sin(sample.frames*.03f)*.3f,.1f)};
                        if(sample.frames%40<20)state=state.WithButton(GamepadButton.West);InputSystem.QueueStateEvent(pad,state);
                        frameCapture.RenderFrame();yield return null;
                        float now=Time.realtimeSinceStartup;times.Add(now-previous);previous=now;sample.frames++;
                    }
                    frameCapture.FlushGpu();times.Sort();sample.seconds=Time.realtimeSinceStartup-start;sample.averageFps=sample.frames/sample.seconds;sample.p95FrameMs=times[Mathf.Min(times.Count-1,Mathf.FloorToInt(times.Count*.95f))]*1000;report.renderedSamples.Add(sample);
                    File.WriteAllText(Path.Combine(output,"progresso.json"),JsonUtility.ToJson(sample));
                }
            }
            }
            InputSystem.QueueStateEvent(pad,new GamepadState());float elapsed=0;int frames=0;foreach(var sample in report.renderedSamples){elapsed+=sample.seconds;frames+=sample.frames;report.p95FrameMs=Mathf.Max(report.p95FrameMs,sample.p95FrameMs);}report.averageFps=frames/elapsed;
            WorldCapture(lab.WorldCamera,"capitulo-final-windows");report.captures++;
            report.savesPreserved=before==SaveBytes(DarklightProgress.Path)&&backup==SaveBytes(DarklightProgress.Path+".bak");report.success=report.forms==6&&report.formsAvailable&&report.ltTransforms&&report.pauseFreezes&&report.savesPreserved&&report.minimumThreeMinutes&&report.violetIdleAnimated&&report.loadingScreen&&GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset;
            InputSystem.RemoveDevice(pad);Write(report);Application.Quit(report.success?0:2);
        }
        static void Write(Report report)=>File.WriteAllText(Path.Combine(output,"resultado-executavel.json"),JsonUtility.ToJson(report,true));
    }
}

