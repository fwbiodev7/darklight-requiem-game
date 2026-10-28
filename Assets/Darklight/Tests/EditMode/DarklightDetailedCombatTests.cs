using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightDetailedCombatTests
    {
        [SetUp] public void RequireImportedStrikePages()
        {
            if(!System.IO.File.Exists("Assets/Darklight/ArtSources/DetailedCombat20261017/manifest.json"))
                Assert.Ignore("Individual strike candidates await whole-frame visual review and are not production assets.");
        }
        [TestCase(0,0,"Leve1","Attacks")]
        [TestCase(1,1,"Presas1","Weapons")]
        public void EachIndividualStrikeFollowsItsRealTimingInEightDirectionsAndThreeForms(int skin,int weapon,string key,string folder)
        {
            var attack=AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Darklight/Data/"+folder+"/"+key+".asset");Assert.That(attack,Is.Not.Null);
            float[] times={attack.startup*.25f,attack.startup*.75f,attack.startup+attack.active*.25f,attack.startup+attack.active*.75f,
                attack.startup+attack.active+attack.recovery*.25f,attack.startup+attack.active+attack.recovery*.75f};
            for(int direction=0;direction<8;direction++)
            {
                var page=TodayCharacterBank.Page(skin,0,weapon,direction,"Attack",key);Assert.That(page,Is.Not.Null);Assert.That(page.phases,Is.EqualTo(6));
                Assert.That(page.startupFrameEnd,Is.EqualTo(2));Assert.That(page.activeFrameEnd,Is.EqualTo(4));
                for(int phase=0;phase<6;phase++)
                {
                    var model=new CombatModel(new CombatLoadout{lights=new[]{attack}});
                    var facing=(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);
                    model.Tick(.0001f,new CombatInput{lightPressed=true,aim=facing});model.Tick(times[phase]-.0001f,default);
                    var normal=TodayCharacterBank.Resolve(skin,0,weapon,direction,model,Vector2.zero);
                    int frame=Array.IndexOf(page.directions,direction)*6+phase;
                    Assert.That(normal.name,Is.EqualTo(page.spriteNames[frame]));Assert.That(TodayCharacterBank.WholeFrameId(normal),Is.EqualTo(frame+1));
                    for(int form=1;form<4;form++)
                    {
                        Assert.That(DarklightTransformedBank.TryResolve(normal,form,out var complete,out var mask,out int owner),Is.True);
                        Assert.That(complete,Is.Not.Null);Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.EqualTo(frame+1));
                        Assert.That(complete.pixelsPerUnit,Is.EqualTo(normal.pixelsPerUnit));Assert.That(DarklightCanonicalComplexion.ForTexture(complete.texture).w,Is.EqualTo(1));
                    }
                }
            }
        }
        [Test] public void DetailedFirstStrikeNeverReplacesOtherComboCutsOrHeavyAttacks()
        {
            foreach(var sample in new[]{(skin:0,weapon:0,key:"Leve2",folder:"Attacks"),(skin:1,weapon:1,key:"Presas2",folder:"Weapons")})
            {
                var attack=AssetDatabase.LoadAssetAtPath<AttackDefinition>("Assets/Darklight/Data/"+sample.folder+"/"+sample.key+".asset");Assert.That(attack,Is.Not.Null);
                var model=new CombatModel(new CombatLoadout{lights=new[]{attack}});model.Tick(.01f,new CombatInput{lightPressed=true});
                var generic=TodayCharacterBank.Page(sample.skin,0,sample.weapon,0,"Attack");Assert.That(generic,Is.Not.Null);
                Assert.That(TodayCharacterBank.Page(sample.skin,0,sample.weapon,0,"Attack",sample.key),Is.Null);
                var sprite=TodayCharacterBank.Resolve(sample.skin,0,sample.weapon,0,model,Vector2.zero);
                CollectionAssert.Contains(generic.spriteNames,sprite.name);
            }
        }
    }
}
