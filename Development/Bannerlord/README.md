# Bannerlord Reference Assemblies

This folder contains the Bannerlord reference assemblies (DLLs) required to build the project.

## Source

The DLLs are downloaded from official Bannerlord.ReferenceAssemblies NuGet packages:
- **Version**: 1.3.6.102656 (Bannerlord 1.3.6)
- **Source**: https://www.nuget.org/packages/Bannerlord.ReferenceAssemblies.Core/

These are metadata-only reference assemblies created by the BUTR team specifically for building Bannerlord mods.

## Updating DLLs

If you need to update to a different Bannerlord version, run the download script:

**Linux/Mac:**
```bash
./Development/download-dlls.sh
```

The script will download the latest reference assemblies from NuGet.

## Alternative: Copy from Game Installation (Windows)

If you prefer to use DLLs from your local game installation (not recommended):

```powershell
.\Development\copy-dlls.ps1 -GameFolder "PATH_TO_YOUR_BANNERLORD"
```

**Example:**
```powershell
.\Development\copy-dlls.ps1 -GameFolder "D:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord"
```
