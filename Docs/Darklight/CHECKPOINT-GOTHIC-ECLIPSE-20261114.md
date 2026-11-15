# Gothic Eclipse — validação e build em preparação — 14/11/2026

## Integrado e revisado

- Corrigida a vinculação de `_MainTex` por SpriteRenderer no Remaster, incluindo mudanças de folhas. A textura de um desenho anterior estava sendo amostrada por outro trecho: isso mudava a arquitetura aparente e fazia personagens parecerem gigantes. Sem alterar câmera, PPU, pivôs, colisões ou fontes de Fabius Normal.
- Filtro do chão inclui os trechos de detalhe e preserva textura, transformação e limites de cada fonte. Fundos, shaders originais e partículas permanecem preservados; efeitos adicionais são reversíveis.
- Chuva em três planos, pool limitado a 88 partículas, três véus baixos, luar localizado e reflexos discretos. Reduzidos os riscos prateados artificiais do piso após inspeção das capturas.
- Presets contextuais: noite chuvosa nas ruas, dia encoberto no Jardim, crepúsculo no Observatório, iluminação artificial/interior sem chuva e sem feixes lunares, frio sobrenatural no Véu. Ativados após revisão da região 0 e amostras diurna/interior; não se declara revisão visual individual dos dez mapas.
- Luz lateral dinâmica: fontes locais existentes produzem realce no contorno voltado à luz e atenuação por distância; acompanha troca de lado sem editar a arte. Qualidade/intensidade zero desligam o realce adicional. Removidos focos frios/quentes arbitrários fixados na posição inicial do jogador; arena usa os braseiros existentes.
- Arena reconecta Remaster e atmosfera ao seu próprio cenário após substituir o mapa temporário. Sombras existentes preservadas; corrigida a contaminação de textura que também afetava sua apresentação.

## Evidência final desta etapa

PlayMode: **4/4 passaram**, sem mascarar falhas:
`work/session20261114/PlayMode-2026-11-14T184603-930844+0000/tests.xml`.

Casos: arena/região 0, intensidades 0/0,85/1 e desligar/qualidade zero/reentrada; persistência sanitizada; fonte lateral esquerda/direita/distante; presets diurno/interior sem chuva. Saves e preferências protegidos pelos testes, áudio silencioso.

Capturas reais em `outputs/gothic-eclipse-20261114/gameplay/`: pares `arena-*` e `region0-*` na mesma câmera/posição; `actor-light-left/right.png`; `region4-day.png`; `region2-interior.png`. Candidatas de diagnóstico `*-only` e `*-explicit-textures-*` são históricas, não comparações finais.

Medição: Ryzen 5 5500 / RTX 3050 / D3D11, Editor PlayMode, 1280×720, mundo pausado, qualidade 2; 5 aquecimentos + 20 amostras. Custo sincronizado Render + ReadPixels de 1 pixel, sem simulação/PNG: arena desligada 2,478 ms, intensidade 0,85 3,321 ms; região 0 desligada 2,968 ms, intensidade 0,85 5,117 ms. Não é FPS de gameplay. Dados e intervalos em `capture-cost-and-geometry.txt`.

## Direção mais recente do usuário

**Suspender UI 2.0 e tentar build agora**, mesmo antes da reformulação dos menus. Esta ordem supera a condição anterior de aguardar UI 2.0. Nenhum layout/rota/fonte de menu foi alterado nesta etapa. A fonte licenciada Pixelify Sans foi somente preparada/copied como recurso, sem ligação à interface.

Build em curso pelo pipeline existente: `python Tools/Testing/run_session_20261114.py build --unrestricted-memory`; destino novo `outputs/build-windows-gothic-20261114/Windows/`. Resultado de compilação e smoke test devem ser registrados no relatório de entrega, sem presumir sucesso.

Transformações e limitações de cobertura: consultar `CHECKPOINT-TRANSFORMACOES-OFICIAIS-20261114.md`. As sete falhas históricas de folhas Ruptura ausentes não foram resolvidas nem ocultadas por este lote.
