# DARKLIGHT: REQUIEM — retorno e retomada de 03/11/2026

## GDD

O plano integral enviado pelo usuário foi preservado em `Docs/Darklight/Planejamento/PROLOGO-CINEMATOGRAFICO-20261103.md` e indexado no GDD. Estado: **planejado, não implementado**. Ordem atual: Fabius Normal → Gothic Eclipse → Lucius Normal → prólogo cinematográfico → transformações → Combate IA 2.0. Não iniciar o prólogo durante a finalização de Fabius.

## Pacote recebido do Antigravity

Conferência de integridade desta retomada: 16 folhas / 72 clips / 144 figuras de Parry, Hurt e Dead; 48 arquivos de atlas, descritores e fontes com SHA-256 iguais ao manifesto, zero divergências. Isso comprova preservação, não gameplay ou perfeição anatômica automática. Evidência: `outputs/retomada-20261103/antigravity-return-integrity.json`.

Totais preservados: 52 folhas / 167 clips / 492 figuras copiados para Assets; apenas 33 folhas / 89 clips com recibos históricos de importação. Permanecem 19 folhas pendentes: 16 de estados essenciais e 3 de caminhada Moto. Não alterar flags para imported/runtime-verified sem recibo novo.

As faíscas de defesa Moto têm código e âncoras preparados; compilação relatada pelo Antigravity e ausência de `error CS` no log consultado não comprovam contato, oclusão, frequência de emissão ou funcionamento em gameplay. As três formas de Fabius continuam Normal/Ruptura/Eclipse Absoluto; nenhuma quarta forma foi reintroduzida.

## Importação pequena — resultado real

Uma única tentativa nova, com **uma folha e três figuras**, em `outputs/retomada-20261103/single-sheet-request.json`. A disponibilidade inicial permitia o início pelo guarda existente. Mesmo assim, a Unity ainda processava o refresh de áudios anteriores quando o guarda encerrou a execução por RAM.

- Início: 2026-11-03T21:20:48Z; fim: 21:21:48Z.
- `stopReason: Stopped before system RAM exhaustion`.
- RAM disponível na parada: 2.44 GiB; piso preservado: 2.50 GiB.
- Commit disponível na parada: 7.69 GiB; piso preservado: 3.00 GiB.
- Nenhum recibo novo de importação; nenhuma build ou suíte de gameplay executada.
- Monitor: `outputs/retomada-20261103/single-sheet-import-memory.json`.
- Log: `work/retomada-20261102/packet-import-182048/import-reviewed.log`.

**Diagnóstico útil:** reduzir `--request` não limita o refresh inicial da Unity. Não afirmar que basta liberar commit acima de 3.5 GiB: esta tentativa parou por RAM, com commit sobrando. O último MP3 observado já usa Streaming/preload desativado. Não mudar seus metadados aleatoriamente, apagar o cache aquecido, rebaixar guardas, alterar pagefile ou encerrar apps do usuário. Não repetir a mesma abertura sem melhora mensurável de RAM ou diagnóstico técnico novo.

## Caminhadas — produção seletiva

Dois subagentes de arte trabalharam somente em pastas próprias, sem Unity/Git/Assets. Candidatos novos não foram promovidos. Punhos SW e Moto W/E continuam pendentes; não declarar Fabius finalizado.

Achado: a referência KatanaSW aprovada também repete o mesmo apoio. Continua válida como autoridade de identidade/escala, mas **não é autoridade para alternância de passada**. A caminhada Punhos E V11 oferece contraste mecânico dos contatos A/B. Não regenerar a Katana aprovada por esse achado sem delimitar um defeito e seu escopo.

Fontes, hashes, contatos, prévias e defeitos das tentativas estão em:

- `outputs/retomada-20261103/fists-sw-resume/`
- `outputs/retomada-20261103/moto-we-resume/`
- `ArtSources/WeaponRedesign20261103/fists-sw-resume/`
- `ArtSources/WeaponRedesign20261103/moto-we-resume/`

Esses diretórios são **candidatos bloqueados**, não uma extensão do staging aprovado. Não reutilizar duplicação de apoio, espelhamento, montagem de pernas, resampling ou apenas variação de PPU para simular uma caminhada correta.

## Próximo trabalho técnico do Antigravity

1. Antes de escrever, conferir controle de responsabilidade e ausência de Unity/Git/integrador concorrente.
2. Investigar o custo do refresh inicial: separar Unity principal, worker de importação e pressão de outros processos. Registrar picos de RAM/commit por processo e fase; preservar cache aquecido e dados do usuário.
3. Somente com folga real de memória ou correção comprovada, importar o pedido pequeno existente com os guardas atuais. Se parar novamente, preservar evidência e bloquear; não repetir em ciclo.
4. Importar as demais folhas pendentes apenas após sucesso, preservando GUIDs, retângulos variáveis, pivôs, PPU e pixels. Não reintegrar folhas já importadas.
5. Após fechamento artístico e importação, validar gameplay: estados essenciais, retorno/hold de morte, famílias compartilhadas, faíscas Moto (um disparo por entrada, contato das serras e oclusão), transições e escala. Não confundir testes Python com EditMode/PlayMode.
6. Fabius validado precede Gothic e Lucius; manter prólogo como planejamento futuro no GDD. Não antecipar transformações ou build pesada.

Publicação anterior confirmada: `852e612cd38d12845c2bc0134e9c1af4fb610d26`, somente fontes revisadas/handoff. Não usar esse hash como prova de publicação de runtime ou gameplay atualizado. Publicação continua restrita à cópia `work/publicacao-20261021`, branch `integracao-completa-20261026`.
