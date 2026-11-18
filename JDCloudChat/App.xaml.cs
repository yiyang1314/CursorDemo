using AyuUI.Controll.IOC;
using AyuUI.Controll.IOC.Simple;
using AyuUI.Controll.Messaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace JDCloudChat
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private static void ConfigureServices()
        {
            var container = AyuContainer.Default;

            // 1. 自动扫描当前程序集内所有标记了 [Service] 的类
            container.RegisterFromAssembly(Assembly.GetExecutingAssembly());
            container.RegisterSingleton<AyuMessenger>();
            container.RegisterSingleton<AyuMessenger>();

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureServices();
            base.OnStartup(e);
        }
    }
}
