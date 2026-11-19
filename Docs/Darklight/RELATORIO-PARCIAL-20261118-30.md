# Checkpoint para avaliação — 18/11/2026

**Pausa registrada ao medir30%restantes da janela/76%semanal.** Nenhuma nova geração é iniciada depois desse marco; fontes e configuração parcial preservadas para decisão do usuário.

## Condições da rodada

Retomada do commit local `d704ddbe778cf53db1ce84176d831454054513b2`, branch `codex/menus-combat-20261117`. Ordem vigente: continuar o trabalho e **parar novos lotes aos30% restantes**, apresentando o resultado para o usuário decidir continuação ou build. A ferramenta oferece saldo de uma janela de300min e semanal; não oferece contador diário separado. Reserva semanal20% preservada.

**Sem novos testes, Unity/Play, build ou push nesta rodada.** Menus de ontem continuam destinados à avaliação manual. Estados abaixo não certificam funcionamento no jogo.

## Artes produzidas e revisadas

A pedido do usuário, os lotes passaram a agrupar duas direções. A tentativa16figuras devolveu1086×1448, reduzindo a densidade do corpoN de~386 para289px. Foi escolhida a alternativa autorizada de **10figuras/folha, cinco por direção**:2Idle e3Walk. A caminhada usa contatoA→passagem→contatoB→passagem, reaproveitando a figura inteira intermediária sem mirror ou deformação.

- **S aprovado em17/11:** oito quadros originais intactos, sem reduzir ou redesenhar a caminhada. SHA256 `202f33f42d937ab55cc5cc2162b41c8d1881ea3ea329e8d18e04c895de70740e`.
- **SW:** candidata individual já estava produzida quando a ordem mudou; foi aproveitada em vez de desperdiçada. A seleção conserva7/8quadros RGBA originais e corrige somente a figura completa de contatoB. O pequeno rebound original foi considerado natural; não se deslocou uma figura estática para simular salto. O quarto desenho de caminhada fica preservado na fonte, fora da sequência que reutiliza a passagem neutra.
- **N/NE:** seleção pareada com10figuras. Empacotamento técnico em1920×1024 preserva pixels e figuras completas, sem resample. Oito células corretas foram preservadas e duas figuras NE corrigidas (salto e contato oposto). N tem lift11px (~2,61%); NE6px (~1,38%). Fundo transparente confirmado por leitura de alpha/revisão estática; RGB marrom oculto não foi apagado por engano.
- **W/E:** nova folha gerada e preservada como candidata. Contatos alternados ainda precisam ser confirmados/corrigidos; não entra no conjunto revisado por inferência. Fonte/medidas finais constam no manifesto artístico.
- **NW/SE:** não iniciados nesta rodada para respeitar a pausa solicitada.

Identidade, roupa, rosto, cabelo, pele, manoplas/grevas e proporções seguem o frontal aprovado. Pulinhos são poses desenhadas, com linha do chão explícita, sem ancorar cada sola elevada para apagar o movimento. Originais e pivôs antigos de Fabius Normal não foram editados.

Fontes/selecionadas/prompts/provas/GIFs: `ArtSources/FistsCombatStance20261118/`. Receita: `recipe.json`. S permanece na pasta de17/11. Reprodução contínua não foi conferida; revisão estática não é teste de runtime.

Fechamento artístico:26figuras nas fontes selecionadas,25referenciadas no playback (um WalkSW original fica preservado fora da sequência). W/E tem10figuras candidatas completas, retângulos reais e alpha confirmado; apoio alternado/pivôs continuam pendentes. Arquivo: `candidates/FabiusFistsPair-W-E-v1-source.png`, SHA256 `274f1f931ff8749b956e593bb7a74d7fa2361a5d49ca2003eed0e3e88182bedc`. Recibo: `review/PairWE-v1.json`. Nenhuma geração ficou em voo na pausa.

## Integração preparada

Novo `DarklightFistsCombatStanceBank`, com três hooks mínimos no banco existente, limitado a **Réquiem/Normal/Punhos/Idle+Walk**. Não muda ataques, defesa, dano, morte ou transformações.

Configurados em Resources **S, SW, N, NE:8clips,25figuras únicas,28tempos**, em três PNGs copiados byte a byte das fontes revisadas. MetadadosSpriteMultiple, IDs/GUIDs estáveis, retângulos reais, ground explícito e PPU único por PNG; densidades calibradas pela altura física de referência, sem consertar anatomia por escala. Point, Clamp e ausência de mipmaps fazem parte das guardas.

**Pacote incompleto e inativo.** O banco só disponibiliza a substituição quando houver16clips nas oito direções e os sprites importados realmente corresponderem aos metadados. Por enquanto permanece o fallback integral das animações atuais; não há mistura parcial. Configuração de importação é distinta de importação efetivamente realizada pela Unity.

Recibo: `outputs/retomada-20261118/fists-combat-stance/import-configuration.json` — importConfigured=true, imported=false, runtimeVerified=false. Gerador: `Tools/AnimationPreview/prepare_fists_combat_stance_20261118.py --apply`.

## Compatibilidade aérea de Lucius

Implementação específica do Combate2.0, sem declarar a bossfight2.0 concluída:

- Lucius ganha abertura breve para lançamento após quebra de postura ou parry bem-sucedido. Fora dela permanece resistente; outros bosses/blindados mantêm a regra anterior.
- Uma vez lançado, usa o limite de juggle, gravidade, slam e imunidade de aterrissagem existentes. Não executa ataque terrestre durante voo.
- Altura visual fica no renderer do LuciusView, sem mover Rigidbody/colliders. O offset genérico não interfere no renderer próprio nem desloca o ator-root para representar altura. Queda continua após morte aérea.
- Sem alterar HP, dano, os três golpes canônicos, armas, formas ou artes de Lucius. IA/dificuldade completa permanece para a etapa posterior.

Somente revisão de código; nenhuma compilação, suíte, cena ou audição executada nesta rodada.

## Pendências para a decisão do usuário

1. Revisar/corrigir os contatos W/E preservando figuras boas; produzir NW/SE em lote pareado.
2. Fechar oito direções e configurar o pacote completo, mantendo frames aprovados e identidade. Só então o override poderá ficar disponível após importação.
3. Avaliação manual dos menus; integração final de Combate2.0 e bossfightLucius2.0 seguem pendentes.
4. **Uma build feita agora ainda usaria a locomoção atual dos Punhos**, pois o novo pacote está incompleto. O usuário decidirá se prefere continuar as artes ou gerar build para avaliar menus/código já escritos. Não gerar automaticamente.
5. Map Remaster2.0 continua reservado para depois de19/11 às21:11 Brasília e da fase autorizada; nenhum mapa físico foi alterado.

MP3 privado, saves, fontes rejeitadas e alterações concorrentes em configuração/caches permanecem protegidos. Loading aprovado/publicado e evidências históricas não foram refeitos. Este checkpoint é parcial: não declara conclusão global, validação do jogo ou publicação desta entrega.
