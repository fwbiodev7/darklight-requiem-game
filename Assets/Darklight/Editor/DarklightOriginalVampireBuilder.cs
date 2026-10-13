using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightOriginalVampireBuilder
    {
        const string Root="Assets/Darklight/Resources/Darklight/OriginalVampireArt";
        static readonly Dictionary<Texture2D,Color32[]> pixels=new();
        static readonly Dictionary<Texture2D,Color32[]> masks=new();
        static readonly Dictionary<Texture2D,Texture2D> imported=new();
        static CharacterAppearanceProfile profile;
        struct Shape {public int left,right,bottom,top;public Vector2 lip;public float width;}
        static Color32[] Read(Sprite s)
        {
            if(!pixels.TryGetValue(s.texture,out var p))
            {
                var t=new Texture2D(2,2,TextureFormat.RGBA32,false);t.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(s.texture)));p=t.GetPixels32();pixels[s.texture]=p;UnityEngine.Object.DestroyImmediate(t);
            }
            return p;
        }
        static bool Brown(Color32 c)=>c.a>180&&c.r>28&&c.r<165&&c.r>c.g*1.12f&&c.g>c.b*1.06f&&c.g>20;
        static bool Skin(Color32 c)=>c.a>180&&c.r>150&&c.g>70&&c.g<215&&c.b>30&&c.b<180&&c.r>c.g+25&&c.g>c.b+10;
        static Shape Analyze(Sprite s)
        {
            var p=Read(s);int w=s.texture.width,x0=(int)s.rect.x,y0=(int)s.rect.y,x1=(int)s.rect.xMax,y1=(int)s.rect.yMax;
            int l=x1,r=x0,top=y0,fl=x1,fr=x0,ft=y0,fb=y1;
            int lower=y0+(y1-y0)*35/100;var visited=new HashSet<int>();var best=new List<int>();
            // Raised weapons can be above the head. Locate the largest actual scalp component,
            // rather than treating the sprite's upper third as the character's face.
            for(int y=lower;y<y1;y++)for(int x=x0;x<x1;x++)
            {
                int start=y*w+x;if(visited.Contains(start)||!Brown(p[start]))continue;
                var group=new List<int>();var queue=new Queue<int>();queue.Enqueue(start);visited.Add(start);
                while(queue.Count>0)
                {
                    int i=queue.Dequeue();group.Add(i);int px=i%w,py=i/w;
                    for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                    {
                        int nx=px+dx,ny=py+dy,ni=ny*w+nx;
                        if(nx<x0||nx>=x1||ny<lower||ny>=y1||visited.Contains(ni)||!Brown(p[ni]))continue;
                        visited.Add(ni);queue.Enqueue(ni);
                    }
                }
                if(group.Count>best.Count)best=group;
            }
            foreach(int i in best){l=Math.Min(l,i%w);r=Math.Max(r,i%w);top=Math.Max(top,i/w);}
            if(best.Count<5){l=x0+(x1-x0)/3;r=x0+(x1-x0)*2/3;top=y1-2;}
            int extension=Mathf.CeilToInt(s.pixelsPerUnit*.16f),initialL=l,initialR=r;
            for(int y=Mathf.Max(lower,top-Mathf.CeilToInt(s.pixelsPerUnit*.65f));y<=top;y++)for(int x=Mathf.Max(x0,initialL-extension);x<Mathf.Min(x1,initialR+extension);x++)if(Brown(p[y*w+x])){l=Math.Min(l,x);r=Math.Max(r,x);}
            for(int y=Mathf.Max(y0,top-Mathf.CeilToInt(s.pixelsPerUnit*.8f));y<top;y++)for(int x=l+(r-l)/5;x<r-(r-l)/5;x++)if(Skin(p[y*w+x])){fl=Math.Min(fl,x);fr=Math.Max(fr,x);ft=Math.Max(ft,y);fb=Math.Min(fb,y);}
            float cx=fr>=fl?(fl+fr)*.5f:(l+r)*.5f;
            float hw=Mathf.Clamp((r-l+1)/s.pixelsPerUnit,.35f,.95f);
            float lip=ft>fb?Mathf.Lerp(fb,ft,.28f):top-(y1-y0)*.19f;
            int bottom=Mathf.RoundToInt(lip-s.pixelsPerUnit*.16f);
            return new Shape{left=Mathf.Max(x0,Mathf.FloorToInt(cx-hw*s.pixelsPerUnit*.54f)),right=Mathf.Min(x1,Mathf.CeilToInt(cx+hw*s.pixelsPerUnit*.54f)),bottom=Mathf.Max(y0,bottom),top=Mathf.Min(y1,top+2),lip=new Vector2((cx-x0-s.pivot.x)/s.pixelsPerUnit,(lip-y0-s.pivot.y)/s.pixelsPerUnit),width=hw};
        }
        static OriginalVampireFrame Bind(Sprite s)
        {
            var shape=Analyze(s);var p=Read(s);int tw=s.texture.width;
            if(!masks.TryGetValue(s.texture,out var m)){m=new Color32[p.Length];masks[s.texture]=m;}
            int x0=(int)s.rect.x,y0=(int)s.rect.y,x1=(int)s.rect.xMax,y1=(int)s.rect.yMax,protectedCount=0;
            float cx=s.rect.x+s.pivot.x+shape.lip.x*s.pixelsPerUnit,cy=s.rect.y+s.pivot.y+shape.lip.y*s.pixelsPerUnit;
            int padding=Mathf.Max(3,Mathf.CeilToInt(s.pixelsPerUnit*.045f));
            var protect=new HashSet<int>();
            // Protect the original red weapon and its neighboring steel edge, guard and grip.
            for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
            {
                var c=p[y*tw+x];if(c.a<180||c.r<75||c.g>c.r*.42f||c.b>c.r*.55f)continue;
                if(x>=shape.left&&x<shape.right&&y>=shape.bottom&&y<shape.top)continue;
                for(int dy=-padding;dy<=padding;dy++)for(int dx=-padding;dx<=padding;dx++)if(x+dx>=x0&&x+dx<x1&&y+dy>=y0&&y+dy<y1)protect.Add((y+dy)*tw+x+dx);
            }
            for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)
            {
                int i=y*tw+x;var c=p[i];if(c.a<180)continue;
                if(protect.Contains(i)){protectedCount++;m[i]=new Color32();continue;}
                bool inHead=x>=shape.left&&x<shape.right&&y>=shape.bottom&&y<shape.top;
                bool iris=false;
                if(inHead&&y>cy+s.pixelsPerUnit*.01f&&y<cy+s.pixelsPerUnit*.16f)
                {
                    bool red=c.r>65&&c.g<50&&c.b<60;
                    float offset=Mathf.Abs(x-cx)/Mathf.Max(1,shape.width*s.pixelsPerUnit);
                    bool pupil=c.r<50&&c.g<40&&c.b<40&&offset>.09f&&offset<.32f;
                    bool nearFace=false;for(int dy=-2;dy<=2&&!nearFace;dy++)for(int dx=-2;dx<=2;dx++)if(x+dx>=x0&&x+dx<x1&&y+dy>=y0&&y+dy<y1&&Skin(p[(y+dy)*tw+x+dx])){nearFace=true;break;}
                    iris=(red||pupil)&&nearFace&&offset>.09f&&offset<.32f;
                }
                // Existing gray seams/studs only: never tint skin, brown hair or the white shirt emblem.
                bool neutral=Mathf.Max(c.r,Mathf.Max(c.g,c.b))-Mathf.Min(c.r,Mathf.Min(c.g,c.b))<18;
                bool trim=!inHead&&neutral&&c.r>45&&c.r<200&&y>y0+s.pixelsPerUnit*.38f&&y<shape.bottom&&Mathf.Abs(x-cx)<shape.width*s.pixelsPerUnit*.82f;
                m[i]=new Color32(iris?(byte)255:(byte)0,trim?(byte)180:(byte)0,0,inHead?(byte)255:(byte)0);
            }
            return new OriginalVampireFrame{source=s,lip=shape.lip,headWidth=shape.width,nativeAge=AssetDatabase.GetAssetPath(s).Contains("Stage"),protectedPixels=protectedCount};
        }
        static Texture2D SaveTexture(string path,int width,int height,Color32[] data,bool sprite,float ppu=100,Vector2 pivot=default)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);texture.SetPixels32(data);texture.Apply();var encoded=texture.EncodeToPNG();UnityEngine.Object.DestroyImmediate(texture);
            var existing=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(existing!=null&&File.Exists(path)&&File.ReadAllBytes(path).SequenceEqual(encoded))
            {
                var exact=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(!sprite||exact!=null&&Mathf.Abs(exact.pixelsPerUnit-ppu)<.001f&&Vector2.Distance(new Vector2(exact.pivot.x/width,exact.pivot.y/height),pivot)<.001f)return existing;
            }
            File.WriteAllBytes(path,encoded);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var ti=(TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType=sprite?TextureImporterType.Sprite:TextureImporterType.Default;ti.filterMode=FilterMode.Point;ti.mipmapEnabled=false;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=4096;ti.wrapMode=TextureWrapMode.Clamp;ti.alphaIsTransparency=sprite;ti.sRGBTexture=sprite;
            if(sprite){ti.spriteImportMode=SpriteImportMode.Single;ti.spritePixelsPerUnit=ppu;}
            ti.SaveAndReimport();
            if(sprite)
            {
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(ti);provider.InitSpriteEditorDataProvider();
                var capability=provider.GetDataProvider<ISpriteFrameEditCapability>();
                if(capability==null||!capability.GetEditCapability().HasCapability(EEditCapability.EditPivot))throw new InvalidOperationException("Missing exact head pivot capability");
                var rects=provider.GetSpriteRects();foreach(var rect in rects){rect.alignment=SpriteAlignment.Custom;rect.pivot=pivot;}
                provider.SetSpriteRects(rects);provider.Apply();ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        public static void Build()
        {
            pixels.Clear();masks.Clear();imported.Clear();Directory.CreateDirectory(Root);
            profile=AssetDatabase.LoadAssetAtPath<CharacterAppearanceProfile>("Assets/Darklight/Data/Appearance.asset");
            var v=UnityEngine.Object.FindFirstObjectByType<DarklightCharacterView>();var game=UnityEngine.Object.FindFirstObjectByType<DarklightCampaign>();if(v==null||game==null)throw new InvalidOperationException("Original campaign scene required");
            var originals=new HashSet<Sprite>();void Add(Sprite[] sprites){if(sprites!=null)foreach(var s in sprites)if(s!=null)originals.Add(s);}
            Add(v.idle);Add(v.walk);Add(v.slash);Add(v.actions);Add(v.motion);Add(game.rockIdle);Add(game.rockMotion);Add(game.rockActions);Add(game.rockSlash);
            foreach(var def in game.weaponDefinitions){Add(def.Poses(0));Add(def.Poses(1));}
            foreach(string folder in new[]{"AppearanceBodies","Walking"})foreach(string file in Directory.GetFiles("Assets/Darklight/Resources/Darklight/"+folder,"*.png"))Add(AssetDatabase.LoadAllAssetsAtPath(file).OfType<Sprite>().ToArray());
            var frames=originals.Select(Bind).ToArray();
            foreach(var pair in masks)
            {
                string guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(pair.Key));imported[pair.Key]=SaveTexture(Root+"/Mask-"+guid+".png",pair.Key.width,pair.Key.height,pair.Value,false);
            }
            foreach(var frame in frames)frame.mask=imported[frame.source.texture];
            const string catPath="Assets/Darklight/Resources/Darklight/OriginalVampireCatalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<DarklightOriginalVampireCatalog>(catPath);if(catalog==null){catalog=ScriptableObject.CreateInstance<DarklightOriginalVampireCatalog>();AssetDatabase.CreateAsset(catalog,catPath);}
            catalog.frames=frames;catalog.aura=Resources.LoadAll<Sprite>("Darklight/AnimationCatalog/OriginalFormAuras");Array.Sort(catalog.aura,(a,b)=>Number(a).CompareTo(Number(b)));
            if(catalog.aura.Length!=12)throw new InvalidOperationException("12 aura frames required");EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            File.WriteAllText(Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/original-vampire-build.txt")),"SUCCESS: "+frames.Length+" original frames; no separate heads; 12 aura frames; body and weapon textures unchanged");
        }
        static int Number(Sprite s)=>int.Parse(s.name.Substring(s.name.LastIndexOf('_')+1));
    }
}
