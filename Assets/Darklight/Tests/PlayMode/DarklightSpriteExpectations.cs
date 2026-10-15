using UnityEngine;
namespace Darklight.Tests
{
    internal static class DarklightSpriteExpectations
    {
        public static Sprite Today(int skin,int age,int weapon,int direction,CombatModel model,Vector2 movement)
            =>TodayCharacterBank.Resolve(skin,0,weapon,direction,model,movement)??TodayCharacterBank.StablePose(skin,0,weapon,direction);
        public static Texture2D Mask(Sprite sprite)=>TodayCharacterBank.Mask(sprite)??DarklightUserSpriteBank.Mask(sprite);
        public static void ClearTodayCache()=>TodayCharacterBank.ClearCache();
    }
}
