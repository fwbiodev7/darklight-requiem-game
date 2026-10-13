using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightBridgeTests
    {
        [UnityTest] public IEnumerator AuthoredGardenBridgesAndObservatoryStairsAllowBothDirections()
        {
            DarklightAutomation.Silent=true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;var lab=DarklightCombatLab.Instance;
            var joins=new[]{new Vector4(611,497,730,497),new Vector4(1057,675,1175,675),new Vector4(810,359,810,412),new Vector4(754,598,754,645)};
            for(int j=0;j<joins.Length;j++)
            {
                game.PreviewChapter(j==3?5:4);yield return null;yield return null;
                lab.SetPaused(true);Physics2D.SyncTransforms();
                var v=joins[j];Vector2 a=game.Map.Art.World(new Vector2(v.x,v.y)),b=game.Map.Art.World(new Vector2(v.z,v.w));
                string obstruction="";
                foreach(var hit in Physics2D.CircleCastAll(a,.3f,(b-a).normalized,Vector2.Distance(a,b)))
                    if(!hit.collider.isTrigger&&hit.collider.GetComponentInParent<DarklightCombatant>()==null)
                        obstruction+=" hit="+game.Map.Art.Pixel(hit.point)+" center="+game.Map.Art.Pixel(hit.collider.bounds.center)+" size="+hit.collider.bounds.size;
                Assert.That(DarklightNavigation.Clear(a,b,.3f),Is.True,"Join "+j+" has a physical obstruction:"+obstruction);
                var body=lab.Player.GetComponent<Rigidbody2D>();
                for(int direction=0;direction<2;direction++)
                {
                    body.position=direction==0?a:b;Physics2D.SyncTransforms();Vector2 end=direction==0?b:a;
                    for(int step=0;step<160&&Vector2.Distance(body.position,end)>.015f;step++)
                    {
                        Vector2 delta=Vector2.ClampMagnitude(end-body.position,.12f);
                        DarklightNavigation.SafeStep(body,delta,.3f);Physics2D.SyncTransforms();
                    }
                    Assert.That(Vector2.Distance(body.position,end),Is.LessThan(.03f),"Physical traversal blocked: join "+j+" direction "+direction);
                }
                lab.SetPaused(false);
            }
        }
    }
}
