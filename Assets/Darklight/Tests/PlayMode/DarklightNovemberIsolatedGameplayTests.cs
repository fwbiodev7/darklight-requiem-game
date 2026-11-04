using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;

namespace Darklight.Tests
{
    public sealed partial class DarklightNovemberIsolatedGameplayTests
    {
        GameObject actor;
        DarklightCharacterView view;
        SpriteRenderer art;
        CombatModel model;
        DarklightWeaponRedesignCatalog catalog;
        DarklightCombatProfiles profiles;
        WeaponDefinition[] weapons;
        WeaponDefinition weapon;
        readonly Dictionary<UnityEngine.Object,string> originals=new();
        readonly Dictionary<string,Sheet> manifests=new();
        HashSet<DarklightCombatFX> priorFx;
        bool priorSilent,priorPause;float priorVolume;

        [Serializable] sealed class Sheet { public Frame[] frames; }
        [Serializable] sealed class Frame { public string id;public int[] rect;public float[] pivot; }

        [SetUp] public void SetupIsolatedRuntime()
        {
            priorSilent=DarklightAutomation.Silent;priorPause=AudioListener.pause;priorVolume=AudioListener.volume;
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            priorFx=new HashSet<DarklightCombatFX>(UnityEngine.Object.FindObjectsByType<DarklightCombatFX>(FindObjectsSortMode.None));
            Assert.That(DarklightCampaign.Instance,Is.Null,"Isolated fixture must start without the campaign scene");
            Assert.That(DarklightCombatLab.Instance,Is.Null,"Isolated fixture must start without the combat lab");
            var names=new[]{"Requiem","PresasDoVeu","LacoDaRuptura"};
            var source=names.Select(n=>AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/"+n+".asset")).ToArray();
            foreach(var item in source)Assert.That(item,Is.Not.Null);
            foreach(var item in EditorUtility.CollectDependencies(source.Cast<UnityEngine.Object>().ToArray()))
                if(item is WeaponDefinition||item is AttackDefinition)originals[item]=EditorJsonUtility.ToJson(item);
            profiles=new DarklightCombatProfiles();weapons=profiles.Create(source);
            var text=Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root+"catalog");Assert.That(text,Is.Not.Null);
            catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(text.text);
            Assert.That(DarklightWeaponRedesignBank.ValidCatalog(catalog),Is.True);DarklightWeaponRedesignBank.ClearCache();
            actor=new GameObject("NovemberIsolatedActor");
            var visual=new GameObject("FabioVisual");visual.transform.SetParent(actor.transform,false);
            art=visual.AddComponent<SpriteRenderer>();view=actor.AddComponent<DarklightCharacterView>();
            TestContext.Progress.WriteLine("isolatedRuntime=true; campaignSceneValidated=false; graphicsValidated=false; real CharacterView/CombatModel and imported assets, no campaign scene/camera/URP/map/HUD, no preferences or saves written.");
        }

        [TearDown] public void CleanupIsolatedRuntime()
        {
            try
            {
                AssertSilent();
                foreach(var item in originals)Assert.That(EditorJsonUtility.ToJson(item.Key),Is.EqualTo(item.Value),item.Key.name+": authored original must remain unchanged");
            }
            finally
            {
                if(actor!=null)UnityEngine.Object.DestroyImmediate(actor);
                if(priorFx!=null)foreach(var fx in UnityEngine.Object.FindObjectsByType<DarklightCombatFX>(FindObjectsSortMode.None))
                    if(!priorFx.Contains(fx))UnityEngine.Object.DestroyImmediate(fx.gameObject);
                profiles?.Dispose();profiles=null;originals.Clear();manifests.Clear();DarklightWeaponRedesignBank.ClearCache();
                DarklightAutomation.Silent=priorSilent;AudioListener.pause=priorPause;AudioListener.volume=priorVolume;
            }
        }

