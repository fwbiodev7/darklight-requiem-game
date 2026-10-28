using System;
using UnityEngine;

namespace Darklight
{
    [Serializable]public sealed class DarklightStageResult
    {
        public int chapter,kills,dodges,parries,hits,combos,bestCombo,bestRank,finalRank,specials;
        public float damageTaken,seconds;
        public bool completed;
        public void Normalize(int id)
        {
            chapter=id;kills=Mathf.Max(0,kills);dodges=Mathf.Max(0,dodges);parries=Mathf.Max(0,parries);
            hits=Mathf.Max(0,hits);combos=Mathf.Clamp(combos,0,hits/3);bestCombo=Mathf.Clamp(bestCombo,0,hits);
            bestRank=Mathf.Clamp(bestRank,0,6);finalRank=Mathf.Clamp(finalRank,0,6);specials=Mathf.Clamp(specials,0,hits);
            damageTaken=DarklightBlood.Finite(damageTaken,0,100000000);seconds=DarklightBlood.Finite(seconds,0,100000000);
        }
    }
    public sealed class DarklightStageStatistics
    {
        public DarklightStageResult Result{get;private set;}
        int lastDodge=int.MinValue;
        public void Begin(DarklightStageResult result){Result=result;lastDodge=int.MinValue;}
        bool Recording=>Result!=null&&!Result.completed;
        public void Tick(float dt){if(Recording)Result.seconds+=Mathf.Max(0,dt);}
        public void Kill(){if(Recording)Result.kills++;}
        public void Damage(float actualHealthLost){if(Recording&&actualHealthLost>0&&!float.IsNaN(actualHealthLost)&&!float.IsInfinity(actualHealthLost))Result.damageTaken+=actualHealthLost;}
        public bool Dodge(int maneuver)
        {
            if(!Recording||maneuver==lastDodge)return false;lastDodge=maneuver;Result.dodges++;return true;
        }
        public void Parry(){if(Recording)Result.parries++;}
        public void Observe(DarklightStyleMeter style){if(Recording)Result.bestRank=Mathf.Max(Result.bestRank,style.PeakRankIndex);}
        public void Hit(DarklightStyleMeter style,bool special)
        {
            if(!Recording)return;Result.hits++;if(style.ComboHits==3)Result.combos++;
            Result.bestCombo=Mathf.Max(Result.bestCombo,style.ComboHits);Result.bestRank=Mathf.Max(Result.bestRank,style.RankIndex);
            if(special)Result.specials++;
        }
        public void Finish(DarklightStyleMeter style)
        {
            if(!Recording)return;Result.finalRank=style.RankIndex;Result.bestRank=Mathf.Max(Result.bestRank,style.PeakRankIndex);
            Result.completed=true;
        }
    }
    public sealed partial class DarklightProgress
    {
        public DarklightStageResult[] stageResults;
        public void NormalizeStageResults()
        {
            if(stageResults==null)stageResults=new DarklightStageResult[ChapterCapacity];
            else if(stageResults.Length!=ChapterCapacity)Array.Resize(ref stageResults,ChapterCapacity);
            for(int i=0;i<stageResults.Length;i++)stageResults[i]?.Normalize(i);
        }
        public DarklightStageResult StageResult(int chapter)
        {
            NormalizeStageResults();chapter=Mathf.Clamp(chapter,0,ChapterCapacity-1);
            return stageResults[chapter]??=new DarklightStageResult{chapter=chapter};
        }
    }
}
