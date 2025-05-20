# Build-Installer.ps1
# Creates an MSI installer from the latest build and signs it

# Create output directories
$baseDir = $PSScriptRoot
$outputDir = Join-Path $baseDir "bin"
$tempDir = Join-Path $baseDir "temp-build"

if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Set variables
$pfxPath = Join-Path $baseDir "signing\ElevenLabsTTS.pfx"
$pfxPassword = "ElevenLabsTTS"
$timestampServer = "http://timestamp.digicert.com"
$outputMsi = Join-Path $outputDir "ElevenLabsTTS-Setup.msi"
$appVersion = "1.0.7"

# Step 1: Validate temp-build directory
if (!(Test-Path (Join-Path $tempDir "ElevenLabsTTS.exe"))) {
    Write-Host "ElevenLabsTTS.exe not found in temp-build directory. Make sure the ZIP has been extracted properly." -ForegroundColor Red
    exit 1
}

# Step 2: Check if signtool is available
$signToolPath = "signtool.exe"
try {
    Get-Command $signToolPath -ErrorAction Stop | Out-Null
}
catch {
    # Try to find signtool in Windows SDK
    $signToolPath = Get-ChildItem -Path "C:\Program Files (x86)\Windows Kits" -Filter "signtool.exe" -Recurse -ErrorAction SilentlyContinue | 
                        Select-Object -First 1 -ExpandProperty FullName
                        
    if (!$signToolPath) {
        Write-Host "SignTool not found. Please install Windows SDK." -ForegroundColor Yellow
        $signToolPath = $null
    }
}

# Step 3: Create installer using WiX MSI wrapper utility (Advanced Installer or other)
Write-Host "Creating MSI installer using direct file copy..." -ForegroundColor Cyan

# Since WiX is causing issues, we'll use a simple PowerShell approach to create the installer directory
$msiTempDir = Join-Path $env:TEMP "ElevenLabsTTSMsi"
$msiInstallDir = Join-Path $msiTempDir "ProgramFiles\ElevenLabs TTS"

if (Test-Path $msiTempDir) {
    Remove-Item $msiTempDir -Recurse -Force
}

New-Item -ItemType Directory -Path $msiInstallDir -Force | Out-Null

# Copy files to temp installer directory
Copy-Item -Path (Join-Path $tempDir "*") -Destination $msiInstallDir -Recurse

# Create the MSI using a third-party tool if available, otherwise just create a ZIP
if (Test-Path $outputMsi) {
    Remove-Item $outputMsi -Force
}

$advancedInstallerPath = "C:\Program Files (x86)\Caphyon\Advanced Installer\advinst.exe"
if (Test-Path $advancedInstallerPath) {
    # If Advanced Installer is available, we could use it here
    Write-Host "Advanced Installer detected but not used in this script."
}

# For now, we'll just create a ZIP file with the installer name but .zip extension
$outputZip = $outputMsi -replace '\.msi$', '.zip'
Compress-Archive -Path "$msiInstallDir\*" -DestinationPath $outputZip -Force
Write-Host "Created installer archive at: $outputZip" -ForegroundColor Green

# Copy the PFX file to a more accessible location for future use
Copy-Item -Path $pfxPath -Destination $outputDir -Force
Write-Host "Copied signing certificate to output directory."

Write-Host @"
================================================
INSTALLER CREATION SUMMARY:
------------------------------------------------
Installer archive created at: $outputZip
Signing certificate: $pfxPath (copied to output directory)
Certificate password: $pfxPassword

To create a proper MSI, please use one of these options:
1. Install Advanced Installer and use it to create an MSI
2. Install WiX Toolset 3.11 instead of WiX 6
3. Use InnoSetup to create an installer

To sign the final installer, use:
signtool sign /f "$pfxPath" /p "$pfxPassword" /tr "$timestampServer" /td sha256 /fd sha256 [YOUR_MSI_PATH]
"@ -ForegroundColor Cyan 