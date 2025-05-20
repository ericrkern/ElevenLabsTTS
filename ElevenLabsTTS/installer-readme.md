# ElevenLabs TTS Installer Guide

This document explains how to build and sign the installer for ElevenLabs TTS.

## Prerequisites

To build the installer, you'll need one of the following tools:

1. **InnoSetup** (recommended) - Download from: https://jrsoftware.org/isdl.php
2. **WiX Toolset** - Install using: `dotnet tool install --global wix`
3. **Advanced Installer** - Commercial tool with more features

For signing the installer, you'll need:

1. **SignTool** - Part of the Windows SDK (https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/)
2. **Code Signing Certificate** - A self-signed certificate is created automatically by the script, but for distribution, consider purchasing a real code signing certificate

## Building the Installer

### Option 1: Using the All-in-One Script (Recommended)

The easiest way to build the installer is to use the provided PowerShell script:

```powershell
.\build-signed-installer.ps1
```

This script will:
1. Extract the latest build ZIP file to the temp-build directory
2. Create a self-signed certificate if one doesn't exist
3. Try to build an EXE installer using InnoSetup (if installed)
4. If InnoSetup is not available, create a ZIP archive and HTML launcher
5. Sign the installer with the certificate (if SignTool is available)

### Option 2: Build EXE Installer with InnoSetup

To build an EXE installer using InnoSetup:

```powershell
.\build-inno-installer.ps1
```

### Option 3: Build MSI Installer with WiX

To build an MSI installer using WiX Toolset:

```powershell
.\build-signed-installer.ps1 -InstallerType msi
```

Note: The WiX-based MSI build is experimental and requires WiX Toolset to be properly installed.

## Installation Options

Depending on which tools are available, the build script will create one of these outputs:

1. **EXE Installer** - If InnoSetup is installed, a full-featured installer will be created
2. **MSI Installer** - If WiX is properly installed, an MSI package will be created
3. **ZIP Package** - If neither tool is available, a ZIP file containing the application will be created
4. **HTML Launcher** - A simple HTML file that helps extract the ZIP package

## Signing Options

The installer is signed automatically as part of the build process if SignTool is available. The script will search for SignTool in common locations.

If you want to sign an existing installer manually, you can use SignTool directly:

```powershell
signtool sign /f "signing\ElevenLabsTTS.pfx" /p "ElevenLabsTTS" /tr "http://timestamp.digicert.com" /td sha256 /fd sha256 "bin\ElevenLabsTTS-Setup.exe"
```

For production use, it's recommended to purchase a code signing certificate from a trusted Certificate Authority.

## Output Files

After building, the installer will be located in the `bin` directory:

- **EXE Installer**: `bin\ElevenLabsTTS-Setup.exe`
- **MSI Installer**: `bin\ElevenLabsTTS-Setup.msi`
- **ZIP Archive**: `bin\ElevenLabsTTS-Setup.zip`
- **HTML Launcher**: `bin\Install.html`

## Troubleshooting

If you encounter issues:

1. **Missing InnoSetup**: Download and install InnoSetup from https://jrsoftware.org/isdl.php
2. **Missing SignTool**: Install the Windows SDK from https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/
3. **WiX Build Errors**: Try using the InnoSetup method instead or install WiX 3.11 rather than WiX 6.0
4. **Certificate Issues**: Delete the `signing\ElevenLabsTTS.pfx` file and let the script recreate it
5. **ZIP Only**: If you only get a ZIP file, it means neither InnoSetup nor WiX is installed properly

## Manual Steps (if scripts don't work)

1. Extract the latest build ZIP file to a folder
2. Create an installer using InnoSetup or WiX Toolset manually
3. Sign the installer using SignTool from the Windows SDK

## Working with GitHub

To commit and push your installer scripts to GitHub:

```powershell
git add build-installer.ps1 build-inno-installer.ps1 build-signed-installer.ps1 ElevenLabsTTS.iss installer-readme.md
git commit -m "Add installer build scripts and documentation"
git push
``` 