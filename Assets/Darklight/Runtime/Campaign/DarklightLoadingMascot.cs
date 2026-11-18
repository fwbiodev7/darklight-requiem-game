using UnityEngine;
using UnityEngine.UI;

namespace Darklight
{
    // Presentation only: never controls scene loading, progress or transition timing.
    [RequireComponent(typeof(RawImage))]
    public sealed class DarklightLoadingMascot : MonoBehaviour
    {
        public enum Variant { GatoRuptura, MiniFabius }
        public const int FrameCount = 8;
        RawImage image;
        Texture2D cat, fabius;
        float started;
        bool running;
        public Variant SelectedVariant { get; private set; }
        public int CurrentFrame { get; private set; } = -1;

        public static Variant ChooseVariant(float sample) => sample < .5f ? Variant.GatoRuptura : Variant.MiniFabius;

        public static DarklightLoadingMascot Attach(Transform parent, Vector2 position)
        {
            var go = new GameObject("LoadingMascot", typeof(RectTransform), typeof(RawImage));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(36, 36);
            return go.AddComponent<DarklightLoadingMascot>();
        }

        void Awake()
        {
            image = GetComponent<RawImage>();
            image.raycastTarget = false;
            cat = Resources.Load<Texture2D>("Darklight/Loading/Mascots/gato-ruptura");
            fabius = Resources.Load<Texture2D>("Darklight/Loading/Mascots/mini-fabius");
        }

        // Enabling the existing loading root is the single selection boundary.
        void OnEnable() => PlayVariant(ChooseVariant(Random.value));

        public void PlayVariant(Variant variant)
        {
            SelectedVariant = variant;
            image.texture = variant == Variant.GatoRuptura ? cat : fabius;
            running = image.texture != null;
            image.enabled = running;
            started = Time.unscaledTime;
            CurrentFrame = -1;
            if (running) ApplyFrame(0);
        }

        void Update()
        {
            if (!running) return;
            float frameSeconds = SelectedVariant == Variant.GatoRuptura ? .18f : .12f;
            int frame = (int)((Time.unscaledTime - started) / frameSeconds) % FrameCount;
            if (frame != CurrentFrame) ApplyFrame(frame);
        }

        void ApplyFrame(int frame)
        {
            CurrentFrame = frame;
            image.uvRect = new Rect((frame % 4) * .25f, frame < 4 ? .5f : 0, .25f, .5f);
        }

        void OnDisable()
        {
            running = false;
            CurrentFrame = -1;
            if (image != null) image.texture = null;
        }
    }
}
