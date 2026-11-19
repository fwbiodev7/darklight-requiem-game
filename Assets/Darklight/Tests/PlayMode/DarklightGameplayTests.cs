using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightGameplayTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        [UnitySetUp] public IEnumerator Prepare(){DarklightAutomation.Silent=true;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewCampaign();}
        [UnityTearDown] public IEnumerator Finish(){InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;yield return null;}
        void Move(Vector2 point){var body=lab.Player.GetComponent<Rigidbody2D>();body.position=point;lab.Player.transform.position=point;Physics2D.SyncTransforms();lab.Player.ResetModel();}
        const BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic;
        void RunArmoredContact(DarklightEnemy enemy)
        {
            typeof(DarklightEnemy).GetMethod("StartEnemyAttack",Hidden).Invoke(enemy,new object[]{game,Vector2.up,DarklightThreat.Area,1.45f,false});
            typeof(DarklightEnemy).GetField("timer",Hidden).SetValue(enemy,0f);
            typeof(DarklightEnemy).GetMethod("TickEnemyAttack",Hidden).Invoke(enemy,new object[]{game,lab});
        }
        [UnityTest] public IEnumerator DialogueUsesRemappedInteractAndCheckpointHasDistinctArt()
        {
            var art=Resources.Load<Sprite>("Darklight/Props/CheckpointShrine");Assert.That(art,Is.Not.Null);Assert.That(art.texture.filterMode,Is.EqualTo(FilterMode.Point));
            var checkpoint=GameObject.Find("Checkpoint").GetComponent<SpriteRenderer>();Assert.That(checkpoint.sprite,Is.EqualTo(art));Assert.That(art.pivot.y/art.rect.height,Is.LessThan(.1f));
            lab.Input.Action("Interact").ApplyBindingOverride(0,"<Keyboard>/k");lab.Input.Action("Interact").ApplyBindingOverride(1,"<Gamepad>/buttonNorth");
            Move(game.Map.SafeSpawn(game.Map.Ally));Assert.That(game.TryInteract(),Is.True);yield return new WaitForSecondsRealtime(.18f);
            var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();try{
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.K));yield return null;Assert.That(game.DialogueTyping,Is.False);int line=game.DialogueLine;InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Y));yield return null;Assert.That(game.DialogueLine,Is.EqualTo(line+1));Assert.That(game.DialogueTyping,Is.True);
            }finally{lab.Input.Action("Interact").RemoveAllBindingOverrides();InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);}
        }
        [UnityTest] public IEnumerator LongDialoguePaginatesAndMouseDismissDoesNotAttackOrRepeatCallback()
        {
            int done=0;string longText="";for(int i=0;i<45;i++)longText+="Vespera permanece entre o sangue e o eclipse. ";
            typeof(DarklightCampaign).GetMethod("Speak",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{new[]{"LYRA|"+longText},(System.Action)(()=>done++)});
            var pages=(string[])typeof(DarklightCampaign).GetField("dialogue",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);Assert.That(pages.Length,Is.GreaterThan(1));
            var text=lab.PixelHUD.transform.Find("HUD640x360/Menus/Dialogo/Fala").GetComponent<UnityEngine.UI.Text>();var generator=new TextGenerator();var settings=text.GetGenerationSettings(text.rectTransform.rect.size);settings.verticalOverflow=VerticalWrapMode.Overflow;
            foreach(var page in pages)Assert.That(generator.GetPreferredHeight(page.Substring(page.IndexOf('|')+1),settings)/text.pixelsPerUnit,Is.LessThanOrEqualTo(text.rectTransform.rect.height),"Dialogue overflow");
            for(int i=0;i<pages.Length-1;i++){game.AdvanceDialogue();game.AdvanceDialogue();}game.AdvanceDialogue();Assert.That(done,Is.Zero);yield return new WaitForSecondsRealtime(.18f);
            var mouse=InputSystem.AddDevice<Mouse>();try{int serial=lab.Player.Model.AttackSerial;var box=lab.PixelHUD.transform.Find("HUD640x360/Menus/Dialogo/Caixa") as RectTransform;var point=RectTransformUtility.WorldToScreenPoint(null,box.TransformPoint(box.rect.center));InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;Assert.That(done,Is.EqualTo(1));Assert.That(game.Active,Is.True);Assert.That(lab.Player.Model.AttackSerial,Is.EqualTo(serial));game.AdvanceDialogue();Assert.That(done,Is.EqualTo(1));}finally{InputSystem.RemoveDevice(mouse);}
        }
        [UnityTest] public IEnumerator DialogueTypesPausesAndAdvancesWithKeyboardAndController()
        {
            Move(game.Map.SafeSpawn(game.Map.Ally));Assert.That(game.TryInteract(),Is.True);Assert.That(game.DialogueTyping,Is.True);int line=game.DialogueLine;Vector3 position=lab.Player.transform.position;
            yield return new WaitForSecondsRealtime(.24f);Assert.That(game.DialogueVisibleCharacters,Is.GreaterThan(0));Assert.That(lab.Player.transform.position,Is.EqualTo(position));
            var root=lab.PixelHUD.transform.Find("HUD640x360/Menus/Dialogo");Assert.That(root.gameObject.activeInHierarchy,Is.True);Assert.That(root.Find("RetratoNPC").GetComponent<UnityEngine.UI.RawImage>().texture,Is.Not.Null);
            var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();try{
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));yield return null;Assert.That(game.DialogueTyping,Is.False);Assert.That(game.DialogueLine,Is.EqualTo(line));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.A));yield return null;Assert.That(game.DialogueLine,Is.EqualTo(line+1));Assert.That(game.DialogueTyping,Is.True);
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return new WaitForSecondsRealtime(.18f);
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.B));yield return null;Assert.That(game.Active,Is.True);Assert.That(lab.Paused,Is.False);Assert.That(lab.Player.Model.State,Is.EqualTo(CombatState.Neutral),"Cancel leaked into dodge");
            }finally{InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);}
        }
        [UnityTest] public IEnumerator CrowdedStrikesHitEveryTargetAndEnemyAreasCannotCrossWalls()
        {
            lab.SetPaused(true);var fighter=lab.Player;var old=fighter.loadout;var targets=new List<GameObject>();var attack=ScriptableObject.CreateInstance<AttackDefinition>();var loadout=new CombatLoadout{heavy=old.heavy,charged=old.charged,counter=old.counter,skill=old.skill,aerial=old.aerial,weaponSpecial=old.weaponSpecial};
            attack.range=3;attack.angle=360;attack.damage=5;attack.startup=.05f;attack.active=.2f;attack.recovery=.2f;loadout.lights=new[]{attack};fighter.loadout=loadout;fighter.ResetModel();
            try{for(int i=0;i<40;i++){var obj=new GameObject("AlvoMultidao"+i);obj.transform.position=fighter.transform.position+Vector3.down*.6f;obj.AddComponent<CircleCollider2D>().isTrigger=true;obj.AddComponent<HealthSystem>().SetMaxHealth(100,false);obj.AddComponent<DarklightCombatant>();targets.Add(obj);}Physics2D.SyncTransforms();
                fighter.Model.Tick(.01f,new CombatInput{lightPressed=true,aim=Vector2.down});fighter.Model.Tick(.06f,default);
                typeof(DarklightFighter).GetMethod("ResolveStrike",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(fighter,null);
                foreach(var target in targets)Assert.That(target.GetComponent<HealthSystem>().CurrentHealth,Is.LessThan(100),target.name);
            }finally{fighter.loadout=old;fighter.ResetModel();foreach(var target in targets)Object.Destroy(target);Object.Destroy(attack);}
            Vector2 point=game.Map.SafeSpawn(game.Map.Spawn+Vector2.down);
            typeof(DarklightCampaign).GetMethod("Spawn",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{RequiemEnemyKind.Armored,point,false});
            var enemy=Object.FindFirstObjectByType<DarklightEnemy>();Move(point+Vector2.up*.8f);var wall=new GameObject("ParedeAtaque").AddComponent<BoxCollider2D>();wall.transform.position=(point+(Vector2)fighter.transform.position)*.5f;wall.size=new Vector2(.5f,.1f);Physics2D.SyncTransforms();
            var actor=fighter.GetComponent<DarklightCombatant>();actor.Health.SetInvincibilityDuration(0);float hp=actor.Health.CurrentHealth;
            RunArmoredContact(enemy);Assert.That(actor.Health.CurrentHealth,Is.EqualTo(hp));Object.Destroy(wall.gameObject);yield return null;
            Physics2D.SyncTransforms();RunArmoredContact(enemy);Assert.That(actor.Health.CurrentHealth,Is.LessThan(hp));
        }

        [UnityTest] public IEnumerator EnemyPilotSpritesAppearOnlyDuringTheirCombatPoses()
        {
            game.EnterTraining();
            typeof(DarklightCampaign).GetMethod("SpawnTraining",Hidden).Invoke(game,new object[]{1});
            var hunter=Object.FindFirstObjectByType<DarklightEnemy>();
            Assert.That(hunter.kind,Is.EqualTo(RequiemEnemyKind.Hunter));
            Vector2 origin=game.Map.Spawn;
            float previousCaptureDelta=Time.captureDeltaTime;
            Time.captureDeltaTime=1f/60f;
            try
            {
                string directory=DarklightTestOutput.GetDirectory("enemy-pilots-20261119");
                var hunterArt=hunter.GetComponentInChildren<SpriteRenderer>();
                for(int i=0;i<190&&hunterArt.sprite.name!="HunterLungePrep20261119";i++)yield return null;
                Assert.That(hunterArt.sprite.name,Is.EqualTo("HunterLungePrep20261119"));
                Assert.That(hunter.StateName,Is.EqualTo("Preparando"));
                Assert.That(hunter.GetComponentInChildren<DarklightAttackWarning>().Visible,Is.True);
                CaptureEnemyPose(directory,"hunter-preparacao");
                for(int i=0;i<50&&hunter.StateName!="Ataque";i++)yield return null;
                Assert.That(hunter.StateName,Is.EqualTo("Ataque"));
                Assert.That(hunterArt.sprite.name,Does.StartWith("EnemyAttacks_"));
                Assert.That(hunter.GetComponentInChildren<DarklightAttackWarning>().Visible,Is.False);
                CaptureEnemyPose(directory,"hunter-impacto");

                typeof(DarklightCampaign).GetMethod("SpawnTraining",Hidden).Invoke(game,new object[]{2});
                var armored=Object.FindFirstObjectByType<DarklightEnemy>();
                Assert.That(armored.kind,Is.EqualTo(RequiemEnemyKind.Armored));
                armored.body.position=origin+Vector2.left*1.65f;
                typeof(DarklightEnemy).GetField("timer",Hidden).SetValue(armored,1f);
                Move(origin);
                var armoredArt=armored.GetComponentInChildren<SpriteRenderer>();
                var fighter=lab.Player;
                fighter.enabled=false;
                try
                {
                    fighter.Model.Tick(DuelStep,new CombatInput
                    {aim=Vector2.left,heavyPressed=true,heavyHeld=true});
                    for(int i=0;i<55&&!armored.Guarding;i++)yield return null;
                    Assert.That(armored.Guarding,Is.True,$"Player state {lab.Player.Model.State}, facing {lab.Player.Model.Facing}");
                    Assert.That(armoredArt.sprite.name,Is.EqualTo("ArmoredGuard20261119"));
                    CaptureEnemyPose(directory,"armored-guarda");
                    fighter.ResetModel();
                    for(int i=0;i<40&&armored.Guarding;i++)yield return null;
                    Assert.That(armoredArt.sprite.name,Is.Not.EqualTo("ArmoredGuard20261119"));
                }
                finally{fighter.enabled=true;}
                foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
            }
            finally{Time.captureDeltaTime=previousCaptureDelta;}
        }

        void CaptureEnemyPose(string directory,string name)
        {
            if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
            var camera=lab.WorldCamera;
            var target=camera.targetTexture;
            var previous=RenderTexture.active;
            var pixels=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            try
            {
                DarklightNativeCapture.Render(camera,target);
                RenderTexture.active=target;
                pixels.ReadPixels(new Rect(0,0,target.width,target.height),0,0);
                pixels.Apply();
                File.WriteAllBytes(Path.Combine(directory,name+".png"),pixels.EncodeToPNG());
            }
            finally{RenderTexture.active=previous;Object.Destroy(pixels);}
        }

        const float DuelStep=1f/60f;

        DarklightEnemy StartLuciusTraining()
        {
            game.EnterTraining();
            typeof(DarklightCampaign).GetMethod("SpawnTraining",Hidden).Invoke(game,new object[]{6});
            var lucius=game.CurrentBoss;
            Assert.That(lucius,Is.Not.Null);
            Assert.That(lucius.IsLucius,Is.True);
            Assert.That(game.TrainingDamage,Is.True);
            return lucius;
        }

        static IEnumerator DuelUntil(System.Func<bool> condition,int frames,string reason)
        {
            for(int i=0;i<frames&&!condition();i++)yield return null;
            Assert.That(condition(),Is.True,reason);
        }

        void PlaceDuelPlayer(DarklightEnemy lucius,float distance)
        {
            Move(lucius.body.position+Vector2.right*distance);
            lab.Player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        }

        static DarklightAttackWarning AttackWarning(DarklightEnemy enemy)=>enemy.GetComponentInChildren<DarklightAttackWarning>();

        [UnityTest]
        public IEnumerator LuciusAnimatesGuardInPlaceWhenBothSidestepsAreBlocked()
        {
            var lucius=StartLuciusTraining();
            var fighter=lab.Player;
            var walls=new List<GameObject>();
            float previousDelta=Time.captureDeltaTime;
            Time.captureDeltaTime=DuelStep;
            fighter.enabled=false;
            game.Progress.difficulty=2;
            try
            {
                var anchor=lucius.body.position;
                Move(anchor+Vector2.down*1.25f);
                foreach(float side in new[]{-1f,1f})
                {
                    var wall=new GameObject("Lucius guard corridor wall");
                    walls.Add(wall);
                    wall.transform.position=anchor+Vector2.right*(side*.56f);
                    wall.AddComponent<BoxCollider2D>().size=new Vector2(.12f,4f);
                }
                Physics2D.SyncTransforms();
                Assert.That(DarklightNavigation.Clear(anchor,fighter.transform.position),Is.True);
                Assert.That(DarklightNavigation.Clear(anchor,anchor+Vector2.left*.64f),Is.False);
                Assert.That(DarklightNavigation.Clear(anchor,anchor+Vector2.right*.64f),Is.False);
                fighter.Model.Tick(DuelStep,new CombatInput{aim=Vector2.up,heavyPressed=true,heavyHeld=true});
                yield return DuelUntil(()=>lucius.Guarding,42,"Lucius did not guard when neither sidestep was clear");
                var seen=new HashSet<string>();
                var aim=lucius.CombatModel.Facing;
                for(int i=0;i<30&&lucius.Guarding;i++)
                {
                    string sprite=lucius.LuciusView.Renderer.sprite.name;
                    Assert.That(sprite,Does.StartWith("Redesign_LuciusNormalGuardFront20261119_"));
                    seen.Add(sprite);
                    Assert.That(lucius.body.linearVelocity,Is.EqualTo(Vector2.zero));
                    Assert.That(lucius.CombatModel.Facing,Is.EqualTo(aim));
                    yield return null;
                }
                Assert.That(seen.Count,Is.EqualTo(3),"Guard must present anticipation, contact and recovery");
            }
            finally
            {
                foreach(var wall in walls)Object.Destroy(wall);
                fighter.enabled=true;
                Time.captureDeltaTime=previousDelta;
            }
        }

        [UnityTest]
        public IEnumerator LuciusCompletesThreePhasesWithRealAttacksParryAerialCounterAndDodge()
        {
            var lucius=StartLuciusTraining();
            var fighter=lab.Player;
            var playerHealth=fighter.GetComponent<DarklightCombatant>().Health;
            var opponent=lucius.GetComponent<DarklightCombatant>();
            var model=lucius.CombatModel;
            game.Progress.luciusStance=-1;
            float previousDelta=Time.captureDeltaTime;
            Time.captureDeltaTime=DuelStep;
            fighter.enabled=false;
            try
            {
                PlaceDuelPlayer(lucius,1.12f);
                var launch=game.weaponDefinitions[0].loadout.aerial;
                Assert.That(launch,Is.Not.Null);
                Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Launch),Is.False,"Normal Lucius must resist an unearned launch");

                yield return DuelUntil(()=>model.State==CombatState.Attack,180,"Lucius never chose an attack in Normal");
                Assert.That(lucius.Phase,Is.EqualTo(1));
                Assert.That(AttackWarning(lucius).Visible,Is.True);
                var normalKey=model.Attack.animationKey;
                float healthBefore=playerHealth.CurrentHealth;
                yield return DuelUntil(()=>model.StateTime>=model.Attack.startup-.055f,60,"Attack never reached the parry window");
                Vector2 towardLucius=(lucius.body.position-(Vector2)fighter.transform.position).normalized;
                fighter.Model.Tick(DuelStep,new CombatInput{aim=towardLucius,parryPressed=true});
                for(int i=0;i<9&&!lucius.LuciusLaunchExposed;i++)
                {
                    yield return null;
                    fighter.Model.Tick(DuelStep,default);
                }
                Assert.That(lucius.LuciusLaunchExposed,Is.True,"A correctly timed parry must expose Lucius to an aerial counter");
                Assert.That(playerHealth.CurrentHealth,Is.EqualTo(healthBefore));
                Assert.That(model.AttackConnected,Is.False);
                Assert.That(AttackWarning(lucius).Visible,Is.False);

                int attackSerial=model.AttackSerial;
                Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Launch),Is.True);
                Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Launch),Is.False,"Launch cannot be chained into another launch");
                for(int i=0;i<6;i++)yield return null;
                Assert.That(lucius.AirHeight,Is.GreaterThan(0));
                Assert.That(model.AttackSerial,Is.EqualTo(attackSerial));
                Assert.That(AttackWarning(lucius).Visible,Is.False);
                for(int i=0;i<10;i++)Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Suspend),Is.True);
                Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Suspend),Is.False,"The juggle cap must stop repeated suspension");
                Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Slam),Is.True);
                yield return DuelUntil(()=>!lucius.Airborne,90,"Aerial counter did not finish on the ground");
                Assert.That(lucius.ReactAir(launch,DarklightAirReaction.Launch),Is.False,"Landing must close the launch window");

                Assert.That(opponent.Receive(lucius.Health.MaxHealth*.42f,Vector2.left),Is.True);
                yield return DuelUntil(()=>lucius.LuciusTransforming,180,"Arconte transition never began after crossing 60% HP");
                Assert.That(lucius.Phase,Is.EqualTo(2));
                Assert.That(opponent.Receive(1,Vector2.left),Is.False,"The transformation must protect its damage gate");
                yield return DuelUntil(()=>!lucius.LuciusTransforming,330,"Arconte transition never released control");
                Assert.That(lucius.LuciusView.appearance,Is.EqualTo(DarklightLuciusAppearance.Arconte));
                PlaceDuelPlayer(lucius,1.12f);
                yield return DuelUntil(()=>model.AttackSerial>attackSerial,150,"Arconte did not resume attacks");
                Assert.That(AttackWarning(lucius).Visible,Is.True);
                var arconteKey=model.Attack.animationKey;
                healthBefore=playerHealth.CurrentHealth;
                yield return DuelUntil(()=>playerHealth.CurrentHealth<healthBefore,90,"An undefended in-range strike dealt no damage");
                Assert.That(model.AttackConnected,Is.True);
                Assert.That(AttackWarning(lucius).Visible,Is.False);

                yield return DuelUntil(()=>model.State==CombatState.Neutral,120,"Arconte did not leave recovery");
                attackSerial=model.AttackSerial;
                Assert.That(opponent.Receive(lucius.Health.MaxHealth*.35f,Vector2.left),Is.True);
                yield return DuelUntil(()=>lucius.LuciusFinalExchange,130,"The final exchange never began below 25% HP");
                Assert.That(lucius.Phase,Is.EqualTo(3));
                Assert.That(model.AttackSerial,Is.EqualTo(attackSerial),"Phase entry must give the player an opening");
                PlaceDuelPlayer(lucius,1.12f);
                yield return DuelUntil(()=>model.AttackSerial>attackSerial,160,"Lucius did not attack in phase three");
                var finalKey=model.Attack.animationKey;
                healthBefore=playerHealth.CurrentHealth;
                yield return DuelUntil(()=>model.StateTime>=model.Attack.startup-.055f,60,"No dodge window in phase three");
                fighter.Model.Tick(DuelStep,new CombatInput{aim=Vector2.left,move=Vector2.down,dodgePressed=true});
                fighter.Model.Tick(.055f,default);
                Assert.That(fighter.Model.Invulnerable,Is.True);
                yield return DuelUntil(()=>model.State==CombatState.Neutral,100,"Lucius never recovered after the dodged strike");
                Assert.That(playerHealth.CurrentHealth,Is.EqualTo(healthBefore));
                Assert.That(model.AttackConnected,Is.False,"A dodge was incorrectly counted as confirmed damage");

                attackSerial=model.AttackSerial;
                PlaceDuelPlayer(lucius,1.12f);
                yield return DuelUntil(()=>model.AttackSerial>attackSerial,145,"Lucius did not resume decisions after missing");
                Assert.That(model.Attack.animationKey,Is.Not.EqualTo(finalKey),"Lucius repeated a strike despite other attacks in range");
                string followupKey=model.Attack.animationKey;
                Assert.That(opponent.Receive(lucius.Health.CurrentHealth+1,Vector2.left),Is.True);
                yield return DuelUntil(()=>model.State==CombatState.Dead,60,"Lucius did not stop after defeat");
                Assert.That(game.CurrentBoss,Is.Null);
                Assert.That(AttackWarning(lucius).Visible,Is.False);
                Assert.That(lucius.body.linearVelocity,Is.EqualTo(Vector2.zero));
                foreach(var audio in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(audio.isPlaying,Is.False);
                TestContext.WriteLine($"AI strikes: Normal={normalKey}, Arconte={arconteKey}, Final={finalKey}, Followup={followupKey}; three phases completed.");
            }
            finally{fighter.enabled=true;Time.captureDeltaTime=previousDelta;}
        }

        [UnityTest]
        public IEnumerator LuciusOnlyDefendsAgainstVisibleAimedThreatsAfterReactionDelay()
        {
            var lucius=StartLuciusTraining();
            var fighter=lab.Player;
            var model=lucius.CombatModel;
            game.Progress.difficulty=2;
            float previousDelta=Time.captureDeltaTime;
            Time.captureDeltaTime=DuelStep;
            fighter.enabled=false;
            GameObject wall=null;
            try
            {
                PlaceDuelPlayer(lucius,1.27f);
                fighter.Model.Tick(DuelStep,new CombatInput{aim=Vector2.right,heavyPressed=true,heavyHeld=true});
                for(int i=0;i<13;i++)yield return null;
                Assert.That(lucius.Guarding||model.State==CombatState.Dodge,Is.False,"Lucius reacted to a charge pointed away from him");

                wall=new GameObject("Lucius reaction test wall");
                wall.transform.position=(lucius.body.position+(Vector2)fighter.transform.position)*.5f;
                wall.AddComponent<BoxCollider2D>().size=new Vector2(.14f,4f);
                Physics2D.SyncTransforms();
                fighter.ResetModel();
                fighter.Model.Tick(DuelStep,new CombatInput{aim=Vector2.left,heavyPressed=true,heavyHeld=true});
                Assert.That(DarklightNavigation.Clear(lucius.body.position,fighter.transform.position),Is.False);
                for(int i=0;i<14;i++)yield return null;
                Assert.That(lucius.Guarding||model.State==CombatState.Dodge,Is.False,"Lucius defended against a threat behind a wall");

                wall.SetActive(false);
                Physics2D.SyncTransforms();
                Assert.That(DarklightNavigation.Clear(lucius.body.position,fighter.transform.position),Is.True);
                for(int i=0;i<5;i++)yield return null;
                Assert.That(lucius.Guarding||model.State==CombatState.Dodge,Is.False,"Lucius defended earlier than the minimum reaction delay");
                yield return DuelUntil(()=>lucius.Guarding||model.State==CombatState.Dodge,17,"Lucius ignored a visible charge aimed at him");
                if(model.State==CombatState.Dodge)
                {
                    int direction=DarklightCharacterView.DirectionIndex(model.Motion);
                    var appearance=lucius.LuciusView.appearance;
                    if(!DarklightLuciusSpriteBank.TryActionPage(appearance,"Dodge",direction,null,out var page))
                        Assert.That(DarklightLuciusSpriteBank.TryActionPage(appearance,"Walk",direction,null,out page),Is.True);
                    Assert.That(page.spriteNames,Does.Contain(lucius.LuciusView.Renderer.sprite.name));
                    TestContext.WriteLine($"Lucius presented {page.action} in dodge travel direction {direction}.");
                }
            }
            finally{if(wall!=null)Object.Destroy(wall);fighter.enabled=true;Time.captureDeltaTime=previousDelta;}
        }

        [UnityTest]
        public IEnumerator LuciusChoosesThrustAtLongReachAndKeepsItsAimLocked()
        {
            var lucius=StartLuciusTraining();
            var fighter=lab.Player;
            var playerHealth=fighter.GetComponent<DarklightCombatant>().Health;
            var model=lucius.CombatModel;
            var sword=lucius.LuciusLoadout.lights;
            float cutReach=Mathf.Max(sword[0].range,sword[1].range);
            float thrustReach=sword[2].range;
            Assert.That(thrustReach,Is.GreaterThan(cutReach));
            float distance=(cutReach+thrustReach)*.5f;
            float previousDelta=Time.captureDeltaTime;
            Time.captureDeltaTime=DuelStep;
            fighter.enabled=false;
            GameObject wall=null;
            try
            {
                PlaceDuelPlayer(lucius,distance);
                wall=new GameObject("Lucius thrust test wall");
                wall.transform.position=(lucius.body.position+(Vector2)fighter.transform.position)*.5f;
                wall.AddComponent<BoxCollider2D>().size=new Vector2(.14f,8f);
                Physics2D.SyncTransforms();
                for(int i=0;i<63;i++)yield return null;
                Assert.That(model.AttackSerial,Is.Zero,"Lucius committed to an attack while separated by a wall");

                wall.SetActive(false);
                PlaceDuelPlayer(lucius,distance);
                yield return DuelUntil(()=>model.State==CombatState.Attack,130,"Lucius did not engage from thrust-only range");
                Assert.That(model.Attack.animationKey,Is.EqualTo(DarklightLuciusCombatProfile.SwordThrust));
                Assert.That(AttackWarning(lucius).Radius,Is.EqualTo(thrustReach));
                Assert.That(AttackWarning(lucius).GetComponent<SpriteRenderer>().sprite.texture.name,Does.EndWith("_35"));
                Vector2 initialAim=model.Facing;
                Vector2 sidestep=Quaternion.Euler(0,0,67)*initialAim;
                Move(lucius.body.position+sidestep*distance);
                float healthBefore=playerHealth.CurrentHealth;
                yield return DuelUntil(()=>model.State==CombatState.Neutral,120,"The missed thrust never ended");
                Assert.That(model.Facing,Is.EqualTo(initialAim));
                Assert.That(playerHealth.CurrentHealth,Is.EqualTo(healthBefore));
                Assert.That(model.AttackConnected,Is.False);

                int serial=model.AttackSerial;
                PlaceDuelPlayer(lucius,distance);
                for(int i=0;i<12;i++)yield return null;
                Assert.That(model.AttackSerial,Is.EqualTo(serial),"Lucius cut short the opening after a clean miss");
                Assert.That(lucius.body.linearVelocity,Is.EqualTo(Vector2.zero));
                yield return DuelUntil(()=>playerHealth.CurrentHealth<healthBefore,160,"A second thrust on a stationary target did not connect");
                Assert.That(model.Attack.animationKey,Is.EqualTo(DarklightLuciusCombatProfile.SwordThrust));
                Assert.That(model.AttackConnected,Is.True);
                TestContext.WriteLine($"Cuts max reach={cutReach:F2}; thrust reach={thrustReach:F2}; tested at {distance:F2} units with a wall and a 67-degree sidestep.");
            }
            finally{if(wall!=null)Object.Destroy(wall);fighter.enabled=true;Time.captureDeltaTime=previousDelta;}
        }
        [UnityTest] public IEnumerator AllSixAlliesCanBeTalkedToAndCollectedItemsCannotBeClaimedAgain()
        {
            string save=File.Exists(DarklightProgress.Path)?File.ReadAllText(DarklightProgress.Path):null;
            for(int chapter=0;chapter<6;chapter++)
            {
                game.PreviewChapter(chapter);Move(game.Map.SafeSpawn(game.Map.Ally));
                Assert.That(game.TryGetInteraction(out var target),Is.True,"Ally missing at chapter "+chapter);Assert.That(target.Id,Is.EqualTo("ally"));var npc=GameObject.Find(chapter==1?"IcaroNox":chapter==3||chapter==4?"KaelAliado":"LyraMoreau");Assert.That((Vector2)npc.transform.position,Is.EqualTo(target.Point),"Interaction point must match visible NPC");
                Assert.That(game.TryInteract(),Is.True);Assert.That(game.Active,Is.False,"Dialogue did not pause");Assert.That(lab.Paused,Is.True);
                game.PreviewChapter(chapter);Move(game.Map.SafeSpawn(game.Map.Relic));int shards=game.Progress.shards;
                Assert.That(game.TryGetInteraction(out target),Is.True);Assert.That(target.Id,Is.EqualTo("relic"));Assert.That(game.TryInteract(),Is.True);
                Assert.That(game.Progress.shards,Is.EqualTo(shards+1));Assert.That(game.Progress.relics[chapter],Is.True);
                Assert.That(game.TryGetInteraction(out target)&&target.Id=="relic",Is.False,"Collected item stayed active");
                yield return null;
            }
            Assert.That(File.Exists(DarklightProgress.Path)?File.ReadAllText(DarklightProgress.Path):null,Is.EqualTo(save));
        }
        [UnityTest] public IEnumerator MouseInteractionNeverAttacksAndWallsPreventRemoteConversation()
        {
            Move(game.Map.SafeSpawn(game.Map.Ally));yield return null;Canvas.ForceUpdateCanvases();
            var button=lab.PixelHUD.transform.Find("HUD640x360/BotaoInteragir") as RectTransform;Assert.That(button.gameObject.activeSelf,Is.True);
            var mouse=InputSystem.AddDevice<Mouse>();try{
                int serial=lab.Player.Model.AttackSerial;Vector2 screen=RectTransformUtility.WorldToScreenPoint(null,button.TransformPoint(button.rect.center));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=screen}.WithButton(MouseButton.Left));yield return null;yield return null;
                Assert.That(game.Active,Is.False);Assert.That(lab.Player.Model.AttackSerial,Is.EqualTo(serial));
            }finally{InputSystem.RemoveDevice(mouse);}
            game.PreviewChapter(0);Vector2 npc=game.Map.SafeSpawn(game.Map.Ally);Move(game.Map.SafeSpawn(npc+Vector2.down));
            var wall=new GameObject("ParedeTeste").AddComponent<BoxCollider2D>();wall.transform.position=(npc+(Vector2)lab.Player.transform.position)*.5f;wall.size=new Vector2(.4f,.2f);Physics2D.SyncTransforms();
            Assert.That(game.TryGetInteraction(out var target)&&target.Id=="ally",Is.False,"Interaction crossed a wall");Object.Destroy(wall.gameObject);
        }
        [UnityTest] public IEnumerator AreaThreatRequiresEvadingAndWeaponsKeepTheirDifferentCounters()
        {
            var actor=lab.Player.GetComponent<DarklightCombatant>();var hp=actor.Health;hp.SetInvincibilityDuration(0);lab.Player.ResetModel();
            lab.Player.Model.Tick(.01f,new CombatInput{parryPressed=true,aim=Vector2.down});float before=hp.CurrentHealth;
            Assert.That(actor.Receive(5,Vector2.up,true),Is.True);Assert.That(hp.CurrentHealth,Is.LessThan(before));
            yield return new WaitForSeconds(.3f);lab.Player.ResetModel();lab.Player.Model.Tick(.01f,new CombatInput{dodgePressed=true});lab.Player.Model.Tick(.08f,default);before=hp.CurrentHealth;
            Assert.That(actor.Receive(5,Vector2.up,true),Is.False);Assert.That(hp.CurrentHealth,Is.EqualTo(before));
            var daggers=game.weaponDefinitions[1].loadout;var chain=game.weaponDefinitions[2].loadout;
            Assert.That(daggers.counter.hitPulses,Is.EqualTo(3));Assert.That(chain.counter.pullTarget,Is.True);Assert.That(chain.counter.range,Is.GreaterThan(daggers.counter.range));
            Assert.That(daggers.lights[0].recoveryChainFraction,Is.LessThan(chain.lights[0].recoveryChainFraction));
            var warning=new GameObject("AvisoTeste").AddComponent<DarklightAttackWarning>();warning.Show(Vector2.zero,Vector2.up,2.5f,DarklightThreat.Area,.5f);
            Assert.That(warning.GetComponent<SpriteRenderer>().bounds.size.x,Is.EqualTo(5).Within(.001f));warning.Hide();Assert.That(warning.Visible,Is.False);Object.Destroy(warning.gameObject);
        }
        [UnityTest] public IEnumerator LessonsTrackSuccessfulActionsAndMapSelectDoesNotOpenMP3()
        {
            lab.Coach.Restart();lab.Coach.Record(DarklightLesson.Strike);lab.Coach.Record(DarklightLesson.Strike);Assert.That(lab.Coach.Learned,Is.Zero);lab.Coach.Record(DarklightLesson.Strike);Assert.That(lab.Coach.Learned,Is.EqualTo(1));
            lab.SetPaused(true);lab.Coach.Record(DarklightLesson.Parry);Assert.That(lab.Coach.Learned,Is.EqualTo(1));lab.SetPaused(false);lab.Coach.Record(DarklightLesson.Parry);lab.Coach.Record(DarklightLesson.Dodge);Assert.That(lab.Coach.Learned,Is.EqualTo(7));
            var pad=InputSystem.AddDevice<Gamepad>();try{InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Select));yield return null;yield return null;Assert.That(game.Atlas.IsOpen,Is.True);Assert.That(lab.Input.Action("MP3").IsPressed(),Is.False);}finally{InputSystem.RemoveDevice(pad);}
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
    }
}


