using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightCampaignRouteTests
    {
        [Test]
        public void OnlyMatriarchAndNeroGateTheTenMapsBeforeTheFinalPortal()
        {
            Assert.That(DarklightStory.Chapters, Has.Length.EqualTo(10));
            var mandatory = new List<(int Chapter, RequiemEnemyKind Kind)>();
            for (int chapter = 0; chapter < DarklightStory.Chapters.Length; chapter++)
            {
                Assert.That(DarklightCampaignRoute.GateTitle(chapter), Is.Not.Empty);
                Assert.That(DarklightMapDefinition.ForChapter(chapter).Arenas, Has.Length.EqualTo(4));
                if (DarklightCampaignRoute.TryBoss(chapter, out var kind))
                    mandatory.Add((chapter, kind));
            }
            CollectionAssert.AreEqual(new[]
            {
                (2, RequiemEnemyKind.Matriarch),
                (8, RequiemEnemyKind.KaelFractured)
            }, mandatory);
            Assert.That(DarklightCampaignRoute.FinalChapter, Is.EqualTo(9));
        }

        [TestCase(3)]
        [TestCase(4)]
        public void OldNoctharCheckpointKeepsProgressAndDropsLegacyBossPhase(int checkpoint)
        {
            var original = new DarklightProgress
            {
                chapter = 9, checkpoint = checkpoint, bossPhase = 3, level = 8,
                xp = 120, shards = 7, luciusStance = 2, expansionFlags = DarklightCampaign.KaelPurified
            };
            original.relics[0] = true;
            var decoded = DarklightProgress.Decode(JsonUtility.ToJson(original));
            Assert.That(decoded, Is.Not.Null);
            Assert.That(decoded.chapter, Is.EqualTo(9));
            Assert.That(decoded.checkpoint, Is.EqualTo(checkpoint));
            Assert.That(decoded.bossPhase, Is.EqualTo(1));
            Assert.That(decoded.level, Is.EqualTo(8));
            Assert.That(decoded.xp, Is.EqualTo(120));
            Assert.That(decoded.shards, Is.EqualTo(7));
            Assert.That(decoded.relics[0], Is.True);
            Assert.That(decoded.luciusStance, Is.EqualTo(2));
            Assert.That(decoded.expansionFlags, Is.EqualTo(DarklightCampaign.KaelPurified));
        }
    }
}
