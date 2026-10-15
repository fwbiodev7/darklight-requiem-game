using UnityEngine;
namespace Darklight
{
    // Compatibility entry points now select only the fixed young character catalog.
    public sealed class DarklightAnimationCatalog
    {
        public Sprite Parry(int skin,int age,int weapon,int direction,CombatModel model)=>model.State==CombatState.Parry?TodayCharacterBank.Resolve(skin,0,weapon,direction,model,Vector2.zero):null;
        public Sprite Dodge(int skin,int age,int weapon,int direction,CombatModel model)=>model.State==CombatState.Dodge?TodayCharacterBank.Resolve(skin,0,weapon,direction,model,Vector2.zero):null;
        public Sprite Light(int skin,int age,int weapon,int direction,CombatModel model)=>model.State==CombatState.Attack||model.ActiveThisTick?TodayCharacterBank.Resolve(skin,0,weapon,direction,model,Vector2.zero):null;
    }
}
