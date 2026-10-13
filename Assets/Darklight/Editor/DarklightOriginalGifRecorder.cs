using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightOriginalGifRecorder
    {
        static GameObject root;static Camera camera;static RenderTexture target;static Texture2D readback;
        static DarklightCharacterView[] views;static SpriteRenderer[] auras;static Sprite[] auraFrames;
        static string folder;static int index;
        public static void Start(string output)
        {
            if(!Application.isPlaying||DarklightCampaign.Instance==null)throw new InvalidOperationException("Silent campaign preview required");
            folder=output;Directory.CreateDirectory(folder);index=0;DarklightAutomation.Silent=true;
            root=new GameObject("GravacaoAprovadaNaoSalvar"){hideFlags=HideFlags.DontSave};
            camera=new GameObject("CameraGif").AddComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.transform.position=new Vector3(0,0,-10);
            camera.orthographic=true;camera.orthographicSize=9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.025f,.035f);camera.cullingMask=1<<31;
            target=new RenderTexture(900,1080,24);target.filterMode=FilterMode.Point;camera.targetTexture=target;readback=new Texture2D(900,1080,TextureFormat.RGB24,false);
            views=new DarklightCharacterView[12];auras=new SpriteRenderer[12];
            auraFrames=Resources.LoadAll<Sprite>("Darklight/AnimationCatalog/OriginalFormAuras");Array.Sort(auraFrames,(a,b)=>Number(a).CompareTo(Number(b)));
            for(int age=0;age<4;age++)for(int form=0;form<3;form++)
            {
                int n=age*3+form;var obj=new GameObject("Idade"+(age+1)+"Forma"+(form+1));obj.transform.SetParent(root.transform,false);obj.transform.position=new Vector3((form-1)*4.6f,5.4f-age*4.1f,0);
                var body=new GameObject("Corpo").AddComponent<SpriteRenderer>();body.transform.SetParent(obj.transform,false);body.sharedMaterial=DarklightCampaign.Instance.pixelMaterial;
                views[n]=obj.AddComponent<DarklightCharacterView>();views[n].enabled=false;
            }
            EditorApplication.update+=Step;
        }
        static int Number(Sprite s)=>int.Parse(s.name.Substring(s.name.LastIndexOf('_')+1));
        static void Step()
        {
            try
            {
                int skin=index/72,frame=index%72,weapon=frame/24,phase=frame%24;
                var definition=DarklightCampaign.Instance.weaponDefinitions[weapon];
                Vector2 facing=phase<4?Vector2.down:phase<8?new Vector2(1,-1).normalized:Vector2.down;
                for(int n=0;n<12;n++)
                {
                    var model=new CombatModel(definition.loadout);Vector2 move=Vector2.zero;
                    if(phase<8){move=facing;model.Tick(.01f+(phase%4)*.1f,new CombatInput{move=move,aim=facing});}
                    else if(phase<16){model.Tick(.001f,new CombatInput{aim=facing,lightPressed=true});model.Tick(model.Attack.Duration*((phase-8)+.2f)/8.3f,new CombatInput{aim=facing});}
                    else if(phase<20){model.Tick(.001f,new CombatInput{aim=facing,parryPressed=true});model.Tick(.02f+(phase-16)*.055f,new CombatInput{aim=facing});}
                    else{model.Tick(.001f,new CombatInput{move=facing,aim=facing,dodgePressed=true});model.Tick(.025f+(phase-20)*.045f,new CombatInput{aim=facing});}
                    int form=n%3+1,age=n/3+1;views[n].PresentPreview(model,move,skin,age,form,definition);
                    foreach(var tr in views[n].GetComponentsInChildren<Transform>())tr.gameObject.layer=31;
                }
                camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,900,1080),0,0);readback.Apply();RenderTexture.active=previous;
                File.WriteAllBytes(Path.Combine(folder,(skin==0?"Requiem":"Rock")+"-"+frame.ToString("D3")+".png"),readback.EncodeToPNG());
                index++;if(index%24==0)File.WriteAllText(Path.Combine(folder,"progresso.txt"),index+"/144");
                if(index==144){Cleanup();File.WriteAllText(Path.Combine(folder,"resultado.txt"),"SUCCESS: 2 skins x 3 forms x 4 ages x 3 weapons x 4 movement families; actual Unity renderer");}
            }
            catch(Exception e){Cleanup();File.WriteAllText(Path.Combine(folder,"resultado.txt"),e.ToString());}
        }
        static void Cleanup()
        {
            EditorApplication.update-=Step;if(root!=null)UnityEngine.Object.DestroyImmediate(root);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(readback!=null)UnityEngine.Object.DestroyImmediate(readback);
        }
    }
}
