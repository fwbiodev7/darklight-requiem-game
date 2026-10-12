using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightSceneTests
    {
        [UnitySetUp] public IEnumerator LoadLab()
        {
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCombatLab.unity");
            yield return null;
            DarklightCombatLab.Instance.SetPaused(false);
            yield return null; yield return null;
        }
        [UnityTest] public IEnumerator SceneLoadsTheAuthoredSpritesAndDedicatedActions()
        {
            var lab=DarklightCombatLab.Instance;
            Assert.That(lab.Targets.Length,Is.EqualTo(3));
            var view=lab.Player.GetComponent<DarklightCharacterView>();
            Assert.That(view.walk.Length,Is.EqualTo(32));Assert.That(view.slash.Length,Is.EqualTo(32));Assert.That(view.idle.Length,Is.EqualTo(8));
            Assert.That(lab.Input.Asset.name,Is.EqualTo("DarklightInputs"));
            Assert.That(lab.WorldCamera.targetTexture.width,Is.EqualTo(1280));
            Assert.That(lab.WorldCamera.targetTexture.height,Is.EqualTo(720));
            yield return null;
        }
        [UnityTest] public IEnumerator KatanaDamagesOneTargetOnlyOnceWithDuplicateHurtboxes()
        {
            var lab=DarklightCombatLab.Instance; var target=lab.Targets[0];
            var extra=target.gameObject.AddComponent<CircleCollider2D>();extra.radius=.4f;extra.isTrigger=true;
            lab.Player.GetComponent<Rigidbody2D>().position=(Vector2)target.transform.position-Vector2.right;
            Physics2D.SyncTransforms();
            lab.Player.Model.Tick(.01f,new CombatInput { lightPressed=true,aim=Vector2.right });
            yield return new WaitForSeconds(.3f);
            Assert.That(target.Health.CurrentHealth,Is.EqualTo(136).Within(.001f));
            Assert.That(lab.Targets[1].Health.CurrentHealth,Is.EqualTo(150));
        }
        [UnityTest] public IEnumerator PauseFreezesCombatAndResetRestoresHealthAndTargets()
        {
            var lab=DarklightCombatLab.Instance;lab.Targets[0].GetComponent<DarklightCombatant>().Receive(20,Vector2.zero);
            lab.Player.Model.Tick(.01f,new CombatInput { lightPressed=true });lab.SetPaused(true);
            float clock=lab.Player.Model.Clock;
            yield return new WaitForSeconds(.15f);
            Assert.That(lab.Player.Model.Clock,Is.EqualTo(clock));Assert.That(lab.Input.Gameplay.enabled,Is.False);
            lab.ResetEncounter();yield return null;yield return null;
            Assert.That(DarklightCombatLab.Instance.Targets[0].Health.CurrentHealth,Is.EqualTo(150));
            Assert.That(DarklightCombatLab.Instance.Player.GetComponent<HealthSystem>().CurrentHealth,Is.EqualTo(100));
        }
        [UnityTest] public IEnumerator PixelHudTracksDamageCooldownAndRestoredEncounter()
        {
            var lab=DarklightCombatLab.Instance;var hud=lab.PixelHUD;
            Assert.That(hud,Is.Not.Null);
            lab.Player.GetComponent<HealthSystem>().TakeDamage(10);
            lab.Targets[0].GetComponent<DarklightCombatant>().Receive(20,Vector2.zero);
            lab.Player.Model.Tick(.01f,new CombatInput { skillPressed=true });
            yield return null;yield return null;
            Assert.That(hud.healthText.text,Is.EqualTo("90 / 100"));
            Assert.That(hud.healthFill.fillAmount,Is.EqualTo(.9f).Within(.001f));
            Assert.That(hud.targetFills[0].fillAmount,Is.EqualTo(130f/150f).Within(.001f));
            Assert.That(hud.skillFill.fillAmount,Is.LessThan(1));
            lab.ResetEncounter();yield return null;yield return null;yield return null;
            Assert.That(DarklightCombatLab.Instance.PixelHUD.healthFill.fillAmount,Is.EqualTo(1));
        }
    }
}
