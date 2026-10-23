using UnityEngine;
namespace Darklight
{
    // Observes established gameplay states; it neither changes combat timing nor writes saves.
    public sealed class TodayCampaignSoundDesign : MonoBehaviour
    {
        DarklightCampaign game; DarklightCombatLab lab; readonly TodayStepContacts steps = new();
        readonly TodayLandingContacts landings=new();
        CombatState state; bool casting; int form; float heartbeat; int region=-1;
        void Awake() { game = GetComponent<DarklightCampaign>(); lab = GetComponent<DarklightCombatLab>(); }
        void LateUpdate()
        {
            if (game == null || lab?.Player == null || game.AudioDirector == null) return;
            if (region != game.MapRegion) { region = game.MapRegion; game.AudioDirector.Region(region); steps.Reset();landings.Reset(); }
            var model = lab.Player.Model; var health = lab.Player.GetComponent<HealthSystem>();
            bool walking = game.Active && lab.CombatDelta > 0 && model.State == CombatState.Neutral && !model.ActiveThisTick &&
                model.Motion.sqrMagnitude > .01f && !game.Transforming && !game.VampireCasting && !game.WearingHeadphones && !game.Weapons.Swapping;
            Vector2 p = lab.Player.transform.position;
            float height=model.State==CombatState.Attack&&model.Attack.kind==StrikeKind.Aerial?
                Mathf.Sin(Mathf.Clamp01(model.StateTime/model.Attack.Duration)*Mathf.PI)*.35f:0;
            if(game.AbyssRider?.Active==true)height+=game.AbyssRider.JumpHeight+game.AbyssRider.RunOverLift;
            if(landings.Tick(game.Active&&lab.CombatDelta>0,!health.IsDead,height>.025f))
                game.AudioDirector.Land(TodaySurfaces.Resolve(region,p,game.Map.Spawn,game.Map),p);
            if (steps.Tick(model.Clock, walking, p)) game.AudioDirector.Footstep(TodaySurfaces.Resolve(region, p, game.Map.Spawn, game.Map), p);
            if (!game.Active || lab.CombatDelta <= 0 || DarklightAutomation.Silent) return;
            if (model.State != state)
            {
                if (model.State == CombatState.Dodge) game.AudioDirector.Play("dl-power-dodge", DarklightAudioCategory.Character, .18f);
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
            { heartbeat = model.Clock + Mathf.Lerp(.65f, 1.2f, health.HealthPercent / .28f); game.AudioDirector.Play("dl-heartbeat", DarklightAudioCategory.Character, .14f, null, 1, .55f); }
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
