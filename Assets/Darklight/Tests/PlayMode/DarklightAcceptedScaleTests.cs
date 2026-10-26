using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed partial class DarklightAbyssPrototypeTests
    {
        [UnityTest]public IEnumerator CaptureAcceptedLiftAtTheStandingCharacterScaleInGameplay()
        {
            static string folder => DarklightTestOutput.GetDirectory("cavaleiro-final-20261016/anatomy-correction/gameplay");
            Directory.CreateDirectory(folder);var view=fighter.GetComponent<DarklightCharacterView>();var art=fighter.transform.Find("FabioVisual").GetComponent<SpriteRenderer>();
            foreach(var enemy in game.MapEnemies){enemy.enabled=false;enemy.transform.position=(Vector3)game.Map.Spawn+Vector3.right*6;enemy.GetComponent<Rigidbody2D>().position=enemy.transform.position;}
            fighter.Model.Tick(.01f,new CombatInput{aim=Vector2.down});view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");
            float neutralHeight=art.sprite.bounds.size.y*art.transform.lossyScale.y;Vector3 foot=art.transform.position;
            var follow=lab.WorldCamera.GetComponent<DarklightLabCamera>();follow.enabled=false;lab.WorldCamera.transform.position=(Vector3)body.position+new Vector3(0,.75f,-10);
            DarklightNativeCapture.Screen(folder+"01-standing-saws.png");
            StepExpanded(.01f,new CombatInput{skillPressed=true,aim=Vector2.down});StepExpanded(.35f,new CombatInput{aim=Vector2.down});
            view.Present(fighter.Model,Vector2.zero);Call(view,"LateUpdate");
            Assert.That(art.sprite.name,Is.EqualTo("Abyss_CorrectedNormalSlamRaiseS_0_0"),"The accepted pose must be selected by real slam timing");
            Assert.That(art.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(art.sprite.pixelsPerUnit,Is.EqualTo(381));
            float bodyHeight=740f/art.sprite.pixelsPerUnit*art.transform.lossyScale.y;
            Assert.That(bodyHeight/neutralHeight,Is.InRange(.95f,1.05f),"Compare body crown-to-sole, excluding overhead equipment");
            Assert.That(Vector3.Distance(foot,art.transform.position),Is.LessThan(.001f),"Boot contact must remain grounded");
            DarklightNativeCapture.Screen(folder+"02-accepted-lift.png");
            File.WriteAllText(folder+"scale-evidence.json","{\"normalSouthLiftIntegrated\":true,\"bodyWorldHeight\":"+bodyHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"standingWorldHeight\":"+neutralHeight.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"scaleRatio\":"+(bodyHeight/neutralHeight).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"ppu\":381,\"unchangedCollision\":true}");
            follow.enabled=true;yield return null;
        }
    }
}
