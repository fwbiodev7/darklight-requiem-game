using NUnit.Framework;

namespace Darklight.Tests
{
    public sealed class DarklightDeathReversionTests
    {
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void DeathRevertsEveryFormEvenAfterGameplayAlreadyClearedIt(int form)
        {
            var view=new DarklightDeathReversion();
            Assert.That(view.Observe(form,false,10),Is.EqualTo(form));
            Assert.That(view.Observe(0,true,11),Is.EqualTo(form));
            Assert.That(view.SourceForm,Is.EqualTo(form));
            Assert.That(view.Observe(0,true,11.4f),Is.Zero);
            Assert.That(view.Active,Is.True);
            Assert.That(view.Observe(0,true,12),Is.Zero);
            Assert.That(view.Active,Is.False);
        }

        [Test] public void NormalDeathDoesNotCreateATransformation()
        {
            var view=new DarklightDeathReversion();
            view.Observe(0,false,2);
            Assert.That(view.Observe(0,true,3),Is.Zero);
            Assert.That(view.Active,Is.False);
        }

        [Test] public void DeathMenuPauseDoesNotFreezeTheUnscaledTransition()
        {
            var view=new DarklightDeathReversion();
            view.Observe(3,false,20);view.Observe(0,true,21);
            Assert.That(view.Observe(0,true,21.5f),Is.Zero);
            Assert.That(view.Progress,Is.GreaterThan(.6f));
            view.Observe(0,true,22);
            Assert.That(view.Progress,Is.EqualTo(1));
        }

        [Test] public void RepeatedDeadPresentationCannotRestartOrRewindTheForm()
        {
            var view=new DarklightDeathReversion();
            view.Observe(2,false,3);view.Observe(0,true,4);view.Observe(0,true,4.5f);
            Assert.That(view.Observe(2,true,4.5f),Is.Zero);
            Assert.That(view.Observe(2,true,4.1f),Is.Zero);
            Assert.That(view.SourceForm,Is.EqualTo(2));
        }

        [Test] public void RevivingClearsThePreviousDeathAndAllowsAFreshTransformation()
        {
            var view=new DarklightDeathReversion();
            view.Observe(3,false,1);view.Observe(0,true,2);view.Observe(0,true,3);
            Assert.That(view.Observe(0,false,4),Is.Zero);
            Assert.That(view.SourceForm,Is.Zero);
            view.Observe(1,false,5);
            Assert.That(view.Observe(0,true,6),Is.EqualTo(1));
        }

        [Test] public void TransformingDeathBeforeAnAliveRenderStillHasAnExit()
        {
            var view=new DarklightDeathReversion();
            Assert.That(view.Observe(3,true,7),Is.EqualTo(3));
            Assert.That(view.Observe(0,true,8),Is.Zero);
            Assert.That(view.Active,Is.False);
        }
    }
}
