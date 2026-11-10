using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    // Exercises the real combat clock and imported sprite resolver in EditMode.
    // No campaign, PlayMode, saves, preferences or audio playback.
    public sealed class DarklightFabiusRecoveryTests
    {
        DarklightWeaponRedesignCatalog catalog;
        readonly List<UnityEngine.Object> owned=new();
        bool priorPause;float priorVolume;
        [SetUp] public void Setup()
        {
            Debug.Log("FabiusRecoveryBegin: "+TestContext.CurrentContext.Test.FullName);
            priorPause=AudioListener.pause;priorVolume=AudioListener.volume;
            AudioListener.pause=true;AudioListener.volume=0;
            DarklightWeaponRedesignBank.ClearCache();
            catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(
                Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root+"catalog").text);
            Assert.That(DarklightWeaponRedesignBank.ValidCatalog(catalog),Is.True);
        }
        [TearDown] public void Cleanup()
        {
            DarklightWeaponRedesignBank.ClearCache();
            foreach(var obj in owned)if(obj!=null)UnityEngine.Object.DestroyImmediate(obj);
            owned.Clear();AudioListener.pause=priorPause;AudioListener.volume=priorVolume;
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
        static Vector2 Facing(int d)=>new Vector2(-Mathf.Sin(d*Mathf.PI/4),-Mathf.Cos(d*Mathf.PI/4));
        AttackDefinition Attack(string key,StrikeKind kind)
        {
            var a=ScriptableObject.CreateInstance<AttackDefinition>();owned.Add(a);
            a.displayName=key;a.animationKey=key;a.kind=kind;
            a.startup=.25f;a.active=.2f;a.recovery=.35f;a.chargeSeconds=.45f;
            return a;
        }
        static CombatModel Model(int weapon,int direction,CombatLoadout loadout=null)
        {
            var model=new CombatModel(loadout??new CombatLoadout{weapon=weapon});
            model.Tick(.001f,new CombatInput{aim=Facing(direction)});return model;
        }
        static Sprite Resolve(CombatModel model,int weapon,int direction)
        {
            Assert.That(DarklightWeaponRedesignBank.TryResolve(0,weapon,0,direction,model,model.Motion,null,out var sprite),Is.True);
            Assert.That(sprite,Is.Not.Null);Assert.That(DarklightWeaponRedesignBank.IsAuthored(sprite),Is.True);
            return sprite;
        }
        [TestCase("Parry",0)][TestCase("Parry",2)][TestCase("Parry",3)]
        [TestCase("Hurt",0)][TestCase("Hurt",2)][TestCase("Hurt",3)]
        [TestCase("Dead",0)][TestCase("Dead",2)][TestCase("Dead",3)]
        public void EssentialStatesReachEveryAuthoredPhaseAndRecoverOrHold(string action,int weapon)
        {
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&p.weapon==weapon&&p.action==action&&p.presentation!="Mounted").ToArray();
            Assert.That(pages.Select(p=>p.direction),Is.EquivalentTo(Enumerable.Range(0,8)));
            foreach(var page in pages)
            {
                var model=Model(weapon,page.direction);
                if(action=="Parry")model.Tick(.001f,new CombatInput{parryPressed=true});
                else model.Hurt(action=="Dead");
                float combatClock=model.Clock,elapsed=0;
                for(int i=0;i<page.spriteNames.Length;i++)
                {
                    float target=elapsed+page.durationMs[i]*.0005f;
                    model.Tick(target-model.StateTime,default);
                    Assert.That(model.State.ToString(),Is.EqualTo(action),page.id);
                    var sprite=Resolve(model,weapon,page.direction);
                    Assert.That(sprite.name,Is.EqualTo(page.spriteNames[i]),page.id);
                    Assert.That(sprite.texture.width,Is.EqualTo(page.nativeSize[0]));
                    Assert.That(sprite.texture.height,Is.EqualTo(page.nativeSize[1]));
                    elapsed+=page.durationMs[i]*.001f;
                }
                model.Tick(elapsed-model.StateTime+.3f,default);
                if(action=="Dead")
                {
                    Assert.That(model.Clock,Is.EqualTo(combatClock));
                    Assert.That(model.Motion,Is.EqualTo(Vector2.zero));
                    Assert.That(Resolve(model,weapon,page.direction).name,Is.EqualTo(page.spriteNames.Last()));
                    model.Tick(1,new CombatInput{lightPressed=true,dodgePressed=true});
                    Assert.That(model.State,Is.EqualTo(CombatState.Dead));
                    Assert.That(Resolve(model,weapon,page.direction).name,Is.EqualTo(page.spriteNames.Last()));
                }
                else
                {
                    Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
                    // Motorcycle neutral poses live in the dedicated bank, as
                    // CharacterView does not require a duplicate Idle catalog page.
                    var neutral=weapon==3?DarklightMotorcycleRedesignBank.DualSawsPose(page.direction,model):
                        Resolve(model,weapon,page.direction);
                    Assert.That(neutral,Is.Not.Null);
                    Assert.That(page.spriteNames,Does.Not.Contain(neutral.name));
                }
            }
        }
        [TestCase(0,"Carregado")][TestCase(2,"Juízo da Ruína")][TestCase(3,"Serra de Entrada")]
        public void ChargingUsesOnlyWindupThenReleasesOrCancels(int weapon,string key)
        {
            var heavy=Attack(key,StrikeKind.Heavy);var charged=Attack(key,StrikeKind.Charged);
            var loadout=new CombatLoadout{weapon=weapon,heavy=heavy,charged=charged};
            foreach(int d in Enumerable.Range(0,8))
            {
                var page=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==weapon&&p.direction==d&&p.action=="Attack"&&p.attackKey==key);
                int windup=page.startupFrames>0?page.startupFrames:Mathf.Max(1,page.spriteNames.Length-2);
                var model=Model(weapon,d,loadout);
                model.Tick(.001f,new CombatInput{heavyPressed=true,heavyHeld=true});
                Assert.That(Resolve(model,weapon,d).name,Is.EqualTo(page.spriteNames[0]));
                model.Tick(.7f,new CombatInput{heavyHeld=true});
                Assert.That(model.State,Is.EqualTo(CombatState.Charging));
                Assert.That(Resolve(model,weapon,d).name,Is.EqualTo(page.spriteNames[windup-1]));
                model.Tick(.01f,new CombatInput{heavyReleased=true});
                Assert.That(model.State,Is.EqualTo(CombatState.Attack));Assert.That(model.Attack,Is.SameAs(charged));
                Assert.That(page.spriteNames,Does.Contain(Resolve(model,weapon,d).name));
                model=Model(weapon,d,loadout);
                model.Tick(.001f,new CombatInput{heavyPressed=true,heavyHeld=true});
                model.Tick(.01f,new CombatInput{heavyReleased=true});
                Assert.That(model.Attack,Is.SameAs(heavy));
                model=Model(weapon,d,loadout);
                model.Tick(.001f,new CombatInput{heavyPressed=true,heavyHeld=true});
                model.Tick(.01f,new CombatInput{heavyHeld=true,dodgePressed=true,aim=Facing(d)});
                Assert.That(model.State,Is.EqualTo(CombatState.Dodge));
                Assert.That(Resolve(model,weapon,d).name,Does.Contain("Dodge"));
            }
        }
        [Test] public void SerraEntryUsesItsOwnWholeSequenceInEightDirections()
        {
            var attack=Attack("Serra de Entrada",StrikeKind.Light);
            foreach(int d in Enumerable.Range(0,8))
            {
                var page=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==3&&p.direction==d&&p.attackKey==attack.displayName);
                var model=Model(3,d,new CombatLoadout{weapon=3,lights=new[]{attack}});
                model.Tick(.001f,new CombatInput{lightPressed=true});
                var seen=new HashSet<string>();int phase=0;
                foreach(float time in new[]{0f,.15f,.3f,.55f})
                {
                    if(time>model.StateTime)model.Tick(time-model.StateTime,default);
                    var name=Resolve(model,3,d).name;
                    Assert.That(name,Is.EqualTo(page.spriteNames[phase++]));seen.Add(name);
                }
                Assert.That(seen,Is.EquivalentTo(page.spriteNames));
                model.Tick(1,default);Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
            }
        }
        [TestCase(0)][TestCase(2)][TestCase(3)]
        public void CharacterViewPreviewUsesRecoveredWholeFigureWithoutCampaign(int weapon)
        {
            Assert.That(DarklightCampaign.Instance,Is.Null);
            Assert.That(DarklightCombatLab.Instance,Is.Null);
            var actor=new GameObject("RecoveryPreview");owned.Add(actor);
            var child=new GameObject("WholeFigure");child.transform.SetParent(actor.transform,false);
            var art=child.AddComponent<SpriteRenderer>();var view=actor.AddComponent<DarklightCharacterView>();
            typeof(DarklightCharacterView).GetMethod("Awake",System.Reflection.BindingFlags.Instance|
                System.Reflection.BindingFlags.NonPublic).Invoke(view,null);
            var equipped=ScriptableObject.CreateInstance<WeaponDefinition>();owned.Add(equipped);
            equipped.id=(DarklightWeaponId)weapon;
            foreach(int d in Enumerable.Range(0,8))
            {
                var model=Model(weapon,d);model.Hurt();
                view.PresentPreview(model,Vector2.zero,0,0,0,equipped);
                Assert.That(view.UsesWeaponRedesignSprite,Is.True);
                var page=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==weapon&&p.direction==d&&p.action=="Hurt");
                Assert.That(art.sprite.name,Is.EqualTo(page.spriteNames[0]));
                Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f));
                Assert.That(art.color,Is.EqualTo(Color.white));Assert.That(art.flipX,Is.False);
                Assert.That(art.transform.localRotation,Is.EqualTo(Quaternion.identity));
                var legacy=actor.GetComponent<DarklightOriginalVampireView>();
                Assert.That(typeof(DarklightOriginalVampireView).GetField("catalog",System.Reflection.BindingFlags.Instance|
                    System.Reflection.BindingFlags.NonPublic).GetValue(legacy),Is.Null);
            }
        }
        [Test] public void RuinFistsReuseCompatibleMotionsWithTheirOwnCombatValues()
        {
            var loadout=DarklightRuinFistsProfile.Create(owned.Add);
            Assert.That(DarklightCombatProfiles.AttackAnimationKey(loadout.aerial),Is.EqualTo("Martelo da Ruína"));
            Assert.That(loadout.aerial.kind,Is.EqualTo(StrikeKind.Aerial));
            Assert.That(loadout.aerial.ruinStrike,Is.EqualTo(DarklightRuinStrike.AerialSlam));
            Assert.That(loadout.weaponSpecial.hitPulses,Is.EqualTo(3));
            foreach(var attack in loadout.lights.Concat(new[]{loadout.heavy,loadout.charged,loadout.counter,
                loadout.skill,loadout.aerial,loadout.weaponSpecial,loadout.pauseBranch,loadout.heavyBranch}))
            foreach(int d in Enumerable.Range(0,8))
            {
                string key=DarklightCombatProfiles.AttackAnimationKey(attack);
                var page=catalog.pages.Single(p=>p.skin==0&&p.form==0&&p.weapon==2&&p.direction==d&&p.action=="Attack"&&p.attackKey==key);
                var model=Model(2,d,new CombatLoadout{weapon=2,lights=new[]{attack}});
                model.Tick(.001f,new CombatInput{lightPressed=true});
                Assert.That(model.Attack,Is.SameAs(attack));
                Assert.That(page.spriteNames,Does.Contain(Resolve(model,2,d).name),attack.displayName);
            }
        }
    }
}
