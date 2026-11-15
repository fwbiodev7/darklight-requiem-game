using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed class DarklightIntegrationFixesTests
    {
        [UnityTest]
        public IEnumerator FiveAppearancesAndGothicTrainingLoadThroughGameplay()
        {
            using var save = DarklightProgress.IsolateStorageForTests(
                System.IO.Path.GetFullPath("work/codex-fixes-20261113/test-save.json"));
            DarklightAutomation.Silent = true;
            yield return SceneManager.LoadSceneAsync("Assets/Darklight/Scenes/DarklightCampaign.unity");
            yield return null;
            yield return DarklightIntegrationFixesCheck.Run("work/codex-fixes-20261113/editor");
            Assert.That(System.IO.File.Exists("work/codex-fixes-20261113/editor/runtime.json"), Is.True);
        }
    }
}
