using UnityEngine;

namespace Darklight
{
    /// <summary>
    /// Sistema centralizado de ordenação de profundidade (Z-sorting) milimétrico para Unity 2D.
    /// Elimina inversões de camada e Z-fighting entre Player, Inimigos, NPCs e Cenário.
    /// </summary>
    public static class DarklightSorting
    {
        public const int BaseOrder = 3000;
        public const float Precision = 100f;

        public static int OrderForY(float y)
        {
            return BaseOrder - Mathf.RoundToInt(y * Precision);
        }
    }
}

