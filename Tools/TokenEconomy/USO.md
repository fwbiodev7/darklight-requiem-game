# Saída compacta de comandos

Adaptação local da ideia de guardar saída bruta e expor somente evidências úteis. Sem Distill instalado, API, modelo local, troca do modelo do Codex ou alegação de 99% de economia. O [Distill consultado em 20/10/2026](https://github.com/samuelfaj/distill) descreve atualmente um harness/TUI mais amplo; este helper independente não é integração com ele.

```powershell
& <python-existente> Tools/TokenEconomy/run_compact.py --label auditoria-sprites -- <python-existente> <script-de-auditoria>
```

O diretório padrão de logs é `../work/compact/` relativo à raiz do projeto. `--log-dir` muda o destino quando necessário e autorizado. Execução usa argv sem shell: para comandos PowerShell, passar `powershell.exe -NoProfile -File <script.ps1>`; nunca compor texto do usuário para execução. `--timeout` limita duração; timeout retorna 124. `--xml` lê NUnit/JUnit recém-gerado e recusa resultado antigo. XML de falha produz status de falha mesmo com exit code 0. Exit code do comando é preservado.

O resumo inclui código real, duração, hash/tamanho/caminho do log, contagem de trechos com sinais e exemplos limitados com contexto final de falha. Mensagens extensas são truncadas **somente no resumo**, com log bruto preservado. As contagens são de trechos de log, não de testes ou erros únicos. Ausência de palavras de erro não prova sucesso; exit code e relatório estruturado são a autoridade. Para diagnóstico, ler somente o trecho relevante do log.

Para busca/inventário, reduzir já na origem (`rg` por caminho/padrão, contagens e campos JSON). Não executar comando desnecessário para depois resumir. Para validação, preferir resumo nativo do script. Não reler o log completo nem repetir testes sem novas alterações/falhas.

Teste de volume artificial mede **bytes**, não tokens faturados. Somente relatar economia de tokens após contar entrada/saída com tokenizador e modelo reais, incluindo o custo de qualquer resumidor. Nunca atribuir redução de 99% ao trabalho inteiro com base em um exemplo sintético.
