using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightComplexionRenderTests
    {
        [UnityTest] public IEnumerator PixelShaderCorrectsWarmSkinAndPreservesHairMetalAndEnergy()=>Probe("Darklight/PixelSprite");
        static IEnumerator Probe(string shader)
        {
            Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(GraphicsDeviceType.Null),"This color test requires an EditMode graphics device");
            var pixels=new[]{new Color32(250,140,97,255),new Color32(220,220,220,255),new Color32(110,75,53,255),new Color32(250,30,45,255),new Color32(160,30,225,255)};
            var image=new Texture2D(5,1,TextureFormat.RGBA32,false,false){filterMode=FilterMode.Point};image.SetPixels32(pixels);image.Apply();
            var sprite=Sprite.Create(image,new Rect(0,0,5,1),new Vector2(.5f,.5f),1);
            var figure=new GameObject("ColorProbe");var cameraObject=new GameObject("ColorProbeCamera");
            var material=new Material(Shader.Find(shader));
            var oldPipeline=GraphicsSettings.defaultRenderPipeline;var oldQualityPipeline=QualitySettings.renderPipeline;
            var lightObject=new GameObject("PaletteWhiteGlobalLight");
            if(shader.EndsWith("SpriteLit"))
            {
                var pipeline=Resources.Load<RenderPipelineAsset>("Darklight/Rendering/Pipeline");
                GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;
                System.Type type=null;
                foreach(var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    if((type=assembly.GetType("UnityEngine.Rendering.Universal.Light2D"))!=null)break;
                type??=System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.2D.Runtime");
                Assert.That(type,Is.Not.Null);var light=lightObject.AddComponent(type);
                type.GetProperty("lightType").SetValue(light,System.Enum.Parse(type.GetProperty("lightType").PropertyType,"Global"));
                type.GetProperty("intensity").SetValue(light,1f);
            }
            var target=new RenderTexture(100,20,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){filterMode=FilterMode.Point};
            var readback=new Texture2D(100,20,TextureFormat.RGBA32,false,false);var previous=RenderTexture.active;
            try
            {
                figure.layer=31;var renderer=figure.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;
                var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.5f;
                if(shader.EndsWith("SpriteLit"))
                {
                    var type=System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                    var data=cameraObject.AddComponent(type);type.GetMethod("SetRenderer").Invoke(data,new object[]{0});
                }
                camera.transform.position=new Vector3(0,0,-10);camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;
                target.Create();camera.targetTexture=target;
                var block=new MaterialPropertyBlock();block.SetVector("_CanonicalSkinShift",new Vector4(-4/255f,25/255f,17/255f,1));renderer.SetPropertyBlock(block);
                yield return null;yield return null;
                DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,100,20),0,0);readback.Apply();
                var expected=new[]{new Color32(246,165,114,255),pixels[1],pixels[2],pixels[3],pixels[4]};
                for(int i=0;i<5;i++)
                {
                    var actual=(Color32)readback.GetPixel(i*20+10,10);
                    Assert.That((int)actual.r,Is.EqualTo((int)expected[i].r).Within(3),"red sample "+i);
                    Assert.That((int)actual.g,Is.EqualTo((int)expected[i].g).Within(3),"green sample "+i);
                    Assert.That((int)actual.b,Is.EqualTo((int)expected[i].b).Within(3),"blue sample "+i);
                }
            }
            finally
            {
                RenderTexture.active=previous;Object.DestroyImmediate(figure);Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(lightObject);target.Release();
                Object.DestroyImmediate(target);Object.DestroyImmediate(readback);Object.DestroyImmediate(material);Object.DestroyImmediate(sprite);Object.DestroyImmediate(image);
                GraphicsSettings.defaultRenderPipeline=oldPipeline;QualitySettings.renderPipeline=oldQualityPipeline;
            }
        }
    }
}
