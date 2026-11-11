using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightLuciusSpriteBankTests
    {
        static DarklightLuciusSpriteCatalog Valid()=>new(){schemaVersion=1,characterId="Lucius",form="Normal",weapon="Sword",complete=false,
            pages=Enumerable.Range(0,8).Select(d=>new DarklightLuciusSpritePage{
                id="LuciusNormalIdle8V1_Idle_"+d,action="Idle",direction=d,
                resourcePath=DarklightLuciusSpriteBank.Root+"LuciusNormalIdle8V1",
                imageSha256=new string('a',64),sourceSha256=new string('b',64),nativeSize=new[]{1024,1024},
                pixelsPerUnit=128,spriteNames=new[]{"Redesign_LuciusNormalIdle8V1_"+d.ToString("00")},
                durationMs=new[]{350},loop=true,wholeFigures=true,status="visual-reviewed"}).ToArray()};
        [TearDown] public void Clear()=>DarklightLuciusSpriteBank.ClearCache();
        [Test] public void EightReviewedIdleViewsActivateWithoutClaimingCompleteKit()
        {var data=Valid();Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.True);Assert.That(data.complete,Is.False);}
        [Test] public void DuplicateContextsAndIdsAreRejected()
        {
            var data=Valid();data.pages[7].direction=6;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[7].id=data.pages[6].id;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages=data.pages.Take(7).ToArray();Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [TestCase("Lucien","Normal","Sword")][TestCase("Lucius","Fenrir","Sword")][TestCase("Lucius","Normal","Katana")]
        public void CharacterFormAndWeaponCannotAddressOtherBanks(string identity,string form,string weapon)
        {var data=Valid();data.characterId=identity;data.form=form;data.weapon=weapon;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);}
        [TestCase("Darklight/SecondaryCharacters/Lucien")][TestCase("Darklight/LuciusNormal20261109/../Lucien")]
        [TestCase("Darklight/LuciusNormal20261109/Sheet.png")][TestCase("Darklight/LuciusNormal20261109/")]
        public void ResourcePathsStayWithinTheirOwnNormalizedPacket(string path)
        {var data=Valid();data.pages[0].resourcePath=path;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);}
        [Test] public void ReviewHashesWholeFiguresAndNormalizedNamesAreRequired()
        {
            var data=Valid();data.complete=true;data.pages[0].status="candidate";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False,"complete cannot approve candidate art");
            data=Valid();data.pages[0].wholeFigures=false;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].sourceSha256="missing";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].imageSha256=new string('z',64);Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].spriteNames[0]="Redesign_Lucien_00";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].id="Lucius Idle";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [Test] public void DirectionsDensitiesTimingsAndActionsAreValidated()
        {
            var data=Valid();data.pages[0].direction=8;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].pixelsPerUnit=float.NaN;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].pixelsPerUnit=0;Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].durationMs=new[]{0};Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].durationMs=Array.Empty<int>();Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
            data=Valid();data.pages[0].action="Unknown";Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [Test] public void SharedFrameClockLoopsOrHoldsLastNativePose()
        {
            var page=Valid().pages[0];page.spriteNames=new[]{"Redesign_Lucius_A","Redesign_Lucius_B"};page.durationMs=new[]{100,200};
            Assert.That(DarklightLuciusSpriteBank.Frame(page,.15f),Is.EqualTo(1));
            Assert.That(DarklightLuciusSpriteBank.Frame(page,.35f),Is.Zero);
            page=Valid().pages[0];page.spriteNames=new[]{"Redesign_Lucius_A","Redesign_Lucius_B"};page.durationMs=new[]{100,200};page.loop=false;
            Assert.That(DarklightLuciusSpriteBank.Frame(page,10),Is.EqualTo(1));
        }
        [Test] public void InitialResourcesResolveEightNativeIdleViewsAndReportFallback()
        {
            var text=Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root+"catalog");Assert.That(text,Is.Not.Null);
            var data=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.True);
            var pages=data.pages.Where(p=>p.action=="Idle").ToArray();Assert.That(pages,Has.Length.EqualTo(8));
            foreach(var page in pages)
            {
                Assert.That(page.spriteNames,Has.Length.EqualTo(1),"Initial kit provides static views, not an invented idle cycle");
                Assert.That(DarklightLuciusSpriteBank.TryPose("Idle",page.direction,5,out var idle,out bool fallback),Is.True);
                Assert.That(fallback,Is.False);Assert.That(idle.name,Is.EqualTo(page.spriteNames[0]));
                Assert.That(idle.texture.width,Is.EqualTo(page.nativeSize[0]));Assert.That(idle.texture.height,Is.EqualTo(page.nativeSize[1]));
                Assert.That(idle.pixelsPerUnit,Is.EqualTo(page.pixelsPerUnit).Within(.001f));
                Assert.That(DarklightLuciusSpriteBank.TryPose("Walk",page.direction,1,out var walk,out fallback),Is.True);
                bool hasWalk=DarklightLuciusSpriteBank.TryActionPage(DarklightLuciusAppearance.Normal,"Walk",page.direction,null,out _);
                Assert.That(fallback,Is.EqualTo(!hasWalk));
                if(!hasWalk)Assert.That(walk,Is.SameAs(idle));
            }
        }
        [Test] public void WalkArmedPageValidatesWithLoopAndRejectsInvalidContracts()
        {
            var page = new DarklightLuciusSpritePage
            {
                id = "LuciusWalkArmedSSW20261111V1_WalkArmed_S",
                action = "WalkArmed",
                direction = 0,
                resourcePath = DarklightLuciusSpriteBank.Root + "LuciusWalkArmedSSW20261111V1",
                imageSha256 = new string('a', 64),
                sourceSha256 = new string('b', 64),
                nativeSize = new[] { 1536, 1024 },
                pixelsPerUnit = 214.97f,
                spriteNames = new[] { "Redesign_LuciusWalkArmed_00", "Redesign_LuciusWalkArmed_01" },
                durationMs = new[] { 140, 130 },
                loop = true,
                wholeFigures = true,
                status = "visual-reviewed"
            };
            Assert.That(DarklightLuciusSpriteBank.ValidPage(page), Is.True, "WalkArmed must allow loop=true for continuous locomotion");

            page.attackKey = "SwordHorizontal";
            Assert.That(DarklightLuciusSpriteBank.ValidPage(page), Is.False, "Locomotion actions cannot carry an attackKey");
            page.attackKey = null;

            page.action = "WalkArmedInvalid";
            Assert.That(DarklightLuciusSpriteBank.ValidPage(page), Is.False, "Unknown action must be rejected");
        }
    }

    // Test-only marker sequences reuse imported whole Idle views. They are not
    // production animation pages, art approval, or evidence of a finished kit.
    [Parallelizable(ParallelScope.None)]
    public sealed class DarklightLuciusPresentationIncrementalTests
    {
        readonly System.Collections.Generic.List<UnityEngine.Object> owned=new();
        DarklightLuciusSpriteCatalog data;
        DarklightLuciusAppearance appearance;
        static readonly string[] Keys={DarklightLuciusCombatProfile.SwordHorizontal,
            DarklightLuciusCombatProfile.SwordVertical,DarklightLuciusCombatProfile.SwordThrust};
        [SetUp] public void Prepare()
        {
            appearance=DarklightLuciusAppearance.Normal;
            DarklightLuciusSpriteBank.ClearCache();
            var text=Resources.Load<TextAsset>(DarklightLuciusSpriteBank.Root+"catalog");
            Assert.That(text,Is.Not.Null);
            data=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(text.text);
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data,appearance),Is.True);
            Assert.That(data.pages.Count(p=>p.action=="Idle"),Is.EqualTo(8));
            data.pages=data.pages.Where(p=>p.action=="Idle").ToArray();
            const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
            var catalogs=(System.Collections.Generic.Dictionary<DarklightLuciusAppearance,DarklightLuciusSpriteCatalog>)
                typeof(DarklightLuciusSpriteBank).GetField("catalogs",flags).GetValue(null);
            var attempted=(System.Collections.Generic.HashSet<DarklightLuciusAppearance>)
                typeof(DarklightLuciusSpriteBank).GetField("attempted",flags).GetValue(null);
            catalogs[appearance]=data;attempted.Add(appearance);
        }
        [TearDown] public void Dispose()
        {
            DarklightLuciusSpriteBank.ClearCache();
            foreach(var item in owned)if(item!=null)UnityEngine.Object.DestroyImmediate(item);
            owned.Clear();
        }
        DarklightLuciusSpritePage Page(string action,int direction,string key=null,int offset=0)
        {
            var template=data.pages.First(p=>p.action=="Idle"&&p.direction==direction);
            var page=JsonUtility.FromJson<DarklightLuciusSpritePage>(JsonUtility.ToJson(template));
            page.id="LuciusTest_"+action+"_"+direction+"_"+(key??"None");
            page.action=action;page.attackKey=key;page.loop=action=="Idle"||action=="Walk";
            var markers=data.pages.Where(p=>p.action=="Idle").OrderBy(p=>p.direction).ToArray();
            page.spriteNames=Enumerable.Range(0,3).Select(i=>markers[(i+offset)%8].spriteNames[0]).ToArray();
            page.durationMs=new[]{100,100,100};
            return page;
        }
        void Install(params DarklightLuciusSpritePage[] pages)
        {
            data.pages=data.pages.Where(p=>!pages.Any(n=>n.action==p.action&&n.direction==p.direction&&(n.attackKey??"")==(p.attackKey??"")))
                .Concat(pages).ToArray();
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data,appearance),Is.True);
            // Keep production API small; inject only the test catalog, not gameplay state.
            const System.Reflection.BindingFlags flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
            // EDITH reconciliation: the hooded increment has one cache per appearance.
            var catalogs=(System.Collections.Generic.Dictionary<DarklightLuciusAppearance,DarklightLuciusSpriteCatalog>)
                typeof(DarklightLuciusSpriteBank).GetField("catalogs",flags).GetValue(null);
            var attempted=(System.Collections.Generic.HashSet<DarklightLuciusAppearance>)
                typeof(DarklightLuciusSpriteBank).GetField("attempted",flags).GetValue(null);
            catalogs[appearance]=data;attempted.Add(appearance);
        }
        CombatLoadout Loadout()
        {
            var bases=Keys.Select(key=>{
                var attack=ScriptableObject.CreateInstance<AttackDefinition>();owned.Add(attack);
                attack.startup=.2f;attack.active=.1f;attack.recovery=.3f;
                return attack;
            }).ToArray();
            return DarklightLuciusCombatProfile.Create(bases,owned.Add);
        }
        DarklightLuciusView View()
        {
            var go=new GameObject("Lucius isolated presentation test");owned.Add(go);
            return go.AddComponent<DarklightLuciusView>();
        }
        static Vector2 Facing(int direction)
        {float angle=direction*Mathf.PI/4;return new Vector2(-Mathf.Sin(angle),-Mathf.Cos(angle));}
        static void Expect(DarklightLuciusView view,CombatModel model,DarklightLuciusSpritePage page,int index,Vector2 movement=default)
        {
            float clock=model.Clock,time=model.StateTime;int serial=model.AttackSerial;var state=model.State;
            Assert.That(view.Present(model,movement),Is.True);
            Assert.That(view.UsesIdleFallback,Is.False);
            Assert.That(view.Renderer.sprite.name,Is.EqualTo(page.spriteNames[index]));
            Assert.That(model.Clock,Is.EqualTo(clock));Assert.That(model.StateTime,Is.EqualTo(time));
            Assert.That(model.AttackSerial,Is.EqualTo(serial));Assert.That(model.State,Is.EqualTo(state));
        }
        [Test] public void ThreeKeysCoexistForEveryDirectionAndDuplicateTechniqueIsRejected()
        {
            var attacks=Enumerable.Range(0,8).SelectMany(d=>Keys.Select((key,i)=>Page("Attack",d,key,i*2))).ToArray();
            data.pages=data.pages.Concat(attacks).ToArray();
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data,appearance),Is.True);
            data.pages=data.pages.Concat(new[]{Page("Attack",0,Keys[0])}).ToArray();
            data.pages[data.pages.Length-1].id+="_Duplicate";
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False);
        }
        [TestCase(null)][TestCase("")][TestCase("Leve1")][TestCase("SwordUnknown")]
        public void GenericOrForeignAttackPagesAreRejected(string key)
        {Assert.That(DarklightLuciusSpriteBank.ValidPage(Page("Attack",0,key)),Is.False);}
        [Test] public void NonAttackPagesCannotHideAnAttackKey()
        {Assert.That(DarklightLuciusSpriteBank.ValidPage(Page("Walk",0,Keys[0])),Is.False);}

        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)]
        [TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void ModelSelectsAllThreeAttacksWithoutCrossTechniqueFallback(int direction)
        {
            var pages=Keys.Select((key,i)=>Page("Attack",direction,key,i*2)).ToArray();Install(pages);
            var loadout=Loadout();var view=View();
            // Light, heavy release and counter are genuine inputs for the three aliases.
            for(int technique=0;technique<3;technique++)
            {
                var model=new CombatModel(loadout);var aim=Facing(direction);model.Tick(1,default);
                if(technique==0)model.Tick(.01f,new CombatInput{lightPressed=true,aim=aim});
                else if(technique==1)
                {
                    model.Tick(.01f,new CombatInput{heavyPressed=true,heavyHeld=true,aim=aim});
                    model.Tick(.01f,new CombatInput{heavyReleased=true,aim=aim});
                }
                else
                {
                    model.Tick(.01f,new CombatInput{parryPressed=true,aim=aim});
                    Assert.That(model.Defend(),Is.True);model.Tick(.27f,default);
                    model.Tick(.01f,new CombatInput{lightPressed=true,aim=aim});
                }
                Assert.That(model.State,Is.EqualTo(CombatState.Attack));
                Assert.That(model.Attack.animationKey,Is.EqualTo(Keys[technique]));
                Expect(view,model,pages[technique],0);
                model.Tick(.15f,default);Expect(view,model,pages[technique],1);
                model.Tick(0,default);Expect(view,model,pages[technique],1);
            }
        }
        [Test] public void MissingKeyReportsIdleRatherThanPresentingAnotherTechnique()
        {
            Install(Page("Attack",0,Keys[0]));var view=View();
            var idle=data.pages.First(p=>p.action=="Idle"&&p.direction==0).spriteNames[0];
            foreach(string key in new[]{Keys[1],"Unknown",null})
            {
                Assert.That(view.PresentPose("Attack",0,.15f,key),Is.True);
                Assert.That(view.UsesIdleFallback,Is.True);Assert.That(view.Renderer.sprite.name,Is.EqualTo(idle));
            }
        }
        [Test] public void ComboRestartsTheExistingStateClockForEachTechnique()
        {
            var pages=Keys.Select((key,i)=>Page("Attack",0,key,i*2)).ToArray();Install(pages);
            var model=new CombatModel(Loadout());var view=View();
            model.Tick(.01f,new CombatInput{lightPressed=true});
            for(int i=0;i<3;i++)
            {
                Assert.That(model.Attack.animationKey,Is.EqualTo(Keys[i]));
                Assert.That(model.AttackSerial,Is.EqualTo(i+1));Expect(view,model,pages[i],0);
                model.Tick(.15f,default);Expect(view,model,pages[i],1);
                if(i==2)break;
                // Cross the contact once, then queue a genuine recovery chain.
                model.Tick(.26f,default);Assert.That(model.ActiveThisTick,Is.True);
                model.Tick(.01f,new CombatInput{lightPressed=true});
            }
        }
        [TestCase("Walk")][TestCase("Dodge")][TestCase("Parry")]
        [TestCase("Charging")][TestCase("Hurt")][TestCase("Dead")]
        public void MissingEssentialPageIsReportedAsIdleFallback(string action)
        {
            // Remove this action only from the injected copy; real catalog stays intact.
            data.pages=data.pages.Where(p=>p.action!=action).ToArray();Install();
            var view=View();Assert.That(view.PresentPose(action,0,.15f),Is.True);
            Assert.That(view.UsesIdleFallback,Is.True);
            Assert.That(view.Renderer.sprite.name,Is.EqualTo(data.pages.First(p=>p.action=="Idle"&&p.direction==0).spriteNames[0]));
        }
        [TestCase("Idle")][TestCase("Walk")][TestCase("Dodge")][TestCase("Parry")]
        [TestCase("Charging")][TestCase("Hurt")][TestCase("Dead")]
        public void EssentialStatesUseExistingClockAndNeverAdvanceItFromRendering(string action)
        {
            var page=Page(action,0);Install(page);var model=new CombatModel(Loadout());var view=View();
            Vector2 movement=action=="Walk"?Vector2.down:Vector2.zero;
            if(action=="Idle"||action=="Walk")model.Tick(.15f,new CombatInput{move=movement});
            else
            {
                model.Tick(1,default);
                if(action=="Dodge")model.Tick(.01f,new CombatInput{dodgePressed=true,move=Vector2.down});
                else if(action=="Parry")model.Tick(.01f,new CombatInput{parryPressed=true});
                else if(action=="Charging")model.Tick(.01f,new CombatInput{heavyPressed=true,heavyHeld=true});
                else model.Hurt(action=="Dead");
                Expect(view,model,page,0);
                model.Tick(.15f,new CombatInput{heavyHeld=action=="Charging"});
            }
            if(action=="Walk")
            {
                Expect(view,model,page,0,movement);
                view.transform.position+=Vector3.down*(.15f*2.3f);
            }
            Expect(view,model,page,1,movement);Expect(view,model,page,1,movement);
            model.Tick(0,new CombatInput{heavyHeld=action=="Charging"});Expect(view,model,page,1,movement);
            if(action=="Dead")
            {
                float clock=model.Clock;model.Tick(.5f,default);Expect(view,model,page,2);
                Assert.That(model.Clock,Is.EqualTo(clock));
            }
            else if(action=="Dodge"||action=="Parry"||action=="Hurt")
            {
                model.Tick(.3f,default);Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
                Assert.That(view.Present(model),Is.True);Assert.That(view.UsesIdleFallback,Is.False);
                Assert.That(view.Renderer.sprite.name,Is.EqualTo(data.pages.First(p=>p.action=="Idle"&&p.direction==0).spriteNames[0]));
            }
        }

        [Test] public void HoodedAttackKeysStayInTheirOwnAppearanceWhenPreviewingNormal()
        {
            appearance=DarklightLuciusAppearance.Hooded;
            data=JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(Resources.Load<TextAsset>(DarklightLuciusSpriteBank.HoodedRoot+"catalog").text);
            var pages=Keys.Select((key,i)=>Page("Attack",0,key,i*2)).ToArray();Install(pages);
            var view=View();view.appearance=appearance;
            for(int i=0;i<3;i++)
            {
                Assert.That(view.PresentPose("Attack",0,.15f,Keys[i]),Is.True);
                Assert.That(view.UsesIdleFallback,Is.False);
                Assert.That(view.Renderer.sprite.name,Is.EqualTo(pages[i].spriteNames[1]));
            }
            view.appearance=DarklightLuciusAppearance.Normal;
            Assert.That(view.PresentPose("Attack",0,.15f,Keys[0]),Is.True);
            Assert.That(view.UsesIdleFallback,Is.True);
            Assert.That(view.Renderer.sprite.name,Does.StartWith("Redesign_LuciusNormal"));
            view.appearance=appearance;
            Assert.That(view.PresentPose("Attack",0,.15f,"Unknown"),Is.True);
            Assert.That(view.UsesIdleFallback,Is.True);
            Assert.That(view.Renderer.sprite.name,Does.StartWith("Redesign_LuciusHooded"));
        }
    }
}
