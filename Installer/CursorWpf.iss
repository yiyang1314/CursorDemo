#define AppName "CursorWpf"
#ifndef AppVersion
  #define AppVersion "1.1.0"
#endif
[Setup]
AppId={{6F7D0B8E-5B2C-4C64-AB34-6F4A20D91B13}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\CursorWpf
DefaultGroupName=CursorWpf
OutputDir=..\Release
OutputBaseFilename=CursorWpf_V{#AppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\CursorWpf.App\Assets\CursorWpf.ico
[Files]
Source: "..\Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\CursorWpf"; Filename: "{app}\CursorWpf.exe"
Name: "{autodesktop}\CursorWpf"; Filename: "{app}\CursorWpf.exe"; Tasks: desktopicon
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked
[Run]
Filename: "{app}\CursorWpf.exe"; Description: "Launch CursorWpf"; Flags: postinstall nowait skipifsilent