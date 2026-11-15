using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    [Parallelizable(ParallelScope.None)]
    public sealed class DarklightFabiusFormsTests
    {
        static Sprite Resolve(int form, int weapon, int direction, CombatModel model, Vector2 movement = default)
        {
            Assert.That(DarklightFabiusFormsBank.TryResolveFormPose(form, weapon, direction, model, movement, out var sprite), Is.True);
            Assert.That(sprite, Is.Not.Null);
            return sprite;
        }

        [SetUp]
        public void ClearBank() => DarklightFabiusFormsBank.ClearCache();

        [Test]
        public void RupturaWalkResolvesAllEightDirections()
        {
            Assert.That(DarklightFabiusFormsBank.Ready, Is.True);
            var model = new CombatModel(new CombatLoadout { weapon = 0 });
            Assert.That(Resolve(1, 0, 0, model, Vector2.down).name, Does.StartWith("FabiusRupturaWalkSV1_"));
            Assert.That(Resolve(1, 0, 2, model, Vector2.left).name, Does.StartWith("FabiusRupturaWalkWV1_"));
            Assert.That(Resolve(1, 0, 4, model, Vector2.up).name, Does.StartWith("FabiusRupturaWalkNV1_"));
            Assert.That(Resolve(1, 0, 6, model, Vector2.right).name, Does.StartWith("FabiusRupturaWalkV1_"));

            Assert.That(Resolve(1, 0, 1, model, Vector2.one).name, Is.EqualTo("FabiusRupturaWalkSOSEV1_00"));
            Assert.That(Resolve(1, 0, 7, model, Vector2.one).name, Is.EqualTo("FabiusRupturaWalkSOSEV1_06"));
            Assert.That(Resolve(1, 0, 3, model, Vector2.one).name, Is.EqualTo("FabiusRupturaWalkNONEV1_00"));
            Assert.That(Resolve(1, 0, 5, model, Vector2.one).name, Is.EqualTo("FabiusRupturaWalkNONEV1_06"));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(7)]
        public void ReviewedRupturaKatanaResolvesAuthoredDirections(int direction)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                attack.startup = .1f;
                attack.active = .2f;
                var model = new CombatModel(new CombatLoadout { weapon = 0, lights = new[] { attack } });
                model.Tick(.016f, new CombatInput { lightPressed = true });
                Assert.That(model.State, Is.EqualTo(CombatState.Attack));
                Assert.That(Resolve(1, 0, direction, model).name, Does.StartWith("Session14RupturaKatana"));
            }
            finally
            {
                Object.DestroyImmediate(attack);
            }
        }

        [TestCase(0, "Session14RupturaKatanaFront_")]
        [TestCase(2, "Session14RupturaFistsFront_")]
        [TestCase(3, "Session14RupturaSawsBack_")]
        public void SupportedAttackWeaponsResolveTheirExactEastSheet(int weapon, string expectedPrefix)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                attack.startup = .1f;
                attack.active = .2f;
                var model = new CombatModel(new CombatLoadout { weapon = weapon, lights = new[] { attack } });
                model.Tick(.016f, new CombatInput { lightPressed = true });
                Assert.That(Resolve(1, weapon, 6, model).name, Does.StartWith(expectedPrefix));
            }
            finally
            {
                Object.DestroyImmediate(attack);
            }
        }

        [TestCase(1, 0)]
        [TestCase(1, 2)]
        [TestCase(1, 3)]
        [TestCase(2, 0)]
        [TestCase(2, 3)]
        public void NewWholeAttackClipsFollowCombatContactAcrossEightDirections(int form, int weapon)
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                attack.startup = .4f;
                attack.active = .15f;
                attack.recovery = .3f;
                for (int direction = 0; direction < 8; direction++)
                {
                    var model = new CombatModel(new CombatLoadout { weapon = weapon, lights = new[] { attack } });
                    model.Tick(.016f, new CombatInput { lightPressed = true });
                    var anticipation = Resolve(form, weapon, direction, model);
                    Assert.That(anticipation.name, Does.StartWith("Session14"));
                    model.Tick(.3f, default);
                    Assert.That(Resolve(form, weapon, direction, model), Is.EqualTo(anticipation), "Long startup must hold anticipation, not show impact early.");
                    model.Tick(.11f, default);
                    var contact = Resolve(form, weapon, direction, model);
                    Assert.That(contact, Is.Not.EqualTo(anticipation));
                    model.Tick(.16f, default);
                    Assert.That(Resolve(form, weapon, direction, model), Is.Not.EqualTo(contact));
                }
            }
            finally { Object.DestroyImmediate(attack); }
        }

        [Test]
        public void HurtAndDeadUseTheirElapsedFrameAndHoldTheLastFrame()
        {
            var hurt = new CombatModel(new CombatLoadout { weapon = 0 });
            hurt.Hurt(false, 5f);
            string firstHurt = Resolve(1, 0, 6, hurt).name;
            Assert.That(firstHurt, Does.EndWith("_00"));
            hurt.Tick(1f, default);
            string lastHurt = Resolve(1, 0, 6, hurt).name;
            Assert.That(lastHurt, Does.EndWith("_07"));
            hurt.Tick(.5f, default);
            Assert.That(Resolve(1, 0, 6, hurt).name, Is.EqualTo(lastHurt), "Hurt clips clamp instead of looping.");

            var dead = new CombatModel(new CombatLoadout { weapon = 0 });
            dead.Hurt(true);
            Assert.That(Resolve(2, 0, 6, dead).name, Does.EndWith("_00"), "Dead must start at the first authored frame.");
            dead.Tick(2f, default);
            Assert.That(Resolve(2, 0, 6, dead).name, Does.EndWith("_07"), "Dead advances to its final authored frame.");
            dead.Tick(1f, default);
            Assert.That(Resolve(2, 0, 6, dead).name, Does.EndWith("_07"), "Dead holds the final authored frame.");
        }

        [Test]
        public void ChargingAndUnmappedDirectionsLeaveTheBasePoseAvailable()
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                var model = new CombatModel(new CombatLoadout { weapon = 0, heavy = attack });
                model.Tick(.016f, new CombatInput { heavyPressed = true, heavyHeld = true });
                Assert.That(model.State, Is.EqualTo(CombatState.Charging));
                Assert.That(DarklightFabiusFormsBank.TryResolveFormPose(1, 0, 6, model, Vector2.zero, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(attack);
            }

            var idle = new CombatModel(new CombatLoadout { weapon = 0 });
            Assert.That(Resolve(2, 0, 0, idle, Vector2.down).name, Is.EqualTo("FabiusSerafimWalkNSV1_06"));
            Assert.That(Resolve(2, 0, 4, idle, Vector2.up).name, Is.EqualTo("FabiusSerafimWalkNSV1_00"));
            Assert.That(Resolve(2, 0, 0, idle).name, Is.EqualTo("Redesign_FabiusSerafimOfficial20261114_07"));
        }

        [Test]
        public void UnsupportedFormAndWeaponDoNotBorrowAnotherClip()
        {
            var attack = ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                attack.startup = .1f;
                attack.active = .2f;
                var katana = new CombatModel(new CombatLoadout { weapon = 0, lights = new[] { attack } });
                katana.Tick(.016f, new CombatInput { lightPressed = true });
                Assert.That(DarklightFabiusFormsBank.TryResolveFormPose(9, 0, 6, katana, Vector2.zero, out _), Is.False);

                var unsupportedWeapon = new CombatModel(new CombatLoadout { weapon = 1, lights = new[] { attack } });
                unsupportedWeapon.Tick(.016f, new CombatInput { lightPressed = true });
                Assert.That(DarklightFabiusFormsBank.TryResolveFormPose(1, 1, 6, unsupportedWeapon, Vector2.zero, out _), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(attack);
            }
        }

        [Test]
        public void GuardRetainsBasePoseAndSawsUseTheTransformedLocomotion()
        {
            var guard = new CombatModel(new CombatLoadout { weapon = 0 });
            guard.Tick(.016f, new CombatInput { parryPressed = true });
            Assert.That(guard.State, Is.EqualTo(CombatState.Parry));
            Assert.That(DarklightFabiusFormsBank.TryResolveFormPose(1, 0, 6, guard, Vector2.right, out _), Is.False);
            var saws = new CombatModel(new CombatLoadout { weapon = 3 });
            Assert.That(Resolve(1, 3, 6, saws, Vector2.right).name, Does.StartWith("FabiusRupturaSawsWalkLateral20261113_"));
        }

        [TestCase(1, "DiagS")]
        [TestCase(7, "DiagS")]
        [TestCase(2, "Lateral")]
        [TestCase(6, "Lateral")]
        [TestCase(3, "DiagN")]
        [TestCase(5, "DiagN")]
        public void RupturaLocomotionSwitchesToTheRequestedWeaponAndAuthoredDirection(int direction, string view)
        {
            var model = new CombatModel(new CombatLoadout { weapon = 0 });
            var katana = Resolve(1, 0, direction, model, Vector2.one);
            var fists = Resolve(1, 2, direction, model, Vector2.one);
            var saws = Resolve(1, 3, direction, model, Vector2.one);
            int firstFrame = direction == 1 || direction == 2 || direction == 3 ? 0 : 5;
            Assert.That(fists.name, Is.EqualTo($"FabiusRupturaFistsWalk{view}20261113_{firstFrame:00}"));
            Assert.That(saws.name, Is.EqualTo($"FabiusRupturaSawsWalk{view}20261113_{firstFrame:00}"));
            Assert.That(fists, Is.Not.SameAs(katana));
            Assert.That(saws, Is.Not.SameAs(katana));
            Assert.That(saws, Is.Not.SameAs(fists));
            Assert.That(Resolve(1, 0, direction, model, Vector2.one), Is.SameAs(katana), "Switching back must preserve Katana's authored clip.");
            Assert.That(Resolve(1, 2, direction, model), Is.SameAs(fists), "Idle must also use the requested weapon's authored view.");
            Assert.That(Resolve(1, 3, direction, model), Is.SameAs(saws));
        }

        [Test]
        public void IdleRetainsBothFormsAcrossAllEightDirections()
        {
            var model = new CombatModel(new CombatLoadout { weapon = 0 });
            for (int direction = 0; direction < 8; direction++)
            {
                Assert.That(Resolve(1, 0, direction, model).name, Does.StartWith(direction==0?"Redesign_FabiusRupturaOfficial20261114":"FabiusRuptura"));
                Assert.That(Resolve(2, 0, direction, model).name, Does.StartWith(direction==0?"Redesign_FabiusSerafimOfficial20261114":"FabiusSerafim"));
            }
        }
        [Test]
        public void LoadedFormSpritesHaveAuthoredOwnershipAndClearLegacyProperties()
        {
            var model = new CombatModel(new CombatLoadout { weapon = 0 });
            var sprite = Resolve(1, 0, 6, model, Vector2.right);
            Assert.That(DarklightFabiusFormsBank.IsAuthored(sprite), Is.True);

            var go = new GameObject("Fabius authored ownership test");
            try
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                DarklightFabiusFormsBank.ApplyOwnership(renderer);
                var properties = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(properties);
                Assert.That(properties.GetFloat("_WholeSprite"), Is.EqualTo(1));
                Assert.That(properties.GetFloat("_VampireForm"), Is.Zero);
                Assert.That(properties.GetFloat("_DarkForm"), Is.Zero);
                Assert.That(properties.GetFloat("_RuinFists"), Is.Zero);
                Assert.That(properties.GetVector("_CanonicalSkinShift"), Is.EqualTo(Vector4.zero));
                Assert.That(properties.GetTexture("_VampireMask"), Is.SameAs(Texture2D.blackTexture));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
