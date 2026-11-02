# DARKLIGHT: REQUIEM — GOTHIC ECLIPSE ATMOSPHERIC OVERHAUL

## MISSÃO

Implementar em DARKLIGHT: REQUIEM a atmosfera Dark Fantasy cinematográfica da imagem de referência anexada, com fidelidade visual máxima, preservando os mapas, personagens, animações e sistemas existentes.

**A imagem final aprovada, com os dois personagens lado a lado no chão, é a referência visual principal.**

O objetivo NÃO é redesenhar os mapas. É reproduzir o resultado utilizando iluminação dinâmica, shaders, partículas, efeitos climáticos, reflexos e pós-processamento.

A atmosfera atual está visualmente insuficiente: a névoa é simples, a chuva pouco expressiva, a iluminação não possui profundidade suficiente e o ambiente não transmite completamente a sensação Dark Fantasy desejada.

Corrigir esses problemas sem aumentar desnecessariamente o escopo.

## 1. PRESERVAÇÃO OBRIGATÓRIA

Não modificar:

- Geometria dos dez mapas.
- Tilesets e estruturas existentes.
- Sprites canônicos de Fabius e Lucius.
- Proporções dos personagens.
- Colisões e hitboxes.
- Navegação e movimentação.
- Combate e animações aprovadas.
- Posicionamento dos elementos arquitetônicos.

Não gerar mapas novos, substituir cenários ou reconstruir fases.

A transformação visual deve acontecer principalmente na renderização.

## 2. CHUVA CINEMATOGRÁFICA

Substituir a aparência atual da chuva por um sistema multicamadas.

Implementar:

- Chuva diagonal fina e numerosa.
- Gotas em velocidades diferentes.
- Camadas de chuva atrás e à frente dos personagens.
- Pequenos respingos nas superfícies.
- Ondulações circulares nas poças.
- Variações naturais de intensidade.
- Direção influenciada pelo vento.

Evitar gotas exageradamente grossas, repetição perceptível e partículas que prejudiquem a leitura dos personagens.

A chuva deve transmitir profundidade e movimento constante.

## 3. NÉVOA DARK FANTASY

Implementar névoa em três níveis:

**Névoa rasteira:** próxima ao chão, deslocando-se lentamente.

**Névoa intermediária:** entre objetos e estruturas, proporcionando profundidade.

**Névoa atmosférica:** suave, próxima ao fundo, simulando umidade e distância.

Utilizar sprites animados ou shaders compatíveis com a renderização existente.

A névoa não deve parecer uma camada branca uniforme cobrindo a tela.

## 4. ILUMINAÇÃO GÓTICA CINEMATOGRÁFICA

Criar uma iluminação de alto contraste, inspirada na imagem aprovada.

### Iluminação fria

- Tons azulados e violetas.
- Sombras profundas.
- Feixes suaves de luz lunar.
- Redução controlada da luminosidade geral.

### Iluminação quente

- Postes e lanternas emitindo luz âmbar.
- Iluminação gradual nas superfícies próximas.
- Brilho suave nas bordas dos objetos.
- Reflexos quentes no chão molhado.

### Iluminação de combate

- Katana produzindo flashes durante ataques.
- Ruptura emitindo luz vermelha.
- Eclipse Absoluto emitindo luz violeta e prateada.
- Golpes poderosos iluminando brevemente a chuva.

Avaliar o pipeline atual antes de introduzir URP 2D ou modificar shaders existentes.

## 5. CHÃO MOLHADO E REFLEXOS

Esta é uma das melhorias visuais mais importantes.

A referência possui um chão escuro, úmido e reflexivo.

Implementar:

- Brilho sutil nas superfícies molhadas.
- Reflexos das lanternas.
- Reflexos suaves dos personagens quando apropriado.
- Ondulações produzidas pela chuva.
- Reflexos temporários dos ataques.
- Pequenas poças em regiões adequadas.

Utilizar máscaras, shaders e overlays para evitar alterações permanentes nos tilesets.

Não transformar todos os materiais em espelhos.

Reflexos devem respeitar o tipo de superfície e a perspectiva do mapa.

## 6. COLOR GRADING — GOTHIC ECLIPSE

Criar um preset visual inspirado na referência:

