using UnityEngine;
namespace Darklight
{
    public static class DarklightWholeCharacterBank
    {
        // Historical names never reload retired age textures.
        public static Sprite[] Sheet(string name)=>System.Array.Empty<Sprite>();
        public static Sprite Resolve(int skin,int age,int weapon,int direction,CombatModel model,Vector2 movement)=>TodayCharacterBank.Resolve(skin,0,weapon,direction,model,movement);
        public static Texture2D Mask(Sprite sprite)=>TodayCharacterBank.Mask(sprite);
    }
}
