# Katana Normal Walk NE/E/SE — 01/11/2026

Estado: **não resolvido**. Duas gerações de12 figuras inteiras,24 candidatas no total,ambas rejeitadas. Nenhum atlas,catálogo,Assets,Runtime,Unity ou Git alterado por este agente. Não continuar com terceira tentativa equivalente.

- V1: referência SW/W contaminou direções; NE virouNW eSE virouSW. E ainda repete apoio. Fonte `ArtSources/WeaponRedesign20261101/katana-walk/ReqKatanaNormalWalkNE_E_SE20261101V1-source.png`, SHA256 `2c4cf2b3a30ef6101060da3e6255ec4dc88f06e0b7ac0e6916b1002e31e10865`;1448×1086,12 figuras completas,sem bordas opacas cortadas.
- V2: câmeras NE/E/SE corrigidas; contatos opostos ainda não invertem pernas. Fonte `ArtSources/WeaponRedesign20261101/katana-walk/ReqKatanaNormalWalkNE_E_SE20261101V2-source.png`, SHA256 `0249eb6c2fa265f13bf2a7df1bbefca3237585a40c2279570460730abfcc5ef6`;1536×1024,12 figuras completas,alpha real,nenhum resíduo externo com alpha≥32 além16px. Fonte inteira composta em branco inspecionada; aparência de fundo escuro na saída da ferramenta não constitui fundo opaco real.

Defeito concreto: E4/6 eSE8/10 mantêm a mesma perna próxima atrás; NE0/2 também repetem o mesmo contato. Passagens distintas não substituem contato contrário. Detalhes em `review-v1.json` e `review-v2.json`; prompts e referências preservados em ArtSources. Não promover ou publicar essas fontes rejeitadas.

Próximo passo: obter desenho inteiro com quadril/joelho/boot da perna próxima inequivocamente para frente no contato contrário, mantendo cabeça/corpo/katana juntos e a identidade canônica. Não espelhar,rotacionar ou montar membros. Depois completar4 fases×3direções,400ms porloop,pivôs manuais e revisão sequencial/playback. Caminhadas atuais corretas S/SW/W/NW/N permanecem preservadas.

Prioridade vigente: FabiusNormal → LuciusNormal → atmosfera → transformações de ambos por último. Leve3 traseira foi entregue separadamente em `../katana/leve3-rear-receipt.json`;12 figuras/3clips visual-reviewed,importação e teste pertencem à coordenação. Nenhuma reivindicação de trabalho global concluído.
