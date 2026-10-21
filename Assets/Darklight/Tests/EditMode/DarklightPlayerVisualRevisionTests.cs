using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightPlayerVisualRevisionTests
    {
        const string Revised="Darklight/PlayerVisualRevision20261020/RequiemKatanaVioletFront";
        const string Original="Darklight/TransformedYoung/RequiemAttackKatanaYoungCardinal_Violet";
        [TearDown] public void Clear(){DarklightTransformedBank.ClearCache();DarklightCanonicalComplexion.ClearCache();}
        [Test] public void OnlyFourFrontalVioletKatanaFramesAreReplaced()
        {
            var normal=Resources.LoadAll<Sprite>("Darklight/TodayCharacterAnimations/RequiemAttackKatanaYoungCardinal");
            Assert.That(normal.Length,Is.EqualTo(16));
            foreach(var sprite in normal)
            {
                int index=int.Parse(sprite.name.Substring(sprite.name.LastIndexOf('_')+1));
                Assert.That(DarklightTransformedBank.TryResolve(sprite,2,out var frame,out var mask,out int owner),Is.True);
                Assert.That(owner,Is.EqualTo(index+1));Assert.That(mask,Is.Not.Null);
                string path=AssetDatabase.GetAssetPath(frame);
                Assert.That(path,Is.EqualTo("Assets/Darklight/Resources/"+(index<4?Revised:Original)+".png"));
                Assert.That(DarklightCanonicalComplexion.ForTexture(frame.texture).w,Is.EqualTo(1));
                foreach(int form in new[]{1,3})
                {
                    Assert.That(DarklightTransformedBank.TryResolve(sprite,form,out var other,out _,out _),Is.True);
                    Assert.That(AssetDatabase.GetAssetPath(other),Does.Not.Contain("PlayerVisualRevision20261020"));
                }
            }
        }
        [Test] public void SharpSheetImportsAtNativeSizeWithPointFilteringAndStablePivots()
        {
            var sprites=Resources.LoadAll<Sprite>(Revised).OrderBy(s=>s.name).ToArray();
            Assert.That(sprites.Length,Is.EqualTo(4));
            foreach(var sprite in sprites)
            {
                Assert.That(sprite.texture.width,Is.EqualTo(1792));Assert.That(sprite.texture.height,Is.EqualTo(448));
                Assert.That(sprite.pivot.x,Is.EqualTo(224).Within(.001));Assert.That(sprite.pivot.y,Is.EqualTo(48).Within(.001));
                Assert.That(sprite.pixelsPerUnit,Is.EqualTo(110.55276).Within(.001));
            }
            foreach(string suffix in new[]{"","_Owner"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Darklight/Resources/"+Revised+suffix+".png");
                Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                Assert.That(importer.sRGBTexture,Is.EqualTo(suffix==""));
            }
        }
        [TestCase("Darklight/PixelSprite")]
        [TestCase("Darklight/Remaster/SpriteLit")]
        [TestCase("Darklight/Prototype/YoungSpriteLit")]
        public void IrisShadersHaveNoCompilationErrors(string name)
        {
            var shader=Shader.Find(name);Assert.That(shader,Is.Not.Null);
            var errors=ShaderUtil.GetShaderMessages(shader).Where(m=>m.severity.ToString()=="Error").ToArray();
            Assert.That(errors,Is.Empty,string.Join("\n",errors.Select(e=>e.message)));
        }
    }
}
