# Instruções para Agentes de IA - LordLife

Este documento fornece contexto e diretrizes para agentes de IA trabalhando neste repositório.

## 📋 Visão Geral do Projeto

**LordLife** é um mod para **Mount & Blade II: Bannerlord**:
- **Versão do jogo**: 1.3.6
- **DLC suportada**: War Sails (versão mais recente)
- **Versão do mod**: 0.0.1
- **Tipo**: Singleplayer / Community Mod
- **Linguagem**: C# (.NET 4.7.2 e .NET 6)

## 🏗️ Arquitetura do Projeto

### Arquivos Principais
| Arquivo | Descrição |
|---------|-----------|
| `SubModule.cs` | Ponto de entrada do mod, herda de `MBSubModuleBase` |
| `Bannerlord.LordLife.csproj` | Configuração do projeto .NET |
| `_Module/SubModule.xml` | Metadados do mod para o Bannerlord Launcher |

### Namespace
- Namespace principal: `Bannerlord.LordLife`

### Dependências do Mod
Carregar ANTES do LordLife:
1. `Bannerlord.Harmony` v2.2.2+ (recomendado v2.3.3+)
2. `Native`
3. `SandBoxCore`
4. `Sandbox`
5. `StoryMode`
6. `CustomBattle`

## 🔧 Comandos Úteis

### Build
```bash
# Build para todas as plataformas
dotnet build

# Build Release
dotnet build -c Release

# Build específico para .NET 4.7.2 (Windows)
dotnet build -f net472

# Build específico para .NET 6 (Windows Store)
dotnet build -f net6
```

### Limpeza
```bash
dotnet clean
```

## 📁 Estrutura de Diretórios

```
/
├── .github/
│   └── agents.md           # Este arquivo
├── .vscode/                # Configurações do VSCode
├── _Module/
│   └── SubModule.xml       # Configuração do módulo
├── Properties/
│   └── launchSettings.json
├── SubModule.cs            # Classe principal
├── Bannerlord.LordLife.csproj
├── README.md
└── .gitignore
```

## 🎯 Padrões de Código

### Convenções
- **Linguagem dos comentários**: Português (PT-BR)
- **Logs**: Prefixar com `[LordLife]`
- **Mensagens ao usuário**: Português
- **Código**: Seguir padrões C# convencionais

### Exemplo de Log
```csharp
Debug.Print("[LordLife] Descrição da ação.");
```

### Exemplo de Mensagem In-Game
```csharp
InformationManager.DisplayMessage(
    new InformationMessage(
        "Mensagem em português!",
        Colors.Green
    )
);
```

## 🔌 APIs TaleWorlds

### Principais Namespaces
```csharp
using TaleWorlds.Library;           // Debug, Colors, etc.
using TaleWorlds.MountAndBlade;     // MBSubModuleBase, etc.
using TaleWorlds.Core;              // Core game systems
using TaleWorlds.CampaignSystem;    // Campaign mechanics
using TaleWorlds.GauntletUI;        // UI Framework
using TaleWorlds.Localization;      // Localization
```

### Ciclo de Vida do SubModule
1. `OnSubModuleLoad()` - Mod carregado
2. `OnBeforeInitialModuleScreenSetAsRoot()` - Antes da tela inicial
3. `OnSubModuleUnloaded()` - Mod descarregado

### Harmony Patching
```csharp
using HarmonyLib;

[HarmonyPatch(typeof(TargetClass))]
[HarmonyPatch("TargetMethod")]
class MyPatch
{
    static void Prefix() { /* antes do método */ }
    static void Postfix() { /* depois do método */ }
}
```

## ⚠️ Considerações Importantes

1. **Caminhos Hardcoded**: O `.csproj` contém caminhos absolutos para DLLs do jogo. Estes podem precisar ser ajustados para diferentes máquinas.

2. **Variáveis do SubModule.xml**: Usam placeholders como `$moduleid$`, `$modulename$`, `$version$` que são substituídos durante o build.

3. **Compatibilidade**: O mod é projetado para Bannerlord 1.3.6 com War Sails DLC.

4. **Multi-Target**: O projeto compila para `net472` (Steam/GOG) e `net6` (Windows Store).

## 📦 Pacotes NuGet Utilizados

| Pacote | Versão | Propósito |
|--------|--------|-----------|
| `Lib.Harmony` | 2.4.2 | Runtime patching |
| `Harmony.Extensions` | 3.2.0.77 | Extensões do Harmony |
| `BUTR.Harmony.Analyzer` | 1.0.1.50 | Análise de código |
| `Bannerlord.BuildResources` | 1.1.0.129 | Recursos de build |
| `Nullable` | 1.3.1 | Suporte a nullable |
| `IsExternalInit` | 1.0.3 | Suporte a init |

## 🚀 Próximos Passos Sugeridos

Para desenvolver novas funcionalidades:

1. **Behaviors de Campanha**: Criar classes que herdam de `CampaignBehaviorBase`
2. **Modelos de Jogo**: Criar modelos que herdam de classes base do TaleWorlds
3. **UI Customizada**: Usar GauntletUI para criar interfaces
4. **Patches**: Usar Harmony para modificar comportamentos existentes

## 📚 Recursos Externos

- [TaleWorlds API Documentation v1.0.3](https://apidoc.bannerlord.com/v/1.0.3/namespace_tale_worlds.html) - Referência oficial das classes e métodos da API TaleWorlds
- [Bannerlord Documentation](https://docs.bannerlordmodding.com/)
- [BUTR GitHub](https://github.com/BUTR)
- [Harmony Wiki](https://harmony.pardeike.net/)
- [TaleWorlds Modding Discord](https://discord.gg/mountandblade)
