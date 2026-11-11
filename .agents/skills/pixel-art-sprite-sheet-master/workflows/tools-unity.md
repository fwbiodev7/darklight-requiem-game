# Editores e Unity 2D

## Aseprite / LibreSprite

Descubra executável/versão local. Preserve `.aseprite`/`.ase` com tags, layers e duração. Use `scripts/editor_export.py --editor <exe> --source <arquivo> --out <pasta-nova>`: executa batch sem UI e gera PNG/JSON-array sem trim/rotação. O helper testa flags em `--help` e interrompe quando incompatíveis. LibreSprite pode ter diferenças de versão; se faltar JSON-array, exporte PNG sem trim pela UI e anote retângulos inteiros com `grid`. Não alegar suporte de CLI testado se o editor não estiver instalado.

Exporte tags como ação+direção; normalize com `from-aseprite`. Exportações trimmed/rotated são recusadas pelo normalizador para não perder o quadro original. Reexporte sem essas opções ou use o importador existente que preserve sourceSize e offsets. Não converter automaticamente metadados arbitrários em uma grade.

## Unity

Antes de importar, leia a versão e os pacotes do projeto. Reutilize catálogos/importadores customizados antes de criar outro. `unity-json` converte o contrato desta skill para retângulos Unity bottom-left e pivôs normalizados bottom-left; esse JSON é um **contrato de integração**, Unity não o importa automaticamente.

- Texture Type Sprite (2D and UI); Point; mipmaps off; wrap Clamp; NPOT None; alpha preservado; compressão None ou equivalente comprovadamente sem perda; verificar também overrides de plataforma e limite máximo da textura.
- PPU coerente com tamanho anatômico aprovado e escala do mundo, igual nos estados equivalentes. Não fixar um PPU universal para todos os projetos.
- Sprite Mode Multiple ao usar slices; projetos com `Sprite.Create` podem usar folha única e catálogo próprio.
- Manter `.meta`, GUIDs, nomes e sprite IDs existentes. Para novos imports, usar Sprite Editor/Data Provider compatível com o pacote instalado; não reescrever YAML `.meta` para inventar IDs.
- Clips usam timings declarados; hold final para morte; loop intencional; eventos e colisões no frame/fase corretos. Não adicionar AnimationEvents que reproduzam áudio durante validação silenciosa.
- Pixel Perfect Camera/resolução de referência, escala inteira e filtragem coerentes. Testar câmera, movimento subpixel e render targets; Point sozinho não garante pixel-perfect em tela.
- Validar direção, skin, forma, transições, evento de contato e ausência de fallback oculto. Som e controle físico somente se realmente autorizados/testados.

`audit --unity-meta <png.meta>` faz inspeção textual básica das configurações; overrides, IDs, clips, shaders e execução real exigem validação no Editor. Relate explicitamente esse limite.

Fontes técnicas primárias (verificar versão instalada quando necessário):
- [Aseprite CLI](https://www.aseprite.org/docs/cli/)
- [LibreSprite](https://github.com/LibreSprite/LibreSprite)
- [Unity TextureImporter](https://docs.unity3d.com/ScriptReference/TextureImporter.html)
- [Unity Sprite Data Provider](https://docs.unity3d.com/Packages/com.unity.2d.sprite@1.0/manual/DataProvider.html)
- [Pillow](https://pillow.readthedocs.io/en/stable/)
