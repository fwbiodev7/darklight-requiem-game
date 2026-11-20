using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    /// <summary>Map-scoped assistance. Allies never enter the hostile registry or own boss damage.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(DarklightCombatant))]
    public sealed class DarklightCampaignAlly : MonoBehaviour
    {
        public enum AllyState { Follow, Wait, Reposition, Attack, Recovery, Incapacitated }
        public AllyState State { get; private set; }
        public CombatModel Model { get; private set; }
        public DarklightEnemy Target => target;

        DarklightCampaign game;
        DarklightCombatLab lab;
        DarklightMapDefinition map;
        Rigidbody2D body;
        HealthSystem health;
        SpriteRenderer art;
        Sprite idle;
        DarklightLadyPresentation ladyPresentation;
        Color restingTint;
        AttackDefinition assist;
        DarklightEnemy target;
        readonly List<Vector2> route = new();
        Vector2 desiredVelocity;
        float cooldown, recovery, repath;
        bool ranged, initialized;

        public void Initialize(DarklightCampaign campaign, DarklightCombatLab combatLab, bool ranged)
        {
            Stop();
            if (health != null) health.OnDamageReceived -= Hurt;
            ReleaseDefinition();
            game = campaign;
            lab = combatLab;
            map = campaign != null ? campaign.Map : null;
            this.ranged = ranged;
            body = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthSystem>();
            art = GetComponentInChildren<SpriteRenderer>();
            idle = art != null ? art.sprite : null;
            ladyPresentation = null;
            if (art != null) restingTint = art.color;
            var combatant = GetComponent<DarklightCombatant>();
            // Refuse an accidentally reused enemy/player actor rather than converting its faction.
            initialized = game != null && lab != null && map != null && body != null &&
                health != null && combatant != null && !combatant.player &&
                GetComponent<DarklightEnemy>() == null && GetComponent<DarklightFighter>() == null;
            if (!initialized) { enabled = false; return; }
            if (ranged && art != null) ladyPresentation = new DarklightLadyPresentation(art, body.position);
            assist = ScriptableObject.CreateInstance<AttackDefinition>();
            assist.hideFlags = HideFlags.HideAndDontSave;
            assist.displayName = ranged ? "Apoio de Lady" : "Intervenção de Nero";
            assist.damage = ranged ? 7f : 10f;
            assist.range = ranged ? 4.8f : 1.65f;
            assist.startup = ranged ? .32f : .26f;
            assist.active = .1f;
            assist.recovery = .55f;
            // Private definitions avoid mutating an approved weapon or sharing state between allies.
            Model = new CombatModel(new CombatLoadout { lights = new[] { assist } });
            health.OnDamageReceived += Hurt;
            target = null;
            route.Clear();
            cooldown = .8f;
            recovery = repath = 0;
            State = AllyState.Wait;
            enabled = true;
        }

        bool CanTick => initialized && game != null && lab != null && game.Active &&
            game.Map == map && lab.CombatDelta > 0 && lab.Player != null &&
            lab.Player.Model.State != CombatState.Dead && health != null && !health.IsDead;

        void Update()
        {
            if (!initialized) return;
            // Parent destruction normally handles transitions; this also closes deferred-destroy gaps.
            if (game == null || lab == null || game.Map != map) { Stop(); Destroy(gameObject); return; }
            if (health == null || health.IsDead)
            {
                State = AllyState.Incapacitated;
                if (game.Active) ladyPresentation?.PresentDefeat(lab.CombatDelta);
                Stop();
                return;
            }
            if (!CanTick) { Stop(); return; }
            float dt = lab.CombatDelta;
            cooldown = Mathf.Max(0, cooldown - dt);
            recovery = Mathf.Max(0, recovery - dt);
            repath -= dt;
            Model.Tick(dt, default);
            desiredVelocity=Vector2.zero;
            if (recovery > 0)
                State = AllyState.Recovery;
            else if (Model.State == CombatState.Attack)
            {
                State = Model.StateTime >= assist.startup + assist.active ? AllyState.Recovery : AllyState.Attack;
                if (!CanAssistTarget(target) || ReservedForPlayer(target)) Model.FinishAttackContact();
                else if (Model.ActiveThisTick &&
                    Vector2.Distance(body.position, target.transform.position) <= assist.range &&
                    DarklightNavigation.Clear(body.position, target.transform.position) && Model.RegisterHit(target))
                {
                    // No area overlap or projectile: only the validated hostile receives this contact.
                    var receiver = target.GetComponent<DarklightCombatant>();
                    if (receiver != null && !receiver.player)
                    {
                        bool hit = receiver.Receive(assist.damage, ((Vector2)target.transform.position - body.position).normalized);
                        if (ladyPresentation != null)
                            DarklightCombatFX.LadyShot(ladyPresentation.Muzzle(Model.Facing), (Vector2)target.transform.position + Vector2.up * .62f, hit);
                    }
                }
            }
            else
            {
                target = SelectTarget();
                Vector2 player = lab.Player.transform.position;
                float playerDistance = Vector2.Distance(body.position, player);
                if (playerDistance > 8f)
                {
                    FollowPlayer(player,playerDistance);
                }
                else if (target != null)
                {
                    Vector2 foe = target.transform.position;
                    float distance = Vector2.Distance(body.position, foe);
                    bool sight = DarklightNavigation.Clear(body.position, foe);
                    if (cooldown <= 0 && distance <= assist.range && sight)
                    {
                        Model.TriggerAttack(assist, foe - body.position);
                        cooldown = ranged ? 2f : 1.65f;
                        State = AllyState.Attack;
                    }
                    else if (ranged && distance < 2.2f)
                    {
                        State = AllyState.Reposition;
                        MoveToward(body.position + (body.position - foe).normalized * 1.5f);
                    }
                    else if (distance > assist.range * .85f || !sight)
                    {
                        State = AllyState.Reposition;
                        MoveToward(foe);
                    }
                    else State = AllyState.Wait;
                }
                else FollowPlayer(player,playerDistance);
            }
            if (art != null && !ranged)
            {
                art.sortingOrder = DarklightSorting.OrderForY(body.position.y);
                int direction = DarklightCharacterView.DirectionIndex(Model.Facing);
                art.sprite = Model.State == CombatState.Attack
                    ? DarklightCastActionFrames.Get("Nero", "Attack", direction, DarklightCastActionFrames.AttackPhase(Model, assist)) ?? idle
                    : idle;
                art.color = State == AllyState.Attack
                    ? Color.Lerp(restingTint, new Color(.8f, .75f, 1f), .18f) : restingTint;
            }
        }

        void LateUpdate()
        {
            if(ladyPresentation==null)return;
            Vector2 position=transform.position;
            if(!CanTick){ladyPresentation.Pause(position);return;}
            art.sortingOrder=DarklightSorting.OrderForY(position.y);
            ladyPresentation.Present(position,body.linearVelocity,Model,assist,lab.CombatDelta);
        }

        void FollowPlayer(Vector2 player,float distance)
        {
            float approachDistance=ranged&&State==AllyState.Follow?1.95f:2.6f;
            if(distance<=approachDistance){State=AllyState.Wait;return;}
            State=AllyState.Follow;
            float speed=Mathf.Lerp(4.8f,6.2f,Mathf.InverseLerp(5f,8f,distance));
            if(ranged)speed*=Mathf.SmoothStep(.30f,1f,Mathf.InverseLerp(1.95f,3.15f,distance));
            MoveToward(player,speed);
        }

        /// <summary>Faction is determined by the campaign registry, never by collider proximity.</summary>
        public bool CanAssistTarget(DarklightEnemy enemy)
        {
            if (!initialized || game == null || game.Map != map || enemy == null || !enemy.isActiveAndEnabled ||
                enemy.Boss || enemy.IsLucius || enemy.Visitor != DarklightVisitor.None || enemy.Resolved ||
                enemy.Health == null || enemy.Health.IsDead || !enemy.AcceptDamage ||
                enemy.GetComponent<DarklightCampaignAlly>() != null) return false;
            var receiver = enemy.GetComponent<DarklightCombatant>();
            if (receiver == null || receiver.player) return false;
            foreach (var current in game.MapEnemies) if (current == enemy) return true;
            return false;
        }

        bool ReservedForPlayer(DarklightEnemy enemy)
        {
            if (enemy == null || lab == null || lab.Player == null) return true;
            // Preserve launch, stagger/finisher openings and the player's current combo reach.
            if (enemy.Airborne || enemy.Vulnerable || lab.Player.AirCombo?.Target == enemy) return true;
            var player = lab.Player.Model;
            float distance = Vector2.Distance(enemy.transform.position, lab.Player.transform.position);
            if ((player.State == CombatState.Attack || player.State == CombatState.Charging) &&
                distance <= Mathf.Max(3.5f, (player.Attack != null ? player.Attack.range : 0) + 1f)) return true;
            return player.ComboCount > 0 && distance < 3.5f;
        }

        DarklightEnemy SelectTarget()
        {
            DarklightEnemy best = null;
            float bestScore = float.MinValue;
            Vector2 player = lab.Player.transform.position;
            foreach (var enemy in game.MapEnemies)
            {
                if (!CanAssistTarget(enemy) || ReservedForPlayer(enemy)) continue;
                float toPlayer = Vector2.Distance(enemy.transform.position, player);
                float toAlly = Vector2.Distance(enemy.transform.position, body.position);
                if (toPlayer > 7f || toAlly > 9f) continue;
                bool attacking = enemy.StateName == "Preparando" || enemy.StateName == "Ataque";
                float score = (attacking ? 12f : 0f) + (toPlayer < 3f ? 5f : 0f) - toPlayer - toAlly * .4f;
                if (score <= bestScore) continue;
                best = enemy;
                bestScore = score;
            }
            return best;
        }

        internal void Regroup()
        {
            if(!initialized||!ranged)return;
            if(health.IsDead)
            {
                health.SetMaxHealth(health.MaxHealth,false);
                Model=new CombatModel(new CombatLoadout {lights=new[] {assist}});
                ladyPresentation?.ResetDefeat();
                recovery=0;
            }
            target=null;
            route.Clear();
            repath=0;
            Model.FinishAttackContact();
        }

        void MoveToward(Vector2 destination,float speed=3.4f)
        {
            Rect bounds = map.Bounds;
            const float margin = .4f;
            destination.x = Mathf.Clamp(destination.x, bounds.xMin + margin, bounds.xMax - margin);
            destination.y = Mathf.Clamp(destination.y, bounds.yMin + margin, bounds.yMax - margin);
            Vector2 direction = destination - body.position;
            if (!DarklightNavigation.Clear(body.position, destination))
            {
                if (repath <= 0)
                {
                    DarklightNavigation.Instance?.FindPath(body.position, destination, route);
                    repath = .5f;
                }
                while (route.Count > 0 && Vector2.Distance(body.position, route[0]) < .35f) route.RemoveAt(0);
                direction = route.Count > 0 ? route[0] - body.position : Vector2.zero;
            }
            else route.Clear();
            desiredVelocity = direction.sqrMagnitude > .04f ? direction.normalized * speed : Vector2.zero;
        }

        void FixedUpdate()
        {
            if (body == null) return;
            if(!CanTick){body.linearVelocity=Vector2.zero;return;}
            float step=Time.fixedDeltaTime;
            bool locomotion=State==AllyState.Follow||State==AllyState.Reposition||State==AllyState.Wait;
            Vector2 requested=ranged&&locomotion
                ? Vector2.MoveTowards(body.linearVelocity,desiredVelocity,30f*step)
                : desiredVelocity;
            body.linearVelocity=DarklightNavigation.SlidingVelocity(body,requested,step);
        }

        void Hurt(float amount)
        {
            if (amount <= 0) return;
            recovery = .65f;
            Model?.Hurt(health.IsDead, recovery);
            State = health.IsDead ? AllyState.Incapacitated : AllyState.Recovery;
            Stop();
        }

        void Stop()
        {
            desiredVelocity = Vector2.zero;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        void OnDisable()
        {
            Stop();
            Model?.FinishAttackContact();
            if (art != null) art.color = restingTint;
        }

        void OnDestroy()
        {
            if (health != null) health.OnDamageReceived -= Hurt;
            ReleaseDefinition();
        }

        void ReleaseDefinition()
        {
            if (assist == null) return;
            if (Application.isPlaying) Destroy(assist);
            else DestroyImmediate(assist);
            assist = null;
        }
    }
}
