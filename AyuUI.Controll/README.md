# AyuUI.Controll

> AyuUI 是一套面向 WPF 的自研 UI 框架：提供可换风格的窗口标题栏、主题化控件样式，以及轻量 IOC、消息总线、Toast、Dialog 等 MVVM 基础设施。不依赖任何第三方 UI 库（如 MahApps、HandyControl）。

本仓库中的 `JDCloudChat` 即为使用该框架的真实宿主示例。
> 仓库代码：https://github.com/yiyang1314/CursorDemo.git
> - AyuUI.Controll：UI框架库
> - CursorWpf.App： Demo Cursor示例
> - JDCloudChat： 京东云AI API接口聊天demo
---

## 1. 目标框架

| 目标框架 | 说明 | 宿主选择 |
| --- | --- | --- |
| `net48` | .NET Framework 4.8 WPF 程序集 | 传统 WPF 工程（packages.config / HintPath）自动选中 `lib\net48` |
| `net6.0-windows` | .NET 6 WPF | SDK 风格工程自动选中 `lib\net6.0-windows7.0` |
| `net8.0-windows` | .NET 8 WPF | SDK 风格工程自动选中 `lib\net8.0-windows7.0` |

- 三套 dll **内置同一套主题 BAML 资源**（`Themes/GenericTheme.xaml`、`Pages/AyuToastContainer.xaml` 等），宿主只需引用 dll，无需额外拷贝 xaml 文件。
- 各框架的依赖差异已在包内按 TFM 分组处理：

| 依赖 | net48 | net6.0-windows | net8.0-windows |
| --- | --- | --- | --- |
| `Newtonsoft.Json` 13.0.4 | ✔ | ✔ | ✔ |
| `System.Text.Json` | 8.0.5 | 内置 | 内置 |
| `System.Configuration.ConfigurationManager` | 框架自带的 System.Configuration | 6.0.0 | 8.0.0 |
| `System.Drawing.Common` | 框架自带的 System.Drawing | 6.0.0 | 8.0.0 |
| `Microsoft.WindowsDesktop.App.WPF` | —（框架引用） | ✔ | ✔ |

---

## 2. 安装与引用

### 2.1 NuGet 包

```powershell
dotnet add package AyuUI.Controll --version 1.0.0
```

传统（非 SDK）工程也可以直接引用本地包目录：

```xml
<Reference Include="AyuUI.Controll">
  <HintPath>..\packages\AyuUI.Controll.1.0.0\lib\net48\AyuUI.Controll.dll</HintPath>
</Reference>
```

`packages.config` 方式：

```xml
<packages>
  <package id="AyuUI.Controll" version="1.0.0" targetFramework="net48" />
</packages>
```

### 2.2 源码工程引用（本仓库内调试）

```xml
<ProjectReference Include="..\AyuUI.Controll\AyuUI.Controll.csproj" />
```

### 2.3 XAML 命名空间

```xml
xmlns:control="clr-namespace:AyuUI.Controll.Controls;assembly=AyuUI.Controll"
```

> ⚠️ 主题资源随 dll 一起分发，但 **宿主必须引用与当前源码一致的包版本**。若 XAML 报
> `Cannot find resource named 'BoolToVisibilityConverter'`，通常不是 Pack URI 写错，
> 而是宿主引用的是旧版 `AyuUI.Controll.dll`（BAML 里还没有该资源）。重新打包并刷新
> 本地 `packages` 目录即可，详见第 10 节。

---

## 3. 主题接入（窗口主题）

### 3.1 在 App.xaml 合并主题（必须）

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/AyuUI.Controll;component/Themes/GenericTheme.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 3.2 窗口级自动加载（防御性）

`CustomWindow` / `BaseWindow` 在构造时会自行加载同一份主题字典并套用窗口样式，
因此即使宿主忘记在 App.xaml 合并，窗口外观仍然可用；但页面内 `{StaticResource ...}`
引用主题资源时仍需要 App 级合并，推荐两者都保持。

### 3.3 主题提供的资源键

| 键 | 类型 | 用途 |
| --- | --- | --- |
| `KeycapDarkMenuStyle` / `KeycapSubMenuItemStyle` | Menu / MenuItem | 现代按键胶囊风菜单 |
| `VsDarkMenuStyle` / `VsSubMenuItemStyle` | Menu / MenuItem | Visual Studio 风菜单 |
| `TitleBarBtn` / `CloseWinBtn` | Button | 标题栏图标按钮 / 关闭按钮 |
| `TitleBarButtonStyle` / `CloseButtonStyle` | Button | BaseWindow 标题栏按钮 |
| `CommonToolsTemplate` / `SystemWindowButtonsTemplate` | ControlTemplate | 标题栏左侧工具区 / 右侧窗口按钮组 |
| `PortalSearchBoxStyle` | TextBox | ModernPortal 风格搜索框 |
| `BoolToVisibilityConverter` / `ShortcutKeyConverter` | IValueConverter | 布尔转可见性 / 快捷键文本 |
| `{x:Static MenuItem.SeparatorStyleKey}` | Separator | 深色菜单分割线 |

