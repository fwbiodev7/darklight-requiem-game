# DARKLIGHT: REQUIEM — Handoff de recuperação e integração (11/11/2026)

**Situação:** usuário não conseguiu concluir commit nem build na escola. Todo o material foi enviado por ZIPs. Trabalhar no PC com Unity e Git, sem assumir que alterações locais anteriores tenham sido salvas no GitHub.

## 1. Segurança e estado real
- Ler AGENTS.md e o handoff oficial mais recente em Docs/Darklight.
- Registrar `git status --short`, `git branch --show-current`, `git log -5 --oneline`, `git remote -v` e o commit remoto antes de mexer.
- Fazer BACKUP/commit de arquivos locais não versionados ou alterados antes de merge/rebase. Não usar reset --hard/clean em conteúdo do usuário.
- Comparar arquivos deste pacote com `ArtSources/` e `Assets/Darklight/` existentes, usando hashes; integrar SOMENTE faltantes ou versões aprovadas mais novas.
- Evitar uso desnecessário do Codex/OpenBot; manter reserva de cota semanal e impedir ações destrutivas.

## 2. O que existe neste pacote
- Fabius Ruptura e Serafim Carmesim: 10 folhas organizadas (Katana, transformação, Punhos, Moto e Moto→Serras, 5 por forma) + variantes de arte. Reutilizar movimentos e pivôs da forma Normal já estabilizada; auras/asas/VFX em camadas quando possível.
- Lucius Normal: folhas de saque, caminhada, defesa, dano, morte, esquiva e ataques + referências. Confirmar quais são canônicas pelo histórico do projeto. Não supor que toda folha contém 8 direções.
- Lucius Arconte do Abismo: identidade branca-prateada com mechas verdes, olhos BRANCOS luminosos sem íris, aura verde-negra; golpes, transformação e ultimate. 'Nexo Fenrir' permanece nome do poder/entidade, 'Arconte do Abismo' é o nome exibido da forma.
- Quatro inimigos: Crawler, Shooter, Armored, Spectral.
- Três bosses anteriores a Lucius: Kael Voss (cap. 1), Cantor do Abismo (cap. 2), Matriarca Rubra (cap. 3). Lucius como boss final.
- Arena e materiais promocionais são referências, não assets de colisão/UI.
- A pasta 98_RASCUNHOS_NAO_INTEGRAR contém imagens erradas/montagens; jamais importar automaticamente. A imagem recortada da Matriarca é CANDIDATA, requer inspeção.

## 3. Preparar imagens
1. Extrair todos os ZIPs em pasta isolada preservando nomes/caminhos e conferir SHA-256 do INVENTARIO.csv.
2. Identificar resoluções/grade REAIS, recortar frame a frame sem cortar personagem/asas/armas, converter fundo branco/quadriculado em alfa PNG real, sem apagar cabelos claros, brilhos ou efeitos. Não assumir transparência onde houver RGB.
3. Conferir cada frame com miniaturas/atlas; rejeitar cortes incompletos ou duplicados. Pixel-art com Point/no bilinear, sem redimensionamento suave.
4. Pivôs: pés e centro corporal estáveis no humano; flutuação/centro do corpo nos espectros; contato de solo nos VFX. Nada de ancorar pela extensão da lâmina/asa/efeito.
5. Gerar `.png.meta`, `.json`, catálogo e animações somente nas famílias disponíveis. Reusar o importador e testes existentes.

## 4. Implementação incremental
- Primeiro integrar apenas o que falta dos pacotes Fabius e Lucius; preservar trabalho do Antigravity reportado nos commits 670fe079 e 93b032e2, mas confirmar no Git se eles realmente estão acessíveis.
- Depois integrar Crawler (rasteiro, salto/garra), Shooter (bala mágica violeta), Armored (mace pesada e investida), Spectral (flutuação/orbes/efeitos verdes).
- Depois Kael Voss (zweihänder com gelo, sweep, parry, estocada), Cantor (flutuação e ondas/pilares sonoros violeta), Matriarca (vôo, garras, mergulho). Usar IA e prefabs existentes, evitar scripts duplicados.
- Atenção às folhas dos bosses: material de ataque, locomoção e morte está em versões distintas. Kael Walk/Dead dedicadas NÃO foram localizadas como spritesheets finais neste pacote: se não existirem no repo, registrar pendência e fallback, não declarar completas.
- Não alterar o visual do HUD; redesenho foi adiado pelo usuário. Preservar menus, 10 mapas e gameplay existente enquanto o GDD reduz o elenco a 4 inimigos comuns + 3 bosses + Lucius final.

## 5. Testes e fechamento
- EditMode e PlayMode, spawn, ataque, dano, morte, colisão, transições, animações em todas direções disponíveis, Fabius sem jitter e Lucius boss fase 2. Não afirmar 100% sem logs reais.
- Criar `Docs/Darklight/PENDENCIAS-ARTISTICAS-INIMIGOS-BOSSES-20261111.md` por personagem e por animação com N/NE/E/SE/S/SW/W/NW marcados como real, fallback ou faltante; incluir faltantes de Fabius e Lucius.
- Validar build Windows x64 com Unity da versão exata do projeto; testar inicialização. Se RAM insuficiente, um único log claro e processo de build reproduzível, sem loops.
- Commit em branch de integração própria, conferir `git status`, registrar hash, fazer push após validação (sem force push), e informar caminho absoluto do EXE, ZIP, testes, branch, commit e pendências.
- Economizar tokens: 1 inventário, 1 plano de lotes, execução por script, 1 relatório final; não gerar novos sprites e não refazer o que já está aprovado.
