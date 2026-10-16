using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace Darklight.Tests
{
    public sealed class DarklightDialogueVisualChecks
    {
        [UnityTest]public IEnumerator RenderActualTexturedDialogueWithApprovedPortraits()
        {
            DarklightAutomation.Silent=true;yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");yield return null;yield return null;
            var game=DarklightCampaign.Instance;game.PreviewChapter(3);yield return null;
            var speak=typeof(DarklightCampaign).GetMethod("Speak",BindingFlags.Instance|BindingFlags.NonPublic);
            var capture=typeof(DarklightCombatCaptureTests).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic);
            speak.Invoke(game,new object[]{new[]{"LYRA|Desative os condutores. Quando Lucien cair, descubra quem alimenta o eclipse."},null});game.AdvanceDialogue();yield return null;
            capture.Invoke(null,new object[]{"dialogue-lyra"});
            speak.Invoke(game,new object[]{new[]{"FÁBIO|Meu sangue não decide o meu futuro. Vou atravessar o Véu e descobrir o que aconteceu em Vespera."},null});game.AdvanceDialogue();yield return null;
            capture.Invoke(null,new object[]{"dialogue-fabio"});
        }
    }
}
