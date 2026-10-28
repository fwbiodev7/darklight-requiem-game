# DARKLIGHT: REQUIEM — instruções do projeto

## Direção vigente — 27/10/2026
- Réquiem é a única skin ativa; Katana, Punhos e Moto; Normal, Ruptura e Eclipse Absoluto. Instruções históricas contrárias abaixo estão superadas pelo contrato ESCOPO-CANONICO-20261026.md.
- O usuário exige revisão e correção autônomas das artes, sem pedir aprovação a cada lote. As animações aprovadas da Katana são a referência principal de identidade, desenho e maturidade, inclusive para o piloto da moto. PPU não corrige proporções ou rosto infantilizados.
- Usar agentes com contexto curto e tarefas independentes, conforme .codex/config.toml e .codex/agents/. Arte: gpt-6.1-sol/high; auditorias simples: gpt-6-luna/high. No máximo três subagentes simultâneos. A coordenação permanece responsável pela integração e execução única da Unity.
- Não deixar arte corrigível parada aguardando aprovação humana; corrigir figuras completas afetadas e preservar as corretas. Registrar candidato/revisado/importado/testado separadamente. Não declarar o trabalho inteiro concluído por terminar um lote.

## Encerramento e retomada automáticos — autorização permanente de 25/10/2026
- Autorização específica da retomada18h50 de25/10: executar Play Mode e auditoria/reprodução controlada dos sons existentes, com saves isolados e preservação de preferências/originais. Registrar reprodução técnica, saída medida e avaliação auditiva como estados distintos. Essa autorização supera a suspensão abaixo somente nesta rodada; não inclui adquirir/substituir/redistribuir músicas comerciais.
- Ajuste explícito da retomada13h50: encerrar esta rodada perto de **15% restantes**, com commit/push e checkpoint. Este horário/limiar supera os10% anteriores na rodada atual. Ordem artística reiterada: **Normal de Réquiem e Rock com todas as armas/ações → transformações → Lucius/O Encapuzado**. Manter moto exclusiva deRock.
- Em todas as próximas sessões deste trabalho, acompanhar o saldo de uso quando houver trabalho sustentado. Perto de 10% restantes, não iniciar novos lotes; concluir ou preservar com segurança os que estiverem em voo.
- Antes de encerrar por limite, salvar checkpoint datado com concluído, produzido, revisado, integrado, testado e pendente, fontes/hashes, falhas, agentes/fragmentos e próximos passos exatos. Não confundir esses estados nem declarar conclusão global parcial.
- Fazer commit/push do escopo DARKLIGHT autorizado pela cópia isolada, excluindo rejeitadas/superadas, saves, caches, MP3 originais, builds e outros projetos. Preservar alterações alheias.
- Criar/atualizar a retomada automática neste mesmo chat com esse contexto e os caminhos dos documentos, usando a automação existente e evitando duplicatas. Agendar após a renovação do limite disponível, com pequena margem; respeitar horário explícito do usuário quando houver. Não esperar novo comando para retomar. Hoje, 25/10, a retomada explícita é às 13h50 America/Sao_Paulo.
- A próxima sessão deve continuar a lista real com subagentes por arma/forma e repetir este procedimento ao se aproximar do limite. Ao concluir todo o objetivo, não perpetuar agendamentos desnecessários.
- Último ajuste visual obrigatório: cor/mechas fortes de Ruptura/Eclipse não autorizam aumentar volume, comprimento ou silhueta do cabelo. Presas/Ruptura crescida foi criticada; usar o corte canônico como autoridade e corrigir só figuras completas afetadas. Escala, idade e proporções permanecem constantes.

## Retomada urgente — feedback do usuário em 24/10/2026
- Prioridade máxima: `Docs/Darklight/PENDENCIAS-20261024.md`. Terminar o redesign de todas as skins/armas/formas/direções/animações/combos pela ficha `ArtSources/WeaponRedesign20261023/references/equipment-four-forms.png`, reanexada em 24/10. Normal deve ficar sem transformação e com equipamento neutro da coluna NORMAL; o usuário rejeitou a aparência de Ruptura no Normal. Esta correção supera instruções anteriores de energia carmesim no Normal.
- O redesign atual é parcial: Punhos/katana só têm primeiro ataque novo frontal; outras ações/posições ainda reutilizam arte antiga e filtros. Não declarar conclusão global nem aprovação visual. Retomar pela matriz existente, com leituras incrementais, folhas de 8–12 figuras inteiras e testes focados somente quando justificados.

