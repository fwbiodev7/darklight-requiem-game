# Moto Absoluto — preparo de 25/10/2026

Responsável exclusivo: `/root/moto_absoluto`. Fonte produzida pelo coordenador com a ferramenta integrada de imagens; este agente conferiu a fonte inteira, o pôster e o Normal aprovado, empacotou figuras completas e revisou o contato. Sem alteração de consumer/catálogos compartilhados, Unity, Play, áudio ou commit por este agente.

## Fonte, identidade e escala

- Fonte preservada: `ArtSources/WeaponRedesign20261025/candidates/RockMotorcycleAbsolutoRide8SlamS-v1-source.png`, SHA-256 `fe64f3803a433bf44696c7bfb3616b13bd5f230f8af83fd1cb06919defd4c0c7`, 1536×1024 RGBA. Prompt irmão `-v1-prompt.txt` preservado. Referência íntegra: painel Absoluto de `ArtSources/WeaponRedesign20261025/references/fabius-transformations-motorcycle.png`.
- Fabius/Rock mantém o rosto jovem canônico, corpo e roupas Rock: colete sem mangas, camiseta de caveira, corrente, calça e botas. Cabelo prateado/branco, moto com placas físicas prata/brancas e fissuras vermelhas. Olhos vermelho escuro e pele quente limpa, sem filtro da transformação. Não foi aplicada recoloração, montagem anatômica, espelhamento ou alteração dos pixels da fonte.
- Guias específicos em `ArtSources/WeaponRedesign20261025/candidates/RockMotorcycleAbsolutoRide8SlamS-v1-guides.json`. PPU **125.2525253**, por corpo em pé de cerca de 248px/1.98u (coroa y742, sola y990), excluindo a moto erguida e os braços. Medição aproximada ±6px pela sobreposição parcial da máquina na coroa; escala no jogo continua pendente. Normal usa 247px/1.98u.
- Comparação de quatro figuras completas vista: `outputs/redesign-armas-20261025/moto-absoluto/scale-compare-normal-absoluto.png`, com evidência JSON e script `Tools/AnimationPreview/compare_motorcycle_absoluto_scale_20261025.py`. Ambas as formas foram renderizadas a **100 pixels por unidade de mundo**, ancoradas no apoio, sem ajuste para caber no quadro. Fonte/atlas originais ficaram intactos. Cabeça/rosto, tronco e pernas têm proporções comparáveis; não foi observada infantilização ou redução corporal do Absoluto nesse contato. O volume extra do cabelo prateado não foi usado para encolher o corpo. Isso não certifica todas as poses em cena.

## Oito vistas conferidas

| Índice | Direção | Verificação física |
|---|---|---|
| 0 | S — frente | Rosto, guidão e carenagem frontais; a roda traseira fica naturalmente ocluída. |
| 1 | SW — frente/esquerda | Frente e lateral esquerda em três quartos; duas rodas em profundidade. |
| 2 | W — esquerda | Perfil esquerdo com duas rodas em linha horizontal, motor e escape legíveis. |
| 3 | NW — costas/esquerda | Costas do colete, nuca prateada e traseira da moto; orientação para cima/esquerda. |
| 4 | N — costas | Nuca e costas reais, sem rosto frontal; conjunto traseiro alinhado. |
| 5 | NE — costas/direita | Costas e lateral direita, guidão para cima/direita, mechas prateadas na nuca. |
| 6 | E — direita | Perfil direito próprio, duas rodas e braço oposto visíveis. |
| 7 | SE — frente/direita | Frente e lateral direita em três quartos; carenagem e rosto orientados para baixo/direita. |

Pares opostos possuem desenhos distintos e não foram espelhados tecnicamente. Corrente, motor, mãos e escape foram conferidos como partes das figuras inteiras; continuidade mecânica tridimensional exata entre lados não é certificada pelo contato estático. Oito vistas estáticas não equivalem a oito ciclos animados.

## Pacote e limites

- Atlas/manifesto: `Assets/Darklight/Resources/Darklight/WeaponRedesign20261023/RockMotoForm3Ride8SlamS20261025V2.{png,json}`, 2640×1257. Fonte íntegra rastreada em `ArtSources/WeaponRedesign20261023/RockMotoForm3-RockMotorcycleAbsolutoRide8SlamS-v1-source.png`, sem sobrescrever fontes antigas. Doze figuras com **reconstrução RGBA exata**, oito poses montadas estáticas e quatro fases de Esmagamento Vertical terrestre S. As fases preservam 232/348/160/580ms; a escala não usa altura da moto acima da cabeça.
- Auditoria: **0 erros / 14 avisos**: doze alertas de cores acima da paleta declarada, um de altura aparente com a moto erguida e um de alpha parcial. Fonte tem 784475 pixels alpha zero, doze componentes inteiros sem tocar borda opaca e margem direita estreita de dois pixels na recuperação. Sem quantização ou limpeza automática; resolução lógica de pixel art não certificada.
- Contato do atlas inspecionado: `outputs/redesign-armas-20261023/RockMotoForm3Ride8SlamS20261025V2/preview/contact.png`. Paleta, nove GIFs e HTML gerados no mesmo pacote. GIF/HTML **não vistos em movimento**; Unity/importação/gameplay pendentes. Ver a ordem dos quatro desenhos não comprova a execução no jogo.
- Faltam **parada/repouso e idle montado**, demais golpes, transições, drift, aéreo, finalização e outras direções do slam. As poses montadas atuais são usadas somente no avanço ativo pelo consumer preparado. Réquiem/moto continua N/A. Nenhuma aprovação visual global do usuário foi inferida.

Próxima conferência concreta após importação coordenada: apoio e proporção ao alternar condução ativa → recuperação antiga, especialmente o apoio da recuperação S com a moto inclinada. Depois completar parada e idle em subtarefa própria, mantendo a mesma escala corporal.
