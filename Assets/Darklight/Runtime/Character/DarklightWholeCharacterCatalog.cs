using System;
using UnityEngine;
namespace Darklight
{
    [Serializable] public sealed class WholeCharacterPalette { public Texture2D source;public string mask; }
    public sealed class DarklightWholeCharacterCatalog:ScriptableObject { public WholeCharacterPalette[] palettes; }
}
