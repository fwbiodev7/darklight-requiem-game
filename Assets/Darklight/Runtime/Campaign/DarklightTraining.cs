using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public bool Training {get;private set;}
        public bool TrainingPassive {get;private set;}=true;
        public bool TrainingDamage {get;private set;}
        public bool TrainingHitboxes {get;private set;}
        DarklightProgress suspendedProgress;
        Vector2 suspendedPosition;float suspendedHealth;
        bool trainingFromMain;
        UnityEngine.UI.Text trainingReadout;
        void TickTechnicalHUD(){UpdateAbyssHUD();if(trainingReadout!=null)trainingReadout.gameObject.SetActive(Training&&Active);}
        public void EnterTraining()
        {
            if(Training){Open("Training");return;}
            trainingFromMain=panel=="Main";suspendedProgress=Progress;
            suspendedPosition=lab.Player.transform.position;suspendedHealth=health.CurrentHealth;
            StopPower();Training=true;
            var practice=trainingFromMain?DarklightProgress.Load()??suspendedProgress:suspendedProgress;
            Progress=JsonUtility.FromJson<DarklightProgress>(JsonUtility.ToJson(practice));
            Progress.weaponUnlocks=DarklightPlayableScope.WeaponMask;Progress.checkpoint=0;Progress.chapter=0;Progress.completed=false;
            LoadMap(0);levelRoot.gameObject.SetActive(false);Destroy(levelRoot.gameObject);
            levelRoot=new GameObject("ArenaDeTreinamento").transform;Map=DarklightMapDefinition.TrainingArena();
            DarklightTrainingFloor.Build(levelRoot,Map,floorTiles[0],pixelMaterial);
            Physics2D.SyncTransforms();var start=Map.SafeSpawn(Map.Spawn);
            lab.Player.transform.position=start;lab.Player.GetComponent<Rigidbody2D>().position=start;
            var camera=lab.WorldCamera.GetComponent<DarklightLabCamera>();camera.bounds=Map.Bounds;
            lab.WorldCamera.transform.position=new Vector3(start.x,start.y+camera.followOffset.y,-10);
            health.FullHeal();Blood.Set(100);Style.Reset();threats.Clear();encounterStarted=false;chapterCleared=false;
            if(trainingReadout==null){trainingReadout=TextAt(uiRoot,"TreinoCombos",204,6,218,38,5);trainingReadout.alignment=TextAnchor.UpperCenter;}
            trainingReadout.gameObject.SetActive(true);SpawnTraining(0);Open("Training");
        }
        public void ExitTraining()
        {
            if(!Training)return;
            StopPower();Training=false;TrainingHitboxes=false;
            Progress=suspendedProgress??new DarklightProgress();suspendedProgress=null;
            LoadMap(Progress.chapter);
            var safe=Map.SafeSpawn(suspendedPosition);lab.Player.transform.position=safe;lab.Player.GetComponent<Rigidbody2D>().position=safe;
            if(suspendedHealth<health.CurrentHealth)health.TakeDamage(health.CurrentHealth-suspendedHealth);
            if(trainingReadout!=null)trainingReadout.gameObject.SetActive(false);
            if(trainingFromMain)Open("Main");else Close();
        }
        void TickTraining(float dt)
        {
            Blood.Set(100);if(!TrainingDamage&&health.HealthPercent<1)health.FullHeal();
            if(Form>0)FormTime=Mathf.Max(FormTime,180);
            if(trainingReadout!=null)trainingReadout.text="TREINO · "+DifficultyName+"\nCOMBO "+lab.Player.Model.ComboCount+" · "+Style.Rank+"\nPAUSA: ALVOS / ARMAS / FORMAS";
            if(TrainingDamage&&health.IsDead){StopPower();health.SetMaxHealth(health.MaxHealth,false);lab.Player.ResetModel();lab.Player.GetComponent<Rigidbody2D>().position=Map.Spawn;Notice("TREINO: VIDA RESTAURADA");}
        }
        void SpawnTraining(int type)
        {
            foreach(var enemy in enemies)if(enemy!=null){enemy.gameObject.SetActive(false);Destroy(enemy.gameObject);}enemies.Clear();threats.Clear();
            TrainingPassive=type==0;
            var origin=Map.Spawn+Vector2.up*2.7f;
            var kinds=type==0?new[]{RequiemEnemyKind.Hunter}:type==1?new[]{RequiemEnemyKind.Hunter}:type==2?new[]{RequiemEnemyKind.Armored}:type==3?new[]{RequiemEnemyKind.Shooter}:type==4?new[]{RequiemEnemyKind.Hunter,RequiemEnemyKind.Armored,RequiemEnemyKind.Shooter}:new[]{RequiemEnemyKind.Kael};
            for(int i=0;i<kinds.Length;i++){Spawn(kinds[i],origin+Vector2.right*(i-(kinds.Length-1)*.5f)*2.3f,type==4&&i==0);if(TrainingPassive)enemies[enemies.Count-1].Health.SetMaxHealth(10000,false);}
            Close();
        }
        void SetTrainingForm(int form)
        {
            EndForm(false);lab.Player.ResetModel();
            form=DarklightPlayableScope.Form(form);if(form>0){Progress.preferredForm=form;Form=form;FormTime=180;BeginFormPresentation(form,true);}
            Close();
        }
        void BuildTraining()
        {
            heading.text="ARENA DE TREINAMENTO";
            bodyText.text="O progresso real fica preservado.\nSangue e tempo de forma ilimitados.\n\nLeve repetido: sequencia\nLeve, pausa, leve: variacao\nDois leves + pesado: final\nTroca de arma apos acerto\nAereo: suspender alvo leve\n\nAlvo atual: "+(TrainingPassive?"PASSIVO":"ATIVO");
            Button("RETOMAR TREINO",Close);Button("ESCOLHER INIMIGOS",()=>Open("TrainingEnemies"));
            Button("ARMA: "+Weapons.Equipped.displayName,()=>{Weapons.RequestNext();Close();});
            Button("TESTAR CAVALEIRO · REQUIEM",StartAbyssPrototype);Button("GUIA DO CAVALEIRO",ShowAbyssHelp);Button("DESAFIOS DO CAVALEIRO",StartAbyssLessons);
            Button("ESCOLHER TRANSFORMACAO",()=>Open("TrainingForms"));
            Button("RECEBER DANO: "+(TrainingDamage?"SIM":"NAO"),()=>{TrainingDamage=!TrainingDamage;RefreshPanel();});
            Button("VER HITBOXES: "+(TrainingHitboxes?"SIM":"NAO"),()=>{TrainingHitboxes=!TrainingHitboxes;RefreshPanel();});
            Button("DIFICULDADE: "+DifficultyName,()=>{difficultyForNewGame=false;Open("Difficulty");});Button("SAIR DA ARENA",ExitTraining);
        }
        void BuildTrainingEnemies()
        {
            heading.text="ADVERSARIOS DO TREINO";bodyText.text="Alvo passivo para sequencias; inimigos ativos para praticar defesa, postura e pressao. Escolher outro encontro limpa o anterior.";
            string[] labels={"ALVO PASSIVO","CACADOR","BLINDADO","ATIRADOR","GRUPO MISTO + ELITE","KAEL"};
            for(int i=0;i<labels.Length;i++){int chosen=i;Button(labels[i],()=>SpawnTraining(chosen));}Button("VOLTAR",()=>Open("Training"));
        }
        void BuildTrainingForms()
        {
            heading.text="FORMAS NO TREINO";bodyText.text="Teste Réquiem e suas armas com cada forma. A troca conserva as preferencias reais fora desta sessao.";
            string[] labels={"NORMAL","RUPTURA","ECLIPSE ABSOLUTO"};
            for(int i=0;i<labels.Length;i++){int chosen=i;Button(labels[i],()=>SetTrainingForm(chosen));}
            Button("VOLTAR",()=>Open("Training"));
        }
    }
    public static class DarklightTrainingFloor
    {
        public static void Build(Transform root,DarklightMapDefinition map,Sprite floor,Material material)
        {
            var grid=map.GridBounds;
            for(int y=grid.yMin;y<grid.yMax;y++)for(int x=grid.xMin;x<grid.xMax;x++)
            {
                if(!map.Walkable(new Vector2Int(x,y)))continue;
                var renderer=new GameObject("PisoTreino").AddComponent<SpriteRenderer>();renderer.transform.SetParent(root,false);
                renderer.sprite=floor;renderer.sharedMaterial=material;renderer.sortingOrder=-200;
                renderer.transform.localPosition=(Vector3)((new Vector2(x,y)+Vector2.one*.5f)*map.CellSize);
                renderer.transform.localScale=Vector3.one*map.CellSize/floor.bounds.size.x;renderer.color=((x+y)&1)==0?new Color(.52f,.5f,.6f):new Color(.45f,.44f,.52f);
            }
            DarklightPaintedMap.BuildGeometry(root,map);
        }
    }
}
