using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    // Explicit opt-in Windows validation; never installed during ordinary play.
    public sealed class DarklightMapBuildCheck:MonoBehaviour
    {
        static string output;
        [Serializable]sealed class Region{public int map,anchors,paths,frames;public float meanFrameMs,p95FrameMs,maxFrameMs;public bool revision2,captured;}
        [Serializable]sealed class Report{public bool success,savesPreserved;public int width,height,targetFrameRate,vSync,quality;public string unity,device,graphics,limitation;public List<Region> regions=new();}
        static void Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"--requiem-map-check");if(i<0||i+1>=args.Length)return;
            output=args[i+1];Directory.CreateDirectory(output);DarklightAutomation.Silent=true;AudioListener.volume=0;Application.runInBackground=true;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded+=(s,m)=>{if(FindFirstObjectByType<DarklightMapBuildCheck>()==null)new GameObject("VerificacaoMapasWindows").AddComponent<DarklightMapBuildCheck>();};
        }
        static string Saves()=>Directory.Exists(Application.persistentDataPath)?string.Join("|",Directory.GetFiles(Application.persistentDataPath,"*.json*",SearchOption.AllDirectories).OrderBy(p=>p).Select(p=>p+":"+Convert.ToBase64String(File.ReadAllBytes(p)))):"";
        IEnumerator Start()
        {
            string saves=Saves();while(!SplashScreen.isFinished)yield return null;yield return new WaitForSecondsRealtime(.4f);
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            var report=new Report{width=Screen.width,height=Screen.height,targetFrameRate=Application.targetFrameRate,vSync=QualitySettings.vSyncCount,quality=DarklightRemaster.QualityLevel,unity=Application.unityVersion,device=SystemInfo.processorType,graphics=SystemInfo.graphicsDeviceName,limitation="Short automated rendered Windows samples in a hidden window, with other desktop processes active; not human gameplay or a sustained FPS guarantee."};
            if(game==null||lab==null){Write(report);Application.Quit(2);yield break;}
            var body=lab.Player.GetComponent<Rigidbody2D>();var path=new List<Vector2>();
            for(int chapter=6;chapter<10;chapter++)
            {
                game.PreviewChapter(chapter);Call(game,"Close");yield return null;yield return null;
                var region=new Region{map=chapter+1,revision2=game.Map.Art.revision>=2};report.regions.Add(region);
                var anchors=new List<Vector2>(game.Map.Arenas){game.Map.Ally,game.Map.Relic,game.Map.Portal};anchors.AddRange(game.Map.Checkpoints);region.anchors=anchors.Count;
                foreach(var target in anchors)if(DarklightNavigation.Instance.FindPath(game.Map.SafeSpawn(game.Map.Spawn),game.Map.SafeSpawn(target),path))region.paths++;
                Vector2 center=game.Map.SafeSpawn(game.Map.Arenas[0]);body.position=center;lab.Player.transform.position=center;Physics2D.SyncTransforms();
                // Keep actors alive to sample real enemy updates/rendering without writing death saves.
                float until=Time.realtimeSinceStartup+1;while(Time.realtimeSinceStartup<until){lab.Player.GetComponent<HealthSystem>().FullHeal();yield return null;}
                var samples=new List<float>();until=Time.realtimeSinceStartup+6;
                while(Time.realtimeSinceStartup<until){lab.Player.GetComponent<HealthSystem>().FullHeal();samples.Add(Time.unscaledDeltaTime*1000);yield return null;}
                samples.Sort();region.frames=samples.Count;if(samples.Count>0){region.meanFrameMs=samples.Average();region.p95FrameMs=samples[Mathf.Min(samples.Count-1,Mathf.FloorToInt(samples.Count*.95f))];region.maxFrameMs=samples[^1];}
                var camera=lab.WorldCamera;var rt=camera.targetTexture;RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=rt});var old=RenderTexture.active;var image=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
                try{RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,"map"+(chapter+1)+"-windows.png"),image.EncodeToPNG());region.captured=image.GetPixels32().Count(p=>p.r+p.g+p.b>100)>1000;}
                finally{RenderTexture.active=old;Destroy(image);}
            }
            lab.SetPaused(true);report.savesPreserved=saves==Saves();report.success=report.regions.Count==4&&report.regions.All(r=>r.revision2&&r.paths==r.anchors&&r.frames>5&&r.captured)&&report.savesPreserved;Write(report);Application.Quit(report.success?0:2);
        }
        static void Write(Report r)=>File.WriteAllText(Path.Combine(output,"resultado-mapas-windows.json"),JsonUtility.ToJson(r,true));
    }
}
