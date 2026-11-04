using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Darklight.Editor
{
    public static class DarklightNovemberIntegrationDelivery
    {
        [Serializable] sealed class Receipt
        {
            public string status="imported",completedUtc;
            public int clips,figures,sheets;
            public bool unityImported=true,runtimeVerified=false;
            public string[] resources;
        }
        public static void Import()
        {
            DarklightAutomation.Silent=true;AudioListener.pause=true;AudioListener.volume=0;
            const string bank="Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/";
            var catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(File.ReadAllText(bank+"catalog.json"));
            if(!DarklightWeaponRedesignBank.ValidCatalog(catalog))throw new InvalidDataException("Invalid animation catalog");
            var pages=catalog.pages.Where(p=>p.skin==0&&p.form==0&&((p.weapon==0&&p.action=="Attack"&&p.attackKey=="Pesado")||(p.weapon==0&&p.action=="Walk"&&(p.direction==5||p.direction==6||p.direction==7))||(p.weapon==2&&p.action=="Walk"&&(p.direction==3||p.direction==4||p.direction==5||p.direction==7)))).ToArray();
            var resources=pages.Select(p=>p.resourcePath).Distinct().OrderBy(p=>p).ToArray();
            int figures=pages.Sum(p=>p.spriteNames.Length);
            if(pages.Length!=15||figures!=60||resources.Length!=7)throw new InvalidDataException("Expected15 clips,60 figures,7 reviewed sheets");
            int previousWorkers=EditorUserSettings.desiredImportWorkerCount;
            try
            {
                EditorUserSettings.desiredImportWorkerCount=1;AssetDatabase.ForceToDesiredWorkerCount();
                foreach(var resource in resources)
                {
                    DarklightWeaponRedesignImporter.ImportStandalone("Assets/Darklight/Resources/"+resource+".png");
                    EditorUtility.UnloadUnusedAssetsImmediate();GC.Collect();
                }
                AssetDatabase.ImportAsset(bank+"catalog.json",ImportAssetOptions.ForceSynchronousImport);
                AssetDatabase.SaveAssets();DarklightWeaponRedesignBank.ClearCache();
                var receipt=new Receipt{completedUtc=DateTime.UtcNow.ToString("o"),clips=pages.Length,figures=figures,sheets=resources.Length,resources=resources};
                Directory.CreateDirectory("outputs/retomada-20261102");
                File.WriteAllText("outputs/retomada-20261102/unity-import-receipt.json",JsonUtility.ToJson(receipt,true));
                Debug.Log("NOVEMBER_INTEGRATION_IMPORTED:15clips/60figures/7sheets; runtime tests remain separate");
            }
            finally { EditorUserSettings.desiredImportWorkerCount=previousWorkers;AssetDatabase.ForceToDesiredWorkerCount(); }
        }
    }
}
