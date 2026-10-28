# Punhos Normal e moto Normal — preparo de 25/10/2026

Estado desta entrega: fontes completas empacotadas, descritores e consumidores preparados. Importação Unity, validação em gameplay e nova build dependem da consolidação principal. Nenhum Play ou áudio foi iniciado neste preparo.

## Punhos — Réquiem/Normal

- Fonte v2 preservada: `ArtSources/WeaponRedesign20261024/candidates/ReqFistsNormalDireto_S_SW_W-v2-source.png`, SHA-256 `569470770dc7793b22cc4f3aeb7dded4ea102a8ce647d07338fb7ce1a599f7d6`.
- Atlas e manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqFistsNormalDiretoSSWW20261025V2.{png,json}`. Nove figuras completas, S/SW/W, `Combat_02/03/11`: startup/contato/recuperação de **Direto da Ruína**, 75/65/130 ms. PPU 169.6969697; suportes inteiros provenientes da inspeção anterior. Nenhum redimensionamento anatômico.
- Isolamento por componentes inteiros; reconstrução RGBA exata, incluindo alpha parcial. A grade uniforme não foi usada para cortar a fonte. Fonte integral permanece preservada.
- Fragmento com as três páginas para o catálogo compartilhado: `outputs/redesign-armas-20261025/punhos-moto/catalog-fragment.json`. A página frontal substitui a anterior apenas neste contexto Normal; SW/W são cobertura adicional.
- Auditoria estrutural: **0 erros, 11 avisos**. Avisos: cores acima da paleta declarada, alpha parcial e variação de altura aparente em SW. Não apagar cores/alpha automaticamente. Contato e GIFs: `outputs/redesign-armas-20261023/ReqFistsNormalDiretoSSWW20261025V2/preview/`.
- Não contempla Rock, outras formas, outros golpes/combos, outras cinco direções ou locomoção.

## Falha do teste legado dos Punhos

O resultado de 24/10 registrou falha em `AuthoredFistPunchesUseIndependentWholeFiguresAndClearEveryLegacyPalette`, antes de qualquer verificação de paleta. O catálogo compartilhado contém `attackKey` com mojibake `Direto da RuÃ­na` (U+00C3 U+00AD), enquanto o profile C# e a fonte `import-jobs.json` contêm `Direto da Ruína` (U+00ED). O comparador exato rejeita todas as quatro páginas. Corrigir **as chaves do catálogo**, preservando a comparação estrita e o teste. Consolidar JSON com UTF-8 explícito; não relaxar garantias nem trocar ataque narrativo para contornar a falha.

## Moto — Rock/Normal

- Fonte aprovada v2 preservada: `RockMotorcycleNormalRedesign_Ride8_SlamS-v2-source.png`, SHA-256 `a1473da99dd43904dc27b834b0242a5b8d93a5506a610ec622dd88a882c95b29`.
- Atlas/manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/RockMotoForm0Ride8SlamS20261025V2.{png,json}`. Oito vistas montadas **estáticas**, sem espelhamento, e quatro fases do Esmagamento Vertical frontal.
- PPU único 124.7474747: corpo do piloto em pé no quadro 9, cabelo y742 até sola y989, 247px/1.98 unidades. A altura da moto erguida não define escala anatômica. Conferência de escala em cena ainda pendente.
- Auditoria estrutural: **0 erros, 14 avisos** de cores, alpha parcial e variação de altura com máquina erguida. RGBA exato e figuras inteiras preservados.
- `DarklightMotorcycleRedesignBank` seleciona as vistas montadas somente durante a fase ativa de condução; as quatro fases frontais substituem somente o Esmagamento Vertical terrestre S. Drift, finalizador, aéreo, outras direções de slam, caminhada e transições continuam nos consumidores anteriores. Mecânica e timings permanecem intactos.
- `DarklightMotorcycleRedesignImporter` chama `DarklightWeaponRedesignImporter.ImportStandalone` para cada forma realmente presente. O coordenador deve expor este método, ajustar o bloqueio de espelhamento em `CharacterView` para sprites do novo banco e chamar `ApplyOwnership` após a apresentação antiga.
- Dois testes de edição verificam oito vistas distintas, quatro fases, ausência de empréstimo frontal e limpeza das máscaras/paletas antigas. Ainda não executados neste preparo.

## Contrato para os próximos agentes de forma

`Tools/AnimationPreview/prepare_punhos_moto_20261025.py --only motorcycle --motorcycle-source <fonte> --motorcycle-form 1 --motorcycle-guides <guias.json>` empacota uma forma sem tocar o catálogo compartilhado. Use índices1/2/3 para Ruptura/Eclipse/Absoluto.

O JSON de guias exige `supportAnchors`: lista de12 pares inteiros `[x,y]`, na ordem S/SW/W/NW/N/NE/E/SE/antecipação/elevação/contato/recuperação, e `pixelsPerUnit`: escala baseada no corpo inteiro do piloto em pé. Medir a própria fonte inspecionada; nunca copiar os suportes da forma Normal sem conferir. Campos adicionais documentando a medição são preservados no manifesto. Fonte deve conter12 figuras completas separadas (4colunas × 3linhas); o isolamento usa componentes reais, não limites uniformes de células. Preservar pele e olhos; não usar paleta/material para simular desenho novo.

O script mantém o fragmento dos Punhos e acumula os registros de outras formas em `prepared.json` e `preparation-jobs.json`. Ele não importa Unity, não conclui runtime e não altera a cobertura global.
