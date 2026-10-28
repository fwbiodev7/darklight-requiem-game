using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightTransitionTimingTests
    {
        [Test] public void TravelIsFullyCoveredBeforeTheMapChanges()
        {
            Assert.That(DarklightTransitionTiming.Fade(0,true),Is.Zero);
            Assert.That(DarklightTransitionTiming.Fade(1,true),Is.EqualTo(1));
            Assert.That(DarklightTransitionTiming.Fade(0,false),Is.EqualTo(1));
            Assert.That(DarklightTransitionTiming.Fade(1,false),Is.Zero);
        }
        [Test] public void OutOfRangeDeltaCannotExposeMoreThanTheScreen()
        {
            Assert.That(DarklightTransitionTiming.Fade(-1,true),Is.Zero);
            Assert.That(DarklightTransitionTiming.Fade(4,true),Is.EqualTo(1));
            Assert.That(DarklightTransitionTiming.Fade(4,false),Is.Zero);
        }
        [Test] public void CoverIsMonotonicAndRevealNeverFlashesBackToBlack()
        {
            float last=0;
            for(int i=0;i<=100;i++)
            {
                float value=DarklightTransitionTiming.Fade(i/100f,true);
                Assert.That(value,Is.GreaterThanOrEqualTo(last));last=value;
                Assert.That(value+DarklightTransitionTiming.Fade(i/100f,false),Is.EqualTo(1).Within(.0001f));
            }
        }
        [Test] public void PresentationCanBeDismissedImmediatelyWithoutAPartialOverlay()
        {
            Assert.That(DarklightTransitionTiming.Cue(.3f,0),Is.Zero);
            Assert.That(DarklightTransitionTiming.Cue(-1,1.5f),Is.Zero);
            Assert.That(DarklightTransitionTiming.Cue(1.5f,1.5f),Is.Zero);
            Assert.That(DarklightTransitionTiming.Cue(2,1.5f),Is.Zero);
        }
        [Test] public void CueFadesAtBothEndsInsteadOfFlashingInstantly()
        {
            Assert.That(DarklightTransitionTiming.Cue(.09f,1.5f),Is.EqualTo(.5f).Within(.001f));
            Assert.That(DarklightTransitionTiming.Cue(.5f,1.5f),Is.EqualTo(1));
            Assert.That(DarklightTransitionTiming.Cue(1.325f,1.5f),Is.EqualTo(.5f).Within(.001f));
        }
    }
}
