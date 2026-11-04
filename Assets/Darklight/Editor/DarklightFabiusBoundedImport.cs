using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Darklight.Editor
{
    // Explicit request only: never scans/reimports every historical atlas.
    public static class DarklightFabiusBoundedImport
    {
        [Serializable] sealed class Request { public string[] paths; public string receipt; }
        [Serializable] sealed class Sheet { public Frame[] frames; public int[] nativeSize; public float pixelsPerUnit; }
        [Serializable] sealed class Frame { public string id; public int[] rect; public float[] pivot; }
        [Serializable] sealed class Row { public string path,guid; public int sprites,width,height; public float pixelsPerUnit; public bool settingsVerified,rectsPivotsVerified; }
        [Serializable] sealed class Receipt { public string editorVersion; public bool completed,catalogValid; public Row[] sheets; public int sprites; public bool runtimeVerified; }
        const string Prefix="Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/";
        public static void Run()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-darklightFabiusRequest");
            if(at<0||at+1>=args.Length)throw new InvalidDataException("Explicit import request required");
            var request=JsonUtility.FromJson<Request>(File.ReadAllText(args[at+1]));
            if(request?.paths==null||request.paths.Length<1||request.paths.Length>20||string.IsNullOrEmpty(request.receipt))
                throw new InvalidDataException("Invalid bounded import request");
            foreach(string path in request.paths)
                if(!path.StartsWith(Prefix,StringComparison.Ordinal)||path.Contains("..")||!path.EndsWith(".png",StringComparison.Ordinal)||!File.Exists(path))
                    throw new InvalidDataException("Unexpected import target: "+path);
            var receipt=new Receipt {editorVersion=Application.unityVersion,sheets=new Row[request.paths.Length],runtimeVerified=false};
            for(int i=0;i<request.paths.Length;i++)
            {
                string path=request.paths[i];
                DarklightWeaponRedesignImporter.ImportStandalone(path);
                var sheet=JsonUtility.FromJson<Sheet>(File.ReadAllText(Path.ChangeExtension(path,"json")));
                var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(x=>x.name);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                bool settings=importer.textureType==TextureImporterType.Sprite&&importer.spriteImportMode==SpriteImportMode.Multiple&&
                    importer.filterMode==FilterMode.Point&&!importer.mipmapEnabled&&importer.npotScale==TextureImporterNPOTScale.None&&
                    importer.textureCompression==TextureImporterCompression.Uncompressed;
                bool geometry=sprites.Count==sheet.frames.Length;
                foreach(var frame in sheet.frames)
                {
                    if(!sprites.TryGetValue(frame.id,out var sprite)){geometry=false;continue;}
                    var expected=new Rect(frame.rect[0],sheet.nativeSize[1]-frame.rect[1]-frame.rect[3],frame.rect[2],frame.rect[3]);
                    var pivot=new Vector2(frame.pivot[0]*frame.rect[2],frame.pivot[1]*frame.rect[3]);
                    geometry&=sprite.rect==expected&&Vector2.Distance(sprite.pivot,pivot)<.01f&&Mathf.Abs(sprite.pixelsPerUnit-sheet.pixelsPerUnit)<.001f;
                }
                if(!settings||!geometry)throw new InvalidDataException("Imported settings/geometry disagree: "+path);
                receipt.sheets[i]=new Row {path=path,guid=AssetDatabase.AssetPathToGUID(path),sprites=sprites.Count,
                    width=sheet.nativeSize[0],height=sheet.nativeSize[1],pixelsPerUnit=sheet.pixelsPerUnit,
                    settingsVerified=settings,rectsPivotsVerified=geometry};
                receipt.sprites+=sprites.Count;
                Debug.Log("FABIUS_SHEET_IMPORTED "+path+" sprites="+sprites.Count);
            }
            var catalog=JsonUtility.FromJson<DarklightWeaponRedesignCatalog>(File.ReadAllText(Prefix+"catalog.json"));
            receipt.catalogValid=DarklightWeaponRedesignBank.ValidCatalog(catalog);
            if(!receipt.catalogValid)throw new InvalidDataException("Merged catalog rejected");
            AssetDatabase.SaveAssets();DarklightWeaponRedesignBank.ClearCache();receipt.completed=true;
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(request.receipt)));
            File.WriteAllText(request.receipt,JsonUtility.ToJson(receipt,true));
            Debug.Log("FABIUS_BOUNDED_IMPORT_COMPLETE sheets="+receipt.sheets.Length+" sprites="+receipt.sprites);
        }
    }
}
