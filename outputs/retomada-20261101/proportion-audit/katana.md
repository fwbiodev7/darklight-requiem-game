# Auditoria cruzada — Katana Normal, 01/11/2026

**56 figuras novas examinadas**:32Leve3+24Dodge, contra8Idles aprovados da mesma direção. Leitura estática independente; nenhuma arte alterada. Fontes/atlas tiveram hashes conferidos; inventário completo e status por ID em `katana.json`.

## Cinco quadros com desproporção de pernas

A extensão da coxa+canela cresce em relação à cabeça e ao tronco, visível nas figuras completas nativas e em escalaUnity. Não é acusação por altura doBBox, agachamento, arma ou amplitude do canvas. Landmarks manuais de triagem têm incerteza20px e quadris parcialmente ocultos; não são reconstrução de ossos. PPU não corrige o desenho.

| Direção | Quadro exato | Perna/cabeça Idle→pose, aprox. | Evidência integral |
|---|---|---|---|
|W|`Redesign_ReqKatanaNormalDodgeS_SW_W20261101V1_07`|0.9→1.62|`katana-previews/W-Dodge-1-focus-world.png`|
|E|`Redesign_ReqKatanaNormalDodgeE_SE20261101V1_01`|1.07→1.92|`katana-previews/E-Dodge-1-focus-world.png`|
|SE|`Redesign_ReqKatanaNormalDodgeE_SE20261101V1_04`|0.95→1.86|`katana-previews/SE-Dodge-1-focus-world.png`|
|NW|`Redesign_ReqKatanaNormalDodgeNW_N20261101Selected_01`|1.19→1.83|`katana-previews/NW-Dodge-1-focus-world.png`|
|NE|`Redesign_ReqKatanaNormalDodgeNE20261101V5_01`|1.08→2.01|`katana-previews/NE-Dodge-1-focus-world.png`|

Fontes/hashes dos afetados:

- `ArtSources/WeaponRedesign20261101/katana-dodge/ReqKatanaNormalDodgeS_SW_W20261101V1-source.png` — SHA256 `e8eabe37e97e224689d52a4b54cf0d6a3259bdb5cd3106d49939cd15bf2af841`.

- `ArtSources/WeaponRedesign20261101/katana-dodge/ReqKatanaNormalDodgeE_SE20261101V1-source.png` — SHA256 `474020086461fb6c64243fa552fe7959730482df7eee0f2c5c910987d0a8868a`.

- `ArtSources/WeaponRedesign20261101/katana-dodge/ReqKatanaNormalDodgeNW_N_NE20261101V1-source.png` — SHA256 `67ea4960e6715dfb6217d7358da647d207d8a8f2c5193483b6a96ee40005c5c2`.

- `ArtSources/WeaponRedesign20261101/katana-dodge/ReqKatanaNormalDodgeNE20261101V5-source.png` — SHA256 `277c5cfda3da67a4501a01798eb7fa389d048d19b6cb57c594002ff3ad6eaa75`.

**NEV5 tem aprovação humana e permanece intacta.** O achado não revoga essa aprovação nem autoriza sobrescrever o original. Sua imagem está no catálogo, mas `.png.meta` ainda faltava no snapshot; PPU242.9292929 veio do descriptor, sem alegar importaçãoUnity.

## Figuras preserváveis e observações secundárias

- Leve3:32figuras conferidas, sem defeito seletivo de pernas confirmado. Preservar todas; posição da espada, flexão de joelhos, giro do tronco e altura menor não são defeitos por si. Roupa/katana/botas/identidade adulta permanecem coerentes.
- Dodge:18figuras sem desproporção de pernas confirmada,5afetadas acima,1limítrofe: `Redesign_ReqKatanaNormalDodgeS_SW_W20261101V1_04` (SW baixa). Nesta última, a articulação/quadril ocultos pedem confirmação específica antes de corrigir; comparar `katana-previews/SW-Dodge-1-focus-world.png`.
- Prontidão/retornoDodgeS/SW/W/E/SE mostram tendência de massa de cabeça/cabelo maior queIdle, mais clara emS0/S2. Alerta secundário de confiança média, não aprovação de um rosto infantil nem motivo para regenerar folhas inteiras. Comparar `katana-previews/S-Dodge-0-focus-world.png`; IDs correspondentes em `secondaryCautions` doJSON.
- DodgeS baixa eN baixa mantêm projeções encurtadas/solas giradas plausíveis; não as classifiquei porBBox. As demais regiões/equipamentos corretos dos quadros afetados devem ser preservados ao produzir futuros substitutos de figuras inteiras.

## Método e limites

Foram vistos os8contatos nativos por direção em `katana-previews/*-native.png`, os7painéis integrais do helper principal em `katana/*-worldscale.png` e os focos completos com régua. ProjeçãoUnity:escala comum1.325×80pxWorld/PPU no helper; focos1.325×160pxWorld/PPU. PPU das texturas importadas conferido nos `.meta`, excetoNEV5 declarada acima. Recortes/layout de prévia abrangem figuras completas, sem montagem ou redimensionamento anatômico; versões principais antigas que cortavam só o painel não foram tratadas como defeito do asset.

Nenhuma reprodução contínua, Unity nova, áudio, importação ou Git executados nesta auditoria. Resultado é inspeção estática, não certificação de anatomia oculta ou teste runtime. Correções futuras devem atingir somente os5quadros completos apontados após preservar fontes e aprovaçãoNEV5; não redimensionar partes nem esconder desproporção comPPU.

ParryPunhos permaneceu pausado:uma fonte9figuras S/SW/W foi apenas preservada, SHA256 `591c6741ea7825480dfce3e9afb5cb388122fa3e5bead218f6a5848bc8a2f944`; sem preparo, PPU, promoção ou importação.
