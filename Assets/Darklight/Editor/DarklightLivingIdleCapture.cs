using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Darklight.Editor
{
    public static class DarklightLivingIdleCapture
    {
        public static void Capture()=>CaptureKind("Idle");
        public static void CaptureMotion()=>CaptureKind("Motion");
        public static void CaptureAll(){Capture();CaptureMotion();}
        public static void CaptureSkinReview()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit-mode capture only");
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var go=new GameObject("SkinReferenceCamera");var camera=go.AddComponent<Camera>();camera.enabled=false;
            camera.orthographic=true;camera.orthographicSize=3.1f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.06f,.075f);
            var target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point};target.Create();camera.targetTexture=target;
            var renderers=new SpriteRenderer[8];var previous=RenderTexture.active;
            try
            {
                for(int skin=0;skin<2;skin++)for(int form=0;form<4;form++)
                {
                    var sprite=TodayCharacterBank.YoungIdle(skin,0,0,0);var mask=TodayCharacterBank.Mask(sprite);int owner=TodayCharacterBank.WholeFrameId(sprite);
                    if(form>0)
                    {
                        if(!DarklightTransformedBank.TryResolve(sprite,form,out var transformed,out var formedMask,out int id))throw new Exception("Missing form");
                        sprite=transformed;mask=formedMask;owner=id;
                    }
                    var renderer=new GameObject("ApprovedWholeFigure_"+skin+"_"+form).AddComponent<SpriteRenderer>();renderers[skin*4+form]=renderer;
                    renderer.transform.position=new Vector3((form-1.5f)*2.4f,skin==0?.4f:-2.3f,0);renderer.sprite=sprite;
                    renderer.sharedMaterial=new Material(Shader.Find("Darklight/PixelSprite"));renderer.sharedMaterial.SetTexture("_VampireMask",mask);
                    renderer.sharedMaterial.SetFloat("_WholeFrameId",owner);DarklightCanonicalComplexion.Apply(renderer);
                }
                camera.Render();RenderTexture.active=target;
                var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
                try{image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("outputs/implementacao-fase2-20261017/pele-original-frente-unity.png",image.EncodeToPNG());}
                finally{Object.DestroyImmediate(image);}
            }
            finally
            {
                RenderTexture.active=previous;
                foreach(var renderer in renderers)if(renderer!=null){Object.DestroyImmediate(renderer.sharedMaterial);Object.DestroyImmediate(renderer.gameObject);}
                camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(go);
            }
        }
        static void CaptureKind(string kind)
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit-mode capture only");
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            string output=Path.GetFullPath("outputs/implementacao-fase2-20261017/unity-preview");Directory.CreateDirectory(output);
            var go=new GameObject("EditOnlyCamera");var camera=go.AddComponent<Camera>();camera.enabled=false;
            camera.orthographic=true;camera.orthographicSize=6.2f;camera.transform.position=new Vector3(0,0,-10);
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.06f,.075f);
            var target=new RenderTexture(2048,1024,24){filterMode=FilterMode.Point};target.Create();camera.targetTexture=target;
            var renderers=new SpriteRenderer[32];
            for(int i=0;i<32;i++)
            {
                var renderer=new GameObject("WholeFigure_"+i).AddComponent<SpriteRenderer>();renderers[i]=renderer;
                renderer.transform.position=new Vector3((i%8-3.5f)*2.5f,3.1f-i/8*2.5f,0);
                renderer.sharedMaterial=new Material(Shader.Find("Darklight/PixelSprite"));
            }
            for(int skin=0;skin<2;skin++)for(int phase=0;phase<6;phase++)
            {
                float clock=phase==5?6.05f:phase*.52f;
                for(int form=0;form<4;form++)for(int direction=0;direction<8;direction++)
                {
                    var sprite=TodayCharacterBank.YoungIdle(skin,0,direction,clock);
                    if(kind=="Motion" && phase<5)
                    {
                        var page=TodayCharacterBank.Page(skin,0,0,direction,"Motion");
                        string name=page.spriteNames[Array.IndexOf(page.directions,direction)*page.phases+phase];
                        sprite=Resources.LoadAll<Sprite>(page.resourcePath).Single(s=>s.name==name);
                    }
                    var mask=TodayCharacterBank.Mask(sprite);int owner=TodayCharacterBank.WholeFrameId(sprite);
                    if(form>0)
                    {
                        if(!DarklightTransformedBank.TryResolve(sprite,form,out var transformed,out var formedMask,out int frame))throw new Exception("Missing form");
                        sprite=transformed;mask=formedMask;owner=frame;
                    }
                    var renderer=renderers[form*8+direction];renderer.sprite=sprite;
                    renderer.sharedMaterial.SetTexture("_VampireMask",mask);renderer.sharedMaterial.SetFloat("_WholeFrameId",owner);
                    DarklightCanonicalComplexion.Apply(renderer);
                }
                camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
                var image=new Texture2D(2048,1024,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,2048,1024),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(output,(skin==0?"requiem":"rock")+(kind=="Motion"?"-motion":"")+"-"+phase+".png"),image.EncodeToPNG());
                RenderTexture.active=previous;Object.DestroyImmediate(image);
            }
            foreach(var renderer in renderers){Object.DestroyImmediate(renderer.sharedMaterial);Object.DestroyImmediate(renderer.gameObject);}
            camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(go);
            Debug.Log("Captured "+kind+": both skins, eight directions, four forms, with the production pixel shader in EditMode.");
        }
    }
}
