using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Darklight.Editor
{
    // Explicit opt-in Windows delivery. No AutoRunPlayer and no scene Play.
    public static class DarklightWeaponRedesignBuild
    {
        const string Art="Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/";
        const string Audio="Assets/Darklight/Resources/Darklight/Audio/Delivery20261023/";
        [Serializable] sealed class Gate
        {public string path,sha256,endTimeUtc;public int passed;[NonSerialized]public DateTime EndUtc;}
        [Serializable] sealed class Receipt
        {
            public string result,executable,previousRedesignBuild,exception;
            public Gate editMode,playMode;
            public int errors,warnings,redesignPages,redesignSheets,redesignFigures,deliveredEffects,totalAudioResources;
            public ulong bytes;public double seconds;
            public bool redesignComplete,audioEnabledForInteractiveLaunch,automationSilent=true,audioAuditioned=false,autoRunPlayer=false;
            public string activeSkin="Réquiem",activeForms="Normal / Ruptura / Eclipse Absoluto",activeWeapons="Katana / Punhos da Ruína / Moto";
        }
        [Serializable] sealed class AudioEntry{public string id,original,sha256,asset,assetSha256;}
        [Serializable] sealed class AudioManifest{public int sourceCount,preparedCount,deferredCount;public AudioEntry[] sources;}
        static string Hash(string path)
        {
            using var hash=SHA256.Create();using var stream=File.OpenRead(path);
            return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-","").ToLowerInvariant();
        }
        static void Require(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
        static string Argument(string name)
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,name);
            if(index<0||index+1>=args.Length||args[index+1].StartsWith("-",StringComparison.Ordinal))
                throw new ArgumentException("Provide the final test XML explicitly: "+name+" <path>");
            string path=Path.GetFullPath(args[index+1]);
            if(!File.Exists(path))throw new FileNotFoundException("Missing final test XML",path);
            return path;
        }
        static Gate ValidateGate(string path,int minimum,params (string name,int cases)[] required)
        {
            var root=XDocument.Load(path).Root;Require(root!=null&&root.Name.LocalName=="test-run","Not a Unity test result: "+path);
            var cases=root.Descendants("test-case").ToArray();
            Require((string)root.Attribute("result")=="Passed"&&cases.Length>=minimum&&
                cases.All(c=>(string)c.Attribute("result")=="Passed"),"Final tests are missing, skipped or failed: "+path);
            foreach(var group in required)
                Require(cases.Count(c=>((string)c.Attribute("classname")??"").EndsWith("."+group.name,StringComparison.Ordinal))>=group.cases,
                    "Final XML must include "+group.name+" (at least "+group.cases+"): "+path);
            string ended=(string)root.Attribute("end-time");
            Require(DateTime.TryParse(ended,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal|DateTimeStyles.AdjustToUniversal,out var end),
                "Final XML has no valid completion time: "+path);
            return new Gate{path=path,sha256=Hash(path),passed=cases.Length,endTimeUtc=ended,EndUtc=end};
        }
        static void ValidateFreshness(Gate edit,Gate play)
        {
            var files=Directory.GetFiles("Assets/Darklight","*.cs",SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Art,"*",SearchOption.AllDirectories).Where(p=>!p.EndsWith(".meta",StringComparison.OrdinalIgnoreCase)))
                .Concat(Directory.GetFiles("Assets/Darklight/Resources/Darklight/CombatContacts20261027","*",SearchOption.AllDirectories).Where(p=>!p.EndsWith(".meta",StringComparison.OrdinalIgnoreCase)))
                .Concat(Directory.GetFiles(Audio,"*.wav",SearchOption.TopDirectoryOnly))
                .Append("ArtSources/AudioDelivery20261023/manifest.json");
            foreach(string path in files)
            {
                DateTime changed=File.GetLastWriteTimeUtc(path);
                Require(changed<=edit.EndUtc.AddSeconds(1)&&changed<=play.EndUtc.AddSeconds(1),
                    "Final XML predates a relevant script/resource; rerun both final checks before building: "+path);
            }
        }
        static Sprite[] ValidateSheet(string path)
        {
            var sheet=JsonUtility.FromJson<DarklightWeaponRedesignImporter.Sheet>(File.ReadAllText(Path.ChangeExtension(path,"json")));
            Require(sheet!=null&&sheet.wholeFigures&&sheet.frames?.Length>0&&sheet.nativeSize?.Length==2&&
                sheet.image==Path.GetFileName(path)&&sheet.sourcePath?.StartsWith("ArtSources/",StringComparison.Ordinal)==true&&
                !sheet.sourcePath.Contains(".."),"Invalid complete-figure descriptor: "+path);
            Require(File.Exists(sheet.sourcePath)&&Hash(path)==sheet.imageSha256&&Hash(sheet.sourcePath)==sheet.sourceSha256,
                "A preserved source or imported PNG changed: "+path);
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;
            Require(importer!=null&&importer.textureType==TextureImporterType.Sprite&&importer.spriteImportMode==SpriteImportMode.Multiple&&
                importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&importer.textureCompression==TextureImporterCompression.Uncompressed,
                "Import the reviewed redesign sheet with its dedicated importer before building: "+path);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            Require(sprites.Length==sheet.frames.Length&&sprites.Select(s=>s.name).Distinct().Count()==sprites.Length,
                "Imported whole-figure count/identities do not match: "+path);
            foreach(var frame in sheet.frames)
            {
                var sprite=sprites.SingleOrDefault(s=>s.name==frame.id);
                Require(sprite!=null&&sprite.texture.width==sheet.nativeSize[0]&&sprite.texture.height==sheet.nativeSize[1]&&
                    Mathf.Abs(sprite.pixelsPerUnit-sheet.pixelsPerUnit)<.001f,"Missing or resized imported figure: "+frame.id);
            }
            return sprites;
        }
        static void ValidateRedesign(Receipt receipt)
        {
            var text=Resources.Load<TextAsset>(DarklightWeaponRedesignBank.Root+"catalog");
            Require(text!=null,"Missing imported redesign catalog");
            var catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(text.text);
            Require(DarklightWeaponRedesignBank.ValidCatalog(catalog)&&catalog.pages.Length>0,"Invalid redesign catalog");
            var sheets=new Dictionary<string,Sprite[]>(StringComparer.Ordinal);
            foreach(var page in catalog.pages)
            {
                string path="Assets/Darklight/Resources/"+page.resourcePath+".png";
                if(!sheets.TryGetValue(path,out var frames))sheets[path]=frames=ValidateSheet(path);
                Require(page.spriteNames.All(name=>frames.Any(s=>s.name==name)),"Unresolved imported context: "+page.id);
                Require(Hash(path)==page.imageSha256&&Hash(page.sourcePath)==page.sourceSha256,
                    "Catalog and actual imported source disagree: "+page.id);
                Require(frames.All(s=>s.texture.width==page.nativeSize[0]&&s.texture.height==page.nativeSize[1]&&
                    Mathf.Abs(s.pixelsPerUnit-page.pixelsPerUnit)<.001f),"Catalog scale/dimensions disagree: "+page.id);
            }
            // Preserve and validate legacy sheets, but gate delivery on the active arsenal.
            foreach(int weapon in new[]{0,2})foreach(int direction in Enumerable.Range(0,8))
                Require(catalog.pages.Any(p=>p.skin==0&&p.weapon==weapon&&p.form==0&&p.direction==direction&&p.action=="Idle"&&p.spriteNames.Length==4),
                    "Missing reviewed Requiem Normal idle for weapon/direction "+weapon+"/"+direction);
            string motorcycle=Art+"RequiemMotoForm0Ride8SlamS20261027V3.png";
            var motorcycleFrames=ValidateSheet(motorcycle);sheets[motorcycle]=motorcycleFrames;
            Require(motorcycleFrames.Length==12,"Require eight Requiem motorcycle views and four whole slam phases");
            string dualSaws="Assets/Darklight/Resources/"+DarklightMotorcycleRedesignBank.RequiemDualSawsResource+".png";
            var dualFrames=ValidateSheet(dualSaws);sheets[dualSaws]=dualFrames;
            Require(dualFrames.Length==12,"Require eight on-foot dual-saw views and four whole attack phases");
            string motorcycleIdle=Art+"RequiemMotoForm0IdleSSWW20261027MatureV1.png";
            var idleFrames=ValidateSheet(motorcycleIdle);sheets[motorcycleIdle]=idleFrames;
            Require(idleFrames.Length==12,"Require the complete four-phase mounted idle in S/SW/W");
            string rearIdle=Art+"RequiemMotoForm0IdleNW_N_NE20261027MatureV1.png";
            var rearIdleFrames=ValidateSheet(rearIdle);sheets[rearIdle]=rearIdleFrames;
            Require(rearIdleFrames.Length==12,"Require the complete four-phase rear mounted idle in NW/N/NE");
            string sideIdle=Art+"RequiemMotoForm0IdleE_SE20261027MatureV1.png";
            var sideIdleFrames=ValidateSheet(sideIdle);sheets[sideIdle]=sideIdleFrames;
            Require(sideIdleFrames.Length==8,"Require four-phase mature mounted idle in E/SE");
            Require(ValidateSheet("Assets/Darklight/Resources/Darklight/CombatContacts20261027/ContactBursts.png").Length==12,"Require the authored pixel-art contact clips");
            string icons=Art+"EquipmentIcons.png";
            if(File.Exists(icons))
            {
                var frames=ValidateSheet(icons);sheets[icons]=frames;
                foreach(int weapon in new[]{0,1,2})foreach(int form in new[]{0,1,2,3})
                    Require(frames.Any(s=>s.name=="Redesign_Icon_Weapon"+weapon+"_Form"+form),"Missing reviewed equipment icon");
            }
            receipt.redesignPages=catalog.pages.Length;receipt.redesignSheets=sheets.Count;
            receipt.redesignFigures=sheets.Values.Sum(s=>s.Length);receipt.redesignComplete=catalog.complete;
        }
        static void ValidateAudio(Receipt receipt)
        {
            var manifest=JsonUtility.FromJson<AudioManifest>(File.ReadAllText("ArtSources/AudioDelivery20261023/manifest.json"));
            Require(manifest?.sources!=null&&manifest.sourceCount==23&&manifest.sources.Length==23&&manifest.preparedCount==19&&manifest.deferredCount==4,
                "Unexpected audio delivery manifest");
            foreach(var entry in manifest.sources)
                Require(entry.original?.StartsWith("ArtSources/AudioDelivery20261023/",StringComparison.Ordinal)==true&&
                    !entry.original.Contains("..")&&File.Exists(entry.original)&&Hash(entry.original)==entry.sha256,
                    "A supplied SFX original changed or is missing: "+entry.id);
            var clips=Resources.LoadAll<AudioClip>("Darklight/Audio/Delivery20261023");
            Require(clips.Length==19,"Import all nineteen bounded SFX before building");
            foreach(string id in DarklightSoundDirector.DeliveredIds)
            {
                var entry=manifest.sources.SingleOrDefault(s=>s.id==id);
                Require(entry?.asset?.StartsWith(Audio,StringComparison.Ordinal)==true&&!entry.asset.Contains("..")&&
                    File.Exists(entry.asset)&&Hash(entry.asset)==entry.assetSha256,"Prepared SFX changed or is missing: "+id);
                var clip=clips.SingleOrDefault(c=>c.name==DarklightSoundDirector.DeliveredPrefix(id));
                Require(clip!=null&&clip.channels==1&&clip.length>=.2f&&clip.length<=6.6f&&
                    (clip.frequency==24000||clip.frequency==32000)&&clip.loadType==AudioClipLoadType.DecompressOnLoad,
                    "Invalid bounded SFX import: "+id);
                var importer=AssetImporter.GetAtPath(entry.asset) as AudioImporter;
                Require(importer!=null&&importer.defaultSampleSettings.compressionFormat==AudioCompressionFormat.PCM&&!importer.forceToMono,
                    "SFX import must preserve prepared PCM and gain: "+id);
            }
            var all=Resources.LoadAll<AudioClip>("Darklight/Audio");
            Require(all.Length==151&&all.Count(c=>!c.name.StartsWith("dl-user-",StringComparison.Ordinal))==132,
                "The existing audio bank must remain intact alongside the nineteen delivered effects");
            Require(Regex.IsMatch(File.ReadAllText("ProjectSettings/AudioManager.asset"),@"(?m)^\s*m_DisableAudio:\s*0\s*$"),
                "Project audio must stay enabled for the user's interactive executable");
            receipt.deliveredEffects=clips.Length;receipt.totalAudioResources=all.Length;receipt.audioEnabledForInteractiveLaunch=true;
        }
        static string PreservePrevious(string root,string output)
        {
            string prefix=Path.GetFullPath(root)+Path.DirectorySeparatorChar;
            Require(Path.GetFullPath(output).StartsWith(prefix,StringComparison.OrdinalIgnoreCase),"Build output must remain inside the redesign output folder");
            if(!Directory.Exists(output)||Directory.GetFileSystemEntries(output).Length==0)return null;
            string backup=Path.GetFullPath(Path.Combine(root,"Windows-previous-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss",CultureInfo.InvariantCulture)+"-"+Guid.NewGuid().ToString("N").Substring(0,6)));
            Require(backup.StartsWith(prefix,StringComparison.OrdinalIgnoreCase),"Previous build destination escaped the redesign output folder");
            Require((new DirectoryInfo(output).Attributes&FileAttributes.ReparsePoint)==0,"Do not move a linked build directory");
            Directory.Move(output,backup);return backup;
        }
        public static void Build()
        {
            // These are process-local guards; no audio setting or preference is
            // serialized into the build. Normal player startup resets Silent.
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            var receipt=new Receipt
            {
                editMode=ValidateGate(Argument("-darklightRedesignEditResults"),57,
                    ("DarklightDeliveredSfxTests",4),("DarklightAudioExpansionTests",7),("DarklightTechnicalDeliveryTests",3),("DarklightWeaponRedesignTests",7),("DarklightSimplifiedScopeTests",3),("DarklightSaveContractTests",6),("DarklightTestOutputTests",5)),
                playMode=ValidateGate(Argument("-darklightRedesignPlayResults"),12,
                    ("DarklightDeliveredSfxGameplayTests",2),("DarklightWeaponRedesignGameplayTests",3),("DarklightSimplifiedScopeGameplayTests",3))
            };
            ValidateFreshness(receipt.editMode,receipt.playMode);ValidateRedesign(receipt);ValidateAudio(receipt);
            string root=Path.GetFullPath("outputs/retomada-20261027"),output=Path.Combine(root,"Windows");
            Directory.CreateDirectory(root);receipt.previousRedesignBuild=PreservePrevious(root,output);Directory.CreateDirectory(output);
            receipt.executable=Path.Combine(output,"DarklightRequiem.exe");
            try
            {
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes=new[]{"Assets/Darklight/Scenes/DarklightCampaign.unity"},locationPathName=receipt.executable,
                    target=BuildTarget.StandaloneWindows64,options=BuildOptions.CompressWithLz4
                });
                var summary=report.summary;receipt.result=summary.result.ToString();receipt.errors=(int)summary.totalErrors;
                receipt.warnings=(int)summary.totalWarnings;receipt.bytes=summary.totalSize;receipt.seconds=summary.totalTime.TotalSeconds;
                Require(summary.result==BuildResult.Succeeded&&File.Exists(receipt.executable),"Weapon redesign Windows build failed");
                Debug.Log("WEAPON_REDESIGN_BUILD_OK: "+receipt.executable);
            }
            catch(Exception error){receipt.exception=error.Message;if(string.IsNullOrEmpty(receipt.result))receipt.result="Exception";throw;}
            finally{File.WriteAllText(Path.Combine(root,"build-result.json"),JsonUtility.ToJson(receipt,true));}
        }
    }
}
