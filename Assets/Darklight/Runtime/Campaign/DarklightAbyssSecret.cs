using System;
using UnityEngine;
namespace Darklight
{
    public sealed partial class DarklightCampaign
    {
        // Optional late-campaign discovery. Existing saves keep their chapter,
        // relics, boss state and three original weapon bits unchanged.
        public bool AbyssSecretAvailable=>!Training&&MapRegion==6&&Progress.chapter>=6&&(Progress.weaponUnlocks&8)==0&&(Progress.chapter>6||Progress.relics!=null&&Progress.relics.Length>6&&Progress.relics[6]);
        public Vector2 AbyssSecretPoint=>InteractionAnchor(Map.Relic+Vector2.right*.65f);
        bool InteractAbyssSecret(string id)
        {
            if(id!="abyss_motor")return false;
            if(!AbyssSecretAvailable)return true;
            int previous=Progress.weaponUnlocks;Progress.weaponUnlocks|=8;
            try
            {
                if(!DarklightAutomation.Silent&&!PlaytestSession)
                {
                    if(ExploringNewRegion)Progress.Save();else SaveCheckpoint();
                }
            }
            catch(System.IO.IOException e){Progress.weaponUnlocks=previous;Debug.LogWarning("Cavaleiro save: "+e.Message);Notice("NAO FOI POSSIVEL SALVAR A DESCOBERTA. TENTE NOVAMENTE.");return true;}
            Speak(new[]{"FÁBIO|Um motor selado entre memórias... As duas metades ainda respondem ao meu sangue.","FÁBIO|Cavaleiro do Abismo. Vou abrir meu próprio caminho."});
            Notice("ARMA SECRETA LIBERADA · CAVALEIRO DO ABISMO · SKIN ROCK");return true;
        }
    }
}
