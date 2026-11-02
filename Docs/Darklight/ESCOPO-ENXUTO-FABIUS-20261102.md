# Fabius Normal — escopo reduzido por ordem de 02/11/2026

Este ajuste supera as filas antigas de técnicas e variações. O usuário pediu reduzir a quantidade para concluir Fabius, aproveitando o que já foi produzido. Não abrir novas técnicas nem transformações. Os nomes e mecanismos de combos existentes podem compartilhar os mesmos golpes-base; não exigem um lote gráfico por nome.

## Corte aplicado

| Fila antiga | Decisão atual |
| --- | --- |
| Mais quatro técnicas gráficas da Katana | Dispensadas. Silêncio e Crepúsculo usam Leve3; Ruptura Precisa usa Pesado; Eclipse Partido usa Carregado. |
| Mais quatro técnicas gráficas dos Punhos | Dispensadas. Queda usa Martelo, Espiral/Órbita usam Giro, Sentença usa Juízo. |
| Cinco sequências aéreas novas por arma | Dispensadas como lotes próprios. Golpes aéreos usam figuras inteiras já prontas; perseguição é deslocamento sem contato, sem golpe inventado. |
| Dezoito técnicas restantes da Moto, cada uma com oito clips próprios | Fila cancelada. Golpes das serras compartilham Serra de Entrada/Dentes Cruzados. Pilotagem reaproveita a apresentação montada existente; impacto no chão é uma família, não uma sequência nova para cada nome. |
| Doze estados/variações próprios por arma | Reduzidos a defesa, dano e morte. Carregamento segura a antecipação já desenhada. Corrida e pequenas transições reaproveitam caminhada/idle quando coerentes. Sem novos lotes cosméticos de idles. |

A reutilização de ataques foi implementada explicitamente em `DarklightWeaponRedesignBank.AttackAnimationKey`, mantendo os tempos e eventos de cada comando. Não é remoção de combos nem mudança de dano. A tela pode continuar apresentando os nomes de variação existentes; eles não geram mais pendências gráficas individuais.

Código e artes desta produção estão implementados localmente. Publicação de runtime/artes continua condicionada aos gates finais; um commit de checkpoint documental não certifica nem publica essa implementação.

Carregamento preserva um clip próprio, caso exista; caso contrário usa a preparação completa revisada de Carregado/Juízo/Dentes. Não há montagem de membros, alteração de rosto, espelhamento nem recoloração da skin.

## Lista curta para fechar Fabius

1. Finalizar somente caminhadas realmente faltantes: Punhos SW e serras a pé da Moto nas oito direções, com passada baixa e alternância comprovada. Katana Walk já cobre oito direções.
2. Concluir defesa, dano e morte. Reusar figuras completas entre contextos somente quando roupa, equipamento, direção e pose forem compatíveis.
3. Resolver apenas falhas comprovadas na família de impacto/pilotagem da Moto. Não gerar os antigos dezoito lotes nem recriar transições aprovadas.
4. Validar as famílias compartilhadas, proporções, transições e oito direções. Os testes permanecem adiados até o final da produção, conforme a última ordem do usuário.

Fabius não será declarado completo por retirar itens da planilha. Caminhada incorreta e estados essenciais ausentes continuam pendências reais. Todas as artes corretas e fontes históricas ficam preservadas; as filas antigas são históricas, não tarefas a reiniciar.

Depois de fechar esta lista: Gothic Eclipse nos mapas existentes, Lucius e demais etapas do plano mestre. Não antecipá-las durante o fechamento de Fabius.
