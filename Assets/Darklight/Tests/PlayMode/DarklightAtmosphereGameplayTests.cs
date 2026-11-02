using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        [UnityTest] public IEnumerator PixelAtmospherePreservesGeometryAndCharacterAndCapturesGameplay()
        {
            // The shared animation fixture enters training, which replaces the campaign map.
            Call(game,"LoadMap",0);yield return null;yield return null;
            lab.SetPaused(true);fighter.enabled=false;
            foreach(var enemy in game.MapEnemies)if(enemy!=null)enemy.enabled=false;
            lab.WorldCamera.transform.position=fighter.transform.position+new Vector3(0,1,-10);
            Context(0,0,0,0);
            var atmosphere=DarklightAtmosphereController.Current;
            Assert.That(atmosphere,Is.Not.Null);
            atmosphere.SetAllRegionsEnabled(false);atmosphere.ApplyRegion(0);
            var effects=(Transform)typeof(DarklightAtmosphereController).GetField("effectsRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(atmosphere);
            var colliders=Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
            var positions=colliders.ToDictionary(c=>c,c=>(c.transform.position,c.transform.localScale,c.offset,c.enabled));
            var renderers=game.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>!r.transform.IsChildOf(effects)).ToArray();
            var sprites=renderers.ToDictionary(r=>r,r=>r.sprite);
            atmosphere.SetEffectsEnabled(false);yield return null;
            var colors=renderers.ToDictionary(r=>r,r=>r.color);
            var body=art.sprite;var scale=art.transform.localScale;
            const string folder="outputs/retomada-20261101/atmosphere/gameplay";
            Directory.CreateDirectory(folder);
            Capture("atmosphere-before");File.Copy(Path.Combine(Folder,"atmosphere-before.png"),folder+"/before.png",true);
            atmosphere.SetEffectsEnabled(true);yield return null;
            Assert.That(atmosphere.ActiveParticleCount,Is.GreaterThan(0).And.LessThanOrEqualTo(atmosphere.MaxParticles));
            Assert.That(atmosphere.MaxParticles,Is.LessThanOrEqualTo(88));
            Assert.That(Resources.LoadAll<Sprite>("Darklight/AtmosphereTextures20261101/AtmosphereTextures"),Has.Length.EqualTo(6));
            foreach(var texture in Resources.LoadAll<Texture2D>("Darklight/AtmosphereTextures20261101/AtmosphereTextures"))
                Assert.That(texture.filterMode,Is.EqualTo(FilterMode.Point));
            Capture("atmosphere-after");File.Copy(Path.Combine(Folder,"atmosphere-after.png"),folder+"/after.png",true);
            foreach(var item in positions)
            {
                Assert.That(item.Key.transform.position,Is.EqualTo(item.Value.Item1));
                Assert.That(item.Key.transform.localScale,Is.EqualTo(item.Value.Item2));
                Assert.That(item.Key.offset,Is.EqualTo(item.Value.Item3));Assert.That(item.Key.enabled,Is.EqualTo(item.Value.Item4));
            }
            foreach(var item in sprites)Assert.That(item.Key.sprite,Is.SameAs(item.Value));
            Assert.That(art.sprite,Is.SameAs(body));Assert.That(art.color,Is.EqualTo(Color.white));Assert.That(art.transform.localScale,Is.EqualTo(scale));
            // Validate each configuration on the representative map before enabling
            // automatic application to the other original maps.
            atmosphere.SetAllRegionsEnabled(true);
            for(int region=0;region<10;region++)
            {atmosphere.ApplyRegion(region);Assert.That(atmosphere.PresetIndex,Is.EqualTo(region));Assert.That(atmosphere.ActiveParticleCount,Is.LessThanOrEqualTo(88));}
            atmosphere.SetEffectsEnabled(false);yield return null;
            foreach(var item in colors)Assert.That(item.Key.color,Is.EqualTo(item.Value),"Disabling must restore the original render colors");
            Assert.That(atmosphere.ActiveParticleCount,Is.Zero);
            atmosphere.ApplyRegion(0);atmosphere.SetAllRegionsEnabled(false);atmosphere.SetEffectsEnabled(true);
            AssertSilent();
            Report("pixel-atmosphere-map0",1,6,"Original campaign map rendered before/after in PlayMode. Original collider transforms, sprite references and player color/scale preserved. Six pixel-art fog/moon textures, bounded88 effects, reversible filter. Ten preset configurations checked on this map; not a human route test of all maps.");
        }
        [UnityTest] public IEnumerator WeaponDodgeParticlesAreDistinctAndExpireQuickly()
        {
            foreach(int weapon in new[]{0,2,3})
            {
                Context(0,weapon,0,0);var fx=DarklightActionParticles.Current;Call(fx,"Advance",2f);
                int count=fx.Count(DarklightActionParticleKind.Dodge);
                DarklightActionParticles.Emit(DarklightActionParticleKind.Dodge,fighter.transform.position,Vector2.down);
                string profile=weapon==0?"Katana":weapon==2?"Punhos":"Moto";
                Assert.That(fx.LastPresentedSprite.name,Does.Contain(profile));
                Assert.That(fx.LastLifetime,Is.EqualTo(.145f).Within(.001f));
                Assert.That(fx.Count(DarklightActionParticleKind.Dodge),Is.EqualTo(count+1));
                Call(fx,"Advance",.2f);Assert.That(fx.ActiveCount,Is.Zero);
                Assert.That(art.color,Is.EqualTo(Color.white));AssertSilent();yield return null;
            }
        }
    }
}
