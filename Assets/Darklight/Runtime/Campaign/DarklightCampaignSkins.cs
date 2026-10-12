using System;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public Sprite[] rockIdle,rockMotion,rockActions,rockSlash,rockFormFrames;
        public Sprite[] CurrentFormFrames=>SelectedSkin==1?rockFormFrames:formFrames;
        public int SelectedSkin {get;private set;}
        public void SelectSkin(int index,bool persist=true)
        {
            if(index<0||index>1)throw new ArgumentOutOfRangeException(nameof(index));
            if(index==1&&(rockIdle?.Length!=8||rockMotion?.Length!=24||rockActions?.Length!=32||rockSlash?.Length!=32))throw new InvalidOperationException("Rock skin is incomplete");
            if(requiemPreviewIdle==null&&SelectedSkin==0)requiemPreviewIdle=lab.Player.GetComponent<DarklightCharacterView>().idle;
            SelectedSkin=index;
            lab.Player.GetComponent<DarklightCharacterView>().SetSkin(index,rockIdle,rockMotion,rockActions,rockSlash);
            if(persist&&!DarklightAutomation.Silent){PlayerPrefs.SetInt("Darklight.Skin",index);PlayerPrefs.Save();}
        }
    }
}
