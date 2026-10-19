# QA offline dos controles e das fontes

Oito casos corrigidos passaram: Requiem e Rock, com adagas nas formas normal, vermelha, roxa e branca. No quadro-fonte 7, o segundo impacto agora usa somente a âncora da lâmina visível. A exclusão explícita removeu a âncora dos olhos antes da transferência para as outras formas. Os oito hashes das fontes conferem; nenhum PNG foi alterado e nenhuma arma oculta foi inventada.

A auditoria anterior conferiu 36 PNG, 731 componentes e 711 poses inteiras selecionadas, com hashes, dimensões, alpha e ownership preservados. Conferiu também 485 transferências de âncoras e a lógica de 32 sequências: 647 avanços de quadro, oito variantes do Cavaleiro com três eventos cada, 28 seleções de repouso, loop, pausa, câmera lenta e limite de 900 ms na transformação.

Após a correção, foram repetidos somente os oito casos afetados. Não há falha concreta aberta encontrada por esta auditoria. Resultados completos e hash do manifesto estão em [QA-CONTROLES.json](QA-CONTROLES.json).

## Limites reais

- Esta auditoria executou dados e lógica em memória. Não realizou revisão visual no navegador, Unity Play, reprodução de áudio ou teste de controle físico.
- Geometria e hashes não comprovam identidade, anatomia, qualidade de movimento ou aprovação visual.
- A seleção frontal não comprova cobertura das oito direções. As direções dependem das fontes realmente disponíveis na mesma forma.
- A transferência das âncoras coloridas usa a calibração manual do normal. A conta correta não garante coincidência perfeita com cada lâmina colorida.
