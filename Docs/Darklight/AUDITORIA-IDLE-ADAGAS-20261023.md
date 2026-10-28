# Auditoria do idle das adagas Réquiem — 23/10/2026

Estado: diagnóstico estático concluído. Nenhuma arte, banco, shader ou gameplay alterado por esta auditoria. Nenhum Play, Unity ou som iniciado. A inspeção visual usou as figuras inteiras e as duas referências completas.

## Causa e limites da evidência

- Não existe página `Idle`, `Motion` ou `Idle*` da arma `1`/skin `0` no catálogo atual. Os idles vivos de 17/10 foram importados somente para arma `0`. `TodayCharacterBank.Resolve` tenta `Idle`, cai em `Combat` e alterna as fases **0 e 5** de cada linha a cada **0,5 s**, usando o relógio de combate (`Runtime/Character/TodayCharacterCatalog.cs:97–118`). `LivingPose` não encontra substituto para as adagas (`:141–153`). Não há idle de seis fases/piscar das adagas em produção.
- As fontes atuais já desenham duas lâminas curvas compridas, vermelhas inclusive na forma Normal. A silhueta diverge das novas Presas do Caos, curtas, serrilhadas, com ponta e guarda distintas. O texto anexado determina metal escuro com energia vermelha para Réquiem, embora a ficha geral rotule sua coluna neutra como Normal; seguir o pedido textual e preservar a identidade cromática aprovada. Selecionar uma máscara diferente não redesenha a arma. A correção solicitada requer figuras completas novas.
- Os fios ondulados vermelho/laranja visíveis nas laterais da captura são compatíveis com a aura procedural da forma **Ruptura** (`DarklightSmallTransformationAura.cs:18–24,63–70`); a malha existe separadamente atrás do personagem e é dimensionada pela referência frontal da katana. Não provam vazamento de atlas. A captura isolada não contém o estado serializado para certificar a forma ou a fase exata.
- A auditoria dos **64 quadros selecionados** (oito direções × quatro formas × duas fases) encontrou **zero pixels com alpha ≥166 pertencendo a outro owner** e zero divergência dimensional imagem/máscara. Isso afasta uma máscara de ownership errada como causa dessas lâminas compridas. Não substitui validação por renderização Unity.
- Não há transformação anatômica em runtime nesse caminho. O shader altera pigmentos/íris e descarta ownership; a arte de cada forma é substituída inteira. Escala visual do renderer é fixa `1.325` (`DarklightCharacterView.cs:6–7,87`).

## Seleção exata atual

Direções: `0 S`, `1 SO`, `2 O`, `3 NO`, `4 N`, `5 NE`, `6 L`, `7 SE` (`DarklightCharacterView.cs:48`). As páginas normais são `RequiemCombatDaggersYoungCardinal`/`Diagonal`. Nome normal completo: `Today_<página>_<índice>`.

| Direção | Página normal | Índices normais | Índices de suporte nas formas |
|---|---|---|---|
| S (0) | Cardinal | 0, 5 | 0, 1 |
| SO (1) | Diagonal | 0, 5 | 4, 5 |
| O (2) | Cardinal | 8, 13 | 8, 9 |
| NO (3) | Diagonal | 8, 13 | 12, 13 |
| N (4) | Cardinal | 16, 21 | 16, 17 |
| NE (5) | Diagonal | 16, 21 | 20, 21 |
| L (6) | Cardinal | 24, 29 | 24, 25 |
| SE (7) | Diagonal | 24, 29 | 28, 29 |

Formas: `0 Normal`, `1 Ruptura`, `2 Eclipse`, `3 Absoluto`.

- Ruptura: `Darklight/TransformedYoung/RequiemCombatSupportDaggersYoung_Crimson`, nomes `Transformed_RequiemCombatSupportDaggersYoung_Crimson_<índice>`.
- Eclipse, seis direções: mesmo padrão `_Violet`.
- Eclipse S/SO: substituições de 20/10 em `Darklight/PlayerVisualRevision20261020/RequiemDaggersViolet8`, nomes `RequiemDaggersViolet8_0,1` para S e `_4,5` para SO. Os demais quadros desta folha são poses de esquiva, sem seleção no idle.
- Absoluto: mesmo padrão de suporte `_White`.
- `ownerId` é índice físico da figura na respectiva textura +1; não o índice virtual da página Combat. O `TransformedCatalog` armazena essas referências explícitas. A ordem virtual não pode ser ordenada lexicalmente pelo nome nem pelo sufixo do atlas de destino.

PPUs atuais: Normal Cardinal `94.80725`; Normal Diagonal `100.3991`; suporte Crimson `83.638855`; suporte Violet `87.41215`; suporte White `83.88705`; folha Eclipse nova `177.38693467336682`. Os pivôs são por quadro, próximos ao chão; valores e retângulos exatos estão no inventário abaixo. Um PPU desses bancos não deve ser copiado arbitrariamente para uma nova fonte com resolução diferente.

