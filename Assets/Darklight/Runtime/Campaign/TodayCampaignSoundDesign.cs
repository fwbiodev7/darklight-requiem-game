using UnityEngine;
namespace Darklight
{
    // Observes established gameplay states; it neither changes combat timing nor writes saves.
    public sealed class TodayCampaignSoundDesign : MonoBehaviour
    {
        DarklightCampaign game; DarklightCombatLab lab; readonly TodayStepContacts steps = new();
        readonly TodayLandingContacts landings=new();
        readonly TodayPowerAudioTransitions powerTransitions=new();
        readonly TodayCooldownAudio cooldowns=new();
        CombatState state; CombatModel observedModel; bool casting,dead; int form,weapon=-1; float heartbeat,cloth,lastStep=-10; int region=-1;
        void Awake() { game = GetComponent<DarklightCampaign>(); lab = GetComponent<DarklightCombatLab>(); }
        void LateUpdate()
        {
            if (game == null || lab?.Player == null || game.AudioDirector == null) return;
            if (region != game.MapRegion) { region = game.MapRegion; game.AudioDirector.Region(region); steps.Reset();landings.Reset(); powerTransitions.Reset();cooldowns.Reset();weapon=-1; }
            var model = lab.Player.Model; var health = lab.Player.GetComponent<HealthSystem>();
            if(!ReferenceEquals(observedModel,model))
            { observedModel=model;heartbeat=cloth=0;lastStep=-10;steps.Reset();landings.Reset();powerTransitions.Reset();cooldowns.Reset();weapon=-1; }
            // Death pauses combat before LateUpdate. A single character voice
            // may finish across that pause; scene/map cleanup still stops it.
            if (health.IsDead)
            {
                game.AudioDirector.StopDelivered("PC04");game.AudioDirector.StopDelivered("PC06");
                powerTransitions.Reset();cooldowns.Reset();weapon=-1;
                if (!dead) game.AudioDirector.PlayerDeath();
                dead=true;return;
            }
            dead=false;
            bool walking = game.Active && lab.CombatDelta > 0 && model.State == CombatState.Neutral && !model.ActiveThisTick &&
                model.Motion.sqrMagnitude > .01f && !game.Transforming && !game.VampireCasting && !game.WearingHeadphones && !game.Weapons.Swapping;
            Vector2 p = lab.Player.transform.position;
            float height=lab.Player.AirCombo?.Height>0?lab.Player.AirCombo.Height:model.State==CombatState.Attack&&model.Attack.kind==StrikeKind.Aerial?
                Mathf.Sin(Mathf.Clamp01(model.StateTime/model.Attack.Duration)*Mathf.PI)*.35f:0;
            if(game.AbyssRider?.Active==true)height+=game.AbyssRider.JumpHeight+game.AbyssRider.RunOverLift;
            if(landings.Tick(game.Active&&lab.CombatDelta>0,!health.IsDead,height>.025f))
                game.AudioDirector.Land(TodaySurfaces.Resolve(region,p,game.Map.Spawn,game.Map),p);
            bool footContact=steps.Tick(model.Clock, walking, p);
            if (footContact) {lastStep=model.Clock;game.AudioDirector.Footstep(TodaySurfaces.Resolve(region, p, game.Map.Spawn, game.Map), p);}
            if (!walking || model.Clock-lastStep>.3f) game.AudioDirector.StopDelivered("PC06");
            if (health.HealthPercent >= .28f) game.AudioDirector.StopDelivered("PC04");
            bool advancing=game.Active&&lab.CombatDelta>0;
            ObservePowerEnds(advancing);
            int equipped=(int)game.Weapons.Equipped.id;
            if (equipped!=weapon)
            {
                if(weapon>=0&&advancing){game.AudioDirector.StowWeapon();game.AudioDirector.EquipWeapon(equipped);}
                weapon=equipped;
            }
            if (!advancing) {state=model.State;form=game.PresentedForm;casting=castReleased=false;return;}
            if (footContact && model.Clock >= cloth)
            { cloth=model.Clock+5.2f;game.AudioDirector.Delivered("PC06",DarklightAudioCategory.Character,.08f,5.2f); }
            if (model.State != state)
            {
                if (model.State == CombatState.Dodge) game.AudioDirector.PlayerDodge();
                state = model.State;
            }
            // Vampire pulses already have authored .12/.18s contact points; release a texture on that exact crossing.
            if (game.VampireCasting && !casting && !castReleased) casting = true;
            if (casting && game.VampireCasting && game.VampireCastProgress >= (game.Progress.vampireSelected == 0 ? .24f : .36f))
            { game.AudioDirector.Ability(game.Progress.vampireSelected); casting = false; castReleased = true; }
            if (!game.VampireCasting) { casting = false; castReleased = false; }
            if (game.PresentedForm != form)
            { form = game.PresentedForm; if (form > 0) game.AudioDirector.Transform(form); }
            if (health.HealthPercent < .28f && !health.IsDead && model.Clock >= heartbeat)
            { heartbeat = model.Clock + 7.2f; game.AudioDirector.LowHealthBreath(); }
        }
        void ObservePowerEnds(bool advancing)
        {
            var events=powerTransitions.Tick(advancing,game.PresentedForm,game.Blood.FrenzyRemaining>0,game.Resonance.Active,game.WearingHeadphones);
            if((events&(TodayPowerAudioEvent.FormEnded|TodayPowerAudioEvent.FrenzyEnded|TodayPowerAudioEvent.ResonanceEnded))!=0)
                game.AudioDirector.PowerEnded();
            if((events&TodayPowerAudioEvent.HeadphonesRemoved)!=0)game.AudioDirector.RemoveHeadphones();
            int selected=game.Progress.vampireSelected;
            bool bloodReady=DarklightVampireSkills.Unlocked(game.Progress,selected)&&
                game.Blood.Recovery[selected]<=0&&game.Blood.Amount>=DarklightVampireSkills.Costs[selected]&&!game.VampireCasting;
            if(cooldowns.Tick(advancing,game.Blood.Recovery,selected,bloodReady,game.Resonance.Recovery,game.Resonance.Selected,
                game.Progress.mp3Unlocked&&game.Resonance.Ready&&!game.WearingHeadphones))game.AudioDirector.CooldownReady();
        }
        bool castReleased;
    }
    // Suspend the contact while paused; death and map changes discard it.
    public sealed class TodayLandingContacts
    {
        bool armed;
        public void Reset()=>armed=false;
        public bool Tick(bool advancing,bool alive,bool airborne)
        {
            if(!alive){Reset();return false;}
            if(!advancing)return false;
            if(airborne){armed=true;return false;}
            bool landed=armed;armed=false;return landed;
        }
    }
}