- Sombras azul-marinho.
- Tons frios e dessaturados.
- Pretos profundos sem esmagar detalhes.
- Luzes âmbar.
- Vermelhos preservados nos detalhes dos personagens.
- Realces prateados e violetas.

Evitar saturação excessiva, excesso de bloom e aparência artificial.

O resultado precisa continuar sendo pixel art nítida.

## 7. POSICIONAMENTO DA PERSONAGEM SECUNDÁRIA

Na cena demonstrativa da referência, a personagem de cabelo castanho que anteriormente aparecia sobre a estrutura à esquerda deve ficar no chão, ao lado de Fabius.

Requisitos:

- Manter o sprite original.
- Preservar escala e proporções.
- Posicioná-la à esquerda de Fabius.
- Alinhar os pés ao mesmo plano de chão.
- Manter distância natural entre os personagens.
- Corrigir a ordem de renderização.
- Não deixar duplicatas na posição anterior.

**Importante:** esta alteração deve ocorrer somente na cena demonstrativa correspondente, caso essa personagem e essa posição existam realmente no projeto. Não alterar outros NPCs ou cenas por suposição.

## 8. ARQUITETURA TÉCNICA

Criar ou adaptar um sistema centralizado:

`DarklightAtmosphereController`

Responsabilidades:

- Presets climáticos.
- Intensidade da chuva.
- Densidade da névoa.
- Iluminação ambiente.
- Frequência dos relâmpagos.
- Intensidade dos reflexos.
- Qualidade gráfica.
- Transições atmosféricas.

Criar presets reutilizáveis por mapa, preferencialmente com ScriptableObjects.

Evitar duplicação de scripts, materiais e sistemas de partículas.

Usar pooling, batching e otimizações adequadas.

## 9. INTEGRAÇÃO NOS DEZ MAPAS

Primeiro implementar e validar a atmosfera em um mapa existente.

Após aprovação técnica:

- Aplicar o sistema aos demais mapas.
- Configurar parâmetros individuais.
- Preservar as características de cada região.
- Evitar chuva obrigatória em todos os ambientes.
- Não modificar o layout dos mapas.

A atmosfera deve ser configurável e reversível.

## 10. PRIORIDADES E LIMITES

A ordem principal do desenvolvimento continua sendo:

1. Conclusão e revisão das animações de Fabius.
2. Criação e integração de Lucius.
3. Implementação do Combate 2.0.
4. Atmospheric Overhaul Gothic Eclipse.
5. QA, otimização e build jogável.

**Não permitir que a atmosfera atrase Lucius ou o Combate 2.0.**

Se houver recursos limitados, concluir primeiro uma versão funcional da atmosfera, documentando o polimento restante.

Não reiniciar trabalhos artísticos já aprovados.

## 11. VALIDAÇÃO VISUAL OBRIGATÓRIA

Capturar screenshots reais da Unity antes e depois das alterações.

Comparar com a imagem de referência nos seguintes critérios:

- Profundidade da iluminação.
- Qualidade da chuva.
- Densidade e distribuição da névoa.
- Reflexos no chão.
- Contraste das cores.
- Legibilidade dos personagens.
- Fidelidade à pixel art.
- Desempenho.

Não considerar a implementação concluída apenas porque os componentes foram adicionados.

O resultado deve ser perceptivelmente superior à atmosfera atual.

Se algum efeito da imagem não puder ser reproduzido sem modificar o cenário, implementar a melhor aproximação não destrutiva e documentar a limitação.

## 12. ENTREGA FINAL

Entregar:

- Sistema atmosférico integrado.
- Presets configuráveis.
- Screenshots reais de comparação.
- Relatório de desempenho.
- Testes de regressão.
- Build jogável para Windows, se possível.
- Commits e push seguros.
- Checkpoint atualizado.

Preservar o progresso e registrar pendências antes do esgotamento dos tokens.

## RESULTADO ESPERADO

Quero que DARKLIGHT: REQUIEM tenha uma atmosfera Dark Fantasy cinematográfica comparável à referência aprovada:

Chuva intensa, névoa volumosa, iluminação azulada, lanternas quentes, chão molhado, reflexos, sombras profundas e uma sensação de mundo sombrio e sobrenatural.

**Não quero mapas refeitos. Quero os mapas existentes visualmente transformados pela atmosfera.**

Priorizar fidelidade, desempenho, estabilidade e integração real com a gameplay.