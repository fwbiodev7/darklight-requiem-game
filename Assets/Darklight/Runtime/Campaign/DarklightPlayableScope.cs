using UnityEngine;
namespace Darklight
{
    // Serialized weapon IDs and removed artwork remain compatible with legacy saves.
    public static class DarklightPlayableScope
    {
        public const int FinalForm=2,WeaponMask=(1<<0)|(1<<2)|(1<<3);
        public static bool WeaponActive(int id)=>id==0||id==2||id==3;
        public static int Form(int legacy)=>Mathf.Clamp(legacy,0,FinalForm);
        public static string FormName(int form)=>Form(form)==1?"RUPTURA":Form(form)==2?"ECLIPSE ABSOLUTO":"NORMAL";
        public static void Migrate(DarklightProgress state)
        {
            if(state==null)return;
            if(state.equippedWeapon==1){state.legacyEquippedWeapon=1;state.equippedWeapon=0;}
            if(state.preferredForm==3){state.legacyPreferredForm=3;state.preferredForm=FinalForm;}
            // A completed old dagger objective stays credited as its replacement.
            if((state.missionWeaponHits&2)!=0)state.missionWeaponHits|=8;
            state.preferredForm=Form(state.preferredForm);
        }
    }
}
