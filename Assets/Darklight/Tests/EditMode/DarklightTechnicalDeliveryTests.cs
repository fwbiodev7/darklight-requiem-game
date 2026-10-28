using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightTechnicalDeliveryTests
    {
        [Test] public void LandingContactIsUniquePausedSafelyAndDiscardedByDeathOrMapChange()
        {
            var contact=new TodayLandingContacts();
            Assert.That(contact.Tick(true,true,false),Is.False);
            Assert.That(contact.Tick(true,true,true),Is.False);
            Assert.That(contact.Tick(false,true,false),Is.False);
            Assert.That(contact.Tick(true,true,false),Is.True);
            Assert.That(contact.Tick(true,true,false),Is.False);
            contact.Tick(true,true,true);contact.Tick(true,false,false);
            Assert.That(contact.Tick(true,true,false),Is.False);
            contact.Tick(true,true,true);contact.Reset();Assert.That(contact.Tick(true,true,false),Is.False);
        }
        [Test] public void ExistingAudioCuesResolveWithoutStartingAnySource()
        {
            var clips=Resources.LoadAll<AudioClip>("Darklight/Audio");
            Assert.That(clips.Count(c=>!c.name.StartsWith("dl-user-",System.StringComparison.Ordinal)),Is.EqualTo(132),"Preserve the original bank");
            Assert.That(clips.Count(c=>c.name.StartsWith("dl-user-",System.StringComparison.Ordinal)),Is.EqualTo(DarklightSoundDirector.DeliveredIds.Length));
            foreach(var clip in clips)
            {Assert.That(clip.length,Is.GreaterThan(0),clip.name);Assert.That(clip.channels,Is.InRange(1,2),clip.name);Assert.That(clip.frequency,Is.GreaterThanOrEqualTo(22050),clip.name);}
            var prefixes=new[]{"rpg-metalLatch","rpg-metalClick","rpg-drawKnife","rpg-knifeSlice","impact-impactPunch_heavy","dl-power-parry","dl-power-dodge","dl-power-counter","dl-power-critical","dl-power-finisher","dl-power-rupture","dl-power-eclipse","dl-power-absolute","dl-heartbeat","dl-web-creature-alert","interface-confirmation_","interface-click_"};
            foreach(var prefix in prefixes)Assert.That(clips.Any(c=>c.name.StartsWith(prefix,System.StringComparison.Ordinal)),Is.True,prefix);
            for(int region=0;region<10;region++)Assert.That(clips.Any(c=>c.name=="dl-ambient-"+DarklightSoundDirector.RegionLayer(region)),Is.True,"Region "+region);
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
        [Test] public void EveryFormHasAllTwelveDirectionalActionPoses()
        {
            DarklightAbyssSpriteBank.ClearCache();
            for(int form=0;form<4;form++)Assert.That(DarklightAbyssSpriteBank.HasCompleteDirections(form),Is.True,"Form "+form);
        }
    }
}
