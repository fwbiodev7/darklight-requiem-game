using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightLoadingMascotTests
    {
        [Test] public void SelectorDividesTheUnitIntervalEqually()
        {
            int cats=0;
            for(int i=0;i<1000;i++)
                if(DarklightLoadingMascot.ChooseVariant((i+.5f)/1000)==DarklightLoadingMascot.Variant.GatoRuptura) cats++;
            Assert.That(cats,Is.EqualTo(500));
            Assert.That(DarklightLoadingMascot.ChooseVariant(0),Is.EqualTo(DarklightLoadingMascot.Variant.GatoRuptura));
            Assert.That(DarklightLoadingMascot.ChooseVariant(.5f),Is.EqualTo(DarklightLoadingMascot.Variant.MiniFabius));
            Assert.That(DarklightLoadingMascot.ChooseVariant(1),Is.EqualTo(DarklightLoadingMascot.Variant.MiniFabius));
        }

        [Test] public void BothAtlasesAndBackdropAreUncompressedPointAssets()
        {
            const string root="Assets/Darklight/Resources/Darklight/Loading/";
            foreach(var name in new[]{"Mascots/gato-ruptura.png","Mascots/mini-fabius.png","gothic-loading-20261117.png"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(root+name);
                Assert.That(importer,Is.Not.Null,name);
                Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(importer.mipmapEnabled,Is.False);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                Assert.That(importer.npotScale,Is.EqualTo(TextureImporterNPOTScale.None));
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(root+name);
                Assert.That(texture,Is.Not.Null);
                if(name.StartsWith("Mascots/"))
                {
                    Assert.That(importer.DoesSourceTextureHaveAlpha(),Is.True);
                    Assert.That(texture.width%4,Is.Zero);
                    Assert.That(texture.height%2,Is.Zero);
                    Assert.That(texture.width/4,Is.EqualTo(texture.height/2),"Square frame cells preserve proportions");
                }
            }
            Assert.That(Resources.Load<Sprite>("Darklight/Loading/gothic-loading-20261117"),Is.Not.Null);
        }
    }
}
