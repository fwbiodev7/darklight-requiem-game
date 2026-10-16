using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    // Optional scenery only: no colliders, combat timing, rewards or save writes.
    public sealed class DarklightRegionAtmosphere:MonoBehaviour
    {
        DarklightCampaign game;ParticleSystem dust;Light2D glimmer;int chapter;
        float remaining;bool approached;
        public Vector2 Point{get;private set;}
        public bool Playing=>remaining>0;
        public float Remaining=>remaining;
        public int Activations{get;private set;}
        public static string Bed(int c)=>c switch{6=>"leaves",7=>"veil",8=>"urban",_=>"cave"};
        public static string Cue(int c)=>c switch{6=>"drip",7=>"portal",8=>"thunder",_=>"whisper"};
        public static Color Moon(int c)=>c switch{6=>new Color(.43f,.56f,.88f),7=>new Color(.82f,.79f,.65f),8=>new Color(.67f,.30f,.37f),_=>new Color(.44f,.37f,.71f)};
        public static Color Accent(int c)=>c switch{6=>new Color(.38f,.65f,1),7=>new Color(1,.83f,.39f),8=>new Color(1,.21f,.18f),_=>new Color(.67f,.38f,1)};
        public static string Label(int c)=>c switch{6=>"DESPERTAR O POÇO LUNAR",7=>"REVELAR O HALO PARTIDO",8=>"ECOAR O SINAL DAS RUÍNAS",_=>"ALINHAR O ANEL DO ECLIPSE"};
        public static string Message(int c)=>c switch{6=>"A lua revela nomes que o sangue tentou apagar.",7=>"Por baixo do ouro, o Véu ainda se move.",8=>"As ruínas devolvem o eco de uma batalha antiga.",_=>"Os anéis respondem. O coração do eclipse continua batendo."};
        public void Initialize(DarklightCampaign owner,int index)
        {
            game=owner;chapter=index;
            Point=game.Map.SafeSpawn(index==6?game.Map.Art.World(new Vector2(804,628)):game.Map.Art.World(game.Map.Art.areas[0].point)+Vector2.down*1.4f);
            transform.position=Point;
            var go=new GameObject("EcoAmbientalSemColisao");go.transform.SetParent(transform,false);
            dust=go.AddComponent<ParticleSystem>();dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=dust.main;main.loop=false;main.duration=3;main.startLifetime=new ParticleSystem.MinMaxCurve(.7f,1.6f);main.startSpeed=.3f;main.startSize=new ParticleSystem.MinMaxCurve(.025f,.055f);main.startColor=Accent(index)*new Color(1,1,1,.35f);main.maxParticles=48;main.simulationSpace=ParticleSystemSimulationSpace.World;main.playOnAwake=false;
            var emission=dust.emission;emission.rateOverTime=10;
            var shape=dust.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=1.6f;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");renderer.sortingOrder=15;
            glimmer=new GameObject("ReflexoAmbientalLimitado").AddComponent<Light2D>();glimmer.transform.SetParent(transform,false);glimmer.lightType=Light2D.LightType.Point;glimmer.color=Accent(index);glimmer.pointLightOuterRadius=4;glimmer.intensity=0;glimmer.enabled=false;
        }
        public bool Activate()
        {
            if(game==null||!game.Active||Playing)return false;
            remaining=3.2f;Activations++;
            if(DarklightRemaster.QualityLevel>0){dust.Play();glimmer.enabled=true;}
            game.AudioDirector?.Play("dl-ambient-"+Cue(chapter),DarklightAudioCategory.Ambient,.13f,Point,1,3);
            return true;
        }
        void Update()
        {
            if(game==null)return;
            float dt=DarklightCombatLab.Instance.CombatDelta;
            if(!game.Active||dt<=0){if(dust.isPlaying)dust.Pause();glimmer.enabled=false;return;}
            if(!approached&&Vector2.Distance(DarklightCombatLab.Instance.Player.transform.position,Point)<3.5f){approached=true;Activate();}
            if(remaining<=0)return;
            remaining=Mathf.Max(0,remaining-dt);
            bool effects=DarklightRemaster.QualityLevel>0;
            glimmer.enabled=effects&&remaining>0;
            // Slow single envelope; no camera motion, rapid flashes or opaque overlay.
            glimmer.intensity=effects?Mathf.Sin((3.2f-remaining)/3.2f*Mathf.PI)*.22f:0;
            if(effects&&dust.isPaused)dust.Play();
            if(!effects||remaining<=0)dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    public sealed partial class DarklightCampaign
    {
        DarklightRegionAtmosphere regionAtmosphere;
        void InitializeRegionAtmosphere(int chapter)
        {
            regionAtmosphere=null;
            if(Map?.Art==null||Map.Art.revision<2||chapter<6)return;
            var go=new GameObject("AtmosferaDaRegiao");go.transform.SetParent(levelRoot,false);
            regionAtmosphere=go.AddComponent<DarklightRegionAtmosphere>();regionAtmosphere.Initialize(this,chapter);
        }
        bool RegionAtmosphereTarget(out DarklightInteractionTarget target)
        {
            target=default;if(regionAtmosphere==null||regionAtmosphere.Playing)return false;
            target=new DarklightInteractionTarget("environment_echo",DarklightRegionAtmosphere.Label(MapRegion),regionAtmosphere.Point);return true;
        }
        bool InteractRegionAtmosphere(string id)
        {
            if(id!="environment_echo"||regionAtmosphere==null)return false;
            if(regionAtmosphere.Activate())Notice(DarklightRegionAtmosphere.Message(MapRegion));
            return true;
        }
    }
}
