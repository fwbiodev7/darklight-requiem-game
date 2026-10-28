using System;
using System.IO;
using UnityEngine;

namespace Darklight.Tests
{
    /// <summary>
    /// Centralized, portable output path resolver for automated tests and evidence generation.
    /// Replaces hardcoded absolute machine paths with safe, portable project or runtime paths.
    /// </summary>
    public static class DarklightTestOutput
    {
        const string DefaultSubfolder = "artifacts";

        /// <summary>
        /// Root directory for test evidence and artifacts.
        /// In Editor: outputs/test-artifacts/
        /// In Standalone Player: Application.temporaryCachePath/test-artifacts/
        /// </summary>
        public static string Root
        {
            get
            {
#if UNITY_EDITOR
                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string root = Path.Combine(projectRoot, "outputs", "test-artifacts");
#else
                string root = Path.Combine(Application.temporaryCachePath, "test-artifacts");
#endif
                string normalized = Path.GetFullPath(root).Replace('\\', '/');
                if (!Directory.Exists(normalized))
                {
                    Directory.CreateDirectory(normalized);
                }
                return normalized;
            }
        }

        /// <summary>
        /// Gets a safe, isolated directory under Root for a specific test suite or category.
        /// Creates the directory automatically if it does not exist.
        /// Returns the normalized path guaranteed to end with '/'.
        /// </summary>
        /// <param name="category">Subdirectory name or relative category path.</param>
        /// <returns>Normalized absolute path ending with '/'.</returns>
        public static string GetDirectory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                category = DefaultSubfolder;

            if (Path.IsPathRooted(category))
                throw new ArgumentException("Test output categories must be relative", nameof(category));
            category = category.Replace('\\', '/');
            foreach (string segment in category.Split('/'))
                if (segment == ".." || segment.IndexOf(':') >= 0)
                    throw new ArgumentException("Test output categories cannot traverse outside Root", nameof(category));
            category = category.Trim('/');
            if (string.IsNullOrWhiteSpace(category))
                category = DefaultSubfolder;

            string target = Path.Combine(Root, category);
            string fullPath = Path.GetFullPath(target).Replace('\\', '/');

            // Security check: ensure path cannot escape Root
            string rootNormalized = Root;
            if (!fullPath.Equals(rootNormalized, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.StartsWith(rootNormalized + "/", StringComparison.OrdinalIgnoreCase))
            {
                fullPath = rootNormalized + "/" + DefaultSubfolder;
            }

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (!fullPath.EndsWith("/"))
                fullPath += "/";

            return fullPath;
        }

        /// <summary>
        /// Resolves a full file path for writing test evidence inside a designated category directory.
        /// </summary>
        public static string GetFilePath(string category, string fileName)
        {
            string dir = GetDirectory(category);
            string safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeName) || safeName == "." || safeName == "..")
                throw new ArgumentException("A test artifact needs a file name", nameof(fileName));
            return dir + safeName;
        }
    }
}
