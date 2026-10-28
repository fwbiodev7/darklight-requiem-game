using UnityEngine;
namespace Darklight
{
    public static class DarklightIdleFrameTiming
    {
        // Five complete breathing poses plus one separately authored closed-eye pose.
        // Blinking is occasional; it is never a mandatory sixth phase of every breath.
        public static int Frame(int phases,float clock,int blinkFrame=-1)
        {
            if(phases<=1 || float.IsNaN(clock) || float.IsInfinity(clock))return 0;
            clock=Mathf.Max(0,clock);
            if(blinkFrame!=5 || phases!=6)return Mathf.FloorToInt(clock*2)%phases;
            float interval=clock%14.3f;
            if(interval>=6 && interval<6.13f || interval>=14.12f && interval<14.25f)return blinkFrame;
            return Mathf.FloorToInt(clock/.52f)%5;
        }
    }
}
