using System.Globalization;
namespace Darklight
{
    // Grapheme boundaries keep accented letters intact, even in decomposed text.
    public sealed class DarklightDialogueReveal
    {
        readonly string text; readonly int[] boundaries; int count; float remaining;
        public DarklightDialogueReveal(string value){text=value??"";boundaries=StringInfo.ParseCombiningCharacters(text);}
        public bool Complete=>count>=boundaries.Length;
        public int Count=>count;
        public string VisibleText=>Complete?text:text.Substring(0,boundaries[count]);
        public void RevealAll(){count=boundaries.Length;remaining=0;}
        public bool Tick(float delta)
        {
            if(Complete||delta<=0)return false;
            int old=count;remaining-=delta;
            while(remaining<=0&&!Complete){char c=text[boundaries[count++]];remaining+=1f/30+(c=='.'||c=='!'||c=='?'?.14f:c==','||c==';'||c==':'?.06f:0);}
            return old!=count;
        }
    }
}
