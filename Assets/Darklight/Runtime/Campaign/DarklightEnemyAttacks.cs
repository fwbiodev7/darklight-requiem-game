using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        DarklightThreat committedThreat;
        float committedRange, impactDuration, recoveryDuration;
        int committedProjectiles;
        Vector2 EnemyProjectileOrigin => body.position + Vector2.up * .35f;
        bool LateBladeContact => committedThreat != DarklightThreat.Projectile &&
            (kind is RequiemEnemyKind.Valtherion or RequiemEnemyKind.Seraphine or RequiemEnemyKind.KaelFractured);
        int SecondaryBurstCount => !phaseTwo ? 0 : kind switch
        {
            RequiemEnemyKind.Kael or RequiemEnemyKind.Warden => 3,
            RequiemEnemyKind.Sentinel or RequiemEnemyKind.Matriarch => 8,
            _ => 0
        };

        public bool AreaAttack => (state == 1 || state == 2 ? committedThreat : NextEnemyThreat) == DarklightThreat.Area;

        internal bool HoldsGroundAttack => state == 1 || state == 2;
        internal DarklightThreat NextEnemyThreat => ExpandedBoss
            ? (ExpandedRanged ? DarklightThreat.Projectile : ExpandedArea ? DarklightThreat.Area : DarklightThreat.Parryable)
            : kind switch
            {
                RequiemEnemyKind.Shooter or RequiemEnemyKind.Spectral or RequiemEnemyKind.Cantor or RequiemEnemyKind.Lucien
                    => DarklightThreat.Projectile,
                RequiemEnemyKind.Armored or RequiemEnemyKind.Matriarch or RequiemEnemyKind.Sentinel
                    => DarklightThreat.Area,
                _ => DarklightThreat.Parryable
            };

        float EnemyAttackRange(DarklightThreat threat) => threat == DarklightThreat.Projectile ? 6f :
            ExpandedBoss && threat == DarklightThreat.Area ? 3.8f : Boss ? 2.5f : 1.45f;

        (float Windup, float Impact, float Recovery) EnemyAttackTiming(DarklightThreat threat) => kind switch
        {
            RequiemEnemyKind.Armored => (.70f, .18f, .31f),
            RequiemEnemyKind.Crawler => (.52f, .15f, .20f),
            RequiemEnemyKind.Hunter => (.43f, .25f, .21f),
            RequiemEnemyKind.Shooter => (.58f, .16f, .27f),
            RequiemEnemyKind.Spectral => (.60f, .18f, .29f),
            _ when ExpandedBoss => (threat == DarklightThreat.Area ? .90f :
                kind == RequiemEnemyKind.Valtherion && Phase == 1 ? .85f : .60f, .25f, .26f),
            _ when Boss => (.60f, .25f, .22f),
            _ => (.52f, .16f, .19f)
        };

        void SetEnemyFacing(Vector2 direction)
        {
            if (direction.sqrMagnitude > .0001f) facing = direction.normalized;
            art.flipX = facing.x < -.15f;
        }

        bool CanPrepareEnemyAttack(Vector2 playerPosition, Vector2 delta, float range) =>
            delta.sqrMagnitude < range * range && DarklightNavigation.Clear(body.position, playerPosition);

        void StartEnemyAttack(DarklightCampaign game, Vector2 direction, DarklightThreat threat, float range, bool followUp = false)
        {
            SetEnemyFacing(direction);
            locked = facing;
            committedThreat = threat;
            committedRange = range;
            committedProjectiles = threat == DarklightThreat.Projectile ? EnemyProjectileCount(game) : 0;

            var timing = EnemyAttackTiming(threat);
            float preparation = followUp ? Mathf.Min(timing.Windup, Mathf.Max(.48f, timing.Windup * .82f)) : timing.Windup;
            windup = timer = Mathf.Max(.32f, preparation * game.DifficultyProfile.windup);
            impactDuration = timing.Impact;
            recoveryDuration = timing.Recovery;

            state = 1;
            dash = Vector2.zero;
            body.linearVelocity = Vector2.zero;
            ResetEnemyStride();
            PresentEnemyWindup();
            game.EnemyAudioWarning(this);
        }

        void StopEnemyAttack()
        {
            state = chainRemaining = 0;
            guardUntil = 0;
            dash = Vector2.zero;
            body.linearVelocity = Vector2.zero;
            warning?.Hide();
        }

        void PresentEnemyWindup()
        {
            float progress = Mathf.Clamp01(1f - timer / windup);
            bool channeling = kind == RequiemEnemyKind.Nocthar && committedThreat == DarklightThreat.Projectile;
            // Common-class poses lead into the first attack frame. Nocthar has a distinct raised-hand charge pose.
            bool pilotShown = kind == RequiemEnemyKind.Hunter && progress >= .72f && ShowEnemyPilot("HunterLungePrep20261119");
            if (!pilotShown)
            {
                if ((!Boss || channeling) && progress < .38f) Show(3);
                else ShowAttack(LateBladeContact && progress >= .72f ? 1 : 0);
            }

            var color = committedThreat switch
            {
                DarklightThreat.Area => new Color(1f, .15f, .18f),
                DarklightThreat.Projectile => new Color(.72f, .45f, 1f),
                _ => new Color(1f, .82f, .4f)
            };
            SetEnemyTint(Color.Lerp(Color.white, color, .28f + .25f * progress));
            if (warning == null)
            {
                warning = new GameObject("AvisoDoAtaque").AddComponent<DarklightAttackWarning>();
                warning.transform.SetParent(transform, false);
            }
            if (committedThreat == DarklightThreat.Projectile)
                warning.ShowProjectileSpread(EnemyProjectileOrigin, locked, committedRange, progress, committedProjectiles);
            else warning.Show(body.position, locked, committedRange, committedThreat, progress);

            int burst = SecondaryBurstCount;
            if (burst > 0) warning.ShowProjectileOverlay(burst == 8 ? Vector2.up : locked, burst, burst == 8 ? 45 : 24, progress);
        }

        void TickEnemyAttack(DarklightCampaign game, DarklightCombatLab lab)
        {
            if (state == 1)
            {
                body.linearVelocity = Vector2.zero;
                if (timer > 0)
                {
                    PresentEnemyWindup();
                    return;
                }

                warning?.Hide();
                state = 2;
                timer = impactDuration + recoveryDuration;
                serial++;
                ShowAttack(LateBladeContact ? 2 : 1);
                SetEnemyTint(Color.white);
                PerformEnemyAttack(game, lab);
                return;
            }

            if (timer <= 0)
            {
                if (ContinueTacticalSequence(game, lab)) return;
                StopEnemyAttack();
                timer = (phaseTwo ? .5f : Boss ? .7f : .8f) * game.DifficultyProfile.cooldown;
                Show(0);
                return;
            }

            bool inContact = timer > recoveryDuration;
            SetEnemyTint(Color.white);
            int attackPose = !inContact ? 3 : !LateBladeContact && timer > recoveryDuration + impactDuration * .5f ? 1 : 2;
            ShowAttack(attackPose);
            body.linearVelocity = DarklightNavigation.SlidingVelocity(body,
                inContact ? dash * game.EnemyTimeScale : Vector2.zero, Time.fixedDeltaTime);
        }

        int EnemyProjectileCount(DarklightCampaign game)
        {
            if (ExpandedBoss) return Phase == 1 ? 3 : 5;
            if (Boss) return phaseTwo ? 7 : 3;
            int baseCount = kind == RequiemEnemyKind.Spectral ? 3 : 1;
            return Mathf.Min(5, baseCount + game.Difficulty + (elite ? 1 : 0));
        }

        void FireEnemySpread(DarklightCampaign game, float damage, float speed)
            => FireEnemyProjectiles(game, EnemyProjectileOrigin, locked, committedProjectiles,
                DarklightAttackWarning.DefaultProjectileSpacing, damage, speed);

        static void FireEnemyProjectiles(DarklightCampaign game, Vector2 origin, Vector2 direction, int projectileCount,
            int spacing, float damage, float speed)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                float angle = DarklightAttackWarning.ProjectileOffset(i, projectileCount, spacing);
                game.Fire(origin, (Vector2)(Quaternion.Euler(0, 0, angle) * direction), damage, speed);
            }
        }

        void TryEnemyMelee(DarklightCombatLab lab, float damage)
        {
            Vector2 position = lab.Player.transform.position;
            Vector2 delta = position - body.position;
            if (delta.sqrMagnitude >= committedRange * committedRange ||
                (!AreaAttack && Vector2.Angle(locked, delta) >= 70f) || !DarklightNavigation.Clear(body.position, position)) return;
            Vector2 direction = delta.sqrMagnitude > .0001f ? delta.normalized : locked;
            lab.Player.GetComponent<DarklightCombatant>().Receive(damage, direction, AreaAttack);
        }

        void PerformEnemyAttack(DarklightCampaign game, DarklightCombatLab lab)
        {
            float damage = (Boss ? 14f : kind == RequiemEnemyKind.Armored ? 15f : 8f) + game.Progress.chapter * 1.3f;
            if (elite) damage *= 1.3f;
            game.AudioDirector?.EnemyIdentity(kind, AreaAttack ? "heavy" : "attack", transform.position);

            if (ExpandedBoss)
            {
                PerformExpanded(game, lab, damage);
                return;
            }
            if (committedThreat == DarklightThreat.Projectile)
            {
                FireEnemySpread(game, damage, phaseTwo ? 5f : 3.8f);
                return;
            }

            TryEnemyMelee(lab, damage);
            if (kind == RequiemEnemyKind.Hunter) dash = locked * 4.2f;
            else if (kind is RequiemEnemyKind.Kael or RequiemEnemyKind.Warden)
                dash = locked * (phaseTwo ? 5f : 3f);
            int burst = SecondaryBurstCount;
            if (burst == 3) FireEnemyProjectiles(game, body.position, locked, burst, 24, damage * .65f, 4.7f);
            else if (burst == 8) FireEnemyProjectiles(game, body.position, Vector2.up, burst, 45, damage * .6f, 3f);
        }
    }
}
