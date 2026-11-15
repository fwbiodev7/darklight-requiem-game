# Revisão de artes e integração — 12/11/2026

Base: `425376f62b6f8425135d210c1e3e4195e8f42d6a`, main. Coordenador com três subagentes: render-validator, asset-cleaner e integration-tester. Monitor OpenBot de cota não encontrado neste PC; nenhum saldo/reset foi presumido.

## Arte produzida nesta rodada

Referências: imagens anexadas pelo usuário nesta conversa. Lucius Arconte usa cabelo prateado, roupa preta/dourada e aura verde. Fabius Serafim Carmesim usa cabelo escuro/carmesim, roupa preta/vermelha, aura vermelha e asas. Essas referências orientam este lote e não alteram Fabius Normal.

| Fonte em ArtSources/EssentialStates20261112 | Quadros 0–5 | Quadros 6–11 | Estado |
| --- | --- | --- | --- |
| FabiusSerafim_Walk_Defense_12f.png | Caminhada | Defesa | Produzido/revisado |
| FabiusSerafim_Hurt_Death_12f.png | Dano | Morte | Produzido/revisado; usuário aprovou a primeira folha exibida |
| LuciusArconte_Walk_Defense_12f.png | Caminhada | Defesa | Produzido/revisado |
| LuciusArconte_Hurt_Death_12f.png | Dano | Morte | Produzido/revisado |

Total: quatro PNGs RGBA nativos de 1086×1448, grade de 3 colunas por 4 linhas, 48 figuras completas. Não houve edição dos pixels após a geração. JSONs por folha e manifest.json registram SHA-256, retângulos das células inteiras, ação, transparência e revisão necessária nas bordas. Timings são provisórios; direção é explicitamente `unassigned-single-view`. Não representa cobertura em oito direções. Defesa é uma sequência visual, sem criar uma mecânica nova de parry.

Importado novo: **zero**. Testado novo em runtime: **zero**. A calibração de direção/apoio dos pés, revisão de efeitos tocando as células e validação Unity permanecem pendentes. Não trocar o catálogo ativo por estas fontes sem concluir esses passos. O script `Tools/AnimationPreview/prepare_essential_states_20261112.py` reproduz os metadados sem modificar PNGs, pivôs ou importadores Unity.

## Auditoria das integrações anteriores

- Fabius: 21 folhas, 198 quadros. Os hashes dos 21 PNGs correspondem ao catálogo. Todos os quadros tinham tag E, inclusive ações WalkN/W/S. O resolver ignorava direction, usava a primeira folha com prefixo Walk e intervalos fixos de 100 ms. A seleção da forma era depois sobrescrita pelo banco de redesign.
- Lucius Arconte: 40 páginas ativas (8 Idle, 8 Transformation, 24 Attack). FloatFly, HurtDissipate, FloatLateral, DashNONE, DashSOSE e WalkNS estão salvos, mas não mapeados no catálogo. Não se declarou essas ações concluídas no jogo.
- `LuciusArconteWalkNSV1.png`: dois banners “CAMINHADA — SUL/NORTE” incorporados à fonte e 24 quadros com tag E. Não liberar esse arquivo como caminhada final. Alpha máximo 253; isso não indica transparência excessiva por si só.
- `FabiusSerafimHurtKnockdownV1.png`: alpha toca bordas laterais das células 00/01/05/06; possível corte de asas/VFX. Requer revisão visual em movimento; não houve apagamento automático.
- Fontes JPEG de HurtKnockdown tinham fundo branco, mas os PNGs contêm alpha real. Canvas branco de prévia e RGB oculto em A=0 não provam fundo residual. Halos de filtragem permanecem sem confirmação por renderização.
- `LuciusArconteAttacksV1.png`: hash idêntico à fonte selecionada, transparência válida. Energia verde/nuvens escuras são conteúdo; pequenos pontos suspeitos não foram apagados sem diagnóstico.

## Correções implementadas em código

