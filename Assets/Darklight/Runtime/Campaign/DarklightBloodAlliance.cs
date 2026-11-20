using System;

namespace Darklight
{
    [Flags]
    public enum DarklightAllianceFlags
    {
        None=0, LadyAvailable=1, NeroDefeated=2, NeroSurvived=4, NeroRevenge=8,
        ThreatRecognized=16, TruceStarted=32, TruceEnded=64, RivalryPending=128
    }

    public sealed partial class DarklightProgress
    {
        // Separate schema keeps campaign version/indexes compatible with existing saves.
        public int allianceVersion;
        public DarklightAllianceFlags allianceFlags=DarklightAllianceFlags.LadyAvailable;
        public bool LadyAvailable=>HasAllianceFlag(DarklightAllianceFlags.LadyAvailable);
        public bool NeroDefeated=>HasAllianceFlag(DarklightAllianceFlags.NeroDefeated);
        public bool NeroSurvived=>HasAllianceFlag(DarklightAllianceFlags.NeroSurvived);
        public bool NeroRevenge=>HasAllianceFlag(DarklightAllianceFlags.NeroRevenge);
        public bool ThreatRecognized=>HasAllianceFlag(DarklightAllianceFlags.ThreatRecognized);
        public bool TruceStarted=>HasAllianceFlag(DarklightAllianceFlags.TruceStarted);
        public bool TruceEnded=>HasAllianceFlag(DarklightAllianceFlags.TruceEnded);
        public bool RivalryPending=>HasAllianceFlag(DarklightAllianceFlags.RivalryPending);
        public bool HasAllianceFlag(DarklightAllianceFlags flag)=>(allianceFlags&flag)==flag;
        public void SetLadyAvailable(bool available)
        {
            allianceVersion=1;
            if(available)allianceFlags|=DarklightAllianceFlags.LadyAvailable;
            else allianceFlags&=~DarklightAllianceFlags.LadyAvailable;
        }
        public void RecordNeroSurvival()
        {
            allianceVersion=1;
            allianceFlags|=DarklightAllianceFlags.NeroDefeated|DarklightAllianceFlags.NeroSurvived|
                DarklightAllianceFlags.NeroRevenge|DarklightAllianceFlags.RivalryPending;
        }
        internal void NormalizeBloodAlliance()
        {
            allianceFlags&=(DarklightAllianceFlags)255;
            if(allianceVersion==0)
            {
                allianceFlags|=DarklightAllianceFlags.LadyAvailable;
                // Chapter 8 purification and chapter 9 already imply Nero survived.
                if(chapter>DarklightCampaignRoute.NeroChapter||
                    chapter==DarklightCampaignRoute.NeroChapter&&checkpoint>=4||
                    (expansionFlags&DarklightCampaign.KaelPurified)!=0)
                    RecordNeroSurvival();
                // Completed legacy finales cannot replay the truce or respawn Nero.
                if(chapter==DarklightCampaignRoute.FinalChapter&&(luciusFate!=0||completed))
                    allianceFlags|=DarklightAllianceFlags.ThreatRecognized|DarklightAllianceFlags.TruceStarted|
                        DarklightAllianceFlags.TruceEnded|DarklightAllianceFlags.RivalryPending;
            }
            allianceVersion=1;
        }
    }

    public sealed partial class DarklightCampaign
    {
        public bool NeroTruceActive=>Progress!=null&&Progress.chapter==DarklightCampaignRoute.FinalChapter&&
            Progress.NeroSurvived&&Progress.ThreatRecognized&&Progress.TruceStarted&&!Progress.TruceEnded&&
            Progress.luciusFate==0&&!Progress.completed;

        // True means this narrative gate consumed the interaction; continuation runs after dialogue.
        public bool TryStartNeroTruce(Action continuation=null)
        {
            if(Training||ExploringNewRegion||Progress==null||Progress.chapter!=DarklightCampaignRoute.FinalChapter||
                !Progress.NeroSurvived||Progress.TruceStarted||Progress.TruceEnded||Progress.luciusFate!=0||Progress.completed)return false;
            Speak(new[]{
                "NERO|Não confunda isso com perdão, Fabius.",
                "FABIUS|Então por que está aqui?",
                "NERO|Eu vi o que Lucius alimenta no Coração do Eclipse. Se ele romper a fronteira, não haverá nada pelo que lutar.",
                "NERO|Primeiro acabamos com ele. Depois... será só você e eu.",
                "FABIUS|Até Lucius cair. Uma trégua, nada além disso."
            },()=>{
                var previous=Progress.allianceFlags;
                Progress.allianceFlags|=DarklightAllianceFlags.ThreatRecognized|DarklightAllianceFlags.TruceStarted;
                if(SaveCheckpoint())continuation?.Invoke();
                else Progress.allianceFlags=previous;
            });
            return true;
        }

        void EndNeroTruce()
        {
            if(Progress==null||!Progress.TruceStarted||Progress.TruceEnded)return;
            Progress.allianceFlags|=DarklightAllianceFlags.TruceEnded|DarklightAllianceFlags.RivalryPending;
        }
    }
}
