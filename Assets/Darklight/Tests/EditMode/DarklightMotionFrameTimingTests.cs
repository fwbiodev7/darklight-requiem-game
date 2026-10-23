using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightMotionFrameTimingTests
    {
        [TestCase("StartMove",0,1,2,.12f)]
        [TestCase("Brake",2,3,5,.14f)]
        [TestCase("Turn",3,4,5,.14f)]
        public void EachActionUsesItsOwnCompletePosesAndSettles(string clip,int first,int middle,int last,float duration)
        {
            Assert.That(DarklightMotionFrameTiming.Frame(clip,0),Is.EqualTo(first));
            Assert.That(DarklightMotionFrameTiming.Frame(clip,duration*.5f),Is.EqualTo(middle));
            Assert.That(DarklightMotionFrameTiming.Frame(clip,duration),Is.EqualTo(last));
            Assert.That(DarklightMotionFrameTiming.Frame(clip,10),Is.EqualTo(last));
        }
        [Test] public void InvalidTimesAndUnrelatedActionsDoNotSelectAnOutOfRangeFrame()
        {
            Assert.That(DarklightMotionFrameTiming.Frame("StartMove",float.NaN),Is.Zero);
            Assert.That(DarklightMotionFrameTiming.Frame("Turn",-1),Is.EqualTo(3));
            Assert.That(DarklightMotionFrameTiming.Supports("Attack"),Is.False);
        }
    }
}
