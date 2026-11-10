using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        public bool IsLucius { get; private set; }
        public CombatModel CombatModel { get; private set; }
        public DarklightLuciusView LuciusView { get; private set; }
        public AttackDefinition CurrentLuciusAttack => CombatModel?.Attack;
        int luciusAttackStep;
        float luciusEngageRange = 1.85f;

        public void InitializeLucius(CombatLoadout loadout = null, DarklightLuciusView view = null)
        {
            IsLucius = true;
            if (view == null) view = GetComponent<DarklightLuciusView>() ?? gameObject.AddComponent<DarklightLuciusView>();
            LuciusView = view;
            if (loadout == null)
            {
                var game = DarklightCampaign.Instance;
                if (game?.weaponDefinitions != null && game.weaponDefinitions.Length > 0 &&
                    game.weaponDefinitions[0]?.loadout?.lights?.Length == 3)
                {
                    loadout = DarklightLuciusCombatProfile.Create(game.weaponDefinitions[0].loadout.lights, _ => { });
                }
            }
            if (loadout != null)
            {
                CombatModel = new CombatModel(loadout);
            }
            if (art != null) art.enabled = false;
        }

        public bool TickLucius(DarklightCampaign game, DarklightCombatLab lab, float dt)
        {
            if (!IsLucius || CombatModel == null) return false;

            if (Health.IsDead)
            {
                warning?.Hide();
                body.linearVelocity = Vector2.zero;
                CombatModel.Hurt(true);
                LuciusView?.Present(CombatModel, Vector2.zero);
                return true;
            }

            if (stagger > 0)
            {
                warning?.Hide();
                body.linearVelocity = Vector2.zero;
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

            float speed = (phaseTwo ? 2.8f : 2.2f) * (game?.DifficultyProfile.speed ?? 1f);

            if (CombatModel.State == CombatState.Neutral)
            {
                warning?.Hide();
                bool sight = DarklightNavigation.Clear(body.position, playerPos);
                bool tooFar = distance > luciusEngageRange * 0.95f;
                Vector2 desired = Vector2.zero;

                if (tooFar)
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
                        desired = delta.normalized * speed;
                    }
                }
                else if (distance < luciusEngageRange * 0.4f && sight)
                {
                    desired = -delta.normalized * (speed * 0.5f);
                }

                body.linearVelocity = DarklightNavigation.SlidingVelocity(body, desired, Time.fixedDeltaTime);
                CombatModel.Tick(dt, new CombatInput { move = desired.normalized, aim = aim });

                if (!tooFar && sight && timer <= 0 && (game == null || game.RequestAttack(this)))
                {
                    ExecuteLuciusAttack(luciusAttackStep, aim, game);
                    luciusAttackStep = (luciusAttackStep + 1) % 3;
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
                    timer = (phaseTwo ? 0.35f : 0.6f) * (game?.DifficultyProfile.cooldown ?? 1f);
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

            CombatModel.TriggerAttack(attack);
            locked = aim;
            game?.EnemyAudioWarning(this);
            serial++;
        }

        public void ResolveLuciusStrike(DarklightCombatLab lab, DarklightCampaign game)
        {
            var player = lab?.Player;
            if (player == null) return;
            var combatant = player.GetComponent<DarklightCombatant>();
            if (combatant == null || combatant.Health.IsDead) return;

            Vector2 playerPos = player.transform.position;
            Vector2 toPlayer = playerPos - body.position;
            float distance = toPlayer.magnitude;
            var attack = CombatModel?.Attack;
            if (attack == null) return;

            float maxRange = attack.range;
            float maxAngle = attack.hitShape == HitShape.Sweep ? 85f :
                             attack.hitShape == HitShape.Sector ? 60f : 35f;

            bool inRange = distance <= maxRange;
            bool inAngle = distance <= 0.3f || Vector2.Angle(CombatModel.Facing, toPlayer) <= maxAngle;
            bool sight = DarklightNavigation.Clear(body.position, playerPos);

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
