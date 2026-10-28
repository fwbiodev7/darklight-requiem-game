using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightCombatImpactTests
    {
        [UnityTest]public IEnumerator ContactsRenderFreezeWithPauseAndReuseOneBoundedEffectObject()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;
            game.Blood.Set(100);game.Progress.preferredForm=3;Assert.That(game.TryTransformPlayer(),Is.True);
            typeof(DarklightCampaign).GetMethod("TickFormState",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,new object[]{2f});
            var point=lab.Player.transform.position+Vector3.right*1.2f;DarklightCombatFX.Impact(point,true);yield return null;yield return null;
            var effect=lab.GetComponentInChildren<DarklightCombatImpacts>();Assert.That(effect,Is.Not.Null);var mesh=effect.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh.triangles.Length,Is.GreaterThan(0));Assert.That(effect.transform.parent,Is.EqualTo(lab.transform));
            string output=DarklightTestOutput.GetDirectory("remaster-production-20261014/captures");DarklightNativeCapture.Screen(output+"impacto-branco.png");
            for(int i=0;i<100;i++)DarklightCombatFX.Impact(point+Vector3.right*(i%3)*.03f,i%2==0);
            yield return null;yield return null;Assert.That(lab.GetComponentsInChildren<DarklightCombatImpacts>().Length,Is.EqualTo(1));Assert.That(mesh.vertexCount,Is.LessThanOrEqualTo(8192));
            lab.SetPaused(true);var before=mesh.vertices;var colors=mesh.colors;yield return new WaitForSecondsRealtime(.13f);
            Assert.That(mesh.vertices.SequenceEqual(before),Is.True);Assert.That(mesh.colors.SequenceEqual(colors),Is.True);
            lab.SetPaused(false);yield return new WaitForSeconds(.8f);Assert.That(effect.GetComponent<MeshRenderer>().enabled,Is.False);
        }
    }
}
