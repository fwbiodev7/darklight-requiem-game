using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightCombatCaptureTests
    {
        static string Folder => DarklightTestOutput.GetDirectory("combat-overhaul-20261015/captures");
        static void Capture(string name)
        {
            Directory.CreateDirectory(Folder);var lab=DarklightCombatLab.Instance;var canvas=lab.PixelHUD.canvas;
            var nodes=canvas.GetComponentsInChildren<Transform>(true);var layers=new int[nodes.Length];for(int i=0;i<nodes.Length;i++){layers[i]=nodes[i].gameObject.layer;nodes[i].gameObject.layer=30;}
            var previousMode=canvas.renderMode;var previousCamera=canvas.worldCamera;float plane=canvas.planeDistance;
            float previousScale=canvas.scaleFactor,previousScaler=lab.PixelHUD.scaler.scaleFactor;
            var cameraObject=new GameObject("QA_CanvasCapture");var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=5;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            var rt=new RenderTexture(1280,720,24){antiAliasing=1};rt.Create();camera.targetTexture=rt;var previous=RenderTexture.active;
            var image=new Texture2D(1280,720,TextureFormat.RGBA32,false);
            try
            {
                camera.transform.position=new Vector3(0,0,-10);canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;lab.PixelHUD.scaler.scaleFactor=2;canvas.scaleFactor=2;
                Canvas.ForceUpdateCanvases();lab.WorldCamera.Render();camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                File.WriteAllBytes(Folder+name+".png",image.EncodeToPNG());int lit=0;foreach(var pixel in image.GetPixels32())if(pixel.r+pixel.g+pixel.b>100)lit++;
                Assert.That(lit,Is.GreaterThan(10000),"Rendered UI capture was empty");
            }
            finally{canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=plane;lab.PixelHUD.scaler.scaleFactor=previousScaler;canvas.scaleFactor=previousScale;for(int i=0;i<nodes.Length;i++)if(nodes[i]!=null)nodes[i].gameObject.layer=layers[i];RenderTexture.active=previous;rt.Release();Object.Destroy(rt);Object.Destroy(image);Object.Destroy(cameraObject);}
        }
        [UnityTest]public IEnumerator MainDifficultyAndPlayableArenaRenderWithTheirExistingPixelArt()
        {
            DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;var game=DarklightCampaign.Instance;
            game.PreviewPanel("Main");yield return null;Capture("main");game.PreviewPanel("Difficulty");yield return null;Capture("difficulty");
            game.EnterTraining();yield return null;Capture("training-menu");game.PreviewPanel("TrainingEnemies");yield return null;Capture("training-enemies");
            typeof(DarklightCampaign).GetMethod("Close",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(game,null);yield return new WaitForSeconds(.5f);Capture("training-playable");game.ExitTraining();
        }
    }
}
