using UnityEngine;
namespace Darklight.Tests
{
    internal static class DarklightSpriteExpectations
    {
        static readonly DarklightAnimationCatalog Approved=new();
        // Exact new selection is checked only where it may replace a preserved set.
        public static Sprite Today(int skin,int age,int weapon,int direction,CombatModel model,Vector2 movement)
        {
            if(skin==0&&age==4&&weapon==0&&(Approved.Light(skin,age,weapon,direction,model)!=null||Approved.Parry(skin,age,weapon,direction,model)!=null||Approved.Dodge(skin,age,weapon,direction,model)!=null))return null;
            var existing=DarklightWholeCharacterBank.Resolve(skin,age,weapon,direction,model,movement);
            if(existing!=null)return null;
            return TodayCharacterBank.Resolve(skin,age,weapon,direction,model,movement);
        }
        public static Texture2D Mask(Sprite sprite)=>TodayCharacterBank.Mask(sprite)??DarklightUserSpriteBank.Mask(sprite);
        public static void ClearTodayCache()=>typeof(TodayCharacterBank).GetMethod("ClearCache",System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static)?.Invoke(null,null);
    }
}
