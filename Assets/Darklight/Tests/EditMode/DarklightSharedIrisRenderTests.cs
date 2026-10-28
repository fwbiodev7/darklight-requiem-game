using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightSharedIrisRenderTests
    {
        [UnityTest] public IEnumerator EveryFormKeepsRedIrisesBlackPupilsWhiteGlintsAndOtherMaterials()
        {
            Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(GraphicsDeviceType.Null));
            var pixels=new[]{new Color32(205,8,20,255),new Color32(20,10,10,255),new Color32(244,244,244,255),
                new Color32(145,25,220,255),new Color32(240,15,30,255),new Color32(246,174,130,255)};
            var texture=new Texture2D(6,1,TextureFormat.RGBA32,false,false){filterMode=FilterMode.Point};
            texture.SetPixels32(pixels);texture.Apply();
            var selector=new Texture2D(6,1,TextureFormat.RGBA32,false,true){filterMode=FilterMode.Point};
            selector.SetPixels32(new[]{new Color32(255,0,0,255),new Color32(255,0,0,255),new Color32(255,0,0,255),
                new Color32(0,0,0,255),new Color32(0,0,0,255),new Color32(0,0,0,255)});selector.Apply();
            var sprite=Sprite.Create(texture,new Rect(0,0,6,1),Vector2.one*.5f,1);
            var figure=new GameObject("SharedIrisProbe");var cameraObject=new GameObject("SharedIrisCamera");
            var material=new Material(Shader.Find("Darklight/PixelSprite"));
            var target=new RenderTexture(120,20,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){filterMode=FilterMode.Point};
            var readback=new Texture2D(120,20,TextureFormat.RGBA32,false,false);var previous=RenderTexture.active;
            try
            {
                figure.layer=31;var renderer=figure.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;
                var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.5f;
                camera.transform.position=new Vector3(0,0,-10);camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;
                target.Create();camera.targetTexture=target;
                var expected=(Color32[])pixels.Clone();expected[0]=new Color32(196,8,20,255);
                for(int form=0;form<4;form++)
                {
                    var block=new MaterialPropertyBlock();block.SetTexture("_VampireMask",selector);
                    block.SetFloat("_VampireForm",form);block.SetFloat("_WholeSprite",1);renderer.SetPropertyBlock(block);
                    yield return null;yield return null;
                    DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;
                    readback.ReadPixels(new Rect(0,0,120,20),0,0);readback.Apply();
                    for(int i=0;i<6;i++)
                    {
                        var actual=(Color32)readback.GetPixel(i*20+10,10);
                        Assert.That((int)actual.r,Is.EqualTo((int)expected[i].r).Within(3),"red form/sample "+form+"/"+i);
                        Assert.That((int)actual.g,Is.EqualTo((int)expected[i].g).Within(3),"green form/sample "+form+"/"+i);
                        Assert.That((int)actual.b,Is.EqualTo((int)expected[i].b).Within(3),"blue form/sample "+form+"/"+i);
                        Assert.That((int)actual.a,Is.EqualTo(255));
                    }
                }
            }
            finally
            {
                RenderTexture.active=previous;Object.DestroyImmediate(figure);Object.DestroyImmediate(cameraObject);target.Release();
                Object.DestroyImmediate(target);Object.DestroyImmediate(readback);Object.DestroyImmediate(material);
                Object.DestroyImmediate(sprite);Object.DestroyImmediate(texture);Object.DestroyImmediate(selector);
            }
        }
    }
}
