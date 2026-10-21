using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightCanonicalComplexionTests
    {
        [Test] public void PlayerTexturesShareTheRequiemRestPaletteAndKeepOwnerProperties()
        {
            var source=Resources.Load<TextAsset>("Darklight/CanonicalComplexion");Assert.That(source,Is.Not.Null);
            var data=JsonUtility.FromJson<DarklightCanonicalComplexion.Data>(source.text);
            Assert.That(data.reference,Does.Contain("2026-10-14 120511"));Assert.That(data.entries.Length,Is.GreaterThanOrEqualTo(261));
            CollectionAssert.AreEqual(new float[]{246,174,130},data.referenceMedian);
            var names=new System.Collections.Generic.HashSet<string>();
            foreach(var entry in data.entries)
            {
                Assert.That(names.Add(entry.texture),Is.True,"Duplicate texture palette: "+entry.texture);
                Assert.That(entry.shift.Length,Is.EqualTo(3));Assert.That(entry.sourceMedian.Length,Is.EqualTo(3));
                for(int channel=0;channel<3;channel++)
                {
                    Assert.That(float.IsNaN(entry.shift[channel])||float.IsInfinity(entry.shift[channel]),Is.False);
                    Assert.That(Mathf.Abs(entry.shift[channel]),Is.LessThanOrEqualTo(.30f));
                    Assert.That(entry.sourceMedian[channel]+entry.shift[channel]*255,Is.EqualTo(data.referenceMedian[channel]).Within(.001f),entry.texture);
                }
            }
            var sprite=TodayCharacterBank.YoungIdle(1,0,0,0);var go=new GameObject("PaletteFixture");
            try
            {
                var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=sprite;var block=new MaterialPropertyBlock();
                block.SetFloat("_WholeFrameId",4);renderer.SetPropertyBlock(block);DarklightCanonicalComplexion.Apply(renderer);
                renderer.GetPropertyBlock(block);Assert.That(block.GetFloat("_WholeFrameId"),Is.EqualTo(4));
                Assert.That(block.GetVector("_CanonicalSkinShift").w,Is.EqualTo(1));
                DarklightCanonicalComplexion.Apply(renderer,false);renderer.GetPropertyBlock(block);
                Assert.That(block.GetVector("_CanonicalSkinShift"),Is.EqualTo(Vector4.zero));
            }
            finally{Object.DestroyImmediate(go);}
        }
        [Test] public void LegacyTexturesDoNotReceiveTheNewPalette()
        {
            var image=new Texture2D(1,1){name="LegacyProtectedDeath"};
            try{Assert.That(DarklightCanonicalComplexion.ForTexture(image),Is.EqualTo(Vector4.zero));}
            finally{Object.DestroyImmediate(image);DarklightCanonicalComplexion.ClearCache();}
        }
    }
}
