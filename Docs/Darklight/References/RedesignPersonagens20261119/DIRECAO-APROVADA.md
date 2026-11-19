# DARKLIGHT: REQUIEM — REDESIGN OFICIAL DE PERSONAGENS 2.0

## Objetivo

Atualizar os designs de quatro personagens de DARKLIGHT: REQUIEM utilizando as quatro imagens anexadas como **referências visuais principais e aprovadas**:

1. Lyra — aliada de Fabius desde o início.
2. Matriarca Rubra — chefe do início da campanha.
3. Kael Voss — inicialmente inimigo, posteriormente aliado.
4. Nocthar — chefe importante da campanha.

**ATENÇÃO: Esta tarefa somente poderá começar depois que Lucius 2.0 estiver completamente finalizado e validado.**

Não interromper nem antecipar o encerramento de Lucius para iniciar os redesigns.

## 1. Identidade artística obrigatória

Utilizar como referência técnica o estilo pixel art já estabelecido para Lucius Normal e Arconte.

Os personagens devem possuir:

- Pixel art 2D autêntica, com pixels definidos.
- Proporções compatíveis com a escala oficial do jogo.
- Contornos nítidos e iluminação controlada.
- Sombras detalhadas sem excesso de ruído visual.
- Visual gótico cinematográfico e dark fantasy.
- Animações fluidas, sem deformação corporal.
- Silhuetas reconhecíveis mesmo em resolução reduzida.
- Coerência entre vistas frontais, traseiras, laterais e diagonais.

Não simplesmente importar as imagens de referência como sprites. Elas são concept sheets e precisam ser adaptadas e validadas para utilização real no Unity.

Preservar os pivôs, o PPU, os alinhamentos e o sistema atual de animações sempre que possível.

## 2. LYRA — PRIORIDADE MÁXIMA DO REDESIGN

Lyra é uma aliada de Fabius desde o início da campanha.

Seu novo design deverá seguir fielmente a imagem aprovada:

- Cabelo castanho-avermelhado volumoso.
- Rosto bonito, expressivo e confiante.
- Jaqueta gótica preta com detalhes vermelhos.
- Decote destacado conforme a referência.
- Proporções femininas equilibradas.
- Calças escuras, botas e acessórios.
- Livro característico preservado.
- Visual inspirado livremente no estilo de personagens como Lady de Devil May Cry, sem copiar sua aparência.

**REGRA OBRIGATÓRIA DE PROPORÇÃO:**

Lyra deverá possuir a mesma altura visual e escala de Fabius Normal em gameplay, utilizando os sprites aprovados de Fabius como referência.

Não transformar Lyra em uma personagem desproporcionalmente alta, pequena ou com estilo artístico incompatível.

Manter sua identidade narrativa como aliada desde o começo. Não alterar diálogos, comportamento ou participação na campanha sem necessidade técnica comprovada.

Priorizar:

1. Sprite base com proporção correta.
2. Vistas necessárias para movimentação.
3. Idle e caminhada.
4. Animações existentes que precisem de adaptação.
5. Testes de escala e integração na campanha.

A arma de fogo mostrada na referência é um elemento conceitual. Não adicionar novas mecânicas de disparo sem autorização.

## 3. MATRIARCA RUBRA — REDESIGN DE CHEFE

Reproduzir o design aprovado na imagem:

- Cabelos longos e prateados.
- Vestido gótico vermelho e preto.
- Silhueta feminina imponente.
- Capa ornamentada.
- Asas semelhantes às de um morcego.
- Halo vermelho de espinhos com rosa e crânio.
- Rosas carmesins e detalhes de caveiras.
- Espada vermelha de aparência sobrenatural.

Preservar a identidade de chefe vampírica e aristocrática.

A Matriarca deverá parecer ameaçadora, elegante e visualmente distinta dos inimigos comuns.

Adaptar os elementos decorativos à resolução real do jogo, evitando detalhes minúsculos que prejudiquem a leitura.

Preservar padrões de ataque, colisões e lógica atual da bossfight.

## 4. KAEL VOSS — INIMIGO QUE SE TORNA ALIADO

Utilizar a imagem aprovada como referência principal:

