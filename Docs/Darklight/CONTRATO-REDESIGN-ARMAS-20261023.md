# Redesign de armas — banco aditivo de personagens completos

Data: 23/10/2026. Esta infraestrutura não declara o redesign inteiro concluído. Cada página é uma cobertura exata de skin, arma, forma, direção e ação. O relatório de cobertura distingue figuras novas, contextos restantes, importação e verificação em execução.

## Consumidor e preservação

`DarklightCharacterView` seleciona a apresentação aprovada existente e as variações de repouso. Em seguida, `DarklightWeaponRedesignBank` consulta apenas o contexto exato. Quando encontra uma página revisada e importada, apresenta o novo quadro completo. Não recorta/transplanta cabeça, olhos, corpo, mão ou arma, não espelha direções, não modifica colisores, eventos, dano ou relógio do combate. Nenhum catálogo, PNG ou GUID anterior é substituído.

Quando a cobertura nova não existe, a seleção existente continua no mesmo contexto solicitado. Isso é continuidade da arte anterior, **não** implementação nova nesse contexto. Não é permitido emprestar outro desenho de arma, skin, forma ou direção para aumentar a cobertura declarada. Ícones anteriores permanecem até existir arte dedicada, sem retirar armas dos corpos gerados.

O sprite novo contém a forma final. Após a apresentação de aura/efeitos, `ApplyAuthoredOwnership` limpa a máscara de proprietário e a transformação cromática anterior (`_WholeFrameId=0`, máscara preta, `_VampireForm=0`, `_CanonicalSkinShift=0`). Assim, uma máscara velha não apaga a adaga e a paleta de Absoluto não converte uma figura já desenhada. A aura, eventos e parâmetros de gameplay permanecem do sistema existente.

## Identidades

| Campo | Valores |
|---|---|
| `skin` | 0 Réquiem, 1 Rock |
| `weapon` | 0 katana Réquiem, 1 Presas do Caos, 2 Punhos da Ruína |
| `form` | 0 Normal, 1 Ruptura, 2 Eclipse, 3 Absoluto |
| `direction` | 0 S, 1 SO, 2 O, 3 NO, 4 N, 5 NE, 6 L, 7 SE |
| `action` | `Idle`, `Walk`, `Run`, `Attack`, `Charging`, `Dodge`, `Parry`, `Hurt`, `Dead`, `Transformation`, `IdleAlert`, `IdleImpatient`, `IdleInspect`, `IdleFatigued`, `IdleWounded`, `StartMove`, `Brake`, `Turn` |

As transições/variações de repouso são ações próprias. Uma página `Idle` não substitui automaticamente `IdleWounded`, `Brake` ou `Transformation`. Em preview sem variação de repouso, o modelo escolhe `Idle` ou `Walk` pela movimentação. Arma da moto não pertence a este banco.

## Folha física

PNG e JSON lado a lado em `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`. Cada PNG pode conter várias direções; as páginas do catálogo apontam os nomes correspondentes. Fonte original intacta em `ArtSources/WeaponRedesign20261023/`.

```json
{
  "schemaVersion": 1,
  "image": "ReqDaggersNormalIdleA.png",
  "imageSha256": "SHA-256 real do PNG entregue, 64 caracteres hexadecimais",
  "sourcePath": "ArtSources/WeaponRedesign20261023/ReqDaggersNormalIdleA-original.png",
  "sourceSha256": "SHA-256 real da fonte preservada, 64 caracteres hexadecimais",
  "sourceSize": [1536, 1024],
  "nativeSize": [1024, 1024],
  "pixelsPerUnit": 128,
  "status": "visual-reviewed",
  "wholeFigures": true,
  "frames": [
    {"id": "Redesign_ReqDaggersNormalIdleA_00", "rect": [0, 0, 128, 256], "pivot": [0.5, 0.08]}
  ]
}
```

As dimensões e o PPU do exemplo são ilustrativos. `nativeSize` é o tamanho real do PNG entregue, não uma resolução lógica presumida. `sourceSize` registra a fonte original separadamente. `rect` tem origem superior esquerda e formato x,y,largura,altura; `pivot` é normalizado com origem inferior esquerda, medido dentro do quadro completo. Não sobrepor retângulos. Organizar as figuras inteiras no atlas não autoriza mudar os pixels ou montar anatomia.

