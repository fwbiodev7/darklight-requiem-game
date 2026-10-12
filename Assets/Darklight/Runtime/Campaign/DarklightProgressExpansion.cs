using System;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightProgress
    {
        public const int CurrentVersion=2,ChapterCapacity=10;
        public int vampireMastery,vampireSkills,vampireSelected,awakeningState,progressionNodes;
        public bool legacyFinaleCompleted;
        public int expansionFlags,bossPhase=1,missionWave,missionWeaponHits;
        public bool SecretEndingAvailable {get {if((expansionFlags&15)!=15||relics==null||relics.Length<ChapterCapacity)return false;for(int i=0;i<ChapterCapacity;i++)if(!relics[i])return false;return true;}}
        public float blood;public float[] vampireRecoveries=new float[6];
        public long narrativeMilestones;
        public static readonly string[] MasteryNames={"SANGUE LATENTE","SANGUE DESPERTO","SANGUE DOMINADO","HERDEIRO DO ECLIPSE"};
        public bool LearnVampireSkill(int skill)
        {
            if(skill<0||skill>=6||points<1||vampireMastery<DarklightVampireSkills.Mastery[skill]||(vampireSkills&(1<<skill))!=0)return false;
            points--;vampireSkills|=1<<skill;vampireSelected=skill;return true;
        }
        public void CompleteMilestone(int milestone,int masteryLevel)
        {
            if(milestone<0||milestone>=63)return;
            narrativeMilestones|=1L<<milestone;vampireMastery=Mathf.Max(vampireMastery,Mathf.Clamp(masteryLevel,0,3));
        }
        static DarklightProgress DecodeExpansion(string json)
        {
            if(string.IsNullOrWhiteSpace(json)||!json.Contains("\"version\"")||!json.Contains("\"chapter\"")||!json.Contains("\"level\""))return null;
            DarklightProgress s;try{s=JsonUtility.FromJson<DarklightProgress>(json);}catch(ArgumentException){return null;}
            if(s==null||s.version<1||s.version>CurrentVersion||s.chapter<0||s.chapter>=DarklightStory.Chapters.Length||s.level<1||s.level>1000)return null;
            bool legacy=s.version==1;
            if(legacy)
            {
                s.legacyFinaleCompleted=s.completed;
                s.formsUnlocked=Mathf.Max(s.formsUnlocked,s.chapter>=5?3:s.chapter>=4?2:s.chapter>=1?1:0);
                s.vampireMastery=s.chapter>=4||s.formsUnlocked>=2?2:s.chapter>0||s.formsUnlocked>0?1:0;
                if(s.vampireMastery>0){s.awakeningState=2;s.vampireSkills|=1;s.narrativeMilestones|=1L;}
            }
            s.version=CurrentVersion;
            s.expansionFlags&=15;s.missionWeaponHits&=7;s.missionWave=Mathf.Clamp(s.missionWave,0,2);s.bossPhase=Mathf.Clamp(s.bossPhase,1,3);
            s.weaponUnlocks=(s.weaponUnlocks&7)|1;s.equippedWeapon=Mathf.Clamp(s.equippedWeapon,0,2);
            if((s.weaponUnlocks&(1<<s.equippedWeapon))==0)s.equippedWeapon=0;
            s.appearanceStage=Mathf.Clamp(s.appearanceStage,0,4);s.narrativeDays=Mathf.Clamp(s.narrativeDays,0,36500);s.appearanceChapter=Mathf.Clamp(s.appearanceChapter,0,ChapterCapacity-1);
            s.checkpoint=Mathf.Clamp(s.checkpoint,0,4);s.potions=Mathf.Clamp(s.potions,0,99);
            s.attack=Mathf.Clamp(s.attack,0,5);s.vitality=Mathf.Clamp(s.vitality,0,5);s.mastery=Mathf.Clamp(s.mastery,0,5);s.points=Mathf.Max(0,s.points);s.xp=Mathf.Max(0,s.xp);
            s.formsUnlocked=Mathf.Clamp(s.formsUnlocked,0,3);s.preferredForm=Mathf.Clamp(s.preferredForm,0,s.FormUnlocked);
            var relics=new bool[ChapterCapacity];if(s.relics!=null)Array.Copy(s.relics,relics,Mathf.Min(s.relics.Length,ChapterCapacity));s.relics=relics;
            s.resonanceEnergy=json.Contains("\"resonanceEnergy\"")?DarklightBlood.Finite(s.resonanceEnergy,0,100):100;
            s.resonanceSelected=Mathf.Clamp(s.resonanceSelected,0,2);s.resonanceCooldown=DarklightBlood.Finite(s.resonanceCooldown,0,120);
            var resonance=new float[3];for(int i=0;i<3;i++)resonance[i]=DarklightBlood.Finite(s.resonanceRecoveries!=null&&i<s.resonanceRecoveries.Length?s.resonanceRecoveries[i]:s.resonanceCooldown,0,120);s.resonanceRecoveries=resonance;
            s.blood=DarklightBlood.Finite(s.blood,0,100);s.vampireMastery=Mathf.Clamp(s.vampireMastery,0,3);s.vampireSkills&=63;s.vampireSelected=Mathf.Clamp(s.vampireSelected,0,5);s.awakeningState=Mathf.Clamp(s.awakeningState,0,2);
            var recoveries=new float[6];for(int i=0;i<6;i++)recoveries[i]=DarklightBlood.Finite(s.vampireRecoveries!=null&&i<s.vampireRecoveries.Length?s.vampireRecoveries[i]:0,0,120);s.vampireRecoveries=recoveries;
            return s;
        }
    }
}
