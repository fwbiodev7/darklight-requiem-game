using UnityEngine;
namespace Darklight
{
    public enum DarklightWeaponId { Katana,Daggers,Chain,AbyssRider }
    [CreateAssetMenu(menuName="Darklight/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public DarklightWeaponId id;public string displayName;public int unlockAfterChapter;
        public CombatLoadout loadout;
        public Sprite[] requiemPoses,rockPoses;
        public Sprite icon;
        public Sprite[] Poses(int skin)=>skin==1?rockPoses:requiemPoses;
    }
}
