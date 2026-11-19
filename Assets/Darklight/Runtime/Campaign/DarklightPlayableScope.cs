using UnityEngine;
namespace Darklight
{
    // Serialized weapon IDs and removed artwork remain compatible with legacy saves.
    public static class DarklightPlayableScope
    {
        // Keep serialized IDs stable: only Katana (0) and Ruin Fists (2) are playable.
        public const int FinalForm=2,WeaponMask=(1<<0)|(1<<2);
        public static bool WeaponActive(int id)=>id==0||id==2;
        public static int Form(int legacy)=>Mathf.Clamp(legacy,0,FinalForm);
        public static string FormName(int form)=>Form(form)==1?"RUPTURA":Form(form)==2?"SERAFIM CARMESIM":"NORMAL";
        public static void Migrate(DarklightProgress state)
        {
            if(state==null)return;
            // Preserve removed selections as history, but never restore them as active gear.
            // Unlock bits are intentionally left intact for old saves and progression history.
            if(state.equippedWeapon==1||state.equippedWeapon==3)
            {state.legacyEquippedWeapon=state.equippedWeapon;state.equippedWeapon=0;}
            if(state.preferredForm==3){state.legacyPreferredForm=3;state.preferredForm=FinalForm;}
            // A completed old dagger objective stays credited as its replacement.
            if((state.missionWeaponHits&2)!=0)state.missionWeaponHits|=8;
            state.preferredForm=Form(state.preferredForm);
        }
    }
}
