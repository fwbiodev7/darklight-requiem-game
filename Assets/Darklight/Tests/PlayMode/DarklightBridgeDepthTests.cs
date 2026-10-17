using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightBridgeDepthTests
    {
        [UnityTest] public IEnumerator OpaqueTreeFloorCannotConcealTheHeroOnTheBridge()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;game.PreviewChapter(4);yield return null;yield return null;
            var owner=Object.FindFirstObjectByType<DarklightBridgeVisibility>();Assert.That(owner,Is.Not.Null);
            var body=lab.Player.GetComponent<Rigidbody2D>();
            foreach(var pixel in new[]{new Vector2(600,260),new Vector2(672,490),new Vector2(1150,665)})
            {
                body.position=game.Map.Art.World(pixel);lab.Player.transform.position=body.position;Physics2D.SyncTransforms();yield return null;yield return null;
                Assert.That(owner.ProtectingPlayer,Is.True,"Bridge corridor: "+pixel);
            }
            body.position=game.Map.Art.World(new Vector2(600,260));lab.Player.transform.position=body.position;lab.SetPaused(true);Physics2D.SyncTransforms();
            yield return null;yield return null;
            var camera=lab.WorldCamera;camera.transform.position=new Vector3(body.position.x,body.position.y,-10);
            var diagnostic=new System.Text.StringBuilder();var block=new MaterialPropertyBlock();
            foreach(var item in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                item.GetPropertyBlock(block);
                if(block.GetFloat("_BridgeForeground")>.5f||item.name=="FabioVisual")diagnostic.AppendLine(item.name+" shader="+item.sharedMaterial?.shader.name+" flag="+block.GetFloat("_BridgeForeground")+" bounds="+item.bounds+" order="+item.sortingOrder);
            }
            diagnostic.AppendLine("visible="+Shader.GetGlobalFloat("_BridgeActorVisible")+" bounds="+Shader.GetGlobalVector("_BridgeActorBounds"));
            File.WriteAllText(Path.GetFullPath("work/pendencias-20261017/bridge-diagnostic.txt"),diagnostic.ToString());
            var before=Capture(camera,false);var after=Capture(camera,true);
            try
            {
                int changed=0;var a=before.GetPixels32();var b=after.GetPixels32();
                for(int i=0;i<a.Length;i++)if(a[i].r!=b[i].r||a[i].g!=b[i].g||a[i].b!=b[i].b)changed++;
                string output=Path.GetFullPath("outputs/validacao-pendencias-20261017");Directory.CreateDirectory(output);
                File.WriteAllBytes(Path.Combine(output,"ponte-antes-unity.png"),before.EncodeToPNG());
                File.WriteAllBytes(Path.Combine(output,"ponte-corrigida-unity.png"),after.EncodeToPNG());
                Assert.That(changed,Is.GreaterThan(20),"The real foreground overlap must be removed, not only its metadata changed");
            }
            finally{Object.Destroy(before);Object.Destroy(after);}
            body.position=game.Map.Art.World(game.Map.Art.spawn);lab.Player.transform.position=body.position;yield return null;
            Assert.That(owner.ProtectingPlayer,Is.False);game.PreviewChapter(0);yield return null;
            Assert.That(Shader.GetGlobalFloat("_BridgeActorVisible"),Is.Zero);
        }
        static Texture2D Capture(Camera camera,bool correction)
        {
            var previous=RenderTexture.active;var target=camera.targetTexture;
            Shader.SetGlobalFloat("_BridgeActorVisible",correction?1:0);camera.Render();RenderTexture.active=target;
            var result=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            result.ReadPixels(new Rect(0,0,target.width,target.height),0,0);result.Apply();RenderTexture.active=previous;return result;
        }
    }
}
