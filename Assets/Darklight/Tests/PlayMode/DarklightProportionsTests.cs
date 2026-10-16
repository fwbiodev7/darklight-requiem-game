using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightProportionsTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        [UnitySetUp] public IEnumerator Prepare(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;}
        [UnityTest] public IEnumerator YoungRockKeepsFourDistinctStepsAndScaleAcrossLegacyAgeInputs()
        {
            game.PreviewCampaign();game.SelectSkin(1,false);game.Progress.weaponUnlocks=7;
            var view=lab.Player.GetComponent<DarklightCharacterView>();var art=lab.Player.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            foreach(int stage in new[]{0,1,2,3,4,0})
            {
            lab.Player.GetComponent<CharacterAppearanceSystem>().PreviewStage(stage);
            for(int weapon=0;weapon<3;weapon++)
            {
                game.Progress.equippedWeapon=weapon;game.Weapons.Restore();
                for(int d=0;d<8;d++)
                {
                    var model=new CombatModel(game.Weapons.Equipped.loadout);Vector2 move=Quaternion.Euler(0,0,-d*45)*Vector2.down;
                    var frames=new HashSet<Sprite>();
                    for(int f=0;f<4;f++)
                    {
                        model.Tick(f==0?.01f:.1f,new CombatInput{move=move,aim=move});view.Present(model,move);
                        var today=DarklightSpriteExpectations.Today(1,stage,weapon,d,model,move);
                        if(today!=null)
                        {
                            Assert.That(art.sprite,Is.SameAs(today));Assert.That(view.UsesCompleteAgeSprite,Is.True);Assert.That(art.flipX,Is.False);
                        }
                        frames.Add(art.sprite);
                        var block=new MaterialPropertyBlock();art.GetPropertyBlock(block);Assert.That(block.GetFloat("_Physique"),Is.Zero,"Whole body walking was deformed a second time");
                        Assert.That(art.transform.localScale,Is.EqualTo(Vector3.one*1.325f));Assert.That(art.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                    }
                    bool todayWalk=TodayCharacterBank.Page(1,stage,weapon,d,"Walk")!=null;
                    Assert.That(frames.Count,Is.EqualTo(4),"Walking must use the available distinct authored steps");
                    model.Tick(.01f,new CombatInput{aim=move,lightPressed=true});view.Present(model,move);
                    if(view.UsesAgePoseFallback)DarklightAgeWeaponRegressionTests.AssertIdentity(art.sprite,1,stage,weapon);else Assert.That(art.sprite.name,Does.Not.StartWith("RockWalk"),"Walking replaced accepted combat art");
                    Assert.That(model.State,Is.EqualTo(CombatState.Attack));
                }
            }
            }
            yield return null;
        }
        [UnityTest] public IEnumerator AllTenMapImagesAndCollisionsShareScaleAndCharactersHaveHumanProportions()
        {
            string save=System.IO.File.Exists(DarklightProgress.Path)?System.IO.File.ReadAllText(DarklightProgress.Path):null;
            for(int n=0;n<10;n++)
            {
                if(n<6)game.PreviewChapter(n);else game.ExploreRegion(n);yield return null;
                var ground=GameObject.Find("MapaIlustrado").GetComponent<SpriteRenderer>();var bounds=ground.bounds;
                Assert.That(bounds.size.x,Is.EqualTo(game.Map.Bounds.width).Within(.005f));Assert.That(bounds.size.y,Is.EqualTo(game.Map.Bounds.height).Within(.005f));
                Assert.That(game.Map.CellSize,Is.EqualTo(.5f*DarklightWorldScale.Environment).Within(.0001f));
                var path=new List<Vector2>();
                foreach(var point in new[]{game.Map.Ally,game.Map.Relic,game.Map.Portal})
                    Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,game.Map.SafeSpawn(point),path),Is.True,"No path at region "+n);
                var source=game.Map.Art.spawn;Vector2 roundtrip=game.Map.Art.Pixel(game.Map.Art.World(source));Assert.That(Vector2.Distance(roundtrip,source),Is.LessThan(.005f));
                string npc=n==1?"IcaroNox":n==3||n==4?"KaelAliado":"LyraMoreau";
                var obj=GameObject.Find(npc);if(n<6&&obj!=null)
                {
                    var r=obj.GetComponent<SpriteRenderer>();Assert.That(r.bounds.size.y,Is.InRange(2.35f,3.0f),"Undersized NPC "+npc);
                }
            }
            game.PreviewChapter(0);game.PreviewPanel("Pause");
            var spawn=typeof(DarklightCampaign).GetMethod("Spawn",BindingFlags.Instance|BindingFlags.NonPublic);
            for(int i=0;i<11;i++)spawn.Invoke(game,new object[]{(RequiemEnemyKind)i,game.Map.Spawn,false});
            yield return null;
            foreach(var enemy in Object.FindObjectsByType<DarklightEnemy>(FindObjectsSortMode.None))
            {
                var r=enemy.GetComponentInChildren<SpriteRenderer>();Assert.That(r.bounds.size.y,Is.EqualTo(DarklightWorldScale.EnemyHeight(enemy.kind)).Within(.01f));
                var standingScale=r.transform.localScale;
                for(int phase=0;phase<4;phase++){typeof(DarklightEnemy).GetMethod("ShowAttack",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(enemy,new object[]{phase});Assert.That(r.transform.localScale,Is.EqualTo(standingScale),"Attack rescaled the complete body instead of preserving its authored posture");Assert.That(r.bounds.size.y,Is.InRange(DarklightWorldScale.EnemyHeight(enemy.kind)*.55f,DarklightWorldScale.EnemyHeight(enemy.kind)*1.8f));}
            }
            Assert.That(System.IO.File.Exists(DarklightProgress.Path)?System.IO.File.ReadAllText(DarklightProgress.Path):null,Is.EqualTo(save));
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
    }
}





