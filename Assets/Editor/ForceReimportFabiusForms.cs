// This editor script forces Unity to reimport the Fabius form assets before building.
// It is temporary and can be removed after a successful build.
using UnityEditor;
using UnityEngine;

public static class ForceReimportFabiusForms
{
    // Entry point for the batch build.
    public static void Build()
    {
        Darklight.Editor.DarklightFabiusFormsImporter.ImportAll();
        // Now invoke the existing build method.
        Darklight.Editor.DarklightPlaytestBuilder.Build();
    }
}
