# DARKLIGHT: REQUIEM — repasse de arte Codex → Antigravity

## Missão

Receber as artes finalizadas pelo Codex, integrar somente o que ainda falta e validar Fabius Réquiem Normal. Não gerar outro redesign, ampliar a lista de golpes ou substituir arte aprovada.

**Workspace original:** `C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main`

**Pasta única de entrega:** `C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/outputs/entrega-artes-fabius-20261103/`

Leia o AGENTS.md e este documento. O objetivo é permitir troca de responsável, nunca duas integrações simultâneas.

## 1. Trava de responsabilidade — obrigatória

Antes de qualquer escrita no jogo, ler `CONTROLE-DE-INTEGRACAO.json` na pasta de entrega.

- `owner: Codex`, `state: PRODUCING`: apenas leitura e preparação técnica em `work/antigravity-fabius-integration/`. Não editar Assets, catálogo, runtime, cenas, metas ou publicação.
- `owner: Antigravity`, `state: RELEASED`: o Codex encerrou gerações/escritas e liberou a integração deste snapshot. Conferir hashes e processos antes de assumir.
- Arquivo ausente, estado desconhecido ou entrega incompleta: não presumir liberação por horário, silêncio ou esgotamento aparente de tokens. Solicitar a troca explícita de responsabilidade ao usuário.

Depois da liberação, registrar `state: INTEGRATING`, com responsável e horário. Não iniciar outro Editor se houver Unity, importação, build ou escrita Git ativa sobre o projeto. Não encerrar processos ou aplicativos do usuário sem autorização.

Se o Codex voltar enquanto você integra, ele deverá permanecer em leitura/preparação. Ao terminar, gravar `state: INTEGRATED_AWAITING_REVIEW` ou `BLOCKED`, listar evidências/pendências e liberar os arquivos. Retomada automática não autoriza assumir arquivos sob integração ativa de outro agente.

**Uma única pessoa/agente por vez controla Assets, Unity, catálogo e Git.** Não basta dividir nomes de scripts se ambos alteram o mesmo catálogo ou importam o mesmo atlas.

## 2. O que é uma entrega válida

O snapshot final deverá conter:

- `staging/`: PNGs nativos e descritores JSON revisados.
- `PAGES.json`: páginas de catálogo referentes somente às entregas revisadas.
- `manifest.json`: hashes, fontes, destinos propostos, estado de cópia/importação/teste e base de integração.
- `sources/`: fontes necessárias preservadas, quando incluídas; não importar a pasta inteira como recursos de gameplay.
- `PENDENCIAS.md`: candidatos excluídos, falhas conhecidas e próximos passos.
- Este prompt e o arquivo de controle de responsabilidade.

Enquanto o controle disser PRODUCING, a pasta pode conter somente um snapshot parcial. Não integrar um pacote em gravação. A versão RELEASED deve ter manifesto final e hashes estáveis.

Conferir SHA-256 de cada arquivo antes e depois de copiar. Fonte preservada não significa sprite aprovado: importar apenas os atlas/páginas explicitamente revisados. Não promover `candidate`, `superseded`, `rejected` ou revisão inconclusiva automaticamente. Não ignorar erros de contrato para acelerar a integração.

## 3. Recuperar o estado real

Ler incrementalmente:

- `Docs/Darklight/ESCOPO-ENXUTO-FABIUS-20261102.md`.
- `Docs/Darklight/CORRECAO-CANONICA-FORMAS-20261102.md`.
- `outputs/retomada-20261102/animation-progress-current.json`.
- `outputs/retomada-20261102/art-integration.json`.
- `outputs/retomada-20261102/release-progress.json`.
- O recibo Git mais recente e os checkpoints em `outputs/retomada-20261103/`.

Histórico relevante: 89 clips/324 quadros/33 folhas da sessão anterior foram importados. Os15 clips iniciais tiveram3 testes PlayMode isolados antes de mudanças posteriores. Isso não valida os novos estados, as alterações técnicas recentes ou a campanha completa.

