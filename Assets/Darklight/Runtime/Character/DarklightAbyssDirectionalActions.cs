using UnityEngine;
namespace Darklight
{
    public static partial class DarklightAbyssSpriteBank
    {
        static readonly Sprite[][] directionCache=new Sprite[12][];
        static readonly string[] actionViews={"W","NW","N"};
        public static bool Mirror(CombatModel model)=>DarklightCharacterView.DirectionIndex(model.Facing)>4;
        static Sprite DirectionalFrame(int form,int view,int frame)
        {
            int slot=form*3+view-2;
            if(directionCache[slot]==null)
            {
                var sprites=new Sprite[12];
                foreach(var sprite in Resources.LoadAll<Sprite>("Darklight/AbyssPrototype/ActionF"+form+actionViews[view-2]))
                {var parts=sprite.name.Split('_');if(int.TryParse(parts[parts.Length-1],out int index)&&index>=0&&index<12)sprites[index]=sprite;}
                directionCache[slot]=sprites;
            }
            return directionCache[slot][Mathf.Clamp(frame,0,11)];
        }
        static Sprite ResolveDirectional(DarklightAbyssRider rider,CombatModel model,int form)
        {
            int direction=DarklightCharacterView.DirectionIndex(model.Facing),view=direction>4?8-direction:direction;
            if(view<2)return null;
            if(model.State==CombatState.Parry||model.State==CombatState.Hurt)return DirectionalFrame(form,view,11);
            if(model.State==CombatState.Dodge)return DirectionalFrame(form,view,8);
            int frame=0;
            if(rider.Slamming)
            {
                var attack=model.Attack;float time=model.StateTime;
                frame=rider.SlamFromMounted&&time<.18f?7:!rider.SlamFromMounted&&time<.1f?0:!rider.SlamFromMounted&&time<.24f?5:time<attack.startup?9:time<attack.startup+attack.active+.16f?10:time<attack.startup+attack.active+.3f?4:0;
            }
            else if(rider.Dashing)
            {
                var attack=model.Attack;float time=model.StateTime;
                if(time<attack.startup){float t=time/attack.startup;frame=t<.22f?0:t<.48f?5:t<.76f?6:7;}
                else if(time<attack.startup+attack.active)frame=7;
                else{float t=(time-attack.startup-attack.active)/.34f;frame=t<.15f?7:t<.65f?8:t<.9f?4:0;}
            }
            else if(model.State==CombatState.Attack)
            {
                var attack=model.Attack;float time=model.StateTime;
                if(time<attack.startup)frame=time<attack.startup*.45f?0:1;
                else if(time<attack.startup+attack.active)frame=rider.Sustaining?2+Mathf.FloorToInt((time-attack.startup)*14)%2:time<attack.startup+attack.active*.5f?2:3;
                else frame=time<attack.startup+attack.active+attack.recovery*.55f?4:0;
            }
            return DirectionalFrame(form,view,frame);
        }
    }
}
