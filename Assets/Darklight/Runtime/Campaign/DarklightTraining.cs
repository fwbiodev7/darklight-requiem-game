using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public bool Training {get;private set;}
        public bool TrainingPassive {get;private set;}=true;
        public bool TrainingDamage {get;private set;}
        public bool TrainingHitboxes {get;private set;}
        bool luciusDuelMode;
        DarklightEnemy luciusDuelEnemy;
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
            DarklightTrainingFloor.Build(levelRoot,Map,pixelMaterial);
            InitializeRemaster(0);
            InitializeRegionAtmosphere(0);
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
            StopPower();Training=false;TrainingHitboxes=false;luciusDuelMode=false;luciusDuelEnemy=null;
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
            if(trainingReadout!=null)
            {
                if(luciusDuelMode)
                {
                    string luciusStatus=luciusDuelEnemy!=null&&!luciusDuelEnemy.Health.IsDead&&!luciusDuelEnemy.Resolved
                        ? $"LUCIUS: {Mathf.CeilToInt(luciusDuelEnemy.Health.CurrentHealth)}/{Mathf.CeilToInt(luciusDuelEnemy.Health.MaxHealth)} HP"
                        : luciusDuelEnemy!=null && luciusDuelEnemy.Resolved ? "LUCIUS POUPADO - PAUSA: REINICIAR X1" : "LUCIUS CAIU - PAUSA: REINICIAR X1";
                    trainingReadout.text=$"X1: FABIUS VS LUCIUS\n{luciusStatus}\nCOMBO {lab.Player.Model.ComboCount} · {Style.Rank}";
                }
                else
                {
                    trainingReadout.text="TREINO · "+DifficultyName+"\nCOMBO "+lab.Player.Model.ComboCount+" · "+Style.Rank+"\nPAUSA: ALVOS / ARMAS / FORMAS";
                }
            }
            if(TrainingDamage&&health.IsDead)
            {
                StopPower();health.SetMaxHealth(health.MaxHealth,false);health.FullHeal();lab.Player.ResetModel();lab.Player.GetComponent<Rigidbody2D>().position=Map.Spawn;
                if(luciusDuelMode){Notice("X1: FABIUS CAIU — REINICIANDO");RestartLuciusDuel();}
                else Notice("TREINO: VIDA RESTAURADA");
            }
        }
        void SpawnTraining(int type)
        {
            foreach(var enemy in enemies)if(enemy!=null){enemy.gameObject.SetActive(false);Destroy(enemy.gameObject);}enemies.Clear();threats.Clear();
            TrainingPassive=type==0;
            luciusDuelMode=type==6;
            luciusDuelEnemy=null;
            var origin=Map.Spawn+Vector2.up*(type==6?1.85f:2.7f);
            if(type==6)
            {
                TrainingDamage=true;
                if(Progress.luciusStance==0)Progress.luciusStance=1;
                var foe=SpawnLucius(origin);
                luciusDuelEnemy=foe;
                Close();
                Notice("X1 INICIADO: FABIUS VS LUCIUS");
                return;
            }
            var kinds=type==0?new[]{RequiemEnemyKind.Hunter}:type==1?new[]{RequiemEnemyKind.Hunter}:type==2?new[]{RequiemEnemyKind.Armored}:type==3?new[]{RequiemEnemyKind.Shooter}:type==4?new[]{RequiemEnemyKind.Hunter,RequiemEnemyKind.Armored,RequiemEnemyKind.Shooter}:new[]{RequiemEnemyKind.Kael};
            for(int i=0;i<kinds.Length;i++){Spawn(kinds[i],origin+Vector2.right*(i-(kinds.Length-1)*.5f)*2.3f,type==4&&i==0);if(TrainingPassive)enemies[enemies.Count-1].Health.SetMaxHealth(10000,false);}
            Close();
        }
        public void RestartLuciusDuel()
        {
            if(!Training)return;
            health.FullHeal();
            Blood.Set(100);
            lab.Player.ResetModel();
            lab.Player.GetComponent<Rigidbody2D>().position=Map.Spawn;
            SpawnTraining(6);
            Notice("X1 REINICIADO");
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
            bodyText.text="O progresso real fica preservado.\nSangue e tempo de forma ilimitados.\n\nLeve repetido: sequencia\nLeve, pausa, leve: variacao\nDois leves + pesado: final\nTroca de arma apos acerto\nAereo: suspender alvo leve\n\nAlvo atual: "+(luciusDuelMode?"X1 LUCIUS":TrainingPassive?"PASSIVO":"ATIVO");
            if(luciusDuelMode)
            {
                Button("REINICIAR X1 LUCIUS",RestartLuciusDuel);
                Button("HISTORICO: "+(Progress.luciusStance<0?"HOSTIL":"CONCILIADOR"),()=>
                {
                    Progress.luciusStance=Progress.luciusStance<0?1:-1;
                    RefreshPanel();
                });
            }
            Button("RETOMAR TREINO",Close);Button("ESCOLHER INIMIGOS",()=>Open("TrainingEnemies"));
            Button("ARMA: "+Weapons.Equipped.displayName,()=>{Weapons.RequestNext();Close();});
            Button("TESTAR CAVALEIRO · REQUIEM",StartAbyssPrototype);Button("GUIA DO CAVALEIRO",ShowAbyssHelp);Button("DESAFIOS DO CAVALEIRO",StartAbyssLessons);Button("TESTAR BOSSFIGHT",()=>SpawnTraining(6));
            Button("ESCOLHER TRANSFORMACAO",()=>Open("TrainingForms"));
            Button("RECEBER DANO: "+(TrainingDamage?"SIM":"NAO"),()=>{TrainingDamage=!TrainingDamage;RefreshPanel();});
            Button("VER HITBOXES: "+(TrainingHitboxes?"SIM":"NAO"),()=>{TrainingHitboxes=!TrainingHitboxes;RefreshPanel();});
            Button("DIFICULDADE: "+DifficultyName,()=>{difficultyForNewGame=false;Open("Difficulty");});Button("SAIR DA ARENA",ExitTraining);
        }
        void BuildTrainingEnemies()
        {
            heading.text="ADVERSARIOS DO TREINO";bodyText.text="Alvo passivo para sequencias; inimigos ativos para praticar defesa, postura e pressao. Escolher outro encontro limpa o anterior.";
            string[] labels={"ALVO PASSIVO","CACADOR","BLINDADO","ATIRADOR","GRUPO MISTO + ELITE","KAEL","FABIUS VS LUCIUS"};
            for(int i=0;i<labels.Length;i++){int chosen=i;Button(labels[i],()=>SpawnTraining(chosen));}Button("VOLTAR",()=>Open("Training"));
        }
        void BuildTrainingForms()
        {
            heading.text="FORMAS NO TREINO";bodyText.text="Teste Réquiem e suas armas com cada forma. A troca conserva as preferencias reais fora desta sessao.";
            string[] labels={"NORMAL","RUPTURA","SERAFIM CARMESIM"};
            for(int i=0;i<labels.Length;i++){int chosen=i;Button(labels[i],()=>SetTrainingForm(chosen));}
            Button("VOLTAR",()=>Open("Training"));
        }
    }
    public static class DarklightTrainingFloor
    {
        public static void Build(Transform root,DarklightMapDefinition map,Material material)
        {
            var artwork=Resources.Load<Sprite>("Darklight/Maps/TrainingCourtyard");
            if(artwork==null)throw new System.InvalidOperationException("TrainingCourtyard sprite is missing.");
            DarklightPaintedMap.Build(root,map,artwork,material,false);

            if(DarklightRemaster.QualityLevel<=0)return;

            var emberMaterial=Resources.Load<Material>("Darklight/SmallTransformationAura");
            if(emberMaterial==null)return;
            // The light pools are painted into the paving. Embers rise from the four perimeter braziers.
            foreach(var pixel in new[]{new Vector2(199,285),new Vector2(1336,285),new Vector2(185,580),new Vector2(1352,580)})
            {
                var brazier=new GameObject("BrasasDaArena");
                brazier.transform.SetParent(root,false);
                brazier.transform.localPosition=map.Art.World(pixel);
                var embers=brazier.AddComponent<ParticleSystem>();
                embers.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=embers.main;
                main.startLifetime=new ParticleSystem.MinMaxCurve(.45f,.9f);
                main.startSpeed=0;
                main.startSize=new ParticleSystem.MinMaxCurve(.025f,.06f);
                main.startColor=new Color(1,.56f,.2f,.7f);
                main.maxParticles=5;
                var emission=embers.emission;
                emission.rateOverTime=3;
                var shape=embers.shape;
                shape.shapeType=ParticleSystemShapeType.Circle;
                shape.radius=.08f;
                var drift=embers.velocityOverLifetime;
                drift.enabled=true;
                drift.x=new ParticleSystem.MinMaxCurve(-.1f,.1f);
                drift.y=new ParticleSystem.MinMaxCurve(.45f,.8f);
                drift.z=new ParticleSystem.MinMaxCurve(0f,0f);
                var renderer=brazier.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial=emberMaterial;
                renderer.sortingOrder=-180;
                embers.Play();
            }
        }
    }
}
