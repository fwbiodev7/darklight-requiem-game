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
        }
        public CombatInput ReadAbyss(CombatInput original,DarklightAbyssRider rider)
        {
            original.lightPressed=Action("AbyssSaw").WasPressedThisFrame();original.lightHeld=Action("AbyssSaw").IsPressed();original.lightReleased=Action("AbyssSaw").WasReleasedThisFrame();
            bool drift=rider.Dashing&&Action("AbyssDrift").WasPressedThisFrame();
            original.heavyPressed=drift;original.heavyHeld=original.heavyReleased=false;
            original.weaponSpecialPressed=Action("AbyssBike").WasPressedThisFrame();
            if(drift){original.skillPressed=false;original.aerialPressed=false;}
            return original;
        }
    }
}
