using UnityEngine;
namespace Darklight
{
    // Reuses the entire approved shrine sprite. No deformation, cutouts or audio.
    public sealed class DarklightCheckpointPulse:MonoBehaviour
    {
        SpriteRenderer glow;float started,peak;
        public void Present(SpriteRenderer source,bool reduced)
        {
            if(glow==null)
            {
                var child=new GameObject("LuzDoSeloSalvo");child.transform.SetParent(transform,false);
                glow=child.AddComponent<SpriteRenderer>();
            }
            glow.sprite=source.sprite;glow.sharedMaterial=source.sharedMaterial;
            var properties=new MaterialPropertyBlock();source.GetPropertyBlock(properties);glow.SetPropertyBlock(properties);
            glow.sortingLayerID=source.sortingLayerID;glow.sortingOrder=source.sortingOrder+1;
            glow.flipX=source.flipX;glow.flipY=source.flipY;
            peak=reduced?.18f:.4f;started=Time.unscaledTime;glow.enabled=true;
        }
        void Update()
        {
            if(glow==null || !glow.enabled)return;
            float alpha=DarklightTransitionTiming.Cue(Time.unscaledTime-started,.8f);
            glow.color=new Color(.64f,1,.82f,alpha*peak);if(alpha<=0&&Time.unscaledTime>started)glow.enabled=false;
        }
    }
}
