using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightWeaponRedesignTests
    {
        static DarklightWeaponRedesignPage Idle()=>new(){
            id="ReqDaggersIdleS",skin=0,weapon=1,form=0,direction=0,action="Idle",attackKey="",
            resourcePath=DarklightWeaponRedesignBank.Root+"ReqDaggersIdle",imageSha256=new string('a',64),
            sourcePath="ArtSources/WeaponRedesign20261023/original.png",sourceSha256=new string('b',64),
            sourceSize=new[]{1536,1024},nativeSize=new[]{1024,1024},pixelsPerUnit=128,status="visual-reviewed",wholeFigures=true,
            spriteNames=new[]{"Redesign_ReqDaggersIdle_00","Redesign_ReqDaggersIdle_01","Redesign_ReqDaggersIdle_02","Redesign_ReqDaggersIdle_03"},
            durationMs=new[]{350,250,180,350},sourceSpriteNames=System.Array.Empty<string>(),loop=true};
        [Test] public void OnlyReviewedWholeFiguresWithPreservedSourcesCanActivate()
        {
            var page=Idle();Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.True);
            page.status="candidate";Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.False);
            page.status="visual-reviewed";page.wholeFigures=false;Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.False);
            page.wholeFigures=true;page.sourceSha256="unverified";Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.False);
            page.sourceSha256=new string('b',64);page.pixelsPerUnit=float.NaN;Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.False);
        }
        [Test] public void IdleTimingsLoopUsingTheirDeclaredDurationsWithoutResampling()
        {
            var page=Idle();
            Assert.That(DarklightWeaponRedesignBank.Frame(page,0),Is.EqualTo(0));
            Assert.That(DarklightWeaponRedesignBank.Frame(page,.351f),Is.EqualTo(1));
            Assert.That(DarklightWeaponRedesignBank.Frame(page,.601f),Is.EqualTo(2));
            Assert.That(DarklightWeaponRedesignBank.Frame(page,.781f),Is.EqualTo(3));
            Assert.That(DarklightWeaponRedesignBank.Frame(page,1.131f),Is.EqualTo(0));
            page.loop=false;Assert.That(DarklightWeaponRedesignBank.Frame(page,50),Is.EqualTo(3));
        }
        [Test] public void CombatRequiresExactExistingPhaseMappingAndCannotUseIdleTiming()
        {
            var page=Idle();page.action="Attack";page.attackKey="Corte duplo";
            Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.False,"An attack cannot be broadly time-resampled");
            page.sourceSpriteNames=new[]{"Existing_windup","Existing_contact","Existing_follow","Existing_recovery"};
            Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.True);
            Assert.That(DarklightWeaponRedesignBank.Frame(page,500,"Existing_contact"),Is.EqualTo(1));
            Assert.That(DarklightWeaponRedesignBank.Frame(page,0,"Other_weapon_contact"),Is.EqualTo(-1));
            page.sourceSpriteNames[3]=page.sourceSpriteNames[2];Assert.That(DarklightWeaponRedesignBank.ValidPage(page),Is.False);
        }
        [Test] public void PartialCatalogCannotShadowAnotherDirectionFormWeaponOrClip()
        {
            var normal=Idle();var eclipse=Idle();eclipse.id="EclipseIdleS";eclipse.form=2;
            var data=new DarklightWeaponRedesignCatalog{schemaVersion=1,complete=false,fallbackPolicy=DarklightWeaponRedesignBank.FallbackPolicy,pages=new[]{normal,eclipse}};
            Assert.That(DarklightWeaponRedesignBank.ValidCatalog(data),Is.True);
            eclipse.form=0;Assert.That(DarklightWeaponRedesignBank.ValidCatalog(data),Is.False,"Same exact context twice is ambiguous");
            eclipse.direction=7;Assert.That(DarklightWeaponRedesignBank.ValidCatalog(data),Is.True);
            eclipse.direction=8;Assert.That(DarklightWeaponRedesignBank.ValidCatalog(data),Is.False,"There is no invented/mirrored ninth direction");
        }
        [Test] public void ActionSelectionRetainsLivingIdleAndMovementTransitions()
        {
            var model=new CombatModel(new CombatLoadout());
            Assert.That(DarklightWeaponRedesignBank.Action(model,Vector2.zero),Is.EqualTo("Idle"));
            Assert.That(DarklightWeaponRedesignBank.Action(model,Vector2.left),Is.EqualTo("Walk"));
            Assert.That(DarklightWeaponRedesignBank.Action(model,Vector2.zero,"IdleWounded"),Is.EqualTo("IdleWounded"));
            Assert.That(DarklightWeaponRedesignBank.Action(model,Vector2.left,"StartMove"),Is.EqualTo("StartMove"));
            model.Hurt();Assert.That(DarklightWeaponRedesignBank.Action(model,Vector2.zero,"Idle"),Is.EqualTo("Hurt"));
        }
        [Test] public void ImportedIdleContextsResolveTheirOwnFiguresAndClearPreviousFormMasks()
        {
            var text=Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root+"catalog");
            Assert.That(text,Is.Not.Null,"The reviewed figure sources and partial catalog must be imported before delivery");
            var catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(text.text);
            Assert.That(DarklightWeaponRedesignBank.ValidCatalog(catalog),Is.True);
            Assert.That(catalog.pages.Length,Is.GreaterThan(0));
            DarklightWeaponRedesignBank.ClearCache();
            var holder=new GameObject("Redesign ownership verification");var renderer=holder.AddComponent<SpriteRenderer>();
            try
            {
                var model=new CombatModel(new CombatLoadout());var properties=new MaterialPropertyBlock();int verified=0;
                foreach(var page in catalog.pages)
                {
                    if(page.action!="Idle"||page.sourceSpriteNames?.Length>0)continue;
                    Assert.That(DarklightWeaponRedesignBank.TryResolve(page.skin,page.weapon,page.form,page.direction,model,Vector2.zero,null,out var sprite),Is.True,page.id);
                    Assert.That(sprite.name,Is.EqualTo(page.spriteNames[0]));Assert.That(sprite.pixelsPerUnit,Is.EqualTo(page.pixelsPerUnit).Within(.001f));
                    Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprite.texture.mipmapCount,Is.EqualTo(1));
                    renderer.sprite=sprite;properties.SetFloat("_VampireForm",3);properties.SetFloat("_WholeFrameId",245);
                    properties.SetTexture("_VampireMask",Texture2D.whiteTexture);properties.SetVector("_CanonicalSkinShift",Vector4.one);renderer.SetPropertyBlock(properties);
                    DarklightWeaponRedesignBank.ApplyAuthoredOwnership(renderer);renderer.GetPropertyBlock(properties);
                    Assert.That(properties.GetFloat("_VampireForm"),Is.Zero,"Authored transformation colors cannot be recolored");
                    Assert.That(properties.GetFloat("_WholeFrameId"),Is.Zero,"Legacy ownership IDs must not erase new daggers");
                    Assert.That(properties.GetTexture("_VampireMask"),Is.SameAs(Texture2D.blackTexture));
                    Assert.That(properties.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));verified++;
                }
                Assert.That(verified,Is.GreaterThan(0));
                for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int form=0;form<4;form++)for(int direction=0;direction<8;direction++)
                {
                    bool declared=false;
                    foreach(var page in catalog.pages)if(page.skin==skin&&page.weapon==weapon&&page.form==form&&page.direction==direction&&page.action=="Idle"&&!(page.sourceSpriteNames?.Length>0))declared=true;
                    Assert.That(DarklightWeaponRedesignBank.TryResolve(skin,weapon,form,direction,model,Vector2.zero,null,out _),Is.EqualTo(declared),$"skin={skin}, weapon={weapon}, form={form}, direction={direction}: no direction/form borrowing");
                }
            }
            finally{Object.DestroyImmediate(holder);DarklightWeaponRedesignBank.ClearCache();}
        }
        [Test] public void OptionalEquipmentIconsKeepLegacyFallbackOutsideExactWeaponAndForm()
        {
            var fallback=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.zero,1);
            try
            {
                DarklightWeaponRedesignBank.ClearCache();
                Assert.That(DarklightWeaponRedesignBank.Icon(3,0,fallback),Is.SameAs(fallback),"Motorcycle icon is outside this redesign");
                Assert.That(DarklightWeaponRedesignBank.Icon(0,4,fallback),Is.SameAs(fallback));
                var descriptor=Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root+"EquipmentIcons");
                var sprites=descriptor==null?System.Array.Empty<Sprite>():Resources.LoadAll<Sprite>(DarklightWeaponRedesignBank.Root+"EquipmentIcons");
                for(int weapon=0;weapon<3;weapon++)for(int form=0;form<4;form++)
                {
                    string name="Redesign_Icon_Weapon"+weapon+"_Form"+form;Sprite expected=fallback;
                    foreach(var sprite in sprites)if(sprite.name==name)expected=sprite;
                    Assert.That(DarklightWeaponRedesignBank.Icon(weapon,form,fallback),Is.SameAs(expected),name);
                }
            }
            finally{Object.DestroyImmediate(fallback);DarklightWeaponRedesignBank.ClearCache();}
        }
        [Test] public void AuthoredFistPunchesUseIndependentWholeFiguresAndClearEveryLegacyPalette()
        {
            var owned=new System.Collections.Generic.List<Object>();
            var loadout=DarklightRuinFistsProfile.Create(owned.Add);
            var holder=new GameObject("Authored fist form verification");
            var renderer=holder.AddComponent<SpriteRenderer>();
            var textures=new System.Collections.Generic.HashSet<Texture>();
            try
            {
                DarklightWeaponRedesignBank.ClearCache();
                for(int form=0;form<4;form++)
                {
                    var model=new CombatModel(loadout);
                    model.Tick(.001f,new CombatInput{lightPressed=true,aim=Vector2.down});
                    Assert.That(model.State,Is.EqualTo(CombatState.Attack));
                    Texture texture=null;var phases=new System.Collections.Generic.HashSet<Sprite>();
                    foreach(float time in new[]{0f,model.Attack.startup+.005f,model.Attack.startup+model.Attack.active+.03f})
                    {
                        while(model.StateTime+.0001f<time)model.Tick(Mathf.Min(.01f,time-model.StateTime),new CombatInput{aim=Vector2.down});
                        // CharacterView resolves this normal source before OriginalVampireView.
                        // A transformed source name must not be required by the additive bank.
                        var source=TodayCharacterBank.Resolve(0,0,2,0,model,Vector2.zero);
                        Assert.That(source,Is.Not.Null);
                        Assert.That(DarklightWeaponRedesignBank.TryResolve(0,2,form,0,model,Vector2.zero,source,out var authored),Is.True,
                            $"Punch form={form}, source={source.name} must use its authored full figure");
                        Assert.That(authored.name,Does.StartWith("Redesign_ReqFists"));
                        Assert.That(DarklightWeaponRedesignBank.IsAuthored(authored),Is.True);
                        phases.Add(authored);
                        if(texture==null)texture=authored.texture;
                        Assert.That(authored.texture,Is.SameAs(texture));
                        renderer.sprite=authored;
                        var properties=new MaterialPropertyBlock();
                        properties.SetFloat("_VampireForm",3);properties.SetFloat("_DarkForm",3);
                        properties.SetFloat("_RuinFists",1);properties.SetFloat("_WholeFrameId",200);
                        properties.SetTexture("_VampireMask",Texture2D.whiteTexture);
                        properties.SetVector("_CanonicalSkinShift",Vector4.one);renderer.SetPropertyBlock(properties);
                        DarklightWeaponRedesignBank.ApplyAuthoredOwnership(renderer);renderer.GetPropertyBlock(properties);
                        foreach(string key in new[]{"_VampireForm","_DarkForm","_RuinFists","_WholeFrameId"})
                            Assert.That(properties.GetFloat(key),Is.Zero,key+" must not recolor or erase the authored fist figure");
                        Assert.That(properties.GetTexture("_VampireMask"),Is.SameAs(Texture2D.blackTexture));
                        Assert.That(properties.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
                    }
                    Assert.That(phases.Count,Is.EqualTo(3),"Startup, contact and recovery require their three supplied figures");
                    Assert.That(textures.Add(texture),Is.True,"Every authored form must have its own full source texture");
                }
            }
            finally
            {
                Object.DestroyImmediate(holder);foreach(var item in owned)Object.DestroyImmediate(item);
                DarklightWeaponRedesignBank.ClearCache();
            }
        }
    }
}