- Cabelos prateados.
- Olhos azuis.
- Roupa preta com detalhes azul-escuros.
- Armadura leve e acessórios metálicos.
- Capa longa com elementos azuis.
- Espada brilhante azul.
- Aparência de espadachim habilidoso e misterioso.

Kael precisa funcionar visualmente tanto como adversário quanto como aliado.

Preservar sua identidade nas diferentes etapas narrativas.

Não confundir Kael com Lucius, apesar de ambos possuírem cabelos prateados. Suas roupas, silhuetas, armas e cores devem diferenciá-los claramente.

Preservar a história de sua rendição e posterior purificação, bem como o estado de Kael Fraturado.

Não alterar a sequência narrativa existente.

## 5. NOCTHAR — REDESIGN DO CHEFE

Recriar Nocthar utilizando a referência aprovada:

- Armadura negra pesada.
- Espinhos e ornamentos ameaçadores.
- Capacete com aparência sobrenatural.
- Olhos luminosos violetas.
- Cristal roxo no peito.
- Capa escura e espectral.
- Espada de energia violeta.
- Aura sombria com elementos roxos.

O resultado deverá transmitir a presença de um chefe poderoso.

Preservar os sistemas de combate existentes, ataques de área e efeitos já implementados.

A forma final de Nocthar possui recursos próprios e não deve ser substituída automaticamente pelo redesign de sua forma base.

Avaliar separadamente a compatibilidade visual da forma final.

## 6. PRODUÇÃO ECONÔMICA DE SPRITES

A produção deve ocorrer de forma incremental, evitando desperdício de tokens.

Procedimento:

1. Auditar os sprites ativos dos quatro personagens.
2. Identificar quais animações existentes podem ser reaproveitadas.
3. Criar um pequeno piloto de Lyra, comparando diretamente sua escala à de Fabius.
4. Validar a qualidade do piloto antes de expandir.
5. Adaptar as animações necessárias, usando como referência o padrão de oito direções do jogo.
6. Somente depois continuar Matriarca, Kael e Nocthar, um de cada vez.

Não gerar todas as animações de todos os personagens simultaneamente.

Priorizar mudanças de maior impacto visual com menor custo.

As referências anexadas não devem ser tratadas como folhas já prontas para importação. Conferir consistência temporal, recortes, transparência, proporções e alinhamento de cada quadro.

## 7. Integração e validação

Após cada piloto:

- Importar com segurança no Unity.
- Conferir PPU e pivôs.
- Verificar escala em relação a Fabius e Lucius.
- Validar animações e transições.
- Conferir alinhamento com colisores e efeitos.
- Verificar ausência de erros no Console.
- Executar apenas testes direcionados necessários.
- Preservar sprites anteriores para reversão.

Não modificar personagens não relacionados.

Não alterar saves de produção, mapas, áudio ou sistemas de gameplay fora do escopo.

Não gerar build, publicar ou realizar push sem autorização explícita.

## 8. Ordem oficial de desenvolvimento

Respeitar a seguinte sequência:

1. Finalizar e validar completamente Lucius 2.0.
2. Redesign de Lyra, Matriarca Rubra, Kael Voss e Nocthar.
3. Concluir pendências relevantes dos demais inimigos e chefes.
4. Evolução gráfica e polimento Gothic Eclipse.
5. Cutscenes cinematográficas.
6. Demais etapas conforme os checkpoints oficiais.

Se as etapas anteriores já estiverem concluídas, não reabrir trabalho aprovado sem necessidade.

## 9. Proteção das cotas

Preservar pelo menos 20% da cota semanal do Codex.

Evitar regenerações repetitivas e produção desnecessária de sprites.

Não ultrapassar as reservas para finalizar um personagem.

Caso falte orçamento, encerrar a produção no último ponto estável e registrar o próximo lote necessário.

## Entrega

Ao terminar cada personagem, informar:

- Sprites criados e reaproveitados.
- Animações concluídas e pendentes.
- Comparação de proporções com os personagens existentes.
- Testes executados e seus resultados reais.
- Arquivos alterados.
- Problemas identificados.
- Cota restante, somente quando verificável.

**OBJETIVO FINAL:** modernizar os quatro personagens mantendo fidelidade às referências aprovadas, identidade visual coerente com Lucius 2.0, integração segura com Unity e consumo controlado de tokens.

Não considerar o redesign finalizado sem validação visual e aprovação do usuário.