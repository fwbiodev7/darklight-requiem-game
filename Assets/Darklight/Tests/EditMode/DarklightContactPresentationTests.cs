using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Darklight.Tests
{
    public sealed class DarklightContactPresentationTests
    {
        AttackDefinition attack;GameObject root;Texture2D texture;Sprite wideSprite;
        CombatModel Begin()
        {
            attack=ScriptableObject.CreateInstance<AttackDefinition>();
            attack.startup=.1f;attack.active=.05f;attack.recovery=.2f;
            var model=new CombatModel(new CombatLoadout{lights=new[]{attack}});
            model.Tick(.001f,new CombatInput{lightPressed=true});return model;
        }
        [TearDown]public void Cleanup()
        {
            if(root!=null)
            {
                foreach(var filter in root.GetComponentsInChildren<MeshFilter>())
                    if(filter.sharedMesh!=null)Object.DestroyImmediate(filter.sharedMesh);
                Object.DestroyImmediate(root);
            }
            if(wideSprite!=null)Object.DestroyImmediate(wideSprite);
            if(texture!=null)Object.DestroyImmediate(texture);
            if(attack!=null)Object.DestroyImmediate(attack);
        }
        [Test]public void ConfirmedContactSurvivesATickThatFinishesTheWholeAttack()
        {
            var model=Begin();model.Tick(attack.Duration+.01f,default);
            Assert.That(model.State,Is.EqualTo(CombatState.Neutral));
            Assert.That(model.ActiveThisTick,Is.True);Assert.That(model.StateTime,Is.Zero);
            Assert.That(DarklightAttackFrameTiming.Frame(12,3,6,model),Is.EqualTo(5));
            model.Tick(.01f,default);Assert.That(model.ActiveThisTick,Is.False);
        }
        [Test]public void ContactCrossingIntoRecoveryStillUsesTheFinalContactPose()
        {
            var model=Begin();model.Tick(attack.startup+attack.active+.02f,default);
            Assert.That(model.State,Is.EqualTo(CombatState.Attack));Assert.That(model.ActiveThisTick,Is.True);
            Assert.That(DarklightAttackFrameTiming.Frame(12,3,6,model),Is.EqualTo(5));
            model.Tick(.01f,default);Assert.That(model.ActiveThisTick,Is.False);
            Assert.That(DarklightAttackFrameTiming.Frame(12,3,6,model),Is.GreaterThanOrEqualTo(6));
        }
        [Test]public void RibbonsUseOnlyConfirmedContactsIncludingTheLastLongTick()
        {
            root=new GameObject("ContactPresentationTest");var body=root.AddComponent<SpriteRenderer>();
            var trailObject=new GameObject("Trail");trailObject.transform.SetParent(root.transform);
            var trail=trailObject.AddComponent<DarklightCombatTrail>();var model=Begin();
            trail.Present(body,0,model,1);var renderer=trailObject.GetComponent<MeshRenderer>();
            Assert.That(renderer.enabled,Is.False,"Windup must not produce a contact ribbon");
            model.Tick(attack.Duration+.01f,default);
            foreach(int weapon in new[]{0,1,2,3})
            {
                trail.Present(body,weapon,model,1);Assert.That(renderer.enabled,Is.True);
                var mesh=trailObject.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(mesh.vertexCount,Is.GreaterThan(0));Assert.That(mesh.colors.Any(c=>c.a>0),Is.True);
            }
            model.Tick(.01f,default);trail.Present(body,0,model,1);Assert.That(renderer.enabled,Is.False);
        }
        [Test]public void ChainAndOtherRibbonsFollowTheFormPalette()
        {
            root=new GameObject("RibbonPaletteTest");var body=root.AddComponent<SpriteRenderer>();
            var trailObject=new GameObject("Trail");trailObject.transform.SetParent(root.transform);
            var trail=trailObject.AddComponent<DarklightCombatTrail>();var model=Begin();model.Tick(attack.startup+.01f,default);
            foreach(int weapon in new[]{0,1,2,3})foreach(int form in new[]{1,2,3})
            {
                trail.Present(body,weapon,model,form);
                var colors=trailObject.GetComponent<MeshFilter>().sharedMesh.colors.Where(c=>c.a>.001f).ToArray();
                Assert.That(colors.Length,Is.GreaterThan(0));
                if(form==1)Assert.That(colors.Any(c=>c.r>c.b*2),Is.True);
                if(form==2)Assert.That(colors.Any(c=>c.b>c.r&&c.b>c.g),Is.True);
                if(form==3)Assert.That(colors.All(c=>Mathf.Abs(c.r-c.b)<.15f),Is.True);
            }
        }
        [TestCase(0)][TestCase(1)]public void AuraAndBurstDoNotGrowWhenAWeaponSpriteIsMuchLarger(int skin)
        {
            root=new GameObject("StableBodyBoundsTest");var visual=new GameObject("Body");visual.transform.SetParent(root.transform);
            var body=visual.AddComponent<SpriteRenderer>();var view=root.AddComponent<DarklightCharacterView>();
            typeof(DarklightCharacterView).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(view,null);
            view.SetSkin(skin,null,null,null,null);Assert.That(body.sprite,Is.Not.Null);
            var expected=view.ReferenceBodyBounds;
            var auraObject=new GameObject("Aura");auraObject.transform.SetParent(visual.transform);
            var burstObject=new GameObject("Burst");burstObject.transform.SetParent(visual.transform);
            var aura=auraObject.AddComponent<DarklightSmallTransformationAura>();var burst=burstObject.AddComponent<DarklightTransformationBurst>();
            aura.Present(body,3,1,false);burst.Present(body,3,.4f,true,true);
            var auraVertices=auraObject.GetComponent<MeshFilter>().sharedMesh.vertices;
            var burstVertices=burstObject.GetComponent<MeshFilter>().sharedMesh.vertices;
            var lightPosition=burstObject.transform.Find("LuzDoDespertar").localPosition;
            texture=new Texture2D(100,100);wideSprite=Sprite.Create(texture,new Rect(0,0,100,100),Vector2.zero,.1f);body.sprite=wideSprite;
            Assert.That(body.sprite.bounds.size.y,Is.GreaterThan(expected.size.y*100));
            aura.Present(body,3,1,false);burst.Present(body,3,.4f,true,true);
            Assert.That(view.ReferenceBodyBounds,Is.EqualTo(expected));
            CollectionAssert.AreEqual(auraVertices,auraObject.GetComponent<MeshFilter>().sharedMesh.vertices);
            CollectionAssert.AreEqual(burstVertices,burstObject.GetComponent<MeshFilter>().sharedMesh.vertices);
            Assert.That(burstObject.transform.Find("LuzDoDespertar").localPosition,Is.EqualTo(lightPosition));
        }
    }
}
