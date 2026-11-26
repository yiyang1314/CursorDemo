#define AppName "JDCloudChat"
#ifndef AppVersion
  #define AppVersion "1.1.0"
#endif
; 载荷目录由调用方通过 ISCC /DPayloadDir=... 传入；未传时回退到本地构建暂存目录。
; 本地 build.bat 传 %JD_PUBLISH%，CI 传 $env:RUNNER_TEMP\JDCloudChat。
#ifndef PayloadDir
  #define PayloadDir "..\Build\temp\JDCloudChat"
#endif
[Setup]
AppId={{8C4A2F51-3D7B-4E19-9C62-2B5E7A1D4F08}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\JDCloudChat
DefaultGroupName=JDCloudChat
OutputDir=..\Release
OutputBaseFilename=JDCloudChat_V{#AppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
[Files]
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\JDCloudChat"; Filename: "{app}\JDCloudChat.exe"
Name: "{autodesktop}\JDCloudChat"; Filename: "{app}\JDCloudChat.exe"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked
[Run]
Filename: "{app}\JDCloudChat.exe"; Description: "Launch JDCloudChat"; Flags: postinstall nowait skipifsilent
