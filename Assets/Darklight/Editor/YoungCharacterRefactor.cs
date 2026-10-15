using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Darklight.Editor
{
    public static class YoungCharacterRefactor
    {
        [Serializable] class Plan { public Input[] inputs; }
        [Serializable] class Input { public string name,source,palette,target,paletteTarget,sourceHash; public bool move; }
        [Serializable] class Report { public bool complete; public int moved,copied,deleted,framesKept; public long removedBytes; public string imported,error; public string[] removed; }
        static readonly string Work="C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/young-only-20261014";
        const string Root="Assets/Darklight/";
        static string Checked(string relative)
        {
            if(!relative.StartsWith(Root,StringComparison.Ordinal)||relative.Contains(".."))throw new InvalidOperationException("Unsafe asset path: "+relative);
            var full=Path.GetFullPath(relative);
            var allowed=Path.GetFullPath(Root).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)+Path.DirectorySeparatorChar;
            if(!full.StartsWith(allowed,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Asset outside game: "+full);
            return full;
        }
        static string Hash(string path)
        { using var sha=System.Security.Cryptography.SHA256.Create();return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant(); }
        public static string Apply()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle");
            var report=new Report();var removed=new List<string>();
            try
            {
                var plan=JsonUtility.FromJson<Plan>(File.ReadAllText(Work+"/young-inputs.json"));
                if(plan.inputs.Length!=23)throw new InvalidOperationException("Unexpected young source count");
                var keep=new HashSet<string>(StringComparer.Ordinal);
                // Validate every input before the first mutation. A repeated call can resume completed moves.
                foreach(var item in plan.inputs)
                {
                    Checked(item.target);Checked(item.paletteTarget);keep.Add(item.target);keep.Add(item.paletteTarget);
                    string input=item.move?Checked(File.Exists(item.source)?item.source:item.target):Path.GetFullPath(item.source);
                    if(!item.move&&!input.StartsWith(Path.GetFullPath(Path.GetDirectoryName(Work)+"/resume-20261014/prepared")+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Unapproved input: "+input);
                    if(Hash(input)!=item.sourceHash)throw new InvalidOperationException("Source bytes changed: "+item.name);
                    if(!File.Exists(item.move&& !File.Exists(item.palette)?item.paletteTarget:item.palette))throw new FileNotFoundException("Palette missing",item.palette);
                }
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach(var item in plan.inputs)
                    {
                        foreach(var pair in new[]{new[]{item.source,item.target},new[]{item.palette,item.paletteTarget}})
                        {
                            if(item.move)
                            {
                                if(!File.Exists(pair[0]))continue;
                                string error=AssetDatabase.MoveAsset(pair[0],pair[1]);
                                if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);
                                report.moved++;
                            }
                            else if(!File.Exists(pair[1])){File.Copy(pair[0],Checked(pair[1]),false);report.copied++;}
                        }
                    }
                    File.Copy(Work+"/young-manifest.json",Checked(Root+"ArtSources/WholeCharacters20261013/manifest.json"),true);
                }
                finally { AssetDatabase.StopAssetEditing(); }
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                // Import first, so a rejected manifest cannot destroy the existing art.
                report.imported=TodayCharacterImporter.Import();
                foreach(var item in plan.inputs)if(Hash(Checked(item.target))!=item.sourceHash)throw new InvalidOperationException("Move changed pixels: "+item.name);
                string[] retired={Root+"Resources/Darklight/AppearanceBodies/",Root+"Resources/Darklight/Walking/",Root+"Resources/Darklight/WholeCharacterAnimations/",Root+"Resources/Darklight/WholeCharacterPalettes/"};
                bool Retired(string path)=>retired.Any(path.StartsWith);
                var original=AssetDatabase.LoadAssetAtPath<DarklightOriginalVampireCatalog>(Root+"Resources/Darklight/OriginalVampireCatalog.asset");
                if(original!=null)
                {
                    original.frames=original.frames.Where(f=>f?.source!=null&&!Retired(AssetDatabase.GetAssetPath(f.source))&&(!f.nativeAge||keep.Contains(AssetDatabase.GetAssetPath(f.source)))).ToArray();
                    foreach(var f in original.frames)f.nativeAge=false;
                    report.framesKept=original.frames.Length;EditorUtility.SetDirty(original);
                    typeof(DarklightOriginalVampireCatalog).GetField("lookup",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)?.SetValue(original,null);
                }
                foreach(string guid in AssetDatabase.FindAssets("t:CharacterAppearanceProfile",new[]{Root.TrimEnd('/')}))
                {
                    var profile=AssetDatabase.LoadAssetAtPath<CharacterAppearanceProfile>(AssetDatabase.GUIDToAssetPath(guid));
                    profile.stages=new[]{new AppearanceStage{name="Jovem",minimumNarrativeDays=0}};
                    profile.completeAnimationStages=new[]{true};profile.allowHeightVariants=false;
                    profile.hair=profile.windHair=profile.facialHair=profile.shoulders=Array.Empty<Sprite>();
                    profile.attachments=Array.Empty<AppearanceAttachment>();profile.bodyVariants=Array.Empty<AppearanceBodyVariant>();EditorUtility.SetDirty(profile);
                }
                var whole=AssetDatabase.LoadAssetAtPath<DarklightWholeCharacterCatalog>(Root+"Resources/Darklight/WholeCharacterCatalog.asset");
                if(whole!=null){whole.palettes=Array.Empty<WholeCharacterPalette>();EditorUtility.SetDirty(whole);}
                AssetDatabase.SaveAssets();
                var delete=new HashSet<string>(StringComparer.Ordinal);
                foreach(string folder in retired.Append(Root+"Resources/Darklight/TodayCharacterAnimations/"))
                    if(Directory.Exists(folder))foreach(string file in Directory.GetFiles(Checked(folder),"*",SearchOption.TopDirectoryOnly))
                    {
                        string relative=file.Substring(Directory.GetCurrentDirectory().Length+1).Replace('\\','/');
                        if(!relative.EndsWith(".meta",StringComparison.Ordinal)&&!keep.Contains(relative))delete.Add(relative);
                    }
                string oldCatalog=Root+"Resources/Darklight/AnimationCatalog/";
                foreach(string file in Directory.GetFiles(Checked(oldCatalog),"*",SearchOption.TopDirectoryOnly))
                {
                    string leaf=Path.GetFileName(file);
                    if((leaf.StartsWith("Rock")||leaf.StartsWith("Requiem"))&&leaf.Contains("Stage")&&!leaf.EndsWith(".meta"))delete.Add(oldCatalog+leaf);
                }
                var masks=new HashSet<string>(original?.frames.Where(f=>f.mask!=null).Select(f=>AssetDatabase.GetAssetPath(f.mask))??Enumerable.Empty<string>());
                string maskFolder=Root+"Resources/Darklight/OriginalVampireArt/";
                if(Directory.Exists(maskFolder))foreach(string file in Directory.GetFiles(Checked(maskFolder),"*.png"))
                {string relative=maskFolder+Path.GetFileName(file);if(!masks.Contains(relative))delete.Add(relative);}
                AssetDatabase.StartAssetEditing();
                try
                {
                    foreach(string path in delete.OrderBy(p=>p,StringComparer.Ordinal))
                    {
                        long bytes=new FileInfo(Checked(path)).Length;
                        if(!AssetDatabase.DeleteAsset(path))throw new InvalidOperationException("Could not delete retired art: "+path);
                        report.deleted++;report.removedBytes+=bytes;removed.Add(path);
                    }
                }
                finally { AssetDatabase.StopAssetEditing(); }
                File.WriteAllText(Checked(Root+"ArtSources/WholeCharacters20261013/generation-prompts.json"),JsonUtility.ToJson(plan,true));
                AssetDatabase.SaveAssets();AssetDatabase.Refresh();TodayCharacterBank.ClearCache();
                report.complete=true;
            }
            catch(Exception ex){report.error=ex.ToString();Debug.LogException(ex);}
            finally{report.removed=removed.ToArray();File.WriteAllText(Work+"/applied-result.json",JsonUtility.ToJson(report,true));}
            return report.complete?report.imported+"; removed "+report.deleted+" retired files":report.error;
        }
    }
}
