using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightGearTests
    {
        AttackDefinition attack;
        [SetUp]public void Setup(){attack=ScriptableObject.CreateInstance<AttackDefinition>();attack.startup=.2f;attack.active=.2f;attack.recovery=.4f;}
        [TearDown]public void TearDown()=>Object.DestroyImmediate(attack);
        bool Command(DarklightGearModel gear,int serial,float time=.4f,bool connected=true,float dt=.2f)=>gear.Tick(dt,true,serial,attack,time,connected,true,true,false,0);
        [Test]public void FourDistinctConfirmedPreciseCommandsReachThreeGears()
        {
            var gear=new DarklightGearModel();Assert.That(gear.Level,Is.EqualTo(1));
            Assert.That(Command(gear,1),Is.True);Assert.That(gear.Level,Is.EqualTo(1));
            Assert.That(Command(gear,2),Is.True);Assert.That(gear.Level,Is.EqualTo(2));
            Command(gear,3);Command(gear,4);Assert.That(gear.Level,Is.EqualTo(3));Assert.That(gear.Speed,Is.LessThan(1.3f));Assert.That(gear.Posture,Is.GreaterThan(1.5f));
        }
        [Test]public void SpamAndMissedAttacksCannotEarnRotation()
        {
            var gear=new DarklightGearModel();for(int i=0;i<500;i++)Command(gear,i,.04f,true,.01f);
            Assert.That(gear.Rotation,Is.Zero);for(int i=0;i<8;i++)Assert.That(Command(gear,i,.4f,false),Is.False);Assert.That(gear.Level,Is.EqualTo(1));
        }
        [Test]public void OneSwingCanOnlyRewardOneCommandAndPauseCannotProgress()
        {
            var gear=new DarklightGearModel();Command(gear,1);float rotation=gear.Rotation;
            Assert.That(gear.Tick(0,true,2,attack,.4f,true,true,true,false,0),Is.False);Assert.That(gear.Rotation,Is.EqualTo(rotation));
            Assert.That(Command(gear,1),Is.False);Assert.That(gear.PreciseCommands,Is.EqualTo(1));
        }
        [Test]public void InterruptionAndIdleDecayRotationWithoutRepeatedHurtPenalty()
        {
            var gear=new DarklightGearModel();for(int i=0;i<4;i++)Command(gear,i);
            gear.Tick(.1f,true,4,attack,0,false,false,false,true,0);float after=gear.Rotation;
            gear.Tick(.1f,true,4,attack,0,false,false,false,true,0);Assert.That(gear.Rotation,Is.EqualTo(after));
            for(int i=0;i<100;i++)gear.Tick(.1f,true,4,null,0,false,false,false,false,0);
            Assert.That(gear.Level,Is.EqualTo(1));Assert.That(gear.Rotation,Is.Zero);
        }
        [Test]public void WhiteFormWidensTimingRatherThanGrantingFreeGear()
        {
            Assert.That(DarklightGearModel.TimingWindow(attack,.32f,0),Is.False);Assert.That(DarklightGearModel.TimingWindow(attack,.32f,3),Is.True);
            var gear=new DarklightGearModel();gear.Tick(2,true,1,attack,.32f,true,true,false,false,3);Assert.That(gear.Rotation,Is.Zero);
        }
    }
}
