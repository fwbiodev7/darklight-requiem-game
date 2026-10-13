using System;
using UnityEngine;
namespace Darklight
{
    [Serializable] public sealed class CompleteAgeFrame
    {
        public Sprite source;
        public int age;
        public string page;
        public int index;
    }
    [Serializable] public sealed class CompleteAgePalette
    {
        public Texture2D source;
        public string mask;
    }
    public sealed class DarklightCompleteAgeCatalog : ScriptableObject
    {
        public CompleteAgeFrame[] frames;
        public CompleteAgePalette[] nativePalettes;
    }
}
