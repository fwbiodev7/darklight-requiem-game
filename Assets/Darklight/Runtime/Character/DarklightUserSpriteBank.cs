using System;
using UnityEngine;
namespace Darklight
{
    // The supplied sheets contain complete young characters, never body/head layers.
    public static class DarklightUserSpriteBank
    {
        static readonly string[,] Names={{"requiemkatana1","requiemadaga1","requiemfoice1"},{"rockkatana1","rockadaga1","rockfoice1"}};
        static readonly Sprite[][] Pages=new Sprite[6][];
        public static Sprite[] Page(int skin,int weapon)
        {
            skin=Mathf.Clamp(skin,0,1);weapon=Mathf.Clamp(weapon,0,2);int key=skin*3+weapon;
            if(Pages[key]==null)
            {
                var sprites=Resources.LoadAll<Sprite>("Darklight/UserWholeSprites/"+Names[skin,weapon]);
                Array.Sort(sprites,(a,b)=>string.CompareOrdinal(a.name,b.name));Pages[key]=sprites;
            }
            return Pages[key];
        }
        static readonly System.Collections.Generic.Dictionary<Texture2D,Texture2D> Masks=new();
        public static Texture2D Mask(Sprite sprite)
        {
            if(sprite==null)return Texture2D.blackTexture;
            if(!Masks.TryGetValue(sprite.texture,out var mask))
            {mask=Resources.Load<Texture2D>("Darklight/UserWholeMasks/"+sprite.texture.name+"_mask");Masks[sprite.texture]=mask;}
            return mask??Texture2D.blackTexture;
        }
        static Sprite[] standaloneAuras;
        public static Sprite[] FormAuras
        {
            get
            {
                if(standaloneAuras==null)
                {
                    standaloneAuras=Resources.LoadAll<Sprite>("Darklight/AnimationCatalog/OriginalFormAuras");
                    Array.Sort(standaloneAuras,(a,b)=>int.Parse(a.name.Substring(a.name.LastIndexOf('_')+1)).CompareTo(int.Parse(b.name.Substring(b.name.LastIndexOf('_')+1))));
                }
                return standaloneAuras;
            }
        }
        public static int Pose(CombatModel model,Vector2 movement)
        {
            if(model.State==CombatState.Dead||model.State==CombatState.Hurt)return 7;
            if(model.State==CombatState.Parry)return 4;
            if(model.State==CombatState.Dodge)return model.StateTime<.09f?5:6;
            if(model.State==CombatState.Charging)return 2;
            if(model.State==CombatState.Attack||model.ActiveThisTick)
            {
                if(model.StateTime<model.Attack.startup)return 2;
                if(model.StateTime<model.Attack.startup+model.Attack.active+model.Attack.recovery*.5f)return 3;
                return 7;
            }
            return movement.sqrMagnitude>.01f?(int)(model.Clock*8f)%2:7;
        }
        public static Sprite Resolve(int skin,int age,int weapon,CombatModel model,Vector2 movement)
        {
            // Preserve narrative aging until complete whole-character age sheets exist.
            if(age!=0)return null;
            var page=Page(skin,weapon);return page.Length==8?page[Pose(model,movement)]:null;
        }
    }
}
