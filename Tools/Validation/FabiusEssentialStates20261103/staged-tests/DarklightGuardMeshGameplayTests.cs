using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
#if UNITY_EDITOR
namespace Darklight.Tests
{
    public sealed partial class DarklightNovemberIsolatedGameplayTests
    {
        [UnityTest] public IEnumerator NovemberMotorcycleGuardMeshRendersAndExpiresWithBoundedPool()
        {
            Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null),"This probe requires --force-d3d11, not --headless-tests");
            var context=new GameObject("GuardMeshDependencyContext");context.SetActive(false);
            var lab=context.AddComponent<DarklightCombatLab>();var game=context.AddComponent<DarklightCampaign>();
            var controller=context.AddComponent<DarklightWeaponController>();
            var cameraObject=new GameObject("GuardMeshOffscreenCamera");var camera=cameraObject.AddComponent<Camera>();camera.enabled=false;
            camera.orthographic=true;camera.orthographicSize=.35f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;
            camera.cullingMask=1<<31;camera.allowHDR=false;camera.allowMSAA=false;
            var target=new RenderTexture(64,64,24,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point,antiAliasing=1};target.Create();camera.targetTexture=target;
            var readback=new Texture2D(64,64,TextureFormat.RGBA32,false);var priorTarget=RenderTexture.active;float priorCapture=Time.captureDeltaTime;
            var priorParticles=DarklightActionParticles.Current;var particles=actor.AddComponent<DarklightActionParticles>();
            var particleInstance=typeof(DarklightActionParticles).GetField("instance",BindingFlags.Static|BindingFlags.NonPublic);
            var late=typeof(DarklightCombatImpacts).GetMethod("LateUpdate",BindingFlags.Instance|BindingFlags.NonPublic);
            var hitField=typeof(DarklightCombatImpacts).GetField("hits",BindingFlags.Instance|BindingFlags.NonPublic);
            int[] pixels=new int[8];DarklightCombatImpacts impacts=null;
            try
            {
                // Inactive context prevents campaign/map/audio/save initialization.
                typeof(DarklightCombatLab).GetProperty("Instance").SetValue(null,lab);
                typeof(DarklightCampaign).GetProperty("Instance").SetValue(null,game);
                typeof(DarklightCampaign).GetProperty("Weapons").SetValue(game,controller);
                particleInstance.SetValue(null,particles);Time.captureDeltaTime=.016f;yield return null;
                foreach(var page in EssentialPages("Parry").Where(p=>p.weapon==3).OrderBy(p=>p.direction))
                {
                    EssentialContext(3,page.direction);typeof(DarklightWeaponController).GetProperty("Equipped").SetValue(controller,weapon);
                    DarklightActionParticles.Observe(view,model,art,3);Tick(Facing(page.direction),new CombatInput{parryPressed=true});
                    DarklightActionParticles.Observe(view,model,art,3);
                    impacts=Object.FindObjectsByType<DarklightCombatImpacts>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single();
                    // Normal gameplay has an active lab. Only the mesh is moved
                    // out of this deliberately inactive dependency-only context.
                    impacts.transform.SetParent(actor.transform,true);impacts.gameObject.layer=31;
                    late.Invoke(impacts,null);var renderer=impacts.GetComponent<MeshRenderer>();var mesh=impacts.GetComponent<MeshFilter>().sharedMesh;
                    camera.transform.position=particles.LastMotorcycleGuardPoint+Vector3.back*10;
                    camera.Render();RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,64,64),0,0);readback.Apply();
                    pixels[page.direction]=readback.GetPixels32().Count(c=>c.r+c.g+c.b>10);
                    Assert.That(pixels[page.direction]>0,Is.EqualTo(page.direction!=4),page.id+": actual offscreen spark pixels, north contact occluded");
                    if(page.direction!=4)
                    {
                        Assert.That(renderer.sharedMaterial.shader.isSupported,Is.True);
                        Assert.That(mesh.vertexCount,Is.InRange(4,32));
                        foreach(var v in mesh.vertices){Assert.That(v.x*64,Is.EqualTo(Mathf.Round(v.x*64)).Within(.0001f));Assert.That(v.y*64,Is.EqualTo(Mathf.Round(v.y*64)).Within(.0001f));}
                        Assert.That(Vector3.Distance(impacts.transform.TransformPoint(mesh.bounds.center),particles.LastMotorcycleGuardPoint),Is.LessThan(.3f));
                    }
                    Directory.CreateDirectory("outputs/retomada-20261103/guard-mesh-probe");File.WriteAllBytes("outputs/retomada-20261103/guard-mesh-probe/direction-"+page.direction+".png",readback.EncodeToPNG());
                    for(int i=0;i<20;i++)late.Invoke(impacts,null);
                    Assert.That(renderer.enabled,Is.False,"Guard mesh must expire without leaving visible geometry");
                    impacts.transform.SetParent(lab.transform,true);
                }
                var originalMesh=impacts.GetComponent<MeshFilter>().sharedMesh;
                for(int i=0;i<128;i++)DarklightCombatImpacts.EmitMotorcycle(Vector3.zero,Vector2.right,DarklightMotorcycleImpact.Guard);
                Assert.That(((Array)hitField.GetValue(impacts)).Length,Is.EqualTo(16));
                Assert.That(impacts.GetComponent<MeshFilter>().sharedMesh,Is.SameAs(originalMesh));
                Assert.That(Object.FindObjectsByType<DarklightCombatImpacts>(FindObjectsInactive.Include,FindObjectsSortMode.None),Has.Length.EqualTo(1));
                File.WriteAllText("outputs/retomada-20261103/guard-mesh-probe/pixels.json","{\"pixelCountsByDirection\":["+string.Join(",",pixels)+"],\"poolCapacity\":16,\"isolatedMeshProbe\":true,\"campaignSceneValidated\":false}");
                AssertSilent();Report("MotorcycleGuardMesh",8,8,"actual offscreen pixels, north occlusion, 64-grid geometry, expiry and mesh/pool reuse; not campaign composition QA");
            }
            finally
            {
                RenderTexture.active=priorTarget;Time.captureDeltaTime=priorCapture;particleInstance.SetValue(null,priorParticles);
                typeof(DarklightCombatLab).GetProperty("Instance").SetValue(null,null);typeof(DarklightCampaign).GetProperty("Instance").SetValue(null,null);
                camera.targetTexture=null;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(readback);
                Object.DestroyImmediate(cameraObject);Object.DestroyImmediate(context);
            }
        }
    }
}
#endif
