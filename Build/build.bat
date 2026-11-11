@echo off
setlocal EnableExtensions EnableDelayedExpansion
rem ============================================================
rem  CursorWpf one-click build / publish / package script
rem  Merged from: build.bat + build_all.bat + find_msbuild.bat
rem               + publish.bat + package.bat
rem
rem  Pipeline: locate MSBuild engine -> build Release -> stage
rem            Publish folder -> create ZIP -> create EXE installer
rem            (Inno Setup 6) -> MSI (WiX Toolset 5) -> clean staging
rem
rem  Artifacts (naming rule: ProjectName_VX.X.X):
rem    Release\CursorWpf_V<version>.zip
rem    Release\CursorWpf_V<version>.exe   (requires Inno Setup 6)
rem    Release\CursorWpf_V<version>.msi   (requires .NET 8 SDK + WiX 5)
rem  Version source: Build\version.txt (plain ASCII X.Y.Z, no BOM)
rem ============================================================
cd /d "%~dp0.."
set "ROOT=%CD%"
set "BUILD_DIR=%~dp0"
set "SLN=%ROOT%\CursorWpf.sln"
set "PUBLISH=%ROOT%\Publish"
set "RELEASE=%ROOT%\Release"

echo ============================================================
echo  CursorWpf automated build, publish and package
echo ============================================================

rem ---------- 1) read version ----------
if not exist "%ROOT%\Build\version.txt" (
    echo [ERROR] Build\version.txt not found.
    exit /b 1
)
set "VERSION="
set /p VERSION=<"%ROOT%\Build\version.txt"
echo %VERSION%| findstr /r "^[0-9][0-9]*\.[0-9][0-9]*\.[0-9][0-9]*$" >nul
if errorlevel 1 (
    echo [ERROR] Invalid version "!VERSION!" in Build\version.txt.
    echo         Save it as plain ASCII like "1.1.0" - no BOM, no spaces.
    exit /b 1
)
echo [INFO] Version: %VERSION%

if not exist "%SLN%" (
    echo [ERROR] Solution not found: %SLN%
    exit /b 2
)

rem ---------- 2) locate MSBuild engine ----------
set "MSBUILD_EXE="
set "MSBUILD_MODE="
set "MSBUILD_WINFX="

rem 2a) msbuild.exe on PATH
for /f "delims=" %%I in ('where msbuild.exe 2^>nul') do if not defined MSBUILD_EXE (
    set "MSBUILD_EXE=%%~I"
    set "MSBUILD_MODE=VS"
)

rem 2b) Visual Studio instances via vswhere (works with any custom install dir)
if not defined MSBUILD_EXE (
    set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
    if exist "!VSWHERE!" for /f "usebackq delims=" %%I in (`"!VSWHERE!" -latest -products * -requires Microsoft.Component.MSBuild -property installationPath 2^>nul`) do (
        for %%B in ("%%~I\MSBuild\Current\Bin\MSBuild.exe" "%%~I\MSBuild\15.0\Bin\MSBuild.exe") do if not defined MSBUILD_EXE if exist "%%~B" (
            set "MSBUILD_EXE=%%~B"
            set "MSBUILD_MODE=VS"
        )
    )
)

rem 2c) known install paths
if not defined MSBUILD_EXE (
    for %%B in (
        "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
        "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    ) do if not defined MSBUILD_EXE if exist "%%~B" (
        set "MSBUILD_EXE=%%~B"
        set "MSBUILD_MODE=VS"
    )
)

rem 2d) dotnet SDK fallback: legacy WPF needs the SDK WinFX targets wired in
if not defined MSBUILD_EXE (
    set "DOTNET="
    for /f "delims=" %%I in ('where dotnet.exe 2^>nul') do if not defined DOTNET set "DOTNET=%%~I"
    if not defined DOTNET if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET=%ProgramFiles%\dotnet\dotnet.exe"
    if defined DOTNET (
        for /f "delims=" %%L in ('"!DOTNET!" --list-sdks 2^>nul') do set "SDKLINE=%%L"
        if defined SDKLINE (
            for /f "tokens=1 delims= " %%A in ("!SDKLINE!") do set "SDKVER=%%A"
            set "SDKROOT=!SDKLINE:*[=!"
            set "SDKROOT=!SDKROOT:]=!"
            set "SDKDIR=!SDKROOT!\!SDKVER!"
            set "WINFX=!SDKDIR!\Sdks\Microsoft.NET.Sdk.WindowsDesktop\targets\Microsoft.WinFX.targets"
            if exist "!WINFX!" (
                set "MSBUILD_EXE=!DOTNET!"
                set "MSBUILD_MODE=DOTNET"
                set "MSBUILD_WINFX=!WINFX!"
            )
        )
    )
)

if not defined MSBUILD_EXE (
    echo [ERROR] MSBuild engine not found. Install one of:
    echo   1. Visual Studio 2022+ with ".NET desktop development" workload, or
    echo   2. Visual Studio Build Tools with MSBuild + .NET Framework 4.8 targeting pack, or
    echo   3. .NET SDK 6.0 or later ^(used via "dotnet msbuild" + Build\wpf-legacy.targets^).
    exit /b 2
)
echo [INFO] MSBuild: "%MSBUILD_EXE%" (mode: %MSBUILD_MODE%)

