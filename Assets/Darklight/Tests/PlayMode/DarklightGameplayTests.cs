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
            typeof(DarklightEnemy).GetMethod("Perform",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(enemy,new object[]{game,lab,1.45f,false});Assert.That(actor.Health.CurrentHealth,Is.EqualTo(hp));Object.Destroy(wall.gameObject);yield return null;
            Physics2D.SyncTransforms();typeof(DarklightEnemy).GetMethod("Perform",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(enemy,new object[]{game,lab,1.45f,false});Assert.That(actor.Health.CurrentHealth,Is.LessThan(hp));
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


