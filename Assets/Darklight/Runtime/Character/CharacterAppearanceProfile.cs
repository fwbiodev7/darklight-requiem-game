using System;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed class AppearanceStage
    {
        public string name;
        public int minimumNarrativeDays;
        [Range(0,4)] public int hair, facialHair;
        [Range(0,2)] public int physique;
        // Height is an authored sprite variant, never a Transform scale multiplier.
        public int heightVariant;
    }
    [Serializable] public sealed class AppearanceAttachment
    {
        public Sprite source;
        public Vector2 crown, upperLip, shoulders;
        public float headWidth, shoulderWidth;
    }
    [Serializable] public sealed class AppearanceBodyVariant
    {
        public Sprite source, replacement;
        public int physique,heightVariant;
    }
    [CreateAssetMenu(menuName="Darklight/Character Appearance")]
    public sealed class CharacterAppearanceProfile : ScriptableObject
    {
        public int startingAge=17;
        public int[] chapterDays={0,73,146,219,292,365};
        public AppearanceStage[] stages;
        public Sprite[] hair, windHair, facialHair, shoulders;
        public AppearanceAttachment[] attachments;
        public AppearanceBodyVariant[] bodyVariants;
        public bool allowHeightVariants;
        public AppearanceAttachment Find(Sprite source)
        {
            if(attachments!=null)foreach(var item in attachments)if(item.source==source)return item;
            return null;
        }
        public int StageForDays(int days)
        {
            int stage=0;
            if(stages!=null)for(int i=0;i<stages.Length;i++)if(days>=stages[i].minimumNarrativeDays)stage=i;
            return stage;
        }
    }
}
