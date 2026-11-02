# Matriz de lacunas — Fabius Normal, 01/11/2026

**Recorte:** skin 0, forma Normal 0, 8 direções. Cobertura do catálogo e dos bancos ativos inspecionados; não certifica conclusão artística nem validação Unity.

| Arma | Catálogo atual | Lacuna objetiva |
|---|---|---|
| Katana | 37 páginas: Idle 8/8, Walk 5/8, Leve1/2/3 8/8 cada | 72 páginas para 9 chaves do loadout faltantes; Walk NE/E/SE; AirCombo contextual também tem 5 chaves próprias sem cobertura |
| Punhos | 40 páginas: Idle 8/8; três golpes leves e Ascensão da Ruína em 8/8; sem Walk | 64 páginas para 8 chaves faltantes; Walk sem páginas importadas. V2 tem quatro figuras S escolhidas, mas `visualApproval=false`, sem loop contínuo/import/runtime; SW/W rejeitados por perna líder repetida. AirCombo acrescenta 4 chaves contextualizadas |
| Moto | 8 páginas Dentes Cruzados, uma por direção; só consumidas no contexto a pé | 184 páginas para as 20 chaves reais (loadout base + expansões); AirCombo acrescenta 4 chaves totalizando 216 faltantes. Banco montado: ride estático 8/8, idle animado 8/8×4, slam 4 fases só S, montagem/desmontagem 6+7 quadros só S; sem ciclo Walk. Dentes no catálogo não é clip montado: `CharacterView` desvia quando `MountedPresentation=true`. O contrato canônico 26/10 permite Moto para Fabius Réquiem; as fontes do piloto Rock ainda exigem revisão própria. |

Cada ataque precisa de um clip por chave e direção, com fases de startup/contato/recuperação mapeadas pelo relógio do ataque. Não separar essas fases em clips redundantes. O JSON lista chaves, tempos e contagens. Fallback para golpe primário, Idle/Walk, pose de moto ou bancos anteriores mantém o personagem desenhado, mas não completa as ações específicas.

## Ações que faltam

Charging, Dodge, Parry, Hurt e Dead não têm clips próprios (120 contextos por arma/direção). `ValidPage` reconhece os nomes, mas rejeita clips temporizados sem `sourceSpriteNames`; com nomes-fonte, `MapsWholeSequence` só aceita Walk ou Attack. A correção prevista no consumer é pré-requisito para importar esses estados. Run/StartMove/Brake/Turn reutilizam Walk; IdleAlert/Impatient/Inspect/Fatigued/Wounded cai em Idle. Não gerar variantes separadas sem pedido visual distinto. Sem chaves exclusivas de Ruptura/Eclipse confirmadas no loadout do Normal: `Eclipse Partido` é atribuído globalmente apesar do nome; `FormAbility` é efeito pós-contato disponível só em `Form>0`, enquanto `FormStyle>=2` só habilita encadear um skill durante ataque. Nenhuma transformação é projetada nesta etapa.

**Fontes:** `normal-gap-matrix.json`; runtime `DarklightWeaponRedesignBank`, `DarklightCharacterView`, `DarklightMotorcycleRedesignBank`; loadouts e relatórios de caminhada listados no JSON. Nenhuma execução Unity ou Git nesta auditoria.

