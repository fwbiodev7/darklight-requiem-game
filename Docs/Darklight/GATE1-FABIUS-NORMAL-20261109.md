# Fabius Réquiem Normal — Gate 1 funcional encerrado em 09/11/2026

## Escopo e evidência atual

Conforme a definição do prompt `RETOMADA-MESTRE-20261109-2046.md`, concluída a recuperação/integração do escopo funcional de Fabius Normal nas três armas. O encerramento não declara QA completo da campanha, controle físico ou build Windows nova.

- Catálogo ativo: **280 sequências, 898 referências a quadros, 101 folhas**; total bruto393páginas. Recuperadas nesta retomada80sequências/288quadros/27folhas: Serra de Entrada e nove famílias Katana/Punhos, oito direções. Fontes, pixels, GUIDs, recortes, pivôs e tempos preservados.
- Charging usa antecipações inteiras revisadas e sustenta a última pose, sem repetir o contato do ataque. Entrada, hold, liberação curta/longa e cancelamento por esquiva verificados nos24contextos arma/direção.
- Golpes compatíveis de Punhos compartilham movimentos, mantendo atributos, trajetória aérea e contatos próprios. Órbita/Espiral→Giro; Sentença→Juízo; Queda→Martelo para o movimento descendente, com o sistema aéreo existente. Não são coreografias novas exclusivas.
- Moto: sul mantém montagem/desmontagem detalhada; sete direções reutilizam duas poses inteiras próprias de cada direção, com reversão e hold. Reuso explicitamente identificado no banco e recibo; não são sete animações desenhadas novas.
- Corrigida a carga desnecessária do catálogo de paletas legado em figuras inteiras revisadas. O defeito foi demonstrado em dois testes PlayMode, corrigido no consumidor e revalidado.
- Candidato concorrente de profundidade/alpha das partículas passou na validação isolada e fica incorporado ao escopo pertinente: ordem baseada no contato no chão, alpha local0.05, sem modificar material/arte do personagem.

## Testes aprovados

1. **72/72 EditMode**, zero ignorados/inconclusivos: `outputs/retomada-20261109/resume/editmode-2026-11-10T001718-937048+0000/tests.xml`. Inclui estados essenciais, carregamento, consumidor CharacterView, aliases, combate e transições Moto.
2. **11/11 PlayMode isolados e mudos**, após correção do catálogo legado: `outputs/retomada-20261109/resume/playmode-2026-11-10T002514-374971+0000/tests.xml`. Inclui as144figuras dos72estados, Serra32quadros/oito direções, Charging24contextos, caminhadas afetadas, fases/contato dos pesados, defesa Moto e profundidade/alpha das partículas.
3. **1/1 PlayMode gráfico isolado**, dispositivo D3D11 e câmera offscreen: `outputs/retomada-20261109/resume/playmode-2026-11-10T002713-874191+0000/tests.xml`. Faísca Moto: pixels renderizados, norte oculto, grade64, expiração e pool/mesh limitados. Não é composição da campanha inteira.

Não somar execuções anteriores falhas/interrompidas aos resultados aprovados. Os XML e monitores são novos; recibos históricos de03/11 não substituíram estes testes. O snapshot final mantém as fontes de teste e os hashes das recuperações.

## Autorizações e limites

O usuário autorizou nesta retomada PlayMode isolado, totalmente mudo, sem tocar saves/preferências; depois autorizou explicitamente usar a RAM necessária. A exceção do runner fica restrita aos filtros isolados de Fabius: reserva básica1GiB física/2GiB commit. Importações/builds gerais continuam2.5/3GiB. Comandos amplos/duplicados ou execução arbitrária não podem usar essa exceção.

Campanha longa, composição visual geral, equilíbrio, IA, teste físico de controle e build Windows permanecem nas etapas posteriores. A ordenação do mesh geral de impactos na cena será revista no Gothic Eclipse/QA de composição; não há falha crítica demonstrada por esse ponto na verificação isolada encerrada.

## Próxima etapa autorizada

**Iniciar Lucius Normal**, espada única e identidade visual própria, reaproveitando o suporte técnico compatível de Fabius. Referências locais: `ArtSources/LuciusDesign20261109/README.md`. Aparência encapuzada preserva o mistério até a revelação. Nexo Fenrir é sua única transformação futura; não ampliar armas/formas. Depois bosses → Gothic Eclipse → transformações → Combate IA2.0 → QA/build.
