# LordLife

<p align="center">
  <img src="logo.png" alt="LordLife Logo"/>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Bannerlord-1.3.6-blue" alt="Bannerlord Version"/>
  <img src="https://img.shields.io/badge/DLC-War%20Sails-green" alt="War Sails DLC"/>
  <img src="https://img.shields.io/badge/ModVersion-0.0.1-orange" alt="Mod Version"/>
</p>

## 📖 Descrição

**LordLife** é um mod para **Mount & Blade II: Bannerlord** versão 1.3.6, com suporte à DLC **War Sails**. Este mod expande a experiência de jogo, adicionando funcionalidades para enriquecer a vida do lorde em Calradia.

## 🎮 Requisitos

- **Mount & Blade II: Bannerlord** versão **1.3.6** ou superior
- **DLC War Sails** (mais recente)
- **Bannerlord.Harmony** v2.2.2 ou superior (recomendado v2.3.3+)

## 📦 Dependências do Mod

O mod depende dos seguintes módulos (carregados antes do LordLife):
- `Bannerlord.Harmony` v2.2.2+ (recomendado v2.3.3+)
- `Native`
- `SandBoxCore`
- `Sandbox`
- `StoryMode`
- `CustomBattle`

## 🚀 Instalação

1. Baixe a versão mais recente do mod
2. Extraia o conteúdo na pasta `Modules` do seu Bannerlord:
   ```
   {Diretório do jogo}/Modules/Bannerlord.LordLife/
   ```
3. Inicie o Bannerlord Launcher
4. Ative o mod `LordLife` na lista de mods
5. Certifique-se de que as dependências estão carregadas **antes** do LordLife

## 🛠️ Compilação do Projeto

### Pré-requisitos
- **.NET SDK 6.0** ou superior
- **.NET Framework 4.7.2** (para build Windows)
- **Visual Studio 2022** ou **VSCode** com extensão C#

### Configuração

1. Clone o repositório:
   ```bash
   git clone https://github.com/MacLove13/LordLife.git
   ```

2. **(Opcional) Copiar DLLs do jogo** - Se você preferir usar as DLLs da sua instalação local do jogo ao invés das DLLs de referência NuGet:

   **No Windows (PowerShell):**
   
   Abra o PowerShell na pasta do projeto e execute:
   
   ```powershell
   .\Development\copy-dlls.ps1 -GameFolder "CAMINHO_DA_SUA_INSTALACAO_DO_BANNERLORD"
   ```

   **Exemplos de caminhos comuns:**
   
   - **Steam (padrão):**
     ```powershell
     .\Development\copy-dlls.ps1 -GameFolder "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"
     ```
   
   - **Steam (biblioteca personalizada):**
     ```powershell
     .\Development\copy-dlls.ps1 -GameFolder "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord"
     ```
   
   - **GOG:**
     ```powershell
     .\Development\copy-dlls.ps1 -GameFolder "C:\GOG Games\Mount & Blade II Bannerlord"
     ```
   
   - **Epic Games:**
     ```powershell
     .\Development\copy-dlls.ps1 -GameFolder "C:\Program Files\Epic Games\Mount & Blade II Bannerlord"
     ```
   
   - **Xbox Game Pass:**
     ```powershell
     .\Development\copy-dlls.ps1 -GameFolder "C:\XboxGames\Mount & Blade II Bannerlord" -BinariesFolder "Gaming.Desktop.x64_Shipping_Client"
     ```

   > 💡 **Dica**: Se você não sabe onde o jogo está instalado, procure por "Mount & Blade II Bannerlord" no explorador de arquivos ou verifique nas configurações da sua plataforma de jogos (Steam, GOG, etc).
   
   > ⚠️ **Erro ao executar o script?** Se você receber um erro sobre políticas de execução, execute o PowerShell como **Administrador** e rode:
   > ```powershell
   > Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   > ```
   > Depois tente executar o script novamente.

3. Compile o projeto:
   ```bash
   dotnet build -c Release
   ```

> 📝 **Nota**: As DLLs de referência do Bannerlord estão incluídas no repositório na pasta `Development/Bannerlord/`. Elas foram baixadas dos pacotes NuGet oficiais do Bannerlord (Bannerlord.ReferenceAssemblies) versão 1.3.6.102656. Usar o script `copy-dlls.ps1` é opcional e só é necessário se você quiser usar as DLLs da sua instalação local do jogo.

### Targets Suportados
- `net472` - Windows (Steam/GOG)
- `net6` - Windows Store/Xbox

## 📁 Estrutura do Projeto

```
Bannerlord.LordLife/
├── Development/
│   ├── Bannerlord/        # DLLs do jogo (não comitadas)
│   │   └── README.md      # Instruções sobre as DLLs
│   └── copy-dlls.ps1      # Script para copiar DLLs do jogo
├── _Module/
│   └── SubModule.xml      # Configuração do módulo para Bannerlord
├── Properties/
│   └── launchSettings.json
├── .vscode/               # Configurações do VSCode
├── SubModule.cs           # Classe principal do mod
├── Bannerlord.LordLife.csproj
├── README.md
└── .gitignore
```

## 🔧 Desenvolvimento

### APIs TaleWorlds Utilizadas
- `TaleWorlds.MountAndBlade` - Core da engine do jogo
- `TaleWorlds.Library` - Utilitários e debug
- `TaleWorlds.CampaignSystem` - Sistema de campanha
- `TaleWorlds.Core` - Funcionalidades centrais
- `TaleWorlds.GauntletUI` - Sistema de UI

### Bibliotecas Auxiliares
- **Harmony 2.4.2** - Para patching de métodos
- **BUTR.Harmony.Analyzer** - Análise de código Harmony
- **Bannerlord.BuildResources** - Recursos de build

## 📝 Licença

Este projeto está sob a licença MIT.

## 🤝 Contribuição

Contribuições são bem-vindas! Por favor, abra uma issue ou pull request.

---

<p align="center">Feito com ❤️ para a comunidade Bannerlord</p>
