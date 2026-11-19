using System;
using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    // Additive, all-or-nothing locomotion. No legacy page, pivot or attack is replaced.
    public static class DarklightFistsCombatStanceBank
    {
        public const string Root="Darklight/FistsCombatStance20261118/";
        [Serializable] public sealed class Frame
        {public string id;public int[] rect;public float[] pivot;public int durationMs;}
        [Serializable] public sealed class Page
        {
            public string id,action,resourcePath,imageSha256,sourceSha256,sourcePath,status;
            public int direction;public int[] nativeSize,sequence,durationMs;public float pixelsPerUnit;
            public bool wholeFigures,loop;public Frame[] frames;
            [NonSerialized] internal Sprite[] sprites;
            [NonSerialized] internal DarklightWeaponRedesignPage clock;
        }
        [Serializable] public sealed class Catalog
        {public int schemaVersion,skin,weapon,form;public bool complete,importConfigured;public Page[] pages;}
        static readonly Page[,] poses=new Page[2,8];
        static readonly HashSet<Sprite> authored=new();
        static bool attempted,ready;
        static bool Name(string value)
        {
            if(string.IsNullOrEmpty(value))return false;
            foreach(char c in value)if(!(c>='A'&&c<='Z'||c>='a'&&c<='z'||c>='0'&&c<='9'||c=='_'||c=='-'))return false;
            return true;
        }
        public static bool ValidCatalog(Catalog data)
        {
            if(data==null||data.schemaVersion!=1||data.skin!=0||data.weapon!=2||data.form!=0||
                !data.complete||!data.importConfigured||data.pages?.Length!=16)return false;
            var ids=new HashSet<string>(StringComparer.Ordinal);var frames=new HashSet<string>(StringComparer.Ordinal);
            var contexts=new HashSet<string>(StringComparer.Ordinal);
            var importers=new Dictionary<string,Page>(StringComparer.Ordinal);
            foreach(var page in data.pages)
            {
                if(page==null||!Name(page.id)||!ids.Add(page.id)||(page.action!="Idle"&&page.action!="Walk")||
                    page.direction<0||page.direction>7||!contexts.Add(page.action+":"+page.direction)||
                    page.status!="visual-reviewed"||!page.wholeFigures||!page.loop||
                    page.frames==null||page.frames.Length<(page.action=="Idle"?2:3)||
                    page.sequence==null||page.durationMs?.Length!=page.sequence.Length||page.sequence.Length==0||
                    !DarklightWeaponRedesignBank.Sha256(page.imageSha256)||!DarklightWeaponRedesignBank.Sha256(page.sourceSha256)||
                    string.IsNullOrEmpty(page.sourcePath)||!page.sourcePath.StartsWith("ArtSources/",StringComparison.Ordinal)||page.sourcePath.Contains("..")||
                    string.IsNullOrEmpty(page.resourcePath)||!page.resourcePath.StartsWith(Root,StringComparison.Ordinal)||!Name(page.resourcePath.Substring(Root.Length))||
                    page.nativeSize?.Length!=2||page.nativeSize[0]<=0||page.nativeSize[1]<=0||
                    page.pixelsPerUnit<=0||float.IsNaN(page.pixelsPerUnit)||float.IsInfinity(page.pixelsPerUnit))return false;
                // Unity assigns PPU per texture, not per directional clip.
                if(importers.TryGetValue(page.resourcePath,out var shared))
                {
                    if(shared.pixelsPerUnit!=page.pixelsPerUnit||shared.imageSha256!=page.imageSha256||
                        shared.nativeSize[0]!=page.nativeSize[0]||shared.nativeSize[1]!=page.nativeSize[1])return false;
                }
                else importers[page.resourcePath]=page;
                foreach(var frame in page.frames)
                {
                    if(frame==null||!Name(frame.id)||!frame.id.StartsWith("Redesign_FabiusFistsCombat",StringComparison.Ordinal)||
                        !frames.Add(frame.id)||frame.durationMs<=0||frame.rect?.Length!=4||
                        frame.rect[0]<0||frame.rect[1]<0||frame.rect[2]<=0||frame.rect[3]<=0||
                        (long)frame.rect[0]+frame.rect[2]>page.nativeSize[0]||(long)frame.rect[1]+frame.rect[3]>page.nativeSize[1]||
                        frame.pivot?.Length!=2)return false;
                    foreach(float coordinate in frame.pivot)
                        if(float.IsNaN(coordinate)||float.IsInfinity(coordinate)||coordinate<0||coordinate>1)return false;
                }
                var used=new bool[page.frames.Length];
                for(int i=0;i<page.sequence.Length;i++)
                {
                    int index=page.sequence[i];
                    if(index<0||index>=page.frames.Length||page.durationMs[i]<=0)return false;
                    used[index]=true;
                }
                foreach(bool included in used)if(!included)return false;
            }
            return contexts.Count==16;
        }
        static void Load()
        {
            if(attempted)return;attempted=true;
            var text=Resources.Load<TextAsset>(Root+"catalog");if(text==null)return;
            Catalog data;
            try{data=JsonUtility.FromJson<Catalog>(text.text);}catch(ArgumentException){return;}
            if(!ValidCatalog(data))return;
            var sheets=new Dictionary<string,Dictionary<string,Sprite>>(StringComparer.Ordinal);
            foreach(var page in data.pages)
            {
                if(!sheets.TryGetValue(page.resourcePath,out var named))
                {
                    named=new(StringComparer.Ordinal);
                    foreach(var sprite in Resources.LoadAll<Sprite>(page.resourcePath))
                        if(sprite!=null&&!named.TryAdd(sprite.name,sprite))return;
                    sheets[page.resourcePath]=named;
                }
                page.sprites=new Sprite[page.frames.Length];
                for(int i=0;i<page.frames.Length;i++)
                {
                    var frame=page.frames[i];
                    if(!named.TryGetValue(frame.id,out var sprite)||sprite.texture.width!=page.nativeSize[0]||
                        sprite.texture.height!=page.nativeSize[1]||Mathf.Abs(sprite.pixelsPerUnit-page.pixelsPerUnit)>.001f||
                        sprite.texture.filterMode!=FilterMode.Point||sprite.texture.mipmapCount>1||
                        sprite.texture.wrapMode!=TextureWrapMode.Clamp)return;
                    var expected=new Rect(frame.rect[0],page.nativeSize[1]-frame.rect[1]-frame.rect[3],frame.rect[2],frame.rect[3]);
                    if(sprite.rect!=expected||Mathf.Abs(sprite.pivot.x-frame.pivot[0]*frame.rect[2])>.05f||
                        Mathf.Abs(sprite.pivot.y-frame.pivot[1]*frame.rect[3])>.05f)return;
                    page.sprites[i]=sprite;
                }
                var names=new string[page.sequence.Length];
                for(int i=0;i<names.Length;i++)names[i]=page.frames[page.sequence[i]].id;
                page.clock=new DarklightWeaponRedesignPage{spriteNames=names,durationMs=page.durationMs,loop=true};
            }
            // Publish only after every native sprite, rect, pivot and density is
            // present in Unity. A configured .meta alone cannot activate the kit.
            foreach(var page in data.pages)
            {
                poses[page.action=="Idle"?0:1,page.direction]=page;
                foreach(var sprite in page.sprites)authored.Add(sprite);
            }
            ready=true;
        }
        public static bool TryPose(string action,int direction,float time,out Sprite sprite)
        {
            sprite=null;
            if((action!="Idle"&&action!="Walk")||direction<0||direction>7)return false;
            Load();if(!ready)return false;
            var page=poses[action=="Idle"?0:1,direction];
            int frame=DarklightWeaponRedesignBank.Frame(page.clock,time);
            if(frame<0)return false;sprite=page.sprites[page.sequence[frame]];return true;
        }
        public static bool IsAuthored(Sprite sprite)=>ready&&sprite!=null&&authored.Contains(sprite);
        public static void ClearCache()
        {
            attempted=ready=false;authored.Clear();Array.Clear(poses,0,poses.Length);
        }
    }
}
