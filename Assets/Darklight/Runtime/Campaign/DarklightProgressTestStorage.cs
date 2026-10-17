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
                return null;
#endif
            }
        }
        public static bool HasIsolatedTestStorage=>IsolatedTestPath!=null;
#if UNITY_EDITOR
        static string editorTestPath;
        // Editor-only isolation for full campaign tests. A player build can never
        // redirect its save, and a fixture can never select the user's save folder.
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
