# DARKLIGHT: REQUIEM — GOTHIC ECLIPSE
## Escopo gráfico e atmosférico enxuto — 02/11/2026

**PRIORIDADE:** executar somente após a conclusão e validação de Fabius Réquiem Normal.

**OBJETIVO:** transformar a atmosfera visual dos mapas existentes de DARKLIGHT: REQUIEM em uma experiência dark fantasy de alta qualidade, próxima da imagem de referência "Duelo Sob a Chuva Sombria", preservando integralmente a estrutura dos cenários, sprites, colisores e gameplay.

**DIRETRIZ ABSOLUTA: NÃO RECONSTRUIR OS DEZ MAPAS.**

A implementação deve aproveitar o pacote atmosférico preparado em 01/11, adicionando somente os efeitos necessários para atingir a referência.

Não transformar esta etapa em outro redesign extenso.

---

## 1. Recuperar o trabalho existente

Localizar e revisar os arquivos preparados pelo agente de atmosfera em 01/11:

- `DarklightAtmosphereController.cs`
- `AtmosphereOverlay.shader`
- `AtmosphereTextures.png`
- `AtmosphereTextures.json`

Consultar:

`Docs/Darklight/GOTHIC-ECLIPSE-PLANO-20261101.md`

Verificar os recibos e fontes da produção anterior.

**IMPORTANTE:** o pacote anterior foi produzido e revisado em código, mas não foi importado, compilado, executado ou aprovado visualmente.

Não declarar sua implementação concluída antes de executar os testes.

Preservar integralmente as fontes aprovadas.

## 2. Integrar a infraestrutura atmosférica

Copiar o controlador para:

`Assets/Darklight/Runtime/Campaign/DarklightAtmosphereController.cs`

Copiar o shader para:

`Assets/Darklight/Resources/Darklight/Rendering/AtmosphereOverlay.shader`

Copiar as texturas e o JSON para:

`Assets/Darklight/Resources/Darklight/AtmosphereTextures20261101/`

Configurar as texturas conforme o contrato existente: Default, alpha preservado, sRGB, Clamp, Bilinear, sem mipmaps e sem compressão com perdas.

A filtragem Bilinear deve ser aplicada somente aos overlays atmosféricos suaves. Preservar Point Filter nos sprites de personagens, mapas e elementos de pixel art que dependam de pixels nítidos.

Integrar em:

`DarklightRegionAtmosphere.cs`

No método:

`DarklightCampaign.InitializeRegionAtmosphere(int chapter)`

Imediatamente após `regionAtmosphere=null;` e antes do retorno antecipado, inserir:

`DarklightAtmosphereController.Attach(this,levelRoot,lab.WorldCamera,chapter);`

Verificar que a assinatura e o ciclo de vida continuam compatíveis com o código atual.

Não substituir o sistema de iluminação existente.

## 3. Aparência visual desejada

### A. Color grading dark fantasy

Aplicar aos pisos e fundos elegíveis:

- Sombras azul-marinho e azul-esverdeadas.
- Realces frios prateados.
- Dessaturação moderada.
- Contraste cinematográfico controlado.
- Preservação dos detalhes originais.

Não recolorir personagens, NPCs, inimigos, HUD ou elementos interativos.

Usar o material de filtro de chão reversível já preparado.

Não modificar permanentemente texturas ou materiais originais.

### B. Iluminação lunar

Aproveitar os três motivos `moon_0..2` existentes.

Criar feixes suaves de luz azul-prateada, com direção consistente e distribuição localizada.

A luz deve atravessar visualmente o ambiente sem criar uma camada branca opaca.

Evitar feixes exagerados, iluminação uniforme ou efeitos que dificultem enxergar os personagens.

Não adicionar sombras dinâmicas pesadas.

### C. Névoa em camadas

Aproveitar `fog_0..2`.

Manter:

- Névoa baixa.
- Profundidade entre camadas.
- Transparência suave.
- Movimento discreto, se compatível com as configurações de acessibilidade.

Não cobrir rostos, HUD, ataques ou objetos importantes.

### D. Chuva cinematográfica

Utilizar o pool existente de partículas.

Priorizar:

- Gotas finas e diagonais.
- Distribuição natural.
- Densidade moderada.
- Velocidade visual coerente.
- Variações discretas de comprimento e transparência.

A chuva não deve comprometer a leitura das animações de combate.

Não criar milhares de GameObjects individuais.

Respeitar o limite atual de 88 renderizadores de chuva/partículas.

### E. Piso molhado — módulo opcional

Este é um dos principais diferenciais da referência.

Adicionar uma camada leve de aparência úmida sobre pisos elegíveis, sem modificar os tiles originais.

Efeitos desejados:

- Brilhos azulados discretos.
- Reflexos âmbar próximos de fontes quentes.
- Pequenos círculos de gotas.
- Manchas úmidas com transparência variável.
- Destaques de luz em áreas selecionadas.

Não implementar reflexos completos em tempo real, espelhamento integral do personagem ou renderização duplicada de toda a cena.

Preferir shader simples ou sprites de destaque, com orçamento limitado.

**Regra:** se o piso não for adequado, desativar o efeito.

### F. Iluminação quente localizada — módulo opcional

A referência possui contraste entre luz lunar fria e iluminação âmbar.

Reaproveitar tochas, janelas, fogueiras e lanternas que já existirem nos mapas.

Adicionar apenas halos suaves ou brilhos locais.

