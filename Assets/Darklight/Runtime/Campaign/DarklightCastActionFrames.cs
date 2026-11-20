using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    static class DarklightCastActionFrames
    {
        const string ResourcesPath = "Darklight/OfficialCast/CastActions/";
        static readonly string[] Directions = { "S", "SW", "W", "NW", "N", "NE", "E", "SE" };
        static readonly Dictionary<string, Dictionary<string, Sprite>> Clips = new();
        public const float FallFrameTime = .112f;

        public static Sprite Get(string character, string action, int direction, int phase)
        {
            string prefix = $"{character}_{action}";
            string bearing = Directions[direction];
            if (!Clips.TryGetValue(prefix, out var frames))
            {
                frames = new Dictionary<string, Sprite>();
                foreach (string directions in character == "Lady" ? new[] { "S-E-N", "SW-W-NW", "SE-NE" } : new[] { "S-E-N" })
                    foreach (var sprite in Resources.LoadAll<Sprite>(ResourcesPath + prefix + "_" + directions))
                        frames[sprite.name] = sprite;
                Clips.Add(prefix, frames);
            }
            frames.TryGetValue($"{prefix}_{bearing}_{phase}", out var result);
            return result;
        }

        public static int AttackPhase(CombatModel model, AttackDefinition attack)
        {
            if (model.ActiveThisTick) return 2;
            var elapsed = model.StateTime;
            if (elapsed < attack.startup) return elapsed < attack.startup * .36f ? 0 : 1;
            return elapsed < attack.startup + attack.active + attack.recovery * .42f ? 3 : 4;
        }

        public static int FallPhase(float elapsed) => Mathf.Clamp((int)(elapsed / FallFrameTime), 0, 4);
    }
}
