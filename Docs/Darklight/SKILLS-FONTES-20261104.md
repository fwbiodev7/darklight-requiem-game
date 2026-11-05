# Fontes, equivalência e dependências — 04/11/2026

Pesquisa de disponibilidade/documentação, sem instalação de ferramentas externas Unity nem alteração MCP. Skills locais são instruções próprias; não incorporam arquivos externos nem substituem pacotes Unity.

| Fonte consultada | Licença observada | Compatibilidade e decisão |
| --- | --- | --- |
| [Unity Agent Plugin oficial](https://github.com/Unity-Technologies/unity-agent-plugin) | [Unity Companion License](https://github.com/Unity-Technologies/unity-agent-plugin/blob/main/LICENSE.md) para projetos dependentes de Unity | Existe; catálogo de skills para agentes, sem provar ponte Editor ativa. Avaliar instalação seletiva quando necessária; não instalar catálogo inteiro nem sobrescrever configuração funcional. |
| [Unity Open MCP Timeline](https://github.com/AlexeyPerov/Unity-Open-MCP/blob/master/skills/extensions/timeline/SKILL.md) | [MIT do repositório](https://github.com/AlexeyPerov/Unity-Open-MCP/blob/master/LICENSE) | Exige servidor conectado, pacote Timeline e grupo opt-in ativo. Timeline já declarada; ferramentas/servidor indisponíveis nesta sessão. Não instalar MCP concorrente. |
| [Unity Open MCP Cinemachine](https://github.com/AlexeyPerov/Unity-Open-MCP/blob/master/skills/extensions/cinemachine/SKILL.md) | MIT do mesmo repositório | Extensão exige Cinemachine 3.x, sem suporte 2.x. Pacote ausente no manifesto. Não instalar só para documentação; confirmar versões na futura integração. |
| [Unity Pixel Perfect oficial](https://github.com/Unity-Technologies/skills/blob/main/skills/2d-pixel-perfect/SKILL.md) | [Unity Companion License](https://github.com/Unity-Technologies/skills/blob/main/LICENSE.md) | Existe e distingue URP/Built-in. Detectar pipeline real; em URP, verificar implementação própria antes de instalar standalone Built-in. Defaults externos não substituem câmera/resolução/pivôs aprovados. |
| [Pixel Art Fundamentals](https://github.com/ProkStudio/skills/blob/main/skills/game/2d/pixel-art-fundamentals/SKILL.md) | [Licença raiz cobre somente scaffolding](https://github.com/ProkStudio/skills/blob/main/LICENSE); skills conservam licenças upstream | Existe, mas licença específica ainda não confirmada: não copiar/instalar. Workflow equivalente local em `Tools/Skills/pixel-art-sprite-sheet-master/` e skills de arte do catálogo. |

Consulta de `LICENSE` sem extensão em Unity Skills retornou 404; arquivo confirmado é `LICENSE.md`. Identificação documental, sem parecer jurídico ou redistribuição de terceiros.

## Mecanismo e validação

[Documentação oficial de skills](https://developers.openai.com/codex/skills/) e [uso oficial em repositórios](https://developers.openai.com/blog/skills-agents-sdk): SKILL.md, frontmatter name/description e recursos opcionais. Usado `C:/Users/Usuario/.codex/skills/.system/skill-creator/SKILL.md`; `scripts/quick_validate.py` verifica estrutura/frontmatter, sem provar execução Unity.

Dependências novas do jogo: nenhuma. Para executar o validador oficial, foi instalado PyYAML 6.0.3 somente em `work/skills-20261104/pydeps`, sem modificar Python global ou Unity. O Python padrão não possuía `yaml`; a execução oficial passou após essa instalação isolada. Configurações MCP, Packages/manifest.json, Packages/packages-lock.json e ProjectSettings sem alteração desta rodada. Testes estruturais/exercícios de leitura estão no checkpoint; não equivalem a Editor, importação ou gameplay. Instalações futuras devem registrar versão/commit, licença específica, arquivos alterados, dependências e teste real.
