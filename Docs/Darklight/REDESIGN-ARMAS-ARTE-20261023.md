# Redesign de armas — entrega de arte parcial, 23/10/2026

Estado desta etapa: **seis folhas prontas para importação**, fontes preservadas e QA técnico/visual de prévias concluído. Este relatório não confirma importação, shader ou gameplay Unity; a rodada Unity é coordenada separadamente. Nenhum som foi iniciado pelo fluxo de arte.

## Cobertura entregue

| Folha | Contexto | Figuras | Fonte → atlas | PPU fixo |
|---|---|---:|---|---:|
| ReqDaggersNormalIdleA | Réquiem Normal, adagas Idle S/SO/O | 12 | 1448×1086 → 1820×1176 | 170.454545 |
| ReqDaggersRupturaIdleA | Réquiem Ruptura, adagas Idle S/SO/O | 12 | 1536×1024 → 1892×1122 | 164.141414 |
| ReqDaggersEclipseIdleA | Réquiem Eclipse, adagas Idle S/SO/O | 12 | 1448×1086 → 1804×1167 | 172.727273 |
| ReqDaggersAbsolutoIdleA | Réquiem Absoluto, adagas Idle S/SO/O | 12 | 1448×1086 → 1820×1167 | 171.969697 |
| ReqKatanaNormalSouth | Réquiem Normal, katana S Idle/Leve1 | 8 | 1448×1086 → 2700×808 | 174.242424 |
| ReqFistsNormalSouth | Réquiem Normal, punhos S Idle/Direto da Ruína | 8 | 1448×1086 → 2044×772 | 175.252525 |

São **64 figuras completas, 16 clips de contexto**. Um retorno de soco permanece somente na prévia/folha; 63 figuras têm referência de runtime explícita. O catálogo declara `complete=false` e fallback para o mesmo contexto anterior. O inventário genérico contém 1536 contextos centrais possíveis (duas skins × três armas × quatro formas × oito direções × oito ações); 16 possuem algum clip novo. Isso não significa que existam 1520 defeitos no jogo: os contextos restantes continuam nas artes anteriores e ainda não têm o redesign.

Nomes, hashes, dimensões e correspondência fonte→figura estão nos JSON ao lado dos PNG em `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/`. Originais e referências completas em `ArtSources/WeaponRedesign20261023/`. Nenhum GUID/meta legado foi alterado pela preparação; Unity administra os metas da nova entrega.

## Registro de escala e fases

PPU = mediana da altura da primeira direção na fonte / **1.98 unidades locais**, fixo por atlas. O registro usa somente translação inteira de cada figura completa, sem reescala anatômica. O apoio dos pés mantém âncora por quadro. Idle: neutral/inhale/blink/return, **350/250/180/350 ms**. O banco novo usa esses tempos explícitos, sem mudar o relógio/eventos do combate.

Katana `Leve1`: mapeamento exato de `Today_RequiemAttackKatanaYoungCardinal_0/1/2/3` para os quatro novos quadros de ataque. Punhos `Direto da Ruína`: `Ruin_Requiem_Normal_S_Combat_02/03/11` para startup/contato/recuperação. Nenhuma pose de soco substitui chutes, uppercut, slam ou outras técnicas.

## Preservação e rejeições

- Reconstrução exata de todos os pixels RGBA de alpha positivo dos quadros selecionados, incluindo alpha suave e partículas desconectadas. Contagem fonte/atlas idêntica. Nenhuma montagem de membros/armas, recoloração, quantização ou apagamento global de fundo.
- O aparente marrom da fonte Ruptura é RGB transparente. Os pixels distantes mais de 16 px dos grupos têm alpha máximo 1/255. Todos foram conservados. Não há fundo visível removido.
- As duas linhas Walk das fontes katana/punhos Normal foram rejeitadas por falta de alternância correta das pernas. As oito figuras continuam intactas nos originais, sem entrar nos atlas importáveis. A caminhada correspondente segue no banco anterior.
- Arte herdada, fontes aprovadas, saves e referências de outros projetos foram preservados.

## Evidências de QA

- `Tools/AnimationPreview/prepare_weapon_redesign.py`: localização de figuras inteiras, prova RGBA e empacotamento; escrita atômica dos arquivos finais.
- `outputs/redesign-armas-20261023/redesign-asset-qa.json`: **zero erros estruturais, 70 avisos**, distribuídos entre excesso de cores e alpha parcial nas seis fontes. Nenhuma certificação de pixel art indexada nativa; não corrigir esses avisos com quantização global.
- `coverage.json`: **zero erros** de fontes, hashes, referências, contexto duplicado e correspondência catálogo/folhas.
- `browser-review/results.json`: prévias de **16 clips e 64 quadros** carregadas, avanço manual e reprodução verificados, **zero erros JavaScript**. Os seis contatos completos e as seis fontes completas foram inspecionados visualmente. As prévias não certificam renderização Unity.
- Contatos, GIFs por clip, HTML interativo e paletas estão em `outputs/redesign-armas-20261023/<nome da folha>/`. As paletas registram cores reais e não alteram imagens.

## Pendências reais

Importação/renderização e gameplay no Unity; restante das direções de idle das adagas; ataques/locomoção das adagas; demais formas/direções/ações de katana e punhos; caminhada com pernas alternadas; skin Rock e demais contextos da matriz. Fontes Ruptura S adicionais estão na fila de revisão fora dos Assets durante a rodada Unity; ainda não fazem parte desta entrega estável.

Diagnóstico detalhado do bug anterior de idle, seletores, nomes e PPUs legados: `AUDITORIA-IDLE-ADAGAS-20261023.md`.
