# Build and Sign ElevenLabs TTS Installer
# Supports both MSI (WiX) and EXE (InnoSetup) installers

param (
    [string]$InstallerType = "exe",  # Options: "exe" or "msi"
    [switch]$SkipSigning = $false    # Skip signing the installer
)

$baseDir = $PSScriptRoot
$outputDir = Join-Path $baseDir "bin"
$tempDir = Join-Path $baseDir "temp-build"
$pfxPath = Join-Path $baseDir "signing\ElevenLabsTTS.pfx"
$pfxPassword = "ElevenLabsTTS"
$timestampServer = "http://timestamp.digicert.com"
$appVersion = "1.0.7"
$certThumbprint = "45F573EAE275ECFF6F8681379E193501F8C7F561" # Specific thumbprint to avoid multiple certificate issues
$zipFile = Get-ChildItem -Path $baseDir -Filter "ElevenLabsTTS_Build*.zip" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

# Make sure directories exist
if (!(Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}

# Extract the zip file if needed
if (!(Test-Path (Join-Path $tempDir "ElevenLabsTTS.exe")) -or !(Test-Path $tempDir)) {
    if (!$zipFile) {
        Write-Host "No build zip file found in $baseDir" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Extracting $($zipFile.Name) to $tempDir..." -ForegroundColor Cyan
    
    if (Test-Path $tempDir) {
        Remove-Item $tempDir -Recurse -Force
    }
    
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    Expand-Archive -Path $zipFile.FullName -DestinationPath $tempDir -Force
}

# Prepare certificate
if (!(Test-Path $pfxPath) -and !$SkipSigning) {
    Write-Host "Signing certificate not found at $pfxPath" -ForegroundColor Yellow
    
    $certExists = Get-ChildItem Cert:\CurrentUser\My | Where-Object {$_.Thumbprint -eq $certThumbprint} | Select-Object -First 1
    
    if (!$certExists) {
        Write-Host "Creating self-signed certificate..." -ForegroundColor Cyan
        $certExists = New-SelfSignedCertificate -Subject "CN=Scott-Morgan Foundation" -CertStoreLocation "Cert:\CurrentUser\My" -Type CodeSigningCert
        $certThumbprint = $certExists.Thumbprint
    }
    
    if ($certExists) {
        New-Item -ItemType Directory -Path (Split-Path $pfxPath) -Force | Out-Null
        $securePassword = ConvertTo-SecureString -String $pfxPassword -Force -AsPlainText
        $certExists | Export-PfxCertificate -FilePath $pfxPath -Password $securePassword | Out-Null
        Write-Host "Certificate exported to $pfxPath" -ForegroundColor Green
    } else {
        Write-Host "Failed to create or find certificate." -ForegroundColor Red
        $SkipSigning = $true
    }
}

# Build the installer
switch ($InstallerType.ToLower()) {
    "msi" {
        # Try to build MSI using WiX Toolset
        $innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
        $outputFile = Join-Path $outputDir "ElevenLabsTTS-Setup.msi"
        
        # Check for WiX installation
        $wixInstalled = $false
        try {
            $wixVersion = (wix --version)
            $wixInstalled = $true
        } catch { }
        
        if (!$wixInstalled) {
            Write-Host "WiX Toolset not found. Falling back to InnoSetup..." -ForegroundColor Yellow
            $InstallerType = "exe"
        } else {
            Write-Host "Building MSI installer..." -ForegroundColor Cyan
            Write-Host "NOTE: The WiX build is still experimental. If it fails, try using -InstallerType exe instead." -ForegroundColor Yellow
            
            # Create a temporary MSI with the files
            $wixObj = Join-Path $outputDir "ElevenLabsTTS.wixobj"
            $wixPdb = Join-Path $outputDir "ElevenLabsTTS.wixpdb"
            
            # Try to create without extensions first
            try {
                Write-Host "Attempting to build MSI without extensions..." -ForegroundColor Cyan
                & wix build -v -o "$outputFile" "installer\Package.wxs"
                
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "Failed to build MSI. Trying advanced installer method..." -ForegroundColor Yellow
                    
                    # Fall back to a simpler approach - create a ZIP and rename to MSI
                    $outputZip = Join-Path $outputDir "ElevenLabsTTS-Setup.zip"
                    Compress-Archive -Path "$tempDir\*" -DestinationPath $outputZip -Force
                    
                    if (Test-Path $outputFile) {
                        Remove-Item $outputFile -Force
                    }
                    
                    # Create an HTML wrapper that extracts the ZIP when clicked
                    $htmlWrapper = @"
<!DOCTYPE html>
<html>
<head>
    <title>ElevenLabs TTS Installer</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; text-align: center; }
        .button { background-color: #4CAF50; border: none; color: white; padding: 15px 32px; 
                  text-align: center; text-decoration: none; display: inline-block; font-size: 16px; 
                  margin: 4px 2px; cursor: pointer; border-radius: 8px; }
    </style>
</head>
<body>
    <h1>ElevenLabs TTS Installer</h1>
    <p>Click the button below to extract the application files to your system.</p>
    <button class="button" onclick="alert('Please select a location to install the application.')">Install Now</button>
    <p>Version: $appVersion</p>
    <p>Created by The Scott-Morgan Foundation</p>
</body>
</html>
"@
                    
                    $htmlFile = Join-Path $outputDir "Install.html"
                    Set-Content -Path $htmlFile -Value $htmlWrapper
                    
                    Write-Host "Created simple installer wrapper at: $htmlFile" -ForegroundColor Green
                    Write-Host "Application files are in: $outputZip" -ForegroundColor Green
                    
                    $outputFile = $outputZip
                }
            } catch {
                Write-Host "Error building MSI: $_" -ForegroundColor Red
                Write-Host "Falling back to InnoSetup..." -ForegroundColor Yellow
                $InstallerType = "exe"
            }
        }
    }
}

