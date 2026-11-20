using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Darklight
{
    // Runs only when requested by the focused tests or the standalone QA flag.
    public sealed class DarklightIntegrationFixesCheck : MonoBehaviour
    {
        [Serializable] sealed class LightingReceipt
        {
            public string region, preset, graphics;
            public bool singleMoon, lantern, portal, ladyMuzzleAndHit, contactExpires;
            public int samples, timingSamples;
            public float averageFrameMs, p95FrameMs;
            public double averageCpuFrameMs, averageGpuFrameMs;
        }
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
            Application.runInBackground = true;
            // AfterSceneLoad precedes Campaign.Start, which prepares health, menus and lights.
            yield return null;
            while (!SplashScreen.isFinished) yield return null;
            yield return CheckLighting(output);
            yield return Run(output);
            Application.Quit(0);
        }
        static IEnumerator CheckLighting(string output)
        {
            var game=DarklightCampaign.Instance;
            var lab=DarklightCombatLab.Instance;
            Require(game!=null&&lab!=null,"Campaign must initialize before the standalone lighting check.");
            typeof(DarklightCampaign).GetMethod("Close",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(game,null);
            var remaster=game.GetComponent<DarklightRemaster>();
            remaster.SetQuality(2,false);
            game.ReducedFlashes=false;
            yield return null;
            var lights=remaster.GetComponentsInChildren<Light2D>();
            int moons=0;
            var result=new LightingReceipt {
                region=DarklightStory.Places[0],preset=DarklightRemaster.AmbientPresetName(0),
                graphics=SystemInfo.graphicsDeviceName
            };
            foreach(var light in lights)
            {
                if(light.lightType==Light2D.LightType.Global&&light.enabled)moons++;
                if(light.name=="Lamparina"&&light.enabled)result.lantern=true;
                if(light.name=="Portal"&&light.enabled)result.portal=true;
            }
            result.singleMoon=moons==1;
            Vector2 position=lab.Player.transform.position;
            DarklightCombatFX.LadyShot(position+Vector2.up*.8f,position+new Vector2(1,.8f),true);
            int ActiveContacts()
            {
                int count=0;
                foreach(var light in lights)if(light.name=="LuzDeContato"&&light.enabled)count++;
                return count;
            }
            result.ladyMuzzleAndHit=ActiveContacts()==2;
            DarklightNativeCapture.Screen(Path.Combine(output,"gothic-map1-contact.png"));
            yield return new WaitForSecondsRealtime(.25f);
            result.contactExpires=ActiveContacts()==0;
            var frames=new float[60];
            var timings=new FrameTiming[1];
            for(int i=0;i<frames.Length;i++)
            {
                FrameTimingManager.CaptureFrameTimings();
                yield return null;
                frames[i]=Time.unscaledDeltaTime*1000;
                result.averageFrameMs+=frames[i];
                if(FrameTimingManager.GetLatestTimings(1,timings)>0)
                {
                    result.timingSamples++;
                    result.averageCpuFrameMs+=timings[0].cpuFrameTime;
                    result.averageGpuFrameMs+=timings[0].gpuFrameTime;
                }
            }
            Array.Sort(frames);
            result.samples=frames.Length;
            result.averageFrameMs/=frames.Length;
            result.p95FrameMs=frames[56];
            if(result.timingSamples>0)
            {
                result.averageCpuFrameMs/=result.timingSamples;
                result.averageGpuFrameMs/=result.timingSamples;
            }
            File.WriteAllText(Path.Combine(output,"gothic-lighting.json"),JsonUtility.ToJson(result,true));
            Require(result.singleMoon&&result.lantern&&result.portal&&result.ladyMuzzleAndHit&&result.contactExpires,
                "Standalone lighting validation failed.");
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
                if (form > 0)
                {
                    string expected = form == 1 ? "FabiusRuptura" : "FabiusSerafim";
                    Require(name.StartsWith(expected,StringComparison.Ordinal)||
                        name.StartsWith("Redesign_"+expected,StringComparison.Ordinal),
                        $"Wrong transformed identity: {name}.");
                }
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
