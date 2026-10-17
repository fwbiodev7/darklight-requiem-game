using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightReturnPointTests
    {
        [Test] public void OldSavesKeepTheirExistingChapterAndCheckpointFallback()
        {
            var p=DarklightProgress.Decode("{\"version\":2,\"chapter\":3,\"checkpoint\":2,\"level\":4}");
            Assert.That(p,Is.Not.Null);Assert.That(p.checkpoint,Is.EqualTo(2));
            Assert.That(p.TryReturnPoint(3,2,out _),Is.False);
        }

        [Test] public void ActivatingAnotherSpawnPointReplacesThePreviousPoint()
        {
            var p=new DarklightProgress();p.RecordReturnPoint(2,1,new Vector2(4,5));
            Assert.That(p.RecordReturnPoint(2,1,new Vector2(8,9)),Is.True);
            Assert.That(p.TryReturnPoint(2,1,out var point),Is.True);
            Assert.That(point,Is.EqualTo(new Vector2(8,9)));
        }

        [Test] public void AReturnPointCannotLeakIntoAnotherChapterOrLaterEncounter()
        {
            var p=new DarklightProgress();p.RecordReturnPoint(2,1,new Vector2(4,5));
            Assert.That(p.TryReturnPoint(3,1,out _),Is.False);
            Assert.That(p.TryReturnPoint(2,2,out _),Is.False);
        }

        [Test] public void NewFieldsRoundTripWithoutLosingExistingCampaignProgress()
        {
            var p=new DarklightProgress{chapter=7,checkpoint=3,level=11,weaponUnlocks=15};
            p.RecordReturnPoint(7,3,new Vector2(12.5f,8.25f));
            var loaded=DarklightProgress.Decode(JsonUtility.ToJson(p));
            Assert.That(loaded.level,Is.EqualTo(11));Assert.That(loaded.weaponUnlocks,Is.EqualTo(15));
            Assert.That(loaded.TryReturnPoint(7,3,out var point),Is.True);
            Assert.That(point,Is.EqualTo(p.returnPoint));
        }

        [Test] public void InvalidCoordinatesNeverReplaceAnExistingValidSpawn()
        {
            var p=new DarklightProgress();p.RecordReturnPoint(1,0,new Vector2(4,5));
            Assert.That(p.RecordReturnPoint(1,0,new Vector2(float.NaN,3)),Is.False);
            Assert.That(p.RecordReturnPoint(1,0,new Vector2(3,float.PositiveInfinity)),Is.False);
            Assert.That(p.TryReturnPoint(1,0,out var point),Is.True);
            Assert.That(point,Is.EqualTo(new Vector2(4,5)));
        }

        [Test] public void CompletedChapterReturnPointsRemainValid()
        {
            var p=new DarklightProgress();Assert.That(p.RecordReturnPoint(9,4,new Vector2(1,2)),Is.True);
            Assert.That(p.TryReturnPoint(9,4,out _),Is.True);
        }
    }
}
