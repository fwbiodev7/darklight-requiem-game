# Cristais brancos — candidatas v2

Duas edições integrais pela ferramenta integrada: Requiem e Rock, oito figuras frontais grandes em cada folha de 1536 × 1024. Os cristais brancos/prateados agora têm facetas e saliência nos ombros e punhos. As figuras, armas, botas e margens permanecem inteiras; Rock segue sem mangas e sem tecido abaixo do cinto. Não houve montagem por partes, recorte de elementos, redução ou alteração das fontes originais.

As duas folhas novas ficam **não selecionadas**, com `visualApproval: false`. Frontend, builder, plano de seleção e jogo não foram alterados. Foram feitas uma geração por fonte e nenhuma correção adicional.

Calibração proposta pela pose neutra 3, com marcas aproximadas de topo do cabelo e sola completa: Requiem 431 px → 266 px, escala 0,61716937355; Rock 440 px → 278 px, escala 0,63181818182. A altura mudou menos de 1% em relação à fonte anterior; a medida exclui a extensão da arma. A escala foi registrada, sem ser aplicada ao jogo.

## Ressalvas da inspeção

- Há oito componentes inteiros por folha e nenhum chega à borda com alpha de figura. Persistem resíduos mínimos de alpha1 na borda: um pixel no Requiem e quinze no Rock. As fontes anteriores também tinham resíduos; não houve limpeza algorítmica de alpha.
- O contorno frontal do cabelo branco foi preservado. Uma folha apenas frontal não comprova diretamente o comprimento na parte superior das costas; o desenho aprovado foi usado como referência e essa conferência permanece visual.
- A edição gerativa não é idêntica pixel a pixel à fonte. Cristais, identidade, proporção e continuidade de caminhada ainda são candidatas à revisão visual.
- Esta inspeção não executou Unity, navegador, áudio ou controle físico.

Prompts, entradas e hashes: [FONTES-CRISTAIS-BRANCOS-V2.json](FONTES-CRISTAIS-BRANCOS-V2.json). Retângulos reais, alpha e calibração: [QA-CRISTAIS-BRANCOS-V2.json](QA-CRISTAIS-BRANCOS-V2.json).
