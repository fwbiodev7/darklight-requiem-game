using System;
using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        // Explicit launch location for a requested play session; loading alone never writes a save.
        void StartRequestedLocation()
        {
            if(Array.Exists(Environment.GetCommandLineArgs(),arg=>arg=="--requiem-playtest")){BeginPlaytestSession();return;}
            foreach(var arg in Environment.GetCommandLineArgs())
            {
                const string prefix="--requiem-start-map=";
                if(!arg.StartsWith(prefix,StringComparison.Ordinal)||!int.TryParse(arg.Substring(prefix.Length),out int chapter)||chapter<0||chapter>=10)continue;
                Progress=DarklightProgress.Load()??new DarklightProgress();
                if(chapter<6){Progress.chapter=chapter;Progress.checkpoint=0;Progress.completed=false;LoadMap(chapter);Close();}
                else ExploreRegion(chapter);
                Debug.Log("DARKLIGHT requested start: map "+(chapter+1)+" - "+(chapter<6?DarklightStory.Places[chapter]:Map.Art.title)+"; campaign save was not overwritten by launch.");
                return;
            }
        }
    }
}