Há artes e código aprovados localmente que ainda não foram publicados. **O GitHub não é automaticamente a cópia mais nova do trabalho.** Não usar pull/reset/checkout/clean para substituir o workspace original.

## 4. Como integrar sem quebrar os atlas

Usar o fluxo existente:

`Tools/Delivery/integrate_reviewed_20261031.py`

Primeiro conferir contrato, sem escrita em Assets:

```powershell
python Tools/Delivery/integrate_reviewed_20261031.py --staged outputs/entrega-artes-fabius-20261103/staging --pages outputs/entrega-artes-fabius-20261103/PAGES.json --names NOME_EXATO_DA_FOLHA --output outputs/retomada-20261102 --check-only
```

Trocar `NOME_EXATO_DA_FOLHA` pelo nome real do manifesto, sem extensão. Repetir somente para folhas que precisam de integração. Quando o contrato passar, executar a mesma linha sem `--check-only`.

Não reintegrar em massa clips já importados/testados: isso pode sobrescrever recibos de validação com estado pendente. Se o destino já possui o mesmo conteúdo e contrato, registrar como existente e seguir à etapa que falta.

Importar depois, com Unity fechada e guardas ativos:

```powershell
python Tools/Delivery/import_ready_packet_20261102.py --pending
```

Esse comando importa; não executa testes de gameplay nem gera build. Preservar GUIDs/metas existentes. Metas novas devem ser criadas pelo fluxo Unity apropriado, sem atribuir novamente IDs a sprites antigos.

**Não aplicar o fatiador de grade uniforme/PPU32 aos atlas atuais.** Eles podem usar retângulos variáveis, pivôs medidos e `framePixelsPerUnit`. Preservar resolução nativa, nomes Redesign, origem, densidades, tempos e flags de loop. O consumidor atual é o catálogo e seu banco de sprites; não é necessário fabricar um `.anim` genérico para cada página.

Não recortar membros, montar anatomia, espelhar assimetrias ou redimensionar PNGs. Reorganizar figuras completas conforme o manifesto é permitido. PPU expressa densidade de raster, não corrige proporções.

## 5. Fidelidade e feedback direto do usuário

- Réquiem é a única skin ativa de Fabius; Katana, Punhos e Moto.
- `WorldVisualScale = 1.325`. Corpo, cabeça e tronco não devem crescer/diminuir entre quadros. Não normalizar pela altura total de poses agachadas, armas ou cadáveres.
- Preservar as expressões de dano aprovadas: expressivas e bonitinhas, sem alterar identidade, cor da pele, olhos vermelho-escuros ou volume do cabelo.
- Moto Parry atual foi aprovada visualmente; não regenerar por preferência pessoal.
- O candidato antigo `SW-four-phase-inspection-only.gif` tinha oscilação de escala. Não importá-lo por engano. Usar apenas a versão final explicitamente revisada/liberada; tamanho corrigido não comprova alternância da passada.
- Caminhadas precisam de apoio alternado e passada baixa; duplicação/bobbing não contam como ciclo.
- Defesa/dano/morte são os únicos estados novos essenciais. Carregamento e variações cosméticas reaproveitam as famílias existentes.
- Fabius tem somente Normal, Ruptura e Eclipse Absoluto. A suposta forma Abissal foi removida e não deve voltar. Preservar Moto/Cavaleiro do Abismo e fontes históricas.
- Lucius mantém o cânone próprio do prompt oficial: olhos azuis, cabelo claro e arte própria, sem ser Fabius recolorido.

## 6. Faísca na defesa da Moto

O usuário pediu faíscas curtas laranja/prata no encontro das serras. Verificar a implementação atual antes de copiar qualquer versão anterior:

