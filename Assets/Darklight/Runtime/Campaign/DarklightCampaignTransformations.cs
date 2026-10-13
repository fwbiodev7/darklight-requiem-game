using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        float formPresentationTime,formPresentationDuration;
        int presentationForm;bool enteringForm,formReleased;
        public bool Transforming=>formPresentationTime>0;
        public float TransformationProgress=>Transforming?1-formPresentationTime/formPresentationDuration:1;
        // Gameplay stats use Form; presentation changes at the energy release, never by scaling the body.
        public int PresentedForm=>!Transforming?Form:enteringForm?(formReleased?Form:0):(formReleased?0:presentationForm);
        public bool TryTransformPlayer()
        {
            if(!Active||MenuDismissedThisFrame||health.IsDead||Transforming||VampireCasting||WearingHeadphones||Weapons.Swapping||lab.Player.Model.State!=CombatState.Neutral)return false;
            if(Form>0){EndForm();return true;}
            int unlocked=Progress.FormUnlocked;
            if(unlocked==0){Notice("RUPTURA DESPERTA APOS KAEL");return false;}
            if(Energy<60){Notice("TRANSFORMACAO: PRECISA 60 DE SANGUE");return false;}
            Form=Progress.preferredForm>0?Mathf.Min(unlocked,Progress.preferredForm):unlocked;
            Energy-=60;FormTime=12+Progress.mastery*2;BeginFormPresentation(Form,true);
            Notice(new[]{"","RUPTURA","ECLIPSE","ABSOLUTO"}[Form]);Sound(3);return true;
        }
        void BeginFormPresentation(int form,bool entering)
        {
            presentationForm=form;enteringForm=entering;formReleased=false;
            formPresentationTime=formPresentationDuration=entering?.65f:.35f;
            lab.Player.Model.SuspendInput();
            DarklightCombatFX.Transformation(lab.Player.transform.position,form,false);
        }
        void TickFormPresentation(float dt)
        {
            if(!Transforming||dt<=0)return;
            if(health.IsDead){ClearFormPresentation();return;}
            formPresentationTime=Mathf.Max(0,formPresentationTime-dt);
            if(!formReleased&&TransformationProgress>=.45f)
            {
                formReleased=true;DarklightCombatFX.Transformation(lab.Player.transform.position,presentationForm,true);
            }
        }
        void ClearFormPresentation(){formPresentationTime=0;presentationForm=0;formReleased=false;}
    }
}
