# DARKLIGHT: REQUIEM — concluídos e pendências para 28/10/2026

Atualizado em 28/10/2026 17:58:42, Brasília. Documento único da rodada. O redesign completo continua parcial.

## Entregas e decisões

- Checkpoint enviado ao GitHub às21h39: `ce985ad2f6cd5148f7dcea51d8b911b7807b5fee`, branch `integracao-completa-20261026`. A revisão posterior deve usar os resultados abaixo.
- Antigravity integrado por fast-forward até `f26ecf4`, dez commits preservados e backup PRIVATE validado antes da integração. O diff original de apoio não alterava Runtime nem artes. Corrigidas as incompatibilidades encontradas com o escopo simplificado e os fixtures locais.
- Arsenal ativo: Katana (0), Punhos da Ruína (2), Moto/duas serras (3). Fabius somente Réquiem. Formas 0 Normal, 1 Ruptura, 2 Eclipse Absoluto fundido. Rock/Presas/form3 permanecem arquivos históricos, sem seleção ativa.
- Regra humana mais recente: excluir 100% dos quadros antigos da apresentação ativa do jogador. O consumidor atual busca somente figuras do redesign; lacunas usam pose/ciclo novo da mesma arma e direção. Técnicas sem fonte própria compartilham golpe novo; formas ainda sem fonte conservam a figura Normal nova sem filtro de transformação. Isso exclui mistura antiga, mas não conclui a coreografia de cada técnica.
- Moto: a configuração a pé empunha duas serras circulares; investidas e técnicas montadas exibem a moto e retornam às serras. A separação visual completa e as demais animações ainda precisam de folhas específicas.
- Katana continua autoridade de rosto, roupa, maturidade, cabelo, olhos e proporção. WorldVisualScale 1.325 mantido. Correções por figuras completas; sem montagem anatômica.
- Configuração de subagentes aplicada no projeto: três perfis, contexto curto, até três agentes, modelos por função e permissões herdadas. Artistas e auditor foram utilizados; nenhuma configuração global alterada.
- Limpeza segura já executada: 19.703.023.347 bytes removidos em caches de cópia antiga e oito builds superadas. Fontes, saves, histórico e build anterior preservados.

## Arte produzida, revisada e selecionada

| Lote | Conteúdo / estado |
|---|---|
| Katana SW/W V1 | 12 figuras; aprovação humana preservada, dois clips integrados. |
| Katana NW/N | 12 figuras; nove originais preservadas, três reparadas inteiras; dois clips integrados. |
| Katana NE/E e SE | Candidatas preservadas fora da seleção ativa; repetição da perna dianteira impede declarar ciclo completo. |
| Punhos Chute | 24 figuras nas oito direções, três fases por direção; integração de 26/27 preservada. |
| Punhos Ascensão da Ruína | 24 figuras novas, oito direções, antecipação/uppercut/recuperação; 140/120/280 ms. Duas folhas revisadas e adicionadas ao catálogo. |
| Moto Normal V3 | Oito vistas montadas e quatro fases Slam S, identidade adulta revisada. |
| Moto idle MatureV1 | 32 figuras, oito direções, quatro fases. Fontes mantidas para modo montado; não são idle a pé com serras. |
| Duas serras Normal V1 | Oito vistas a pé e quatro fases de ataque Sul. Oito figuras originais preservadas e quatro reparadas inteiras. Demais ataques/direções continuam compartilhando poses, sem declaração de animação exclusiva. |
| Contatos pixel art | 12 efeitos Punch/Katana/Heavy. Escala pelo conteúdo visível, diâmetros 1.4/1.65/2.1 unidades; quatro fases em 260 ms, partículas pixeladas e redução de efeitos respeitada. Disparo somente após acerto real. |

Catálogo principal: 174 clips de contexto; Moto e contatos têm bancos separados. Contagem não significa conclusão. Fontes, prompts, hashes, rects, PPU, avisos e prévias ficam em `outputs/retomada-20261027/` e nos manifests dos assets. Rejeitadas e intermediários ficam fora da publicação. O helper da cópia publicada rebaseia fontes mistas para atlas contendo somente figuras selecionadas; o workspace ativo e fontes originais são preservados.

## Validação real desta revisão

| Etapa | Resultado |
|---|---|
| import | concluída, saída zero |
| edit | 82/82 aprovados |
| play | 23/23 aprovados |
| build | concluída, saída zero |

Falha reportada pelo processo: `nenhuma registrada nesta execução`.

ZIP: `DarklightRequiem-Windows-20261027-221104.zip`; 1410501399 bytes; SHA-256 `25afe58f9801d3c92f0bd39975b111ace261d3448f241f899b9b6bc697dd6387`.

