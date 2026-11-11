# CursorWpf.MSI

WiX Toolset 4 SDK 项目，为 CursorWpf (.NET Framework 4.8) 生成标准 Windows Installer（MSI）。

## 构建

先完成主程序 Release 编译，并将运行文件和文档暂存到仓库根目录的 `Publish` 文件夹，然后执行：

```powershell
dotnet build .\CursorWpf.MSI\CursorWpf.MSI.wixproj -c Release -p:AppVersion=1.1.0 -p:PublishDir="$PWD\Publish" -p:OutputPath="$PWD\Release\"
```

输出：`Release\CursorWpf_V1.1.0.msi`。

要求：安装 .NET 8 SDK（用于构建 WiX SDK 项目）。目标应用仍为 .NET Framework 4.8，不会改变主程序运行时。

## 安装行为

- 安装到 64 位 Program Files 下的 CursorWpf 目录。
- 创建开始菜单快捷方式。
- 支持同一 UpgradeCode 的大版本升级和旧版本卸载。
- 阻止降级覆盖较新版本。
- 使用嵌入式 CAB，MSI 可独立分发。
