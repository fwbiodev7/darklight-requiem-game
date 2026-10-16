using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    // Opt-in smoke check for the Windows executable. Inert during normal play.
    public sealed class DarklightAbyssBuildCheck:MonoBehaviour
    {
        static string output;
        [Serializable]sealed class Report{public bool success,pauseFrozen,savesPreserved,preferencesPreserved;public int forms,drifts,slams,sawReturns,moved,captures;public string exception;}
        static void Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,args);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--requiem-abyss-check");if(i<0||i+1>=args.Length)return;
            output=args[i+1];Directory.CreateDirectory(output);DarklightAutomation.Silent=true;AudioListener.volume=0;Application.runInBackground=true;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=(scene,mode)=>{if(FindFirstObjectByType<DarklightAbyssBuildCheck>()==null)new GameObject("VerificacaoCavaleiroWindows").AddComponent<DarklightAbyssBuildCheck>();};
        }
        static string Saves()=>Directory.Exists(Application.persistentDataPath)?string.Join("|",Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories).OrderBy(p=>p).Select(p=>p+":"+Convert.ToBase64String(File.ReadAllBytes(p)))):"";
        static string Prefs()
        {
            string value="";foreach(var key in new[]{"Master","Music","Effects","Ambient","UI","Enemies","Player"})value+=key+":"+PlayerPrefs.HasKey("Darklight."+key)+":"+PlayerPrefs.GetFloat("Darklight."+key)+"|";
            foreach(var key in new[]{"VisualQuality","Flashes","FrameRate","Fullscreen","Height","Lessons","Shake","Skin","Tips","VSync","Width"})value+=key+":"+PlayerPrefs.HasKey("Darklight."+key)+":"+PlayerPrefs.GetInt("Darklight."+key)+"|";
            return value+PlayerPrefs.GetString("Darklight.Bindings");
        }
        void Capture(Camera camera,string name)
        {
            var rt=camera.targetTexture;RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});var previous=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());}finally{RenderTexture.active=previous;Destroy(image);}
        }
        IEnumerator Start()
        {
            var report=new Report();string saves=Saves(),prefs=Prefs();while(!SplashScreen.isFinished)yield return null;yield return new WaitForSecondsRealtime(.4f);
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;if(game==null||lab==null){report.exception="Campaign initialization failed";Write(report);Application.Quit(2);yield break;}
            var keys=InputSystem.AddDevice<Keyboard>();lab.Input.Asset.RemoveAllBindingOverrides();lab.Input.Action("AbyssBike").ApplyBindingOverride(0,"<Keyboard>/p");lab.Input.Action("AbyssDrift").ApplyBindingOverride(0,"<Keyboard>/u");lab.Input.Action("AbyssSaw").ApplyBindingOverride(0,"<Keyboard>/i");lab.Input.Action("Skill").ApplyBindingOverride(0,"<Keyboard>/o");
            game.EnterTraining();yield return null;yield return null;game.SelectSkin(1,false);game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);Call(game,"Close");
            var fighter=lab.Player;var rider=game.AbyssRider;var body=fighter.GetComponent<Rigidbody2D>();
            for(int form=0;form<4;form++)
            {
                InputSystem.QueueStateEvent(keys,new KeyboardState());Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);fighter.ResetModel();body.position=game.Map.Spawn;fighter.transform.position=body.position;Physics2D.SyncTransforms();
                foreach(var e in game.MapEnemies){e.enabled=false;e.GetComponent<Rigidbody2D>().position=game.Map.Spawn+Vector2.right*6;e.transform.position=e.GetComponent<Rigidbody2D>().position;}
                Call(game,"Close");yield return null;yield return null;var start=body.position;InputSystem.QueueStateEvent(keys,new KeyboardState(Key.P));bool driftSent=false,slamSent=false,sawSent=false,drift=false,slam=false,saw=false,mounted=false;float timeout=Time.realtimeSinceStartup+8;
                while(Time.realtimeSinceStartup<timeout&&!saw)
                {
                    yield return null;var m=fighter.Model;var press=new List<Key>();
                    if(rider.Dashing&&!rider.Drifting&&!driftSent){press.Add(Key.U);driftSent=true;}
                    if(rider.Drifting&&m.ActiveThisTick&&!slamSent){press.Add(Key.O);slamSent=true;}
                    if(rider.Slamming&&!m.ActiveThisTick&&m.StateTime>=m.Attack.startup+m.Attack.active+.24f&&!sawSent){press.Add(Key.I);sawSent=true;}
                    InputSystem.QueueStateEvent(keys,new KeyboardState(press.ToArray()));mounted|=rider.Dashing;drift|=rider.Drifting;slam|=rider.Slamming;saw|=sawSent&&m.State==CombatState.Attack&&m.Attack.kind==StrikeKind.Light&&m.ActiveThisTick;
                }
                if(mounted)report.forms++;if(drift)report.drifts++;if(slam)report.slams++;if(saw)report.sawReturns++;if(Vector2.Distance(start,body.position)>.3f)report.moved++;Capture(lab.WorldCamera,"forma"+form+"-windows");report.captures++;
            }
            InputSystem.QueueStateEvent(keys,new KeyboardState());lab.SetPaused(true);float clock=fighter.Model.Clock;var stopped=body.position;yield return new WaitForSecondsRealtime(.2f);report.pauseFrozen=Mathf.Approximately(clock,fighter.Model.Clock)&&Vector2.Distance(stopped,body.position)<.01f;
            InputSystem.RemoveDevice(keys);report.savesPreserved=saves==Saves();report.preferencesPreserved=prefs==Prefs();report.success=report.forms==4&&report.drifts==4&&report.slams==4&&report.sawReturns==4&&report.moved==4&&report.pauseFrozen&&report.savesPreserved&&report.preferencesPreserved;Write(report);Application.Quit(report.success?0:2);
        }
        static void Write(Report report)=>File.WriteAllText(Path.Combine(output,"resultado-executavel.json"),JsonUtility.ToJson(report,true));
    }
}
