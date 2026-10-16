using UnityEngine;
namespace Darklight
{
    // Whole-character action sheets, resolved by gameplay timing and facing.
    public static partial class DarklightAbyssSpriteBank
    {
        static readonly string[,] pages={
            {"Join","AttackSix0","AttackSix1","Dismount","Form0Slam","Form0Finisher"},
            {"Form1Join","Form1AttackSix0","Form1AttackSix1","Form1Dismount","Form1Slam","Form1Finisher"},
            {"Form2Join","Form2AttackSix0","Form2AttackSix1","Form2Dismount","Form2Slam","Form2Finisher"},
            {"Form3Join","Form3AttackSix0","Form3AttackSix1","Form3Dismount","Form3Slam","Form3Finisher"}};
        static readonly Sprite[][] cache=new Sprite[24][];
        public static Sprite Icon=>Frame(0,0,0,0);
        static Sprite[] Page(int form,int page)
        {
            form=Mathf.Clamp(form,0,3);int slot=form*6+page;if(cache[slot]!=null)return cache[slot];
            var result=new Sprite[8];string name=pages[form,page];
            foreach(var sprite in Resources.LoadAll<Sprite>("Darklight/AbyssPrototype/"+name))
            {
                string[] parts=sprite.name.Split('_');int count=parts.Length;
                if(count>=4&&int.TryParse(parts[count-2],out int row)&&int.TryParse(parts[count-1],out int phase)&&row>=0&&row<2&&phase>=0&&phase<4)result[row*4+phase]=sprite;
            }
            cache[slot]=result;return result;
        }
        static Sprite Frame(int form,int page,int row,int phase)=>Page(form,page)[row*4+phase];
        public static void Preload(int form){for(int page=0;page<6;page++)Page(form,page);}
        public static void ClearCache(){for(int i=0;i<cache.Length;i++)cache[i]=null;}
        public static bool HasCompleteForm(int form)
        {
            for(int row=0;row<2;row++)
            {
                for(int phase=0;phase<4;phase++)if(Frame(form,0,row,phase)==null)return false;
                for(int phase=0;phase<3;phase++)if(Frame(form,1,row,phase)==null||Frame(form,2,row,phase)==null)return false;
                if(Frame(form,3,row,1)==null||Frame(form,3,row,2)==null)return false;
                for(int phase=0;phase<2;phase++)if(Frame(form,4,row,phase)==null||Frame(form,5,row,phase)==null)return false;
            }
            return true;
        }
        public static Sprite Resolve(DarklightAbyssRider rider,CombatModel model)
        {
            int row=model.Facing.x<-.2f?1:0,form=Mathf.Clamp(DarklightCampaign.Instance?.PresentedForm??0,0,3);
            var directional=ResolveDirectional(rider,model,form);if(directional!=null)return directional;
            if(rider.Slamming)
            {
                var attack=model.Attack;
                if(rider.SlamFromMounted&&model.StateTime<.18f)return Frame(form,0,row,3);
                if(!rider.SlamFromMounted&&model.StateTime<.1f)return Frame(form,0,row,0);
                if(!rider.SlamFromMounted&&model.StateTime<.24f)return Frame(form,0,row,1);
                if(model.StateTime<attack.startup)return Frame(form,4,row,0);
                if(model.StateTime<attack.startup+attack.active+.16f)return Frame(form,4,row,1);
                if(model.StateTime<attack.startup+attack.active+.3f)return Frame(form,0,row,1);
                return Frame(form,0,row,0);
            }
            if(rider.Sustaining&&model.StateTime>=model.Attack.startup&&model.StateTime<model.Attack.startup+model.Attack.active)
            {
                int phase=2+Mathf.FloorToInt((model.StateTime-model.Attack.startup)*14)%2;
                return Frame(form,row+1,phase/3,phase%3);
            }
            if(rider.Dashing)
            {
                var attack=model.Attack;
                if((rider.Drifting||rider.Finishing)&&model.StateTime>=attack.startup*.7f&&model.StateTime<attack.startup+attack.active)return Frame(form,5,row,0);
                if(model.StateTime<attack.startup)return Frame(form,0,row,Mathf.Min(3,Mathf.FloorToInt(model.StateTime/attack.startup*4)));
                if(model.StateTime<attack.startup+attack.active)return Frame(form,0,row,3);
                int phase=Mathf.FloorToInt((model.StateTime-attack.startup-attack.active)/.34f*4);
                if(phase<=0)return Frame(form,0,row,3); // Correct inline-wheel mounted pose.
                if(rider.Finishing&&phase==1)return Frame(form,5,row,1);
                return phase>=3?Frame(form,0,row,0):Frame(form,3,row,phase);
            }
            if(model.State==CombatState.Attack)
            {
                var attack=model.Attack;int phase;
                if(model.StateTime<attack.startup)phase=model.StateTime<attack.startup*.45f?0:1;
                else if(model.StateTime<attack.startup+attack.active)phase=model.StateTime<attack.startup+attack.active*.5f?2:3;
                else phase=model.StateTime<attack.startup+attack.active+attack.recovery*.55f?4:5;
                return Frame(form,row+1,phase/3,phase%3);
            }
            return Frame(form,0,row,0);
        }
    }
}
