using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightAbyssRider
    {
        public int Learned{get;private set;}int lessonSerial=-1;bool rodeBefore;float lessonDistance;
        public void ResetLessons(){Learned=0;lessonSerial=-1;rodeBefore=false;lessonDistance=0;}
        public void NoteMovement(float distance){if(!Active||distance<=0||distance>.8f)return;lessonDistance+=distance;if(lessonDistance>=2)Learned|=1;}
        void NoteLessons()
        {
            if(!Active)return;var m=fighter.Model;
            if(Gear.Level>=2)Learned|=32;if(Gear.Level>=3)Learned|=64;
            if(DashDistance>.4f){Learned|=4;rodeBefore=true;}if(DriftDistance>.25f)Learned|=8;
            if(m.AttackConnected&&lessonSerial!=m.AttackSerial)
            {
                lessonSerial=m.AttackSerial;
                if(!Dashing&&!Slamming){Learned|=2;if(rodeBefore)Learned|=16;}
                if(Finishing)Learned|=128;if(Sustaining)Learned|=256;if(Slamming)Learned|=512;
            }
        }
        public string LessonText(DarklightInput input)
        {
            string[] tasks={"CAMINHE 2 METROS","ACERTE COM AS SERRAS","MONTE E AVANCE COM A MOTO","DERRAPE DURANTE A INVESTIDA","APÓS SALTAR, CONTINUE O COMBO","ALCANCE A ENGRENAGEM II","ALCANCE A ENGRENAGEM III","FINALIZE UM ALVO EXPOSTO","SEGURE AS SERRAS APÓS ACERTAR","ACERTE O ESMAGAMENTO"};
            string saw=input.BindingLabel("AbyssSaw"),bike=input.BindingLabel("AbyssBike"),drift=input.BindingLabel("AbyssDrift");
            string[] hints={"Use o movimento configurado; as pernas seguem o chão.",saw+": toque para atacar.",bike+": montar e investir.",drift+": pressione enquanto pilota.",saw+": toque durante a aterrissagem.","Acerte e toque "+saw+" quando ENGATE! brilhar.","Mais dois engates precisos. Evite apertar sem ritmo.",bike+": use quando III mostrar FINALIZAR.","Segure "+saw+" após um acerto; solte para parar.",input.BindingLabel("Skill")+": golpe pesado com a moto."};
            for(int i=0;i<10;i++)if((Learned&(1<<i))==0)return "CAVALEIRO · ETAPA "+(i+1)+"/10\n"+tasks[i]+"\n"+hints[i];
            return "CAVALEIRO · 10/10 ETAPAS CONCLUÍDAS\nCombine serras, moto e engates.\nPAUSA > ARMAS: REVER COMANDOS";
        }
    }
}
