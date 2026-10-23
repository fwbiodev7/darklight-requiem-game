using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightRuinFistsProfileTests
    {
        readonly List<Object> owned=new();
        CombatLoadout Loadout()=>DarklightRuinFistsProfile.Create(owned.Add);
        [TearDown] public void CleanUp(){foreach(var item in owned)Object.DestroyImmediate(item);owned.Clear();}
        [Test] public void ThirdSlotHasFourDifferentMartialContactsAndNoChainPull()
        {
            var l=Loadout();Assert.That(l.weapon,Is.EqualTo(2));Assert.That(l.lights.Length,Is.EqualTo(4));
            var distinct=new HashSet<DarklightRuinStrike>();
            foreach(var a in l.lights){distinct.Add(a.ruinStrike);Assert.That(a.range,Is.LessThan(2));Assert.That(a.pullTarget,Is.False);}
            Assert.That(distinct.Count,Is.EqualTo(4));Assert.That(l.lights[3].finisher,Is.True);
            foreach(var a in new[]{l.heavy,l.charged,l.counter,l.skill,l.aerial,l.weaponSpecial,l.pauseBranch,l.heavyBranch})
            {Assert.That(a.pullTarget,Is.False);Assert.That(a.blink,Is.False);Assert.That(a.ruinStrike,Is.Not.EqualTo(DarklightRuinStrike.None));}
            Assert.That(l.heavy.launch,Is.True);Assert.That(l.charged.damage,Is.GreaterThan(l.heavy.damage));
        }
        [Test] public void FourInputsReachSlamAndFifthRestartsWithoutTouchingOtherLoadouts()
        {
            var l=Loadout();var model=new CombatModel(l);
            for(int i=0;i<5;i++)
            {
                model.Tick(.001f,new CombatInput{lightPressed=true});
                Assert.That(model.Attack,Is.SameAs(l.lights[i%4]));
                model.Tick(model.Attack.Duration+.001f,default);
            }
        }
        [Test] public void AttackFramesUseContactPhaseRatherThanWholeAttackDuration()
        {
            var l=Loadout();var a=l.lights[0];
            Assert.That(DarklightRuinFistsBank.AttackFrame(a,0,false,0),Is.EqualTo(2));
            Assert.That(DarklightRuinFistsBank.AttackFrame(a,a.startup,true,.5f),Is.EqualTo(3));
            Assert.That(DarklightRuinFistsBank.AttackFrame(a,a.startup+a.active+.001f,false,1),Is.EqualTo(11));
            Assert.That(DarklightRuinFistsBank.AttackFrame(l.aerial,l.aerial.startup,true,.5f),Is.EqualTo(9));
        }
        [Test] public void IncompleteDirectionalOrFormCatalogCannotActivateReplacement()
        {
            var pages=new List<DarklightRuinFistsPage>();
            for(int skin=0;skin<2;skin++)for(int form=0;form<4;form++)for(int dir=0;dir<8;dir++)for(int bank=0;bank<2;bank++)
            {
                string key=$"{skin}_{form}_{dir}_{bank}";
                var names=new string[12];for(int i=0;i<12;i++)names[i]="Ruin_test_"+key+"_"+i;
                pages.Add(new DarklightRuinFistsPage{skin=skin,form=form,direction=dir,bank=bank==0?"Combat":"Motion",
                    resourcePath="Darklight/RuinFists/test_"+key,palettePath="Darklight/RuinFists/test_"+key+"_Owner",spriteNames=names});
            }
            var data=new DarklightRuinFistsCatalog{complete=true,pages=pages.ToArray()};
            Assert.That(DarklightRuinFistsBank.Complete(data),Is.True);
            foreach(var page in data.pages)if(page.form==1||page.form==2)
            {
                var source=pages.Find(p=>p.skin==page.skin&&p.form==0&&p.direction==page.direction&&p.bank==page.bank);
                page.paletteVariant=true;page.resourcePath=source.resourcePath;page.palettePath=source.palettePath;page.spriteNames=source.spriteNames;
            }
            Assert.That(DarklightRuinFistsBank.Complete(data),Is.True,"Explicit material variants reuse only the same skin/direction/action geometry");
            string alias=data.pages[16].resourcePath;data.pages[16].resourcePath=data.pages[0].resourcePath+"_wrong";
            Assert.That(DarklightRuinFistsBank.Complete(data),Is.False);data.pages[16].resourcePath=alias;
            string oldPath=data.pages[127].resourcePath;data.pages[127].resourcePath=data.pages[0].resourcePath;
            Assert.That(DarklightRuinFistsBank.Complete(data),Is.False,"An atlas from another direction cannot fill a missing page");
            data.pages[127].resourcePath=oldPath;
            data.pages[127].direction=0;Assert.That(DarklightRuinFistsBank.Complete(data),Is.False);
            data.complete=false;Assert.That(DarklightRuinFistsBank.Complete(data),Is.False);
        }
        [Test] public void FormsHaveDistinctEnergyWithoutChangingTheIrisPalette()
        {
            var colors=new HashSet<Color>();for(int i=0;i<4;i++)colors.Add(DarklightRuinFistsProfile.EnergyColor(i));
            Assert.That(colors.Count,Is.EqualTo(4));
            Assert.That(DarklightRuinFistsProfile.EnergyColor(0).maxColorComponent,Is.LessThan(DarklightRuinFistsProfile.EnergyColor(1).maxColorComponent));
        }
        [Test] public void LowWalkUsesAllFourContactsWithoutThePreviousFastCadence()
        {
            for(int phase=0;phase<4;phase++)
                Assert.That(DarklightRuinFistsBank.WalkFrame(phase*.15f+.025f),Is.EqualTo(phase));
            Assert.That(DarklightRuinFistsBank.WalkFrame(.625f),Is.EqualTo(0));
            Assert.That(DarklightRuinFistsBank.WalkFrame(123.425f),Is.InRange(0,3));
        }
    }
}
