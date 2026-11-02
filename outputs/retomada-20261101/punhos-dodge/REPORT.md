# Punhos Normal — Dodge nas8direções, 01/11/2026

**Preparação artística atual:24 figuras inteiras,8 clips em3folhas, todos visual-reviewed.** A coordenação registra importação/runtime nos recibos principais; esta tarefa não declara conclusão global de Fabius/jogo nem aprovação humana de todas as folhas.

| Folha | Figuras/clips | Fonte | Atlas nativo | PPU corporal | Auditoria |
|---|---:|---|---|---:|---|
| S/SW/W V1 |9/3|1536×1024|2388×1314|163.6363636364|0erros/10avisos|
| NW/N/NE V1 |9/3|1536×1024|2304×1440|153.0303030303|0erros/10avisos|
| E/SE V1 |6/2|1536×1024|1698×1198|233.0808080808|0erros/7avisos|

Os9quadros S/SW/W aprovados foram preservados. Seus hashes de fonte/atlas abaixo foram reconferidos durante cada finalização; nenhum pixel mudou.

- NW/N/NE fonte SHA256 `a798da542f69862a87341a5ed468c59581bee704f3d68e3a549fb2b5b3cd1634`; atlas `824fe55244b3ea38e04590a7d78762eaf5a65e8f6f5ec479d53c9bd019a69027`.
- E/SE fonte SHA256 `05358a068b6edadb79f39beaf39f131901dacfd282cc929fb5f464dbe3a3b5bc`; atlas `1bc81059217daa2a79bd5dd8fbfb824007ede9e07349b487107678e3ee16e7e2`.
- `catalog-fragment.json`:8páginas agregadas. `catalog-fragment-back.json` e `catalog-fragment-right.json`:incrementais3+2páginas. `staging/` contém os3PNGs e respectivos manifestos. Prompts e fontes originais ficam em `ArtSources/WeaponRedesign20261101/punhos-dodge/`.
- Retângulos reais fonte→atlas→clip, pivôs, referências/hashes e prova de igualdadeRGBA exata registrados por figura. Sem espelhamento, montagem anatômica, redução, deformação, recoloração ou remoção de alpha.
-3fases por direção:40/150/40ms,total230ms,sem loop. Invulnerabilidade20–190ms pertence ao runtime e não foi testada por este agente. PPU usa cabelo–solas das recuperações/1,98unidades, incerteza4px; pivôs usam projeção das duas botas apoiadas, incerteza6px. Nenhuma escala/PPU justifica mudança anatômica.
- NW/NE apresentam vistas traseiras reais; N não mostra rosto/olhos e apresenta calcanhares, sem garras nos calcanhares. Cabelo curto castanho, roupa preta/vermelha, manoplas/grevas prateadas e identidade canônica conferidos nas figuras completas.
- E/SE foi gerado com joelho traseiro flexionado e esquiva compacta após o feedback sobre alongamento em outra arte. Comparativo inteiro enviado antes da promoção: `right-whole-identity-comparison-native.png`, sem redimensionar referência ou desenhos. `anatomy-screening-right.json` guarda landmarks aproximados de figuras completas, incerteza12px e quadris parcialmente ocultos. Coxa+canela projetadas/cabeça:Eidle1,26→baixo0,95;SEidle1,22→baixo0,94. Não se detectou haste alongada; flexão/escorço encurtam projeção. Isso é triagem visual, não prova do comprimento ósseo oculto idêntico.
- Fontes/contatos integrais nativos e2×nearest-neighbor claros inspecionados. Paletas, contatos, HTML e8GIFs gerados nas pastas por folha. **Movimento contínuo ainda não visto nesta tarefa**; não alegar playback humano, importação Unity ou teste de invulnerabilidade por este agente.
- `delivery-check.json`:PASS,24figuras/8direções exatas0–7,3frames por clip,230ms/nonloop,contextos/hashes/PPU/pivôs/RGBA conferidos. Auditorias:0erros/27avisos totais (24excessos de cores sobre256 +3alpha parcial). Alpha além16px tem máximo1/255 nas folhas frontal/traseira e2/255 emE/SE; nenhuma remoção global para esconder avisos. Resolução lógica indexada de pixel art não comprovada.
- Nenhum Assets/catalog compartilhado/Runtime/Unity/Git alterado. `visualApproval=false`;visual-reviewed registra inspeção estática do agente. `checkpoint.json` separa cobertura pronta dos estados de integração da coordenação.

