# Continuidade — prévias dos estados essenciais

Retomada a partir de AUDITORIA-ARTES-E-INTEGRACAO-20261112.md. O usuário pediu para ver as artes e entender o aviso de duração máxima da conversa durante a retomada.

- Revisadas visualmente as quatro fontes completas em ArtSources/EssentialStates20261112; preservados seus pixels e hashes.
- Criado Tools/AnimationPreview/preview_essential_states_20261112.py. Gera oito GIFs, seis quadros por ação (48 no total), em outputs/essential-states-20261112/previews, com fundos claro/escuro e relatório de alpha nas bordas.
- Verificados os hashes das fontes, limites dos retângulos, ordem e contagem dos seis quadros por clipe e contagem dos quadros dos GIFs exportados. Somente Walk repete. GIF tem paleta quantizada; PNG continua sendo a autoridade visual.
- Direções e pivôs ainda não calibrados. As prévias usam células inteiras na resolução nativa; não representam contato estabilizado no chão nem renderização Unity.
- A folha de caminhada/defesa de Serafim tem aura chegando aos limites das células. Diagnósticos de alpha não autorizam apagar efeitos ou afirmar que a anatomia foi cortada.
- Examinado o log headless anterior: a pilha fatal passa por Mono wakeup_pipes_init / System.Net.Sockets / HttpListener e UnityEditor.BuildReportRestService. Há também mensagens WMI/licenciamento. A causa raiz não está confirmada; não presumir que ativar licença resolverá tudo. Não houve nova tentativa idêntica, execução Unity ou build nesta retomada.
- Monitor OpenBot read_codex_limits.py ausente no caminho conhecido. Saldo e reset desconhecidos.
- Nenhum catálogo, importador, pivô ou PNG de Fabius Normal alterado. Alterações locais anteriores preservadas. Nenhum commit/push nesta etapa de prévias.

Próxima tarefa: calibrar direções e apoio dos quadros inteiros; revisar o contato de aura com as bordas, preparar importação e concluir os gates Unity do relatório anterior antes de declarar integração ou build validada.
