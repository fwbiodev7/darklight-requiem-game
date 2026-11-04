using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Scenery only. No asset edits, global grading, lights, audio, settings writes or actor scans.
    [DefaultExecutionOrder(2100)]
    public sealed class DarklightAtmosphereController : MonoBehaviour
    {
        public static DarklightAtmosphereController Current { get; private set; }
        public bool EffectsEnabled { get; private set; } = true;
        public bool AllRegionsEnabled { get; private set; } = true;
        public int PresetIndex { get; private set; }
        public string ActivePresetName => UsesGothicEclipse ? GothicPreset.DisplayName : Presets[PresetIndex].Name;
        public bool GothicEclipseEnabled => gothicEclipseEnabled;
        public bool UsesGothicEclipse => gothicEclipseEnabled && PresetIndex==0;
        [SerializeField] bool gothicEclipseEnabled=true;
        [SerializeField] DarklightAtmospherePreset gothicEclipsePreset;
        DarklightAtmospherePreset runtimePreset;
        bool ownsRuntimePreset;
        DarklightAtmospherePreset GothicPreset
        {
            get
            {
                if(gothicEclipsePreset!=null) return gothicEclipsePreset;
                if(runtimePreset==null)
                {
                    runtimePreset=Resources.Load<DarklightAtmospherePreset>(DarklightAtmospherePreset.RuaResourcePath);
                    if(runtimePreset==null)
                    {
                        runtimePreset=ScriptableObject.CreateInstance<DarklightAtmospherePreset>();
                        runtimePreset.hideFlags=HideFlags.HideAndDontSave;
                        ownsRuntimePreset=true;
                    }
                }
                return runtimePreset;
            }
        }
        public int ActiveParticleCount { get; private set; }
        public int MaxParticles => 88;
        public float Intensity { get => intensity; set { intensity = Mathf.Clamp01(value); Refresh(); } }

        readonly struct Preset
        {
            public readonly string Name;
            public readonly Color Fog, Ground;
            public readonly int Rain;
            public readonly float Wind;
            public Preset(string name, Color fog, Color ground, int rain, float wind)
            { Name=name; Fog=fog; Ground=ground; Rain=rain; Wind=wind; }
        }
        static readonly Preset[] Presets = {
            new Preset("Rua das Cinzas / chuva fria", new Color(.30f,.43f,.47f), new Color(.76f,.85f,.91f), 48,.32f),
            new Preset("Distrito do Vinil / nevoa urbana", new Color(.32f,.40f,.46f), new Color(.80f,.86f,.92f), 34,.38f),
            new Preset("Catedral do Sangue / pedra umida", new Color(.32f,.42f,.41f), new Color(.80f,.87f,.87f), 8,.14f),
            new Preset("Laboratorio Eclipse / bruma mineral", new Color(.30f,.42f,.46f), new Color(.83f,.90f,.94f), 0,.12f),
            new Preset("Jardim dos Ecos / luar entre folhas", new Color(.32f,.47f,.45f), new Color(.78f,.88f,.89f), 28,.26f),
            new Preset("Observatorio Vespera / vento lunar", new Color(.34f,.44f,.56f), new Color(.77f,.85f,.95f), 20,.48f),
            new Preset("Cripta dos Primeiros / nevoa baixa", new Color(.31f,.43f,.44f), new Color(.78f,.87f,.90f), 0,.12f),
            new Preset("Catedral do Crepusculo / pedra azul", new Color(.33f,.44f,.47f), new Color(.79f,.87f,.92f), 8,.18f),
            new Preset("Veu Profundo / chuva velada", new Color(.30f,.43f,.46f), new Color(.77f,.86f,.91f), 56,.38f),
            new Preset("Coracao do Eclipse / luar palido", new Color(.35f,.43f,.49f), new Color(.80f,.87f,.94f), 12,.28f)
        };
        struct Mote { public SpriteRenderer View; public Vector2 Position; public bool Rain; public float Speed; }
        readonly Dictionary<SpriteRenderer,Color> originals = new Dictionary<SpriteRenderer,Color>();
        readonly SpriteRenderer[] fog = new SpriteRenderer[3];
        readonly SpriteRenderer[] moon = new SpriteRenderer[3];
        readonly Sprite[] fogArt = new Sprite[3], moonArt = new Sprite[3];
        readonly List<Sprite> atlasSlices = new List<Sprite>();
        readonly Dictionary<SpriteRenderer,SpriteRenderer> filters = new Dictionary<SpriteRenderer,SpriteRenderer>();
        readonly Mote[] particles = new Mote[88];
        DarklightCampaign game;
        Transform mapRoot, effectsRoot;
        Camera view;
        Material material, filterMaterial;
        Texture2D texture;
        Sprite soft, drop;
        bool built, visible;
        int quality=-1;
        bool reduced;
        float intensity=.85f, clock;
        [System.Serializable] sealed class Atlas { public string origin; public Frame[] frames; }
        [System.Serializable] sealed class Frame { public string name; public int x,y,width,height; }

        public static DarklightAtmosphereController Attach(DarklightCampaign owner, Transform root, Camera camera, int region)
        {
            var controller=owner.GetComponent<DarklightAtmosphereController>() ?? owner.gameObject.AddComponent<DarklightAtmosphereController>();
            controller.Initialize(owner,root,camera,region);
            return controller;
        }
        void Awake() { Current=this; EffectsEnabled=PlayerPrefs.GetInt("Darklight.Atmosphere",1)==1; }
        public void Initialize(DarklightCampaign owner, Transform root, Camera camera, int region)
        {
            RestoreGround();
            game=owner; mapRoot=root; view=camera;
            if(!built) Build();
            // Capture only explicit ground renderers, never NPCs, props, actors or UI.
            if(mapRoot!=null)
            {
                var painted=mapRoot.Find("MapaIlustrado");
                if(painted!=null) Capture(painted.GetComponent<SpriteRenderer>());
                var floors=mapRoot.Find("Pisos_64px");
                if(floors!=null) foreach(var renderer in floors.GetComponentsInChildren<SpriteRenderer>(true)) Capture(renderer);
            }
            ApplyRegion(region);
        }
        void Capture(SpriteRenderer renderer)
        {
            if(renderer==null) return;
            originals[renderer]=renderer.color;
            if(!built || renderer.sprite==null) return;
            var filter=Create("DarkFantasyFilter_"+renderer.name,renderer.sprite,renderer.sortingOrder+1);
            filter.sharedMaterial=filterMaterial;
            filter.sortingLayerID=renderer.sortingLayerID;
            filter.flipX=renderer.flipX; filter.flipY=renderer.flipY;
            filter.transform.SetPositionAndRotation(renderer.transform.position,renderer.transform.rotation);
            filter.transform.localScale=renderer.transform.lossyScale;
            filters[renderer]=filter;
        }
        public void ApplyRegion(int region)
        {
            PresetIndex=Mathf.Clamp(region,0,Presets.Length-1);
            clock=0;
            for(int i=0;i<particles.Length;i++)
            {
                var p=particles[i];
                p.Position=new Vector2(Fraction(i*37+13)*2-1,Fraction(i*59+7)*2-1);
                particles[i]=p;
            }
            Refresh();
        }
        static float Fraction(int seed) => (seed%101)/101f;
        public void SetEffectsEnabled(bool enabled) { EffectsEnabled=enabled; Refresh(); }
        public void SetAllRegionsEnabled(bool enabled) { AllRegionsEnabled=enabled; Refresh(); }
        // False restores the existing map atmosphere; the settings switch disables all overlays.
        public void SetGothicEclipseEnabled(bool enabled) { gothicEclipseEnabled=enabled; Refresh(); }
        public void SetGothicEclipsePreset(DarklightAtmospherePreset preset) { gothicEclipsePreset=preset; Refresh(); }
        bool Enclosed => PresetIndex==2 || PresetIndex==3 || PresetIndex==6 || PresetIndex==7 || PresetIndex==9;
        bool Allowed => EffectsEnabled && isActiveAndEnabled && (PresetIndex==0 || AllRegionsEnabled) && intensity>0 && built && mapRoot!=null && view!=null;

        void Build()
        {
            // Resources shader is deliberately retained in standalone builds; no Shader.Find stripping risk.
            var shader=Resources.Load<Shader>("Darklight/Rendering/AtmosphereOverlay");
            if(shader==null) return;
            material=new Material(shader) { name="RuntimeAtmosphereOnly" };
            filterMaterial=new Material(shader) { name="DarkFantasyFilter_MapGroundOnly" };
            filterMaterial.SetFloat("_GroundFilter",1);
            texture=new Texture2D(64,64,TextureFormat.RGBA32,false) { name="RuntimePixelAtmosphere", filterMode=FilterMode.Point, wrapMode=TextureWrapMode.Clamp };
            var colors=new Color[64*64];
            for(int y=0;y<64;y++) for(int x=0;x<64;x++)
            {
                float radius=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).magnitude;
                float alpha=Mathf.SmoothStep(0,1,Mathf.Clamp01(1-radius));
                colors[y*64+x]=new Color(1,1,1,Mathf.Round(alpha*alpha*8)/8);
            }
            texture.SetPixels(colors); texture.Apply(false,true);
            soft=Sprite.Create(texture,new Rect(0,0,64,64),Vector2.one*.5f,64);
            drop=Sprite.Create(texture,new Rect(28,0,8,64),Vector2.one*.5f,64);
            effectsRoot=new GameObject("AtmosferaFriaSemColisao").transform;
            effectsRoot.SetParent(transform,false);
            LoadAtlas();
            for(int i=0;i<fog.Length;i++) fog[i]=Create("NevoaBaixa"+i,soft,-195+i);
            for(int i=0;i<moon.Length;i++) moon[i]=Create("LuarFrioIndependente"+i,moonArt[i]!=null?moonArt[i]:soft,-192+i);
            for(int i=0;i<particles.Length;i++) particles[i].View=Create("ChuvaOuVento"+i,soft,-190);
            built=true;
        }
        void LoadAtlas()
        {
            const string path="Darklight/AtmosphereTextures20261101/AtmosphereTextures";
            var sprites=Resources.LoadAll<Sprite>(path);
            foreach(var sprite in sprites)
                for(int i=0;i<3;i++)
                {
                    if(sprite.name=="fog_"+i || sprite.name.EndsWith("_fog_"+i)) fogArt[i]=sprite;
                    if(sprite.name=="moon_"+i || sprite.name.EndsWith("_moon_"+i)) moonArt[i]=sprite;
                }
            var image=Resources.Load<Texture2D>(path);
            var json=Resources.Load<TextAsset>(path);
            if(image==null || json==null) return;
            Atlas atlas;
            try { atlas=JsonUtility.FromJson<Atlas>(json.text); }
            catch(System.ArgumentException) { return; }
            if(atlas?.frames==null) return;
            foreach(var frame in atlas.frames)
            {
                if(frame==null || frame.width<=0 || frame.height<=0) continue;
                int y=atlas.origin=="top-left" ? image.height-frame.y-frame.height : frame.y;
                if(frame.x<0 || y<0 || frame.x+frame.width>image.width || y+frame.height>image.height) continue;
                for(int i=0;i<3;i++)
                {
                    bool isFog=frame.name=="fog_"+i, isMoon=frame.name=="moon_"+i;
                    if(!isFog && !isMoon || isFog && fogArt[i]!=null || isMoon && moonArt[i]!=null) continue;
                    var sprite=Sprite.Create(image,new Rect(frame.x,y,frame.width,frame.height),Vector2.one*.5f,256,0,SpriteMeshType.FullRect);
                    sprite.name=frame.name; atlasSlices.Add(sprite);
                    if(isFog) fogArt[i]=sprite; else moonArt[i]=sprite;
                }
            }
        }
        SpriteRenderer Create(string name, Sprite sprite, int order)
        {
            var obj=new GameObject(name); obj.transform.SetParent(effectsRoot,false);
            var renderer=obj.AddComponent<SpriteRenderer>(); renderer.sprite=sprite;
            renderer.sharedMaterial=material; renderer.sortingOrder=order; renderer.enabled=false;
            return renderer;
        }
        void Refresh()
        {
            quality=DarklightRemaster.QualityLevel;
            reduced=game!=null && game.ReducedFlashes;
            // Pausing freezes the world clock, but must preserve its lighting and fog.
            visible=Allowed;
            var preset=Presets[PresetIndex];
            var gothic=UsesGothicEclipse ? GothicPreset : null;
            foreach(var item in originals)
                if(item.Key!=null) item.Key.color=item.Value;
            if(filterMaterial!=null)
            {
                // Reuse the single map-only filter; Remaster remains the sole light/Volume owner.
                filterMaterial.SetColor("_FilterColor",gothic!=null ? gothic.GroundPalette : preset.Ground);
                filterMaterial.SetFloat("_Strength",(gothic!=null ? gothic.GroundStrength : .28f)*intensity);
                filterMaterial.SetFloat("_MapExposure",gothic!=null ? gothic.MapExposureStops : 0);
            }
            foreach(var item in filters) if(item.Value!=null)
            {
                item.Value.enabled=visible && item.Key!=null && item.Key.enabled;
                if(item.Key!=null) item.Value.color=originals[item.Key];
            }
            // Retain Remaster quality budgets and the existing pool. Interiors never receive rain.
            int requestedRain=Enclosed ? 0 : gothic!=null ? gothic.RainDrops : preset.Rain;
            int rain=quality==0 ? requestedRain/4 : quality==1 ? requestedRain/2 : requestedRain;
            int motes=quality==0 ? 4 : quality==1 ? 8 : 14;
            ActiveParticleCount=visible ? Mathf.Min(MaxParticles,Mathf.RoundToInt((rain+motes)*intensity)) : 0;
            for(int i=0;i<particles.Length;i++)
            {
                var p=particles[i];
                p.Rain=i<Mathf.RoundToInt(rain*intensity); p.Speed=.85f+Fraction(i*11+5)*.65f;
                if(p.View!=null)
                {
                    p.View.enabled=i<ActiveParticleCount;
                    p.View.sprite=p.Rain?drop:soft;
                    p.View.color=p.Rain ? new Color(.66f,.79f,.83f,.13f*intensity) : new Color(.60f,.74f,.73f,.09f*intensity);
                    p.View.transform.localScale=p.Rain ? new Vector3(.13f,.22f,1) : new Vector3(.035f,.035f,1);
                    p.View.transform.localRotation=Quaternion.Euler(0,0,p.Rain?-16:0);
                }
                particles[i]=p;
            }
            for(int i=0;i<fog.Length;i++) if(fog[i]!=null)
            {
                // One low veil per map; atlas variants are alternatives, never stacked.
                fog[i].enabled=visible && i==(gothic!=null ? gothic.FogVariant : PresetIndex%fog.Length);
                fog[i].sprite=fogArt[i]!=null ? fogArt[i] : soft;
                var color=gothic!=null ? gothic.FogColor : fogArt[i]!=null ? new Color(.66f,.79f,.81f) : preset.Fog;
                color.a=(gothic!=null ? gothic.FogOpacity : .045f)*intensity; fog[i].color=color;
            }
            for(int i=0;i<moon.Length;i++) if(moon[i]!=null)
            {
                moon[i].enabled=visible && quality>0 && i==(gothic!=null ? gothic.MoonVariant : (PresetIndex+1)%moon.Length);
                var color=gothic!=null ? gothic.MoonColor : new Color(.67f,.83f,.98f);
                color.a=(gothic!=null ? gothic.MoonOpacity : Enclosed?.015f:.04f)*intensity;
                moon[i].color=color;
            }
        }
        void LateUpdate()
        {
            bool active=Allowed;
            if(active!=visible || quality!=DarklightRemaster.QualityLevel || reduced!=(game!=null && game.ReducedFlashes)) Refresh();
            if(!visible) return;
            float dt=DarklightCombatLab.Instance!=null ? DarklightCombatLab.Instance.CombatDelta : Time.deltaTime;
            dt=Mathf.Clamp(dt,0,.05f);
            clock+=dt;
            float height=view.orthographicSize*2, width=height*view.aspect;
            Vector3 center=view.transform.position; center.z=0;
            var preset=Presets[PresetIndex];
            var gothic=UsesGothicEclipse ? GothicPreset : null;
            for(int i=0;i<fog.Length;i++)
            {
                // Fog stays behind every actor and prop. Slow drift has no flashes or opacity pulse.
                float drift=reduced ? 0 : Mathf.Sin(clock*(gothic!=null ? gothic.FogDriftSpeed : .045f)+i*2)*.15f;
                fog[i].transform.position=center+new Vector3(width*drift*.2f,-height*.36f,0);
                Fit(fog[i],width*1.45f,height*(gothic!=null ? gothic.FogHeight : .42f));
            }
            for(int i=0;i<moon.Length;i++)
            {
                moon[i].transform.position=center+new Vector3(-width*.32f,height*.28f,0);
                moon[i].transform.rotation=Quaternion.Euler(0,0,moonArt[i]!=null?0:-24);
                Fit(moon[i],width*(moonArt[i]!=null?.72f:.16f),height*1.15f);
            }
            for(int i=0;i<ActiveParticleCount;i++)
            {
                var p=particles[i];
                p.Position.x+=dt*(gothic!=null ? gothic.Wind : preset.Wind)*p.Speed*(p.Rain?.20f:.045f);
                p.Position.y+=dt*(p.Rain ? -1.1f*p.Speed : .012f);
                if(p.Position.y < -1.1f) p.Position.y+=2.2f;
                if(p.Position.y > 1.1f) p.Position.y-=2.2f;
                if(p.Position.x > 1.1f) p.Position.x-=2.2f;
                if(p.Position.x < -1.1f) p.Position.x+=2.2f;
                p.View.transform.position=center+new Vector3(p.Position.x*width*.5f,p.Position.y*height*.5f,0);
                particles[i]=p;
            }
        }
        void RestoreGround()
        {
            foreach(var item in originals) if(item.Key!=null) item.Key.color=item.Value;
            originals.Clear();
            foreach(var filter in filters.Values) if(filter!=null) { filter.enabled=false; Destroy(filter.gameObject); }
            filters.Clear();
        }
        static void Fit(SpriteRenderer renderer,float width,float height)
        {
            var size=renderer.sprite.bounds.size;
            renderer.transform.localScale=new Vector3(width/Mathf.Max(.001f,size.x),height/Mathf.Max(.001f,size.y),1);
        }
        void OnEnable() { if(built) Refresh(); }
        void OnDisable() { Refresh(); }
        void OnDestroy()
        {
            RestoreGround();
            if(Current==this) Current=null;
            if(effectsRoot!=null) Destroy(effectsRoot.gameObject);
            if(soft!=null) Destroy(soft); if(drop!=null) Destroy(drop);
            if(texture!=null) Destroy(texture); if(material!=null) Destroy(material);
            if(filterMaterial!=null) Destroy(filterMaterial);
            if(ownsRuntimePreset && runtimePreset!=null) Destroy(runtimePreset);
            foreach(var sprite in atlasSlices) if(sprite!=null) Destroy(sprite);
        }
    }
}
