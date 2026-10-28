using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightVisitorPersistenceTests
    {
        IDisposable storage;
        string path;

        [SetUp] public void IsolateSave()
        {
            path=Path.GetFullPath("work/session-20261024/persistence/"+Guid.NewGuid().ToString("N")+".json");
            storage=DarklightProgress.IsolateStorageForTests(path);
        }
        [TearDown] public void RestoreStorage()=>storage?.Dispose();

        [Test] public void OneHundredDiskReloadsPreserveEveryReadAndUniqueVisitorReward()
        {
            var state=new DarklightProgress{weaponUnlocks=15,equippedWeapon=2,formsUnlocked=3,preferredForm=3};
            for(int i=0;i<DarklightMultiverseCatalog.Entries.Length;i++)
                if(state.ReadMultiverse(i))state.Award(15);
            foreach(var visitor in new[]{DarklightVisitor.MossDetonator,DarklightVisitor.NeonRevenant,DarklightVisitor.AshRonin})
                Assert.That(state.ClaimVisitor(visitor),Is.True);
            int level=state.level,xp=state.xp,points=state.points;
            for(int cycle=0;cycle<100;cycle++)
            {
                state.chapter=cycle%DarklightProgress.ChapterCapacity;state.checkpoint=cycle%5;state.Save();
                state=DarklightProgress.Load();Assert.That(state,Is.Not.Null,"Disk reload "+cycle);
                Assert.That(state.chapter,Is.EqualTo(cycle%10));Assert.That(state.checkpoint,Is.EqualTo(cycle%5));
                Assert.That(state.multiverseRead,Is.EqualTo((1<<DarklightMultiverseCatalog.Entries.Length)-1));
                Assert.That(state.multiverseDefeated,Is.EqualTo(14));Assert.That(state.shards,Is.EqualTo(3));
                Assert.That(state.level,Is.EqualTo(level));Assert.That(state.xp,Is.EqualTo(xp));Assert.That(state.points,Is.EqualTo(points));
                Assert.That(state.weaponUnlocks,Is.EqualTo(15));Assert.That(state.equippedWeapon,Is.EqualTo(2));
                Assert.That(state.formsUnlocked,Is.EqualTo(3));Assert.That(state.preferredForm,Is.EqualTo(3));
                for(int i=0;i<DarklightMultiverseCatalog.Entries.Length;i++)Assert.That(state.ReadMultiverse(i),Is.False);
                foreach(var visitor in new[]{DarklightVisitor.MossDetonator,DarklightVisitor.NeonRevenant,DarklightVisitor.AshRonin})
                    Assert.That(state.ClaimVisitor(visitor),Is.False,"Reward repeated after reload "+cycle);
            }
            Assert.That(File.Exists(path+".bak"),Is.True);Assert.That(File.Exists(path+".tmp"),Is.False);
        }

        [Test] public void CorruptPrimaryRecoversDefeatedVisitorsFromAtomicBackup()
        {
            var state=new DarklightProgress{chapter=3,checkpoint=2};
            for(int i=1;i<=3;i++)Assert.That(state.ClaimVisitor((DarklightVisitor)i),Is.True);
            state.Save();state.checkpoint=3;state.Save();File.WriteAllText(path,"interrupted write");
            var restored=DarklightProgress.Load();Assert.That(restored,Is.Not.Null);
            Assert.That(restored.chapter,Is.EqualTo(3));Assert.That(restored.checkpoint,Is.EqualTo(2));
            Assert.That(restored.multiverseDefeated,Is.EqualTo(14));Assert.That(restored.shards,Is.EqualTo(3));
            for(int i=1;i<=3;i++)Assert.That(restored.ClaimVisitor((DarklightVisitor)i),Is.False);
        }


        [TestCase(-1)] [TestCase(0)] [TestCase(4)] [TestCase(int.MaxValue)]
        public void InvalidVisitorsCannotMutateProgress(int visitor)
        {
            var state=new DarklightProgress();string before=JsonUtility.ToJson(state);
            Assert.That(state.ClaimVisitor((DarklightVisitor)visitor),Is.False);
            Assert.That(JsonUtility.ToJson(state),Is.EqualTo(before));
        }
    }
}
