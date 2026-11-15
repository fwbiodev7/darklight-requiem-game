# Transformações oficiais integradas — 14/11/2026

As três folhas anexadas na retomada oficial são a autoridade vigente e **substituem todas as candidatas anteriores**, inclusive desenhos antes aprovados durante a exploração. Não há aprovação pendente para essas três fontes. Ordem executada: Ruptura → Serafim Carmesim → Lucius Arconte.

## Produzido, revisado e importado

- Fontes preservadas byte a byte em `ArtSources/OfficialTransformations20261114/raw/`, com hashes em `manifest.json`.
- Três folhas RGB1774×887, duas linhas/quatro colunas; **24 quadros temporais inteiros**, não oito direções. Nenhuma geração/recriação das fontes oficiais.
- Derivados RGBA: extração do fundo preto conectado à borda e alpha proporcional na franja externa escura. RGB original preservado integralmente, pretos encerrados no corpo mantidos. Contatos em fundo neutro e âncoras inspecionados. Sem montagem anatômica ou distorção.
- Assets em `Assets/Darklight/Resources/Darklight/OfficialTransitions20261114/`; Point, sem mipmaps/compressão destrutiva, pivôs no plano de apoio da fonte, sem reposicionar botas aéreas no chão ou somar outra levitação.
- AnimationClips em `Assets/Darklight/Animations/OfficialTransitions20261114/`, bindings `FabioVisual`/`LuciusVisual`, oito chaves de poses e chave final de sustentação, sem loop.
- Durações: Ruptura1,25s; Serafim4,6s; Arconte4,2s. Surgimento das asas e levitação vêm do desenho; holds aéreos recebem o tempo cinematográfico. Controllers existentes usam as mesmas ordens/tempos dos clips, sem instalar Animator concorrente.

## Runtime e limites

- Banco oficial conectado aos controladores existentes; troca de estado liberada aos52%, entrada não reinicia, custo/pausa/morte/cancelamento/retorno de controle preservados.
- Durante as folhas oficiais, ocultados lift/aura/burst procedurais e overlays de ressonância para evitar duplicar os efeitos já desenhados.
- Repouso frontal transformado usa a pose final oficial, preservando expressão/roupa/aura aprovadas. Movimento, ataques e outras direções continuam usando os bancos existentes. Cancelamento/disable sai explicitamente da pose de transição e limpa bloqueios/efeitos.
- **Fabius oficial com Katana; outras armas conservam fallback compatível**, sem introduzir uma espada desenhada em personagens equipados com Punhos/Serras. Não há fontes oficiais novas específicas dessas armas. Lucius mantém a espada nas costas na sequência oficial.
- As três sequências são uma vista cinematográfica cada. Não se anunciou cobertura direcional nova nem conclusão das caminhadas ausentes. Os28 PNGs recuperados e26 fontes ativas da etapa anterior permanecem preservados/integrados.

## Evidência nova

- Última importação: exit0, `work/session20261114/official-import-2026-11-14T180857-213136+0000/receipt.json`.
- EditMode: **33/33 passaram**, `work/session20261114/EditMode-2026-11-14T181024-426442+0000/tests.xml`. Inclui ordem/holds dos24 frames, três bindings/tempos de clips, ownership de cores, entradas inválidas, cancelamento Lucius e estabilidade Fabius Normal.
- PlayMode/captura: **9/9 passaram**, `outputs/session20261114/official-transitions/capture-results.xml`; log `work/session20261114/capture-2026-11-14T181137-828901+0000/unity.log`, sem erros de script/assert observados. Executou controllers reais com simulação manual para captura isolada, progress storage em work e áudio silencioso. Não é teste com controle físico nem benchmark de FPS.
- Contatos runtime de Ruptura/Serafim/Lucius em `outputs/session20261114/official-transitions/*-contact.png`, conferidos: forma final não volta imediatamente à aparência antiga e auras não se duplicam.
- Baseline de Fabius Normal: **698 arquivos, zero alterações de bytes** após as execuções.
- As sete falhas de testes que cobram folhas de caminhada Ruptura ainda inexistentes seguem registradas no relatório anterior; não foram mascaradas. Elas não representam fontes fornecidas por essas três folhas temporais.

## Próximo passo autorizado

Iniciar imediatamente Gothic Eclipse conforme plano14/11, validar arena e região0, depois UI2.0. Reutilizar atmosfera/URP2D/88 partículas, mapas, dados, HUD e navegação atuais. Nenhuma build nova foi declarada pronta. Fazer regressões e build consolidada após essas etapas, revisar diff e tentar Git normal somente no conjunto pertinente; se bloquear, preservar entrega para Antigravity.

Limpeza de disco: usuário pediu continuar e entregar a lista depois. Nenhum arquivo excluído. Duplicata de1,90GiB comprovada, mas exclusão rejeitada por política; sem contorno. Lista e condições em `LIMPEZA-DISCO-20261114.md`.
