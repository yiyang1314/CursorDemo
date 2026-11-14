using AyuUI.Controll.Commands;
using AyuUI.Controll.Controls;
using AyuUI.Controll.Dialogs;
using AyuUI.Controll.IOC.Attributes;
using AyuUI.Controll.Messaging;
using AyuUI.Controll.Toasts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;

namespace AyuUI.Controll.ViewModels
{
    [Service(ServiceLifetime.Singleton)]
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IDialogService _dialog;
        private readonly IMessenger _messenger;
        private string _deviceInfo = "等待设备上报...";
        public string DeviceInfo
        {
            get => _deviceInfo;
            set  { _deviceInfo = value; OnPropertyChanged(); } 
        }
        private bool _isDarkTheme = true;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set { _isDarkTheme = value; OnPropertyChanged(); }
        }

        private string _currentLangText = "中";
        public string CurrentLangText
        {
            get => _currentLangText;
            set { _currentLangText = value; OnPropertyChanged(); }
        }
        public ObservableCollection<NavigationItem> MenuItems { get; set; }

        private NavigationItem? _selectedMenuItem;
        public NavigationItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set { _selectedMenuItem = value; OnPropertyChanged(); }
        }


        public ICommand OpenSettingsCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand OpenHelpCommand { get; }
        public ICommand SwitchLanguageCommand { get; }

        public MainViewModel(IMessenger messenger,IDialogService dialog)
        {
            _messenger = messenger;
            _dialog = dialog;
            MenuItems = new ObservableCollection<NavigationItem>
            {
                new NavigationItem { Id = "home", Title = "首页概览", IconData = "M10,20V14H14V20H19V12H22L12,3L2,12H5V20H10Z", IsSelected = true },
                new NavigationItem { Id = "device", Title = "设备监控", IconData = "M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4Z" },
                new NavigationItem { Id = "recipe", Title = "配方管理", IconData = "M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z" },
                new NavigationItem { Id = "settings", Title = "系统设置", IconData = "M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.53,5.05 19.27,4.97 19.05,5.05L16.56,6.05C16.04,5.65 15.48,5.32 14.87,5.07L14.49,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.51,2.42L9.13,5.07C8.52,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.97 4.47,5.05 4.34,5.27L2.34,8.73C2.21,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.21,15.05 2.34,15.27L4.34,18.73C4.47,18.95 4.73,19.03 4.95,18.95L17.44,17.95C17.96,18.35 18.52,18.68 19.13,18.93L19.51,21.58C19.54,21.82 19.75,22 20,22H24C24.25,22 24.46,21.82 24.49,21.58L24.87,18.93C25.48,18.68 26.04,18.35 26.56,17.95L29.05,18.95C29.27,19.03 29.53,18.95 29.66,18.73L31.66,15.27C31.79,15.05 31.73,14.78 31.54,14.63L19.43,12.97Z" }
            };
            OpenSettingsCommand = new RelayCommand(() =>
            {
                _messenger.Publish(new DeviceStatusMessage("PLC-01", "运行正常", 42.5), token: "PLC_Channel");
            });

            ToggleThemeCommand = new RelayCommand(() =>
            {
                IsDarkTheme = !IsDarkTheme;
                // TODO: 在此处动态替换 Application.Current.Resources.MergedDictionaries 中的主题资源字典
                _messenger.Publish(new ThemeChangedMessage(true));
            });

            OpenHelpCommand = new RelayCommand(() =>
            {
                _dialog.Error($"OpenHelpCommand: {CurrentLangText}");
            });

            SwitchLanguageCommand = new RelayCommand(() =>
            {
                CurrentLangText = CurrentLangText == "中" ? "EN" : "中";
                _dialog.Info($"当前主题已切换为: {CurrentLangText}");
                // TODO: 在此处动态替换语言资源字典 (zh-CN.xaml / en-US.xaml)
            });

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
