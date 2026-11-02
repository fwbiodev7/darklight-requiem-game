# Gothic Eclipse — plano de implementação e pendências

Referência principal: `ArtSources/AtmosphereReference20261101/gothic-eclipse-approved-reference.png` (cópia local privada da imagem enviada). Pedido integral preservado em `GOTHIC-ECLIPSE-PEDIDO-20261101.md`.

## Estado e prioridade

A atmosfera atual é uma base funcional, não uma reprodução concluída da referência. A build em preparação recebe somente o acabamento suave: variantes de névoa/luar escolhidas por mapa em vez de seis imagens sobrepostas, névoa baixa, luar lateral, filtro menos intenso e cores originais preservadas. Chão molhado, reflexos, chuva multicamadas e luz com profundidade continuam PENDENTES.

Concluir a build e abrir para feedback. O detalhamento mais recente de Fabius permanece: ataques/caminhadas → transformações → defesas/restante. O novo prompt coloca Fabius → Lucius → Combate2.0 → overhaul completo; essa ordem substitui a sequência histórica do documento mestre para o trabalho posterior. Não deixar o overhaul atrasar os personagens ou reiniciar arte aprovada. Lucius/Combate2.0 não são declarados concluídos por terem conteúdo legado.

## Base técnica real

O projeto já referencia um pipeline configurado; existem Renderer2D, material SpriteLit, Volume e DarklightRemaster, que já gerencia luzes, sombras e pós-processamento. Reutilizar e inspecionar essas instalações antes de alterar configurações. DarklightAtmosphereController deve coordenar a atmosfera e conversar com o remaster, sem duplicar luzes ou empilhar filtros. Não trocar pipeline ou editar mapas por conveniência.

Os cenários incluem arte de iluminação já pintada. Para separar chão, paredes, água e coberturas, serão necessárias máscaras de renderização alinhadas ao cenário original. Isso preserva os PNGs/tilesets; a máscara é um recurso novo. Sem essas máscaras, reflexos e névoa intermediária vazariam sobre paredes. Não prometer equivalência física com uma imagem ilustrada.

## Execução em etapas

1. **Preset e primeiro mapa.** Criar um preset reutilizável (preferencialmente ScriptableObject) com paleta, exposição, chuva, vento, névoa, luar, luzes quentes e reflexos; substituir números dispersos. Escolher a Rua das Cinzas para comparação fixa antes/depois. Registrar fotos, tempos de frame e memória da base no mesmo local/câmera/resolução. Manter uma opção de desativar e restaurar o original.
2. **Luz e cor.** Ajustar o Volume/remaster existente: sombras azul-marinho sem perder detalhe, realces frios e lanternas âmbar. Usar fontes reais de luz de postes/lanternas e máscaras de oclusão, sem inserir lanternas imaginárias. Feixes lunares suaves respeitam coberturas e não atravessam indiscriminadamente a tela. Preservar roupa, pele e olhos de Fabius; evitar uma coloração global que mude a skin. Redução de flashes continua respeitada.
3. **Chuva com profundidade.** Duas camadas finas, atrás e à frente dos atores, com velocidades e alpha diferentes, vento e variação lenta. Pool para impactos curtos no chão e ondulações somente nas máscaras de água/poças. Zero chuva interior por padrão. Reaproveitar sistemas existentes; limites propostos por qualidade:128/256/384 gotas,8/16/24 respingos/ondulações ativos. Esses números são teto inicial a medir, não desempenho comprovado.
4. **Névoa em três profundidades.** Rasteira recortada pelo chão, intermediária entre estruturas via máscaras/ordenação e fundo muito sutil. Pixel art nítida e movimento lento. Intensidade total controlada; não sobrepor as três texturas de cada tipo em tela inteira. Sem véu branco uniforme e com a área de combate legível.
5. **Superfícies molhadas.** Máscara por mapa distingue pedra exposta, água, interiores e materiais secos. Shader/overlay escurece e dá brilho apenas ao chão adequado. Primeiro refletir lanternas; depois reflexos suaves dos sprites completos dos atores e ataques, ancorados nos pés, recortados pela máscara e perturbados por ondas. Renderização reduzida/opcional conforme qualidade; não transformar o mapa em espelho. Originais não recebem pintura permanente.
6. **Combate.** Ligar pequenos pulsos temporários às partículas/eventos já existentes. Katana prateada; Ruptura vermelha; Eclipse violeta/prata. Limitar duração e número de luzes, sem tingir a skin ou esconder antecipação/impacto. Integrar após as formas e animações próprias estarem prontas.
7. **Cena demonstrativa.** Identificar a personagem castanha real na captura antes de mexer. Somente na demonstração correspondente, colocá-la à esquerda de Fabius com pés alinhados, sprite inteiro/escala intactos e ordem correta; remover a duplicata anterior. Não inferir que ela seja Lucius nem mudar NPCs/posições narrativas nos dez mapas.
8. **Expandir após validar.** Configurar os outros nove mapas individualmente. Mapas fechados recebem bruma/luz apropriadas; não impor chuva a todos. Transições graduais, presets reversíveis e desativação dos efeitos caros. Não alterar colisões, navegação, objetivos, arquitetura, áudio ou combate.

## Critérios para concluir

- Capturas reais antes/depois e comparação com a referência em iluminação, chuva, profundidade, chão molhado e legibilidade; imagem gerada não serve como prova de gameplay.
- Mesmos sprites canônicos, escala1.325 e proporções; leitura clara de inimigos/ataques/HUD nas quatro qualidades.
- Referências e GUIDs íntegros; geometria, colisões, hitboxes, objetivos e saves preservados.
- Medir tempo de frame mediano/p95, memória e quantidade de efeitos no executável, com mesma câmera e carga. Alvo inicial60fps a1280x720 na máquina de teste; registrar hardware e reduzir efeitos se não houver margem. Sem números de performance inventados.
- QA do primeiro mapa, depois amostra real dos dez e regressões afetadas. Build Windows, commit/push pela cópia isolada e recibos. Não aprovar apenas pela existência dos componentes.

## Pendências rastreáveis

PENDENTE: presets editáveis; revisão integrada do remaster; máscaras de chão/coberturas; luz âmbar localizada; chuva multicamadas; respingos/ondas; névoa em três planos; superfícies molhadas; reflexos de luz/atores/ataques; luz de combate; cena demonstrativa condicional; medição de performance e revisão dos dez mapas. A implementação completa não cabe na margem reservada à build/Git desta sessão e segue registrada sem declarar o resultado da referência entregue.
