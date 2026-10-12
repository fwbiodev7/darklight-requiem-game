using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
 public sealed class DarklightMapDetailTests
 {
  [UnityTest] public IEnumerator NativeDetailsPreserveWorldCoordinatesForegroundAndNavigation()
  {
   DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
   var game=DarklightCampaign.Instance;game.PreviewChapter(0);yield return null;
   var details=Object.FindFirstObjectByType<DarklightMapDetails>();Assert.That(details,Is.Not.Null);
   Assert.That(details.Data.chunks.Length,Is.GreaterThanOrEqualTo(2));
   foreach(var c in details.Data.chunks){var sprite=Resources.Load<Sprite>(c.asset);Assert.That(sprite,Is.Not.Null);Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(c.width*c.scale,c.height*c.scale)));Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(sprite.texture.mipmapCount,Is.EqualTo(1));
    var obj=GameObject.Find("TrechoDetalhado_"+c.x+"_"+c.y);var bounds=obj.GetComponent<SpriteRenderer>().bounds;
    Assert.That(bounds.size.x,Is.EqualTo(c.width/game.Map.Art.pixelsPerUnit).Within(.001));Assert.That(bounds.size.y,Is.EqualTo(c.height/game.Map.Art.pixelsPerUnit).Within(.001));
    Assert.That(Vector2.Distance(bounds.center,game.Map.Art.World(new Vector2(c.x+c.width*.5f,c.y+c.height*.5f))),Is.LessThan(.001));
   }
   var source=Resources.Load<Sprite>("Darklight/Maps/map1");
   var testRect=new Rect(740,720,150,140);float area=0;
   foreach(var part in details.Partition(testRect)){area+=part.width*part.height;var sprite=details.Foreground(source,game.Map,part);Assert.That(sprite.bounds.size.x,Is.EqualTo(part.width/game.Map.Art.pixelsPerUnit).Within(.001));Assert.That(sprite.bounds.size.y,Is.EqualTo(part.height/game.Map.Art.pixelsPerUnit).Within(.001));Assert.That(sprite.pivot.y,Is.Zero);Object.Destroy(sprite);}
   Assert.That(area,Is.EqualTo(testRect.width*testRect.height).Within(.001));
   var lab=DarklightCombatLab.Instance;Assert.That(lab.WorldCamera.orthographicSize,Is.EqualTo(4.96875f).Within(.001));
   Assert.That(720f/(lab.WorldCamera.orthographicSize*2)/game.Map.Art.pixelsPerUnit,Is.EqualTo(4f).Within(.001));
   var path=new System.Collections.Generic.List<Vector2>();foreach(var p in new[]{game.Map.Ally,game.Map.Relic,game.Map.Portal})Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,game.Map.SafeSpawn(p),path),Is.True);
   game.PreviewChapter(1);yield return null;Assert.That(Object.FindFirstObjectByType<DarklightMapDetails>(),Is.Not.Null,"Next region must load its own details");
  }
  [UnityTest] public IEnumerator EveryRegionHasFourRegisteredNativeAreasAndWalkableSeams()
  {
   DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
   var game=DarklightCampaign.Instance;
   for(int n=0;n<10;n++){
    if(n<6)game.PreviewChapter(n);else game.ExploreRegion(n);yield return null;
    var detail=Object.FindFirstObjectByType<DarklightMapDetails>();Assert.That(detail,Is.Not.Null,"Missing region "+n);int baseCount=0;float covered=0;
    foreach(var c in detail.Data.chunks){var s=Resources.Load<Sprite>(c.asset);Assert.That(s,Is.Not.Null);Assert.That(s.texture.width,Is.EqualTo(c.width*c.scale));Assert.That(s.texture.height,Is.EqualTo(c.height*c.scale));Assert.That(s.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(s.texture.mipmapCount,Is.EqualTo(1));if(c.scale==2){baseCount++;covered+=c.width*c.height;}}
    Assert.That(baseCount,Is.EqualTo(4));Assert.That(covered,Is.EqualTo(game.Map.Art.width*game.Map.Art.height));
    var path=new System.Collections.Generic.List<Vector2>();foreach(var point in new[]{new Vector2(768,512),game.Map.Art.spawn}){var target=game.Map.SafeSpawn(game.Map.Art.World(point));Assert.That(DarklightNavigation.Instance.FindPath(game.Map.Spawn,target,path),Is.True,"No physical route through detailed area "+n);}
   }
   game.PreviewChapter(0);yield return null;yield return null;
   var message=(UnityEngine.UI.Text)typeof(DarklightCampaign).GetField("messageText",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(game);
   Assert.That(message.text,Does.Not.Contain("LER MEMORIAS"),"Exploration instruction leaked into campaign");
  }
 }
}
