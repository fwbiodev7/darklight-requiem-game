# Entrega para testes públicos — 15/11/2026

Publicação autorizada pelo usuário na main e como pré-release para download pelo site.

## Escopo

Integra o trabalho publicado em `413f637` (animações, transformações oficiais, Gothic Eclipse e correções de colisão) à main, acrescentando documentação e o diagnóstico de testes de 15/11. Não implementa as oito fases futuras nem altera o site.

## Pacote Windows x64

Origem: `outputs/build-windows-gothic-20261114-feedback/Windows/`, entrega testada manualmente pelo usuário em 14/11. Não recompilada nesta publicação. O build-result original registra falha na cópia final da Unity; o distribution-result registra o reparo com cópias independentes e hashes dos dados. Ambos são preservados no ZIP.

Arquivo: `DarklightRequiem-Windows-x64-Teste-20261115.zip`. Extrair a pasta inteira antes de abrir `DarklightRequiem.exe`. Verificar o SHA256 fornecido junto ao download. O pacote não inclui saves pessoais.

Tag prevista: `teste-20261115`. Identificar no site como **Versão de testes — Windows x64**, vinculando ao asset da pré-release, sem apresentar esta entrega como versão final.

## Validação e limitações

O relatório `RELATORIO-TESTES-FINAIS-20261115.md` registra 70 testes únicos aprovados e uma falha pendente no percurso automático da região 0. As outras nove regiões não foram validadas por esse percurso. Esses testes pertencem ao checkout e não certificam automaticamente o executável empacotado. Nenhuma nova execução do jogo ou reprodução de áudio foi feita nesta publicação.

As melhorias de Combate 2.0, HUD/menus 2.0, Lucius 2.0 e demais fases do `weekly_reset_prompt.md` são planejamento futuro. Moto e Serras ainda podem existir nesta build histórica; sua remoção pertence à Fase 1 futura. Não anunciar o arsenal reduzido como já entregue neste pacote.

O código é publicado no Git; o binário e seu checksum são assets de uma pré-release GitHub. O site pode usar o link de download sem hospedar o executável no histórico Git.
