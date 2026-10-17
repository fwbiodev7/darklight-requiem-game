using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    // Opt-in evidence capture and explicit world+HUD rendering without a visible swap chain.
    public static class DarklightNativeCapture
    {
        public static void Render(Camera camera,RenderTexture target)
        {
            if(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset)
                RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
            else camera.Render();
        }
        public sealed class FrameSession:System.IDisposable
        {
            readonly DarklightCombatLab lab;readonly Canvas canvas;readonly RenderMode mode;readonly Camera oldCamera,camera;
            readonly float plane,scale,scaler;readonly int mask;readonly GameObject obj;readonly Texture2D probe;
            readonly Dictionary<GameObject,int> layers=new();readonly List<RectTransform> rects=new();bool disposed;
            public RenderTexture Target{get;}
            public FrameSession()
            {
                lab=DarklightCombatLab.Instance;canvas=lab.PixelHUD.canvas;mode=canvas.renderMode;oldCamera=canvas.worldCamera;plane=canvas.planeDistance;scale=canvas.scaleFactor;scaler=lab.PixelHUD.scaler.scaleFactor;mask=lab.WorldCamera.cullingMask;
                obj=new GameObject("CameraDeCaptura");camera=obj.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=3.75f;camera.cullingMask=1<<5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;camera.nearClipPlane=.01f;camera.farClipPlane=100;camera.allowHDR=false;camera.allowMSAA=false;
                if(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset){var data=camera.GetUniversalAdditionalCameraData();data.SetRenderer(0);data.renderPostProcessing=false;data.antialiasing=AntialiasingMode.None;}
                Target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point,antiAliasing=1};Target.Create();camera.targetTexture=Target;probe=new Texture2D(1,1,TextureFormat.RGB24,false);
                lab.WorldCamera.cullingMask=mask&~(1<<5);canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;lab.PixelHUD.scaler.scaleFactor=2;canvas.scaleFactor=2;
            }
            public void RenderFrame()
            {
                if(disposed)throw new System.ObjectDisposedException(nameof(FrameSession));
                canvas.GetComponentsInChildren(true,rects);
                foreach(var rect in rects){var go=rect.gameObject;if(!layers.ContainsKey(go))layers.Add(go,go.layer);go.layer=5;}
                Render(lab.WorldCamera,lab.WorldCamera.targetTexture);Canvas.ForceUpdateCanvases();Render(camera,Target);
            }
            public void FlushGpu()
            {
                var previous=RenderTexture.active;
                try{RenderTexture.active=Target;probe.ReadPixels(new Rect(0,0,1,1),0,0);probe.Apply();}
                finally{RenderTexture.active=previous;}
            }
            public void Dispose()
            {
                if(disposed)return;disposed=true;
                if(canvas!=null){canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=plane;canvas.scaleFactor=scale;}
                if(lab!=null){lab.PixelHUD.scaler.scaleFactor=scaler;lab.WorldCamera.cullingMask=mask;}
                foreach(var pair in layers)if(pair.Key!=null)pair.Key.layer=pair.Value;
                camera.targetTexture=null;Target.Release();Object.Destroy(Target);Object.Destroy(probe);Object.Destroy(obj);Canvas.ForceUpdateCanvases();
            }
        }
        public static void Screen(string path)
        {
            var active=RenderTexture.active;Texture2D image=null;
            try
            {
                using(var session=new FrameSession())
                {
                    session.RenderFrame();RenderTexture.active=session.Target;image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=active;
                }
            }
            finally{RenderTexture.active=active;if(image!=null)Object.Destroy(image);}
        }
    }
}
