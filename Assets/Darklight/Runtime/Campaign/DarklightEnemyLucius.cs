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
        public bool LuciusFinalExchange { get; private set; }
        float luciusTransformationTime;
        int luciusAttackStep;
        const float LuciusCloseRange = .83f;
        const float LuciusGuardDuration = .35f;
        float luciusDodgeCooldown;
        float luciusGuardCooldown;
        int luciusResolvedAttackSerial = -1;
        Vector2 luciusDodgeDirection;

        public void InitializeLucius(CombatLoadout loadout = null, DarklightLuciusView view = null)
        {
            IsLucius = true;
            if (view == null) view = GetComponent<DarklightLuciusView>() ?? gameObject.AddComponent<DarklightLuciusView>();
            LuciusView = view;
            if (loadout == null)
            {
                // Both campaign and training spawn Lucius without an explicit loadout.
                // His sword must use the same bases regardless of Fabius's equipped weapon.
                var weapons = DarklightCampaign.Instance?.weaponDefinitions;
                var katana = weapons?.Length > 0 ? weapons[(int)DarklightWeaponId.Katana]?.loadout : null;
                var practiceLoadout = DarklightCombatLab.Instance?.Player?.loadout;
                if (katana?.lights?.Length != DarklightLuciusCombatProfile.VariationCount &&
                    practiceLoadout?.weapon == (int)DarklightWeaponId.Katana)
                    katana = practiceLoadout;
                if (katana?.lights?.Length == DarklightLuciusCombatProfile.VariationCount)
                    loadout = DarklightLuciusCombatProfile.Create(katana.lights, _ => { });
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
            timer = .85f;
            if (art != null) art.enabled = false;
        }

        public bool TickLucius(DarklightCampaign game, DarklightCombatLab lab, float dt)
        {
            if (!IsLucius) return false;
            if (Resolved) { CancelLuciusTransformation(); warning?.Hide(); body.linearVelocity=Vector2.zero; return true; }
            if (CombatModel == null) InitializeLucius(null, LuciusView);
            if(dt<=0){if(body!=null)body.linearVelocity=Vector2.zero;return true;}

            if (Health != null && Health.IsDead)
            {
                CancelLuciusTransformation();
                warning?.Hide();
                if (body != null) body.linearVelocity = Vector2.zero;
                if(CombatModel.State!=CombatState.Dead)CombatModel.Hurt(true);
                CombatModel.Tick(dt,default);
                LuciusView?.Present(CombatModel, Vector2.zero);
                return true;
            }

            TickPosture(dt);
            if (Airborne || stagger > 0)
            {
                warning?.Hide();
                body.linearVelocity = Vector2.zero;
                if (CombatModel.State == CombatState.Hurt) CombatModel.Tick(dt, default);
                if (CombatModel.State != CombatState.Hurt) CombatModel.Hurt(false, Airborne ? .3f : stagger);
                LuciusView?.Present(CombatModel, Vector2.zero);
                PresentLuciusAura();
                return true;
            }

            if (CombatModel.State == CombatState.Neutral && !phaseTwo && Health != null && Health.HealthPercent <= .60f)
            {
                phaseTwo = true;
                Phase = 2;
                CombatModel.SuspendInput();
                guardUntil = posture = luciusLaunchWindowUntil = 0;
                luciusFormReleased=false;
                luciusOfficialTransition=DarklightOfficialTransitionBank.TryPose(3,0,out _);
                if(!luciusOfficialTransition)DarklightCombatFX.Transformation(transform.position,3,false,ArconteEnergy);
                game?.BossPhaseTwo();
                game?.Notice("LUCIUS | Voce ainda reconhece seu irmao?");
                luciusTransformationTime = LuciusTransformationDuration;
            }
            if (luciusTransformationTime > 0)
            {
                luciusTransformationTime = Mathf.Max(0,luciusTransformationTime-dt);
                float progress=1-luciusTransformationTime/LuciusTransformationDuration;
                if(!luciusFormReleased&&progress>=.52f)
                {
                    luciusFormReleased=true;
                    if(LuciusView!=null)LuciusView.appearance=DarklightLuciusAppearance.Arconte;
                    if(!luciusOfficialTransition)DarklightCombatFX.Transformation(transform.position,3,true,ArconteEnergy);
                }
                body.linearVelocity = Vector2.zero;
                warning?.Hide();
                // Complete existing authored poses; no procedural facial/body edits.
                if(LuciusView!=null)
                {
                    float authoredTime = DarklightLuciusSpriteBank.TryActionPage(DarklightLuciusAppearance.Arconte,"Transformation",0,null,out var transition)
                        ? DarklightLuciusSpriteBank.FitStateTime(transition,progress,1f) : progress;
                    LuciusView.PresentPose("Transformation",0,authoredTime);
                }
                PresentLuciusTransition(progress);
                if (!LuciusTransforming)
                {
                    timer = .45f;
                    luciusPunishUntil = clock + timer;
                }
                return true;
            }
            if (phaseTwo && !LuciusFinalExchange && CombatModel.State == CombatState.Neutral &&
                Health != null && Health.HealthPercent <= .25f)
            {
                LuciusFinalExchange = true;
                Phase = 3;
                luciusAttackStep = 0;
                timer = Mathf.Max(timer, .7f);
                luciusPunishUntil = clock + timer;
                game?.Notice("FABIUS | A primeira espada era de madeira. Lembra?");
            }

            var player = lab?.Player;
            if (player == null || player.Model.State == CombatState.Dead)
            {
                warning?.Hide();
                body.linearVelocity = Vector2.zero;
                return true;
            }

            Vector2 playerPos = player.transform.position;
            Vector2 delta = playerPos - body.position;
            float distance = delta.magnitude;
            Vector2 aim = delta.sqrMagnitude > 0.0001f ? delta.normalized : facing;
            if (CombatModel.State == CombatState.Neutral && !Guarding) facing = aim;

            luciusDodgeCooldown = Mathf.Max(0, luciusDodgeCooldown - dt);
            luciusGuardCooldown = Mathf.Max(0, luciusGuardCooldown - dt);
            repath = Mathf.Max(0, repath - dt);

            float speed = (phaseTwo ? 2.85f : 2.3f) * (game?.DifficultyProfile.speed ?? 1f);

            var playerModel = player.Model;
            bool sight = DarklightNavigation.Clear(body.position, playerPos);
            bool playerThreat = ReadLuciusThreat(playerModel, distance, aim, sight, game?.DifficultyProfile.reaction ?? .22f);

            if (CombatModel.State == CombatState.Neutral && !Guarding && playerThreat)
            {
                Vector2 flank = new Vector2(-aim.y, aim.x) * (serial % 2 == 0 ? 1f : -1f);
                if (luciusDodgeCooldown <= 0 && distance < 2.2f && TryLuciusSidestep(flank, aim, dt))
                {
                    luciusAnsweredThreat = true;
                    LuciusView?.Present(CombatModel, body.linearVelocity);
                    PresentLuciusAura();
                    return true;
                }
                if (luciusGuardCooldown <= 0 && distance < 2.0f)
                {
                    guardUntil = clock + LuciusGuardDuration;
                    luciusGuardCooldown = 2.5f;
                    luciusAnsweredThreat = true;
                }
            }

            if (CombatModel.State == CombatState.Neutral)
            {
                warning?.Hide();
                if (Guarding || clock < luciusPunishUntil)
                {
                    body.linearVelocity = Vector2.zero;
                    CombatModel.Tick(dt, new CombatInput { aim = Guarding ? facing : aim });
                }
                else
                {
                    int pattern = sight ? ChooseLuciusPattern(distance, playerModel) : -1;
                    if (TryLuciusDecisionAttack(game, pattern, aim))
                    {
                        body.linearVelocity = Vector2.zero;
                    }
                    else
                    {
                        Vector2 desired = pattern < 0 ? LuciusPursuit(playerPos, aim, sight) * speed :
                            distance < LuciusCloseRange ? -aim * (speed * .55f) : Vector2.zero;
                        body.linearVelocity = DarklightNavigation.SlidingVelocity(body, desired, Time.fixedDeltaTime);
                        CombatModel.Tick(dt, new CombatInput { move = body.linearVelocity.normalized, aim = aim });
                    }
                }
            }
            else if (CombatModel.State == CombatState.Attack)
            {
                body.linearVelocity = Vector2.zero;
                CombatModel.Tick(dt, new CombatInput { aim = locked });

                PresentLuciusAttackWarning(CombatModel.Attack);

                if (CombatModel.ActiveThisTick)
                {
                    ResolveLuciusStrike(lab, game);
                }

                if (CombatModel.State == CombatState.Neutral)
                {
                    FinishLuciusExchange(game);
                }
            }
            else
            {
                CombatModel.Tick(dt, default);
                body.linearVelocity = CombatModel.State==CombatState.Dodge
                    ?DarklightNavigation.SlidingVelocity(body,luciusDodgeDirection*2.8f,Time.fixedDeltaTime)
                    :Vector2.zero;
            }

            if (Guarding && CombatModel.State == CombatState.Neutral && LuciusView != null)
            {
                int direction = DarklightCharacterView.DirectionIndex(facing);
                float elapsed = LuciusGuardDuration - (guardUntil - clock);
                if(DarklightLuciusSpriteBank.TryActionPage(LuciusView.appearance,"Parry",direction,null,out var guard))
                    elapsed=DarklightLuciusSpriteBank.FitStateTime(guard,elapsed,LuciusGuardDuration);
                LuciusView.PresentPose("Parry",direction,elapsed);
            }
            else LuciusView?.Present(CombatModel, body.linearVelocity);
            PresentLuciusAura();
            return true;
        }

        Vector2 LuciusPursuit(Vector2 playerPosition, Vector2 aim, bool sight)
        {
            if (sight)
            {
                Vector2 side = new Vector2(-aim.y, aim.x) * ((int)(clock * 1.5f) % 2 == 0 ? .3f : -.3f);
                return (aim + side).normalized;
            }

            if (repath <= 0)
            {
                DarklightNavigation.Instance?.FindPath(body.position, playerPosition, route);
                repath = .45f;
            }
            while (route.Count > 0 && Vector2.Distance(body.position, route[0]) < .35f) route.RemoveAt(0);
            return route.Count > 0 ? (route[0] - body.position).normalized : aim;
        }

        bool TryLuciusSidestep(Vector2 flank, Vector2 aim, float dt)
        {
            const float evadeDistance = 2.8f * global::Darklight.CombatModel.DodgeDuration;
            if (!DarklightNavigation.Clear(body.position, body.position + flank * evadeDistance))
            {
                flank = -flank;
                if (!DarklightNavigation.Clear(body.position, body.position + flank * evadeDistance)) return false;
            }

            CombatModel.Tick(dt, new CombatInput { move = flank, aim = aim, dodgePressed = true });
            luciusDodgeDirection = flank;
            luciusDodgeCooldown = 2f;
            timer = Mathf.Max(timer, .22f);
            serial++;
            body.linearVelocity = DarklightNavigation.SlidingVelocity(body, flank * 2.8f, Time.fixedDeltaTime);
            return true;
        }

        static int LuciusStrikeHalfAngle(HitShape shape) => shape == HitShape.Sweep ? 85 : shape == HitShape.Sector ? 60 : 35;

        void PresentLuciusAttackWarning(AttackDefinition attack)
        {
            if (CombatModel.State != CombatState.Attack || attack == null || CombatModel.StateTime >= attack.startup)
            {
                warning?.Hide();
                return;
            }
            if (warning == null)
            {
                warning = GetComponentInChildren<DarklightAttackWarning>() ??
                          new GameObject("AvisoAtaqueLucius").AddComponent<DarklightAttackWarning>();
                warning.transform.SetParent(transform, false);
            }
            float progress = Mathf.Clamp01(CombatModel.StateTime / Mathf.Max(.001f, attack.startup));
            warning.Show(body.position, locked, attack.range, DarklightThreat.Parryable, progress, LuciusStrikeHalfAngle(attack.hitShape));
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
            if (CombatModel == null || !CombatModel.ActiveThisTick || luciusResolvedAttackSerial == CombatModel.AttackSerial) return;
            var player = lab?.Player;
            if (player == null) return;
            var combatant = player.GetComponent<DarklightCombatant>();
            if (combatant == null || (combatant.Health != null && combatant.Health.IsDead)) return;

            Vector2 playerPos = player.transform.position;
            Vector2 luciusPos = body != null ? body.position : (Vector2)transform.position;
            Vector2 toPlayer = playerPos - luciusPos;
            float distance = toPlayer.magnitude;
            var attack = CombatModel.Attack;
            if (attack == null) return;

            bool inRange = distance <= attack.range;
            bool inAngle = distance <= .3f || Vector2.Angle(CombatModel.Facing, toPlayer) <= LuciusStrikeHalfAngle(attack.hitShape);
            if (!inRange || !inAngle || !DarklightNavigation.Clear(luciusPos, playerPos)) return;

            luciusResolvedAttackSerial = CombatModel.AttackSerial;
            float damage = CombatModel.AttackDamage;
            if (game != null) damage += game.Progress.chapter * 1.5f;
            Vector2 direction = distance > .001f ? toPlayer / distance : CombatModel.Facing;
            Vector2 force = direction * attack.knockback;
            var defender = player.Model;
            bool parried = defender != null && defender.PerfectParry && Vector2.Angle(defender.Facing, -force) <= 70f;
            if (combatant.Receive(damage, force))
            {
                CombatModel.ConfirmHit();
                DarklightCombatFX.Impact(playerPos, attack.kind != StrikeKind.Light);
            }
            else if (parried)
            {
                stagger = Mathf.Max(stagger, .52f);
                OpenLuciusLaunchWindow(.5f);
                timer = Mathf.Max(timer, .65f);
                CombatModel.Hurt(false, stagger);
                warning?.Hide();
                var clash = (playerPos + luciusPos) * .5f;
                lab.WorldCamera?.GetComponent<DarklightLabCamera>()?.ConfirmContact(clash);
                DarklightCombatFX.Impact(clash, true);
            }
        }
    }
}
