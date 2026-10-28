# DARKLIGHT REQUIEM — REDESIGN COMPLETO DAS ARMAS E EQUIPAMENTOS

## OBJETIVO PRINCIPAL

Realizar um redesign visual completo das armas de Darklight Requiem, utilizando **a imagem anexada como referência artística principal e obrigatória**.

O objetivo é modernizar e aprimorar as armas existentes, tornando-as mais detalhadas, agressivas, proporcionais e visualmente impressionantes, sem abandonar a identidade pixel art original do jogo.

**Execute todo o processo autonomamente:** analise o repositório, localize os sprites atuais, crie as novas versões, adapte as animações, integre os assets no Unity e valide o funcionamento.

Não entregue apenas conceitos ou imagens ilustrativas. Quero as novas armas implementadas de verdade no jogo.

---

## 1. RÉQUIEM — KATANA

Redesenhar a katana seguindo o modelo apresentado na imagem.

### Características visuais

- Lâmina longa, elegante e levemente curva.
- Metal escuro com fio vermelho brilhante.
- Rachaduras e veios de energia carmesim percorrendo a lâmina.
- Guarda preta com detalhes metálicos.
- Cabo escuro com acabamento vermelho.
- Silhueta reconhecível mesmo em sprites pequenos.
- Aparência sobrenatural, agressiva e refinada.

A katana deve transmitir a sensação de uma arma demoníaca extremamente poderosa, sem parecer desproporcional ao personagem.

### Variações por transformação

**Réquiem:** vermelho carmesim intenso, metal negro e energia sanguínea.

**Ruptura:** manter o desenho-base da katana, adaptando a energia e os detalhes à identidade visual já aprovada dessa transformação.

**Eclipse:** energia violeta, reflexos roxos e metal escurecido.

**Absoluto:** lâmina clara/prateada, rachaduras vermelhas e energia intensa, conforme a referência.

Cada transformação deve possuir sua identidade própria sem transformar a katana em uma arma completamente diferente.

---

## 2. PRESAS DO CAOS — ADAGAS DUPLAS

Redesenhar as duas adagas utilizando o modelo apresentado na imagem.

### Características visuais

- Duas adagas curtas, curvas e extremamente afiadas.
- Lâminas serrilhadas e assimétricas nos detalhes, mas equilibradas como par.
- Metal prateado escurecido.
- Rachaduras de energia vermelha ou violeta.
- Cabos negros com detalhes ornamentais.
- Pontas agressivas e silhuetas bem definidas.
- Aparência de armas rápidas, letais e sobrenaturais.

As adagas devem transmitir velocidade, brutalidade e precisão.

### Variações por transformação

- **Réquiem:** metal escuro com energia vermelha.
- **Ruptura:** adaptar os detalhes à paleta e aos efeitos aprovados da forma.
- **Eclipse:** lâminas com energia roxa e reflexos violetas.
- **Absoluto:** metal claro com detalhes carmesins.

Manter consistência entre as duas adagas e preservar sua identidade durante ataques rápidos.

---

## 3. PUNHOS DA RUÍNA — MANOPLAS E GREVAS ABISSAIS

Redesenhar o equipamento de combate corpo a corpo com base nas manoplas e grevas apresentadas na imagem.

**Este equipamento substitui visualmente a antiga corrente e foice. Não reintroduzir a foice ou a corrente como arma principal.**

### Manoplas Abissais

- Armaduras envolvendo mãos e antebraços.
- Metal negro, vermelho e prateado.
- Garras e placas pontiagudas.
- Cristais ou fissuras energéticas.
- Estrutura agressiva, mas proporcional às mãos.
- Articulações que permitam animações naturais de socos.

### Grevas Abissais

- Proteções metálicas envolvendo pernas e pés.
- Garras nos pés e pontas ornamentais.
- Placas sobrepostas com detalhes energéticos.
- Design compatível com chutes, saltos e golpes descendentes.
- Volume adequado para não prejudicar a leitura das animações.

### Transformações

Cada forma deverá modificar a energia, os detalhes e as cores do equipamento.

- **Réquiem:** vermelho e negro.
- **Ruptura:** identidade cromática original aprovada.
- **Eclipse:** violeta e negro.
- **Absoluto:** prata, branco e vermelho.

As manoplas e grevas devem parecer um único conjunto de armadura.

---

## 4. ADAPTAÇÃO AO PERSONAGEM