        static Vector2 Facing(int direction)=>(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);
        void Context(int skin,int weaponIndex,int form,int direction)
        {
            Assert.That(skin,Is.Zero);Assert.That(form,Is.Zero);
            weapon=weapons[weaponIndex];model=new CombatModel(weapon.loadout);
            model.Tick(.001f,new CombatInput{aim=Facing(direction)});Present();
            Assert.That(DarklightCharacterView.DirectionIndex(model.Facing),Is.EqualTo(direction));
        }
        void Present(Vector2 movement=default)
        {
            view.PresentPreview(model,movement,0,0,0,weapon);
            typeof(DarklightCharacterView).GetMethod("LateUpdate",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(view,null);
        }
        void Tick(Vector2 facing,CombatInput input=default)
        {input.aim=facing;model.FormStyle=0;model.Tick(.01f,input,1);Present(model.Motion);}
        static void AssertSilent()
        {
            Assert.That(DarklightAutomation.Silent,Is.True);Assert.That(AudioListener.pause,Is.True);Assert.That(AudioListener.volume,Is.Zero);
            foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False,source.name);
        }
        static void Report(string key,int contexts,int figures,string message)=>TestContext.Progress.WriteLine(
            key+": contexts="+contexts+"; figures="+figures+"; isolatedRuntime=true; campaignSceneValidated=false; graphicsValidated=false; "+message);
        void AssertAuthored(DarklightWeaponRedesignPage page)
        {
            Assert.That(view.UsesWeaponRedesignSprite,Is.True,page.id);
            Assert.That(art.sprite,Is.Not.Null,page.id);Assert.That(page.spriteNames,Does.Contain(art.sprite.name),page.id);
            Assert.That(art.flipX,Is.False,page.id);
            var block=new MaterialPropertyBlock();art.GetPropertyBlock(block);
            Assert.That(block.GetFloat("_VampireForm"),Is.Zero);Assert.That(block.GetFloat("_WholeFrameId"),Is.Zero);
            Assert.That(block.GetFloat("_RuinFists"),Is.Zero);Assert.That(block.GetTexture("_VampireMask"),Is.SameAs(Texture2D.blackTexture));
            Assert.That(block.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
        }
        void AssertNativeFigure(DarklightWeaponRedesignPage page,Sprite sprite)
        {
            Assert.That(sprite.texture.width,Is.EqualTo(page.nativeSize[0]),page.id);
            Assert.That(sprite.texture.height,Is.EqualTo(page.nativeSize[1]),page.id);
            if(!manifests.TryGetValue(page.resourcePath,out var sheet))
            {var text=Resources.Load<TextAsset>(page.resourcePath);Assert.That(text,Is.Not.Null,page.id+": sheet manifest");manifests[page.resourcePath]=sheet=JsonUtility.FromJson<Sheet>(text.text);}
            var frame=sheet.frames.Single(f=>f.id==sprite.name);
            Assert.That(sprite.rect,Is.EqualTo(new Rect(frame.rect[0],page.nativeSize[1]-frame.rect[1]-frame.rect[3],frame.rect[2],frame.rect[3])),page.id);
            Assert.That(sprite.pivot.x,Is.EqualTo(frame.pivot[0]*frame.rect[2]).Within(.01f),page.id);
            Assert.That(sprite.pivot.y,Is.EqualTo(frame.pivot[1]*frame.rect[3]).Within(.01f),page.id);
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
            AssertAuthored(page);AssertNativeFigure(page,art.sprite);
            var legacyView=actor.GetComponent<DarklightOriginalVampireView>();
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
                    AssertNativeFigure(page,sprite);
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
            Report("november-20261102-catalog",15,60,"Imported raw sprites retain global PPU, native sheet dimensions, figure rectangles and pivots; rendered density is checked separately.");
            AssertSilent();yield return null;
        }

        [UnityTest] public IEnumerator NovemberSevenWalkClipsRepeatEveryPoseAndReturnToIdle()
        {
            int figures=0;
            foreach(var page in NovemberPages().Where(p=>p.action=="Walk"))
            {
                Context(0,page.weapon,0,page.direction);var facing=Facing(page.direction);
                var visits=page.spriteNames.ToDictionary(n=>n,n=>0);string previous=null;
                float clock=model.Clock;
                for(int tick=0;tick<180;tick++)
                {
                    Tick(facing,new CombatInput{move=facing});
                    Assert.That(model.State,Is.EqualTo(CombatState.Neutral),page.id);
                    AssertNovemberFigure(page);
                    string current=art.sprite.name;
                    if(current!=previous){visits[current]++;previous=current;}
                }
                Assert.That(model.Clock,Is.GreaterThan(clock+1.7f),page.id+": advance the real model clock");
                foreach(var visit in visits)Assert.That(visit.Value,Is.GreaterThanOrEqualTo(2),page.id+": repeated pose "+visit.Key);
                figures+=visits.Count;
                var idle=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==page.weapon&&p.direction==page.direction&&p.action=="Idle");
                for(int tick=0;tick<100;tick++){Tick(facing);if(idle.spriteNames.Contains(art.sprite.name))break;}
                AssertAuthored(idle);Assert.That(art.color,Is.EqualTo(Color.white));
                AssertSilent();yield return null;
            }
            Assert.That(figures,Is.EqualTo(28));
            Report("november-20261102-walk",7,figures,"Isolated runtime check, without campaign scene or graphical certification: seven new November 2 Walk clips looped all four whole poses through the real model clock and CharacterView, then returned to their directional Idle. Other clips are not counted.");
        }

