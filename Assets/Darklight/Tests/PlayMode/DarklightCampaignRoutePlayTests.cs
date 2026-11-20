using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightCampaignRoutePlayTests
    {
        DarklightCampaign game;
        DarklightCombatLab lab;

        [UnitySetUp]
        public IEnumerator LoadCampaign()
        {
            DarklightAutomation.Silent = true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            yield return null;
            game = DarklightCampaign.Instance;
            lab = DarklightCombatLab.Instance;
        }

        void Call(string name, params object[] args) =>
            typeof(DarklightCampaign).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(game, args);

        void EnterLastCheckpoint(int chapter)
        {
            game.PreviewChapter(chapter);
            game.Progress.checkpoint = 3;
            Call("LoadMap", chapter);
            Call("Close");
        }

        void Move(Vector2 point)
        {
            lab.Player.ResetModel();
            lab.Player.GetComponent<Rigidbody2D>().position = point;
            lab.Player.transform.position = point;
            Physics2D.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator DefeatingTheGuardsWithLadyOpensTheFirstPortal()
        {
            EnterLastCheckpoint(0);
            yield return null;
            Assert.That(game.AllyName, Is.EqualTo("LADY"));
            var lady = GameObject.Find("LyraMoreau").GetComponent<SpriteRenderer>();
            Assert.That(lady.sprite, Is.SameAs(Resources.Load<Sprite>("Darklight/OfficialCast/Lady")));
            Assert.That(lady.transform.localScale.x, Is.EqualTo(DarklightCharacterView.WorldVisualScale * .96f).Within(.0001f));
            Assert.That(game.MapEnemies, Is.Empty);
            Assert.That(game.Progress.checkpoint, Is.EqualTo(3));
            Assert.That(GameObject.Find("ritual_seal_0"), Is.Null);
            Move(game.Map.SafeSpawn(game.Map.Arenas[3]));
            yield return null;
            CollectionAssert.AreEquivalent(new[] {RequiemEnemyKind.Crawler, RequiemEnemyKind.Hunter},
                game.MapEnemies.Select(enemy => enemy.kind).ToArray());
            Assert.That(game.CurrentBoss, Is.Null);
            Call("CallLady");
            var ally = Object.FindFirstObjectByType<DarklightCampaignAlly>();
            Assert.That(ally, Is.Not.Null);
            Assert.That(ally.name, Is.EqualTo("LadyApoio"));
            Assert.That(ally.GetComponentInChildren<SpriteRenderer>().sprite, Is.SameAs(lady.sprite));
            var companion = ally.GetComponent<DarklightCombatant>();
            Assert.That(companion.Receive(20, Vector2.right), Is.False);
            Assert.That(Physics2D.GetIgnoreCollision(ally.GetComponent<Collider2D>(), lab.Player.GetComponent<Collider2D>()), Is.True);
            Move(game.Map.SafeSpawn(game.Map.Arenas[3]));
            float deadline = Time.realtimeSinceStartup + 8f;
            while (Time.realtimeSinceStartup < deadline && game.MapEnemies.All(enemy => enemy.Health.HealthPercent >= 1f))
                yield return null;
            Assert.That(game.MapEnemies.Any(enemy => enemy.Health.HealthPercent < 1f), Is.True, "Lady must damage a guardian without player input");
            ally.GetComponent<HealthSystem>().TakeDamage(9999);
            yield return null;
            Assert.That(ally.State, Is.EqualTo(DarklightCampaignAlly.AllyState.Incapacitated));
            StringAssert.StartsWith("Lady_Defeat_", ally.GetComponentInChildren<SpriteRenderer>().sprite.name);
            foreach (var enemy in game.MapEnemies) enemy.Health.TakeDamage(enemy.Health.MaxHealth + 1);
            for (int i = 0; i < 20 && game.Progress.checkpoint != 4; i++) yield return null;
            Assert.That(game.Progress.checkpoint, Is.EqualTo(4));
            Assert.That(GameObject.Find("Portal"), Is.Not.Null);
            Assert.That(Object.FindFirstObjectByType<DarklightCampaignAlly>(), Is.SameAs(ally));
            Call("CallLady");
            Assert.That(Object.FindFirstObjectByType<DarklightCampaignAlly>(), Is.Null);
            if (DarklightProgress.HasIsolatedTestStorage)
                Assert.That(DarklightProgress.Load().checkpoint, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator BloodCathedralPresentsMatriarchAndHerDefeatOpensThePortal()
        {
            EnterLastCheckpoint(2);
            yield return null;
            var design = Resources.Load<Sprite>("Darklight/OfficialCast/MatriarcaRubra");
            Assert.That(design, Is.Not.Null, "Matriarch pilot must import as a Sprite");
            Move(game.Map.SafeSpawn(game.Map.Arenas[3]));
            Assert.That(game.Map.InArena(3, lab.Player.transform.position), Is.True);
            GameObject introduction = null;
            for (int frame = 0; frame < 30 && introduction == null; frame++)
            {
                yield return null;
                introduction = GameObject.Find("MatriarcaApresentacao");
            }
            Assert.That(introduction, Is.Not.Null);
            Assert.That(introduction.GetComponent<SpriteRenderer>().sprite, Is.SameAs(design));
            Assert.That(introduction.transform.localScale.y, Is.EqualTo(DarklightCharacterView.WorldVisualScale));
            foreach (string _ in DarklightStory.BossLines[2]) Call("NextDialogue");
            yield return null;
            Assert.That(game.CurrentBoss?.kind, Is.EqualTo(RequiemEnemyKind.Matriarch));
            Assert.That(GameObject.Find("MatriarcaApresentacao"), Is.Null);
            var boss = game.CurrentBoss;
            Assert.That(boss.attackFrames[0].name, Is.EqualTo("MatriarcaRubraCombat_04"));
            Assert.That(boss.GetComponentInChildren<SpriteRenderer>().transform.localScale.y, Is.EqualTo(DarklightCharacterView.WorldVisualScale));
            boss.Health.TakeDamage(boss.Health.MaxHealth + 1);
            yield return null;
            StringAssert.StartsWith("MatriarcaRubra_Defeat_", boss.GetComponentInChildren<SpriteRenderer>().sprite.name);
            float deadline = Time.realtimeSinceStartup + 3;
            while (Time.realtimeSinceStartup < deadline && game.Progress.checkpoint != 4) yield return null;
            Assert.That(game.Progress.checkpoint, Is.EqualTo(4));
            Assert.That(GameObject.Find("Portal"), Is.Not.Null);
        }
    }
}