Utilize a aparência do personagem apresentada na imagem como referência visual, mas compare-a com os sprites originais e aprovados do repositório.

Preserve:

- Rosto e olhos.
- Cabelos e suas cores por transformação.
- Tom de pele original.
- Roupas e acessórios.
- Proporções anatômicas.
- Silhueta e escala do personagem.
- Identidade visual de cada transformação.

Não redesenhe o personagem inteiro desnecessariamente.

As armas devem se adaptar às mãos, aos braços e às pernas do personagem, nunca o contrário.

Evite armas flutuantes, empunhaduras incorretas, mudanças bruscas de tamanho ou equipamentos atravessando o corpo.

---

## 5. PIXEL ART E ANIMAÇÕES

Todas as novas armas devem respeitar rigorosamente o estilo pixel art de Darklight Requiem.

### Requisitos obrigatórios

- Pixels bem definidos.
- Ausência de borrões e suavização excessiva.
- Sombras e iluminação coerentes.
- Contornos consistentes.
- Proporções uniformes.
- Detalhes legíveis na resolução real do jogo.
- Sprites completos, sem membros cortados.
- Coerência entre quadros de animação.

Adaptar os novos designs às animações existentes de:

- Repouso.
- Caminhada e corrida.
- Ataques leves e pesados.
- Combos.
- Ataques aéreos.
- Esquivas e deslocamentos.
- Transformações.
- Habilidades especiais.
- Direções suportadas pelo sistema.

**Não basta alterar o sprite da arma no inventário. O novo design precisa aparecer corretamente nas mãos e nos pés do personagem durante o gameplay.**

Se a skill `$pixel-art-sprite-sheet-master` estiver disponível, utilize-a para orientar a criação, revisão e validação das sprite sheets.

---

## 6. IMPLEMENTAÇÃO DIRETA NO UNITY

1. Inspecione os sprites e bancos de animação existentes.
2. Identifique quais assets precisam ser atualizados.
3. Utilize a imagem anexada como referência de design.
4. Crie as versões finais das três armas.
5. Produza as variações necessárias para as transformações.
6. Integre as armas aos sprites completos do personagem.
7. Atualize ícones, inventário e interfaces onde necessário.
8. Preserve pivôs, escalas, colisões e sincronização dos ataques.
9. Verifique compatibilidade com o combate aéreo e os combos existentes.
10. Execute testes de renderização e gameplay.
11. Corrija automaticamente problemas visuais e técnicos encontrados.

Não substitua arte aprovada por placeholders, partes desconectadas ou recolorações genéricas.

Preserve as fontes originais e não sobrescreva alterações preexistentes não relacionadas.

---

## 7. PRIORIDADE DE QUALIDADE

O objetivo não é simplesmente adicionar detalhes às armas, mas conseguir um redesign que combine:

**Estética de Devil May Cry + fantasia sombria + identidade original de Darklight Requiem + pixel art consistente.**

As armas precisam parecer parte do mesmo universo e manter coerência com os personagens.

Priorize qualidade sobre quantidade de detalhes.

Se algum elemento da imagem não funcionar na escala real do jogo, simplifique-o cuidadosamente sem perder a silhueta e a identidade.

---

## 8. EXECUÇÃO AUTÔNOMA E VALIDAÇÃO

Faça todo o trabalho sem solicitar confirmação para decisões técnicas rotineiras.

Não pare após analisar ou criar um planejamento. Prossiga diretamente para a produção dos assets, integração e testes.

Valide as armas nas skins, transformações, direções e animações aplicáveis.

Não declare uma arma concluída apenas porque seu desenho está pronto. Ela só estará concluída quando estiver integrada, visualmente coerente e funcional no jogo.

Caso existam limitações de tempo, contexto ou ferramentas, registre as pendências reais e preserve o progresso para retomada.

Ao terminar, apresente um relatório curto com as armas atualizadas, transformações cobertas, animações integradas, testes realizados e pendências restantes.

## ORDEM FINAL

**Implemente um redesign completo da katana Réquiem, das adagas Presas do Caos e dos Punhos da Ruína, incluindo Manoplas e Grevas Abissais, usando a imagem fornecida como referência principal.**

Não altere desnecessariamente os sistemas de combate existentes, não comprometa as animações aprovadas e não descarte o trabalho anterior.

**Quero o redesign aplicado no jogo, não apenas uma imagem conceitual. Comece imediatamente.**