using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    // Every frame is a newly drawn complete character, never a recomposed head/body.
    public static class DarklightWholeCharacterBank
    {
        static readonly Dictionary<string,Sprite[]> sheets=new();
        static readonly Dictionary<Texture2D,Texture2D> masks=new();
        static DarklightWholeCharacterCatalog catalog;
        public static Sprite[] Sheet(string name)
        {
            if(!sheets.TryGetValue(name,out var frames))
            {
                frames=Resources.LoadAll<Sprite>("Darklight/WholeCharacterAnimations/"+name);
                Array.Sort(frames,(a,b)=>int.Parse(a.name[(a.name.LastIndexOf('_')+1)..]).CompareTo(int.Parse(b.name[(b.name.LastIndexOf('_')+1)..])));
                sheets[name]=frames;
            }
            return frames;
        }
        public static Sprite Resolve(int skin,int age,int weapon,int direction,CombatModel model,Vector2 movement)
        {
            if(age<1||age>4||weapon!=0)return null;
            string name=null;int phase=0;
            if(skin==0&&age<=3&&model.State==CombatState.Neutral&&!model.ActiveThisTick&&movement.sqrMagnitude>.01f)
            {name="RequiemWalkStage"+age;phase=(int)(model.Clock*10f)%4;}
            else if(skin==1&&(model.State==CombatState.Attack||model.ActiveThisTick))
            {
                name="RockAttackStage"+age;
                phase=model.ActiveThisTick?1:model.StateTime<model.Attack.startup?0:model.StateTime<model.Attack.startup+model.Attack.active?1:model.StateTime<model.Attack.startup+model.Attack.active+model.Attack.recovery*.5f?2:3;
            }
            if(name==null)return null;var frames=Sheet(name);
            return frames.Length==32?frames[Mathf.Clamp(direction,0,7)*4+phase]:null;
        }
        public static Texture2D Mask(Sprite sprite)
        {
            if(sprite==null)return null;
            if(masks.TryGetValue(sprite.texture,out var mask))return mask;
            if(sprite.name.StartsWith("Whole_"))mask=Resources.Load<Texture2D>("Darklight/WholeCharacterAnimations/"+sprite.texture.name+"_Palette");
            else
            {
                catalog??=Resources.Load<DarklightWholeCharacterCatalog>("Darklight/WholeCharacterCatalog");
                if(catalog?.palettes!=null)foreach(var palette in catalog.palettes)if(palette.source==sprite.texture)
                {mask=Resources.Load<Texture2D>("Darklight/WholeCharacterPalettes/"+palette.mask);break;}
            }
            masks[sprite.texture]=mask;return mask;
        }
    }
}
