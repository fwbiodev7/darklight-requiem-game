using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight
{
    [DefaultExecutionOrder(-900)]
    public sealed partial class DarklightCampaign : MonoBehaviour
    {
        public static DarklightCampaign Instance {get;private set;}
        public Sprite[] floorTiles, architecture, enemyFrames, bossFrames, enemyAttacks, bossAttacks, extras; public Sprite enemyBarFrame;
        public Sprite[] uiArt,hudArt,icaroFrames,lyraFrames;public Sprite menuBackground;
        public AudioClip[] tracks;
        public AudioClip menuMusic;
        public AudioSource MenuMusicSource {get;private set;}
        public float[] beatsPerMinute={120,120,190,190,123};
        public float[] beatOffsets={0,0,0,0,0};
        public Material pixelMaterial;
        public DarklightProgress Progress {get;private set;}=new DarklightProgress();
        public float Energy {get=>Blood.Amount;private set=>Blood.Set(value);}
        public int Form {get;private set;}
        public float FormTime {get;private set;}
        public string Rank => Style.Rank;
        public float DamageMultiplier => Progress.Damage*(Form==0?1:Form==1?1.5f:2)*Resonance.Damage*(Blood.FrenzyRemaining>0?1.1f:1);
        public float AttackSpeed => Mathf.Min(1.85f,(Form==1?1.04f:Form==2?1.32f:1)*Resonance.Speed*(Blood.FrenzyRemaining>0?1.2f:1));
        public float MovementSpeed => (Form==2?1.22f:Form==1?1.08f:1)*Resonance.Agility;
        public float IncomingMultiplier => (Form==2?.75f:Form==1?1.12f:1)*(Resonance.Active==1?.9f:1);
        public bool WearingHeadphones => phonesTime>0;
        public bool Active => panel==""&&!deathMenuPending&&!screenTransitionBusy;
        public string Objective {get;private set;}="";
        public DarklightEnemy CurrentBoss => enemies.Find(e=>e!=null&&!e.Health.IsDead&&!e.Resolved&&e.Boss);
        readonly List<DarklightEnemy> enemies=new();
        DarklightCombatLab lab; HealthSystem health; Transform levelRoot; SpriteRenderer aura;
        AudioSource music,sfx,ambient; SpriteRenderer headphones; AudioClip[] tones;
        int encounter, lastAttack=-1, kills, selectedTrack, resonance=-1;
        bool encounterStarted, chapterCleared; float slowTime; int lastSkillSerial=-1;
        
        public bool ResistsStagger => resonance==1;
        public DarklightResonance Resonance {get;}=new DarklightResonance();
        public DarklightMapDefinition Map {get;private set;}
        public float EnemyTimeScale => slowTime>0?.55f:1;
        public Sprite[] formFrames;
        public float StrikeRangeBonus => Form==2?.15f:0;
        float style,idleStyle,noticeTime,resonanceTime,resonanceCooldown,phonesTime;
        readonly float[] rhythmSamples=new float[256];float rhythmEnvelope;MaterialPropertyBlock resonanceBlock;
        
        string notice=""; Action dialogueDone; string[] dialogue;int dialogueIndex;
        public float MusicVolume=.5f, EffectsVolume=.7f; public bool ReducedFlashes;
        void Awake()
        {
            Instance=this;lab=GetComponent<DarklightCombatLab>();
            MusicVolume=PlayerPrefs.GetFloat("Darklight.Music",.5f);EffectsVolume=PlayerPrefs.GetFloat("Darklight.Effects",.7f);ReducedFlashes=PlayerPrefs.GetInt("Darklight.Flashes",0)==1;
            music=gameObject.AddComponent<AudioSource>();music.playOnAwake=false;music.loop=false;music.spatialBlend=0;music.volume=MusicVolume;
            sfx=gameObject.AddComponent<AudioSource>();sfx.playOnAwake=false;sfx.spatialBlend=0;
            ambient=gameObject.AddComponent<AudioSource>();ambient.playOnAwake=false;ambient.loop=true;ambient.spatialBlend=0;
            MenuMusicSource=gameObject.AddComponent<AudioSource>();MenuMusicSource.playOnAwake=false;MenuMusicSource.loop=true;MenuMusicSource.spatialBlend=0;MenuMusicSource.volume=MusicVolume;MenuMusicSource.clip=menuMusic;
            tones=new[]{Tone(880,.075f),Tone(170,.11f),Tone(110,.18f),Tone(650,.3f)};
        }
        void Start()
        {
            health=lab.Player.GetComponent<HealthSystem>();InitializeWeapons();SelectSkin(PlayerPrefs.GetInt("Darklight.Skin",0),false);CreateUI();InitializeAtlas();InitializeSoundDesign();LoadMap(0);Open("Main");
            lab.PixelHUD.death.gameObject.SetActive(false);ambient.clip=null;ambient.volume=0;ApplySavedVideoPreferences();StartRequestedLocation();
        }
        void Update()
        {
            TickApprovedPresentation();
            if(MenuMusicSource!=null)MenuMusicSource.volume=MusicVolume;
            if(TickGameOver())return;
            if(Active&&!Training&&health!=null&&health.IsDead){BeginGameOver();return;}
            TickUI(); if(!Active||MenuDismissedThisFrame)return;TryStartFirstAwakeningMusic();
            float dt=lab.CombatDelta;if(dt<=0)return;
            TickFormState(dt);TickVampire(dt);noticeTime=Mathf.Max(0,noticeTime-dt); idleStyle+=dt;
            Style.Tick(dt);style=Style.Points;if(!Training)StageStats.Tick(dt);
            slowTime=Mathf.Max(0,slowTime-dt);

            int previousPower=Resonance.Active;Resonance.Tick(dt);resonance=Resonance.Active;resonanceTime=Resonance.Remaining;resonanceCooldown=Resonance.Recovery[Resonance.Selected];
            if(previousPower>=0 && resonance<0){FinishMusicEvent();Notice("DARK RESONANCE: recarga do modo");}
            if(lab.Input.Action("PowerNext").WasPressedThisFrame())Resonance.Cycle(1);
            if(lab.Input.Action("PowerPrevious").WasPressedThisFrame())Resonance.Cycle(-1);
            if(lab.Input.Action("Power").WasPressedThisFrame())TryActivatePower();
            if(phonesTime>0)
            {
                phonesTime-=dt;if(lab.Player.Model.State==CombatState.Hurt){phonesTime=0;Notice("Fones interrompidos");}
                else if(phonesTime<=0)BeginResonance();
            }
            if(resonance>=0)
            {
                if(resonance==0)health.Heal(Blood.AllowHealing(health.MaxHealth*.014f*dt,health.MaxHealth));
            }
            if(lab.Player.Model.State==CombatState.Attack&&lab.Player.Model.Attack.kind==StrikeKind.Skill&&lab.Player.Model.AttackSerial!=lastSkillSerial&&lab.Player.Model.ActiveThisTick){lastSkillSerial=lab.Player.Model.AttackSerial;FormAbility();}
            if(lab.Input.Action("Transform").WasPressedThisFrame())TransformPlayer();
            if(lab.Input.Action("MP3").WasPressedThisFrame()){Open("MP3");return;}
            if(lab.Input.Action("Potion").WasPressedThisFrame()&&!(AbyssRider?.Active==true&&AbyssRider.Dashing&&lab.Input.Action("AbyssDrift").WasPressedThisFrame()))UsePotion();
            if(lab.Input.Action("Journal").WasPressedThisFrame()){Open("Journal");return;}
            if(lab.Input.Action("Interact").WasPressedThisFrame())Interact();
            if(Training){TickTraining(dt);UpdateAura();return;}
            if(ExploringNewRegion){UpdateRegionExploration();UpdateAura();return;}
            if(!encounterStarted&&!chapterCleared&&Map.InArena(encounter,lab.Player.transform.position)){StartEncounter();return;}
            if(encounterStarted&&enemies.TrueForAll(e=>e==null||e.Health.IsDead||e.Resolved))ClearEncounter();
            UpdateAura();
            // Walking contacts are emitted in TodayCampaignSoundDesign after movement presentation.
        }
        void LoadMap(int chapter)
        {
            if(phaseLoading){pendingMapChapter=chapter;return;}
            var stages=BuildMapInStages(chapter);while(stages.MoveNext()){}
        }
        IEnumerator BuildMapInStages(int chapter)
        {
            battleSettleUntil=cueDuration=0;if(cueRoot!=null)cueRoot.gameObject.SetActive(false);
            deathMenuPending=false;exploredRegion=-1;notice="";noticeTime=0;Style.Reset();threats.Clear();if(!Training)StageStats.Begin(Progress.StageResult(chapter));
            if(levelRoot!=null){levelRoot.gameObject.SetActive(false);Destroy(levelRoot.gameObject);}
            foreach(var shot in FindObjectsByType<DarklightProjectile>())Destroy(shot.gameObject);
            enemies.Clear();levelRoot=new GameObject(DarklightStory.Places[chapter]).transform;
            Map=DarklightMapDefinition.ForChapter(chapter);
            SetLoadingProgress(.14f,"MONTANDO O CENÁRIO");yield return null;
            DarklightMapLayout.Build(levelRoot,chapter,floorTiles,architecture,pixelMaterial);
            SetLoadingProgress(.72f,"PREPARANDO PERSONAGEM E COLISÕES");yield return null;
            Physics2D.SyncTransforms();
            encounter=Mathf.Clamp(Progress.checkpoint,0,3);encounterStarted=false;chapterCleared=Progress.checkpoint==4;
            Vector2 fallback=Progress.checkpoint==4?Map.Arenas[3]:Map.Checkpoints[encounter];
            Vector3 start=Map.SafeSpawn(Progress.TryReturnPoint(Progress.chapter,Progress.checkpoint,out var savedReturn)?savedReturn:fallback);
            Progress.RecordReturnPoint(Progress.chapter,Progress.checkpoint,start);
            lab.Player.transform.position=start;lab.Player.GetComponent<Rigidbody2D>().position=start;
            Weapons.Restore();RestoreAppearance();health.SetMaxHealth(Progress.MaxHealth,false);
            var mapCamera=lab.WorldCamera.GetComponent<DarklightLabCamera>();mapCamera.bounds=Map.Bounds;mapCamera.followOffset=new Vector2(0,.75f);
            lab.WorldCamera.transform.position=start+new Vector3(0,.75f,-10);
            ClearFormPresentation();Form=0;FormTime=0;slowTime=0;lastSkillSerial=-1;phonesTime=0;resonance=-1;Resonance.Restore(Progress);resonanceCooldown=Resonance.Recovery[Resonance.Selected];RestoreVampire();style=0;
            if(aura==null)
            {aura=new GameObject("AuraRequiem").AddComponent<SpriteRenderer>();aura.transform.SetParent(lab.Player.transform,false);aura.transform.localPosition=Vector3.up*.6f;aura.sharedMaterial=pixelMaterial;aura.sortingOrder=75;}
            aura.sprite=extras[4];aura.enabled=false;
            AddProp("Checkpoint",Resources.Load<Sprite>("Darklight/Props/CheckpointShrine")??extras[3],Map.Spawn+Vector2.up*1.8f,1.3f);
            if(!Progress.relics[chapter])AddProp("Fragmento",extras[5],Map.Relic,.7f);
            if(chapter==1)AddProp("IcaroNox",extras[0],Map.Ally,1.8f);
            if(chapter==0||chapter==2||chapter==5||chapter==6||chapter==8)AddProp("LyraMoreau",extras[1],Map.Ally,1.8f);
            if(chapter==3||chapter==4||chapter==7||chapter==9)AddProp("KaelAliado",bossFrames[0],Map.Ally,1.8f);
            if(chapterCleared)AddProp("Portal",extras[3],Map.Portal,2.5f);
            SetLoadingProgress(.88f,"PREPARANDO OBJETIVOS E ATMOSFERA");yield return null;
            PrepareNarrativeMap();Objective=Goal();firstAwakeningMusicPending=false;music.Stop();ambient.clip=null;ambient.volume=0;AudioDirector?.Region(chapter);InitializeRemaster(chapter);InitializeRegionAtmosphere(chapter);
            BuildMultiverse(chapter);
            SetLoadingProgress(.99f,"CONCLUINDO A PASSAGEM");yield return null;
        }
        void InitializeRemaster(int chapter){var view=GetComponent<DarklightRemaster>()??gameObject.AddComponent<DarklightRemaster>();view.Initialize(lab.WorldCamera,lab.Player.transform,levelRoot,chapter);}
        void AddProp(string name,Sprite sprite,Vector2 position,float height)
        {
            if(name=="Checkpoint")height=2.3f;else if(name=="Portal"||name=="SaidaDaRegiao")height=4.2f;
            if(name=="IcaroNox"&&icaroFrames?.Length==8)sprite=icaroFrames[0];
            else if(name=="LyraMoreau"&&lyraFrames?.Length==8)sprite=lyraFrames[0];
            position=InteractionAnchor(position);
            height=name=="LyraMoreau"?2.5f:name=="IcaroNox"?2.7f:name=="KaelAliado"?2.85f:height*DarklightWorldScale.Environment;
            var obj=new GameObject(name);obj.transform.SetParent(levelRoot,false);obj.transform.position=position;var r=obj.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=pixelMaterial;DarklightSecondaryStyleBank.Attach(r);obj.transform.localScale=Vector3.one*height/sprite.bounds.size.y;r.sortingOrder=100-Mathf.RoundToInt(position.y*10);if(name=="IcaroNox"||name=="LyraMoreau"){var view=obj.AddComponent<DarklightNPCView>();view.frames=name=="IcaroNox"?icaroFrames:lyraFrames;}else if(name=="KaelAliado")obj.AddComponent<DarklightActorIdle>();}
        string Goal()=>chapterCleared?"PORTAL LIBERTO - INTERAGIR PARA AVANCAR":ExpandedChapter?MissionTitle:(encounter==3?"ENFRENTE "+DarklightStory.Bosses[Progress.chapter]:"ENCONTRE O SELO "+(encounter+1)+" / 3");
        public void PreviewCampaign() { Progress=new DarklightProgress();LoadMap(0);Close(); }
        public void PreviewChapter(int chapter) { Progress=new DarklightProgress{chapter=Mathf.Clamp(chapter,0,DarklightStory.Chapters.Length-1)};LoadMap(Progress.chapter);Close(); }
        public void NewGame()
        {if(phaseLoading)return;var old=DarklightProgress.Load();StopMenuMusic();BeginPhaseTransition(0,()=>{atlas?.ResetCampaign();Progress=new DarklightProgress{difficulty=newGameDifficulty,extremeUnlocked=DarklightDifficultyProfile.ExtremeUnlocked(old)};LoadMap(0);},()=>{SaveCheckpoint();Speak(DarklightStory.Introductions[0]);});}
        public void ContinueGame()
        {if(phaseLoading)return;var data=DarklightProgress.Load();if(data==null){Notice("Nenhum save valido");return;}StopMenuMusic();if(data.completed){Progress=data;Open("Victory");return;}BeginPhaseTransition(data.chapter,()=>{Progress=data;LoadMap(Progress.chapter);},()=>SaveCheckpoint());}
        void StartMenuMusic()
        {
            if(MenuMusicSource==null)return;
            MenuMusicSource.clip=menuMusic;MenuMusicSource.volume=MusicVolume;
            if(!DarklightAutomation.Silent&&menuMusic!=null&&!MenuMusicSource.isPlaying)MenuMusicSource.Play();
        }
        void StopMenuMusic(){if(MenuMusicSource!=null)MenuMusicSource.Stop();}
        void StartEncounter()
        {
            if(BeginNarrativeEncounter())return;encounterStarted=true;Objective=encounter==3?"DERROTE "+DarklightStory.Bosses[Progress.chapter]:"LIMPE O SELO "+(encounter+1)+" / 3";
            if(encounter==3)Speak(DarklightStory.BossLines[Progress.chapter],()=>{ambient.clip=null;Spawn((RequiemEnemyKind)(5+Progress.chapter),Map.Arenas[3],false);});
            else
            {
                int count=3+Mathf.Min(Progress.chapter,3);
                for(int i=0;i<count;i++)
                {int kind=(i+encounter+Progress.chapter)%Mathf.Min(5,3+Progress.chapter);Vector2 pos=Map.Arenas[encounter]+new Vector2((i%3-1)*1.2f,(i/3==0?1:-1)*(1.1f+(i%2)*.6f));Spawn((RequiemEnemyKind)kind,pos,Progress.chapter>2&&i==0);}
                Notice("O SELO SE FECHOU");
            }
        }
        void Spawn(RequiemEnemyKind kind,Vector2 pos,bool elite)
        {
            pos=Map.SafeSpawn(pos,kind==RequiemEnemyKind.Armored?.45f:.32f);
            var obj=new GameObject(kind.ToString());obj.transform.SetParent(levelRoot,false);obj.transform.position=pos;
            Sprite[] frames=new Sprite[4];
            if((int)kind>=11)Array.Copy(ExpandedBossFrames,((int)kind-11)*8,frames,0,4);
            else Array.Copy((int)kind<5?enemyFrames:bossFrames,((int)kind<5?(int)kind:(int)kind-5)*4,frames,0,4);
            var art=new GameObject("Sprite").AddComponent<SpriteRenderer>();art.transform.SetParent(obj.transform,false);art.sprite=frames[0];art.sharedMaterial=pixelMaterial;DarklightSecondaryStyleBank.Attach(art);
            float height=DarklightWorldScale.EnemyHeight(kind);
            art.transform.localScale=Vector3.one*height/frames[0].bounds.size.y;
            var rigid=obj.AddComponent<Rigidbody2D>();rigid.gravityScale=0;rigid.freezeRotation=true;rigid.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var collider=obj.AddComponent<CircleCollider2D>();collider.radius=kind==RequiemEnemyKind.Armored?.42f:.28f;collider.offset=Vector2.up*.18f;
            var hp=obj.AddComponent<HealthSystem>();obj.AddComponent<DarklightCombatant>();
            var foe=obj.AddComponent<DarklightEnemy>();foe.kind=kind;foe.elite=elite;foe.poses=frames;foe.attackFrames=new Sprite[4];if((int)kind>=11)Array.Copy(ExpandedBossFrames,((int)kind-11)*8+4,foe.attackFrames,0,4);else Array.Copy(foe.Boss?bossAttacks:enemyAttacks,(foe.Boss?(int)kind-5:(int)kind)*4,foe.attackFrames,0,4);
            hp.SetMaxHealth((foe.Boss?260+Progress.chapter*65:kind==RequiemEnemyKind.Armored?90:50+Progress.chapter*7)*(elite?1.45f:1),false);if(foe.ExpandedBoss)foe.RestorePhase(Progress.bossPhase);enemies.Add(foe);AddEnemyBar(foe);obj.AddComponent<TodayEnemyAudio>();if(Training)obj.AddComponent<DarklightCombatDebug>();
            if(foe.Boss&&!Training)ShowPresentationCue(DarklightStory.Bosses[Progress.chapter],"ENTER / A: DISPENSAR   ·   COMANDOS LIVRES",true);
        }
        void ClearEncounter()
        {
            if(FinishNarrativeCombat())return;encounterStarted=false;BeginBattleSettle();
            if(encounter==3)
            {
                chapterCleared=true;Progress.checkpoint=4;Progress.formsUnlocked=Mathf.Max(Progress.formsUnlocked,Progress.chapter>=9?3:Progress.chapter>=3?2:1);Progress.potions=Mathf.Min(9,Progress.potions+2);Progress.points++;Energy=100;health.FullHeal();
                AddProp("Portal",extras[3],Map.Portal,2.5f);Objective=Goal();
                ExpandedChapterReward();Progress.bossPhase=1;VampireChapterReward();UnlockWeaponReward();StageStats.Finish(Style);SaveCheckpoint();Speak(DarklightStory.Conclusions[Progress.chapter],()=>Open("StageResults"));if(Progress.chapter!=0&&Progress.chapter!=2)Notice("CAPITULO CONCLUIDO");
            }
            else {encounter++;Progress.checkpoint=encounter;Progress.missionWave=0;health.Heal(health.MaxHealth*.18f);Energy=Mathf.Min(100,Energy+15);Objective=Goal();SaveCheckpoint();Notice("SELO LIBERTO - CHECKPOINT SALVO");}
        }
        void Interact(){if(!TryInteract())Notice("APROXIME-SE DE UM NPC, MEMÓRIA OU CHECKPOINT");}
        public bool RequestAttack(DarklightEnemy enemy)
        {return TechnicalAttackPermit(enemy);}
        public void EnemyDefeated(DarklightEnemy enemy)
        {
            if(RewardVisitor(enemy))return;
            if(Training)return;StageStats.Kill();int previous=Progress.level;Progress.Award(enemy.Boss?150:18+Progress.chapter*4);kills++;
            Energy=Mathf.Min(100,Energy+(enemy.Boss?30:8));Resonance.Gain(enemy.Boss?20:5);if(Progress.chapter==1&&Progress.mp3Quest>0&&Progress.mp3Quest<4)Progress.mp3Quest++;
            if(previous!=Progress.level){health.SetMaxHealth(Progress.MaxHealth);health.FullHeal();Notice("NIVEL "+Progress.level+" - +1 PONTO");Sound(3);}
        }
        public void Strike(StrikeKind kind)
        {if(!Blood.RewardStrike(kind,lab.Player.Model.AttackSerial))return;bool varied=(int)kind!=lastAttack;TechnicalStrike(kind);lastAttack=(int)kind;idleStyle=0;Resonance.Gain(varied?2:1);RecordWeaponTrial();}
        public void BossPhaseTwo(){AudioDirector?.EnemyWarning(true,lab.Player.transform.position);}
        public void Defended(){if(!Blood.RewardParry())return;Style.Defense(true);if(!Training)StageStats.Parry();style=Style.Points;idleStyle=0;AudioDirector?.Play("dl-power-parry",DarklightAudioCategory.Combat,.38f);}
        public void Damaged(float actualHealthLost=0){CancelVampireCast();if(!Training)StageStats.Damage(actualHealthLost);Style.Damage();style=Style.Points;phonesTime=0;AudioDirector?.PlayerPain();}
        public void Evaded(float maneuverStart){if(!Training&&StageStats.Dodge(Mathf.RoundToInt(maneuverStart*10000))){Style.Defense(false);style=Style.Points;}}
        void TransformPlayer()=>TryTransformPlayer();
        void EndForm(bool present=true)
        {
            int previous=Form;Form=0;FormTime=0;slowTime=0;
            if(present&&previous>0)BeginFormPresentation(previous,false);else ClearFormPresentation();
        }
        void FormAbility()
        {
            if(Form==0||Transforming)return;
            if(Form==1)
            {
                foreach(var enemy in enemies)if(enemy!=null&&!enemy.Health.IsDead)
                {Vector2 delta=enemy.transform.position-lab.Player.transform.position;if(delta.magnitude<6&&Vector2.Angle(lab.Player.Model.Facing,delta)<55)enemy.GetComponent<DarklightCombatant>().Receive(14*DamageMultiplier,delta.normalized);}
                Notice("CORTE DA RUPTURA");
            }
            else if(Form==2)
            {
                Vector2 step=lab.Player.Model.Facing*2.2f;var rb=lab.Player.GetComponent<Rigidbody2D>();
                bool blocked=false;foreach(var hit in Physics2D.CircleCastAll(rb.position,.25f,step.normalized,step.magnitude))if(!hit.collider.isTrigger&&hit.collider.GetComponentInParent<DarklightCombatant>()==null)blocked=true;
                if(!blocked)rb.position+=step;
                foreach(var enemy in enemies)if(enemy!=null&&!enemy.Health.IsDead&&Vector2.Distance(enemy.transform.position,rb.position)<5f)enemy.GetComponent<DarklightCombatant>().Receive(34*DamageMultiplier,Vector2.up);
                slowTime=2.5f;Notice("PASSO DO JUIZO — ECLIPSE ABSOLUTO");
            }
            else
            {
                slowTime=2.5f;
                foreach(var enemy in enemies)if(enemy!=null&&!enemy.Health.IsDead&&Vector2.Distance(enemy.transform.position,lab.Player.transform.position)<5)enemy.GetComponent<DarklightCombatant>().Receive(34*DamageMultiplier,Vector2.up);
                Notice("JUIZO DO ECLIPSE");
            }
            Sound(3);
        }
        void UsePotion(){if(Progress.potions<=0){Notice("SEM ELIXIRES");AudioDirector?.ActionDenied();return;}if(health.IsDead||health.HealthPercent>=1)return;Progress.potions--;health.Heal(health.MaxHealth*.45f);AudioDirector?.Delivered("PC05");Notice("ELIXIR: +45% VIDA");SaveCheckpoint();}
        void SelectSong(int index)
        {
            if(!Progress.mp3Unlocked){Notice("DESBLOQUEIE COM ICARO NOX NO CAPITULO 2");return;}
            Resonance.Select(index<2?0:index<4?2:1);
            if(!Resonance.Ready||phonesTime>0){Notice("DARK RESONANCE EM RECARGA");return;}
            selectedTrack=index;Close();TryActivatePower();
        }
        public bool TryActivatePower()
        {
            if(Transforming)return false;
            if(!Progress.mp3Unlocked){Notice("DESBLOQUEIE O MP3 COM ICARO NOX");AudioDirector?.ActionDenied();return false;}
            if(!Resonance.Ready||phonesTime>0){Notice("MODO EM RECARGA OU SEM ENERGIA");AudioDirector?.ActionDenied();return false;}
            if(lab.Player.Model.State==CombatState.Dead||lab.Player.Model.State==CombatState.Hurt)return false;
            phonesTime=.28f;AudioDirector?.Delivered("PW10");Notice("COLOCANDO FONES");return true;
        }
        void BeginResonance()
        {
            if(!Resonance.Activate())return;resonance=Resonance.Active;resonanceTime=Resonance.Remaining;
            if(resonance==0 && selectedTrack>1)selectedTrack=0;
            if(resonance==1)selectedTrack=4;
            if(resonance==2 && (selectedTrack<2||selectedTrack>3))selectedTrack=2;
            ambient.Pause();music.Stop();music.clip=tracks[selectedTrack];music.volume=0;SkinMusic?.BeginEvent();
            if(!DarklightAutomation.Silent&&music.clip!=null){music.time=0;music.Play();}
            Sound(resonance==0?0:resonance==1?2:3);Notice(DarklightResonance.Names[resonance]);
        }
        void StopPower(){firstAwakeningMusicPending=false;StopVampire();Resonance.Stop();resonance=-1;phonesTime=0;EndForm(false);if(aura!=null)aura.enabled=false;if(headphones!=null)headphones.enabled=false;FinishMusicEvent();}
        void UpdateAura()
        {
            if(aura==null)return;
            if(headphones==null){headphones=new GameObject("Fones").AddComponent<SpriteRenderer>();headphones.transform.SetParent(lab.Player.transform,false);headphones.sprite=extras[8];headphones.sharedMaterial=pixelMaterial;headphones.transform.localScale=Vector3.one*(.37f*DarklightCharacterView.WorldVisualScale)/headphones.sprite.bounds.size.y;}
            headphones.enabled=phonesTime>0||resonance>=0;headphones.sortingOrder=lab.Player.GetComponentInChildren<SpriteRenderer>().sortingOrder+1;headphones.transform.localPosition=Vector3.up*DarklightCharacterView.WorldVisualScale*Mathf.Lerp(.75f,1.65f,1-Mathf.Clamp01(phonesTime/.28f));aura.enabled=resonance>=0||phonesTime>0;
            float bpm=beatsPerMinute[selectedTrack];
            float beat=music.isPlaying?Mathf.Sin((music.time-beatOffsets[selectedTrack])*bpm/60*Mathf.PI*2):Mathf.Sin(lab.Player.Model.Clock*5);
            if(music.isPlaying && !DarklightAutomation.Silent)
            {
                music.GetOutputData(rhythmSamples,0);float peak=0;foreach(float sample in rhythmSamples)peak=Mathf.Max(peak,Mathf.Abs(sample));
                rhythmEnvelope=Mathf.Lerp(rhythmEnvelope,peak,Mathf.Clamp01(Time.deltaTime*18));beat=Mathf.Clamp01(rhythmEnvelope*5);
            }
            resonanceBlock ??= new MaterialPropertyBlock();resonanceBlock.SetFloat("_ResonanceTint",1);aura.SetPropertyBlock(resonanceBlock);
            float pulse=ReducedFlashes?1:1+.08f*beat;aura.transform.localScale=Vector3.one*(2.5f*DarklightCharacterView.WorldVisualScale/aura.sprite.bounds.size.y)*pulse;
            aura.color=phonesTime>0?new Color(.6f,.7f,1,.45f):Form==3?new Color(1,.9f,.65f,.65f):Form==2?new Color(.78f,.65f,1,.7f):resonance>=0?new Color(DarklightResonance.Colors[resonance].r,DarklightResonance.Colors[resonance].g,DarklightResonance.Colors[resonance].b,.6f):new Color(1,.2f,.3f,.6f);
        }
        public void Fire(Vector3 position,Vector2 direction,float damage,float speed)
        {var obj=new GameObject("CorteDoVeu");obj.transform.position=position;var sprite=obj.AddComponent<SpriteRenderer>();sprite.sprite=extras[5];sprite.sharedMaterial=pixelMaterial;sprite.sortingOrder=140;obj.transform.localScale=Vector3.one*.38f/sprite.sprite.bounds.size.y;var projectile=obj.AddComponent<DarklightProjectile>();projectile.direction=direction;projectile.damage=damage;projectile.speed=speed;}
        public void Telegraph(Vector3 position,Vector2 direction,float radius,bool area)
        {var obj=new GameObject("Aviso");obj.transform.SetParent(levelRoot,false);obj.transform.position=position+(Vector3)(area?Vector2.zero:direction*.65f);var sprite=obj.AddComponent<SpriteRenderer>();sprite.sprite=extras[4];sprite.sharedMaterial=pixelMaterial;sprite.sortingOrder=80;sprite.color=new Color(1,.13f,.2f,.48f);obj.transform.localScale=Vector3.one*(radius/sprite.sprite.bounds.size.y);Destroy(obj,.04f);}
        public void Notice(string message){notice=message;noticeTime=3.5f;}
        bool SaveCheckpoint()
        {
            if(Training)return false;if(Progress.completed)Progress.extremeUnlocked=true;SaveMapExploration();
            if(health!=null&&!health.IsDead&&Map!=null&&!ExploringNewRegion&&!Progress.TryReturnPoint(Progress.chapter,Progress.checkpoint,out _))
            {
                Vector2 anchor=Progress.checkpoint==4?Map.Arenas[3]:Map.Checkpoints[Mathf.Clamp(Progress.checkpoint,0,3)];
                Progress.RecordReturnPoint(Progress.chapter,Progress.checkpoint,Map.SafeSpawn(anchor));
            }
            if(DarklightAutomation.Silent||ExploringNewRegion||PlaytestSession)return !ExploringNewRegion;
            Resonance.Store(Progress);Blood.Store(Progress);try{Progress.Save();return true;}catch(Exception e){Notice("NAO FOI POSSIVEL SALVAR");Debug.LogError(e);return false;}
        }
        void Speak(string[] lines,Action done=null){dialogue=PrepareDialogueLines(lines);dialogueIndex=0;dialogueDone=done;Open("Dialogue");}
        void NextDialogue(){if(++dialogueIndex<dialogue.Length){RefreshPanel();return;}Close();var done=dialogueDone;dialogueDone=null;done?.Invoke();}
        public void Sound(int index){AudioDirector?.Play(index==1?"rpg-knifeSlice":index==2?"impact-impactPunch_heavy":"interface-confirmation_",index==1?DarklightAudioCategory.Combat:index==2?DarklightAudioCategory.Character:DarklightAudioCategory.Interface,index==3?.23f:.18f);}
        static AudioClip Tone(float frequency,float duration)
        {int count=(int)(22050*duration);var data=new float[count];for(int i=0;i<count;i++){float t=i/22050f;data[i]=Mathf.Sin(t*frequency*Mathf.PI*2)*Mathf.Pow(1-(float)i/count,2)*.3f;}var clip=AudioClip.Create("RequiemSFX",count,1,22050,false);clip.SetData(data,0);return clip;}
        void OnDestroy(){combatProfiles?.Dispose();if(Instance==this)Instance=null;rebinding?.Dispose();foreach(var tone in tones)if(tone!=null)Destroy(tone);}
    }
}



