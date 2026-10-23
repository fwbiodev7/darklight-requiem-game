using System;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightBridgeEditReview
    {
        public static void Validate()
        {
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
            var original=UnityEngine.Object.FindFirstObjectByType<DarklightCombatLab>().Player.GetComponent<CapsuleCollider2D>();
            var size=original.size;var offset=original.offset;var direction=original.direction;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var map=DarklightMapDefinition.ForChapter(4);var root=new GameObject("BridgeEditCollisionReview");
            typeof(DarklightPaintedMap).GetMethod("BuildGeometry",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{root.transform,map});
            var actor=new GameObject("ActualPlayerCapsuleEditProbe");var body=actor.AddComponent<Rigidbody2D>();body.gravityScale=0;
            var capsule=actor.AddComponent<CapsuleCollider2D>();capsule.size=size;capsule.offset=offset;capsule.direction=direction;
            // Follow the visible landings: bridge 2 turns south at its east bank,
            // and bridge 3 turns north-east. Foreground protection rectangles also
            // include rails/void and are not traversal centerlines.
            var decks=new[]{
                new[]{new Vector2(515,254),new Vector2(563,255),new Vector2(605,267),new Vector2(648,285)},
                new[]{new Vector2(603,500),new Vector2(651,500),new Vector2(711,500),new Vector2(748,518),new Vector2(790,501)},
                new[]{new Vector2(1060,674),new Vector2(1115,675),new Vector2(1165,681),new Vector2(1203,654),new Vector2(1245,632)}};
            var report=new System.Text.StringBuilder();int crossings=0;var failures=new System.Collections.Generic.List<string>();
            try
            {
                foreach(var deck in decks)
                {
                    foreach(bool reverse in new[]{false,true})
                    {
                        var path=(Vector2[])deck.Clone();if(reverse)Array.Reverse(path);
                        body.position=map.Art.World(path[0]);Physics2D.SyncTransforms();int totalSteps=0;float remaining=0;
                        for(int segment=1;segment<path.Length;segment++)
                        {
                            var b=map.Art.World(path[segment]);int steps=0;
                            while(Vector2.Distance(body.position,b)>.06f&&steps++<400)
                            {
                                var velocity=Vector2.ClampMagnitude((b-body.position)/.02f,4.5f);
                                body.position+=DarklightNavigation.SlidingVelocity(body,velocity,.02f)*.02f;Physics2D.SyncTransforms();
                            }
                            remaining=Vector2.Distance(body.position,b);totalSteps+=steps;
                            if(remaining>.06f){failures.Add("Bridge "+(crossings/2+1)+" segment "+segment+" reverse="+reverse+" remaining="+remaining);break;}
                        }
                        report.AppendLine("bridge="+(crossings/2+1)+" reverse="+reverse+" steps="+totalSteps+" remaining="+remaining.ToString("F4"));
                        crossings++;
                    }
                }
                string output=Path.GetFullPath("outputs/expansao-20261023/bridge-edit-review.txt");Directory.CreateDirectory(Path.GetDirectoryName(output));
                report.AppendLine("EDITMODE ONLY: manually advanced actual capsule through existing collision queries; no Play, physics simulation, audio, controller or performance certification.");File.WriteAllText(output,report.ToString());
                if(failures.Count>0)throw new InvalidOperationException(string.Join("; ",failures));
                Debug.Log("BRIDGE_EDIT_REVIEW_OK: "+crossings+" bidirectional crossings, actual capsule and production SlidingVelocity");
            }
            finally{UnityEngine.Object.DestroyImmediate(actor);UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}
