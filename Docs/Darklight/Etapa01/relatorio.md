# Etapa 01 — pacote da referência visual aprovada

Fábio aprovado com rosto e cabelo preservados, proporção adulta, sobretudo de couro preto, katana e pequenos spikes metálicos nos ombros. Fonte final desta revisão: `fabio-sprites-spikes-v4.png`. A versão com spikes substitui v3 como referência de figurino, sem excluir os estudos anteriores.

## Entregas

- PNG aprovado em oito vistas estáticas, copiado para os arquivos de trabalho do jogo.
- Manifesto de inspeção com dimensões, retângulos detectados e limitações.
- Prévia de escala em navegador, com propostas de 64/72 pixels e referência do atlas herdado.
- Prompts e evidências técnicas preservados.
- GDD consolidado e plano de montagem atualizados com Dark Resonance e trilha do protótipo.

## Inspeção e limites

A fonte mede 1536×1024 e usa RGBA; a análise detectou oito componentes principais pelo limiar alpha≥200. Esse limiar encontra a silhueta, mas não inclui necessariamente todas as bordas suaves. Os valores medidos estão em `verificacao-v4.json`; o manifesto não é um contrato definitivo de importação.

O equipamento ultrapassa limites de células regulares, há alpha intermediário e fundo/halo suave. Revisar as diagonais traseiras, mão dominante e corrente antes de fechar oito direções de produção. Pivôs, PPU e resolução lógica permanecem propostos. A folha atual é estática e não contém animações de caminhada ou ataque.

A comparação usa CSS com amostragem pixelada e não altera nenhum PNG. Ela mostra opções de escala, sem aplicar câmera, PPU ou mudanças nas cenas. O atlas antigo foi copiado somente como referência de escala; seu personagem não representa a identidade de DARKLIGHT.

## Próxima implementação preparada

Acabamento dos recortes e pacote de ações do protagonista; depois, laboratório de combate da etapa 02 conforme `../planejamento/montagem.md`. Dark Resonance permanece exclusivamente no planejamento. Nenhuma cena, script de gameplay ou áudio foi alterado nesta entrega; não foram executados novos testes Unity.
