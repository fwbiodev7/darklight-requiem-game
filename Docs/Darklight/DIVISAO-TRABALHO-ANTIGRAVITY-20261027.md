# Divisão de trabalho Antigravity — 27/10/2026

## Limites e base de integração

- Trabalhe em clone próprio, baseado no HEAD remoto da branch `integracao-completa-20261026`. Consulte o SHA ao iniciar; `f26ecf4` é a base histórica do apoio Antigravity e já existem checkpoints posteriores. O histórico foi preservado por fast-forward.
- Compare as branches `17` e `20` por arquivo e intenção: há 220 diferenças. Produza inventário de mudanças, riscos, sobreposições e recomendações; não faça merge cego nem reescreva o histórico.
- Não altere `Assets/Darklight/Runtime/`, arte/fontes visuais, a branch principal ou a cópia de publicação. Não integre nem publique alterações. Entregue relatórios e propostas revisáveis.
- O checkout raiz desta sessão é legado (`codex/fundacao-darklight`, `412cd5f`); as verificações da integração abaixo foram executadas explicitamente na cópia `work/publicacao-20261021`, branch e hash declarados acima. Não use o checkout raiz para inferir o conteúdo das branches 17/20 nem para validar a integração publicada.

## Tarefas atribuídas

1. **Diferenças 17/20:** enumerar os 220 caminhos, agrupar por propósito, identificar mudanças que se anulam, duplicações, arquivos removidos/renomeados e dependências. Citar hashes e linhas/caminhos relevantes. Recomendar uma sequência de integração sem executá-la.
2. **CI e portabilidade:** executar somente verificações não Unity no clone próprio; verificar caminhos Windows codificados, pressupostos de Python/pacotes, dependências não declaradas, diferenças de maiúsculas/minúsculas e se CI diferencia falha, bloqueio, zero testes e relatório ausente. Reportar comandos e versões usados.
3. **Smoke de entrada do jogador:** propor e, no clone isolado, executar um smoke curto do fluxo de entrada/remapeamento e primeira área, sem tocar nos fontes de Runtime. Registrar teste selecionado, resultado real, falhas e evidência. Não declarar validação de gameplay além desse fluxo.
4. **GUID e importação Unity:** auditar GUIDs duplicados, referências YAML sem resolução, `.meta` ausentes e colisões de nomes no clone-alvo. Separar defeitos reais de recursos/fontes adicionados ainda não importados pela Unity. Não criar ou editar `.meta` automaticamente; fornecer caminho, GUID e impacto para revisão.

## Runner e filtros para o gate Unity da coordenação

Use uma execução por filtro, com o runner `Tools/Testing/run_tests.py`; confira `executed > 0`, `status == PASSED` e código de saída zero em cada execução. O runner atual deve rejeitar XML legado fora de `<test-run>`, resumo incompatível com os casos e qualquer saída Unity não zero. Preserve resultados separados por modo.

- EditMode, contrato dos efeitos sonoros e automação silenciosa: `DarklightDeliveredSfxTests`.
- EditMode, transições de forma, cooldown e baselines após pausa/morte/mapa: `DarklightAudioCompletionTests`.
- PlayMode, fluxo focado de controle/entrada: `DarklightPlayerControlsTests.FirstChapterTriggerAndRemapMenuRemainFunctionalAfterCancelAndConflict`.
- PlayMode, efeitos carregados sem fontes extras e silêncio: executar separadamente `DarklightDeliveredSfxGameplayTests.DeliveredEffectsLoadAndRealPotionPainPowerHooksStaySilentWithoutExtraSources` e `DarklightDeliveredSfxGameplayTests.WeaponFormMapDeathAndReturnKeepTheSameSilentPoolAndPreserveStorage`.

Esses testes de áudio afirmam o modo silencioso e não são uma avaliação auditiva. Não iniciar música nem alegar teste físico de controle. Use `--nographics` somente para filtros que não dependam de renderização. O filtro PlayMode de controles pode precisar de gráficos; não o execute com `--nographics`.

## Auditorias históricas da base f26 — não são o gate atual

- Corrigido `run_tests.py`: antes, XML com raiz arbitrária ou contadores `passed`/`failed` incompatíveis ainda podia resultar em `PASSED`. Agora exige `<test-run>` e contadores coerentes. Adicionados testes para XML NUnit legado e resumo incompatível.
- Os fixtures do teste de contrato agora ficam sob `outputs/tooling-fixtures` dentro do projeto e são removidos ao fim do teste.
- `python -m unittest discover -s Tools/Testing/tests -p test_validation_contracts.py -v`: **12 testes aprovados**.
- Venv `work/tool-runtime-20261026/Scripts/python.exe` confirmado; Python 3.14.8 e Pillow 13.0.0.
- `validate_assets.py` na cópia de publicação: 0 erros, 2 PNGs duplicados informativos, 1 lacuna `BLOCKED` (não há folhas autorais do piloto Requiem no clone) e 1 aviso (transformações da katana têm somente vista Sul autoral). A integridade dos PNGs foi verificada com Pillow. O relatório textual imprime 0 avisos apesar do aviso estruturado e o script termina com código 0 mesmo com `blocked: 1`; encaminhar essa inconsistência como tarefa de CI/portabilidade, sem declarar cobertura aprovada.
- `audit_unity_metadata.py` na cópia de publicação: 1.783 assets, zero `.meta` ausentes, GUIDs duplicados ou colisões de caixa; 19 referências YAML não resolvidas. O utilitário não classifica essas 19 referências no resumo; classificar built-ins/pacotes antes de tratá-las como defeito.
- `verify_integration.py` executado pela ferramenta que aceita `--base b719c527 --ref f26ecf4`, apontando para `work/publicacao-20261021`: **PASS**, HEAD `f26ecf490d1f35c48eb4f19656550fb4ab4c3144`, 10 commits à frente, 9 Docs, 34 testes, 5 Tools, 1 Editor, 0 Runtime, 0 arte, 0 outros.
- O verificador que está dentro da cópia de publicação ainda não aceita `--base`/`--ref`; a chamada anterior ignorou esses argumentos. O resultado explícito acima veio do verificador atualizado no checkout raiz com a cópia de publicação como projeto-alvo. Antigravity deve incluir suporte a argumentos e tratamento de falhas Git na auditoria de portabilidade.
- `build_preflight.py` na revisão anterior passou, mas foi executado no checkout raiz legado; não usar esse resultado como gate da cópia publicada. Reexecutar somente se a coordenação precisar desse gate.
- Evidência da coordenação: compilação Unity limpa e 11/11 novos contratos de save/outputs aprovados. Esta revisão não iniciou Unity.

## Retorno esperado do Antigravity

Entregar um relatório curto com base/hash confirmados, contagem dos 220 caminhos conferida, mudanças agrupadas, problemas reproduzidos, comandos/versões e recomendação por etapa. Separar evidência de hipótese. Não reportar PASS quando a ferramenta bloqueou, não executou testes ou analisou checkout diferente.

## Nota da coordenação sobre os alvos de auditoria

Os avisos fixos de ausência de Moto Réquiem pertencem à ferramenta histórica do clone f26, não descrevem os recursos atuais do checkout de trabalho. O validador atualizado em Tools/Validation/validate_assets.py deriva cobertura dos manifestos e retorna falha para errors/blocked. A cópia publicada será sincronizada pela allowlist antes do commit. A validação final deve usar essa versão sincronizada e registrar explicitamente script, diretório e hash; não transportar bloqueios do clone antigo como resultado do projeto atual.