- `DarklightActionParticles`, `DarklightCombatImpacts`, `DarklightCharacterView` e `DarklightWeaponRedesignBank`.
- Páginas Moto Parry podem conter `guardContact = [u,v]`, coordenadas normalizadas no retângulo do sprite, origem inferior esquerda. Converter usando rect, pivot, PPU e transformação reais.
- Não usar o centro automático do personagem/atlas. Respeitar a oclusão quando o contato fica atrás do corpo.
- Reutilizar o pool, emitir uma vez por entrada de defesa, duração curta e menos partículas em ReducedFlashes. Sem spawn por frame, flash no rosto, mudança de paleta ou tremor de câmera por uma faísca pequena.

Arte aprovada, código escrito e efeito validado no runtime são estados diferentes; registrar cada um.

## 7. Memória, testes e evidências

Unity6000.6.0f1. Preservar os guardas de2,5GiB de RAM disponível e3GiB de commit disponível, além da folga de disco. Não baixar guardas, alterar pagefile ou fechar apps do usuário para forçar execução.

Duas importações recentes foram interrompidas de forma controlada por pressão de commit. O usuário informou que fecharia alguns aplicativos. Medir novamente antes da próxima execução; não presumir que a memória já foi liberada. O runner passou a limitar Bee com `BEE_BUILD_THREADS=1` somente no processo filho. Não repetir build pesada às cegas.

Depois de terminar a produção/importação, executar verificações direcionadas: compilação real, sprites/nomes/rects/pivôs/densidade, oito direções, escala estável, loops de caminhada, defesa/dano/morte, retorno ao idle, corpo final de morte sustentado e faísca da Moto. Validar também que o buffer externo não duplica comandos nem altera dodge e que hitstop não quebra as janelas.

Não tratar16 testes Python de ferramentas como aprovação do combate Unity. O preflight é útil, mas seu PASS anterior continha8 metas ausentes e401 referências não resolvidas como avisos. Contagem de370 arquivos C# não é compilação; separar avisos de pacotes/legado de erros reais.

Não alegar60FPS, Zero-GC, aprovação visual ou build atualizada sem medição/recibos. Não iniciar música. Usar saves isolados e preservar preferências, fontes, arte aprovada e última build válida.

## 8. Publicação e encerramento

Publicar exclusivamente pela cópia `work/publicacao-20261021`, branch `integracao-completa-20261026`, remoto `fwbiodev7/darklight-requiem-game`. A raiz Git legada não é destino de publicação.

Verificar branch/remoto/HEAD/status/index e usar allowlist explícita. Não incluir fontes privadas de referência, rejeitadas, caches, saves, MP3, builds, ZIPs ou outros projetos. Não contornar gates de publicação. Se não houver validação suficiente, preservar o trabalho local e registrar o bloqueio; checkpoint documental não equivale a runtime publicado.

Entregar recibo com arquivos aplicados/existentes/rejeitados/adaptados, hashes, testes realmente executados, falhas, commit/push e próxima prioridade. Encerrar sua posse dos arquivos no controle de integração.

Ordem posterior continua: concluir e validar Fabius → Gothic Eclipse → Lucius Normal → transformações → Combate IA2.0 → QA/build Windows final. Os pacotes Antigravity anteriores exigem nova comparação com o código atual e `git apply --check`; não aplicá-los todos nem sobrescrever melhorias mais recentes.


## Atualização de 03/11/2026 — retorno conferido

Antes de integrar, ler `Docs/Darklight/RETORNO-ANTIGRAVITY-E-RETOMADA-20261103.md`. Uma tentativa com uma só folha também parou por RAM no refresh de áudio, com 7.69 GiB de commit disponível. Não repetir aberturas apenas reduzindo a lista de sprites. As 16 folhas essenciais preservam seus hashes; as três caminhadas continuam pendentes e os candidatos novos não foram promovidos. Faíscas Moto continuam aguardando gameplay. Plano do prólogo salvo no GDD, para depois de Lucius Normal e antes das transformações. O arquivo de controle continua sendo a autoridade para troca de responsável.
