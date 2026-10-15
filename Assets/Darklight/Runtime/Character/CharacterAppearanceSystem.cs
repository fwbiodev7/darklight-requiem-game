using UnityEngine;
using UnityEngine.UI;
namespace Darklight
{
    // The protagonist keeps the approved young identity in every chapter.
    // Legacy save/API fields remain readable; they no longer select age artwork.
    public sealed class CharacterAppearanceSystem:MonoBehaviour
    {
        public CharacterAppearanceProfile profile;
        public int Stage=>0;
        public int RenderingStage=>0;
        public int Physique=>0;
        bool cleaned;
        public void Restore(DarklightProgress state){if(state!=null)state.appearanceStage=0;}
        public void PreviewStage(int ignored){}
        public Sprite ResolveBody(Sprite original)=>original;
        public Sprite IdleBody(int skin,int direction,float clock=0,bool requireCompleteAnimations=false)=>TodayCharacterBank.YoungIdle(skin,0,direction,clock);
        static void RemoveLegacy(Transform root)
        {
            foreach(var child in root.GetComponentsInChildren<Transform>(true))
            {
                if(child==root)continue;
                if(child.name!="Aparencia_Ombros"&&child.name!="Aparencia_Cabelo"&&child.name!="Aparencia_Cavanhaque"&&child.name!="AparenciaRetratoCabelo"&&child.name!="AparenciaRetratoRosto")continue;
                child.gameObject.SetActive(false);
                if(Application.isPlaying)Object.Destroy(child.gameObject);else Object.DestroyImmediate(child.gameObject);
            }
        }
        public void Present(SpriteRenderer display,int facing,int skin,int form,CombatModel model){if(display==null||cleaned)return;RemoveLegacy(display.transform);cleaned=true;}
        public void UpdatePortrait(Image portrait){if(portrait!=null)RemoveLegacy(portrait.transform);}
    }
}
