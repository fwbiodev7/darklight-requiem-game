using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightMotorcycleRedesignTests
    {
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void EachMotorcycleFormUsesEightDistinctDrawnViewsAndFourExactSlamStages(int form)
        {
            DarklightMotorcycleRedesignBank.ClearCache();
            var views=new System.Collections.Generic.HashSet<Sprite>();
            for(int direction=0;direction<8;direction++)
            {
                var sprite=DarklightMotorcycleRedesignBank.Riding(form,direction);
                Assert.That(sprite,Is.Not.Null,"Missing authored motorcycle direction "+direction);
                Assert.That(views.Add(sprite),Is.True,"Asymmetric directions need separate drawings");
                Assert.That(DarklightMotorcycleRedesignBank.IsAuthored(sprite),Is.True);
            }
            var phases=new System.Collections.Generic.HashSet<Sprite>();
            foreach(float time in new[]{0f,.3f,.6f,.9f})
            {
                var sprite=DarklightMotorcycleRedesignBank.GroundSlam(form,0,time,.58f,.16f,.58f);
                Assert.That(sprite,Is.Not.Null);Assert.That(phases.Add(sprite),Is.True);
            }
            for(int direction=1;direction<8;direction++)
                Assert.That(DarklightMotorcycleRedesignBank.GroundSlam(form,direction,.6f,.58f,.16f,.58f),Is.Null,
                    "A frontal slam cannot stand in for another direction");
            Assert.That(DarklightMotorcycleRedesignBank.Riding(-1,0),Is.Null);
            Assert.That(DarklightMotorcycleRedesignBank.Riding(4,0),Is.Null);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void AuthoredMotorcycleClearsLegacyMasksAndPaletteWithoutTintingRider(int form)
        {
            var holder=new GameObject("Motorcycle palette verification");
            try
            {
                var renderer=holder.AddComponent<SpriteRenderer>();
                renderer.sprite=DarklightMotorcycleRedesignBank.Riding(form,6);Assert.That(renderer.sprite,Is.Not.Null);
                var properties=new MaterialPropertyBlock();
                foreach(string key in new[]{"_VampireForm","_DarkForm","_RuinFists","_WholeFrameId"})properties.SetFloat(key,3);
                properties.SetTexture("_VampireMask",Texture2D.whiteTexture);
                properties.SetVector("_CanonicalSkinShift",Vector4.one);renderer.SetPropertyBlock(properties);
                DarklightMotorcycleRedesignBank.ApplyOwnership(renderer);renderer.GetPropertyBlock(properties);
                foreach(string key in new[]{"_VampireForm","_DarkForm","_RuinFists","_WholeFrameId"})Assert.That(properties.GetFloat(key),Is.Zero,key);
                Assert.That(properties.GetTexture("_VampireMask"),Is.SameAs(Texture2D.blackTexture));
                Assert.That(properties.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
            }
            finally{Object.DestroyImmediate(holder);DarklightMotorcycleRedesignBank.ClearCache();}
        }
    }
}
