#preproc ispp
#define MyAppName "LwXLabs.Mcp.Template.DotNet"
#define MyPublisher "LwXLabs"
#define MyExeName "LwXLabs.Mcp.Template.DotNet.exe"
#define PublishDir SourcePath + "artifacts\publish\win-x64"
#ifndef SkipPublish
  #define PublishExitCode Exec("powershell.exe", "-NoProfile -ExecutionPolicy Bypass -File scripts\Publish.ps1", SourcePath, 1, SW_SHOWNORMAL)
  #if PublishExitCode != 0
    #error "Publish failed. Fix the build before compiling Setup."
  #endif
#endif
#define PublishedExe PublishDir + "\" + MyExeName
#ifnexist PublishedExe
  #error "Missing published executable."
#endif
#define MyVersion GetVersionNumbersString(PublishedExe)

[Setup]
AppId={{B3D38780-5F23-4FD5-9CA2-5A76B3C1AE6E}
AppName={#MyAppName}
AppPublisher={#MyPublisher}
AppVersion={#MyVersion}
VersionInfoVersion={#MyVersion}
DefaultDirName={localappdata}\Programs\LwXLabs\{#MyAppName}
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64os
UninstallDisplayIcon={app}\{#MyExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
DisableProgramGroupPage=yes
OutputDir=artifacts\installer
OutputBaseFilename={#MyAppName}-{#MyVersion}-Setup
LicenseFile=LICENSE

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}\Local web status"; Filename: "{app}\{#MyExeName}"; Parameters: "--http"; WorkingDir: "{app}"
Name: "{autoprograms}\{#MyAppName}\Uninstall"; Filename: "{uninstallexe}"
