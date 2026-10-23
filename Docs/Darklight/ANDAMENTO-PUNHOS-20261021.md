# Punhos da Ruína — andamento e pendências, 21/10/2026

## Situação da entrega

**Em preparação; as animações completas ainda não estão integradas.** O visual da arma e o nome já foram aprovados pelo usuário. A aprovação não foi solicitada novamente. A matriz completa continua aberta: duas skins × quatro formas × oito direções × bancos Combat/Motion de 12 figuras, alvo de 128 bancos/1536 figuras. Não contar candidatos gerados como cobertura revisada.

## Alterações desta continuação

- Perfil próprio no índice 2: sequência soco/chute/giro/finalização, uppercut, golpe aéreo descendente, carregado, resposta e especial. Alcances curtos e ausência de puxão/blink. Katana, adagas e Cavaleiro preservam seus caminhos existentes.
- Catálogo e importador preparados para a matriz inteira, identificação própria de sprites e máscaras, manutenção do GUID da arma e do índice dos saves. A substituição permanece desativada sem catálogo completo; nenhuma das folhas candidatas foi importada como arte final.
- Rastros de socos/chutes e impactos preparados com cor da forma e intensidade de combo/carregado. A implementação compila, mas o acabamento da aura da arte e a sincronização visual ainda exigem integração/revisão.
- A captura de cabelo indicada pelo usuário foi preservada integralmente em `ArtSources/AbsoluteHairReference20261021`. Absoluto: ondas brancas volumosas até a região superior/média das costas, acima do cinto. Novas figuras completas em `Requiem_Absoluto_SE_Combat_HairV2`, `Rock_Absoluto_SE_Combat_HairV2` e `absoluto-cardinal-hair-v2`; as duas versões anteriores de cabelo comprido foram superadas.
- Após o usuário rejeitar a marcha lateral, foram produzidos passos mais baixos como figuras completas. Prévia mais recente: `outputs/punhos-ruina-20261021/caminhada-baixa-v5/preview/preview.html`, oito desenhos por skin. Apoio do chão anotado pelo corpo conectado, sem deslocamento senoidal do canvas, espelhamento ou montagem anatômica. **A alternância convincente das pernas ainda não está confirmada.** A folha rejeitada de joelhos altos não pode entrar na entrega.
- Folhas novas de combate de frente e perfil esquerdo usam uma única referência direcional inteira. Ainda candidatas; conferir contatos, escala e todos os quadros antes de contá-las como cobertura.

## Evidências e limites

Atualização final desta revisão: prévia guiada em `outputs/punhos-ruina-20261021/caminhada-guiada-v11/preview/preview.html`, quatro quadros por skin, 150 ms/quadro. Oito figuras, zero erros e nove avisos estruturais (paleta/alpha). Requiem conserva os quatro quadros completos da fonte v9; Rock usa quatro novas figuras inteiras e uma única escala uniforme nearest-neighbor. Os oito quadros e os controles foram inspecionados no navegador. Melhoraram joelhos baixos, balanço dos braços e estabilidade do chão; a leitura inequívoca da alternância das pernas continua em revisão. A prévia v10 tem erro de escala variável de Rock e foi superada pela v11; não entregar a v10.

A cadência preparada no catálogo dos Punhos passou de 0,4 s para **0,6 s por ciclo**, mantendo as demais armas intactas. Última execução Unity EditMode: **6 executados, 6 aprovados, zero falhas**, em `../work/ruin-fists-cadence-editmode-20261021.xml`; resumo `../work/compact/20261021T233324Z-5e9538db-ruin-fists-cadence-editmode.json`. São seis casos distintos no total, não onze somando as duas execuções. A substituição ainda depende do catálogo completo e não foi ativada no jogo.

