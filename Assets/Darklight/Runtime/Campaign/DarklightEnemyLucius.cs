// DarklightEnemyLucius.cs
// Candidata - NAO INTEGRAR antes de Fabius Normal + Gothic Eclipse + sprites aprovados.
// Destino proposto: Assets/Darklight/Runtime/Campaign/DarklightEnemyLucius.cs
//
// Extensao partial de DarklightEnemy para comportamento melee de Lucius.
// Substitui o comportamento ranged hardcoded para RequiemEnemyKind.Lucien.
//
// REGRAS:
// - Nao le input de teclado/mouse/controle de Fabius.
// - Nao acessa DarklightCampaign.weaponDefinitions nem loadout de Fabius.
// - Usa apenas campos observaveis: posicao, estado do CombatModel, distancia, relogio.
// - Janelas de reacao com atraso (reaction delay) configuravel; sem reacao perfeita.
// - Loadout proprio via DarklightLuciusLoadout.Get().

using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        // -- Estado Lucius --
        float luciusReactionDelay = 0.28f;  // janela justa: nao reage no mesmo frame
        float luciusSpecialCooldown;
        float luciusHeavyCooldown;
        int   luciusComboStep;
        bool  luciusMeleeReady;
        float luciusDistancePreference = 2.0f; // distancia de engajamento preferida

        bool IsLucius => kind == RequiemEnemyKind.Lucien;

        // Chamado por DarklightEnemy.Update() antes da logica ranged.
        // Retorna true se consumiu o tick; o enemy principal nao deve continuar.
        bool TickLuciusBoss(DarklightCampaign game, DarklightCombatLab lab, float dt,
            Vector2 delta, float distance)
        {
            if (!IsLucius) return false;

            // Posicao da fase 2 registrada em DarklightEnemy.phaseTwo (campo existente).
            float engageRange = phaseTwo ? luciusDistancePreference * 0.85f : luciusDistancePreference;

            // -- Cooldowns proprios --
            luciusHeavyCooldown  = Mathf.Max(0, luciusHeavyCooldown - dt);
            luciusSpecialCooldown= Mathf.Max(0, luciusSpecialCooldown - dt);

            // -- Movimento: controle de distancia --
            if (state == 0 && stagger <= 0 && !Health.IsDead)
            {
                bool tooFar  = distance > engageRange * 1.3f;
                bool tooClose= distance < engageRange * 0.55f;
                Vector2 desired;
                float speed = phaseTwo ? 3.0f : 2.4f;

                if (tooClose)
                    desired = -delta.normalized * speed * 0.7f;
                else if (tooFar)
                    desired = delta.normalized * speed;
                else
                {
                    // Flanquear lateralmente para evitar filas
                    float sideSign = ((int)(clock * 0.4f) % 2 == 0) ? 1f : -1f;
                    var perp = new Vector2(-delta.normalized.y, delta.normalized.x) * sideSign;
                    desired = (delta.normalized * 0.5f + perp * 0.5f).normalized * speed * 0.6f;
                }

                bool sight = DarklightNavigation.Clear(body.position, lab.Player.transform.position);
                if (!sight)
                {
                    if (repath <= 0)
                    {
                        DarklightNavigation.Instance?.FindPath(body.position,
                            lab.Player.transform.position, route);
                        repath = 0.4f;
                    }
                    while (route.Count > 0 && Vector2.Distance(body.position, route[0]) < 0.35f)
                        route.RemoveAt(0);
                    if (route.Count > 0) desired = (route[0] - body.position).normalized * speed;
                }

                body.linearVelocity = DarklightNavigation.SlidingVelocity(body, desired, Time.fixedDeltaTime);
                facing = delta.normalized;
                Show(tooFar || tooClose ? 1 + (int)(clock * 12) % 2 : 0);
            }

            // -- Decisao de ataque --
            if (state == 0 && stagger <= 0 && timer <= 0 && !Health.IsDead)
            {
                bool inRange = distance <= engageRange * 1.15f;
                bool sight   = DarklightNavigation.Clear(body.position, lab.Player.transform.position);
                bool permit  = game.RequestAttack(this);
                var  player  = lab.Player.Model;

                // Reacao defensiva com atraso justo (nao perfeito)
                bool playerAttacking = (player.State == CombatState.Attack || player.State == CombatState.Charging)
                    && distance < (player.Attack?.range ?? 2f) + 1.0f;
                if (playerAttacking)
                {
                    if (!luciusMeleeReady) { luciusMeleeReady = true; }
                    // Aplica atraso de reacao
                    if (clock < dodgeReadAt || dodgeReadAt < 0)
                    {
                        if (dodgeReadAt < 0) dodgeReadAt = clock + luciusReactionDelay;
                    }
                    else if (clock >= dodgeReadAt && clock >= guardReady)
                    {
                        // Esquiva lateral contextual (boss nao guarda sempre)
                        if (phaseTwo || serial % 3 == 0)
                        {
                            var side = new Vector2(-delta.normalized.y, delta.normalized.x) *
                                       (serial % 2 == 0 ? 1f : -1f);
                            DarklightNavigation.SafeStep(body, side * 0.7f);
                            guardReady = clock + (phaseTwo ? 1.4f : 2.0f);
                            timer = 0.25f; serial++;
                        }
                        dodgeReadAt = -1;
                    }
                }
                else { luciusMeleeReady = false; dodgeReadAt = -1; }

                // Escolha de tecnica
                if (inRange && sight && permit)
                {
                    StrikeKind chosen;
                    if (luciusSpecialCooldown <= 0 && (phaseTwo || serial % 5 == 4))
                    {
                        chosen = StrikeKind.Skill;
                        luciusSpecialCooldown = DarklightLuciusLoadout.SpecialCooldown;
                    }
                    else if (luciusHeavyCooldown <= 0 && serial % 3 == 2)
                    {
                        chosen = StrikeKind.Heavy;
                        luciusHeavyCooldown = DarklightLuciusLoadout.HeavyCooldown;
                    }
                    else
                    {
                        chosen = StrikeKind.Light;
                    }

                    float windup = chosen == StrikeKind.Skill ? DarklightLuciusLoadout.SpecialStartup :
                                   chosen == StrikeKind.Heavy  ? DarklightLuciusLoadout.HeavyStartup   :
                                   DarklightLuciusLoadout.ComboStartup;
                    windup = Mathf.Max(0.32f, windup * game.DifficultyProfile.windup);

                    state  = 1;
                    timer  = this.windup = windup;
                    locked = delta.normalized;
                    body.linearVelocity = Vector2.zero;
                    game.EnemyAudioWarning(this);
                    serial++;
                }
            }

            return true; // consumiu o tick
        }
    }
}