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
            // Rock arrays remain serialized for legacy preservation. Active saves/preferences migrate to Réquiem.
            index=0;
            if(requiemPreviewIdle==null&&SelectedSkin==0)requiemPreviewIdle=lab.Player.GetComponent<DarklightCharacterView>().idle;
            SelectedSkin=index;Weapons?.EnforceSkin();
            lab.Player.GetComponent<DarklightCharacterView>().SetSkin(index,rockIdle,rockMotion,rockActions,rockSlash);
            if(persist&&!DarklightAutomation.Silent){PlayerPrefs.SetInt("Darklight.Skin",index);PlayerPrefs.Save();}
        }
    }
}
