using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        float formPresentationTime,formPresentationDuration;
        public const float TransformationEnterDuration=4.6f,TransformationExitDuration=.55f;
        public const float RupturaEnterDuration=1.25f;
        public static float TransformationEntryDuration(int form)=>form==1?RupturaEnterDuration:TransformationEnterDuration;
        public const float MinimumTransformationDuration=180f;
        public static float TransformationDuration(int mastery)=>MinimumTransformationDuration+Mathf.Max(0,mastery)*2;
        int presentationForm;bool enteringForm,formReleased,officialFormPresentation;
        public bool OfficialTransformationPresentation=>Transforming&&officialFormPresentation;
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
            Form=Progress.preferredForm>0?Mathf.Min(unlocked,DarklightPlayableScope.Form(Progress.preferredForm)):unlocked;
            Energy-=60;FormTime=TransformationDuration(Progress.mastery);BeginFormPresentation(Form,true);
            Notice(DarklightPlayableScope.FormName(Form));Sound(3);return true;
        }
        void BeginFormPresentation(int form,bool entering)
        {
            presentationForm=form;enteringForm=entering;formReleased=false;
            formPresentationTime=formPresentationDuration=entering?TransformationEntryDuration(form):TransformationExitDuration;
            officialFormPresentation=entering&&Weapons?.Equipped!=null&&(int)Weapons.Equipped.id==0&&DarklightOfficialTransitionBank.TryPose(form,0,out _);
            lab.Player.Model.SuspendInput();
            if(!officialFormPresentation)DarklightCombatFX.Transformation(lab.Player.transform.position,form,false,DarklightFormController.ColorRuptura);
        }
        void TickFormPresentation(float dt)
        {
            if(!Transforming||dt<=0)return;
            if(health.IsDead){EndForm(false);return;}
            formPresentationTime=Mathf.Max(0,formPresentationTime-dt);
            if(!formReleased&&TransformationProgress>=.52f)
            {
                formReleased=true;if(!officialFormPresentation)DarklightCombatFX.Transformation(lab.Player.transform.position,presentationForm,true,DarklightFormController.ColorRuptura);
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
        void ClearFormPresentation()
        {
            formPresentationTime=0;presentationForm=0;formReleased=false;officialFormPresentation=false;
            if(lab!=null&&lab.Player!=null)
                lab.Player.GetComponent<DarklightTransformationPresentation>()?.ResetPresentation();
        }
        void OnDisable()
        {
            ClearFormPresentation();Form=0;FormTime=0;
            if(lab!=null&&lab.Player!=null)
            {
                lab.Player.Model.SuspendInput();
                lab.Player.GetComponent<DarklightOriginalVampireView>()?.ClearEffects();
            }
        }
    }
}

