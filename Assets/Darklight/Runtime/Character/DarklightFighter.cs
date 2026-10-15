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
        void Awake() { Model = new CombatModel(loadout); body = GetComponent<Rigidbody2D>(); view = GetComponent<DarklightCharacterView>();weapons=GetComponent<DarklightWeaponController>();gameObject.AddComponent<DarklightCombatDebug>(); var col=GetComponent<Collider2D>(); if(col!=null&&col.sharedMaterial==null) col.sharedMaterial=DarklightNavigation.Frictionless; }
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
            Model.FormStyle=game?.Form??0;Model.Tick(delta, input, game?.AttackSpeed ?? 1);
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
        void ResolveStrike()=>DarklightStrikeResolver.Resolve(this,body,contacts);
    }
}
