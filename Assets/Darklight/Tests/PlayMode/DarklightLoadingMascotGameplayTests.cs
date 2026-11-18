using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Darklight.Tests
{
    public sealed class DarklightLoadingMascotGameplayTests
    {
        GameObject owned;
        IDisposable storage;
        bool silent,paused;
        float volume,timeScale;
        UnityEngine.Random.State randomState;
        Scene campaignScene;
        [SetUp] public void Protect()
        {
            silent=DarklightAutomation.Silent;paused=AudioListener.pause;volume=AudioListener.volume;
            timeScale=Time.timeScale;randomState=UnityEngine.Random.state;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/loading-mascots-20261117/"+Guid.NewGuid().ToString("N")+".json"));
        }
        [UnityTearDown] public IEnumerator Restore()
        {
            if(owned!=null)UnityEngine.Object.Destroy(owned);
            if(campaignScene.IsValid()&&campaignScene.isLoaded)
            {
                var empty=SceneManager.CreateScene("LoadingQAEmpty");SceneManager.SetActiveScene(empty);
                yield return SceneManager.UnloadSceneAsync(campaignScene);
            }
            storage?.Dispose();Time.timeScale=timeScale;UnityEngine.Random.state=randomState;
            DarklightAutomation.Silent=silent;AudioListener.pause=paused;AudioListener.volume=volume;
            yield return null;
        }
        [UnityTest] public IEnumerator BothLoopsUseEveryFrameWhilePausedAndStopWhenHidden()
        {
            owned=new GameObject("LoadingOnly",typeof(RectTransform));owned.SetActive(false);
            var mascot=DarklightLoadingMascot.Attach(owned.transform,new Vector2(6,-322));
            Time.timeScale=0;
            foreach(var variant in new[]{DarklightLoadingMascot.Variant.GatoRuptura,DarklightLoadingMascot.Variant.MiniFabius})
            {
                // Re-seed immediately before activation to exercise the actual OnEnable selection.
                for(int seed=0;seed<100;seed++)
                {
                    UnityEngine.Random.InitState(seed);
                    if(DarklightLoadingMascot.ChooseVariant(UnityEngine.Random.value)!=variant)continue;
                    UnityEngine.Random.InitState(seed);break;
                }
                owned.SetActive(true);
                Assert.That(mascot.SelectedVariant,Is.EqualTo(variant));
                var image=mascot.GetComponent<RawImage>();var texture=image.texture;
                Assert.That(texture,Is.Not.Null);Assert.That(image.raycastTarget,Is.False);
                var frames=new HashSet<int>();float until=Time.realtimeSinceStartup+1.65f;
                while(Time.realtimeSinceStartup<until)
                {
                    frames.Add(mascot.CurrentFrame);Assert.That(image.texture,Is.SameAs(texture));
                    Assert.That(mascot.SelectedVariant,Is.EqualTo(variant));
                    Assert.That(image.uvRect.width,Is.EqualTo(.25f));Assert.That(image.uvRect.height,Is.EqualTo(.5f));
                    yield return null;
                }
                Assert.That(frames.Count,Is.EqualTo(8));
                owned.SetActive(false);yield return new WaitForSecondsRealtime(.2f);
                Assert.That(mascot.CurrentFrame,Is.EqualTo(-1));Assert.That(image.texture,Is.Null);
                Assert.That(owned.GetComponentsInChildren<RawImage>(true).Length,Is.EqualTo(1));
            }
        }

        [UnityTest] public IEnumerator ExistingLoadingCompletesTwiceAndKeepsBothMascotsClearOfContent()
        {
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            campaignScene=SceneManager.GetActiveScene();yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            game.PreviewChapter(0);yield return null;
            var transition=typeof(DarklightCampaign).GetMethod("BeginPhaseTransition",BindingFlags.Instance|BindingFlags.NonPublic);
            int loads=0,ready=0;
            for(int i=0;i<2;i++)
            {
                int chapter=1;
                if(i>0){game.PreviewChapter(0);yield return null;}
                string output=Path.GetFullPath("outputs/loading-mascots-20261117");Directory.CreateDirectory(output);
                string framesPath=Path.Combine(output,i==0?"phase1-phase2-cat":"phase1-phase2-mini");Directory.CreateDirectory(framesPath);
                var timestamps=new List<float>();float captureStarted=Time.realtimeSinceStartup,nextCapture=0;
                void CaptureFrame()
                {
                    float elapsed=Time.realtimeSinceStartup-captureStarted;if(elapsed<nextCapture)return;
                    DarklightNativeCapture.Screen(Path.Combine(framesPath,$"frame-{timestamps.Count:0000}.png"));
                    timestamps.Add(elapsed);nextCapture=elapsed+.125f;
                }
                CaptureFrame();
                transition.Invoke(game,new object[]{chapter,(Action)(()=>{loads++;game.PreviewChapter(chapter);}),(Action)(()=>ready++)});
                Assert.That(game.PhaseLoading,Is.True);
                Transform root=null;float timeout=Time.realtimeSinceStartup+60;
                while(Time.realtimeSinceStartup<timeout)
                {
                    root=lab.PixelHUD.transform.Find("HUD640x360/PassagemDeFase");
                    if(root!=null&&root.gameObject.activeSelf)break;
                    CaptureFrame();yield return null;
                }
                Assert.That(root,Is.Not.Null);
                Assert.That(root.gameObject.activeSelf,Is.True);
                var mascot=root.GetComponentInChildren<DarklightLoadingMascot>(true);
                Assert.That(mascot,Is.Not.Null);
                var variant=(DarklightLoadingMascot.Variant)i;
                mascot.PlayVariant(variant); // deterministic evidence; normal loading uses OnEnable's 50/50 choice.
                var rect=(RectTransform)mascot.transform;
                var teaser=root.Find("Pressagio").GetComponent<Text>();string originalTeaser=teaser.text;
                for(int teaserChapter=0;teaserChapter<10;teaserChapter++)
                {
                    teaser.text="“"+DarklightCampaign.PhaseTeaser(teaserChapter)+"”";Canvas.ForceUpdateCanvases();
                    Assert.That(teaser.preferredHeight,Is.LessThanOrEqualTo(teaser.rectTransform.rect.height));
                    var bounds=teaser.rectTransform.rect;var vertices=teaser.cachedTextGenerator.verts;
                    for(int vertex=0;vertex<vertices.Count-4;vertex++)
                    {
                        Vector3 point=vertices[vertex].position/teaser.pixelsPerUnit;
                        Assert.That(point.x,Is.InRange(bounds.xMin-1,bounds.xMax+1),"Teaser glyph must fit the inner panel");
                        Assert.That(point.y,Is.InRange(bounds.yMin-1,bounds.yMax+1),"Teaser glyph must fit the inner panel");
                    }
                }
                teaser.text=originalTeaser;
                foreach(var name in new[]{"Estado","Continuar","Trilho","PreviaDoMapa","Pressagio"})
                {
                    var other=(RectTransform)root.Find(name);
                    var a=new Vector3[4];var b=new Vector3[4];rect.GetWorldCorners(a);other.GetWorldCorners(b);
                    Assert.That(new Rect(a[0],a[2]-a[0]).Overlaps(new Rect(b[0],b[2]-b[0])),Is.False,name);
                }
                while(loads<=i&&Time.realtimeSinceStartup<timeout){CaptureFrame();yield return null;}
                Assert.That(loads,Is.EqualTo(i+1));
                Assert.That(root.Find("PreviaDoMapa").GetComponent<Image>().sprite.name,Is.EqualTo("preview"+(chapter+1)));
                Canvas.ForceUpdateCanvases();yield return null;
                Assert.That(teaser.font.material.mainTexture.filterMode,Is.EqualTo(FilterMode.Point));
                DarklightNativeCapture.Screen(Path.Combine(output,i==0?"gato-ruptura.png":"mini-fabius.png"));
                while(game.PhaseLoading&&Time.realtimeSinceStartup<timeout)
                {
                    Assert.That(mascot.SelectedVariant,Is.EqualTo(variant));CaptureFrame();yield return null;
                }
                Assert.That(game.PhaseLoading,Is.False);Assert.That(ready,Is.EqualTo(i+1));
                Assert.That(root.gameObject.activeSelf,Is.False);Assert.That(mascot.CurrentFrame,Is.EqualTo(-1));
                Assert.That(root.GetComponentsInChildren<DarklightLoadingMascot>(true).Length,Is.EqualTo(1));
                Assert.That(game.Progress.chapter,Is.EqualTo(chapter));
                while(!game.Active&&Time.realtimeSinceStartup<timeout){CaptureFrame();yield return null;}
                Assert.That(game.Active,Is.True);CaptureFrame();
                File.WriteAllText(Path.Combine(framesPath,"timestamps.json"),"["+string.Join(",",timestamps.ConvertAll(t=>t.ToString("F4",System.Globalization.CultureInfo.InvariantCulture)))+"]");
            }
            foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
    }
}
