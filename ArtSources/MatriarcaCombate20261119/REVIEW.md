# Matriarca Rubra — piloto de combate, 19/11/2026

**Estado:** produzido e revisado visualmente; staging ainda não importado nem testado na Unity. Uma geração integrada, nenhuma correção adicional. Nenhuma alteração em Assets, Runtime, catálogo ou Git por este agente.

Entregas principais: `MatriarcaRubraCombat-staging-v1.png`, `.png.meta` e `.json`; fonte preservada em `MatriarcaRubraCombat-source-v1.png`. A folha fonte é 1448×1086, oito células de 362×543. O atlas técnico é 1536×1216, oito células de 384×608. Houve somente translação inteira de figuras completas, sem recortar anatomia, redimensionar, espelhar ou modificar cores/alpha. `whole-frame-pixel-proof.json` comprova igualdade RGBA dos oito quadros com suas respectivas células originais.

O idle novo serve somente à inspeção; **base[0] permanece o idle aprovado em OfficialCast**. Ordem dos outros nomes registrada integralmente no JSON e no TextureImporter: base StepA / StepB / Guard; ataque Preparation / Contact / Followthrough / Recovery. PPU **169.576346273751**, localScale esperado **1.325**, corpo de referência **351 px → 2.74257 unidades**, sem incluir halo. Apoio técnico comum no atlas: **(192,520)** dentro da célula, pivô normalizado **(0.5,0.144736842105263)**. Pequenas variações do comprimento desenhado do corpo permanecem; não foram corrigidas por deformação.

Consumidor consultado: `DarklightEnemy.cs` e `DarklightEnemyAttacks.cs`. Matriarca comum usa preparação de 600 ms × modificador de dificuldade, contato total de 250 ms (125 ms em attack[1], 125 ms em attack[2]), recuperação de 220 ms. A passada é alternada pela distância percorrida, a cada 0.28 unidade; os 180 ms do preview são ilustrativos. Fases 0/1/2/3 mantidas. Não há arcos ou grandes efeitos embutidos que cubram os avisos de gameplay.

Inspeção: fonte integral, contato estático e prévia HTML local abertos. Conferidos a passagem Preparation→Contact→Followthrough→Recovery, o retorno para a espada baixa e a alternância das botas A/B, com reprodução e avanço de quadros no navegador. Cabelo longo prateado/lavanda, olhos vermelhos, pele clara quente, vestido rubro/preto, ornamentos dourados, rosas, crânios, asas e halo são reconhecíveis. A mão direita conserva a espada e a outra contrabalança. Preparação eleva espada à direita da personagem; contato atravessa o corpo; seguimento termina do lado esquerdo antes do retorno.

Limites visuais reais: o detalhe da nova folha é mais compacto que o idle de alta resolução; continuidade exata entre ambos precisa ser vista pela coordenação na câmera real. A fonte contém **472386 cores RGBA distintas**, 256 níveis de alpha e emissão vermelha tênue no espaço vazio. Não representa uma paleta indexada estrita; nenhuma limpeza global, quantização ou corte de brilho foi aplicada. `palette.json` conserva as 256 cores mais frequentes e a contagem total, evitando uma tabela de 53 MB. A cor material mantém rubro/prata/dourado; a frequência automática de cores não substitui essa leitura.

Auditoria final em `audit-final/report.json`: **0 erros, 2 avisos**, referentes a alpha parcial e ausência de comprovação da importação/runtime. O aviso inicial de diferença decimal no PPU foi corrigido alinhando a precisão exata do JSON e `.meta`; `audit/` é a evidência anterior, superada por `audit-final/`.

Prévia: `preview/preview.html` (sem áudio), GIFs derivados e `preview/contact.png`; `contact-source-dark.png` é a referência compacta 4×2 em fundo escuro. PNG/JSON são autoritativos, pois GIF quantiza cores. A revisão local não equivale a aprovação humana desta animação.

Pendências da coordenação: integrar sete nomes respeitando o idle original; verificar altura/apoio nas transições e contato no consumidor real. O código existente aplica flipX por direção, portanto este piloto frontal não certifica vistas opostas assimétricas. Sem novas direções ou expansão artística nesta entrega.

SHA-256 da fonte: `48fe3c450c8d0275a8ee2926c55ddd5f51f35189656b51a4edb277c4a2d77952`.

SHA-256 do staging: `7e7e83d1fbdfbc114b11394734b487cad0033591383708ac9c9d3d9194f03c74`.

Referências e hashes completos estão no manifesto. O prompt usado solicitou as oito figuras completas na câmera frontal do idle aprovado, com as oito ações acima, constância de identidade, arma na mão direita, transparência e ausência de grandes VFX; o serviço integrado image_gen realizou a geração.