Standalone automático: 12 cenários, success=True. Execução sem ativar a janela; medir foco separadamente. Recibo em `standalone-auto-222919/result.json`.

Os gates anteriores passaram 82/82 EditMode e21/21 PlayMode, mas antecedem a exclusão total do legado, Heavy novo e duas serras. Só os gates frescos acima validam esta revisão. Novos testes percorrem todos os golpes Katana/Punhos e os estados das três armas nas oito direções/formas, além do fluxo real serras→investida→serras e dos impactos maiores. Testes automáticos não equivalem a campanha humana completa ou controle físico. PC04/PC06 seguem 2/2 pela evidência de26/10; não repetidos sem mudança sonora.

## Pendências prioritárias para amanhã

1. Ler o resultado mais recente do gate/build/standalone e corrigir qualquer falha registrada; não repetir gates aprovados sem mudança relevante. Abrir a build mais recente, não o ZIP anterior.
2. Completar ciclos próprios de idle/caminhada/corrida/transições de Punhos e serras a pé; Katana ainda precisa NE/E e SE com alternância real.
3. Desenhar cada técnica ainda compartilhada, em oito direções: combos restantes, pesados/carregados, contra-ataques, habilidades, aéreos, finalizadores e técnicas especiais. A matriz detalhada abaixo é a autoridade da cobertura.
4. Moto: desenhar separação/reunião das duas serras, locomoção a pé e montada, transições e fases/direções próprias dos 20 ataques. Oito poses estáticas e um corte Sul não fecham essa lista.
5. Completar defesa/parry, esquiva, dano, morte, alertas e transformação. Poses novas de continuidade não contam como esses ciclos completos.
6. Depois do Normal, completar Ruptura e Eclipse Absoluto fundido violeta/prata, com anatomia/cabelo constantes. Fontes antigas chamadas Eclipse não certificam a fusão.
7. Auditar semanticamente as 220 diferenças das branches antigas17/20 sem merge cego. Não reintroduzir Rock/Presas nem animação superada. Nunca merge main sem autorização.
8. Validar campanha humana, audição subjetiva e controle físico quando disponível. Depois do redesign: Combat AI2.0, Sangue do Abismo/Lucius/O Encapuzado e site; Minecraft pausado.

## Trabalho independente para Antigravity

Usar clone próprio baseado no HEAD remoto de `integracao-completa-20261026`. Pode auditar diferenças17/20, CI/portabilidade, runner/relatórios, GUIDs/referências e smoke de entrada com evidências. Não alterar artes, Runtime, checkout original ou integrar/publicar. Entregar propostas com hashes e caminhos para revisão. Handoff operacional: `Docs/Darklight/DIVISAO-TRABALHO-ANTIGRAVITY-20261027.md`.

## Publicação e retomada

Somente a cópia `work/publicacao-20261021` publica para `fwbiodev7/darklight-requiem-game`, branch `integracao-completa-20261026`. Git da raiz pertence a projeto legado. Excluir builds/ZIPs, saves, caches, MP3 originais, referências privadas e artes rejeitadas. O recibo local do envio fica em `outputs/retomada-20261027/final-git-receipt.json`. Nenhum merge main autorizado.

Não iniciar novos lotes ao atingir o limiar de uso; preservar fontes e tarefas em voo. Em22h03 o saldo consultado era16%na janela e8%semanal. Documento pronto para retomada manual em28/10; retomada automática agendada para02/11 às07h45, após a renovação semanal informada pelo serviço. O lembrete21h40 foi concluído. Começar lendo este documento e os recibos reais.

## Matriz completa de cobertura artística

# Auditoria de cobertura de animações — 27/10/2026

## Escopo ativo

Requiem; armas IDs 0 Katana, 2 Punhos, 3 Moto. Formas ativas: Normal (0), Ruptura (1), Eclipse Absoluto fundido (2). Form 3 é legado. As folhas antigas chamadas Eclipse/form 2 também são legado: não certificam a forma fundida violeta/prata e foram retiradas da cobertura ativa.

## Katana e Punhos: cobertura exata

`—` = nenhum clip específico multi-quadro; cada direção omitida está faltando. Direções: S, SW, W, NW, N, NE, E, SE. Alertas agrupam IdleAlert/Impatient/Inspect/Fatigued/Wounded. Movimento agrupa Run/StartMove/Brake/Turn.

