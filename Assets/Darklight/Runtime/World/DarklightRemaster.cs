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
        readonly List<(Bounds Bounds, int Layer, int Order)> actorOcclusion=new();
        MaterialPropertyBlock textureBinding;
        static readonly int MainTexture=Shader.PropertyToID("_MainTex");
        static readonly (string Name, Color Color, float Intensity)[] Presets = {
            ("Night Rain", new Color(.43f,.54f,.76f), .82f),
            ("Overcast Day", new Color(1f,.93f,.81f), .88f),
            ("Dusk", new Color(1f,.72f,.49f), .81f),
            ("Gothic Interior", new Color(.62f,.49f,.57f), .82f),
            ("Veil Supernatural", new Color(.42f,.49f,.73f), .76f)
        };
        // Rua, Distrito, Catedral, Laboratório, Jardim, Observatório, Cripta, Véspera, Véu, Coração.
        static readonly int[] RegionPresets = { 0, 0, 3, 3, 1, 2, 3, 2, 4, 4 };
        public static string AmbientPresetName(int chapter) => Presets[RegionPresets[Mathf.Clamp(chapter,0,9)]].Name;
        internal static readonly Color ArconteLight = new Color(.24f,1f,.38f);
        internal static readonly Color ArconteViolet = new Color(.64f,.24f,1f);
        const int ContactCapacity = 4;
        readonly Light2D[] contacts = new Light2D[ContactCapacity];
        readonly float[] contactStarts = new float[ContactCapacity];
        readonly float[] contactDurations = new float[ContactCapacity];
        readonly float[] contactStrengths = new float[ContactCapacity];
        readonly List<Light2D> flames = new();
        readonly List<float> flameStrengths = new();
        static DarklightRemaster activeLighting;
        float nextScan;
        Light2D moon, weaponLight, arconteLight;
        Color ambientFrom, ambientTarget;
        float intensityFrom, intensityTarget, ambientBlend;
        public void Initialize(Camera camera,Transform player,Transform mapRoot,int chapter)
        {
            textureBinding??=new MaterialPropertyBlock();
            this.camera=camera;this.player=player;this.mapRoot=mapRoot;
            spriteTemplate??=Resources.Load<Material>("Darklight/Rendering/SpriteLit");shadowMaterial??=Resources.Load<Material>("Darklight/Rendering/WholeShadow");
            if(spriteTemplate==null||shadowMaterial==null)return;
            if(volume==null)
            {
                volume=gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=Resources.Load<VolumeProfile>("Darklight/Rendering/Volume");
                var data=camera.GetComponent<UniversalAdditionalCameraData>()??camera.gameObject.AddComponent<UniversalAdditionalCameraData>();data.SetRenderer(0);data.renderPostProcessing=QualityLevel>0;data.antialiasing=AntialiasingMode.None;data.volumeLayerMask=1;
                camera.allowHDR=true;camera.allowMSAA=false;camera.allowDynamicResolution=false;
                CreateEmbers();
            }
            activeLighting=this;
            foreach(var light in lamps)
                if(light!=null&&light!=moon){light.enabled=false;Destroy(light.gameObject);}
            lamps.Clear();
            flames.Clear();
            flameStrengths.Clear();
            var preset=Presets[RegionPresets[Mathf.Clamp(chapter,0,9)]];
            ambientTarget=preset.Color;
            intensityTarget=preset.Intensity;
            if(moon==null)
                moon=Point("LuaAmbiente",player.position,ambientTarget,intensityTarget,0,true);
            else
                lamps.Add(moon);
            ambientFrom=moon.color;
            intensityFrom=moon.intensity;
            ambientBlend=0;
            if(mapRoot!=null&&mapRoot.name=="ArenaDeTreinamento")
            {
                foreach(Transform child in mapRoot)if(child.name=="BrasasDaArena")
                    Flame("Braseiro",child.position+Vector3.up*.45f,new Color(1,.55f,.23f),1.1f,4.5f);
            }
            else if(DarklightCampaign.Instance?.Map!=null)
            {
                var map=DarklightCampaign.Instance.Map;
                Point("Portal",map.Portal,new Color(.58f,.16f,1),.8f,4);
                foreach(var p in map.Checkpoints)Flame("Lamparina",LampPosition(p),new Color(1,.62f,.28f),1.25f,5);
                Point("Memoria",map.Ally+Vector2.up,new Color(.1f,.85f,.72f),.7f,4.5f);
                if(mapRoot!=null)foreach(Transform child in mapRoot)
                    if(child.name.StartsWith("Tocha",System.StringComparison.OrdinalIgnoreCase)||
                       child.name.StartsWith("Braseiro",System.StringComparison.OrdinalIgnoreCase))
                        Flame("Tocha",child.position+Vector3.up*.45f,new Color(1,.46f,.19f),1.05f,4);
            }
            // Dedicated transient lights are excluded from the static-light quality budget.
            if(weaponLight==null)
                weaponLight=Point("AuraFabius",player.position,DarklightFormController.ColorRuptura,0,2.8f,tracked:false);
            if(arconteLight==null)
                arconteLight=Point("AuraArconte",player.position,ArconteLight,0,3.4f,tracked:false);
            weaponLight.enabled=arconteLight.enabled=false;
            for(int i=0;i<contacts.Length;i++)
            {
                if(contacts[i]==null)
                    contacts[i]=Point("LuzDeContato",player.position,Color.white,0,2,tracked:false);
                contacts[i].enabled=false;
            }
            ApplyQuality();SyncScene();nextScan=0;
        }
        void Flame(string name, Vector3 position, Color tint, float strength, float radius)
        {
            flames.Add(Point(name,position,tint,strength,radius));
            flameStrengths.Add(strength);
        }
        Light2D Point(string name,Vector3 point,Color color,float intensity,float radius,bool global=false,bool tracked=true)
        {
            var go=new GameObject(name);go.transform.SetParent(transform,false);go.transform.position=point;
            var light=go.AddComponent<Light2D>();light.lightType=global?Light2D.LightType.Global:Light2D.LightType.Point;light.color=color;light.intensity=intensity;
            if(!global){light.pointLightOuterRadius=radius;light.pointLightInnerRadius=.15f;light.shadowIntensity=.6f;}
            if(tracked)lamps.Add(light);
            return light;
        }
        internal static void ContactLight(Vector3 position, Color tint, float intensity, float radius, float duration)
        {
            var lighting=activeLighting;
            if(lighting==null||!lighting.isActiveAndEnabled||QualityLevel==0||
               DarklightCampaign.Instance?.ReducedFlashes==true)return;
            int slot=0;
            for(int i=0;i<ContactCapacity;i++)
            {
                if(!lighting.contacts[i].enabled){slot=i;break;}
                if(lighting.contactStarts[i]<lighting.contactStarts[slot])slot=i;
            }
            var light=lighting.contacts[slot];
            light.transform.position=position;
            light.color=tint;
            light.pointLightOuterRadius=radius;
            light.intensity=intensity;
            light.enabled=true;
            lighting.contactStarts[slot]=Time.unscaledTime;
            lighting.contactDurations[slot]=duration;
            lighting.contactStrengths[slot]=intensity;
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
            int q=QualityLevel;int limit=4+q*4;
            for(int i=0;i<lamps.Count;i++)lamps[i].enabled=i<limit;
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
                bool actor=r.name=="FabioVisual"||r.GetComponentInParent<DarklightEnemy>()!=null||
                    r.name=="IcaroNox"||r.name=="LyraMoreau"||r.name=="KaelAliado"||
                    r.name=="KaelPurificado"||r.name=="MatriarcaApresentacao"||
                    r.GetComponentInParent<DarklightCampaignAlly>()!=null;
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
        internal static void TrackVisibleActor(SpriteRenderer actor)
        {
            var lighting=activeLighting;
            if(lighting!=null&&actor!=null&&!lighting.litActors.Contains(actor))lighting.litActors.Add(actor);
        }
        internal static int OrderBehindActors(Bounds bounds,int layer,int original)
        {
            var lighting=activeLighting;
            if(lighting==null)return original;
            int order=original;
            foreach(var actor in lighting.actorOcclusion)
                if(actor.Layer==layer&&actor.Order<=order&&
                   bounds.min.x<actor.Bounds.max.x&&bounds.max.x>actor.Bounds.min.x&&
                   bounds.min.y<actor.Bounds.max.y&&bounds.max.y>actor.Bounds.min.y)
                    order=actor.Order-1;
            return order;
        }
        void LateUpdate()
        {
            if(spriteTemplate==null||player==null)return;
            foreach(var renderer in boundSprites)BindSpriteTexture(renderer);
            UpdateActorLighting();
            float time=DarklightCombatLab.Instance?.Player.Model.Clock??Time.time;
            var game=DarklightCampaign.Instance;
            bool reduced=game?.ReducedFlashes==true;
            if(moon!=null&&ambientBlend<1)
            {
                ambientBlend=Mathf.Min(1,ambientBlend+Time.unscaledDeltaTime/.8f);
                float blend=Mathf.SmoothStep(0,1,ambientBlend);
                moon.color=Color.Lerp(ambientFrom,ambientTarget,blend);
                moon.intensity=Mathf.Lerp(intensityFrom,intensityTarget,blend);
            }
            int form=game?.PresentedForm??0;
            bool showAuras=QualityLevel>0&&game?.Active==true;
            weaponLight.enabled=showAuras&&form>0;
            if(weaponLight.enabled)
            {
                weaponLight.transform.position=player.position+Vector3.up*(form==2?1.2f:.85f);
                weaponLight.color=form==2?new Color(1f,.32f,.43f):DarklightFormController.ColorRuptura;
                weaponLight.pointLightOuterRadius=form==2?4f:2.8f;
                weaponLight.intensity=reduced?.33f:form==2?.63f:.54f;
            }
            var boss=showAuras?game.CurrentBoss:null;
            arconteLight.enabled=boss!=null&&boss.IsLucius&&boss.LuciusView?.appearance==DarklightLuciusAppearance.Arconte;
            if(arconteLight.enabled)
            {
                arconteLight.transform.position=boss.transform.position+Vector3.up*.95f;
                arconteLight.color=boss.Phase>=3?Color.Lerp(ArconteLight,ArconteViolet,.55f):ArconteLight;
                arconteLight.intensity=reduced?.38f:.69f;
            }
            for(int i=0;i<contacts.Length;i++)
            {
                var contact=contacts[i];
                if(!contact.enabled)continue;
                float elapsed=(Time.unscaledTime-contactStarts[i])/contactDurations[i];
                if(reduced||QualityLevel==0||elapsed>=1)contact.enabled=false;
                else contact.intensity=contactStrengths[i]*(1-elapsed)*(1-elapsed);
            }
            for(int i=0;i<flames.Count;i++)
                if(flames[i]!=null&&flames[i].enabled)
                    flames[i].intensity=flameStrengths[i]+(reduced?0:Mathf.Sin(time*2.1f+i)*.045f);
            if(embers!=null){embers.transform.position=camera.transform.position+Vector3.forward*10;var main=embers.main;main.simulationSpeed=DarklightCampaign.Instance?.Active==true?1:0;}
            if(Time.unscaledTime>=nextScan){nextScan=Time.unscaledTime+.35f;SyncScene();}
            actorOcclusion.Clear();
            foreach(var actor in litActors)
                if(actor!=null&&actor.sprite!=null&&actor.enabled&&actor.gameObject.activeInHierarchy&&actor.color.a>.01f)
                    actorOcclusion.Add((actor.bounds,actor.sortingLayerID,actor.sortingOrder));
        }
        void OnDestroy(){if(activeLighting==this)activeLighting=null;foreach(var material in clones.Values)Destroy(material);}
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
