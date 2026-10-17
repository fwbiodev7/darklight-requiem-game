using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Darklight
{
    public readonly struct DarklightInteractionTarget
    {
        public readonly string Id,Label;public readonly Vector2 Point;
        public DarklightInteractionTarget(string id,string label,Vector2 point){Id=id;Label=label;Point=point;}
    }
    public sealed partial class DarklightCampaign
    {
        RectTransform interactionButton;Image interactionBackground;Text interactionText,interactionKey;
        readonly System.Collections.Generic.Dictionary<Vector2,Vector2> interactionAnchors=new();
        DarklightMapDefinition interactionMap;
        Vector2 InteractionAnchor(Vector2 authored){if(interactionMap!=Map){interactionAnchors.Clear();interactionMap=Map;}if(!interactionAnchors.TryGetValue(authored,out var point)){point=Map.SafeSpawn(authored);interactionAnchors[authored]=point;}return point;}
        public Vector2 CheckpointPoint=>InteractionAnchor(Map.Spawn+Vector2.up*1.8f);
        int interactionClickFrame=-1;
        public bool InteractionClickConsumed=>interactionClickFrame==Time.frameCount;
        public bool TryGetInteraction(out DarklightInteractionTarget target)
        {
            target=default;if(Training||!Active||Map==null||lab.Player.Model.State!=CombatState.Neutral||WearingHeadphones)return false;
            Vector2 player=lab.Player.transform.position;float best=float.MaxValue;DarklightInteractionTarget nearest=default;
            void Consider(string id,string label,Vector2 authored,float range)
            {
                Vector2 point=InteractionAnchor(authored);float distance=(point-player).sqrMagnitude;
                if(distance>range*range||distance>=best||!DarklightNavigation.Clear(player,point))return;
                best=distance;nearest=new DarklightInteractionTarget(id,label,point);
            }
            if(AbyssSecretAvailable)Consider("abyss_motor","DESPERTAR MOTOR SELADO",AbyssSecretPoint,2.1f);
            if(ExploringNewRegion)
            {
                Consider("region_exit","SAIR DA REGIÃO",Map.Portal,2.5f);
                for(int i=0;i<Map.Art.areas.Length;i++)Consider("memory_"+i,"LER: "+Map.Art.areas[i].name,Map.Art.World(Map.Art.areas[i].point),2.3f);
            }
            else
            {
                if(chapterCleared)Consider("portal","ATRAVESSAR PORTAL",Map.Portal,2.5f);
                if(!Progress.relics[Progress.chapter])Consider("relic","RECOLHER MEMÓRIA",Map.Relic,2.3f);
                if(NarrativeTarget(out var narrative))Consider(narrative.Id,narrative.Label,narrative.Point,2.7f);
                if(RegionAtmosphereTarget(out var environmental))Consider(environmental.Id,environmental.Label,environmental.Point,2.2f);
                Consider("ally","FALAR: "+AllyName,Map.Ally,2.7f);
                if(!encounterStarted)Consider("checkpoint","SALVAR E RECUPERAR VIDA",CheckpointPoint,2.3f);
            }
            target=nearest;return nearest.Id!=null;
        }
        public bool TryInteract()
        {
            if(!TryGetInteraction(out var target))return false;
            PerformInteraction(target);return true;
        }
        void TickInteractionInput()
        {
            if(interactionButton==null&&uiRoot!=null)
            {
                interactionButton=RectAt(uiRoot,"BotaoInteragir",207,250,226,24);
                interactionBackground=interactionButton.gameObject.AddComponent<Image>();interactionBackground.raycastTarget=false;
                DarklightHUDTheme.Frame(interactionButton);interactionKey=DarklightHUDTheme.Key(interactionButton,font,"TeclaInteragir",13,4,22,16);
                interactionText=TextAt(interactionButton,"Acao",42,4,171,16,5);interactionText.alignment=TextAnchor.MiddleCenter;interactionText.horizontalOverflow=HorizontalWrapMode.Wrap;interactionText.verticalOverflow=VerticalWrapMode.Truncate;
            }
            if(interactionButton==null)return;
            bool available=TryGetInteraction(out var target);interactionButton.gameObject.SetActive(available);if(!available)return;
            interactionKey.text=lab.Input.BindingLabel("Interact");interactionText.text=target.Label;
            bool hover=Mouse.current!=null&&RectTransformUtility.RectangleContainsScreenPoint(interactionButton,Mouse.current.position.ReadValue());
            interactionBackground.color=hover?new Color(.22f,.025f,.07f,.96f):new Color(.025f,.022f,.03f,.92f);
            if(hover&&Mouse.current.leftButton.wasPressedThisFrame){interactionClickFrame=Time.frameCount;PerformInteraction(target);interactionButton.gameObject.SetActive(false);}
        }
        void PerformInteraction(DarklightInteractionTarget target)
        {
            if(InteractAbyssSecret(target.Id))return;
            if(ExploringNewRegion)
            {
                if(target.Id=="region_exit"){Open("Regions");return;}
                int area=int.Parse(target.Id.Substring(7));Speak(new[]{"MEMÓRIA DO VÉU|"+Map.Art.areas[area].description});return;
            }
            if(InteractRegionAtmosphere(target.Id))return;
            if(InteractNarrative(target.Id))return;
            if(target.Id=="portal")
            {
                if(Progress.chapter==DarklightStory.Chapters.Length-1){Open("Ending");return;}
                TravelToChapter(Progress.chapter+1);return;
            }
            if(target.Id=="relic")
            {
                Progress.relics[Progress.chapter]=true;Progress.shards++;Progress.Award(45);health.SetMaxHealth(Progress.MaxHealth);Progress.potions=Mathf.Min(9,Progress.potions+1);
                var fragment=levelRoot.Find("Fragmento");if(fragment!=null){fragment.gameObject.SetActive(false);Destroy(fragment.gameObject);}
                Notice("MEMÓRIA RECUPERADA: +45 XP, +1 ELIXIR");SaveCheckpoint();return;
            }
            if(target.Id=="checkpoint")
            {
                health.FullHeal();Progress.RecordReturnPoint(Progress.chapter,Progress.checkpoint,Map.SafeSpawn(target.Point));
                if(SaveCheckpoint()){PulseCheckpoint();Notice("CHECKPOINT SALVO - VIDA RESTAURADA");}return;
            }
            if(Progress.chapter==1)
            {
                if(Progress.mp3Unlocked)Speak(new[]{"ÍCARO NOX|A música vem de você. O aparelho só abre o caminho. M ou PAUSA > DARK RESONANCE abre o MP3. SELECT abre o mapa."});
                else if(Progress.mp3Quest==0){Progress.mp3Quest=1;Speak(new[]{"ÍCARO NOX|Meu MP3 capta o Véu. Os Ecos levaram três componentes. Derrote-os e traga-os para mim.","FÁBIO|Vou recuperar seu equipamento. Quero saber o que essas frequências escondem."});SaveCheckpoint();}
                else if(Progress.mp3Quest>=4){Progress.mp3Unlocked=true;Speak(new[]{"ÍCARO NOX|Aqui está. Escolha a frequência: Love regenera, Thrash concentra, Heavy fortalece.","FÁBIO|Vou usar o que me fortalece sem deixar de ser eu."});SaveCheckpoint();Sound(3);}
                else Speak(new[]{"ÍCARO NOX|Ainda faltam "+(4-Progress.mp3Quest)+" componentes. Os Ecos desta rua carregam as peças."});return;
            }
            if(Progress.chapter==0)Speak(new[]{"LYRA|Fábio, as rupturas tomaram Vespera. Siga os selos para abrir a passagem.","LYRA|O mapa revela os caminhos que você explora. Procure as memórias e volte aos checkpoints para recuperar vida.","LYRA|Golpes dourados aceitam parry. O anel vermelho exige esquiva. "+Objective});
            else Speak(new[]{AllyName+"|"+(ExpandedChapter?MissionDescription+" "+Objective:"Siga os selos. "+Objective)});
        }
    }
}


