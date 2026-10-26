using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    [TestFixture]
    public sealed class DarklightSaveContractTests
    {
        string isolatedSavePath;
        IDisposable storageScope;

        [SetUp]
        public void SetUp()
        {
            string testDir = DarklightTestOutput.GetDirectory("save-contracts");
            isolatedSavePath = Path.Combine(testDir, "test-save-" + Guid.NewGuid().ToString("N") + ".json").Replace('\\', '/');
            storageScope = DarklightProgress.IsolateStorageForTests(isolatedSavePath);
        }

        [TearDown]
        public void TearDown()
        {
            storageScope?.Dispose();
            foreach (string ext in new[] { "", ".bak", ".tmp" })
            {
                string target = isolatedSavePath + ext;
                if (File.Exists(target))
                {
                    try { File.Delete(target); } catch { }
                }
            }
        }

        [Test]
        public void NewProgressHasConsistentInitialContract()
        {
            var p = new DarklightProgress();
            Assert.That(p.version, Is.EqualTo(2), "Contract version must be 2");
            Assert.That(p.level, Is.EqualTo(1));
            Assert.That(p.potions, Is.EqualTo(3));
            Assert.That(p.weaponUnlocks, Is.EqualTo(1), "Initial save must unlock Katana (slot 0)");
            Assert.That(p.equippedWeapon, Is.EqualTo(0));
            Assert.That(p.chapter, Is.EqualTo(0));
            Assert.That(p.checkpoint, Is.EqualTo(0));
        }

        [Test]
        public void SaveAndLoadRoundtripPreservesAllCoreProgressFields()
        {
            var original = new DarklightProgress
            {
                chapter = 2,
                checkpoint = 3,
                level = 5,
                xp = 120,
                points = 4,
                attack = 2,
                vitality = 1,
                mastery = 3,
                potions = 2,
                shards = 15,
                weaponUnlocks = 7, // Katana (1) | Daggers (2) | Chain/Punhos (4)
                equippedWeapon = 2, // Punhos da Ruina slot
                difficulty = 1
            };

            original.Save();
            Assert.That(File.Exists(isolatedSavePath), Is.True, "Primary save file must exist on disk");

            var loaded = DarklightProgress.Load();
            Assert.That(loaded, Is.Not.Null);
            Assert.That(loaded.chapter, Is.EqualTo(2));
            Assert.That(loaded.checkpoint, Is.EqualTo(3));
            Assert.That(loaded.level, Is.EqualTo(5));
            Assert.That(loaded.xp, Is.EqualTo(120));
            Assert.That(loaded.points, Is.EqualTo(4));
            Assert.That(loaded.attack, Is.EqualTo(2));
            Assert.That(loaded.vitality, Is.EqualTo(1));
            Assert.That(loaded.mastery, Is.EqualTo(3));
            Assert.That(loaded.weaponUnlocks, Is.EqualTo(7));
            Assert.That(loaded.equippedWeapon, Is.EqualTo(2), "Slot 2 (Chain/Punhos) must be preserved exactly");
            Assert.That(loaded.difficulty, Is.EqualTo(1));
        }

        [Test]
        public void CorruptPrimarySaveFallsBackGracefullyToBackup()
        {
            var backup = new DarklightProgress
            {
                chapter = 1,
                checkpoint = 4,
                level = 3,
                weaponUnlocks = 3,
                equippedWeapon = 1
            };

            // Write valid backup file and corrupt primary file
            string backupPath = isolatedSavePath + ".bak";
            File.WriteAllText(backupPath, JsonUtility.ToJson(backup));
            File.WriteAllText(isolatedSavePath, "corrupt_invalid_json");

            var recovered = DarklightProgress.Load();
            Assert.That(recovered, Is.Not.Null, "Must recover from valid .bak file");
            Assert.That(recovered.chapter, Is.EqualTo(1));
            Assert.That(recovered.checkpoint, Is.EqualTo(4));
            Assert.That(recovered.level, Is.EqualTo(3));
            Assert.That(recovered.equippedWeapon, Is.EqualTo(1));
        }

        [Test]
        public void MissingSaveFilesReturnNullWithoutThrowing()
        {
            if (File.Exists(isolatedSavePath)) File.Delete(isolatedSavePath);
            if (File.Exists(isolatedSavePath + ".bak")) File.Delete(isolatedSavePath + ".bak");

            Assert.DoesNotThrow(() =>
            {
                var p = DarklightProgress.Load();
                Assert.That(p, Is.Null, "Loading a non-existent save must safely return null");
            });
        }

        [Test]
        public void WeaponEnumIdsMaintainStrictNumericalContinuity()
        {
            // Validates that enum integer values cannot shift or break save compatibility
            Assert.That((int)DarklightWeaponId.Katana, Is.EqualTo(0));
            Assert.That((int)DarklightWeaponId.Daggers, Is.EqualTo(1));
            Assert.That((int)DarklightWeaponId.Chain, Is.EqualTo(2), "Slot 2 integer value must remain 2 for Punhos da Ruina");
            Assert.That((int)DarklightWeaponId.AbyssRider, Is.EqualTo(3));
        }

        [Test]
        public void AudioPreferencesClampVolumeSafely()
        {
            // Verify VolumePercent logic contract
            float normal = DarklightCampaign.VolumePercent(0.5f);
            Assert.That(normal, Is.EqualTo(50f).Within(0.01f));

            float clampedLow = DarklightCampaign.VolumePercent(-0.5f);
            Assert.That(clampedLow, Is.EqualTo(0f));

            float clampedHigh = DarklightCampaign.VolumePercent(1.5f);
            Assert.That(clampedHigh, Is.EqualTo(100f));
        }
    }
}
