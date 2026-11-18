# Loading gótico e mascotes — entrega de 17/11/2026

## Escopo e autorizações

O usuário autorizou expressamente iniciar esta tarefa apesar da cota semanal abaixo da reserva. A exceção vale para este loading, não para antecipar fases ou remasterizar mapas. Em seguida aprovou as duas folhas geradas, pediu mesclar as referências de menus ao loading atual, corrigir vazamento de texto, enriquecer a barra e mostrar GIFs reais da fase 1 para a fase 2.

Base local: `35cdfdf`. O controlador existente `DarklightCampaignLoading.cs`, Canvas, prévias dos dez destinos, textos narrativos, progresso e callbacks de transição foram reutilizados. Nenhuma lógica de gameplay, save, mapa, combate, áudio ou duração do loading foi alterada.

## Arte produzida e integrada

- **Gato Ruptura:** oito quadros com pelagem escura, mechas vermelhas, olhos amarelo-esverdeados, sobretudo e bolinha de lã; poses de observar, levantar/tocar com a pata, acompanhar e retornar, com variação de orelhas/cabeça/rabo.
- **Mini Fabius:** oito quadros de caminhada no lugar, mãos vazias, cabelo escuro/vermelho, botas e sobretudo preto/carmesim; variação de pernas, braços, cabelo e roupa. Mascote chibi específico da UI; nenhuma alteração de Fabius Normal/pivôs.
- **Fundo gótico:** cenário/ornamentos próprios para o loading, inspirado nos menus aprovados, com eclipse, arquitetura, molduras violetas e espaços sem texto para conteúdo dinâmico. Sem textos/botões/personagens embutidos na imagem.
- Ferramenta de geração: **image_gen integrada**. Prompts completos em `Tools/Validation/LoadingMascots20261117/generation.json`; hashes e dimensões em `assets.json` e `packing.json`.
- As folhas originais tinham 1774×887. O empacotamento técnico preservou os pixels RGBA de oito células inteiras, adicionando apenas padding para um atlas regular 1776×888 (células 444×444). Sem redimensionamento, recoloração ou montagem anatômica. Fontes geradas preservadas localmente em `ArtSources/LoadingMascots20261117/selected/`.
- Referências privadas WhatsApp/menu permaneceram fora da publicação. Os seis menus anexados foram consultados visualmente; a geração do fundo recebeu diretamente menu principal, configurações e mapa. Não foram executadas as instruções de mod CatJump impressas na referência do gato.

Assets finais em `Assets/Darklight/Resources/Darklight/Loading/`: `Mascots/gato-ruptura.png`, `Mascots/mini-fabius.png` e `gothic-loading-20261117.png`. Filtro Point, sem mipmaps/compressão, alpha nas duas folhas e sem distorção das células.

## Integração e acabamento

`DarklightLoadingMascot` é uma apresentação independente do carregamento. Reutiliza um RawImage de 36×36 unidades no Canvas 640×360, no canto inferior esquerdo junto ao estado. Sorteia gato/mini em 50/50 no OnEnable, mantém a escolha enquanto visível, percorre oito UVs sem criar objetos por quadro e usa tempo não escalado. Ao esconder o loading, encerra a animação e libera a referência visual. Um mesmo objeto é reutilizado nas próximas passagens.

O tema ajusta somente a apresentação dentro do loading: prévia com aspecto preservado na moldura esquerda, presságio com margem interna à direita, status/porcentagem separados e mascote fora da barra/textos. O presságio usa largura 164 e fonte 7, com quebra e truncamento corretos. As dez mensagens canônicas foram verificadas no mesh real do texto. A textura dinâmica da fonte permanece Point após reconstrução do atlas.

A barra passou a ter moldura dupla violeta, interior escuro, preenchimento carmesim com reflexo e um pequeno cristal na ponta. Continua mostrando o progresso real. O fallback visual anterior permanece disponível se o asset gótico não existir. Nenhuma espera artificial foi adicionada; as esperas já presentes no controlador foram preservadas.

## Validação própria

**Quatro testes únicos aprovados: 2 EditMode + 2 PlayMode.** Os PlayMode foram repetidos após alterações de texto/barra e correção da nitidez da fonte; não contam como testes únicos adicionais.

- Divisão do intervalo de seleção em duas metades, incluindo fronteira 0,5.
- Assets Point, sem compressão/mipmaps, transparência e atlas de oito células quadradas.
- As duas variantes selecionadas pelo OnEnable com amostras determinísticas, sem trocar durante a exibição; todos os oito frames alcançados com `Time.timeScale=0`; parada ao esconder e reutilização sem duplicar RawImages.
- Duas transições reais **Rua das Cinzas → Distrito do Vinil**, uma capturada com cada variante fixada apenas pelo teste. Runtime normal mantém sorteio. Prévia correta, progresso/retorno ao gameplay e callbacks íntegros; mascote não sobrepõe texto/barra/prévia/presságio; dez presságios cabem nos limites; fonte Point.
- Execuções silenciosas, armazenamento de campanha isolado em `work/loading-mascots-20261117/`, preferências restauradas e saves reais preservados conforme receipts do runner existente.
- Nenhuma falha de teste, erro novo de compilação C# ou exceção de gameplay encontrada nos logs finais. Verificação visual das duas variantes e da captura final de texto/barra realizada.

Evidências locais:

- `outputs/loading-mascots-20261117/editmode-20261117T113102.xml` — 2/2, exit code 0.
- `outputs/loading-mascots-20261117/playmode-20261117T113839.xml` — 2/2 final, exit code 0; `.summary.json` confirma saves preservados/preferências restauradas.
- Logs `work/loading-mascots-edit-20261117.log` e `work/loading-mascots-play-final-20261117.log`.
- Capturas finais `outputs/loading-mascots-20261117/gato-ruptura.png` e `mini-fabius.png`.
- GIFs reais `phase1-phase2-cat.gif` e `phase1-phase2-mini.gif`, com tempos registrados em `phase1-phase2-*/timestamps.json`. O conversor usa os frames capturados e a duração real, arredondada aos centésimos do GIF, sem prolongar o loading.
- Prévias separadas dos ciclos: `gato-ruptura-loop.gif` e `mini-fabius-loop.gif`. São prévias do atlas, distintas das capturas de gameplay.

Validação executada no Editor Unity 6000.6.0f1 em Windows. O novo runtime usa APIs UI multiplataforma e código Editor fica em pasta/assembly Editor. Não foi gerada uma nova build standalone Windows; a build funcional existente foi preservada.

## Preservação e próximo trabalho

Sem alteração do site, das seis telas de menu, dos mapas, de Fabius Normal/pivôs, dos saves reais, MP3 ou fontes privadas. Alterações concorrentes em `.codex/config.toml` e caches Python foram preservadas fora do commit.

O pedido separado **Map Remaster 2.0** foi salvo em `Docs/Darklight/MAP-REMASTER-2-PRIORIDADE-20261119.md` e apontado no Prompt Mestre. Está aguardando a próxima sessão, cota real, horário mínimo e fase autorizada; nenhuma pesquisa/auditoria/remasterização de mapas foi iniciada aqui.
