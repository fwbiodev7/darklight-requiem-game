using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightChecklistValidationTests
    {
        DarklightCampaign game;
        DarklightCombatLab lab;

        [UnitySetUp]
        public IEnumerator Prepare()
        {
            DarklightAutomation.Silent = true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            yield return null;
            yield return null;
            game = DarklightCampaign.Instance;
            lab = DarklightCombatLab.Instance;
            game.Progress.weaponUnlocks = 7;
            game.Progress.formsUnlocked = 3;
            game.Progress.awakeningState = 2;
            game.Blood.Set(100);
        }

        [UnityTest]
        public IEnumerator Test01_TrocarKatanaAdagasCorrenteNoEstagioAdulto()
        {
            game.PreviewCampaign();
            game.Progress.weaponUnlocks = 7;
            var view = lab.Player.GetComponent<DarklightCharacterView>();
            var art = lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            var appearance = lab.Player.GetComponent<CharacterAppearanceSystem>();
            appearance.PreviewStage(1);
            Assert.That(appearance.Stage, Is.EqualTo(1));

            for (int weapon = 0; weapon < 3; weapon++)
            {
                game.Progress.equippedWeapon = weapon;
                game.Weapons.Restore();
                yield return null;

                Assert.That(appearance.Stage, Is.EqualTo(1), "Estágio adulto foi perdido na troca de arma " + weapon);
                Assert.That((int)game.Weapons.Equipped.id, Is.EqualTo(weapon), "Arma equipada incorreta no step " + weapon);
                var model = new CombatModel(game.Weapons.Equipped.loadout);
                
                // Em repouso (idle)
                view.Present(model, Vector2.zero);
                Assert.That(appearance.Stage, Is.EqualTo(1));
                if (weapon == 1) // Adagas
                {
                    Assert.That(art.sprite.name, Does.Not.Contain("IdleStage"), "Ao equipar adagas em repouso não pode mostrar a animação de idle da katana!");
                }
            }
        }

        [UnityTest]
        public IEnumerator Test02_CaminharEAtaqueComTresArmasCorrespondeEquipada()
        {
            game.PreviewCampaign();
            game.Progress.weaponUnlocks = 7;
            var view = lab.Player.GetComponent<DarklightCharacterView>();
            var art = lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();

            for (int weapon = 0; weapon < 3; weapon++)
            {
                game.Progress.equippedWeapon = weapon;
                game.Weapons.Restore();
                yield return null;

                var model = new CombatModel(game.Weapons.Equipped.loadout);
                model.Tick(0.1f, new CombatInput { move = Vector2.right, aim = Vector2.right });
                view.Present(model, Vector2.right);
                Assert.That(art.sprite, Is.Not.Null);

                model.Tick(0.01f, new CombatInput { aim = Vector2.right, lightPressed = true });
                view.Present(model, Vector2.zero);
                Assert.That(model.State, Is.EqualTo(CombatState.Attack));
                Assert.That((int)game.Weapons.Equipped.id, Is.EqualTo(weapon));
            }
        }

        [UnityTest]
        public IEnumerator Test03_AlternarRockERequiemSemMudarIdade()
        {
            game.PreviewCampaign();
            var appearance = lab.Player.GetComponent<CharacterAppearanceSystem>();
            appearance.PreviewStage(2);
            int initialStage = appearance.Stage;

            game.SelectSkin(1, false);
            yield return null;
            Assert.That(appearance.Stage, Is.EqualTo(initialStage), "Alternar para Rock alterou a idade!");

            game.SelectSkin(0, false);
            yield return null;
            Assert.That(appearance.Stage, Is.EqualTo(initialStage), "Alternar para Requiem alterou a idade!");
        }

        [UnityTest]
        public IEnumerator Test04_AtivarTransformacaoVampirescaETrocarArma()
        {
            game.PreviewChapter(4);
            yield return null;
            game.Progress.weaponUnlocks = 7;
            game.Progress.formsUnlocked = 3;
            game.Progress.awakeningState = 2;
            var appearance = lab.Player.GetComponent<CharacterAppearanceSystem>();
            appearance.PreviewStage(1);
            game.Progress.preferredForm = 2;
            game.Blood.Set(100);
            lab.Player.ResetModel();
            Assert.That(game.TryTransformPlayer(), Is.True, "Falha ao ativar transformação vampiresca");
            typeof(DarklightCampaign).GetMethod("TickFormPresentation", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, new object[] { 0.7f });
            Assert.That(game.PresentedForm, Is.EqualTo(2));

            for (int weapon = 0; weapon < 3; weapon++)
            {
                game.Progress.equippedWeapon = weapon;
                game.Weapons.Restore();
                yield return null;

                Assert.That(game.PresentedForm, Is.EqualTo(2), "Forma vampiresca perdida na troca de arma");
                Assert.That((int)game.Weapons.Equipped.id, Is.EqualTo(weapon));
                Assert.That(appearance.Stage, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator Test05_AndarDiagonalmenteContraParedesEQuinas()
        {
            game.PreviewChapter(0);
            yield return null;
            var body = lab.Player.GetComponent<Rigidbody2D>();
            Vector2 spawn = game.Map.SafeSpawn(game.Map.Arenas[0]);
            body.position = spawn;
            Physics2D.SyncTransforms();

            Vector2 diagonal = new Vector2(1, 1).normalized * 0.2f;
            Vector2 initialPos = body.position;
            for (int i = 0; i < 20; i++)
            {
                DarklightNavigation.SafeStep(body, diagonal, 0.3f);
                Physics2D.SyncTransforms();
            }

            float moved = Vector2.Distance(body.position, initialPos);
            Assert.That(moved, Is.GreaterThan(0.05f), "Jogador ficou preso sem deslizar");
        }

        [UnityTest]
        public IEnumerator Test06_PassarPorEscadasPontesECorredoresEstreitos()
        {
            game.PreviewChapter(4);
            yield return null;
            var joins = new[] { new Vector4(611, 497, 730, 497), new Vector4(810, 359, 810, 412) };
            var body = lab.Player.GetComponent<Rigidbody2D>();

            for (int j = 0; j < joins.Length; j++)
            {
                Vector2 a = game.Map.Art.World(new Vector2(joins[j].x, joins[j].y));
                Vector2 b = game.Map.Art.World(new Vector2(joins[j].z, joins[j].w));

                Assert.That(DarklightNavigation.Clear(a, b, 0.28f), Is.True);
                body.position = a;
                Physics2D.SyncTransforms();
                for (int step = 0; step < 100 && Vector2.Distance(body.position, b) > 0.02f; step++)
                {
                    Vector2 delta = Vector2.ClampMagnitude(b - body.position, 0.15f);
                    DarklightNavigation.SafeStep(body, delta, 0.28f);
                    Physics2D.SyncTransforms();
                }
                Assert.That(Vector2.Distance(body.position, b), Is.LessThan(0.05f));
            }
        }

        [UnityTest]
        public IEnumerator Test07_VerificarSeNenhumaParedeSolidaPodeSerAtravessada()
        {
            game.PreviewChapter(0);
            yield return null;
            var body = lab.Player.GetComponent<Rigidbody2D>();
            Vector2 wallPoint = game.Map.Bounds.min + new Vector2(1, 1);
            body.position = wallPoint;
            Physics2D.SyncTransforms();

            Vector2 intoWall = Vector2.left * 0.5f;
            Vector2 posBefore = body.position;
            for (int i = 0; i < 30; i++)
            {
                DarklightNavigation.SafeStep(body, intoWall, 0.3f);
                Physics2D.SyncTransforms();
            }

            Assert.That(body.position.x, Is.GreaterThanOrEqualTo(posBefore.x - 0.05f));
        }
    }
}
