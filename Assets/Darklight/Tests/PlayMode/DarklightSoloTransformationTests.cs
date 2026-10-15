using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightSoloTransformationTests
    {
        const string Output="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-prototype-20261014/solo/";
        [Serializable] class HeightProof{public int skin,form;public float normal,transformed;}
        [Serializable] class Proof{public string status;public int frames;public bool pipelineRestored;public List<HeightProof> heights=new();}
        static object Call(string method,params object[] args)=>Type.GetType("Darklight.Editor.RemasterPrototype, Darklight.Editor").GetMethod(method,BindingFlags.Public|BindingFlags.Static).Invoke(null,args);
        static void Capture(RenderTexture target,Texture2D image,string name)
        {
            var previous=RenderTexture.active;try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(Output+name,image.EncodeToPNG());}finally{RenderTexture.active=previous;}
        }
        [UnityTest]public IEnumerator SoloFormsHaveNormalHeightAndCompleteTransitionEndpoints()
        {
            Directory.CreateDirectory(Output);DarklightAutomation.Silent=true;AudioListener.volume=0;
            var result=new Proof{status="RUNNING"};
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;
            lab.SetPaused(true);game.enabled=false;lab.Player.enabled=false;lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled=false;
            var oldGraphics=GraphicsSettings.defaultRenderPipeline;var oldQuality=QualitySettings.renderPipeline;var oldTarget=lab.WorldCamera.targetTexture;
            var target=new RenderTexture(768,896,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();
            var image=new Texture2D(768,896,TextureFormat.RGBA32,false);lab.WorldCamera.targetTexture=target;
            try
            {
                Call("Enable");
                for(int skin=0;skin<2;skin++)
                {
                    Call("SoloSkin",skin);Call("ApplyTransformation",0);Call("AuraStep",0,0f);Call("SoloLayers");yield return null;yield return null;
                    Assert.That(lab.WorldCamera.projectionMatrix.m00/lab.WorldCamera.projectionMatrix.m11,Is.EqualTo((float)target.height/target.width).Within(.001f),"Portrait capture must preserve the sprite's proportions");
                    float height=(float)Call("SoloBodyHeight");Capture(target,image,$"skin-{skin}-normal.png");result.frames++;
                    for(int form=1;form<4;form++)
                    {
                        Call("ApplyTransformation",form);Call("SetWeaponLight",form);float transformed=(float)Call("SoloBodyHeight");
                        Assert.That(transformed,Is.EqualTo(height).Within(.002f),"Transformation must keep the normal body's height");result.heights.Add(new HeightProof{skin=skin,form=form,normal=height,transformed=transformed});
                        for(int frame=0;frame<72;frame++){Call("AuraStep",form,frame/12f);Call("SoloLayers");yield return null;yield return null;Capture(target,image,$"skin-{skin}-aura-{form}-{frame:00}.png");result.frames++;}
                        for(int frame=0;frame<36;frame++){Call("TransitionStep",form,frame/35f);Call("SoloLayers");yield return null;yield return null;Capture(target,image,$"skin-{skin}-transition-{form}-{frame:00}.png");result.frames++;}
                        Assert.That((float)Call("SoloBodyHeight"),Is.EqualTo(height).Within(.002f));
                    }
                }
                result.status="SUCCESS";Assert.That(result.frames,Is.EqualTo(650));
            }
            finally
            {
                Call("Restore");result.pipelineRestored=GraphicsSettings.defaultRenderPipeline==oldGraphics&&QualitySettings.renderPipeline==oldQuality;
                lab.WorldCamera.targetTexture=oldTarget;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);File.WriteAllText(Output+"proof.json",JsonUtility.ToJson(result,true));
            }
            Assert.That(result.pipelineRestored,Is.True);
        }
    }
}
