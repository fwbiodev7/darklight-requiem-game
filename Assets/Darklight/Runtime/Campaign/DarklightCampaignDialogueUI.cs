using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        RectTransform dialogueRoot,dialogueBox; RawImage dialoguePortrait;
        Text dialogueName,dialogueText,dialogueCue,dialogueSkip,dialoguePage;
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
            Color steel=new(.59f,.55f,.51f);
            Color crimson=new(.55f,.09f,.14f);
            dialogueBox=RectAt(dialogueRoot,"Caixa",32,259,382,86);
            var speech=RectAt(dialogueBox,"MolduraDialogo",72,4,310,82);
            DarklightHUDTheme.Frame(speech,steel);
            ImageAt(speech,"TraçoCarmesim",12,12,1,41,null).color=crimson;
            ImageAt(speech,"DivisorRodape",4,62,302,1,null).color=new Color(.32f,.29f,.27f);

            var portraitFrame=RectAt(dialogueBox,"MolduraRetrato",0,0,75,80);
            DarklightHUDTheme.Frame(portraitFrame,steel);
            dialoguePortrait=RectAt(portraitFrame,"RetratoNPC",4,4,67,65).gameObject.AddComponent<RawImage>();
            dialoguePortrait.raycastTarget=false;
            var nameplate=RectAt(dialogueBox,"PlacaNome",4,71,67,15);
            DarklightHUDTheme.Frame(nameplate,steel);
            nameplate.GetComponent<Image>().color=new Color(.29f,.035f,.075f);
            dialogueName=TextAt(nameplate,"NomeNPC",3,1,61,13,7);
            dialogueName.alignment=TextAnchor.MiddleCenter;
            dialogueName.resizeTextForBestFit=true;
            dialogueName.resizeTextMinSize=4;
            dialogueName.resizeTextMaxSize=7;

            dialogueText=TextAt(speech,"Fala",23,13,273,42,10);
            dialogueText.supportRichText=false;
            dialogueText.horizontalOverflow=HorizontalWrapMode.Wrap;
            dialogueText.verticalOverflow=VerticalWrapMode.Truncate;
            dialogueText.lineSpacing=1.3f;
            dialogueCue=TextAt(speech,"Avancar",21,65,174,12,5);
            dialogueCue.alignment=TextAnchor.MiddleLeft;
            ImageAt(speech,"DivisorComandos",202,66,1,11,null).color=crimson;
            dialogueSkip=TextAt(speech,"Pular",211,65,48,12,5);
            dialogueSkip.alignment=TextAnchor.MiddleLeft;
            var page=RectAt(speech,"IndicadorPagina",264,65,32,12);
            DarklightHUDTheme.Frame(page,crimson);
            dialoguePage=TextAt(page,"Pagina",0,0,32,12,6);
            dialoguePage.alignment=TextAnchor.MiddleCenter;
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
            Sprite art=DialoguePortrait(key);
            dialogueName.text=speaker.ToUpperInvariant();dialoguePortrait.gameObject.SetActive(art!=null);
            if(art!=null)
            {
                Rect crop=art.rect;
                bool isPortrait=key=="FABIO"||key=="FABIUS";
                if(key.Contains("MATRIARCA")&&art.name=="MatriarcaRubra")
                {
                    crop=new Rect(crop.x+crop.width*.28f,crop.y+crop.height*.43f,crop.width*.44f,crop.height*.55f);
                    isPortrait=true;
                }
                float height=isPortrait?crop.height:crop.height*.56f;
                float scale=Mathf.Min(67/crop.width,65/height);
                float width=crop.width*scale;
                dialoguePortrait.texture=art.texture;
                dialoguePortrait.uvRect=new Rect(crop.x/art.texture.width,(crop.yMax-height)/art.texture.height,
                    crop.width/art.texture.width,height/art.texture.height);
                dialoguePortrait.rectTransform.anchoredPosition=new Vector2(Mathf.Round(4+(67-width)*.5f),-4);
                dialoguePortrait.rectTransform.sizeDelta=new Vector2(Mathf.Round(width),Mathf.Round(height*scale));
            }
            dialogueReveal=new DarklightDialogueReveal(separator<0?line:line.Substring(separator+1));dialogueText.text="";dialogueInputReady=Time.unscaledTime+.15f;
            // The remapped interaction key can also advance a conversation while gameplay actions are disabled.
            var advance=lab.Input.Menu("DialogueAdvance");var interaction=lab.Input.Action("Interact");
            for(int i=0;i<interaction.bindings.Count;i++){string path=interaction.bindings[i].effectivePath;if(path?.StartsWith("<Keyboard>")==true)advance.ApplyBindingOverride(2,path);else if(path?.StartsWith("<Gamepad>")==true)advance.ApplyBindingOverride(3,path);}
            RefreshDialogueCue();
        }
        static Sprite OfficialCastSprite(string name)=>Resources.Load<Sprite>("Darklight/OfficialCast/"+name);
        Sprite DialoguePortrait(string key)
        {
            if(key.Contains("ICARO"))return extras[0];
            if(key.StartsWith("LADY")||key.StartsWith("LYRA"))return OfficialCastSprite("Lady")??extras[1];
            if(key=="FABIO"||key=="FABIUS")
                return hudPortraits[0]??Resources.Load<Sprite>("Darklight/HUD/Portrait0");
            if(key.Contains("NERO")||key.Contains("KAEL"))return OfficialCastSprite("Nero")??bossFrames[0];
            if(key.Contains("MATRIARCA"))return OfficialCastSprite("MatriarcaRubra")??bossFrames[8];
            if(key=="LUCIUS"&&DarklightLuciusSpriteBank.TryPose("Idle",0,0,out var lucius))return lucius;
            return null;
        }
        void RefreshDialogueCue()
        {
            string key=lab.Input.BindingLabel("Interact");
            if(!lab.Input.UsingGamepad)key+=" / ENTER / ESPAÇO";
            dialogueCue.text=key+": "+(DialogueTyping?"REVELAR":"CONTINUAR");
            dialogueSkip.text=(lab.Input.UsingGamepad?"B":"ESC")+": PULAR";
            dialoguePage.text=(dialogueIndex+1)+"/"+dialogue.Length;
        }
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
