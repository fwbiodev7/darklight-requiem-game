# DARKLIGHT REQUIEM — pendências para 24/10/2026

Atualizada em 24/10 após revisão direta do usuário. **Redesign incompleto e sem aprovação visual final.** Esta lista é a prioridade de retomada da próxima sessão. Uma prévia, um filtro ou um teste aprovado não equivalem a desenho novo e integração completa.

Fechamento e próxima sessão: `FECHAMENTO-20261024.md` e **`PENDENCIAS-20261025.md`**. O usuário pediu encerrar a rodada e enviar as alterações de hoje ao GitHub. As novas fontes aprovadas e as integrações pendentes estão discriminadas nesses documentos.

### Retomada ativa — jogo primeiro

O usuário pausou expressamente o pack Minecraft para priorizar o jogo principal e pediu um subagente responsável por cada arma: katana, Presas do Caos, Punhos da Ruína e moto. A coordenação principal revisa as fontes e integra/testa os lotes. Preservar os sprites recentes que o usuário elogiou e aprovou visualmente (“ficou muito bom esses sprites”), distinguindo essa aprovação de direção/animação/importação/runtime ainda pendentes. Isso não aprova o redesign global nem autoriza reutilizar os quadros de caminhada rejeitados.

Produção atual: katana Réquiem Normal, lâmina de aço neutra, caminhada real e primeiro ataque em direções faltantes. Pipeline preparado para as duas skins e Walk/Run sem alterar os159 quadros anteriores. Fontes novas/candidatas em `ArtSources/WeaponRedesign20261024/candidates/`; mapa de movimentos em `work/unity-redesign-resume-20261024/motion-scope.json`. Minecraft permanece preservado e pausado em `CONTINUIDADE-DARKLIGHT-PVP.md`.

## URGENTE — terminar o redesign de todas as skins com as novas armas e novos designs delas

- [ ] **Terminar o redesign de todas as skins com as novas armas e os novos designs da ficha, com TODAS as direções, animações e combos de TODAS as skins, no Normal e em todas as transformações.** Seguir obrigatoriamente a Ficha de Equipamentos Abissais em Pixel Art reanexada pelo usuário em 24/10. Referência íntegra preservada: `ArtSources/WeaponRedesign20261023/references/equipment-four-forms.png`; original: `C:/Users/Usuario/Downloads/Ficha de Equipamentos Abissais em Pixel Art.png`.
- [ ] Cobrir **Réquiem e Rock; katana, Presas do Caos, manoplas e grevas dos Punhos; Normal, Ruptura, Eclipse e Absoluto; todas as oito direções e todas as animações e combos**. A moto e seus golpes também continuam na matriz de cobertura, com design próprio já existente.
- [ ] **Corrigir o Normal:** o usuário identificou que a apresentação nova parece Ruptura. Normal deve ser o personagem sem transformação, com o equipamento neutro da coluna NORMAL da ficha; Ruptura recebe a energia vermelha, Eclipse a violeta e Absoluto o equipamento claro com fissuras vermelhas. A referência e o feedback de 24/10 prevalecem sobre a orientação anterior de pintar carmesim no Normal. Preservar olhos, pele, rosto e identidades aprovadas.
- [ ] **Completar todas as direções dos Punhos:** o novo desenho/ataque cobre somente a frente. Desenhar e integrar as demais posições, sem espelhar assimetrias ou atribuir um filtro como se fosse uma nova figura.
- [ ] **Completar as animações de TODAS as armas:** o usuário constatou reaproveitamento de animações antigas em outras posições e ações. Atualizar caminhada/corrida, transições, leves, pesados, carregados, combos, aéreos, esquivas, defesas, habilidades, dano, morte e transformações com personagens inteiros e novos equipamentos visíveis.
- [ ] Substituir a dependência dos filtros vermelho/roxo nos contextos cujo desenho ainda é antigo. Cada forma precisa dos equipamentos/figuras correspondentes à ficha. Preservar timings, eventos, colisões e gameplay aprovados; não montar personagens por partes.
- [ ] Validar a matriz **skin × arma × forma × direção × ação/golpe**. As 48 páginas atuais são parciais: adagas têm idle novo em oito direções; katana/Punhos têm idle e somente o primeiro ataque frontal nas quatro formas. Isso **não** conclui os demais ataques, combos ou posições. O usuário não aprovou o Normal nem o redesign global.
- [ ] Atualizar a prévia e a build após completar esses desenhos, mostrando uma arma por vez, ações independentes e combos, Normal e transformações. Manter visível a diferença entre arte própria nova e apresentação antiga com filtro até a substituição efetiva.

### Economia de tokens na retomada

- Partir desta lista e de `outputs/redesign-armas-20261024/coverage.json`; ler apenas os contextos faltantes e as fontes pertinentes. Não repetir inventários completos, leituras extensas de conversas ou suítes já válidas sem mudança/falha que justifique.
- Gerar folhas de 8–12 personagens inteiros por identidade e revisar o desenho antes de ampliar o lote. Integrar e verificar cada cobertura real, evitando novas gerações que repitam a mesma direção/ação.
- Registrar resultados curtos e verificáveis. Não anunciar cobertura completa, economia percentual ou aprovação artística sem evidência.

## Executado em 24/10 — escopo técnico parcial

