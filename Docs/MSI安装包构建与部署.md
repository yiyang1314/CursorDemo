# MSI 安装包构建与部署

## 项目结构

- `CursorWpf.App`：WPF 主程序，目标框架 .NET Framework 4.8。
- `CursorWpf.MSI`：WiX Toolset 4 SDK 安装包工程。
- `Installer/CursorWpf.iss`：原有 Inno Setup EXE 安装程序，继续保留。

## 构建流程

1. 使用 Visual Studio MSBuild 编译 `CursorWpf.sln` 的 Release 配置。
2. 将主程序输出及 Docs 文档复制到根目录 `Publish`。
3. 生成 ZIP 便携包。
4. 使用 Inno Setup 6 生成 EXE 安装程序（本机安装了 Inno Setup 时）。
5. 使用 .NET 8 SDK 和 WiX Toolset 4 SDK 项目生成 MSI 安装程序。

## 版本命名

版本统一读取 `Build/version.txt`，格式为三段数字，例如 `1.1.0`。

- `CursorWpf_V1.1.0.zip`
- `CursorWpf_V1.1.0.exe`
- `CursorWpf_V1.1.0.msi`

## MSI 安装特性

MSI 安装到 64 位 Program Files，创建开始菜单快捷方式，支持 Major Upgrade 和阻止降级安装。MSI 使用嵌入式 CAB，不依赖旁置 CAB 文件。

## 本机环境

- Visual Studio 2022 / MSBuild
- .NET Framework 4.8 Developer Pack
- .NET 8 SDK（用于 WiX SDK 工程）
- Inno Setup 6（可选；用于 EXE 安装包）

## 注意

MSI 仅在主程序 Release 编译成功且 Publish 目录完整后构建。发布流水线会检查 MSI 文件存在且非空后才上传 Release。
