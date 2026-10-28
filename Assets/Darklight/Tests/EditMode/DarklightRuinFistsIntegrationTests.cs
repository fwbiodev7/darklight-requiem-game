using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightRuinFistsIntegrationTests
    {
        readonly List<UnityEngine.Object> owned=new();
        CombatLoadout Loadout()=>DarklightRuinFistsProfile.Create(owned.Add);
        [TearDown]public void Clean(){foreach(var o in owned)UnityEngine.Object.DestroyImmediate(o);owned.Clear();}
        [Test]public void EveryImportedConfigurationHasTwelveNativeSpritesAndAnUncompressedPointMask()
        {
            Assert.That(DarklightRuinFistsBank.Ready,Is.True,"The replacement must be imported before integration tests");
            var text=Resources.Load<TextAsset>("Darklight/RuinFists/catalog");Assert.That(text,Is.Not.Null);
            var catalog=JsonUtility.FromJson<DarklightRuinFistsCatalog>(text.text);
            Assert.That(DarklightRuinFistsBank.Complete(catalog),Is.True);
            Assert.That(catalog.pages.Count(p=>!p.paletteVariant),Is.EqualTo(64),"Normal and Absoluto have independent whole figures");
            Assert.That(catalog.pages.Select(p=>p.resourcePath).Distinct().Count(),Is.EqualTo(64));
            foreach(var p in catalog.pages)
            {
                Assert.That(p.paletteVariant,Is.EqualTo(p.form==1||p.form==2));
                if(!p.paletteVariant)continue;
                var normal=catalog.pages.Single(n=>n.skin==p.skin&&n.form==0&&n.direction==p.direction&&n.bank==p.bank);
                Assert.That(p.resourcePath,Is.EqualTo(normal.resourcePath));Assert.That(p.palettePath,Is.EqualTo(normal.palettePath));
                CollectionAssert.AreEqual(normal.spriteNames,p.spriteNames,"Palette variants cannot borrow another direction or action bank");
            }
            foreach(var p in catalog.pages)
            {
                var sprites=Resources.LoadAll<Sprite>(p.resourcePath);Assert.That(sprites.Length,Is.EqualTo(12),p.resourcePath);
                var mask=Resources.Load<Texture2D>(p.palettePath);Assert.That(mask,Is.Not.Null,p.palettePath);
                Assert.That(mask.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(mask.mipmapCount,Is.EqualTo(1));
                foreach(string name in p.spriteNames)
                {
                    var sprite=sprites.Single(s=>s.name==name);Assert.That(sprite.pixelsPerUnit,Is.EqualTo(220/1.99f).Within(.001f));
                    Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprite.texture.width,Is.EqualTo(mask.width));
                    Assert.That(sprite.texture.format,Is.EqualTo(TextureFormat.RGBA32));
                }
            }
        }
        [Test]public void BothSkinsTransformAllDirectionsWithoutBorrowingAnotherDirectionOrLosingOwnership()
        {
            for(int skin=0;skin<2;skin++)for(int dir=0;dir<8;dir++)
            {
                var normal=DarklightRuinFistsBank.Idle(skin,dir,0);Assert.That(normal,Is.Not.Null);
                Assert.That(DarklightRuinFistsBank.FrameId(normal),Is.EqualTo(1));
                for(int form=1;form<4;form++)
                {
                    Assert.That(DarklightRuinFistsBank.TryTransform(normal,form,out var transformed,out var mask,out int owner),Is.True);
                    Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.EqualTo(1));
                    Assert.That(DarklightRuinFistsBank.UsesMaterialPalette(normal,form),Is.EqualTo(form==1||form==2));
                    if(form==3)Assert.That(transformed,Is.Not.SameAs(normal),"Absoluto requires its own whole figure and hair drawing");
                    else Assert.That(transformed,Is.SameAs(normal),"Ruptura/Eclipse retain the same geometry with material-owned palette channels");
                }
            }
        }
        [Test]public void ImportedWalkDefenseDamageDeathAndComboContactsResolveWithTheirWholeFigureMasks()
        {
            var loadout=Loadout();
            for(int skin=0;skin<2;skin++)for(int dir=0;dir<8;dir++)
            {
                void Check(CombatModel model,Vector2 move)
                {
                    var sprite=DarklightRuinFistsBank.Resolve(skin,dir,model,move);
                    Assert.That(sprite,Is.Not.Null,$"skin={skin} dir={dir} state={model.State}");
                    Assert.That(DarklightRuinFistsBank.Mask(sprite),Is.Not.Null);Assert.That(DarklightRuinFistsBank.FrameId(sprite),Is.InRange(1,12));
                    for(int form=1;form<4;form++)Assert.That(DarklightRuinFistsBank.TryTransform(sprite,form,out _,out _,out _),Is.True);
                }
                var model=new CombatModel(loadout);Check(model,Vector2.zero);
                for(int i=0;i<4;i++){model.Tick(.15f,new CombatInput{move=Vector2.left});Check(model,Vector2.left);}
                model.Tick(.001f,new CombatInput{dodgePressed=true,move=Vector2.left});Check(model,Vector2.zero);
                model.Tick(.1f,default);Check(model,Vector2.zero);model.Tick(.4f,default);
                model.Tick(.001f,new CombatInput{parryPressed=true});Check(model,Vector2.zero);model.Tick(.4f,default);
                for(int i=0;i<4;i++)
                {
                    model.Tick(.001f,new CombatInput{lightPressed=true});Check(model,Vector2.zero);
                    model.Tick(model.Attack.startup+.001f,default);Assert.That(model.ActiveThisTick,Is.True);Check(model,Vector2.zero);
                    model.Tick(model.Attack.active+model.Attack.recovery+.01f,default);Check(model,Vector2.zero);
                }
                model.Hurt();Check(model,Vector2.zero);model.Tick(.3f,default);model.Hurt(true);Check(model,Vector2.zero);
            }
        }
        [Test]public void DeathFinishesAndHoldsTheProneWholeFigureWhileCombatIsPaused()
        {
            var model=new CombatModel(Loadout());model.Hurt(true);
            var collapse=DarklightRuinFistsBank.Resolve(0,2,model,Vector2.zero,10);
            var prone=DarklightRuinFistsBank.Resolve(0,2,model,Vector2.zero,10.4f);
            Assert.That(model.StateTime,Is.Zero,"The paused combat clock did not advance");
            Assert.That(collapse,Is.Not.Null);Assert.That(prone,Is.Not.Null);Assert.That(prone,Is.Not.SameAs(collapse));
            Assert.That(DarklightRuinFistsBank.FrameId(prone),Is.EqualTo(12));
            Assert.That(DarklightRuinFistsBank.Resolve(0,2,model,Vector2.zero,14),Is.SameAs(prone));
            var reversion=new DarklightDeathReversion();reversion.Observe(3,false,9);reversion.Observe(0,true,10);
            Assert.That(reversion.Observe(0,true,10.4f),Is.Zero);Assert.That(reversion.Observe(0,true,11),Is.Zero);
        }
    }
}