## Economia de contexto e spritesheets — 20/10/2026
- Reduzir saídas antes de enviá-las ao contexto: buscar com `rg` por escopo, selecionar campos/contagens e ler somente trechos relevantes. Não imprimir `git status` completo neste checkout com muitas alterações; usar contagens ou caminhos pertinentes.
- Para comandos verbosos, usar `Tools/TokenEconomy/run_compact.py --label <objetivo> -- <executável> <argumentos>`. Logs completos em `../work/compact/`; preservar exit code, erros e evidências. Um resumo não substitui resultado estruturado, teste executado ou inspeção visual.
- Não alegar 99% de economia, Distill instalado ou modelo local ativo sem verificação e medição. O helper atual é determinístico e não muda o modelo do Codex.
- Para spritesheets, usar também `Tools/Skills/pixel-art-sprite-sheet-master/SKILL.md` (nome de exibição Pixel Art Sprite Sheet Master), preservando as regras locais de personagens inteiros, fontes, arte aprovada e suspensão de áudio. Não confundir prévia candidata com integração no Unity.
- Referência principal dos olhos do player, indicada expressamente em 20/10: `ArtSources/PlayerEyeReference20261020/referencia-olhos-player.png` (captura 19/10/2026 20:10:51). Preservar o mesmo desenho e cor dos olhos em todas as formas; não tingir íris, pálpebras ou pele pela transformação. Inspecionar a referência inteira, sem recortar/transplantar partes.
- Se apenas alguns sprites tiverem vermelho indevido abaixo dos olhos ou outro defeito, corrigir somente esses quadros completos e preservar os corretos pixel a pixel. Não regenerar toda a folha por defeitos localizados. Correção seletiva não autoriza montagem de olhos/cabeça/corpo por partes.
- Na forma roxa, manter as mechas roxas do cabelo e os cristais roxos. A referência principal dos olhos não autoriza copiar as mechas vermelhas da captura para essa forma.
- Refinamento mais recente dos olhos em 20/10: o vinho ficou marrom e o bordô anterior ficou pouco vermelho. Usar íris **vermelho escuro claramente visível**, próxima à captura, em todas as formas. Guia atual: base #C40814, sombra #5A0000; preservar pupilas, branco/brilho e pele limpa. Guias anteriores #80172C/#450B18 foram superados. Não recolorir cabelo, cristais, roupas ou armas junto dos olhos.
- Padronizar a pele do player em todas as folhas/formas/armas pela Requiem em repouso aprovada; base de pigmento medida [246,174,130], mantendo luz/sombra e identidades das skins. A correção do player não autoriza uniformizar a pele de todos os NPCs.
- Padronizar a nitidez das artes borradas pela qualidade dos sprites separados de 20/10. Para novos lotes da mesma identidade, o usuário pediu **folhas de 8–12 sprites por geração**, em vez de uma geração por sprite. Dar espaço aos quadros, verificar contagem/qualidade e preservar personagens inteiros. Defeitos localizados continuam restritos aos quadros afetados.
- Em 20/10 o usuário reiterou commit/push ao GitHub do DARKLIGHT ao encerrar esta rodada e lista datada das pendências restantes. Preservar alterações alheias; não enviar candidatas rejeitadas ou arquivos de outros projetos.

- O usuário autorizou em 10/10/2026 construir autonomamente o jogo completo: seis capítulos, inimigos, chefes, progressão, menus, interfaces e Dark Resonance. Essa autorização substitui os antigos checkpoints de aprovação por etapa e aprovação visual para arte nova; preservar Fábio e HUD já aprovados.
- O usuário aprovou as melhorias e testes focados de movimentação, combate, animações, mapas, poderes e menus. Evitar suítes repetidas; testar silenciosamente, sem iniciar reprodução de músicas. Polir HUD e menu sem substituir a arte aprovada.
- Nunca modificar o original C:/Users/Usuario/segredo-varginha-game-teste-cutscene-gdd-ldd-main. Não reintroduzir conteúdo de Varginha no jogo.
- Usar a skill segredo-varginha-pixelart para arte raster, com a ferramenta integrada de imagens. Preservar as fontes e registrar recortes reais.
- Usar somente os cinco MP3 fornecidos, sem baixar outras músicas. Projeto pessoal local. NPCs originais.
- Não publicar site/repositório, fazer commits/pushes ou modificar portfólio sem autorização específica.
- O usuário autorizou em 10/10/2026 a build 0.1 Alpha, commit e publicação pública em fwbiodev7 e um site próprio do jogo. Não modificar o portfólio ou outros repositórios.
- Reutilizar combate e HUD existentes; teclado/mouse e controle em gameplay e menus. Não alegar teste com controle físico.
- Manter logs em ../work/.
- O usuário pediu para parar de iniciar música automaticamente e testar áudio somente quando ele mandar. Não iniciar Play, MP3, trilha ou aviso sonoro por conta própria. Abrir a Unity em edição; testes sonoros e o aviso de conclusão ficam suspensos até pedido explícito.

## Regra permanente de arte — autorização expressa em 12/10/2026
- Nunca gerar recortes de elementos ou montar personagens por partes, inclusive no preparo das imagens. A primeira opção para corrigir arte é gerar sprites novos e completos. Cabeça, corpo e arma devem permanecer juntos no mesmo quadro inteiro. A identificação técnica de quadros inteiros da folha é permitida.
- Preservar roupas, armas, escala, estilo de ataque e envelhecimento aprovados. Esta regra substitui a instrução antiga sobre registrar recortes.
- Após gerar e implementar os sprites corrigidos, testar e documentar as alterações e fazer commit/push das mudanças pertinentes no repositório DARKLIGHT do GitHub, conforme autorização específica do usuário em 12/10/2026. Preservar saves, fontes originais e alterações alheias; excluir artes rejeitadas da entrega.
