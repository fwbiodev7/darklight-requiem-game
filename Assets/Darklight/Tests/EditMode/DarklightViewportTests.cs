using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightViewportTests
    {
        [TestCase(1280,720)][TestCase(1920,1080)][TestCase(2560,1440)][TestCase(1024,576)]
        public void MatchingAspectFillsTheWindowWithoutIntegerScaleBorders(int width,int height)
        {
            var r=DarklightViewport.Fit(width,height);Assert.That(r.x,Is.EqualTo(0).Within(.001));Assert.That(r.y,Is.EqualTo(0).Within(.001));
            Assert.That(r.width,Is.EqualTo(width).Within(.001));Assert.That(r.height,Is.EqualTo(height).Within(.001));
        }
        [TestCase(2560,1080)][TestCase(1024,768)][TestCase(720,1280)]
        public void OtherAspectsRemainCenteredAndKeepTheHudAndWorldAligned(int width,int height)
        {
            var r=DarklightViewport.Fit(width,height);Assert.That(r.center.x,Is.EqualTo(width*.5f).Within(.001));Assert.That(r.center.y,Is.EqualTo(height*.5f).Within(.001));
            Assert.That(r.width/r.height,Is.EqualTo(16f/9).Within(.0001));Assert.That(r.xMin,Is.GreaterThanOrEqualTo(-.001));Assert.That(r.yMin,Is.GreaterThanOrEqualTo(-.001));
            Assert.That(r.xMax,Is.LessThanOrEqualTo(width+.001));Assert.That(r.yMax,Is.LessThanOrEqualTo(height+.001));
        }
    }
}
