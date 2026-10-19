# Pendências de hoje — 18/10/2026

Registro histórico. Estado atual e entregas: [PENDENCIAS-20261019.md](PENDENCIAS-20261019.md).

Registro em andamento. Encerramento solicitado para 22h30, horário de Brasília. O fechamento deve atualizar o horário e o commit efetivo; não considerar este rascunho um push concluído.

## Concluído e verificável na prévia

- [x] Remover redução automática de exibição e calibrar altura pelo corpo, sem medir a arma.
- [x] Comparação com original em escala fixa; referência intacta e apoio comum.
- [x] Selecionar fontes grandes da caminhada frontal nas duas skins e quatro formas, excluindo folhas reduzidas/rejeitadas.
- [x] Incluir 28 combos frontais: katana/adagas/corrente nas duas skins e Cavaleiro somente Rock.
- [x] Separar 3 golpes de katana, 4 de adagas, 3 de corrente, 3 de serras e investida da moto.
- [x] Aplicar efeitos próprios por arma e paleta de cada transformação, com branca prateada.
- [x] Corrigir efeitos falsos no cabelo/cristais e olhos; preservar apenas armas visíveis.
- [x] Preservar arma/forma em repouso e na transição.
- [x] Identificar figuras inteiras, sem montagem de partes; preservar hashes dos PNGs.
- [x] Conferir scripts, dados, limites e carregamento das armas/formas no navegador.
- [ ] Concluir a conferência de moto, branca e transformação no navegador e registrar evidências.
- [ ] Promover a página revisada para previa.html.
- [ ] Finalizar commit/push da seleção correta, sem arte rejeitada ou alterações alheias.

## Arte e animação ainda abertas

- [ ] Corrigir a alternância real dos pés nas outras sete direções, nas duas skins/formas. As novas SW/SE foram rejeitadas após tentativas específicas; não ampliar essas falhas para outras formas.
- [ ] Corrigir diferenças residuais de largura da cabeça, torso/pernas e expressão entre famílias.
- [ ] Conferir cristais brancos salientes, cabelo até a parte superior das costas e pele baseada na Requiem original.
- [ ] Ataques direcionais e caminhada com as outras armas, sem simular cobertura inexistente.
- [ ] Revisar continuidade dos ciclos e transições de todos os estados com o usuário. A auditoria técnica não equivale à aprovação visual.

## Integração e jogo

- [ ] Integrar apenas o material coerente/aprovado no Unity; esta revisão ainda é protótipo externo.
- [ ] Conferir tempos e eventos de dano reais com os novos combos e efeitos.
- [ ] Testar combate real, colisões, custo dos efeitos e desempenho.
- [ ] Reavaliar pendências gerais anteriores: retorno ao checkpoint/Game Over, efeitos de áudio restantes e profundidade das pontes do mapa 5. Não repetir suítes sem mudança ou motivo novo.
- [ ] Validar uma nova build após a integração.

## Preservação e publicação

Não modificar os 8 arquivos protegidos de morte/reversão ou os saves. Não iniciar áudio. Usar somente os arquivos selecionados pelo manifesto e uma lista explícita no clone de publicação; o checkout principal e o clone contêm alterações/exclusões alheias que não podem entrar por inclusão global. A autorização atual é commit/push no repositório DARKLIGHT; não alterar portfólio nem publicar outro site.
