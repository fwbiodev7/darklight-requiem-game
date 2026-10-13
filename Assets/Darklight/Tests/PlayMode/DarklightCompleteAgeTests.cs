using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightCompleteAgeTests
    {
        [Test] public void GeneratedWholeCharactersCoverReportedAgesAndDirections()
        {
            for(int age=1;age<=4;age++)
            {
                if(age<=3)Assert.That(DarklightWholeCharacterBank.Sheet("RequiemWalkStage"+age).Length,Is.EqualTo(32));
                Assert.That(DarklightWholeCharacterBank.Sheet("RockAttackStage"+age).Length,Is.EqualTo(32));
            }
            Assert.That(Resources.Load<DarklightCompleteAgeCatalog>("Darklight/CompleteAgeCatalog"),Is.Null,"Rejected offline head compositions must not be packaged");
        }
        [Test] public void CompleteAnimationFramesHaveSafeImportSettingsAndPaletteData()
        {
            for(int skin=0;skin<2;skin++)for(int age=1;age<=(skin==0?3:4);age++)
            foreach(var frame in DarklightWholeCharacterBank.Sheet((skin==0?"RequiemWalk":"RockAttack")+"Stage"+age))
            {
                Assert.That(frame.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(frame.texture.mipmapCount,Is.EqualTo(1));
                Assert.That(DarklightWholeCharacterBank.Mask(frame),Is.Not.Null);Assert.That(frame.pixelsPerUnit,Is.InRange(80,130));
                Assert.That(frame.rect.height/frame.pixelsPerUnit,Is.InRange(1.4f,2.8f));
            }
        }
    }
}
