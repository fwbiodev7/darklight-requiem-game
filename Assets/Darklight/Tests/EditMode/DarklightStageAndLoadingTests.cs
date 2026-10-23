using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace Darklight.Tests
{
    public sealed class DarklightStageAndLoadingTests
    {
        [Test]public void ConfirmedHitsBuildChainsWhileDefenseDoesNotInventHits()
        {
            var style=new DarklightStyleMeter();for(int i=0;i<4;i++)style.Hit(i,0);
            Assert.That(style.ComboHits,Is.EqualTo(4));Assert.That(style.CompletedCombos,Is.EqualTo(1));
            style.Defense(true);Assert.That(style.TotalHits,Is.EqualTo(4));Assert.That(style.ComboHits,Is.EqualTo(4));
            style.Tick(DarklightStyleMeter.ComboWindow+.01f);Assert.That(style.ComboHits,Is.Zero);Assert.That(style.BestCombo,Is.EqualTo(4));
            for(int i=0;i<3;i++)style.Hit(i+10,1);Assert.That(style.CompletedCombos,Is.EqualTo(2));
            style.Damage();Assert.That(style.ComboHits,Is.Zero);Assert.That(style.TotalHits,Is.EqualTo(7));
        }
        [Test]public void RepetitionCannotFarmSSSAndThePeakSurvivesDamage()
        {
            var repeated=new DarklightStyleMeter();for(int i=0;i<100;i++)repeated.Hit(5,0);Assert.That(repeated.RankIndex,Is.LessThan(4));
            var varied=new DarklightStyleMeter();for(int i=0;i<24;i++)varied.Hit(i,i%3,true,true);
            Assert.That(varied.Rank,Is.EqualTo("SSS"));Assert.That(varied.RankProgress,Is.EqualTo(1));
            varied.Damage();Assert.That(varied.RankIndex,Is.LessThan(6));Assert.That(varied.PeakRankIndex,Is.EqualTo(6));
        }
        [Test]public void StageRecordsSuccessfulManeuversActualDamageAndFreezesAfterCompletion()
        {
            var result=new DarklightStageResult{chapter=2};var stats=new DarklightStageStatistics();stats.Begin(result);var style=new DarklightStyleMeter();
            stats.Kill();Assert.That(stats.Dodge(101),Is.True);Assert.That(stats.Dodge(101),Is.False);Assert.That(stats.Dodge(102),Is.True);
            stats.Parry();stats.Damage(12.5f);stats.Damage(0);stats.Damage(-10);stats.Damage(float.NaN);stats.Damage(float.PositiveInfinity);
            for(int i=0;i<6;i++){style.Hit(i,0);stats.Hit(style,i==5);}stats.Tick(15.5f);stats.Finish(style);
            Assert.That(result.kills,Is.EqualTo(1));Assert.That(result.dodges,Is.EqualTo(2));Assert.That(result.parries,Is.EqualTo(1));
            Assert.That(result.damageTaken,Is.EqualTo(12.5f));Assert.That(result.combos,Is.EqualTo(1));Assert.That(result.bestCombo,Is.EqualTo(6));
            Assert.That(result.hits,Is.EqualTo(6));Assert.That(result.specials,Is.EqualTo(1));Assert.That(result.seconds,Is.EqualTo(15.5f));
            string frozen=JsonUtility.ToJson(result);stats.Kill();stats.Damage(40);stats.Tick(20);stats.Hit(style,true);stats.Dodge(103);
            Assert.That(JsonUtility.ToJson(result),Is.EqualTo(frozen));
        }
        [Test]public void OldSavesGetEmptyStatisticsAndNewResultsRoundTripWithoutChangingUnlocks()
        {
            var progress=new DarklightProgress{chapter=2,weaponUnlocks=7,equippedWeapon=2};
            var result=progress.StageResult(2);result.kills=18;result.bestRank=6;result.hits=32;result.bestCombo=12;result.combos=4;result.completed=true;
            var restored=DarklightProgress.Decode(JsonUtility.ToJson(progress));Assert.That(restored,Is.Not.Null);
            Assert.That(restored.equippedWeapon,Is.EqualTo(2));Assert.That(restored.weaponUnlocks,Is.EqualTo(7));Assert.That(restored.StageResult(2).bestRank,Is.EqualTo(6));
            Assert.That(restored.StageResult(2).kills,Is.EqualTo(18));Assert.That(restored.StageResult(0).hits,Is.Zero);
            var old=DarklightProgress.Decode("{\"version\":2,\"chapter\":0,\"level\":1}");Assert.That(old,Is.Not.Null);Assert.That(old.StageResult(0).bestRank,Is.Zero);
        }
        [Test]public void LoadingProgressOnlyCompletesAfterTheFinalStageAndNeverMovesBackward()
        {
            var progress=new DarklightLoadingProgress();progress.Report(.72f,"CENÁRIO");progress.Report(.14f,"PERSONAGEM");
            Assert.That(progress.Value,Is.EqualTo(.72f));Assert.That(progress.Ready,Is.False);
            progress.Report(1,"ÚLTIMA ETAPA");Assert.That(progress.Value,Is.EqualTo(.99f));Assert.That(progress.Ready,Is.False);
            progress.Complete();Assert.That(progress.Value,Is.EqualTo(1));Assert.That(progress.Ready,Is.True);progress.Report(.2f,"ANTERIOR");Assert.That(progress.Value,Is.EqualTo(1));
            progress.Reset();Assert.That(progress.Value,Is.Zero);Assert.That(progress.Ready,Is.False);
        }
        [Test]public void LoadingCatchesNestedFailuresAndDisposesEveryStage()
        {
            int disposed=0;Exception failure=null;
            IEnumerator Child(){try{yield return null;throw new InvalidOperationException("test-stage-failure");}finally{disposed++;}}
            IEnumerator Parent(){try{yield return Child();yield return null;}finally{disposed++;}}
            var runner=DarklightLoadingSequence.Run(Parent(),e=>failure=e);int yields=0;while(runner.MoveNext())yields++;
            Assert.That(failure?.Message,Is.EqualTo("test-stage-failure"));Assert.That(disposed,Is.EqualTo(2));Assert.That(yields,Is.EqualTo(1));
        }
    }
}
