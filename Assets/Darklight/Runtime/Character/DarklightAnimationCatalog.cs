using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    /// <summary>Exact skin/age/weapon/action lookup. Missing sheets remain visible in the coverage audit.</summary>
    public sealed class DarklightAnimationCatalog
    {
        readonly Dictionary<string,Sprite[]> sheets=new();
        public Sprite Light(int skin,int age,int weapon,int direction,CombatModel model)
        {
            // Only visually reviewed packages are enabled; the rejected directional draft is never selected.
            if(skin!=1||age!=4||weapon!=0||model.State!=CombatState.Attack&&!model.ActiveThisTick||model.Attack.kind!=StrikeKind.Light)return null;
            string key=(skin==1?"Rock":"Requiem")+"KatanaLightStage"+age;
            if(!sheets.TryGetValue(key,out var frames))
            {
                frames=Resources.LoadAll<Sprite>("Darklight/AnimationCatalog/"+key);
                Array.Sort(frames,(a,b)=>int.Parse(a.name[(a.name.LastIndexOf('_')+1)..]).CompareTo(int.Parse(b.name[(b.name.LastIndexOf('_')+1)..])));sheets[key]=frames;
            }
            if(frames.Length!=32)return null;
            int frame=model.StateTime<model.Attack.startup?0:model.ActiveThisTick?1:model.StateTime<model.Attack.startup+model.Attack.active+model.Attack.recovery*.5f?2:3;
            return frames[Mathf.Clamp(direction,0,7)*4+frame];
        }
    }
}
