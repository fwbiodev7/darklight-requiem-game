# DARKLIGHT: REQUIEM — tarefa para o Antigravity

## Missão

Preparar um pacote enxuto de melhorias do Gothic Eclipse, aproveitando o código e a arte existentes. O Codex continua responsável por finalizar Fabius Normal, integrar as entregas, controlar a Unity e publicar.

Trabalhe de forma independente, em arquivos separados. A aplicação do pacote à gameplay ocorrerá somente após concluir e validar Fabius Normal. Transformações continuam para depois de Lucius Normal.

**Projeto:** `C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main`

Leia o AGENTS.md do projeto. Você não está sozinho no workspace: preserve alterações de outros agentes e não reverta arquivos para versões antigas.

## Sua área exclusiva de escrita

`C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/work/antigravity-gothic-eclipse/`

Pode ler o projeto e preparar versões candidatas nessa pasta. **Não editar diretamente Assets, ProjectSettings, Packages, fontes de personagens ou a cópia de publicação.** Não fazer Git, abrir Unity, executar PlayMode, gerar builds, limpar disco ou alterar automações. Não copiar o projeto inteiro nem criar Library/caches novos.

## Autoridades e referências

Leia apenas os trechos relevantes e reutilize o contexto:

- [Escopo oficial Gothic Eclipse](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/GOTHIC-ECLIPSE-ESCOPO-ENXUTO-20261102.md).
- [Referências adicionais de iluminação, chuva e paleta](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/GOTHIC-ECLIPSE-REFERENCIAS-20261102.md).
- [Imagens privadas e hashes](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/ArtSources/AtmosphereReference20261102/references.json).
- [Plano anterior, para recuperar o que existe](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Docs/Darklight/GOTHIC-ECLIPSE-PLANO-20261101.md).
- [Estado atual e ordem de trabalho](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/outputs/retomada-20261102/continuation-state.json).

As imagens servem para atmosfera e composição cromática. Não copiar arquitetura, personagens ou tiles, nem publicar as referências privadas.

## Ponto de partida real

Os seguintes arquivos **já existem**. Não reconstruir o sistema nem substituir versões atuais por um estágio antigo:

- [Controlador](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Campaign/DarklightAtmosphereController.cs).
- [Shader](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Resources/Darklight/Rendering/AtmosphereOverlay.shader).
- [Atlas](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Resources/Darklight/AtmosphereTextures20261101/AtmosphereTextures.png).
- [Descritor do atlas](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Resources/Darklight/AtmosphereTextures20261101/AtmosphereTextures.json).
- [Integração regional](C:/Users/Usuario/Documents/Codex/2026-10-10/f/darklight-requiem-main/Assets/Darklight/Runtime/Campaign/DarklightRegionAtmosphere.cs).

Reconcilie afirmações históricas dos documentos com os recibos recentes. Não tratar revisão de código como teste executado. Na leitura de 02/11, o controlador tem dez presets, pool limitado a 88 partículas e `AllRegionsEnabled = true`; o novo plano pede validar primeiro a região 0 com as demais bloqueadas. Documentar essa diferença e preparar um gate reversível na candidata, sem alterar configurações do jogador ou ativar a candidata no projeto.

## Entregas prioritárias

1. **Diagnóstico curto:** localizar problemas concretos de opacidade, distribuição de luz, duplicação de efeitos, ordenação, restauração e custo. Citar arquivo/método e evidência; não repetir auditoria geral do jogo.
2. **Candidata para a região 0:** ajustar a infraestrutura existente para luar azul-prateado localizado, névoa baixa translúcida e chuva fina diagonal. O resultado deve ter profundidade, sem parecer uma camada azul ou branca sobre a tela inteira.
3. **Reversibilidade e orçamento:** preservar materiais/cores originais, restauração ao desativar, intensidade zero, menus/pausa, troca de região e destruição. Reutilizar pool; respeitar 88 renderizadores de chuva/partículas. Sem objetos, materiais ou texturas alocados a cada frame.
4. **Presets:** propor ajustes pequenos para os dez presets, com diferenças entre interiores/exteriores. Não expandir para todos antes do gate visual da região 0. Não fazer chover através de tetos ou paredes.
5. **Opcionais, apenas se baratos:** piso úmido com brilhos locais e halos âmbar de fontes de luz que já existem. Sem reflexos completos, espelhar personagem/cena, novas construções ou novas lanternas. Se não houver máscara/âncora confiável, registrar como opcional desativado.

Manter a pixel art nítida e a opacidade baixa, conforme feedback do usuário. Point Filter em personagens/mapas; Bilinear somente nos overlays suaves quando previsto no contrato. Não tingir pele, olhos, cabelo, roupas, NPCs, inimigos, HUD ou objetos interativos. Não reconstruir os dez mapas, alterar geometria, colisores, navegação, câmera, áudio, saves ou preferências.

Não produzir novas bibliotecas de partículas ou imagens sem necessidade comprovada. Fabius, Lucius, transformações e IA 2.0 estão fora desta tarefa.

## Formato de entrega

Dentro da sua pasta exclusiva, entregar:

- `REPORT.md`: o que encontrou, o que mudou e o que ainda depende de integração/validação.
- `candidate/`: somente arquivos novos ou modificados, organizados conforme o destino no projeto.
- `PATCH.diff`: diferenças contra a versão que você efetivamente leu, sem mudanças alheias.
- `manifest.json`: caminho de origem/destino proposto, SHA-256 da base e SHA-256 da candidata para cada arquivo. Se a base mudar enquanto trabalha, avisar; não sobrescrever a versão nova.
- `VALIDATION.md`: verificações direcionadas que o coordenador deve executar, com resultado esperado. Distinguir análise estática, compilação, teste funcional e aprovação visual.

Não afirmar que Unity compilou, testes passaram, performance melhorou ou screenshots representam gameplay se isso não foi executado. Prepare um protocolo de capturas antes/depois na região 0, com câmera, personagem e configurações iguais; não fabricar capturas.

## Critério de aceite do pacote

Pacote pequeno, aplicável por arquivo, rastreável por hash, reversível e sem conflito com Fabius. Toda pendência deve ser objetiva. Sem novas etapas cosméticas ou redesign extenso. O coordenador fará integração e validação depois de Fabius Normal; aprovação do pacote preparado não equivale a Gothic Eclipse concluído.

Economize tokens: mensagens curtas, saídas filtradas e logs detalhados em arquivo. Não instalar dependências nem iniciar subagentes adicionais por conta própria.
