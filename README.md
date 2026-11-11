# CursorWpf

**CursorWpf 1.1.0** is a desktop IDE-style workspace prototype built with **WPF + .NET Framework 4.8 + MVVM**.

## Highlights
- Custom Windows title bar and app icon; three-column workspace (activity rail, explorer, AI chat, editor).
- Dark / Light palettes with high-contrast editor text.
- Local file/folder open, text editing and save; image preview; user profile popup.
- Chinese/English switching command foundation; help documentation.
- Local build/package scripts and GitHub Actions CI/release workflow.

> AI chat is explicitly a local UI demo in this version. No model endpoint is configured and no remote AI response is simulated as a real service.

## Requirements
- Windows 10/11
- Visual Studio 2019 or 2022 with .NET desktop development workload
- .NET Framework 4.8 Developer Pack / Targeting Pack
- Optional: Inno Setup 6 for the EXE installer

## Build and package
Run `Build\\build_all.bat`. It locates MSBuild via PATH, `vswhere.exe`, and common Visual Studio paths, then compiles Release, stages `Publish`, and creates `Release\\CursorWpf_V1.1.0.zip`. If Inno Setup 6 is installed, it also creates the EXE installer.

## GitHub CI and release
- Push/PR to `main`: Windows 2022 build + ZIP artifact.
- Push a version tag such as `v1.1.0`: build + ZIP + Inno Setup EXE + GitHub Release.
- Update `Build/version.txt` before tagging. The release filename convention is `CursorWpf_VX.x.x.zip` and `CursorWpf_VX.x.x.exe`.

## Project layout
- `CursorWpf.App/` WPF application, MVVM view model, services, themes and assets.
- `Docs/` requirements, architecture, usage and build documentation.
- `Build/` version and local build/package scripts.
- `Installer/` Inno Setup configuration.
- `.github/workflows/` CI and release automation.
