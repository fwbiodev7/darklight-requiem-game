using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightCanonicalComplexionTests
    {
        [Test] public void NewlyAuthoredTexturesHaveBoundedReferencePaletteAndKeepOwnerProperties()
        {
            var source=Resources.Load<TextAsset>("Darklight/CanonicalComplexion");Assert.That(source,Is.Not.Null);
            var data=JsonUtility.FromJson<DarklightCanonicalComplexion.Data>(source.text);
            int expected=System.IO.File.Exists("Assets/Darklight/ArtSources/ConsistentWalk20261017/manifest.json")?64:32;
            if(System.IO.File.Exists("Assets/Darklight/ArtSources/AbyssDirectional20261018/manifest.json"))expected+=12;
            Assert.That(data.reference,Does.Contain("2026-10-14 120511"));Assert.That(data.entries.Length,Is.EqualTo(expected));
            foreach(var entry in data.entries)foreach(float value in entry.shift)Assert.That(Mathf.Abs(value),Is.LessThanOrEqualTo(.16f));
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
