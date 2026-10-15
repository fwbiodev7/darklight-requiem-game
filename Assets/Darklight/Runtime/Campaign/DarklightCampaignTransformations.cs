using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        float formPresentationTime,formPresentationDuration;
        public const float TransformationEnterDuration=1.8f,TransformationExitDuration=.55f;
        public const float MinimumTransformationDuration=180f;
        public static float TransformationDuration(int mastery)=>MinimumTransformationDuration+Mathf.Max(0,mastery)*2;
        int presentationForm;bool enteringForm,formReleased;
        public bool Transforming=>formPresentationTime>0;
        public float TransformationProgress=>Transforming?1-formPresentationTime/formPresentationDuration:1;
        public int TransformationForm=>presentationForm;
        public bool EnteringTransformation=>enteringForm;
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
            Energy-=60;FormTime=TransformationDuration(Progress.mastery);BeginFormPresentation(Form,true);
            Notice(new[]{"","RUPTURA","ECLIPSE","ABSOLUTO"}[Form]);Sound(3);return true;
        }
        void BeginFormPresentation(int form,bool entering)
        {
            presentationForm=form;enteringForm=entering;formReleased=false;
            formPresentationTime=formPresentationDuration=entering?TransformationEnterDuration:TransformationExitDuration;
            lab.Player.Model.SuspendInput();
            DarklightCombatFX.Transformation(lab.Player.transform.position,form,false);
        }
        void TickFormPresentation(float dt)
        {
            if(!Transforming||dt<=0)return;
            if(health.IsDead){ClearFormPresentation();return;}
            formPresentationTime=Mathf.Max(0,formPresentationTime-dt);
            if(!formReleased&&TransformationProgress>=.52f)
            {
                formReleased=true;DarklightCombatFX.Transformation(lab.Player.transform.position,presentationForm,true);
            }
        }
        void TickFormState(float dt)
        {
            if(dt<=0)return;
            // The entry cinematic does not consume the player's three minutes of control.
            float activeDelta=Transforming&&enteringForm?Mathf.Max(0,dt-formPresentationTime):dt;
            TickFormPresentation(dt);
            if(Form>0){FormTime=Mathf.Max(0,FormTime-activeDelta);if(FormTime<=0)EndForm();}
        }
        void ClearFormPresentation(){formPresentationTime=0;presentationForm=0;formReleased=false;}
    }
}
