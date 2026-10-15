using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightRemasterProductionTests
    {
        const string Output="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-production-20261014/captures/";
        static void Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
        static void Capture(Camera camera,string name)
        {
            var target=camera.targetTexture;var request=new UniversalRenderPipeline.SingleCameraRequest{destination=target};Assert.That(RenderPipeline.SupportsRenderRequest(camera,request),Is.True);RenderPipeline.SubmitRenderRequest(camera,request);
            var previous=RenderTexture.active;var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            try{RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(Output+name+".png",image.EncodeToPNG());}finally{RenderTexture.active=previous;Object.Destroy(image);}
        }
        [UnityTest]public IEnumerator MapsFormsAndControlsRenderOnProductionPipeline()
        {
            Directory.CreateDirectory(Output);DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;var remaster=game.GetComponent<DarklightRemaster>();Assert.That(remaster,Is.Not.Null);Assert.That(GraphicsSettings.currentRenderPipeline,Is.InstanceOf<UniversalRenderPipelineAsset>());
            var camera=lab.WorldCamera;var data=camera.GetComponent<UniversalAdditionalCameraData>();Assert.That(data.renderPostProcessing,Is.True);Assert.That(data.volumeLayerMask.value&1,Is.EqualTo(1));
            var volume=game.GetComponent<Volume>();Assert.That(volume.sharedProfile.components.Count,Is.EqualTo(4));Assert.That(volume.enabled,Is.True);
            game.PreviewChapter(0);yield return null;yield return null;Capture(camera,"normal");
            for(int skin=0;skin<2;skin++)for(int form=1;form<=3;form++)
            {
                game.SelectSkin(skin,false);lab.Player.ResetModel();game.Blood.Set(100);game.Progress.preferredForm=form;Assert.That(game.TryTransformPlayer(),Is.True);
                Invoke(game,"TickFormPresentation",DarklightCampaign.TransformationEnterDuration*.5f);yield return null;Capture(camera,$"skin-{skin}-transition-{form}");
                Invoke(game,"TickFormPresentation",DarklightCampaign.TransformationEnterDuration);yield return null;Capture(camera,$"skin-{skin}-aura-{form}");
                Assert.That(game.TryTransformPlayer(),Is.True);Invoke(game,"TickFormPresentation",DarklightCampaign.TransformationExitDuration+.01f);yield return null;
            }
            foreach(int chapter in new[]{1,3,6,9}){game.PreviewChapter(chapter);yield return null;yield return null;Capture(camera,"map-"+chapter);}
            game.PreviewPanel("Pause");yield return null;yield return new WaitForEndOfFrame();DarklightNativeCapture.Screen(Output+"pause-ui.png");yield return null;
            game.PreviewPanel("Controls");yield return null;yield return new WaitForEndOfFrame();DarklightNativeCapture.Screen(Output+"controls-keyboard.png");yield return null;
            typeof(DarklightCampaign).GetField("controlsPad",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(game,true);game.PreviewPanel("Controls");yield return null;yield return new WaitForEndOfFrame();DarklightNativeCapture.Screen(Output+"controls-gamepad.png");yield return null;
            game.PreviewPanel("Transformations");yield return null;yield return new WaitForEndOfFrame();DarklightNativeCapture.Screen(Output+"transformations-ui.png");yield return null;
            for(int q=0;q<4;q++){remaster.SetQuality(q);yield return null;Assert.That(data.renderPostProcessing,Is.EqualTo(q>0));Capture(camera,"quality-"+q);}
        }
    }
}
