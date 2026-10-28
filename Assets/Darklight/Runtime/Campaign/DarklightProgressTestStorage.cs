using System;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightProgress
    {
        static string IsolatedTestPath
        {
            get
            {
#if UNITY_EDITOR
                return editorTestPath;
#else
                return playerTestPath;
#endif
            }
        }
        public static bool HasIsolatedTestStorage=>IsolatedTestPath!=null;
#if !UNITY_EDITOR
        static string playerTestPath;
        // Explicit standalone QA only. Normal launches keep their existing save location.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ConfigureStandaloneTestStorage()
        {
            playerTestPath=null;
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"--darklight-isolated-session");
            if(index<0)return;
            if(index+1>=args.Length||args[index+1].StartsWith("-",StringComparison.Ordinal))
                throw new ArgumentException("--darklight-isolated-session requires a QA output directory");
            string root=System.IO.Path.GetFullPath(args[index+1]);
            string real=System.IO.Path.GetFullPath(Application.persistentDataPath).TrimEnd(System.IO.Path.DirectorySeparatorChar);
            if(root.Equals(real,StringComparison.OrdinalIgnoreCase)||root.StartsWith(real+System.IO.Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Standalone QA must not use the user's save directory");
            string session=System.IO.Path.Combine(root,"session-"+Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(session);
            playerTestPath=System.IO.Path.Combine(session,"isolated-campaign.json");
            System.IO.File.WriteAllText(System.IO.Path.Combine(root,"active-session.txt"),session);
        }
#endif
#if UNITY_EDITOR
        static string editorTestPath;
        // Editor fixtures and explicit standalone QA use distinct process-local scopes.
        public static IDisposable IsolateStorageForTests(string path)
        {
            string absolute=System.IO.Path.GetFullPath(path);
            string root=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../work"))+System.IO.Path.DirectorySeparatorChar;
            if(!absolute.StartsWith(root,StringComparison.OrdinalIgnoreCase)||!absolute.EndsWith(".json",StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Test saves must be JSON files inside this project's work directory",nameof(path));
            if(editorTestPath!=null)throw new InvalidOperationException("A save fixture is already active");
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolute));editorTestPath=absolute;
            return new StorageScope();
        }
        sealed class StorageScope:IDisposable
        {
            bool disposed;
            public void Dispose(){if(disposed)return;disposed=true;editorTestPath=null;}
        }
#endif
    }
}
