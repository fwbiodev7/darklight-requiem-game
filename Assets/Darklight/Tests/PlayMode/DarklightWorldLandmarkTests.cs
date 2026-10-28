using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightWorldLandmarkTests
    {
        [UnityTest]public IEnumerator LargerShrineAndPortalsRemainReachableAndDoNotAddBlockers()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;lab.SetPaused(true);
            string folder=DarklightTestOutput.GetDirectory("minimapa-reforma-20261015/authorial/map-screen-r2/captures");
            Transform Level()=>(Transform)typeof(DarklightCampaign).GetField("levelRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            void At(Vector2 p){lab.Player.ResetModel();lab.Player.GetComponent<Rigidbody2D>().position=p;lab.Player.transform.position=p;Physics2D.SyncTransforms();}
            var shrine=Level().Find("Checkpoint");Assert.That(shrine.GetComponent<SpriteRenderer>().bounds.size.y,Is.EqualTo(2.3f*DarklightWorldScale.Environment).Within(.01f));Assert.That(shrine.GetComponent<Collider2D>(),Is.Null);
            At(game.CheckpointPoint);Assert.That(game.TryGetInteraction(out var target),Is.True);Assert.That(target.Id,Is.EqualTo("checkpoint"));
            At(game.CheckpointPoint+Vector2.right*2.7f);lab.WorldCamera.transform.position=(Vector3)game.CheckpointPoint+new Vector3(0,.75f,-10);yield return null;DarklightNativeCapture.Screen(folder+"checkpoint-maior.png");
            game.Progress.checkpoint=4;typeof(DarklightCampaign).GetMethod("LoadMap",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{0});yield return null;lab.SetPaused(true);
            var portal=Level().Find("Portal");Assert.That(portal,Is.Not.Null);Assert.That(portal.GetComponent<SpriteRenderer>().bounds.size.y,Is.EqualTo(4.2f*DarklightWorldScale.Environment).Within(.01f));Assert.That(portal.GetComponent<Collider2D>(),Is.Null);
            At(game.Map.SafeSpawn(game.Map.Portal));Assert.That(game.TryGetInteraction(out target),Is.True);Assert.That(target.Id,Is.EqualTo("portal"));
            At(game.Map.SafeSpawn(game.Map.Portal)+Vector2.right*3);lab.WorldCamera.transform.position=(Vector3)game.Map.Portal+new Vector3(0,.75f,-10);yield return null;DarklightNativeCapture.Screen(folder+"portal-maior.png");
            typeof(DarklightCampaign).GetMethod("ExploreRegion",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(game,new object[]{7});yield return null;lab.SetPaused(true);
            // Campaign preview and exploration use the same enlarged exit styling.
            var exit=Level().Find("SaidaDaRegiao");Assert.That(exit,Is.Not.Null);Assert.That(exit.GetComponent<SpriteRenderer>().bounds.size.y,Is.EqualTo(4.2f*DarklightWorldScale.Environment).Within(.01f));
        }
    }
}
