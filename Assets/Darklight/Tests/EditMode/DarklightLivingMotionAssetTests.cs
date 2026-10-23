using System;
using NUnit.Framework;
using UnityEngine;
namespace Darklight.Tests
{
    public sealed class DarklightLivingMotionAssetTests
    {
        [Test] public void StartBrakeAndTurnResolveWholePosesInAllDirectionsAndForms()
        {
            if(TodayCharacterBank.Page(0,0,0,0,"Motion")==null)Assert.Ignore("Motion sources are awaiting complete-form import");
            for(int skin=0;skin<2;skin++)for(int direction=0;direction<8;direction++)foreach(string clip in new[]{"StartMove","Brake","Turn"})
            {
                var living=new DarklightLivingPresentation();
                if(clip=="StartMove")
                {living.Observe(0,CombatState.Neutral,Vector2.zero,1,0,false,false);living.Observe(.01f,CombatState.Neutral,Vector2.right,1,0,false,false);}
                else
                {living.Observe(0,CombatState.Neutral,Vector2.right,1,0,false,false);living.Observe(.2f,CombatState.Neutral,clip=="Brake"?Vector2.zero:Vector2.left,1,0,false,false);}
                Assert.That(living.Clip,Is.EqualTo(clip));
                var page=TodayCharacterBank.Page(skin,0,0,direction,"Motion");Assert.That(page,Is.Not.Null);
                var sprite=TodayCharacterBank.LivingPose(skin,0,direction,living);Assert.That(sprite,Is.Not.Null);
                int frame=Array.IndexOf(page.directions,direction)*page.phases+DarklightMotionFrameTiming.Frame(clip,0);
                Assert.That(sprite.name,Is.EqualTo(page.spriteNames[frame]));Assert.That(TodayCharacterBank.WholeFrameId(sprite),Is.EqualTo(frame+1));
                for(int form=1;form<4;form++)
                {
                    Assert.That(DarklightTransformedBank.TryResolve(sprite,form,out var transformed,out var mask,out int owner),Is.True);
                    Assert.That(transformed,Is.Not.Null);Assert.That(mask,Is.Not.Null);Assert.That(owner,Is.EqualTo(frame+1));
                    Assert.That(transformed.pixelsPerUnit,Is.EqualTo(sprite.pixelsPerUnit));
                }
            }
        }
        [Test] public void BrakingAndTurningSettleIntoTheApprovedIdleOfTheSameSkinAndDirection()
        {
            for(int skin=0;skin<2;skin++)for(int direction=0;direction<8;direction++)foreach(bool turning in new[]{false,true})
            {
                var living=new DarklightLivingPresentation();
                living.Observe(0,CombatState.Neutral,Vector2.right,1,0,false,false);
                var movement=turning?Vector2.left:Vector2.zero;
                living.Observe(.2f,CombatState.Neutral,movement,1,0,false,false);
                living.Observe(.31f,CombatState.Neutral,movement,1,0,false,false);
                Assert.That(living.Clip,Is.EqualTo(turning?"Turn":"Brake"));
                Assert.That(DarklightMotionFrameTiming.Frame(living.Clip,living.ClipTime),Is.EqualTo(5));
                Assert.That(TodayCharacterBank.LivingPose(skin,0,direction,living),Is.SameAs(TodayCharacterBank.YoungIdle(skin,0,direction,0)));
            }
        }
    }
}
