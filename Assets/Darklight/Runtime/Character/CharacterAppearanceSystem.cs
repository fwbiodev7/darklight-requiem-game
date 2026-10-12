using UnityEngine;
using UnityEngine.UI;

namespace Darklight
{
    // Narrative age remains save data. Appearance comes from complete authored sprites.
    public sealed class CharacterAppearanceSystem : MonoBehaviour
    {
        public CharacterAppearanceProfile profile;
        public int Stage {get;private set;}
        public int Physique=>0;
        public int RenderingStage=>profile!=null&&profile.HasCompleteAnimations(Stage)?Stage:0;
        int cachedStage=-1,cachedSkin=-1;
        Sprite[] idleBodies;
        bool cleaned;
        public void Restore(DarklightProgress state)
        {
            if(profile==null||state==null)return;
            state.appearanceChapter=Mathf.Max(state.appearanceChapter,state.chapter);
            int chapter=Mathf.Clamp(state.appearanceChapter,0,profile.chapterDays.Length-1);
            state.narrativeDays=Mathf.Max(state.narrativeDays,profile.chapterDays[chapter]);
            Stage=Mathf.Clamp(Mathf.Max(state.appearanceStage,profile.StageForDays(state.narrativeDays)),0,profile.stages.Length-1);
            state.appearanceStage=Stage;
        }
        public void PreviewStage(int stage)=>Stage=profile==null?0:Mathf.Clamp(stage,0,profile.stages.Length-1);
        // Never reapply the former body-width variants to complete animation art.
        public Sprite ResolveBody(Sprite original)=>original;
        public Sprite IdleBody(int selectedSkin,int direction,float clock=0,bool requireCompleteAnimations=false)
        {
            int age=requireCompleteAnimations?RenderingStage:Stage;
            if(age==0)return null;
            selectedSkin=Mathf.Clamp(selectedSkin,0,1);
            if(cachedStage!=age||cachedSkin!=selectedSkin)
            {
                cachedStage=age;cachedSkin=selectedSkin;
                idleBodies=Resources.LoadAll<Sprite>("Darklight/AppearanceBodies/"+(selectedSkin==1?"Rock":"Requiem")+"IdleStage"+age);
                System.Array.Sort(idleBodies,(a,b)=>int.Parse(a.name.Substring(a.name.LastIndexOf('_')+1)).CompareTo(int.Parse(b.name.Substring(b.name.LastIndexOf('_')+1))));
            }
            return idleBodies?.Length==16?idleBodies[Mathf.Clamp(direction,0,7)*2+(int)(clock*2)%2]:null;
        }
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
        public void Present(SpriteRenderer display,int facing,int selectedSkin,int darkForm,CombatModel model)
        {
            if(display==null||cleaned)return;
            RemoveLegacy(display.transform);cleaned=true;
        }
        public void UpdatePortrait(Image portrait)
        {
            if(portrait==null)return;
            RemoveLegacy(portrait.transform);
            var replacement=IdleBody(DarklightCampaign.Instance?.SelectedSkin??0,0);
            if(replacement!=null)portrait.sprite=replacement;
        }
    }
}
