using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    public static class DarklightFabiusFormsBank
    {
        public const string Root = "Darklight/FabiusForms20261111/";

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
            public Page[] frames;
        }

        [Serializable]
        public sealed class Catalog
        {
            public SheetData[] pages;
        }

        static Catalog catalog;
        static bool attempted;
        static readonly Dictionary<string, Sprite[]> loadedSheets = new(StringComparer.Ordinal);
        static readonly Dictionary<string, Sprite> frameLookup = new(StringComparer.Ordinal);

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
            frameLookup.Clear();
        }

        static void EnsureLoaded()
        {
            if (attempted) return;
            attempted = true;
            var text = Resources.Load<TextAsset>(Root + "catalog");
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                try
                {
                    catalog = JsonUtility.FromJson<Catalog>(text.text);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[FabiusFormsBank] Error loading catalog: " + e.Message);
                }
            }
        }

        public static bool TryResolveFormPose(int form, int weapon, int direction, CombatModel model, Vector2 movement, out Sprite sprite)
        {
            sprite = null;
            if (form <= 0 || !Ready) return false;

            string formName = form == 1 ? "Ruptura" : form == 2 ? "Serafim" : null;
            if (formName == null) return false;

            // Identificar ação solicitada
            bool isMoving = movement.sqrMagnitude > 0.01f;
            bool isDead = model != null && model.State == CombatState.Dead;
            bool isHurt = model != null && model.State == CombatState.Hurt;
            bool isAttack = model != null && (model.State == CombatState.Attack || model.State == CombatState.Charging);
            bool isDodge = model != null && model.State == CombatState.Dodge;

            string targetAction = null;
            if (isDead || isHurt) targetAction = "HurtDead";
            else if (isDodge) targetAction = form == 1 ? "ActivationDodge" : "FlyGlide";
            else if (isAttack)
            {
                if (weapon == 2) targetAction = "FistAttack";
                else if (weapon == 3) targetAction = "MotoToSaws";
                else targetAction = "KatanaAttack";
            }
            else if (isMoving)
            {
                targetAction = form == 1 ? "Walk" : "FlyGlide";
            }

            if (targetAction == null) return false;

            // Buscar sheet correspondente
            foreach (var page in catalog.pages)
            {
                if (page.form != formName) continue;
                if (page.action == targetAction || (targetAction == "Walk" && page.action.StartsWith("Walk")) ||
                    (targetAction == "FlyGlide" && (page.action == "FlyGlide" || page.action == "Fly8D")))
                {
                    var sprites = LoadSheetSprites(page.name);
                    if (sprites != null && sprites.Length > 0)
                    {
                        float time = model != null ? (model.State == CombatState.Neutral ? model.Clock : model.StateTime) : 0f;
                        int frameIdx = Mathf.FloorToInt(time / 0.1f) % sprites.Length;
                        if (isDead) frameIdx = sprites.Length - 1; // Último frame deitada no chão
                        sprite = sprites[frameIdx];
                        return true;
                    }
                }
            }

            return false;
        }

        public static Sprite[] LoadSheetSprites(string sheetName)
        {
            if (loadedSheets.TryGetValue(sheetName, out var cached)) return cached;
            var sprites = Resources.LoadAll<Sprite>(Root + sheetName);
            if (sprites != null && sprites.Length > 0)
            {
                Array.Sort(sprites, (a, b) => string.CompareOrdinal(a.name, b.name));
                loadedSheets[sheetName] = sprites;
                foreach (var s in sprites) frameLookup[s.name] = s;
                return sprites;
            }
            return null;
        }
    }
}

