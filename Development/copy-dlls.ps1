# Copy Bannerlord DLLs Script
# This script copies all required Bannerlord DLLs from your game installation to the Development/Bannerlord folder

param(
    [Parameter(Mandatory=$false)]
    [string]$GameFolder = "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord",
    
    [Parameter(Mandatory=$false)]
    [string]$BinariesFolder = "Win64_Shipping_Client"
)

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$TargetDir = Join-Path $ScriptDir "Bannerlord"

# Create target directory if it doesn't exist
if (-not (Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
    Write-Host "Created directory: $TargetDir"
}

# Define source paths
$BinPath = Join-Path $GameFolder "bin\$BinariesFolder"
$NativeModulePath = Join-Path $GameFolder "Modules\Native\bin\$BinariesFolder"
$SandBoxModulePath = Join-Path $GameFolder "Modules\SandBox\bin\$BinariesFolder"
$SandBoxCoreModulePath = Join-Path $GameFolder "Modules\SandBoxCore\bin\$BinariesFolder"
$StoryModeModulePath = Join-Path $GameFolder "Modules\StoryMode\bin\$BinariesFolder"
$CustomBattleModulePath = Join-Path $GameFolder "Modules\CustomBattle\bin\$BinariesFolder"
$BirthAndDeathModulePath = Join-Path $GameFolder "Modules\BirthAndDeath\bin\$BinariesFolder"

# Function to copy DLLs from a source path
function Copy-DLLs {
    param (
        [string]$SourcePath,
        [string]$Description
    )
    
    if (Test-Path $SourcePath) {
        $dllFiles = Get-ChildItem -Path $SourcePath -Filter "*.dll" -ErrorAction SilentlyContinue
        if ($dllFiles) {
            Write-Host "`nCopying $Description DLLs from: $SourcePath"
            foreach ($dll in $dllFiles) {
                $destPath = Join-Path $TargetDir $dll.Name
                Copy-Item -Path $dll.FullName -Destination $destPath -Force
                Write-Host "  Copied: $($dll.Name)"
            }
        } else {
            Write-Host "No DLL files found in: $SourcePath" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Path not found: $SourcePath" -ForegroundColor Yellow
    }
}

Write-Host "=== Bannerlord DLL Copy Script ===" -ForegroundColor Cyan
Write-Host "Game Folder: $GameFolder"
Write-Host "Binaries Folder: $BinariesFolder"
Write-Host "Target Directory: $TargetDir"
Write-Host ""

# Verify game folder exists
if (-not (Test-Path $GameFolder)) {
    Write-Host "ERROR: Game folder not found at: $GameFolder" -ForegroundColor Red
    Write-Host "Please specify the correct game folder path using the -GameFolder parameter" -ForegroundColor Red
    Write-Host ""
    Write-Host "Example usage:" -ForegroundColor Yellow
    Write-Host '  .\copy-dlls.ps1 -GameFolder "C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord"' -ForegroundColor Yellow
    exit 1
}

# Copy DLLs from all locations
Copy-DLLs -SourcePath $BinPath -Description "Main Binaries"
Copy-DLLs -SourcePath $NativeModulePath -Description "Native Module"
Copy-DLLs -SourcePath $SandBoxModulePath -Description "SandBox Module"
Copy-DLLs -SourcePath $SandBoxCoreModulePath -Description "SandBoxCore Module"
Copy-DLLs -SourcePath $StoryModeModulePath -Description "StoryMode Module"
Copy-DLLs -SourcePath $CustomBattleModulePath -Description "CustomBattle Module"
Copy-DLLs -SourcePath $BirthAndDeathModulePath -Description "BirthAndDeath Module"

Write-Host ""
Write-Host "=== Copy Complete ===" -ForegroundColor Green
$totalDlls = (Get-ChildItem -Path $TargetDir -Filter "*.dll" | Measure-Object).Count
Write-Host "Total DLLs copied: $totalDlls"
Write-Host "Target directory: $TargetDir"
