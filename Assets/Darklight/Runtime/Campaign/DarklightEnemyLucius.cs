using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        public bool IsLucius { get; private set; }
        public CombatModel CombatModel { get; private set; }
        public DarklightLuciusView LuciusView { get; private set; }
        public AttackDefinition CurrentLuciusAttack => CombatModel?.Attack;
        public CombatLoadout LuciusLoadout => CombatModel?.Loadout;
        int luciusAttackStep;
        float luciusEngageRange = 1.85f;
        float luciusDodgeCooldown;
        float luciusGuardCooldown;

        public void InitializeLucius(CombatLoadout loadout = null, DarklightLuciusView view = null)
        {
            IsLucius = true;
            if (view == null) view = GetComponent<DarklightLuciusView>() ?? gameObject.AddComponent<DarklightLuciusView>();
            LuciusView = view;
            if (loadout == null)
            {
                var game = DarklightCampaign.Instance;
                var weaponController = game?.Weapons ?? DarklightCombatLab.Instance?.Player?.GetComponent<DarklightWeaponController>();
                var equipped = weaponController?.Equipped;
                if (equipped?.loadout?.lights?.Length == 3)
                {
                    loadout = DarklightLuciusCombatProfile.Create(equipped.loadout.lights, _ => { });
                }
                else if (game?.weaponDefinitions != null && game.weaponDefinitions.Length > 0 &&
                    game.weaponDefinitions[0]?.loadout?.lights?.Length == 3)
                {
                    loadout = DarklightLuciusCombatProfile.Create(game.weaponDefinitions[0].loadout.lights, _ => { });
                }
                else if (DarklightCombatLab.Instance?.Player?.loadout?.lights?.Length == 3)
                {
                    loadout = DarklightLuciusCombatProfile.Create(DarklightCombatLab.Instance.Player.loadout.lights, _ => { });
                }
            }
            if (loadout == null)
            {
                var a1 = ScriptableObject.CreateInstance<AttackDefinition>();
                a1.animationKey = DarklightLuciusCombatProfile.SwordHorizontal;
                a1.hitShape = HitShape.Sweep; a1.range = 1.85f; a1.startup = 0.14f; a1.active = 0.12f; a1.recovery = 0.22f; a1.damage = 18f; a1.knockback = 2.5f;
                var a2 = ScriptableObject.CreateInstance<AttackDefinition>();
                a2.animationKey = DarklightLuciusCombatProfile.SwordVertical;
                a2.hitShape = HitShape.Sector; a2.range = 1.85f; a2.startup = 0.16f; a2.active = 0.14f; a2.recovery = 0.24f; a2.damage = 22f; a2.knockback = 3.0f;
                var a3 = ScriptableObject.CreateInstance<AttackDefinition>();
                a3.animationKey = DarklightLuciusCombatProfile.SwordThrust;
                a3.hitShape = HitShape.Thrust; a3.range = 2.10f; a3.startup = 0.18f; a3.active = 0.10f; a3.recovery = 0.20f; a3.damage = 24f; a3.knockback = 4.0f;
                loadout = DarklightLuciusCombatProfile.Create(new[] { a1, a2, a3 }, _ => { });
            }
            CombatModel = new CombatModel(loadout);
            if (art != null) art.enabled = false;
        }

        public bool TickLucius(DarklightCampaign game, DarklightCombatLab lab, float dt)
        {
            if (!IsLucius) return false;
            if (CombatModel == null) InitializeLucius(null, LuciusView);

            if (Health != null && Health.IsDead)
            {
                warning?.Hide();
                if (body != null) body.linearVelocity = Vector2.zero;
                CombatModel.Hurt(true);
                LuciusView?.Present(CombatModel, Vector2.zero);
                return true;
            }

            if (stagger > 0)
            {
                warning?.Hide();
                if (body != null) body.linearVelocity = Vector2.zero;
                CombatModel.Hurt(false, stagger);
                LuciusView?.Present(CombatModel, Vector2.zero);
                return true;
            }

            var player = lab?.Player;
            if (player == null) return false;

            Vector2 playerPos = player.transform.position;
            Vector2 delta = playerPos - body.position;
            float distance = delta.magnitude;
            Vector2 aim = delta.sqrMagnitude > 0.0001f ? delta.normalized : facing;
            facing = aim;

            luciusDodgeCooldown = Mathf.Max(0, luciusDodgeCooldown - dt);
            luciusGuardCooldown = Mathf.Max(0, luciusGuardCooldown - dt);

            float speed = (phaseTwo ? 2.85f : 2.3f) * (game?.DifficultyProfile.speed ?? 1f);

            // Defensive reaction: if player attacks and is close, Lucius can dodge or guard
            var playerModel = player.Model;
            bool playerThreat = playerModel != null && (playerModel.State == CombatState.Attack || playerModel.State == CombatState.Charging) && distance < 2.5f;

            if (CombatModel.State == CombatState.Neutral && playerThreat)
            {
                if (luciusDodgeCooldown <= 0 && distance < 2.2f)
                {
                    // Agile tactical sidestep
                    Vector2 flank = new Vector2(-delta.normalized.y, delta.normalized.x) * (serial % 2 == 0 ? 1f : -1f);
                    DarklightNavigation.SafeStep(body, flank * 0.75f, 0.22f);
                    luciusDodgeCooldown = 2.0f;
                    timer = 0.22f;
                    serial++;
                }
                else if (luciusGuardCooldown <= 0 && distance < 2.0f && Vector2.Angle(facing, delta) < 60f)
                {
                    // Raise defensive posture
                    guardUntil = clock + 0.35f;
                    luciusGuardCooldown = 2.5f;
                }
            }

            if (CombatModel.State == CombatState.Neutral)
            {
                warning?.Hide();
                bool sight = DarklightNavigation.Clear(body.position, playerPos);
                bool tooFar = distance > luciusEngageRange * 0.95f;
                Vector2 desired = Vector2.zero;

                if (clock < guardUntil)
                {
                    // Standing ground in defensive guard
                    body.linearVelocity = Vector2.zero;
                    CombatModel.Tick(dt, new CombatInput { aim = aim });
                }
                else if (tooFar)
                {
                    if (!sight)
                    {
                        if (repath <= 0)
                        {
                            DarklightNavigation.Instance?.FindPath(body.position, playerPos, route);
                            repath = 0.45f;
                        }
                        while (route.Count > 0 && Vector2.Distance(body.position, route[0]) < 0.35f)
                            route.RemoveAt(0);
                        desired = route.Count > 0 ? (route[0] - body.position).normalized * speed : delta.normalized * speed;
                    }
                    else
                    {
                        // Flank slightly while closing in for natural swordsman footwork
                        Vector2 direct = delta.normalized;
                        Vector2 perp = new Vector2(-direct.y, direct.x) * ((int)(clock * 1.5f) % 2 == 0 ? 0.3f : -0.3f);
                        desired = (direct + perp).normalized * speed;
                    }

                    body.linearVelocity = DarklightNavigation.SlidingVelocity(body, desired, Time.fixedDeltaTime);
                    CombatModel.Tick(dt, new CombatInput { move = desired.normalized, aim = aim });
                }
                else if (distance < luciusEngageRange * 0.45f && sight)
                {
                    // Keep spacing
                    desired = -delta.normalized * (speed * 0.55f);
                    body.linearVelocity = DarklightNavigation.SlidingVelocity(body, desired, Time.fixedDeltaTime);
                    CombatModel.Tick(dt, new CombatInput { move = desired.normalized, aim = aim });
                }
                else
                {
                    // In engagement pocket: stop and evaluate attack
                    body.linearVelocity = Vector2.zero;
                    CombatModel.Tick(dt, new CombatInput { aim = aim });

                    if (sight && timer <= 0 && (game == null || game.RequestAttack(this)))
                    {
                        ExecuteLuciusAttack(luciusAttackStep, aim, game);
                        luciusAttackStep = (luciusAttackStep + 1) % 3;
                    }
                }
            }
            else if (CombatModel.State == CombatState.Attack)
            {
                body.linearVelocity = Vector2.zero;
                CombatModel.Tick(dt, new CombatInput { aim = aim });

                var attack = CombatModel.Attack;
                if (attack != null && CombatModel.StateTime < attack.startup)
                {
                    if (warning == null)
                    {
                        warning = GetComponentInChildren<DarklightAttackWarning>() ??
                                  new GameObject("AvisoAtaqueLucius").AddComponent<DarklightAttackWarning>();
                        warning.transform.SetParent(transform, false);
                    }
                    float progress = Mathf.Clamp01(CombatModel.StateTime / Mathf.Max(0.001f, attack.startup));
                    warning.Show(body.position, CombatModel.Facing, attack.range, DarklightThreat.Parryable, progress);
                }
                else
                {
                    warning?.Hide();
                }

                if (CombatModel.ActiveThisTick)
                {
                    ResolveLuciusStrike(lab, game);
                }

                if (CombatModel.State == CombatState.Neutral)
                {
                    timer = (phaseTwo ? 0.35f : 0.55f) * (game?.DifficultyProfile.cooldown ?? 1f);
                }
            }
            else
            {
                body.linearVelocity = Vector2.zero;
                CombatModel.Tick(dt, default);
            }

            LuciusView?.Present(CombatModel, body.linearVelocity);
            return true;
        }

        public void ExecuteLuciusAttack(int attackIndex, Vector2 aim, DarklightCampaign game = null)
        {
            if (CombatModel == null) return;
            var loadout = CombatModel.Loadout;
            if (loadout?.lights == null || loadout.lights.Length < 3) return;

            int idx = Mathf.Clamp(attackIndex % 3, 0, 2);
            var attack = loadout.lights[idx];

            CombatModel.TriggerAttack(attack, aim);
            facing = aim.sqrMagnitude > 0.001f ? aim.normalized : facing;
            locked = facing;
            game?.EnemyAudioWarning(this);
            serial++;
        }

        public void ResolveLuciusStrike(DarklightCombatLab lab, DarklightCampaign game)
        {
            var player = lab?.Player;
            if (player == null) return;
            var combatant = player.GetComponent<DarklightCombatant>();
            if (combatant == null || (combatant.Health != null && combatant.Health.IsDead)) return;

            Vector2 playerPos = player.transform.position;
            Vector2 luciusPos = body != null ? body.position : (Vector2)transform.position;
            Vector2 toPlayer = playerPos - luciusPos;
            float distance = toPlayer.magnitude;
            var attack = CombatModel?.Attack;
            if (attack == null) return;

            float maxRange = attack.range;
            float maxAngle = attack.hitShape == HitShape.Sweep ? 85f :
                             attack.hitShape == HitShape.Sector ? 60f : 35f;

            bool inRange = distance <= maxRange;
            bool inAngle = distance <= 0.3f || Vector2.Angle(CombatModel.Facing, toPlayer) <= maxAngle;
            bool sight = DarklightNavigation.Clear(luciusPos, playerPos);

            if (inRange && inAngle && sight)
            {
                if (CombatModel.RegisterHit(combatant))
                {
                    float damage = CombatModel.AttackDamage;
                    if (game != null) damage += game.Progress.chapter * 1.5f;
                    Vector2 force = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : CombatModel.Facing;
                    bool received = combatant.Receive(damage, force * attack.knockback);
                    if (received)
                    {
                        CombatModel.ConfirmHit();
                        DarklightCombatFX.Impact(playerPos, attack.kind != StrikeKind.Light);
                    }
                }
            }
        }
    }
}
