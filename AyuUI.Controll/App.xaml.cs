using AyuUI.Controll.IOC;
using AyuUI.Controll.IOC.Simple;
using AyuUI.Controll.Simples; // 替换为你的窗口所在命名空间
using System;
using System.Reflection;
using System.Windows;

namespace AyuUI.Controll
{
    public partial class App : Application
    {
        //    [STAThread]
        //    public static void Main(string[] args)
        //    {
        //        // 1. 全局异常捕获（可选，防止程序崩溃无响应）
        //        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        //        {
        //            var ex = e.ExceptionObject as Exception;
        //            MessageBox.Show($"发生未捕获的异常: {ex?.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        //        };

        //        // 2. 初始化 Application 实例
        //        var app = new App();

        //        // 3. 初始化组件（加载 App.xaml 中定义的资源）
        //        app.InitializeComponent();

        //        // 4. 实例化并显示你的自定义窗口
        //        var mainWindow = new AyuSimpleWindow();

        //        // 设置为主窗口并运行
        //        app.MainWindow = mainWindow;
        //        mainWindow.Show();

        //        // 5. 启动应用程序消息循环
        //        app.Run();
        //    }
        //}
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
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureServices();
            base.OnStartup(e);
        }
    }
}
