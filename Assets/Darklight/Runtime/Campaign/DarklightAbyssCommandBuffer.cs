using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightAbyssRider
    {
        const float CommandWindow=.42f;
        bool bikeBuffered,slamBuffered;
        float bikeUntil,slamUntil;
        void ClearCommands(){bikeBuffered=slamBuffered=false;bikeUntil=slamUntil=0;}
        void PrepareCommands(float dt,ref CombatInput input)
        {
            var m=fighter.Model;
            if(m.State==CombatState.Hurt||m.State==CombatState.Dead||input.dodgePressed||input.parryPressed){ClearCommands();return;}
            if(Dashing){ClearCommands();return;} // Mounted commands have their own bounded transition.
            if(m.Clock>bikeUntil)bikeBuffered=false;if(m.Clock>slamUntil)slamBuffered=false;
            if(input.weaponSpecialPressed)
            {bikeBuffered=m.WeaponSpecialRemaining<=0&&game.Blood.Amount>=DashCost;bikeUntil=m.Clock+dt+CommandWindow;slamBuffered=false;}
            else if(input.skillPressed)
            {slamBuffered=m.SkillRemaining<=0&&game.Blood.Amount>=SlamCost;slamUntil=m.Clock+dt+CommandWindow;bikeBuffered=false;}
            bool ready=CanStart(m)&&!BlocksSwap;
            input.weaponSpecialPressed=bikeBuffered&&ready;
            input.skillPressed=!bikeBuffered&&slamBuffered&&ready;
        }
        void CaptureStoppedCommands(ref CombatInput input)
        {
            if(Dashing)
            {
                var m=fighter.Model;
                if(!Drifting&&!Finishing&&!DashBlocked&&!driftUsed&&m.StateTime<=m.Attack.startup+m.Attack.active&&input.heavyPressed)
                {driftQueued=true;driftSide=Vector2.Dot(input.move,new Vector2(-dashFacing.y,dashFacing.x))<0?-1:1;}
                if(input.skillPressed&&m.StateTime<=m.Attack.startup+m.Attack.active){slamQueued=true;driftQueued=false;}
                input.heavyPressed=input.skillPressed=input.weaponSpecialPressed=false;
                return;
            }
            PrepareCommands(0,ref input);
            // The rider reserves and charges only when time resumes and a transition is safe.
            input.weaponSpecialPressed=input.skillPressed=false;
        }
    }
}
