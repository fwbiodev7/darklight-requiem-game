# Auditoria Git e preservação local — 26/10/2026

**Estado desta coleta: históricos locais preservados e bundles verificados; nenhuma integração executada por este agente.** A coordenação posteriormente concluiu autenticação e fetch e assumiu a criação do backup privado. O estado remoto final deve ser confirmado pelo recibo da coordenação; este documento não declara backup privado validado. Esta auditoria não executou fetch, checkout, stage, commit, push, exclusão ou alteração de referências.

## Bases distintas

- A cópia de publicação correta é `work/publicacao-20261021`, com remoto `https://github.com/fwbiodev7/darklight-requiem-game.git`, branch atual `codex/expansao-aerea-moto-20261023`, HEAD `b719c527c736009e3ff560fd124ad07f67ae30d0`. Tem 25 commits alcançáveis, histórico não raso e nenhuma alteração rastreada no momento da coleta. Existem **162 arquivos não rastreados**, registrados separadamente; eles não entram no bundle Git.
- O `.git` da raiz do workspace é um histórico legado de outro projeto, sem remoto, com 40 commits alcançáveis e HEAD `412cd5fea9c4f856e7cecf457eaa838a14ba0b19`. O estado inicial tinha 1.636 exclusões e 16 modificações rastreadas, além do conteúdo atual não rastreado. **Não usar esse histórico como origem da publicação DARKLIGHT e não enviar suas referências ao GitHub.** O backup dessa base é exclusivamente local.
- Nenhuma das duas bases é shallow. Os arquivos de trabalho, fontes e alterações ainda não commitadas continuam nos locais originais; não foram descartados. Bundle preserva histórico alcançável pelas referências, não a árvore de trabalho, reflogs ou objetos sem referência.

## Referências DARKLIGHT conhecidas

| Referência | Commit | Relação observada |
|---|---|---|
| `main` | `04a271f46ace3c68288705ef9ee8ef5f7c732898` | Ancestral da branch de expansão; nenhum commit exclusivo de main. |
| `codex/punhos-da-ruina-20261021` | `04a271f46ace3c68288705ef9ee8ef5f7c732898` | Idêntica a main; não há trabalho exclusivo para reaplicar. |
| `codex/expansao-aerea-moto-20261023` | `b719c527c736009e3ff560fd124ad07f67ae30d0` | Oito commits à frente de main; base candidata para a integração posterior ao backup remoto. |
| `origin/main` | `04a271f46ace3c68288705ef9ee8ef5f7c732898` | Igual à main local na referência remota já armazenada. |
| `origin/codex/expansao-aerea-moto-20261023` | `b719c527c736009e3ff560fd124ad07f67ae30d0` | Igual à branch local na referência já armazenada. |
| `origin/HEAD` | `04a271f46ace3c68288705ef9ee8ef5f7c732898` | Referência simbólica para `origin/main`. |
| tag `v0.1.0-alpha` | `4c8e693679e0312a15aba1cab642b261a9078cab` | Preservada no bundle. |

São três branches locais, duas branches remotas conhecidas, um HEAD remoto simbólico e uma tag. **Sem fetch nesta auditoria:** isso não certifica a lista atual de todas as branches existentes no servidor. A ancestralidade de main e punhos em relação à expansão retornou código 0; as comparações dos dois pares local/upstream conhecidos retornaram `0 0`.

Os oito commits exclusivos da expansão são: `f912437` (combate aéreo/moto/encontros), `0c2e6bb` (contatos aéreos e PlayMode), `fccef17` (registro de teste Windows), `67227a9`, `7895bea`, `da1e720`, `5c91bf6` (arte, validação e checkpoints) e `b719c52` (registro da build). Seus commits permanecem intactos; isso não substitui a validação Unity do novo escopo.

## Bundles locais verificados

Arquivos e recibos em `work/retomada-20261026/git-backup/`:

| Bundle | Tamanho | SHA-256 | Verificação |
|---|---:|---|---|
| `darklight-publicacao-all-refs-20261026.bundle` | 1.883.626.407 bytes | `30948646ce4ae2c61b0511c118e2bd4ac0fd47d7bbadde73985d08f35ab7345c` | `git bundle verify`: exit 0, histórico completo; oito refs incluindo HEAD, todas as sete refs esperadas presentes. |
| `legacy-root-all-refs-LOCAL-ONLY-20261026.bundle` | 629.634.641 bytes | `90e828f49531ed8d07d64b6e07a816f3ffb9472b4906f96ed4c9cf0eda5ffea1` | `git bundle verify`: exit 0, histórico completo; cinco refs incluindo HEAD. Somente local. |

No legado, `main` e `codex/fundacao-darklight` apontam para o mesmo HEAD; tags `alpha-0.3` (`6edf6528301f41282ffc5b8b295384707baaa8e3`) e `jogo_varginha_build_alpha` (`a9f7e374a15c67c8c558c21a1ae40d2017a59263`) foram preservadas apenas nesse bundle local. Nenhuma referência foi misturada com DARKLIGHT.

Recibos: `darklight-bundle-receipt.json`, `legacy-bundle-receipt.json`. Logs completos: `*-bundle-create.log`, `*-bundle-verify.log`, `*-bundle-refs.txt`. As referências anteriores e o estado de trabalho da publicação estão em `darklight-refs-before.txt` e `darklight-working-status-before.txt`. Não enviar bundles, logs locais com caminhos privados ou o histórico legado como parte do jogo.

## LFS, submódulos e dados sensíveis

