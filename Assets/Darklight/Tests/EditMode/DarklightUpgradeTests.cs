using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightUpgradeTests
    {
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)]
        public void EveryChapterHasConnectedArenasAndOptionalExploration(int chapter)
        {
            var map=DarklightMapDefinition.ForChapter(chapter);var visited=new HashSet<Vector2Int>();var q=new Queue<Vector2Int>();
            Vector2Int Cell(Vector2 p)=>new Vector2Int(Mathf.FloorToInt(p.x/map.CellSize),Mathf.FloorToInt(p.y/map.CellSize));
            q.Enqueue(Cell(map.Spawn));visited.Add(Cell(map.Spawn));
            while(q.Count>0){var c=q.Dequeue();foreach(var step in new[]{Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right})if(map.Walkable(c+step)&&visited.Add(c+step))q.Enqueue(c+step);}
            foreach(var p in map.Arenas)Assert.That(visited.Contains(Cell(p)),Is.True,"Disconnected arena");
            foreach(var p in map.Checkpoints)Assert.That(visited.Contains(Cell(p)),Is.True,"Unsafe checkpoint");
            foreach(var p in new[]{map.Ally,map.Relic,map.Portal})Assert.That(visited.Contains(Cell(p)),Is.True);
            Assert.That(visited.Count,Is.GreaterThan(350));Assert.That(map.Bounds.height,Is.GreaterThan(28));
        }
        [TestCase(0)][TestCase(1)][TestCase(2)]
        public void PowersRespectCostsDurationAndIndependentRecovery(int mode)
        {
            var power=new DarklightResonance();power.Select(mode);Assert.That(power.Activate(),Is.True);
            Assert.That(power.Energy,Is.EqualTo(100-DarklightResonance.Costs[mode]));Assert.That(power.Activate(),Is.False);
            power.Tick(0);Assert.That(power.Remaining,Is.EqualTo(DarklightResonance.Durations[mode]));
            power.Tick(DarklightResonance.Durations[mode]+.1f);Assert.That(power.Active,Is.EqualTo(-1));Assert.That(power.Recovery[mode],Is.EqualTo(DarklightResonance.Cooldowns[mode]));
            power.Select((mode+1)%3);Assert.That(power.Activate(),Is.True,"Other style has its own cooldown");
        }
        [Test] public void LegacySaveCooldownMigratesAndActivePowerCannotBypassRecoveryOnReload()
        {
            var p=DarklightProgress.Decode("{\"version\":1,\"chapter\":1,\"level\":2,\"resonanceCooldown\":17}");Assert.That(p.resonanceEnergy,Is.EqualTo(100));var a=new DarklightResonance();a.Restore(p);
            foreach(var cd in a.Recovery)Assert.That(cd,Is.EqualTo(17));a.Tick(20);a.Select(1);a.Activate();a.Store(p);
            var restored=new DarklightResonance();restored.Restore(p);Assert.That(restored.Active,Is.EqualTo(-1));Assert.That(restored.Recovery[1],Is.GreaterThanOrEqualTo(40));Assert.That(restored.Energy,Is.EqualTo(a.Energy));
        }
    }
}
