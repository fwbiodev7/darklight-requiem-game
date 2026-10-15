using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        string panel="Main",returnPanel="Main";
        int menuDismissFrame=-1;public bool MenuDismissedThisFrame=>menuDismissFrame==Time.frameCount;
        readonly Dictionary<string,int> focusMemory=new();
        Text weaponText;Image weaponIcon;
        Text powerText,menuFooter;Image powerFill,powerFrame;int selection;float nextNav;int settingsDirection=1;
        RectTransform uiRoot,overlay,menuRoot;Text heading,bodyText,chapterText,energyText,objectiveText,bossText,messageText,statusText;
        Image energyFill,bossFill,portrait,menuBackdrop;Font font;
        Sprite UIArt(int index,Sprite fallback)=>uiArt!=null&&uiArt.Length>index?uiArt[index]:fallback;
        Sprite HUDArt(int index,Sprite fallback)=>hudArt!=null&&hudArt.Length>index?hudArt[index]:fallback;
        readonly List<(RectTransform rect,Text text,Image image,Action action,bool enabled)> buttons=new();
        InputActionRebindingExtensions.RebindingOperation rebinding;
        string rebindAction;
        readonly Dictionary<DarklightEnemy,(RectTransform rect,Image fill)> enemyBars=new();
        public void PauseForFocus(){if(Active)Open("Pause");}
        public void PreviewPanel(string name,int previewSelection=0)
        {
            if(name=="BindingDevice")rebindAction="Light";
            Open(name);selection=Mathf.Clamp(previewSelection,0,Mathf.Max(0,buttons.Count-1));
        }
        void CreateUI()
        {
            font=lab.PixelHUD.pixelFont;if(font.material.mainTexture!=null)font.material.mainTexture.filterMode=FilterMode.Point;uiRoot=lab.PixelHUD.transform.Find("HUD640x360") as RectTransform;
            PolishHUD();
            weaponIcon=ImageAt(uiRoot,"ArmaEquipada",12,310,24,24,weaponDefinitions[0].icon);weaponIcon.preserveAspect=true;weaponText=TextAt(uiRoot,"Arma",42,310,196,26,5);
            chapterText=TextAt(uiRoot,"Capitulo",264,31,112,10,5);chapterText.alignment=TextAnchor.MiddleCenter;
            energyText=TextAt(uiRoot,"Energia",12,78,168,10,5);energyText.alignment=TextAnchor.MiddleCenter;
            energyFill=ImageAt(uiRoot,"EnergiaBarra",12,90,132,2,null);energyFill.color=new Color(.45f,.15f,.75f);
            objectiveText=TextAt(uiRoot,"Objetivo",208,10,292,18,5);objectiveText.alignment=TextAnchor.UpperCenter;objectiveText.horizontalOverflow=HorizontalWrapMode.Wrap;
            statusText=TextAt(uiRoot,"Progresso",12,64,188,10,5);statusText.alignment=TextAnchor.UpperLeft;
            bossText=TextAt(uiRoot,"Chefe",210,66,220,12,6);bossText.alignment=TextAnchor.MiddleCenter;
            bossFill=ImageAt(uiRoot,"VidaChefe",210,80,220,3,null);bossFill.color=new Color(.7f,.03f,.15f);
            messageText=TextAt(uiRoot,"Aviso",140,278,360,18,6);messageText.alignment=TextAnchor.MiddleCenter;
            var mode=uiRoot.Find("Modo");if(mode!=null)mode.GetComponent<Text>().text="";
            overlay=RectAt(uiRoot,"Menus",0,0,640,360);menuBackdrop=overlay.gameObject.AddComponent<Image>();menuBackdrop.color=new Color(.02f,.02f,.04f,.88f);menuBackdrop.raycastTarget=false;
            powerFrame=ImageAt(uiRoot,"MP3Compacto",432,302,196,46,HUDArt(2,extras[7]));powerFrame.type=Image.Type.Sliced;powerFrame.pixelsPerUnitMultiplier=8.333333f;
            ImageAt(powerFrame.transform,"MP3",7,7,22,28,extras[2]).preserveAspect=true;
            powerText=TextAt(powerFrame.transform,"Modo",36,7,151,30,6);powerText.horizontalOverflow=HorizontalWrapMode.Wrap;
            powerFill=ImageAt(powerFrame.transform,"Energia",36,37,145,3,null);
            menuRoot=RectAt(overlay,"Painel",24,16,592,328);
            var frame=ImageAt(menuRoot,"Moldura",0,0,592,328,UIArt(0,extras[6]));
            frame.color=Color.white;frame.type=Image.Type.Sliced;frame.pixelsPerUnitMultiplier=8.333333f;
            heading=TextAt(menuRoot,"Titulo",22,18,548,28,10);heading.alignment=TextAnchor.MiddleLeft;
            bodyText=TextAt(menuRoot,"Texto",334,155,232,132,7);bodyText.horizontalOverflow=HorizontalWrapMode.Wrap;bodyText.verticalOverflow=VerticalWrapMode.Truncate;bodyText.alignment=TextAnchor.UpperCenter;
            portrait=ImageAt(menuRoot,"Retrato",367,62,160,82,extras[0]);portrait.preserveAspect=true;
            menuFooter=TextAt(menuRoot,"Navegacao",22,305,548,12,6);menuFooter.text="SETAS / LS: ESCOLHER    ENTER / A: OK    ESC / B: VOLTAR";
            CreateCinematicMenu();CreateMinimalHUD();InitializeMenuPolish();overlay.SetAsLastSibling();
        }
        void LateUpdate()
        {
            if(uiRoot==null)return;
            TickMenuPolish();UpdateEnemyBars();
            if(Weapons?.Equipped!=null){weaponIcon.sprite=Weapons.Equipped.icon;weaponText.text=Weapons.Equipped.displayName+"  "+lab.Input.BindingLabel("WeaponNext")+"\n"+(Weapons.Equipped.id==DarklightWeaponId.Katana?lab.Input.BindingLabel("Skill")+": ONDA":lab.Player.Model.WeaponSpecialRemaining>0?"ESPECIAL: "+lab.Player.Model.WeaponSpecialRemaining.ToString("0.0")+"s":lab.Input.BindingLabel("WeaponSpecial")+": ESPECIAL");}
            chapterText.text=(ExploringNewRegion?"REGIÃO "+(RegionChapter+1):"CAP. "+(Progress.chapter+1))+"  "+Rank;
            energyText.text=(Form>0?new[]{"","RUPTURA","ECLIPSE","ABSOLUTO"}[Form]+" "+FormTime.ToString("0"):"SANGUE "+Energy.ToString("0"));
            energyFill.rectTransform.sizeDelta=new Vector2(132*Mathf.Clamp01(Energy/100),2);
            Vector2 goal=ExploringNewRegion?RegionGoal:chapterCleared?Map.Portal:Map.Arenas[encounter];Vector2 delta=goal-(Vector2)lab.Player.transform.position;
            string compass=new[]{"S","SO","O","NO","N","NE","L","SE"}[DarklightCharacterView.DirectionIndex(delta)];
            objectiveText.text=ExploringNewRegion?"MEMÓRIAS "+VisitedRegionAreas+"/"+Map.Art.areas.Length+"  "+compass+"  "+delta.magnitude.ToString("0")+"m":encounterStarted?Objective:(chapterCleared?"PORTAL":encounter==3?"CHEFE":"SELO "+(encounter+1)+"/3")+"  "+compass+"  "+delta.magnitude.ToString("0")+"m";statusText.text="NV "+Progress.level+"  XP "+Progress.xp+"/"+Progress.NeededXP+"  "+lab.Input.BindingLabel("Potion")+":"+Progress.potions;
            int selected=Resonance.Selected;float cooldown=Resonance.Recovery[selected];
            powerFrame.gameObject.SetActive(Active&&Progress.mp3Unlocked);powerFill.color=DarklightResonance.Colors[selected];powerFill.rectTransform.sizeDelta=new Vector2(145*Resonance.Energy/100,3);
            powerText.text=DarklightResonance.Names[selected]+"\n"+(Resonance.Active==selected?Resonance.Remaining.ToString("0.0")+"s ATIVO":cooldown>0?cooldown.ToString("0")+"s RECARGA":lab.Input.BindingLabel("Power")+": ATIVAR")+"   "+Resonance.Energy.ToString("0")+"%\n1/2 OU DIRECIONAL: MODO";
            messageText.text=noticeTime>0?notice:"";
            var boss=CurrentBoss;bossText.text=boss==null?"":DarklightStory.Bosses[Progress.chapter];bossFill.gameObject.SetActive(boss!=null);
            if(boss!=null)bossFill.rectTransform.sizeDelta=new Vector2(220*boss.Health.HealthPercent,3);
            music.volume=MusicVolume;ambient.volume=AmbientVolume*(CurrentBoss!=null?.7f:1);TickMinimalHUD();TickPlaytestGuide();TickTechnicalHUD();
        }
        void TickUI()
        {
            if(uiRoot==null)return;
            if(phaseLoading){TickLoadingScreen();return;}
            if(bindingWaiting||rebinding!=null){TickBindingCapture();return;}
            if(Time.unscaledTime<captureAfter)return;
            TickInteractionInput();if(atlas!=null&&!Training&&atlas.TickInput())return;var input=lab.Input;
            bool emergencyPause=Keyboard.current?.escapeKey.wasPressedThisFrame==true;
            foreach(var pad in Gamepad.all)emergencyPause|=pad.startButton.wasPressedThisFrame;
            if(input.Menu("Pause").WasPressedThisFrame()||emergencyPause)
            {if(Active){Open("Pause");return;}if(panel=="Pause"){Close();return;}}
            if(Active)return;
            if(panel=="Dialogue"){TickDialogueInput();return;}
            Vector2 nav=input.Menu("Navigate").ReadValue<Vector2>();
            if(nav.sqrMagnitude<.15f)nextNav=0;
            else if(Time.unscaledTime>=nextNav)
            {
                if(Mathf.Abs(nav.y)>.4f && buttons.Count>0)
                {MoveSelection(nav.y>0?-1:1);MenuSound();}
                else if(Mathf.Abs(nav.x)>.4f&&panel=="Options")
                {if(selection<6){selection=6;}else{settingsDirection=nav.x<0?-1:1;Activate(selection);settingsDirection=1;}}
                nextNav=Time.unscaledTime+.2f;
            }
            if(input.Menu("Cancel").WasPressedThisFrame())
            {
                if(panel=="Training"){Close();return;}if(panel=="TrainingEnemies"||panel=="TrainingForms"){Open("Training");return;}if(panel=="CombatOptions"){Open("Pause");return;}if(panel=="Difficulty"){Open(difficultyForNewGame?"Main":Training?"Training":"CombatOptions");return;}
                if(panel=="Pause")Close();else if(panel=="Dialogue"){dialogueIndex=dialogue.Length-1;NextDialogue();}
                else if(panel!="Main"&&panel!="Death"&&panel!="Ending")Open(panel=="Victory"||panel=="LoadGames"?"Main":panel=="Skins"?returnPanel:panel=="BindingSlot"||panel=="BindingDevice"?"Controls":panel=="BindingConflict"?"BindingSlot":panel=="Transformations"?"Pause":panel=="Controls"||panel=="Extras"?"Options":panel=="RegionDetails"?"Regions":panel=="Regions"?regionMenuReturn:returnPanel);return;
            }
            if(input.Menu("Submit").WasPressedThisFrame())Activate(selection);
            if(Mouse.current!=null)
            {
                Vector2 mouse=Mouse.current.position.ReadValue();bool click=Mouse.current.leftButton.wasPressedThisFrame;
                if(panel=="Controls"&&Mathf.Abs(Mouse.current.scroll.y.ReadValue())>.01f)MoveSelection(Mouse.current.scroll.y.ReadValue()>0?-1:1);
                bool dragging=TickAudioDrag(mouse);
                if(!dragging)for(int i=0;i<buttons.Count;i++)if(RectTransformUtility.RectangleContainsScreenPoint(buttons[i].rect,mouse))
                {if(buttons[i].enabled&&(click||Mouse.current.delta.ReadValue().sqrMagnitude>0)&&selection!=i){selection=i;MenuSound();}if(!TickSettingsPointer(i,mouse,click)&&click){Activate(i);break;}}
            }
            int first=Mathf.Max(0,selection-7);
            for(int i=0;i<buttons.Count;i++)StyleCinematicButton(i,first);
            TickCinematicMenu();
        }
        void MoveSelection(int direction)
        {for(int step=0;step<buttons.Count;step++){selection=(selection+direction+buttons.Count)%buttons.Count;if(buttons[selection].enabled)break;}}
        void PolishHUD()
        {
            if(hudPortrait!=null){RestoreMinimalHUDVisibility();return;}
            void Place(string name,float x,float y,float w,float h,int size=0)
            {var child=uiRoot.Find(name) as RectTransform;if(child==null)return;child.anchoredPosition=new Vector2(x,-y);child.sizeDelta=new Vector2(w,h);if(size>0&&child.TryGetComponent<Text>(out var t))t.fontSize=size;}
            Place("VidaMoldura",10,8,168,53);Place("Nome",57,17,102,9,6);Place("Vida",57,30,76,10);Place("VidaValor",57,31,76,9,5);
            var life=uiRoot.Find("VidaMoldura").GetComponent<Image>();life.sprite=HUDArt(0,life.sprite);life.type=Image.Type.Simple;
            Place("HabilidadeMoldura",504,91,128,44);Place("HabilidadeNome",546,100,76,8,5);Place("HabilidadeProntidao",541,111,62,8);Place("HabilidadeTempo",541,112,62,7,5);
            var skill=uiRoot.Find("HabilidadeMoldura").GetComponent<Image>();skill.sprite=HUDArt(1,skill.sprite);skill.type=Image.Type.Simple;
            foreach(string name in new[]{"Katana","Leve","LeveBotao","Parry","Defesa","ParryBotao","Esquiva","EsquivaNome","EsquivaBotao","HabilidadeBotao","Modo"}){var item=uiRoot.Find(name);if(item!=null)item.gameObject.SetActive(false);}
            Place("Ajuda",12,338,408,10,6);
        }
        void Activate(int i){if(i<0||i>=buttons.Count||!buttons[i].enabled)return;MenuSound();buttons[i].action();}
        void Open(string value)
        {
            if(value=="Main")StartMenuMusic();
            if(value=="Regions"&&panel!="RegionDetails")regionMenuReturn=panel=="Extras"?"Extras":"Pause";
            if(value=="Skins"&&panel!="Skins")returnPanel=panel=="Main"?"Main":"Pause";
            if(value=="LoadGames")returnPanel="Main";
            if(value=="Options"||value=="Credits")returnPanel=panel=="Main"||panel=="Victory"?"Main":"Pause";
            else if(value=="VampireSkills"||value=="Skills"||value=="Inventory"||value=="Journal"||value=="MP3")returnPanel="Pause";
            focusMemory[panel]=selection;panel=value;selection=focusMemory.TryGetValue(value,out var savedFocus)?savedFocus:0;nextNav=Time.unscaledTime+.15f;lab.SetPaused(true);AudioDirector?.StopCombat();music.Pause();ambient.Pause();RefreshPanel();
        }
        void Close(){menuDismissFrame=Time.frameCount;StopMenuMusic();panel="";SetHUDVisible(true);overlay.gameObject.SetActive(false);lab.SetPaused(false);if(!DarklightAutomation.Silent&&(resonance>=0||HasFirstAwakeningMusic))music.UnPause();}
        void SetHUDVisible(bool value){foreach(Transform child in uiRoot)if(child.name!="Mundo640x360"&&child.name!="Menus"&&child.name!="PassagemDeFase")child.gameObject.SetActive(value);if(loadingRoot!=null)loadingRoot.gameObject.SetActive(phaseLoading);if(value)PolishHUD();overlay.SetAsLastSibling();}
        void ClearButtons(){foreach(var b in buttons)Destroy(b.rect.gameObject);buttons.Clear();menuBorders.Clear();}
        void Button(string label,Action action,bool enabled=true)
        {
            int i=buttons.Count;float y=62+i*28;
            var image=ImageAt(menuRoot,"Botao"+i,22,y,286,24,UIArt(enabled?1:3,extras[7]));image.color=Color.white;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=8.333333f;
            var text=TextAt(image.transform,"Rotulo",8,0,270,24,6);text.text=label;text.alignment=TextAnchor.MiddleCenter;
            buttons.Add((image.rectTransform,text,image,action,enabled));
            if(CinematicFront||CinematicPause)AddCinematicBorder(image,text,i);
        }
        void RefreshPanel()
        {
            if(panel=="Dialogue"){ShowDialogue();return;}
            if(dialogueRoot!=null)dialogueRoot.gameObject.SetActive(false);
            menuRoot.gameObject.SetActive(true);FlushAudioPreferences();draggedAudioSlider=-1;ClearButtons();settingsSliders.Clear();SetHUDVisible(!(panel=="Main"||panel=="Skins"||panel=="ConfirmNew"||((panel=="Options"||panel=="Controls"||panel=="Credits"||panel=="BindingDevice")&&returnPanel=="Main")));overlay.gameObject.SetActive(true);portrait.gameObject.SetActive(false);
            bool front=CinematicFront;
            menuRoot.Find("Moldura").GetComponent<Image>().color=front?new Color(1,1,1,.72f):Color.white;heading.fontSize=10;heading.rectTransform.sizeDelta=new Vector2(548,24);heading.alignment=TextAnchor.UpperLeft;
            menuBackdrop.sprite=front?menuBackground:null;menuBackdrop.color=front&&menuBackground!=null?Color.white:new Color(.02f,.02f,.04f,.88f);
            bodyText.fontSize=7;bodyText.rectTransform.anchoredPosition=new Vector2(334,-155);bodyText.rectTransform.sizeDelta=new Vector2(232,132);bodyText.alignment=TextAnchor.UpperLeft;portrait.rectTransform.anchoredPosition=new Vector2(367,-62);portrait.rectTransform.sizeDelta=new Vector2(160,82);
            switch(panel)
            {
                case "Main":BuildCinematicMain();break;
                case "Difficulty":BuildDifficulty();break;
                case "CombatOptions":BuildCombatOptions();break;
                case "Training":BuildTraining();break;
                case "TrainingEnemies":BuildTrainingEnemies();break;
                case "TrainingForms":BuildTrainingForms();break;
                case "Regions":ShowRegions();break;
                case "RegionDetails":ShowRegionDetails();break;
                case "LoadGames":BuildSavePicker();break;
                case "Extras":
                    heading.text="EXTRAS";bodyText.text="Treine o combate ou explore as novas regiões.";Button("LABORATORIO DE COMBATE",()=>UnityEngine.SceneManagement.SceneManager.LoadScene("DarklightCombatLab"));Button("EXPLORAR NOVAS REGIÕES",()=>Open("Regions"));Button("VOLTAR",()=>Open("Options"));break;
                case "Skins":BuildVisualPicker();break;
                case "ConfirmNew":heading.text="NOVO JOGO";bodyText.text="Iniciar uma nova campanha substitui o save atual.\nA copia anterior fica no arquivo .bak.";Button("INICIAR CAMPANHA",NewGame);Button("VOLTAR",()=>Open("Main"));break;
                case "Pause":
                    if(ExploringNewRegion){heading.text="EXPLORAÇÃO";bodyText.text=Map.Art.title;Button("RETOMAR",Close);Button("ESCOLHER REGIÃO",()=>Open("Regions"));Button("MAPA DA FASE",()=>atlas.TryOpen());Button("OPÇÕES",()=>Open("Options"));Button("MENU PRINCIPAL",()=>Open("Main"));break;}
                    heading.text="PAUSA";bodyText.text=DarklightStory.Chapters[Progress.chapter]+"\n"+DarklightStory.Places[Progress.chapter];
                    Button("RETOMAR",Close);Button("COMBATE E TREINO",()=>Open(Training?"Training":"CombatOptions"));Button("TRANSFORMACOES",()=>Open("Transformations"));Button("HABILIDADES",()=>Open("Skills"));Button("INVENTARIO",()=>Open("Inventory"));Button("DIARIO",()=>Open("Journal"));Button("DARK RESONANCE",()=>Open("MP3"));Button("TROCAR SKIN",()=>Open("Skins"));Button("MAPA DA FASE",()=>atlas.TryOpen());Button("OPCOES",()=>Open("Options"));Button("MENU PRINCIPAL",()=>{if(Training){trainingFromMain=true;ExitTraining();}else{StopPower();SaveCheckpoint();Open("Main");}});if(PlaytestSession)Button("FERRAMENTAS DE TESTE",()=>Open("Playtest"));break;
                case "Playtest":BuildPlaytestTools();break;
                case "Skills":BuildSkillPaths();break;
                case "VampireSkills":BuildVampireSkills();break;
                case "Inventory":
                    heading.text="INVENTARIO";bodyText.text="V / LS: trocar armas\nPresas: apos Kael | Corrente: apos Matriarca\nELIXIRES: "+Progress.potions+" | MEMORIAS: "+Progress.shards+"/10\nMP3: "+(Progress.mp3Unlocked?"DESBLOQUEADO":"MISSAO FREQUENCIA DO VEU");
                    Button("USAR ELIXIR (+45% VIDA)",()=>{UsePotion();RefreshPanel();},Progress.potions>0&&health.HealthPercent<1&&!health.IsDead);for(int i=0;i<weaponDefinitions.Length;i++){int index=i;Button(weaponDefinitions[i].displayName+(Weapons.Equipped==weaponDefinitions[i]?" - EQUIPADA":""),()=>{Weapons.Request(index);Close();},Weapons.Unlocked(i));}Button("VOLTAR",()=>Open("Pause"));break;
                case "Journal":
                    heading.text="DIARIO";bodyText.text=Objective+"\n\n"+(Progress.mp3Unlocked?"Frequencia do Veu concluida.":Progress.mp3Quest>0?"Frequencia do Veu: "+Mathf.Min(3,Progress.mp3Quest-1)+"/3 componentes. Volte a Icaro.":"Icaro Nox espera no distrito do vinil (cap. 2).")+"\nMemorias opcionais: aproxime-se dos fragmentos e use E / A.";
                    Button("VOLTAR",()=>Open("Pause"));break;
                case "MP3":
                    heading.text="DARK RESONANCE";bodyText.text=!Progress.mp3Unlocked?"Recupere tres componentes para Icaro Nox\nno capitulo 2. A campanha funciona sem o MP3.":resonance>=0?"Modo ativo - "+resonanceTime.ToString("0")+"s restantes":resonanceCooldown>0?"RECARGA: "+resonanceCooldown.ToString("0")+"s":"LOVE: cura 1.4%/s, 18s\nFORCA: dano +45%, 12s\nFOCO: ataque +40%, 14s\nRecargas: 32/28/30s\nC/RS ativa; 1/2 escolhe";
                    portrait.gameObject.SetActive(true);portrait.sprite=extras[2];portrait.rectTransform.anchoredPosition=new Vector2(367,-62);
                    string[] names={"LOVE - JOIN ME IN DEATH","LOVE - WINGS OF A BUTTERFLY","THRASH - SEEK & DESTROY","THRASH - CREEPING DEATH","HEAVY - ENTER SANDMAN"};
                    for(int i=0;i<5;i++){int index=i;Button(names[i],()=>SelectSong(index),Progress.mp3Unlocked&&resonance<0&&phonesTime<=0&&Resonance.Recovery[index<2?0:index<4?2:1]<=0&&Resonance.Energy>=DarklightResonance.Costs[index<2?0:index<4?2:1]);}Button("VOLTAR",()=>Open("Pause"));break;
                case "Options":BuildSettings();break;
                case "Controls":BuildControls();break;
                case "BindingDevice":BuildControls();break;
                case "BindingSlot":BuildBindingSlot();break;
                case "BindingConflict":BuildBindingConflict();break;
                case "Transformations":BuildTransformations();break;
                case "Death":if(Training){health.SetMaxHealth(health.MaxHealth,false);lab.Player.ResetModel();Open("Training");return;}heading.text="O VEU TE ALCANCOU";bodyText.text="Retome do ultimo selo libertado.\nAs habilidades e a campanha continuam suas.";Button("RETOMAR CHECKPOINT",()=>{StopPower();LoadMap(Progress.chapter);Close();});Button("MENU PRINCIPAL",()=>Open("Main"));break;
                case "Ending":BuildEndings();break;
                case "Victory":
                    heading.text="REQUIEM CONCLUIDO";bodyText.text=EndingDescription;
                    bodyText.text+="\nMemorias: "+Progress.shards+"/10 | Nivel: "+Progress.level;if(Progress.legacyFinaleCompleted&&Progress.chapter<=5)Button("CONTINUAR: HERANÇA CARMESIM",ResumeLegacyExpansion);Button("CREDITOS",()=>Open("Credits"));Button("REVISITAR CAPITULOS",()=>Open("ChapterSelect"));Button("MENU PRINCIPAL",()=>Open("Main"));break;
                case "ChapterSelect":heading.text="REVISITAR VESPERA";bodyText.text="Rejogue com seus atributos atuais.\nO save concluido e preservado ate escolher uma fase.";for(int i=0;i<DarklightStory.Chapters.Length;i++){int chapter=i;Button((i+1)+" - "+DarklightStory.Chapters[i],()=>TravelToChapter(chapter));}Button("VOLTAR",()=>Open("Victory"));break;
                case "Credits":heading.text="CREDITOS";bodyText.text="Criacao e protagonista: Fabio\nDesenvolvimento assistido por Codex\nPixel art: segredo-varginha-pixelart + geracao assistida\nFonte: Press Start 2P, CodeMan38 (OFL)\nMP3 pessoais: HIM e Metallica | devfabio.vercel.app\nSFX CC0: Kenney, JaggedStone e Thimras\nLicenças: AUDIO-LICENSES.json";Button("VOLTAR",()=>Open(returnPanel));break;
            }
            RefreshAppearancePortrait();LayoutCinematicPanel(front);selection=Mathf.Clamp(selection,0,Mathf.Max(0,buttons.Count-1));if(buttons.Count>0&&!buttons[selection].enabled)MoveSelection(1);
        }

        void AddEnemyBar(DarklightEnemy enemy)
        {
            if(enemy.Boss)return;var frame=ImageAt(uiRoot,"EcoVida",0,0,52,10,enemyBarFrame);
            var fill=ImageAt(frame.transform,"Vida",6,4,40,3,null);fill.color=new Color(.7f,.06f,.15f);
            frame.transform.SetSiblingIndex(1);enemyBars.Add(enemy,(frame.rectTransform,fill));
        }
        void UpdateEnemyBars()
        {
            var removed=new List<DarklightEnemy>();
            foreach(var item in enemyBars)
            {
                if(item.Key==null){Destroy(item.Value.rect.gameObject);removed.Add(item.Key);continue;}
                Vector3 p=lab.WorldCamera.WorldToScreenPoint(item.Key.transform.position+Vector3.up*(DarklightWorldScale.EnemyHeight(item.Key.kind)+.15f));p.x*=640f/lab.WorldCamera.pixelWidth;p.y*=360f/lab.WorldCamera.pixelHeight;
                bool visible=Active&&!item.Key.Health.IsDead&&(item.Key.Health.HealthPercent<1||Vector2.Distance(item.Key.transform.position,lab.Player.transform.position)<2.2f)&&p.x>26&&p.x<614&&p.y>45&&p.y<290;
                item.Value.rect.gameObject.SetActive(visible);item.Value.rect.anchoredPosition=new Vector2(Mathf.Round(p.x)-26,-Mathf.Round(360-p.y));
                item.Value.fill.rectTransform.sizeDelta=new Vector2(40*item.Key.Health.HealthPercent,3);
            }
            foreach(var enemy in removed)enemyBars.Remove(enemy);
        }

        void Upgrade(int stat)
        {if(Progress.points<=0)return;Progress.points--;if(stat==0)Progress.attack++;else if(stat==1){Progress.vitality++;health.SetMaxHealth(Progress.MaxHealth);}else Progress.mastery++;SaveCheckpoint();RefreshPanel();}
        void Finish(int ending){StopPower();Progress.ending=ending;Progress.completed=true;SaveCheckpoint();Open("Victory");Sound(3);}
        string ActionLabel(string name)=>name switch{"Move"=>"MOVER","Potion"=>"ELIXIR","Journal"=>"DIARIO","Pause"=>"PAUSAR","MapPan"=>"MOVER MAPA","MapCursor"=>"CURSOR DO MAPA","MapZoomIn"=>"APROXIMAR MAPA","MapZoomOut"=>"AFASTAR MAPA","MapCenter"=>"CENTRALIZAR MAPA","MapMark"=>"MARCAR MAPA","MapClear"=>"APAGAR MARCA","MapObjective"=>"OBJETIVO","Interact"=>"INTERAGIR","Light"=>"LEVE","Heavy"=>"PESADO","Dodge"=>"ESQUIVA","Parry"=>"PARRY","Skill"=>"ONDA","Transform"=>"TRANSFORMAR","MP3"=>"MP3","Map"=>"MAPA DA FASE","Power"=>"ATIVAR PODER","PowerNext"=>"PROXIMO PODER","PowerPrevious"=>"PODER ANTERIOR","WeaponNext"=>"TROCAR ARMA","Aerial"=>"ATAQUE AEREO","WeaponSpecial"=>"ESPECIAL DA ARMA","Vampire"=>"PODER DE SANGUE","VampireNext"=>"PROXIMO PODER DE SANGUE",_=>name};
        RectTransform RectAt(Transform parent,string name,float x,float y,float w,float h)
        {var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);return rect;}
        Image ImageAt(Transform parent,string name,float x,float y,float w,float h,Sprite sprite)
        {var image=RectAt(parent,name,x,y,w,h).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;if(sprite!=null)image.material=pixelMaterial;return image;}
        Text TextAt(Transform parent,string name,float x,float y,float w,float h,int size)
        {var text=RectAt(parent,name,x,y,w,h).gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.color=new Color(.94f,.9f,.87f);text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Overflow;text.verticalOverflow=VerticalWrapMode.Overflow;var outline=text.gameObject.AddComponent<Outline>();outline.effectColor=Color.black;outline.effectDistance=new Vector2(1,-1);return text;}
    }
}
