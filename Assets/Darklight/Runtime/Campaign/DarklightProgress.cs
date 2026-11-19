using System;
using System.IO;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed partial class DarklightProgress
    {
        // Zero is an undecided relationship in pre-duel saves.
        public int luciusStance, luciusFate;
        public int difficulty; public bool extremeUnlocked;
        public int version=2, chapter, checkpoint, level=1, xp, points, attack, vitality, mastery, potions=3, shards;
        public int mp3Quest, ending, formsUnlocked; public int preferredForm=1; public bool mp3Unlocked, completed;
        public float resonanceCooldown;
        public float resonanceEnergy=100; public int resonanceSelected; public float[] resonanceRecoveries;
        public bool[] relics=new bool[ChapterCapacity];
        public int weaponUnlocks=1,equippedWeapon;
        public int legacyEquippedWeapon=-1,legacyPreferredForm=-1;
        public int appearanceStage,narrativeDays,appearanceChapter;
        public static string Path => IsolatedTestPath ?? System.IO.Path.Combine(Application.persistentDataPath,"requiem-campaign.json");
        public int NeededXP => 60+level*35;
        public int MaxHealth => 100+(level-1)*8+vitality*15;
        // This edition starts with every form available; story milestones remain independent.
        public int FormUnlocked => DarklightPlayableScope.FinalForm;
        public float Damage => 1+(level-1)*.055f+attack*.1f;
        public void Award(int amount)
        {
            xp+=amount; while(xp>=NeededXP) { xp-=NeededXP; level++; points++; }
        }
        public void Save()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            string temp=Path+".tmp"; File.WriteAllText(temp,JsonUtility.ToJson(this,true));
            if(File.Exists(Path))ReplaceWithTransientRetry(temp,Path,Path+".bak");else File.Move(temp,Path);
        }
        static void ReplaceWithTransientRetry(string source,string destination,string backup)
        {
            for(int attempt=0;;attempt++)
            {
                try{File.Replace(source,destination,backup);return;}
                // Windows can transiently refuse removal of the replaced file
                // (1175). Retry only this observed error, keeping the atomic save
                // and backup operation; all other/exhausted failures still escape.
                catch(IOException error) when((error.HResult&0xffff)==1175&&attempt<3&&File.Exists(source)&&File.Exists(destination))
                {System.Threading.Thread.Sleep(10<<attempt);}
            }
        }
        public static DarklightProgress Load()
            => LoadFromPath(Path);
        // The same reader can validate imported/local backup files without
        // touching the player's active save location.
        public static DarklightProgress LoadFromPath(string savePath)
        {
            if(string.IsNullOrWhiteSpace(savePath))return null;
            foreach(string path in new[]{savePath,savePath+".bak"})
                try {if(!File.Exists(path))continue;var state=Decode(File.ReadAllText(path));if(state!=null)return state;}catch(Exception e){Debug.LogWarning("Save: "+e.Message);}
            return null;
        }
        public static DarklightProgress Decode(string json)=>DecodeExpansion(json);
    }

    public enum RequiemEnemyKind { Crawler, Shooter, Armored, Hunter, Spectral, Kael, Cantor, Matriarch, Sentinel, Warden, Lucien, Valtherion, Seraphine, KaelFractured, Nocthar }
    public static class DarklightStory
    {
        public static readonly string[] Chapters=new[]{"A NOITE DA RUPTURA","VOZES DO ABISMO","SANGUE DESPERTO","O PROJETO ECLIPSE","ALEM DO VEU","O ULTIMO ECLIPSE","HERANÇA CARMESIM","A CIDADE SEM AMANHECER","O TRONO DO VAZIO","DARKLIGHT: REQUIEM"};
        public static readonly string[] Places=new[]{"Rua das Cinzas","Distrito do Vinil","Catedral do Sangue","Laboratorio Eclipse","Jardim dos Ecos","Observatorio Vespera","Cripta dos Primeiros","Catedral do Crepúsculo","Véu Profundo","Coração do Eclipse"};
        public static readonly string[] Bosses=new[]{"NERO","CANTOR DO ABISMO","MATRIARCA RUBRA","SENTINELA ECLIPSE","GUARDIAO DO VEU","LUCIEN VEYR","VALTHERION — PRIMEIRO GUARDIÃO","SERAPHINE — RAINHA DA NOITE","NERO FRATURADO","NOCTHAR — REI DO VAZIO"};
        public static readonly string[][] Introductions={
            new[]{"FABIUS|O eclipse apagou as luzes. Mas algo continua brilhando na minha mão...","LADY|As fendas cercaram a rua. Vamos fechar os três selos e tirar as pessoas daqui.","FABIUS|Réquiem. Se você me escolheu, vamos abrir esse caminho juntos."},
            new[]{"LADY|Os Ecos repetem vozes de pessoas desaparecidas. A origem está no distrito do vinil.","FABIUS|Vou procurar os sinais. Ninguém merece ser esquecido no escuro.","LADY|Ícaro Nox tem uma loja à esquerda da praça. Ele entende frequências que não deviam existir."},
            new[]{"LADY|Seu sangue reagiu aos registros da catedral. Você nasceu com uma ponte para o Véu.","FABIUS|Então essa escuridão também é minha. Mas eu escolho o que fazer com ela.","LADY|A Matriarca guarda a memória da primeira Ruptura. Quebre seus selos."},
            new[]{"LADY|Estes registros ligam o Projeto Eclipse ao observatório. Três servidores mantêm a passagem fechada.","FABIUS|Vou desligar o núcleo e descobrir quem ainda o controla.","LADY|Eu acompanho os sinais daqui. Volte inteiro."},
            new[]{"FABIUS|A cidade está do outro lado... Aqui as memórias viram pedra.","LADY|O sinal está fraco. Procure os fragmentos e mantenha o portal aberto.","FABIUS|Ainda consigo ouvir você. Vou seguir esse caminho."},
            new[]{"LADY|Os condutores levam a energia da cidade para o Véu. A assinatura de Lucius aparece em cada registro.","FABIUS|Ele conhecia esta espada antes de mim. Quero ouvir o que tem a dizer.","LADY|Abra a passagem pelo observatório. Eu procuro os registros que faltam."},
            new[]{"LADY|Os registros do observatório apontam para uma cripta mais antiga que o projeto.","FABIUS|Quero saber quem deixou essas inscrições.","LADY|A linhagem que protegia a fronteira usava o símbolo da sua espada. Procure a memória ancestral."},
            new[]{"LADY|Há sobreviventes cercados na praça e um abrigo que ainda resiste.","FABIUS|Primeiro tiramos as pessoas daqui. Depois entramos na catedral.","LADY|Vou conduzir quem conseguir atravessar. Proteja a entrada."},
            new[]{"LADY|Os três selos abrem o caminho para Nero. Ele procura uma saída desde que a corrupção tomou o Véu Profundo.","NERO|Há uma voz dentro dessas pedras. Se ela me alcançar, lembre quem eu era.","FABIUS|Vou quebrar essa corrupção. Você sai daqui comigo."},
            new[]{"NERO|A corrupção se foi. As fendas do Coração do Eclipse continuam alimentando a passagem.","LADY|Leve nossas memórias. Você não precisa enfrentar Lucius sozinho.","FABIUS|Vou encerrar o eclipse. A escolha será nossa."}
        };
        public static readonly string[][] BossLines={
            new[]{"LADY|Os símbolos da rua mostram a ordem. Vamos fechar estes selos.","FABIUS|Eu protejo a passagem."},
            new[]{"ÍCARO NOX|A frequência certa enfraquece a fenda. Afaste os Ecos do mecanismo.","FABIUS|Vou devolver essas vozes."},
            new[]{"MATRIARCA|Seu sangue lembra o caminho. Fique comigo, filho da Ruptura.","FABIUS|Meu sangue não decide o meu futuro."},
            new[]{"LADY|A última patrulha protege os condutores. Desative o circuito.","FABIUS|Vamos abrir a passagem."},
            new[]{"LADY|Observe o caminho entre os fragmentos. O Véu esconde a saída à vista de todos.","FABIUS|Vou encontrar a passagem."},
            new[]{"LADY|As criaturas se juntaram no último condutor.","FABIUS|Então soltamos a cidade desse circuito."},
            new[]{"LADY|Estes guardiões foram tomados pela fenda. Liberte a passagem para a memória.","FABIUS|Meu sangue não me obriga a repetir os erros deles."},
            new[]{"LADY|O abrigo está quase livre. Há uma última patrulha na catedral.","FABIUS|Ninguém vai ficar para trás."},
            new[]{"NERO|A voz... ela já está aqui. Não deixe que me transforme em mais uma arma.","FABIUS|Vou quebrar a corrupção. Você volta comigo."},
            new[]{"NERO|Eu seguro a passagem. Feche as fendas antes de entrar.","LADY|Depois delas, só resta Lucius."}
        };
        public static readonly string[][] Conclusions={
            new[]{"LADY|Os selos responderam ao seu sangue. Os registros do distrito podem explicar isso.","FABIUS|Primeiro uma rua segura. Agora temos uma chance."},
            new[]{"LADY|As vozes apontaram para a catedral. O projeto começou muito antes do eclipse.","FABIUS|Então vamos descobrir quem abriu a primeira porta."},
            new[]{"FABIUS|A memória era minha mãe protegendo a cidade... Ela não fugiu.","LADY|E você pode continuar o que ela começou."},
            new[]{"LADY|O núcleo caiu. Os sinais do observatório ficaram claros.","FABIUS|Vamos seguir a energia antes que outra fenda se abra."},
            new[]{"FABIUS|A ponte continua aberta. Equilíbrio ainda exige escolha.","LADY|Volte para casa. Eu mantenho o sinal aceso."},
            new[]{"LADY|Os condutores desligaram, mas a origem está além do observatório.","FABIUS|Vamos levar estes registros à cripta."},
            new[]{"FABIUS|A memória da linhagem atravessou o eclipse.","LADY|Vespera precisa desse equilíbrio. A passagem para o abrigo está aberta."},
            new[]{"FABIUS|Todos chegaram ao abrigo. Agora podemos entrar no Véu Profundo.","LADY|Há um espadachim preso do outro lado. A corrupção ainda pode ser rompida."},
            new[]{"NERO|Eu ouvi você no escuro. Você podia ter terminado a luta... e escolheu me trazer de volta.","FABIUS|Nenhum de nós nasceu para ser uma arma. Abra o caminho. Vamos acabar com isso."},
            new[]{"NERO|As fendas fecharam. O caminho até Lucius está livre.","LADY|Você tem a nossa confiança, Fabius. Volte para nós."}
        };
    }
}
