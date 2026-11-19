using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightEnemy
    {
        public const float LuciusTransformationDuration=4.2f;
        static readonly Color ArconteEnergy=new Color(.24f,1f,.38f,1);
        bool luciusFormReleased,luciusOfficialTransition;
        DarklightTransformationPresentation luciusLift;
        DarklightTransformationBurst luciusBurst;
        DarklightSmallTransformationAura luciusAura;
        public bool LuciusTransforming=>luciusTransformationTime>0;
        void PresentLuciusTransition(float progress)
        {
            var renderer=LuciusView?.Renderer;if(renderer==null)return;
            if(luciusOfficialTransition&&DarklightOfficialTransitionBank.TryPose(3,progress,out var approved))
            {
                renderer.sprite=approved;DarklightOfficialTransitionBank.ApplyOwnership(renderer);
                luciusLift?.ResetPresentation();luciusBurst?.Hide();luciusAura?.Hide();
                return;
            }
            luciusLift??=GetComponent<DarklightTransformationPresentation>()??gameObject.AddComponent<DarklightTransformationPresentation>();
            luciusLift.Present(renderer,progress,LuciusTransforming);
            if(luciusBurst==null)
            {
                var child=new GameObject("ArconteAwakening");child.transform.SetParent(renderer.transform,false);
                luciusBurst=child.AddComponent<DarklightTransformationBurst>();
            }
            luciusBurst.Present(renderer,3,progress,true,LuciusTransforming,ArconteEnergy);
            PresentLuciusAura();
        }
        void PresentLuciusAura()
        {
            var renderer=LuciusView?.Renderer;if(renderer==null)return;
            if(!phaseTwo||luciusOfficialTransition&&LuciusTransforming||DarklightOfficialTransitionBank.IsAuthored(renderer.sprite)){luciusAura?.Hide();return;}
            if(luciusAura==null)
            {
                var child=new GameObject("ArconteAura");child.transform.SetParent(renderer.transform,false);
                luciusAura=child.AddComponent<DarklightSmallTransformationAura>();
            }
            luciusAura.Present(renderer,3,clock,Health==null||Health.IsDead||Resolved,ArconteEnergy);
        }
        public void CancelLuciusTransformation()
        {
            luciusTransformationTime=0;luciusOfficialTransition=false;
            var renderer=LuciusView!=null?LuciusView.Renderer:null;
            if(renderer!=null&&DarklightOfficialTransitionBank.IsAuthored(renderer.sprite))
                LuciusView.PresentPose("Idle",0,0,null,false);
            luciusLift?.ResetPresentation();luciusBurst?.Hide();luciusAura?.Hide();
        }
        void OnDisable()
        {
            warning?.Hide();
            CancelLuciusTransformation();
        }
    }
}
