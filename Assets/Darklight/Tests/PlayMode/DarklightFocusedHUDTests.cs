using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightFocusedHUDTests
    {
        const string Folder="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/revision-bugs-20261016-1957/captures/hud/";
        DarklightCampaign game;DarklightCombatLab lab;RectTransform root;
        static T Field<T>(object value,string name)=>(T)value.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(value);
        float Alpha(string name)=>root.Find(name).GetComponent<CanvasGroup>().alpha;
        [UnitySetUp]public IEnumerator Prepare()
        {
            DarklightAutomation.Silent=true;AudioListener.volume=0;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            game=DarklightCampaign.Instance;lab=DarklightCombatLab.Instance;game.PreviewChapter(0);yield return null;lab.SetPaused(true);
            root=Field<RectTransform>(game,"uiRoot");Directory.CreateDirectory(Folder);
        }
        [UnityTest]public IEnumerator ExplorationKeepsHealthAndRightCornerMapWithoutPermanentControlCards()
        {
            yield return new WaitForSecondsRealtime(6.5f);
            Assert.That(lab.PixelHUD.healthText.gameObject.activeInHierarchy,Is.True);Assert.That(lab.PixelHUD.healthFill.gameObject.activeInHierarchy,Is.True);
            var map=Field<RectTransform>(game.Atlas,"miniRoot");Assert.That(map.gameObject.activeInHierarchy,Is.True);
            Assert.That(map.anchoredPosition,Is.EqualTo(new Vector2(512,-8)));Assert.That(map.anchoredPosition.x+map.rect.width,Is.LessThanOrEqualTo(632));
            foreach(var name in new[]{"ArmaHUD","TeclaAtaque","RotuloAtaque","TeclaParry","TeclaEsquiva","MenuHUD","Progresso","TeclaElixir","MP3Compacto"})Assert.That(Alpha(name),Is.LessThan(.01f),name);
            Assert.That(root.Find("DicaDeCombate").gameObject.activeInHierarchy,Is.False);Assert.That(Field<RectTransform>(game.Atlas,"trackedRoot").gameObject.activeInHierarchy,Is.False);
            DarklightNativeCapture.Screen(Folder+"01-exploracao-limpa.png");
            game.PreviewChapter(7);yield return null;lab.SetPaused(true);yield return new WaitForSecondsRealtime(.3f);
            DarklightNativeCapture.Screen(Folder+"02-fase8-contexto-curto.png");
        }
        [UnityTest]public IEnumerator LowHealthAndWeaponChangesShowUsefulContextWhileTrainingRetainsControls()
        {
            var hp=lab.Player.GetComponent<HealthSystem>();hp.TakeDamage(hp.MaxHealth*.7f);yield return new WaitForSecondsRealtime(.3f);
            Assert.That(Alpha("TeclaElixir"),Is.GreaterThan(.9f));Assert.That(Alpha("ElixiresHUD"),Is.GreaterThan(.9f));
            game.Progress.weaponUnlocks=7;lab.SetPaused(false);game.Weapons.Request(1);yield return null;yield return null;yield return new WaitForSecondsRealtime(.3f);
            Assert.That(game.Weapons.Equipped.id,Is.EqualTo(DarklightWeaponId.Daggers));
            Assert.That(Alpha("ArmaHUD"),Is.GreaterThan(.9f));Assert.That(Alpha("TeclaAtaque"),Is.LessThan(.01f));
            DarklightNativeCapture.Screen(Folder+"03-contexto-vida-baixa-e-troca.png");
            yield return new WaitForSecondsRealtime(3.3f);Assert.That(Alpha("ArmaHUD"),Is.LessThan(.01f));
            game.EnterTraining();yield return null;yield return null;game.SelectSkin(1,false);
            typeof(DarklightCampaign).GetMethod("SetTrainingForm",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{0});
            lab.Player.ResetModel();game.Weapons.Request(3);game.Weapons.BeforeTick(.01f,true);yield return null;yield return null;yield return new WaitForSecondsRealtime(.5f);
            File.WriteAllText(Folder+"training-state.txt","active="+game.Active+" training="+game.Training+" paused="+lab.Paused+" skin="+game.SelectedSkin+" weapon="+game.Weapons.Equipped.id+" alpha="+Alpha("TeclaAtaque")+" panel="+Field<string>(game,"panel"));
            DarklightNativeCapture.Screen(Folder+"04-treino-controles-e-engrenagem.png");
            Assert.That(game.Active,Is.True,"HUD training context requires actual gameplay, not a menu");Assert.That(game.AbyssRider.Active,Is.True);Assert.That(Alpha("TeclaAtaque"),Is.GreaterThan(.9f));
            var gear=root.Find("Engrenagens").GetComponent<UnityEngine.UI.Text>();Assert.That(gear.text,Does.Not.Contain("\n"));Assert.That(gear.gameObject.activeInHierarchy,Is.True);
            Assert.That(Field<RectTransform>(game.Atlas,"trackedRoot").gameObject.activeInHierarchy,Is.False,"Campaign objectives must not linger in training");
        }
    }
}
