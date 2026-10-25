# Moto Eclipse — preparo de 25/10/2026

Responsável: `/root/moto_eclipse`. **Fechamento corrigido: a v2 de cabelo forte foi retirada por aumentar o volume; o atlas ativo voltou à fonte inteira v1.** Aprovação de cor não autoriza aumentar largura/comprimento do cabelo. Nenhuma nova geração nesta correção.

## Fonte ativa e auditoria

- Fonte ativa: `ArtSources/WeaponRedesign20261025/candidates/RockMotorcycleEclipseRide8SlamS-v1-source.png`, SHA-256 `ec921bbbbe2936ee23f45858bc10872d16a032015e20bd5aef670ca5bff2f29c`, 1536×1024 RGBA. Cópia versionada: `ArtSources/WeaponRedesign20261023/RockMotoForm2-RockMotorcycleEclipseRide8SlamS-v1-source.png`.
- Atlas/manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/RockMotoForm2Ride8SlamS20261025V2.{png,json}`. V2 é somente o identificador de recurso já existente; o manifesto aponta para a fonte v1 acima.
- Doze figuras completas e RGBA exato: oito vistas montadas estáticas e quatro fases do Esmagamento Vertical terrestre S. Sem montagem anatômica, espelhamento ou recoloração por script. Fontes e pôster integral inspecionados.
- Guias ativos: `RockMotorcycleEclipseRide8SlamS-v1-guides.json`; PPU124.7474747 pelo piloto em pé (~247px/1.98u), excluindo moto erguida. Escala em cena pendente.
- QA refeita após reversão: **0 erros/14 avisos** de cores, alpha parcial e altura aparente com moto erguida. Sem limpeza/quantização automática; resolução lógica de pixel art não certificada.

## Retirada da v2 por volume

Comparação numérica na imagem inteira, alpha>128, em faixas que abrangem cabelo: Normal/Rock S x129–239 (111px), v2 S x126–244 (119px); Normal/Rock N x126–233 (108px), v2 N x119–239 (121px). Faixas S y15–94 e N y345–424. A inspeção integral confirmou aumento lateral e na nuca. As faixas são evidência comparativa, não uma máscara semântica perfeita de cabelo.

A v2/prompt e QA retirada ficam apenas como histórico local: `outputs/redesign-armas-20261025/moto-eclipse/rejected-hair-volume-v2-qa/`. Não publicar v2 nem auxiliares. Reforçar violeta mantendo **o mesmo volume curto da Normal** permanece pendente; a v1 tem cor mais discreta e não conclui esse ajuste.

## Oito vistas conferidas

| Direção | Observação |
|---|---|
| S | Frente real, rosto e carenagem; roda traseira ocluída naturalmente. |
| SW | Frente/esquerda em três quartos, duas rodas em profundidade. |
| W | Perfil esquerdo, motor e escape legíveis. |
| NW | Costas/esquerda em três quartos, colete e nuca visíveis. |
| N | Costas verdadeiras, sem rosto frontal. |
| NE | Costas/direita em três quartos, nuca visível. |
| E | Perfil direito com desenho próprio. |
| SE | Frente/direita em três quartos. |

Pares opostos são distintos, sem espelhamento técnico. Corrente/motor/escape conferidos na figura inteira; continuidade mecânica tridimensional exata não certificada. Contato v1 inspecionado; prévias refeitas em `outputs/redesign-armas-20261023/RockMotoForm2Ride8SlamS20261025V2/preview/`. Movimento HTML/GIF não inspecionado. Nenhuma importação Unity, Play, áudio, gameplay ou commit por este agente.

## Lacunas

Faltam parada/repouso e ciclo idle montado, outros golpes, drift, aéreo, finalizador, transições e outras direções do slam. Consumidor usa as poses estáticas somente em condução ativa; oito vistas não são oito ciclos idle. Próxima revisão: escala/pivô na troca condução ativa→recuperação antiga e cabelo violeta forte sem crescer.