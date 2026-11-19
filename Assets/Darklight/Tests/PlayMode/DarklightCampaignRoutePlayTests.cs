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

        void Activate(string id)
        {
            var marker = GameObject.Find(id);
            Assert.That(marker, Is.Not.Null, id);
            Move(marker.transform.position);
            Assert.That(game.TryGetInteraction(out var target), Is.True, id);
            Assert.That(target.Id, Is.EqualTo(id));
            Assert.That(game.TryInteract(), Is.True);
        }

        [UnityTest]
        public IEnumerator LadyAndTheThreeSealRitualLeadToTheFirstPortal()
        {
            EnterLastCheckpoint(0);
            yield return null;
            Assert.That(game.AllyName, Is.EqualTo("LADY"));
            var lady = GameObject.Find("LyraMoreau").GetComponent<SpriteRenderer>();
            Assert.That(lady.sprite, Is.SameAs(Resources.Load<Sprite>("Darklight/OfficialCast/Lady")));
            Assert.That(lady.transform.localScale.x, Is.EqualTo(DarklightCharacterView.WorldVisualScale * .96f).Within(.0001f));
            Activate("ritual_clue");
            Call("NextDialogue");
            Activate("ritual_seal_0");
            Activate("ritual_seal_2");
            Assert.That(game.MapEnemies, Is.Empty);
            Assert.That(game.Progress.checkpoint, Is.EqualTo(3));
            Activate("ritual_seal_0");
            Activate("ritual_seal_1");
            Activate("ritual_seal_2");
            CollectionAssert.AreEquivalent(new[] {RequiemEnemyKind.Crawler, RequiemEnemyKind.Hunter},
                game.MapEnemies.Select(enemy => enemy.kind).ToArray());
            Assert.That(game.CurrentBoss, Is.Null);
            foreach (var enemy in game.MapEnemies) enemy.Health.TakeDamage(enemy.Health.MaxHealth + 1);
            for (int i = 0; i < 20 && game.Progress.checkpoint != 4; i++) yield return null;
            Assert.That(game.Progress.checkpoint, Is.EqualTo(4));
            Assert.That(GameObject.Find("Portal"), Is.Not.Null);
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
            for (int i = 0; i < 20 && game.Progress.checkpoint != 4; i++) yield return null;
            Assert.That(game.Progress.checkpoint, Is.EqualTo(4));
            Assert.That(GameObject.Find("Portal"), Is.Not.Null);
        }
    }
}
