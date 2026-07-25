[Setup]
AppName=Hungry Fast Food Admin
AppVersion=1.0.0
DefaultDirName={code:GetInstallDir}\HungryFastFood
DefaultGroupName=Hungry Fast Food
UninstallDisplayIcon={app}\HungryFastFoodAdmin.exe
Compression=lzma2
SolidCompression=yes
OutputDir=installer_output
OutputBaseFilename=HungryFastFood_Setup

[Code]
var
  CustomDirPage: TInputDirWizardPage;

function GetInstallDir(Param: String): String;
begin
  // Check if D: drive exists, else use C:
  if DirExists('D:\') then
    Result := 'D:\'
  else
    Result := 'C:\Program Files';
end;

[Files]
Source: "..\HungryFastFoodAdmin\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "..\HungryFastFoodAdmin\Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\Hungry Fast Food Admin"; Filename: "{app}\HungryFastFoodAdmin.exe"
Name: "{group}\Uninstall"; Filename: "{uninstallexe}"
Name: "{commondesktop}\Hungry Fast Food Admin"; Filename: "{app}\HungryFastFoodAdmin.exe"

[Run]
Filename: "{app}\HungryFastFoodAdmin.exe"; Description: "Launch Hungry Fast Food Admin"; Flags: postinstall nowait skipifsilent