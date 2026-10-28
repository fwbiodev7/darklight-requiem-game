# Critérios de qualidade e relatório

## Portões de entrega

**Estrutura:** fonte e SHA rastreáveis; PNG legível com alpha real quando isolado; retângulos válidos; sem perda de pixels; frames/pivôs/tempos válidos; clips e matriz solicitada completos; GUIDs preservados. Falha estrutural bloqueia integração.

**Inspeção visual obrigatória:** identidade do repouso; largura/altura anatômicas, rosto/olhos/pele/cabelo, roupa e arma constantes; mãos/pés completos; ausência de olheiras indevidas; sombras coerentes; clusters limpos e resolução nativa; transparência sem tabuleiro desenhado. Examinar todas as direções/skins solicitadas. Detectores numéricos são alertas, não aprovação artística.

**Animação:** poses distintas de verdade; contatos e passagens corretos; arma acompanhando mão; pivô sem jitter indevido; retorno ao início e transições; morte sem loop; janela de dano conforme gameplay. Frames idênticos só quando hold intencional.

**Unity:** importação/configurações reais, metadados consumidos, nomes/IDs, PPU e pixel-perfect na câmera, transições e sincronização das ações afetadas. Prévia HTML não prova integração. Registrar ferramenta/versão e teste executado; nunca declarar teste de controle físico sem execução real.

## Relatório após cada entrega

Registrar: objetivo, referências e hashes; arquivos criados/modificados e status; nativo/fonte, atlas, frames, ações/direções/skins, paleta, pivôs, PPU e timings; preservação de originais; correções; auditoria com erros/avisos; contato e prévia animada **vistos ou ainda não vistos**; teste no Editor/runtime **executado ou pendente**; limitações e próximo defeito concreto. `visualApproval` só quando há aprovação explícita; `visual-reviewed` significa inspeção documentada pelo agente e não aprovação do usuário.

Cada entrega deve permitir distinguir arte completa, candidata rejeitada, prévia local, asset importado e build verificada. Se qualquer requisito pedido faltar, listar como pendência; não substituir por promessa de qualidade profissional automática.
