using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Darklight
{
    // Explicit, silent, disposable Player QA. No effect in ordinary gameplay.
    public sealed class DarklightHudPerformanceCapture:MonoBehaviour
    {
        static string output;
        public static bool Active=>!string.IsNullOrEmpty(output);
        [Serializable]sealed class Sample
        {
            public string scene;public int requestedWidth,requestedHeight,width,height,frames,enemies;
            public float seconds,averageFps,p95FrameMs,focusedRatio,activeGameplayRatio;public bool healthInsideScreen,mapInsideScreen,fullMapInsideScreen;
        }
        [Serializable]sealed class Result
        {
            public bool success,savesPreserved,preferencesPreserved,physicalControllerTested;
            public string gpu,cpu,pipeline,exception,physicalControllerStatus,measurementConditions;
            public int ramMB;public List<Sample> samples=new();public string[] connectedGamepads;
        }
        Result result=new();DarklightCampaign game;DarklightCombatLab lab;
        readonly Dictionary<string,byte[]> saves=new();string preferences;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--requiem-hud-performance-capture");if(i<0||i+1>=args.Length)return;
            output=args[i+1];Directory.CreateDirectory(output);DarklightAutomation.Silent=true;AudioListener.volume=0;Application.runInBackground=true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=(s,m)=>{if(FindAnyObjectByType<DarklightHudPerformanceCapture>()==null)new GameObject("VerificacaoHUDDesempenho").AddComponent<DarklightHudPerformanceCapture>();};
        }
        static object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        static string PrefFingerprint()
        {
            var s=new System.Text.StringBuilder();
            foreach(var k in new[]{"Darklight.Master","Darklight.Music","Darklight.Effects","Darklight.Ambient","Darklight.UI","Darklight.Enemies","Darklight.Player"})s.Append(k).Append(PlayerPrefs.HasKey(k)).Append(PlayerPrefs.GetFloat(k));
            foreach(var k in new[]{"Darklight.VisualQuality","Darklight.Flashes","Darklight.FrameRate","Darklight.Fullscreen","Darklight.Height","Darklight.Lessons","Darklight.Shake","Darklight.Skin","Darklight.Tips","Darklight.VSync","Darklight.Width"})s.Append(k).Append(PlayerPrefs.HasKey(k)).Append(PlayerPrefs.GetInt(k));
            return s.Append(PlayerPrefs.HasKey("Darklight.Bindings")).Append(PlayerPrefs.GetString("Darklight.Bindings")).ToString();
        }
        bool Inside(RectTransform r)
        {
            var p=new Vector3[4];r.GetWorldCorners(p);
            return p.All(v=>v.x>=-.5f&&v.y>=-.5f&&v.x<=Screen.width+.5f&&v.y<=Screen.height+.5f);
        }
        void Capture(string name)
        {
            var t=ScreenCapture.CaptureScreenshotAsTexture();
            try{if(t.width!=Screen.width||t.height!=Screen.height)throw new InvalidOperationException("Screenshot resolution mismatch");File.WriteAllBytes(Path.Combine(output,name+".png"),t.EncodeToPNG());}
            finally{Destroy(t);}
        }
        IEnumerator Start()
        {
            preferences=PrefFingerprint();if(Directory.Exists(Application.persistentDataPath))foreach(var p in Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories))saves[p]=File.ReadAllBytes(p);
            result.gpu=SystemInfo.graphicsDeviceName;result.cpu=SystemInfo.processorType;result.ramMB=SystemInfo.systemMemorySize;result.pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.GetType().Name??"BuiltIn";
            result.connectedGamepads=Gamepad.all.Select(p=>p.description.product+" / "+p.description.interfaceName).ToArray();result.physicalControllerStatus="Device inventory only; no human/physical button evaluation performed";
            Application.logMessageReceived+=OnLog;
            while(!UnityEngine.Rendering.SplashScreen.isFinished)yield return null;yield return new WaitForSecondsRealtime(.8f);
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;
            File.WriteAllText(Path.Combine(output,"ready.txt"),"Player ready; activate the actual window, then create run.request");
            while(!File.Exists(Path.Combine(output,"run.request")))yield return null;
            var root=lab.PixelHUD.transform.Find("HUD640x360");
            QualitySettings.vSyncCount=0;Application.targetFrameRate=120;game.GetComponent<DarklightRemaster>().SetQuality(3,false);
            result.measurementConditions="Standalone Windows Player, preset 3, VSync off, 120 FPS cap, real framebuffer, 5 seconds per sample after 6.3 second warmup; screenshot encoding excluded; automatic gameplay, not human evaluation";
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1600,900),new Vector2Int(1920,1080),new Vector2Int(1024,768)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.8f);
                for(int scenario=0;scenario<3;scenario++)
                {
                    if(game.Training)game.ExitTraining();
                    if(scenario<2){game.PreviewChapter(scenario==0?0:7);lab.SetPaused(true);}
                    else
                    {
                        game.EnterTraining();yield return null;yield return null;game.SelectSkin(1,false);Call(game,"SetTrainingForm",0);Call(game,"SpawnTraining",4);
                        lab.Player.ResetModel();game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);lab.SetPaused(false);
                    }
                    yield return new WaitForSecondsRealtime(6.3f);
                    var sample=new Sample{scene=scenario==0?"exploration-dark":scenario==1?"exploration-bright":"motorcycle-combat-three-AI",requestedWidth=size.x,requestedHeight=size.y,width=Screen.width,height=Screen.height,enemies=game.MapEnemies.Count};
                    sample.healthInsideScreen=Inside((RectTransform)root.Find("VidaMoldura"));var mini=(RectTransform)root.Find("Minimapa");sample.mapInsideScreen=scenario==2||Inside(mini);
                    var frameTimes=new List<float>();float start=Time.realtimeSinceStartup,previous=start,nextAttack=0;int focused=0,playing=0;
                    while(Time.realtimeSinceStartup-start<5)
                    {
                        if(scenario==2&&Time.realtimeSinceStartup>=nextAttack)
                        {
                            nextAttack=Time.realtimeSinceStartup+.85f;var fighter=lab.Player;var input=new CombatInput{lightPressed=sample.frames%3!=0,weaponSpecialPressed=sample.frames%3==0,aim=Vector2.up};
                            game.AbyssRider.BeforeCombat(.01f,ref input);fighter.Model.Tick(.01f,input,game.AbyssRider.AttackClockSpeed(game.AttackSpeed));game.AbyssRider.AfterCombat();
                        }
                        yield return new WaitForEndOfFrame();float now=Time.realtimeSinceStartup;frameTimes.Add((now-previous)*1000);previous=now;sample.frames++;if(Application.isFocused)focused++;if(game.Active&&(scenario<2||!lab.Paused))playing++;
                    }
                    sample.seconds=Time.realtimeSinceStartup-start;sample.averageFps=sample.frames/sample.seconds;frameTimes.Sort();sample.p95FrameMs=frameTimes[Mathf.Min(frameTimes.Count-1,Mathf.CeilToInt(frameTimes.Count*.95f)-1)];sample.focusedRatio=focused/(float)sample.frames;sample.activeGameplayRatio=playing/(float)sample.frames;
                    Capture(size.x+"x"+size.y+"-"+sample.scene);
                    sample.fullMapInsideScreen=true;
                    if(scenario<2&&game.Atlas.TryOpen())
                    {
                        yield return new WaitForSecondsRealtime(.25f);yield return new WaitForEndOfFrame();
                        sample.fullMapInsideScreen=Inside((RectTransform)root.Find("Menus/MapaDaFase"));Capture(size.x+"x"+size.y+"-"+sample.scene+"-map");game.Atlas.CloseMap();
                    }
                    result.samples.Add(sample);File.WriteAllText(Path.Combine(output,"progress.json"),JsonUtility.ToJson(result,true));
                }
            }
            result.savesPreserved=saves.All(p=>File.Exists(p.Key)&&File.ReadAllBytes(p.Key).SequenceEqual(p.Value));result.preferencesPreserved=preferences==PrefFingerprint();
            result.success=result.samples.Count==12&&result.samples.All(s=>s.frames>0&&s.healthInsideScreen&&s.mapInsideScreen&&s.fullMapInsideScreen&&s.activeGameplayRatio>.99f)&&result.savesPreserved&&result.preferencesPreserved&&string.IsNullOrEmpty(result.exception);
            File.WriteAllText(Path.Combine(output,"result.json"),JsonUtility.ToJson(result,true));Application.Quit(result.success?0:2);
        }
        void OnLog(string message,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)result.exception=(result.exception??"")+message+"\n";}
    }
}
