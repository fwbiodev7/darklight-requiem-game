using System;
using UnityEngine;

namespace Darklight
{
    // Exactly three native sword choreographies. Optional combat input slots
    // alias these same instances; they never manufacture another move or power.
    public static class DarklightLuciusCombatProfile
    {
        public const string SwordHorizontal="SwordHorizontal";
        public const string SwordVertical="SwordVertical";
        public const string SwordThrust="SwordThrust";
        public const int VariationCount=3;

        public static CombatLoadout Create(AttackDefinition[] fabiusBase,Action<UnityEngine.Object> own)
        {
            if(fabiusBase==null||fabiusBase.Length!=VariationCount)
                throw new ArgumentException("Lucius requires exactly three existing sword attack bases.",nameof(fabiusBase));
            if(own==null)throw new ArgumentNullException(nameof(own));
            foreach(var source in fabiusBase)
                if(source==null)throw new ArgumentException("Lucius sword bases must all exist.",nameof(fabiusBase));
            string[] keys={SwordHorizontal,SwordVertical,SwordThrust};
            string[] names={"Corte Horizontal","Corte Vertical","Estocada"};
            HitShape[] shapes={HitShape.Sweep,HitShape.Sector,HitShape.Thrust};
            var attacks=new AttackDefinition[VariationCount];
            for(int i=0;i<attacks.Length;i++)
            {
                var attack=attacks[i]=UnityEngine.Object.Instantiate(fabiusBase[i]);own(attack);
                attack.name="Lucius_"+keys[i];attack.displayName=names[i];attack.animationKey=keys[i];
                // Keep authored timing, damage, range, width, angle, contacts,
                // movement and cancellation data. Only select the sword pattern.
                attack.hitShape=shapes[i];
            }
            return new CombatLoadout{
                weapon=0,attackBuffer=.18f,defenseBuffer=.18f,lights=attacks,
                heavy=attacks[1],charged=attacks[1],counter=attacks[2],skill=attacks[0],
                aerial=attacks[1],weaponSpecial=attacks[0],pauseBranch=attacks[2],heavyBranch=attacks[1],
                swapEntry=attacks[2],airFinisher=attacks[1]};
        }
    }
}
