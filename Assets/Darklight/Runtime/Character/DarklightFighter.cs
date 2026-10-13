using UnityEngine;

namespace Darklight
{
    [RequireComponent(typeof(Rigidbody2D), typeof(DarklightCharacterView))]
    public sealed class DarklightFighter : MonoBehaviour
    {
        public CombatLoadout loadout;
        public CombatModel Model { get; private set; }
        Rigidbody2D body;
        DarklightCharacterView view;
        DarklightWeaponController weapons;
        readonly Collider2D[] contacts = new Collider2D[128];
        void Awake() { Model = new CombatModel(loadout); body = GetComponent<Rigidbody2D>(); view = GetComponent<DarklightCharacterView>();weapons=GetComponent<DarklightWeaponController>(); var col=GetComponent<Collider2D>(); if(col!=null&&col.sharedMaterial==null) col.sharedMaterial=DarklightNavigation.Frictionless; }
        void Update()
        {
            var lab = DarklightCombatLab.Instance;
            if (lab == null || !lab.Ready) return;
            float delta = lab.CombatDelta;
            var game = DarklightCampaign.Instance;
            var input = !lab.Paused && game?.MenuDismissedThisFrame!=true ? lab.Input.Read(body.position, lab.WorldCamera, lab.Viewport) : default;
            if(game?.InteractionClickConsumed==true)input.lightPressed=false;
            if(game != null && game.WearingHeadphones) { input.lightPressed=input.heavyPressed=input.skillPressed=input.aerialPressed=input.weaponSpecialPressed=false; }
            if(game?.VampireCasting==true){input.lightPressed=input.heavyPressed=input.heavyHeld=input.skillPressed=input.aerialPressed=input.weaponSpecialPressed=input.parryPressed=input.dodgePressed=false;input.move*=.25f;}
            if(game?.Transforming==true)input=default;
            weapons?.BeforeTick(delta,!lab.Paused&&game?.WearingHeadphones!=true&&game?.VampireCasting!=true&&game?.Transforming!=true);
            Model.Tick(delta, input, game?.AttackSpeed ?? 1);
            if(delta>0)weapons?.AfterTick();
            if (delta > 0 && Model.ActiveThisTick){game?.WeaponSwing(Model.Attack,Model.AttackSerial);ResolveStrike();}
            view.Present(Model, Model.Motion);
            if (game == null && input.move.sqrMagnitude < .01f && lab.Input.Action("Interact").WasPressedThisFrame()) lab.ResetEncounter();
        }
        public void ResetModel() { Model = new CombatModel(loadout); body.linearVelocity = Vector2.zero; }
        void FixedUpdate()
        {
            var requested=DarklightCombatLab.Instance?.CombatDelta > 0 && DarklightCampaign.Instance?.Transforming!=true ? Model.Motion * 4.5f * (DarklightCampaign.Instance?.MovementSpeed ?? 1) : Vector2.zero;
            body.linearVelocity=DarklightNavigation.SlidingVelocity(body,requested,Time.fixedDeltaTime);
        }
        void ResolveStrike()
        {
            AttackDefinition attack = Model.Attack;
            int count = Physics2D.OverlapCircle(body.position, attack.range + (DarklightCampaign.Instance?.StrikeRangeBonus ?? 0), new ContactFilter2D { useTriggers = true, useLayerMask = false }, contacts);
            for (int i = 0; i < count; i++)
            {
                var target = contacts[i].GetComponentInParent<DarklightCombatant>();
                if (target == null || target.player || target.Health.IsDead) continue;
                Vector2 toTarget = (Vector2)target.transform.position - body.position;
                if (Vector2.Angle(Model.Facing, toTarget) > attack.angle * .5f) continue;
                // Solid scenery blocks attacks; hurtbox triggers do not.
                var blockers = Physics2D.RaycastAll(body.position, toTarget.normalized, toTarget.magnitude);
                bool blocked = false;
                foreach (var hit in blockers)
                    if (!hit.collider.isTrigger && hit.collider.GetComponentInParent<DarklightCombatant>() == null && hit.collider.gameObject != gameObject) blocked = true;
                if (blocked || !Model.RegisterHit(target)) continue;
                if (target.Receive(Model.AttackDamage * (DarklightCampaign.Instance?.DamageMultiplier ?? 1)*(DarklightCampaign.Instance?.VampireDamage(attack.kind)??1), toTarget.normalized * attack.knockback))
                { DarklightCombatLab.Instance.Coach?.Record(DarklightLesson.Strike);DarklightCombatLab.Instance.Hitstop(attack.hitPulses>1?.010f:attack.kind==StrikeKind.Light?.022f:.035f); DarklightCampaign.Instance?.WeaponHit(target,attack,Model.AttackSerial);var enemy=target.GetComponent<DarklightEnemy>();enemy?.Struck(attack.knockback*(DarklightCampaign.Instance?.Resonance.Impact??1),attack.kind != StrikeKind.Light);if(attack.pullTarget)enemy?.PullToward(body.position,1.6f); DarklightCombatFX.Impact(target.transform.position,attack.kind!=StrikeKind.Light); }
            }
        }
    }
}