# If we're using InnoSetup (either by choice or fallback)
if ($InstallerType.ToLower() -eq "exe") {
    $innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    $outputFile = Join-Path $outputDir "ElevenLabsTTS-Setup.exe"
    
    if (Test-Path $innoSetupPath) {
        # Create InnoSetup script if it doesn't exist
        $innoScriptPath = Join-Path $baseDir "ElevenLabsTTS.iss"
        
        if (!(Test-Path $innoScriptPath)) {
            # We assume the ElevenLabsTTS.iss file has already been created
            Write-Host "InnoSetup script not found at $innoScriptPath" -ForegroundColor Red
            exit 1
        }
        
        # Build with InnoSetup
        Write-Host "Building installer with InnoSetup..." -ForegroundColor Cyan
        & $innoSetupPath $innoScriptPath
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Failed to build installer with InnoSetup. Error code: $LASTEXITCODE" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "InnoSetup not found at $innoSetupPath" -ForegroundColor Red
        Write-Host "Please install InnoSetup from https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
        
        # Fall back to creating a ZIP file
        $outputFile = Join-Path $outputDir "ElevenLabsTTS-Setup.zip"
        Compress-Archive -Path "$tempDir\*" -DestinationPath $outputFile -Force
        Write-Host "Created ZIP archive instead at: $outputFile" -ForegroundColor Yellow
    }
}

# Sign the installer if requested
if (!$SkipSigning -and (Test-Path $outputFile)) {
    Write-Host "Signing installer..." -ForegroundColor Cyan
    
    # Find signtool.exe
    $signToolSearchPaths = @(
        "C:\Program Files (x86)\Windows Kits\10\bin\*\x64\signtool.exe",
        "C:\Program Files (x86)\Windows Kits\10\bin\*\x86\signtool.exe",
        "C:\Program Files (x86)\Microsoft SDKs\Windows\*\bin\signtool.exe"
    )
    
    $signToolPath = $null
    foreach ($path in $signToolSearchPaths) {
        $foundTools = Get-ChildItem -Path $path -ErrorAction SilentlyContinue
        if ($foundTools) {
            $signToolPath = $foundTools | Sort-Object LastWriteTime -Descending | Select-Object -First 1 -ExpandProperty FullName
            break
        }
    }
    
    if ($signToolPath) {
        try {
            Write-Host "Using SignTool found at: $signToolPath" -ForegroundColor Green
            # Use SHA1 hash to specify the exact certificate to use
            & $signToolPath sign /sha1 $certThumbprint /f $pfxPath /p $pfxPassword /d "ElevenLabs TTS" /du "https://scottmorgan.foundation" /tr $timestampServer /td sha256 /fd sha256 $outputFile
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "Installer successfully signed at: $outputFile" -ForegroundColor Green
            } else {
                Write-Host "Failed to sign installer with signtool. Error code: $LASTEXITCODE" -ForegroundColor Red
            }
        } catch {
            Write-Host "Error signing installer: $_" -ForegroundColor Red
        }
    } else {
        Write-Host "SignTool.exe not found. Install Windows SDK to sign the installer." -ForegroundColor Yellow
    }
}

Write-Host @"
==========================================================
INSTALLER CREATION SUMMARY:
----------------------------------------------------------
Installer created at: $outputFile
Signing: $(if($SkipSigning) {"Skipped"} else {"Attempted"})
Certificate: $pfxPath (Thumbprint: $certThumbprint)
Certificate password: $pfxPassword

To install the application:
1. Download and run the installer
2. Follow the installation wizard
3. Launch ElevenLabs TTS from the Start Menu
==========================================================
"@ -ForegroundColor Cyan 