1. Banco de formas resolve ação e direção explicitamente, usa IDs e durações do catálogo e mantém fallback quando não existe mapeamento. WalkN/W/S usam a identidade explícita da ação para corrigir a tag E provisória. Não se inferem diagonais ou linhas de Fly8D/WalkNS.
2. Só locomoção repete; ataque, esquiva, dano e morte respeitam duração e seguram o último quadro. Morte começa no primeiro quadro e avança pelo relógio de apresentação. Charging mantém o resolver existente. Parry e movimento com serras mantêm a pose base até haver mapeamento próprio.
3. CharacterView escolhe a arte de forma depois do redesign, somente Réquiem e fora de pilotagem. Ownership do banco novo impede reaplicação da paleta legada e da coloração de dano sobre os pixels já desenhados. Auras/transições existentes permanecem.
4. Importadores de Fabius e complementos de Lucius preservam spriteID por nome e sincronizam a tabela de identidade; GUID novo somente para nome novo. Não foram executados nesta rodada.
5. Testes EditMode acrescentam seleção cardinal, fallback nas direções sem mapeamento, armas suportadas, clocks de dano/morte, Charging, Parry e propriedade de material. Esses testes compilaram, mas não executaram na Unity.

Nenhuma arte, retângulo, PPU, GUID ou pivô já calibrado de Fabius Normal foi alterado. As alterações locais preexistentes em DarklightAirControllerTests.cs e CHECKPOINT-TRANSFORMACOES-E-HANDOFF-20261111.md foram preservadas e excluídas da publicação desta rodada.

## Validação efetivamente executada

- Preparação de metadados: 4 folhas/48 quadros, alpha real, sem células vazias, PNGs preservados e hashes registrados.
- Compilação C# independente com Roslyn do SDK instalado na Unity 6000.6.0f1 e referências/responses existentes do projeto: Runtime, Editor e EditModeTests, exit code 0 em todas. Não equivale a importação nem execução Unity. Recibos locais em `work/essential-states-20261112/compiler/receipt.json`.
- Primeira tentativa EditMode com dispositivo gráfico: crash de inicialização, exit 3758096385, sem XML; erros de WMI/licença e DirectX12. Log em `outputs/test-artifacts/runner/20261112T211036-d2ccbe269e2649398b6c504500562cbe/unity_editmode.log`.
- A tentativa sem gráficos primeiro encontrou o lock residual da própria tentativa anterior; após confirmar ausência de processo Unity, o lock foi removido. Nova execução headless também falhou na inicialização, exit 3758096385 e sem XML. Log em `outputs/test-artifacts/runner/20261112T211923-fc52407523934d759499da71a02af59a/unity_editmode.log`.
- Nenhum teste foi contado como aprovado por essas tentativas. Sem PlayMode, reprodução de áudio, FPS/memória de gameplay, benchmark visual, controle físico ou build nova.

## Próximos passos exatos

1. Restaurar inicialização/licença da Unity no ambiente normal e rodar uma vez os filtros DarklightFabiusFormsTests, DarklightLuciusSpriteBankTests e DarklightFabiusAnimationStabilityTests; executar teste de shader com dispositivo gráfico.
2. Calibrar direção e apoio de cada célula nova inteira; confirmar se efeito tocando borda é completo. Separar Walk/Guard/Hurt/Dead em clipes de seis quadros sem montagem de partes nem reamostragem. Preservar IDs de assets existentes.
3. Integrar as novas folhas no catálogo de cada personagem apenas nas vistas verificadas. Mapear defesa, dano e morte do Arconte ao consumidor real, sem chamar ausência de direção de cobertura completa.
4. Validar seleção em cena, transições, bloqueio de pose, duração versus janela de combate e fundos claro/escuro. Medir FPS/memória em alvo apenas após execução real. Gerar build quando esses gates passarem.

A revisão pode ser aberta como PR em rascunho para preservar e revisar as mudanças. Não mesclar como entrega validada enquanto a execução Unity permanecer bloqueada.
