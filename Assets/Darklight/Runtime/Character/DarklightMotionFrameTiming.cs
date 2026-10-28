using UnityEngine;
namespace Darklight
{
    public static class DarklightMotionFrameTiming
    {
        public static bool Supports(string clip)=>clip=="StartMove"||clip=="Brake"||clip=="Turn";
        public static int Frame(string clip,float time)
        {
            if(float.IsNaN(time)||float.IsInfinity(time))time=0;
            float duration=clip=="StartMove"?.12f:.14f;
            int phase=Mathf.Clamp(Mathf.FloorToInt(Mathf.Max(0,time)/duration*3),0,2);
            if(clip=="StartMove")return phase;
            if(clip=="Brake")return phase==0?2:phase==1?3:5;
            if(clip=="Turn")return phase==0?3:phase==1?4:5;
            return 0;
        }
    }
}
