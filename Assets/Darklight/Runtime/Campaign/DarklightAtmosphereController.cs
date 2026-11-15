using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Scenery only. No asset edits, global grading, lights, audio or actor scans.
    [DefaultExecutionOrder(2100)]
    public sealed class DarklightAtmosphereController : MonoBehaviour
    {
        public static DarklightAtmosphereController Current { get; private set; }
        public bool EffectsEnabled { get; private set; } = true;
        public bool AllRegionsEnabled { get; private set; } = true;
        public int PresetIndex { get; private set; }
        public string ActivePresetName => Presets[PresetIndex].Name;
        public int ActiveParticleCount { get; private set; }
        public int MaxParticles => 88;
        public const string IntensityPreferenceKey = "Darklight.AtmosphereIntensity";
        public const float DefaultIntensity = .85f;
        public float Intensity { get => intensity; set => SetIntensity(value, false); }
        public int ActiveFogLayerCount { get; private set; }
        public int ActiveRainPlaneCount { get; private set; }
        public int ActiveBoundaryFogCount { get; private set; }
        public static float SavedIntensity => SanitizeIntensity(PlayerPrefs.GetFloat(IntensityPreferenceKey, DefaultIntensity));
        static float SanitizeIntensity(float value) => float.IsNaN(value) || float.IsInfinity(value) ? DefaultIntensity : Mathf.Clamp01(value);
        // Preview is nonpersistent; settings explicitly opt in when accepting a value.
        public void SetIntensity(float value, bool persist = true)
        {
            intensity = SanitizeIntensity(value);
            if(persist) { PlayerPrefs.SetFloat(IntensityPreferenceKey, intensity); PlayerPrefs.Save(); }
            Refresh();
        }

        readonly struct Preset
        {
            public readonly string Name;
            public readonly Color Fog, Ground, Light;
            public readonly int Rain;
            public readonly float Wind, FogStrength, LightStrength, Wetness, FilterStrength;
            public readonly bool Daylight;
            public Preset(string name, Color fog, Color ground, int rain, float wind,
                float fogStrength, Color light, float lightStrength, float wetness, float filterStrength, bool daylight=false)
            { Name=name; Fog=fog; Ground=ground; Rain=rain; Wind=wind; FogStrength=fogStrength; Light=light;
              LightStrength=lightStrength; Wetness=wetness; FilterStrength=filterStrength; Daylight=daylight; }
        }
        // Visual treatments of the existing Places, not new events or a day/night gameplay clock.
        static readonly Preset[] Presets = {
            new Preset("Rua das Cinzas / noite chuvosa", new Color(.45f,.57f,.60f), new Color(.76f,.85f,.91f),66,.32f,1f,new Color(.70f,.82f,.96f),.105f,1f,.48f),
            new Preset("Distrito do Vinil / noite urbana",new Color(.47f,.52f,.57f),new Color(.83f,.87f,.92f),28,.28f,.7f,new Color(.74f,.82f,.94f),.07f,.7f,.35f),
            new Preset("Catedral do Sangue / interior",new Color(.52f,.56f,.53f),new Color(.89f,.92f,.89f),0,.08f,.38f,new Color(.88f,.85f,.73f),0,.18f,.24f),
            new Preset("Laboratorio Eclipse / luz artificial",new Color(.50f,.60f,.61f),new Color(.90f,.96f,.97f),0,.05f,.15f,new Color(.71f,.90f,.95f),0,0,.16f),
            new Preset("Jardim dos Ecos / dia encoberto",new Color(.65f,.72f,.64f),new Color(.97f,.99f,.94f),0,.19f,.40f,new Color(1f,.96f,.79f),.075f,.12f,.12f,true),
            new Preset("Observatorio Vespera / crepusculo",new Color(.56f,.56f,.65f),new Color(.88f,.87f,.97f),12,.42f,.6f,new Color(.89f,.81f,.95f),.065f,.4f,.26f),
            new Preset("Cripta dos Primeiros / subterraneo",new Color(.44f,.54f,.51f),new Color(.84f,.90f,.88f),0,.05f,.55f,new Color(.58f,.72f,.71f),0,.22f,.30f),
            new Preset("Catedral do Crepusculo / interior",new Color(.59f,.54f,.51f),new Color(.95f,.89f,.85f),0,.06f,.32f,new Color(.97f,.83f,.66f),0,.08f,.20f),
            new Preset("Veu Profundo / frio sobrenatural",new Color(.42f,.55f,.62f),new Color(.77f,.86f,.94f),48,.34f,.95f,new Color(.66f,.80f,1f),.08f,.8f,.42f),
            new Preset("Coracao do Eclipse / interior ritual",new Color(.57f,.48f,.55f),new Color(.91f,.85f,.92f),0,.05f,.30f,new Color(.84f,.71f,.84f),0,0,.24f)
        };
        struct Mote { public SpriteRenderer View; public Vector2 Position; public bool Rain; public float Speed, Depth; }
        readonly Dictionary<SpriteRenderer,Color> originals = new Dictionary<SpriteRenderer,Color>();
        readonly SpriteRenderer[] fog = new SpriteRenderer[3];
        readonly SpriteRenderer[] boundaryFog = new SpriteRenderer[12];
        readonly SpriteRenderer[] moon = new SpriteRenderer[3];
        readonly SpriteRenderer[] lampReflections = new SpriteRenderer[4];
        readonly Vector2[] reflectionPoints = new Vector2[4];
        int reflectionCount;
        static readonly int Wetness = Shader.PropertyToID("_Wetness");
        static readonly int SurfaceClock = Shader.PropertyToID("_SurfaceClock");
        readonly Sprite[] fogArt = new Sprite[3], moonArt = new Sprite[3];
        readonly List<Sprite> atlasSlices = new List<Sprite>();
        readonly Dictionary<SpriteRenderer,SpriteRenderer> filters = new Dictionary<SpriteRenderer,SpriteRenderer>();
        readonly Mote[] particles = new Mote[88];
        DarklightCampaign game;
        Transform mapRoot, effectsRoot;
        Camera view;
        Material material, filterMaterial, reflectionMaterial;
        Texture2D texture;
        Sprite soft, drop;
        bool built, visible;
        int quality=-1;
        bool reduced;
        float intensity=.85f, clock;
        Rect boundaryBounds;
        bool hasBoundary;
        [System.Serializable] sealed class Atlas { public string origin; public Frame[] frames; }
        [System.Serializable] sealed class Frame { public string name; public int x,y,width,height; }

        public static DarklightAtmosphereController Attach(DarklightCampaign owner, Transform root, Camera camera, int region)
        {
            var controller=owner.GetComponent<DarklightAtmosphereController>() ?? owner.gameObject.AddComponent<DarklightAtmosphereController>();
            controller.Initialize(owner,root,camera,region);
            return controller;
        }
        void Awake() { Current=this; EffectsEnabled=PlayerPrefs.GetInt("Darklight.Atmosphere",1)==1; intensity=SavedIntensity; }
        public void Initialize(DarklightCampaign owner, Transform root, Camera camera, int region)
        {
            RestoreGround();
            game=owner; mapRoot=root; view=camera;
            var boundaryMap=root!=null&&root.name=="ArenaDeTreinamento"
                ? DarklightMapDefinition.TrainingArena() : owner?.Map;
            hasBoundary=boundaryMap!=null;
            if(hasBoundary)
            {
                boundaryBounds=boundaryMap.Bounds;
                if(root!=null)boundaryBounds.position+=(Vector2)root.position;
            }
            if(!built) Build();
            // Capture only explicit ground renderers, never NPCs, props, actors or UI.
            if(mapRoot!=null)
            {
                var painted=mapRoot.Find("MapaIlustrado");
                if(painted!=null) Capture(painted.GetComponent<SpriteRenderer>());
                foreach(Transform child in mapRoot)
                    if(child.name.StartsWith("TrechoDetalhado_"))Capture(child.GetComponent<SpriteRenderer>());
                var floors=mapRoot.Find("Pisos_64px");
                if(floors!=null) foreach(var renderer in floors.GetComponentsInChildren<SpriteRenderer>(true)) Capture(renderer);
            }
            reflectionCount=0;
            if(mapRoot!=null && mapRoot.name=="ArenaDeTreinamento")
            {
                // Reuse actual arena braziers; never invent lamps at campaign coordinates.
                foreach(Transform child in mapRoot)
                    if(child.name=="BrasasDaArena" && reflectionCount<reflectionPoints.Length)
                        reflectionPoints[reflectionCount++]=child.position;
            }
            else if(game?.Map?.Checkpoints!=null)
                foreach(var checkpoint in game.Map.Checkpoints)
                    if(reflectionCount<reflectionPoints.Length)
                        reflectionPoints[reflectionCount++]=DarklightRemaster.LampPosition(checkpoint);
            ApplyRegion(region);
        }
        void Capture(SpriteRenderer renderer)
        {
            if(renderer==null) return;
            originals[renderer]=renderer.color;
            if(!built || renderer.sprite==null) return;
            var filter=Create("DarkFantasyFilter_"+renderer.name,renderer.sprite,renderer.sortingOrder+1);
            filter.sharedMaterial=filterMaterial;
            var textureBinding=new MaterialPropertyBlock();
            textureBinding.SetTexture("_MainTex",renderer.sprite.texture);filter.SetPropertyBlock(textureBinding);
            filter.sortingLayerID=renderer.sortingLayerID;
            filter.flipX=renderer.flipX; filter.flipY=renderer.flipY;
            filter.drawMode=renderer.drawMode; filter.size=renderer.size;
            filter.tileMode=renderer.tileMode;
            filter.spriteSortPoint=renderer.spriteSortPoint;
            filter.transform.SetPositionAndRotation(renderer.transform.position,renderer.transform.rotation);
            var sourceScale=renderer.transform.lossyScale;
            var parentScale=effectsRoot.lossyScale;
            filter.transform.localScale=new Vector3(sourceScale.x/parentScale.x,sourceScale.y/parentScale.y,sourceScale.z/parentScale.z);
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
        bool Allowed => EffectsEnabled && isActiveAndEnabled && (PresetIndex==0 || AllRegionsEnabled) && intensity>0 && DarklightRemaster.QualityLevel>0 && built && mapRoot!=null && view!=null;

        void Build()
        {
            // Resources shader is deliberately retained in standalone builds; no Shader.Find stripping risk.
            var shader=Resources.Load<Shader>("Darklight/Rendering/AtmosphereOverlay");
            if(shader==null) return;
            material=new Material(shader) { name="RuntimeAtmosphereOnly" };
            filterMaterial=new Material(shader) { name="DarkFantasyFilter_MapGroundOnly" };
            filterMaterial.SetFloat("_GroundFilter",1);
            reflectionMaterial=new Material(shader) { name="GothicEclipseLampReflection" };
            reflectionMaterial.SetFloat("_LampReflection",1);
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
            for(int i=0;i<boundaryFog.Length;i++) boundaryFog[i]=Create("NevoaSombriaDaBorda"+i,soft,-180);
            for(int i=0;i<moon.Length;i++) moon[i]=Create("LuarFrioIndependente"+i,moonArt[i]!=null?moonArt[i]:soft,-192+i);
            for(int i=0;i<lampReflections.Length;i++)
            {
                var reflection=Create("ReflexoLamparina"+i,soft,-196);
                reflection.sharedMaterial=reflectionMaterial;
                lampReflections[i]=reflection;
            }
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
            foreach(var item in originals)
                if(item.Key!=null) item.Key.color=item.Value;
            if(filterMaterial!=null)
            {
                filterMaterial.SetColor("_FilterColor",preset.Ground);
                filterMaterial.SetFloat("_Strength",preset.FilterStrength*intensity);
                filterMaterial.SetFloat(Wetness,quality>0 ? preset.Wetness : 0);
            }
            for(int i=0;i<lampReflections.Length;i++)
            {
                var reflection=lampReflections[i];
                if(reflection==null) continue;
                reflection.enabled=visible && quality>0 && preset.Rain>0 && i<reflectionCount;
                if(!reflection.enabled) continue;
                reflection.transform.position=reflectionPoints[i]+Vector2.down*.72f;
                Fit(reflection,1.05f,1.85f);
                reflection.color=new Color(1f,.55f,.19f,.32f*intensity*Mathf.Clamp01(preset.Rain/28f));
            }
            foreach(var item in filters) if(item.Value!=null)
            {
                item.Value.enabled=visible && item.Key!=null && item.Key.enabled;
                if(item.Key!=null) item.Value.color=originals[item.Key];
            }
            int rain=quality==0 ? 0 : quality==1 ? preset.Rain/2 : preset.Rain;
            int motes=quality==0 ? 4 : quality==1 ? 8 : 14;
            ActiveParticleCount=visible ? Mathf.Min(MaxParticles,Mathf.RoundToInt((rain+motes)*intensity)) : 0;
            ActiveRainPlaneCount=visible && rain>0 ? (quality==1?2:3) : 0;
            for(int i=0;i<particles.Length;i++)
            {
                var p=particles[i];
                p.Depth=quality==1 ? (i%2==0?.42f:.78f) : (.36f+(i%3)*.32f);
                p.Rain=i<Mathf.RoundToInt(rain*intensity); p.Speed=(.72f+Fraction(i*11+5)*.38f)*(.7f+p.Depth*.65f);
                if(p.View!=null)
                {
                    p.View.enabled=i<ActiveParticleCount;
                    p.View.sprite=p.Rain?drop:soft;
                    p.View.color=p.Rain ? new Color(.67f,.81f,.94f,(.16f+.14f*p.Depth)*intensity) : new Color(.60f,.74f,.73f,.09f*intensity);
                    p.View.transform.localScale=p.Rain ? new Vector3(.07f+.035f*p.Depth,.22f+.26f*p.Depth,1) : new Vector3(.035f,.035f,1);
                    p.View.transform.localRotation=Quaternion.Euler(0,0,p.Rain?16:0);
                    p.View.sortingOrder=-190+(i%3);
                }
                particles[i]=p;
            }
            ActiveFogLayerCount=visible ? (quality==1?2:3) : 0;
            for(int i=0;i<fog.Length;i++) if(fog[i]!=null)
            {
                // Three sparse low veils occupy different depth bands, always below actors.
                fog[i].enabled=visible && i<ActiveFogLayerCount;
                // The supplied fog atlas has bright filaments; a sparse soft veil avoids
                // horizontal silver ribbons and preserves pavement texture.
                fog[i].sprite=soft;
                var color=preset.Fog;
                color.a=(i==0?.065f:i==1?.042f:.026f)*preset.FogStrength*intensity; fog[i].color=color;
            }
            ActiveBoundaryFogCount=visible&&hasBoundary ? boundaryFog.Length : 0;
            foreach(var edge in boundaryFog) if(edge!=null)
            {
                edge.enabled=visible&&hasBoundary;
                edge.color=new Color(.085f,.055f,.13f,.78f*intensity);
            }
            for(int i=0;i<moon.Length;i++) if(moon[i]!=null)
            {
                moon[i].enabled=visible && preset.LightStrength>0 && i<(quality==1?1:2);
                moon[i].sprite=preset.Daylight ? soft : (moonArt[i]!=null?moonArt[i]:soft);
                var light=preset.Light; light.a=preset.LightStrength*(i==0?1f:.5f)*intensity;
                moon[i].color=light;
            }
        }
        void LateUpdate()
        {
            bool active=Allowed;
            if(active!=visible || quality!=DarklightRemaster.QualityLevel || reduced!=(game!=null && game.ReducedFlashes)) Refresh();
            if(!visible) return;
            float dt=DarklightCombatLab.Instance!=null ? DarklightCombatLab.Instance.CombatDelta : Time.deltaTime;
            AdvanceAtmosphere(dt);
        }
        // One bounded simulation step shared by runtime and deterministic motion checks.
        void AdvanceAtmosphere(float dt)
        {
            if(!visible) return;
            dt=Mathf.Clamp(dt,0,.05f);
            clock+=dt;
            if(quality>0 && dt>0 && !reduced)
            {
                filterMaterial.SetFloat(SurfaceClock,clock);
                reflectionMaterial.SetFloat(SurfaceClock,clock);
            }
            float height=view.orthographicSize*2, width=height*view.aspect;
            Vector3 center=view.transform.position; center.z=0;
            var preset=Presets[PresetIndex];
            UpdateBoundaryFog();
            for(int i=0;i<fog.Length;i++)
            {
                if(!fog[i].enabled) continue;
                // Fog stays behind every actor and prop. Slow drift has no flashes or opacity pulse.
                float drift=reduced ? 0 : Mathf.Sin(clock*.045f+i*2)*.15f;
                fog[i].transform.position=center+new Vector3(width*(drift*.2f+(i-1)*.18f),height*(-.39f+i*.20f),0);
                Fit(fog[i],width*(.85f-i*.13f),height*(.28f+i*.045f));
            }
            for(int i=0;i<moon.Length;i++)
            {
                if(!moon[i].enabled) continue;
                moon[i].transform.position=center+new Vector3(width*(i==0?-.31f:.24f),height*(i==0?.25f:.36f),0);
                moon[i].transform.rotation=Quaternion.Euler(0,0,preset.Daylight?-22:moonArt[i]!=null?0:-24);
                Fit(moon[i],width*(preset.Daylight?.48f:moonArt[i]!=null?(i==0?.53f:.32f):.16f),height*(preset.Daylight?.9f:1.15f));
            }
            for(int i=0;i<ActiveParticleCount;i++)
            {
                var p=particles[i];
                p.Position.x+=dt*preset.Wind*p.Speed*(p.Rain?.20f:.045f);
                p.Position.y+=dt*(p.Rain ? -1.1f*p.Speed : .012f);
                if(p.Position.y < -1.1f) p.Position.y+=2.2f;
                if(p.Position.y > 1.1f) p.Position.y-=2.2f;
                if(p.Position.x > 1.1f) p.Position.x-=2.2f;
                p.View.transform.position=center+new Vector3(p.Position.x*width*.5f,p.Position.y*height*.5f,0);
                particles[i]=p;
            }
        }
        void UpdateBoundaryFog()
        {
            if(!hasBoundary||ActiveBoundaryFogCount==0)return;
            // World-space perimeter, not a camera vignette. The dense portion
            // stays outside the map; only its soft inner fringe meets the floor.
            float depth=Mathf.Clamp(Mathf.Min(boundaryBounds.width,boundaryBounds.height)*.10f,2.5f,5f);
            for(int i=0;i<boundaryFog.Length;i++)
            {
                var edge=boundaryFog[i];if(edge==null||!edge.enabled)continue;
                int side=i/3,segment=i%3;
                bool vertical=side<2;
                float drift=reduced?0:Mathf.Sin(clock*.035f+i*1.7f)*.022f;
                float t=(segment+.5f)/3f+drift;
                Vector2 point=vertical
                    ? new Vector2(side==0?boundaryBounds.xMin:boundaryBounds.xMax,Mathf.LerpUnclamped(boundaryBounds.yMin,boundaryBounds.yMax,t))
                    : new Vector2(Mathf.LerpUnclamped(boundaryBounds.xMin,boundaryBounds.xMax,t),side==2?boundaryBounds.yMin:boundaryBounds.yMax);
                Vector2 outward=side==0?Vector2.left:side==1?Vector2.right:side==2?Vector2.down:Vector2.up;
                edge.transform.position=point+outward*depth*.5f;
                float length=(vertical?boundaryBounds.height:boundaryBounds.width)/3f*2.25f;
                Fit(edge,vertical?depth*1.6f:length,vertical?length:depth*1.6f);
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
            if(reflectionMaterial!=null) Destroy(reflectionMaterial);
            foreach(var sprite in atlasSlices) if(sprite!=null) Destroy(sprite);
        }
    }
}
