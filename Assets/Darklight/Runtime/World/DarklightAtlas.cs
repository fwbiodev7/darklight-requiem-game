using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight
{
    [DefaultExecutionOrder(110)]
    public sealed class DarklightAtlas : MonoBehaviour
    {
        sealed class Geometry
        {
            public DarklightMapDefinition map;public Rect bounds;public int width,height;
            public bool[] floor,seen;public Color32[] pixels;public Texture2D texture;public bool dirty=true;
            public int discovered;
            public DarklightAtlasRegion record;
        }
        sealed class KnownEnemy{public Vector2 position;public DarklightEnemy enemy;}
        readonly Dictionary<int,Geometry> cache=new();
        readonly Dictionary<DarklightEnemy,KnownEnemy> detected=new();
        readonly List<DarklightEnemy> expired=new();
        readonly List<DarklightMapPoint> points=new(),objectives=new(),visiblePoints=new();
        readonly List<Vector2> route=new();readonly HashSet<int> routeCells=new();
        readonly RaycastHit2D[] sightHits=new RaycastHit2D[32];
        readonly UnityEngine.UI.RawImage[] miniIcons=new UnityEngine.UI.RawImage[64],fullIcons=new UnityEngine.UI.RawImage[64];
        readonly UnityEngine.UI.Text[] objectiveLabels=new UnityEngine.UI.Text[3];
        readonly RectTransform[] objectiveRows=new RectTransform[3];
        readonly Texture2D[] symbols=new Texture2D[9],artSymbols=new Texture2D[9];
        readonly Color[] symbolColors={new Color(.96f,.95f,.9f),new Color(.94f,.72f,.24f),new Color(.32f,.65f,.95f),new Color(.85f,.1f,.18f),new Color(.72f,.035f,.12f),new Color(.27f,.8f,.43f),new Color(.68f,.31f,.92f),new Color(.72f,.67f,.6f),new Color(.5f,.85f,.92f)};
        DarklightCampaign game;DarklightCombatLab lab;Font font;
        Geometry geometry;DarklightAtlasState state;
        UnityEngine.CanvasGroup trackedContext;float trackedUntil,trackedFade;string trackedPresentationKey;
        RectTransform miniRoot,miniViewport,fullRoot,fullViewport,zoomIn,zoomOut,closeButton,centerButton,trackedRoot;
        UnityEngine.UI.RawImage miniTerrain,fullTerrain,miniArt,fullArt,cursor;
        Texture2D activeMicroart;RectTransform miniZoomIn,miniZoomOut;UnityEngine.CanvasGroup fullFadeGroup;float fullFade,miniZoom=1,shownMiniZoom=1;
        public Texture2D MicroartTexture=>activeMicroart;
        public float MiniZoom=>miniZoom;
        public bool CapturesPointer
        {
            get{var mouse=Mouse.current;return game!=null&&game.Active&&!IsOpen&&miniRoot!=null&&miniRoot.gameObject.activeInHierarchy&&mouse!=null&&Hit(miniRoot,mouse.position.ReadValue())&&(mouse.leftButton.isPressed||mouse.leftButton.wasPressedThisFrame||mouse.rightButton.isPressed||mouse.rightButton.wasPressedThisFrame||mouse.scroll.ReadValue().y!=0);}
        }
        public bool StructureRevealed=>geometry!=null&&activeMicroart!=null;
        UnityEngine.UI.Text miniTitle,fullTitle,description,status,trackedHUD,trackedTitle,trackedDistance,miniBinding;
        UnityEngine.UI.RawImage trackedIcon,routeArrow;
        UnityEngine.UI.Text routeHint;
        Vector2 center,lastReveal=new(float.PositiveInfinity,0),padCursor;
        float zoom=1,nextReveal,nextMarkers,nextRoute,nextNavigate,nextSave;
        int region=-1,focus,firstObjective;bool pendingSave;
        string storage;
        public bool IsOpen {get;private set;}
        public bool Available=>game!=null&&game.Map?.Art!=null;
        public int TerrainUploads {get;private set;}
        public int RevealQueries {get;private set;}
        public int VisibleEnemyCount {get;private set;}
        public double LastUpdateMilliseconds {get;private set;}
        public float Zoom=>zoom;
        public string TrackedObjective=>geometry?.record.tracked??"";
        public int DiscoveredCount=>geometry?.discovered??0;
        public string StoragePath=>storage;
        public void Initialize(DarklightCampaign owner,DarklightCombatLab arena,RectTransform hud,RectTransform overlay,Font pixelFont)
        {
            game=owner;lab=arena;font=pixelFont;storage=Path.Combine(Application.persistentDataPath,"requiem-map.json");
            state=DarklightAutomation.Silent?new DarklightAtlasState():DarklightAtlasState.Load(storage);
            MakeSymbols();
            miniRoot=Rect(hud,"Minimapa",512,8,120,82);DarklightHUDTheme.Frame(miniRoot);
            miniTitle=Text(miniRoot,"Local","MAPA",6,5,61,10,5);
            miniBinding=Text(miniRoot,"Atalho","",61,5,27,10,4);miniBinding.alignment=TextAnchor.MiddleRight;
            miniZoomOut=Rect(miniRoot,"DiminuirZoom",90,4,11,12);Panel(miniZoomOut,new Color(.09f,.035f,.05f));Text(miniZoomOut,"Rotulo","-",0,0,11,12,5);
            miniZoomIn=Rect(miniRoot,"AumentarZoom",103,4,11,12);Panel(miniZoomIn,new Color(.09f,.035f,.05f));Text(miniZoomIn,"Rotulo","+",0,0,11,12,5);
            miniViewport=Rect(miniRoot,"Territorio",4,18,112,60);Panel(miniViewport,Color.black);miniViewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();miniArt=Raw(miniViewport,"Microarte",0,0,112,60,null);miniTerrain=Raw(miniViewport,"Geometria",0,0,112,60,null);MakeIcons(miniViewport,miniIcons,4);
            trackedRoot=Rect(hud,"DestinoAcompanhado",508,94,124,22);trackedContext=trackedRoot.gameObject.AddComponent<UnityEngine.CanvasGroup>();trackedContext.alpha=0;trackedContext.blocksRaycasts=false;DarklightHUDTheme.Frame(trackedRoot);
            trackedIcon=Raw(trackedRoot,"Simbolo",7,8,8,8,symbols[1]);trackedIcon.color=symbolColors[1];
            trackedTitle=Text(trackedRoot,"Titulo","",21,5,72,11,5);trackedDistance=Text(trackedRoot,"Distancia","",94,5,24,11,5);trackedDistance.alignment=TextAnchor.MiddleRight;
            trackedHUD=Text(trackedRoot,"Rotulo","",21,19,98,13,5);
            routeArrow=Raw(trackedRoot,"DirecaoDoDestino",7,35,8,8,symbols[0]);routeArrow.color=symbolColors[1];
            routeHint=Text(trackedRoot,"Orientacao","",21,35,98,11,4);
            trackedHUD.gameObject.SetActive(false);routeHint.gameObject.SetActive(false);trackedIcon.gameObject.SetActive(false);routeArrow.rectTransform.anchoredPosition=new Vector2(7,-7);
            overlay.SetAsLastSibling();
            fullRoot=Rect(overlay,"MapaDaFase",0,0,640,360);fullFadeGroup=fullRoot.gameObject.AddComponent<UnityEngine.CanvasGroup>();fullFadeGroup.alpha=0;Panel(fullRoot,new Color(.012f,.012f,.019f,1));
            MapPanel(Rect(fullRoot,"MolduraDoAtlas",8,6,624,348),0);
            Text(fullRoot,"Identidade","DARKLIGHT: REQUIEM  /  ATLAS DE VESPERA",24,14,490,12,4);
            fullTitle=Text(fullRoot,"Local","MAPA DA FASE",24,29,490,22,8);
            MapPanel(Rect(fullRoot,"MolduraDoTerritorio",16,64,412,242),0);
            Text(fullRoot,"TerritorioTitulo","TERRITORIO",25,54,200,10,4);
            fullViewport=Rect(fullRoot,"Territorio",24,74,396,224);Panel(fullViewport,Color.black);
            fullViewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();fullArt=Raw(fullViewport,"Microarte",0,0,396,224,null);fullTerrain=Raw(fullViewport,"Geometria",0,0,396,224,null);MakeIcons(fullViewport,fullIcons,8);
            cursor=Raw(fullViewport,"CursorDoControle",194,108,8,8,symbols[(int)DarklightMapSymbol.Waypoint]);cursor.color=Color.white;
            zoomOut=SmallButton("Menos","-",300,47,22);zoomIn=SmallButton("Mais","+",325,47,22);centerButton=SmallButton("Centrar","CENTRAR",350,47,70);closeButton=SmallButton("Fechar","FECHAR",552,19,65);
            MapPanel(Rect(fullRoot,"PainelDaJornada",438,51,186,274),0);
            Text(fullRoot,"ObjetivosTitulo","OBJETIVOS",450,61,162,12,5);
            for(int i=0;i<3;i++){objectiveRows[i]=Rect(fullRoot,"Objetivo"+i,448,77+i*19,168,17);MapPanel(objectiveRows[i],1);objectiveLabels[i]=Text(objectiveRows[i],"Rotulo","",7,0,154,17,5);}
            MapPanel(Rect(fullRoot,"MolduraDaDescricao",448,138,168,60),0);
            description=Text(fullRoot,"Descricao","",455,145,154,46,5);description.lineSpacing=1.5f;description.resizeTextForBestFit=true;description.resizeTextMinSize=4;description.resizeTextMaxSize=5;
            Text(fullRoot,"LegendaTitulo","LEGENDA",450,204,162,10,4);
            string[] legend={"Jogador","Objetivo principal","Objetivo secundário","Inimigo detectado","Chefe conhecido","Checkpoint","Saída liberada"};
            for(int i=0;i<legend.Length;i++){var icon=Raw(fullRoot,"LegendaIcone"+i,449,218+i*15,14,14,artSymbols[i]);icon.color=Color.white;Text(fullRoot,"Legenda"+i,legend[i],469,219+i*15,145,12,5);}
            status=Text(fullRoot,"Estado","",24,309,396,12,4);
            MapPanel(Rect(fullRoot,"RodapeDoAtlas",16,328,608,24),1);
            Text(fullRoot,"Navegacao","TAB / SELECT: FECHAR   WASD / LS: MOVER   +/- / LT-RT: ZOOM\nMOUSE DIR.: ARRASTAR   SETAS / D-PAD: OBJETIVO   ENTER / A: SEGUIR   X: DESTINO   Y: LIMPAR",25,333,590,18,4);
            padCursor=new Vector2(198,112);fullRoot.gameObject.SetActive(false);miniRoot.gameObject.SetActive(false);
        }
        RectTransform SmallButton(string name,string label,float x,float y,float w)
        {var r=Rect(fullRoot,name,x,y,w,15);MapPanel(r,1);var t=Text(r,"Rotulo",label,3,0,w-6,15,5);t.alignment=TextAnchor.MiddleCenter;return r;}
        void MapPanel(RectTransform rect,int index)
        {
            var image=rect.GetComponent<UnityEngine.UI.Image>();if(image==null)image=rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite=game.uiArt!=null&&game.uiArt.Length>index?game.uiArt[index]:null;image.material=null;
            image.type=UnityEngine.UI.Image.Type.Sliced;image.pixelsPerUnitMultiplier=8.333333f;image.color=Color.white;image.raycastTarget=false;
        }
        public void UseStorage(string path){Flush();storage=path;state=DarklightAtlasState.Load(path);cache.Clear();geometry=null;region=-1;}
        public void ResetCampaign()
        {
            state.regions.RemoveAll(r=>r.id<6);foreach(int id in new[]{0,1,2,3,4,5})if(cache.TryGetValue(id,out var g)){Destroy(g.texture);cache.Remove(id);}
            geometry=null;region=-1;pendingSave=true;Flush();
        }
        public bool[] SavedAreas(int id,int count)
        {var r=state?.regions.Find(x=>x.id==id);return r?.visitedAreas?.Length==count?(bool[])r.visitedAreas.Clone():null;}
        public void RememberAreas(int id,bool[] visited)
        {if(state==null||visited==null)return;EnsureGeometry();var r=state.regions.Find(x=>x.id==id);if(r==null)return;r.visitedAreas=(bool[])visited.Clone();pendingSave=true;}
        void EnsureGeometry()
        {
            if(!Available)return;
            int id=game.MapRegion;if(region==id&&geometry?.map==game.Map)return;
            Flush();region=id;detected.Clear();route.Clear();routeCells.Clear();lastReveal=new Vector2(float.PositiveInfinity,0);
            if(!cache.TryGetValue(id,out geometry))
            {
                var grid=game.Map.GridBounds;geometry=new Geometry{map=game.Map,width=grid.width,height=grid.height};
                geometry.bounds=new Rect(grid.x*game.Map.CellSize,grid.y*game.Map.CellSize,grid.width*game.Map.CellSize,grid.height*game.Map.CellSize);
                geometry.record=state.Region(id,grid.width,grid.height);geometry.seen=DarklightAtlasState.DecodeFog(geometry.record.fog,grid.width*grid.height);
                geometry.floor=new bool[geometry.seen.Length];geometry.pixels=new Color32[geometry.seen.Length];foreach(bool known in geometry.seen)if(known)geometry.discovered++;
                for(int y=0;y<grid.height;y++)for(int x=0;x<grid.width;x++)geometry.floor[y*grid.width+x]=game.Map.Walkable(new Vector2Int(grid.x+x,grid.y+y));
                geometry.texture=new Texture2D(grid.width,grid.height,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="MapaConhecido_"+id};cache[id]=geometry;
            }
            var previousMicroart=activeMicroart;activeMicroart=Resources.Load<Texture2D>("Darklight/Minimap/Map"+(id+1));
            miniArt.texture=fullArt.texture=activeMicroart;miniArt.enabled=fullArt.enabled=activeMicroart!=null;
            if(previousMicroart!=null&&previousMicroart!=activeMicroart)Resources.UnloadAsset(previousMicroart);
            miniTerrain.texture=fullTerrain.texture=geometry.texture;center=geometry.bounds.center;zoom=1;
            fullTitle.text=id>=0&&id<DarklightStory.Places.Length?DarklightStory.Places[id].ToUpperInvariant():!string.IsNullOrWhiteSpace(game.Map.Art?.title)?game.Map.Art.title.ToUpperInvariant():"MAPA DA FASE";miniTitle.text="MAPA "+(id+1);
            nextReveal=nextMarkers=nextRoute=0;geometry.dirty=true;focus=0;
        }
        int Index(Vector2 world)
        {
            if(geometry==null||!geometry.bounds.Contains(world))return -1;
            int x=Mathf.FloorToInt((world.x-geometry.bounds.xMin)/geometry.map.CellSize),y=Mathf.FloorToInt((world.y-geometry.bounds.yMin)/geometry.map.CellSize);
            return x>=0&&x<geometry.width&&y>=0&&y<geometry.height?y*geometry.width+x:-1;
        }
        public bool Known(Vector2 world)=>Index(world)>=0;
        public bool Observed(Vector2 world){int i=Index(world);return i>=0&&geometry.seen[i];}
        bool MarkerKnown(DarklightMapPoint point)=>point.symbol==DarklightMapSymbol.Primary||point.symbol==DarklightMapSymbol.Checkpoint||point.symbol==DarklightMapSymbol.Exit||Observed(point.position);
        Vector2 CellCenter(int x,int y)=>geometry.bounds.min+new Vector2(x+.5f,y+.5f)*geometry.map.CellSize;
        bool Sight(Vector2 from,Vector2 to,float margin=0)
        {
            Vector2 delta=to-from;if(delta.sqrMagnitude<.001f)return true;
            var filter=new ContactFilter2D{useLayerMask=true,layerMask=Physics2D.DefaultRaycastLayers,useTriggers=Physics2D.queriesHitTriggers};
            int count=Physics2D.Raycast(from,delta.normalized,filter,sightHits,delta.magnitude);
            if(count==sightHits.Length)return false;
            for(int i=0;i<count;i++)if(!sightHits[i].collider.isTrigger&&sightHits[i].collider.GetComponentInParent<DarklightCombatant>()==null&&sightHits[i].distance<delta.magnitude-margin)return false;
            return true;
        }
        void Reveal()
        {
            if(geometry==null||!game.Active)return;
            Vector2 p=lab.Player.transform.position;if((p-lastReveal).sqrMagnitude<geometry.map.CellSize*geometry.map.CellSize*.16f)return;lastReveal=p;
            float cell=geometry.map.CellSize;int px=Mathf.FloorToInt((p.x-geometry.bounds.xMin)/cell),py=Mathf.FloorToInt((p.y-geometry.bounds.yMin)/cell),radius=Mathf.CeilToInt(6/cell);
            for(int y=Mathf.Max(0,py-radius);y<=Mathf.Min(geometry.height-1,py+radius);y++)for(int x=Mathf.Max(0,px-radius);x<=Mathf.Min(geometry.width-1,px+radius);x++)
            {
                int i=y*geometry.width+x;if(geometry.seen[i])continue;var pos=CellCenter(x,y);if((pos-p).sqrMagnitude>36)continue;
                RevealQueries++;if(!Sight(p,pos,cell*.72f))continue;geometry.seen[i]=true;geometry.discovered++;geometry.dirty=true;pendingSave=true;
            }
        }
        void RefreshPoints()
        {
            points.Clear();visiblePoints.Clear();objectives.Clear();if(geometry==null)return;game.FillMapObjectives(points);
            foreach(var p in points)
            {
                bool known=MarkerKnown(p);if(known)visiblePoints.Add(p);
                if(p.symbol==DarklightMapSymbol.Primary||known&&p.trackable)objectives.Add(p);
            }
            var valid=points.Find(p=>p.id==geometry.record.tracked&&!p.completed);
            if(valid==null&&geometry.record.tracked!=""){geometry.record.tracked="";pendingSave=true;}
            var alive=game.MapEnemies;expired.Clear();foreach(var kv in detected)
                if(kv.Value.enemy==null||kv.Value.enemy.Health.IsDead||kv.Value.enemy.Resolved)expired.Add(kv.Key);
            foreach(var id in expired)detected.Remove(id);
            Vector2 player=lab.Player.transform.position;
            foreach(var enemy in alive)
            {
                if(enemy==null||enemy.Health.IsDead||enemy.Resolved)continue;Vector2 position=enemy.transform.position;
                if(Vector2.Distance(player,position)<=7&&Known(position)&&Sight(player,position))
                {
                    if(!detected.TryGetValue(enemy,out var record)){record=new KnownEnemy{enemy=enemy};detected[enemy]=record;}record.position=position;
                }
            }
            VisibleEnemyCount=detected.Count;
            foreach(var kv in detected){var e=kv.Value;visiblePoints.Add(new DarklightMapPoint{id="enemy_"+kv.Key.GetHashCode(),title=e.enemy.Boss?"Chefe conhecido":"Inimigo detectado",description="Última posição observada.",position=e.position,symbol=e.enemy.Boss?DarklightMapSymbol.Boss:DarklightMapSymbol.Enemy,trackable=false});}
            if(geometry.record.hasWaypoint)visiblePoints.Add(new DarklightMapPoint{id="waypoint",title="Destino marcado",description="Destino escolhido por você.",position=geometry.record.waypoint,symbol=DarklightMapSymbol.Waypoint});
            focus=Mathf.Clamp(focus,0,Mathf.Max(0,objectives.Count-1));
            var tracked=points.Find(x=>x.id==geometry.record.tracked&&!x.completed)??points.Find(x=>x.symbol==DarklightMapSymbol.Primary&&!x.completed);
            if(tracked==null)tracked=points.Find(x=>x.symbol==DarklightMapSymbol.Exit&&!x.completed&&Known(x.position));
            trackedRoot.gameObject.SetActive(game.Active&&tracked!=null);
            string presentation=tracked==null?null:region+":"+tracked.id;if(presentation!=trackedPresentationKey){trackedPresentationKey=presentation;trackedUntil=Time.unscaledTime+6;}
            trackedTitle.text=tracked==null?"":tracked.id.StartsWith("seal_")&&int.TryParse(tracked.id.Substring(5),out int seal)?seal<3?"SELO "+(seal+1)+" / 3":"OBJETIVO":tracked.title.ToUpperInvariant();
            if(trackedTitle.text.Length>14)trackedTitle.text=trackedTitle.text.Substring(0,12)+"..";
            trackedDistance.text=tracked==null?"":Vector2.Distance(player,tracked.position).ToString("0")+"m";
            trackedHUD.text=tracked==null?"":tracked.symbol==DarklightMapSymbol.Primary?(game.ExploringNewRegion?"Explore a memória":tracked.id.StartsWith("seal_")?"Liberte o selo":tracked.description):tracked.symbol==DarklightMapSymbol.Exit?"Interaja para sair":tracked.symbol==DarklightMapSymbol.Secondary?"Recupere a memória":tracked.symbol==DarklightMapSymbol.Checkpoint?"Ponto de retorno":"Converse com o aliado";
            if(tracked!=null){trackedIcon.texture=symbols[(int)tracked.symbol];trackedIcon.color=symbolColors[(int)tracked.symbol];}
            RefreshObjectiveUI();
        }
        void RefreshObjectiveUI()
        {
            firstObjective=Mathf.Max(0,focus-2);
            for(int row=0;row<3;row++)
            {
                int i=firstObjective+row;bool visible=i<objectives.Count;objectiveRows[row].gameObject.SetActive(visible);if(!visible)continue;
                var p=objectives[i];objectiveLabels[row].text=(p.completed?"CONCLUÍDO: ":geometry.record.tracked==p.id?"SEGUINDO: ":"")+p.title;
                objectiveLabels[row].color=p.completed?new Color(.43f,.45f,.48f):i==focus?symbolColors[(int)p.symbol]:new Color(.8f,.78f,.73f);
                MapPanel(objectiveRows[row],i==focus?2:1);
            }
            var item=focus<objectives.Count?objectives[focus]:null;
            description.text=item==null?"Explore para descobrir objetivos e pontos de interesse.":item.description+(!Known(item.position)?"\n\nO local ainda não foi explorado.":"");
        }
        DarklightMapPoint RouteGoal()=>geometry.record.hasWaypoint?new DarklightMapPoint{position=geometry.record.waypoint}:points.Find(p=>p.id==geometry.record.tracked&&!p.completed)??points.Find(p=>p.symbol==DarklightMapSymbol.Primary&&!p.completed)??points.Find(p=>p.symbol==DarklightMapSymbol.Exit&&!p.completed&&Known(p.position));
        void RefreshRoute()
        {
            routeCells.Clear();route.Clear();var target=RouteGoal();
            if(target!=null&&Known(target.position)&&DarklightNavigation.Instance!=null&&DarklightNavigation.Instance.FindPath(lab.Player.transform.position,target.position,route))
            {
                bool known=true;foreach(var p in route)if(!Known(p)){known=false;break;}
                if(known)foreach(var p in route){int i=Index(p);if(i>=0)routeCells.Add(i);}
            }
            geometry.dirty=true;
        }
        void UpdateRouteGuide()
        {
            var target=geometry==null?null:RouteGoal();
            bool known=target!=null&&Known(target.position);
            routeArrow.gameObject.SetActive(known);
            if(!known){routeHint.text=lab.Input.BindingLabel("Map")+": EXPLORE O MAPA";return;}
            Vector2 player=lab.Player.transform.position,destination=target.position;
            bool followsRoute=routeCells.Count>0;
            if(followsRoute)foreach(var step in route)if(Vector2.Distance(player,step)>1.2f){destination=step;break;}
            Vector2 direction=destination-player;
            routeArrow.rectTransform.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg-90);
            routeHint.text=Vector2.Distance(player,target.position)<2?lab.Input.BindingLabel("Interact")+": INTERAGIR":followsRoute?"SIGA A ROTA DOURADA":"DESTINO / "+lab.Input.BindingLabel("Map");
        }
        void UploadTerrain()
        {
            if(!geometry.dirty)return;
            for(int i=0;i<geometry.seen.Length;i++)geometry.pixels[i]=routeCells.Contains(i)?new Color32(210,160,57,190):activeMicroart!=null?geometry.floor[i]?new Color32(0,0,0,0):new Color32(0,0,0,62):geometry.floor[i]?new Color32(81,84,94,255):new Color32(27,29,39,255);
            geometry.texture.SetPixels32(geometry.pixels);geometry.texture.Apply(false,false);geometry.dirty=false;TerrainUploads++;
        }
        public void RefreshNow()
        {
            EnsureGeometry();if(geometry==null)return;Reveal();RefreshPoints();RefreshRoute();UploadTerrain();Draw();
        }
        void LateUpdate()
        {
            bool measure=Application.isEditor||Debug.isDebugBuild;
            long started=measure?System.Diagnostics.Stopwatch.GetTimestamp():0;
            UpdateAtlas();
            if(measure)LastUpdateMilliseconds=(System.Diagnostics.Stopwatch.GetTimestamp()-started)*1000d/System.Diagnostics.Stopwatch.Frequency;
        }
        void UpdateAtlas()
        {
            if(miniRoot==null)return;
            miniRoot.gameObject.SetActive(Available&&game.Active);fullFade=Mathf.MoveTowards(fullFade,IsOpen?1:0,Time.unscaledDeltaTime*10);fullFadeGroup.alpha=fullFade;fullRoot.gameObject.SetActive(IsOpen||fullFade>0);shownMiniZoom=Mathf.Lerp(shownMiniZoom,miniZoom,1-Mathf.Exp(-Time.unscaledDeltaTime*14));
            if(!Available){trackedFade=0;trackedContext.alpha=0;trackedRoot.gameObject.SetActive(false);return;}EnsureGeometry();miniBinding.text="["+lab.Input.BindingLabel("Map")+"]";float now=Time.unscaledTime;
            if(now>=nextReveal){nextReveal=now+.2f;Reveal();}
            if(now>=nextMarkers){nextMarkers=now+.2f;RefreshPoints();}
            if(now>=nextRoute&&(game.Active||IsOpen)){nextRoute=now+.8f;RefreshRoute();}
            UploadTerrain();Draw();UpdateRouteGuide();
            trackedFade=Mathf.MoveTowards(trackedFade,game.Active&&!string.IsNullOrEmpty(trackedTitle.text)&&now<trackedUntil?1:0,Time.unscaledDeltaTime*6);trackedContext.alpha=trackedFade;trackedRoot.gameObject.SetActive(trackedFade>.001f);
            if(pendingSave&&now>=nextSave){nextSave=now+15;Flush();}
        }
        public bool TryOpen()
        {
            if(!Available){game?.Notice("MAPA INDISPONÍVEL NESTE LOCAL");return false;}
            RefreshNow();IsOpen=true;center=geometry.bounds.center;zoom=1;padCursor=WorldToFull(lab.Player.transform.position);nextNavigate=Time.unscaledTime+.15f;
            game.OpenAtlasPanel();fullRoot.gameObject.SetActive(true);Draw();return true;
        }
        public void CloseMap(){if(!IsOpen)return;IsOpen=false;Flush();fullRoot.gameObject.SetActive(false);game.CloseAtlasPanel();}
        public void TrackFocused()
        {
            if(geometry==null||focus>=objectives.Count||objectives[focus].completed)return;
            geometry.record.tracked=objectives[focus].id;pendingSave=true;nextRoute=0;RefreshObjectiveUI();
        }
        public bool SetWaypoint(Vector2 world)
        {
            int i=Index(world);if(i<0||!geometry.floor[i])return false;
            if(!DarklightNavigation.Clear(world,world+Vector2.right*.001f,.3f))return false;
            geometry.record.waypoint=world;geometry.record.hasWaypoint=true;pendingSave=true;nextRoute=0;RefreshPoints();return true;
        }
        public void ClearWaypoint(){if(geometry==null)return;geometry.record.hasWaypoint=false;pendingSave=true;nextRoute=0;RefreshPoints();}
        public bool TickInput()
        {
            if(miniRoot==null)return false;
            var input=lab.Input;var mouse=Mouse.current;
            if(input.Menu("Map").WasPressedThisFrame()&&(game.Active||IsOpen||game.IsPausePanel))
            {if(IsOpen)CloseMap();else TryOpen();return true;}
            if(!IsOpen)
            {
                if(game.Active&&mouse!=null&&Hit(miniRoot,mouse.position.ReadValue()))
                {
                    float wheel=mouse.scroll.ReadValue().y;if(wheel!=0){SetMiniZoom(miniZoom+Mathf.Sign(wheel)*.25f);return true;}
                    if(mouse.leftButton.wasPressedThisFrame)
                    {if(Hit(miniZoomIn,mouse.position.ReadValue()))SetMiniZoom(miniZoom+.25f);else if(Hit(miniZoomOut,mouse.position.ReadValue()))SetMiniZoom(miniZoom-.25f);else TryOpen();return true;}
                }
                return false;
            }
            if(input.Menu("Cancel").WasPressedThisFrame()||input.Menu("Pause").WasPressedThisFrame()){CloseMap();return true;}
            Vector2 pan=input.Menu("MapPan").ReadValue<Vector2>();center+=pan*Time.unscaledDeltaTime*18/zoom;
            Vector2 moveCursor=input.Menu("MapCursor").ReadValue<Vector2>();padCursor+=new Vector2(moveCursor.x,-moveCursor.y)*Time.unscaledDeltaTime*115;
            padCursor=new Vector2(Mathf.Clamp(padCursor.x,4,392),Mathf.Clamp(padCursor.y,4,220));
            float change=input.Menu("MapZoomIn").ReadValue<float>()-input.Menu("MapZoomOut").ReadValue<float>();
            if(Mathf.Abs(change)>.01f)SetZoom(zoom+change*Time.unscaledDeltaTime*1.4f);
            if(input.Menu("MapCenter").WasPressedThisFrame()){center=lab.Player.transform.position;padCursor=new Vector2(198,112);}
            float nav=input.Menu("MapObjective").ReadValue<float>();if(Mathf.Abs(nav)<.4f)nextNavigate=0;
            else if(Time.unscaledTime>=nextNavigate){focus=objectives.Count==0?0:(focus+(nav>0?-1:1)+objectives.Count)%objectives.Count;nextNavigate=Time.unscaledTime+.2f;RefreshObjectiveUI();}
            if(input.Menu("Submit").WasPressedThisFrame())TrackFocused();
            if(input.Menu("MapMark").WasPressedThisFrame())SetWaypoint(ViewToWorld(padCursor));
            if(input.Menu("MapClear").WasPressedThisFrame())ClearWaypoint();
            if(mouse!=null)
            {
                Vector2 pos=mouse.position.ReadValue();bool click=mouse.leftButton.wasPressedThisFrame;
                if(click&&Hit(closeButton,pos)){CloseMap();return true;}
                if(click&&Hit(zoomIn,pos))SetZoom(zoom+.25f);
                if(click&&Hit(zoomOut,pos))SetZoom(zoom-.25f);
                if(click&&Hit(centerButton,pos))center=lab.Player.transform.position;
                for(int r=0;r<3;r++)if(click&&objectiveRows[r].gameObject.activeSelf&&Hit(objectiveRows[r],pos)){focus=firstObjective+r;TrackFocused();}
                if(Hit(fullViewport,pos))
                {
                    Vector2 local=Local(fullViewport,pos);float wheel=mouse.scroll.ReadValue().y;
                    if(wheel!=0){Vector2 before=ViewToWorld(local);SetZoom(zoom+Mathf.Sign(wheel)*.25f);center+=before-ViewToWorld(local);}
                    if(mouse.rightButton.isPressed){var delta=mouse.delta.ReadValue()/lab.PixelHUD.canvas.scaleFactor;center+=new Vector2(-delta.x,-delta.y)/MapScale;}
                    if(click)
                    {
                        var world=ViewToWorld(local);DarklightMapPoint nearest=null;float distance=8/MapScale;
                        foreach(var p in objectives)if(Known(p.position)&&Vector2.Distance(world,p.position)<distance){nearest=p;distance=Vector2.Distance(world,p.position);}
                        if(nearest!=null){focus=objectives.IndexOf(nearest);TrackFocused();}else SetWaypoint(world);
                    }
                }
            }
            center=new Vector2(Mathf.Clamp(center.x,geometry.bounds.xMin,geometry.bounds.xMax),Mathf.Clamp(center.y,geometry.bounds.yMin,geometry.bounds.yMax));Draw();return true;
        }
        float MapScale=>Mathf.Min(396/geometry.bounds.width,224/geometry.bounds.height)*zoom;
        Vector2 ViewToWorld(Vector2 p)=>center+new Vector2(p.x-198,112-p.y)/MapScale;
        public Vector2 WorldToFull(Vector2 p)=>new Vector2(198+(p.x-center.x)*MapScale,112-(p.y-center.y)*MapScale);
        float MiniScale=>Mathf.Min(miniViewport.rect.width/28f,miniViewport.rect.height/18f)*shownMiniZoom;
        public Vector2 WorldToMini(Vector2 p){Vector2 player=lab.Player.transform.position;return new Vector2(miniViewport.rect.width*.5f+(p.x-player.x)*MiniScale,miniViewport.rect.height*.5f-(p.y-player.y)*MiniScale);}
        public Vector2 MiniToWorld(Vector2 p){Vector2 player=lab.Player.transform.position;return player+new Vector2(p.x-miniViewport.rect.width*.5f,miniViewport.rect.height*.5f-p.y)/MiniScale;}
        public void SetMiniZoom(float value){miniZoom=Mathf.Clamp(value,.65f,3);}
        public void SetZoom(float value){zoom=Mathf.Clamp(value,.65f,4);SetMiniZoom(value);}
        void Draw()
        {
            if(geometry==null)return;
            float scale=MapScale,w=geometry.bounds.width*scale,h=geometry.bounds.height*scale;
            var fullPosition=new Vector2(198+(geometry.bounds.xMin-center.x)*scale,-(112-(geometry.bounds.yMax-center.y)*scale));
            fullTerrain.rectTransform.anchoredPosition=fullArt.rectTransform.anchoredPosition=fullPosition;fullTerrain.rectTransform.sizeDelta=fullArt.rectTransform.sizeDelta=new Vector2(w,h);
            Vector2 player=lab.Player.transform.position;float miniScale=MiniScale;
            var miniPosition=new Vector2(miniViewport.rect.width*.5f+(geometry.bounds.xMin-player.x)*miniScale,-(miniViewport.rect.height*.5f-(geometry.bounds.yMax-player.y)*miniScale));
            miniTerrain.rectTransform.anchoredPosition=miniArt.rectTransform.anchoredPosition=miniPosition;miniTerrain.rectTransform.sizeDelta=miniArt.rectTransform.sizeDelta=geometry.bounds.size*miniScale;
            int count=0;
            foreach(var p in visiblePoints){if(count>=miniIcons.Length-1)break;PlaceIcon(count++,p.position,p.symbol,p.completed);}
            PlaceIcon(count++,lab.Player.transform.position,DarklightMapSymbol.Player,false);
            miniIcons[count-1].transform.SetAsLastSibling();fullIcons[count-1].transform.SetAsLastSibling();
            for(int i=count;i<miniIcons.Length;i++){miniIcons[i].gameObject.SetActive(false);fullIcons[i].gameObject.SetActive(false);}
            cursor.rectTransform.anchoredPosition=new Vector2(padCursor.x-4,-(padCursor.y-4));cursor.gameObject.SetActive(IsOpen&&lab.Input.UsingGamepad);
            status.text="FASE REVELADA   ZOOM "+zoom.ToString("0.0")+"x"+(routeCells.Count>0?"   ROTA DO OBJETIVO":"   MARQUE UM DESTINO");
        }
        void PlaceIcon(int i,Vector2 p,DarklightMapSymbol symbol,bool complete)
        {
            void Place(UnityEngine.UI.RawImage icon,Vector2 point,int size,bool full)
            {
                bool inside=full?point.x>=0&&point.x<=396&&point.y>=0&&point.y<=224:point.x>=0&&point.x<=miniViewport.rect.width&&point.y>=0&&point.y<=miniViewport.rect.height;
                icon.gameObject.SetActive(inside);icon.texture=full?artSymbols[(int)symbol]:symbols[(int)symbol];icon.color=complete?new Color(.38f,.4f,.44f):full?Color.white:symbolColors[(int)symbol];
                icon.rectTransform.sizeDelta=Vector2.one*(full?(symbol==DarklightMapSymbol.Player?14:12):symbol==DarklightMapSymbol.Player?7:size);
                icon.rectTransform.anchoredPosition=new Vector2(Mathf.Round(point.x),-Mathf.Round(point.y));
                icon.rectTransform.localRotation=symbol==DarklightMapSymbol.Player?Quaternion.Euler(0,0,Mathf.Atan2(lab.Player.Model.Facing.y,lab.Player.Model.Facing.x)*Mathf.Rad2Deg-90):Quaternion.identity;
            }
            Place(miniIcons[i],WorldToMini(p),4,false);Place(fullIcons[i],WorldToFull(p),8,true);
        }
        public string Snapshot()
        {
            foreach(var kv in cache)kv.Value.record.fog=DarklightAtlasState.EncodeFog(kv.Value.seen);
            return JsonUtility.ToJson(state);
        }
        public void Flush()
        {
            if(state==null||!pendingSave)return;Snapshot();if(!DarklightAutomation.Silent&&!DarklightAutomation.ProtectSaves&&!game.PlaytestSession)
            {try{state.Save(storage);}catch(IOException e){Debug.LogWarning("Mapa: "+e.Message);return;}}pendingSave=false;
        }
        void MakeSymbols()
        {
            string[][] masks={
                new[]{"............",".....##.....","....####....","....####....","...######...","...######...","..########..","..###..###..",".....##.....",".....##.....","............","............"},
                new[]{"............",".....##.....","....####....","...######...","..###..###..",".####..####.",".####..####.","..########..","...##..##...","....####....",".....##.....","............"},
                new[]{"............",".....##.....","....####....","...##..##...","..##....##..",".##......##.",".##......##.","..##....##..","...##..##...","....####....",".....##.....","............"},
                new[]{"............",".....##.....","....####....","...######...","..###..###..","..###..###..",".####..####.",".##########.",".####..####.",".##########.","............","............"},
                new[]{"............","..#..##..#..","..########..",".##########.",".##..##..##.",".##..##..##.",".##########.","..###..###..","...######...","...#.#..#...","...######...","............"},
                new[]{"............",".....##.....","....####....","....####....","...######...","...##..##...","...######...",".....##.....",".....##.....","..########..","..########..","............"},
                new[]{"............","...######...","..########..","..##....##..","..##....##..","..##....##..","..##.##.##..","..##.##.##..","..##.##.##..","..########..","..########..","............"},
                new[]{"............","..########..",".##########.",".##......##.",".##......##.",".##......##.",".##########.","..########..","...###......","...##.......","...#........","............"},
                new[]{"............","....####....","...######...","..##....##..","..##....##..","..##....##..","...######...","....####....",".....##.....",".....##.....","............","............"}};
            const int size=12;
            for(int n=0;n<9;n++)
            {
                var t=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp,name="IconeMapa_"+(DarklightMapSymbol)n};
                var data=new Color32[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    bool filled=masks[n][y][x]=='#',edge=false;
                    if(!filled)for(int oy=-1;oy<=1;oy++)for(int ox=-1;ox<=1;ox++)
                    {int px=x+ox,py=y+oy;if(px>=0&&px<size&&py>=0&&py<size&&masks[n][py][px]=='#')edge=true;}
                    data[(size-1-y)*size+x]=filled?new Color32(255,255,255,255):edge?new Color32(9,7,13,255):new Color32(0,0,0,0);
                }
                t.SetPixels32(data);t.Apply();symbols[n]=t;
                artSymbols[n]=Resources.Load<Texture2D>("Darklight/Minimap/Icons/"+(DarklightMapSymbol)n);if(artSymbols[n]==null)artSymbols[n]=t;
            }
        }
        void MakeIcons(Transform parent,UnityEngine.UI.RawImage[] icons,int size){for(int i=0;i<icons.Length;i++){icons[i]=Raw(parent,"Marcador"+i,0,0,size,size,null);icons[i].rectTransform.pivot=Vector2.one*.5f;icons[i].gameObject.SetActive(false);}}
        static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
        static void Panel(RectTransform r,Color color){var i=r.gameObject.AddComponent<UnityEngine.UI.Image>();i.color=color;i.raycastTarget=false;}
        static void Border(RectTransform r,float w,float h)
        {for(int n=0;n<4;n++){var edge=Rect(r,"Borda"+n,n==3?w-.5f:0,n==1?h-.5f:0,n<2?w:.5f,n<2?.5f:h);Panel(edge,new Color(.27f,.3f,.34f));}}
        UnityEngine.UI.Text Text(Transform parent,string name,string value,float x,float y,float w,float h,int size)
        {var t=Rect(parent,name,x,y,w,h).gameObject.AddComponent<UnityEngine.UI.Text>();t.font=font;t.fontSize=size;t.text=value;t.color=new Color(.89f,.88f,.83f);t.raycastTarget=false;t.lineSpacing=1.7f;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;return t;}
        static UnityEngine.UI.RawImage Raw(Transform parent,string name,float x,float y,float w,float h,Texture texture)
        {var i=Rect(parent,name,x,y,w,h).gameObject.AddComponent<UnityEngine.UI.RawImage>();i.texture=texture;i.raycastTarget=false;return i;}
        static bool Hit(RectTransform r,Vector2 p)=>RectTransformUtility.RectangleContainsScreenPoint(r,p);
        static Vector2 Local(RectTransform r,Vector2 p){RectTransformUtility.ScreenPointToLocalPointInRectangle(r,p,null,out var local);return new Vector2(local.x,-local.y);}
        void OnDestroy()
        {Flush();foreach(var g in cache.Values)if(g.texture!=null)Destroy(g.texture);foreach(var t in symbols)if(t!=null)Destroy(t);if(miniRoot!=null)Destroy(miniRoot.gameObject);if(fullRoot!=null)Destroy(fullRoot.gameObject);if(trackedRoot!=null)Destroy(trackedRoot.gameObject);}
    }
}
