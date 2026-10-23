using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed class DarklightLitPaletteTests
    {
        [UnityTest] public IEnumerator ActualCampaignRendererCorrectsOnlyWarmSkin()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var lab=DarklightCombatLab.Instance;DarklightCampaign.Instance.PreviewCampaign();lab.SetPaused(true);
            var actual=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            Assert.That(actual.sharedMaterial.shader.name,Is.EqualTo("Darklight/Remaster/SpriteLit"));
            var pixels=new[]{new Color32(250,140,97,255),new Color32(220,220,220,255),new Color32(110,75,53,255),new Color32(250,30,45,255),new Color32(160,30,225,255)};
            var image=new Texture2D(5,1,TextureFormat.RGBA32,false,false){filterMode=FilterMode.Point};image.SetPixels32(pixels);image.Apply();
            var sprite=Sprite.Create(image,new Rect(0,0,5,1),new Vector2(.5f,.5f),1);var probe=new GameObject("LitPaletteProbe");
            var renderer=probe.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=actual.sharedMaterial;renderer.sortingOrder=32000;
            var camera=lab.WorldCamera;camera.cullingMask=~0;camera.transform.position=new Vector3(0,0,-10);camera.orthographicSize=3.6f;
            var target=new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);target.Create();camera.targetTexture=target;
            var readback=new Texture2D(1280,720,TextureFormat.RGBA32,false,false);var previous=RenderTexture.active;
            var type=System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            var data=camera.GetComponent(type);type.GetProperty("renderPostProcessing").SetValue(data,false);
            var lightType=System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.2D.Runtime");
            foreach(var light in Object.FindObjectsByType(lightType,FindObjectsSortMode.None))((Behaviour)light).enabled=false;
            var lamp=new GameObject("FlatWhitePaletteLight");var component=lamp.AddComponent(lightType);
            lightType.GetProperty("lightType").SetValue(component,System.Enum.Parse(lightType.GetProperty("lightType").PropertyType,"Global"));
            lightType.GetProperty("intensity").SetValue(component,1f);
            try
            {
                var block=new MaterialPropertyBlock();block.SetVector("_CanonicalSkinShift",new Vector4(-4/255f,25/255f,17/255f,1));renderer.SetPropertyBlock(block);
                yield return null;yield return null;camera.cullingMask=~0;camera.orthographicSize=3.6f;camera.aspect=1280f/720;camera.transform.position=new Vector3(0,0,-10);
                camera.Render();RenderTexture.active=target;
                readback.ReadPixels(new Rect(0,0,1280,720),0,0);readback.Apply();
                System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath("work/pendencias-20261017/lit-palette-probe.png"),readback.EncodeToPNG());
                var expected=new[]{new Color32(246,165,114,255),pixels[1],pixels[2],pixels[3],pixels[4]};
                for(int i=0;i<5;i++)
                {
                    var point=camera.WorldToViewportPoint(new Vector3(i-2,0,0));var p=(Color32)readback.GetPixel(Mathf.RoundToInt(point.x*1280),Mathf.RoundToInt(point.y*720));
                    Assert.That((int)p.r,Is.EqualTo((int)expected[i].r).Within(3),"red sample "+i);
                    Assert.That((int)p.g,Is.EqualTo((int)expected[i].g).Within(3),"green sample "+i);
                    Assert.That((int)p.b,Is.EqualTo((int)expected[i].b).Within(3),"blue sample "+i);
                }
            }
            finally
            {
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();Object.Destroy(target);Object.Destroy(readback);Object.Destroy(probe);Object.Destroy(lamp);Object.Destroy(sprite);Object.Destroy(image);
            }
        }
    }
}
