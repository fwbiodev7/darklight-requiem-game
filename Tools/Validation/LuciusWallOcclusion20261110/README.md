# Teste de parede de Lucius — candidato preservado,10/11/2026

Uma adição à fixture existente comprovará obstrução real, remoção da parede no mesmo AttackSerial e ausência de dano duplo. A fixture completa está em staged-tests somente como fonte revisável; não entra na compilação Unity.

O runtime atual já consulta DarklightNavigation.Clear. O relatório antigo atribuía cobertura de parede a HitboxRespectsRangeAndAngle, mas esse caso só testa alcance e ângulo. Foram corrigidos os nomes reais dos11testes na documentação, sem reexecutar seus resultados históricos.

Duas tentativas isoladas da adição pararam no guarda de RAM durante inicialização/reload. A última usou a autorização de memória somente para esse nome exato, piso1.5GiB RAM/2GiB commit; mínimo livre1.461GiB. Nenhum XML novo, zero casos certificados. Não promover a adição como testada nem copiar esta fixture sobre versão nova sem comparar a base.

Alteração de whitelist e fixture ativa no snapshot são candidatas locais, fora deste commit. Código runtime, arte, identidades e dados de jogador não foram alterados. A próxima tentativa deve conferir novas entregas e recursos reais; não repetir a matriz ou testes de Fabius.
