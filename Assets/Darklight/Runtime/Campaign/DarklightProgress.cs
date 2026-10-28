using System;
using System.IO;
using UnityEngine;

namespace Darklight
{
    [Serializable] public sealed partial class DarklightProgress
    {
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
        public static readonly string[] Bosses=new[]{"KAEL VOSS","CANTOR DO ABISMO","MATRIARCA RUBRA","SENTINELA ECLIPSE","GUARDIAO DO VEU","LUCIEN VEYR","VALTHERION — PRIMEIRO GUARDIÃO","SERAPHINE — RAINHA DA NOITE","KAEL — CAVALEIRO FRATURADO","NOCTHAR — REI DO VAZIO"};
        public static readonly string[][] Introductions={
            new[]{"FABIO|O eclipse apagou as luzes. Mas algo continua brilhando na minha mao...","LYRA|Sou Lyra. As criaturas vieram das fendas. Siga a rua; Kael sabe o que e essa espada.","FABIO|Requiem. Se voce me escolheu, vamos tirar essas pessoas daqui."},
            new[]{"LYRA|Os Ecos repetem vozes de pessoas desaparecidas. A origem esta no distrito do vinil.","FABIO|Vou procurar os sinais. Ninguem merece ser esquecido no escuro.","LYRA|Icaro Nox tem uma loja a esquerda da praca. Ele entende frequencias que nao deviam existir."},
            new[]{"LYRA|Seu sangue reagiu aos registros da catedral. Voce nasceu com uma ponte para o Veu.","FABIO|Entao essa escuridao tambem e minha. Mas eu escolho o que fazer com ela.","LYRA|A Matriarca guarda a memoria da primeira Ruptura. Quebre seus selos."},
            new[]{"KAEL|Lucien criou o Projeto Eclipse para domesticar o Veu. Eu fui sua primeira arma.","FABIO|Voce pode escolher de novo. Abra o caminho e eu desligo o nucleo.","LYRA|Tres servidores sustentam o laboratorio. A Sentinela protege o ultimo."},
            new[]{"FABIO|A cidade esta do outro lado... Aqui as memorias viram pedra.","LYRA|O sinal esta fraco. Procure os fragmentos e mantenha o portal aberto.","KAEL|O Guardiao exige um preco. Nao entregue a ele o que faz de voce humano."},
            new[]{"LUCIEN|Voce trouxe todos os fragmentos. Finalmente o ultimo eclipse sera eterno.","FABIO|Eu nao sou seu recipiente. Vespera vai ver o sol outra vez.","LYRA|Desative os condutores. Quando Lucien cair, descubra quem alimenta o eclipse."},
            new[]{"LYRA|Lucien caiu, mas o eclipse não terminou. Os registros apontam para uma cripta muito mais antiga que o projeto.","FÁBIO|Não vamos confundir silêncio com vitória. Quero saber quem deixou essas inscrições.","LYRA|A linhagem que protegia a fronteira usava o mesmo símbolo da sua espada. Entre na cripta e procure a memória ancestral."},
            new[]{"KAEL|O sol desapareceu. Há sobreviventes cercados na praça e um abrigo que ainda resiste.","FÁBIO|Primeiro tiramos as pessoas daqui. Depois entramos na catedral.","LYRA|Seraphine está usando o eclipse para despertar algo além do Véu. Não deixe que ela decida por você."},
            new[]{"LYRA|Os três selos antigos abrem o caminho para o trono. O Véu Profundo guarda versões distorcidas das nossas memórias.","KAEL|Há uma voz dentro dessas pedras. Se ela me alcançar, lembre quem eu era.","FÁBIO|Eu não vou abandonar você. Lucien servia a uma coisa maior: Nocthar."},
            new[]{"KAEL|A corrupção se foi, mas ainda consigo ouvir Nocthar. O Coração do Eclipse está tentando fundir os mundos.","LYRA|Leve nossas memórias. Você não precisa destruir metade de si para salvar a outra.","FÁBIO|Katana, punhos e motocicleta. Tudo o que aprendi me trouxe até aqui. Agora a escolha é minha."}
        };
        public static readonly string[][] BossLines={
            new[]{"KAEL|Nao e a espada que decide quem voce e. Mostre que consegue enfrenta-la.","FABIO|Entao pare de falar e venha."},
            new[]{"CANTOR|Cada voz perdida canta dentro de mim. Voce tambem vai cantar.","FABIO|Eu vou devolver essas vozes."},
            new[]{"MATRIARCA|Seu sangue lembra o caminho. Fique comigo, filho da Ruptura.","FABIO|Meu sangue nao decide o meu futuro."},
            new[]{"SENTINELA|Portador nao autorizado. Protocolo Eclipse: erradicacao.","KAEL|Ele preve sua primeira investida. Varie seus ataques."},
            new[]{"GUARDIAO|A ponte pede uma memoria. Qual delas voce abandonara?","FABIO|Nenhuma. Eu vou abrir passagem com Requiem."},
            new[]{"LUCIEN|Sem mim, o Veu vai devorar tudo!","FABIO|Voce confundiu controle com salvacao. Acabou."},
            new[]{"VALTHERION|Eu vi guardiões virarem tiranos. Prove que sua linhagem ainda sabe conter o próprio sangue.","FÁBIO|Meu sangue não me obriga a repetir seus erros."},
            new[]{"SERAPHINE|A humanidade é uma fome sem fim. Entregue-se e verá como o sangue é simples.","FÁBIO|Meu sangue não decide quem eu sou."},
            new[]{"KAEL|A voz... ela já está aqui. Não deixe que me transforme em mais uma arma.","FÁBIO|Vou quebrar a corrupção. Você volta comigo."},
            new[]{"NOCTHAR|Você é apenas uma aberração entre dois mundos.","FÁBIO|Não. Eu sou a prova de que eles podem coexistir."}
        };
        public static readonly string[][] Conclusions={
            new[]{"KAEL|A Ruptura despertou. Controle-a antes que ela controle voce.","LYRA|Ha registros no distrito. Agora temos uma chance."},
            new[]{"LYRA|As vozes apontaram para a catedral. O projeto comecou muito antes de Lucien.","FABIO|Entao vamos descobrir quem abriu a primeira porta."},
            new[]{"FABIO|A memoria era minha mae protegendo a cidade... Ela nao fugiu.","LYRA|E voce nao precisa repetir o sacrificio dela."},
            new[]{"KAEL|O nucleo caiu. Voce aprendeu a unir luz e sombra: Eclipse.","LYRA|Lucien atravessou. Sigam-no antes que o portal feche."},
            new[]{"FABIO|O Guardiao deixou a ponte aberta. Equilibrio ainda nao e poder absoluto.","KAEL|Volte para casa. Eu mantenho o caminho."},
            new[]{"LUCIEN|Eu... so queria que a perda deixasse de existir.","FABIO|Apagar a vida nao cura a perda. Mas o eclipse continua. Lyra, precisamos dos registros dele."},
            new[]{"VALTHERION|Você escolheu quando a força parecia mais fácil. A memória da linhagem agora é sua.","LYRA|Vespera precisa desse equilíbrio. O eclipse avançou sobre a catedral."},
            new[]{"FÁBIO|Ela tentou transformar cada pessoa em combustível. Nós ainda estamos aqui.","KAEL|A última Ruptura está aberta. Vamos juntos. Não quero que você carregue isso sozinho."},
            new[]{"KAEL|Eu ouvi você no escuro. Você podia ter terminado a luta... e escolheu me trazer de volta.","FÁBIO|Nenhum de nós nasceu para ser uma arma. Abra o caminho. Vamos acabar com isso."},
            new[]{"NOCTHAR|Sem a fusão, os mundos voltarão a se ferir.","LYRA|Equilíbrio não é ausência de escolha. Fábio, este futuro pertence a quem vai vivê-lo."}
        };
    }
}
