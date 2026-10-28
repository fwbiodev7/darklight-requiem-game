# Revisão Antigravity — 26/10/2026

**Estado: revisão concluída; integração e validação Unity ainda não realizadas por esta revisão.** Não aplicar o conjunto sem as correções abaixo. Nenhum Runtime, arte, branch, índice ou commit foi modificado nesta auditoria.

## Referências e escopo comprovado

- Cópia Git auditada: `work/publicacao-20261021`.
- Referência obtida: `origin/antigravity/apoio-redesign-20261026`, HEAD `f26ecf490d1f35c48eb4f19656550fb4ab4c3144`.
- Intervalo solicitado: `3c7b280^..f26ecf4` (seis commits; o pai do primeiro é `43f85d8`).
- Diferença verificada: **41 caminhos: 34 de testes/metadados, cinco ferramentas, um documento e um script Editor**. Zero caminhos em `Assets/Darklight/Runtime/`; zero imagens, fontes de arte, cenas, prefabs, animações ou controladores alterados. A alegação de ausência de edição de Runtime/arte está confirmada pelo diff real.
- Os arquivos já existentes do conjunto de portabilidade no workspace coincidem com a base da branch após normalização de finais de linha/BOM. Não foi encontrada sobreposição textual com as edições de gameplay de hoje. Isso não elimina incompatibilidades de comportamento dos testes com o novo escopo.
- Os três GUIDs novos de `.meta` não colidem com os metadados atuais de `Assets/`: `67d6f5b76f5e454097a3bf0cb73b57ff`, `7b0ad58275ed4b66a5345dff4c2a6736`, `a84f391823904e2298716381920392bc`.

| Commit | Conteúdo | Seleção recomendada |
|---|---|---|
| `3c7b280` | Portabilidade de saídas, Common asmdef e proteções de reflexão | Aproveitar mantendo a intenção, com correções de sintaxe/isolamento descritas abaixo. |
| `9ea7001` | Metadados do helper | Preservar junto do helper e do asmdef correspondente. |
| `3f6baf3` | Validadores de assets/metadados | Aproveitar após corrigir contadores, saídas de erro e cobertura dinâmica. |
| `f232a96` | Contratos de saves | Aproveitar com adaptação explícita do caso de seleção legada. |
| `58c1d91` | Runner, preflight e handoff | Aproveitar após impedir falsos resultados positivos. |
| `f26ecf4` | Handoff | Preservar como histórico; corrigir caracteres de controle e registrar limites reais da evidência. |

## Correções necessárias antes de considerar integrado

### 1. Bloqueio de compilação em oito declarações locais

O commit `3c7b280` substitui variáveis locais por propriedades `static string nome => ...` **dentro de métodos**. Essas declarações não são propriedades locais válidas em C#. Corrigir cada uma para `string nome = DarklightTestOutput.GetDirectory(...);`, preservando o diretório e a intenção do teste:

| Arquivo em `Assets/Darklight/Tests/PlayMode/` | Linha na branch |
|---|---:|
| `DarklightAbyssDeliveryMediaTests.cs` | 17 |
| `DarklightAbyssFlowGameplayTests.cs` | 16 |
| `DarklightAbyssPrototypeTests.cs` | 42, 107, 228 |
| `DarklightAbyssRunOverGameplayTests.cs` | 16 |
| `DarklightAcceptedScaleTests.cs` | 12 |
| `DarklightMotorcycleImpactTests.cs` | 69 |

Propriedades equivalentes declaradas no nível da classe podem permanecer. O handoff afirma sintaxe válida, mas a inspeção encontrou estes oito casos; a revisão não executou compilação Unity para fingir confirmação dessa afirmação.

### 2. Contrato de save precisa respeitar a migração atual

`DarklightSaveContractTests.cs:109` espera recuperar `equippedWeapon == 1` da cópia `.bak`. O Runtime atual migra Presas para Katana (`0`) e registra `legacyEquippedWeapon == 1`. Corrigir a asserção para esse contrato, mantendo as verificações de capítulo/checkpoint/nível e preservação de bits de unlock. A estabilidade do enum `Daggers == 1` continua válida: remover sua seleção ativa não autoriza renumerar IDs nem apagar histórico.

O roundtrip com Punhos `2`, os valores iniciais, os IDs dos enums e `VolumePercent` continuam compatíveis. A suíte nova tem seis testes e ainda precisa executar na Unity após a integração.

### 3. Runner permite falso PASS

