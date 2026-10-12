using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        DarklightAtlas atlas;
        bool mapReturnToPause;
        public DarklightAtlas Atlas=>atlas;
        public IReadOnlyList<DarklightEnemy> MapEnemies=>enemies;
        public int MapRegion=>ExploringNewRegion?RegionChapter:Progress.chapter;
        public bool IsPausePanel=>panel=="Pause";
        void InitializeAtlas(){atlas=gameObject.AddComponent<DarklightAtlas>();atlas.Initialize(this,lab,uiRoot,overlay,font);}
        internal void OpenAtlasPanel()
        {
            mapReturnToPause=panel=="Pause";Open("Map");menuRoot.gameObject.SetActive(false);cinematicRoot.gameObject.SetActive(false);
            menuBackdrop.enabled=false;SetHUDVisible(false);
        }
        internal void CloseAtlasPanel(){menuRoot.gameObject.SetActive(true);if(mapReturnToPause)Open("Pause");else Close();}
        public void FillMapObjectives(List<DarklightMapPoint> points)
        {
            if(Map==null)return;
            void Add(string id,string title,string description,Vector2 p,DarklightMapSymbol symbol,bool completed=false)
            {points.Add(new DarklightMapPoint{id=id,title=title,description=description,position=InteractionAnchor(p),symbol=symbol,completed=completed});}
            if(ExploringNewRegion)
            {
                bool done=VisitedRegionAreas>=Map.Art.areas.Length;
                if(!done){var area=Map.Art.areas[regionGoal];Add("area_"+regionGoal,area.name,area.description,Map.Art.World(area.point),DarklightMapSymbol.Primary);}
                for(int i=0;i<Map.Art.areas.Length;i++)if(done||i!=regionGoal)
                {var a=Map.Art.areas[i];Add("area_"+i,a.name,a.description,Map.Art.World(a.point),DarklightMapSymbol.Secondary,regionVisited[i]);}
                Add("exit","Saída da região","Retorne à escolha de regiões. As memórias da campanha ficam preservadas.",Map.Portal,DarklightMapSymbol.Exit);
                return;
            }
            if(!chapterCleared)Add("seal_"+encounter,Objective,MissionDescription,Map.Arenas[encounter],DarklightMapSymbol.Primary);
            else Add("exit","Portal libertado","Aproxime-se e interaja para concluir esta fase.",Map.Portal,DarklightMapSymbol.Exit);
            if(!Progress.relics[Progress.chapter])Add("fragment","Memória do Véu","Recupere o fragmento para registrar uma memória, ganhar experiência e um elixir.",Map.Relic,DarklightMapSymbol.Secondary);
            for(int i=0;i<=Mathf.Min(3,Progress.checkpoint);i++)Add("checkpoint_"+i,"Checkpoint "+(i+1),"Um ponto conhecido de retorno nesta fase.",i==0?CheckpointPoint:Map.Checkpoints[i],DarklightMapSymbol.Checkpoint);
            if(!encounterStarted)Add("ally",AllyName,Progress.chapter==1&&Progress.mp3Quest>0&&!Progress.mp3Unlocked?"Recupere os componentes do MP3 e retorne a Ícaro. Restam "+Mathf.Max(0,4-Progress.mp3Quest)+".":"Aproxime-se e interaja para conversar.",Map.Ally,DarklightMapSymbol.Interest);
        }
        internal void SaveMapExploration(){atlas?.Flush();}
        internal bool[] RestoreMapAreas(int region,int count)=>atlas?.SavedAreas(region,count);
        internal void RememberMapAreas(){if(ExploringNewRegion)atlas?.RememberAreas(RegionChapter,regionVisited);}
    }
}
