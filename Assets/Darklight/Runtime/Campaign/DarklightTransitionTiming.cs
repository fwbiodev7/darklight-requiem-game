using UnityEngine;
namespace Darklight
{
    public static class DarklightTransitionTiming
    {
        public static float Fade(float progress,bool covering)
        {
            float p=Mathf.Clamp01(progress);float eased=p*p*(3-2*p);
            return covering?eased:1-eased;
        }
        public static float Cue(float elapsed,float duration)
        {
            if(duration<=0 || elapsed<=0 || elapsed>=duration)return 0;
            return Mathf.Min(Mathf.Clamp01(elapsed/.18f),Mathf.Clamp01((duration-elapsed)/.35f));
        }
    }
}
