using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        int luciusReadSerial=-1,luciusPreviousPattern=-1;
        CombatState luciusReadState;
        float luciusReadAt=-1,luciusPunishUntil,luciusHitRecoilUntil;
        bool luciusAnsweredThreat;

        bool ReadLuciusThreat(CombatModel target, float distance, Vector2 towardPlayer, bool sight, float reaction)
        {
            if (target == null || (target.State != CombatState.Attack && target.State != CombatState.Charging))
            {
                luciusReadAt = -1;
                luciusReadSerial = -1;
                luciusAnsweredThreat = false;
                return false;
            }

            bool charging = target.State == CombatState.Charging;
            if (luciusReadSerial != target.AttackSerial || luciusReadState != target.State)
            {
                bool releasedCharge = !charging && luciusReadState == CombatState.Charging &&
                    target.AttackSerial == luciusReadSerial + 1 && luciusReadSerial >= 0;
                if (!releasedCharge)
                {
                    luciusReadAt = -1;
                    luciusAnsweredThreat = false;
                }
                luciusReadSerial = target.AttackSerial;
                luciusReadState = target.State;
            }

            var attack = charging ? target.Loadout?.charged ?? target.Loadout?.heavy : target.Attack;
            bool visible = sight && attack != null && (charging || target.StateTime < attack.startup + attack.active) &&
                distance <= attack.range + .45f && Vector2.Angle(target.Facing, -towardPlayer) <= 85f;
            if (!visible)
            {
                luciusReadAt = -1;
                return false;
            }

            if (luciusReadAt < 0) luciusReadAt = clock + Mathf.Max(.12f, reaction * (phaseTwo ? .85f : 1f));
            return !luciusAnsweredThreat && clock >= luciusReadAt && clock >= luciusPunishUntil;
        }

        int ChooseLuciusPattern(float distance, CombatModel target)
        {
            var attacks = CombatModel.Loadout?.lights;
            if (attacks == null || attacks.Length < 3) return -1;
            bool recovery = target?.State == CombatState.Attack && target.Attack != null &&
                target.StateTime >= target.Attack.startup + target.Attack.active;
            int preferred = distance > attacks[0].range * .9f || recovery ? 2 :
                distance < attacks[1].range * .7f ? 0 : luciusAttackStep;
            int repeat = -1;
            for (int i = 0; i < 3; i++)
            {
                int candidate = (preferred + i) % 3;
                if (distance > attacks[candidate].range) continue;
                if (candidate != luciusPreviousPattern) return candidate;
                repeat = candidate;
            }
            return repeat;
        }

        bool TryLuciusDecisionAttack(DarklightCampaign game, int pattern, Vector2 aim)
        {
            if (pattern < 0 || timer > 0 || clock < luciusPunishUntil || (game != null && !game.RequestAttack(this))) return false;
            ExecuteLuciusAttack(pattern, aim, game);
            PresentLuciusAttackWarning(CombatModel.Attack);
            luciusPreviousPattern = pattern;
            luciusAttackStep = (pattern + 1) % 3;
            return true;
        }

        void FinishLuciusExchange(DarklightCampaign game)
        {
            float opening = CombatModel.AttackConnected ? .10f : LuciusFinalExchange ? .22f : phaseTwo ? .25f : .30f;
            luciusPunishUntil = clock + opening;
            float cadence = LuciusFinalExchange ? (luciusAttackStep == 0 ? .75f : .15f) : phaseTwo ? .35f : .55f;
            timer = Mathf.Max(opening, cadence * (game?.DifficultyProfile.cooldown ?? 1f));
        }

        internal void ReactToLuciusDamage()
        {
            if (Health.IsDead)
            {
                CombatModel?.Hurt(true);
            }
            else
            {
                // Consecutive hits still damage and build posture. Only an earned stagger can extend the opening.
                if (Airborne || stagger > 0 || clock < luciusHitRecoilUntil) return;
                luciusHitRecoilUntil = clock + .60f;
                timer = Mathf.Max(timer, .26f);
                CombatModel?.Hurt();
            }
            guardUntil = 0;
            body.linearVelocity = Vector2.zero;
            warning?.Hide();
        }
    }
}
