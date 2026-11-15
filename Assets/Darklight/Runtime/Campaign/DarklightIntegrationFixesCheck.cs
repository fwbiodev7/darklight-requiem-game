using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Darklight
{
    // Runs only when requested by the focused tests or the standalone QA flag.
    public sealed class DarklightIntegrationFixesCheck : MonoBehaviour
    {
        [Serializable] public sealed class Receipt
        {
            public bool passed, gothicArena, gothicMinimap, finalBossBar, spawnClear, movementAccepted, arconteAtSixty;
            public string[] appearances;
            public float[] spriteHeights;
            public string error;
        }
        static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
        static void SelectForm(DarklightCampaign game, int form)
            => typeof(DarklightCampaign).GetMethod("SetTrainingForm", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(game, new object[] { form });

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--darklight-integration-check");
            if (index < 0 || index + 1 >= args.Length) return;
            var check = new GameObject("IntegrationFixesCheck").AddComponent<DarklightIntegrationFixesCheck>();
            check.StartCoroutine(check.Standalone(args[index + 1]));
        }
        IEnumerator Standalone(string output)
        {
            Require(DarklightProgress.HasIsolatedTestStorage, "Standalone QA requires --darklight-isolated-session.");
            DarklightAutomation.Silent = true;
            AudioListener.volume = 0;
            yield return Run(output);
            Application.Quit(0);
        }
        public static IEnumerator Run(string output)
        {
            Directory.CreateDirectory(output);
            var result = new Receipt();
            var names = new List<string>();
            var heights = new List<float>();
            DarklightAutomation.Silent = true;
            float deadline = Time.realtimeSinceStartup + 20;
            while (DarklightCampaign.Instance == null && Time.realtimeSinceStartup < deadline) yield return null;
            var game = DarklightCampaign.Instance;
            var lab = DarklightCombatLab.Instance;
            Require(game != null && lab != null, "Campaign and combat lab must load.");
            game.EnterTraining();
            yield return null;
            var floor = GameObject.Find("ArenaDeTreinamento");
            Require(floor != null, "Training root was not created.");
            var ground = floor.transform.Find("MapaIlustrado").GetComponent<SpriteRenderer>();
            result.gothicArena = ground.sprite == Resources.Load<Sprite>("Darklight/Maps/TrainingCourtyard");
            Require(result.gothicArena, "Training is displaying another map.");
            game.Atlas.RefreshNow();
            var atlasTexture=typeof(DarklightAtlas).GetField("activeMicroart",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game.Atlas);
            result.gothicMinimap=atlasTexture==ground.sprite.texture;
            Require(result.gothicMinimap,"Training minimap still displays the chapter map.");
            var start = lab.Player.GetComponent<Rigidbody2D>().position;
            result.spawnClear = true;
            foreach (var collider in Physics2D.OverlapCircleAll(start, .28f))
                if (collider.transform.IsChildOf(floor.transform) && !collider.isTrigger)
                    result.spawnClear = false;
            Require(result.spawnClear, "A map collider blocks the player spawn.");

            // Exercise the same model/physics entry point used by gameplay.
            game.RestartLuciusDuel();
            typeof(DarklightCampaign).GetProperty("TrainingDamage").SetValue(game, false);
            lab.SetPaused(false);
            var body = lab.Player.GetComponent<Rigidbody2D>();
            var checkStart = body.position;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
                InputSystem.Update();
                yield return new WaitForSeconds(.15f);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
            }
            finally { InputSystem.RemoveDevice(keyboard); }
            result.movementAccepted = Vector2.Distance(body.position, checkStart) > .005f;
            Require(result.movementAccepted, "The player cannot move inside the arena.");
            lab.Player.ResetModel();

            var view = lab.Player.GetComponent<DarklightCharacterView>();
            var renderer = lab.Player.GetComponentInChildren<SpriteRenderer>();
            string previous = null;
            for (int form = 0; form <= 2; form++)
            {
                SelectForm(game, form);
                deadline = Time.realtimeSinceStartup + 5;
                while (game.Transforming && Time.realtimeSinceStartup < deadline) yield return null;
                Require(game.PresentedForm == form, "Requested Fabius form never became active.");
                lab.SetPaused(true);
                view.Present(lab.Player.Model, Vector2.zero);
                string name = renderer.sprite != null ? renderer.sprite.name : "";
                Require(name.Length > 0 && name != previous, "Fabius transformation did not change the selected sprite.");
                if (form > 0) Require(name.StartsWith(form == 1 ? "FabiusRuptura" : "FabiusSerafim"), "Wrong transformed identity.");
                float height = renderer.sprite.rect.height / renderer.sprite.pixelsPerUnit;
                if (form > 0) Require(height / heights[0] > .8f && height / heights[0] < 1.3f,
                    "Fabius transformed scale differs from Normal.");
                heights.Add(height);
                names.Add(name); previous = name;
                DarklightNativeCapture.Screen(Path.Combine(output, "fabius-" + form + ".png"));
                lab.SetPaused(false);
            }
            var lucius = game.CurrentBoss;
            Require(lucius != null && lucius.IsLucius, "TESTAR BOSSFIGHT did not spawn Lucius.");
            lab.SetPaused(true);
            Require(lucius.LuciusView.PresentPose("Idle", 0, 0), "Lucius Normal failed to load.");
            names.Add(lucius.LuciusView.Renderer.sprite.name);
            heights.Add(lucius.LuciusView.Renderer.sprite.rect.height / lucius.LuciusView.Renderer.sprite.pixelsPerUnit);
            DarklightNativeCapture.Screen(Path.Combine(output, "lucius-normal.png"));
            lucius.Health.TakeDamage(lucius.Health.MaxHealth * .41f);
            lab.SetPaused(false);
            deadline = Time.realtimeSinceStartup + 5;
            while (lucius.LuciusView.appearance != DarklightLuciusAppearance.Arconte &&
                Time.realtimeSinceStartup < deadline) yield return null;
            result.arconteAtSixty = lucius.LuciusView.appearance == DarklightLuciusAppearance.Arconte;
            Require(result.arconteAtSixty, "Lucius did not enter Arconte below 60% HP.");
            yield return new WaitForSeconds(1);
            lab.SetPaused(true);
            Require(lucius.LuciusView.PresentPose("Idle", 0, 0), "Arconte failed to load.");
            names.Add(lucius.LuciusView.Renderer.sprite.name);
            heights.Add(lucius.LuciusView.Renderer.sprite.rect.height / lucius.LuciusView.Renderer.sprite.pixelsPerUnit);
            DarklightNativeCapture.Screen(Path.Combine(output, "lucius-arconte.png"));
            Require(heights[4] / heights[3] > .8f && heights[4] / heights[3] < 1.25f, "Arconte scale differs from Lucius Normal.");
            result.spriteHeights = heights.ToArray();
            var bossFrame=lab.PixelHUD.transform.Find("HUD640x360/LuciusFinalBoss");
            result.finalBossBar=bossFrame!=null&&bossFrame.gameObject.activeInHierarchy;
            Require(result.finalBossBar,"Lucius final boss bar is hidden.");
            result.appearances = names.ToArray();
            result.passed = true;
            File.WriteAllText(Path.Combine(output, "runtime.json"), JsonUtility.ToJson(result, true));
        }
    }
}
