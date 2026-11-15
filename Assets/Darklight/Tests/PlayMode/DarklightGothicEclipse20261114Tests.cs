using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Darklight.Tests
{
    public sealed partial class DarklightWeaponRedesignGameplayTests
    {
        [UnityTest] public IEnumerator GothicEclipse20261114ArenaAndRegionZeroAreLayeredReversibleAndBounded()
        {
            var remaster=game.GetComponent<DarklightRemaster>();
            int previousQuality=DarklightRemaster.QualityLevel;
            var atmosphere=DarklightAtmosphereController.Current;
            float previousIntensity=atmosphere.Intensity;
            bool previousEnabled=atmosphere.EffectsEnabled;
            const string folder="outputs/gothic-eclipse-20261114/gameplay";
            Directory.CreateDirectory(folder);
            var evidence=new StringBuilder();
            evidence.AppendLine("Hardware: "+SystemInfo.processorType+"; GPU: "+SystemInfo.graphicsDeviceName+"; API: "+SystemInfo.graphicsDeviceType+"; OS: "+SystemInfo.operatingSystem);
            evidence.AppendLine("1280x720, editor PlayMode, camera fixed, world paused. Synchronized capture render cost (Render+1px ReadPixels); excludes PNG encoding and simulation; NOT gameplay FPS. Quality2. 5 warmup renders + 20 measured samples per state; one-pixel synchronous GPU flush.");
            try
            {
                remaster.SetQuality(2,false);
                for(int scene=0;scene<2;scene++)
                {
                    if(scene==1) { Call(game,"LoadMap",0); yield return null; yield return null; }
                    lab.SetPaused(true);fighter.enabled=false;
                    foreach(var enemy in game.MapEnemies)if(enemy!=null)enemy.enabled=false;
                    lab.WorldCamera.transform.position=fighter.transform.position+new Vector3(0,1,-10);
                    atmosphere=DarklightAtmosphereController.Current;
                    Assert.That(atmosphere.AllRegionsEnabled,Is.True,"Contextual presets enabled after representative-map review");
                    atmosphere.Intensity=.85f;
                    var effects=(Transform)typeof(DarklightAtmosphereController).GetField("effectsRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(atmosphere);
                    Assert.That(effects.GetComponentsInChildren<SpriteRenderer>(true).Count(r=>r.name.StartsWith("ChuvaOuVento")),Is.EqualTo(88));
                    var filterPairs=(Dictionary<SpriteRenderer,SpriteRenderer>)typeof(DarklightAtmosphereController).GetField("filters",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(atmosphere);
                    foreach(var pair in filterPairs)
                    {
                        Assert.That(Vector3.Distance(pair.Key.bounds.center,pair.Value.bounds.center),Is.LessThan(.001f),"Filter center must match its source");
                        Assert.That(Vector3.Distance(pair.Key.bounds.size,pair.Value.bounds.size),Is.LessThan(.001f),"Filter size must match its source");
                        Assert.That(pair.Value.sprite,Is.SameAs(pair.Key.sprite));
                        Assert.That(pair.Value.drawMode,Is.EqualTo(pair.Key.drawMode));
                        evidence.AppendLine("Source: "+pair.Key.name+"; bounds="+pair.Key.bounds+"; localToWorld="+pair.Key.transform.localToWorldMatrix+"; sprite="+pair.Key.sprite.name+"; texture="+pair.Key.sprite.texture.name+" "+pair.Key.sprite.texture.width+"x"+pair.Key.sprite.texture.height+"; PPU="+pair.Key.sprite.pixelsPerUnit);
                    }
                    var cameraPosition=lab.WorldCamera.transform.position;
                    float cameraSize=lab.WorldCamera.orthographicSize;
                    var cameraProjection=lab.WorldCamera.projectionMatrix;
                    var groundMatrices=filterPairs.Keys.ToDictionary(r=>r,r=>r.transform.localToWorldMatrix);
                    var colliders=Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
                    var geometry=colliders.ToDictionary(c=>c,c=>(c.transform.position,c.transform.localScale,c.offset,c.enabled));
                    var actors=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Where(r=>!r.transform.IsChildOf(effects)).ToDictionary(r=>r,r=>(r.color,r.sprite,r.transform.localScale));
                    string label=scene==0?"arena":"region0";
                    atmosphere.SetEffectsEnabled(false);yield return null;
                    GothicCaptureTimed(label+"-off",folder,evidence);
                    atmosphere.SetEffectsEnabled(true);
                    foreach(float strength in new[]{0f,.85f,1f})
                    {
                        atmosphere.Intensity=strength;yield return null;
                        GothicCaptureTimed(label+"-intensity-"+(strength==0?"0":strength==1?"1":"085"),folder,evidence);
                        Assert.That(lab.WorldCamera.transform.position,Is.EqualTo(cameraPosition));
                        Assert.That(lab.WorldCamera.orthographicSize,Is.EqualTo(cameraSize));
                        Assert.That(lab.WorldCamera.projectionMatrix,Is.EqualTo(cameraProjection));
                        foreach(var ground in groundMatrices)Assert.That(ground.Key.transform.localToWorldMatrix,Is.EqualTo(ground.Value));
                    }
                    atmosphere.Intensity=.85f;yield return null;
                    foreach(var r in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                        if(r.sprite!=null&&r.sharedMaterial!=null&&r.sharedMaterial.shader.name=="Darklight/Remaster/SpriteLit")
                        {
                            var binding=new MaterialPropertyBlock();r.GetPropertyBlock(binding);
                            Assert.That(binding.GetTexture("_MainTex"),Is.SameAs(r.sprite.texture),"Each map chunk/actor must bind its own current sheet: "+r.name);
                        }
                    Capture("gothic-"+label+"-on");File.Copy(Path.Combine(Folder,"gothic-"+label+"-on.png"),folder+"/"+label+"-on.png",true);
                    Assert.That(atmosphere.ActiveParticleCount,Is.InRange(1,88));
                    Assert.That(atmosphere.ActiveFogLayerCount,Is.EqualTo(3));
                    Assert.That(atmosphere.ActiveRainPlaneCount,Is.EqualTo(3));
                    foreach(var c in geometry)
                    {
                        Assert.That((c.Key.transform.position,c.Key.transform.localScale,c.Key.offset,c.Key.enabled),Is.EqualTo(c.Value),"Atmosphere must preserve collider geometry");
                    }
                    foreach(var actor in actors)
                        Assert.That((actor.Key.color,actor.Key.sprite,actor.Key.transform.localScale),Is.EqualTo(actor.Value),"Atmosphere cannot recolor or replace actors, props or floor sources");
                    int rendererCount=effects.GetComponentsInChildren<SpriteRenderer>(true).Length;
                    atmosphere.Intensity=0;yield return null;
                    Assert.That(effects.GetComponentsInChildren<SpriteRenderer>(true).All(r=>!r.enabled),Is.True);
                    atmosphere.Intensity=.85f;remaster.SetQuality(0,false);yield return null;
                    Assert.That(atmosphere.ActiveParticleCount,Is.Zero);
                    Assert.That(effects.GetComponentsInChildren<SpriteRenderer>(true).All(r=>!r.enabled),Is.True,"Quality zero restores the base scene");
                    remaster.SetQuality(2,false);yield return null;
                    atmosphere.SetAllRegionsEnabled(false);atmosphere.ApplyRegion(1);
                    Assert.That(atmosphere.ActiveParticleCount,Is.Zero,"Unreviewed regions remain gated");
                    atmosphere.SetAllRegionsEnabled(true);atmosphere.ApplyRegion(0);yield return null;
                    Assert.That(effects.GetComponentsInChildren<SpriteRenderer>(true).Length,Is.EqualTo(rendererCount),"Toggling never grows the renderer pool");
                    atmosphere.enabled=false;yield return null;
                    Assert.That(effects.GetComponentsInChildren<SpriteRenderer>(true).All(r=>!r.enabled),Is.True);
                    atmosphere.enabled=true;yield return null;
                    Assert.That(game.GetComponents<DarklightAtmosphereController>().Length,Is.EqualTo(1));
                    Assert.That(game.GetComponentsInChildren<Transform>(true).Count(t=>t.name=="AtmosferaFriaSemColisao"),Is.EqualTo(1),"Reentry must reuse the single effects root");
                    foreach(var texture in Resources.LoadAll<Texture2D>("Darklight/AtmosphereTextures20261101/AtmosphereTextures"))
                        Assert.That(texture.filterMode,Is.EqualTo(FilterMode.Point));
                }
                // Deterministic motion step avoids AI, input and camera changes during validation.
                var rain=game.GetComponentsInChildren<SpriteRenderer>(true).First(r=>r.name=="ChuvaOuVento0");
                var startRain=rain.transform.position;
                Call(atmosphere,"AdvanceAtmosphere",.025f);
                Assert.That(Vector3.Distance(rain.transform.position,startRain),Is.GreaterThan(.001f),"Rain must actually travel, not merely allocate renderers");
                var clockField=typeof(DarklightAtmosphereController).GetField("clock",BindingFlags.Instance|BindingFlags.NonPublic);
                float frozenClock=(float)clockField.GetValue(atmosphere);
                var frozenRain=rain.transform.position;
                yield return null;yield return null;
                Assert.That((float)clockField.GetValue(atmosphere),Is.EqualTo(frozenClock),"Paused world freezes atmosphere time");
                Assert.That(rain.transform.position,Is.EqualTo(frozenRain));
                File.WriteAllText(folder+"/capture-cost-and-geometry.txt",evidence.ToString());
                AssertSilent();
            }
            finally
            {
                File.WriteAllText(folder+"/capture-cost-and-geometry.txt",evidence.ToString());
                remaster.SetQuality(previousQuality,false);
                if(atmosphere!=null) { atmosphere.Intensity=previousIntensity;atmosphere.SetEffectsEnabled(previousEnabled); }
            }
        }

        void GothicCaptureTimed(string label,string folder,StringBuilder evidence)
        {
            var camera=lab.WorldCamera;var before=camera.targetTexture;var active=RenderTexture.active;
            var target=new RenderTexture(1280,720,24){filterMode=FilterMode.Point,antiAliasing=1};target.Create();
            var pixels=new Texture2D(1280,720,TextureFormat.RGB24,false);
            var probe=new Texture2D(1,1,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;
                double total=0,peak=0,minimum=double.MaxValue;
                var timer=new Stopwatch();
                for(int sample=-5;sample<20;sample++)
                {
                    Call(DarklightAtmosphereController.Current,"AdvanceAtmosphere",1f/60);
                    timer.Restart();DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;
                    probe.ReadPixels(new Rect(0,0,1,1),0,0);probe.Apply();timer.Stop();
                    if(sample>=0) { total+=timer.Elapsed.TotalMilliseconds;peak=System.Math.Max(peak,timer.Elapsed.TotalMilliseconds);minimum=System.Math.Min(minimum,timer.Elapsed.TotalMilliseconds); }
                }
                pixels.ReadPixels(new Rect(0,0,1280,720),0,0);pixels.Apply();
                File.WriteAllBytes(folder+"/"+label+".png",pixels.EncodeToPNG());
                evidence.AppendLine(label+": mean Render+ReadPixels ms="+(total/20).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"; min ms="+minimum.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"; max ms="+peak.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));
            }
            finally { camera.targetTexture=before;RenderTexture.active=active;target.Release();Object.Destroy(target);Object.Destroy(pixels);Object.Destroy(probe); }
        }

        [UnityTest] public IEnumerator GothicEclipse20261114ActorEdgeLightFollowsLocalSourceAndRestores()
        {
            Call(game,"LoadMap",0);yield return null;yield return null;
            lab.SetPaused(true);fighter.enabled=false;
            var remaster=game.GetComponent<DarklightRemaster>();remaster.SetQuality(2,false);remaster.SyncScene();
            var atmosphere=DarklightAtmosphereController.Current;atmosphere.SetEffectsEnabled(true);atmosphere.Intensity=.85f;
            var actor=Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).First(r=>r.name=="FabioVisual");
            var lights=(List<UnityEngine.Rendering.Universal.Light2D>)typeof(DarklightRemaster).GetField("lamps",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(remaster);
            var local=lights.Where(l=>l.lightType!=UnityEngine.Rendering.Universal.Light2D.LightType.Global&&l.name!="LuzDaArma").ToArray();
            Assert.That(local.Length,Is.GreaterThan(0));
            var states=local.ToDictionary(l=>l,l=>(l.transform.position,l.enabled));
            var key=local[0];var block=new MaterialPropertyBlock();var sourceSprite=actor.sprite;Color color=actor.color;
            var evidence=new StringBuilder();const string folder="outputs/gothic-eclipse-20261114/gameplay";
            try
            {
                foreach(var light in local)light.enabled=false;key.enabled=true;
                lab.WorldCamera.transform.position=actor.bounds.center+new Vector3(0,0,-10);
                foreach(float side in new[]{-1f,1f})
                {
                    key.transform.position=actor.bounds.center+new Vector3(side*1.5f,.35f,0);
                    Call(remaster,"UpdateActorLighting");actor.GetPropertyBlock(block);
                    Assert.That(block.GetFloat("_ActorLightStrength"),Is.GreaterThan(.05f));
                    Assert.That(Mathf.Sign(block.GetVector("_ActorLightDirection").x),Is.EqualTo(side));
                    GothicCaptureTimed("actor-light-"+(side<0?"left":"right"),folder,evidence);
                    Assert.That(actor.sprite,Is.SameAs(sourceSprite));Assert.That(actor.color,Is.EqualTo(color));
                }
                key.transform.position=actor.bounds.center+Vector3.right*100;
                Call(remaster,"UpdateActorLighting");actor.GetPropertyBlock(block);Assert.That(block.GetFloat("_ActorLightStrength"),Is.Zero);
                key.transform.position=actor.bounds.center+Vector3.left;
                atmosphere.Intensity=0;Call(remaster,"UpdateActorLighting");actor.GetPropertyBlock(block);Assert.That(block.GetFloat("_ActorLightStrength"),Is.Zero);
                File.WriteAllText(folder+"/actor-light-evidence.txt",evidence.ToString());AssertSilent();
            }
            finally
            {
                foreach(var pair in states){pair.Key.transform.position=pair.Value.Item1;pair.Key.enabled=pair.Value.Item2;}
                atmosphere.Intensity=.85f;Call(remaster,"UpdateActorLighting");
            }
        }

        [UnityTest] public IEnumerator GothicEclipse20261114DayAndInteriorsHaveDistinctDryAtmospheres()
        {
            const string folder="outputs/gothic-eclipse-20261114/gameplay";var evidence=new StringBuilder();
            foreach(int chapter in new[]{4,2})
            {
                Call(game,"LoadMap",chapter);yield return null;yield return null;
                lab.SetPaused(true);fighter.enabled=false;
                var atmosphere=DarklightAtmosphereController.Current;
                atmosphere.SetAllRegionsEnabled(true);atmosphere.SetEffectsEnabled(true);atmosphere.Intensity=.85f;
                game.GetComponent<DarklightRemaster>().SetQuality(2,false);
                lab.WorldCamera.transform.position=fighter.transform.position+new Vector3(0,1,-10);yield return null;
                Assert.That(atmosphere.ActiveRainPlaneCount,Is.Zero,"No outdoor rain inside or on the overcast day map");
                Assert.That(atmosphere.ActivePresetName,Does.Contain(chapter==4?"dia encoberto":"interior"));
                GothicCaptureTimed(chapter==4?"region4-day":"region2-interior",folder,evidence);
                var effects=(Transform)typeof(DarklightAtmosphereController).GetField("effectsRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(atmosphere);
                if(chapter==2)Assert.That(effects.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("Luar")).All(r=>!r.enabled),Is.True);
                atmosphere.Intensity=0;yield return null;
                Assert.That(effects.GetComponentsInChildren<SpriteRenderer>(true).All(r=>!r.enabled),Is.True);
                atmosphere.SetAllRegionsEnabled(true);
            }
            File.WriteAllText(folder+"/contextual-presets-evidence.txt",evidence.ToString());AssertSilent();
        }

        [UnityTest] public IEnumerator GothicEclipse20261114IntensityPreferenceIsSanitizedAndExplicitlyPersisted()
        {
            string key=DarklightAtmosphereController.IntensityPreferenceKey;
            bool existed=PlayerPrefs.HasKey(key);float original=PlayerPrefs.GetFloat(key);
            var atmosphere=DarklightAtmosphereController.Current;
            float previous=atmosphere.Intensity;
            try
            {
                PlayerPrefs.DeleteKey(key);
                Assert.That(DarklightAtmosphereController.SavedIntensity,Is.EqualTo(.85f));
                atmosphere.SetIntensity(.37f,true);
                Assert.That(PlayerPrefs.GetFloat(key),Is.EqualTo(.37f));
                atmosphere.Intensity=.62f;
                Assert.That(PlayerPrefs.GetFloat(key),Is.EqualTo(.37f),"Preview must not persist");
                atmosphere.Intensity=float.NaN;Assert.That(atmosphere.Intensity,Is.EqualTo(.85f));
                atmosphere.Intensity=float.PositiveInfinity;Assert.That(atmosphere.Intensity,Is.EqualTo(.85f));
                atmosphere.Intensity=-1;Assert.That(atmosphere.Intensity,Is.Zero);
                atmosphere.Intensity=2;Assert.That(atmosphere.Intensity,Is.EqualTo(1));
                PlayerPrefs.SetFloat(key,float.NaN);Assert.That(DarklightAtmosphereController.SavedIntensity,Is.EqualTo(.85f));
            }
            finally
            {
                if(existed)PlayerPrefs.SetFloat(key,original);else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();atmosphere.Intensity=previous;
            }
            yield return null;
        }
    }
}
