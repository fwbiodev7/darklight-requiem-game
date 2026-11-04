using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
namespace Darklight.Tests
{
    public sealed partial class DarklightNovemberIsolatedGameplayTests
    {
        [UnityTest] public IEnumerator NovemberFinalThreeWalkDirectionsLoopWithStableScaleAndReturnToIdle()
        {
            var request=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(File.ReadAllText("outputs/retomada-20261103/final-walks/pages.json"));
            Assert.That(request.pages,Has.Length.EqualTo(3),"Fists SW and Moto W/E must all be present, not legacy fallbacks");
            Assert.That(request.pages.Select(p=>p.weapon+":"+p.direction),Is.EquivalentTo(new[]{"2:1","3:2","3:6"}));
            foreach(var selected in request.pages)
            {
                var page=catalog.pages.Single(p=>p.resourcePath==selected.resourcePath&&p.skin==0&&p.form==0&&p.weapon==selected.weapon&&p.direction==selected.direction&&p.action=="Walk");
                Assert.That(page.spriteNames,Has.Length.EqualTo(4));Assert.That(page.loop,Is.True);
                EssentialContext(page.weapon,page.direction);var facing=Facing(page.direction);
                var visits=page.spriteNames.ToDictionary(n=>n,n=>0);string prior=null;float minHeight=float.MaxValue,maxHeight=0;
                for(int tick=0;tick<180;tick++)
                {
                    Tick(facing,new CombatInput{move=facing});AssertAuthored(page);AssertNativeFigure(page,art.sprite);
                    Assert.That(art.color,Is.EqualTo(Color.white));Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f));
                    int index=System.Array.IndexOf(page.spriteNames,art.sprite.name);
                    float density=page.framePixelsPerUnit?.Length==4?page.framePixelsPerUnit[index]:page.pixelsPerUnit;
                    Assert.That(art.sprite.pixelsPerUnit,Is.EqualTo(density).Within(.001f));
                    float height=art.sprite.bounds.size.y*art.transform.lossyScale.y;minHeight=Mathf.Min(minHeight,height);maxHeight=Mathf.Max(maxHeight,height);
                    if(prior!=art.sprite.name){visits[art.sprite.name]++;prior=art.sprite.name;}
                }
                foreach(var visit in visits)Assert.That(visit.Value,Is.GreaterThanOrEqualTo(2),page.id+": repeated authored phase "+visit.Key);
                Assert.That(maxHeight/minHeight,Is.LessThan(1.12f),page.id+": no shrinking/growing cycle");
                for(int tick=0;tick<100;tick++){Tick(facing);if(!page.spriteNames.Contains(art.sprite.name))break;}
                Assert.That(page.spriteNames,Does.Not.Contain(art.sprite.name),page.id+": leave walk");
                if(page.weapon==3)
                {
                    var idle=DarklightMotorcycleRedesignBank.DualSawsPose(page.direction,model);
                    Assert.That(idle,Is.Not.Null);Assert.That(art.sprite.name,Is.EqualTo(idle.name),page.id+": matching directional idle");
                }
                else
                {
                    var idle=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==page.weapon&&p.direction==page.direction&&p.action=="Idle");
                    Assert.That(idle.spriteNames,Does.Contain(art.sprite.name),page.id+": any current phase of the animated directional idle");
                }
                Assert.That(art.color,Is.EqualTo(Color.white));AssertSilent();yield return null;
            }
            Report("FinalThreeWalkDirections",3,12,"actual model clock, every phase repeated, native rect/pivot/density, world scale, bounded pose-height variation and directional idle return; gait artwork separately reviewed");
        }
    }
}
#endif
