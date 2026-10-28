using System;
using UnityEngine;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        int exploredRegion=-1,regionGoal,regionSelection=6;
        bool[] regionVisited;
        string regionMenuReturn="Extras";
        public bool ExploringNewRegion=>exploredRegion>=6;
        public int RegionChapter=>exploredRegion;
        public int VisitedRegionAreas {get;private set;}
        public Vector2 RegionGoal=>VisitedRegionAreas>=Map.Art.areas.Length?Map.Portal:Map.Art.World(Map.Art.areas[regionGoal].point);
        void ShowRegions()
        {
            heading.text="NOVAS REGIÕES";bodyText.text="Quatro caminhos além da campanha atual.\n\nExplore as memórias de cada local; seu capítulo e seus checkpoints ficam preservados.";
            for(int i=6;i<10;i++){int n=i;var art=DarklightMapDefinition.ForChapter(i).Art;Button((i+1)+" - "+art.title.ToUpperInvariant(),()=>{regionSelection=n;Open("RegionDetails");});}
            Button("VOLTAR",()=>Open(regionMenuReturn));
        }
        void ShowRegionDetails()
        {
            var art=DarklightMapDefinition.ForChapter(regionSelection).Art;
            heading.text=art.title.ToUpperInvariant();bodyText.text=art.chapterTitle+"\n\n"+art.description;
            Button("EXPLORAR",()=>ExploreRegion(regionSelection));Button("VOLTAR",()=>Open("Regions"));
        }

        public void ExploreRegion(int chapter)
        {
            if(chapter<6||chapter>9)throw new ArgumentOutOfRangeException(nameof(chapter));
            StopPower();LoadMap(Progress.chapter);
            levelRoot.gameObject.SetActive(false);Destroy(levelRoot.gameObject);
            Map=DarklightMapDefinition.ForChapter(chapter);
            levelRoot=new GameObject(Map.Art.title).transform;
            DarklightMapLayout.Build(levelRoot,chapter,floorTiles,architecture,pixelMaterial);
            Physics2D.SyncTransforms();exploredRegion=chapter;regionSelection=chapter;
            regionVisited=RestoreMapAreas(chapter,Map.Art.areas.Length)??new bool[Map.Art.areas.Length];regionGoal=0;VisitedRegionAreas=0;foreach(bool visited in regionVisited)if(visited)VisitedRegionAreas++;
            lab.Player.ResetModel();Weapons.Restore();health.FullHeal();
            var body=lab.Player.GetComponent<Rigidbody2D>();body.position=Map.SafeSpawn(Map.Spawn);
            lab.Player.transform.position=new Vector3(body.position.x,body.position.y,lab.Player.transform.position.z);
            var camera=lab.WorldCamera.GetComponent<DarklightLabCamera>();camera.bounds=Map.Bounds;
            lab.WorldCamera.transform.position=lab.Player.transform.position+new Vector3(0,.75f,-10);
            Physics2D.SyncTransforms();encounterStarted=false;chapterCleared=false;
            AddProp("SaidaDaRegiao",extras[3],Map.Portal,1.3f);
            for(int i=0;i<Map.Art.areas.Length;i++)AddProp("MemoriaDaRegiao_"+i,extras[5],Map.Art.World(Map.Art.areas[i].point),.6f);
            Objective="EXPLORE "+Map.Art.areas[0].name.ToUpperInvariant();
            BuildMultiverse(chapter);InitializeRemaster(chapter);Close();RememberMapAreas();UpdateRegionExploration();Notice("E / A: LER MEMORIAS  |  ESC: MENU DA REGIAO");
        }
        void UpdateRegionExploration()
        {
            for(int i=0;i<regionVisited.Length;i++)
            {
                if(regionVisited[i]||Vector2.Distance(lab.Player.transform.position,Map.Art.World(Map.Art.areas[i].point))>2)continue;
                regionVisited[i]=true;VisitedRegionAreas++;RememberMapAreas();Notice(Map.Art.areas[i].name.ToUpperInvariant());
            }
            if(VisitedRegionAreas==regionVisited.Length){Objective="MEMÓRIAS COMPLETAS - RETORNE À SAÍDA";return;}
            for(int i=0;i<regionVisited.Length;i++)if(!regionVisited[i]){regionGoal=i;Objective="EXPLORE "+Map.Art.areas[i].name.ToUpperInvariant();break;}
        }
        void ReadRegionMemory()
        {
            if(Vector2.Distance(lab.Player.transform.position,Map.Portal)<2){Open("Regions");return;}
            foreach(var area in Map.Art.areas)
                if(Vector2.Distance(lab.Player.transform.position,Map.Art.World(area.point))<2)
                {Speak(new[]{"MEMORIA DO VEU|"+area.description});return;}
            Notice("APROXIME-SE DE UMA MEMORIA PARA LER  |  ESC: REGIOES");
        }
    }
}
