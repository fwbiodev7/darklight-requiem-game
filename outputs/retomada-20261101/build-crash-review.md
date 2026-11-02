# Build crash review — 2026-11-01

## Evidência observada

- `release-progress.json`: import concluído (exit 0); Edit Mode **144/144** e Play Mode **28/28** passaram. Windows build terminou com exit `2147483651` (hex `0x80000003`) e `buildSucceeded=false`; não houve standalone nem pacote ZIP.
- A exceção nativa está em `work/retomada-20261101/release-195201/build.log` e no relatório espelhado em `%LOCALAPPDATA%/Temp/Unity/Editor/Crashes/Crash_2026-11-01_230046262/build.log`. Build começou 19:58:44 e crashou às 20:00:46 (-03).
- Stack: `MimallocPrimErrorHandler → _mi_error_message → _mi_malloc_generic → MimallocAllocator::Allocate → MemoryManager::Allocate → malloc_internal → core::vector<signed char>::reserve_impl → SharedTextureData → Texture2D::CreateSharedTextureDataWhenReading → Texture2D::Transfer<StreamedBinaryRead> → SerializedFile::ReadObject → PersistentManager::ReadAndActivateObjectThreaded → BuildPlayer`. Entrada do build gerado em `DarklightWeaponRedesignBuild.cs:213`.
- O log lista repetidos avisos `Evict-before-extend: reclaimed cold content for ...-byte alloc` imediatamente antes de `Crash!!!`; a API e o stack identificam a fase de leitura/desserialização de `Texture2D`, mas não dão nome do asset nem motivo textual preciso da falha do allocator. Portanto, falha ao alocar/erro de allocator é evidência; OOM/asset corrompido são hipóteses não confirmadas.
- O relatório registra Windows 11, RAM física 16,176 MB, Unity `6000.6.0f1` / `6000.6.0.63725`, processo batch x64. `release-progress.json` mediu 10,819,047,424 bytes livres antes da build; espaço em disco não demonstra disponibilidade de RAM/commit no instante do crash.
- Comparação: `work/retomada-20261101/release-094232/build.log` terminou `Build Successful` / `Build Finished, Result: Success` na mesma instalação Unity; build incremental avaliou 524 itens, com `0 items updated`. Também continha avisos `Evict-before-extend`. Assim, esses avisos isolados não distinguem o crash atual. A build anterior é evidência de sucesso anterior, não valida a build atual (conteúdo/estado podem diferir).

## Retomada segura sugerida

1. Fazer no máximo uma tentativa controlada após encerrar processos Unity remanescentes da tentativa crashada e iniciar um processo Unity novo, mantendo `6000.6.0f1`, mesmos inputs/opções e build target. Não alterar versões nem boot allocator params nesta primeira tentativa.
2. Antes da tentativa, registrar RAM disponível/commit do Windows e garantir que não há outra Unity/build. Preservar projeto e caches; não limpar `Library` por padrão.
3. Se repetir, guardar o novo log e crash report e comparar a posição da falha. Só então isolar assets de textura em lotes, guiado por evidência. Evitar mexer em Burst, plugins nativos ou backend gráfico: os frames disponíveis não os mostram como causa.

## Limites

Não foi inspecionado o dump binário. Não foi inferida causa-raiz definitiva a partir do nome `MimallocPrimErrorHandler`; o erro alocado exato não aparece no log. Nenhum processo Unity foi iniciado.
