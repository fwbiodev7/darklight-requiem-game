using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        bool controlsPad,controlsMap,bindingWaiting,captureUIEnabled,captureGameplayEnabled;
        float captureAfter,captureDeadline;
        DarklightInput.BindingSlot bindingSlot;
        string candidatePath;
        readonly string[] gameplayCommands={"Move","Interact","Light","Heavy","Dodge","Parry","Skill","Transform","Potion","MP3","Journal","Map","Pause","Power","PowerNext","PowerPrevious","WeaponNext","Aerial","WeaponSpecial","Vampire","VampireNext","AbyssSaw","AbyssBike","AbyssDrift"};
        readonly string[] mapCommands={"MapPan","MapCursor","MapZoomIn","MapZoomOut","MapCenter","MapMark","MapClear","MapObjective"};
        string SlotTitle(DarklightInput.BindingSlot slot)
        {
            string title=ActionLabel(slot.action.name);
            if(slot.binding.isPartOfComposite)title+=" "+(slot.binding.name.ToLowerInvariant() switch {"up"=>"CIMA","down"=>"BAIXO","left"=>"ESQUERDA","right"=>"DIREITA","positive"=>"PROXIMO","negative"=>"ANTERIOR",_=>slot.binding.name.ToUpperInvariant()});
            if(slot.action.name=="Move"&&slot.index>=6)title+=" (ALTERNATIVO)";
            return title;
        }
        void BuildControls()
        {
            heading.text="CONTROLES";
            bodyText.text="Escolha o dispositivo e o comando.\nOs comandos sao salvos automaticamente.\n\nESC / MENU cancela a captura.\nA / ENTER confirma; B / ESC volta.\n\nLT / L2 transforma por padrao.\nAs tres formas estao liberadas.";
            Button("DISPOSITIVO: "+(controlsPad?"CONTROLE":"TECLADO / MOUSE"),()=>{controlsPad=!controlsPad;RefreshPanel();});
            Button("COMANDOS: "+(controlsMap?"MAPA":"JOGO"),()=>{controlsMap=!controlsMap;RefreshPanel();});
            foreach(string name in controlsMap?mapCommands:gameplayCommands)
            foreach(var slot in lab.Input.Slots(name,controlsPad))
            {
                var selected=slot;
                Button(SlotTitle(slot)+"  "+lab.Input.SlotLabel(slot),()=>{bindingSlot=selected;Open("BindingSlot");});
            }
            Button("RESTAURAR "+(controlsPad?"CONTROLE":"TECLADO / MOUSE"),()=>{lab.Input.RestoreBindings(controlsPad);RefreshPanel();});
            Button("VOLTAR",()=>Open("Options"));
        }
        void BuildBindingSlot()
        {
            if(bindingSlot.action==null){bindingSlot=lab.Input.Slots("Light",controlsPad)[0];}
            heading.text=SlotTitle(bindingSlot);
            bodyText.text="ATUAL: "+lab.Input.SlotLabel(bindingSlot)+"\n\nEscolha um novo comando.\nConflitos precisam ser resolvidos antes de salvar.\n\nESC / MENU cancela.\nA captura encerra em 10 segundos.";
            Button("ALTERAR COMANDO",StartBindingCapture);
            Button("RESTAURAR ESTE COMANDO",()=>{if(lab.Input.RestoreSlot(bindingSlot))RefreshPanel();else bodyText.text="O padrao ja esta em uso. Restaure o dispositivo na tela anterior ou troque os comandos.";});
            if(bindingSlot.action.name!="Move"&&bindingSlot.action.name!="Pause")Button("REMOVER ATRIBUICAO",()=>{lab.Input.Assign(bindingSlot,"");RefreshPanel();});
            Button("VOLTAR",()=>Open("Controls"));
        }
        void StartBindingCapture()
        {
            if(rebinding!=null||bindingWaiting)return;
            captureUIEnabled=lab.Input.UI.enabled;captureGameplayEnabled=lab.Input.Gameplay.enabled;
            lab.Input.Asset.Disable();bindingWaiting=true;captureAfter=Time.unscaledTime+.2f;captureDeadline=Time.unscaledTime+10;
            bodyText.text="Solte o botao de confirmacao.\nDepois pressione o novo comando.\n\nESC / MENU cancela.";
        }
        void RestoreCaptureInput()
        {
            bindingWaiting=false;
            if(captureUIEnabled)lab.Input.UI.Enable();if(captureGameplayEnabled)lab.Input.Gameplay.Enable();
            // Consume the confirming input before the menu can interpret it again.
            nextNav=Time.unscaledTime+.2f;captureAfter=Time.unscaledTime+.15f;
        }
        void TickBindingCapture()
        {
            bool cancel=Keyboard.current?.escapeKey.wasPressedThisFrame==true;
            foreach(var pad in Gamepad.all)cancel|=pad.startButton.wasPressedThisFrame;
            if(cancel||Time.unscaledTime>=captureDeadline)
            {
                if(rebinding!=null)rebinding.Cancel();else {RestoreCaptureInput();Open("BindingSlot");bodyText.text=cancel?"Remapeamento cancelado. Seu comando foi preservado.":"Tempo esgotado. Seu comando foi preservado.";}
                return;
            }
            if(!bindingWaiting||Time.unscaledTime<captureAfter)return;
            bool held=Mouse.current?.leftButton.isPressed==true||Keyboard.current?.enterKey.isPressed==true;
            foreach(var pad in Gamepad.all)held|=pad.buttonSouth.isPressed;
            if(held)return;
            bindingWaiting=false;
            bool vector=bindingSlot.action.expectedControlType=="Vector2"&&!bindingSlot.binding.isPartOfComposite;
            rebinding=bindingSlot.action.PerformInteractiveRebinding(bindingSlot.index)
                .WithExpectedControlType(vector?"Vector2":"Button")
                .WithControlsExcluding("<Mouse>/position").WithControlsExcluding("<Mouse>/delta").WithControlsExcluding("<Mouse>/scroll")
                .WithCancelingThrough("<Keyboard>/escape").WithMagnitudeHavingToBeGreaterThan(controlsPad?.65f:.2f)
                .WithTimeout(Mathf.Max(.1f,captureDeadline-Time.unscaledTime)).OnMatchWaitForAnother(.12f)
                .OnApplyBinding((op,path)=>candidatePath=path)
                .OnComplete(op=>{op.Dispose();rebinding=null;RestoreCaptureInput();FinishBindingCapture();})
                .OnCancel(op=>{op.Dispose();rebinding=null;RestoreCaptureInput();Open("BindingSlot");bodyText.text="Captura encerrada. Seu comando foi preservado.";});
            rebinding.WithControlsHavingToMatchPath(controlsPad?"<Gamepad>/*":"<Keyboard>/*");
            if(!controlsPad)rebinding.WithControlsHavingToMatchPath("<Mouse>/*");
            candidatePath=null;bodyText.text=vector?"Mova o analogico ou o direcional.\nESC / MENU cancela.":"Pressione o novo botao.\nESC / MENU cancela.";rebinding.Start();
        }
        void FinishBindingCapture()
        {
            if(!DarklightInput.CanAssign(bindingSlot,candidatePath)){Open("BindingSlot");bodyText.text="Este comando e reservado ou incompativel. Tente outro botao.";return;}
            if(lab.Input.Conflicts(bindingSlot,candidatePath).Count>0){Open("BindingConflict");return;}
            lab.Input.Assign(bindingSlot,candidatePath);Open("Controls");Notice("COMANDO SALVO");
        }
        void BuildBindingConflict()
        {
            var conflicts=lab.Input.Conflicts(bindingSlot,candidatePath);
            heading.text="COMANDO EM USO";bodyText.text="Este botao ja esta atribuido a:";
            foreach(var slot in conflicts)bodyText.text+="\n"+SlotTitle(slot);
            bodyText.text+="\n\nSeu comando anterior continua ativo.";
            if(lab.Input.CanSwap(bindingSlot,candidatePath,conflicts))Button("TROCAR OS DOIS COMANDOS",()=>{if(lab.Input.Swap(bindingSlot,candidatePath)){Open("Controls");Notice("COMANDOS TROCADOS");}else RefreshPanel();});
            Button("ESCOLHER OUTRO",()=>{Open("BindingSlot");StartBindingCapture();});
            Button("CANCELAR",()=>Open("BindingSlot"));
        }
        void BuildTransformations()
        {
            heading.text="TRANSFORMACOES";bodyText.text="As tres formas estao liberadas.\nEscolha a forma e pressione "+lab.Input.BindingLabel("Transform")+" durante o jogo.\nPressione novamente para voltar ao normal.\n\nCusto: 60 de sangue.\nDuracao minima: 3 minutos.\nDominio do Veu aumenta a duracao.";
            for(int i=1;i<=3;i++){int form=i;Button(new[]{"","RUPTURA — CARMESIM","ECLIPSE — VIOLETA","ABSOLUTO — BRANCA"}[i]+(Progress.preferredForm==i?" [EQUIPADA]":""),()=>{Progress.preferredForm=form;SaveCheckpoint();RefreshPanel();});}
            Button("VOLTAR",()=>Open("Pause"));
        }
    }
}
