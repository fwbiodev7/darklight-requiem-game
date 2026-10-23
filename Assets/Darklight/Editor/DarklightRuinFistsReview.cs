using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Darklight.Editor
{
    // EditMode capture of the imported textures through the actual player material.
    public static class DarklightRuinFistsReview
    {
        public static void ImportAndCapture()
        {
            DarklightRuinFistsImporter.Import();
            Capture();
            DarklightInterfaceReview.Capture();
        }
        public static void Capture()
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new InvalidOperationException("A graphics device is required for native palette review");
            var catalog=JsonUtility.FromJson<DarklightRuinFistsCatalog>(Resources.Load<TextAsset>("Darklight/RuinFists/catalog").text);
            if(!DarklightRuinFistsBank.Ready||!DarklightRuinFistsBank.Complete(catalog))throw new InvalidOperationException("Incomplete imported bank");
            string output=Path.GetFullPath("outputs/punhos-ruina-final-20261021");
            int captures=0;
            foreach(var page in catalog.pages)
            {
                var texture=Resources.Load<Texture2D>(page.resourcePath);
                var mask=Resources.Load<Texture2D>(page.palettePath);
                var sprites=Resources.LoadAll<Sprite>(page.resourcePath);
                if(texture==null||mask==null||sprites.Length!=12||texture.filterMode!=FilterMode.Point)throw new InvalidOperationException("Invalid imported page: "+page.resourcePath);
                foreach(string name in page.spriteNames)if(!sprites.Any(s=>s.name==name))throw new InvalidOperationException("Missing imported sprite: "+name);
                var figure=new GameObject("RuinNativeFigure");var cameraObject=new GameObject("RuinNativeCamera");
                var sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,1);
                var material=new Material(Shader.Find("Darklight/PixelSprite"));
                var target=new RenderTexture(texture.width,texture.height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB){filterMode=FilterMode.Point};
                var readback=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false,false);
                var previous=RenderTexture.active;
                try
                {
                    figure.layer=31;var renderer=figure.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;
                    var block=new MaterialPropertyBlock();block.SetTexture("_VampireMask",mask);block.SetFloat("_WholeSprite",1);
                    block.SetFloat("_RuinFists",1);block.SetFloat("_VampireForm",page.paletteVariant?page.form:0);
                    block.SetVector("_CanonicalSkinShift",DarklightCanonicalComplexion.ForTexture(texture));renderer.SetPropertyBlock(block);
                    var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=texture.height*.5f;
                    camera.transform.position=new Vector3(0,0,-10);camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;
                    camera.backgroundColor=Color.clear;camera.allowHDR=false;camera.allowMSAA=false;
                    target.Create();camera.targetTexture=target;DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;
                    readback.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);readback.Apply();
                    int visible=0;foreach(var pixel in readback.GetPixels32())if(pixel.a>128&&Math.Max(pixel.r,Math.Max(pixel.g,pixel.b))>20)visible++;
                    if(visible<5000)throw new InvalidOperationException("Empty native rendering: "+page.resourcePath);
                    string name=$"Ruin_{(page.skin==0?"Requiem":"Rock")}_{new[]{"Normal","Ruptura","Eclipse","Absoluto"}[page.form]}_{new[]{"S","SW","W","NW","N","NE","E","SE"}[page.direction]}_{page.bank}";
                    string directory=Path.Combine(output,name);Directory.CreateDirectory(directory);File.WriteAllBytes(Path.Combine(directory,"rendered.png"),readback.EncodeToPNG());captures++;
                }
                finally
                {
                    RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(figure);UnityEngine.Object.DestroyImmediate(cameraObject);
                    target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(readback);
                    UnityEngine.Object.DestroyImmediate(sprite);UnityEngine.Object.DestroyImmediate(material);
                }
            }
            File.WriteAllText(Path.Combine(output,"native-capture.json"),"{\"configurations\":"+captures+",\"physicalBanks\":"+catalog.pages.Count(p=>!p.paletteVariant)+",\"playMode\":false,\"audio\":false,\"graphicsDevice\":\""+SystemInfo.graphicsDeviceType+"\"}\n");
            Debug.Log("Punhos da Ruína: "+captures+" imported configurations captured through PixelSprite in EditMode; no Play or audio.");
        }
    }
}
