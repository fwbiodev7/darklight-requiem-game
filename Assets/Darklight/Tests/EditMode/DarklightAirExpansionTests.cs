using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightAirExpansionTests
    {
        [Test]public void HitstopFreezesAirPhysicsAndImmunity()
        {var b=new DarklightAirBody();Assert.That(b.React(DarklightAirReaction.Launch),Is.True);b.Tick(.1f);float h=b.Height,v=b.Velocity;for(int i=0;i<50;i++)b.Tick(0);Assert.That(b.Height,Is.EqualTo(h));Assert.That(b.Velocity,Is.EqualTo(v));}
        [Test]public void DifferentLaunchForcesChangeApexWithoutMovingTheGroundPlane()
        {var a=new DarklightAirBody();var b=new DarklightAirBody();a.React(DarklightAirReaction.Launch,2.6f);b.React(DarklightAirReaction.Launch,5);a.Tick(.2f);b.Tick(.2f);Assert.That(b.Height,Is.GreaterThan(a.Height));}
        [Test]public void BossAndArmorRejectLaunchAndFollowups()
        {var b=new DarklightAirBody();Assert.That(b.React(DarklightAirReaction.Launch,4,true),Is.False);Assert.That(b.React(DarklightAirReaction.Slam),Is.False);Assert.That(b.Airborne,Is.False);}
        [Test]public void AirComboCannotRelauchOrSuspendForever()
        {var b=new DarklightAirBody();b.React(DarklightAirReaction.Launch);Assert.That(b.React(DarklightAirReaction.Launch),Is.False);for(int i=0;i<10;i++)Assert.That(b.React(DarklightAirReaction.Suspend),Is.True);Assert.That(b.React(DarklightAirReaction.Suspend),Is.False);b.Tick(6);Assert.That(b.Airborne,Is.False);Assert.That(b.React(DarklightAirReaction.Launch),Is.False);b.Tick(.81f);Assert.That(b.React(DarklightAirReaction.Launch),Is.True);}
        [Test]public void SlamLandsExactlyOnceAndCannotBeCancelledByAnotherHit()
        {var b=new DarklightAirBody();b.React(DarklightAirReaction.Launch);b.Tick(.15f);Assert.That(b.React(DarklightAirReaction.Slam),Is.True);Assert.That(b.React(DarklightAirReaction.Suspend),Is.False);Assert.That(b.Tick(.2f),Is.True);Assert.That(b.SlamPending,Is.True);Assert.That(b.Tick(.2f),Is.False);Assert.That(b.Height,Is.Zero);}
        [Test]public void ThrowLeavesAWindowForTheSecondChase()
        {var b=new DarklightAirBody();b.React(DarklightAirReaction.Launch);b.Tick(.2f);b.React(DarklightAirReaction.Throw);float h=b.Height;b.Tick(.15f);Assert.That(b.Height,Is.EqualTo(h));b.Tick(.2f);Assert.That(b.Airborne,Is.True);}
        [TestCase(0)][TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)][TestCase(5)][TestCase(6)][TestCase(7)]
        public void AirCancelRequiresAHitAndRecoveryAndCanRedirect(int direction)
        {
            var hit=ScriptableObject.CreateInstance<AttackDefinition>();var next=ScriptableObject.CreateInstance<AttackDefinition>();
            try
            {
                hit.startup=.1f;hit.active=.1f;hit.recovery=.2f;
                var m=new CombatModel(new CombatLoadout{lights=new[]{hit}});m.Tick(.01f,new CombatInput{lightPressed=true});
                Assert.That(m.TryAirFollowup(next,default),Is.False);m.Tick(.11f,default);m.RegisterHit(this);
                Assert.That(m.TryAirFollowup(next,default),Is.False);m.Tick(.12f,default);m.Tick(.001f,default);
                var face=(Vector2)(Quaternion.Euler(0,0,direction*45)*Vector2.down);
                Assert.That(m.TryAirFollowup(next,new CombatInput{aim=face}),Is.True);Assert.That(Vector2.Distance(m.Facing,face),Is.LessThan(.001f));
                m.Hurt();Assert.That(m.TryAirFollowup(hit,default),Is.False);
            }
            finally{Object.DestroyImmediate(hit);Object.DestroyImmediate(next);}
        }
        [Test]public void WhiffCannotEnterAnAirFinisherDuringRecovery()
        {var a=ScriptableObject.CreateInstance<AttackDefinition>();try{var m=new CombatModel(new CombatLoadout{lights=new[]{a}});m.Tick(.01f,new CombatInput{lightPressed=true});m.Tick(a.startup+a.active+.01f,default);Assert.That(m.TryAirFollowup(a,default),Is.False);}finally{Object.DestroyImmediate(a);}}
        [Test]public void SixMotorcycleMovesRetainAllOriginalLightsAndBoundContactPulses()
        {
            var obj=new GameObject("EditModeMotorcycleProfile");
            try
            {
                var rider=obj.AddComponent<DarklightAbyssRider>();var weapon=rider.Initialize(null);
                Assert.That(rider.ExpandedMoves.Length,Is.EqualTo(6));Assert.That(weapon.loadout.lights.Length,Is.EqualTo(7));
                Assert.That(weapon.loadout.lights[0].displayName,Is.EqualTo("Serra de Entrada"));
                foreach(var a in rider.ExpandedMoves){Assert.That(a.hitPulses,Is.InRange(1,6));Assert.That(a.startup,Is.GreaterThan(0));Assert.That(a.recovery,Is.GreaterThan(0));}
                Assert.That(weapon.loadout.airFinisher.airReaction,Is.EqualTo(DarklightAirReaction.Slam));
                Assert.That(weapon.loadout.heavyBranch.displayName,Is.EqualTo("Roda Destruidora"));
            }
            finally{Object.DestroyImmediate(obj);}
        }
    }
    public sealed class DarklightMultiverseTests
    {
        [Test]public void OldSaveGetsEmptyDiscoveryFlagsAndRewardsAreGrantedOnlyOnce()
        {
            var p=DarklightProgress.Decode("{\"version\":2,\"chapter\":0,\"level\":1}");Assert.That(p.multiverseRead,Is.Zero);
            Assert.That(p.ReadMultiverse(0),Is.True);Assert.That(p.ReadMultiverse(0),Is.False);
            Assert.That(p.ClaimVisitor(DarklightVisitor.AshRonin),Is.True);int xp=p.xp,shards=p.shards;
            Assert.That(p.ClaimVisitor(DarklightVisitor.AshRonin),Is.False);Assert.That(p.xp,Is.EqualTo(xp));Assert.That(p.shards,Is.EqualTo(shards));
            var restored=DarklightProgress.Decode(JsonUtility.ToJson(p));Assert.That(restored.multiverseDefeated,Is.EqualTo(p.multiverseDefeated));
        }
        [Test]public void AllTenMapsHaveReferencesAndThreeOptionalChallenges()
        {var maps=new System.Collections.Generic.HashSet<int>();int challenges=0;foreach(var e in DarklightMultiverseCatalog.Entries){maps.Add(e.Chapter);if(e.Challenge!=DarklightVisitor.None)challenges++;}Assert.That(maps.Count,Is.EqualTo(10));Assert.That(challenges,Is.EqualTo(3));}
        [TestCase("MossDetonator",8)][TestCase("NeonRevenant",8)][TestCase("AshRonin",8)][TestCase("Relics",12)][TestCase("MotorcycleSouth",12)][TestCase("Maelis",8)]
        public void ImportedSheetsExposeCompleteFiguresWithStableNames(string name,int count)
        {
            var sprites=Resources.LoadAll<Sprite>("Darklight/Multiverse/"+name);System.Array.Sort(sprites,(a,b)=>string.CompareOrdinal(a.name,b.name));
            Assert.That(sprites.Length,Is.EqualTo(count));for(int i=0;i<count;i++){Assert.That(sprites[i].name,Is.EqualTo(name+"_"+i.ToString("00")));Assert.That(sprites[i].texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprites[i].texture.mipmapCount,Is.EqualTo(1));Assert.That(sprites[i].bounds.size.y,Is.GreaterThan(0));}
        }
    }
}
