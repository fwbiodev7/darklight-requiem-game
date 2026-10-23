using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightConsistencyTests
    {
        [Test] public void SixWalkingPosesUseEveryFrameAndKeepTheOriginalStrideDuration()
        {
            for(int phases=4;phases<=6;phases+=2)
            {
                var seen=Enumerable.Range(0,phases).Select(i=>TodayCharacterBank.WalkFrame(phases,(i+.25f)*.4f/phases)).ToArray();
                Assert.That(seen,Is.EqualTo(Enumerable.Range(0,phases).ToArray()));
                for(int i=0;i<phases;i++)Assert.That(TodayCharacterBank.WalkFrame(phases,(i+.25f)*.4f/phases+.4f),Is.EqualTo(i));
            }
        }
        [Test] public void RequiemWalksCoverThreeWeaponsEightDirectionsAndAllFormsWithUniformAtlasScale()
        {
            if(!System.IO.File.Exists("Assets/Darklight/ArtSources/ConsistentWalk20261017/manifest.json"))Assert.Ignore("New complete walk sources are staged outside production pending visual QA/import.");
            TodayCharacterBank.ClearCache();DarklightTransformedBank.ClearCache();
            var catalog=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            Assert.That(catalog.pages.Count(p=>p.skin==0&&p.kind=="Walk"),Is.EqualTo(6));
            for(int weapon=0;weapon<3;weapon++)for(int direction=0;direction<8;direction++)
            {
                var page=TodayCharacterBank.Page(0,0,weapon,direction,"Walk");
                Assert.That(page.name,Does.Contain("WalkConsistent"));Assert.That(page.phases,Is.EqualTo(6));
                Assert.That(TodayCharacterBank.ValidDescriptor(page),Is.True);
                var frames=Resources.LoadAll<Sprite>(page.resourcePath).ToDictionary(s=>s.name);int row=Array.IndexOf(page.directions,direction);
                foreach(var normal in page.spriteNames.Skip(row*6).Take(6).Select(n=>frames[n]))
                {
                    Assert.That(normal.rect.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(normal.rect.yMin,Is.GreaterThanOrEqualTo(0));
                    Assert.That(normal.rect.xMax,Is.LessThanOrEqualTo(normal.texture.width));Assert.That(normal.rect.yMax,Is.LessThanOrEqualTo(normal.texture.height));
                    Assert.That(TodayCharacterBank.WholeFrameId(normal),Is.InRange(1,24));
                    for(int form=1;form<=3;form++)
                    {
                        Assert.That(DarklightTransformedBank.TryResolve(normal,form,out var formed,out var mask,out int owner),Is.True);
                        Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.InRange(1,24));
                        Assert.That(formed.bounds.size.y/normal.bounds.size.y,Is.InRange(.85f,1.15f));
                        Assert.That(formed.rect.height/formed.pixelsPerUnit,Is.InRange(1.55f,2.65f));
                    }
                }
            }
        }
        [Test] public void CorrectedSawFramesHaveWholeOwnershipAndNoOversizedRendererScaling()
        {
            if(!System.IO.File.Exists("Assets/Darklight/ArtSources/ConsistentAbyss20261017/manifest.json"))Assert.Ignore("New complete saw sources await production import.");
            var text=Resources.Load<TextAsset>("Darklight/AbyssPrototype/WholeOwnership");Assert.That(text,Is.Not.Null);
            var pages=JsonUtility.FromJson<DarklightAbyssSpriteBank.WholeManifest>(text.text).pages.Where(p=>!p.name.StartsWith("ActionF")).ToArray();Assert.That(pages.Length,Is.EqualTo(8));
            var go=new GameObject("WholeSawOwner");
            try
            {
                var renderer=go.AddComponent<SpriteRenderer>();var block=new MaterialPropertyBlock();
                foreach(var page in pages)
                {
                    var frames=Resources.LoadAll<Sprite>("Darklight/AbyssPrototype/"+page.name);Assert.That(frames.Length,Is.EqualTo(6));
                    foreach(var sprite in frames)
                    {
                        renderer.sprite=sprite;DarklightAbyssSpriteBank.ApplyWholeOwnership(renderer);renderer.GetPropertyBlock(block);
                        Assert.That(block.GetFloat("_WholeFrameId"),Is.InRange(1,6));Assert.That(block.GetTexture("_VampireMask"),Is.Not.Null);
                        Assert.That(sprite.pixelsPerUnit,Is.EqualTo(197));
                        Assert.That(sprite.rect.height/sprite.pixelsPerUnit,Is.InRange(1.6f,2.65f));
                    }
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(go);DarklightAbyssSpriteBank.ClearCache();}
        }
    }
}
