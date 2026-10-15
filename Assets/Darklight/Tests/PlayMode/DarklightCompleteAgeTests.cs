using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightCompleteAgeTests
    {
        [Test] public void OnlyYoungSheetsRemainAndMovementAndAttackCoverEveryWeaponDirection()
        {
            var catalog=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            Assert.That(catalog.pages.All(p=>p.age==0&&p.name.Contains("Young")&&!p.name.Contains("Stage")),Is.True);
            for(int skin=0;skin<2;skin++)for(int weapon=0;weapon<3;weapon++)for(int d=0;d<8;d++)
            foreach(string kind in new[]{"Walk","Attack"})
                Assert.That(TodayCharacterBank.Page(skin,0,weapon,d,kind),Is.Not.Null,$"Missing {skin}/{weapon}/{d}/{kind}");
            for(int age=1;age<5;age++)foreach(string skin in new[]{"Rock","Requiem"})
                Assert.That(Resources.LoadAll<Sprite>("Darklight/AppearanceBodies/"+skin+"IdleStage"+age),Is.Empty);
            Assert.That(Resources.Load<DarklightCompleteAgeCatalog>("Darklight/CompleteAgeCatalog"),Is.Null);
        }
        [Test] public void YoungFramesKeepPointSamplingGroundPivotsAndMatchingPalettes()
        {
            var catalog=Resources.Load<TodayCharacterCatalog>("Darklight/TodayCharacterCatalog");
            foreach(var page in catalog.pages)foreach(var frame in Resources.LoadAll<Sprite>(page.resourcePath))
            {
                Assert.That(frame.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(frame.texture.mipmapCount,Is.EqualTo(1));
                Assert.That(frame.pixelsPerUnit,Is.GreaterThan(0));var mask=TodayCharacterBank.Mask(frame);Assert.That(mask,Is.Not.Null);
                Assert.That(mask.width,Is.EqualTo(frame.texture.width));Assert.That(mask.height,Is.EqualTo(frame.texture.height));
            }
        }
    }
}