- Compilação das três assemblies Runtime/Editor/EditModeTests: zero erros. Log: `../work/compact/20261021T225550Z-4c8bb5e2-ruin-assemblies.log`.
- Unity 6000.6.0f1, EditMode, `DarklightRuinFistsProfileTests`: **5 executados, 5 aprovados, zero falhas**. XML: `../work/ruin-fists-editmode-20261021.xml`; resumo: `../work/compact/20261021T230257Z-84e74007-ruin-fists-editmode.json`. Verificam contatos/combos, fases e rejeição de catálogo incompleto/duplicado; não certificam desenho ou gameplay integrado.
- Prévia de caminhada v5: 16 figuras completas, conservação RGBA das fontes, zero erros estruturais e 17 avisos. Os avisos são paleta contínua acima de 256 cores e alpha parcial. Há HTML/GIF/contato, JSON e histogramas reais. Não houve quantização nem certificação de resolução lógica nativa.
- Fontes e alguns estados do HTML foram inspecionados; a revisão integral dos loops e sua transição com gameplay permanece aberta. Revisão técnica não equivale a aprovação anatômica.
- Nenhum Play, música, audição, teste de controle físico ou build nova. O pedido mais recente autoriza **um aviso sonoro de conclusão**, seguido de commit/push; não autoriza iniciar música ou Play.

## Problemas artísticos em aberto

1. As primeiras folhas de uma direção com muitas poses repetiram SE em vez da vista solicitada. A fila automática foi interrompida. As candidatas S/SW/W/NW/NE/E anteriores não são cobertura validada só por seus nomes. Nas folhas N há contatos laterais que exigem revisão.
2. As folhas de oito vistas distinguem melhor as câmeras, mas algumas células SW ainda copiam SE; algumas antecipações de soco já mostram o braço estendido. Preservar os quadros corretos e substituir somente figuras completas afetadas.

   A tentativa adicional RefocusedV3 usou somente referência inteira da câmera correta + aparência da skin/forma + olhos. Gerou Rock Normal S/W e Requiem Normal N/SW. As poses de guarda distinguem as câmeras, porém N ainda apresenta contatos laterais e SW tem inconsistências de orientação em alguns golpes; essas folhas continuam candidatas, sem ativação ou cobertura certificada. Não retomar expansão cega por nome de arquivo.
3. As tentativas de caminhada baixaram os joelhos, mas ainda repetem a perna aparente em alguns contatos e precisam de balanço de braços mais convincente. O estudo independente `walk-west-text-only-v7` altera a identidade e não é uma substituição utilizável. Não transformar repetição de poses em alegação de ciclo real.
4. As fontes geradas ainda têm cores contínuas/alpha parcial. Refinamento de pixel art nativa e alinhamento de escala/pivô continuam necessários.

## Próximas pendências autorizadas

1. Resolver a caminhada lateral com apoio e alternância claros; aplicar o mesmo padrão às direções restantes e conferir início/fim em movimento.
2. Completar realmente os 128 bancos, preservando cabelo, olhos vermelhos, pele e cristais/mechas de cada forma. Não usar outra direção, forma ou arma para encobrir ausências.
3. Empacotar personagens inteiros, máscaras, fontes/hashes, PPU/pivôs e eventos; revisar e importar. Validar repouso, locomoção, contatos, combos, esquiva, guarda/parry, dano e morte em edição, mantendo Play/áudio suspensos.
4. Atualizar os textos/ícones restantes do terceiro estilo e conferir aura, intensidade e oclusão nas duas skins/quatro formas. Só ativar a substituição com cobertura completa.
5. Depois, retomar `PENDENCIAS-20261020.md`: olhos/nitidez nos demais sprites, ciclos reais e transições restantes, pontes do mapa 5, Cavaleiro e avaliação de capítulos/desempenho conforme as permissões vigentes. Esses itens não foram declarados concluídos nesta continuação.
6. Ao concluir de fato, tocar o aviso curto solicitado e fazer commit/push somente dos arquivos pertinentes, excluindo candidatas rejeitadas, saves e conteúdo de outros projetos.

## Preparação do GitHub

Destino verificado: `https://github.com/fwbiodev7/darklight-requiem-game.git`, main remota `04a271f46ace3c68288705ef9ee8ef5f7c732898`. O checkout principal vem da fundação antiga, tem 1652 alterações rastreadas anteriores e não tem remoto; não enviar seu estado indiscriminadamente.

Foi criada uma cópia isolada em `work/publicacao-20261021`, a partir da publicação existente, e configurado o destino correto. A cópia anterior em `../work/public-alpha` não foi modificada. Commit/push e aviso de conclusão **ainda pendentes**, porque a matriz artística e a integração não foram concluídas. Nenhum conteúdo foi enviado ao repositório Varginha.
