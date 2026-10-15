using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Darklight.Editor
{
    // Review assets only. Does not change player animation selection or campaign data.
    public static class DefinitiveAnimationPreview
    {
        const string Root="Assets/Darklight/ArtSources/DefinitiveApproved20261012";
        [Serializable] sealed class Frame {public string name;public float[] rect,pivot;}
        [Serializable] sealed class Sheet {public string name;public float pixelsPerUnit;public Frame[] frames;}
        [Serializable] sealed class Sheets {public Sheet[] sheets;}
        sealed class Pose {public Sprite sprite;public Texture2D mask;public Vector2 lip;public float width;}
        static readonly Dictionary<string,Sprite[]> sprites=new();
        static readonly Dictionary<string,Pose[]> poses=new();
        static readonly Dictionary<Texture2D,Color32[]> pixels=new();
        static GameObject root;static Camera camera;static RenderTexture target;static Texture2D readback;static Material material;
        static SpriteRenderer[] bodies,heads,weapons,auras;
        static string folder;static int index;
        static readonly int[] timeline={0,1,0,2,0,1,0,2, 0,3,3,4,5,5,0,0, 0,6,6,6,0, 0,7,7,2,0};
        static Color32[] Read(Texture2D tex)
        {
            if(!pixels.TryGetValue(tex,out var p)){var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(tex)));p=t.GetPixels32();pixels[tex]=p;UnityEngine.Object.DestroyImmediate(t);}return p;
        }
        static int Number(Sprite s)=>int.Parse(s.name[(s.name.LastIndexOf('_')+1)..]);
        static bool Brown(Color32 c)=>c.a>166&&c.r>28&&c.r<195&&c.r>c.g*1.12f&&c.g>c.b*1.06f&&c.g>20;
        static bool Red(Color32 c)=>c.a>166&&c.r>75&&c.r>c.g*3.5f&&c.r>c.b*3.5f;
        static bool Skin(Color32 c)=>c.a>166&&c.r>200&&c.g>90&&c.g<215&&c.b>30&&c.b<175&&c.r>c.g+60&&c.g>c.b+20;
        static void Import(Sheet sheet)
        {
            string path=Root+"/"+sheet.name+".png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Multiple;ti.spritePixelsPerUnit=sheet.pixelsPerUnit;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.alphaIsTransparency=true;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=4096;ti.wrapMode=TextureWrapMode.Clamp;ti.SaveAndReimport();
            var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(ti);provider.InitSpriteEditorDataProvider();
            var cap=provider.GetDataProvider<ISpriteFrameEditCapability>();var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(cap==null||names==null||!cap.GetEditCapability().HasCapability(EEditCapability.CreateAndDeleteSprite|EEditCapability.EditSpriteRect|EEditCapability.EditPivot|EEditCapability.EditSpriteName))throw new InvalidOperationException("Sprite editor capabilities missing");
            var old=provider.GetSpriteRects().ToDictionary(r=>r.name,r=>r.spriteID);int h=AssetDatabase.LoadAssetAtPath<Texture2D>(path).height;
            var rects=sheet.frames.Select(f=>new SpriteRect{name=f.name,spriteID=old.TryGetValue(f.name,out var id)?id:GUID.Generate(),rect=new Rect(f.rect[0],h-f.rect[1]-f.rect[3],f.rect[2],f.rect[3]),alignment=SpriteAlignment.Custom,pivot=new Vector2(f.pivot[0],f.pivot[1])}).ToArray();
            provider.SetSpriteRects(rects);names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));provider.Apply();ti.SaveAndReimport();
            var bank=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();Array.Sort(bank,(a,b)=>Number(a).CompareTo(Number(b)));sprites[sheet.name]=bank;
        }
        static Pose[] BuildPoses(string name)
        {
            var bank=sprites[name];var source=bank[0].texture;var p=Read(source);var mask=new Color32[p.Length];var result=new Pose[8];int tw=source.width;
            var analyze=typeof(DarklightOriginalVampireBuilder).GetMethod("Analyze",BindingFlags.NonPublic|BindingFlags.Static);
            for(int n=0;n<8;n++)
            {
                var s=bank[n];var shape=analyze.Invoke(null,new object[]{s});var st=shape.GetType();
                int Field(string key)=>(int)st.GetField(key).GetValue(shape);
                int l=Field("left"),r=Field("right"),b=Field("bottom"),t=Field("top");var lip=(Vector2)st.GetField("lip").GetValue(shape);float width=(float)st.GetField("width").GetValue(shape);
                int x0=(int)s.rect.x,y0=(int)s.rect.y,x1=(int)s.rect.xMax,y1=(int)s.rect.yMax;
                var visited=new HashSet<int>();var protect=new HashSet<int>();
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
                {
                    int start=y*tw+x;if(visited.Contains(start)||!Red(p[start]))continue;var group=new List<int>();var q=new Queue<int>();q.Enqueue(start);visited.Add(start);int minX=x,maxX=x,minY=y,maxY=y;
                    while(q.Count>0){int i=q.Dequeue();group.Add(i);int px=i%tw,py=i/tw;minX=Math.Min(minX,px);maxX=Math.Max(maxX,px);minY=Math.Min(minY,py);maxY=Math.Max(maxY,py);
                        for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){int nx=px+dx,ny=py+dy,j=ny*tw+nx;if(nx<x0||nx>=x1||ny<y0||ny>=y1||visited.Contains(j)||!Red(p[j]))continue;visited.Add(j);q.Enqueue(j);}}
                    if(group.Count>35&&(maxX-minX>(r-l)*1.12f||maxY-minY>(r-l)*1.8f))foreach(int i in group)for(int dy=-3;dy<=3;dy++)for(int dx=-3;dx<=3;dx++){int nx=i%tw+dx,ny=i/tw+dy;if(nx>=x0&&nx<x1&&ny>=y0&&ny<y1)protect.Add(ny*tw+nx);}
                }
                // Selectors preserve source color pixels; they only identify palette/head/weapon regions.
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
                {
                    int i=y*tw+x;var c=p[i];if(c.a<166)continue;bool head=x>=l&&x<r&&y>=b&&y<t;
                    bool hair=head&&Brown(c)&&!Skin(c);bool red=Red(c)&&!(head&&Brown(c));
                    float mouthY=s.rect.y+s.pivot.y+lip.y*s.pixelsPerUnit,centerX=s.rect.x+s.pivot.x+lip.x*s.pixelsPerUnit;
                    // Keep the original neckline, lapels and shoulders. Remove only the old hair/face.
                    bool face=head&&y>=mouthY-s.pixelsPerUnit*.05f&&Mathf.Abs(x-centerX)<width*s.pixelsPerUnit*.40f&&(Skin(c)||red||c.r>150&&c.g>65&&c.r>c.g+25);
                    bool erase=hair||face;
                    mask[i]=new Color32(red?(byte)255:(byte)0,hair?(byte)255:(byte)0,protect.Contains(i)?(byte)255:(byte)0,erase?(byte)255:(byte)0);
                }
                result[n]=new Pose{sprite=s,lip=lip,width=width};
            }
            var texture=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);texture.SetPixels32(mask);texture.Apply();texture.filterMode=FilterMode.Point;texture.wrapMode=TextureWrapMode.Clamp;
            foreach(var pose in result)pose.mask=texture;return result;
        }
        static Vector2 FacePivot(Sprite s,out float width)
        {
            var p=Read(s.texture);int tw=s.texture.width,l=(int)s.rect.xMax,r=(int)s.rect.x,b=(int)s.rect.yMax,t=(int)s.rect.y;
            for(int y=(int)s.rect.y;y<s.rect.yMax;y++)for(int x=(int)s.rect.x;x<s.rect.xMax;x++)if(Skin(p[y*tw+x])){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);}
            if(r<l)throw new InvalidOperationException("Face pixels absent: "+s.name);width=r-l+1;return new Vector2(((l+r)*.5f-s.rect.x-s.pivot.x)/s.pixelsPerUnit,(Mathf.Lerp(b,t,.28f)-s.rect.y-s.pivot.y)/s.pixelsPerUnit);
        }
        public static void Start(string output)
        {
            if(root!=null)throw new InvalidOperationException("Preview already running");folder=output;Directory.CreateDirectory(folder);index=0;pixels.Clear();sprites.Clear();poses.Clear();
            var sheets=JsonUtility.FromJson<Sheets>(File.ReadAllText(Root+"/sheets.json"));foreach(var s in sheets.sheets)Import(s);
            foreach(string skin in new[]{"Requiem","Rock"})foreach(string weapon in new[]{"Katana","Daggers","Chain"})poses[skin+weapon+"Motion"]=BuildPoses(skin+weapon+"Motion");
            material=new Material(Shader.Find("Darklight/DefinitiveAnimationPreview"));if(material.shader==null)throw new InvalidOperationException("Preview shader missing");
            root=new GameObject("PreviewDefinitivoNaoSalvar"){hideFlags=HideFlags.DontSave};camera=new GameObject("CameraGif").AddComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.transform.position=new Vector3(0,0,-10);camera.orthographic=true;camera.orthographicSize=9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.025f,.025f,.035f);camera.cullingMask=1<<31;camera.allowMSAA=false;camera.allowHDR=false;camera.allowDynamicResolution=false;
            target=new RenderTexture(1200,1440,24){filterMode=FilterMode.Point,antiAliasing=1};camera.targetTexture=target;readback=new Texture2D(1200,1440,TextureFormat.RGB24,false);
            bodies=new SpriteRenderer[12];heads=new SpriteRenderer[12];weapons=new SpriteRenderer[12];auras=new SpriteRenderer[12];
            for(int n=0;n<12;n++){var tr=new GameObject("Idade"+(n/3+1)+"Forma"+(n%3+1)).transform;tr.SetParent(root.transform,false);tr.localPosition=new Vector3((n%3-1)*4.6f,5.4f-n/3*4.1f,0);tr.localScale=Vector3.one*DarklightCharacterView.WorldVisualScale;
                SpriteRenderer Add(string name,int order){var sr=new GameObject(name).AddComponent<SpriteRenderer>();sr.transform.SetParent(tr,false);sr.gameObject.layer=31;sr.sharedMaterial=material;sr.sortingOrder=order;return sr;}
                bodies[n]=Add("CorpoAprovado",0);heads[n]=Add("IdadeAprovada",1);weapons[n]=Add("ArmaPreservada",2);auras[n]=Add("AuraProxima",-1);}
            EditorApplication.update+=Step;
        }
        static void Step()
        {
            try
            {
                int perSkin=timeline.Length*3,skinIndex=index/perSkin,frame=index%perSkin,weaponIndex=frame/timeline.Length,phase=frame%timeline.Length;
                string skin=skinIndex==0?"Requiem":"Rock",weapon=new[]{"Katana","Daggers","Chain"}[weaponIndex];var pose=poses[skin+weapon+"Motion"][timeline[phase]];var headBank=sprites[skin+"Heads"];
                // One scalp scale for the complete age bank: longer hair never enlarges the face.
                float headFit=poses[skin+weapon+"Motion"][0].width/headBank[0].bounds.size.x;
                for(int n=0;n<12;n++)
                {
                    int age=n/3,form=n%3+1;bool aged=true;var block=new MaterialPropertyBlock();block.SetTexture("_Mask",pose.mask);block.SetFloat("_Form",form);block.SetFloat("_CutHead",1);
                    bodies[n].sprite=pose.sprite;bodies[n].SetPropertyBlock(block);
                    weapons[n].enabled=aged;weapons[n].sprite=pose.sprite;block.SetFloat("_WeaponOnly",1);block.SetFloat("_CutHead",0);weapons[n].SetPropertyBlock(block);
                    heads[n].enabled=aged;if(aged){var hs=headBank[age*3+form-1];var offset=FacePivot(hs,out _);heads[n].sprite=hs;heads[n].transform.localScale=Vector3.one*headFit;heads[n].transform.localPosition=(Vector3)(pose.lip-offset*headFit);heads[n].transform.localRotation=timeline[phase]==7?Quaternion.Euler(0,0,-12):Quaternion.identity;block.Clear();block.SetFloat("_Raw",1);heads[n].SetPropertyBlock(block);}
                    auras[n].sprite=sprites["TightAuras"][(phase%4)*3+form-1];auras[n].transform.localScale=new Vector3(1.1f/auras[n].sprite.bounds.size.x,2.05f/auras[n].sprite.bounds.size.y,1);auras[n].transform.localPosition=Vector3.up*.02f;block.Clear();block.SetFloat("_Raw",1);block.SetFloat("_AlphaClip",.08f);auras[n].SetPropertyBlock(block);
                }
                camera.Render();var prev=RenderTexture.active;RenderTexture.active=target;readback.ReadPixels(new Rect(0,0,1200,1440),0,0);readback.Apply();RenderTexture.active=prev;File.WriteAllBytes(Path.Combine(folder,skin+"-"+frame.ToString("D3")+".png"),readback.EncodeToPNG());
                index++;if(index%26==0)File.WriteAllText(Path.Combine(folder,"progresso.txt"),index+"/"+(perSkin*2));
                if(index==perSkin*2){Cleanup();File.WriteAllText(Path.Combine(folder,"resultado.txt"),"SUCCESS: front animation review; 2 skins, 4 ages, 3 forms, 3 weapons, walk/attack/guard/dodge; no gameplay activation");}
            }
            catch(Exception e){Cleanup();File.WriteAllText(Path.Combine(folder,"resultado.txt"),e.ToString());}
        }
        static void Cleanup(){EditorApplication.update-=Step;if(root!=null)UnityEngine.Object.DestroyImmediate(root);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}if(readback!=null)UnityEngine.Object.DestroyImmediate(readback);if(material!=null)UnityEngine.Object.DestroyImmediate(material);foreach(var bank in poses.Values)if(bank.Length>0&&bank[0].mask!=null)UnityEngine.Object.DestroyImmediate(bank[0].mask);}
    }
}
