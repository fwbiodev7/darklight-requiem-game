using System.Collections;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightTechnicalPlayTests
    {
        DarklightCampaign game;
        static void Call(object target,string name,params object[] args)=>target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        [UnitySetUp]public IEnumerator Setup(){DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;game.PreviewCampaign();yield return null;}
        [UnityTest]public IEnumerator ArenaPreservesCampaignAndExposesAllWeapons(){var original=game.Progress;var json=JsonUtility.ToJson(original);game.EnterTraining();yield return null;Assert.That(game.Training,Is.True);Assert.That(game.Progress,Is.Not.SameAs(original));Assert.That(game.Progress.weaponUnlocks,Is.EqualTo(7));Assert.That(Object.FindObjectsByType<DarklightEnemy>(FindObjectsSortMode.None).Length,Is.EqualTo(1));game.ExitTraining();yield return null;Assert.That(game.Progress,Is.SameAs(original));Assert.That(JsonUtility.ToJson(original),Is.EqualTo(json));}
        [UnityTest]public IEnumerator ThreeFormsSelectTheirProvidedThemeWithoutSound(){game.EnterTraining();yield return null;string[] names={"FirstVampireAwakening","VioletTransformation","WhiteTransformation"};for(int i=1;i<=3;i++){Call(game,"SetTrainingForm",i);yield return null;game.TransformationMusic.EvaluateMix(1);Assert.That(game.Form,Is.EqualTo(i));Assert.That(game.TransformationMusic.Source.clip.name,Is.EqualTo(names[i-1]));Assert.That(game.TransformationMusic.Source.isPlaying,Is.False);Assert.That(game.TransformationMusic.Source.volume,Is.Zero);}game.ExitTraining();}
        [UnityTest]public IEnumerator MixedTrainingEncounterSpawnsDistinctActiveRoles(){game.EnterTraining();Call(game,"SpawnTraining",4);yield return null;var enemies=Object.FindObjectsByType<DarklightEnemy>(FindObjectsSortMode.None);Assert.That(enemies.Length,Is.EqualTo(3));Assert.That(enemies.Select(e=>e.kind).Distinct().Count(),Is.EqualTo(3));Assert.That(game.TrainingPassive,Is.False);Assert.That(enemies.Count(e=>e.elite),Is.EqualTo(1));game.ExitTraining();}
        [UnityTest]public IEnumerator RuntimeHitboxesAndTechnicalLoadoutsAreAvailableInPlayableTraining(){game.EnterTraining();typeof(DarklightCampaign).GetProperty("TrainingHitboxes").SetValue(game,true);Call(game,"Close");yield return null;yield return null;var player=DarklightCombatLab.Instance.Player;Assert.That(player.GetComponent<DarklightCombatDebug>(),Is.Not.Null);Assert.That(player.GetComponentsInChildren<LineRenderer>().Any(l=>l.enabled),Is.True);foreach(var weapon in game.weaponDefinitions){Assert.That(weapon.loadout.pauseBranch,Is.Not.Null);Assert.That(weapon.loadout.heavyBranch,Is.Not.Null);Assert.That(weapon.loadout.attackBuffer,Is.LessThanOrEqualTo(weapon.id==DarklightWeaponId.AbyssRider?.45f:.18f));}game.ExitTraining();}
        [UnityTest]public IEnumerator TrainingDefaultDoesNotDamageThePlayerOrAwardCampaignXP(){game.EnterTraining();Call(game,"Close");yield return null;var actor=DarklightCombatLab.Instance.Player.GetComponent<DarklightCombatant>();float hp=actor.Health.CurrentHealth;int xp=game.Progress.xp;Assert.That(actor.Receive(50,Vector2.right),Is.False);Assert.That(actor.Health.CurrentHealth,Is.EqualTo(hp));var enemy=Object.FindFirstObjectByType<DarklightEnemy>();enemy.Health.TakeDamage(enemy.Health.CurrentHealth);yield return null;Assert.That(game.Progress.xp,Is.EqualTo(xp));game.ExitTraining();}
        static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        static void Set(object target,string name,object value)=>target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
        void PrepareContact(DarklightFighter player,DarklightEnemy enemy,Vector2 facing,StrikeKind kind=StrikeKind.Light)
        {
            player.ResetModel();player.transform.position=Vector2.zero;player.GetComponent<Rigidbody2D>().position=Vector2.zero;
            if(kind==StrikeKind.Charged){player.Model.Tick(.01f,new CombatInput{heavyPressed=true,heavyHeld=true,aim=facing});player.Model.Tick(.8f,new CombatInput{heavyHeld=true,aim=facing});player.Model.Tick(.01f,new CombatInput{heavyReleased=true,aim=facing});}
            else player.Model.Tick(.01f,new CombatInput{lightPressed=kind==StrikeKind.Light,aerialPressed=kind==StrikeKind.Aerial,aim=facing});
            var attack=player.Model.Attack;enemy.transform.position=facing*attack.range*.6f;enemy.GetComponent<Rigidbody2D>().position=facing*attack.range*.6f;
            Physics2D.SyncTransforms();player.Model.Tick(attack.startup+attack.active*.6f,new CombatInput{aim=facing});
        }
        [UnityTest]public IEnumerator RealWeaponContactHitsInEightDirectionsAndCannotCrossWallsOrExceedRange()
        {
            game.EnterTraining();Call(game,"Close");yield return null;
            var lab=DarklightCombatLab.Instance;lab.SetPaused(true);var player=lab.Player;var enemy=Object.FindFirstObjectByType<DarklightEnemy>();enemy.enabled=false;
            for(int weapon=0;weapon<3;weapon++)
            {
                player.ResetModel();game.Weapons.Request(weapon);game.Weapons.BeforeTick(.01f,true);
                for(int direction=0;direction<8;direction++)
                {
                    float a=direction*Mathf.PI/4;var facing=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                    PrepareContact(player,enemy,facing);float before=enemy.Health.CurrentHealth;
                    Call(player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.LessThan(before),"Contact missed weapon "+weapon+" direction "+direction);
                }
            }
            player.ResetModel();game.Weapons.Request(0);game.Weapons.BeforeTick(.01f,true);
            PrepareContact(player,enemy,Vector2.up);enemy.transform.position=Vector2.up*(player.Model.Attack.range*.9f);enemy.GetComponent<Rigidbody2D>().position=Vector2.up*(player.Model.Attack.range*.9f);Physics2D.SyncTransforms();
            var nearest=enemy.GetComponent<Collider2D>().ClosestPoint(Vector2.zero);Assert.That(nearest.y,Is.GreaterThan(.1f));
            var wall=new GameObject("QA_StrikeWall");wall.transform.position=nearest*.5f;wall.AddComponent<BoxCollider2D>().size=new Vector2(3,.06f);Physics2D.SyncTransforms();
            float hp=enemy.Health.CurrentHealth;Call(player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(hp),"Sword crossed solid scenery");Object.DestroyImmediate(wall);
            PrepareContact(player,enemy,Vector2.up);enemy.transform.position=Vector2.up*(player.Model.Attack.range+1);enemy.GetComponent<Rigidbody2D>().position=Vector2.up*(player.Model.Attack.range+1);Physics2D.SyncTransforms();hp=enemy.Health.CurrentHealth;Call(player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(hp),"Sword exceeded its reach");
            lab.SetPaused(false);game.ExitTraining();
        }
        [UnityTest]public IEnumerator ArmoredGuardBlocksLightAndAChargedHitBreaksIt()
        {
            game.EnterTraining();Call(game,"SpawnTraining",2);yield return null;var lab=DarklightCombatLab.Instance;lab.SetPaused(true);var enemy=Object.FindFirstObjectByType<DarklightEnemy>();enemy.enabled=false;
            var player=lab.Player;PrepareContact(player,enemy,Vector2.up);Set(enemy,"facing",Vector2.down);Set(enemy,"guardUntil",100f);float hp=enemy.Health.CurrentHealth;
            Call(player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(hp));Assert.That(player.Model.AttackConnected,Is.False,"A guarded attempt counted as a confirmed hit");
            PrepareContact(player,enemy,Vector2.up,StrikeKind.Charged);Call(player,"ResolveStrike");Assert.That(enemy.Health.CurrentHealth,Is.LessThan(hp));Assert.That(enemy.Guarding,Is.False);
            lab.SetPaused(false);game.ExitTraining();
        }
        [UnityTest]public IEnumerator LauncherSuspendsAWholeCharacterWithoutMovingItsGroundColliderUp()
        {
            game.EnterTraining();Call(game,"Close");yield return null;var lab=DarklightCombatLab.Instance;lab.SetPaused(true);var enemy=Object.FindFirstObjectByType<DarklightEnemy>();enemy.enabled=false;
            PrepareContact(lab.Player,enemy,Vector2.up,StrikeKind.Aerial);Assert.That(lab.Player.Model.Attack.launch,Is.True);Call(lab.Player,"ResolveStrike");
            Assert.That(enemy.Airborne,Is.True);var ground=enemy.GetComponent<Rigidbody2D>().position;Call(enemy,"LateUpdate");
            Assert.That(enemy.GetComponentInChildren<SpriteRenderer>().transform.localPosition.y,Is.GreaterThan(0));Assert.That(enemy.GetComponent<Rigidbody2D>().position,Is.EqualTo(ground));
            lab.SetPaused(false);game.ExitTraining();
        }
        [UnityTest]public IEnumerator DifficultyChangesActualEnemyFollowupTimingWithoutChangingHP()
        {
            game.EnterTraining();Call(game,"Close");yield return null;float[] gap=new float[2],hp=new float[2];
            for(int mode=0;mode<2;mode++)
            {
                game.Progress.difficulty=mode;Call(game,"SpawnTraining",1);yield return null;
                var enemy=Object.FindFirstObjectByType<DarklightEnemy>();var lab=DarklightCombatLab.Instance;
                lab.Player.GetComponent<Rigidbody2D>().position=Vector2.zero;enemy.GetComponent<Rigidbody2D>().position=Vector2.up*1.1f;lab.WorldCamera.transform.position=new Vector3(0,0,-10);Physics2D.SyncTransforms();hp[mode]=enemy.Health.MaxHealth;
                float first=-1,timeout=Time.realtimeSinceStartup+9;
                while(Field<int>(enemy,"serial")<2&&Time.realtimeSinceStartup<timeout)
                {if(first<0&&Field<int>(enemy,"serial")==1)first=Field<float>(enemy,"clock");yield return null;}
                Assert.That(Field<int>(enemy,"serial"),Is.GreaterThanOrEqualTo(2),"Enemy never delivered its second attack");gap[mode]=Field<float>(enemy,"clock")-first;
            }
            Assert.That(hp[0],Is.EqualTo(hp[1]),"Difficulty relied on inflated enemy HP");Assert.That(gap[1],Is.LessThan(gap[0]*.8f),"Hard did not actually deliver its follow-up sooner");game.ExitTraining();
        }
        [UnityTest]public IEnumerator TrainingEntryCentersCameraHasNoCampaignInteractionAndRecoversFromDeath()
        {
            game.EnterTraining();Call(game,"Close");yield return null;var lab=DarklightCombatLab.Instance;
            var viewport=lab.WorldCamera.WorldToViewportPoint(lab.Player.transform.position);
            Assert.That(viewport.x,Is.InRange(.1f,.9f));Assert.That(viewport.y,Is.InRange(.1f,.9f));
            Assert.That(game.TryGetInteraction(out _),Is.False,"Training exposed a campaign interaction");
            Set(game,"<TrainingDamage>k__BackingField",true);var hp=lab.Player.GetComponent<DarklightCombatant>().Health;hp.TakeDamage(hp.MaxHealth*2);
            yield return new WaitForSeconds(.1f);Assert.That(hp.IsDead,Is.False);Assert.That(game.Training&&game.Active,Is.True,"Training death opened campaign death menu");game.ExitTraining();
        }
        [UnityTest]public IEnumerator DifficultySelectorLocksExtremeUntilCompletionAndTrainingHUDLeavesWithArena()
        {
            game.Progress.extremeUnlocked=false;game.Progress.completed=false;game.Progress.legacyFinaleCompleted=false;game.PreviewPanel("Difficulty");yield return null;
            var buttons=(System.Collections.IList)Field<object>(game,"buttons");var option=buttons[2];Assert.That((bool)option.GetType().GetField("Item5").GetValue(option),Is.False);
            game.Progress.extremeUnlocked=true;game.PreviewPanel("Difficulty");yield return null;buttons=(System.Collections.IList)Field<object>(game,"buttons");option=buttons[2];Assert.That((bool)option.GetType().GetField("Item5").GetValue(option),Is.True);
            ((System.Action)option.GetType().GetField("Item4").GetValue(option))();Assert.That(game.Difficulty,Is.EqualTo(2));
            game.EnterTraining();Call(game,"Close");yield return null;Assert.That(Field<UnityEngine.UI.Text>(game,"trainingReadout").gameObject.activeSelf,Is.True);
            game.ExitTraining();yield return null;Assert.That(Field<UnityEngine.UI.Text>(game,"trainingReadout").gameObject.activeSelf,Is.False);
        }
    }
}
