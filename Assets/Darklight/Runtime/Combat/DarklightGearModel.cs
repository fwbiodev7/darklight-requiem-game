using UnityEngine;
namespace Darklight
{
    // Combat-clock state, no rendering/audio/input-device or save dependency.
    public sealed class DarklightGearModel
    {
        public float Rotation{get;private set;}
        public int Level=>Rotation>=4?3:Rotation>=2?2:1;
        public float Speed=>Level==3?1.24f:Level==2?1.12f:1;
        public float Damage=>Level==3?1.2f:Level==2?1.09f:1;
        public float Posture=>Level==3?1.55f:Level==2?1.25f:1;
        public int PreciseCommands{get;private set;}
        public int Mistakes{get;private set;}
        public bool WindowOpen{get;private set;}
        float clock,lastSuccess,lastCommand=-10;int acceptedSerial=-1;bool wasInterrupted;
        public static bool TimingWindow(AttackDefinition a,float time,int form)
        {
            if(a==null)return false;
            float tolerance=form>=2?.045f:0;
            return time>=a.startup+a.active-.045f-tolerance&&time<=a.startup+a.active+Mathf.Min(.14f,a.recovery*.35f)+tolerance;
        }
        public bool Tick(float dt,bool equipped,int serial,AttackDefinition attack,float time,bool connected,bool attacking,bool command,bool interrupted,int form)
        {
            if(dt<=0)return false;
            clock+=dt;WindowOpen=equipped&&attacking&&connected&&TimingWindow(attack,time,form);
            if(interrupted&&!wasInterrupted)Rotation=Mathf.Max(0,Rotation-1.4f);
            wasInterrupted=interrupted;
            if(!equipped)Rotation=Mathf.Max(0,Rotation-dt*1.4f);
            else if(clock-lastSuccess>2.8f&&!attacking)Rotation=Mathf.Max(0,Rotation-dt*.75f);
            if(!equipped||!command||!attacking)return false;
            bool spaced=clock-lastCommand>=.14f;lastCommand=clock;
            if(WindowOpen&&spaced&&serial!=acceptedSerial)
            {acceptedSerial=serial;Rotation=Mathf.Min(5,Rotation+1);lastSuccess=clock;PreciseCommands++;return true;}
            Rotation=Mathf.Max(0,Rotation-.45f);Mistakes++;return false;
        }
        public bool TryConsumeOverload(){if(Level!=3)return false;Rotation=2;WindowOpen=false;return true;}
        public void Crash(){Rotation=Mathf.Max(0,Rotation-1);WindowOpen=false;}
        public void Reset(){Rotation=0;clock=0;lastSuccess=0;lastCommand=-10;acceptedSerial=-1;wasInterrupted=false;WindowOpen=false;PreciseCommands=Mistakes=0;}
    }
}
