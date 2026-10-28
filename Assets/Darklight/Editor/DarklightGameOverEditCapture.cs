using System;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Darklight.Editor
{
    // Actual Unity UI rendered in Edit mode. No Play, simulation, save writes or audio.
    public static class DarklightGameOverEditCapture
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        static void Set(DarklightCampaign game,string name,object value)=>typeof(DarklightCampaign).GetField(name,Flags).SetValue(game,value);
        static object Get(DarklightCampaign game,string name)=>typeof(DarklightCampaign).GetField(name,Flags).GetValue(game);
        static void Call(DarklightCampaign game,string name)=>typeof(DarklightCampaign).GetMethod(name,Flags).Invoke(game,null);
        [MenuItem("DARKLIGHT/Verificações/Game Over em edição sem áudio")]
        public static void Capture()
        {
            string output=Path.GetFullPath("outputs/game-over-20261017");Directory.CreateDirectory(output);
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            SessionState.SetString("Darklight.Tests.Output",output);
            try
            {
                if(Application.isPlaying)throw new InvalidOperationException("Edit-only capture must never start Play");
                if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)
                    throw new InvalidOperationException("Capture requires a graphics device, omit -nographics");
                EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
                var lab=Object.FindAnyObjectByType<DarklightCombatLab>();var game=lab.GetComponent<DarklightCampaign>();
                Set(game,"lab",lab);Set(game,"health",lab.Player.GetComponent<HealthSystem>());
                Call(game,"CreateUI");Set(game,"panel","Death");Call(game,"RefreshPanel");Call(game,"TickMenuPolish");
                ((CanvasGroup)Get(game,"menuFade")).alpha=1;
                var root=(RectTransform)Get(game,"menuRoot");
                if(root.sizeDelta!=new Vector2(640,360))throw new InvalidOperationException("Game Over root is not the full design canvas");
                var buttons=root.GetComponentsInChildren<RectTransform>().Where(r=>r.name.StartsWith("Botao")&&r.gameObject.activeSelf).ToArray();
                if(buttons.Length!=3)throw new InvalidOperationException("Game Over must have three active choices");
                foreach(var button in buttons)
                {
                    var pos=button.anchoredPosition;
                    if(pos.x<0 || pos.x+button.sizeDelta.x>640 || -pos.y<0 || -pos.y+button.sizeDelta.y>360)
                        throw new InvalidOperationException("Button outside the design canvas: "+button.name);
                }
                var artwork=(RectTransform)Get(game,"gameOverArtwork");
                foreach(var image in artwork.GetComponentsInChildren<Image>().Where(i=>new[]{"VesperaAprovada","MarcaOriginal","SeloOriginal","NevoaOriginal","MolduraOriginal"}.Contains(i.name)))
                    if(image.sprite==null)throw new InvalidOperationException("Missing real UI texture: "+image.name);
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1024,576)})
                    Render(lab,size,Path.Combine(output,"game-over-unity-"+size.x+"x"+size.y+".png"));
                File.WriteAllText(Path.Combine(output,"captura-unity-validacao.json"),"{\"kind\":\"Unity EditMode UI render\",\"playMode\":false,\"audio\":false,\"saveAccess\":false,\"buttons\":3,\"texturesPresent\":true,\"sizes\":[[1280,720],[1920,1080],[1024,576]]}");
                Debug.Log("Game Over native Edit-mode UI capture succeeded at three sizes; no Play or audio.");
            }
            catch(Exception error)
            {
                File.WriteAllText(Path.Combine(output,"captura-unity-erro.txt"),error.ToString());Debug.LogException(error);EditorApplication.Exit(1);
            }
            finally{SessionState.EraseString("Darklight.Tests.Output");}
        }
        static void Render(DarklightCombatLab lab,Vector2Int size,string path)
        {
            var cameraObject=new GameObject("CaptureGameOverEditOnly");var camera=cameraObject.AddComponent<Camera>();
            var target=new RenderTexture(size.x,size.y,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();
            camera.enabled=false;camera.orthographic=true;camera.orthographicSize=7.5f;camera.cullingMask=1<<5;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.01f;camera.farClipPlane=100;camera.targetTexture=target;
            var canvas=lab.PixelHUD.canvas;
            foreach(var rect in canvas.GetComponentsInChildren<RectTransform>(true))rect.gameObject.layer=5;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            lab.PixelHUD.scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            lab.PixelHUD.scaler.scaleFactor=size.x/640f;canvas.scaleFactor=size.x/640f;
            Canvas.ForceUpdateCanvases();camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;
            var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
            File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=previous;
            camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(image);Object.DestroyImmediate(cameraObject);
        }
    }
}