Inventários preservados em `outputs/redesign-armas-20261023/auditoria-idle-adagas-bindings.json` (32 combinações, 64 seleções, seis texturas) e `auditoria-idle-adagas-selectors.json` (ownership/seletores por figura). São evidência de leitura dos arquivos atuais, não novos bancos nem aprovação artística.

### Amostragem comparativa S / SO / O

Os nomes abaixo são os sprites físicos exatos das primeiras fases atuais. As segundas fases estão entre parênteses. Nas formas Crimson/White/Violet antigas o incremento é +1; no Normal é +5.

| Forma | S | SO | O |
|---|---|---|---|
| Normal | `Today_RequiemCombatDaggersYoungCardinal_0` (`_5`) | `Today_RequiemCombatDaggersYoungDiagonal_0` (`_5`) | `Today_RequiemCombatDaggersYoungCardinal_8` (`_13`) |
| Ruptura | `Transformed_RequiemCombatSupportDaggersYoung_Crimson_0` (`_1`) | `Transformed_RequiemCombatSupportDaggersYoung_Crimson_4` (`_5`) | `Transformed_RequiemCombatSupportDaggersYoung_Crimson_8` (`_9`) |
| Eclipse | `RequiemDaggersViolet8_0` (`_1`) | `RequiemDaggersViolet8_4` (`_5`) | `Transformed_RequiemCombatSupportDaggersYoung_Violet_8` (`_9`) |
| Absoluto | `Transformed_RequiemCombatSupportDaggersYoung_White_0` (`_1`) | `Transformed_RequiemCombatSupportDaggersYoung_White_4` (`_5`) | `Transformed_RequiemCombatSupportDaggersYoung_White_8` (`_9`) |

### Medida da escala atual

Medida direta na imagem nativa, sem modificar pixels: mínimo/máximo Y dos pixels da figura com alpha ≥166 e owner correto; altura inclusiva. Em repouso as armas não ultrapassam cabelo/sola nessas amostras. A medida é da silhueta opaca completa, não um rig anatômico nem o tamanho do retângulo com margem.

| Forma | S: pixels / PPU = unidades locais | SO: pixels / PPU = unidades locais | O: pixels / PPU = unidades locais |
|---|---|---|---|
| Normal | 188 / 94.80725 = **1.98297** | 201 / 100.3991 = **2.00201** | 191 / 94.80725 = **2.01461** |
| Ruptura | 164 / 83.638855 = **1.96081** | 159 / 83.638855 = **1.90103** | 167 / 83.638855 = **1.99668** |
| Eclipse | 353 / 177.38693467336682 = **1.99000** | 357 / 177.38693467336682 = **2.01255** | 182 / 87.41215 = **2.08209** |
| Absoluto | 164 / 83.88705 = **1.95501** | 165 / 83.88705 = **1.96693** | 169 / 83.88705 = **2.01461** |

A escala mundial final multiplica esses valores por `1.325`. Exemplo: Normal S = **2.62744 unidades**; Eclipse S = **2.63675 unidades**. O banco legado já varia entre direções/formas; não propagar essa variação para o novo atlas. A altura local de referência **1.99 unidades** é compatível com o repouso normal e com a revisão Eclipse de 20/10. Calcular o novo PPU como `altura cabeça–sola medida na nova fonte / 1.99`, usando um valor fixo por atlas e poses com anatomia consistente. Se as novas figuras tiverem oscilações de altura indevidas, corrigir a arte inteira; não reescalar cada quadro isolado.

`auditoria-idle-adagas-alturas.json` preserva as 24 amostras, altura nativa, PPU, posição Y na fonte, altura local/mundial e pivô exato. Não redimensiona nem extrai partes.

## Contrato para o idle substituto de figuras completas

