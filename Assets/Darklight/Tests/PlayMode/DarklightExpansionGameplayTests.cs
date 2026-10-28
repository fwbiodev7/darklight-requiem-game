using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace Darklight.Tests
{
    public sealed class DarklightExpansionGameplayTests
    {
        IDisposable storage;DarklightCampaign game;DarklightCombatLab lab;DarklightFighter fighter;DarklightEnemy enemy;Rigidbody2D body;
        static void Call(object target,string method,params object[] args)=>target.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);
        [UnitySetUp] public IEnumerator Setup()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            Directory.CreateDirectory("work/expansion-play-20261023");
            storage=DarklightProgress.IsolateStorageForTests(Path.GetFullPath("work/expansion-play-20261023/save.json"));
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;fighter=lab.Player;body=fighter.GetComponent<Rigidbody2D>();
            game.EnterTraining();yield return null;yield return null;game.SelectSkin(1,false);Call(game,"SetTrainingForm",0);lab.SetPaused(true);
            fighter.enabled=false;fighter.ResetModel();game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);
            enemy=game.MapEnemies[0];enemy.enabled=false;enemy.Health.SetMaxHealth(100000,false);
            Move(body,game.Map.Spawn);Move(enemy.GetComponent<Rigidbody2D>(),body.position+Vector2.down*.9f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){storage?.Dispose();yield return null;}
        static void Move(Rigidbody2D b,Vector2 point){b.position=point;b.transform.position=point;Physics2D.SyncTransforms();}
        void Step(CombatInput input=default)
        {
            const float dt=.01f;Call(enemy,"TickAerial",dt);fighter.AirCombo.BeforeCombat(dt,ref input);game.AbyssRider.BeforeCombat(dt,ref input);
            fighter.Model.Tick(dt,input,game.AbyssRider.AttackClockSpeed(game.AttackSpeed));game.AbyssRider.AfterCombat();
            if(fighter.Model.ActiveThisTick)Call(fighter,"ResolveStrike");Physics2D.SyncTransforms();
        }
        void WaitNeutral(){for(int i=0;i<150&&fighter.Model.State!=CombatState.Neutral;i++)Step(new CombatInput{aim=Vector2.down});}
        void Capture(string name)
        {
            fighter.transform.position=body.position;enemy.transform.position=enemy.GetComponent<Rigidbody2D>().position;Physics2D.SyncTransforms();
            var view=fighter.GetComponent<DarklightCharacterView>();view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");Call(enemy,"LateUpdate");
            lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.8f,-10);
            typeof(DarklightReformedRegionTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{Path.GetFullPath("outputs/expansao-20261023/unity-play/"+name+".png")});
        }
        [UnityTest] public IEnumerator RealResolverLaunchesChasesCombosThrowsAndSlams()
        {
            Step(new CombatInput{aerialPressed=true,aim=Vector2.down});
            for(int i=0;i<100&&!enemy.Airborne;i++)Step(new CombatInput{aim=Vector2.down});
            Assert.That(enemy.Airborne,Is.True,"Launcher must connect through real collider/resolver");
            Assert.That(fighter.AirCombo.Target,Is.SameAs(enemy));Assert.That(fighter.AirCombo.Active,Is.False);
            Capture("01-launch");
            float health=enemy.Health.CurrentHealth;
            Step(new CombatInput{aerialPressed=true,aim=Vector2.down});
            for(int i=0;i<100&&(!fighter.AirCombo.Active||fighter.AirCombo.Chasing);i++)Step(new CombatInput{aim=Vector2.down});
            Assert.That(fighter.AirCombo.Height,Is.GreaterThan(.1f));Assert.That(enemy.Health.CurrentHealth,Is.EqualTo(health),"Chase cannot inflict automatic damage");
            Capture("02-chase");
            WaitNeutral();Step(new CombatInput{lightPressed=true,aim=Vector2.down});
            for(int i=0;i<70&&fighter.AirCombo.Contacts<3;i++)Step(new CombatInput{aim=Vector2.down});
            Assert.That(fighter.AirCombo.Contacts,Is.GreaterThanOrEqualTo(3));Assert.That(enemy.Health.CurrentHealth,Is.LessThan(health));
            Step(new CombatInput{heavyPressed=true,aim=Vector2.down});
            for(int i=0;i<60&&enemy.AirBody.Hits<4;i++)Step(new CombatInput{aim=Vector2.down});
            Assert.That(enemy.AirBody.Hits,Is.GreaterThanOrEqualTo(4),"Throw must hit");
            Capture("03-throw");
            Step(new CombatInput{aerialPressed=true,aim=Vector2.down});
            for(int i=0;i<70&&!fighter.AirCombo.AboveTarget;i++)Step(new CombatInput{aim=Vector2.down});
            Assert.That(fighter.AirCombo.AboveTarget,Is.True);
            Step(new CombatInput{heavyPressed=true,aim=Vector2.down});
            var trace=new System.Text.StringBuilder();
            for(int i=0;i<100&&!enemy.AirBody.SlamPending;i++)
            {Step(new CombatInput{aim=Vector2.down});if(i%5==0){var p=enemy.GetComponent<Collider2D>().ClosestPoint(body.position);trace.AppendLine(i+" "+fighter.Model.Attack?.displayName+" t="+fighter.Model.StateTime+" h="+fighter.AirCombo.Height+" target="+enemy.AirHeight+" hits="+enemy.AirBody.Hits+" hp="+enemy.Health.CurrentHealth+" active="+fighter.Model.ActiveThisTick+" contacts="+fighter.AirCombo.Contacts+" point="+(p-body.position)+" facing="+fighter.Model.Facing+" accept="+fighter.AirCombo.AcceptTarget(enemy)+" shape="+DarklightHitGeometry.Contains(fighter.Model.Attack,body.position,fighter.Model.Facing,p,game.StrikeRangeBonus,fighter.Model.ContactProgress,fighter.Model.ContactStartProgress));}}
            File.WriteAllText("outputs/expansao-20261023/air-play-trace.txt",trace.ToString());
            Assert.That(enemy.AirBody.SlamPending,Is.True,"Final motorcycle pulse must slam the airborne victim: attack="+fighter.Model.Attack?.displayName+" state="+fighter.Model.State+" time="+fighter.Model.StateTime+" height="+fighter.AirCombo.Height+" targetHeight="+enemy.AirHeight+" hits="+enemy.AirBody.Hits+" distance="+Vector2.Distance(body.position,enemy.transform.position));
            Capture("04-final-contact");
            for(int i=0;i<150&&enemy.Airborne;i++)Step();Assert.That(enemy.Airborne,Is.False);
            yield return null;
        }
        [UnityTest] public IEnumerator SixMotorcycleMovesReachContactWithoutRepeatedDamageOrAudio()
        {
            foreach(var attack in game.AbyssRider.ExpandedMoves)
            {
                fighter.ResetModel();enemy.AirBody.Reset();enemy.Health.SetMaxHealth(100000,false);game.Blood.Set(100);
                Move(body,game.Map.Spawn);Move(enemy.GetComponent<Rigidbody2D>(),body.position+Vector2.down*.85f);
                if(attack==game.AbyssRider.ExpandedMoves[2])Step(new CombatInput{skillPressed=true,aim=Vector2.down});
                else if(attack==game.AbyssRider.ExpandedMoves[4])Step(new CombatInput{weaponSpecialPressed=true,move=Vector2.down,aim=Vector2.down});
                else Assert.That(fighter.Model.TryAirFollowup(attack,new CombatInput{aim=Vector2.down}),Is.True);
                for(int i=0;i<140&&fighter.Model.State==CombatState.Attack;i++)Step(new CombatInput{aim=Vector2.down});
                Assert.That(enemy.Health.CurrentHealth,Is.LessThan(enemy.Health.MaxHealth),attack.displayName);
            }
            Assert.That(AudioListener.pause,Is.True);Assert.That(AudioListener.volume,Is.Zero);
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False,source.name);
            yield return null;
        }
        [UnityTest] public IEnumerator HeavyModifierBeforeSawAndParryChordSelectExpandedMoves()
        {
            Step(new CombatInput{heavyPressed=true,heavyHeld=true,aim=Vector2.down});Assert.That(fighter.Model.State,Is.EqualTo(CombatState.Charging));
            Step(new CombatInput{lightPressed=true,heavyHeld=true,aim=Vector2.down});Assert.That(fighter.Model.Attack,Is.SameAs(game.AbyssRider.ExpandedMoves[0]));
            fighter.ResetModel();Step(new CombatInput{lightPressed=true,parryPressed=true,aim=Vector2.down});
            Assert.That(fighter.Model.Attack,Is.SameAs(game.AbyssRider.ExpandedMoves[1]));yield return null;
        }
        [UnityTest] public IEnumerator VirtualKeyboardRunsTheActualUpdatePathForTheSawChord()
        {
            var settings=UnityEngine.InputSystem.InputSystem.settings;var background=settings.backgroundBehavior;var editorInput=settings.editorInputBehaviorInPlayMode;
            settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
            settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard=UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            var action=lab.Input.Action("AbyssSaw");string bindings=UnityEngine.InputSystem.InputActionRebindingExtensions.SaveBindingOverridesAsJson(lab.Input.Asset);
            try
            {
                UnityEngine.InputSystem.InputActionRebindingExtensions.ApplyBindingOverride(action,0,"<Keyboard>/i");
                yield return new WaitForSeconds(1.5f);Call(game,"TickFormPresentation",2f);Call(game,"Close");lab.SetPaused(false);fighter.enabled=true;
                yield return null;yield return null;yield return null;
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.X));yield return null;yield return null;
                UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.X,UnityEngine.InputSystem.Key.I));yield return null;yield return null;
                Assert.That(fighter.Model.Attack,Is.SameAs(game.AbyssRider.ExpandedMoves[0]),"Virtual device must reach input reader, rider and real Fighter.Update; paused="+lab.Paused+" transforming="+game.Transforming+" active="+game.Active+" saw="+action.IsPressed());
            }
            finally
            {lab.SetPaused(true);fighter.enabled=false;UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);UnityEngine.InputSystem.InputActionRebindingExtensions.LoadBindingOverridesFromJson(lab.Input.Asset,bindings,true);settings.backgroundBehavior=background;settings.editorInputBehaviorInPlayMode=editorInput;}
        }
        [UnityTest] public IEnumerator ThreeSecretChallengesSpawnDieRewardOnceAndSurviveReload()
        {
            game.ExitTraining();
            foreach(int index in new[]{1,4,5})
            {
                var entry=DarklightMultiverseCatalog.Entries[index];game.PreviewChapter(entry.Chapter);yield return null;yield return null;lab.SetPaused(true);
                Call(game,"InteractMultiverse","multiverse_"+index);Call(game,"Close");
                Assert.That(game.Progress.multiverseRead&(1<<index),Is.Not.Zero);
                Call(game,"InteractMultiverse","multiverse_"+index);Assert.That(game.MultiverseEnemies.Count,Is.EqualTo(1));
                var visitor=game.MultiverseEnemies[0];Assert.That(visitor.Visitor,Is.EqualTo(entry.Challenge));
                int shards=game.Progress.shards;visitor.Health.TakeDamage(visitor.Health.MaxHealth);
                Assert.That(game.Progress.shards,Is.EqualTo(shards+1));game.EnemyDefeated(visitor);Assert.That(game.Progress.shards,Is.EqualTo(shards+1));
                var saved=DarklightProgress.Decode(JsonUtility.ToJson(game.Progress));Assert.That(saved.multiverseDefeated&(1<<(int)entry.Challenge),Is.Not.Zero);
                Call(game,"LoadMap",entry.Chapter);yield return null;yield return null;lab.SetPaused(true);
                Call(game,"InteractMultiverse","multiverse_"+index);Assert.That(game.MultiverseEnemies.Count,Is.Zero,"Rewarded encounter must stay cleared on return");
            }
        }
        [UnityTest] public IEnumerator ThreeBridgesCrossWithRealFixedPhysicsAndCameraInBothDirections()
        {
            game.ExitTraining();game.PreviewChapter(4);yield return null;yield return null;lab.SetPaused(true);
            var paths=new[]{
                new[]{new Vector2(515,254),new Vector2(563,255),new Vector2(605,267),new Vector2(648,285)},
                new[]{new Vector2(603,500),new Vector2(651,500),new Vector2(711,500),new Vector2(748,518),new Vector2(790,501)},
                new[]{new Vector2(1060,674),new Vector2(1115,675),new Vector2(1165,681),new Vector2(1203,654),new Vector2(1245,632)}};
            int crossed=0;var wait=new WaitForFixedUpdate();
            foreach(var original in paths)foreach(bool reverse in new[]{false,true})
            {
                var path=(Vector2[])original.Clone();if(reverse)Array.Reverse(path);Move(body,game.Map.Art.World(path[0]));
                foreach(var pixel in path)
                {
                    Vector2 target=game.Map.Art.World(pixel);
                    for(int i=0;i<250&&Vector2.Distance(body.position,target)>.075f;i++)
                    {var desired=Vector2.ClampMagnitude((target-body.position)/Time.fixedDeltaTime,4.5f);body.linearVelocity=DarklightNavigation.SlidingVelocity(body,desired,Time.fixedDeltaTime);yield return wait;}
                    body.linearVelocity=Vector2.zero;Assert.That(Vector2.Distance(body.position,target),Is.LessThan(.08f),"Bridge physical blockage at "+pixel);
                }
                crossed++;Assert.That(lab.WorldCamera.GetComponent<DarklightLabCamera>().enabled,Is.True);
            }
            File.WriteAllText("outputs/expansao-20261023/bridge-play-review.txt",crossed+" bidirectional crossings passed using actual Rigidbody2D fixed physics, production SlidingVelocity and enabled game camera. Automated route; no physical controller or human play certification. Audio disabled.");
        }
    }
}
