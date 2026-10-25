# Presas do Caos / Ruptura — revisão de25/10/2026

Entrega segura desta subtarefa: **Réquiem/Ruptura/Presas1/S,SW,W,12 figuras completas em3 páginas**. Fragmento final: `outputs/redesign-armas-20261025/presas-ruptura-pages.json`. O catálogo compartilhado, importação Unity e validação runtime pertencem à coordenação principal; esta preparação não certifica gameplay ou conclusão do redesign.

Fonte final corretiva: `ArtSources/WeaponRedesign20261025/candidates/ReqDaggersRupturaPresas1S_SW_W20261025V4-source.png`. Atlas/manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/ReqDaggersRupturaPresas1S_SW_W20261025V4.{png,json}`. Job e prompt completo estão ao lado da fonte; reprodução por `Tools/AnimationPreview/prepare_presas_ruptura_20261025.py`. Ferramenta usada: **image_gen integrada** em todas as gerações/correções raster, nunca remoção global de cores/alpha por código.

## Correções humanas e limites da aprovação

O usuário aprovou o padrão mais intenso de **cor/mechas vermelhas** da v2 e pediu presença de vermelho na nuca. Depois esclareceu que o cabelo havia crescido indevidamente. Portanto a aprovação não abrange comprimento/volume, folha inteira, integração ou runtime. V4 usa a fonte Normal íntegra como autoridade de corte, silhueta, nuca curta e anatomia; mantém vermelho saturado nas mechas existentes, sem cabelo pendurado sobre as costas. O contato de escala de mundo foi inspecionado: porte e cabelo das três vistas finais ficam comparáveis a Presas Normal e katana canônica. Isso é revisão do agente, não nova aprovação do usuário.

A correção de alpha v2→v3 foi feita por image_gen com fonte inteira e pedido de background-extraction. Não se afirma igualdade pixel a pixel entre fontes geradas v2/v3/v4. Fontes originais foram preservadas; a garantia numérica de igualdade RGBA é **fonte final v4→atlas final**, sem montagem anatômica, espelhamento, deformação ou redimensionamento de pixels.

## Escala, poses e validação

Um único PPU **155.47460669411888** para a folha inteira. Guia: recuperaçãoS Normal, coroa y70→sola y357 =287px, PPU143.93939394, altura1.9938947unidades. RecuperaçãoS v4, coroa y46→sola y356 =310px, preserva essa mesma altura. Arma erguida, capa e efeitos não entram nessa calibração. Estimativa visual de limites anatômicos: ±6px; comparação em câmera Unity permanece pendente. Cabeça/face/cabelo e proporção tronco/pernas foram comparados no contato inteiro, sem normalizar pela bounding box da arma.

Contato em128pixels/unidade de mundo, calculado pelos PPUs reais sem auto-fit: `outputs/redesign-armas-20261025/presas-ruptura-scale/world-scale-contact.png`. Fonte inteira e contato dos12 quadros do atlas foram vistos. Auditoria: **0 erros/14 avisos** (cores por quadro, alpha parcial e altura aparente pela adaga levantada). Paleta, PNG/JSON, contato, HTML/GIF estão em `outputs/redesign-armas-20261023/ReqDaggersRupturaPresas1S_SW_W20261025V4/`. Prévia em movimento não foi inspecionada nesta subtarefa; não se afirma ausência de jitter em runtime.

As páginas mapeiam os nomes legados exatos de `Presas1`; nenhum timing/evento/colisão/save foi alterado. Duração da prévia30/45/38/38ms; startup29.75ms real continua no gameplay. Duas adagas físicas por figura, lâminas prateadas com núcleo/fissuras vermelhos e perfil de presas da referência. Olhos vermelho escuro e pele limpa preservados visualmente.

| Vista | Ataque Presas1 | Parada/Idle novo | Revisão espacial |
| --- | --- | --- | --- |
| S | v4 preparada,4 fases | Retido | Frente real,duas adagas |
| SW | v4 preparada,4 fases | Retido | Diagonal frontal esquerda |
| W | v4 preparada,4 fases | Retido | Perfil esquerdo,um olho visível |
| NW | Candidata retida | Retido | Diagonal traseira produzida; volume de cabelo requer correção |
| N | Candidata retida | Retido | Costas sem rosto produzidas; cabelo requer correção |
| NE | Candidata retida | Retido | Diagonal traseira produzida; cabelo requer correção |
| E | Candidata retida | Retido | Perfil direito produzido; cabelo requer correção |
| SE | Candidata retida | Retido | Diagonal frontal direita produzida; cabelo requer correção |

## Candidatas retidas fora da integração/publicação

**52 figuras atuais retidas:**20 de ataque nas cinco vistas restantes e32 de parada/idle nas oito vistas. Os lotes foram produzidos pela ferramenta integrada, mas o ajuste humano de volume capilar impede tratá-los como finais. Não gerar fragmentos de runtime nem publicá-los até corrigir figuras inteiras afetadas. Parada seria o primeiro quadro do idle real, não duplicação contabilizada como animação. AtaqueE/SE v1 tinha duas silhuetas encostadas e foi superado por v2. IdleE/SE v1 variava proporções e foi superado por v2, ainda retida pela revisão de cabelo. Versões frontais1–3 foram superadas por v4. Apenas v4 entra na entrega segura.

Fontes retidas: `ReqDaggersRupturaPresas1NW_N_NE20261025V1-source.png`, `ReqDaggersRupturaPresas1E_SE20261025V2-source.png`, `ReqDaggersRupturaIdleS_SW_W20261025V1-source.png`, `ReqDaggersRupturaIdleNW_N_NE20261025V1-source.png`, `ReqDaggersRupturaIdleE_SE20261025V2-source.png`, todas na pasta de candidatas de25/10. Não publicar por simples glob da pasta.

Próximo defeito concreto: reconstruir somente as figuras completas com volume/nuca indevidos nessas cinco fontes, seguindo o cabelo curto Normal e o padrão cromático vermelho aprovado; depois revisar8vistas, escala física e idle antes de registrar novas páginas. Rock e outros ataques/ações da mesma forma continuam pendentes.
