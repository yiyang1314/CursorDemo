# 自定义控件
## 1 功能介绍
- 自定义Windows 标题栏
- 自定义菜单风格
- 自定义命令
- 自定义属性通知
- 自定义IOC
- 消息发布与订阅
- 自定box消息弹框
- 自定Toast消息弹框

## 2 安装依赖

> 引入类库
```cs
donet add package AyuUI.Controll 1.0.1
```
> App.xaml引用
```xml
    <Application.Resources>
        <!-- 引入你的通用样式或主题资源字典 -->
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/AyuUI.Controll;component/Themes/Generic.xaml" />
                <ResourceDictionary Source="pack://application:,,,/AyuUI.Controll;component/Themes/GenericTheme.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
```

> 界面引用
```xml
xmlns:controls="clr-namespace:AyuUI.Controll.Controls"
```

## 3 功能组件
### 3.1 Theme1Window 标题栏
> windows主题使用
```xml
<control:BaseWindow x:Class="AyuUI.Controll.Simples.Theme1Window"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:AyuUI.Controll.Simples"
        xmlns:control="clr-namespace:AyuUI.Controll.Controls"
        mc:Ignorable="d"
        Width="1100"
        Height="700"
        WindowStartupLocation="CenterScreen"
        IsDarkTheme="{Binding IsDarkTheme}"
        LanguageText="{Binding CurrentLangText}"
        SettingsCommand="{Binding OpenSettingsCommand}"
        ToggleThemeCommand="{Binding ToggleThemeCommand}"
        HelpCommand="{Binding OpenHelpCommand}"
        ToggleLanguageCommand="{Binding SwitchLanguageCommand}"/>
```

