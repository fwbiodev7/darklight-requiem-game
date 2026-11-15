using UnityEngine;

namespace Darklight
{
    // Presentation only: collider, Rigidbody2D, root, scale and sprite pivots never move.
    [DefaultExecutionOrder(1200)]
    public sealed class DarklightTransformationPresentation : MonoBehaviour
    {
        SpriteRenderer body;
        bool lifting;
        float height;
        public float VisualHeight => height;
        public static float LiftEnvelope(float progress)
        {
            progress = Mathf.Clamp01(progress);
            if (progress < .12f) return 0;
            if (progress < .34f) return Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.12f, .34f, progress));
            if (progress < .76f) return 1;
            return 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.76f, 1, progress));
        }
        public void Present(SpriteRenderer renderer, float progress, bool levitating)
        {
            if (body != renderer) ResetPresentation();
            body = renderer;
            height = levitating ? .62f * LiftEnvelope(progress) : 0;
            if (!levitating) ResetPresentation();
            else lifting = true;
        }
        void LateUpdate()
        {
            var health = GetComponent<HealthSystem>();
            var campaign = GetComponent<DarklightFighter>() != null ? DarklightCampaign.Instance : null;
            if ((health != null && health.IsDead) || (campaign != null && !campaign.enabled)) ResetPresentation();
            if (lifting && body != null && body.transform != transform)
                body.transform.position = DarklightCharacterView.SnapRenderPosition(transform.position) + Vector3.up * height;
        }
        public void ResetPresentation()
        {
            if (lifting && body != null && body.transform != transform)
                body.transform.position = DarklightCharacterView.SnapRenderPosition(transform.position);
            lifting = false;
            height = 0;
        }
        void OnDisable() => ResetPresentation();
    }
}
