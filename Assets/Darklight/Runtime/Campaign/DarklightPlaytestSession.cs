using UnityEngine;
using UnityEngine.UI;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        public bool PlaytestSession {get;private set;}
        Text playtestGuide;RectTransform playtestGuideRoot;
        void BeginPlaytestSession()
        {
            PlaytestSession=true;
            var saved=DarklightProgress.Load();
            Progress=saved==null?new DarklightProgress():DarklightProgress.Decode(JsonUtility.ToJson(saved));
            ConfigurePlaytestResources();Progress.chapter=4;Progress.checkpoint=0;Progress.completed=false;
            Progress.bossPhase=1;Progress.missionWave=0;Progress.equippedWeapon=0;
            ResetPlaytestGarden();
            playtestGuideRoot=RectAt(uiRoot,"GuiaDaSessaoDeTeste",12,75,265,46);
            var background=playtestGuideRoot.gameObject.AddComponent<Image>();background.color=new Color(.025f,.015f,.03f,.8f);background.raycastTarget=false;
            DarklightHUDTheme.Frame(playtestGuideRoot);
            playtestGuide=TextAt(playtestGuideRoot,"Comandos",7,5,250,39,5);playtestGuide.horizontalOverflow=HorizontalWrapMode.Wrap;playtestGuide.lineSpacing=1.35f;
            Debug.Log("DARKLIGHT PLAYTEST READY: Jardim dos Ecos; vampireSkills=63; weapons=7; forms=3; initial appearance; campaign and atlas saves disabled.");
        }
        void ConfigurePlaytestResources()
        {
            Progress.level=Mathf.Max(Progress.level,12);Progress.points=Mathf.Max(Progress.points,12);Progress.weaponUnlocks=7;
            Progress.vampireMastery=3;Progress.vampireSkills=63;Progress.vampireSelected=0;Progress.awakeningState=2;
            Progress.formsUnlocked=3;Progress.preferredForm=0;Progress.potions=9;Progress.blood=100;
            Progress.vampireRecoveries=new float[6];Progress.resonanceRecoveries=new float[3];Progress.resonanceCooldown=0;Progress.resonanceEnergy=100;Progress.mp3Unlocked=true;
        }
        void ResetPlaytestGarden()
        {
            StopPower();lab.Player.ResetModel();Progress.chapter=4;Progress.checkpoint=0;ConfigurePlaytestResources();LoadMap(4);
            // Use the completed original combat catalog while the aged action catalog is still in production.
            Appearance.PreviewStage(0);health.FullHeal();Blood.Set(100);
            Vector2 position=Map.SafeSpawn(Map.Arenas[0]+Vector2.down*5.5f);
            var body=lab.Player.GetComponent<Rigidbody2D>();body.position=position;lab.Player.transform.position=position;
            lab.WorldCamera.transform.position=(Vector3)position+new Vector3(0,.75f,-10);Physics2D.SyncTransforms();Close();
            Notice("SESSÃO DE TESTE — PODERES E TRÊS ARMAS LIBERADOS");
        }
        void RefillPlaytestPowers()
        {
            StopPower();Progress.blood=100;Progress.vampireRecoveries=new float[6];Progress.resonanceRecoveries=new float[3];Progress.resonanceEnergy=100;
            Blood.Restore(Progress);Resonance.Restore(Progress);Progress.potions=9;health.FullHeal();Close();Notice("VIDA, SANGUE E RECARGAS RESTAURADOS");
        }
        void TickPlaytestGuide()
        {
            if(playtestGuide==null)return;playtestGuideRoot.gameObject.SetActive(Active);
            playtestGuide.text="SESSÃO DE TESTE — SAVE PROTEGIDO\n"+lab.Input.BindingLabel("Vampire")+": "+DarklightVampireSkills.Names[Progress.vampireSelected]+"  "+Blood.Recovery[Progress.vampireSelected].ToString("0")+"s\n"+lab.Input.BindingLabel("VampireNext")+": ESCOLHER PODER   "+lab.Input.BindingLabel("WeaponNext")+": ARMA\n"+lab.Input.BindingLabel("Transform")+": FORMA   SANGUE "+Blood.Amount.ToString("0")+" / 100";
        }
        void BuildPlaytestTools()
        {
            heading.text="SESSÃO DE TESTE";bodyText.text="Sua campanha salva permanece preservada.\n\nPoderes vampíricos, formas e três armas liberados.\n\nAparência inicial: catálogo de combate completo.\nAs animações envelhecidas seguem em produção.";
            Button("REABASTECER VIDA E PODERES",RefillPlaytestPowers);
            Button("REINICIAR JARDIM E INIMIGOS",ResetPlaytestGarden);
            Button("VOLTAR",()=>Open("Pause"));
        }
    }
}
