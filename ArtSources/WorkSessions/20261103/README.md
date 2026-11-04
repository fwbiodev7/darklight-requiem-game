# Trabalho preservado — 03/11/2026

Esta pasta preserva fontes, candidatas, versões rejeitadas, scripts, auditorias,
relatórios, prévias e recibos produzidos nesta sessão. **Não é um pacote de assets
aprovados e não é carregada pelo jogo.** A tentativa MotoParry1 infantilizada,
PunhosW com apoio incorreto e demais rejeitadas permanecem apenas como histórico.
Nunca promover o arquivo inteiro para Assets.

`manifest.json` relaciona cada caminho original ao SHA256, tamanho e conteúdo
armazenado. Duplicatas apontam para o mesmo objeto ou um arquivo exato já presente
no repositório. Textos grandes usam gzip sem perda. Nenhuma imagem foi recomprimida
ou redimensionada neste arquivo de preservação.

Validação portátil, a partir da raiz do repositório:

```powershell
python Tools/Delivery/archive_session_20261103.py --verify
```

Para recuperar um arquivo: localize `originalPath` no manifesto, leia `storedPath`
relativo à raiz do repositório, descomprima se `encoding` for `gzip` e confira
`sha256` e `originalBytes`. O conteúdo recuperado corresponde aos bytes originais.
Os caminhos locais absolutos dentro de documentos são proveniência histórica;
use os caminhos relativos do manifesto para localizar o material portátil.

O relatório legível atual está em `Docs/Darklight/FECHAMENTO-20261103.md`.
CONTROLE-LOCAL e RETOMADA arquivados são snapshots anteriores ao commit/push;
declarações antigas de "sem commit" descrevem o instante do snapshot.

Resumo de estado:23folhas/83clips/188frames aplicados localmente; importação Unity
e runtime não certificados. Correções de anatomia/cabelo recentes são candidatas,
não aplicadas. Gothic Eclipse recebeu primeiro incremento de4arquivos, com sintaxe
verificada, sem compilação/avaliação Unity. V3 lateral Moto permanece preservada.

Ficaram fora do Git apenas caches de clone/Unity/Python, listas e logs redundantes
de transporte Git, saves, música, builds e material alheio ao projeto. Não há
limpeza destrutiva do workspace. O índice e a verificação cobrem os arquivos
selecionados de trabalho; não presumem disponibilidade de conteúdo não publicado
do computador de casa.
