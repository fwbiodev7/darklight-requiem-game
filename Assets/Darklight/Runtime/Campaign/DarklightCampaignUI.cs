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
        }
        void LateUpdate()
        {
            if(uiRoot==null)return;
            UpdateEnemyBars();
            if(Weapons?.Equipped!=null){weaponIcon.sprite=Weapons.Equipped.icon;weaponText.text=Weapons.Equipped.displayName+"  V / LS\n"+(Weapons.Equipped.id==DarklightWeaponId.Katana?"F / RB: ONDA":lab.Player.Model.WeaponSpecialRemaining>0?"ESPECIAL: "+lab.Player.Model.WeaponSpecialRemaining.ToString("0.0")+"s":"Z / D-PAD BAIXO: ESPECIAL");}
            chapterText.text="CAP. "+(Progress.chapter+1)+"  "+Rank;
            energyText.text=(Form>0?new[]{"","RUPTURA","ECLIPSE","ABSOLUTO"}[Form]+" "+FormTime.ToString("0"):"ENERGIA "+Energy.ToString("0"));
            energyFill.rectTransform.sizeDelta=new Vector2(132*Mathf.Clamp01(Energy/100),2);
            Vector2 goal=chapterCleared?Map.Portal:Map.Arenas[encounter];Vector2 delta=goal-(Vector2)lab.Player.transform.position;
            string compass=new[]{"S","SO","O","NO","N","NE","L","SE"}[DarklightCharacterView.DirectionIndex(delta)];
            objectiveText.text=encounterStarted?Objective:(chapterCleared?"PORTAL":encounter==3?"CHEFE":"SELO "+(encounter+1)+"/3")+"  "+compass+"  "+delta.magnitude.ToString("0")+"m";statusText.text="NV "+Progress.level+"  XP "+Progress.xp+"/"+Progress.NeededXP+"  R:"+Progress.potions;
            int selected=Resonance.Selected;float cooldown=Resonance.Recovery[selected];
            powerFrame.gameObject.SetActive(Active&&Progress.mp3Unlocked);powerFill.color=DarklightResonance.Colors[selected];powerFill.rectTransform.sizeDelta=new Vector2(145*Resonance.Energy/100,3);
            powerText.text=DarklightResonance.Names[selected]+"\n"+(Resonance.Active==selected?Resonance.Remaining.ToString("0.0")+"s ATIVO":cooldown>0?cooldown.ToString("0")+"s RECARGA":"C / RS: ATIVAR")+"   "+Resonance.Energy.ToString("0")+"%\n1/2 OU DIRECIONAL: MODO";
            messageText.text=noticeTime>0?notice:"";
            var boss=CurrentBoss;bossText.text=boss==null?"":DarklightStory.Bosses[Progress.chapter];bossFill.gameObject.SetActive(boss!=null);
            if(boss!=null)bossFill.rectTransform.sizeDelta=new Vector2(220*boss.Health.HealthPercent,3);
            music.volume=MusicVolume;ambient.volume=MusicVolume*(CurrentBoss!=null?.6f:.4f);
        }
        void TickUI()
        {
            if(uiRoot==null)return;var input=lab.Input;
            if(rebinding!=null)return;
            if(input.Menu("Pause").WasPressedThisFrame())
            {if(Active){Open("Pause");return;}if(panel=="Pause"){Close();return;}}
            if(Active)return;
            Vector2 nav=input.Menu("Navigate").ReadValue<Vector2>();
            if(nav.sqrMagnitude<.15f)nextNav=0;
            else if(Time.unscaledTime>=nextNav)
            {
                if(Mathf.Abs(nav.y)>.4f && buttons.Count>0)
                {MoveSelection(nav.y>0?-1:1);Sound(0);}
                else if(Mathf.Abs(nav.x)>.4f&&panel=="Options"&&selection<3)
                {settingsDirection=nav.x<0?-1:1;Activate(selection);settingsDirection=1;}
                nextNav=Time.unscaledTime+.2f;
            }
            if(input.Menu("Cancel").WasPressedThisFrame())
            {
                if(panel=="Pause")Close();else if(panel=="Dialogue"){dialogueIndex=dialogue.Length-1;NextDialogue();}
                else if(panel!="Main"&&panel!="Death"&&panel!="Ending")Open(panel=="Victory"?"Main":returnPanel);return;
            }
            if(input.Menu("Submit").WasPressedThisFrame())Activate(selection);
            if(Mouse.current!=null)
            {
                Vector2 mouse=Mouse.current.position.ReadValue();bool click=Mouse.current.leftButton.wasPressedThisFrame;
                for(int i=0;i<buttons.Count;i++)if(RectTransformUtility.RectangleContainsScreenPoint(buttons[i].rect,mouse))
                {if(buttons[i].enabled&&(click||Mouse.current.delta.ReadValue().sqrMagnitude>0))selection=i;if(click){Activate(i);break;}}
            }
            int first=Mathf.Max(0,selection-7);
            for(int i=0;i<buttons.Count;i++){var b=buttons[i];b.rect.gameObject.SetActive(i>=first&&i<first+8);b.rect.anchoredPosition=new Vector2(22,-(62+(i-first)*28));b.image.sprite=UIArt(!b.enabled?3:i==selection?2:1,extras[7]);b.image.color=Color.white;b.text.color=b.enabled?new Color(.96f,.9f,.86f):Color.gray;}
        }
        void MoveSelection(int direction)
        {for(int step=0;step<buttons.Count;step++){selection=(selection+direction+buttons.Count)%buttons.Count;if(buttons[selection].enabled)break;}}
        void PolishHUD()
        {
            void Place(string name,float x,float y,float w,float h,int size=0)
            {var child=uiRoot.Find(name) as RectTransform;if(child==null)return;child.anchoredPosition=new Vector2(x,-y);child.sizeDelta=new Vector2(w,h);if(size>0&&child.TryGetComponent<Text>(out var t))t.fontSize=size;}
            Place("VidaMoldura",10,8,168,53);Place("Nome",57,17,102,9,6);Place("Vida",57,30,76,10);Place("VidaValor",57,31,76,9,5);
            var life=uiRoot.Find("VidaMoldura").GetComponent<Image>();life.sprite=HUDArt(0,life.sprite);life.type=Image.Type.Simple;
            Place("HabilidadeMoldura",504,8,128,44);Place("HabilidadeNome",546,17,76,8,5);Place("HabilidadeProntidao",541,28,62,8);Place("HabilidadeTempo",541,29,62,7,5);
            var skill=uiRoot.Find("HabilidadeMoldura").GetComponent<Image>();skill.sprite=HUDArt(1,skill.sprite);skill.type=Image.Type.Simple;
            foreach(string name in new[]{"Katana","Leve","LeveBotao","Parry","Defesa","ParryBotao","Esquiva","EsquivaNome","EsquivaBotao","HabilidadeBotao","Modo"}){var item=uiRoot.Find(name);if(item!=null)item.gameObject.SetActive(false);}
            Place("Ajuda",12,338,408,10,6);
        }
        void Activate(int i){if(i<0||i>=buttons.Count||!buttons[i].enabled)return;Sound(0);buttons[i].action();}
        void Open(string value)
        {
            if(value=="Options"||value=="Credits")returnPanel=panel=="Main"||panel=="Victory"?"Main":"Pause";
            else if(value=="Skills"||value=="Inventory"||value=="Journal"||value=="MP3")returnPanel="Pause";
            focusMemory[panel]=selection;panel=value;selection=focusMemory.TryGetValue(value,out var savedFocus)?savedFocus:0;nextNav=Time.unscaledTime+.15f;lab.SetPaused(true);music.Pause();ambient.Pause();RefreshPanel();
        }
        void Close(){panel="";SetHUDVisible(true);overlay.gameObject.SetActive(false);lab.SetPaused(false);if(!DarklightAutomation.Silent&&resonance>=0)music.UnPause();}
        void SetHUDVisible(bool value){foreach(Transform child in uiRoot)if(child.name!="Mundo640x360"&&child.name!="Menus")child.gameObject.SetActive(value);if(value)PolishHUD();}
        void ClearButtons(){foreach(var b in buttons)Destroy(b.rect.gameObject);buttons.Clear();}
        void Button(string label,Action action,bool enabled=true)
        {
            int i=buttons.Count;float y=62+i*28;
            var image=ImageAt(menuRoot,"Botao"+i,22,y,286,24,UIArt(enabled?1:3,extras[7]));image.color=Color.white;image.type=Image.Type.Sliced;image.pixelsPerUnitMultiplier=8.333333f;
            var text=TextAt(image.transform,"Rotulo",8,0,270,24,6);text.text=label;text.alignment=TextAnchor.MiddleCenter;
            buttons.Add((image.rectTransform,text,image,action,enabled));
        }
        void RefreshPanel()
        {
            ClearButtons();SetHUDVisible(!(panel=="Main"||panel=="Skins"||panel=="ConfirmNew"||((panel=="Options"||panel=="Controls"||panel=="Credits"||panel=="BindingDevice")&&returnPanel=="Main")));overlay.gameObject.SetActive(true);portrait.gameObject.SetActive(false);
            bool front=panel=="Main"||panel=="Skins"||panel=="ConfirmNew"||((panel=="Options"||panel=="Controls"||panel=="Credits"||panel=="BindingDevice")&&returnPanel=="Main");
            menuRoot.Find("Moldura").GetComponent<Image>().color=front?new Color(1,1,1,.72f):Color.white;heading.fontSize=10;heading.rectTransform.sizeDelta=new Vector2(548,24);heading.alignment=TextAnchor.UpperLeft;
            menuBackdrop.sprite=front?menuBackground:null;menuBackdrop.color=front&&menuBackground!=null?Color.white:new Color(.02f,.02f,.04f,.88f);
            bodyText.fontSize=7;bodyText.rectTransform.anchoredPosition=new Vector2(334,-155);bodyText.rectTransform.sizeDelta=new Vector2(232,132);bodyText.alignment=TextAnchor.UpperLeft;portrait.rectTransform.anchoredPosition=new Vector2(367,-62);portrait.rectTransform.sizeDelta=new Vector2(160,82);
            switch(panel)
            {
                case "Main":
                    heading.text="DARKLIGHT: REQUIEM";portrait.gameObject.SetActive(true);portrait.sprite=lab.Player.GetComponent<DarklightCharacterView>().idle[0];portrait.rectTransform.anchoredPosition=new Vector2(358,-52);portrait.rectTransform.sizeDelta=new Vector2(178,174);heading.text="DARKLIGHT: REQUIEM";heading.fontSize=12;bodyText.rectTransform.anchoredPosition=new Vector2(334,-247);bodyText.rectTransform.sizeDelta=new Vector2(232,44);bodyText.fontSize=6;bodyText.alignment=TextAnchor.UpperCenter;bodyText.text="VESPERA\nA LUZ TEM UM PRECO";
                    Button("NOVO JOGO",()=>{if(DarklightProgress.Load()!=null)Open("ConfirmNew");else NewGame();});
                    Button("CONTINUAR",ContinueGame,DarklightProgress.Load()!=null);Button("OPCOES",()=>Open("Options"));Button("CREDITOS",()=>Open("Credits"));Button("VISUAL DE FABIO",()=>Open("Skins"));Button("LABORATORIO DE COMBATE",()=>UnityEngine.SceneManagement.SceneManager.LoadScene("DarklightCombatLab"));Button("SAIR",()=>Application.Quit());break;
                case "Skins":
                    heading.text="VISUAL DE FABIO";portrait.gameObject.SetActive(true);portrait.sprite=lab.Player.GetComponent<DarklightCharacterView>().idle[0];bodyText.text="ESCOLHA SALVA AUTOMATICAMENTE\n\nREQUIEM: sobretudo de couro.\n\nROCK: colete, rebites e correntes.\n\nO visual preserva seus atributos.";Button("REQUIEM"+(SelectedSkin==0?" - EQUIPADO":""),()=>{SelectSkin(0);RefreshPanel();});Button("ROCK"+(SelectedSkin==1?" - EQUIPADO":""),()=>{SelectSkin(1);RefreshPanel();});Button("VOLTAR",()=>Open("Main"));break;
                case "ConfirmNew":heading.text="NOVO JOGO";bodyText.text="Iniciar uma nova campanha substitui o save atual.\nA copia anterior fica no arquivo .bak.";Button("INICIAR CAMPANHA",NewGame);Button("VOLTAR",()=>Open("Main"));break;
                case "Pause":
                    heading.text="PAUSA";bodyText.text=DarklightStory.Chapters[Progress.chapter]+"\n"+DarklightStory.Places[Progress.chapter];
                    Button("RETOMAR",Close);Button("HABILIDADES",()=>Open("Skills"));Button("INVENTARIO",()=>Open("Inventory"));Button("DIARIO",()=>Open("Journal"));Button("DARK RESONANCE",()=>Open("MP3"));Button("OPCOES",()=>Open("Options"));Button("MENU PRINCIPAL",()=>{StopPower();SaveCheckpoint();Open("Main");});break;
                case "Skills":
                    heading.text="HABILIDADES";bodyText.text="PONTOS: "+Progress.points+"\nLAMINA +10% dano | VIGOR +15 vida | DOMINIO +2s forma\nCada atributo admite cinco melhorias.\nT / LT: forma; custo 60 energia.\nDuracao: 12s + 2s por Dominio.\nT / LT de novo encerra; F / RB: golpe da forma.";
                    Button("LAMINA "+Progress.attack+"/5",()=>Upgrade(0),Progress.points>0&&Progress.attack<5);Button("VIGOR "+Progress.vitality+"/5",()=>Upgrade(1),Progress.points>0&&Progress.vitality<5);Button("DOMINIO "+Progress.mastery+"/5",()=>Upgrade(2),Progress.points>0&&Progress.mastery<5);Button("FORMA: "+new[]{"AUTOMATICA","RUPTURA","ECLIPSE","ABSOLUTO"}[Progress.preferredForm],()=>{Progress.preferredForm=(Progress.preferredForm+1)%(Progress.FormUnlocked+1);SaveCheckpoint();RefreshPanel();},Progress.FormUnlocked>0);Button("VOLTAR",()=>Open("Pause"));break;
                case "Inventory":
                    heading.text="INVENTARIO";bodyText.text="V / LS: trocar armas\nPresas: apos Kael | Corrente: apos Matriarca\nELIXIRES: "+Progress.potions+" | MEMORIAS: "+Progress.shards+"/6\nMP3: "+(Progress.mp3Unlocked?"DESBLOQUEADO":"MISSAO FREQUENCIA DO VEU");
                    Button("USAR ELIXIR (+45% VIDA)",()=>{UsePotion();RefreshPanel();},Progress.potions>0&&health.HealthPercent<1&&!health.IsDead);for(int i=0;i<weaponDefinitions.Length;i++){int index=i;Button(weaponDefinitions[i].displayName+(Weapons.Equipped==weaponDefinitions[i]?" - EQUIPADA":""),()=>{Weapons.Request(index);Close();},Weapons.Unlocked(i));}Button("VOLTAR",()=>Open("Pause"));break;
                case "Journal":
                    heading.text="DIARIO";bodyText.text=Objective+"\n\n"+(Progress.mp3Unlocked?"Frequencia do Veu concluida.":Progress.mp3Quest>0?"Frequencia do Veu: "+Mathf.Min(3,Progress.mp3Quest-1)+"/3 componentes. Volte a Icaro.":"Icaro Nox espera no distrito do vinil (cap. 2).")+"\nMemorias opcionais: aproxime-se dos fragmentos e use E / A.";
                    Button("VOLTAR",()=>Open("Pause"));break;
                case "MP3":
                    heading.text="DARK RESONANCE";bodyText.text=!Progress.mp3Unlocked?"Recupere tres componentes para Icaro Nox\nno capitulo 2. A campanha funciona sem o MP3.":resonance>=0?"Modo ativo - "+resonanceTime.ToString("0")+"s restantes":resonanceCooldown>0?"RECARGA: "+resonanceCooldown.ToString("0")+"s":"LOVE: cura 1.4%/s, 18s\nFORCA: dano +45%, 12s\nFOCO: ataque +40%, 14s\nRecargas: 32/28/30s\nC/RS ativa; 1/2 escolhe";
                    portrait.gameObject.SetActive(true);portrait.sprite=extras[2];portrait.rectTransform.anchoredPosition=new Vector2(367,-62);
                    string[] names={"LOVE - JOIN ME IN DEATH","LOVE - WINGS OF A BUTTERFLY","THRASH - SEEK & DESTROY","THRASH - CREEPING DEATH","HEAVY - ENTER SANDMAN"};
                    for(int i=0;i<5;i++){int index=i;Button(names[i],()=>SelectSong(index),Progress.mp3Unlocked&&resonance<0&&phonesTime<=0&&Resonance.Recovery[index<2?0:index<4?2:1]<=0&&Resonance.Energy>=DarklightResonance.Costs[index<2?0:index<4?2:1]);}Button("VOLTAR",()=>Open("Pause"));break;
                case "Options":
                    heading.text="OPCOES";bodyText.text="Setas esquerda/direita ou A/clique para ajustar.\nVolume zero preserva os efeitos do MP3.";
                    Button("MUSICA: "+Mathf.RoundToInt(MusicVolume*100)+"%",()=>{MusicVolume=Mathf.Clamp01(MusicVolume+settingsDirection*.1f);PlayerPrefs.SetFloat("Darklight.Music",MusicVolume);music.volume=MusicVolume;RefreshPanel();});
                    Button("EFEITOS: "+Mathf.RoundToInt(EffectsVolume*100)+"%",()=>{EffectsVolume=Mathf.Clamp01(EffectsVolume+settingsDirection*.1f);PlayerPrefs.SetFloat("Darklight.Effects",EffectsVolume);RefreshPanel();});
                    Button("FLASHES REDUZIDOS: "+(ReducedFlashes?"SIM":"NAO"),()=>{ReducedFlashes=!ReducedFlashes;PlayerPrefs.SetInt("Darklight.Flashes",ReducedFlashes?1:0);RefreshPanel();});
                    Button("TELA: "+(Screen.fullScreen?"CHEIA":"JANELA"),()=>{Screen.fullScreen=!Screen.fullScreen;RefreshPanel();});Button("CONTROLES",()=>Open("Controls"));Button("VOLTAR",()=>Open(returnPanel));break;
                case "Controls":
                    heading.text="CONTROLES";bodyText.text="WASD/LS mover | cursor/RS mirar | E/A interagir\nT/LT transformar | M/SELECT MP3 | R/RT elixir\nEscolha a acao e depois teclado ou controle.";
                    foreach(string name in new[]{"Light","Heavy","Dodge","Parry","Skill","Transform","MP3","Power","PowerNext","PowerPrevious","WeaponNext","Aerial","WeaponSpecial"}){string n=name;Button(ActionLabel(n),()=>{rebindAction=n;Open("BindingDevice");});}Button("VOLTAR",()=>Open("Options"));break;
                case "BindingDevice":heading.text="REMAPEAR "+ActionLabel(rebindAction);bodyText.text="O remapeamento e salvo somente para DARKLIGHT.";Button("TECLADO / MOUSE",()=>Rebind(false));Button("CONTROLE",()=>Rebind(true));Button("PADROES",()=>{lab.Input.Asset.RemoveAllBindingOverrides();PlayerPrefs.DeleteKey("Darklight.Bindings");Open("Controls");});Button("VOLTAR",()=>Open("Options"));break;
                case "Dialogue":
                    string line=dialogue[dialogueIndex];int separator=line.IndexOf('|');string speaker=line.Substring(0,separator);
                    heading.text=speaker;bodyText.text=line.Substring(separator+1);bodyText.alignment=TextAnchor.UpperLeft;bodyText.rectTransform.anchoredPosition=new Vector2(334,-155);bodyText.rectTransform.sizeDelta=new Vector2(232,132);
                    portrait.gameObject.SetActive(true);portrait.sprite=speaker.Contains("ICARO")?extras[0]:speaker=="LYRA"?extras[1]:speaker=="FABIO"?lab.Player.GetComponent<DarklightCharacterView>().idle[0]:speaker=="KAEL"?bossFrames[0]:bossFrames[Progress.chapter*4];
                    Button("CONTINUAR "+(dialogueIndex+1)+"/"+dialogue.Length,NextDialogue);Button("PULAR CENA",()=>{dialogueIndex=dialogue.Length-1;NextDialogue();});break;
                case "Death":heading.text="O VEU TE ALCANCOU";bodyText.text="Retome do ultimo selo libertado.\nAs habilidades e a campanha continuam suas.";Button("RETOMAR CHECKPOINT",()=>{StopPower();LoadMap(Progress.chapter);Close();});Button("MENU PRINCIPAL",()=>Open("Main"));break;
                case "Ending":heading.text="O DESTINO DA PONTE";bodyText.text="Lucien caiu. Requiem pode selar o Veu ou manter\numa ponte sob sua vigilancia. Vespera espera.";Button("SELAR O VEU",()=>Finish(1));Button("GUARDAR A PONTE",()=>Finish(2));break;
                case "Victory":
                    heading.text="REQUIEM CONCLUIDO";bodyText.text=Progress.ending==1?"O sol voltou a Vespera. O Veu foi selado.\nFabio guardou Requiem para quem vier depois.":"Vespera voltou a respirar. A ponte permaneceu.\nFabio e Lyra guardam o caminho entre os mundos.";
                    bodyText.text+="\nMemorias: "+Progress.shards+"/6 | Nivel: "+Progress.level;Button("CREDITOS",()=>Open("Credits"));Button("REVISITAR CAPITULOS",()=>Open("ChapterSelect"));Button("MENU PRINCIPAL",()=>Open("Main"));break;
                case "ChapterSelect":heading.text="REVISITAR VESPERA";bodyText.text="Rejogue com seus atributos atuais.\nO save concluido e preservado ate escolher uma fase.";for(int i=0;i<6;i++){int chapter=i;Button((i+1)+" - "+DarklightStory.Chapters[i],()=>{Progress.chapter=chapter;Progress.checkpoint=0;Progress.completed=false;LoadMap(chapter);SaveCheckpoint();Speak(DarklightStory.Introductions[chapter]);});}Button("VOLTAR",()=>Open("Victory"));break;
                case "Credits":heading.text="CREDITOS";bodyText.text="Criacao e protagonista: Fabio\nDesenvolvimento assistido por Codex\nPixel art: segredo-varginha-pixelart + geracao assistida\nFonte: Press Start 2P, CodeMan38 (OFL)\nMP3 pessoais: HIM e Metallica | devfabio.vercel.app";Button("VOLTAR",()=>Open(returnPanel));break;
            }
            RefreshAppearancePortrait();selection=Mathf.Clamp(selection,0,Mathf.Max(0,buttons.Count-1));if(buttons.Count>0&&!buttons[selection].enabled)MoveSelection(1);
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
                Vector3 p=lab.WorldCamera.WorldToScreenPoint(item.Key.transform.position+Vector3.up*2.1f);p.x*=640f/lab.WorldCamera.pixelWidth;p.y*=360f/lab.WorldCamera.pixelHeight;
                bool visible=Active&&!item.Key.Health.IsDead&&(item.Key.Health.HealthPercent<1||Vector2.Distance(item.Key.transform.position,lab.Player.transform.position)<2.2f)&&p.x>26&&p.x<614&&p.y>45&&p.y<290;
                item.Value.rect.gameObject.SetActive(visible);item.Value.rect.anchoredPosition=new Vector2(Mathf.Round(p.x)-26,-Mathf.Round(360-p.y));
                item.Value.fill.rectTransform.sizeDelta=new Vector2(40*item.Key.Health.HealthPercent,3);
            }
            foreach(var enemy in removed)enemyBars.Remove(enemy);
        }

        void Upgrade(int stat)
        {if(Progress.points<=0)return;Progress.points--;if(stat==0)Progress.attack++;else if(stat==1){Progress.vitality++;health.SetMaxHealth(Progress.MaxHealth);}else Progress.mastery++;SaveCheckpoint();RefreshPanel();}
        void Finish(int ending){StopPower();Progress.ending=ending;Progress.completed=true;SaveCheckpoint();Open("Victory");Sound(3);}
        string ActionLabel(string name)=>name switch{"Light"=>"LEVE","Heavy"=>"PESADO","Dodge"=>"ESQUIVA","Parry"=>"PARRY","Skill"=>"ONDA","Transform"=>"TRANSFORMAR","MP3"=>"MP3","Power"=>"ATIVAR PODER","PowerNext"=>"PROXIMO PODER","PowerPrevious"=>"PODER ANTERIOR","WeaponNext"=>"TROCAR ARMA","Aerial"=>"ATAQUE AEREO","WeaponSpecial"=>"ESPECIAL DA ARMA",_=>name};
        void Rebind(bool pad)
        {
            var action=lab.Input.Action(rebindAction);int index=-1;
            for(int i=0;i<action.bindings.Count;i++)if(action.bindings[i].path.StartsWith(pad?"<Gamepad>":"<Keyboard>")||(!pad&&action.bindings[i].path.StartsWith("<Mouse>"))){index=i;break;}
            if(index<0)return;bodyText.text="Pressione o novo botao. ESC cancela.";
            rebinding=action.PerformInteractiveRebinding(index).WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta").WithCancelingThrough("<Keyboard>/escape")
                .OnComplete(op=>{PlayerPrefs.SetString("Darklight.Bindings",lab.Input.Asset.SaveBindingOverridesAsJson());op.Dispose();rebinding=null;Open("Controls");})
                .OnCancel(op=>{op.Dispose();rebinding=null;Open("Controls");});
            rebinding.WithControlsHavingToMatchPath(pad?"<Gamepad>/*":"<Keyboard>/*");if(!pad)rebinding.WithControlsHavingToMatchPath("<Mouse>/*");rebinding.Start();
        }
        RectTransform RectAt(Transform parent,string name,float x,float y,float w,float h)
        {var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);return rect;}
        Image ImageAt(Transform parent,string name,float x,float y,float w,float h,Sprite sprite)
        {var image=RectAt(parent,name,x,y,w,h).gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;if(sprite!=null)image.material=pixelMaterial;return image;}
        Text TextAt(Transform parent,string name,float x,float y,float w,float h,int size)
        {var text=RectAt(parent,name,x,y,w,h).gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.color=new Color(.94f,.9f,.87f);text.raycastTarget=false;text.horizontalOverflow=HorizontalWrapMode.Overflow;text.verticalOverflow=VerticalWrapMode.Overflow;var outline=text.gameObject.AddComponent<Outline>();outline.effectColor=Color.black;outline.effectDistance=new Vector2(1,-1);return text;}
    }
}
