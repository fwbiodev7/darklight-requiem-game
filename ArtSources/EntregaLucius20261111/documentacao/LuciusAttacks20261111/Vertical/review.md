# SwordVertical — checkpoint11/11/2026

Produzidas duas folhas candidatas de12figuras (24figuras), nenhuma importada ou testada em Unity. Fontes PNG exatas, sem reamostragem, montagem anatômica, espelhamento ou retoque de pixels. Ferramenta: imagegen integrada. Inspecionadas referências inteiras, fonte gerada e sequência estática das fases. Não certificada execução em movimento no consumidor.

## Fontes e estados

- `LuciusSwordVerticalSouthWest20261111V1-wrong-directions.png` — 1086×1448, SHA-256 `2ab064d167b6ec6f8c07dd7bb7ece2f6302bb1706aa9dfd99286a5cfa2eba2ad`. Estado `rejected-wrong-directions`.
  - Requested SW/W/NW were generated facing right. Not a selected delivery.
- `LuciusSwordVerticalSouthWest20261111V2-source.png` — 1086×1448, SHA-256 `17fbc2ded68c94dff1efb8e001862e03297c82ec96a27fe58d0a3949b059640d`. Estado `candidate-visual-reviewed-with-blocker`.
  - SW active: steel blade points down-left but arc/impact remains on right. Requires whole-figure correction before full-sheet integration.
  - Active effects extend near or across nominal cell boundaries; nominal grid is not certified safe slicing.
  - NW retains substantial visible face in startup/recovery, although back coat is readable; verify direction during integration.
- `LuciusSwordVerticalNorthEast20261111V1-source.png` — 1086×1448, SHA-256 `5338a28272e1cf17c09eddec9d9123ad34364050d4a93b0bdd17420ffcda0268`. Estado `candidate-visual-reviewed-with-reservations`.
  - Active blue effects are broad and can overlap nominal cell limits. Whole figures remain visible; exact extraction rectangles require audit.
  - Generated size differs from prompt; retain source without resampling and calibrate PPU from body at integration.

A V1 SouthWest foi rejeitada por direção e preservada apenas como evidência, excluir da seleção de entrega. V2 corrigiu os perfis para esquerda, mas o contato SW ainda tem arco à direita enquanto lâmina aponta à esquerda: defeito específico impeditivo, não aprovação da folha inteira. NE V1 contém as vistas N/NE/E/SE esperadas e fases altas/descendentes/baixas legíveis; o efeito amplo exige extração real antes de usar.

As figuras têm uma só espada empunhada com duas mãos, sem espada duplicada nas costas. Roupa/cabelo/olhos e porte seguem referências inspecionadas. Preparação alta explícita, golpe descendente e recuperação em guarda baixa. O desenho mantém aproximadamente o mesmo centro dos apoios por linha, com flexão/torção natural; pivôs não foram calibrados e estabilidade runtime não foi alegada.

Os retângulos do manifesto são a grade NOMINAL, não fatiamento aprovado: não cortar efeitos usando a grade automaticamente. O tamanho real gerado diverge do pedido; não aumentar ou deformar para simular resolução nativa maior. Transparência real preservada; não removidos halos ou cores de borda por heurística.

Timings consultados no consumidor real: startup140ms/active120ms/recovery220ms, uma figura por fase. O runtime preserva dados existentes de AttackDefinition. Não houve mudança em Assets, Runtime, catálogo compartilhado, Git, Unity ou automação.

Encerrado por orientação do coordenador após informação de98% da cota semanal. NorthEast já estava em voo quando veio a ordem; nenhuma geração nova iniciada depois dela. Próximo passo: corrigir somente a figura inteira SW/active; depois auditar extração/efeitos, calibrar PPU/pivôs pelo corpo e integrar/testar sob coordenação. Candidata revisada não implica importada ou runtime verificada.
