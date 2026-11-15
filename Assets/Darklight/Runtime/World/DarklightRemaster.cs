using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Darklight
{
    [DefaultExecutionOrder(1800)]
    public sealed class DarklightRemaster:MonoBehaviour
    {
        public static readonly string[] QualityNames={"BAIXA","MEDIA","ALTA","ULTRA"};
        static int previewQuality=-1;
        public static int QualityLevel=>previewQuality>=0?previewQuality:Mathf.Clamp(PlayerPrefs.GetInt("Darklight.VisualQuality",2),0,3);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void ResetPreviewQuality()=>previewQuality=-1;
        Camera camera;Transform player,mapRoot;Volume volume;ParticleSystem embers;
        readonly List<Light2D> lamps=new();readonly List<DarklightWholeShadow> shadows=new();
        readonly Dictionary<Material,Material> clones=new();Material spriteTemplate,shadowMaterial;
        readonly List<SpriteRenderer> boundSprites=new();
        readonly List<SpriteRenderer> litActors=new();
        MaterialPropertyBlock textureBinding;
        static readonly int MainTexture=Shader.PropertyToID("_MainTex");
        float nextScan;int mapChapter=-1;Light2D weaponLight;
        public void Initialize(Camera camera,Transform player,Transform mapRoot,int chapter)
        {
            textureBinding??=new MaterialPropertyBlock();
            this.camera=camera;this.player=player;this.mapRoot=mapRoot;mapChapter=chapter;
            spriteTemplate??=Resources.Load<Material>("Darklight/Rendering/SpriteLit");shadowMaterial??=Resources.Load<Material>("Darklight/Rendering/WholeShadow");
            if(spriteTemplate==null||shadowMaterial==null)return;
            if(volume==null)
            {
                volume=gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=Resources.Load<VolumeProfile>("Darklight/Rendering/Volume");
                var data=camera.GetComponent<UniversalAdditionalCameraData>()??camera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.SetRenderer(0);data.renderPostProcessing=QualityLevel>0;data.antialiasing=AntialiasingMode.None;data.volumeLayerMask=1;
                camera.allowHDR=true;camera.allowMSAA=false;camera.allowDynamicResolution=false;
                CreateEmbers();
            }
            foreach(var light in lamps)if(light!=null){light.enabled=false;Destroy(light.gameObject);}lamps.Clear();
            bool day=chapter==4,interior=chapter==2||chapter==3||chapter==6||chapter==7||chapter==9;
            Color ambient=day?new Color(.86f,.89f,.80f):interior?new Color(.63f,.61f,.60f):new Color(.55f,.65f,.81f);
            Point("LuaAmbiente",player.position,ambient,day?.95f:interior?.82f:.78f,0,true);
            if(mapRoot!=null&&mapRoot.name=="ArenaDeTreinamento")
            {
                foreach(Transform child in mapRoot)if(child.name=="BrasasDaArena")
                    Point("Braseiro",child.position+Vector3.up*.45f,new Color(1,.55f,.23f),1.1f,4.5f);
            }
            else if(DarklightCampaign.Instance?.Map!=null)
            {
                var map=DarklightCampaign.Instance.Map;
                foreach(var p in map.Checkpoints)Point("Lamparina",LampPosition(p),new Color(1,.62f,.28f),1.4f,5);
                Point("Memoria",map.Ally+Vector2.up,new Color(.1f,.85f,.72f),.85f,4.5f);
                Point("Portal",map.Portal,new Color(.58f,.16f,1),.8f,4);
            }
            weaponLight=Point("LuzDaArma",player.position,new Color(1,.08f,.16f),.45f,2.8f);
            ApplyQuality();SyncScene();nextScan=0;
        }
        Light2D Point(string name,Vector3 point,Color color,float intensity,float radius,bool global=false)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.position=point;
            var light=go.AddComponent<Light2D>();light.lightType=global?Light2D.LightType.Global:Light2D.LightType.Point;light.color=color;light.intensity=intensity;
            if(!global){light.pointLightOuterRadius=radius;light.pointLightInnerRadius=.15f;light.shadowIntensity=.6f;}
            lamps.Add(light);return light;
        }
        internal static Vector2 LampPosition(Vector2 checkpoint) => checkpoint + new Vector2(.6f,-1.6f);

        void CreateEmbers()
        {
            var go=new GameObject("ParticulasDoEclipse");go.transform.SetParent(transform,false);embers=go.AddComponent<ParticleSystem>();embers.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=embers.main;main.loop=true;main.startLifetime=new ParticleSystem.MinMaxCurve(3,5);main.startSpeed=new ParticleSystem.MinMaxCurve(.08f,.22f);main.startSize=new ParticleSystem.MinMaxCurve(.018f,.035f);main.startColor=new Color(1,.45f,.13f,.3f);main.maxParticles=70;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=embers.emission;emission.rateOverTime=12;var shape=embers.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(16,9,1);
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");renderer.sortingOrder=500;embers.Play();
        }
        public void SetQuality(int quality,bool persist=true)
        {
            quality=Mathf.Clamp(quality,0,3);previewQuality=persist?-1:quality;
            if(persist){PlayerPrefs.SetInt("Darklight.VisualQuality",quality);PlayerPrefs.Save();}ApplyQuality();
        }
        void ApplyQuality()
        {
            int q=QualityLevel;int limit=new[]{4,8,12,16}[q];
            for(int i=0;i<lamps.Count;i++)lamps[i].enabled=i<limit||lamps[i]==weaponLight;
            if(volume!=null){volume.enabled=q>0;volume.weight=q==1?.65f:1;}
            if(camera!=null){var data=camera.GetComponent<UniversalAdditionalCameraData>();if(data!=null)data.renderPostProcessing=q>0;}
            if(embers!=null){var main=embers.main;main.maxParticles=new[]{20,40,70,96}[q];var emission=embers.emission;emission.rateOverTime=new[]{3,7,12,16}[q];}
        }
        public void SyncScene()
        {
            boundSprites.Clear();litActors.Clear();
            foreach(var r in FindObjectsByType<SpriteRenderer>())
            {
                if(r.sharedMaterial==null)continue;
                bool actor=r.name=="FabioVisual"||r.GetComponentInParent<DarklightEnemy>()!=null||r.name=="IcaroNox"||r.name=="LyraMoreau"||r.name=="KaelAliado";
                if(actor)litActors.Add(r);
                if(r.sharedMaterial.shader.name=="Darklight/Remaster/SpriteLit") { boundSprites.Add(r);BindSpriteTexture(r);continue; }
                if(r.sharedMaterial.shader.name!="Darklight/PixelSprite")continue;
                var source=r.sharedMaterial;
                if(!clones.TryGetValue(source,out var lit))
                {
                    lit=new Material(spriteTemplate){name="Remaster_"+source.name};
                    if(source.HasProperty("_MainTex")){lit.SetTexture("_MainTex",source.GetTexture("_MainTex"));lit.SetTextureScale("_MainTex",source.GetTextureScale("_MainTex"));lit.SetTextureOffset("_MainTex",source.GetTextureOffset("_MainTex"));}
                    if(source.HasProperty("_Color"))lit.SetColor("_Color",source.GetColor("_Color"));if(source.HasProperty("_AlphaClip"))lit.SetFloat("_AlphaClip",source.GetFloat("_AlphaClip"));clones.Add(source,lit);
                }
                r.sharedMaterial=lit;
                boundSprites.Add(r);BindSpriteTexture(r);
                if(actor&&r.GetComponent<DarklightWholeShadow>()==null){var shadow=r.gameObject.AddComponent<DarklightWholeShadow>();shadow.Initialize(r,shadowMaterial,this);shadows.Add(shadow);}
            }
            if(mapRoot!=null)foreach(var collider in mapRoot.GetComponentsInChildren<Collider2D>())
            {
                if(collider.isTrigger||collider.GetComponent<ShadowCaster2D>()!=null)continue;
                collider.gameObject.AddComponent<ShadowCaster2D>();
            }
        }
        // Custom URP sprite materials can otherwise retain the preceding draw's texture.
        // Bind the complete current sheet per renderer, preserving every other property.
        void BindSpriteTexture(SpriteRenderer renderer)
        {
            if(renderer==null||renderer.sprite==null)return;
            renderer.GetPropertyBlock(textureBinding);
            if(textureBinding.GetTexture(MainTexture)==renderer.sprite.texture)return;
            textureBinding.SetTexture(MainTexture,renderer.sprite.texture);renderer.SetPropertyBlock(textureBinding);
        }
        void UpdateActorLighting()
        {
            var atmosphere=DarklightAtmosphereController.Current;
            float strength=QualityLevel>0&&atmosphere!=null&&atmosphere.isActiveAndEnabled&&atmosphere.EffectsEnabled?atmosphere.Intensity:0;
            foreach(var actor in litActors)
            {
                if(actor==null||actor.sprite==null)continue;
                var center=actor.bounds.center;Light2D key=null;float best=0;
                if(strength>0)foreach(var light in lamps)
                {
                    if(light==null||!light.enabled||light==weaponLight||light.lightType==Light2D.LightType.Global)continue;
                    float distance=Vector2.Distance(center,light.transform.position);
                    float candidate=Mathf.Pow(Mathf.Clamp01(1-distance/Mathf.Max(.1f,light.pointLightOuterRadius)),2)*light.intensity;
                    if(candidate>best){best=candidate;key=light;}
                }
                actor.GetPropertyBlock(textureBinding);
                textureBinding.SetVector("_ActorTexel",new Vector4(1f/actor.sprite.texture.width,1f/actor.sprite.texture.height,0,0));
                textureBinding.SetFloat("_ActorLightStrength",Mathf.Min(.65f,best*strength));
                if(key!=null)
                {
                    Vector2 direction=((Vector2)(key.transform.position-center)).normalized;
                    textureBinding.SetVector("_ActorLightDirection",new Vector4(direction.x,direction.y,actor.flipX?-direction.x:direction.x,actor.flipY?-direction.y:direction.y));
                    textureBinding.SetColor("_ActorLightColor",key.color.linear);
                    textureBinding.SetVector("_ActorLightBounds",new Vector4(center.x,center.y,Mathf.Max(.01f,actor.bounds.extents.x),Mathf.Max(.01f,actor.bounds.extents.y)));
                }
                actor.SetPropertyBlock(textureBinding);
            }
        }
        public Vector2 ShadowDirection(Vector2 foot,out float length)
        {
            Light2D key=null;float score=0;
            foreach(var light in lamps)
            {
                if(light==null||!light.enabled||light.lightType==Light2D.LightType.Global||light==weaponLight)continue;
                float candidate=light.intensity/(1+((Vector2)light.transform.position-foot).sqrMagnitude);if(candidate>score){score=candidate;key=light;}
            }
            if(key==null){length=.5f;return new Vector2(.8f,-.6f);}
            var away=foot-(Vector2)key.transform.position;length=Mathf.Clamp(away.magnitude*.35f,.55f,1.5f);return away.normalized;
        }
        void LateUpdate()
        {
            if(spriteTemplate==null||player==null)return;
            foreach(var renderer in boundSprites)BindSpriteTexture(renderer);
            UpdateActorLighting();
            float time=DarklightCombatLab.Instance?.Player.Model.Clock??Time.time;
            if(weaponLight!=null)
            {
                weaponLight.transform.position=player.position+Vector3.up*.9f;int form=DarklightCampaign.Instance?.PresentedForm??0;
                weaponLight.color=form==3?new Color(.72f,.9f,1):form==2?new Color(.64f,.15f,1):new Color(1,.08f,.16f);
                weaponLight.intensity=form>0?.7f:.3f;
            }
            for(int i=1;i<lamps.Count-1;i++)if(lamps[i]!=null&&(lamps[i].name=="Lamparina"||lamps[i].name=="Braseiro"))lamps[i].intensity=1.15f+Mathf.Sin(time*2.1f+i)*.035f;
            if(embers!=null){embers.transform.position=camera.transform.position+Vector3.forward*10;var main=embers.main;main.simulationSpeed=DarklightCampaign.Instance?.Active==true?1:0;}
            if(Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.35f;SyncScene();}
        }
        void OnDestroy(){foreach(var material in clones.Values)Destroy(material);}
    }
    [DefaultExecutionOrder(2000)] public sealed class DarklightWholeShadow:MonoBehaviour
    {
        SpriteRenderer source;DarklightRemaster lights;Mesh mesh;MeshRenderer display;Sprite previous;
        Vector2[] points;Vector3[] vertices;int[] triangles;MaterialPropertyBlock block;
        public void Initialize(SpriteRenderer source,Material material,DarklightRemaster lights)
        {
            this.source=source;this.lights=lights;var go=new GameObject("SombraDaFiguraInteira");go.transform.SetParent(source.transform.parent,false);
            mesh=new Mesh{name="Whole moving silhouette"};mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;display=go.AddComponent<MeshRenderer>();display.sharedMaterial=material;block=new MaterialPropertyBlock();
        }
        void LateUpdate()
        {
            if(source==null||display==null)return;
            display.enabled=source.enabled&&source.sprite!=null&&DarklightRemaster.QualityLevel>0&&source.gameObject.activeInHierarchy;
            if(!display.enabled)return;
            if(previous!=source.sprite)
            {
                previous=source.sprite;points=previous.vertices;vertices=new Vector3[points.Length];var input=previous.triangles;triangles=new int[input.Length];for(int i=0;i<input.Length;i++)triangles[i]=input[i];mesh.Clear();mesh.vertices=vertices;mesh.uv=previous.uv;mesh.triangles=triangles;
            }
            Vector2 foot=source.transform.position;Vector2 away=lights.ShadowDirection(foot,out float length);
            for(int i=0;i<points.Length;i++){Vector3 point=source.transform.TransformPoint(points[i]);float h=Mathf.Max(0,point.y-foot.y);Vector3 world=foot+new Vector2((point.x-foot.x)*.8f,0)+away*h*length;vertices[i]=display.transform.InverseTransformPoint(world);}
            mesh.vertices=vertices;mesh.RecalculateBounds();display.sortingLayerID=source.sortingLayerID;display.sortingOrder=source.sortingOrder-2;
            source.GetPropertyBlock(block);block.SetTexture("_MainTex",previous.texture);block.SetColor("_ShadowColor",new Color(.015f,.025f,.045f,.35f));display.SetPropertyBlock(block);
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);if(display!=null)Destroy(display.gameObject);}
    }
}