rem ---------- 3) build Release ----------
echo [BUILD] Release / Any CPU
if /i "%MSBUILD_MODE%"=="DOTNET" (
    "%MSBUILD_EXE%" msbuild "%SLN%" /m /t:Clean,Build /p:Configuration=Release "/p:Platform=Any CPU" "/p:CustomAfterMicrosoftCommonTargets=%BUILD_DIR%wpf-legacy.targets" "/p:WpfSdkWinFXTargets=%MSBUILD_WINFX%" /verbosity:minimal /nologo
) else (
    "%MSBUILD_EXE%" "%SLN%" /m /t:Clean,Build /p:Configuration=Release "/p:Platform=Any CPU" /p:RestorePackages=false /verbosity:minimal
)
if errorlevel 1 (
    echo [ERROR] Build failed.
    exit /b 3
)
echo [OK] Build succeeded.

rem ---------- 4) stage publish folder ----------
if exist "%PUBLISH%" rmdir /s /q "%PUBLISH%"
if exist "%PUBLISH%" (
    ping -n 2 127.0.0.1 >nul
    rmdir /s /q "%PUBLISH%"
)
if not exist "%PUBLISH%" mkdir "%PUBLISH%"
if exist "%PUBLISH%" echo [WARN] Could not fully clear the Publish staging folder; existing files will be overwritten.
if not exist "%ROOT%\CursorWpf.App\bin\Release\CursorWpf.exe" (
    echo [ERROR] Release EXE not found: CursorWpf.App\bin\Release\CursorWpf.exe
    exit /b 4
)
copy /y "%ROOT%\CursorWpf.App\bin\Release\CursorWpf.exe" "%PUBLISH%\" >nul
if exist "%ROOT%\CursorWpf.App\bin\Release\CursorWpf.exe.config" copy /y "%ROOT%\CursorWpf.App\bin\Release\CursorWpf.exe.config" "%PUBLISH%\" >nul
xcopy /e /i /y "%ROOT%\Docs" "%PUBLISH%\Docs" >nul
copy /y "%ROOT%\README.md" "%PUBLISH%\README.md" >nul
echo [OK] Publish folder staged.

rem ---------- 5) package ZIP: Release\CursorWpf_V<version>.zip ----------
set "NAME=CursorWpf_V%VERSION%"
if not exist "%RELEASE%" mkdir "%RELEASE%"
if exist "%RELEASE%\%NAME%.zip" del /q "%RELEASE%\%NAME%.zip"
powershell -NoProfile -ExecutionPolicy Bypass -Command "Compress-Archive -Path '%PUBLISH%\*' -DestinationPath '%RELEASE%\%NAME%.zip' -Force"
if errorlevel 1 (
    echo [ERROR] ZIP packaging failed.
    exit /b 5
)
echo [OK] Created %RELEASE%\%NAME%.zip

rem ---------- 6) package EXE installer: Release\CursorWpf_V<version>.exe ----------
set "ISCC="
where ISCC.exe >nul 2>nul && set "ISCC=ISCC.exe"
if not defined ISCC if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if defined ISCC goto :pack_exe
echo [WARN] Inno Setup 6 not found, EXE installer skipped. ZIP package is still created.
echo         Install Inno Setup 6 from https://jrsoftware.org/isinfo.php to build the EXE.
goto :pack_msi

:pack_exe
"%ISCC%" /DAppVersion=%VERSION% "%ROOT%\Installer\CursorWpf.iss"
if errorlevel 1 (
    echo [ERROR] Inno Setup packaging failed.
    exit /b 6
)
echo [OK] Created %RELEASE%\%NAME%.exe

:pack_msi
rem ---------- 7) package MSI installer via WiX Toolset 5 ----------
set "DOTNET_EXE="
for /f "delims=" %%I in ('where dotnet.exe 2^>nul') do if not defined DOTNET_EXE set "DOTNET_EXE=%%~I"
if not defined DOTNET_EXE if exist "%ProgramFiles%\dotnet\dotnet.exe" set "DOTNET_EXE=%ProgramFiles%\dotnet\dotnet.exe"
if not defined DOTNET_EXE (
    echo [ERROR] .NET SDK not found. Install .NET 8 SDK to build the MSI package.
    exit /b 7
)
echo [MSI] Building CursorWpf MSI with WiX Toolset 5 SDK...
"%DOTNET_EXE%" build "%ROOT%\CursorWpf.MSI\CursorWpf.MSI.wixproj" -c Release -p:AppVersion=%VERSION% -p:PublishDir="%PUBLISH%" -p:OutputPath="%RELEASE%\"
if errorlevel 1 (
    echo [ERROR] MSI packaging failed.
    exit /b 7
)
if not exist "%RELEASE%\%NAME%.msi" (
    echo [ERROR] MSI output missing: %RELEASE%\%NAME%.msi
    exit /b 7
)
for %%F in ("%RELEASE%\%NAME%.msi") do if %%~zF LEQ 0 (
    echo [ERROR] MSI output is empty.
    exit /b 7
)
echo [OK] Created %RELEASE%\%NAME%.msi

:summary
rem ---------- 8) clean temp staging folder ----------
if exist "%PUBLISH%" (
    rmdir /s /q "%PUBLISH%" 2>nul
    if exist "%PUBLISH%" (
        ping -n 2 127.0.0.1 >nul
        rmdir /s /q "%PUBLISH%" 2>nul
    )
)
if exist "%PUBLISH%" (
    echo [WARN] Temp Publish folder could not be fully removed. Delete it manually.
) else (
    echo [OK] Temp Publish folder cleaned.
)

echo ------------------------------------------------------------
echo [SUCCESS] Artifacts:
if exist "%RELEASE%\%NAME%.zip" echo   %RELEASE%\%NAME%.zip
if exist "%RELEASE%\%NAME%.exe" echo   %RELEASE%\%NAME%.exe
if exist "%RELEASE%\%NAME%.msi" echo   %RELEASE%\%NAME%.msi
echo ------------------------------------------------------------
endlocal
exit /b 0
