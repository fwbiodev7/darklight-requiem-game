using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        Sprite[] finalFormFrames;bool finalFormLoaded;
        Sprite[] FinalFormFrames
        {
            get
            {
                if(kind!=RequiemEnemyKind.Nocthar||Phase!=3)return null;
                if(!finalFormLoaded)
                {
                    finalFormLoaded=true;finalFormFrames=DarklightSecondaryStyleBank.Load("NoctharFinal");
                    System.Array.Sort(finalFormFrames,(a,b)=>int.Parse(a.name.Substring(a.name.LastIndexOf('_')+1)).CompareTo(int.Parse(b.name.Substring(b.name.LastIndexOf('_')+1))));
                }
                return finalFormFrames?.Length==8?finalFormFrames:null;
            }
        }
        void ShowFinalForm(int index)
        {
            var frames=FinalFormFrames;
            art.sprite=frames[Mathf.Clamp(index,0,7)];
            // Standing reference shared by all poses: crouching never expands the body or wings.
            art.transform.localScale=Vector3.one*DarklightWorldScale.EnemyHeight(kind)/frames[0].bounds.size.y;
        }
    }
}

