# Redesign — integração das fontes pendentes de 23/10, auditoria de 24/10/2026

Preparação e auditoria estática concluídas. A importação, os testes e a build Unity são coordenados pela tarefa principal; este relatório não transforma a existência do PNG ou de uma prévia em confirmação de execução.

## Cobertura exata preparada

O inventário `outputs/redesign-armas-20261024/coverage.json` registra **17 folhas, 159 figuras físicas, 158 figuras referenciadas por runtime e 48 páginas de contexto**, sem erro estrutural. Uma figura de retorno do soco Normal continua somente no atlas/prévia. Não há holds explícitos de fases legadas. Existem ainda **12 ícones dedicados** de equipamento, separados das figuras dos personagens.

| Skin/arma | Formas | Direções | Ações novas | Páginas |
|---|---|---|---|---:|
| Réquiem/adagas | Normal, Ruptura, Eclipse, Absoluto | S/SO/O/NO/N/NE/L/SE | Idle | 32 |
| Réquiem/katana | Normal, Ruptura, Eclipse, Absoluto | S | Idle e ataque `Leve1` | 8 |
| Réquiem/punhos | Normal, Ruptura, Eclipse, Absoluto | S | Idle e ataque `Direto da Ruína` | 8 |

Adagas Normal possuem quatro figuras distintas em cada direção. Ruptura/Eclipse/Absoluto possuem quatro figuras S/SO/O e duas figuras NO/N/NE/L/SE. Dois desenhos bem definidos são dois desenhos; não são apresentados como um ciclo de quatro poses. Os clips declaram sua duração própria.

Dos 1536 contextos centrais do inventário genérico, 48 possuem algum clip novo e 1488 continuam nas apresentações existentes. Essa conta não certifica todos os combos de um contexto. Rock, caminhada/corrida, chutes, uppercut, slam, esquiva, defesa, dano, morte e variações de repouso ainda precisam de desenhos próprios para completar o redesign. Não há empréstimo de outra forma, arma, skin ou direção.

## Ataque dos punhos e filtro de transformação

A causa encontrada era ausência de páginas novas `Attack` para Ruptura/Eclipse/Absoluto. Só Normal S tinha a página. As outras formas seguiam para `DarklightRuinFistsBank`, cujo banco legado usa a mesma geometria Normal com canais de paleta para Ruptura/Eclipse. Alterar apenas máscara ou matiz não substituía a anatomia/equipamento por uma figura gerada.

`DarklightCharacterView` chama `DarklightWeaponRedesignBank.TryResolve` **antes** de `DarklightOriginalVampireView`. Por isso os nomes fonte corretos do soco são `Ruin_Requiem_Normal_S_Combat_02`, `_03` e `_11` nas quatro formas. Exigir nomes transformados impediria a correspondência. As páginas novas escolhem textura própria de cada forma e `ApplyAuthoredOwnership` já limpa `_VampireForm`, `_DarkForm`, `_RuinFists`, `_WholeFrameId`, `_CanonicalSkinShift` e a máscara anterior. Não foi necessário alterar o shader nem o banco legado.

O mapeamento é restrito a `Direto da Ruína` S. Chutes, técnicas pesadas e outros ataques sem desenhos correspondentes permanecem na arte anterior. A integração não anuncia que esses golpes receberam redesign.

## Fontes recuperadas

`ArtSources/WeaponRedesign20261023/import-jobs-south-20261024.json` prepara seis folhas pendentes, com fonte preservada e figura inteira:

| Fonte gerada de 23/10 | Destino | Seleção fonte |
|---|---|---|
| `exec-131b0957-4ef4-40d7-9dd8-f7ddb915a2bb.png` | `ReqKatanaRupturaSouth` | Idle 0–3; ataque 5/6/10/11 |
| `ReqKatanaEclipseSouth-source.png` preservada | `ReqKatanaEclipseSouth` | Idle 0–3; ataque 5/6/10/11 |
| `ReqKatanaAbsolutoSouth-source.png` preservada | `ReqKatanaAbsolutoSouth` | Idle 0–3; ataque 5/6/10/11 |
| `exec-76f31443-839d-4d4a-b3d9-a66507703a4a.png` | `ReqFistsRupturaSouth` | Idle 0–3; ataque 5/7/10 |
| `exec-27b853df-d823-4ffd-9f4c-81efe644f329.png` | `ReqFistsEclipseSouth` | Idle 0–3; ataque 5/7/10 |
| `exec-4a9ea528-a28c-42a7-bc12-a77e2e5caa6c.png` | `ReqFistsAbsolutoSouth` | Idle 0–3; ataque 5/7/10 |

As quatro fases de katana correspondem a `Today_RequiemAttackKatanaYoungCardinal_0/1/2/3`. As três de punhos correspondem ao startup/contato/recuperação acima. A posição horizontal dos dois apoios dos pés foi anotada para evitar que o pivô siga só o pé mais baixo em cada pose. Os guias Eclipse/Absoluto de katana existentes foram reaproveitados. Não houve montagem anatômica, recorte de arma, espelhamento nem reescala dos desenhos.

`exec-6d10b533-f04b-4725-b2c2-7d963900f214.png` foi identificado como uma folha de **adagas Ruptura** NO/NE/L/SE com duas figuras por direção; não é katana/punhos. A tarefa principal selecionou as folhas de adagas para fechar as oito direções. A seleção final está nos manifestos de cada atlas.

## Ícones e validação

`prepare_weapon_icons.py` foi executado com o runtime Python fornecido. `EquipmentIcons.png` é cópia byte a byte da fonte dedicada preservada; contém 12 conjuntos completos (katana, par de adagas ou conjunto de manoplas/grevas), sem retirar equipamento do corpo do personagem. QA: **zero erros e 25 avisos** de arte contínua/alpha, preservados no relatório. Esses avisos não foram apagados por quantização ou remoção global de fundo.

`audit_weapon_redesign.py` ganhou contagem de figuras únicas realmente referenciadas, figuras físicas sem referência, holds explícitos e matriz das 32 direções/formas de idle das adagas. `--date` registra a data da auditoria sem relabelar importação ou verificação runtime.

Os contatos completos dos três novos atlas de punhos foram inspecionados após o empacotamento: preservam as figuras inteiras de idle/startup/contato/recuperação e a identidade vermelha, roxa e branca das formas. A prova `sourcePixels == packedPixels` também permanece verdadeira nos quatro atlas de punhos. Contato estático e prova RGBA não substituem a captura pela câmera do jogo.

Foi acrescentado `DarklightWeaponRedesignTests.AuthoredFistPunchesUseIndependentWholeFiguresAndClearEveryLegacyPalette`: percorre startup, contato e recuperação com os nomes realmente escolhidos pelo banco anterior, exige três figuras novas em cada uma das quatro formas, texturas independentes e limpeza dos seletores antigos. A execução e o resultado desse teste devem ser registrados pela tarefa principal. O teste não muda o combate, os saves nem as preferências.

Nenhum Play, música, MP3 ou teste sonoro foi iniciado por esta subtarefa. Não houve edição de YAML `.meta`, GUIDs, shader, `prepare_weapon_redesign.py`, banco legado ou alterações de terceiros. A execução Unity e as capturas finais pertencem à rodada coordenada principal.
