using UnityEngine;
namespace Darklight
{
    // Four quiet walking phases based exclusively on RockWalkKatanaYoung.
    // No sprint poses or speed-triggered pose changes.
    public sealed class DarklightAbyssLocomotion:MonoBehaviour
    {
        readonly Sprite[][] pages=new Sprite[12][];
        Rigidbody2D body;Vector2 previous;float phase,travelled,lastMoved=-100;int sampled=-1,lastForm=-1;bool initialized;
        public float ActualSpeed{get;private set;}
        public bool Moving{get;private set;}
        public float Travelled=>travelled;
        public int Pose{get;private set;}
        public bool Mirror{get;private set;}
        public int View{get;private set;}
        void Awake(){body=GetComponent<Rigidbody2D>();}
        Sprite[] Page(int form,int group)
        {
            int slot=form*3+group;if(pages[slot]!=null)return pages[slot];var result=new Sprite[8];
            foreach(var sprite in Resources.LoadAll<Sprite>("Darklight/AbyssPrototype/CalmWalkF"+form+"G"+group))
            {
                string[] parts=sprite.name.Split('_');int n=parts.Length;
                if(n>=3&&int.TryParse(parts[n-2],out int row)&&int.TryParse(parts[n-1],out int step)&&row>=0&&row<2&&step>=0&&step<4)result[row*4+step]=sprite;
            }
            pages[slot]=result;return result;
        }
        public bool HasCompleteForm(int form){form=Mathf.Clamp(form,0,3);for(int d=0;d<5;d++)for(int f=0;f<4;f++)if(Page(form,d/2)[(d%2)*4+f]==null)return false;return true;}
        public bool HasCompleteNormal()=>HasCompleteForm(0);
        public void PreloadNormal(){for(int g=0;g<3;g++)Page(0,g);}
        public void ClearCache(){for(int i=0;i<pages.Length;i++)pages[i]=null;}
        public Sprite Present(CombatModel model,Vector2 intended,int form)
        {
            form=Mathf.Clamp(form,0,3);
            if(lastForm!=form){lastForm=form;sampled=-1;phase=0;lastMoved=-100;initialized=false;}
            if(body==null)return null;Vector2 position=body.position;
            if(sampled!=Time.frameCount)
            {
                sampled=Time.frameCount;float delta=DarklightCombatLab.Instance?.CombatDelta??0;
                float distance=initialized?Vector2.Distance(position,previous):0;initialized=true;previous=position;
                if(distance>.8f){distance=0;phase=0;}
                if(distance>.0005f)lastMoved=model.Clock;
                ActualSpeed=delta>0?body.linearVelocity.magnitude:0;
                Moving=delta>0&&model.State==CombatState.Neutral&&intended.sqrMagnitude>.0025f&&model.Clock-lastMoved<.08f;
                if(Moving)
                {
                    travelled+=distance;DarklightCampaign.Instance?.AbyssRider?.NoteMovement(distance);
                    // Same quiet cadence as katana: at most ten pose changes/s.
                    phase=Mathf.Repeat(phase+Mathf.Min(distance/1.8f,delta*2.5f),1);
                }
            }
            int direction=DarklightCharacterView.DirectionIndex(model.Facing);Mirror=direction>4;View=Mirror?8-direction:direction;
            Pose=Moving?Mathf.Min(3,Mathf.FloorToInt(phase*4)):1;
            return model.State==CombatState.Neutral?Page(form,View/2)[(View%2)*4+Pose]:null;
        }
    }
}
