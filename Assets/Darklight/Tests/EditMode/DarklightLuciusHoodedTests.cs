using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightLuciusHoodedTests
    {
        const DarklightLuciusAppearance Hooded=DarklightLuciusAppearance.Hooded;
        const string Sheet="LuciusHoodedIdle8V1";
        const string Hash="518fd6918833ba75bb04eab1127775c6aa8669f4cacf6265399e555bb81694a0";
        static readonly int[] SoleX={239,187,154,150,226,205,186,188},SoleY={449,449,449,449,448,448,448,450};
        static DarklightLuciusSpriteCatalog Catalog(string root)=>JsonUtility.FromJson<DarklightLuciusSpriteCatalog>(Resources.Load<TextAsset>(root+"catalog").text);
        static string Sha(string path){using var sha=SHA256.Create();return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
        [TearDown] public void Clear()=>DarklightLuciusSpriteBank.ClearCache();

        [Test] public void HoodIsAnAppearanceWithItsOwnValidatedPacketAndNoNewForm()
        {
            var data=Catalog(DarklightLuciusSpriteBank.HoodedRoot);
            Assert.That(data.form,Is.EqualTo("Normal"));Assert.That(data.appearance,Is.EqualTo("Hooded"));
            Assert.That(data.complete,Is.False);Assert.That(data.pages,Has.Length.EqualTo(8));
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data,Hooded),Is.True);
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data),Is.False,"Normal API cannot accept the other appearance packet");
            data.pages[0].resourcePath=DarklightLuciusSpriteBank.Root+"LuciusNormalIdle8V1";
            Assert.That(DarklightLuciusSpriteBank.ValidCatalog(data,Hooded),Is.False);
            Assert.That(DarklightLuciusSpriteBank.TryPose((DarklightLuciusAppearance)99,"Idle",0,0,out _,out _),Is.False);
        }

        [Test] public void PreservedFinalPixelsAndSharedBodyCalibrationAreUsed()
        {
            string runtime="Assets/Darklight/Resources/"+DarklightLuciusSpriteBank.HoodedRoot+Sheet+".png";
            Assert.That(Sha(runtime),Is.EqualTo(Hash));
            Assert.That(Sha("ArtSources/LuciusHooded20261109/selected/HoodedIdle8Final-source.png"),Is.EqualTo(Hash));
            var normal=Catalog(DarklightLuciusSpriteBank.Root);var hood=Catalog(DarklightLuciusSpriteBank.HoodedRoot);
            float ppu=normal.pages[0].pixelsPerUnit;
            Assert.That(ppu,Is.EqualTo(412f/(302f/(520f/3.3f))).Within(.001f),"Approved Normal calibration uses Fabius body, not cloak/sword/hood bounds");
            Assert.That(hood.pages.All(p=>Mathf.Abs(p.pixelsPerUnit-ppu)<.001f),Is.True);
        }

        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void EachNativeViewHasStableSlicePivotDensityAndExplicitStaticFallback(int direction)
        {
            var data=Catalog(DarklightLuciusSpriteBank.HoodedRoot);var page=data.pages.Single(p=>p.direction==direction);
            Assert.That(page.spriteNames,Has.Length.EqualTo(1),"Eight static views are not animated cycles");
            Assert.That(DarklightLuciusSpriteBank.TryPose(Hooded,"Idle",direction,100,out var sprite,out bool fallback),Is.True);
            Assert.That(fallback,Is.False);Assert.That(sprite.name,Is.EqualTo("Redesign_"+Sheet+"_"+direction.ToString("00")));
            Assert.That(sprite.rect,Is.EqualTo(new Rect(direction%4*384,1024-direction/4*512-512,384,512)));
            Assert.That(sprite.pivot.x,Is.EqualTo(SoleX[direction]).Within(.001f));
            Assert.That(sprite.pivot.y,Is.EqualTo(512-SoleY[direction]).Within(.001f));
            Assert.That(sprite.pixelsPerUnit,Is.EqualTo(page.pixelsPerUnit).Within(.001f));
            Assert.That(sprite.texture.width,Is.EqualTo(1536));Assert.That(sprite.texture.height,Is.EqualTo(1024));
            Assert.That(DarklightLuciusSpriteBank.Frame(page,100,Hooded),Is.Zero);
            Assert.That(DarklightLuciusSpriteBank.TryPose(Hooded,"Walk",direction,1,out var walk,out fallback),Is.True);
            Assert.That(fallback,Is.True);Assert.That(walk,Is.SameAs(sprite));
        }

        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void VisibleSolesStayOnActorGroundAtCanonicalScale(int direction)
        {
            var actor=new GameObject("HoodedEditModeContact");var pixels=new Texture2D(2,2,TextureFormat.RGBA32,false);
            try
            {
                Assert.That(ImageConversion.LoadImage(pixels,File.ReadAllBytes("Assets/Darklight/Resources/"+DarklightLuciusSpriteBank.HoodedRoot+Sheet+".png")),Is.True);
                var view=actor.AddComponent<DarklightLuciusView>();view.appearance=Hooded;
                actor.transform.position=new Vector3(3.25f,-2.5f,0);
                Assert.That(view.PresentPose("Idle",direction,0),Is.True);var art=view.Renderer;var sprite=art.sprite;
                var rgba=pixels.GetPixels32();int bottom=-1;
                for(int y=0;y<512&&bottom<0;y++)for(int x=0;x<384;x++)
                    if(rgba[((int)sprite.rect.y+y)*pixels.width+(int)sprite.rect.x+x].a>=128){bottom=y;break;}
                Assert.That(bottom,Is.GreaterThanOrEqualTo(0));
                var ground=art.transform.TransformPoint(new Vector3(0,(bottom-sprite.pivot.y)/sprite.pixelsPerUnit,0));
                Assert.That(ground.y,Is.EqualTo(actor.transform.position.y).Within(.00001f),"Real decoded boot pixels touch ground; no transparent-padding bounds assumption");
                Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
                Assert.That(art.sortingOrder,Is.EqualTo(DarklightSorting.OrderForY(actor.transform.position.y)));
                Assert.That(art.flipX||art.flipY,Is.False);Assert.That(art.color,Is.EqualTo(Color.white));
            }
            finally{UnityEngine.Object.DestroyImmediate(actor);UnityEngine.Object.DestroyImmediate(pixels);}
        }

        [Test] public void SwitchingOptInAppearancePreservesNormalCacheAndOneRenderer()
        {
            var actor=new GameObject("LuciusOptInAppearance");int audio=UnityEngine.Object.FindObjectsByType<AudioSource>().Length;
            try
            {
                var view=actor.AddComponent<DarklightLuciusView>();Assert.That(view.appearance,Is.EqualTo(DarklightLuciusAppearance.Normal));
                for(int direction=0;direction<8;direction++)
                {
                    Assert.That(view.PresentPose("Idle",direction,0),Is.True);var normal=view.Renderer.sprite;var renderer=view.Renderer;
                    view.appearance=Hooded;Assert.That(view.PresentPose("Idle",direction,0),Is.True);
                    Assert.That(view.Renderer.sprite,Is.Not.SameAs(normal));Assert.That(view.UsesIdleFallback,Is.False);
                    view.appearance=DarklightLuciusAppearance.Normal;Assert.That(view.PresentPose("Idle",direction,0),Is.True);
                    Assert.That(view.Renderer.sprite,Is.SameAs(normal));Assert.That(view.Renderer,Is.SameAs(renderer));
                }
                Assert.That(actor.GetComponentsInChildren<SpriteRenderer>(),Has.Length.EqualTo(1));
                Assert.That(actor.GetComponentsInChildren<Component>().All(c=>c is Transform||c is SpriteRenderer||c is DarklightLuciusView),Is.True);
                Assert.That(UnityEngine.Object.FindObjectsByType<AudioSource>(),Has.Length.EqualTo(audio));
            }
            finally{UnityEngine.Object.DestroyImmediate(actor);}
        }
    }
}