Medir cabelo até o apoio dos pés e comparar à figura aprovada da mesma identidade, sem incluir a altura de efeitos ou armas levantadas. A auditoria de adagas existentes mediu S Normal 188px/94,80725PPU = 1,98297 unidades e S Ruptura 164px/83,638855PPU = 1,96081 unidades, antes da escala global 1,325. Usar a medida real da fonte e altura aprovada para obter PPU coerente; não copiar o PPU ilustrativo. Pivô deve continuar no apoio dos pés, estável entre quadros.

O importador confere os hashes reais, os tamanhos PNG, as referências fonte/folha, retângulos, pivôs e nomes antes de importar. Usa Point, sem mipmaps/compressão, RGBA32, NPOT None, Clamp, mesh FullRect e limite 8192. Preserva os IDs Unity por nome em reimportações. Não reescreve YAML `.meta`.

## Catálogo de cobertura

Arquivo: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/catalog.json`.

```json
{
  "schemaVersion": 1,
  "complete": false,
  "fallbackPolicy": "approved-existing-same-context",
  "pages": [{
    "id": "ReqDaggersNormalIdleS",
    "skin": 0, "weapon": 1, "form": 0, "direction": 0,
    "action": "Idle", "attackKey": "",
    "resourcePath": "Darklight/WeaponRedesign20261023/ReqDaggersNormalIdleA",
    "imageSha256": "mesmo SHA-256 real da folha",
    "sourcePath": "ArtSources/WeaponRedesign20261023/ReqDaggersNormalIdleA-original.png",
    "sourceSha256": "mesmo SHA-256 real da fonte",
    "sourceSize": [1536, 1024], "nativeSize": [1024, 1024],
    "pixelsPerUnit": 128,
    "status": "visual-reviewed", "wholeFigures": true, "loop": true,
    "spriteNames": ["Redesign_ReqDaggersNormalIdleA_00", "Redesign_ReqDaggersNormalIdleA_01", "Redesign_ReqDaggersNormalIdleA_02", "Redesign_ReqDaggersNormalIdleA_03"],
    "durationMs": [350, 250, 180, 350],
    "sourceSpriteNames": []
  }]
}
```

`resourcePath` não inclui extensão. Hashes/dimensões/PPU devem coincidir com o JSON físico. O banco carrega apenas folhas necessárias à apresentação. Nomes de figura iniciam com `Redesign_`. Não duplicar o contexto/ação/`attackKey`. `status=candidate` é recusado pelo consumidor; revisão visual é uma etapa real e não um rótulo atribuído automaticamente.

### Repouso e locomoção

`durationMs` define a duração positiva de cada quadro. `loop=false` segura o último; `loop=true` repete o ciclo. Uma página de uma figura é repouso estático direcional, não animação de respiração. Quatro figuras realmente distintas podem animar quatro durações sem inventar intermediárias. O relógio é o modelo existente ou o relógio da variação de repouso.

### Contatos e demais ações

Ataque, carga, esquiva, defesa, dano, morte e transformação exigem `sourceSpriteNames` não vazio. Cada entrada é o nome exato do sprite selecionado pelo consumidor anterior, antes de aplicar a transformação cromática. `spriteNames` possui o mesmo número de entradas. Essa correspondência conserva as fases e os eventos do combate; `durationMs` não reamostra ataques.

É permitido repetir um **nome de saída** em vários mapeamentos para segurar uma pose nova em múltiplas fases legadas, desde que a fonte correspondente esteja explicitamente registrada. Os **nomes fonte** permanecem únicos e os IDs físicos da folha também. Exemplo: oito nomes fonte distintos podem mapear, pela ordem comprovada, para quatro desenhos novos `[novo0,novo0,novo1,novo1,novo2,novo2,novo3,novo3]`. Isso é hold explícito, não oito desenhos. Nunca presumir que os índices de contato são iguais em todas as páginas; consultar `startupFrameEnd`, `activeFrameEnd` e a seleção do ataque real.

`attackKey` opcional corresponde exatamente a `AttackDefinition.displayName`. Página nominal tem prioridade; uma página genérica ainda precisa conter o nome fonte exato da fase atual. Se a fase não estiver mapeada, permanece a figura aprovada existente no contexto atual.

## Validação e limites

- Importar pelo entry point `Darklight.Editor.DarklightWeaponRedesignImporter.Import`, sem rodar em paralelo com outra Unity no mesmo projeto.
- `DarklightWeaponRedesignTests` verifica descritores, fontes, quatro durações de idle, fases exatas de ataque, ausência de empréstimo entre contextos e limpeza das máscaras de forma nos assets importados.
- `python Tools/AnimationPreview/audit_weapon_redesign.py` gera `outputs/redesign-armas-20261023/coverage.json`. Lista os 1536 contextos centrais (2 skins × 3 armas × 4 formas × 8 direções × 8 ações), separa os faltantes e inventaria os bancos anteriores. Um contexto presente ainda não prova que todos os combos ou variações tenham cobertura nova.
- Contato, anatomia, olhos, pele, empunhadura, pernas alternadas, volume do equipamento e leitura em movimento exigem inspeção real. Point e integridade de hashes não certificam pixel art ou coerência artística.
- Testar renderização/gameplay da expansão e áudio em rodadas coordenadas. Não alegar controle físico ou inspeção sonora se não executados.

## Ícones dedicados opcionais

`EquipmentIcons.png` e `EquipmentIcons.json` ficam na mesma pasta Resources e obedecem ao contrato da folha física: fonte original intacta, hashes reais, dimensões nativas, PPU e retângulos/pivôs medidos. `wholeFigures=true` neste caso significa objetos de equipamento completos, gerados separadamente dos personagens. A folha pode conter até 12 IDs físicos únicos: `Redesign_Icon_Weapon0_Form0` até `Redesign_Icon_Weapon2_Form3`. Não precisa criar páginas de personagem para os ícones.

O importador inclui essa folha quando seu JSON existe, confere os 12 nomes permitidos e preserva os IDs Unity na reimportação. A primeira rodada pode acontecer sem ela. `DarklightWeaponRedesignBank.Icon(weapon,form,fallback)` retorna apenas o ícone do equipamento/forma exatos quando o JSON está revisado e o sprite importado existe; na ausência dele, usa o ícone anterior. O slot 3 da moto continua com seu próprio ícone.

O HUD da arma equipada, os botões do inventário e os guias de armas/combos usam esse resolver. `WeaponDefinition.icon`, IDs das armas e assets antigos permanecem intactos. A forma apresentada determina o ícone após o momento de liberação da transformação; não cria uma troca antecipada durante a preparação.

## Testes preparados

Filtro EditMode: `DarklightWeaponRedesignTests` (sete testes de contrato/importação, incluindo ícones opcionais). Filtro PlayMode: `DarklightWeaponRedesignGameplayTests` (três testes de consumo/renderização em campanha). A execução é coordenada pela tarefa principal; escrever um teste não significa que ele passou.

O teste obrigatório de adagas exige Normal e Ruptura, S/SO/O, quatro quadros novos distintos por contexto, com captura pela câmera real do treino. Verifica retorno após ataque, mudanças de arma/skin/forma/direção e ausência de espelhamento ou máscara antiga. As páginas adicionais de Idle/Walk/Attack presentes no catálogo são exercitadas nos seus contextos exatos e ataques correspondentes. Usa saves de teste dentro de `work/`, confere hashes dos saves reais e preferências, desliga áudio e mantém a campanha pausada enquanto avança o relógio do combate de forma determinística. Isso não equivale a teste manual, controle físico ou audição.

Capturas e resultados dessa rodada ficam em `outputs/redesign-armas-20261023/unity-play/`.

O relatório de entrega deve mencionar skins/formas/direções/ações realmente integradas, número de desenhos únicos e holds, teste executado e os contextos restantes. Fontes preservadas e prévias candidatas não equivalem a integração concluída.