        [UnityTest] public IEnumerator NovemberEightPesadoClipsPreservePhasesAndContactTiming()
        {
            int figures=0;
            foreach(var page in NovemberPages().Where(p=>p.action=="Attack"))
            {
                Context(0,0,0,page.direction);var facing=Facing(page.direction);
                var attack=weapon.loadout.heavy;Assert.That(attack.displayName,Is.EqualTo("Pesado"));
                Assert.That(attack.startup,Is.EqualTo(.10f).Within(.00001f));
                Assert.That(attack.active,Is.EqualTo(.11f).Within(.00001f));
                Assert.That(attack.recovery,Is.EqualTo(.20f).Within(.00001f));
                float startup=attack.startup,active=attack.active,recovery=attack.recovery;
                Tick(facing,new CombatInput{heavyPressed=true,heavyHeld=true});
                Assert.That(model.State,Is.EqualTo(CombatState.Charging),page.id);
                Tick(facing,new CombatInput{heavyReleased=true});
                Assert.That(model.State,Is.EqualTo(CombatState.Attack),page.id);
                Assert.That(model.Attack,Is.SameAs(attack),page.id+": short heavy hold must release Pesado");
                var seen=new HashSet<string>();bool contact=false;int ticks=0;
                while(model.State==CombatState.Attack&&ticks++<100)
                {
                    Present();AssertNovemberFigure(page);seen.Add(art.sprite.name);
                    float t=model.StateTime;
                    int expected=t<startup?0:t<startup+active?1:t<startup+active+recovery*.5f?2:3;
                    Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames[expected]),page.id+": phase at "+t);
                    Tick(facing);
                    // Combat contact is an interval crossing, including the tick that leaves active.
                    bool expectedContact=t<startup+active&&model.StateTime>=startup;
                    Assert.That(model.ActiveThisTick,Is.EqualTo(expectedContact),page.id+": contact at "+t);
                    contact|=model.ActiveThisTick;
                }
                Assert.That(model.State,Is.EqualTo(CombatState.Neutral),page.id);
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
            Report("november-20261102-pesado",8,figures,"Isolated runtime check, without campaign scene or graphical certification: eight new November 2 Pesado clips rendered all startup/contact/two recovery poses through heavy press/short hold/release in the real combat model and CharacterView; 100/110/200ms attack timing and interval contact preserved, followed by directional Idle.");
        }
    }
}




#endif



