using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightFinalCampaignTests
    {
        DarklightCampaign game;
        DarklightCombatLab lab;
        float listener;
        const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        static object Invoke(object o, string method, params object[] args) => o.GetType().GetMethod(method, Private).Invoke(o, args);
        static T Field<T>(object o, string field) => (T)o.GetType().GetField(field, Private).GetValue(o);
        [UnitySetUp] public IEnumerator Setup()
        {
            listener = AudioListener.volume; AudioListener.volume = 0; DarklightAutomation.Silent = true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            yield return null; yield return null;
            game = DarklightCampaign.Instance; lab = DarklightCombatLab.Instance;
        }
        [UnityTearDown] public IEnumerator Restore()
        {
            DarklightAutomation.Silent = true; AudioListener.volume = listener; yield return null;
        }
        void Resume(int chapter, int checkpoint)
        {
            game.PreviewChapter(chapter); game.Progress.checkpoint = checkpoint;
            Invoke(game, "LoadMap", chapter); Invoke(game, "Close");
        }
        [UnityTest] public IEnumerator EndingButtonsCompleteAllThreeOutcomesAndKeepUnlocks()
        {
            for (int ending = 1; ending <= 3; ending++)
            {
                Resume(9, 4); var p = game.Progress;
                p.appearanceStage = 4; p.weaponUnlocks = 7; p.formsUnlocked = 3; p.expansionFlags = 15;
                for (int i = 0; i < p.relics.Length; i++) p.relics[i] = true;
                game.PreviewPanel("Ending");
                Invoke(game, "Activate", ending - 1);
                Assert.That(p.ending, Is.EqualTo(ending)); Assert.That(p.completed, Is.True);
                Assert.That(Field<string>(game, "panel"), Is.EqualTo("Victory"));
                var saved = DarklightProgress.Decode(JsonUtility.ToJson(p));
                Assert.That(saved.ending, Is.EqualTo(ending)); Assert.That(saved.completed, Is.True);
                Assert.That(saved.appearanceStage, Is.EqualTo(4)); Assert.That(saved.weaponUnlocks, Is.EqualTo(7));
                Assert.That(saved.FormUnlocked, Is.EqualTo(3)); Assert.That(saved.SecretEndingAvailable, Is.True);
                Assert.That(game.Form, Is.Zero); Assert.That(game.Transforming, Is.False);
                yield return null;
            }
            Resume(9, 4); game.PreviewPanel("Ending"); yield return null;
            var menu = lab.PixelHUD.transform.Find("HUD640x360/Menus/Painel");
            Assert.That(menu.Find("Botao2"), Is.Null, "Secret outcome visible without recorded requirements");
            foreach (var a in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) Assert.That(a.isPlaying, Is.False);
        }
        [UnityTest] public IEnumerator DeathButtonRestoresCheckpointBossPhaseAndNarrativeIdentity()
        {
            for (int chapter = 6; chapter < 10; chapter++)
            {
                Resume(chapter, 3); var p = game.Progress;
                p.bossPhase = 3; p.appearanceStage = 4; p.weaponUnlocks = 7; p.equippedWeapon = 2;
                p.formsUnlocked = 3; p.expansionFlags = 15; p.missionWeaponHits = 7;
                game.Weapons.Restore(); game.Appearance.PreviewStage(4);
                Invoke(game, "Spawn", (RequiemEnemyKind)(chapter + 5), game.Map.SafeSpawn(game.Map.Arenas[3]), false);
                yield return null;
                Assert.That(game.CurrentBoss.Phase, Is.EqualTo(3));
                int points = p.points, level = p.level, flags = p.expansionFlags;
                var playerHealth=lab.Player.GetComponent<HealthSystem>();playerHealth.InstantKill();
                Assert.That(playerHealth.IsDead,Is.True,"Death stimulus was rejected");
                float deadline=Time.realtimeSinceStartup+1f;
                while(Field<string>(game,"panel")!="Death"&&Time.realtimeSinceStartup<deadline)yield return null;
                Assert.That(Field<string>(game, "panel"), Is.EqualTo("Death"));
                Invoke(game, "Activate", 0); yield return null;
                Assert.That(game.Active, Is.True); Assert.That(lab.Player.GetComponent<HealthSystem>().IsDead, Is.False);
                Assert.That(p.chapter, Is.EqualTo(chapter)); Assert.That(p.checkpoint, Is.EqualTo(3));
                Assert.That(p.bossPhase, Is.EqualTo(3)); Assert.That(p.points, Is.EqualTo(points));
                Assert.That(p.level, Is.EqualTo(level)); Assert.That(p.expansionFlags, Is.EqualTo(flags));
                Assert.That(p.equippedWeapon, Is.EqualTo(2)); Assert.That(game.Appearance.Stage, Is.EqualTo(4));
                Assert.That(game.Form, Is.Zero); Assert.That(game.Transforming, Is.False);
                Assert.That(Vector2.Distance(lab.Player.transform.position, game.Map.SafeSpawn(game.Map.Checkpoints[3])), Is.LessThan(.15f));
                Invoke(game, "Spawn", (RequiemEnemyKind)(chapter + 5), game.Map.SafeSpawn(game.Map.Arenas[3]), false);
                Assert.That(game.CurrentBoss.Phase, Is.EqualTo(3));
            }
        }
        [UnityTest] public IEnumerator LegacyFinaleContinuationKeepsRewardsAndCannotRepeatMigration()
        {
            Resume(5, 4); var p = game.Progress;
            p.completed = true; p.legacyFinaleCompleted = true; p.ending = 2;
            p.appearanceStage = 4; p.level = 12; p.points = 7; p.weaponUnlocks = 7; p.formsUnlocked = 3;
            p.relics[0] = true; p.relics[5] = true;
            game.ResumeLegacyExpansion();
            Assert.That(p.chapter, Is.EqualTo(6)); Assert.That(p.checkpoint, Is.Zero); Assert.That(p.completed, Is.False);
            Assert.That(p.bossPhase, Is.EqualTo(1)); Assert.That(p.level, Is.EqualTo(12)); Assert.That(p.points, Is.EqualTo(7));
            Assert.That(p.ending, Is.EqualTo(2)); Assert.That(p.weaponUnlocks, Is.EqualTo(7)); Assert.That(p.FormUnlocked, Is.EqualTo(3));
            Assert.That(p.appearanceStage, Is.EqualTo(4)); Assert.That(p.relics[0] && p.relics[5], Is.True);
            string before = JsonUtility.ToJson(p); game.ResumeLegacyExpansion();
            Assert.That(JsonUtility.ToJson(p), Is.EqualTo(before));
            yield return null;
        }
        [UnityTest] public IEnumerator CompletedCheckpointReloadDoesNotReawardBossOrMilestone()
        {
            foreach (int chapter in new[] { 0, 2, 6, 7, 9 })
            {
                Resume(chapter, 3); Invoke(game, "ClearEncounter");
                Assert.That(game.Progress.checkpoint, Is.EqualTo(4));
                var p = game.Progress; int points = p.points, potions = p.potions, weapons = p.weaponUnlocks;
                long milestones = p.narrativeMilestones;
                for (int repeat = 0; repeat < 2; repeat++)
                {
                    Invoke(game, "LoadMap", chapter); Invoke(game, "Close"); yield return null;
                    Assert.That(p.points, Is.EqualTo(points)); Assert.That(p.potions, Is.EqualTo(potions));
                    Assert.That(p.weaponUnlocks, Is.EqualTo(weapons)); Assert.That(p.narrativeMilestones, Is.EqualTo(milestones));
                    Assert.That(p.checkpoint, Is.EqualTo(4)); Assert.That(game.CurrentBoss, Is.Null);
                }
            }
        }
    }
}
