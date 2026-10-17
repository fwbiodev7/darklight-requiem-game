using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightIdleFrameTimingTests
    {
        [Test] public void BlinksAreOccasionalAndSeparateFromBreathing()
        {
            Assert.That(DarklightIdleFrameTiming.Frame(6,2.6f,5),Is.Not.EqualTo(5));
            Assert.That(DarklightIdleFrameTiming.Frame(6,6.05f,5),Is.EqualTo(5));
            Assert.That(DarklightIdleFrameTiming.Frame(6,6.3f,5),Is.Not.EqualTo(5));
            Assert.That(DarklightIdleFrameTiming.Frame(6,14.18f,5),Is.EqualTo(5));
        }
        [Test] public void BlinkIsBriefAndNeverPersistsAfterWrap()
        {
            Assert.That(DarklightIdleFrameTiming.Frame(6,14.4f,5),Is.Not.EqualTo(5));
            Assert.That(DarklightIdleFrameTiming.Frame(6,20.35f,5),Is.EqualTo(5));
            for(int i=0;i<1000;i++)Assert.That(DarklightIdleFrameTiming.Frame(6,i*.037f,5),Is.InRange(0,5));
        }
        [Test] public void ExistingTwoFramePagesKeepTheirOriginalCadence()
        {
            Assert.That(DarklightIdleFrameTiming.Frame(2,0),Is.Zero);
            Assert.That(DarklightIdleFrameTiming.Frame(2,.6f),Is.EqualTo(1));
            Assert.That(DarklightIdleFrameTiming.Frame(2,1.1f),Is.Zero);
        }
        [Test] public void InvalidClockOrFrameCountCannotIndexOutsideThePage()
        {
            Assert.That(DarklightIdleFrameTiming.Frame(0,1),Is.Zero);
            Assert.That(DarklightIdleFrameTiming.Frame(6,float.NaN,5),Is.Zero);
            Assert.That(DarklightIdleFrameTiming.Frame(6,float.PositiveInfinity,5),Is.Zero);
            Assert.That(DarklightIdleFrameTiming.Frame(6,-1,5),Is.Zero);
        }
    }
}