另有 4 个隐式样式（无需 Key）：`CustomWindow`、`BaseWindow`、`AyuToggleSwitch`、`AyuStateIndicator`。

---

## 4. 窗口

### 4.1 CustomWindow（四种标题栏风格）

```xml
<control:CustomWindow x:Class="Chat.MainWindow"
                      StyleMode="MinimalEditor"
                      Title="Gemini AI 桌面助手"
                      ToggleSidebarCommand="{Binding ToggleThemeCommand}">

    <control:CustomWindow.TitleBarMenuContent>
        <Menu Style="{StaticResource KeycapDarkMenuStyle}" VerticalAlignment="Center">
            <MenuItem Header="文件(F)" >
                <MenuItem Header="新建(N)" />
                <MenuItem Header="保存(G)" />
                <MenuItem Header="另存为(P)" />
                <MenuItem Header="撤销(H)" />
            </MenuItem>
            <MenuItem Header="视图(V)" />
            <MenuItem Header="Git(G)" />
            <MenuItem Header="项目(P)" />
            <MenuItem Header="帮助(H)" >
                <MenuItem Header="注册(N)" />
                <MenuItem Header="文档(G)" />
                <MenuItem Header="关于(P)" />
            </MenuItem>
        </Menu>
    </control:CustomWindow.TitleBarMenuContent>

    <!-- 页面内容 -->
</control:CustomWindow>
```

`StyleMode` 取值：

| 枚举 | 风格 |
| --- | --- |
| `TitleBarStyleMode.ModernTool` | 图标 + 标题 + 右侧功能按钮（默认） |
| `TitleBarStyleMode.VisualStudio` | Logo + 全菜单 + 搜索 + 项目徽章 + 头像 |
| `TitleBarStyleMode.MinimalEditor` | 侧边栏开关 + 搜索 + 精简菜单 |
| `TitleBarStyleMode.ModernPortal` | 多色 Logo + 导航 + 高亮按钮 + 搜索框 |

常用属性：`AppIcon`、`TitleBarMenuContent`、`ProjectBadgeText`、`HighlightActionText`、
`SearchText`、`UserAvatar`、`IsDarkTheme`、`LanguageText`；命令：
`SettingsCommand`、`ToggleThemeCommand`、`HelpCommand`、`ToggleLanguageCommand`、
`ToggleSidebarCommand`、`HighlightActionCommand`、`SearchCommand`。

### 4.2 BaseWindow（经典标题栏）

```xml
<control:BaseWindow x:Class="Demo.Theme1Window"
                    IsDarkTheme="{Binding IsDarkTheme}"
                    LanguageText="{Binding CurrentLangText}"
                    SettingsCommand="{Binding OpenSettingsCommand}"
                    ToggleThemeCommand="{Binding ToggleThemeCommand}"
                    HelpCommand="{Binding OpenHelpCommand}"
                    ToggleLanguageCommand="{Binding SwitchLanguageCommand}" />
```

---

## 5. 控件

### 5.1 AyuNavigationMenu

```xml
<control:AyuNavigationMenu ItemsSource="{Binding MenuItems}"
                           SelectedItem="{Binding SelectedMenuItem}"
                           Position="Left" />
```

- `Position`：`Left` / `Top` / `Bottom`
- `NavigationItem`：`Id`、`Title`、`IconData`、`IsSelected`、`Command`

### 5.2 AyuToggleSwitch / AyuStateIndicator

```xml
<control:AyuToggleSwitch IsChecked="{Binding IsDarkTheme}" />
<control:AyuStateIndicator State="{Binding ConnectionState}" />
```

`IndicatorState` 用于表示在线/离线/警告等状态；两者的外观由主题隐式样式提供。

---

## 6. IOC（注解注册）

```csharp
using AyuUI.Controll.IOC;
using AyuUI.Controll.IOC.Attributes;

[Service(ServiceLifetime.Singleton)]
public class ChatViewModel : ViewModelBase { }

public class MainWindowViewModel : ViewModelBase
{
    [Inject] public ChatViewModel? Chat { get; set; }
}
```

```csharp
var container = AyuContainer.Default;

// 扫描程序集内所有 [Service] 类型并按生命周期注册
container.RegisterFromAssembly(typeof(App).Assembly);
container.RegisterSingleton<AyuMessenger>();
// 手动注册
container.RegisterSingleton<AyuMessenger>();
container.RegisterSingleton<IDialogService, DialogService>();
container.RegisterTransient<MyService>();
container.RegisterInstance(new ConfigService());

// 解析（构造注入 + 属性注入）
var vm = container.Resolve<MainWindowViewModel>();
```

