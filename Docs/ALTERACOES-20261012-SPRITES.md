# DARKLIGHT: REQUIEM — correção por sprites completos

## Regra permanente

Gerar sprites novos e completos para corrigir arte. Não extrair, sobrepor ou remontar cabeça, corpo, membros ou armas, nem na preparação offline. A importação identifica somente quadros do personagem inteiro nas folhas, preservando os pixels da imagem. A regra foi salva em MEMORIA-DARKLIGHT.md e no AGENTS.md do projeto Unity. Esta entrega não reproduz aviso sonoro.

## Mudanças

- Removida a seleção de cabeças independentes e a composição offline rejeitada. Recursos dessas tentativas foram arquivados fora de Resources e excluídos da entrega pública.
- Geradas sete folhas completas pela ferramenta integrada image_gen: caminhada Réquiem nas idades jovem, adulta e madura; ataque de katana Rock nas quatro idades. São 224 quadros, quatro fases e oito direções por folha. Réquiem mantém sobretudo e mangas; Rock mantém colete curto e braços descobertos.
- Preservados os sprites completos já existentes de caminhada e repouso por idade, assim como as seis folhas fornecidas pelo usuário e os desenhos das três armas.
- Cabeça, corpo e arma permanecem no mesmo SpriteRenderer. O shader não apaga retângulos da cabeça. A aura segue o transform visual do personagem e fica atrás dele.
- As transformações vermelha, roxa e branca usam seletores RGB de olhos, roupa e cabelo, sem alterar a geometria ou separar partes. A idade lógica continua determinada pela gameplay.
- Importação Point, mipmaps desligados, sem compressão e FullRect. Pivôs nos pés e PPU calibrado pela altura do personagem inteiro; WorldVisualScale continua 1.325.
- Mantidas a integração das seis folhas do jovem, a trilha fornecida para o primeiro despertar e as correções de transição/menu necessárias à versão atual. Testes e capturas usam uma sessão protegida e silenciosa.

## Validação

Unity 6000.6.0f1, Built-in Render Pipeline. Gameplay: 69/69 testes aprovados. Projeto/importação: 74/74 aprovados. Build Windows: Succeeded, zero erros, um aviso; 2.037.883.379 bytes. Verificadas por SHA256 43 texturas anteriores, seis fontes fornecidas e sete novas folhas idênticas às imagens geradas. Os quatro arquivos de save permaneceram inalterados.

As capturas do executável e o resultado final da inspeção visual constam em verificacao-capturas.json e prints-final/verificacao.json. Capturas: 672 PNGs reais; nenhum renderer de cabeça separado ou apagamento retangular. Inspecionadas as direções cardinais de caminhada Réquiem e de ataque Rock, com as paletas de transformação. São capturas conduzidas pelo relógio de combate; não representam uma campanha manual completa nem um teste com controle físico.

## Cobertura e pendências reais

As novas folhas envelhecidas desta correção cobrem caminhada Réquiem com katana nas idades 1–3 e ataque Rock com katana nas idades 1–4. A caminhada Réquiem da idade 4 e o repouso/caminhada Rock usam folhas completas anteriores. Para outras ações/armas sem uma folha completa daquela idade, o jogo mantém a animação completa anterior; a aparência nela pode ser a idade da fonte. A progressão de idade da campanha não é reiniciada. Ainda falta produzir os conjuntos completos de envelhecimento dessas outras ações, sempre como personagens inteiros. Não foi declarado um catálogo novo completo de todas as combinações.

A ordem de associação dos quadros completos às direções foi conferida novamente; a folha permanece intacta.

## Arquivos para teste

Executável: `Windows/DarklightRequiem.exe`. Manter toda a pasta Windows ao mover o jogo. A versão antiga em execução foi preservada.

As folhas PNG, manifesto de importação e prompts usados estão em sprites-inteiros-20261012. A publicação usa exclusivamente o repositório `https://github.com/fwbiodev7/darklight-requiem-game.git`; saves, fontes pessoais de referência, builds, caches e arte rejeitada ficam fora do commit. O commit desta correção consta no histórico do próprio repositório.