| Arma / forma | Idle | Alertas | Walk | Movimento | Charging | Guard/Parry | Dodge | Hurt | Dead | Transform | Ataques multi-quadro específicos |
|---|---|---|---|---|---|---|---|---|---|---|---|
| Katana / Normal | S/SW/W/NW/N/NE/E/SE | — | S/SW/W/NW/N | — | — | — | — | — | — | — | Leve1 S,SW,W,NW,N,NE,E,SE; Leve2 S,SW,W,NW,N,NE,E,SE |
| Katana / Ruptura | S | — | — | — | — | — | — | — | — | — | Leve1 S |
| Katana / Eclipse Absoluto fundido | — | — | — | — | — | — | — | — | — | — | — |
| Punhos / Normal | S/SW/W/NW/N/NE/E/SE | — | — | — | — | — | — | — | — | — | Direto da Ruína S,SW,W,NW,N,NE,E,SE; Chute Quebra-Osso S,SW,W,NW,N,NE,E,SE; Ascensão da Ruína S,SW,W,NW,N,NE,E,SE |
| Punhos / Ruptura | S | — | — | — | — | — | — | — | — | — | Direto da Ruína S,SW,W |
| Punhos / Eclipse Absoluto fundido | — | — | — | — | — | — | — | — | — | — | — |

### Ataques reais — chaves e direções

**Katana / Normal**
`Leve1`: S,SW,W,NW,N,NE,E,SE; `Leve2`: S,SW,W,NW,N,NE,E,SE; `Leve3`: sem clip exato; `Pesado`: sem clip exato; `Carregado`: sem clip exato; `ContraAtaque`: sem clip exato; `OndaDoVeu`: sem clip exato; `KatanaAerial`: sem clip exato; `Corte do Crepúsculo`: sem clip exato; `Eclipse Partido`: sem clip exato; `Corte do Silencio`: sem clip exato; `Ruptura Precisa`: sem clip exato.
**Katana / Ruptura**
`Leve1`: S; `Leve2`: sem clip exato; `Leve3`: sem clip exato; `Pesado`: sem clip exato; `Carregado`: sem clip exato; `ContraAtaque`: sem clip exato; `OndaDoVeu`: sem clip exato; `KatanaAerial`: sem clip exato; `Corte do Crepúsculo`: sem clip exato; `Eclipse Partido`: sem clip exato; `Corte do Silencio`: sem clip exato; `Ruptura Precisa`: sem clip exato.
**Katana / Eclipse Absoluto fundido**
`Leve1`: sem clip exato; `Leve2`: sem clip exato; `Leve3`: sem clip exato; `Pesado`: sem clip exato; `Carregado`: sem clip exato; `ContraAtaque`: sem clip exato; `OndaDoVeu`: sem clip exato; `KatanaAerial`: sem clip exato; `Corte do Crepúsculo`: sem clip exato; `Eclipse Partido`: sem clip exato; `Corte do Silencio`: sem clip exato; `Ruptura Precisa`: sem clip exato.
**Punhos / Normal**
`Direto da Ruína`: S,SW,W,NW,N,NE,E,SE; `Chute Quebra-Osso`: S,SW,W,NW,N,NE,E,SE; `Giro Devastador`: sem clip exato; `Martelo da Ruína`: sem clip exato; `Ascensão da Ruína`: S,SW,W,NW,N,NE,E,SE; `Juízo da Ruína`: sem clip exato; `Resposta Brutal`: sem clip exato; `Cataclismo da Ruína`: sem clip exato; `Queda da Ruína`: sem clip exato; `Espiral da Ruína`: sem clip exato; `Órbita Devastadora`: sem clip exato; `Sentença da Ruína`: sem clip exato.
**Punhos / Ruptura**
`Direto da Ruína`: S,SW,W; `Chute Quebra-Osso`: sem clip exato; `Giro Devastador`: sem clip exato; `Martelo da Ruína`: sem clip exato; `Ascensão da Ruína`: sem clip exato; `Juízo da Ruína`: sem clip exato; `Resposta Brutal`: sem clip exato; `Cataclismo da Ruína`: sem clip exato; `Queda da Ruína`: sem clip exato; `Espiral da Ruína`: sem clip exato; `Órbita Devastadora`: sem clip exato; `Sentença da Ruína`: sem clip exato.
**Punhos / Eclipse Absoluto fundido**
`Direto da Ruína`: sem clip exato; `Chute Quebra-Osso`: sem clip exato; `Giro Devastador`: sem clip exato; `Martelo da Ruína`: sem clip exato; `Ascensão da Ruína`: sem clip exato; `Juízo da Ruína`: sem clip exato; `Resposta Brutal`: sem clip exato; `Cataclismo da Ruína`: sem clip exato; `Queda da Ruína`: sem clip exato; `Espiral da Ruína`: sem clip exato; `Órbita Devastadora`: sem clip exato; `Sentença da Ruína`: sem clip exato.

