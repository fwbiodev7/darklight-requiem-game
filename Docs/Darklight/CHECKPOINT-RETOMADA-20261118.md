# Retomada de 18/11/2026

**Última direção: usuário suspendeu as novas artes/guardas por custo/tempo e mandou pular para a próxima etapa do projeto.** Manter recursos novos somente FabiusNormal(form0), sem expandir transformações. Preservar selecionadas/candidatas reais e pacote parcial/inativo; não declarar artes concluídas. Próxima etapa autorizada: bossfightLucius2.0, sem novos lotes artísticos, testes, build ou push. Não executar a continuação de quatro direções descrita historicamente abaixo.

**Continuação explicitamente escolhida pelo usuário após o checkpoint:** completar as artes e a integração, em vez de gerar build. Saldo na retomada dessa escolha:22%janela/74%semanal. Escopo fechado em corrigirW/E, produzirNW/SE, fechar16clips nas8direções e configurar o banco. A pausa30 foi liberada apenas para essa continuação; sem novos testes, Unity/build/push. FrontalS e demais selecionadas permanecem intactas.

**PAUSA PARA AVALIAÇÃO ATINGIDA:** leitura30%restantes da janela e76%semanal. Novos lotes suspensos; aguardar decisão explícita do usuário para continuar ou gerar build. Sem build/testes/push. Relatório: `Docs/Darklight/RELATORIO-PARCIAL-20261118-30.md`.

## Ordem vigente

Continuar o trabalho de17/11 desde o commit local `d704ddbe778cf53db1ce84176d831454054513b2`, na branch `codex/menus-combat-20261117`. Preservar as três modificações concorrentes antigas em configuração/caches, sem reset.

- **Pausar novos lotes aos30% restantes da janela disponível**, mostrar concluídos/pendências e aguardar o usuário decidir continuação ou build. Este limiar substitui os15% de ontem. Ferramenta apresenta janela de300min, não um contador diário separado; usar esse saldo real e informar o limite da medição.
- Manter pelo menos20% semanal. Saldo inicial medido:100% janela e87% semanal.
- Sem novos testes, Unity/Play, smoke tests, build ou push automáticos. Avaliação manual dos menus permanece com o usuário. Revisão de arte/código e preparo da integração não comprovam runtime.
- Map Remaster2.0 não começa antes de19/11 às21:11 Brasília e da fase autorizada.

## Arte: lotes pareados e fidelidade

Nova ordem: duas direções por folha, optando por16quadros(8por direção:4Idle e4Walk), mantendo a fluidez do piloto. Pares N/S,NE/SW,E/W,SE/NW. Reutilizar S aprovado integralmente em vez de redesenhá-lo. SW individual iniciado antes da mudança é preservado, revisado e reutilizado no par quando adequado, sem desperdício.

Autoridade: `ArtSources/FistsCombatStance20261117/selected/FabiusFistsCombatS-selected-v1.png`, SHA256 `202f33f42d937ab55cc5cc2162b41c8d1881ea3ea329e8d18e04c895de70740e`. WalkS explícita e visualmente aprovado pelo usuário, RGBA preservado. Idle com pulinhos de preparação, guarda alta, joelhos soltos, troca de apoio, lift contido e chão/pivô constantes. Não regenerar os PNG/pivôs antigos de Fabius Normal, não redesenhar o personagem, não infantilizar, não montar anatomia nem espelhar assimetrias.

Fontes novas em `ArtSources/FistsCombatStance20261118/`. Metadados devem registrar dimensões e retângulos reais, sem presumir que a geração respeitou a grade solicitada. Empacotamento só de figuras completas, preservando pixels; PPU serve para resolução/escala física, não para consertar proporções.

## Responsabilidades

- `lucius_art_review`: produção/revisão das folhas pareadas e manifestos, apenas ArtSources18.
- `essential_recovery`: banco aditivo de posturas/gerador de metadados/Resources novos e hooks mínimos em WeaponRedesignBank; sem editar fontes/catálogo/pivôs antigos.
- Coordenação: integração final, continuidade de Combate2.0, controle de cota e checkpoint. Nenhuma instância Unity será iniciada para testes.

Override de Idle/Walk só poderá ficar disponível quando houver oito direções/16clips completos e configurados. Antes disso, fallback integral atual. Estados produzido, revisado, import-configured, importado e verificado permanecem distintos.

### Ajuste técnico do lote após medir a saída real

A primeira tentativa de16figuras devolveu1086×1448 em vez de1536×2048, com corpoN~289px contra386px no S aprovado (~25%menos densidade). Esta saída foi preservada como candidata, sem chamar qualidade equivalente.

Foi adotada a outra opção expressamente permitida pelo usuário: **10figuras por folha/2direções×5**, layout5×2, com2Idle e3Walk por direção. Playback da caminhada pode usar contato esquerdo→passagem→contato direito→passagem, reutilizando a figura inteira de passagem sem mirror/deformação. Idle mantém guarda/pulinho desenhado e ritmo curto. S permanece com seus8quadros originais, sem redução; resolução/grade reais sempre registradas.

Pares novos: N/NE, W/E, NW/SE; SW individual iniciado antes da mudança será reaproveitado após reparo de apenas2figuras completas (pulinho e contato oposto), preservando6corretas. É uma exceção de reparo localizado, não volta a gerar direções individualmente. O banco aceita contagens variáveis e referências repetidas a sprites inteiros na timeline; não exige64spritesúnicos artificialmente.

## Compatibilidade aérea de Lucius — implementação18/11, sem testes

Parte do Combate2.0, não encerramento da bossfight2.0: janela curta de lançamento após quebra de postura/parry; resistência fora da abertura, teto de juggle e imunidade de aterrissagem existentes. Lucius fica sem ataque/movimento terrestre durante voo e usa offset visual separado do Rigidbody. Queda ao morrer continua atualizando a gravidade. Fontes/pivôs/armas/HP/dano e os3golpes canônicos preservados. Mudanças apenas revisadas por leitura; nenhuma execução Unity/Play/testes/build.
