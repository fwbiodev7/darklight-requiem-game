using UnityEngine;

namespace Darklight
{
    /// <summary>
    /// Canonical Form & Resonance Controller for Fabius Requiem.
    /// Canonical forms strictly: 0: Normal, 1: Ruptura (Crimson), 2: Eclipse Absoluto (Violet).
    /// Zero-GC presentation via MaterialPropertyBlock.
    /// </summary>
    public sealed class DarklightFormController : MonoBehaviour
    {
        public enum FormKind
        {
            Normal = 0,
            Ruptura = 1,
            Eclipse = 2
        }

        public FormKind CurrentForm { get; private set; } = FormKind.Normal;
        public float FormDurationRemaining { get; private set; }

        SpriteRenderer spriteRenderer;
        MaterialPropertyBlock propertyBlock;
        static readonly int FormColorId = Shader.PropertyToID("_FormColor");
        static readonly int FormIntensityId = Shader.PropertyToID("_FormIntensity");

        public static readonly Color ColorNormal = Color.white;
        public static readonly Color ColorRuptura = new Color(0.95f, 0.15f, 0.22f, 1f); // Crimson
        public static readonly Color ColorEclipse = new Color(0.68f, 0.35f, 0.95f, 1f); // Spectral Violet

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        public void TransformTo(FormKind targetForm, float duration = 12f)
        {
            if (CurrentForm == targetForm) return;

            CurrentForm = targetForm;
            FormDurationRemaining = duration;

            DarklightCombatFX.Transformation(transform.position, (int)CurrentForm, release: true);

            var labCam = DarklightCombatLab.Instance?.WorldCamera?.GetComponent<DarklightLabCamera>();
            if (labCam != null)
            {
                labCam.Impulse(CurrentForm == FormKind.Normal ? 1 : 2);
            }

            ApplyVisuals();
        }

        public void RevertToNormal()
        {
            TransformTo(FormKind.Normal, 0f);
        }

        void Update()
        {
            float dt = DarklightCombatLab.Instance?.CombatDelta ?? Time.deltaTime;
            if (dt <= 0 || CurrentForm == FormKind.Normal) return;

            FormDurationRemaining -= dt;
            if (FormDurationRemaining <= 0)
            {
                RevertToNormal();
            }
        }

        void ApplyVisuals()
        {
            if (spriteRenderer == null) return;

            spriteRenderer.GetPropertyBlock(propertyBlock);
            Color tint = CurrentForm switch
            {
                FormKind.Ruptura => ColorRuptura,
                FormKind.Eclipse => ColorEclipse,
                _ => ColorNormal
            };

            float intensity = CurrentForm == FormKind.Normal ? 0f : 0.65f;
            propertyBlock.SetColor(FormColorId, tint);
            propertyBlock.SetFloat(FormIntensityId, intensity);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        public float DamageMultiplier => CurrentForm switch
        {
            FormKind.Ruptura => 1.35f,
            FormKind.Eclipse => 1.15f,
            _ => 1.0f
        };

        public float SpeedMultiplier => CurrentForm switch
        {
            FormKind.Eclipse => 1.25f,
            FormKind.Ruptura => 1.05f,
            _ => 1.0f
        };
    }
}
