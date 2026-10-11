using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    /// <summary>One combat clock, independent of rendering, UI, coroutines and global timeScale.</summary>
    public sealed class CombatModel
    {
        public CombatState State { get; private set; }
        public AttackDefinition Attack { get; private set; }
        public Vector2 Facing { get; private set; } = Vector2.down;
        public Vector2 Motion { get; private set; }
        public float Clock { get; private set; }
        public float StateTime { get; private set; }
        public int AttackSerial { get; private set; }
        public int ComboCount { get; private set; }
        public bool ActiveThisTick { get; private set; }
        public bool Invulnerable => State == CombatState.Dodge && StateTime >= .02f && StateTime < .19f;
        public bool PerfectParry => State == CombatState.Parry && StateTime < .14f;
        public float SkillRemaining => Mathf.Max(0, skillReady - Clock);
        public float AttackDamage => Attack == null ? 0 : Attack.damage * (Attack.kind == StrikeKind.Heavy ? 1 + ComboCount * .15f : 1);
        CombatLoadout loadout;
        int hitPulse=-1;float aerialReady,weaponSpecialReady;
        bool pendingAerial,pendingWeaponSpecial;
        public bool CanEquip=>State==CombatState.Neutral||(State==CombatState.Attack&&StateTime>=Attack.startup+Attack.active+Attack.recovery*.35f);
        public float WeaponSpecialRemaining=>Mathf.Max(0,weaponSpecialReady-Clock);
        public float AerialRemaining=>Mathf.Max(0,aerialReady-Clock);
        public bool Equip(CombatLoadout next){if(!CanEquip||next?.lights==null||next.lights.Length==0)return false;loadout=next;return true;}
        readonly HashSet<object> struck = new();
        StrikeKind queued;
        bool hasQueue;
        float queueUntil, comboUntil, dodgeReady, skillReady, counterUntil;
        Vector2 dodgeDirection; float hurtDuration=.18f;
        bool pendingDodge,pendingParry,pendingSkill;
        float pendingUntil;

        public CombatModel(CombatLoadout loadout) { this.loadout = loadout; }

        public void Tick(float delta, CombatInput input, float attackSpeed = 1)
        {
            ActiveThisTick = false;
            if (State != CombatState.Dead)
            {
                if(input.lightPressed)Queue(StrikeKind.Light);
                if(input.heavyPressed && State==CombatState.Attack)Queue(StrikeKind.Heavy);
                pendingDodge |= input.dodgePressed; pendingParry |= input.parryPressed; pendingSkill |= input.skillPressed;
                pendingAerial|=input.aerialPressed;pendingWeaponSpecial|=input.weaponSpecialPressed;
                if(input.dodgePressed||input.parryPressed||input.skillPressed||input.aerialPressed||input.weaponSpecialPressed)pendingUntil=Clock+.2f;
            }
            if (delta <= 0 || State == CombatState.Dead) { Motion = Vector2.zero; return; }
            Clock += delta;
            if(Clock>pendingUntil)ClearPending();
            if (State == CombatState.Neutral && Clock > comboUntil) ComboCount = 0;
            if (State == CombatState.Attack)
            {
                float previous = StateTime;
                StateTime += delta * attackSpeed;
                ActiveThisTick = previous < Attack.startup + Attack.active && StateTime >= Attack.startup;
                if(ActiveThisTick)
                {
                    int pulse=Mathf.Clamp(Mathf.FloorToInt((StateTime-Attack.startup)/Mathf.Max(.001f,Attack.active)*Mathf.Clamp(Attack.hitPulses,1,6)),0,Mathf.Clamp(Attack.hitPulses,1,6)-1);
                    if(pulse!=hitPulse){struck.Clear();hitPulse=pulse;}
                }
                Motion = Vector2.ClampMagnitude(input.move, 1) * .42f;
                if(ActiveThisTick&&Attack.lungeSpeed>0)Motion=Facing*Attack.lungeSpeed+Vector2.ClampMagnitude(input.move,1)*.15f;
                // Recovery cancellation only: wind-up and contact remain committed.
                if (previous >= Attack.startup + Attack.active && pendingDodge && Clock >= dodgeReady)
                { BeginDodge(input); return; }
                if(previous >= Attack.startup + Attack.active + Attack.recovery*.35f && hasQueue && Clock<=queueUntil)
                { Face(input); ConsumeQueue(); return; }
                if (StateTime >= Attack.Duration) { State = CombatState.Neutral; StateTime = 0; comboUntil = Clock + .55f; }
                return;
            }
            StateTime += delta;
            if (State == CombatState.Dodge)
            { Motion = dodgeDirection * 2.8f; if (StateTime >= .23f) EnterNeutral(); return; }
            if (State == CombatState.Parry || State == CombatState.Hurt)
            { Motion = Vector2.zero; if (StateTime >= (State == CombatState.Parry ? .26f : hurtDuration)) EnterNeutral(); return; }
            if (State == CombatState.Charging)
            {
                Motion = Vector2.ClampMagnitude(input.move, 1) * .55f;
                if(pendingDodge && Clock>=dodgeReady){BeginDodge(input);return;}
                if (input.heavyReleased || !input.heavyHeld) BeginAttack(StateTime >= .45f ? loadout.charged : loadout.heavy);
                return;
            }
            Motion = Vector2.ClampMagnitude(input.move, 1);
            Face(input);
            if (pendingDodge && Clock >= dodgeReady) { BeginDodge(input); return; }
            if (pendingParry) { State = CombatState.Parry; StateTime = 0; ClearQueue(); ClearPending(); return; }
            if (pendingSkill && Clock >= skillReady) { BeginAttack(loadout.skill); skillReady = Clock + 3f; ClearPending(); return; }
            if(pendingWeaponSpecial&&loadout.weaponSpecial!=null&&Clock>=weaponSpecialReady){BeginAttack(loadout.weaponSpecial);weaponSpecialReady=Clock+Mathf.Max(1,loadout.weaponSpecial.cooldown);ClearPending();ClearQueue();return;}
            if(pendingAerial&&loadout.aerial!=null&&Clock>=aerialReady){BeginAttack(loadout.aerial);aerialReady=Clock+Mathf.Max(.5f,loadout.aerial.cooldown);ClearPending();ClearQueue();return;}
            if (input.heavyPressed) { State = CombatState.Charging; StateTime = 0; ClearQueue(); return; }
            if (hasQueue && Clock <= queueUntil)
            {
                ConsumeQueue();
            }
            else if (Clock > queueUntil) ClearQueue();
        }

        void Face(CombatInput input) { if(input.aim.sqrMagnitude>.04f)Facing=input.aim.normalized;else if(input.move.sqrMagnitude>.01f)Facing=input.move.normalized; }
        void ConsumeQueue()
        {
            StrikeKind kind=queued;ClearQueue();
            if(kind==StrikeKind.Heavy)BeginAttack(loadout.heavy);
            else if(Clock<counterUntil){BeginAttack(loadout.counter);counterUntil=0;}
            else {ComboCount=ComboCount%loadout.lights.Length+1;BeginAttack(loadout.lights[ComboCount-1]);}
        }
        void Queue(StrikeKind kind) { queued = kind; hasQueue = true; queueUntil = Clock + .45f; }
        void ClearPending() { pendingDodge=pendingParry=pendingSkill=pendingAerial=pendingWeaponSpecial=false; }
        void ClearQueue() { hasQueue = false; }
        void EnterNeutral() { State = CombatState.Neutral; StateTime = 0; Motion = Vector2.zero; }
        void BeginDodge(CombatInput input)
        {
            dodgeDirection = input.move.sqrMagnitude > .01f ? input.move.normalized : Facing;
            State = CombatState.Dodge; StateTime = 0; dodgeReady = Clock + .38f; Motion = dodgeDirection * 2.8f; ClearPending();
            ActiveThisTick = false; ClearQueue(); ComboCount = 0;
        }
        void BeginAttack(AttackDefinition attack)
        {
            if (attack == null) return;
            Attack = attack; State = CombatState.Attack; StateTime = 0;
            AttackSerial++; struck.Clear();hitPulse=-1; ActiveThisTick = false;
        }
        public bool RegisterHit(object target) => target != null && ActiveThisTick && struck.Add(target);
        public bool Defend()
        {
            if (Invulnerable) return true;
            if (!PerfectParry) return false;
            counterUntil = Clock + .8f; return true;
        }
        public bool Defend(Vector2 towardAttacker)
        { if (Invulnerable) return true; return Vector2.Angle(Facing, towardAttacker) <= 70f && Defend(); }
        public void Hurt(bool dead = false, float duration = .18f)
        {
            hurtDuration=duration; State = dead ? CombatState.Dead : CombatState.Hurt;
            StateTime = 0; Motion = Vector2.zero; ClearQueue(); ClearPending(); ActiveThisTick = false; ComboCount = 0;
        }
        public void SuspendInput() { ClearQueue(); ClearPending(); if (State == CombatState.Charging) EnterNeutral(); }
    }
}
