# Pendências — 20/10/2026

## Entregue

- Skill instalada/versionada; novos lotes em folhas de **8–12 sprites**.
- 12 figuras completas integradas: quatro ataques frontais de katana e oito poses de adagas, Requiem roxa. Adagas v2 é válida; originais, roupa, armas e mechas roxas preservados.
- Pele: 260 fontes anteriores + duas novas, base [246,174,130]. Íris: rampa comum nos canais semânticos dos três shaders.
- 11 testes Unity EditMode distintos, quatro verificações da folha refinada, 23 testes de sprites e oito de saída compacta aprovados. Sem Play/áudio/build nova; 1.0.13 permanece anterior à nova arte.

## Olhos e nitidez de todo o player

1. Corrigir íris e manchas sob os olhos dos demais sprites autorados: duas skins, quatro formas, katana/adagas/corrente e Cavaleiro. Máscaras de ownership sem canal de íris não recebem a correção comum. O pedido global **ainda está aberto**.
2. Usar o desenho da captura em `ArtSources/PlayerEyeReference20261020`. Cor atual: vermelho escuro visível #C40814/#5A0000; vinho marrom rejeitado. Pele pelo repouso Requiem. Preservar cabelo/comprimento branco, mechas/cristais, roupa/armas e idade aprovados.
3. Inventariar os quadros realmente borrados/defeituosos; priorizar Rock, demais formas, outras armas/direções. Gerar folhas de 8–12 figuras completas com o nível do sprite separado em `ArtSources/PlayerStyleReference20261020`. Preservar quadros corretos; não montar partes anatômicas.
4. Revisar cada quadro/clip: proporções, olhos e pele, mãos/armas/pés, cabelo, contornos, alpha, oclusão e costas. Detector numérico não certifica identidade ou aprovação artística. As antigas propostas locais de detecção de olhos não foram integradas.
5. Completar a matriz skin × forma × arma × ação × direção. As oito adagas cobrem frente/sudoeste de um banco de suporte, **não** todos os combos/direções. A katana cobre quatro poses frontais roxas. Preservar outras referências até existirem substitutos revisados.
6. Finalizar, quando necessário, a paleta indexada/antialiasing no editor nativo: as fontes geradas têm muitas cores RGBA e alpha parcial. Não presumir paleta restrita ou grade lógica perfeita só pela aparência. Os histogramas reais foram extraídos; não houve quantização automática das figuras.

## Movimento e jogo

- Completar ciclos reais de caminhada nas outras direções/armas, sem repetir a mesma perna nem substituir arma ausente por katana.
- Rever transições repouso/caminhada/ataque/combos/guarda/esquiva/dano, pivôs e eventos de contato. Clips HTML de prontidão/esquiva são sequências de inspeção; tempos de gameplay preservados. Retornos de crouch/dive reutilizam poses intencionalmente; mais intermediários podem melhorar o movimento.
- Conferir cenário/câmera/iluminação e transições no jogo quando compatível com as autorizações. Nesta rodada só EditMode/HTML; nenhum controle físico testado.
- Gerar nova build depois da revisão global e avaliar seis capítulos/desempenho. A nova arte não está no executável 1.0.13.
- Herdadas de 19/10: percurso completo das três pontes do mapa 5, desempenho prolongado no executável, efeitos restantes de Cavaleiro/moto e sincronização. **Audição, Play e música continuam suspensos até pedido explícito.**

## Continuação técnica

Ver `RELATORIO-ARTE-20261020.md`, `outputs/padronizacao-artes-20261020/QA-ENTREGA.json` e XMLs anexos. Reusar importadores/catálogos, GUIDs e eventos. `inventory_player_eyes.py` recria inventário com retângulos reais. Logs completos em ../work/compact e ../work/visual-20261020-*.log. O resumo não usa modelo local; não alegar 99% de economia. Publicar somente caminhos pertinentes no DARKLIGHT; excluir artes rejeitadas, saves e mudanças alheias. Site e portfólio fora desta rodada.
