using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        void AssertCurrentFigure(string context)
        {
            Assert.That(art.sprite,Is.Not.Null,context);
            Assert.That(art.sprite.name,Does.StartWith("Redesign_"),context);
            Assert.That(DarklightWeaponRedesignBank.IsAuthored(art.sprite)||DarklightMotorcycleRedesignBank.IsAuthored(art.sprite),Is.True,context);
            Assert.That(view.UsesUserWholeSprite,Is.False);Assert.That(art.flipX,Is.False);
            Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
        }
        [UnityTest] public IEnumerator EveryActiveWeaponStateExcludesRetiredCharacterFrames()
        {
            int contexts=0;
            foreach(int weapon in new[]{0,2,3})for(int form=0;form<3;form++)for(int direction=0;direction<8;direction++)
            {
                Context(0,weapon,form,direction);var facing=Facing(direction);
                string key=$"weapon={weapon}, form={form}, direction={direction}";AssertCurrentFigure(key+" idle");
                for(int i=0;i<40;i++){Tick(facing,new CombatInput{move=facing});AssertCurrentFigure(key+" locomotion");}
                for(int i=0;i<40;i++){Tick(facing);AssertCurrentFigure(key+" brake/idle");}
                foreach(var input in new[]{new CombatInput{dodgePressed=true},new CombatInput{parryPressed=true},new CombatInput{heavyPressed=true,heavyHeld=true}})
                {
                    fighter.ResetModel();Tick(facing,input);AssertCurrentFigure(key+" "+fighter.Model.State);
                    for(int i=0;i<35;i++){Tick(facing,new CombatInput{heavyHeld=input.heavyHeld});AssertCurrentFigure(key+" "+fighter.Model.State);}
                }
                fighter.Model.Hurt();Present();AssertCurrentFigure(key+" hurt");
                fighter.Model.Hurt(true);Present();AssertCurrentFigure(key+" dead");
                contexts++;yield return null;
            }
            Assert.That(contexts,Is.EqualTo(72));AssertSilent();
            Report("redesign-only-all-states",contexts,0,"All three active weapons, three forms and eight directions exclude retired player frames in idle, locomotion, brake, dodge, parry, charging, hurt and death. Missing dedicated actions hold reviewed new poses; this is exclusion of legacy presentation, not completion of unique animation art.");
        }
        [UnityTest] public IEnumerator ConfirmedContactEmphasizesWholePoseButWhiffsDoNot()
        {
            Context(0,2,0,0);var body=fighter.GetComponent<Rigidbody2D>();var enemy=game.MapEnemies[0];
            for(int i=1;i<game.MapEnemies.Count;i++)game.MapEnemies[i].gameObject.SetActive(false);
            var enemyBody=enemy.GetComponent<Rigidbody2D>();int before=view.ConfirmedContactPresentations;
            int burstBefore=DarklightPixelContactBurst.Current?.StartedCount??0;
            int impactMeshesBefore=lab.GetComponentsInChildren<DarklightCombatImpacts>(true).Length;
            body.position=game.Map.Spawn;enemyBody.position=body.position+Vector2.down*9;Physics2D.SyncTransforms();
            Tick(Vector2.down,new CombatInput{lightPressed=true});
            for(int i=0;i<80;i++){Tick(Vector2.down);if(fighter.Model.ActiveThisTick)Call(fighter,"ResolveStrike");}
            Assert.That(view.ConfirmedContactPresentations,Is.EqualTo(before),"A whiff cannot create contact emphasis");
            Assert.That(DarklightPixelContactBurst.Current?.StartedCount??0,Is.EqualTo(burstBefore),"A whiff cannot start the imported contact animation");
            Assert.That(lab.GetComponentsInChildren<DarklightCombatImpacts>(true).Length,Is.EqualTo(impactMeshesBefore),"A whiff cannot create pixel impact details");
            fighter.ResetModel();enemyBody.position=body.position+Vector2.down*.8f;Physics2D.SyncTransforms();
            Tick(Vector2.down,new CombatInput{lightPressed=true});
            for(int i=0;i<80&&view.ConfirmedContactPresentations==before;i++){Tick(Vector2.down);if(fighter.Model.ActiveThisTick)Call(fighter,"ResolveStrike");}
            Assert.That(view.ConfirmedContactPresentations,Is.GreaterThan(before),"The real resolver must confirm contact first");
            var burst=DarklightPixelContactBurst.Current;Assert.That(burst,Is.Not.Null);
            Assert.That(burst.StartedCount,Is.GreaterThan(burstBefore),"A confirmed hit must start the imported pixel-art clip");
            Assert.That(burst.LastVisibleDiameterWorld,Is.EqualTo(1.4f).Within(.02f),"The maximum alpha-visible punch motif must fill its measured world diameter");
            Assert.That(lab.GetComponentInChildren<DarklightCombatImpacts>(true),Is.Not.Null,"A confirmed hit layers quantized pixel details under the authored burst");
            var contactFrames=new HashSet<Sprite>{burst.LastPresentedSprite};
            foreach(float delta in new[]{.036f,.066f,.066f}){Call(burst,"Advance",delta);contactFrames.Add(burst.LastPresentedSprite);}
            Assert.That(contactFrames.Count,Is.EqualTo(4),"All four authored contact phases must reach the renderer");
            Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*DarklightCharacterView.WorldVisualScale));
            bool reduced=game.ReducedFlashes;
            try{game.ReducedFlashes=true;var camera=lab.WorldCamera.GetComponent<DarklightLabCamera>();camera.ConfirmContact(enemy.transform.position);Assert.That(camera.ContactFocusActive,Is.False);}
            finally{game.ReducedFlashes=reduced;}
            AssertSilent();yield return null;
        }
        [UnityTest] public IEnumerator ReviewedKatanaWalkLoopsReachAllDeclaredFramesAndReturnToIdle()
        {
            var pages=catalog.pages.Where(p=>p.skin==0&&p.weapon==0&&p.form==0&&p.action=="Walk").OrderBy(p=>p.direction).ToArray();
            Assert.That(pages.Length,Is.GreaterThanOrEqualTo(5),"Preserve S, approved SW/W and new NW/N");
            foreach(var page in pages)
            {
                Context(0,0,0,page.direction);var seen=new HashSet<Sprite>();var facing=Facing(page.direction);
                // Keep the authored start-move transition; measure the walking
                // loop only after the actual presentation enters that action.
                for(int warmup=0;warmup<50;warmup++)
                {
                    Tick(facing,new CombatInput{move=facing});
                    if(view.UsesWeaponRedesignSprite&&page.spriteNames.Contains(art.sprite.name))break;
                }
                AssertAuthored(page);
                for(int step=0;step<85;step++)
                {
                    Tick(facing,new CombatInput{move=facing});AssertAuthored(page);seen.Add(art.sprite);
                }
                Assert.That(seen.Count,Is.EqualTo(page.spriteNames.Length),page.id+": every complete walking figure must render");
                var idle=catalog.pages.Single(p=>p.skin==0&&p.weapon==0&&p.form==0&&p.direction==page.direction&&p.action=="Idle");
                for(int settling=0;settling<50;settling++)
                {
                    Tick(facing);
                    if(view.UsesWeaponRedesignSprite&&idle.spriteNames.Contains(art.sprite.name))break;
                }
                AssertAuthored(idle);
            }
            AssertSilent();yield return null;
        }
    }
}
