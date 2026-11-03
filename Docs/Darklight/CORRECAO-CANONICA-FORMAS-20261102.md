# Correção canônica — quarta forma removida

Ordem direta do usuário em 02/11/2026: a forma Abissal não existe e deve ser removida. O trecho contrário no handoff da Antigravity está rejeitado.

- Fabius: Normal (0), Ruptura (1), Eclipse Absoluto (2).
- Lucius: Normal e, na etapa posterior, Nexo Fenrir, conforme seu escopo próprio.
- Transformações continuam depois das etapas prioritárias; não iniciar novas artes por causa desta correção.

Em `Assets/Darklight/Runtime/Character/DarklightFormController.cs`, removidos o membro `Abyssal`, sua cor e os multiplicadores próprios. `TransformTo` rejeita índices fora do escopo ativo, incluindo 3, antes de aplicar efeitos ou alterar estado.

Moto/Cavaleiro do Abismo permanece autorizado. Referências e arte históricas de índices antigos ficam preservadas para compatibilidade e não significam uma quarta forma ativa. A migração antiga para o escopo canônico não foi removida nem alterada.

Evidência: `outputs/retomada-20261102/remove-unauthorized-abyssal-form.json`. Verificação estática concluída; sem executar Unity ou suítes novas, conforme adiamento de testes. Não alegar validação runtime desta alteração.
