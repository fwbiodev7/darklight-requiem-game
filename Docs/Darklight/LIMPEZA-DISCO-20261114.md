# Lista de limpeza — 14/11/2026

**Nenhum arquivo foi excluído nesta sessão.** Espaço livre medido no início: **8,16 GiB**. A tentativa de remover a duplicata confirmada foi bloqueada antes da execução (`blocked by policy`); não foi tentado outro meio de exclusão.

| Item | Espaço aproximado | Critério |
|---|---:|---|
| `outputs/DarklightRequiem-Windows-20261111/` | **1,90 GiB** | Duplicata confirmada: todos os **199/199 arquivos têm tamanho e SHA-256 idênticos** aos de `outputs/build-windows-20261111/Windows/`. Pode excluir esta cópia mantendo a pasta de build indicada. |
| `outputs/build-windows-test-20261113/` | **1,88 GiB** | Candidata a descarte **após conferir que não precisa mais da build específica de teste**. Não é duplicata exata: há cinco diferenças, incluindo `data.unity3d` e `Darklight.Runtime.dll`; não removida automaticamente. |
| `outputs/build-windows-20261111/` | **1,90 GiB** | Só considerar após a nova build consolidada ser validada e decidir que não precisa mais dessa versão histórica. Se excluir as duas cópias de 11/11, não haverá backup dessa versão nessas pastas. |

Não somar itens condicionais como espaço já liberado. A única duplicata comprovada liberaria aproximadamente 1,90 GiB; nenhuma liberação foi medida porque a política rejeitou a exclusão.

## Preservar

- `Assets`, `ArtSources`, `Docs`, fontes originais, 28 PNGs recuperados, três folhas oficiais, metadados/GUIDs e alterações não commitadas.
- Saves e preferências reais; outros projetos e arquivos pessoais.
- Build funcional de estabilidade de 10/11 e build principal de 13/11 até concluir a substituição validada.
- `Library/DataStore` (**10,69 GiB**) e `Library/PlayerDataCache` (**1,81 GiB**) nesta rodada: são caches de importação/build em uso, não material comprovadamente inútil. Apagá-los exige reconstrução e prejudica a retomada; não foram usados como atalho de limpeza.
- Capturas, XMLs e recibos desta sessão: evidência da integração, não lixo genérico.

Levantamento: `outputs` 26,92 GiB; `work` 3,04 GiB. `Temp` do projeto estava vazio; logs tinham tamanho irrelevante. Encontrados dois dumps Unity antigos de cerca de 1,5 MiB cada — ganho pequeno, não removidos.

Prova detalhada da comparação: `work/disk-cleanup-20261114/duplicate-build-analysis.json`. O relatório contém caminhos, contagens, bytes e as diferenças reais das builds. Nenhuma ACL, configuração de segurança ou restrição do executor foi alterada.
