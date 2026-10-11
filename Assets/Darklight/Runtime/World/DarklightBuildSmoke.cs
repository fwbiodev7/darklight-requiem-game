using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Darklight
{
    /// <summary>Opt-in executable validation. Does nothing in normal play.</summary>
    public sealed class DarklightBuildSmoke : MonoBehaviour
    {
        [Serializable] sealed class Evidence
        { public bool passed,katanaHit,pauseFrozen,screenVisible;public string product,path;public int targets,repaints;public float targetHealth; }
        IEnumerator Start()
        {
            string[] args=Environment.GetCommandLineArgs();
            int index=Array.IndexOf(args,"--darklight-smoke");
            if(index<0 || index+1>=args.Length) { enabled=false;yield break; }
            string output=args[index+1];Directory.CreateDirectory(output);
            while (!UnityEngine.Rendering.SplashScreen.isFinished) yield return null;
            yield return new WaitForSeconds(.3f);
            var lab=GetComponent<DarklightCombatLab>();lab.SetPaused(false);
            yield return null;yield return null;
            var target=lab.Targets[0];lab.Player.GetComponent<Rigidbody2D>().position=(Vector2)target.transform.position-Vector2.right;
            Physics2D.SyncTransforms();lab.Player.Model.Tick(.01f,new CombatInput { lightPressed=true,aim=Vector2.right });
            yield return new WaitForSeconds(.4f);
            bool hit=Mathf.Approximately(target.Health.CurrentHealth,136);
            lab.SetPaused(true);float clock=lab.Player.Model.Clock;
            yield return new WaitForSeconds(.2f);
            bool frozen=Mathf.Approximately(clock,lab.Player.Model.Clock);
            lab.SetPaused(false);yield return null;yield return null;
            lab.WorldCamera.Render();var previous=RenderTexture.active;RenderTexture.active=lab.WorldCamera.targetTexture;
            var image=new Texture2D(640,360,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,640,360),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(output,"jogo-windows.png"),image.EncodeToPNG());Destroy(image);RenderTexture.active=previous;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"tela-windows.png"));
            yield return new WaitForSeconds(.25f);
            var screenImage=new Texture2D(2,2);screenImage.LoadImage(File.ReadAllBytes(Path.Combine(output,"tela-windows.png")));
            bool screenVisible=Array.Exists(screenImage.GetPixels32(),p=>p.r>24 || p.g>24 || p.b>24);Destroy(screenImage);
            File.WriteAllText(Path.Combine(output,"teste-executavel.json"),JsonUtility.ToJson(new Evidence {
                passed=hit&&frozen&&lab.Targets.Length==3&&screenVisible,katanaHit=hit,pauseFrozen=frozen,targets=lab.Targets.Length,
                screenVisible=screenVisible,repaints=lab.RepaintCount,
                targetHealth=target.Health.CurrentHealth,product=Application.productName,path=Application.persistentDataPath },true));
            Application.Quit(hit&&frozen&&screenVisible?0:2);
        }
    }
}
