# Fabius — estados essenciais e QA de 03/11/2026

## Resultado novo

**7 testes PlayMode isolados aprovados, 0 falhas.** XML: `outputs/retomada-20261102/integration-play-190423.xml`; recibo: `outputs/retomada-20261103/essential-states-qa-final.json`.

Cobertura: os 15 clips iniciais, mais 72 clips / 144 figuras de Parry, Hurt e Dead para Katana, Punhos e Moto em oito direções. Cada fase nativa foi percorrida; retângulos, pivôs, sprite selecionado, cor sem tingimento, escala 1.325, retorno ao neutro e hold do último quadro de morte foram verificados com CharacterView e CombatModel reais. A morte avança StateTime sem avançar o relógio de cooldown de combate.

O despacho das faíscas Moto passou nas oito direções: transformação do contato normalizado para mundo, oclusão do contato N e uma emissão por entrada em defesa, com nova emissão na próxima defesa. A apresentação de preview suprime partículas intencionalmente; o teste chama o dispatcher real separadamente. **A malha visível das faíscas na campanha ainda não foi validada.** Esse teste de despacho não comprova renderização nem aprovação gráfica.

Permanecem 167 clips / 492 figuras / 52 folhas ativas importados. Os 87 clips cobertos agora têm evidência isolada atual; isso não aprova a campanha, os demais clips, IA, balanceamento, controle físico ou build.

## Correções da preparação dos testes

A primeira execução ampliada revelou que o fixture antigo tinha somente três perfis convencionais; indexar Moto como quarto perfil produzia IndexOutOfRange. O fixture novo usa a fábrica real `DarklightAbyssRider.Initialize(null)` e mantém campanha/cena ausentes. Outra falha de preparação tentava observar efeitos numa apresentação que propositalmente não os cria; foi separada a checagem de despacho da checagem futura de renderização.

As duas falhas são preservadas nos XMLs `integration-play-185949.xml` e `integration-play-190150.xml`; não foram falhas comprovadas de arte. O XML final aprovado prevalece para o código atual.

## RAM — nova autorização explícita do usuário

O usuário autorizou: “pode utilizar quanto de ram precisar nao ligo e nem uso”. Após duas interrupções do QA pelo piso anterior, foi adicionada opção explícita `--isolated-ram-reserve-gib 1.5`. A exceção só aceita o filtro isolado November, com reserva mínima de 1.5 GiB. Importações, campanha e build mantêm o padrão 2.5 GiB; commit continua em 3 GiB, disco em 1 GiB. Três tentativas de usar a exceção fora do escopo foram recusadas antes de iniciar processos.

Comando aprovado:

```powershell
python Tools/Delivery/validate_15_clips_20261102.py --skip-import --isolated --headless-tests --isolated-ram-reserve-gib 1.5
```

Nenhum app do usuário foi encerrado e nenhuma preferência/pagefile foi alterada. Uma trava temporária vazia de execução interrompida foi removida somente com Unity ausente e acesso exclusivo; recibo `stale-unity-lock-recovery.json`.

## Arte pendente e próximo gate

- Punhos Walk SW: contato B e passagem B naturais e inequívocos.
- Moto Walk W/E: contatos realmente alternados com câmera do tronco constante.
- Validação gráfica da malha das faíscas no jogo real e gate final de Fabius.

Preservar as expressões de dano aprovadas, proporções adultas compactas, cabelo/pele/olhos e os 24 quadros reempacotados sem perdas. Não aprovar candidatos por passar contrato técnico. Não iniciar novos golpes, transformações, Gothic ou Lucius antes do gate atual.

Gothic e Lucius continuam após Fabius. O prólogo registrado no GDD fica após Lucius Normal e antes das transformações/Combate IA 2.0. Nenhuma build nova foi criada.
