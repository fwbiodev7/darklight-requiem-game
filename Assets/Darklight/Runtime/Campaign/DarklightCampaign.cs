using System;
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
        public float[] beatsPerMinute={120,120,190,190,123};
        public float[] beatOffsets={0,0,0,0,0};
        public Material pixelMaterial;
        public DarklightProgress Progress {get;private set;}=new DarklightProgress();
        public float Energy {get;private set;}
        public int Form {get;private set;}
        public float FormTime {get;private set;}
        public string Rank => new[]{"D","C","B","A","S","SS","SSS"}[Mathf.Clamp((int)(style/65),0,6)];
        public float DamageMultiplier => Progress.Damage*(Form==0?1:Form==1?1.35f:Form==2?1.65f:2)*Resonance.Damage;
        public float AttackSpeed => (Form>0?1.12f:1)*Resonance.Speed;
        public float MovementSpeed => (Form>0?1.12f:1)*Resonance.Agility;
        public float IncomingMultiplier => Form==3?.75f:1;
        public bool WearingHeadphones => phonesTime>0;
        public bool Active => panel=="";
        public string Objective {get;private set;}="";
        public DarklightEnemy CurrentBoss => enemies.Find(e=>e!=null&&!e.Health.IsDead&&e.Boss);
        readonly List<DarklightEnemy> enemies=new();
        DarklightCombatLab lab; HealthSystem health; Transform levelRoot; SpriteRenderer aura;
        AudioSource music,sfx,ambient; SpriteRenderer headphones; float nextStep; AudioClip[] tones;
        int encounter, lastAttack=-1, kills, selectedTrack, resonance=-1;
        bool encounterStarted, chapterCleared; float slowTime; int lastSkillSerial=-1;
        
        public bool ResistsStagger => resonance==1;
        public DarklightResonance Resonance {get;}=new DarklightResonance();
        public DarklightMapDefinition Map {get;private set;}
        public float EnemyTimeScale => slowTime>0?.55f:1;
        public Sprite[] formFrames;
        public float StrikeRangeBonus => Form>=2?1.1f:0;
        float style,idleStyle,attackPermit,noticeTime,resonanceTime,resonanceCooldown,phonesTime;
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
            tones=new[]{Tone(880,.075f),Tone(170,.11f),Tone(110,.18f),Tone(650,.3f)};
        }
        void Start()
        {
            health=lab.Player.GetComponent<HealthSystem>();InitializeWeapons();SelectSkin(PlayerPrefs.GetInt("Darklight.Skin",0),false);CreateUI();LoadMap(0);Open("Main");
            lab.PixelHUD.death.gameObject.SetActive(false);ambient.clip=tracks[1];ambient.volume=MusicVolume*.65f;
        }
        void Update()
        {
            TickUI(); if(!Active)return;
            float dt=lab.CombatDelta;if(dt<=0)return;
            noticeTime=Mathf.Max(0,noticeTime-dt); idleStyle+=dt;
            if(idleStyle>2)style=Mathf.Max(0,style-dt*9);
            if(health.IsDead){StopPower();SaveCheckpoint();Open("Death");return;}
            slowTime=Mathf.Max(0,slowTime-dt);
            if(Form>0){FormTime-=dt;if(FormTime<=0)EndForm();}
            int previousPower=Resonance.Active;Resonance.Tick(dt);resonance=Resonance.Active;resonanceTime=Resonance.Remaining;resonanceCooldown=Resonance.Recovery[Resonance.Selected];
            if(previousPower>=0 && resonance<0){music.Stop();Notice("DARK RESONANCE: recarga do modo");}
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
                if(resonance==0)health.Heal(health.MaxHealth*.014f*dt);
            }
            if(lab.Player.Model.State==CombatState.Attack&&lab.Player.Model.Attack.kind==StrikeKind.Skill&&lab.Player.Model.AttackSerial!=lastSkillSerial&&lab.Player.Model.ActiveThisTick){lastSkillSerial=lab.Player.Model.AttackSerial;FormAbility();}
            if(lab.Input.Action("Transform").WasPressedThisFrame())TransformPlayer();
            if(lab.Input.Action("MP3").WasPressedThisFrame()){Open("MP3");return;}
            if(lab.Input.Action("Potion").WasPressedThisFrame())UsePotion();
            if(lab.Input.Action("Journal").WasPressedThisFrame()){Open("Journal");return;}
            if(lab.Input.Action("Interact").WasPressedThisFrame())Interact();
            if(!encounterStarted&&!chapterCleared&&Map.InArena(encounter,lab.Player.transform.position)){StartEncounter();return;}
            if(encounterStarted&&enemies.TrueForAll(e=>e==null||e.Health.IsDead))ClearEncounter();
            UpdateAura();
            if(lab.Player.Model.Motion.sqrMagnitude>.3f&&lab.Player.Model.Clock>=nextStep){nextStep=lab.Player.Model.Clock+.32f;if(!DarklightAutomation.Silent)sfx.PlayOneShot(tones[2],EffectsVolume*.035f);}
        }
        void LoadMap(int chapter)
        {
            if(levelRoot!=null){levelRoot.gameObject.SetActive(false);Destroy(levelRoot.gameObject);}
            foreach(var shot in FindObjectsByType<DarklightProjectile>(FindObjectsSortMode.None))Destroy(shot.gameObject);
            enemies.Clear();levelRoot=new GameObject(DarklightStory.Places[chapter]).transform;
            Map=DarklightMapDefinition.ForChapter(chapter);
            DarklightMapLayout.Build(levelRoot,chapter,floorTiles,architecture,pixelMaterial);
            Physics2D.SyncTransforms();
            encounter=Mathf.Clamp(Progress.checkpoint,0,3);encounterStarted=false;chapterCleared=Progress.checkpoint==4;attackPermit=0;
            Vector3 start=Map.SafeSpawn(Progress.checkpoint==4?Map.Arenas[3]:Map.Checkpoints[encounter]);
            lab.Player.transform.position=start;lab.Player.GetComponent<Rigidbody2D>().position=start;
            Weapons.Restore();RestoreAppearance();health.SetMaxHealth(Progress.MaxHealth,false);
            var mapCamera=lab.WorldCamera.GetComponent<DarklightLabCamera>();mapCamera.bounds=Map.Bounds;mapCamera.followOffset=new Vector2(0,.75f);
            lab.WorldCamera.transform.position=start+new Vector3(0,.75f,-10);
            Form=0;FormTime=0;slowTime=0;lastSkillSerial=-1;phonesTime=0;resonance=-1;Resonance.Restore(Progress);resonanceCooldown=Resonance.Recovery[Resonance.Selected];Energy=0;style=0;
            if(aura==null)
            {aura=new GameObject("AuraRequiem").AddComponent<SpriteRenderer>();aura.transform.SetParent(lab.Player.transform,false);aura.transform.localPosition=Vector3.up*.6f;aura.sharedMaterial=pixelMaterial;aura.sortingOrder=75;}
            aura.sprite=extras[4];aura.enabled=false;
            AddProp("Checkpoint",extras[3],Map.Spawn+Vector2.up*1.8f,1.3f);
            AddProp("Fragmento",extras[5],Map.Relic,.7f);
            if(chapter==1)AddProp("IcaroNox",extras[0],Map.Ally,1.8f);
            if(chapter==0||chapter==2||chapter==5)AddProp("LyraMoreau",extras[1],Map.Ally,1.8f);
            if(chapter==3||chapter==4)AddProp("KaelAliado",bossFrames[0],Map.Ally,1.8f);
            if(chapterCleared)AddProp("Portal",extras[3],Map.Portal,2.5f);
            Objective=Goal();music.Stop();ambient.clip=tracks[chapter%5];ambient.volume=MusicVolume*.4f;
        }
        void AddProp(string name,Sprite sprite,Vector2 position,float height)
        {var obj=new GameObject(name);obj.transform.SetParent(levelRoot,false);obj.transform.position=position;var r=obj.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=pixelMaterial;obj.transform.localScale=Vector3.one*height/sprite.bounds.size.y;r.sortingOrder=100-Mathf.RoundToInt(position.y*10);if(name=="IcaroNox"||name=="LyraMoreau"){var view=obj.AddComponent<DarklightNPCView>();view.frames=name=="IcaroNox"?icaroFrames:lyraFrames;}else if(name=="KaelAliado")obj.AddComponent<DarklightActorIdle>();}
        string Goal()=>chapterCleared?"PORTAL LIBERTO - INTERAGIR PARA AVANCAR":(encounter==3?"ENFRENTE "+DarklightStory.Bosses[Progress.chapter]:"ENCONTRE O SELO "+(encounter+1)+" / 3");
        public void PreviewCampaign() { Progress=new DarklightProgress();LoadMap(0);Close(); }
        public void PreviewChapter(int chapter) { Progress=new DarklightProgress{chapter=Mathf.Clamp(chapter,0,5)};LoadMap(Progress.chapter);Close(); }
        public void NewGame()
        {Progress=new DarklightProgress();LoadMap(0);SaveCheckpoint();Speak(DarklightStory.Introductions[0]);}
        public void ContinueGame()
        {var data=DarklightProgress.Load();if(data==null){Notice("Nenhum save valido");return;}Progress=data;if(data.completed){Open("Victory");return;}LoadMap(Progress.chapter);Close();}
        void StartEncounter()
        {
            encounterStarted=true;Objective=encounter==3?"DERROTE "+DarklightStory.Bosses[Progress.chapter]:"LIMPE O SELO "+(encounter+1)+" / 3";
            if(encounter==3)Speak(DarklightStory.BossLines[Progress.chapter],()=>{ambient.clip=tracks[2];ambient.volume=MusicVolume*.6f;Spawn((RequiemEnemyKind)(5+Progress.chapter),Map.Arenas[3],false);});
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
            Sprite[] frames=new Sprite[4];Array.Copy((int)kind<5?enemyFrames:bossFrames,((int)kind%((int)kind<5?5:6))*4,frames,0,4);
            // Boss array begins at Kael; enums begin at five.
            if((int)kind>=5)Array.Copy(bossFrames,((int)kind-5)*4,frames,0,4);
            var art=new GameObject("Sprite").AddComponent<SpriteRenderer>();art.transform.SetParent(obj.transform,false);art.sprite=frames[0];art.sharedMaterial=pixelMaterial;
            float height=(int)kind>=5?2.15f:kind==RequiemEnemyKind.Armored?2.1f:1.65f;
            art.transform.localScale=Vector3.one*height/frames[0].bounds.size.y;
            var rigid=obj.AddComponent<Rigidbody2D>();rigid.gravityScale=0;rigid.freezeRotation=true;rigid.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            var collider=obj.AddComponent<CircleCollider2D>();collider.radius=kind==RequiemEnemyKind.Armored?.42f:.28f;collider.offset=Vector2.up*.18f;
            var hp=obj.AddComponent<HealthSystem>();obj.AddComponent<DarklightCombatant>();
            var foe=obj.AddComponent<DarklightEnemy>();foe.kind=kind;foe.elite=elite;foe.poses=frames;foe.attackFrames=new Sprite[4];Array.Copy(foe.Boss?bossAttacks:enemyAttacks,(foe.Boss?(int)kind-5:(int)kind)*4,foe.attackFrames,0,4);
            hp.SetMaxHealth((foe.Boss?260+Progress.chapter*65:kind==RequiemEnemyKind.Armored?90:50+Progress.chapter*7)*(elite?1.45f:1),false);enemies.Add(foe);AddEnemyBar(foe);
        }
        void ClearEncounter()
        {
            encounterStarted=false;
            if(encounter==3)
            {
                chapterCleared=true;Progress.checkpoint=4;Progress.formsUnlocked=Mathf.Max(Progress.formsUnlocked,Progress.chapter>=5?3:Progress.chapter>=3?2:1);Progress.potions=Mathf.Min(9,Progress.potions+2);Progress.points++;Energy=100;health.FullHeal();
                AddProp("Portal",extras[3],Map.Portal,2.5f);Objective=Goal();
                UnlockWeaponReward();SaveCheckpoint();Speak(DarklightStory.Conclusions[Progress.chapter]);if(Progress.chapter!=0&&Progress.chapter!=2)Notice("CAPITULO CONCLUIDO");
            }
            else {encounter++;Progress.checkpoint=encounter;health.Heal(health.MaxHealth*.18f);Energy=Mathf.Min(100,Energy+15);Objective=Goal();SaveCheckpoint();Notice("SELO LIBERTO - CHECKPOINT SALVO");}
        }
        void Interact()
        {
            Vector2 p=lab.Player.transform.position;
            if(chapterCleared&&Vector2.Distance(p,Map.Portal)<2){if(Progress.chapter==5){Open("Ending");return;}StopPower();SaveCheckpoint();Progress.chapter++;Progress.checkpoint=0;LoadMap(Progress.chapter);SaveCheckpoint();Speak(DarklightStory.Introductions[Progress.chapter]);return;}
            if(!Progress.relics[Progress.chapter]&&Vector2.Distance(p,Map.Relic)<1.7f)
            {Progress.relics[Progress.chapter]=true;Progress.shards++;Progress.Award(45);health.SetMaxHealth(Progress.MaxHealth);Progress.potions=Mathf.Min(9,Progress.potions+1);Notice("MEMORIA RECUPERADA: +45 XP, +1 ELIXIR");SaveCheckpoint();return;}
            if(Progress.chapter==1&&Vector2.Distance(p,Map.Ally)<2)
            {
                if(Progress.mp3Unlocked)Speak(new[]{"ICARO NOX|A musica vem de voce. O aparelho so abre o caminho. M ou SELECT abre seu MP3. C / RS ativa; 1-2 / direcional escolhe."});
                else if(Progress.mp3Quest==0){Progress.mp3Quest=1;Speak(new[]{"ICARO NOX|Meu MP3 capta o Veu. Os Ecos levaram tres componentes. Derrote-os e traga-os para mim.","FABIO|Vou recuperar seu equipamento. Quero saber o que essas frequencias escondem."});SaveCheckpoint();}
                else if(Progress.mp3Quest>=4){Progress.mp3Unlocked=true;Speak(new[]{"ICARO NOX|Aqui esta. Escolha a frequencia: Love regenera, Thrash concentra, Heavy fortalece.","FABIO|Vou usar o que me fortalece sem deixar de ser eu."});SaveCheckpoint();Sound(3);}
                else Speak(new[]{"ICARO NOX|Ainda faltam "+(4-Progress.mp3Quest)+" componentes. Os Ecos desta rua carregam as pecas."});return;
            }
            if(Progress.chapter!=1&&Vector2.Distance(p,Map.Ally)<2){Speak(new[]{(Progress.chapter==3||Progress.chapter==4?"KAEL|":"LYRA|")+"Siga os selos. "+Objective});return;}
            if(Vector2.Distance(p,Map.Spawn)<2&&!encounterStarted){health.FullHeal();SaveCheckpoint();Notice("CHECKPOINT SALVO - VIDA RESTAURADA");return;}
            Notice("Aproxime-se de um fragmento, aliado ou portal");
        }
        public bool RequestAttack(DarklightEnemy enemy)
        {float now=lab.Player.Model.Clock;if(now<attackPermit)return false;attackPermit=now+(enemy.Boss?.35f:.45f);return true;}
        public void EnemyDefeated(DarklightEnemy enemy)
        {
            int previous=Progress.level;Progress.Award(enemy.Boss?150:18+Progress.chapter*4);kills++;
            Energy=Mathf.Min(100,Energy+(enemy.Boss?30:8));Resonance.Gain(enemy.Boss?20:5);if(Progress.chapter==1&&Progress.mp3Quest>0&&Progress.mp3Quest<4)Progress.mp3Quest++;
            if(previous!=Progress.level){health.SetMaxHealth(Progress.MaxHealth);health.FullHeal();Notice("NIVEL "+Progress.level+" - +1 PONTO");Sound(3);}
        }
        public void Strike(StrikeKind kind)
        {bool varied=(int)kind!=lastAttack;style=Mathf.Min(450,style+(varied?24:7));lastAttack=(int)kind;idleStyle=0;Resonance.Gain(varied?2:1);Energy=Mathf.Min(100,Energy+(varied?5:2)*(resonance==2?1.2f:1));Sound(1);}
        public void BossPhaseTwo(){ambient.clip=tracks[3];}
        public void Defended(){style=Mathf.Min(450,style+40);Energy=Mathf.Min(100,Energy+8);idleStyle=0;Sound(0);}
        public void Damaged(){style=Mathf.Max(0,style-65);phonesTime=0;Sound(2);}
        void TransformPlayer()
        {
            if(Form>0){EndForm();return;}int unlocked=Progress.FormUnlocked;
            if(unlocked==0){Notice("RUPTURA DESPERTA APOS KAEL");return;}
            if(Energy<60){Notice("TRANSFORMACAO: PRECISA 60 DE ENERGIA");return;}
            Form=Progress.preferredForm>0?Mathf.Min(unlocked,Progress.preferredForm):unlocked;Energy-=60;FormTime=12+Progress.mastery*2;Notice(new[]{"","RUPTURA","ECLIPSE","ABSOLUTO"}[Form]);Sound(3);
        }
        void EndForm(){Form=0;FormTime=0;slowTime=0;}
        void FormAbility()
        {
            if(Form==0)return;
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
                foreach(var enemy in enemies)if(enemy!=null&&!enemy.Health.IsDead&&Vector2.Distance(enemy.transform.position,rb.position)<3.5f)enemy.GetComponent<DarklightCombatant>().Receive(24*DamageMultiplier,Vector2.up);
                Notice("PASSO DO ECLIPSE");
            }
            else
            {
                slowTime=2.5f;
                foreach(var enemy in enemies)if(enemy!=null&&!enemy.Health.IsDead&&Vector2.Distance(enemy.transform.position,lab.Player.transform.position)<5)enemy.GetComponent<DarklightCombatant>().Receive(34*DamageMultiplier,Vector2.up);
                Notice("JUIZO DO ECLIPSE");
            }
            Sound(3);
        }
        void UsePotion(){if(Progress.potions<=0){Notice("SEM ELIXIRES");return;}if(health.IsDead||health.HealthPercent>=1)return;Progress.potions--;health.Heal(health.MaxHealth*.45f);Sound(0);Notice("ELIXIR: +45% VIDA");SaveCheckpoint();}
        void SelectSong(int index)
        {
            if(!Progress.mp3Unlocked){Notice("DESBLOQUEIE COM ICARO NOX NO CAPITULO 2");return;}
            Resonance.Select(index<2?0:index<4?2:1);
            if(!Resonance.Ready||phonesTime>0){Notice("DARK RESONANCE EM RECARGA");return;}
            selectedTrack=index;Close();TryActivatePower();
        }
        public bool TryActivatePower()
        {
            if(!Progress.mp3Unlocked){Notice("DESBLOQUEIE O MP3 COM ICARO NOX");return false;}
            if(!Resonance.Ready||phonesTime>0){Notice("MODO EM RECARGA OU SEM ENERGIA");return false;}
            if(lab.Player.Model.State==CombatState.Dead||lab.Player.Model.State==CombatState.Hurt)return false;
            phonesTime=.28f;Notice("COLOCANDO FONES");return true;
        }
        void BeginResonance()
        {
            if(!Resonance.Activate())return;resonance=Resonance.Active;resonanceTime=Resonance.Remaining;
            if(resonance==0 && selectedTrack>1)selectedTrack=0;
            if(resonance==1)selectedTrack=4;
            if(resonance==2 && (selectedTrack<2||selectedTrack>3))selectedTrack=2;
            ambient.Pause();music.clip=tracks[selectedTrack];music.volume=MusicVolume;
            if(!DarklightAutomation.Silent&&music.clip!=null){music.time=0;music.Play();}
            Sound(resonance==0?0:resonance==1?2:3);Notice(DarklightResonance.Names[resonance]);
        }
        void StopPower(){Resonance.Stop();resonance=-1;phonesTime=0;EndForm();music.Stop();}
        void UpdateAura()
        {
            if(aura==null)return;
            if(headphones==null){headphones=new GameObject("Fones").AddComponent<SpriteRenderer>();headphones.transform.SetParent(lab.Player.transform,false);headphones.sprite=extras[8];headphones.sharedMaterial=pixelMaterial;headphones.transform.localScale=Vector3.one*.37f/headphones.sprite.bounds.size.y;}
            headphones.enabled=phonesTime>0||resonance>=0;headphones.sortingOrder=lab.Player.GetComponentInChildren<SpriteRenderer>().sortingOrder+1;headphones.transform.localPosition=Vector3.up*Mathf.Lerp(.75f,1.65f,1-Mathf.Clamp01(phonesTime/.28f));aura.enabled=Form>0||resonance>=0||phonesTime>0;
            float bpm=beatsPerMinute[selectedTrack];
            float beat=music.isPlaying?Mathf.Sin((music.time-beatOffsets[selectedTrack])*bpm/60*Mathf.PI*2):Mathf.Sin(lab.Player.Model.Clock*5);
            if(music.isPlaying && !DarklightAutomation.Silent)
            {
                music.GetOutputData(rhythmSamples,0);float peak=0;foreach(float sample in rhythmSamples)peak=Mathf.Max(peak,Mathf.Abs(sample));
                rhythmEnvelope=Mathf.Lerp(rhythmEnvelope,peak,Mathf.Clamp01(Time.deltaTime*18));beat=Mathf.Clamp01(rhythmEnvelope*5);
            }
            resonanceBlock ??= new MaterialPropertyBlock();resonanceBlock.SetFloat("_ResonanceTint",1);aura.SetPropertyBlock(resonanceBlock);
            float pulse=ReducedFlashes?1:1+.08f*beat;aura.transform.localScale=Vector3.one*(2.5f/aura.sprite.bounds.size.y)*pulse;
            aura.color=phonesTime>0?new Color(.6f,.7f,1,.45f):Form==3?new Color(1,.9f,.65f,.65f):Form==2?new Color(.7f,.3f,1,.7f):resonance>=0?new Color(DarklightResonance.Colors[resonance].r,DarklightResonance.Colors[resonance].g,DarklightResonance.Colors[resonance].b,.6f):new Color(1,.2f,.3f,.6f);
        }
        public void Fire(Vector3 position,Vector2 direction,float damage,float speed)
        {var obj=new GameObject("CorteDoVeu");obj.transform.position=position;var sprite=obj.AddComponent<SpriteRenderer>();sprite.sprite=extras[5];sprite.sharedMaterial=pixelMaterial;sprite.sortingOrder=140;obj.transform.localScale=Vector3.one*.38f/sprite.sprite.bounds.size.y;var projectile=obj.AddComponent<DarklightProjectile>();projectile.direction=direction;projectile.damage=damage;projectile.speed=speed;}
        public void Telegraph(Vector3 position,Vector2 direction,float radius,bool area)
        {var obj=new GameObject("Aviso");obj.transform.SetParent(levelRoot,false);obj.transform.position=position+(Vector3)(area?Vector2.zero:direction*.65f);var sprite=obj.AddComponent<SpriteRenderer>();sprite.sprite=extras[4];sprite.sharedMaterial=pixelMaterial;sprite.sortingOrder=80;sprite.color=new Color(1,.13f,.2f,.48f);obj.transform.localScale=Vector3.one*(radius/sprite.sprite.bounds.size.y);Destroy(obj,.04f);}
        public void Notice(string message){notice=message;noticeTime=3.5f;}
        void SaveCheckpoint()
        {if(DarklightAutomation.Silent)return;Resonance.Store(Progress);try{Progress.Save();}catch(Exception e){Notice("NAO FOI POSSIVEL SALVAR");Debug.LogError(e);}}
        void Speak(string[] lines,Action done=null){dialogue=lines;dialogueIndex=0;dialogueDone=done;Open("Dialogue");}
        void NextDialogue(){if(++dialogueIndex<dialogue.Length){RefreshPanel();return;}Close();var done=dialogueDone;dialogueDone=null;done?.Invoke();}
        public void Sound(int index){if(DarklightAutomation.Silent||tones==null||EffectsVolume<=0)return;sfx.PlayOneShot(tones[index],EffectsVolume*.2f);}
        static AudioClip Tone(float frequency,float duration)
        {int count=(int)(22050*duration);var data=new float[count];for(int i=0;i<count;i++){float t=i/22050f;data[i]=Mathf.Sin(t*frequency*Mathf.PI*2)*Mathf.Pow(1-(float)i/count,2)*.3f;}var clip=AudioClip.Create("RequiemSFX",count,1,22050,false);clip.SetData(data,0);return clip;}
        void OnDestroy(){if(Instance==this)Instance=null;rebinding?.Dispose();foreach(var tone in tones)if(tone!=null)Destroy(tone);}
    }
}
