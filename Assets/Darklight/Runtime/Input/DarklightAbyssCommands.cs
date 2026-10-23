using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightInput
    {
        void InitializeAbyssBindings()
        {
            Add(Gameplay,"AbyssSaw","<Mouse>/rightButton","<Gamepad>/buttonWest");
            Add(Gameplay,"AbyssBike","<Mouse>/leftButton","<Gamepad>/buttonNorth");
            Add(Gameplay,"AbyssDrift","<Keyboard>/r","<Gamepad>/dpad/up");
            Add(Gameplay,"AbyssHeavy","<Keyboard>/x","<Gamepad>/dpad/down");
        }
        public CombatInput ReadAbyss(CombatInput original,DarklightAbyssRider rider)
        {
            original.lightPressed=Action("AbyssSaw").WasPressedThisFrame();original.lightHeld=Action("AbyssSaw").IsPressed();original.lightReleased=Action("AbyssSaw").WasReleasedThisFrame();
            bool drift=rider.Dashing&&Action("AbyssDrift").WasPressedThisFrame();
            original.heavyPressed=drift||!rider.Dashing&&Action("AbyssHeavy").WasPressedThisFrame();
            original.heavyHeld=!rider.Dashing&&Action("AbyssHeavy").IsPressed();original.heavyReleased=!rider.Dashing&&Action("AbyssHeavy").WasReleasedThisFrame();
            original.weaponSpecialPressed=Action("AbyssBike").WasPressedThisFrame();
            if(drift){original.skillPressed=false;original.aerialPressed=false;}
            return original;
        }
    }
}
