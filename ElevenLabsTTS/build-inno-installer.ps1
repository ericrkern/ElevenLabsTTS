# Build Installer with InnoSetup

# Check if InnoSetup is installed
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if (!(Test-Path $innoSetupPath)) {
    Write-Host "InnoSetup not found at $innoSetupPath" -ForegroundColor Yellow
    Write-Host "Please install InnoSetup from https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    Write-Host "Or modify this script with the correct path to ISCC.exe" -ForegroundColor Yellow
    exit 1
}

# Check if the zip file is extracted to temp-build
if (!(Test-Path "temp-build\ElevenLabsTTS.exe")) {
    Write-Host "ElevenLabsTTS.exe not found in temp-build directory." -ForegroundColor Yellow
    Write-Host "Extracting latest build zip..." -ForegroundColor Cyan
    
    # Find the latest zip file with pattern ElevenLabsTTS_Build*.zip
    $latestZip = Get-ChildItem -Path "." -Filter "ElevenLabsTTS_Build*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    
    if ($latestZip) {
        if (Test-Path "temp-build") {
            Remove-Item "temp-build" -Recurse -Force
        }
        
        New-Item -ItemType Directory -Path "temp-build" -Force | Out-Null
        Write-Host "Extracting $($latestZip.Name) to temp-build..." -ForegroundColor Cyan
        Expand-Archive -Path $latestZip.FullName -DestinationPath "temp-build" -Force
    } else {
        Write-Host "No build zip file found. Please extract the latest build to the temp-build directory." -ForegroundColor Red
        exit 1
    }
}

# Make sure bin directory exists
if (!(Test-Path "bin")) {
    New-Item -ItemType Directory -Path "bin" -Force | Out-Null
}

# Create the installer with InnoSetup
Write-Host "Building installer with InnoSetup..." -ForegroundColor Cyan
& $innoSetupPath "ElevenLabsTTS.iss"

if ($LASTEXITCODE -eq 0) {
    $installerPath = "bin\ElevenLabsTTS-Setup.exe"
    if (Test-Path $installerPath) {
        Write-Host "Installer successfully created at: $installerPath" -ForegroundColor Green
    } else {
        Write-Host "Installer creation failed. Check the InnoSetup log for errors." -ForegroundColor Red
    }
} else {
    Write-Host "Failed to build installer. Error code: $LASTEXITCODE" -ForegroundColor Red
} 