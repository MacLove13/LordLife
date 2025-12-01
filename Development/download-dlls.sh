#!/bin/bash
# Download Bannerlord DLLs from NuGet packages
# This script downloads the required Bannerlord reference assemblies from NuGet

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TARGET_DIR="$SCRIPT_DIR/Bannerlord"
TEMP_DIR="/tmp/bannerlord-dlls-$$"

# Bannerlord version 1.3.6
BANNERLORD_VERSION="1.3.6.102656"

echo "=== Bannerlord DLL Download Script ==="
echo "Version: $BANNERLORD_VERSION"
echo "Target Directory: $TARGET_DIR"
echo ""

# Create target directory if it doesn't exist
mkdir -p "$TARGET_DIR"

# Create temp directory
mkdir -p "$TEMP_DIR"

# Function to download and extract DLLs from NuGet package
download_package() {
    local package_name=$1
    local version=$2
    
    echo "Downloading $package_name version $version..."
    
    # Download the nupkg file
    local url="https://api.nuget.org/v3-flatcontainer/${package_name,,}/${version}/${package_name,,}.${version}.nupkg"
    local nupkg_file="$TEMP_DIR/${package_name}.${version}.nupkg"
    
    if curl -L -s -f -o "$nupkg_file" "$url"; then
        # Extract DLLs from the package (nupkg is a zip file)
        local extract_dir="$TEMP_DIR/${package_name}"
        mkdir -p "$extract_dir"
        unzip -q -o "$nupkg_file" -d "$extract_dir" 2>/dev/null
        
        # Copy DLLs from lib folders to target directory
        find "$extract_dir" -name "*.dll" -type f -exec cp {} "$TARGET_DIR/" \; 2>/dev/null
        
        echo "  ✓ Downloaded and extracted $package_name"
    else
        echo "  ✗ Failed to download $package_name"
    fi
}

# Download reference assemblies packages
echo "Downloading Bannerlord Reference Assemblies..."
download_package "Bannerlord.ReferenceAssemblies.Core" "$BANNERLORD_VERSION"
download_package "Bannerlord.ReferenceAssemblies.StoryMode" "$BANNERLORD_VERSION"
download_package "Bannerlord.ReferenceAssemblies.SandBox" "$BANNERLORD_VERSION"
download_package "Bannerlord.ReferenceAssemblies.Native" "$BANNERLORD_VERSION"
download_package "Bannerlord.ReferenceAssemblies.CustomBattle" "$BANNERLORD_VERSION"
download_package "Bannerlord.ReferenceAssemblies.BirthAndDeath" "$BANNERLORD_VERSION"

# Download Newtonsoft.Json (commonly used version with Bannerlord)
echo "Downloading Newtonsoft.Json..."
download_package "Newtonsoft.Json" "13.0.1"

# Clean up temp directory
rm -rf "$TEMP_DIR"

echo ""
echo "=== Download Complete ==="
DLL_COUNT=$(find "$TARGET_DIR" -name "*.dll" -type f 2>/dev/null | wc -l)
echo "Total DLLs: $DLL_COUNT"
echo "Target directory: $TARGET_DIR"
echo ""
echo "DLL files:"
ls -1 "$TARGET_DIR"/*.dll 2>/dev/null | xargs -n1 basename || echo "No DLL files found"