Próximo passo:integrar os5clips novos e conferir Dodge/Idle, direção, movimento contínuo, escala/apoio, duração230ms e janela20–190ms na Unity. Nenhuma direção Dodge pendente na preparação; correções concretas do consumidor devem preservar as figuras corretas.

## Registro histórico do primeiro lote, antes da renovação

**Produzido/revisado:9 figuras inteiras,3 clips S/SW/W.** Integrado=0;runtime verificado=0 nesta tarefa. Nenhuma geração iniciada após o coordenador determinar parada com17%restantes. As5direções restantes não foram geradas.

- Fonte preservada1536×1024: `ArtSources/WeaponRedesign20261101/punhos-dodge/ReqFistsNormalDodgeS_SW_W20261101V1-source.png`, SHA256 `b62ba6bf8b4a31f8f7d3fa3bfedfd1ad2a84d30f8c1f0b5fbb9a850d0553d92a`.
- Atlas RGBA nativo2388×1314: `staging/ReqFistsNormalDodgeS_SW_W20261101V1.png`, SHA256 `6730abb57d135a48bab7cf3b9d003cf44978a589009d0c7e54c9070e89a939fc`.
- Manifesto vizinho `.json`:retângulos reais fonte→atlas, pivôs/âncoras inteiros de apoio, timings, referências/hashes e prova de reconstruçãoRGBA exata. Fonte e pixels de cada figura preservados integralmente; sem redução, espelhamento, deformação ou montagem de partes.
- `catalog-fragment.json`:somente3páginas Dodge,skin0/weapon2/form0,direções0/1/2,statusvisual-reviewed,loopfalse. Destino Resources é proposto; fonte/atlas ainda estão nos diretórios atribuídos.
-3fases por direção:antecipação40ms,investida evasiva baixa150ms,recuperação40ms,total230ms. A janela20–190ms é responsabilidade do runtime; não foi testada aqui.
- PPU163.6363636364, mediana das alturas cabelo–sola das recuperações/1,98unidades; incerteza4px. A fase baixa não define escala; nenhuma dimensão anatômica foi ajustada para caber. Pivôs usam projeção do meio das duas botas apoiadas anotada na fonte, incerteza6px. Contato no consumidor pendente.
- Referências integrais inspecionadas:Idle Punhos S/SW/W25/10 e Idle Katana S/SW/W25/10 para identidade/idade. Olhos20/10 já inspecionados na tarefa anterior. Referências traseiras e E/SE também foram inspecionadas para os próximos lotes, sem gerar.
- Revisão visual:nove figuras completas, cabelo curto castanho, olhos vermelho escuro/pele limpa, rosto e proporções canônicos, roupa preta/vermelha, corrente e manoplas/grevas prateadas. Antecipação/recuperação elevadas; fase central claramente mais baixa por flexão de quadris/joelhos e inclinação de tronco. Equipamento acompanha as mãos; sem aura/transformação, katana ou elementos externos.
- Auditoria executada:0erros/10avisos (9excessos de cores sobre256 + alpha parcial). Alpha além16px das silhuetas tem máximo1/255, sem fundo visível na inspeção clara; nenhum apagamento de cores/alpha. A resolução lógica indexada de pixel art não foi comprovada.
- Fonte/contato completo nativo e2×nearest-neighbor em fundo claro vistos. Contato escuro do pipeline visto. Paleta, contato, HTML e3GIFs gerados em `ReqFistsNormalDodgeS_SW_W20261101V1/`. **Movimento contínuo/Unity não vistos nesta tarefa**; não alegar playback humano ou teste de invulnerabilidade.
- Nenhum Assets/catalog compartilhado/Runtime/Unity/Git alterado. `visualApproval=false`;visual-reviewed registra inspeção do agente.

Próximos passos exatos e recibos: `checkpoint.json`, `visual-review.json`, `ReqFistsNormalDodgeS_SW_W20261101V1-prepared.json`. Coordenador deve integrar o lote revisado e conferir Dodge/Idle, movimento, escala/apoio, duração e janela de invulnerabilidade em Unity. Próxima sessão:9figuras NW/N/NE usando N-v2correto e6figuras E/SE; preservar as9atuais.
