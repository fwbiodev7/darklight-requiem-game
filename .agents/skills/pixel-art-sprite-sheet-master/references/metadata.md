# Contrato e helpers

Execute `python scripts/sprite_pipeline.py --help` e o `--help` do subcomando necessário. Pillow é a única dependência externa. Todos os arquivos de saída são novos; diretórios de entrega não vazios são recusados para evitar mistura de revisões.

```json
{
  "schemaVersion": 1,
  "image": "hero.png",
  "imageSha256": "SHA256 real do PNG",
  "nativeSize": [128, 64],
  "pixelsPerUnit": 32,
  "status": "candidate",
  "frames": [
    {"id": "walk_S_00", "rect": [0, 0, 32, 64], "pivot": [0.5, 0.125], "durationMs": 100, "action": "walk", "direction": "S", "skin": "default", "form": "normal"}
  ],
  "clips": [{"name": "walk_S", "frames": ["walk_S_00"], "loop": true, "allowStatic": false}],
  "requiredClips": ["walk_S"],
  "quality": {"alphaThreshold": 0, "maxColors": 64, "maxHeightRatio": 1.3}
}
```

`rect` = **x,y,width,height** com origem superior esquerda, nunca x0,y0,x1,y1. `pivot` = normalizado bottom-left dentro do quadro completo. `durationMs` é positivo. Nome único estável por frame/clip. `nativeSize` é tamanho do PNG entregue; resolução lógica diferente precisa de campo adicional explícito e revisão. `image` relativo ao JSON. Hash obrigatório para rastrear a fonte. `quality` é um contrato configurável, não garantia artística. `baseline` opcional aponta para imagem aprovada e SHA-256 (caminho relativo ao manifesto) que será verificado sem modificá-la.

- `grid --image hero.png --cell 32 64 --columns 4 --count 8 --out hero.json`: somente fontes com grade conferida. Gera um clip `sequence`; revise nomes/ações/direções/pivôs e decomponha tags reais antes de usar no jogo.
- `from-aseprite --input hero-export.json --ppu 32 --out hero.json`: normaliza frames e tags do editor, incluindo reverse/pingpong.
- `palette --image hero.png --out palette.json`: cores RGBA e frequências completas; resumo limitado em stdout, sem modificação da fonte.
- `audit --manifest hero.json --out auditoria [--unity-meta hero.png.meta]`: bounds, hash, sobreposição, alpha, duplicações, conteúdo estático em clip, durações, pivôs, escala aparente e cobertura declarada. Erro estrutural/arquivo inválido retorna 2; warnings exigem revisão e aparecem no relatório.
- `preview --manifest hero.json --out previa --zoom 3`: contato PNG, HTML interativo autocontido e GIF por clip. Não certifica movimento percebido; examinar visualmente.
- `pack --manifest hero.json --out atlas --columns 4`: reorganiza retângulos de quadros completos em canvas comum, com pivôs em coordenadas inteiras. Sem cortar transparência/efeitos, sem redimensionar desenho e sem alterar fonte. Mantém provenance e clips.
- `unity-json --manifest hero.json --out hero-unity.json`: dimensões, PPU, frames bottom-left, pivôs e sequências com tempos. Adaptar ao importador existente; não instala componente Unity.

Correções localizadas: `python scripts/replace_frames.py --manifest hero.json --plan plano.json --out corrigidos`.

```json
{"wholeFramesConfirmed": true, "replacements": [{"frameId": "walk_S_00", "image": "sprite-completo-corrigido.png"}]}
```

Cada entrada é o **quadro inteiro**, incluindo cabeça/corpo/arma, com o mesmo canvas ou múltiplo inteiro uniforme. O script recusa retângulos sobrepostos, redimensionamento desigual e fonte cortada na borda; salva prova de igualdade RGBA de todos os quadros não selecionados. Sua prova é técnica: comparar visualmente referência dos olhos, pose, roupa e anatomia antes de integrar.

Outputs de `audit`: `report.json` (evidências por frame), `REPORT.md` (conclusão técnica, alertas, limites). Prévia e auditoria usam pastas diferentes. Não gerar relatório de aprovação automaticamente.
