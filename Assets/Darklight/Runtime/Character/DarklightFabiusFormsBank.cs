using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    public static class DarklightFabiusFormsBank
    {
        public const string Root = "Darklight/FabiusForms20261111/";
        static readonly string[] Directions = { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };

        [Serializable]
        public sealed class Page
        {
            public string id;
            public int[] rect;
            public float[] pivot;
            public int durationMs;
            public string action;
            public string direction;
            public string form;
            public int frameIndex;
        }

        [Serializable]
        public sealed class SheetData
        {
            public string name;
            public string image;
            public string imageSha256;
            public int[] nativeSize;
            public int[] frameSize;
            public float pixelsPerUnit;
            public int rows;
            public int cols;
            public string form;
            public string action;
            // Katana, Fists, Saws or Moto; empty preserves the shared legacy catalog.
            public string weapon;
            public bool phaseSynchronized;
            public Page[] frames;
        }

        [Serializable]
        public sealed class Catalog
        {
            public SheetData[] pages;
        }

        sealed class ResolvedFrames
        {
            public Sprite[] sprites;
            public int[] durationsMs;
            public int totalDurationMs;
        }

        static Catalog catalog;
        static bool attempted;
        static readonly Dictionary<string, Dictionary<string, Sprite>> loadedSheets = new(StringComparer.Ordinal);
        static readonly Dictionary<(string sheet, int direction), ResolvedFrames> resolvedFrames = new();
        static readonly HashSet<Sprite> authored = new();
        static readonly MaterialPropertyBlock ownershipBlock = new();

        public static bool Ready
        {
            get
            {
                EnsureLoaded();
                return catalog != null && catalog.pages != null && catalog.pages.Length > 0;
            }
        }

        public static void ClearCache()
        {
            catalog = null;
            attempted = false;
            loadedSheets.Clear();
            resolvedFrames.Clear();
            authored.Clear();
        }

        static void EnsureLoaded()
        {
            if (attempted) return;
            attempted = true;
            var text = Resources.Load<TextAsset>(Root + "catalog");
            if (text == null || string.IsNullOrEmpty(text.text)) return;
            try
            {
                catalog = JsonUtility.FromJson<Catalog>(text.text);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[FabiusFormsBank] Error loading catalog: " + e.Message);
            }
        }

        static string AuthoredDirection(Page frame, string action)
        {
            if (frame == null) return null;
            // These three existing walk manifests contain placeholder E tags;
            // their action identity is the only validated direction source.
            if (action == "WalkN") return "N";
            if (action == "WalkW") return "W";
            if (action == "WalkS") return "S";
            return frame.direction;
        }

        static string WalkAction(int form, int direction)
        {
            if (form == 2)
            {
                if (direction == 0 || direction == 4) return "WalkNS";
                if (direction == 2 || direction == 6 || direction == 7) return "Walk";
                return null;
            }
            return direction switch
            {
                0 => "WalkS",
                1 or 7 => "WalkDiagS",
                2 => "WalkW",
                3 or 5 => "WalkDiagN",
                4 => "WalkN",
                6 => "Walk",
                _ => null
            };
        }

        static bool TryGetSheetFrames(SheetData sheet, int direction, out ResolvedFrames result)
        {
            result = null;
            if (sheet == null || string.IsNullOrEmpty(sheet.name) || sheet.frames == null || direction < 0 || direction >= Directions.Length)
                return false;
            var key = (sheet.name, direction);
            if (resolvedFrames.TryGetValue(key, out result)) return result != null;

            if (!loadedSheets.TryGetValue(sheet.name, out var named))
            {
                named = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                var sprites = Resources.LoadAll<Sprite>(Root + sheet.name);
                if (sprites != null)
                {
                    foreach (var sprite in sprites)
                    {
                        if (sprite == null || string.IsNullOrEmpty(sprite.name)) continue;
                        if (!named.TryAdd(sprite.name, sprite))
                        {
                            loadedSheets[sheet.name] = named;
                            resolvedFrames[key] = null;
                            return false;
                        }
                        authored.Add(sprite);
                    }
                }
                loadedSheets[sheet.name] = named;
            }

            string wantedDirection = Directions[direction];
            var selected = new List<(Page frame, Sprite sprite)>();
            foreach (var frame in sheet.frames)
            {
                if (frame == null || string.IsNullOrEmpty(frame.id) || frame.durationMs <= 0 ||
                    AuthoredDirection(frame, sheet.action) != wantedDirection ||
                    !named.TryGetValue(frame.id, out var sprite) || sprite == null)
                    continue;
                selected.Add((frame, sprite));
            }
            selected.Sort((a, b) =>
            {
                int order = a.frame.frameIndex.CompareTo(b.frame.frameIndex);
                return order != 0 ? order : string.CompareOrdinal(a.frame.id, b.frame.id);
            });
            if (selected.Count == 0)
            {
                resolvedFrames[key] = null;
                return false;
            }

            var frames = new ResolvedFrames
            {
                sprites = new Sprite[selected.Count],
                durationsMs = new int[selected.Count]
            };
            for (int i = 0; i < selected.Count; i++)
            {
                frames.sprites[i] = selected[i].sprite;
                frames.durationsMs[i] = selected[i].frame.durationMs;
                frames.totalDurationMs += selected[i].frame.durationMs;
            }
            if (frames.totalDurationMs <= 0)
            {
                resolvedFrames[key] = null;
                return false;
            }
            resolvedFrames[key] = frames;
            result = frames;
            return true;
        }

        static int FrameIndex(ResolvedFrames frames, float seconds, bool loop)
        {
            if (frames == null || frames.sprites == null || frames.sprites.Length == 0) return -1;
            double milliseconds = float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0d : Math.Max(0d, seconds * 1000d);
            if (loop)
                milliseconds %= frames.totalDurationMs;
            else
                milliseconds = Math.Min(milliseconds, frames.totalDurationMs - 1d);
            int elapsed = 0;
            for (int i = 0; i < frames.durationsMs.Length; i++)
            {
                elapsed += frames.durationsMs[i];
                if (milliseconds < elapsed) return i;
            }
            return frames.sprites.Length - 1;
        }

        public static bool TryResolveFormPose(int form, int weapon, int direction, CombatModel model, Vector2 movement, out Sprite sprite)
        {
            sprite = null;
            if (direction < 0 || direction >= Directions.Length || model == null ||
                (weapon != 0 && weapon != 2 && weapon != 3) || !Ready) return false;

            string formName = form == 1 ? "Ruptura" : form == 2 ? "Serafim" : null;
            if (formName == null) return false;
            // Approved final pose is a frontal, stationary katana idle only.
            // Movement and every other authored direction/action retain their clips.
            if(weapon==0&&direction==0&&model.State==CombatState.Neutral&&movement.sqrMagnitude<=.01f&&
                DarklightOfficialTransitionBank.TryPose(form,1,out sprite))return true;

            bool isDead = model.State == CombatState.Dead;
            bool isHurt = model.State == CombatState.Hurt;
            bool isAttack = model.State == CombatState.Attack;
            bool isDodge = model.State == CombatState.Dodge;
            if (model.State == CombatState.Charging) return false;

            string targetAction = null;
            if (isDead || isHurt)
                targetAction = form == 2 && weapon == 0 && direction == 7 ? (isDead ? "Dead" : "Hurt") : "HurtDead";
            else if (model.State == CombatState.Parry && weapon == 0)
                targetAction = "Guard";
            else if (isDodge)
                targetAction = form == 1 ? "ActivationDodge" : "FlyGlide";
            else if (isAttack)
            {
                if (weapon == 0) targetAction = "KatanaAttack";
                else if (weapon == 2) targetAction = "FistAttack";
                else if (weapon == 3) targetAction = "MotoToSaws";
                else return false;
            }
            else if (model.State == CombatState.Neutral)
            {
                string weaponName = weapon switch { 0 => "Katana", 2 => "Fists", 3 => "Saws", _ => null };
                bool walking = movement.sqrMagnitude > 0.01f;
                foreach (var page in catalog.pages)
                {
                    // Resolve only the requested authored view, before any legacy
                    // action/direction remapping. Idle holds this weapon's first frame.
                    if (page == null || page.form != formName || page.action != "Walk" ||
                        page.weapon != weaponName || !TryGetSheetFrames(page, direction, out var frames))
                        continue;
                    int index = FrameIndex(frames, walking ? model.Clock : 0, walking);
                    if (index < 0 || index >= frames.sprites.Length) return false;
                    sprite = frames.sprites[index];
                    return sprite != null;
                }

                // Directions without a weapon-specific Walk retain existing shared
                // legacy coverage until their own whole-figure art is supplied.
                targetAction = WalkAction(form, direction);
            }

            // Missing diagonal walk views retain this form using the nearest
            // authored cardinal view. Idle holds its first whole walk figure.
            if (model.State == CombatState.Neutral && form == 2 && targetAction == null)
            {
                direction = direction == 1 || direction == 7 ? 0 : 4;
                targetAction = "WalkNS";
            }
            if (string.IsNullOrEmpty(targetAction)) return false;

            bool locomotion = model.State == CombatState.Neutral && movement.sqrMagnitude > 0.01f;
            float time = model.State == CombatState.Neutral ? (locomotion ? model.Clock : 0) : model.StateTime;
            foreach (var page in catalog.pages)
            {
                if (page == null || page.form != formName || page.action != targetAction ||
                    (model.State == CombatState.Neutral && !string.IsNullOrEmpty(page.weapon)) ||
                    !TryGetSheetFrames(page, direction, out var frames))
                    continue;
                int frameIndex = page.phaseSynchronized && isAttack && model.Attack != null && frames.sprites.Length == 3
                    ? (model.StateTime < model.Attack.startup ? 0 : model.StateTime < model.Attack.startup + model.Attack.active ? 1 : 2)
                    : FrameIndex(frames, time, locomotion);
                if (frameIndex < 0 || frameIndex >= frames.sprites.Length) return false;
                sprite = frames.sprites[frameIndex];
                return sprite != null;
            }
            return false;
        }

        // Existing full-figure awakening has one authored view. Keep its authored
        // frame order explicit: frames 5/6 are attacks and never belong to entry.
        public static bool TryTransformationPose(int form,int weapon,int direction,float progress,out Sprite sprite)
        {
            sprite=null;
            if(form!=2||weapon!=0||direction<0||direction>7||!Ready||float.IsNaN(progress)||float.IsInfinity(progress))return false;
            foreach(var page in catalog.pages)
            {
                if(page==null||page.form!="Serafim"||page.action!="TransformationUltimate"||
                    page.name!="FabiusSerafimTransformationUltimateV1"||!TryGetSheetFrames(page,6,out var frames)||frames.sprites.Length!=8)continue;
                // The sheet is visually frontal although legacy metadata labels it E.
                // This cinematic deliberately uses that single complete view.
                int index=progress<.16f?0:progress<.32f?1:progress<.53f?2:progress<.78f?4:progress<.93f?3:7;
                sprite=frames.sprites[index];return sprite!=null;
            }
            return false;
        }

        public static bool IsAuthored(Sprite sprite) => sprite != null && authored.Contains(sprite);

        public static void ApplyOwnership(SpriteRenderer renderer)
        {
            if (renderer == null || !IsAuthored(renderer.sprite)) return;
            renderer.GetPropertyBlock(ownershipBlock);
            ownershipBlock.SetFloat("_WholeSprite", 1);
            ownershipBlock.SetFloat("_WholeFrameId", 0);
            ownershipBlock.SetFloat("_VampireForm", 0);
            ownershipBlock.SetFloat("_DarkForm", 0);
            ownershipBlock.SetFloat("_Physique", 0);
            ownershipBlock.SetFloat("_LayerOcclusion", 0);
            ownershipBlock.SetFloat("_EraseHead", 0);
            ownershipBlock.SetFloat("_RuinFists", 0);
            ownershipBlock.SetFloat("_AlphaClip", .1f);
            ownershipBlock.SetTexture("_VampireMask", Texture2D.blackTexture);
            ownershipBlock.SetVector("_CanonicalSkinShift", Vector4.zero);
            renderer.SetPropertyBlock(ownershipBlock);
        }
    }
}
