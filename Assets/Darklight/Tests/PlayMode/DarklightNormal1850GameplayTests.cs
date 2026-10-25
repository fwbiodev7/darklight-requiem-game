using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        const string Review1850="outputs/redesign-armas-20261025-retomada1850/gameplay";

        [UnityTest] public IEnumerator KatanaSecondLightUsesItsDeclaredFiguresAfterConfirmedFirstContact()
        {
            int contexts=0,figures=0;
            for(int skin=0;skin<2;skin++)for(int direction=0;direction<8;direction++)
            {
                var page=catalog.pages.Single(p=>p.skin==skin&&p.weapon==0&&p.form==0&&p.direction==direction&&p.action=="Attack"&&p.attackKey=="Leve2");
                Context(skin,0,0,direction);Tick(Facing(direction),new CombatInput{lightPressed=true});
                for(int tick=0;tick<160&&fighter.Model.Attack?.displayName!="Leve2";tick++)
                {
                    if(fighter.Model.ActiveThisTick)fighter.Model.ConfirmHit();
                    bool chain=fighter.Model.AttackConnected&&!fighter.Model.ActiveThisTick;
                    Tick(Facing(direction),new CombatInput{lightPressed=chain});
                }
                Assert.That(fighter.Model.Attack?.displayName,Is.EqualTo("Leve2"),"Second light must follow the real combo transition");
                var seen=new HashSet<string>(StringComparer.Ordinal);bool contact=false;
                for(int tick=0;tick<160&&fighter.Model.State==CombatState.Attack;tick++)
                {
                    Present(fighter.Model.Motion);AssertAuthored(page);seen.Add(art.sprite.name);
                    if(fighter.Model.ActiveThisTick&&!contact){if(direction==0||direction==3)Capture("1850-katana-light2-s"+skin+"-d"+direction);contact=true;}
                    Tick(Facing(direction));
                }
                Assert.That(contact,Is.True);Assert.That(seen.Count,Is.EqualTo(page.spriteNames.Distinct().Count()),page.id);
                contexts++;figures+=seen.Count;AssertSilent();yield return null;
            }
            Directory.CreateDirectory(Review1850);
            File.WriteAllText(Path.Combine(Review1850,"katana-second-light-result.json"),
                "{\"contexts\":"+contexts+",\"observedFigures\":"+figures+",\"firstContactSeededByConfirmHit\":true,\"campaignRendered\":true,\"audio\":false}");
        }

        [UnityTest] public IEnumerator CurrentNormalFirstLightsUseDeclaredArtAndReturnToIdle()
        {
            int checkedContexts=0,pending=0,observedFigures=0;
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int direction=0;direction<8;direction++)
            {
                Context(skin,weapon,0,direction);Tick(Facing(direction),new CombatInput{lightPressed=true});
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Attack));
                string key=fighter.Model.Attack.displayName;
                var page=catalog.pages.SingleOrDefault(p=>p.skin==skin&&p.weapon==weapon&&p.form==0&&p.direction==direction&&p.action=="Attack"&&p.attackKey==key);
                if(page==null){pending++;yield return null;continue;}
                var seen=new HashSet<string>(StringComparer.Ordinal);bool contact=false;
                for(int tick=0;tick<160&&fighter.Model.State==CombatState.Attack;tick++)
                {
                    Present(fighter.Model.Motion);AssertAuthored(page);seen.Add(art.sprite.name);
                    if(fighter.Model.ActiveThisTick&&!contact)
                    {
                        if(direction==0||direction==3)Capture("1850-first-light-s"+skin+"-w"+weapon+"-d"+direction);
                        contact=true;
                    }
                    Tick(Facing(direction));
                }
                Assert.That(contact,Is.True,page.id);Assert.That(seen.Count,Is.EqualTo(page.spriteNames.Distinct().Count()),page.id);
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral));Present();
                bool idleDeclared=catalog.pages.Any(p=>p.skin==skin&&p.weapon==weapon&&p.form==0&&p.direction==direction&&p.action=="Idle");
                Assert.That(view.UsesWeaponRedesignSprite,Is.EqualTo(idleDeclared),"Return to idle must match declared coverage");
                checkedContexts++;observedFigures+=seen.Count;AssertSilent();yield return null;
            }
            Directory.CreateDirectory(Review1850);
            File.WriteAllText(Path.Combine(Review1850,"normal-first-lights-result.json"),
                "{\"declaredContextsVerified\":"+checkedContexts+",\"pendingContextsWithoutNewAttackArt\":"+pending+",\"distinctObservedFigures\":"+observedFigures+",\"campaignRendered\":true,\"audio\":false}");
        }

        [UnityTest] public IEnumerator CurrentNormalIdlePagesReachTheCampaignInBothSkins()
        {
            int contexts=0,figures=0;
            foreach(var page in catalog.pages.Where(p=>p.form==0&&p.action=="Idle"))
            {
                Context(page.skin,page.weapon,0,page.direction);
                var observed=new HashSet<string>(StringComparer.Ordinal);
                for(int tick=0;tick<155;tick++)
                {
                    Tick(Facing(page.direction));AssertAuthored(page);observed.Add(art.sprite.name);
                }
                Assert.That(observed.Count,Is.EqualTo(page.spriteNames.Distinct().Count()),page.id);
                contexts++;figures+=observed.Count;
                if(page.direction==0)Capture("1850-normal-idle-skin"+page.skin+"-weapon"+page.weapon);
                AssertSilent();yield return null;
            }
            Directory.CreateDirectory(Review1850);
            File.WriteAllText(Path.Combine(Review1850,"normal-idle-result.json"),
                "{\"contexts\":"+contexts+",\"distinctObservedFigures\":"+figures+",\"campaignRendered\":true,\"isolatedSave\":true,\"audio\":false,\"humanPlay\":false}");
        }

        [UnityTest] public IEnumerator RockKatanaFirstLightUsesItsNewPhasesInAllEightDirections()
        {
            int figures=0;
            for(int direction=0;direction<8;direction++)
            {
                var page=catalog.pages.Single(p=>p.skin==1&&p.weapon==0&&p.form==0&&p.direction==direction&&p.action=="Attack"&&p.attackKey=="Leve1");
                Context(1,0,0,direction);Tick(Facing(direction),new CombatInput{lightPressed=true});
                Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Attack));
                var observed=new HashSet<string>(StringComparer.Ordinal);bool contact=false;
                for(int tick=0;tick<160&&fighter.Model.State==CombatState.Attack;tick++)
                {
                    Present(fighter.Model.Motion);AssertAuthored(page);observed.Add(art.sprite.name);
                    if(fighter.Model.ActiveThisTick&&!contact){Capture("1850-rock-katana-d"+direction+"-contact");contact=true;}
                    Tick(Facing(direction));
                }
                Assert.That(contact,Is.True);Assert.That(observed.Count,Is.EqualTo(page.spriteNames.Distinct().Count()));
                figures+=observed.Count;Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral));
                Present();Assert.That(view.UsesWeaponRedesignSprite,Is.True,"Authored Rock idle must resume after its first light");
                AssertSilent();yield return null;
            }
            Directory.CreateDirectory(Review1850);
            File.WriteAllText(Path.Combine(Review1850,"rock-katana-first-light-result.json"),
                "{\"directions\":8,\"distinctObservedFigures\":"+figures+",\"campaignRendered\":true,\"contactAndIdleReturn\":true,\"audio\":false}");
        }
    }
}