- 生命周期：`ServiceLifetime.Singleton` / `Transient`
- 命名注册/解析：`RegisterSingleton<TService, TImpl>("name")` + `Resolve<T>("name")`
- `InjectProperties(target)` / `[Inject]` 支持属性注入

---

## 7. 消息总线

```csharp
using AyuUI.Controll.Messaging;

// 订阅（recipient 用于批量取消订阅，弱引用，不会阻止 GC）
AyuMessenger.Default.Subscribe<ThemeChangedMessage>(
    this,
    msg => IsDarkTheme = msg.IsDark,
    ThreadOption.UIThread);

// 发布
AyuMessenger.Default.Publish(new ThemeChangedMessage(true));

// 取消订阅
AyuMessenger.Default.Unsubscribe<ThemeChangedMessage>(this);
AyuMessenger.Default.Unsubscribe(this);
```

- `ThreadOption`：`PublisherThread`（默认）、`UIThread`、`BackgroundThread`
- 支持 `token` 区分同类型消息的不同订阅组
- 内置消息示例：`ThemeChangedMessage`、`DeviceStatusMessage`

---

## 8. Dialog

```csharp
using AyuUI.Controll.Dialogs;

// 静态调用
AyuMessageBox.Info("保存成功");
AyuMessageBox.Error("网络异常", "请求失败");
bool ok = AyuMessageBox.Confirm("确定删除该会话？");
string? name = AyuMessageBox.Input("请输入会话名称", defaultValue: "新会话");

// 加载遮罩
using (AyuMessageBox.ShowLoading("正在处理中..."))
{
    // 耗时操作
}
```

服务化用法（可注入到 ViewModel）：

```csharp
container.RegisterSingleton<IDialogService, DialogService>();
```

```csharp
public class FooViewModel
{
    [Inject] public IDialogService? Dialogs { get; set; }

    public void Save()
    {
        if (!Dialogs!.Confirm("确认保存？")) return;
        Dialogs.Info("已保存");
    }
}
```

`LoadingController` 实现 `IDisposable`，`Dispose()`/`Close()` 关闭遮罩。

---

## 9. Toast

```csharp
using AyuUI.Controll.Toasts;

AyuToast.Success("保存成功");
AyuToast.Info("已复制到剪贴板", "提示");
AyuToast.Warning("连接不稳定", duration: 4);
AyuToast.Error("请求失败", "错误", 5, ToastPosition.BottomRight);
```

- 位置：`TopRight`、`BottomRight`（默认）、`TopLeft`、`BottomLeft`、`TopCenter`、`BottomCenter`
- 类型：`Success`、`Info`、`Warning`、`Error`、`Loading`
- Toast 由 `Pages/AyuToastContainer.xaml` 渲染，其样式来自主题，需保证主题已合并（见第 3 节）

---

## 10. 常见问题

**Q1. 运行时抛 `XamlParseException` / `Cannot find resource named 'xxx'`？**

按顺序排查：

1. `App.xaml` 是否合并了 `pack://application:,,,/AyuUI.Controll;component/Themes/GenericTheme.xaml`；
2. 宿主引用的 `AyuUI.Controll.dll` 是否为当前源码打包的最新版本（对比 dll 时间戳与主题源码时间戳）；
3. 包版本是否重复使用同一个版本号导致 `packages` 目录里是旧 dll —— 刷新包目录或升级 `<Version>`；
4. 用以下方式直接验证 dll 内 BAML 是否包含主题资源：
   `AyuUI.Controll.dll` 的 `AyuUI.Controll.g.resources` 中应存在 `themes/generictheme.baml`。

**Q2. 传统 .NET Framework 宿主如何选择正确程序集？**

引用 `lib\net48\AyuUI.Controll.dll`。SDK 风格工程引用包后由 NuGet 自动按 TFM 选择，无需手动指定。

**Q3. 打包产物在哪里？**

执行仓库根目录 `Build\build.bat` 或 `dotnet build AyuUI.Controll\AyuUI.Controll.csproj -c Release`，
产物为 `AyuUI.Controll\bin\Release\AyuUI.Controll.<Version>.nupkg`，一键脚本会额外复制到 `Release\`。

---

## 11. 其他能力

| 组件 | 说明 |
| --- | --- |
| `Commands` | `RelayCommand` / `AsyncRelayCommand`（含泛型参数版本） |
| `ViewModels` | `ObservableObject` / `ViewModelBase`（属性通知、命令辅助） |
| `Convertors` | `BoolToVisibilityConverter`、`BoolToIndicatorStateConverter`、`NullOrZeroToCollapsedConverter` |
| `Helpers` | `JsonHelper`、`ConfigHelper`、`EncryptHelper`、`ImageHelper`（水印/透明度）、`ZipHelper`、`XmlHelper`、`LoggerHelper`、`AiChatHelper`、`ByteConverterHelper` |
| `Pages` | `AyuMessageWindow`（对话框）、`AyuToastContainer`（Toast 容器） |
