using NUnit.Framework;
namespace Darklight.Tests
{
    public sealed class DarklightDialogueTests
    {
        [Test] public void RevealPreservesAccentsAndFirstAdvanceRevealsAll()
        {
            var reveal=new DarklightDialogueReveal("a\u0301 noite");Assert.That(reveal.VisibleText,Is.Empty);reveal.Tick(.001f);Assert.That(reveal.VisibleText,Is.EqualTo("a\u0301"));
            reveal.RevealAll();Assert.That(reveal.Complete,Is.True);Assert.That(reveal.VisibleText,Is.EqualTo("a\u0301 noite"));Assert.That(reveal.Tick(1),Is.False);
        }
        [Test] public void PunctuationPausesAndZeroDeltaCannotAdvance()
        {
            var reveal=new DarklightDialogueReveal("A.B");Assert.That(reveal.Tick(0),Is.False);reveal.Tick(.04f);Assert.That(reveal.VisibleText,Is.EqualTo("A."));reveal.Tick(.08f);Assert.That(reveal.VisibleText,Is.EqualTo("A."));reveal.Tick(.2f);Assert.That(reveal.Complete,Is.True);
        }
    }
}
