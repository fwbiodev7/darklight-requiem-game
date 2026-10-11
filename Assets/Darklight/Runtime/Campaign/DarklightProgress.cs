using System;
using System.IO;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed class DarklightProgress
    {
        public int version=1, chapter, checkpoint, level=1, xp, points, attack, vitality, mastery, potions=3, shards;
        public int mp3Quest, ending, formsUnlocked, preferredForm; public bool mp3Unlocked, completed;
        public float resonanceCooldown;
        public float resonanceEnergy=100; public int resonanceSelected; public float[] resonanceRecoveries;
        public bool[] relics=new bool[6];
        public int weaponUnlocks=1,equippedWeapon;
        public int appearanceStage,narrativeDays,appearanceChapter;
        public static string Path => System.IO.Path.Combine(Application.persistentDataPath,"requiem-campaign.json");
        public int NeededXP => 60+level*35;
        public int MaxHealth => 100+(level-1)*8+vitality*15;
        public int FormUnlocked => Mathf.Max(formsUnlocked,chapter>=5?3:chapter>=4?2:chapter>=1?1:0);
        public float Damage => 1+(level-1)*.055f+attack*.1f;
        public void Award(int amount)
        {
            xp+=amount; while(xp>=NeededXP) { xp-=NeededXP; level++; points++; }
        }
        public void Save()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string temp=Path+".tmp"; File.WriteAllText(temp,JsonUtility.ToJson(this,true));
            if(File.Exists(Path))File.Replace(temp,Path,Path+".bak");else File.Move(temp,Path);
        }
        public static DarklightProgress Load()
        {
            foreach(string path in new[]{Path,Path+".bak"})
                try {if(!File.Exists(path))continue;var state=Decode(File.ReadAllText(path));if(state!=null)return state;}catch(Exception e){Debug.LogWarning("Save: "+e.Message);}
            return null;
        }
        public static DarklightProgress Decode(string json)
        {
            var state=JsonUtility.FromJson<DarklightProgress>(json);
            if(state==null || state.version!=1 || state.chapter<0 || state.chapter>5 || state.level<1)return null;
            state.weaponUnlocks=(state.weaponUnlocks&7)|1;state.equippedWeapon=Mathf.Clamp(state.equippedWeapon,0,2);
            state.appearanceStage=Mathf.Clamp(state.appearanceStage,0,4);state.narrativeDays=Mathf.Clamp(state.narrativeDays,0,36500);state.appearanceChapter=Mathf.Clamp(state.appearanceChapter,0,5);
            state.checkpoint=Mathf.Clamp(state.checkpoint,0,4);state.potions=Mathf.Max(0,state.potions);
            if(state.relics==null || state.relics.Length!=6)state.relics=new bool[6];
            if(!json.Contains("\"resonanceEnergy\""))state.resonanceEnergy=100;
            state.resonanceEnergy=Mathf.Clamp(state.resonanceEnergy,0,100);state.resonanceSelected=Mathf.Clamp(state.resonanceSelected,0,2);
            return state;
        }
    }
    public enum RequiemEnemyKind { Crawler, Shooter, Armored, Hunter, Spectral, Kael, Cantor, Matriarch, Sentinel, Warden, Lucien }
    public static class DarklightStory
    {
        public static readonly string[] Chapters={"A NOITE DA RUPTURA","VOZES DO ABISMO","SANGUE DESPERTO","O PROJETO ECLIPSE","ALEM DO VEU","O ULTIMO ECLIPSE"};
        public static readonly string[] Places={"Rua das Cinzas","Distrito do Vinil","Catedral do Sangue","Laboratorio Eclipse","Jardim dos Ecos","Observatorio Vespera"};
        public static readonly string[] Bosses={"KAEL VOSS","CANTOR DO ABISMO","MATRIARCA RUBRA","SENTINELA ECLIPSE","GUARDIAO DO VEU","LUCIEN VEYR"};
        public static readonly string[][] Introductions={
            new[]{"FABIO|O eclipse apagou as luzes. Mas algo continua brilhando na minha mao...","LYRA|Sou Lyra. As criaturas vieram das fendas. Siga a rua; Kael sabe o que e essa espada.","FABIO|Requiem. Se voce me escolheu, vamos tirar essas pessoas daqui."},
            new[]{"LYRA|Os Ecos repetem vozes de pessoas desaparecidas. A origem esta no distrito do vinil.","FABIO|Vou procurar os sinais. Ninguem merece ser esquecido no escuro.","LYRA|Icaro Nox tem uma loja a esquerda da praca. Ele entende frequencias que nao deviam existir."},
            new[]{"LYRA|Seu sangue reagiu aos registros da catedral. Voce nasceu com uma ponte para o Veu.","FABIO|Entao essa escuridao tambem e minha. Mas eu escolho o que fazer com ela.","LYRA|A Matriarca guarda a memoria da primeira Ruptura. Quebre seus selos."},
            new[]{"KAEL|Lucien criou o Projeto Eclipse para domesticar o Veu. Eu fui sua primeira arma.","FABIO|Voce pode escolher de novo. Abra o caminho e eu desligo o nucleo.","LYRA|Tres servidores sustentam o laboratorio. A Sentinela protege o ultimo."},
            new[]{"FABIO|A cidade esta do outro lado... Aqui as memorias viram pedra.","LYRA|O sinal esta fraco. Procure os fragmentos e mantenha o portal aberto.","KAEL|O Guardiao exige um preco. Nao entregue a ele o que faz de voce humano."},
            new[]{"LUCIEN|Voce trouxe todos os fragmentos. Finalmente o ultimo eclipse sera eterno.","FABIO|Eu nao sou seu recipiente. Vespera vai ver o sol outra vez.","LYRA|Desative os condutores. Quando Lucien cair, escolha o destino da ponte."}
        };
        public static readonly string[][] BossLines={
            new[]{"KAEL|Nao e a espada que decide quem voce e. Mostre que consegue enfrenta-la.","FABIO|Entao pare de falar e venha."},
            new[]{"CANTOR|Cada voz perdida canta dentro de mim. Voce tambem vai cantar.","FABIO|Eu vou devolver essas vozes."},
            new[]{"MATRIARCA|Seu sangue lembra o caminho. Fique comigo, filho da Ruptura.","FABIO|Meu sangue nao decide o meu futuro."},
            new[]{"SENTINELA|Portador nao autorizado. Protocolo Eclipse: erradicacao.","KAEL|Ele preve sua primeira investida. Varie seus ataques."},
            new[]{"GUARDIAO|A ponte pede uma memoria. Qual delas voce abandonara?","FABIO|Nenhuma. Eu vou abrir passagem com Requiem."},
            new[]{"LUCIEN|Sem mim, o Veu vai devorar tudo!","FABIO|Voce confundiu controle com salvacao. Acabou."}
        };
        public static readonly string[][] Conclusions={
            new[]{"KAEL|A Ruptura despertou. Controle-a antes que ela controle voce.","LYRA|Ha registros no distrito. Agora temos uma chance."},
            new[]{"LYRA|As vozes apontaram para a catedral. O projeto comecou muito antes de Lucien.","FABIO|Entao vamos descobrir quem abriu a primeira porta."},
            new[]{"FABIO|A memoria era minha mae protegendo a cidade... Ela nao fugiu.","LYRA|E voce nao precisa repetir o sacrificio dela."},
            new[]{"KAEL|O nucleo caiu. Voce aprendeu a unir luz e sombra: Eclipse.","LYRA|Lucien atravessou. Sigam-no antes que o portal feche."},
            new[]{"FABIO|O Guardiao deixou a ponte aberta. Agora entendo: Absoluto e equilibrio.","KAEL|Volte para casa. Eu mantenho o caminho."},
            new[]{"LUCIEN|Eu... so queria que a perda deixasse de existir.","FABIO|Apagar a vida nao cura a perda. A escolha termina aqui."}
        };
    }
}
