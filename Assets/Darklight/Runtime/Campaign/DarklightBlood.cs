using System;
using UnityEngine;

namespace Darklight
{
    public enum VampireAbility { Claws, ShadowStep, Thirst, Embrace, Frenzy, Judgment }
    public static class DarklightVampireSkills
    {
        public static readonly string[] Names={"GARRAS CARMESINS","PASSO SOMBRIO","SEDE DE SANGUE","ABRACO DO VEU","FRENESI CARMESIM","JULGAMENTO DE SANGUE"};
        public static readonly float[] Costs={15,18,22,30,40,65};
        public static readonly float[] Cooldowns={3,5,14,9,20,24};
        public static readonly float[] Durations={0,0,7,0,6,0};
        public static readonly int[] Mastery={1,1,1,2,2,3};
        public static readonly string[] Descriptions={"Tres cortes curtos; inicia combos.","Avanco seguro; invulneravel por 0,16s.","Contra-ataques e finalizadores recuperam vida.","Atrai alvos comuns; chefes resistem ao controle.","Ataque +20%; consome sangue durante 6s.","Corte de Requiem; alto custo e recuperacao longa."};
        public static bool Unlocked(DarklightProgress state,int skill)=>skill>=0&&skill<6&&state.vampireMastery>=Mastery[skill]&&(state.vampireSkills&(1<<skill))!=0;
    }

    /// <summary>Combat-clock resource. One reward per attack serial, independent of targets/pulses.</summary>
    public sealed class DarklightBlood
    {
        public float Amount {get;private set;}
        public float ThirstRemaining {get;private set;}
        public float FrenzyRemaining {get;private set;}
        public readonly float[] Recovery=new float[6];
        float clock,parryReady,healingBudget,healingWindow;
        int lastSerial=-1,lastKind=-1,repetitions;
        public void Set(float value)=>Amount=Finite(value,0,100);
        public void Gain(float value){if(value>0&&!float.IsInfinity(value))Set(Amount+value);}
        public bool RewardStrike(StrikeKind kind,int serial)
        {
            if(serial<0||serial==lastSerial)return false;
            lastSerial=serial;
            bool varied=(int)kind!=lastKind;repetitions=varied?0:repetitions+1;lastKind=(int)kind;
            Gain(kind==StrikeKind.Counter?8:varied?5:repetitions<3?1:.25f);return true;
        }
        public bool RewardParry()
        {
            if(clock<parryReady)return false;parryReady=clock+.3f;Gain(8);return true;
        }
        public bool Activate(int skill,DarklightProgress state)
        {
            if(!DarklightVampireSkills.Unlocked(state,skill)||Recovery[skill]>0||Amount<DarklightVampireSkills.Costs[skill])return false;
            Set(Amount-DarklightVampireSkills.Costs[skill]);Recovery[skill]=DarklightVampireSkills.Cooldowns[skill];
            if(skill==(int)VampireAbility.Thirst)ThirstRemaining=7;
            if(skill==(int)VampireAbility.Frenzy)FrenzyRemaining=6;
            return true;
        }
        public void Tick(float delta)
        {
            if(delta<=0||float.IsNaN(delta)||float.IsInfinity(delta))return;
            clock+=delta;for(int i=0;i<6;i++)Recovery[i]=Mathf.Max(0,Recovery[i]-delta);
            ThirstRemaining=Mathf.Max(0,ThirstRemaining-delta);
            if(FrenzyRemaining>0){float used=Mathf.Min(delta,FrenzyRemaining);Set(Amount-used*3);FrenzyRemaining=Amount<=0?0:Mathf.Max(0,FrenzyRemaining-delta);}
            healingWindow+=delta;
            if(healingWindow>=1){healingWindow%=1;healingBudget=0;}
        }
        // Love + blood healing share a hard 3% max-health/second ceiling.
        public float AllowHealing(float requested,float maxHealth)
        {
            if(requested<=0||maxHealth<=0||float.IsNaN(requested)||float.IsInfinity(requested))return 0;
            float result=Mathf.Min(requested,Mathf.Max(0,maxHealth*.03f-healingBudget));healingBudget+=result;return result;
        }
        public void StopTemporary(){ThirstRemaining=FrenzyRemaining=0;}
        public void Restore(DarklightProgress state)
        {
            Set(state.blood);clock=parryReady=healingBudget=healingWindow=0;lastSerial=lastKind=-1;repetitions=0;StopTemporary();
            for(int i=0;i<6;i++)Recovery[i]=Finite(state.vampireRecoveries!=null&&i<state.vampireRecoveries.Length?state.vampireRecoveries[i]:0,0,120);
        }
        public void Store(DarklightProgress state){state.blood=Amount;state.vampireRecoveries=(float[])Recovery.Clone();}
        public static float Finite(float value,float min,float max)=>float.IsNaN(value)||float.IsInfinity(value)?min:Mathf.Clamp(value,min,max);
    }
}
