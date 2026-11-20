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
            Assert.That(decoded.LadyAvailable && decoded.NeroDefeated && decoded.NeroSurvived && decoded.NeroRevenge, Is.True);
            Assert.That(decoded.RivalryPending, Is.True);
            Assert.That(decoded.TruceStarted, Is.False);
        }

        [Test]
        public void CompletedLegacyFinaleKeepsTheTruceClosed()
        {
            var progress = DarklightProgress.Decode(JsonUtility.ToJson(new DarklightProgress
            {
                chapter = 9, checkpoint = 4, luciusFate = 1, completed = true
            }));
            Assert.That(progress.NeroSurvived && progress.ThreatRecognized && progress.TruceStarted &&
                progress.TruceEnded && progress.RivalryPending, Is.True);
        }

        [Test]
        public void IntentionalLadyAvailabilitySurvivesSaveRoundTrip()
        {
            var progress = new DarklightProgress();
            progress.SetLadyAvailable(false);
            Assert.That(DarklightProgress.Decode(JsonUtility.ToJson(progress)).LadyAvailable, Is.False);
        }

        [Test]
        public void LadyHasFiveWalkAndShotFramesPerDirection()
        {
            var sprites = Resources.LoadAll<Sprite>("Darklight/OfficialCast/LadySupport");
            Assert.That(sprites, Has.Length.EqualTo(80));
            var names = new HashSet<string>();
            foreach (var sprite in sprites)
            {
                Assert.That(names.Add(sprite.name), Is.True);
                Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
                Assert.That(sprite.pivot.y, Is.GreaterThan(0));
            }
            foreach (string action in new[] { "Walk", "Shot" })
                foreach (string direction in new[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" })
                    for (int frame = 0; frame < 5; frame++)
                        Assert.That(names.Contains($"Lady_{action}_{direction}_{frame}"), Is.True);
        }

        [Test]
        public void NewCastActionsKeepDistinctFramesAndTheirGroundPivots()
        {
            var sprites = Resources.LoadAll<Sprite>("Darklight/OfficialCast/CastActions");
            Assert.That(sprites, Has.Length.EqualTo(100));
            var names = new HashSet<string>();
            foreach (var sprite in sprites)
            {
                Assert.That(names.Add(sprite.name), Is.True);
                Assert.That(sprite.pivot.y, Is.GreaterThan(0));
                Assert.That(sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            }
            foreach (string actor in new[] { "MatriarcaRubra", "Nero" })
                foreach (string action in new[] { "Attack", "Defeat" })
                    foreach (string direction in new[] { "S", "E", "N" })
                        for (int phase = 0; phase < 5; phase++)
                            Assert.That(names.Contains($"{actor}_{action}_{direction}_{phase}"), Is.True);
            foreach (string direction in new[] { "S", "SW", "W", "NW", "N", "NE", "E", "SE" })
                for (int phase = 0; phase < 5; phase++)
                    Assert.That(names.Contains($"Lady_Defeat_{direction}_{phase}"), Is.True);
        }
    }
}