1. Identidade `skin=0, age=0, weapon=1`, `kind=Idle`. O consumidor atual aceita **duas fases** ou **seis fases com `idleBlinkFrame=5`**; uma única fase não passa `ValidDescriptor` (`TodayCharacterCatalog.cs:44–63`). Para respeitar lotes de oito figuras: uma folha Cardinal com quatro direções × duas poses, e outra Diagonal igual, por forma. São 16 figuras por forma, 64 para as quatro formas. Não anunciar oito direções animadas com uma única folha de oito poses.
2. Nominar páginas inéditas/versionadas e manter o contrato `resourcePath="Darklight/TodayCharacterAnimations/"+name`, máscara `...+"_Palette"`; nomes `Today_<name>_<i>` contíguos. Ordem de linha = ordem declarada de `directions`; índice = `row*phases+phase`. Cardinal `[0,2,4,6]`; Diagonal `[1,3,5,7]`.
3. Cada figura contém cabeça, corpo, braços, mãos e **ambas** as adagas completas. Não recortar/transplantar arma ou cabeça. Preservar vestuário, idade, pele aprovada e íris vermelho escuro `#C40814`/sombra `#5A0000`. Eclipse mantém cabelo/cristais roxos; Absoluto mantém cabelo branco. Réquiem usa metal escuro e energia vermelha conforme o prompt textual; cores de energia das outras formas seguem sua identidade aprovada e a ficha nova. Nenhum espelhamento para inventar direções assimétricas.
4. Calibrar um PPU fixo por atlas pela altura cabeça–sola da figura completa, comparada à pose aprovada; nunca adaptar escala do renderer por fase. Registrar pivô no mesmo apoio dos pés e em coordenadas inteiras da imagem. `TodayCharacterImporter.Frame` usa retângulo Unity com origem inferior esquerda e pivô normalizado (`x,y,w,h,pivotX,pivotY`). Evitar margens ou armas que contaminem a estimativa da altura corporal.
5. `ownerMask=true`. Máscara tem mesma resolução/UV da textura, canal alpha inteiro igual ao `ownerId` daquela figura; deve ser importada linear, Point, sem compressão/mips, `alphaIsTransparency=false`. Canal R seleciona íris e só pode marcar a anatomia correta após inspeção; os demais materiais não devem receber o vermelho da íris. Os seletores existentes não certificam anatomia por frequência de cores.
6. Folhas de cada forma usam `DarklightTransformedPage` com `baseName` igual ao novo nome Normal, `form=1/2/3`, nomes de sprites correspondentes, `frameIds` físicos e sua máscara `_Owner`. A correspondência entre fases deve ser um a um. `DarklightOriginalVampireView` atribui sprite, máscara e owner juntos (`:24–29`).
7. Reutilizar `DarklightLivingIdleImporter.ImportManifest(path)` (`Editor/DarklightLivingIdleImporter.cs:28–73`): a lógica é genérica por skin/weapon/kind, embora o comentário antigo cite katana. Exige as três formas completas por página Normal e preserva páginas Combat/Death. `TodayCharacterImporter.ImportTextureFrames` preserva spriteID por nome/previousName, configura Point/FullRect/uncompressed e verifica contagem/nomes (`:53–107`). Manter GUIDs e fontes legadas. Reimportar somente a entrega nova e limpar caches dos bancos.
8. Timing de duas fases: 0,5 s cada; ciclo de 1 s. Timing de seis fases: cinco respirações a cada 0,52 s; piscar ocasional em 6–6,13 s e 14,12–14,25 s do ciclo de 14,3 s (`DarklightIdleFrameTiming.cs:8–15`). `LivingPose` usa `BreathClock`, com cadência modulada por vida/forma. Nenhum evento de dano/combate depende desse idle.
9. Validar oito direções e quatro formas em transições Idle↔Walk/Attack/Dodge/Parry, alinhamento dos pés, ambas as armas, ausência de mudança de identidade e olhos. Revisar contato em 1× e ampliado e animação real antes de marcar `runtime-verified`. Prévia candidata não conta como integração.

Atualização de coordenação: a nova geração usa **três direções S/SO/O × quatro poses** por folha. O integrador criou `DarklightWeaponRedesignBank`, banco adicional com duração explícita por fase, sem modificar `TodayCharacterBank.ValidDescriptor` nem reimportar os bancos legados. O novo contrato usa `Redesign_<sheet>_<índice>`, `durationMs=[350,250,180,350]` e páginas de contexto exato por arma/forma/direção. A terceira pose é um piscar desenhado. As outras cinco direções permanecem legadas até existir arte correspondente. A auditoria acima documenta o comportamento anterior; o novo banco requer verificação própria no Unity.

## Referências inspecionadas

- `C:/Users/Usuario/Downloads/Ficha de Equipamentos Abissais em Pixel Art.png`, 1536×1024, SHA-256 `6C8C6B05E27F16F19F2BFCAD3151294B164F004C0960FA713662788F9E3F6930`.
- `C:/Users/Usuario/Downloads/WhatsApp Image 2026-10-23 at 3.20.04 PM.jpeg`, 1600×800, SHA-256 `CCDDC7AE6098080E518C1D5FF51C3E2A6606917B42F8B1596CC42FD4123DC4B5`.
- `C:/Users/Usuario/Pictures/Screenshots/Captura de tela 2026-10-23 191153.png`, 175×310, SHA-256 `12DD9A6FCC1A41B75501BB910844FC29F6F1D5B6059D6280A7B4721CA4E46039`.

Tamanho das referências não certifica resolução nativa de pixel art. Os arquivos originais não foram alterados.
