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
            if(game?.AbyssRider?.Active==true&&!lab.Paused&&game.MenuDismissedThisFrame!=true)input=lab.Input.ReadAbyss(input,game.AbyssRider);
            bool mapPointer=game?.Atlas?.CapturesPointer==true;
            if(mapPointer){input.lightPressed=input.lightHeld=input.lightReleased=input.heavyPressed=input.heavyHeld=input.weaponSpecialPressed=false;}
            if(game?.InteractionClickConsumed==true){input.lightPressed=false;if(game.AbyssRider?.Active==true)input.weaponSpecialPressed=false;}
            if(game != null && game.WearingHeadphones) { input.lightHeld=false;input.lightPressed=input.heavyPressed=input.skillPressed=input.aerialPressed=input.weaponSpecialPressed=false; }
            if(game?.VampireCasting==true){input.lightHeld=false;input.lightPressed=input.heavyPressed=input.heavyHeld=input.skillPressed=input.aerialPressed=input.weaponSpecialPressed=input.parryPressed=input.dodgePressed=false;input.move*=.25f;}
            if(game?.Transforming==true)input=default;
            weapons?.BeforeTick(delta,!mapPointer&&!lab.Paused&&game?.WearingHeadphones!=true&&game?.VampireCasting!=true&&game?.Transforming!=true);
            if(lab.Paused){delta=0;input=default;}
            game?.AbyssRider?.BeforeCombat(delta,ref input);
            Model.FormStyle=game?.Form??0;Model.Tick(delta,input,game?.AbyssRider?.AttackClockSpeed(game.AttackSpeed)??game?.AttackSpeed??1);
            if(delta>0)game?.AbyssRider?.AfterCombat();
            if(delta>0)weapons?.AfterTick();
            if (delta > 0 && Model.ActiveThisTick){game?.WeaponSwing(Model.Attack,Model.AttackSerial);ResolveStrike();}
            view.Present(Model, Model.Motion);
            if (game == null && input.move.sqrMagnitude < .01f && lab.Input.Action("Interact").WasPressedThisFrame()) lab.ResetEncounter();
        }
        public void ResetModel() { Model = new CombatModel(loadout); body.linearVelocity = Vector2.zero; GetComponent<DarklightAbyssRider>()?.ResetMotion(); }
        void FixedUpdate()
        {
            var requested=DarklightCombatLab.Instance?.CombatDelta > 0 && DarklightCampaign.Instance?.Transforming!=true ? Model.Motion * 4.5f * (DarklightCampaign.Instance?.MovementSpeed ?? 1) : Vector2.zero;
            var rider=DarklightCampaign.Instance?.AbyssRider;
            body.linearVelocity=rider?.Dashing==true&&Model.ActiveThisTick?rider.DashVelocity(body,requested,Time.fixedDeltaTime):DarklightNavigation.SlidingVelocity(body,requested,Time.fixedDeltaTime);
        }
        void ResolveStrike()=>DarklightStrikeResolver.Resolve(this,body,contacts);
    }
}
