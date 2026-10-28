using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightPresentationCacheTests
    {
        [Test] public void WarmFrameOwnershipAndMaskLookupsDoNotAllocateDescriptorStrings()
        {
            TodayCharacterBank.ClearCache();
            var sprite=TodayCharacterBank.YoungIdle(1,0,0,0);
            Assert.That(sprite,Is.Not.Null);var mask=TodayCharacterBank.Mask(sprite);int owner=TodayCharacterBank.WholeFrameId(sprite);
            Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.GreaterThan(0));
            for(int i=0;i<5;i++){TodayCharacterBank.Mask(sprite);TodayCharacterBank.WholeFrameId(sprite);}
            long start=System.GC.GetAllocatedBytesForCurrentThread();int sum=0;
            for(int i=0;i<1000;i++){TodayCharacterBank.Mask(sprite);sum+=TodayCharacterBank.WholeFrameId(sprite);}
            long allocated=System.GC.GetAllocatedBytesForCurrentThread()-start;
            Assert.That(sum,Is.EqualTo(owner*1000));Assert.That(allocated,Is.Zero);
            TodayCharacterBank.ClearCache();Assert.That(TodayCharacterBank.LoadedPageCount,Is.Zero);
        }
    }
}
