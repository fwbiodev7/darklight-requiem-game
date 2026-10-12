using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightNarrativeTests
    {
        [Test]public void NewSavesDoNotInferAdvancedFormsFromChapterOrAppearance()
        {var p=new DarklightProgress{chapter=5,appearanceStage=4};Assert.That(p.FormUnlocked,Is.Zero);p.chapter=9;p.formsUnlocked=2;Assert.That(p.FormUnlocked,Is.EqualTo(2));}
        [Test]public void LegacyInferredFormsArePreservedDuringMigration()
        {var p=new DarklightProgress{version=1,chapter=5,formsUnlocked=0,preferredForm=3,completed=true,ending=2};var s=DarklightProgress.Decode(JsonUtility.ToJson(p));Assert.That(s.formsUnlocked,Is.EqualTo(3));Assert.That(s.preferredForm,Is.EqualTo(3));Assert.That(s.legacyFinaleCompleted,Is.True);Assert.That(s.completed,Is.True);Assert.That(s.ending,Is.EqualTo(2));}
        [Test]public void TenChaptersAndDataArraysStayAligned()
        {Assert.That(DarklightStory.Chapters.Length,Is.EqualTo(10));Assert.That(DarklightStory.Places.Length,Is.EqualTo(10));Assert.That(DarklightStory.Bosses.Length,Is.EqualTo(10));Assert.That(DarklightStory.Introductions.Length,Is.EqualTo(10));Assert.That(DarklightStory.BossLines.Length,Is.EqualTo(10));Assert.That(DarklightStory.Conclusions.Length,Is.EqualTo(10));}
        [Test]public void SecretEndingRequiresEveryRecordedConditionAndAllOptionalMemories()
        {var p=new DarklightProgress{expansionFlags=15};for(int i=0;i<10;i++)p.relics[i]=true;Assert.That(p.SecretEndingAvailable,Is.True);for(int i=0;i<4;i++){p.expansionFlags=15&~(1<<i);Assert.That(p.SecretEndingAvailable,Is.False);}p.expansionFlags=15;for(int i=0;i<10;i++){p.relics[i]=false;Assert.That(p.SecretEndingAvailable,Is.False);p.relics[i]=true;}}
        [TestCase("{}")][TestCase("{\"version\":2,\"chapter\":6}")][TestCase("{\"version\":2,\"chapter\":0,\"level\":2000000000}")]
        public void EmptyOrIncompleteSavesDoNotEnableContinue(string json)=>Assert.That(DarklightProgress.Decode(json),Is.Null);
        [Test]public void ExpandedCheckpointPhaseAndFlagsRoundTripWithoutResettingAge()
        {var p=new DarklightProgress{chapter=9,checkpoint=3,bossPhase=3,appearanceStage=4,expansionFlags=15,missionWeaponHits=7,missionWave=2};var s=DarklightProgress.Decode(JsonUtility.ToJson(p));Assert.That(s.chapter,Is.EqualTo(9));Assert.That(s.bossPhase,Is.EqualTo(3));Assert.That(s.expansionFlags,Is.EqualTo(15));Assert.That(s.appearanceStage,Is.EqualTo(4));Assert.That(s.missionWeaponHits,Is.EqualTo(7));}
    }
}
