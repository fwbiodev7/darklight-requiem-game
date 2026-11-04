using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        int novemberOriginalQuality;
        bool novemberQualityOverridden;
        static FieldInfo NovemberPreviewQuality=>typeof(DarklightRemaster).GetField("previewQuality",BindingFlags.NonPublic|BindingFlags.Static);

        [SetUp] public void NovemberUseTemporaryLowQuality()
        {
            if(!(TestContext.CurrentContext.Test.MethodName??"").StartsWith("November",StringComparison.Ordinal))return;
            var field=NovemberPreviewQuality;Assert.That(field,Is.Not.Null);
            novemberOriginalQuality=(int)field.GetValue(null);novemberQualityOverridden=true;
            var temporary=new GameObject("NovemberIntegrationTemporaryQuality");
            try{temporary.AddComponent<DarklightRemaster>().SetQuality(0,false);}
            finally{UnityEngine.Object.DestroyImmediate(temporary);}
            Assert.That(DarklightRemaster.QualityLevel,Is.Zero);
            TestContext.Progress.WriteLine("November integration checks use temporary quality 0, without saving preferences; this is not FullGraphicQA.");
        }

        [TearDown] public void NovemberRestoreTemporaryQuality()
        {
            if(!novemberQualityOverridden)return;
            NovemberPreviewQuality.SetValue(null,novemberOriginalQuality);novemberQualityOverridden=false;
        }

        DarklightWeaponRedesignPage[] NovemberPages()
        {
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&
                ((p.weapon==0&&p.action=="Attack"&&p.attackKey=="Pesado")||(p.weapon==0&&p.action=="Walk"&&(p.direction==5||p.direction==6||p.direction==7))||(p.weapon==2&&p.action=="Walk"&&(p.direction==3||p.direction==4||p.direction==5||p.direction==7)))).ToArray();
            Assert.That(pages,Has.Length.EqualTo(15),"Only the fifteen newly integrated November 2 clips count here");
            Assert.That(pages.Where(p=>p.weapon==0&&p.action=="Walk").Select(p=>p.direction),Is.EquivalentTo(new[]{5,6,7}));
            Assert.That(pages.Where(p=>p.weapon==2&&p.action=="Walk").Select(p=>p.direction),Is.EquivalentTo(new[]{3,4,5,7}));
            Assert.That(pages.Where(p=>p.weapon==0&&p.action=="Attack"&&p.attackKey=="Pesado").Select(p=>p.direction),Is.EquivalentTo(Enumerable.Range(0,8)));
            return pages;
        }

        void AssertNovemberFigure(DarklightWeaponRedesignPage page)
        {
            AssertAuthored(page);
            var legacyView=fighter.GetComponent<DarklightOriginalVampireView>();
            Assert.That(legacyView,Is.Not.Null,page.id);
            Assert.That(typeof(DarklightOriginalVampireView).GetField("catalog",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(legacyView),Is.Null,page.id+": authored figures must not load legacy palette textures");
            Assert.That(art.color,Is.EqualTo(Color.white),page.id);
            Assert.That(DarklightCharacterView.WorldVisualScale,Is.EqualTo(1.325f));
            Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f),page.id);
            int index=Array.IndexOf(page.spriteNames,art.sprite.name);
            float density=page.framePixelsPerUnit?.Length==page.spriteNames.Length?page.framePixelsPerUnit[index]:page.pixelsPerUnit;
            Assert.That(art.sprite.pixelsPerUnit,Is.EqualTo(density).Within(.001f),page.id);
        }

        [UnityTest] public IEnumerator NovemberIntegrationContainsExactlyFifteenClipsAndSixtyNativePoses()
        {
            var pages=NovemberPages();
            Assert.That(pages.Sum(p=>p.spriteNames.Length),Is.EqualTo(60));
            Assert.That(pages.SelectMany(p=>p.spriteNames).Distinct().Count(),Is.EqualTo(60));
            foreach(var page in pages)
            {
                Assert.That(page.wholeFigures,Is.True,page.id);
                Assert.That(page.spriteNames,Has.Length.EqualTo(4),page.id);
                Assert.That(page.pixelsPerUnit,Is.GreaterThan(0),page.id);
                if(page.framePixelsPerUnit!=null&&page.framePixelsPerUnit.Length>0)
                {Assert.That(page.framePixelsPerUnit,Has.Length.EqualTo(4),page.id);foreach(float density in page.framePixelsPerUnit)Assert.That(density,Is.GreaterThan(0),page.id);}
                var sprites=Resources.LoadAll<Sprite>(page.resourcePath);
                foreach(int index in Enumerable.Range(0,4))
                {
                    var sprite=sprites.SingleOrDefault(s=>s.name==page.spriteNames[index]);
                    Assert.That(sprite,Is.Not.Null,page.id+": missing pose "+index);
                    Assert.That(sprite.pixelsPerUnit,Is.EqualTo(page.pixelsPerUnit).Within(.001f),page.id);
                }
                if(page.action=="Walk")
                {
                    Assert.That(page.loop,Is.True,page.id);
                    Assert.That(page.durationMs,Is.EqualTo(new[]{100,100,100,100}),page.id);
                }
                else
                {
                    Assert.That(page.loop,Is.False,page.id);
                    Assert.That(page.durationMs,Is.EqualTo(new[]{100,110,100,100}),page.id);
                    Assert.That(page.startupFrames,Is.EqualTo(1),page.id);
                    Assert.That(page.activeFrames,Is.EqualTo(1),page.id);
                    Assert.That(page.spriteNames.Length-page.startupFrames-page.activeFrames,Is.EqualTo(2),page.id);
                }
            }
            AssertSilent();yield return null;
        }

        [UnityTest] public IEnumerator NovemberSevenWalkClipsRepeatEveryPoseAndReturnToIdle()
        {
            int figures=0;
            foreach(var page in NovemberPages().Where(p=>p.action=="Walk"))
            {
                Context(0,page.weapon,0,page.direction);var facing=Facing(page.direction);
                var visits=page.spriteNames.ToDictionary(n=>n,n=>0);string previous=null;
                float clock=fighter.Model.Clock;
                for(int tick=0;tick<180;tick++)
                {
                    Tick(facing,new CombatInput{move=facing});
                    Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral),page.id);
                    AssertNovemberFigure(page);
                    string current=art.sprite.name;
                    if(current!=previous){visits[current]++;previous=current;}
                }
                Assert.That(fighter.Model.Clock,Is.GreaterThan(clock+1.7f),page.id+": advance the real model clock");
                foreach(var visit in visits)Assert.That(visit.Value,Is.GreaterThanOrEqualTo(2),page.id+": repeated pose "+visit.Key);
                figures+=visits.Count;
                var idle=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==page.weapon&&p.direction==page.direction&&p.action=="Idle");
                for(int tick=0;tick<100;tick++){Tick(facing);if(idle.spriteNames.Contains(art.sprite.name))break;}
                AssertAuthored(idle);Assert.That(art.color,Is.EqualTo(Color.white));
                AssertSilent();yield return null;
            }
            Assert.That(figures,Is.EqualTo(28));
            Report("november-20261102-walk",7,figures,"Temporary quality 0 integration check, not FullGraphicQA: seven new November 2 Walk clips looped all four whole poses through the real model clock and CharacterView, then returned to their directional Idle. Other clips are not counted.");
        }

        [UnityTest] public IEnumerator NovemberEightPesadoClipsPreservePhasesAndContactTiming()
        {
            int figures=0;
            foreach(var page in NovemberPages().Where(p=>p.action=="Attack"))
            {
                Context(0,0,0,page.direction);var facing=Facing(page.direction);
                var attack=Attacks(fighter.loadout).Single(a=>a.displayName=="Pesado");
                Assert.That(attack.startup,Is.EqualTo(.10f).Within(.00001f));
                Assert.That(attack.active,Is.EqualTo(.11f).Within(.00001f));
                Assert.That(attack.recovery,Is.EqualTo(.20f).Within(.00001f));
                float startup=attack.startup,active=attack.active,recovery=attack.recovery;
                Tick(facing,new CombatInput{heavyPressed=true,heavyHeld=true});
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Charging),page.id);
                Tick(facing,new CombatInput{heavyReleased=true});
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Attack),page.id);
                Assert.That(fighter.Model.Attack,Is.SameAs(attack),page.id+": short heavy hold must release Pesado");
                var seen=new HashSet<string>();bool contact=false;int ticks=0;
                while(fighter.Model.State==CombatState.Attack&&ticks++<100)
                {
                    Present();AssertNovemberFigure(page);seen.Add(art.sprite.name);
                    float t=fighter.Model.StateTime;
                    int expected=t<startup?0:t<startup+active?1:t<startup+active+recovery*.5f?2:3;
                    Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames[expected]),page.id+": phase at "+t);
                    Tick(facing);
                    // Combat contact is an interval crossing, including the tick that leaves active.
                    bool expectedContact=t<startup+active&&fighter.Model.StateTime>=startup;
                    Assert.That(fighter.Model.ActiveThisTick,Is.EqualTo(expectedContact),page.id+": contact at "+t);
                    contact|=fighter.Model.ActiveThisTick;
                }
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral),page.id);
                Assert.That(contact,Is.True,page.id);
                Assert.That(seen,Is.EquivalentTo(page.spriteNames),page.id);
                Assert.That(attack.startup,Is.EqualTo(startup));Assert.That(attack.active,Is.EqualTo(active));Assert.That(attack.recovery,Is.EqualTo(recovery));
                // Clear the final interval flag and require the exact directional Idle.
                Tick(facing);Present();
                var idle=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==0&&p.direction==page.direction&&p.action=="Idle");
                AssertAuthored(idle);Assert.That(art.color,Is.EqualTo(Color.white));
                figures+=seen.Count;AssertSilent();yield return null;
            }
            Assert.That(figures,Is.EqualTo(32));
            Report("november-20261102-pesado",8,figures,"Temporary quality 0 integration check, not FullGraphicQA: eight new November 2 Pesado clips rendered all startup/contact/two recovery poses through heavy press/short hold/release in the real combat model and CharacterView; 100/110/200ms attack timing and interval contact preserved, followed by directional Idle.");
        }
    }
}



