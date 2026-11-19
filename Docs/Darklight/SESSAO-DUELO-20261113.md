# Sessão de 13/11 — Fabius vs. Lucius

Base local main 425376f, com alterações locais anteriores. A validação se refere aos arquivos do checkout. Os 132 testes de 10/11 pertencem à versão anterior.

| Item | Estado da sessão |
| --- | --- |
| Auditoria | 152 entradas em 49 texturas. Nomes, retângulos, dimensões e identidade das fontes conferidos nos quatro catálogos. |
| Animações | Arconte, Encapuzado e Serafim receberam movimentos. A Ruptura continua usando a defesa de base. Direções restantes no relatório artístico. |
| Testes e build | Compilação C# separada com referências da Unity. O Editor encerrou antes de executar as suítes e gerar o player. |
| Inatividade | A partir de oito segundos sem ameaça, o idle de Fabius alterna vistas para olhar ao redor. A direção de combate permanece independente. O primeiro comando interrompe a variação. |
| Duelo | Fase 2 aos 60% de vida, transformação Arconte, fase final aos 25%, foco de câmera no parry e recuperação depois da terceira técnica. Após Nocthar, o histórico narrativo determina rendição ou morte. |
| Arena | Pátio gótico original, piso e colisões, grade em primeiro plano e brasas nos quatro braseiros. 1.148 células conectadas em verificação estática. |
| Feedback | Reprodução técnica e roteiro de jogabilidade abaixo. Avaliação visual e de controle em Windows pendente. |

## Prioridades e reprodução

1. Bloqueador de entrega. Ao iniciar o editor 6000.6.0f1 para EditMode ou para BuildPlayer nesta sessão, o cliente de licença excedeu 60 s e o serviço local BuildReportRestService encerrou com falha nativa no Mono. Esperado: XML dos testes e executável desta sessão. Observado: encerramento anterior a ambos. Logs em work/idle-13.log e work/validation-20261113/build.log. No PowerShell do Windows, a partir da raiz do projeto, executar `.\Tools\Delivery\validar-duelo-20261113.ps1`. O script roda os testes focados e gera a build em outputs/build-windows-20261113/Windows.
2. Abrir o executável gerado, entrar no treino e iniciar Fabius vs Lucius. Com históricos conciliador e hostil, jogar até o fim e repetir o X1. Confirmar o intervalo entre transformação e ataque, o parry e a janela após o terceiro golpe. Sair e conferir o save da campanha. O teste focal de campanha cobre o portal final em dois históricos com save temporário.
3. No treino com alvo passivo, aguardar 8 s e interromper o olhar com movimento, ataque e esquiva. Conferir pivôs e enquadramento na troca de direção. A lógica de inatividade passou em sete testes existentes via Mono e em cinco verificações específicas de gesto e interrupção.
4. Percorrer as bordas do pátio, inclusive com Lucius distante. Observar se a grade encobre ataques, se os braseiros interferem na leitura do parry e se os dois personagens permanecem em quadro. Conferir posicionamento e movimento das novas artes em tamanho de jogo.
