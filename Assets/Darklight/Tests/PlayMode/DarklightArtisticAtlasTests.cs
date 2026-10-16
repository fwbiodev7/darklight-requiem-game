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
    public sealed class DarklightArtisticAtlasTests
    {
        const string Folder="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/minimapa-reforma-20261015/authorial/captures/";
        DarklightCampaign game;DarklightCombatLab lab;DarklightAtlas atlas;
        InputSettings.BackgroundBehavior background;InputSettings.EditorInputBehaviorInPlayMode editorInput;
        static T Field<T>(object value,string name)=>(T)value.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(value);
        [UnitySetUp]public IEnumerator Prepare()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;atlas=game.Atlas;atlas.RefreshNow();lab.SetPaused(true);Directory.CreateDirectory(Folder);
        }
        [UnityTearDown]public IEnumerator Restore(){InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;yield return null;}
        [UnityTest]public IEnumerator RevealedArtDoesNotRevealUnobservedMemoriesOrFutureBosses()
        {
            Assert.That(atlas.StructureRevealed,Is.True);Assert.That(atlas.MicroartTexture,Is.Not.Null);
            Assert.That(atlas.Known(game.Map.Relic),Is.True,"Map structure is already available");
            var points=Field<List<DarklightMapPoint>>(atlas,"visiblePoints");
            Assert.That(points.Exists(p=>p.id=="fragment"),Is.EqualTo(atlas.Observed(game.Map.Relic)),"Memories retain observation gating even when the terrain is visible");Assert.That(points.Exists(p=>p.symbol==DarklightMapSymbol.Primary),Is.True);
            Assert.That(points.Exists(p=>p.symbol==DarklightMapSymbol.Boss||p.symbol==DarklightMapSymbol.Exit),Is.False);
            Assert.That(atlas.MicroartTexture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(atlas.MicroartTexture.mipmapCount,Is.EqualTo(1));yield return null;
        }
        [UnityTest]public IEnumerator TenActualMapsKeepAspectCoordinatesAndBoundedTextureMemory()
        {
            var measurements=new List<string>();Texture2D previous=null;
            for(int n=0;n<10;n++)
            {
                game.PreviewChapter(n);yield return null;lab.SetPaused(true);atlas.RefreshNow();
                Assert.That(atlas.StructureRevealed,Is.True,"Missing phase "+(n+1));Assert.That(atlas.MicroartTexture.name,Is.EqualTo("Map"+(n+1)));
                if(previous!=null)Assert.That(previous,Is.Not.SameAs(atlas.MicroartTexture));
                var grid=game.Map.GridBounds;var size=new Vector2(grid.width,grid.height)*game.Map.CellSize;
                Assert.That((float)atlas.MicroartTexture.width/atlas.MicroartTexture.height,Is.EqualTo(size.x/size.y).Within(.003f));
                var player=(Vector2)lab.Player.transform.position;var mini=Field<RectTransform>(atlas,"miniViewport");
                Assert.That(Vector2.Distance(atlas.WorldToMini(player),mini.rect.size*.5f),Is.LessThan(.001f));
                foreach(var p in new[]{player,game.Map.Spawn,game.Map.Relic,game.Map.Portal})Assert.That(Vector2.Distance(atlas.MiniToWorld(atlas.WorldToMini(p)),p),Is.LessThan(.001f));
                long bytes=UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(atlas.MicroartTexture);Assert.That(bytes,Is.LessThan(8*1024*1024));
                measurements.Add("{\"phase\":"+(n+1)+",\"textureBytes\":"+bytes+",\"width\":"+atlas.MicroartTexture.width+",\"height\":"+atlas.MicroartTexture.height+"}");
                if(n>=0)
                {
                    atlas.RefreshNow();DarklightNativeCapture.Screen(Folder+"map"+(n+1)+"-hud.png");Assert.That(atlas.TryOpen(),Is.True);
                    yield return new WaitForSecondsRealtime(.16f);DarklightNativeCapture.Screen(Folder+"map"+(n+1)+"-full.png");atlas.CloseMap();lab.SetPaused(true);
                }
                previous=atlas.MicroartTexture;
            }
            File.WriteAllText(Folder+"texture-memory.json","["+string.Join(",",measurements)+"]");
        }
        [UnityTest]public IEnumerator MiniZoomButtonsAndWheelDoNotOpenMapOrLeakAnAttack()
        {
            lab.SetPaused(false);
            var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                var root=Field<RectTransform>(atlas,"miniRoot");var plus=Field<RectTransform>(atlas,"miniZoomIn");Canvas.ForceUpdateCanvases();
                Vector2 at(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
                float zoom=atlas.MiniZoom;int serial=lab.Player.Model.AttackSerial;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=at(root),scroll=new Vector2(0,120)});yield return null;yield return null;
                Assert.That(atlas.MiniZoom,Is.GreaterThan(zoom));Assert.That(atlas.IsOpen,Is.False);
                InputSystem.QueueStateEvent(mouse,new MouseState{position=at(plus)});yield return null;zoom=atlas.MiniZoom;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=at(plus),buttons=1});yield return null;yield return null;
                Assert.That(atlas.MiniZoom,Is.GreaterThan(zoom));Assert.That(atlas.IsOpen,Is.False);Assert.That(lab.Player.Model.AttackSerial,Is.EqualTo(serial));
                Assert.That(root.anchoredPosition.x,Is.EqualTo(512));Assert.That(root.anchoredPosition.y,Is.EqualTo(-8));
                atlas.SetMiniZoom(30);Assert.That(atlas.MiniZoom,Is.EqualTo(3));atlas.SetMiniZoom(-4);Assert.That(atlas.MiniZoom,Is.EqualTo(.65f));
            }
            finally{InputSystem.RemoveDevice(mouse);lab.SetPaused(true);}
        }
        [UnityTest]public IEnumerator UnobservedWalkableDestinationsCanBeMarkedWithoutErasingDiscovery()
        {
            var grid=game.Map.GridBounds;Vector2 destination=Vector2.zero;bool found=false;
            for(int y=grid.yMin;y<grid.yMax&&!found;y++)for(int x=grid.xMin;x<grid.xMax&&!found;x++)
            {
                Vector2 p=(new Vector2(x,y)+Vector2.one*.5f)*game.Map.CellSize;
                if(game.Map.Walkable(new Vector2Int(x,y))&&!atlas.Observed(p)&&atlas.SetWaypoint(p)){destination=p;found=true;}
            }
            Assert.That(found,Is.True);Assert.That(atlas.Observed(destination),Is.False);Assert.That(atlas.Snapshot(),Does.Contain("\"hasWaypoint\":true"));
            atlas.ClearWaypoint();Assert.That(atlas.Snapshot(),Does.Not.Contain("\"hasWaypoint\":true"));yield return null;
        }
    }
}
