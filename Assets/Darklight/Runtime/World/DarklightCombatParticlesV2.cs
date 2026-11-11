using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    public enum DarklightParticleV2Family
    {
        ContatoImpacto = 0,
        AtaqueKatana = 1,
        AtaquePunhos = 2,
        AtaqueMoto = 3,
        EsquivaKatana = 4,
        EsquivaPunhos = 5,
        EsquivaMoto = 6
    }

    [DefaultExecutionOrder(1800)]
    public sealed class DarklightCombatParticlesV2 : MonoBehaviour
    {
        public const string ManifestResource = "Darklight/CombatParticlesV2/CombatParticlesV2";
        public const string ResourcePrefix = "Darklight/CombatParticlesV2/";

        [Serializable]
        public sealed class FrameManifest
        {
            public string id;
            public int index;
            public int[] rect;
            public float[] pivot;
            public int durationMs;
        }

        [Serializable]
        public sealed class FamilyManifest
        {
            public string name;
            public string sheet;
            public string sheetFile;
            public string desc;
            public string action;
            public float[] pivot;
            public int frameDurationMs;
            public float recommendedScale;
            public FrameManifest[] frames;
        }

        [Serializable]
        public sealed class RootManifest
        {
            public int schemaVersion;
            public string assetType;
            public string status;
            public int totalFamilies;
            public int totalFrames;
            public int pixelsPerUnit;
            public FamilyManifest[] families;
        }

        sealed class Burst
        {
            public SpriteRenderer art;
            public int familyIndex;
            public float age;
            public float life;
            public bool active;
            public float baseScale;
        }

        static DarklightCombatParticlesV2 instance;
        readonly Burst[] pool = new Burst[24];
        readonly Sprite[][] familySprites = new Sprite[7][];
        readonly int[][] familyDurations = new int[7][];
        readonly float[] familyScales = new float[7] { 1.0f, 1.0f, 1.0f, 1.1f, 1.0f, 1.0f, 1.15f };
        readonly int[] familyCounts = new int[7];

        bool attempted;
        bool ready;
        int cursor;

        public static DarklightCombatParticlesV2 Current => instance;
        public bool IsReady => ready;
        public int TotalFramesLoaded { get; private set; }
        public Sprite LastPresentedSprite { get; private set; }
        public int StartedCount { get; private set; }

        public int Count(DarklightParticleV2Family family) => familyCounts[(int)family];

        public static DarklightCombatParticlesV2 GetOrCreate()
        {
            if (instance != null) return instance;
            var lab = DarklightCombatLab.Instance;
            if (lab == null) return null;
            var obj = new GameObject("CombatParticlesV2");
            obj.transform.SetParent(lab.transform, false);
            instance = obj.AddComponent<DarklightCombatParticlesV2>();
            instance.Initialize();
            return instance;
        }

        public bool Initialize()
        {
            if (attempted) return ready;
            attempted = true;

            var manifestAsset = Resources.Load<TextAsset>(ManifestResource);
            RootManifest manifest = null;
            if (manifestAsset != null && !string.IsNullOrEmpty(manifestAsset.text))
            {
                manifest = JsonUtility.FromJson<RootManifest>(manifestAsset.text);
            }

            int loadedCount = 0;
            for (int f = 0; f < 7; f++)
            {
                var familyEnum = (DarklightParticleV2Family)f;
                string familyName = familyEnum.ToString();
                // Map enum name to manifest family name
                string sheetResourceName = GetSheetResourceName(familyEnum);

                var loaded = Resources.LoadAll<Sprite>(ResourcePrefix + sheetResourceName);
                if (loaded == null || loaded.Length == 0)
                {
                    // Fallback to loading individual frames from Frames/ folder
                    var list = new List<Sprite>();
                    string familyPrefix = GetFamilyPrefix(familyEnum);
                    for (int i = 1; i <= 8; i++)
                    {
                        var single = Resources.Load<Sprite>($"{ResourcePrefix}Frames/{familyPrefix}_{i:D2}");
                        if (single != null) list.Add(single);
                    }
                    loaded = list.ToArray();
                }

                // If loaded, sort by name or frame index
                if (loaded != null && loaded.Length > 0)
                {
                    Array.Sort(loaded, (a, b) => string.CompareOrdinal(a.name, b.name));
                    familySprites[f] = loaded;
                    loadedCount += loaded.Length;
                }
                else
                {
                    familySprites[f] = new Sprite[0];
                }

                // Frame durations
                int duration = GetDefaultDuration(familyEnum);
                if (manifest?.families != null && f < manifest.families.Length && manifest.families[f] != null)
                {
                    duration = manifest.families[f].frameDurationMs > 0 ? manifest.families[f].frameDurationMs : duration;
                    familyScales[f] = manifest.families[f].recommendedScale > 0 ? manifest.families[f].recommendedScale : familyScales[f];
                }

                int frameCount = Mathf.Max(1, familySprites[f].Length);
                familyDurations[f] = new int[frameCount];
                for (int i = 0; i < frameCount; i++)
                {
                    familyDurations[f][i] = duration;
                }
            }

            TotalFramesLoaded = loadedCount;
            ready = loadedCount >= 40; // At least 5 full families loaded successfully
            return ready;
        }

        static string GetSheetResourceName(DarklightParticleV2Family family)
        {
            switch (family)
            {
                case DarklightParticleV2Family.ContatoImpacto: return "Sheet_Contato_Impacto_v2";
                case DarklightParticleV2Family.AtaqueKatana: return "Sheet_Ataque_Katana_v2";
                case DarklightParticleV2Family.AtaquePunhos: return "Sheet_Ataque_Punhos_v2";
                case DarklightParticleV2Family.AtaqueMoto: return "Sheet_Ataque_Moto_v2";
                case DarklightParticleV2Family.EsquivaKatana: return "Sheet_Esquiva_Katana_v2";
                case DarklightParticleV2Family.EsquivaPunhos: return "Sheet_Esquiva_Punhos_v2";
                case DarklightParticleV2Family.EsquivaMoto: return "Sheet_Esquiva_Moto_v2";
                default: return "";
            }
        }

        static string GetFamilyPrefix(DarklightParticleV2Family family)
        {
            switch (family)
            {
                case DarklightParticleV2Family.ContatoImpacto: return "Contato_Impacto";
                case DarklightParticleV2Family.AtaqueKatana: return "Ataque_Katana";
                case DarklightParticleV2Family.AtaquePunhos: return "Ataque_Punhos";
                case DarklightParticleV2Family.AtaqueMoto: return "Ataque_Moto";
                case DarklightParticleV2Family.EsquivaKatana: return "Esquiva_Katana";
                case DarklightParticleV2Family.EsquivaPunhos: return "Esquiva_Punhos";
                case DarklightParticleV2Family.EsquivaMoto: return "Esquiva_Moto";
                default: return "";
            }
        }

        static int GetDefaultDuration(DarklightParticleV2Family family)
        {
            switch (family)
            {
                case DarklightParticleV2Family.ContatoImpacto: return 35;
                case DarklightParticleV2Family.AtaqueKatana: return 40;
                case DarklightParticleV2Family.AtaquePunhos: return 35;
                case DarklightParticleV2Family.AtaqueMoto: return 45;
                case DarklightParticleV2Family.EsquivaKatana: return 40;
                case DarklightParticleV2Family.EsquivaPunhos: return 40;
                case DarklightParticleV2Family.EsquivaMoto: return 45;
                default: return 40;
            }
        }

        public static bool TryEmitContact(Vector3 point, bool heavy = false, int weapon = 0)
        {
            var system = GetOrCreate();
            if (system == null || !system.ready) return false;

            float scale = heavy ? 1.35f : (weapon == 1 ? 1.1f : 0.95f);
            system.Emit(DarklightParticleV2Family.ContatoImpacto, point, Vector2.zero, scale, point.y);
            return true;
        }

        public static bool TryEmitAttack(int weapon, Vector3 point, Vector2 facing, float scaleMultiplier = 1.0f)
        {
            var system = GetOrCreate();
            if (system == null || !system.ready) return false;

            DarklightParticleV2Family family;
            switch (weapon)
            {
                case 1: family = DarklightParticleV2Family.AtaqueKatana; break;
                case 2: family = DarklightParticleV2Family.AtaquePunhos; break;
                case 3: family = DarklightParticleV2Family.AtaqueMoto; break;
                default: family = DarklightParticleV2Family.AtaqueKatana; break;
            }

            system.Emit(family, point, facing, scaleMultiplier, point.y);
            return true;
        }

        public static bool TryEmitDodge(int weapon, Vector3 point, Vector2 facing, float? groundY = null)
        {
            var system = GetOrCreate();
            if (system == null || !system.ready) return false;

            DarklightParticleV2Family family;
            switch (weapon)
            {
                case 1: family = DarklightParticleV2Family.EsquivaKatana; break;
                case 2: family = DarklightParticleV2Family.EsquivaPunhos; break;
                case 3: family = DarklightParticleV2Family.EsquivaMoto; break;
                default: family = DarklightParticleV2Family.EsquivaKatana; break;
            }

            system.Emit(family, point, facing, 1.0f, groundY ?? point.y);
            return true;
        }

        public void Emit(DarklightParticleV2Family family, Vector3 point, Vector2 facing, float scaleMultiplier, float groundY)
        {
            int fIdx = (int)family;
            if (fIdx < 0 || fIdx >= 7) return;
            var sprites = familySprites[fIdx];
            if (sprites == null || sprites.Length == 0) return;

            var game = DarklightCampaign.Instance;
            var burst = pool[cursor];
            if (burst == null)
            {
                var obj = new GameObject("ParticleV2_Burst");
                obj.transform.SetParent(transform, false);
                burst = pool[cursor] = new Burst
                {
                    art = obj.AddComponent<SpriteRenderer>()
                };
                if (game?.pixelMaterial != null)
                {
                    burst.art.sharedMaterial = game.pixelMaterial;
                }
                var block = new MaterialPropertyBlock();
                block.SetFloat("_AlphaClip", 0.05f);
                block.SetFloat("_WholeSprite", 1f);
                block.SetTexture("_VampireMask", Texture2D.blackTexture);
                burst.art.SetPropertyBlock(block);
            }

            cursor = (cursor + 1) % pool.Length;
            burst.familyIndex = fIdx;
            burst.age = 0f;
            burst.active = true;

            var durations = familyDurations[fIdx];
            float totalLife = 0f;
            for (int i = 0; i < sprites.Length; i++)
            {
                totalLife += (i < durations.Length ? durations[i] : 40) / 1000f;
            }
            burst.life = Mathf.Max(0.05f, totalLife);

            burst.art.sprite = sprites[0];
            burst.art.enabled = true;
            burst.art.transform.position = point;

            // Sorting: slightly above character at the same ground plane
            burst.art.sortingOrder = DarklightSorting.OrderForY(groundY) + 2;

            // Direction & rotation
            if (facing.sqrMagnitude > 0.01f)
            {
                if (family == DarklightParticleV2Family.AtaqueKatana || family == DarklightParticleV2Family.EsquivaKatana)
                {
                    float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
                    burst.art.transform.localRotation = Quaternion.Euler(0, 0, angle);
                    burst.art.flipX = false;
                }
                else
                {
                    burst.art.transform.localRotation = Quaternion.identity;
                    burst.art.flipX = facing.x < -0.01f;
                }
            }
            else
            {
                burst.art.transform.localRotation = Quaternion.identity;
                burst.art.flipX = false;
            }

            // Scale
            float baseScale = familyScales[fIdx] * scaleMultiplier;
            if (game?.ReducedFlashes == true) baseScale *= 0.75f;
            burst.art.transform.localScale = Vector3.one * baseScale;
            burst.baseScale = baseScale;

            // Color
            burst.art.color = new Color(1f, 1f, 1f, game?.ReducedFlashes == true ? 0.65f : 1f);

            LastPresentedSprite = burst.art.sprite;
            familyCounts[fIdx]++;
            StartedCount++;
        }

        void Update()
        {
            var lab = DarklightCombatLab.Instance;
            if (lab?.Paused == true && lab.Player?.Model?.State != CombatState.Dead) return;
            Advance(Time.unscaledDeltaTime);
        }

        public void Advance(float delta)
        {
            if (delta <= 0 || float.IsNaN(delta) || float.IsInfinity(delta)) return;

            for (int i = 0; i < pool.Length; i++)
            {
                var b = pool[i];
                if (b == null || !b.active) continue;

                b.age += delta;
                if (b.age >= b.life)
                {
                    b.active = false;
                    b.art.enabled = false;
                    continue;
                }

                // Calculate current frame index
                var sprites = familySprites[b.familyIndex];
                var durations = familyDurations[b.familyIndex];
                if (sprites == null || sprites.Length == 0) continue;

                float elapsedMs = b.age * 1000f;
                int frameIdx = 0;
                while (frameIdx < sprites.Length - 1)
                {
                    int dur = frameIdx < durations.Length ? durations[frameIdx] : 40;
                    if (elapsedMs < dur) break;
                    elapsedMs -= dur;
                    frameIdx++;
                }

                if (frameIdx >= 0 && frameIdx < sprites.Length)
                {
                    b.art.sprite = sprites[frameIdx];
                    LastPresentedSprite = b.art.sprite;
                }
            }
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}

