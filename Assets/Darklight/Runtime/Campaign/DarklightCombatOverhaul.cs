using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public DarklightStyleMeter Style {get;}=new();
        readonly DarklightThreatDirector threats=new();
        DarklightCombatProfiles combatProfiles;
        int newGameDifficulty;
        bool difficultyForNewGame;
        float nextFinisher;
        public int Difficulty=>Mathf.Clamp(Progress.difficulty,0,2);
        public DarklightDifficultyProfile DifficultyProfile=>DarklightDifficultyProfile.For(Difficulty);
        public string DifficultyName=>new[]{"NORMAL","DIFICIL","EXTREMA"}[Difficulty];
        void InitializeTechnicalCombat()
        {combatProfiles=new DarklightCombatProfiles();weaponDefinitions=combatProfiles.Create(weaponDefinitions);}
        bool TechnicalAttackPermit(DarklightEnemy enemy)
        {
            var view=lab.WorldCamera.WorldToViewportPoint(enemy.transform.position);
            bool visible=view.z>0&&view.x>.03f&&view.x<.97f&&view.y>.04f&&view.y<.96f;
            bool ranged=enemy.kind==RequiemEnemyKind.Shooter||enemy.kind==RequiemEnemyKind.Spectral||enemy.kind==RequiemEnemyKind.Cantor||enemy.kind==RequiemEnemyKind.Lucien;
            return threats.Request(enemy,lab.Player.Model.Clock,Difficulty,ranged,visible);
        }
        void BuildCombatOptions()
        {
            heading.text="COMBATE E TREINO";bodyText.text="Dificuldade: "+DifficultyName+"\n\nTreine combos, armas e formas sem alterar o progresso da campanha.\n\nL-L-L: sequencia\nL, pausa, L: variacao\nL-L-Pesado: final\nAereo: suspensao curta\nTroque arma apos confirmar o acerto.";
            Button("DIFICULDADE",()=>{difficultyForNewGame=false;Open("Difficulty");});
            Button("COMBATE AÉREO · GUIA",()=>Open("AirCombat"));
            Button(Training?"FERRAMENTAS DA ARENA":"ENTRAR NA ARENA",()=>{if(Training)Open("Training");else EnterTraining();});
            Button("VOLTAR",()=>Open("Pause"));
        }
        void BuildDifficulty()
        {
            heading.text="DIFICULDADE";bool front=difficultyForNewGame;
            bodyText.text="Normal: leitura e ataques isolados.\nDificil: flanqueamento, defesa e sequencias duplas.\nExtrema: pressao coordenada e sequencias de tres golpes.\n\nExtrema desbloqueia ao concluir a campanha. A dificuldade altera comportamento, nao apenas atributos.";
            var unlocked=Progress;if(front)unlocked=DarklightProgress.Load();
            for(int i=0;i<3;i++){int mode=i;Button(new[]{"NORMAL","DIFICIL","EXTREMA"}[i],()=>{
                if(front){newGameDifficulty=mode;if(DarklightProgress.Load()!=null)Open("ConfirmNew");else NewGame();}
                else {Progress.difficulty=mode;threats.Clear();if(!Training)SaveCheckpoint();Open(Training?"Training":"CombatOptions");}
            },i<2||DarklightDifficultyProfile.ExtremeUnlocked(unlocked));}
            Button("VOLTAR",()=>Open(front?"Main":Training?"Training":"CombatOptions"));
        }
        void TechnicalStrike(StrikeKind kind)
        {
            var attack=lab.Player.Model.Attack;int weapon=(int)(Weapons?.Equipped?.id??DarklightWeaponId.Katana);
            int move=attack==null?(int)kind:attack.GetEntityId().GetHashCode()^(Form*7919);
            Style.Hit(move,weapon,kind==StrikeKind.Aerial,attack!=null&&attack.finisher);style=Style.Points;
            if(!Training)StageStats.Hit(Style,kind==StrikeKind.WeaponSpecial||kind==StrikeKind.Skill||kind==StrikeKind.Aerial);
        }
        public void TechnicalFinisher(Vector3 target)
        {
            if(lab.Player.Model.Clock<nextFinisher)return;nextFinisher=lab.Player.Model.Clock+.5f;
            slowTime=Mathf.Max(slowTime,.22f);lab.Hitstop(.055f);
            lab.WorldCamera.GetComponent<DarklightLabCamera>().Impulse(ReducedFlashes?1:2);
            DarklightCombatFX.Echo(lab.Player.GetComponentInChildren<SpriteRenderer>(),.12f);
            Notice("EXECUCAO · "+Style.Rank);
        }
    }
}
