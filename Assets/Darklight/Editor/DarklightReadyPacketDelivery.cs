using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightReadyPacketDelivery
    {
        [Serializable] sealed class Request { public string[] resources;public int clips,figures;public string receiptPath; }
        [Serializable] sealed class Receipt { public bool unityImported=true,runtimeVerified=false,testsDeferredByUser=true;public int clips,figures,sheets;public string completedUtc;public string[] resources; }
        public static void Import()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-darklightImportRequest");
            if(index<0||index+1>=args.Length)throw new ArgumentException("Import request required");
            var request=JsonUtility.FromJson<Request>(File.ReadAllText(args[index+1]));
            if(request?.resources==null||request.resources.Length==0||request.resources.Distinct().Count()!=request.resources.Length||request.resources.Any(r=>!r.StartsWith(DarklightWeaponRedesignBank.Root,StringComparison.Ordinal)||r.Contains("..")))throw new InvalidDataException("Invalid import resources");
            string output=Path.GetFullPath(request.receiptPath),allowed=Path.GetFullPath("outputs")+Path.DirectorySeparatorChar;
            if(!output.StartsWith(allowed,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("Receipt must remain in outputs");
            var catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(File.ReadAllText("Assets/Darklight/Resources/"+DarklightWeaponRedesignBank.Root+"catalog.json"));
            var pages=catalog.pages.Where(p=>request.resources.Contains(p.resourcePath)).ToArray();
            if(!DarklightWeaponRedesignBank.ValidCatalog(catalog)||pages.Length!=request.clips||pages.Sum(p=>p.spriteNames.Length)!=request.figures)throw new InvalidDataException("Packet and catalog disagree");
            int previousWorkers=EditorUserSettings.desiredImportWorkerCount;
            try
            {
                EditorUserSettings.desiredImportWorkerCount=1;AssetDatabase.ForceToDesiredWorkerCount();
                foreach(string resource in request.resources)
                {
                    DarklightWeaponRedesignImporter.ImportStandalone("Assets/Darklight/Resources/"+resource+".png");
                    EditorUtility.UnloadUnusedAssetsImmediate();GC.Collect();
                }
                AssetDatabase.ImportAsset("Assets/Darklight/Resources/"+DarklightWeaponRedesignBank.Root+"catalog.json",ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.SaveAssets();DarklightWeaponRedesignBank.ClearCache();
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                File.WriteAllText(output,JsonUtility.ToJson(new Receipt{clips=pages.Length,figures=request.figures,sheets=request.resources.Length,completedUtc=DateTime.UtcNow.ToString("o"),resources=request.resources},true));
                Debug.Log("READY_PACKET_IMPORTED:"+pages.Length+"clips; tests deferred by user");
            }
            finally { EditorUserSettings.desiredImportWorkerCount=previousWorkers;AssetDatabase.ForceToDesiredWorkerCount(); }
        }
    }
}
