using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightRuinFistsMaterialTests
    {
        [UnityTest]public IEnumerator AuraChangesPreserveIrisSkinAndWarmArmor()
        {
            var source=new[]{new Color32(205,8,20,255),new Color32(230,140,90,255),new Color32(230,140,90,255),new Color32(160,20,30,255),new Color32(100,60,30,255)};
            var texture=new Texture2D(5,1,TextureFormat.RGBA32,false,false){filterMode=FilterMode.Point};texture.SetPixels32(source);texture.Apply();
            var mask=new Texture2D(5,1,TextureFormat.RGBA32,false,true){filterMode=FilterMode.Point};
            mask.SetPixels32(new[]{new Color32(255,0,0,1),new Color32(0,0,255,1),new Color32(0,0,0,1),new Color32(0,255,0,1),new Color32(0,205,0,1)});mask.Apply();
            var sprite=Sprite.Create(texture,new Rect(0,0,5,1),Vector2.one*.5f,1);var figure=new GameObject("RuinMaterialProbe");var cameraObject=new GameObject("RuinMaterialCamera");
            var material=new Material(Shader.Find("Darklight/PixelSprite"));var target=new RenderTexture(100,20,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var readback=new Texture2D(100,20,TextureFormat.RGBA32,false,false);var previous=RenderTexture.active;
            try
            {
                figure.layer=31;var renderer=figure.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;
                var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.5f;camera.transform.position=new Vector3(0,0,-10);
                camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;target.Create();camera.targetTexture=target;
                var energy=new Color32[3];
                for(int form=0;form<3;form++)
                {
                    var block=new MaterialPropertyBlock();block.SetTexture("_VampireMask",mask);block.SetFloat("_RuinFists",1);block.SetFloat("_WholeSprite",1);
                    block.SetFloat("_WholeFrameId",1);block.SetFloat("_VampireForm",form);block.SetVector("_CanonicalSkinShift",new Vector4(10/255f,5/255f,-3/255f,1));renderer.SetPropertyBlock(block);
                    yield return null;yield return null;DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,100,20),0,0);readback.Apply();
                    var expected=new[]{new Color32(196,8,20,255),new Color32(240,145,87,255),source[2]};
                    for(int i=0;i<3;i++)
                    {
                        var actual=(Color32)readback.GetPixel(i*20+10,10);
                        Assert.That((int)actual.r,Is.EqualTo((int)expected[i].r).Within(3),$"r material {i} form {form}");
                        Assert.That((int)actual.g,Is.EqualTo((int)expected[i].g).Within(3));Assert.That((int)actual.b,Is.EqualTo((int)expected[i].b).Within(3));
                    }
                    energy[form]=(Color32)readback.GetPixel(70,10);
                }
                Assert.That(energy[0],Is.Not.EqualTo(energy[1]));Assert.That(energy[1],Is.Not.EqualTo(energy[2]));
                Assert.That(energy[2].b,Is.GreaterThan(energy[2].r),"Eclipse energy is violet");
            }
            finally
            {
                RenderTexture.active=previous;Object.DestroyImmediate(figure);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);
                Object.DestroyImmediate(readback);Object.DestroyImmediate(sprite);Object.DestroyImmediate(material);Object.DestroyImmediate(texture);Object.DestroyImmediate(mask);
            }
        }
    }
}
