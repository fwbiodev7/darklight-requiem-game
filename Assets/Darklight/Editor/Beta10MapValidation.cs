using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace Darklight.Editor
{
    public static class Beta10MapValidation
    {
        public static string Validate()
        {
            DarklightAutomation.Silent=true;
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
            var player=UnityEngine.Object.FindFirstObjectByType<DarklightCombatLab>().Player;
            var actual=player.GetComponent<CapsuleCollider2D>();var size=actual.size;var offset=actual.offset;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var pipeline=GraphicsSettings.currentRenderPipeline;
            if(pipeline==null||!pipeline.GetType().FullName.Contains("Universal"))throw new InvalidOperationException("Expected existing URP pipeline");
            var root=new GameObject("Map4CollisionCheck");
            var map=DarklightMapDefinition.ForChapter(3);
            typeof(DarklightPaintedMap).GetMethod("BuildGeometry",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{root.transform,map});
            Physics2D.SyncTransforms();
            var landing=map.Art.World(new Vector2(776,800));
            foreach(var pixel in new[]{new Vector2(650,800),new Vector2(980,800)})
                if(!DarklightNavigation.Clear(landing,map.Art.World(pixel)))throw new InvalidOperationException("Lower landing exit blocked: "+pixel);
            var actor=new GameObject("ActualPlayerCapsule");var body=actor.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;
            actor.AddComponent<DarklightCombatant>();
            var capsule=actor.AddComponent<CapsuleCollider2D>();capsule.size=size;capsule.offset=offset;
            var nav=root.GetComponent<DarklightNavigation>();var route=new List<Vector2>();int routes=0,steps=0;
            var targets=new List<Vector2>(map.Arenas);targets.Add(map.Relic);targets.Add(map.Portal);
            foreach(var start in map.Checkpoints)
            foreach(var target in targets)
            {
                body.position=map.SafeSpawn(start);Physics2D.SyncTransforms();
                var destination=map.SafeSpawn(target);
                if(!nav.FindPath(body.position,destination,route))throw new InvalidOperationException("Map4 checkpoint route unavailable: "+start+" -> "+target);
                foreach(var point in route)
                {
                    int attempts=0;
                    while(Vector2.Distance(body.position,point)>.025f)
                    {
                        var delta=Vector2.ClampMagnitude(point-body.position,.08f);
                        var motion=DarklightNavigation.SlidingVelocity(body,delta/.02f,.02f)*.02f;
                        if(motion.sqrMagnitude<.0000001f||++attempts>100)throw new InvalidOperationException("Actual player capsule stuck at "+body.position+" towards "+point);
                        body.position+=motion;Physics2D.SyncTransforms();steps++;
                    }
                }
                routes++;
            }
            UnityEngine.Object.DestroyImmediate(actor);UnityEngine.Object.DestroyImmediate(root);
            return "Map4: "+routes+" routes, "+steps+" player-capsule steps; both landing exits clear; URP preserved.";
        }
    }
}
