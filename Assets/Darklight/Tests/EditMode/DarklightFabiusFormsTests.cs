using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightFabiusFormsTests
    {
        [Test]
        public void FabiusFormsBankLoadsCatalogAndResolvesRupturaAndSerafim()
        {
            DarklightFabiusFormsBank.ClearCache();
            Assert.IsTrue(DarklightFabiusFormsBank.Ready, "Fabius forms bank should load catalog cleanly.");

            // 1: Ruptura Katana Attack
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            attack.startup = 0.1f;
            attack.active = 0.2f;
            var modelAttack = new CombatModel(new CombatLoadout { weapon = 0, lights = new[] { attack } });
            modelAttack.Tick(0.016f, new CombatInput { lightPressed = true });
            modelAttack.Tick(0.15f, default);
            bool resolvedKatana = DarklightFabiusFormsBank.TryResolveFormPose(1, 0, 2, modelAttack, Vector2.zero, out var sprKatana);
            Assert.IsTrue(resolvedKatana, "Ruptura should resolve Katana attack pose.");
            Assert.IsNotNull(sprKatana);

            // 1: Ruptura Walk
            var modelWalk = new CombatModel(new CombatLoadout { weapon = 0 });
            bool resolvedWalk = DarklightFabiusFormsBank.TryResolveFormPose(1, 0, 2, modelWalk, Vector2.right, out var sprWalk);
            Assert.IsTrue(resolvedWalk, "Ruptura should resolve walk pose.");
            Assert.IsNotNull(sprWalk);

            // 2: Serafim Fly Glide
            var modelFly = new CombatModel(new CombatLoadout { weapon = 0 });
            bool resolvedFly = DarklightFabiusFormsBank.TryResolveFormPose(2, 0, 2, modelFly, Vector2.right, out var sprFly);
            Assert.IsTrue(resolvedFly, "Serafim should resolve fly/glide pose.");
            Assert.IsNotNull(sprFly);

            // 2: Serafim Hurt/Dead
            var modelDead = new CombatModel(new CombatLoadout { weapon = 0 });
            modelDead.Hurt(true);
            bool resolvedDead = DarklightFabiusFormsBank.TryResolveFormPose(2, 0, 2, modelDead, Vector2.zero, out var sprDead);
            Assert.IsTrue(resolvedDead, "Serafim should resolve hurt/dead pose.");
            Assert.IsNotNull(sprDead);
        }
    }
}
