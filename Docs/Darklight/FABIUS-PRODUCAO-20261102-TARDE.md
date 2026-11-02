# Fabius Réquiem Normal — produção de 02/11/2026 à tarde

**Escopo substituído pelo pedido mais recente:** `ESCOPO-ENXUTO-FABIUS-20261102.md`. Não reiniciar as filas extensas citadas historicamente abaixo. Retomada existente atualizada para 02/11 às21h15min45s, America/Sao_Paulo, aproximadamente4h28 após o pedido.

Última ordem direta: finalizar Fabius Normal hoje, integrar os lotes concluídos imediatamente e deixar as suítes de testes para o final da produção. Preservar as proporções compactas da Katana corrigida, aprovadas pelo usuário. Não iniciar transformações nesta produção.

## Recuperado e integrado

- Primeiro lote: 15 clips/60 figuras/7 folhas importados. Três testes PlayMode isolados passaram antes das alterações atuais; último XML `outputs/retomada-20261102/integration-play-115427.xml`. Isso não certifica campanha completa, gráficos ou alterações posteriores.
- Carregado Katana: oito direções/32 figuras, importação Unity concluída em 19:18:57 UTC, sem novos testes. A tentativa anterior parou pelo limite de memória; a repetição foi iniciada com folga disponível e guardas preservados.
- ContraAtaque Katana: oito direções/32 figuras, importadas. Preservadas 31 figuras; corrigida integralmente apenas a figura de contato SE.
- Walk Punhos W e E: quatro figuras por direção, importados. Dois quadros originais de cada ciclo preservados; contatos/passagens complementares novos. Revisão estática, sem alegação de movimento contínuo visto ou gameplay aprovado.
- OndaDoVeu Katana: oito direções/32 figuras importadas na Unity em 19:26:42 UTC, sem testes novos.
- Martelo da Ruína Punhos: oito direções/24 figuras importadas na Unity em 19:33 UTC, sem testes novos. Corrigidas cinco figuras completas de câmera; sete vizinhas preservadas. Tempos 160/100/260 ms, uma figura por fase.
- Juízo, Resposta Brutal e Cataclismo dos Punhos: oito direções/24 quadros por técnica, importados. KatanaAerial: oito direções/32 quadros, importados, com três figuras corrigidas integralmente e 29 preservadas. Serra de Entrada da Moto: oito direções/32 quadros, 31 figuras únicas; guarda SE reaproveitada na recuperação.

Neste checkpoint: 89 clips/324 quadros/33 folhas desta sessão, todos importados. Nenhuma suíte nova foi executada nesta produção. Totais e cobertura correntes são atualizados em `outputs/retomada-20261102/animation-progress-current.json`. Não somar números históricos como se fossem entregas novas.

## Trabalho ativo e fidelidade

Três agentes, com fontes e recibos exclusivos por arma: `normal-katana-completion`, `normal-fists-completion`, `normal-moto-completion` dentro de `ArtSources/WeaponRedesign20261102` e `outputs/retomada-20261102`. Somente a coordenação altera Assets, executa Unity e publica.

- Agentes encerrados; nenhuma geração em voo. Não reiniciar técnicas extras: substituídas por famílias compartilhadas conforme o escopo reduzido.
- Walk Punhos SW e Moto oito direções seguem pendentes. Candidatos com apoio repetido ou troca de câmera foram excluídos da integração.
- Próxima produção: caminhadas faltantes, defesa/dano/morte e somente defeitos comprovados de impacto/pilotagem. Fontes e recibos dos três agentes preservados.

Não montar anatomia, espelhar assimetrias nem alterar pixels aprovados. PPU expressa densidade nativa; não corrige pernas ou maturidade do rosto.

## Integração técnica preparada

- Contexto explícito `presentation=OnFoot|Mounted` nas páginas da Moto, preservando o comportamento histórico das páginas sem campo. Sequências a pé não substituem o piloto montado.
- `StateTime` avança na morte, enquanto o relógio de combate permanece congelado e os comandos continuam inativos.
- Sequências aéreas sem contato podem declarar `contactless=true, activeFrames=0`; não adicionam impacto ou dano à Perseguição Aérea.
- Essas alterações compilaram na importação de pacotes. Validação funcional está adiada por ordem do usuário.

## Entrega e preservação

Build Windows atualizada ainda não produzida. Publicação continua exclusivamente em `work/publicacao-20261021`, branch `integracao-completa-20261026`; último push confirmado antes desta produção: `bb4cb6935db62939763c781b174fc9317597508e`. Não publicar novas artes como runtime aprovado nem contornar gates.

Disco observado no início desta produção: aproximadamente 19,6 GB livres. Não apagar caches durante importações ativas; preservar fontes, saves, 59 arquivos únicos recuperados, histórico Git e última build válida. Limpeza final somente de temporários comprovados e caches regeneráveis, com Unity fechada.
