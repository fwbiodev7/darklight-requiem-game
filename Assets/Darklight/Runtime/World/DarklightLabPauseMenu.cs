using UnityEngine;
using UnityEngine.InputSystem;

namespace Darklight
{
    // Uses the laboratory's existing pause, navigation and encounter actions.
    [DefaultExecutionOrder(100)]
    public sealed class DarklightLabPauseMenu : MonoBehaviour
    {
        DarklightCombatLab lab;
        RectTransform root;
        UnityEngine.UI.Text context;
        readonly RectTransform[] buttons=new RectTransform[4];
        readonly UnityEngine.UI.Image[] fills=new UnityEngine.UI.Image[4];
        readonly UnityEngine.UI.Image[,] edges=new UnityEngine.UI.Image[4,4];
        readonly UnityEngine.UI.Text[] cursors=new UnityEngine.UI.Text[4];
        Font font;
        float nextSound;
        AudioSource audio;
        AudioClip tone;
        readonly System.Collections.Generic.Dictionary<GameObject,bool> hudVisibility=new();
        bool wasActive;
        public void Initialize(DarklightCombatLab owner,DarklightPixelHUD hud)
        {
            lab=owner;font=hud.pixelFont;
            root=Rect(hud.transform.Find("HUD640x360"),"PausaCinematografica",0,0,640,360);
            var shade=root.gameObject.AddComponent<UnityEngine.UI.Image>();shade.color=new Color(.015f,.012f,.025f,.68f);shade.raycastTarget=false;
            Text(root,"Identidade","DARKLIGHT: REQUIEM",28,58,290,16,9);
            Text(root,"Titulo","PAUSA",28,83,320,36,18);
            context=Text(root,"Contexto","LABORATÓRIO DE COMBATE\n\nAprimore a lâmina. Domine o Véu.",328,137,278,175,6);
            string[] labels={"RETOMAR","REINICIAR TREINO","ALTERNAR TREINO DE DEFESA","MENU PRINCIPAL"};
            for(int i=0;i<4;i++)
            {
                buttons[i]=Rect(root,"Botao"+i,28,137+i*26,260,22);
                fills[i]=buttons[i].gameObject.AddComponent<UnityEngine.UI.Image>();fills[i].color=new Color(.025f,.03f,.045f,.8f);fills[i].raycastTarget=false;
                Text(buttons[i],"Rotulo",labels[i],22,0,232,22,7);
                cursors[i]=Text(buttons[i],"Selecao",">",7,0,10,22,7);
                for(int n=0;n<4;n++)
                {
                    var r=Rect(buttons[i],"Borda"+n,n==3?259.5f:0,n==1?21.5f:0,n<2?260:.5f,n<2?.5f:22);
                    edges[i,n]=r.gameObject.AddComponent<UnityEngine.UI.Image>();edges[i,n].raycastTarget=false;
                }
            }
            Text(root,"Navegacao","SETAS / LS: ESCOLHER    ENTER / A: OK    ESC / B: VOLTAR",28,338,550,12,4);
            audio=gameObject.AddComponent<AudioSource>();audio.playOnAwake=false;
            // A short quiet tick, following the main menu's existing discrete sounds.
            const int count=900;var data=new float[count];
            for(int i=0;i<count;i++)data[i]=Mathf.Sin(i*2*Mathf.PI*480/44100f)*.14f*(1-i/(float)count);
            tone=AudioClip.Create("NavegacaoPausa",count,1,44100,false);tone.SetData(data,0);
            root.gameObject.SetActive(false);
        }
        public bool ReadPointer(ref int selection,out int clicked)
        {
            clicked=-1;if(root==null||Mouse.current==null)return false;
            var mouse=Mouse.current;bool click=mouse.leftButton.wasPressedThisFrame;
            for(int i=0;i<4;i++)if(RectTransformUtility.RectangleContainsScreenPoint(buttons[i],mouse.position.ReadValue()))
            {
                if((click||mouse.delta.ReadValue().sqrMagnitude>0)&&selection!=i){selection=i;Sound();}
                if(click){clicked=i;Sound();return true;}
            }
            return false;
        }
        int previous=-1;
        void LateUpdate()
        {
            if(root==null)return;
            bool active=lab.Paused&&DarklightCampaign.Instance==null;
            root.gameObject.SetActive(active);
            if(active&&!wasActive)
            {
                hudVisibility.Clear();foreach(Transform child in root.parent)
                    if(child!=root&&child.name!="Mundo640x360")hudVisibility[child.gameObject]=child.gameObject.activeSelf;
            }
            if(active){foreach(var item in hudVisibility)if(item.Key!=null)item.Key.SetActive(false);}
            else if(wasActive){foreach(var item in hudVisibility)if(item.Key!=null)item.Key.SetActive(item.Value);hudVisibility.Clear();}
            wasActive=active;if(!active){previous=-1;return;}
            if(previous>=0&&previous!=lab.PauseSelection)Sound();previous=lab.PauseSelection;
            context.text="LABORATÓRIO DE COMBATE\n\nAprimore a lâmina. Domine o Véu.\n\nTREINO DE DEFESA: "+(lab.Targets.Length>0&&lab.Targets[0]!=null&&lab.Targets[0].DefensePractice?"ATIVO":"INATIVO");
            float t=Mathf.Min(1,Time.unscaledDeltaTime*14);
            for(int i=0;i<4;i++)
            {
                bool focus=i==lab.PauseSelection;
                fills[i].color=Color.Lerp(fills[i].color,focus?new Color(.19f,.035f,.06f,.76f):new Color(.025f,.03f,.045f,.8f),t);
                Color edge=focus?new Color(.63f,.08f,.15f):new Color(.27f,.3f,.32f);
                for(int n=0;n<4;n++)edges[i,n].color=Color.Lerp(edges[i,n].color,edge,t);
                cursors[i].color=focus?new Color(.78f,.1f,.18f):new Color(.38f,.42f,.44f,.8f);
            }
        }
        void Sound()
        {
            if(DarklightAutomation.Silent||Time.unscaledTime<nextSound)return;
            nextSound=Time.unscaledTime+.055f;audio.PlayOneShot(tone,PlayerPrefs.GetFloat("Darklight.Effects",.75f)*.055f);
        }
        static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }
        UnityEngine.UI.Text Text(Transform parent,string name,string value,float x,float y,float w,float h,int size)
        {
            var t=Rect(parent,name,x,y,w,h).gameObject.AddComponent<UnityEngine.UI.Text>();t.font=font;t.fontSize=size;t.text=value;
            t.color=new Color(.91f,.89f,.83f);t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.lineSpacing=1.7f;return t;
        }
        void OnDestroy(){if(root!=null)Destroy(root.gameObject);if(tone!=null)Destroy(tone);}
    }
}
