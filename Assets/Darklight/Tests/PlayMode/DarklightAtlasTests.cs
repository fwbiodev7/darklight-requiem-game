using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightAtlasTests
    {
        DarklightCampaign game;DarklightCombatLab lab;
        InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        [UnitySetUp] public IEnumerator Prepare()
        {
            DarklightAutomation.Silent=true;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;game.Atlas.RefreshNow();
        }
        [UnityTearDown] public IEnumerator Restore(){InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;yield return null;}
        static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);
        static void Invoke(object o,string n,params object[] args)=>o.GetType().GetMethod(n,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(o,args);
        static Vector2 ClickPoint(RectTransform r)=>RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center));
        [UnityTest] public IEnumerator MouseKeyboardAndPadOperateMapAndNeverLeakGameplayInput()
        {
            var a=game.Atlas;var mouse=InputSystem.AddDevice<Mouse>();var keyboard=InputSystem.AddDevice<Keyboard>();var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                Canvas.ForceUpdateCanvases();var mini=lab.PixelHUD.transform.Find("HUD640x360/Minimapa") as RectTransform;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=ClickPoint(mini),buttons=1});yield return null;yield return null;
                Assert.That(a.IsOpen,Is.True);Assert.That(lab.Paused,Is.True);Assert.That(lab.Input.Gameplay.enabled,Is.False);
                var p=lab.Player.transform.position;float clock=lab.Player.Model.Clock;yield return new WaitForSecondsRealtime(.12f);
                Assert.That(lab.Player.transform.position,Is.EqualTo(p));Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));
                InputSystem.QueueStateEvent(mouse,new MouseState());InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));yield return null;yield return null;
                Assert.That(a.IsOpen,Is.False);Assert.That(lab.Paused,Is.False);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Tab));yield return null;yield return null;Assert.That(a.IsOpen,Is.True);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                float originalZoom=a.Zoom;InputSystem.QueueStateEvent(pad,new GamepadState{rightTrigger=1});yield return new WaitForSecondsRealtime(.12f);Assert.That(a.Zoom,Is.GreaterThan(originalZoom));
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
                Assert.That(a.TrackedObjective,Is.Not.Empty);InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                a.SetWaypoint(lab.Player.transform.position);Assert.That(a.Snapshot(),Does.Contain("\"hasWaypoint\":true"));
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.North));yield return null;yield return null;
                Assert.That(a.Snapshot(),Does.Not.Contain("\"hasWaypoint\":true"));
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.East));yield return null;yield return null;
                Assert.That(a.IsOpen,Is.False);InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.Select));yield return null;yield return null;
                Assert.That(a.IsOpen,Is.True,"SELECT failed");Assert.That(Field<string>(game,"panel"),Is.EqualTo("Map"),"SELECT opened MP3 instead");
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;a.CloseMap();game.PreviewPanel("Pause");a.TryOpen();a.CloseMap();
                Assert.That(Field<string>(game,"panel"),Is.EqualTo("Pause"));Assert.That(lab.Paused,Is.True);
                // The existing remapping mechanism also applies to the new map action.
                lab.Input.Menu("Map").ApplyBindingOverride(0,"<Keyboard>/k");InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.K));yield return null;yield return null;
                Assert.That(a.IsOpen,Is.True);lab.Input.Menu("Map").RemoveAllBindingOverrides();
            }
            finally {InputSystem.RemoveDevice(mouse);InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(pad);}
        }
        [UnityTest] public IEnumerator AllTenPhasesKeepDiscoveryAndMapCoordinatesWithoutChangingCampaignSave()
        {
            string saved=File.Exists(DarklightProgress.Path)?File.ReadAllText(DarklightProgress.Path):null;
            for(int n=0;n<10;n++)
            {
                if(n<6)game.PreviewChapter(n);else game.ExploreRegion(n);yield return null;var a=game.Atlas;a.RefreshNow();
                Assert.That(a.Available,Is.True);Assert.That(a.Known(lab.Player.transform.position),Is.True);
                Assert.That(a.DiscoveredCount,Is.GreaterThan(0));Assert.That(a.DiscoveredCount,Is.LessThan(game.Map.GridBounds.width*game.Map.GridBounds.height/3));
                var grid=game.Map.GridBounds;var center=new Vector2(grid.x+grid.width*.5f,grid.y+grid.height*.5f)*game.Map.CellSize;
                Assert.That(Vector2.Distance(a.WorldToMini(center),(lab.PixelHUD.transform.Find("HUD640x360/Minimapa/Territorio") as RectTransform).rect.size*.5f),Is.LessThan(.01f));
                Assert.That(a.TryOpen(),Is.True);Assert.That(lab.Paused,Is.True);a.SetZoom(20);Assert.That(a.Zoom,Is.EqualTo(4));a.SetZoom(-1);Assert.That(a.Zoom,Is.EqualTo(.65f));a.CloseMap();
            }
            game.PreviewChapter(0);yield return null;Assert.That(game.Atlas.DiscoveredCount,Is.GreaterThan(0));
            Assert.That(File.Exists(DarklightProgress.Path)?File.ReadAllText(DarklightProgress.Path):null,Is.EqualTo(saved));
            var empty=new GameObject("SemMapa").AddComponent<DarklightAtlas>();Assert.That(empty.TryOpen(),Is.False);Assert.That(empty.Available,Is.False);Object.Destroy(empty.gameObject);
        }
        [UnityTest] public IEnumerator EnemiesNeedDetectionAndDefeatedEnemiesDisappearWithoutSpoilerBossOrExit()
        {
            var a=game.Atlas;game.PreviewPanel("Pause");var markers=new List<DarklightMapPoint>();game.FillMapObjectives(markers);
            Assert.That(markers.Exists(x=>x.symbol==DarklightMapSymbol.Exit||x.symbol==DarklightMapSymbol.Boss),Is.False);
            Vector2 p=lab.Player.transform.position;
            Invoke(game,"Spawn",RequiemEnemyKind.Crawler,p,false);yield return null;a.RefreshNow();Assert.That(a.VisibleEnemyCount,Is.EqualTo(1));
            var enemy=Object.FindAnyObjectByType<DarklightEnemy>();enemy.GetComponent<HealthSystem>().TakeDamage(10000);yield return null;a.RefreshNow();Assert.That(a.VisibleEnemyCount,Is.Zero);
            Invoke(game,"Spawn",RequiemEnemyKind.Lucien,game.Map.Arenas[3],false);yield return null;a.RefreshNow();Assert.That(a.VisibleEnemyCount,Is.Zero,"Unseen boss was leaked");
            Assert.That(a.SetWaypoint(game.Map.Arenas[3]),Is.False,"Unknown destination was accepted");
        }
        [UnityTest] public IEnumerator FogWaypointTrackingAndAreaMemoriesRoundTripAndIdleUpdatesStayCached()
        {
            var a=game.Atlas;a.TryOpen();a.TrackFocused();Assert.That(a.SetWaypoint(lab.Player.transform.position),Is.True);a.CloseMap();
            string snapshot=a.Snapshot();var decoded=DarklightAtlasState.Decode(snapshot);Assert.That(decoded,Is.Not.Null);
            var record=decoded.regions.Find(r=>r.id==0);Assert.That(record.hasWaypoint,Is.True);Assert.That(record.tracked,Is.Not.Empty);
            var seen=DarklightAtlasState.DecodeFog(record.fog,record.width*record.height);int count=0;foreach(bool v in seen)if(v)count++;Assert.That(count,Is.EqualTo(a.DiscoveredCount));
            string path=Path.Combine(Application.temporaryCachePath,"requiem-map-validation.json");decoded.Save(path);var loaded=DarklightAtlasState.Load(path);Assert.That(JsonUtility.ToJson(loaded),Is.EqualTo(snapshot));
            File.WriteAllText(path,"broken json");decoded.Save(path);Assert.That(DarklightAtlasState.Load(path).regions.Count,Is.GreaterThan(0));
            game.ExploreRegion(6);yield return null;a.RememberAreas(6,new[]{true,false,true,false});Assert.That(a.SavedAreas(6,4),Is.EqualTo(new[]{true,false,true,false}));
            game.ExploreRegion(7);yield return null;game.ExploreRegion(6);yield return null;Assert.That(game.VisitedRegionAreas,Is.GreaterThanOrEqualTo(2));
            game.PreviewPanel("Pause");int uploads=a.TerrainUploads,queries=a.RevealQueries;yield return new WaitForSecondsRealtime(.25f);
            Assert.That(a.RevealQueries,Is.EqualTo(queries));Assert.That(a.TerrainUploads,Is.LessThanOrEqualTo(uploads+1));
            foreach(var source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))Assert.That(source.isPlaying,Is.False);
        }
    }
}

