using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public WeaponDefinition[] weaponDefinitions;
        public DarklightWeaponController Weapons {get;private set;}
        public void RecordWeaponSelection()=>SaveCheckpoint();
        void InitializeWeapons(){InitializeTechnicalCombat();InitializeAbyssRider();Weapons=lab.Player.GetComponent<DarklightWeaponController>();Weapons.Initialize(this);}
        void UnlockWeaponReward()
        {
            int index=Progress.chapter==2?2:-1;
            if(index<0||(Progress.weaponUnlocks&(1<<index))!=0)return;
            Progress.weaponUnlocks|=1<<index;Notice("ARMA LIBERADA: "+weaponDefinitions[index].displayName);
        }
    }
}
