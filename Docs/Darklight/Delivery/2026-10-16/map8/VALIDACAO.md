# Fase 8 — cidade celestial corrompida

Integrada e validada individualmente em 16/10/2026 antes do avanço para a fase 9.

Quatro fontes originais em `chunks/`, sobreposição de 384×256 pixels lógicos, costura dura sem suavização. Base 1536×1024 e detalhe 3072×2048 dividido em quatro partes. Normalização somente NEAREST; a ampliação não inventa detalhe adicional. Backups da base, dados, GUIDs e detalhes anteriores em `backup/`.

Arquitetura própria: abrigo com tendas/arcadas, bastião de defesa, nave aberta da catedral, mercado rasgado pela corrupção, jardim suspenso, praça da fonte, chegada cerimonial e galeria oriental em ruínas. Pontes e escadarias conectam duas aproximações à catedral e retornos alternativos. Âncoras reposicionadas conjuntamente com objetivos, áreas, checkpoints e navegação; índices de missão/capítulo e narrativa preservados.

`map8-tests.xml` e `.summary.json`: **6 aprovados, 0 falhos, 0 ignorados; saves preservados e preferências restauradas**.

- 54 percursos físicos ida/volta, 23.185 passos de cápsula, raio 0,3; inclui junções, jardim, fonte e retorno oriental. Cinco estados de checkpoint carregados com ponto seguro, sem colidir com limite do cenário.
- Quatro detalhes Point, sem mipmaps e com 1536×1024 cada. Oito capturas reais da câmera/HUD em `after/`, todas inspecionadas.
- Resgate interativo leva ao checkpoint 1; três ondas reais avançam até o checkpoint 2. Evidência em `mission-evidence.json`.
- Encontro real único de Seraphine mantém as três fases, invulnerabilidade de transição e pausa.
- Contato das três armas originais em oito direções: 24 combinações na nova arena.
- Halo Partindo opcional reutiliza a atmosfera existente: duração limitada, pausa, interação e nenhum bloqueio de objetivos; teste silencioso confirma nenhuma reprodução de áudio. Camas/cues distintos dos outros mapas reutilizam recursos existentes, sem downloads.

Comparação real antes/depois em `comparacao-map8.png`; fontes anteriores em `../../mapas-reforma-20261015/before/` no workspace. As posições são comparadas por função/área da missão, não por decoração idêntica.

Limites: os percursos e combates de teste não equivalem a avaliação humana prolongada ou garantia de FPS. O formato dos saves não mudou; foram verificados os estados de checkpoint e preservação dos arquivos reais, não toda combinação histórica possível. Não houve nova build após este mapa ainda; a build 1.0.5 é anterior à reforma da fase 8. Gerar build após as fases restantes serem validadas.
