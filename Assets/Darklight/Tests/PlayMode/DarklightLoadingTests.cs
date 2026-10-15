using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightLoadingTests
    {
        const string Output="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-production-20261014/captures/";
        [UnityTest]public IEnumerator NextMapPreviewTeaserAndInputShieldSurviveTheWholeTransition()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;
            for(int chapter=0;chapter<10;chapter++)
            {
                var map=Resources.Load<Sprite>("Darklight/Loading/preview"+(chapter+1));Assert.That(map,Is.Not.Null);Assert.That(map.texture.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(DarklightCampaign.PhaseTeaser(chapter).Length,Is.InRange(35,120));
            }
            var method=typeof(DarklightCampaign).GetMethod("BeginPhaseTransition",BindingFlags.NonPublic|BindingFlags.Instance);
            int loads=0,ready=0;Action load=()=>{loads++;game.PreviewChapter(1);};Action done=()=>ready++;
            method.Invoke(game,new object[]{1,load,done});Assert.That(game.PhaseLoading,Is.True);Assert.That(game.LoadingChapter,Is.EqualTo(1));
            Assert.That(game.Progress.chapter,Is.Zero,"Map changes only after the loading screen can render");
            method.Invoke(game,new object[]{2,(Action)(()=>loads+=10),done});
            float timeout=Time.realtimeSinceStartup+10;
            while(loads==0&&Time.realtimeSinceStartup<timeout)yield return null;
            Assert.That(loads,Is.EqualTo(1));Assert.That(game.PhaseLoading,Is.True);Assert.That(game.Active,Is.False);
            var canvas=lab.PixelHUD.transform.Find("HUD640x360/PassagemDeFase");Assert.That(canvas.gameObject.activeSelf,Is.True);
            var preview=canvas.Find("PreviaDoMapa").GetComponent<UnityEngine.UI.Image>();Assert.That(preview.sprite.name,Is.EqualTo("preview2"));
            Assert.That(canvas.Find("Pressagio").GetComponent<UnityEngine.UI.Text>().text,Does.Contain("vozes perdidas"));
            Assert.That(game.TryTransformPlayer(),Is.False);float clock=lab.Player.Model.Clock;
            yield return new WaitForSecondsRealtime(.12f);Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));
            Directory.CreateDirectory(Output);DarklightNativeCapture.Screen(Output+"loading-distrito-vinil.png");
            while(game.PhaseLoading&&Time.realtimeSinceStartup<timeout)yield return null;
            Assert.That(game.PhaseLoading,Is.False);Assert.That(ready,Is.EqualTo(1));Assert.That(loads,Is.EqualTo(1));Assert.That(game.Progress.chapter,Is.EqualTo(1));
            Assert.That(canvas.gameObject.activeSelf,Is.False);Assert.That(game.Active,Is.True);Assert.That(game.LoadingChapter,Is.EqualTo(-1));
        }
    }
}

