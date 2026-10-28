using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightReturnFileTests
    {
        string directory,path;
        [SetUp] public void Prepare()
        {
            directory=System.IO.Path.GetFullPath("work/return-save-tests/"+Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);path=System.IO.Path.Combine(directory,"campaign.json");
        }
        [TearDown] public void Clean()
        {
            File.Delete(path);File.Delete(path+".bak");Directory.Delete(directory);
        }
        [Test] public void PrimaryWinsAndTheReaderDoesNotChangeEitherFile()
        {
            var saved=new DarklightProgress{chapter=3,checkpoint=2,level=9,equippedWeapon=1,weaponUnlocks=7};
            saved.RecordReturnPoint(3,2,new Vector2(10,12));
            string primary=JsonUtility.ToJson(saved),backup=JsonUtility.ToJson(new DarklightProgress{chapter=1});
            File.WriteAllText(path,primary);File.WriteAllText(path+".bak",backup);
            var restored=DarklightProgress.LoadFromPath(path);
            Assert.That(restored.chapter,Is.EqualTo(3));Assert.That(restored.level,Is.EqualTo(9));
            Assert.That(restored.equippedWeapon,Is.EqualTo(1));Assert.That(restored.TryReturnPoint(3,2,out var point),Is.True);
            Assert.That(point,Is.EqualTo(new Vector2(10,12)));
            Assert.That(File.ReadAllText(path),Is.EqualTo(primary));Assert.That(File.ReadAllText(path+".bak"),Is.EqualTo(backup));
        }
        [Test] public void CorruptPrimaryFallsBackToTheBackupWithoutOverwritingIt()
        {
            File.WriteAllText(path,"not valid json");File.WriteAllText(path+".bak",JsonUtility.ToJson(new DarklightProgress{chapter=4,level=6}));
            Assert.That(DarklightProgress.LoadFromPath(path).chapter,Is.EqualTo(4));
            Assert.That(File.ReadAllText(path),Is.EqualTo("not valid json"));
        }
        [Test] public void MissingPrimaryCanRestoreAnOldBackupWithoutCoordinates()
        {
            File.WriteAllText(path+".bak","{\"version\":2,\"chapter\":2,\"checkpoint\":1,\"level\":5}");
            var restored=DarklightProgress.LoadFromPath(path);
            Assert.That(restored.level,Is.EqualTo(5));Assert.That(restored.checkpoint,Is.EqualTo(1));
            Assert.That(restored.TryReturnPoint(2,1,out _),Is.False);Assert.That(File.Exists(path),Is.False);
        }
        [Test] public void NoFilesLeavesTheCallerFreeToUseItsInMemorySpawnFallback()
        {
            Assert.That(DarklightProgress.LoadFromPath(path),Is.Null);
            Assert.That(DarklightProgress.LoadFromPath(""),Is.Null);Assert.That(File.Exists(path),Is.False);
        }
    }
}
