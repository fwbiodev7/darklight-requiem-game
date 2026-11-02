# Fabius — auditoria incremental de 01/11/2026

Escopo: leitura de AGENTS.md, contrato ESCOPO-CANONICO-20261026.md, retomada30/10, recibos31/10, catálogo e consumidores atuais. Sem Unity, Git ou alteração de Assets. Inspeção visual nova das referências traseiras completas e da candidata Leve3 traseira.

Ordem mais recente informada pelo usuário e transmitida pela coordenação: **Fabius Normal → Lucius Normal → integração da atmosfera → transformações de ambos por último**. Essa prioridade supera a ordem histórica do contrato; não gerar transformações agora.

## Evidência efetiva

- `outputs/retomada-20261031/art-integration.json`: 7 folhas,76 figuras,20 clips, `unityImported=true` e `runtimeVerified=true`. Gates históricos em `release-progress.json`:137/137 EditMode e25/25 PlayMode. Não foram repetidos nesta auditoria; esse recibo ainda deixa build em andamento.
- Catálogo atual confirma Katana Leve3 exata somente S/SW/W/E/SE:20 figuras,5 clips. NW/N/NE estão ausentes. Giro Devastador/Punhos:24 figuras,8 direções exatas. Preservar esses44 quadros integrados.
- Katana Normal: Idle8 direções; Walk S/SW/W/NW/N; Leve1 e Leve2 nas8. Leve3 nas5 acima. Punhos Normal: Idle, Direto da Ruína, Chute Quebra-Osso, Ascensão da Ruína e Giro Devastador nas8 direções. Nenhuma Walk própria.
- Não há páginas próprias de Charging/Dodge/Parry/Hurt/Dead nesses contextos. `DarklightWeaponRedesignBank.TryCurrentPose` usa fallback Walk/Idle; isso mantém desenho atual mas não constitui animação dessas ações. Ataques ausentes usam o primeiro golpe da mesma arma/direção.
- Ruptura: Katana apenas S Idle/Attack; Punhos S Idle/Attack e SW/W Attack. Oito direções completas não estão prontas.
- Nenhuma página `fusedEclipse=true`. Páginas históricas form2 são excluídas pelo consumidor; forma ativa fundida usa fallback Normal. Não contar como redesign Eclipse Absoluto.
- Moto Normal no catálogo principal: Dentes Cruzados SW/W/NW/N/NE. Banco próprio da moto exige auditoria separada; S/E/SE atribuídas ao agente Moto.

## Defeito localizado e autoridade visual

Fonte candidata rejeitada: `ArtSources/WeaponRedesign20261028/katana-normal/ReqKatanaNormalLeve3NW_N_NE20261028Selected-source.png`, SHA256 `227380522a8ff97164d73e09c66a1ae5a5cdbae30829a2eaa6809c90b0dbf726`. Inspeção confirma resíduos cinza semitransparentes externos em torno de espada, cabelo, braços e botas; grande mancha flutuante em NW contato. Fundo escuro do contato pode ocultá-los. Não promover a folha nem remover alpha globalmente.

Referência traseira inspecionada: `ArtSources/WeaponRedesign20261027/katana-normal/ReqKatanaNormalWalkNW_N20261027Selected-source.png`, SHA256 `27cedecb25443d27c729a1a87f10b9196f44c2acd9759257d447f4e0d7c06452`. Preserva corte castanho, identidade, roupa preta/vermelha e katana na mão direita. Referências laterais corretas existentes: `ReqKatanaNormalLeve3S_SW_W20261028Selected-source.png` SHA256 `05a79e4db8b489877b9cad53ca87d0a7b5ebdbd0bb7b8bceed4b83acd23807d5`; `ReqKatanaNormalLeve3E_SE20261028Selected-source.png` SHA256 `1b85c791eb859ab95b5db82abd1dfd00fd87738fd6426643c25b0e7cfe7eaac8` na pasta katana-normal de28/10.

## Timings e menor ação segura

`Assets/Darklight/Data/Attacks/Leve3.asset`:80ms preparação,100ms contato,180ms recuperação. As quatro figuras correspondem40/40/100/180ms; páginas usam startupFrames2/activeFrames1. O consumidor normaliza pela fase real de AttackDefinition. Giro:110/120/200ms conforme `DarklightRuinFistsProfile.cs`;3 figuras.

Corrigir apenas o lote traseiro Leve3 (12 figuras completas/3 direções), mantendo as20 figuras integradas intactas; produzir candidato versionado, registro real de quadros, pivôs pelo corpo, contato/previews e revisão antes da integração coordenada. Uma geração de correção via ferramenta integrada foi salva em `ArtSources/WeaponRedesign20261101/katana-normal/ReqKatanaNormalLeve3NW_N_NE20261101V1-source.png`, SHA256 `4026c67640515f6583e49a004876038a6dd3ed14d82b3c828f15b2214777be15`. Estado inicial: candidato,12 figuras presentes e sem a mancha flutuante anterior na inspeção integral; extração, QA e revisão de movimento ainda pendentes.

Leve3 traseira produzida/revisada: atlas `outputs/retomada-20261101/katana/staged/ReqKatanaNormalLeve3NW_N_NE20261101V1.png`, SHA256 `db7629b55237ad494e21dd75cf03d3551256b18ae5036b49cb4e771402cd1c33`.12 figuras/3 clips; fonte1717×916,atlas2240×984; PPU139.393939 pelo corpo de seis guardas/recuperações sem espada erguida. RGBA reconstruído exatamente,512350 pixels não transparentes preservados;98 pixels a mais de16px da silhueta têm apenas alpha1. QA0 erros/16 avisos: paleta ampla,alpha parcial e altura de poses agachadas. Contatos completos vistos em fundos claro/escuro e ampliação2×; tiras de fases dos três GIFs vistas e durações40/40/100/180ms conferidas. Cabelo,roupa,arma,maturidade e anatomia coerentes com referência traseira; coordenação confirmou identidade também.

Playback interativo não visto: CUA visible:true rejeitado com `IAB visibility is not supported in a subagent thread`; visible:false em localhost retornou `net::ERR_CONNECTION_TIMED_OUT`. HTTP no shell confirmou socket proibido pelas permissões de acesso. Não houve contorno; servidor sessão45353 encerrado via Ctrl-C. Estado `visual-reviewed` refere-se à inspeção estática e sequencial; `motionPreviewViewed=false`, Unity/importação/runtime pendentes da coordenação. Nenhum bloqueio visual residual específico encontrado nas12 figuras desta revisão.

Limites: presença em catálogo não comprova revisão anatômica desta auditoria; recibo histórico não é teste novo. Nenhuma campanha humana, controle físico ou avaliação sonora alegada.

## Caminhada Katana — continuação delimitada

Após entregar Leve3, coordenação atribuiu Walk NE/E/SE,limitada a duas tentativas.24 figuras completas geradas em duas folhas;ambas rejeitadas por não inverterem a perna de apoio nos contatos opostos. V1 também contaminou NE/SE com as direçõesNW/SW da referência de movimento;V2 corrigiu as câmeras,mas manteve o defeito de passo. Nenhuma Walk promovida. Evidência,hashes e próximo defeito específico em `outputs/retomada-20261101/katana-walk/CHECKPOINT.md`. Não iniciar terceira tentativa equivalente,transformações ou mudanças em artes corretas para encobrir essa lacuna.
