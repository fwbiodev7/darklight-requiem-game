using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public enum DarklightDifficulty { Normal,Hard,Extreme }
    public readonly struct DarklightDifficultyProfile
    {
        public readonly float reaction,windup,cooldown,flank,speed;
        public readonly int meleeSlots,projectileSlots,comboLength;
        public DarklightDifficultyProfile(float reaction,float windup,float cooldown,float flank,float speed,int melee,int ranged,int combo)
        {this.reaction=reaction;this.windup=windup;this.cooldown=cooldown;this.flank=flank;this.speed=speed;meleeSlots=melee;projectileSlots=ranged;comboLength=combo;}
        public static DarklightDifficultyProfile For(int mode)=>mode==2?new(.16f,.84f,.65f,.75f,1.12f,3,2,3):mode==1?new(.23f,.92f,.8f,.55f,1.06f,2,1,2):new(.33f,1,1,.3f,1,1,1,1);
        public static bool ExtremeUnlocked(DarklightProgress p)=>p!=null&&(p.completed||p.legacyFinaleCompleted||p.extremeUnlocked);
    }
    // A bounded move history rewards meaningful variation, never raw button count.
    public sealed class DarklightStyleMeter
    {
        readonly int[] history=new int[12];int cursor,count,lastWeapon=-1;float idle;
        public float Points {get;private set;}
        static readonly string[] ranks={"D","C","B","A","S","SS","SSS"};
        public string Rank=>ranks[Mathf.Clamp((int)(Points/65),0,6)];
        public int UniqueRecent {get;private set;}
        public float Hit(int move,int weapon,bool aerial=false,bool finisher=false)
        {
            int repeated=0,unique=0;bool exists=false;
            for(int i=0;i<count;i++){if(history[i]==move){repeated++;exists=true;}bool first=true;for(int j=0;j<i;j++)if(history[j]==history[i])first=false;if(first)unique++;}
            float gain=repeated==0?23:repeated==1?11:repeated==2?4:0;
            if(lastWeapon>=0&&lastWeapon!=weapon&&repeated<2)gain+=10;
            if(aerial&&repeated<2)gain+=5;if(finisher&&repeated<2)gain+=12;
            history[cursor]=move;cursor=(cursor+1)%history.Length;count=Mathf.Min(count+1,history.Length);if(!exists)unique++;UniqueRecent=unique;
            // Two-move loops can build competence, but top ranks require a wider repertoire.
            float cap=unique<3?185:unique<5?320:450;
            Points=Mathf.Min(cap,Points+gain);lastWeapon=weapon;idle=0;return gain;
        }
        public void Defense(bool perfect){Hit(perfect?-100:-101,lastWeapon);idle=0;}
        public void Damage(){Points=Mathf.Max(0,Points-65);}
        public void Tick(float dt){idle+=Mathf.Max(0,dt);if(idle>2)Points=Mathf.Max(0,Points-dt*9);}
        public void Reset(){Points=0;cursor=count=0;lastWeapon=-1;UniqueRecent=0;idle=0;}
    }
    public static class DarklightHitGeometry
    {
        public static bool Contains(AttackDefinition attack,Vector2 origin,Vector2 facing,Vector2 target,float extra,float progress,float previousProgress=0)
        {
            var delta=target-origin;float distance=delta.magnitude;
            if(distance>attack.range+extra||distance<attack.innerRange)return false;
            if(attack.hitShape==HitShape.Thrust)
            {float forward=Vector2.Dot(delta,facing.normalized);float side=Mathf.Abs(delta.x*facing.normalized.y-delta.y*facing.normalized.x);return forward>=0&&side<=attack.width*.5f;}
            float angle=attack.angle;
            if(attack.hitShape==HitShape.Sweep)
            {
                float signed=Vector2.SignedAngle(facing,delta),padding=Mathf.Atan2(attack.width*.5f,Mathf.Max(.1f,distance))*Mathf.Rad2Deg;
                float start=-angle*.5f+angle*previousProgress,end=-angle*.5f+angle*progress;
                return signed>=start-padding&&signed<=end+padding;
            }
            // A sweep covers the traversed arc; animation and contact share this phase.
            return Vector2.Angle(facing,delta)<=angle*.5f;
        }
    }
    // Bound simultaneous commitments; defenders and flankers continue to move.
    public sealed class DarklightThreatDirector
    {
        struct Lease{public DarklightEnemy owner;public float until;public bool ranged;}
        readonly List<Lease> leases=new();
        public bool Request(DarklightEnemy enemy,float now,int mode,bool ranged,bool visible)
        {
            for(int i=leases.Count-1;i>=0;i--)if(leases[i].owner==null||leases[i].owner.Health.IsDead||leases[i].until<=now)leases.RemoveAt(i);
            if(!visible)return false;
            int count=0;foreach(var lease in leases){if(lease.owner==enemy)return false;if(lease.ranged==ranged)count++;}
            var p=DarklightDifficultyProfile.For(mode);int limit=ranged?p.projectileSlots:p.meleeSlots;
            if(count>=limit)return false;
            leases.Add(new Lease{owner=enemy,until=now+1.15f+(p.comboLength-1+(enemy.elite?1:0)+(enemy.Boss?1:0))*.8f,ranged=ranged});return true;
        }
        public void Clear()=>leases.Clear();
    }
}
