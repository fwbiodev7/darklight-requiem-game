using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        RectTransform dialogueRoot,dialogueBox; RawImage dialoguePortrait; Text dialogueName,dialogueText,dialogueCue;
        DarklightDialogueReveal dialogueReveal;float dialogueInputReady;
        public bool DialogueTyping=>panel=="Dialogue"&&dialogueReveal!=null&&!dialogueReveal.Complete;
        public int DialogueVisibleCharacters=>dialogueReveal?.Count??0;
        public int DialogueLine=>dialogueIndex;
        string[] PrepareDialogueLines(string[] lines)
        {
            if(dialogueRoot==null)CreateDialogueUI();
            var pages=new List<string>();var generator=new TextGenerator();var settings=dialogueText.GetGenerationSettings(dialogueText.rectTransform.rect.size);settings.verticalOverflow=VerticalWrapMode.Overflow;
            foreach(string raw in lines){int split=raw.IndexOf('|');string speaker=split<0?"VESPERA":raw.Substring(0,split);string content=split<0?raw:raw.Substring(split+1);string page="";
                foreach(string word in Regex.Split(content,@"(?<=\s)")){string candidate=page+word;if(page.Length>0&&generator.GetPreferredHeight(candidate,settings)/dialogueText.pixelsPerUnit>dialogueText.rectTransform.rect.height){pages.Add(speaker+"|"+page.TrimEnd());page=word.TrimStart();}else page=candidate;}
                pages.Add(speaker+"|"+page.TrimEnd());}
            return pages.ToArray();
        }
        void CreateDialogueUI()
        {
            dialogueRoot=RectAt(overlay,"Dialogo",0,0,640,360);
            dialogueBox=RectAt(dialogueRoot,"Caixa",10,238,620,116);
            var frame=ImageAt(dialogueBox,"MolduraDialogo",0,0,620,116,UIArt(5,UIArt(0,extras[6])));
            frame.type=Image.Type.Sliced;frame.pixelsPerUnitMultiplier=8.333333f;frame.color=Color.white;frame.raycastTarget=false;
            // Reuse the authored menu texture and the HUD portrait rim at native pixel scale.
            var portraitFrame=RectAt(dialogueRoot,"MolduraRetrato",16,240,110,110);DarklightHUDTheme.Frame(portraitFrame);
            var divider=ImageAt(dialogueBox,"Divisor",121,15,1,86,null);divider.color=new Color(.38f,.08f,.14f);
            dialoguePortrait=RectAt(dialogueRoot,"RetratoNPC",19,241,100,90).gameObject.AddComponent<RawImage>();dialoguePortrait.raycastTarget=false;
            dialogueName=TextAt(dialogueRoot,"NomeNPC",16,334,110,13,6);dialogueName.alignment=TextAnchor.MiddleCenter;dialogueName.horizontalOverflow=HorizontalWrapMode.Wrap;
            dialogueText=TextAt(dialogueRoot,"Fala",141,262,470,65,8);dialogueText.supportRichText=false;dialogueText.horizontalOverflow=HorizontalWrapMode.Wrap;dialogueText.verticalOverflow=VerticalWrapMode.Truncate;dialogueText.lineSpacing=1.55f;
            dialogueCue=TextAt(dialogueRoot,"Avancar",141,337,470,10,5);dialogueCue.alignment=TextAnchor.MiddleRight;
            dialogueRoot.gameObject.SetActive(false);
        }
        void ShowDialogue()
        {
            if(dialogueRoot==null)CreateDialogueUI();ClearButtons();SetHUDVisible(false);
            overlay.gameObject.SetActive(true);menuRoot.gameObject.SetActive(false);cinematicRoot.gameObject.SetActive(false);
            menuBackdrop.enabled=true;menuBackdrop.sprite=null;menuBackdrop.color=new Color(.015f,.01f,.025f,.12f);
            dialogueRoot.gameObject.SetActive(true);dialogueRoot.SetAsLastSibling();
            string line=dialogue[dialogueIndex];int separator=line.IndexOf('|');string speaker=separator<0?"VESPERA":line.Substring(0,separator);
            string key=speaker.ToUpperInvariant().Replace("Í","I").Replace("Á","A").Replace("Ó","O").Replace("É","E");
            Sprite art=key.Contains("ICARO")?extras[0]:key.StartsWith("LYRA")?extras[1]:key=="FABIO"?lab.Player.GetComponent<DarklightCharacterView>().idle[0]:key.Contains("KAEL")?bossFrames[0]:key.StartsWith("MEMORIA")?null:bossFrames[Mathf.Clamp(Progress.chapter*4,0,bossFrames.Length-1)];
            dialogueName.text=speaker.ToUpperInvariant();dialoguePortrait.gameObject.SetActive(art!=null);
            if(art!=null){var r=art.rect;float height=r.height*.56f;float width=Mathf.Min(100,90*r.width/height);dialoguePortrait.texture=art.texture;dialoguePortrait.uvRect=new Rect(r.x/art.texture.width,(r.y+r.height-height)/art.texture.height,r.width/art.texture.width,height/art.texture.height);dialoguePortrait.rectTransform.anchoredPosition=new Vector2(19+(100-width)*.5f,-241);dialoguePortrait.rectTransform.sizeDelta=new Vector2(width,90);}
            dialogueReveal=new DarklightDialogueReveal(separator<0?line:line.Substring(separator+1));dialogueText.text="";dialogueInputReady=Time.unscaledTime+.15f;
            // The remapped interaction key can also advance a conversation while gameplay actions are disabled.
            var advance=lab.Input.Menu("DialogueAdvance");var interaction=lab.Input.Action("Interact");
            for(int i=0;i<interaction.bindings.Count;i++){string path=interaction.bindings[i].effectivePath;if(path?.StartsWith("<Keyboard>")==true)advance.ApplyBindingOverride(2,path);else if(path?.StartsWith("<Gamepad>")==true)advance.ApplyBindingOverride(3,path);}
            RefreshDialogueCue();
        }
        void RefreshDialogueCue(){dialogueCue.text=(lab.Input.UsingGamepad?lab.Input.BindingLabel("Interact"):lab.Input.BindingLabel("Interact")+" / ENTER / ESPACO")+": "+(DialogueTyping?"REVELAR":"CONTINUAR")+"    "+(dialogueIndex+1)+"/"+dialogue.Length+"    "+(lab.Input.UsingGamepad?"B":"ESC")+": PULAR";}
        public void AdvanceDialogue()
        {
            if(panel!="Dialogue"||dialogueReveal==null)return;
            if(DialogueTyping){dialogueReveal.RevealAll();dialogueText.text=dialogueReveal.VisibleText;RefreshDialogueCue();}
            else NextDialogue();
        }
        void TickDialogueInput()
        {
            if(dialogueReveal.Tick(Time.unscaledDeltaTime)){dialogueText.text=dialogueReveal.VisibleText;RefreshDialogueCue();}
            if(Time.unscaledTime<dialogueInputReady)return;
            if(lab.Input.Menu("Cancel").WasPressedThisFrame()){dialogueIndex=dialogue.Length-1;NextDialogue();return;}
            bool clicked=Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame&&RectTransformUtility.RectangleContainsScreenPoint(dialogueBox,Mouse.current.position.ReadValue());
            if(lab.Input.Menu("DialogueAdvance").WasPressedThisFrame()||clicked)AdvanceDialogue();
        }
    }
}