`Tools/Testing/run_tests.py:94–106` aceita XML já existente e decide `PASSED` apenas por `failed == 0`. A linha 135 também permite saída zero sem XML quando o processo retorna zero. Falta exigir XML novo, casos executados, resultado agregado e código de saída correto.

**Reprodução somente em memória, com Unity/subprocess/filesystem simulados:** processo retornando `9` e XML `total=0, passed=0, failed=0` produziu `status=PASSED`. Nenhuma Unity foi iniciada nessa reprodução.

Corrigir: XML por execução ou comparação de hash/data prévia; exigir `exit_code == 0`, `test-run` válido, resultado `Passed`, pelo menos um caso efetivamente executado e ausência de falhas/erros. Ausência de XML, XML antigo, zero casos, somente ignorados ou processo falhando devem retornar falha. Preservar distinção entre timeout, bloqueio, falha de lançamento e asserção. Tornar `-nographics` opcional: os testes visuais dependem de renderização. O helper existente `Tools/TokenEconomy/run_compact.py` já implementa boa parte da verificação de XML novo e deve ser reutilizado em vez de duplicar garantias incompletas.

### 4. Validadores também podem informar zero erros indevidamente

`audit_unity_metadata.py:33` e `validate_assets.py:31` inicializam `errors/warnings`, mas incrementam chaves `error/warning`. A CLI imprime os plurais originais e não termina com erro ao encontrar problemas.

**Reprodução somente em memória:** um asset sem `.meta` resultou em `missing_meta=1` e resumo `errors=0, error=1`. Corrigir o mapeamento de severidades, a presença de problemas estruturados e o exit code. As referências YAML ausentes devem distinguir GUIDs de Assets, Packages e internos antes de serem chamadas de quebradas; hoje não entram sequer na contagem de avisos.

O validador de assets contém a falta de moto Réquiem e a matriz antiga como texto fixo (`validate_assets.py:109–118`). Hoje existem 24 figuras Réquiem da moto e novos clips; calcular cobertura pelos descritores reais ou reutilizar `audit_reduced_scope_20261026.py`. Não apresentar páginas legadas/variantes de paleta como redesign completo. A ferramenta atual verifica existência/sintaxe/hashes de duplicatas; não mede dimensões/alpha de PNG como o handoff anuncia. Catálogos obrigatórios ausentes também devem gerar estado explícito.

### 5. Isolamento de caminhos e limites dos checks

- `DarklightTestOutput.cs:52–61`: `StartsWith(root)` sem separador aceita uma pasta irmã chamada `test-artifacts-sibling`; categoria absoluta também pode substituir a raiz no `Path.Combine`. Rejeitar categorias absolutas e validar igualdade com a raiz ou prefixo `raiz + separador`, depois de resolver o caminho. Substituir `..` por vazio não demonstra contenção. Acrescentar testes de caminho relativo, subpasta, raiz absoluta e pasta irmã.
- `DarklightRemasterPrototypeTests`/`DarklightSoloTransformationTests`: ignorar o teste é adequado quando o assembly Editor não existe no Player. No Editor, a mesma ausência deve falhar, para não ocultar regressão do assembly/reflexão. Manter ignorados como ignorados nos relatórios, sem convertê-los em cobertura aprovada.
- `build_preflight.py`: verifica existência de cenas e um asmdef, mas não lê `EditorBuildSettings`, não valida JSON/referências dos asmdefs nem faz a varredura de arquivos privados anunciada; `suspicious_patterns` é declarado na linha 81 e não é usado. Rotular as verificações efetivamente executadas e implementar as faltantes antes de usar como gate de entrega.
- `verify_integration.py:20,81`: erros de Git viram strings vazias e garantias sobre arte/saves são constantes `False`. A classificação omite caminhos não reconhecidos. Receber base/ref por parâmetro, preservar erro de Git, classificar todos os caminhos, derivar garantias do diff e retornar falha se a política for violada. O manifesto atual não substitui auditoria do diff.
- O handoff contém caracteres de controle em nomes/hashes (`\a`, `\b`, `\f`, `\t`) e resultados PASS que não constituem prova de compilação ou execução Unity. Preservar o original no histórico, corrigindo o documento integrado com evidências verdadeiras.

## Estado dos gates de gameplay encontrados nesta revisão

