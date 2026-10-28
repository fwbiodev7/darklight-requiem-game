# PROPOSTA DE PORTABILIDADE PARA SUITE DE TESTES — 26/10/2026
**DARKLIGHT: REQUIEM — Etapa E (Antigravity)**

## Objetivo
Apresentar a solucao tecnica para neutralizar os **29 caminhos absolutos hardcoded** identificados em scripts de teste, garantindo que a suite de testes Unity possa ser executada em qualquer maquina de desenvolvedor, ambiente de CI/CD ou clone local sem falhas por diretorios inexistentes.

---

## 1. O PROBLEMA IDENTIFICADO

Atualmente, 29 scripts contem constantes e literais de string apontando para o diretorio privado da maquina de desenvolvimento do Codex:
`C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/...`

### 1.1 Riscos Concretos
1. **Excecoes de I/O**: Testes que tentam gravar evidências falham ao tentar chamar `Directory.CreateDirectory` ou `File.WriteAllBytes` em caminhos inexistentes ou restritos por permissoes de sistema.
2. **Incompatibilidade Cross-Platform**: Esses caminhos quebram qualquer execucao em ambiente Linux (comum em servidores de build / GitHub Actions).
3. **Falso-Negativo em Testes Automatizados**: Testes de PlayMode e EditMode com logica de assercao valida podem ser reportados como reprovados unicamente pelo erro de gravacao de disco.

---

## 2. SOLUCAO PADRONIZADA E PORTAVEL

Recomenda-se a adocao de uma classe utilitaria estatica simples em `Assets/Darklight/Tests/`:
`DarklightTestOutput.cs`

```csharp
using System.IO;
using UnityEngine;

namespace Darklight.Tests
{
    public static class DarklightTestOutput
    {
        // Retorna um caminho confiavel dentro do cache temporario da sessao ou pasta de outputs do projeto
        public static string GetDirectory(string subfolder)
        {
            #if UNITY_EDITOR
            string baseDir = Path.Combine(Application.dataPath, "../outputs/test-artifacts", subfolder);
            #else
            string baseDir = Path.Combine(Application.temporaryCachePath, "test-artifacts", subfolder);
            #endif
            Directory.CreateDirectory(baseDir);
            return baseDir.Replace('\\', '/') + "/";
        }
    }
}
```

### 2.2 Exemplo de Refatoracao Limpa
Em vez de:
```csharp
const string Output = "C:/Users/Usuario/.codex/.chatgpt-projects/g-p-6aca7556cc3c819199e34a5784ee1236/remaster-prototype-20261014/solo/";
```
Usar:
```csharp
static string Output => DarklightTestOutput.GetDirectory("solo");
```

---

## 3. BENEFICIOS DA APLICACAO
1. **Compatibilidade Imediata**: Os testes passam a gravar na pasta `outputs/test-artifacts/` na raiz do repositorio (que ja e ignorada pelo `.gitignore`) ou no cache temporario.
2. **Zero Risco de Regressao**: Nenhuma logica interna de gameplay ou assercao e modificada.
3. **Pronto para CI**: Permite que pipelines headless executem a suite e arquivem artefatos de teste automaticamente.