Não criar novas construções, postes ou lanternas automaticamente.

Não modificar posições de objetos.

Não adicionar iluminação dinâmica custosa quando um overlay simples for suficiente.

Manter os efeitos de brasas e iluminação já existentes, evitando duplicação visual.

---

## 4. Presets por região

Os dez presets existentes devem ser preservados e ajustados individualmente.

Não aplicar chuva e névoa idênticas a todos os mapas.

Direção geral:

**Florestas:** névoa azulada, luar filtrado, sombras frias e chuva onde fizer sentido.

**Ruínas:** pedras escuras, tons verde-acinzentados, luz fria e brilhos discretos.

**Áreas externas construídas:** contraste entre luar e iluminação quente existente, com pisos molhados quando apropriado.

**Áreas internas:** atmosfera mais contida, sem chuva atravessando tetos ou paredes.

**Áreas de combate:** preservar visibilidade de hitboxes, personagens e efeitos ofensivos.

Utilizar os presets já criados em vez de reconstruir o sistema.

## 5. Integração gradual

### Gate 1 — Primeira região

Manter inicialmente:

`AllRegionsEnabled == false`

Ativar somente a região 0.

Realizar capturas antes/depois com câmera e personagem na mesma posição.

Verificar:

- Identidade do mapa preservada.
- Fabius com rosto, cabelo, pele e olhos inalterados.
- HUD legível.
- Nenhuma geometria alterada.
- Iluminação e névoa equilibradas.
- Piso molhado coerente, caso habilitado.
- Ausência de efeitos duplicados.

Desligar o controlador e confirmar restauração exata das cores originais do chão.

### Gate 2 — Funcionalidade e limites

Confirmar:

- `ApplyRegion(1)` não ativa efeitos quando todas as regiões estão bloqueadas.
- `ApplyRegion(0)` respeita o limite de partículas.
- Qualidade 0 reduz corretamente os efeitos.
- Intensidade 0 restaura a aparência original.
- Pausas e menus ocultam os efeitos conforme o contrato existente.
- Destruição e reinicialização não deixam renderizadores órfãos.
- Troca de mapa restaura o cenário anterior.

Não utilizar `ApplyRegion` como mecanismo de troca real de mapas.

### Gate 3 — Dez regiões

Após a aprovação visual da primeira região, habilitar as demais.

Usar:

`Current.SetAllRegionsEnabled(true)`

Validar a alternância dos presets 0–9 e a restauração das cores originais.

Não executar uma suíte gigantesca desnecessária; utilizar testes direcionados.

## 6. Performance e segurança

Preservar o sistema de pool e os limites de alocação existentes.

Não criar objetos ou materiais a cada frame.

Evitar:

- Sombras dinâmicas complexas.
- Reflexos reais de toda a cena.
- Pós-processamento excessivo.
- Ruído visual agressivo.
- Flashes e flicker.
- Alterações de câmera.
- Modificações de colisores.
- Recriação de mapas.

Respeitar os guardas de RAM e disco definidos no plano mestre.

Se um módulo opcional prejudicar desempenho, estabilidade ou fidelidade visual, desativá-lo sem comprometer a base atmosférica.

## 7. Preservação e reversibilidade

Toda alteração atmosférica deve ser reversível.

Não modificar permanentemente:

- Sprites originais.
- Mapas.
- Posições de personagens.
- Colisores.
- Saves.
- Materiais originais.
- Sistemas de interação.

O Gothic Eclipse deve funcionar como uma camada visual sobre o jogo existente.

Desabilitar os efeitos deve restaurar a aparência anterior sem resíduos.

## 8. Critérios de aprovação visual

Comparar o resultado com a referência enviada.

Buscar:

- Profundidade atmosférica.
- Luz lunar reconhecível.
- Sombras frias.
- Contraste quente/frio.
- Chuva natural.
- Névoa equilibrada.
- Piso úmido convincente.
- Pixel art original preservada.
- Personagens claramente visíveis.

Não tentar reproduzir literalmente a arquitetura ou os personagens da imagem.

A referência serve para iluminação, atmosfera, materiais aparentes e composição cromática, não para substituir os mapas.

Se a primeira versão parecer apenas um filtro azul escuro, ajustar a distribuição de luz e profundidade antes de adicionar novos sistemas.

## 9. Critério de encerramento

Gothic Eclipse estará concluído quando:

1. O controlador e shader estiverem integrados.
2. A primeira região estiver aprovada visualmente.
3. Os dez presets estiverem habilitados e verificados.
4. Os efeitos forem reversíveis.
5. Não houver regressões de gameplay.
6. A performance permanecer aceitável.
7. Os personagens e a HUD estiverem preservados.
8. Os resultados e capturas estiverem documentados.

Não prolongar esta etapa para criar efeitos cosméticos desnecessários.

Não declarar conclusão com base apenas em revisão de código.

## 10. Próxima prioridade

Após aprovação do Gothic Eclipse, iniciar imediatamente o escopo reduzido de **Lucius Normal**.

Não começar transformações ou IA 2.0 antes de concluir Lucius.

**OBJETIVO FINAL:** entregar uma atmosfera dark fantasy próxima da referência, sem reconstruir mapas, sem alterar a identidade dos personagens e sem transformar o Gothic Eclipse em outro ciclo extenso de produção artística.

Priorizar integração, qualidade visual, reversibilidade e desempenho.