- `git lfs ls-files --all -n` retornou exit 0 e **zero caminhos LFS** nas duas bases. Não há diretório local `.git/lfs`, `.gitattributes` rastreado no HEAD nem entradas Git de submódulo (`160000`). Não existe objeto LFS identificado para transferir nesta coleta.
- Triagem de nomes dos **4.715 caminhos históricos únicos** da publicação encontrou zero candidatos a `.env`, chaves privadas/certificados e arquivos convencionais de credenciais. Não foram lidos nem impressos valores de segredos. Essa triagem por nomes não é uma certificação de ausência de credenciais embutidas em conteúdo; tal revisão continua necessária antes da transferência remota exigida pelo pedido.
- Dependências do projeto Unity continuam declaradas em `Packages/manifest.json` e `Packages/packages-lock.json`; esta auditoria não instalou nem atualizou pacotes.

## Decisões e bloqueio

1. **Nenhuma branch foi integrada ou descartada, e nenhum conflito foi produzido.** A branch de punhos já está contida em main e na expansão; não fazer merge redundante.
2. Não criar nem publicar `integracao-completa-20261026` antes de cumprir o backup remoto privado requerido pelo usuário. A branch de expansão é candidata por ancestralidade, sujeita à confirmação das refs remotas e ao novo escopo Réquiem/Katana/Punhos/Moto e três formas.
3. O bloqueio comunicado pela coordenação é **ausência de autenticação GitHub disponível no `gh` deste ambiente**. A auditoria não pediu credenciais, não tentou enviá-las, não criou o repositório e não alterou a visibilidade do principal. Nome solicitado: `darklight-requiem-backup`; URL e visibilidade PRIVATE ainda não foram comprovadas.
4. Próxima etapa: após autenticação existente estar utilizável, atualizar refs e enumerar todas as branches do servidor, revisar o escopo seguro do histórico, criar o backup explicitamente privado, transferir branches/tags e comparar hashes de todas as referências importantes. Somente após validar esse backup iniciar a branch de integração. A raiz legada permanece fora da transferência.
5. Depois da integração isolada, executar compilação/Unity e testes focados do novo escopo, documentar decisões, gerar build verificável e abrir PR apenas se os gates essenciais passarem. Não fazer merge definitivo em main sem autorização. **PR ainda não criado; integração Unity ainda não testada nesta auditoria.**

As pendências de gameplay, arte e build são acompanhadas pela coordenação em documentos próprios. Um bundle local válido não cumpre sozinho a exigência de backup privado remoto validado.

## Atualização após autenticação e fetch pela coordenação

O bloqueio de autenticação descrito acima foi resolvido pela coordenação, com a conta `fwbiodev7`. O fetch trouxe `origin/codex/animacoes-pele-20261017` em `5d48ce20eecf6760c1d5879fd249d6fc7c273a2c`, `origin/codex/padronizacao-visual-20261020` em `bdeb0763feaee10b1e79534b41aa6d9c8fd53026` e tags `v1.0.0-beta.1` a `v1.0.0-beta.5` e `v1.0.13`. O bundle DARKLIGHT inicial antecede esse fetch e **não inclui essas novas refs**; a coordenação decidiu preservar ambos os bundles sem suplemento para reservar disco à build, e validar as refs novas no backup remoto.

A branch de animações é ancestral da padronização (comparação `0 1`). A primeira contém seis commits exclusivos em relação à expansão; a segunda contém esses mesmos seis mais `bdeb076`. Os commits são `f24b242`, `8e66d05`, `126f24b`, `e3536cc`, `7ec2ecc`, `5d48ce2` e `bdeb076`: idle vivo/pele, cristais/morte/pontes, inicialização de testes fora de tela, direções da moto, contatos e revisões de arte. Não é necessário mesclar animações separadamente se padronização for integrada preservando sua ancestralidade.

Comparação de blobs dos caminhos alterados desde main, registrada integralmente em `branch-tree-comparison.json`:

| Branch | Caminhos alterados | Iguais ao HEAD publicado | Iguais em bytes ao workspace atual |
|---|---:|---:|---:|
| animações |598|333|403|
| padronização |715|403|495|

Não foi comprovado que a árvore atual é superset integral: restam220 caminhos diferentes/ausentes frente à padronização, incluindo documentação, metadados, shaders e scripts evoluídos. **Não usar merge `-s ours` com base apenas nessas contagens.** Os shaders atuais acrescentam a proteção `_RuinFists` à correção de pele e o runtime atual acrescenta combate aéreo/seis técnicas; substituir pelos antigos retiraria melhorias. Revisar as diferenças restantes por finalidade antes de registrar decisões de integração. O diff de runtime está em `padronizacao-to-publication-runtime.diff`. As artes/paletas antigas exigem revisão à luz do contrato atual; não restaurar Rock, Presas, cabelo/olhos rejeitados ou formas removidas automaticamente.

Triagem adicional por conteúdo sem exibir valores: **3.821 blobs de texto únicos,46.665.992bytes,zero achados** em padrões de chave privada, tokens GitHub/OpenAI/AWS/Slack e URLs com credenciais. Limites: formatos de texto selecionados até2MB e padrões conhecidos; não certifica ausência de credenciais arbitrárias ou ofuscadas. Recibo `darklight-credential-pattern-audit.json`.

Auditoria de tamanho dos5.598blobs conhecidos antes do fetch: **nenhum acima de100.000.000bytes**. Maior: `Assets/Darklight/Resources/Darklight/Audio/park_ambience_wind.wav`,73.960.096bytes. O histórico já contém os MP3 fornecidos e músicas de runtime; o bundle os preserva como parte do histórico existente, sem acrescentar novos arquivos. Lista de maiores em `darklight-large-blobs.json`.
