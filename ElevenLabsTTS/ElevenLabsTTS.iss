#define AppName "ElevenLabs TTS"
#define AppVersion "1.0.7"
#define AppPublisher "The Scott-Morgan Foundation"
#define AppURL "https://scottmorgan.foundation"
#define AppExeName "ElevenLabsTTS.exe"
#define SourceDir "temp-build"

[Setup]
AppId={{4F066D87-1698-4A1A-9FF5-F8B8E2F7C666}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
AllowNoIcons=yes
LicenseFile=LICENSE
OutputDir=bin
OutputBaseFilename=ElevenLabsTTS-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SignTool=signtool sign /f "signing\ElevenLabsTTS.pfx" /p "ElevenLabsTTS" /tr "http://timestamp.digicert.com" /td sha256 /fd sha256 $f

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent 