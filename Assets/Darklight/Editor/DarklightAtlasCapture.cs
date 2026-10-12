using System;
using System.IO;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightAtlasCapture
    {
        public static string Capture(int width,int height,string path)
        {
            var lab=DarklightCombatLab.Instance;var canvas=lab.PixelHUD.canvas;
            var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldScale=canvas.scaleFactor,oldScaler=lab.PixelHUD.scaler.scaleFactor;
            var rects=canvas.GetComponentsInChildren<RectTransform>(true);var layers=new int[rects.Length];int oldMask=lab.WorldCamera.cullingMask;
            var obj=new GameObject("CameraCapturaResolucao");var camera=obj.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=3.75f;camera.cullingMask=1<<5;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.01f;camera.farClipPlane=100;
            var target=new RenderTexture(width,height,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();camera.targetTexture=target;
            var oldTarget=RenderTexture.active;Texture2D image=null;
            try
            {
                for(int i=0;i<rects.Length;i++){layers[i]=rects[i].gameObject.layer;rects[i].gameObject.layer=5;}
                lab.WorldCamera.cullingMask=oldMask&~(1<<5);lab.WorldCamera.Render();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                float scale=Mathf.Max(1,Mathf.Floor(Mathf.Min(width/1280f,height/720f)));if(width<1280||height<720)scale=Mathf.Min(width/1280f,height/720f);
                lab.PixelHUD.scaler.scaleFactor=2*scale;canvas.scaleFactor=2*scale;Canvas.ForceUpdateCanvases();
                var hud=lab.PixelHUD.transform.Find("HUD640x360") as RectTransform;
                var map=hud.Find("Menus/MapaDaFase/Territorio") as RectTransform;
                var mini=hud.Find("Minimapa") as RectTransform;
                var corners=new Vector3[4];RectTransform r=DarklightCampaign.Instance.Atlas.IsOpen?map:mini;r.GetWorldCorners(corners);
                foreach(var p in corners){var s=camera.WorldToScreenPoint(p);if(s.x<-.1f||s.x>width+.1f||s.y<-.1f||s.y>height+.1f)throw new InvalidOperationException("Map exceeds viewport "+width+"x"+height+": "+s);}
                camera.Render();RenderTexture.active=target;image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
                return width+"x"+height+": within viewport, UI scale="+(2*scale);
            }
            finally
            {
                RenderTexture.active=oldTarget;canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.scaleFactor=oldScale;lab.PixelHUD.scaler.scaleFactor=oldScaler;lab.WorldCamera.cullingMask=oldMask;
                for(int i=0;i<rects.Length;i++)rects[i].gameObject.layer=layers[i];camera.targetTexture=null;target.Release();UnityEngine.Object.Destroy(target);if(image!=null)UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(obj);Canvas.ForceUpdateCanvases();
            }
        }
    }
}