## Moto (ID 3): dual-saw a pé e montada são escopos separados

### Dual-saw a pé

Loadout real: Serra de Entrada, Dentes Cruzados, Giro das Engrenagens, Quebra de Eixo, Sobrecarga das Serras, Contra-Eixo, Esmagamento do Abismo, Ascensao Serrilhada, Engate Preciso, Queda do Motor, Investida Demoniaca, Derrapagem Sangrenta, Serras Continuas, Investida Final, Serra Infernal, Giro Devastador, Esmagamento Vertical, Roda Destruidora, Cavalgada Brutal, Execucao Aerea Motorizada. Banco separado ReqMotoNormalDualSaws8AttackS20261027V1 selecionado e importado às22h12 após reparos completos. Oito poses direcionais e quatro fases Sul são material genérico parcial; não concluem os20ataques nem as3formas. Runtime em validação no gate final.

| Forma ativa | Direções de ataque final importadas |
|---|---|
| Normal | 8 poses prontas +4 fases Sul; ainda sem ciclos próprios de idle/caminhada |
| Ruptura | — |
| Eclipse Absoluto fundido | — |

### Moto montada

| Forma | Idle animado | Vistas estáticas | Ataques exatos |
|---|---|---|---|
| Normal | S/SW/W/NW/N/NE/E/SE; 4 quadros cada | oito direções; não são locomoção | Esmagamento do Abismo e Esmagamento Vertical compartilham um slam Sul de 4 quadros; outras 18 técnicas sem clip específico |
| Ruptura | — | — | nenhum descritor ativo |
| Eclipse Absoluto fundido | — | — | nenhum descritor ativo |

Oito poses montadas não contam como animação. O slam compartilhado é uma única sequência, não dois clips exclusivos.

## Formas antigas

- **Form 2 antigo (Eclipse):** registros de Katana/Punhos separados como legado; não contam para Eclipse Absoluto fundido.
- **Form 3:** legado, fora do escopo jogável ativo.

## Dependências

- Katana and Punhos Normal/Ruptura: add the listed missing exact clips per state, attack key, and direction. The runtime can share a primary same-weapon sequence, but this does not fulfill named-move authored coverage.
- Eclipse Absoluto fused violet/silver: create and import dedicated artwork for both Katana and Punhos; old form-2 Eclipse sheets are legacy and excluded. Then complete all states, all real attack keys, and eight directions.
- Katana: Leve1/Leve2 only have complete Normal 8-direction sequences; current Ruptura has Leve1 South only. All other listed attacks and states lack exact clips in those forms.
- Punhos: Direto da Ruína and Chute Quebra-Osso have complete Normal 8-direction sequences; Ruptura Direto is S/SW/W only. All other listed attacks and states lack exact clips in those forms.
- Locomotion: five exact Walk contexts exist; no exact Run/StartMove/Brake/Turn contexts. Add full cycles/transitions in the active forms and directions.
- Defense/reaction/transformation: no exact Charging, Parry/Guard, Dodge, Hurt, Dead, or Transformation clips exist in active Katana/Punhos contexts. Guard has no distinct bank action today.
- Moto dual-saw on-foot (ID 3): author final attack-specific sequences after the imported generic 8-view/4-phase source for all 20 real moves, eight directions, and three active forms; import/review each descriptor. The current 8 views + 4 South frames are partial generic material only.
- Moto mounted (ID 3): Normal idle loops are present in all eight directions; eight ride views are static poses. Only two ground-slam move keys share one South four-frame clip. Add unique clips for every remaining move/state/direction/form and mounted Ruptura/Eclipse Absoluto descriptors.
- The moto is a separate weapon/loadout, not Katana/Punhos: track dual-saw on-foot and mounted motorcycle as separate presentations with their own complete attack/motion contracts.
- After art production, run Unity import/runtime verification; current source/page review flags do not prove runtime verification.

## Auditoria estrutural e limites

O catálogo ativo atual contém 67 páginas Katana/Punhos em Normal/Ruptura e está com `complete=false`. `Tools/AnimationPreview/audit_weapon_redesign.py` encontrou 0 erros estruturais; sua contagem global abrange skins/armas/forms legados e não é headline de cobertura ativa. `visual-reviewed` não certifica forma fundida, importação nem runtime. Nenhuma Unity foi executada.

Teste atualizado sem execução: `Assets/Darklight/Tests/PlayMode/DarklightSimplifiedScopeGameplayTests.cs`.
