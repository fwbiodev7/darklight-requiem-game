using System;
using System.Collections.Generic;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public const int AncestralMemory=1,SurvivorsSaved=2,KaelPurified=4,WeaponsMastered=8;
        public bool ExpandedChapter=>!ExploringNewRegion&&Progress.chapter>=6;
        readonly Dictionary<RequiemEnemyKind,Sprite[]> expandedBossSheets=new();
        Sprite[] ExpandedBossFramesFor(RequiemEnemyKind kind)
        {
            if(expandedBossSheets.TryGetValue(kind,out var cached))return cached;
            string sheet=kind switch
            {
                RequiemEnemyKind.Valtherion=>"Valtherion",
                RequiemEnemyKind.Seraphine=>"Seraphine",
                RequiemEnemyKind.KaelFractured=>"KaelFractured",
                RequiemEnemyKind.Nocthar=>"Nocthar",
                _=>throw new ArgumentOutOfRangeException(nameof(kind),kind,"Expected an expansion boss")
            };
            var frames=DarklightSecondaryStyleBank.Load(sheet);
            if(frames.Length!=8)throw new InvalidOperationException("Secondary boss sheet incomplete: "+sheet);
            expandedBossSheets.Add(kind,frames);
            return frames;
        }
        public string AllyName=>Progress.chapter==1?"ÍCARO NOX":NeroTruceActive?"NERO":"LADY";
        public bool SecretEndingAvailable=>Progress.SecretEndingAvailable;
        static readonly string[][] MissionTitles={
            new[]{"DECIFRE AS INSCRIÇÕES","RECUPERE A MEMÓRIA ANCESTRAL","LIBERTE OS GUARDIÕES","LIBERTE O SALÃO DOS JURAMENTOS"},
            new[]{"RESGATE OS SOBREVIVENTES","DEFENDA O ABRIGO","INVADE A CATEDRAL","ABRA A PASSAGEM DA CATEDRAL"},
            new[]{"RECUPERE O SELO DA MEMÓRIA","RECUPERE O SELO DO SANGUE","RECUPERE O SELO DO VÉU","QUEBRE A CORRUPÇÃO DE NERO"},
            new[]{"INVADE O CORAÇÃO DO ECLIPSE","REVISITE A ÚLTIMA MEMÓRIA","DOMINE KATANA E PUNHOS","FECHE AS ÚLTIMAS FENDAS"}
        };
        static readonly string[][] MissionDescriptions={
            new[]{"Os registros do observatório levam à cripta. Aproxime-se da inscrição e interaja para descobrir a origem da linhagem.","Recupere a memória após enfrentar os guardiões. Interaja com o vestígio para ouvir o ancestral.","Os antigos guardiões foram corrompidos. Liberte a passagem para o salão dos juramentos.","Libere o salão dos juramentos enfrentando os guardiões corrompidos e abra a passagem para o abrigo."},
            new[]{"Proteja o grupo encurralado derrotando a patrulha. Interaja com os sobreviventes para conduzi-los ao abrigo.","Defenda o abrigo em três ondas. O checkpoint registra cada onda concluída.","Abra caminho pela fortaleza para interromper a origem do eclipse permanente.","Atravesse a última patrulha da catedral e libere a passagem para o Véu Profundo."},
            new[]{"Enfrente as criaturas da fortaleza e recolha o primeiro selo antigo.","O selo do sangue guarda uma versão distorcida dos caçadores de Vespera.","O último selo revela a origem da corrupção. Recupere-o para alcançar Nero.","Enfraqueça a corrupção até Nero ajoelhar. Aproxime-se e interaja para purificá-lo. Nero deve sobreviver."},
            new[]{"Atravesse os últimos guardiões do Coração do Eclipse.","Leia a memória de Lady para reunir as energias humana e ancestral.","Use katana e Punhos da Ruína contra os guardiões. Cada arma deve acertar um alvo durante este desafio.","Feche as últimas fendas enfrentando os grupos de criaturas. A passagem leva ao duelo final contra Lucius."}
        };
        string MissionTitle=>ExpandedChapter?MissionTitles[Progress.chapter-6][Mathf.Clamp(encounter,0,3)]:Goal();
        string MissionDescription=>ExpandedChapter?MissionDescriptions[Progress.chapter-6][Mathf.Clamp(encounter,0,3)]:"Liberte o selo marcado e elimine seus guardiões.";
        bool NarrativeInteractionStage=>ExpandedChapter&&((Progress.chapter==6&&encounter<=1)||(Progress.chapter==7&&encounter==0)||(Progress.chapter==9&&encounter==1));
        bool NarrativeCombatDone,completingNarrativeInteraction;
        void PrepareNarrativeMap()
        {
            NarrativeCombatDone=false;
            if(!ExpandedChapter)return;
            if(Progress.checkpoint<4&&NarrativeInteractionStage)AddProp("VestigioNarrativo",extras[5],Map.Arenas[encounter],.7f);
            if(Progress.chapter==7&&Progress.checkpoint<=1)AddProp("Sobreviventes",lyraFrames?.Length==8?lyraFrames[0]:extras[1],Map.Arenas[0]+Vector2.left*1.6f,1.8f);
        }
        bool BeginNarrativeEncounter()
        {
            if(!ExpandedChapter||encounter==3)return false;
            if(NarrativeCombatDone)return true;
            if(Progress.chapter==6&&encounter==0||Progress.chapter==9&&encounter==1){Objective=MissionTitle;return true;}
            encounterStarted=true;Objective=MissionTitle;
            int count=Progress.chapter==7&&encounter==1?3+Progress.missionWave:4;
            if(Progress.chapter==9&&encounter==2)count=6;
            for(int i=0;i<count;i++)
            {
                var kind=(RequiemEnemyKind)((i+encounter+Progress.chapter)%5);
                Spawn(kind,Map.Arenas[encounter]+new Vector2((i%3-1)*1.3f,i<3?1.4f:-1.4f),i==0&&encounter==2);
            }
            Notice(Progress.chapter==7&&encounter==1?"ABRIGO — ONDA "+(Progress.missionWave+1)+" / 3":MissionTitle);
            return true;
        }
        bool FinishNarrativeCombat()
        {
            if(!ExpandedChapter||encounter==3)return false;
            if(Progress.chapter==7&&encounter==1&&Progress.missionWave<2)
            {
                Progress.missionWave++;encounterStarted=false;SaveCheckpoint();BeginNarrativeEncounter();return true;
            }
            if(NarrativeInteractionStage&&!completingNarrativeInteraction)
            {encounterStarted=false;NarrativeCombatDone=true;Objective=MissionTitle+" — INTERAJA";return true;}
            if(Progress.chapter==9&&encounter==2&&(Progress.missionWeaponHits&DarklightPlayableScope.WeaponMask)!=DarklightPlayableScope.WeaponMask)
            {
                encounterStarted=false;Notice("USE KATANA E PUNHOS — "+WeaponTrialCount+" / 2");BeginNarrativeEncounter();return true;
            }
            if(Progress.chapter==9&&encounter==2)Progress.expansionFlags|=WeaponsMastered;
            return false;
        }
        public int WeaponTrialCount=>((Progress.missionWeaponHits&1)!=0?1:0)+((Progress.missionWeaponHits&4)!=0?1:0);
        void RecordWeaponTrial()
        {if(ExpandedChapter&&Progress.chapter==9&&encounter==2)Progress.missionWeaponHits|=1<<(int)Weapons.Equipped.id;}
        bool NarrativeTarget(out DarklightInteractionTarget target)
        {
            target=default;if(!ExpandedChapter)return false;
            var boss=CurrentBoss;
            if(boss!=null&&boss.kind==RequiemEnemyKind.KaelFractured&&boss.PurificationReady)
            {target=new("purify_kael","PURIFICAR NERO",boss.transform.position);return true;}
            if(!NarrativeInteractionStage||chapterCleared||encounterStarted)return false;
            if(Progress.chapter==6&&encounter==1||Progress.chapter==7&&encounter==0)
                if(!NarrativeCombatDone)return false;
            target=new("narrative",Progress.chapter==7?"CONDUZIR AO ABRIGO":Progress.chapter==6&&encounter==0?"DECIFRAR INSCRIÇÃO":"OUVIR MEMÓRIA",InteractionAnchor(Map.Arenas[encounter]));return true;
        }
        bool InteractNarrative(string id)
        {
            if(id=="purify_kael")
            {
                var boss=CurrentBoss;if(boss==null||!boss.PurificationReady)return true;
                boss.Purify();Progress.expansionFlags|=KaelPurified;
                Progress.RecordNeroSurvival();
                // Nero departs to recover; no friendly prop remains in the cleared arena.
                boss.gameObject.SetActive(false);
                ClearEncounter();return true;
            }
            if(id!="narrative")return false;
            int chapter=Progress.chapter,step=encounter;
            string[] lines=chapter==6&&step==0?new[]{"LADY|Essas inscrições são anteriores ao Projeto Eclipse. Os híbridos protegiam a fronteira entre os mundos.","FABIUS|Meu sangue não é um acidente. Vamos descobrir o que eles deixaram para nós."}:chapter==6?new[]{"ANCESTRAL|O Véu não precisa de um senhor. Precisa de alguém que ainda saiba escolher.","FABIUS|Eu lembro dessa voz. Minha mãe protegia essa mesma ponte."}:chapter==7?new[]{"SOBREVIVENTE|A praça caiu. Há crianças no abrigo e criaturas vindo da catedral.","LADY|Eu conduzo os sobreviventes. Fabius, proteja a entrada até todos chegarem."}:new[]{"LADY|Você nunca esteve sozinho. Tudo o que perdeu continua nas escolhas que faz.","FABIUS|Vou levar essas memórias comigo. Agora posso encarar o eclipse inteiro."};
            if(chapter==6&&step==1)Progress.expansionFlags|=AncestralMemory;
            if(chapter==7&&step==0)Progress.expansionFlags|=SurvivorsSaved;
            // Complete the checkpoint before the dialogue: reloading cannot repeat its reward.
            encounterStarted=false;NarrativeCombatDone=false;completingNarrativeInteraction=true;
            try{ClearEncounter();}finally{completingNarrativeInteraction=false;}
            var vestige=levelRoot.Find("VestigioNarrativo");if(vestige!=null)Destroy(vestige.gameObject);
            if(NarrativeInteractionStage)AddProp("VestigioNarrativo",extras[5],Map.Arenas[encounter],.7f);
            Speak(lines);return true;
        }
        void ExpandedChapterReward()
        {
            if(Progress.chapter==6)Progress.CompleteMilestone(6,2);
            if(Progress.chapter==7)Progress.CompleteMilestone(7,3);
            if(Progress.chapter==9)Progress.CompleteMilestone(9,3);
        }
        public void ResumeLegacyExpansion()
        {
            if(!Progress.completed||!Progress.legacyFinaleCompleted||Progress.chapter>5)return;
            StopMenuMusic();TravelToChapter(6);
        }
        void BuildEndings()
        {
            heading.text="O DESTINO DOS DOIS MUNDOS";
            bodyText.text="O duelo com Lucius terminou. A trégua com Nero chegou ao fim. Lady espera sua escolha.\n\nAmanhecer restaura a barreira.\nGuardião preserva a ponte sob sua proteção.\n\n"+(SecretEndingAvailable?"As memórias permitem uma terceira solução.":"Réquiem Carmesim: reúna as dez memórias, salve os sobreviventes, ouça o ancestral, purifique Nero e domine katana e punhos.");
            Button("AMANHECER",()=>Finish(1));Button("GUARDIÃO DO VÉU",()=>Finish(2));
            if(SecretEndingAvailable)Button("RÉQUIEM CARMESIM",()=>Finish(3));
        }
        string EndingDescription=>Progress.ending==3?"Fabius uniu as energias sem apagar nenhum dos mundos.\nLady permanece ao seu lado. Nero partiu: a rivalidade ainda não terminou.":Progress.ending==1?"O sol voltou a Vespera. A barreira foi restaurada.\nFabius guardou Réquiem para quem vier depois. Nero ainda exige sua revanche.":"Vespera voltou a respirar. A ponte permaneceu.\nFabius e Lady guardam o caminho entre os mundos. Nero não esqueceu sua promessa.";
        public void FinalAwakening()
        {
            Progress.formsUnlocked=Mathf.Max(Progress.formsUnlocked,3);Progress.CompleteMilestone(9,3);
            Blood.Gain(65);Notice("ECLIPSE ABSOLUTO DESPERTO — "+lab.Input.BindingLabel("Transform")+": TRANSFORMAR");SaveCheckpoint();
        }
        internal void SpawnBossSupport(Vector2 origin)
        {
            int live=0;foreach(var e in enemies)if(e!=null&&!e.Boss&&!e.Health.IsDead)live++;
            if(live>=4)return;
            for(int i=0;i<Mathf.Min(2,4-live);i++)Spawn(RequiemEnemyKind.Spectral,origin+new Vector2(i==0?-2:2,-1),false);
        }
    }
}



