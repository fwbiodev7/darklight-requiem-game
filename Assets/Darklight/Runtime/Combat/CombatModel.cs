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
        public bool AttackConnected { get; private set; }
        public uint ContactPulses { get;private set; }
        public float ContactProgress { get;private set; }
        public float ContactStartProgress {get;private set;}
        public int Branch { get;private set; }
        public int FormStyle {get;set;}
        public bool Invulnerable => State == CombatState.Dodge && StateTime >= .02f && StateTime < (FormStyle==2?.21f:FormStyle==1?.18f:.19f);
        public bool PerfectParry => State == CombatState.Parry && StateTime < .14f;
        public float SkillRemaining => Mathf.Max(0, skillReady - Clock);
        public float AttackDamage => Attack == null ? 0 : Attack.damage * (Attack.kind == StrikeKind.Heavy ? 1 + ComboCount * .15f : 1);
        CombatLoadout loadout;
        int hitPulse=-1;float aerialReady,weaponSpecialReady;
        bool pendingAerial,pendingWeaponSpecial;
        public bool CanEquip=>State==CombatState.Neutral||(State==CombatState.Attack&&!ActiveThisTick&&StateTime>=Attack.startup+Attack.active+Attack.recovery*(AttackConnected?(FormStyle==3?0:.08f):Attack.recoveryChainFraction));
        public float WeaponSpecialRemaining=>Mathf.Max(0,weaponSpecialReady-Clock);
        public float AerialRemaining=>Mathf.Max(0,aerialReady-Clock);
        public bool Equip(CombatLoadout next){if(!CanEquip||next?.lights==null||next.lights.Length==0)return false;loadout=next;return true;}
        readonly HashSet<object> struck = new();
        readonly HashSet<object>[] pulseTargets={new(),new(),new(),new(),new(),new()};
        float lastRecovery=-10;
        StrikeKind queued;
        bool hasQueue;
        float queueUntil, comboUntil, dodgeReady, skillReady, counterUntil;
        Vector2 dodgeDirection; float hurtDuration=.18f;
        bool pendingDodge,pendingParry,pendingSkill;
        public const float DefenseBuffer=.22f, AttackBuffer=.32f;
        float dodgeUntil,parryUntil,skillUntil,aerialUntil,specialUntil;

        public CombatModel(CombatLoadout loadout) { this.loadout = loadout; }

        public void Tick(float delta, CombatInput input, float attackSpeed = 1)
        {
            ActiveThisTick = false;ContactPulses=0;
            if (State != CombatState.Dead)
            {
                if(input.lightPressed)Queue(StrikeKind.Light,delta);
                if(input.heavyPressed && State==CombatState.Attack)Queue(StrikeKind.Heavy,delta);
                Buffer(ref pendingDodge,ref dodgeUntil,input.dodgePressed,delta);
                Buffer(ref pendingParry,ref parryUntil,input.parryPressed,delta);
                Buffer(ref pendingSkill,ref skillUntil,input.skillPressed,delta);
                Buffer(ref pendingAerial,ref aerialUntil,input.aerialPressed,delta);
                Buffer(ref pendingWeaponSpecial,ref specialUntil,input.weaponSpecialPressed,delta);
            }
            if (delta <= 0 || State == CombatState.Dead) { Motion = Vector2.zero; return; }
            Clock += delta;
            Expire(ref pendingDodge,dodgeUntil);Expire(ref pendingParry,parryUntil);Expire(ref pendingSkill,skillUntil);Expire(ref pendingAerial,aerialUntil);Expire(ref pendingWeaponSpecial,specialUntil);
            if (State == CombatState.Neutral && Clock > comboUntil) ComboCount = 0;
            if (State == CombatState.Attack)
            {
                float previous = StateTime;
                StateTime += delta * attackSpeed;
                ActiveThisTick = previous < Attack.startup + Attack.active && StateTime >= Attack.startup;
                if(ActiveThisTick)
                {
                    int total=Mathf.Clamp(Attack.hitPulses,1,6);
                    for(int p=0;p<total;p++)
                        if(previous<Attack.startup+Attack.active*(p+1)/total&&StateTime>=Attack.startup+Attack.active*p/total)ContactPulses|=1u<<p;
                    ContactProgress=Mathf.Clamp01((StateTime-Attack.startup)/Mathf.Max(.001f,Attack.active));
                    ContactStartProgress=Mathf.Clamp01((previous-Attack.startup)/Mathf.Max(.001f,Attack.active));
                    int pulse=Mathf.Clamp(Mathf.FloorToInt((StateTime-Attack.startup)/Mathf.Max(.001f,Attack.active)*Mathf.Clamp(Attack.hitPulses,1,6)),0,Mathf.Clamp(Attack.hitPulses,1,6)-1);
                    if(pulse!=hitPulse){struck.Clear();hitPulse=pulse;}
                }
                Motion = Vector2.ClampMagnitude(input.move, 1) * (StateTime<Attack.startup?Attack.windupMove:StateTime>=Attack.startup+Attack.active?Attack.recoveryMove:.42f);
                if(StateTime<Attack.startup*.5f)Face(input);
                if(ActiveThisTick&&Attack.lungeSpeed>0)Motion=Facing*Attack.lungeSpeed+Vector2.ClampMagnitude(input.move,1)*.15f;
                // Recovery cancellation only: wind-up and contact remain committed.
                float cancelAt=Attack.startup+Attack.active+Attack.recovery*Mathf.Max(0,(AttackConnected?Attack.hitCancelFraction:Attack.whiffCancelFraction)-1);
                if(FormStyle==1&&Attack.kind!=StrikeKind.Light)cancelAt+=Attack.recovery*.2f;
                if (!ActiveThisTick&&previous >= cancelAt && pendingDodge && Clock >= dodgeReady)
                { BeginDodge(input); return; }
                if(!ActiveThisTick && StateTime>=Attack.startup+Attack.active && pendingParry){Face(input);BeginParry();return;}
                if(AttackConnected&&!ActiveThisTick&&StateTime>=Attack.startup+Attack.active+Attack.recovery*.12f)
                {
                    if(FormStyle==3&&pendingSkill&&Clock>=skillReady){BeginAttack(loadout.skill);skillReady=Clock+3;return;}
                    if(pendingAerial&&loadout.aerial!=null&&Clock>=aerialReady){BeginAttack(loadout.aerial);aerialReady=Clock+Mathf.Max(.5f,loadout.aerial.cooldown);return;}
                    if(pendingWeaponSpecial&&loadout.weaponSpecial!=null&&Clock>=weaponSpecialReady){BeginAttack(loadout.weaponSpecial);weaponSpecialReady=Clock+Mathf.Max(1,loadout.weaponSpecial.cooldown);return;}
                }
                if(previous >= Attack.startup + Attack.active + Attack.recovery*Attack.recoveryChainFraction && hasQueue && Clock<=queueUntil)
                { Face(input); ConsumeQueue(); return; }
                if (StateTime >= Attack.Duration) { State = CombatState.Neutral; StateTime = 0;lastRecovery=Clock; comboUntil = Clock + .55f; }
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
                if (input.heavyReleased || !input.heavyHeld) BeginAttack(StateTime >= (loadout.charged?.chargeSeconds??.45f) ? loadout.charged : loadout.heavy);
                return;
            }
            Motion = Vector2.ClampMagnitude(input.move, 1);
            Face(input);
            if (pendingDodge && Clock >= dodgeReady) { BeginDodge(input); return; }
            if (pendingParry) { BeginParry(); return; }
            if (pendingSkill && Clock >= skillReady) { BeginAttack(loadout.skill); skillReady = Clock + 3f; ClearPending();ClearQueue(); return; }
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
            if(kind==StrikeKind.Heavy){Branch=2;BeginAttack(ComboCount>=2&&loadout.heavyBranch!=null?loadout.heavyBranch:loadout.heavy);}
            else if(Clock<counterUntil){BeginAttack(loadout.counter);counterUntil=0;}
            else if(ComboCount>0&&Clock-lastRecovery>=.18f&&Clock-lastRecovery<.55f&&loadout.pauseBranch!=null){Branch=1;ComboCount++;BeginAttack(loadout.pauseBranch);lastRecovery=-10;}
            else {Branch=0;ComboCount=ComboCount%loadout.lights.Length+1;BeginAttack(loadout.lights[ComboCount-1]);}
        }
        void Queue(StrikeKind kind,float delta) { queued = kind; hasQueue = true; queueUntil = Clock + Mathf.Max(0,delta) + (loadout.attackBuffer>0?loadout.attackBuffer:AttackBuffer); }
        void Buffer(ref bool flag,ref float expiry,bool pressed,float delta){if(pressed){flag=true;expiry=Clock+Mathf.Max(0,delta)+(loadout.defenseBuffer>0?loadout.defenseBuffer:DefenseBuffer);}}
        void Expire(ref bool flag,float expiry){if(Clock>expiry)flag=false;}
        void BeginParry(){State=CombatState.Parry;StateTime=0;Motion=Vector2.zero;ClearQueue();ClearPending();}
        void ClearPending() { pendingDodge=pendingParry=pendingSkill=pendingAerial=pendingWeaponSpecial=false; }
        void ClearQueue() { hasQueue = false; }
        void EnterNeutral() { State = CombatState.Neutral; StateTime = 0; Motion = Vector2.zero; }
        void BeginDodge(CombatInput input)
        {
            dodgeDirection = input.move.sqrMagnitude > .01f ? input.move.normalized : Facing;
            State = CombatState.Dodge; StateTime = 0; dodgeReady = Clock + .38f; Motion = dodgeDirection * 2.8f; ClearPending();
            ActiveThisTick = false; ClearQueue();if(FormStyle==2){comboUntil=Clock+.7f;lastRecovery=Clock;}else ComboCount = 0;
        }
        void BeginAttack(AttackDefinition attack)
        {
            if (attack == null) return;
            Attack = attack; State = CombatState.Attack; StateTime = 0;
            ClearPending();AttackSerial++; struck.Clear();foreach(var targets in pulseTargets)targets.Clear();hitPulse=-1; ActiveThisTick = false;AttackConnected=false;
        }
        public bool RegisterHit(object target){bool hit=target != null && ActiveThisTick && struck.Add(target);if(hit)AttackConnected=true;return hit;}
        public bool RegisterPulseHit(object target,int pulse)=>target!=null&&ActiveThisTick&&(ContactPulses&(1u<<pulse))!=0&&pulseTargets[pulse].Add(target);
        public void ConfirmHit()=>AttackConnected=true;
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
