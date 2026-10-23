using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightAbyssRider
    {
        public int ExpansionMoveIndex(AttackDefinition attack)=>ExpandedMoves==null?-1:System.Array.IndexOf(ExpandedMoves,attack);
    }
    public static partial class DarklightAbyssSpriteBank
    {
        static Sprite[] expansionSouth;
        static Sprite ResolveExpansion(DarklightAbyssRider rider,CombatModel model,int form)
        {
            if(form!=0||DarklightCharacterView.DirectionIndex(model.Facing)!=0||model.State!=CombatState.Attack)return null;
            int move=rider.ExpansionMoveIndex(model.Attack);if(move<0)return null;
            expansionSouth??=Resources.LoadAll<Sprite>("Darklight/Multiverse/MotorcycleSouth");
            if(expansionSouth.Length!=12)return null;
            // Names are stable; Resource enumeration order is not.
            string name="MotorcycleSouth_"+(move*2+(model.StateTime<model.Attack.startup?0:1)).ToString("00");
            foreach(var sprite in expansionSouth)if(sprite.name==name)return sprite;return null;
        }
    }
}
