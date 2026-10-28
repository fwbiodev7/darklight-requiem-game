using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Darklight.Editor
{
    // Explicit EditMode inspection: no gameplay, save access, audio or scene writes.
    public static class DarklightExpansionReview
    {
        const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Capture()
        {
            DarklightMultiverseImporter.Import();
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new InvalidOperationException("A graphics device is required");
            EditorSceneManager.OpenScene("Assets/Darklight/Scenes/DarklightCampaign.unity");
            var game=UnityEngine.Object.FindFirstObjectByType<DarklightCampaign>();
            if(game==null)throw new InvalidOperationException("Missing campaign component");
            string output=Path.GetFullPath("outputs/expansao-20261023/unity-editmode");Directory.CreateDirectory(output);
            var cameraObject=new GameObject("ExpansionEditCamera");var camera=cameraObject.AddComponent<Camera>();
            camera.enabled=false;camera.orthographic=true;camera.orthographicSize=4.3f;camera.cullingMask=1<<31;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.07f,.1f);camera.allowHDR=false;camera.allowMSAA=false;
            var target=new RenderTexture(960,540,24,RenderTextureFormat.ARGB32){filterMode=FilterMode.Point};target.Create();camera.targetTexture=target;
            var previous=RenderTexture.active;var checks=new List<string>();int captures=0;
            try
            {
                for(int chapter=0;chapter<10;chapter++)
                {
                    var map=DarklightMapDefinition.ForChapter(chapter);var root=new GameObject("ExpansionMapReview_"+chapter);
                    try
                    {
                        typeof(DarklightCampaign).GetProperty("Map").SetValue(game,map);
                        typeof(DarklightCampaign).GetField("levelRoot",Private).SetValue(game,root.transform);
                        DarklightMapLayout.Build(root.transform,chapter,game.floorTiles,game.architecture,game.pixelMaterial);
                        Physics2D.SyncTransforms();typeof(DarklightCampaign).GetMethod("BuildMultiverse",Private).Invoke(game,new object[]{chapter});
                        foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                        var points=(Dictionary<int,Vector2>)typeof(DarklightCampaign).GetField("multiversePoints",Private).GetValue(game);
                        if(points.Count==0)throw new InvalidOperationException("No references on map "+chapter);
                        foreach(var point in points)
                        {
                            var cell=new Vector2Int(Mathf.FloorToInt(point.Value.x/map.CellSize),Mathf.FloorToInt(point.Value.y/map.CellSize));
                            if(!map.Walkable(cell))throw new InvalidOperationException("Unreachable reference "+point.Key);
                            var prop=root.transform.Find("VestígioMultiversal_"+point.Key)?.GetComponent<SpriteRenderer>();
                            if(prop?.sprite==null)throw new InvalidOperationException("Missing rendered prop "+point.Key);
                            camera.transform.position=new Vector3(point.Value.x,point.Value.y+1.3f,-10);
                            Write(camera,target,Path.Combine(output,"map-"+(chapter+1).ToString("00")+"-relic-"+point.Key.ToString("00")+".png"));captures++;
                            checks.Add("map="+(chapter+1)+" entry="+point.Key+" walkable=true sprite="+prop.sprite.name);
                        }
                    }
                    finally{UnityEngine.Object.DestroyImmediate(root);Physics2D.SyncTransforms();}
                }
                File.WriteAllLines(Path.Combine(output,"map-integration.txt"),checks);
                Debug.Log("EXPANSION_EDIT_CAPTURE_OK: "+captures+" actual map/prop captures, 10 maps; no Play/audio/saves");
            }
            finally
            {
                RenderTexture.active=previous;camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(cameraObject);
            }
        }
        static void Write(Camera camera,RenderTexture target,string path)
        {
            var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);
            try{DarklightNativeCapture.Render(camera,target);RenderTexture.active=target;image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