- `outputs/retomada-20261026/final-edit-tests.xml`: **71/71 aprovados**, zero ignorados; término `2026-10-26 21:00:41Z`.
- `outputs/retomada-20261026/final-play-tests.xml`: **18/19 aprovados**, zero ignorados; término `2026-10-26 21:03:57Z`.
- Falha: `DarklightDeliveredSfxGameplayTests.WeaponFormMapDeathAndReturnKeepTheSameSilentPoolAndPreserveStorage`, linha 154; `Expected: 1 / But was: 0`. O caso ainda tenta selecionar Presas na sequência `{1,2,0}`. Adaptar armas/formas ativas preservando a intenção de pool silencioso e armazenamento; não corrigir o Runtime para reativar o conteúdo removido.
- Não há build liberada por esses resultados. As novas artes aprovadas posteriormente e futuras mudanças de testes exigem importação e gates frescos antes da build. PC04/PC06 possuem evidência separada; não foram reexecutados nesta revisão.

## Próxima aplicação recomendada

Preservar os seis commits e suas branches. Integrar os hunks de portabilidade, metadados, referências de assembly e testes úteis em uma branch isolada, acrescentando um commit explícito com as correções acima. Não substituir arquivos de Runtime/arte nem documentos atuais de escopo pelo estado antigo do clone Antigravity. Revalidar importação/compilação, novos seis contratos e caminhos do helper; executar os gates essenciais do jogo antes de considerar a integração estável. Esta revisão não fez cherry-pick, merge, edição de código ou execução Unity.

## Caminhos do intervalo auditado

```text
Assets/Darklight/Editor/RemasterPrototype/RemasterPrototype.cs
Assets/Darklight/Tests/Darklight.Tests.Common.asmdef
Assets/Darklight/Tests/Darklight.Tests.Common.asmdef.meta
Assets/Darklight/Tests/DarklightTestOutput.cs
Assets/Darklight/Tests/DarklightTestOutput.cs.meta
Assets/Darklight/Tests/EditMode/Darklight.EditModeTests.asmdef
Assets/Darklight/Tests/EditMode/DarklightDialogueQARunner.cs
Assets/Darklight/Tests/EditMode/DarklightOverhaulQARunner.cs
Assets/Darklight/Tests/EditMode/DarklightRegionReformQARunner.cs
Assets/Darklight/Tests/EditMode/DarklightSaveContractTests.cs
Assets/Darklight/Tests/EditMode/DarklightSaveContractTests.cs.meta
Assets/Darklight/Tests/PlayMode/Darklight.PlayModeTests.asmdef
Assets/Darklight/Tests/PlayMode/DarklightAbyssControllerReviewTests.cs
Assets/Darklight/Tests/PlayMode/DarklightAbyssDeliveryMediaTests.cs
Assets/Darklight/Tests/PlayMode/DarklightAbyssFlowGameplayTests.cs
Assets/Darklight/Tests/PlayMode/DarklightAbyssPrototypeTests.cs
Assets/Darklight/Tests/PlayMode/DarklightAbyssRunOverGameplayTests.cs
Assets/Darklight/Tests/PlayMode/DarklightAbyssUrgentTests.cs
Assets/Darklight/Tests/PlayMode/DarklightAcceptedScaleTests.cs
Assets/Darklight/Tests/PlayMode/DarklightArtisticAtlasTests.cs
Assets/Darklight/Tests/PlayMode/DarklightCombatCaptureTests.cs
Assets/Darklight/Tests/PlayMode/DarklightCombatImpactTests.cs
Assets/Darklight/Tests/PlayMode/DarklightFocusedHUDTests.cs
Assets/Darklight/Tests/PlayMode/DarklightLoadingTests.cs
Assets/Darklight/Tests/PlayMode/DarklightMap10ReviewTests.cs
Assets/Darklight/Tests/PlayMode/DarklightMap8ReviewTests.cs
Assets/Darklight/Tests/PlayMode/DarklightMap9ReviewTests.cs
Assets/Darklight/Tests/PlayMode/DarklightMotorcycleImpactTests.cs
Assets/Darklight/Tests/PlayMode/DarklightPromoGameplayTests.cs
Assets/Darklight/Tests/PlayMode/DarklightReformedRegionTests.cs
Assets/Darklight/Tests/PlayMode/DarklightRemasterProductionTests.cs
Assets/Darklight/Tests/PlayMode/DarklightRemasterPrototypeTests.cs
Assets/Darklight/Tests/PlayMode/DarklightSoloTransformationTests.cs
Assets/Darklight/Tests/PlayMode/DarklightTrailerGameplayTests.cs
Assets/Darklight/Tests/PlayMode/DarklightWorldLandmarkTests.cs
Docs/Darklight/HANDOFF-ANTIGRAVITY-20261026.md
Tools/Handoff/verify_integration.py
Tools/Preflight/build_preflight.py
Tools/Testing/run_tests.py
Tools/Validation/audit_unity_metadata.py
Tools/Validation/validate_assets.py
```
