; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Choike"
#define MyAppVersion "1.2"
#define MyAppPublisher "YerkoAndrei"
#define MyAppURL "https://github.com/YerkoAndrei"
#define MyAppExeName "Choike.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{633BD292-38E8-49B8-8E75-267A019666EC}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=C:\Users\YerkoAndrei\Desktop
OutputBaseFilename=Instalador Choike
SetupIconFile=C:\Users\YerkoAndrei\Desktop\Proyectos\Choike\Choike\Arte\Choike.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
WizardImageFile="C:\Users\YerkoAndrei\Desktop\Proyectos\Choike\Choike\Arte\big.bmp" 
WizardSmallImageFile="C:\Users\YerkoAndrei\Desktop\Proyectos\Choike\Choike\Arte\small.bmp"

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\YerkoAndrei\Desktop\Proyectos\Choike\Choike\bin\Release\net6.0-windows\publish\win-x86\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

