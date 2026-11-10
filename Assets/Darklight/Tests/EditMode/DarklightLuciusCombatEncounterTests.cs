using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Darklight.Tests
{
    [TestFixture]
    public sealed class DarklightLuciusCombatEncounterTests
    {
        readonly List<UnityEngine.Object> owned = new();

        [TearDown]
        public void Cleanup()
        {
            foreach (var item in owned)
            {
                if (item != null) UnityEngine.Object.DestroyImmediate(item);
            }
            owned.Clear();
            DarklightLuciusSpriteBank.ClearCache();
        }

        AttackDefinition[] GetSwordBases()
        {
            var sword = AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Darklight/Data/Weapons/Requiem.asset");
            Assert.That(sword, Is.Not.Null);
            Assert.That(sword.loadout.lights, Has.Length.EqualTo(3));
            return sword.loadout.lights;
        }

        CombatLoadout CreateLuciusLoadout()
        {
            return DarklightLuciusCombatProfile.Create(GetSwordBases(), owned.Add);
        }

        (GameObject obj, DarklightEnemy enemy, HealthSystem health, DarklightCombatant combatant, DarklightLuciusView view) CreateLuciusActor(CombatLoadout loadout = null)
        {
            var go = new GameObject("Lucius_TestActor");
            owned.Add(go);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            go.AddComponent<CircleCollider2D>();
            var health = go.AddComponent<HealthSystem>();
            health.SetMaxHealth(260, false);
            var combatant = go.AddComponent<DarklightCombatant>();
            var view = go.AddComponent<DarklightLuciusView>();
            view.appearance = DarklightLuciusAppearance.Normal;
            var enemy = go.AddComponent<DarklightEnemy>();
            loadout ??= CreateLuciusLoadout();
            enemy.InitializeLucius(loadout, view);
            return (go, enemy, health, combatant, view);
        }

        (GameObject playerObj, DarklightFighter fighter, HealthSystem playerHealth, DarklightCombatant playerCombatant) CreatePlayerActor(Vector2 position)
        {
            var go = new GameObject("Player_TestActor");
            owned.Add(go);
            go.transform.position = position;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            go.AddComponent<CircleCollider2D>();
            var health = go.AddComponent<HealthSystem>();
            health.SetMaxHealth(100, false);
            var combatant = go.AddComponent<DarklightCombatant>();
            combatant.player = true;
            go.AddComponent<DarklightCharacterView>();
            var fighter = go.AddComponent<DarklightFighter>();
            fighter.loadout = CreateLuciusLoadout();
            fighter.ResetModel();
            return (go, fighter, health, combatant);
        }

        [Test]
        public void LuciusIdentityIsIndependentFromLucienBoss()
        {
            var (_, enemy, _, _, _) = CreateLuciusActor();
            Assert.That(enemy.IsLucius, Is.True, "Lucius actor must be identified as Lucius");
            Assert.That(enemy.kind, Is.Not.EqualTo(RequiemEnemyKind.Lucien), "Lucius must NOT hijack or replace RequiemEnemyKind.Lucien");
            Assert.That((int)RequiemEnemyKind.Lucien, Is.EqualTo(10), "RequiemEnemyKind.Lucien remains intact in chapter 5");
        }

        [Test]
        public void LuciusUsesExactlyThreeSwordAttacksFromCombatProfile()
        {
            var (_, enemy, _, _, _) = CreateLuciusActor();
            var model = enemy.CombatModel;
            Assert.That(model, Is.Not.Null, "CombatModel must be initialized for Lucius");
            var loadout = model.Loadout;
            Assert.That(loadout.lights, Has.Length.EqualTo(3), "Lucius combat profile must contain exactly 3 attacks");

            var expectedKeys = new[]
            {
                DarklightLuciusCombatProfile.SwordHorizontal,
                DarklightLuciusCombatProfile.SwordVertical,
                DarklightLuciusCombatProfile.SwordThrust
            };
            var expectedShapes = new[] { HitShape.Sweep, HitShape.Sector, HitShape.Thrust };

            for (int i = 0; i < 3; i++)
            {
                Assert.That(loadout.lights[i].animationKey, Is.EqualTo(expectedKeys[i]));
                Assert.That(loadout.lights[i].hitShape, Is.EqualTo(expectedShapes[i]));
                Assert.That(loadout.lights[i].range, Is.GreaterThan(0f));
                Assert.That(loadout.lights[i].damage, Is.GreaterThan(0f));
            }
        }

        [Test]
        public void AttackExecutionBindsKeyDirectionAndClock()
        {
            var (_, enemy, _, _, view) = CreateLuciusActor();
            var model = enemy.CombatModel;

            for (int i = 0; i < 3; i++)
            {
                Vector2 aim = Vector2.right; // direction E (index 6)
                enemy.ExecuteLuciusAttack(i, aim);
                Assert.That(model.State, Is.EqualTo(CombatState.Attack));
                Assert.That(model.Attack, Is.Not.Null);
                Assert.That(model.Attack.animationKey, Is.EqualTo(model.Loadout.lights[i].animationKey));
                Assert.That(model.StateTime, Is.EqualTo(0f));

                // Advance clock through windup
                model.Tick(model.Attack.startup * 0.5f, new CombatInput { aim = aim });
                Assert.That(model.StateTime, Is.GreaterThan(0f));
                Assert.That(model.Facing, Is.EqualTo(aim.normalized));

                // Presentation verifies attackKey and direction without error
                Assert.That(view.Present(model), Is.True);
            }
        }

        [Test]
        public void AttackLifecycleFollowsAnticipationImpactRecovery()
        {
            var (_, enemy, _, _, _) = CreateLuciusActor();
            var model = enemy.CombatModel;
            enemy.ExecuteLuciusAttack(0, Vector2.right);

            var attack = model.Attack;
            Assert.That(attack, Is.Not.Null);

            // Phase 1: Antecipação (startup)
            model.Tick(attack.startup * 0.5f, default);
            Assert.That(model.State, Is.EqualTo(CombatState.Attack));
            Assert.That(model.ActiveThisTick, Is.False, "During startup, attack must not be active");

            // Phase 2: Impacto (active)
            model.Tick(attack.startup * 0.6f, default);
            Assert.That(model.ActiveThisTick, Is.True, "During active window, attack must be active this tick");

            // Phase 3: Recuperação (recovery)
            model.Tick(attack.active + 0.01f, default);
            model.Tick(0.01f, default);
            Assert.That(model.ActiveThisTick, Is.False, "During recovery, attack must no longer be active");
            Assert.That(model.State, Is.EqualTo(CombatState.Attack));

            // Phase 4: Retorno ao Neutro
            model.Tick(attack.recovery, default);
            Assert.That(model.State, Is.EqualTo(CombatState.Neutral), "After recovery, model must return to Neutral");
        }

        [Test]
        public void SingleHitDamageApplicationAppliesDamageOnlyOncePerAttack()
        {
            var (luciusObj, enemy, _, _, _) = CreateLuciusActor();
            luciusObj.transform.position = Vector2.zero;
            var (playerObj, _, playerHealth, playerCombatant) = CreatePlayerActor(Vector2.right * 1.0f);

            var labGo = new GameObject("Lab_Mock");
            owned.Add(labGo);
            var lab = labGo.AddComponent<DarklightCombatLab>();
            lab.Player = playerObj.GetComponent<DarklightFighter>();

            float initialHp = playerHealth.CurrentHealth;

            // Trigger attack towards player
            enemy.ExecuteLuciusAttack(0, Vector2.right);
            var attack = enemy.CombatModel.Attack;

            // Tick into active phase
            enemy.CombatModel.Tick(attack.startup + 0.01f, new CombatInput { aim = Vector2.right });
            Assert.That(enemy.CombatModel.ActiveThisTick, Is.True);

            // Resolve strike on first active frame
            enemy.ResolveLuciusStrike(lab, null);
            float hpAfterFirstHit = playerHealth.CurrentHealth;
            Assert.That(hpAfterFirstHit, Is.LessThan(initialHp), "Damage must be applied on hit");

            // Advance further inside the same active window
            enemy.CombatModel.Tick(0.01f, new CombatInput { aim = Vector2.right });
            Assert.That(enemy.CombatModel.ActiveThisTick, Is.True);

            // Resolve strike again during same active window
            enemy.ResolveLuciusStrike(lab, null);
            Assert.That(playerHealth.CurrentHealth, Is.EqualTo(hpAfterFirstHit), "Damage must NOT be applied a second time during the same attack");
        }

        [Test]
        public void HitboxRespectsRangeAndAngle()
        {
            var (luciusObj, enemy, _, _, _) = CreateLuciusActor();
            luciusObj.transform.position = Vector2.zero;

            // Player behind Lucius (at -2 on X while Lucius faces +X)
            var (playerBehindObj, _, playerBehindHealth, _) = CreatePlayerActor(Vector2.left * 1.0f);
            var labGo = new GameObject("Lab_Mock");
            owned.Add(labGo);
            var lab = labGo.AddComponent<DarklightCombatLab>();
            lab.Player = playerBehindObj.GetComponent<DarklightFighter>();

            float initialHp = playerBehindHealth.CurrentHealth;

            enemy.ExecuteLuciusAttack(0, Vector2.right);
            var attack = enemy.CombatModel.Attack;
            enemy.CombatModel.Tick(attack.startup + 0.01f, new CombatInput { aim = Vector2.right });

            // Behind Lucius: should not hit
            enemy.ResolveLuciusStrike(lab, null);
            Assert.That(playerBehindHealth.CurrentHealth, Is.EqualTo(initialHp), "Target behind facing direction must not be hit");

            // Player beyond attack range (at distance 10)
            playerBehindObj.transform.position = Vector2.right * 10f;
            enemy.ResolveLuciusStrike(lab, null);
            Assert.That(playerBehindHealth.CurrentHealth, Is.EqualTo(initialHp), "Target beyond attack range must not be hit");
        }

        [Test]
        public void DamageInterruptsLuciusAttack()
        {
            var (_, enemy, _, luciusCombatant, _) = CreateLuciusActor();
            enemy.ExecuteLuciusAttack(0, Vector2.right);
            Assert.That(enemy.CombatModel.State, Is.EqualTo(CombatState.Attack));

            // Lucius receives damage from player
            bool received = luciusCombatant.Receive(15f, Vector2.left);
            Assert.That(received, Is.True);
            Assert.That(enemy.CombatModel.State, Is.EqualTo(CombatState.Hurt), "Damage must interrupt attack and transition to Hurt state");
            Assert.That(enemy.CombatModel.ActiveThisTick, Is.False, "Interrupted attack cannot remain active");
        }

        [Test]
        public void DeathHaltsLuciusAndEntersDeadState()
        {
            var (_, enemy, health, luciusCombatant, _) = CreateLuciusActor();
            enemy.ExecuteLuciusAttack(1, Vector2.right);

            // Fatal blow
            luciusCombatant.Receive(health.CurrentHealth + 10f, Vector2.left);
            Assert.That(health.IsDead, Is.True);
            Assert.That(enemy.CombatModel.State, Is.EqualTo(CombatState.Dead), "Fatal damage must transition CombatModel to Dead");

            // Tick while dead halts velocity and updates death presentation
            enemy.TickLucius(null, null, 0.05f);
            Assert.That(enemy.GetComponent<Rigidbody2D>().linearVelocity, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void DefenseInterruptionBlocksDamage()
        {
            var (luciusObj, enemy, _, _, _) = CreateLuciusActor();
            luciusObj.transform.position = Vector2.zero;
            var (playerObj, fighter, playerHealth, _) = CreatePlayerActor(Vector2.right * 1.0f);

            var labGo = new GameObject("Lab_Mock");
            owned.Add(labGo);
            var lab = labGo.AddComponent<DarklightCombatLab>();
            lab.Player = fighter;

            // Player enters parry state facing Lucius
            fighter.Model.Tick(0.01f, new CombatInput { parryPressed = true, aim = Vector2.left });
            Assert.That(fighter.Model.State, Is.EqualTo(CombatState.Parry));

            // Lucius attacks
            enemy.ExecuteLuciusAttack(0, Vector2.right);
            var attack = enemy.CombatModel.Attack;
            enemy.CombatModel.Tick(attack.startup + 0.01f, new CombatInput { aim = Vector2.right });

            float initialPlayerHp = playerHealth.CurrentHealth;
            enemy.ResolveLuciusStrike(lab, null);

            // Player parried the strike, so HP was preserved!
            Assert.That(playerHealth.CurrentHealth, Is.EqualTo(initialPlayerHp), "Parried attack must deal 0 damage to player");
        }

        [Test]
        public void NavigationFollowsPursuitAndFlanking()
        {
            var (luciusObj, enemy, _, _, _) = CreateLuciusActor();
            luciusObj.transform.position = Vector2.zero;
            var (playerObj, fighter, _, _) = CreatePlayerActor(Vector2.right * 5.0f); // 5m away (beyond 1.85m engage range)

            var labGo = new GameObject("Lab_Mock");
            owned.Add(labGo);
            var lab = labGo.AddComponent<DarklightCombatLab>();
            lab.Player = fighter;

            // Tick Lucius AI
            enemy.TickLucius(null, lab, 0.05f);

            var rb = luciusObj.GetComponent<Rigidbody2D>();
            Assert.That(rb.linearVelocity.x, Is.GreaterThan(0f), "Lucius must move towards player when beyond engage range");
        }

        [Test]
        public void SafeFallbackWhenTechniqueVisualNotImported()
        {
            var (_, _, _, _, view) = CreateLuciusActor();
            // Presenting an action or technique with unimported key must safely fall back to idle
            bool presented = view.PresentPose("Attack", 0, 0.15f, "SwordHorizontal");
            Assert.That(presented, Is.True, "Presentation must succeed via safe idle fallback");
            Assert.That(view.UsesIdleFallback, Is.True, "Missing candidate frames must report fallback = true without throwing or breaking state");
        }
    }
}