### 3.2 CustomWindow 标题栏
> windows主题使用
```xml
<controls:CustomWindow x:Class="AyuUI.Controll.Simples.AyuSimpleWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:AyuUI.Controll.Simples"
        mc:Ignorable="d"
        xmlns:controls="clr-namespace:AyuUI.Controll.Controls"
        x:Name="RootWin"
        Title="WPF UI"
        Width="1150"
        Height="700"
        StyleMode="VisualStudio"
        ProjectBadgeText="CursorWpf"
        HighlightActionText="Sponsor"
        LanguageText="{Binding CurrentLangText}"
        SettingsCommand="{Binding OpenSettingsCommand}"
        ToggleThemeCommand="{Binding ToggleThemeCommand}"
        HelpCommand="{Binding OpenHelpCommand}"
        ToggleLanguageCommand="{Binding SwitchLanguageCommand}"/>
```
StyleMode样式风格支持四种：
- ModernTool: 风格1：现代工具栏风格 (原图1：图标+标题 + 右侧功能按钮
- VisualStudio: 风格2：Visual Studio 风格 (图1：Logo+全菜单+搜索+项目胶囊徽章+头像)
- MinimalEditor: 风格3：极简编辑器风格 (图2：侧边栏开关+搜索图标+竖线+精简菜单)
- ModernPortal: 风格4：现代门户导航风格 (图3：多色Logo+导航菜单+主题/GitHub+高亮按钮+搜索框)

### 3.3 自定义菜单

> 菜单样式支持两种：VsDarkMenuStyle KeycapDarkMenuStyle
```xml
    <!-- 传入顶部菜单栏 (适用于 VisualStudio / MinimalEditor / ModernPortal 风格) -->
    <controls:CustomWindow.TitleBarMenuContent>
        <!-- KeycapDarkMenuStyle VsDarkMenuStyle-->
        <Menu Style="{StaticResource KeycapDarkMenuStyle}" VerticalAlignment="Center">
            <MenuItem Header="视图(V)" />
            <MenuItem Header="Git(G)" />
            <MenuItem Header="项目(P)" />
            <MenuItem Header="帮助(H)" />
        </Menu>
    </controls:CustomWindow.TitleBarMenuContent>
```

## 4 自定义IOC
### 4.1 定义注入类
> Service：标记IOC；Inject：依赖注入
```cs
[Service(ServiceLifetime.Transient)]
    public class MainViewModel : ObservableObject
    {
        private readonly ILogService _logger;
        private readonly ISerialService _plcPort;
        private readonly ISerialService _scannerPort;

        // 支持属性按名注入（甚至可以直接声明具体类 SerialService）
        [Inject("TempDebug")]
        public SerialService? DebugSerial { get; set; }

        // 构造函数混合注入：普通服务 + 同接口同派生类的不同命名实例
        public MainViewModel(
            ILogService logger,
            [Inject("PLC")] ISerialService plcPort,
            [Inject("Scanner")] ISerialService scannerPort)
        {
            _logger = logger;
            _plcPort = plcPort;
            _scannerPort = scannerPort;

            ExecuteCommand = new RelayCommand(() =>
            {
                _logger.Write("开始多设备通信...");
                var msg1 = _plcPort.Send("READ_STATUS");
                var msg2 = _scannerPort.Send("SCAN_CODE");
                var msg3 = DebugSerial?.Send("PING");

                StatusText = $"{msg1}\n{msg2}\n{msg3}";
            });
        }

        private string _statusText = "就绪";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public RelayCommand ExecuteCommand { get; }
    }
```

### 4.2 手动依赖注入
> 动按名称注册：接口相同(ISerialService)、派生类也相同(SerialService)的多个独立实例
```cs
            // 2. 手
            container.RegisterSingleton<ISerialService, SerialService>("PLC", c => new SerialService("COM1", 9600));
            container.RegisterSingleton<ISerialService, SerialService>("Scanner", c => new SerialService("COM3", 115200));
            container.RegisterTransient<ISerialService, SerialService>("TempDebug", c => new SerialService("COM9", 38400));

```
### 4.3 IOC依赖注册
> 自动扫描当前程序集内所有标记了 [Service] 的类
```cs
        private static void ConfigureServices()
        {
            var container = AyuContainer.Default;

            // 1. 自动扫描当前程序集内所有标记了 [Service] 的类
            container.RegisterFromAssembly(Assembly.GetExecutingAssembly());

            // 2. 手动按名称注册：接口相同(ISerialService)、派生类也相同(SerialService)的多个独立实例
            container.RegisterSingleton<ISerialService, SerialService>("PLC", c => new SerialService("COM1", 9600));
            container.RegisterSingleton<ISerialService, SerialService>("Scanner", c => new SerialService("COM3", 115200));
            container.RegisterTransient<ISerialService, SerialService>("TempDebug", c => new SerialService("COM9", 38400));
        }

```

### 4.4 IOC实列获取
```cs
AyuContainer.Default.Resolve<AyuSimpleWindow>();
```

## 5 自定义消息发布和订阅

### 5.1 消息注册
```cs
    /// <summary>
    /// 轻量级弱引用事件聚合器（支持自动注册到 IoC 容器）
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public sealed class AyuMessenger : IMessenger{}

```

### 5.2 依赖注入

```cs
	[Service(ServiceLifetime.Singleton)]
    public class DeviceMonitorService
    {
        private readonly IMessenger _messenger;

        // 通过 IoC 自动注入 IMessenger
        public DeviceMonitorService(IMessenger messenger)
        {
            _messenger = messenger;
        }
    }

```

### 5.3 消息发布
```cs
    [Service(ServiceLifetime.Singleton)]
    public class DeviceMonitorService
    {
        public async Task StartPollAsync()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                // 在后台线程发布消息到 "PLC_Channel" 频道
                _messenger.Publish(new DeviceStatusMessage("PLC-01", "运行正常", 42.5), token: "PLC_Channel");
            });
        }
    }

```

### 5.4 消息订阅
```cs
    [Service(ServiceLifetime.Singleton)]
    public void DeviceMonitorService ()
    {
            // 1. 订阅 "PLC_Channel" 频道消息，并指定 ThreadOption.UIThread
            // 即使后台线程发布，回调也会自动切回 WPF UI 主线程执行，安全更新绑定属性！
            _messenger.Subscribe<DeviceStatusMessage>(
                recipient: this,
                token: "PLC_Channel",
                action: OnDeviceStatusReceived,
                threadOption: ThreadOption.UIThread);

            // 2. 订阅全局主题切换消息
            _messenger.Subscribe<ThemeChangedMessage>(this, msg =>
            {
                DeviceInfo = $"当前主题已切换为: {(msg.IsDarkTheme ? "深色" : "浅色")}";
            });
    }

```

## 6 自定义box消息弹框

### 6.1 服务注册
> 方式1注册服务
```cs
/// <summary>
    /// 弹窗服务实现（自动注册到 IoC 容器，可在 ViewModel 中直接注入 IDialogService）
    /// </summary>
    [Service(ServiceLifetime.Singleton)]
    public class DialogService : IDialogService
    {
        public void Info(string message, string title = "提示") => AyuMessageBox.Info(message, title);
        public void Error(string message, string title = "错误") => AyuMessageBox.Error(message, title);
        public bool Confirm(string message, string title = "确认操作") => AyuMessageBox.Confirm(message, title);
        public string? Input(string message, string title = "请输入", string placeholder = "请输入内容...", string defaultValue = "")
            => AyuMessageBox.Input(message, title, placeholder, defaultValue);
        public LoadingController ShowLoading(string message = "正在加载中，请稍候...") => AyuMessageBox.ShowLoading(message);
    }

```

> 方式2：直接使用AyuMessageBox
```csharp
        AyuMessageBox.Info(message, title);
        AyuMessageBox.Error(message, title);
        AyuMessageBox.Confirm(message, title);
        AyuMessageBox.Input(message, title, placeholder, defaultValue);
        AyuMessageBox.ShowLoading(message);
```

### 6.2 依赖注入
```csharp
    private readonly IDialogService _dialog;
    public MainViewModel(IMessenger messenger,IDialogService dialog)
    {
        _messenger = messenger;
        _dialog = dialog;
    }

```

### 6.3 消息通知
```cs
  _dialog.Info($"当前主题已切换为: {CurrentLangText}");

```

## 7 自定义TOast弹框
### 7.1 自定义TOast弹框
```cs

        private void OnDeviceStatusReceived(DeviceStatusMessage message)
        {
            DeviceInfo = $"当前主题已切换为: {message.DeviceName}";
            Task.Delay(1000).Wait();
            AyuToast.Success("PLC 设备连接成功！", "通信正常");
            Task.Delay(1000).Wait();
            AyuToast.Info("系统检测到有新的固件版本可供升级。", "更新提示");
            Task.Delay(1000).Wait();
            AyuToast.Warning("当前主轴温度已达到 78°C，请注意散热。", "温度预警");
            Task.Delay(1000).Wait();
            AyuToast.Error("配方参数保存至数据库失败：主键冲突。", "存储异常", duration: 6, ToastPosition.TopRight);
        }
```

## 8 其他组件
### 8.1 状态机
```xml
            <StackPanel Orientation="Vertical" HorizontalAlignment="Center" VerticalAlignment="Center">

                <!-- 1. 多种状态机指示灯展示 -->
                <Border Background="#1E2026" BorderBrush="#2F3340" BorderThickness="1" CornerRadius="10" Padding="20,16" Margin="0,0,0,20">
                    <StackPanel Orientation="Horizontal">
                        <control:AyuStateIndicator State="Offline" Label="离线" Margin="0,0,20,0" />
                        <control:AyuStateIndicator State="Idle" Label="待机就绪" Margin="0,0,20,0" />
                        <control:AyuStateIndicator State="Running" Label="自动运行中" Margin="0,0,20,0" />
                        <control:AyuStateIndicator State="Warning" Label="温度预警" Margin="0,0,20,0" />
                        <control:AyuStateIndicator State="Error" Label="急停报警" />
                    </StackPanel>
                </Border>

                <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center" Orientation="Horizontal">

                    <!-- 示例 1：绑定布尔属性 IsDeviceConnected (true=绿灯 Running, false=灰灯 Offline) -->
                    <control:AyuStateIndicator State="{Binding IsDeviceConnected, Converter={StaticResource BoolToState}}"
                                    Label="设备通信连接状态"
                                    Margin="0,0,0,12" />

                    <!-- 示例 2：绑定布尔属性 HasAlarm 且使用参数 "Error" (true=红灯闪烁 Error, false=绿灯 Running) -->
                    <control:AyuStateIndicator State="{Binding HasAlarm, Converter={StaticResource BoolToState}, ConverterParameter='Error'}"
                                    Label="系统急停报警指示"
                                    Margin="0,0,0,12" />

                    <!-- 示例 3：配合前面的拨动开关，实现开关闭合时指示灯实时联动 -->
                    <StackPanel Orientation="Horizontal">
                        <control:AyuToggleSwitch x:Name="MySwitch" Margin="0,0,12,0" />
                        <control:AyuStateIndicator State="{Binding IsChecked, ElementName=MySwitch, Converter={StaticResource BoolToState}}"
                                        Label="开关实时状态" />
                    </StackPanel>
                    <!-- ========================================================== -->
                    <!-- 1. bool 转可见/折叠示例                                       -->
                    <!-- ========================================================== -->
                    <!-- 当 IsLoading 为 true 时显示此面板 -->
                    <Border Background="#1E2026" Padding="12" Margin="0,0,0,14"
                Visibility="{Binding IsLoading, Converter={StaticResource BoolToVis}}">
                        <TextBlock Text="正在加载中..." Foreground="White" />
                    </Border>

                    <!-- 当 IsError 为 true 时折叠（使用 Inverse 参数：true -> Collapsed） -->
                    <TextBlock Text="一切正常" Foreground="#10B981" 
                   Visibility="{Binding IsError, Converter={StaticResource BoolToVis}, ConverterParameter='Inverse'}" />


                    <!-- ========================================================== -->
                    <!-- 2. 数字为 null 或 0 时转折叠示例                             -->
                    <!-- ========================================================== -->
                    <!-- 当 UnreadCount 数字为 0 或 null 时，整个徽标 Border 自动折叠隐藏；大于 0 时自动显示 -->
                    <Border Background="#EF4444" CornerRadius="10" Padding="8,2" HorizontalAlignment="Left"
                Visibility="{Binding UnreadCount, Converter={StaticResource NullOrZeroToVis}}">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="未读消息: " Foreground="White" FontSize="12" />
                            <TextBlock Text="{Binding UnreadCount}" Foreground="White" FontSize="12" FontWeight="Bold" />
                        </StackPanel>
                    </Border>

                    <!-- 当设备报警代码 ErrorCode 为 0 或 null 时折叠 -->
                    <TextBlock Text="故障代码：无" 
                   Visibility="{Binding ErrorCode, Converter={StaticResource NullOrZeroToVis}}" />

                </StackPanel>
            </StackPanel>
```


### 8.2 指示灯
```xml
                <!-- 2. 开关按钮与状态机动态联动 -->
                <Border Background="#1E2026" BorderBrush="#2F3340" BorderThickness="1" CornerRadius="10" Padding="20,16">
                    <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
                        <TextBlock Text="PLC 自动轮询采集：" Foreground="#E5E7EB" FontSize="13.5" VerticalAlignment="Center" Margin="0,0,12,0" />

                        <!-- 标准拨动开关 -->
                        <control:AyuToggleSwitch IsChecked="{Binding IsPollingEnabled}"
                                      Command="{Binding TogglePollingCommand}"
                                      Width="54"
                                      OnText="ON"
                                      OffText="OFF"
                                      Margin="0,0,24,0" />

                        <!-- 实时联动的状态指示灯 -->
                        <control:AyuStateIndicator State="{Binding PlcCurrentState}"
                                        Label="{Binding PlcStateDescription}"
                                        IndicatorSize="14" />
                    </StackPanel>
                </Border>
```

### 8.3 导航菜单
```xml
                <!-- 示例：配置为左侧边栏导航 (Position="Left") -->
                <control:AyuNavigationMenu Grid.Column="0"
                           ItemsSource="{Binding MenuItems}"
                           SelectedItem="{Binding SelectedMenuItem}"
                           Position="Left" />
```


### 8.4 工具类
```cs
        public void RunDemo()
        {
            // 1. JSON 测试
            var user = new { Name = "AyuUI", Version = "1.0.0", IsActive = true };
            string jsonStr = JsonHelper.Serialize(user, pretty: true);
            JsonHelper.SaveToFile(user, "user_config.json");
            var loadedUser = JsonHelper.LoadFromFile<NavigationItem>("user_config.json");

            // 2. XML 测试
            string xmlStr = XmlHelper.Serialize(user);

            // 3. 压缩与解压测试
            // ZipHelper.CompressDirectory(@"D:\Logs", @"D:\Backup\Logs.zip");
            // ZipHelper.ExtractZip(@"D:\Backup\Logs.zip", @"D:\ExtractLogs");

            // 4. 加密与解密测试
            string originalText = "PLC_Password_123456";
            string encrypted = EncryptHelper.AesEncrypt(originalText);
            string decrypted = EncryptHelper.AesDecrypt(encrypted);

            // 5. MD5 校验
            string md5Hash = EncryptHelper.ToMd5("123456");

            // 1. 将高分辨率 PNG 转换为标准的 WPF 应用程序多尺寸 .ico 图标
            string pngLogo = @"D:\Assets\app_logo.png";
            string iconOut = @"D:\Assets\favicon.ico";
            ImageHelper.ConvertImageToIco(pngLogo, iconOut);

            // 2. 给导出的报表截图添加单行版权文字水印
            string reportImg = @"D:\Reports\chart_01.png";
            string watermarkedImg = @"D:\Reports\chart_01_protected.png";
            ImageHelper.AddTextWatermark(reportImg, watermarkedImg, "内部机密 - 仅限 AyuUI 研发部使用");

            // 3. 添加全屏斜向铺满网格水印（防止截屏传播外泄）
            ImageHelper.AddTileTextWatermark(reportImg, @"D:\Reports\chart_01_tile.png", "AyuUI 授权所有 2026");

            // 4. 给产品图片加上半透明公司 Logo 印章水印
            string productImg = @"D:\Products\device.jpg";
            string logoImg = @"D:\Assets\brand_stamp.png";
            string resultImg = @"D:\Products\device_watermarked.jpg";
            ImageHelper.AddImageWatermark(productImg, logoImg, resultImg, ImageHelper.WatermarkPosition.BottomRight, opacityPercent: 40);
        }
```