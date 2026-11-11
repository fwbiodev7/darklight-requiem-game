# Contrato das skills DARKLIGHT — 04/11/2026

Sete skills locais em `.agents/skills/`, criadas com o `skill-creator` oficial disponível. Abra o `SKILL.md` diretamente se descoberta automática ainda não as listar; nova sessão pode ser necessária. Não mudar configurações globais para forçar descoberta.

## Autoridade e ordem

Fonte direta mais recente: `Docs/Darklight/Planejamento/DIRECAO-CODEX-20261104.txt`, cópia integral do anexo. Supera sequências contrárias de 02/11 e 03/11:

1. Corrigir problemas identificados na build de 03/11.
2. Finalizar Lucius Normal.
3. Melhorar radicalmente Gothic Eclipse.
4. Integrar menus redesenhados.
5. Finalizar transformações reutilizando sistemas existentes.
6. Integrar GDD às dez fases da campanha.
7. Criar cutscenes, prólogo e transições.
8. QA final, otimização e build Windows.

Esta rodada é preparação das skills e preservação para reset semanal, sem iniciar simultaneamente essas frentes. Ao retomar, conferir recibos/alterações posteriores antes de reabrir lotes.

## Limites comuns

- Fabius/Réquiem é a identidade ativa; Katana, Punhos/Grevas e Moto. Lucius tem identidade própria. Fabius: Normal/Ruptura/Eclipse Absoluto; Lucius: Normal/Nexo Fenrir. Sem quarta forma Abissal. Legado não expande escopo.
- Sprites completos, fontes preservadas, correção seletiva de quadros completos, preservando aprovados pixel a pixel. Sem montagem anatômica ou PPU para corrigir proporções. Não tingir pele/olhos nas transformações.
- Não refazer mapas nem duplicar sistemas. Antigravity produz engenharia de suporte em clone independente; revisar contratos, hashes, conflitos e testes antes de incorporar. Sem mensagens externas sem ordem específica.
- Coordenação executa Unity/Git/importações uma única vez. Subagentes autorizados recebem arquivos exclusivos/contexto curto, sem reverter alterações alheias.
- Somente documentação e verificações estáticas nesta rodada. Sem Unity, Play, música ou build para validar texto. Na futura execução, verificar autorização vigente de Play/áudio; exceção sonora de 25/10 não é autorização permanente.
- Preservar saves, preferências, GUIDs, originais, cache aquecido e trabalho alheio. Usar testes isolados/guardas existentes de RAM/commit, sem reduzir pisos para forçar execução.
- Publicação autorizada: cópia isolada `work/publicacao-20261021`, branch `integracao-completa-20261026`, remoto `fwbiodev7/darklight-requiem-game`. Conferir estado antes de publicar; excluir arquivos privados, MP3, builds, caches e candidatos rejeitados. Não publicar indiscriminadamente pela raiz.
- Separar produzido/revisado/integrado/importado/testado/publicado. Compilação, planejamento, prévia e recibo histórico não comprovam integração visual/runtime nova.

## Evidências de partida

`Docs/Darklight/FABIUS-NORMAL-FECHAMENTO-20261103.md` registra 170 clips/504 figuras/55 folhas e 9/9 PlayMode focados históricos; QA geral e composição de partículas na campanha ainda não certificados. Novo relato de frame deslocado/esquiva pouco visível deve ser reproduzido na build correta, sem invalidar toda arte aprovada.

Editor declarado: Unity 6000.6.0f1. Manifesto consultado: Timeline 6.6.0, URP 17.6.0, Input System 1.20.0, sem Cinemachine. Declaração não comprova resolução de pacotes, renderer ativo ou MCP conectado. Consulte `SKILLS-FONTES-20261104.md` para ferramentas externas.