- Importadas 17 folhas/159 figuras completas e 12 ícones. Cobertura real: 48 páginas; nenhuma conclusão global do redesign.
- Corrigida a seleção do primeiro soco frontal das quatro formas e removido o rastro antigo que aparecia na cintura sobre esses quadros.
- Corrigido erro transitório Windows/Mono 1175 na troca atômica do save, com tentativas limitadas e preservação do backup.
- Rodada final: **72/72 testes de edição e 16/16 de jogo**, sem áudio e com saves isolados. São 88 casos distintos; repetições da mesma rodada não são casos novos. Incluídos 100 saves/recargas, 30 recargas de mapas, 18 travessias e ciclos das três IAs. Não certificam campanha humana longa, audição ou controle físico.
- Prévia parcial: `outputs/previa-armas-20261024/ABRIR-PREVIA.cmd`, com armas separadas, ações/combos, formas e indicação de cobertura antiga. O feedback crítico acima permanece urgente apesar dos testes técnicos aprovados.
- Build Windows local parcial gerada em 24/10: `outputs/redesign-armas-20261023/Windows/DarklightRequiem.exe`. Inclui o escopo técnico testado; não significa redesign completo nem aprovação do Normal criticado pelo usuário. A build de expansão anterior foi preservada. Sem novas gerações após o pedido de adiar a conclusão para a próxima sessão.

## Prioridade 1 — teste manual e correções concretas

- [ ] Incorporar os defeitos encontrados pelo usuário na última build; registrar skin, forma, arma, direção e ação de cada problema.
- [ ] Conferir o idle novo das adagas de Réquiem nas quatro formas e oito direções; separar cobertura importada de direções ainda no banco anterior.
- [ ] Conferir transições idle → movimento → ataque → idle, mãos/empunhaduras, pivôs, escala e ausência de máscaras/recoloração antigas sobre as figuras novas.
- [ ] Verificar os novos efeitos com audição manual autorizada: volume, clareza, duração, repetição, sincronismo e silêncio ao pausar/morrer/trocar de cena.

## Prioridade 2 — concluir o redesign das armas

- [ ] Completar katana, Presas do Caos e manoplas/grevas dos Punhos nas skins Réquiem/Rock, formas Normal/Ruptura/Eclipse/Absoluto, direções e ações restantes. Usar a matriz de cobertura; não contar ícones ou folhas frontais como redesign global.
- [ ] Recriar somente os quadros completos de caminhada rejeitados por repetir a mesma perna; preservar os repousos/ataques corretos e fontes integrais.
- [ ] Completar combos, pesados, aéreos, esquivas, habilidades, dano e transições onde o novo equipamento ainda não aparece. Preservar timings, colisões e arte aprovada.
- [ ] Verificar os ícones de equipamento no HUD, inventário e guias conforme as formas efetivamente integradas.
- [ ] Continuar a revisão de olhos/pele/nitidez, somente em defeitos confirmados. Manter olhos vermelho escuro em todas as formas, cabelo/cristais próprios e pele de referência.
- [ ] Retomar o plano de 128 folhas físicas dos Punhos: o catálogo anterior tem 64 fontes/128 configurações e 46 fontes planejadas ausentes; não confundir paletas com novos desenhos.

## Prioridade 3 — áudio recebido e faltante

- [ ] Selecionar os trechos dos quatro MP3 longos já recebidos: WM02 (katana), WM06 (uppercut/martelo/solo), PW02 (Passo Sombrio), PW09 (Absoluto). Não reproduzir faixas longas inteiras em cada golpe.
- [ ] Completar as variações dos 19 efeitos ligados nesta rodada; revisar eventos parciais como guardar arma, retirada dos fones, término de poder e sinal distinto de recarga concluída.
- [ ] Receber os sons restantes da lista de pesquisa; priorizar motor/serra/derrapagem, cadeia aérea e os três visitantes. Bases genéricas existentes não significam ausência de áudio.
- [ ] Retomar separadamente as três músicas de bossfight adiadas: Going Under → Afterlife → See U in Hell, identificando as três principais pela progressão real. MP3 já fornecidos em Downloads; não publicar faixas comerciais sem autorização de distribuição.
- [ ] Atualizar `STATUS-SONS-20261023.md` a cada remessa: recebido, preparado, ligado, testado silenciosamente e ouvido são estados diferentes.

## Prioridade 4 — finalização e entrega

- [ ] Revisar as três IAs opcionais em sessões prolongadas; ampliar direções/animações necessárias e preservar recompensas únicas e saves.
- [ ] Fazer sessões longas nos capítulos, medir desempenho e incorporar teste manual das pontes; não alegar controle físico ou desempenho sustentado com base apenas em testes automáticos.
- [ ] Gerar e verificar uma nova build depois das correções; manter build anterior utilizável e atualizar o link do executável.
- [ ] Fazer commit/push apenas do escopo DARKLIGHT autorizado, pela cópia isolada. Excluir artes rejeitadas, saves, caches e arquivos de outros projetos; respeitar as restrições dos áudios.
- [ ] Registrar o relatório final desta rodada e os testes efetivamente executados, sem somar repetições como casos novos.

## Evidências de referência

- Pedido/design: `PROMPT-REDESIGN-ARMAS-20261023.md` e referências em `ArtSources/WeaponRedesign20261023/`.
- Arte/cobertura: `REDESIGN-ARMAS-ARTE-20261023.md`, `AUDITORIA-IDLE-ADAGAS-20261023.md`, `CONTRATO-REDESIGN-ARMAS-20261023.md` e `outputs/redesign-armas-20261023/`.
- Áudio: `STATUS-SONS-20261023.md`, `LISTA-SONS-PARA-PESQUISA-20261023.md`, `PENDENCIA-MUSICAS-BOSSFIGHTS-20261023.md`.
- Estado anterior e limpeza: `RESUMO-20261023.md`, `EXPANSAO-20261023.md` e `PENDENCIAS-20261023.md`.
