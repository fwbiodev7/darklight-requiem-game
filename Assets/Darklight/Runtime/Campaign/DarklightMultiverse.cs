using System.Collections.Generic;
using UnityEngine;

namespace Darklight
{
    public enum DarklightVisitor { None, MossDetonator, NeonRevenant, AshRonin }
    public readonly struct DarklightMultiverseEntry
    {
        public readonly int Chapter,Art;public readonly string Title,Text;public readonly DarklightVisitor Challenge;
        public DarklightMultiverseEntry(int chapter,int art,string title,string text,DarklightVisitor challenge=DarklightVisitor.None)
        {Chapter=chapter;Art=art;Title=title;Text=text;Challenge=challenge;}
    }
    public static class DarklightMultiverseCatalog
    {
        public static readonly DarklightMultiverseEntry[] Entries={
            new(0,0,"PROCURA-SE: STEVE","Viajante ilegal pelo multiverso dos games. Levou três paredes de Vespera no bolso. Diz que eram só blocos. Se oferecer um mod de viagem, peça passagem de volta."),
            new(0,1,"PORTAL DOS BLOCOS","Um mineiro esqueceu a porta aberta. Algo verde está rangendo do outro lado. Interaja novamente depois de ler para desafiar o Detonador Musgoso.",DarklightVisitor.MossDetonator),
            new(2,2,"O HOMEM DO FIM","Um Enderman foi visto carregando um altar inteiro. Ao lado, olhos brancos riscados na pedra: Herobrine esteve aqui? O vigia não aceita confirmar."),
            new(2,3,"FERRAMENTA EXTRAVIADA","Uma picareta de diamante, quatro blocos e um bilhete: não escave para baixo. Alguém também desenhou anéis e um ouriço correndo por mundos de cubos."),
            new(1,4,"CLÍNICA DA ÚLTIMA NOITE","Night City não consta no mapa. V procura um implante; Johnny Silverhand reclama do rádio; Adam Smasher deve três portas à oficina. Um espectro cibernético protege o terminal. Leia e interaja outra vez para enfrentá-lo.",DarklightVisitor.NeonRevenant),
            new(3,5,"SANTUÁRIO DA RAPOSA","O vento trouxe o nome Jin Sakai. Nenhuma raposa conduz ao mesmo lugar duas vezes. Um ronin de cinzas espera um duelo sem exército. Leia e interaja novamente para aceitar.",DarklightVisitor.AshRonin),
            new(4,6,"O FANTASMA SEM SENHOR","Um viajante de Tsushima deixou a faixa do Fantasma. O recado fala de honra, fumaça e uma katana que só sai da bainha quando o silêncio termina."),
            new(5,7,"DESCANSO DAS CINZAS","Um cavaleiro de Dark Souls deixou uma fogueira. Um Maculado de Elden Ring escreveu: tente descansar. Abaixo: tente de novo. A terceira mensagem só diz que a dificuldade não paga aluguel."),
            new(6,8,"PROCURA-SE: ARTHUR MORGAN","Dutch prometeu um plano para sair do Véu. Arthur pediu que primeiro pagasse a conta do cavalo. A recompensa está escrita em uma moeda que Vespera não conhece."),
            new(7,9,"CONTRATO: LOBO BRANCO","Geralt de Rívia deixou duas espadas desenhadas: aço para homens, prata para monstros. O contratante perguntou qual resolve síndicos do multiverso."),
            new(8,10,"PEDRA DO ÚLTIMO GRITO","Um Dragonborn de Skyrim gritou para abrir a porta. A porta permaneceu fechada. Três guardas pediram afastamento por problemas no joelho."),
            new(9,11,"AGÊNCIA ENTRE MUNDOS","Dante deixou a guitarra; Vergil procura Yamato; Nero quer a oficina de volta. O bilhete sobre Devil Trigger avisa: nenhuma transformação cobre danos à motocicleta.")
        };
    }
    public sealed partial class DarklightProgress
    {
        public int multiverseRead,multiverseDefeated;
        public bool ReadMultiverse(int index)
        {if(index<0||index>=DarklightMultiverseCatalog.Entries.Length)return false;int bit=1<<index;if((multiverseRead&bit)!=0)return false;multiverseRead|=bit;return true;}
        public bool ClaimVisitor(DarklightVisitor visitor)
        {if(visitor<DarklightVisitor.MossDetonator||visitor>DarklightVisitor.AshRonin)return false;int bit=1<<(int)visitor;if((multiverseDefeated&bit)!=0)return false;multiverseDefeated|=bit;Award(80);shards++;return true;}
    }
    public sealed partial class DarklightCampaign
    {
        readonly Dictionary<int,Vector2> multiversePoints=new();
        readonly List<DarklightEnemy> visitors=new();
        readonly HashSet<int> visitorStarted=new();
        bool maelisPresent;Vector2 maelisPoint;
        public IReadOnlyList<DarklightEnemy> MultiverseEnemies=>visitors;
        internal static Sprite[] MultiverseFrames(string name)
        {
            var sprites=Resources.LoadAll<Sprite>("Darklight/Multiverse/"+name);
            System.Array.Sort(sprites,(a,b)=>string.CompareOrdinal(a.name,b.name));return sprites;
        }
        void BuildMultiverse(int chapter)
        {
            multiversePoints.Clear();visitors.Clear();visitorStarted.Clear();maelisPresent=false;if(Training)return;
            var props=MultiverseFrames("Relics");
            for(int i=0;i<DarklightMultiverseCatalog.Entries.Length;i++)
            {
                var e=DarklightMultiverseCatalog.Entries[i];if(e.Chapter!=chapter||e.Art>=props.Length)continue;
                // Use existing walkable anchors; props do not add collision across paths.
                Vector2 point=Map.SafeSpawn(i%2==0?Map.Relic+Vector2.left*3:Map.Ally+Vector2.right*3);
                multiversePoints.Add(i,point);AddProp("VestígioMultiversal_"+i,props[e.Art],point,e.Art==1?2.5f:1.7f);
            }
            if(chapter==0||chapter==1||chapter==3)
            {
                var frames=MultiverseFrames("Maelis");if(frames.Length!=8)return;
                maelisPoint=Map.SafeSpawn(Map.Ally+Vector2.left*3);maelisPresent=true;
                var npc=new GameObject("MaelisCartografaDasFendas");npc.transform.SetParent(levelRoot,false);npc.transform.position=maelisPoint;
                var r=npc.AddComponent<SpriteRenderer>();r.sharedMaterial=pixelMaterial;r.sprite=frames[0];r.sortingOrder=100-Mathf.RoundToInt(maelisPoint.y*10);
                npc.AddComponent<DarklightNPCView>().frames=frames;
            }
        }
        void ConsiderMultiverse(System.Action<string,string,Vector2,float> consider)
        {
            foreach(var p in multiversePoints)consider("multiverse_"+p.Key,DarklightMultiverseCatalog.Entries[p.Key].Title,p.Value,2.5f);
            if(maelisPresent)consider("maelis","FALAR: MAELIS, CARTÓGRAFA DAS FENDAS",maelisPoint,2.5f);
        }
        bool InteractMultiverse(string id)
        {
            if(id=="maelis")
            {
                Speak(new[]{"MAELIS|Sou cartógrafa. Meus mapas eram confiáveis até começarem a mudar de universo.",
                    "MAELIS|Os cartazes e santuários guardam ecos de viajantes. Leia o vestígio e interaja outra vez para aceitar um desafio. Derrotá-los rende uma memória; a recompensa não se repete.",
                    "MAELIS|Primeiro ouvi blocos rangendo. Depois, anúncios de Night City. Agora um ronin só responde ao vento. Nenhum deles pertence ao Véu.",
                    "FÁBIO|Então eu fecho as fendas. Você cuida para não perdermos o caminho."});return true;
            }
            if(!id.StartsWith("multiverse_",System.StringComparison.Ordinal))return false;
            if(!int.TryParse(id.Substring(11),out int index)||!multiversePoints.TryGetValue(index,out var point))return true;
            var e=DarklightMultiverseCatalog.Entries[index];
            if(Progress.ReadMultiverse(index))
            {Progress.Award(15);SaveCheckpoint();Speak(new[]{"VESTÍGIO DO VÉU|"+e.Text});return true;}
            if(e.Challenge==DarklightVisitor.None){Speak(new[]{"VESTÍGIO DO VÉU|"+e.Text});return true;}
            if((Progress.multiverseDefeated&(1<<(int)e.Challenge))!=0){Notice("ECO PACIFICADO · RECOMPENSA JÁ RECOLHIDA");return true;}
            if(visitorStarted.Contains(index)){Notice("O VIAJANTE AINDA ESTÁ POR PERTO");return true;}
            if(SpawnVisitor(e.Challenge,point+Vector2.down*1.6f))visitorStarted.Add(index);
            return true;
        }
        bool SpawnVisitor(DarklightVisitor visitor,Vector2 position)
        {
            var frames=MultiverseFrames(visitor.ToString());
            if(frames.Length!=8){Notice("ENCONTRO SEM ARTE COMPLETA");return false;}
            var obj=new GameObject(visitor.ToString());obj.transform.SetParent(levelRoot,false);obj.transform.position=Map.SafeSpawn(position);
            var art=new GameObject("Sprite").AddComponent<SpriteRenderer>();art.transform.SetParent(obj.transform,false);art.sharedMaterial=pixelMaterial;art.sprite=frames[0];
            var body=obj.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
            obj.AddComponent<CircleCollider2D>().radius=.3f;
            var hp=obj.AddComponent<HealthSystem>();obj.AddComponent<DarklightCombatant>();
            var enemy=obj.AddComponent<DarklightEnemy>();enemy.kind=visitor==DarklightVisitor.MossDetonator?RequiemEnemyKind.Crawler:RequiemEnemyKind.Hunter;
            enemy.poses=new[]{frames[0],frames[1],frames[2],frames[3]};enemy.attackFrames=new[]{frames[4],frames[4],frames[5],frames[6]};
            enemy.ConfigureVisitor(visitor,frames[7]);hp.SetMaxHealth(130+Progress.chapter*12,false);visitors.Add(enemy);AddEnemyBar(enemy);
            Notice(visitor==DarklightVisitor.MossDetonator?"DETONADOR MUSGOSO":visitor==DarklightVisitor.NeonRevenant?"ESPECTRO NEON":"RONIN DAS CINZAS");return true;
        }
        bool RewardVisitor(DarklightEnemy enemy)
        {
            if(enemy.Visitor==DarklightVisitor.None)return false;
            if(Progress.ClaimVisitor(enemy.Visitor)){health.SetMaxHealth(Progress.MaxHealth,false);SaveCheckpoint();Notice("ECO PACIFICADO · +80 XP · +1 FRAGMENTO");}
            return true;
        }
    }
}
