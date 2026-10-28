# Publicação preparada — 24/10/2026

Destino isolado: `work/publicacao-20261021`, remoto `fwbiodev7/darklight-requiem-game`, branch `codex/expansao-aerea-moto-20261023`. Preparação não executa staging, commit, push, remoção ou alteração de saves. Entradas alheias previamente existentes no clone ficam fora do pathspec explícito.

## Autorização de fechamento e fontes novas

Ao encerrar24/10, o usuário autorizou expressamente commit e envio das alterações de hoje. Incluir as fontes aprovadas novas por nomes exatos: katana Light1v1/v2 e Walkv5, Presasv2, Punhosv2 e moto redesenhadav2, com jobs/manifests/prompts/revisões pertinentes. Os masters de katana novos são preservados integralmente para rastrear a seleção de quadros; somente S/Wv1 e SWv2 são utilizados. Esta exceção não inclui pastas candidatas inteiras nem fontes B/C/Walkv4, Punhosv1 ou Motov1 rejeitadas/superadas. As oito fontes mistas antigas continuam seguindo a regra abaixo. As fontes novas ainda não integradas devem continuar identificadas como tal.

Documentação final: `FECHAMENTO-20261024.md`, `PENDENCIAS-20261025.md` e `IDENTIDADE-CANONICA-PLAYER-20261024.md`. Minecraft permanece pausado e fora desta publicação.

## Fontes com quadros descartados

As oito fontes mistas de katana/Punhos não são publicadas. Duas contêm as caminhadas Normal rejeitadas; seis contêm variações não selecionadas. Os originais integrais continuam preservados no projeto local.

Para cada fonte mista, a cópia pública recebe uma fonte canônica em `ArtSources/WeaponRedesign20261023/publication-selected/`, **byte a byte igual ao PNG do atlas aprovado**. Nenhuma figura é desmontada, recolorida ou reescalada. Os manifests e o catálogo da cópia pública apontam para essa fonte real e seu SHA-256/dimensões. As coordenadas de origem passam a ser as próprias células dos atlas. Campos `originalGenerated*` preservam origem, hash, dimensões, índices selecionados/descartados, coordenadas e âncoras anteriores.

O importador Unity pode verificar e reimportar a cópia pública sem as folhas rejeitadas. PNGs de runtime, GUIDs, IDs, retângulos, pivôs, PPU, durações e mapeamentos de ataque não mudam. Os jobs de geração originais permanecem como registro histórico; reproduzir aquela geração integral depende das fontes locais externas. A produção publicada usa os PNGs canônicos aprovados e seus manifests.

A ferramenta refaz independentemente a identificação/atribuição de pixels de cada original e recompõe o atlas selecionado, verificando igualdade RGBA exata e contagem de pixels com alpha positivo antes de criar a cópia canônica. A auditoria estrutural deve passar novamente dentro do clone com as novas referências.

## Áudio e exclusões

Os 19 WAV preparados seguem com hashes originais. MP3 originais de efeitos e músicas não são copiados. O manifesto externo em `AudioSources/Delivery20261023-EXTERNAL-SOURCES.json` registra identidade, hash e estado dos 23 originais. Os quatro longos continuam fora do runtime; músicas de bossfight continuam adiadas.

No clone público, `audit_audio_completion_20261024.py --allow-external-originals` verifica os WAV e identifica os originais ausentes como **externos não verificados**. No projeto local, o comando padrão continua exigindo a conferência de todos os originais.

Nomes de candidatas/rejeitadas, bancos de rascunho, caches, logs e arquivos de outros projetos são excluídos da seleção. A documentação e as referências históricas podem mencionar descartes; isso não inclui as imagens candidatas descartadas.

## Operação e revisão

`prepare_continuation_publication_20261024.py` faz inventário e prova por padrão. `--copy` prepara somente a cópia isolada, sem staging. Requer o Python do projeto com NumPy/Pillow.

Evidências locais em `work/session-20261024/publication/`: `publication-scope.json` contém caminhos, hashes finais, exclusões e prova dos oito atlas; `publication-pathspec.nul` contém somente a seleção revisável. Metadados derivados são explicitamente identificados; os demais arquivos publicados devem coincidir com o checkout de origem. Atualizar a cópia após os resultados finais antes de stage/commit/push.
