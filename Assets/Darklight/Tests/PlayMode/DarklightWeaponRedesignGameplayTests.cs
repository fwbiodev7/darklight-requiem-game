using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace Darklight.Tests
{
    // Real campaign render/consumer checks, deterministically stepping its combat
    // model while paused. These do not certify human input, physical controls or sound.
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        const string Folder="outputs/retomada-20261027/unity-play";
        IDisposable storage;DarklightCampaign game;DarklightCombatLab lab;DarklightFighter fighter;
        DarklightCharacterView view;SpriteRenderer art;DarklightWeaponRedesignCatalog catalog;
        readonly Dictionary<string,string> protectedSaves=new();string preferences;
        static void Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        static string Hash(byte[] bytes){using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(bytes));}
        static string FileHash(string path)=>File.Exists(path)?Hash(File.ReadAllBytes(path)):"absent";
        static string Preferences()
        {
            var value=new StringBuilder();
            foreach(string key in new[]{"Darklight.Master","Darklight.Music","Darklight.Effects","Darklight.Ambient","Darklight.UI","Darklight.Enemies","Darklight.Player"})
                value.Append(key).Append(PlayerPrefs.HasKey(key)).Append(PlayerPrefs.GetFloat(key));
            foreach(string key in new[]{"Darklight.VisualQuality","Darklight.Flashes","Darklight.FrameRate","Darklight.Fullscreen","Darklight.Height","Darklight.Lessons","Darklight.Shake","Darklight.Skin","Darklight.Tips","Darklight.VSync","Darklight.Width"})
                value.Append(key).Append(PlayerPrefs.HasKey(key)).Append(PlayerPrefs.GetInt(key));
            value.Append(PlayerPrefs.HasKey("Darklight.Bindings")).Append(PlayerPrefs.GetString("Darklight.Bindings"));
            return Hash(Encoding.UTF8.GetBytes(value.ToString()));
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;preferences=Preferences();
            string realSave=DarklightProgress.Path;foreach(string path in new[]{realSave,realSave+".bak",realSave+".tmp"})protectedSaves[path]=FileHash(path);
            Directory.CreateDirectory(Folder);Directory.CreateDirectory("work/redesign-play-20261023");
            storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/redesign-play-20261023/"+Guid.NewGuid().ToString("N")+".json"));
            new DarklightProgress().Save();
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;fighter=lab.Player;view=fighter.GetComponent<DarklightCharacterView>();
            art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            game.EnterTraining();yield return null;yield return null;lab.SetPaused(true);fighter.enabled=false;
            foreach(var enemy in game.MapEnemies)if(enemy!=null)enemy.enabled=false;
            lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled=false;
            lab.WorldCamera.transform.position=fighter.transform.position+new Vector3(0,1,-10);lab.WorldCamera.orthographicSize=3.6f;
            var text=Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root+"catalog");Assert.That(text,Is.Not.Null);
            catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(text.text);
            Assert.That(DarklightWeaponRedesignBank.ValidCatalog(catalog),Is.True);DarklightWeaponRedesignBank.ClearCache();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            try
            {
                if(lab!=null)lab.SetPaused(true);if(fighter!=null)fighter.enabled=false;
                AssertSilent();Assert.That(Preferences(),Is.EqualTo(preferences),"User settings must remain unchanged");
                foreach(var file in protectedSaves)Assert.That(FileHash(file.Key),Is.EqualTo(file.Value),"User save must remain unchanged");
            }
            finally{storage?.Dispose();storage=null;DarklightWeaponRedesignBank.ClearCache();protectedSaves.Clear();}
            yield return null;
        }
        static Vector2 Facing(int direction)=>(Vector2)(Quaternion.Euler(0,0,-direction*45)*Vector2.down);
        void Context(int skin,int weapon,int form,int direction)
        {
            game.SelectSkin(skin,false);fighter.ResetModel();game.Weapons.Request(weapon);game.Weapons.BeforeTick(.01f,true);
            Assert.That((int)game.Weapons.Equipped.id,Is.EqualTo(weapon));
            Call(game,"SetTrainingForm",form);Call(game,"TickFormPresentation",2f);lab.SetPaused(true);fighter.ResetModel();
            Assert.That(game.PresentedForm,Is.EqualTo(form));
            fighter.Model.Tick(.001f,new CombatInput{aim=Facing(direction)});Present();
            Assert.That(DarklightCharacterView.DirectionIndex(fighter.Model.Facing),Is.EqualTo(direction));
        }
        void Present(Vector2 movement=default){view.Present(fighter.Model,movement);Call(view,"LateUpdate");}
        void Tick(Vector2 facing,CombatInput input=default)
        {input.aim=facing;fighter.Model.FormStyle=game.Form;fighter.Model.Tick(.01f,input,game.AttackSpeed);Present(fighter.Model.Motion);}
        void AssertAuthored(DarklightWeaponRedesignPage page)
        {
            Assert.That(view.UsesWeaponRedesignSprite,Is.True,page.id+": redesigned figure must reach CharacterView");
            Assert.That(page.spriteNames,Does.Contain(art.sprite.name));Assert.That(art.flipX,Is.False,"Every new direction was drawn explicitly");
            var block=new MaterialPropertyBlock();art.GetPropertyBlock(block);
            Assert.That(block.GetFloat("_VampireForm"),Is.Zero);Assert.That(block.GetFloat("_WholeFrameId"),Is.Zero);
            Assert.That(block.GetFloat("_RuinFists"),Is.Zero,"Authored whole figures must not inherit the Ruin Fists palette filter");
            Assert.That(block.GetTexture("_VampireMask"),Is.SameAs(Texture2D.blackTexture));Assert.That(block.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
            Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
        }
        static void AssertSilent()
        {
            Assert.That(DarklightAutomation.Silent,Is.True);Assert.That(AudioListener.pause,Is.True);Assert.That(AudioListener.volume,Is.Zero);
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False,source.name);
        }
        void Capture(string name)
        {
            Present(fighter.Model.Motion);Physics2D.SyncTransforms();
            var camera=lab.WorldCamera;var before=camera.targetTexture;var active=RenderTexture.active;
            var target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Folder,name+".png"),image.EncodeToPNG());
            }
            finally{camera.targetTexture=before;RenderTexture.active=active;target.Release();Object.Destroy(target);Object.Destroy(image);}
        }
        void Report(string name,int contexts,int figures,string details)
        {
            File.WriteAllText(Path.Combine(Folder,name+".txt"),
                "Date: 2026-10-23\nContexts checked: "+contexts+"\nDistinct observed frames: "+figures+"\n"+details+
                "\nActual campaign CharacterView and world camera rendered in PlayMode. Combat model stepped deterministically with gameplay paused.\nAudio disabled. Isolated test save. This is not human play, physical controller, all-combo, whole-redesign or listening certification.\n");
        }
        [UnityTest] public IEnumerator ArchivedDaggerIdleAssetsRemainReadableOutsideActiveSelectors()
        {
            int contexts=0,figures=0;
            foreach(int form in new[]{0,1})foreach(int direction in new[]{0,1,2})
            {
                var page=catalog.pages.Single(p=>p.skin==0&&p.weapon==1&&p.form==form&&p.direction==direction&&p.action=="Idle");
                var model=new CombatModel(new CombatLoadout());var seen=new HashSet<Sprite>();
                for(int tick=0;tick<155;tick++)
                {
                    model.Tick(.01f,default);
                    Assert.That(DarklightWeaponRedesignBank.TryResolve(0,1,form,direction,model,Vector2.zero,null,out var sprite),Is.True);
                    Assert.That(page.spriteNames,Does.Contain(sprite.name));seen.Add(sprite);
                }
                Assert.That(seen.Count,Is.EqualTo(4));contexts++;figures+=seen.Count;
                Assert.That(game.Weapons.Unlocked(1),Is.False,"Archive validation cannot reactivate Presas");yield return null;
            }
            Report("archived-dagger-idle",contexts,figures,"Legacy asset readability only; Presas is unavailable in active gameplay. This does not certify active character coverage.");
        }

        [UnityTest] public IEnumerator RequiemNormalIdleReturnsAfterEachWholeAttackSequence()
        {
            int contexts=0;
            foreach(int weapon in new[]{0,2})for(int form=0;form<3;form++)for(int direction=0;direction<8;direction++)
            {
                Context(0,weapon,form,direction);
                foreach(var attack in Attacks(fighter.loadout))
                {
                    fighter.ResetModel();Assert.That(fighter.Model.TryAirFollowup(attack,new CombatInput{aim=Facing(direction)}),Is.True);
                    bool contact=false;var seen=new HashSet<Sprite>();
                    for(int tick=0;tick<220&&fighter.Model.State==CombatState.Attack;tick++)
                    {
                        Present(fighter.Model.Motion);
                        Assert.That(view.UsesWeaponRedesignSprite,Is.True,$"No retired attack figure: weapon={weapon}, form={form}, direction={direction}, attack={attack.displayName}, time={fighter.Model.StateTime}");
                        Assert.That(art.sprite.name,Does.StartWith("Redesign_"));seen.Add(art.sprite);
                        contact|=fighter.Model.ActiveThisTick;Tick(Facing(direction));
                    }
                    Assert.That(contact,Is.True);Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Neutral));Present();
                    if(form==0)Assert.That(view.UsesWeaponRedesignSprite,Is.True,"Exact normal idle must resume");
                    Assert.That(seen.Count,Is.GreaterThanOrEqualTo(3),"Anticipation, contact and recovery must use new complete figures");contexts++;
                }
                yield return null;
            }
            Assert.That(contexts,Is.GreaterThan(400));
            AssertSilent();Report("active-transitions",contexts,0,"All Katana/Punhos attacks in eight directions and three active forms use reviewed redesign figures for the complete sequence. Techniques without dedicated art share the primary attack; transformed directions without art use the new Normal figure without palette filters. Unique choreography and transformed coverage remain pending.");
        }

        static IEnumerable<AttackDefinition> Attacks(CombatLoadout loadout)
        {
            var candidates=new List<AttackDefinition>(loadout.lights??Array.Empty<AttackDefinition>());
            candidates.AddRange(new[]{loadout.heavy,loadout.charged,loadout.counter,loadout.skill,loadout.aerial,loadout.weaponSpecial,loadout.pauseBranch,loadout.heavyBranch,loadout.swapEntry,loadout.airFinisher});
            return candidates.Where(a=>a!=null).Distinct();
        }
        [UnityTest] public IEnumerator RequiemNormalAndRupturaFistsSouthPunchUseIndependentFiguresWithoutPaletteFilters()
        {
            var contactSprites=new HashSet<Sprite>();int figures=0;
            for(int form=0;form<2;form++)
            {
                var page=catalog.pages.SingleOrDefault(p=>p.skin==0&&p.weapon==2&&p.form==form&&p.direction==0&&p.action=="Attack"&&p.attackKey=="Direto da Ruína");
                Assert.That(page,Is.Not.Null,"An independent south punch is required for form "+form);
                Context(0,2,form,0);Tick(Vector2.down,new CombatInput{lightPressed=true});
                Assert.That(fighter.Model.Attack?.displayName,Is.EqualTo("Direto da Ruína"));
                var seen=new HashSet<string>();bool contact=false;
                for(int tick=0;tick<100&&fighter.Model.State==CombatState.Attack;tick++)
                {
                    Present(fighter.Model.Motion);AssertAuthored(page);
                    var trail=fighter.GetComponentInChildren<DarklightCombatTrail>(true);Assert.That(trail,Is.Not.Null);
                    Assert.That(trail.GetComponent<MeshRenderer>().enabled,Is.False,"Authored fist energy must not receive a second centered legacy streak");
                    Assert.That(trail.GetComponent<MeshFilter>().sharedMesh.vertexCount,Is.Zero,"The suppressed legacy streak must have no active segments");
                    if(seen.Add(art.sprite.name))Capture("fists-south-form-"+form+"-"+art.sprite.name);
                    if(fighter.Model.ActiveThisTick){contact=true;contactSprites.Add(art.sprite);}
                    Tick(Vector2.down);
                }
                Assert.That(contact,Is.True,"The real punch contact must be reached in form "+form);
                Assert.That(seen.Count,Is.EqualTo(page.spriteNames.Distinct().Count()),"Startup, contact and recovery must all use authored figures");
                figures+=seen.Count;AssertSilent();yield return null;
            }
            Assert.That(contactSprites.Count,Is.EqualTo(2),"Every form must own an independent attack figure instead of recoloring the Normal sprite");
            Report("fists-south-normal-ruptura",2,figures,"Direto da Ruína south: real model startup/contact/recovery rendered for Normal and Ruptura only; archived Eclipse/Absoluto art is not certification of the fused form. Independent complete figures, no inherited Ruin Fists or vampire palette filter, no duplicated centered legacy punch streak. Other directions and techniques retain their recorded coverage.");
        }
        [UnityTest] public IEnumerator OtherDeclaredIdleAndAttackPagesReachTheCampaignConsumer()
        {
            int contexts=0,figures=0;
            foreach(var page in catalog.pages)
            {
                if(page.skin!=0||!DarklightPlayableScope.WeaponActive(page.weapon)||page.form>=2)continue;
                bool mandatory=false;
                if(mandatory||page.action!="Idle"&&page.action!="Walk"&&page.action!="Attack")continue;
                Context(page.skin,page.weapon,page.form,page.direction);var seen=new HashSet<string>(StringComparer.Ordinal);
                if(page.action=="Idle")
                {
                    for(int i=0;i<165;i++){Tick(Facing(page.direction));AssertAuthored(page);if(seen.Add(art.sprite.name))Capture(page.id+"-"+seen.Count);}
                }
                else if(page.action=="Walk")
                {
                    for(int i=0;i<165;i++)
                    {
                        Tick(Facing(page.direction),new CombatInput{move=Facing(page.direction)});
                        if(view.UsesWeaponRedesignSprite&&page.spriteNames.Contains(art.sprite.name))
                        {AssertAuthored(page);if(seen.Add(art.sprite.name))Capture(page.id+"-walk-"+seen.Count);}
                    }
                }
                else
                {
                    var attacks=Attacks(fighter.loadout).Where(attack=>string.IsNullOrEmpty(page.attackKey)||attack.displayName==page.attackKey).ToArray();
                    Assert.That(attacks.Length,Is.GreaterThan(0),page.id+": attackKey must name a real equipped attack");
                    foreach(var attack in attacks)
                    {
                        fighter.ResetModel();fighter.Model.Tick(.001f,new CombatInput{aim=Facing(page.direction)});
                        Assert.That(fighter.Model.TryAirFollowup(attack,new CombatInput{aim=Facing(page.direction)}),Is.True);
                        for(int tick=0;tick<220&&fighter.Model.State==CombatState.Attack;tick++)
                        {
                            Present(fighter.Model.Motion);
                            if(view.UsesWeaponRedesignSprite&&page.spriteNames.Contains(art.sprite.name))
                            {AssertAuthored(page);if(seen.Add(art.sprite.name))Capture(page.id+"-attack-"+seen.Count);}
                            Tick(Facing(page.direction));
                        }
                    }
                }
                Assert.That(seen.Count,Is.GreaterThan(0),page.id+": a declared page must reach the real campaign view");
                contexts++;figures+=seen.Count;AssertSilent();yield return null;
            }
            Report("additional-pages",contexts,figures,"Additional catalog Idle/Walk/Attack pages exercised through actual combat definitions; only declared matching phases are counted. Unmapped techniques remain pending redesign.");
        }
    }
}
